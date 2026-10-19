using MatterHackers.Agg.VertexSource;
using MatterHackers.Csg.Transform;
using MatterHackers.PolygonMesh;
using MatterHackers.PolygonMesh.Processors;
using MatterHackers.VectorMath;
using System;
using System.Collections.Generic;

namespace MatterHackers.Csg.Solids
{
    /// <summary>
    /// Extrudes a 2D closed primitive along the Z axis while continuously rotating (twisting)
    /// and optionally scaling it across vertical layers/slices into a 3D solid.
    /// </summary>
    public class TwistExtrude : CsgObjectWrapper
    {
        public I2DPrimitive Shape { get; }
        public double Height { get; }
        public double TwistDegrees { get; }
        public int Slices { get; }
        public new double Scale { get; }
        public bool Centered { get; }
        public Alignment Alignment { get; }
        public int Resolution { get; }

        public TwistExtrude(
            I2DPrimitive shape,
            double height,
            double twistDegrees = 360,
            int slices = 30,
            double scale = 1.0,
            bool centered = false,
            Alignment alignment = Alignment.z,
            int resolution = 60,
            string name = "")
            : base(name)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            shape.ValidateClosed("twist extrude");

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Extrusion height must be greater than zero.");
            }

            if (scale < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Extrusion scale cannot be negative.");
            }

            Height = height;
            TwistDegrees = twistDegrees;
            Slices = Math.Max(1, slices);
            Scale = scale;
            Centered = centered;
            Alignment = alignment;
            Resolution = Math.Max(3, resolution);

            Mesh mesh = BuildMesh(Shape, Height, TwistDegrees, Slices, Scale, Centered, Resolution);
            CsgObject csgMesh = new MeshContainer(mesh);
            root = Primitive2DHelpers.ApplyAlignment(csgMesh, alignment);
        }

        public TwistExtrude(
            double[] points,
            double height,
            double twistDegrees = 360,
            int slices = 30,
            double scale = 1.0,
            bool centered = false,
            Alignment alignment = Alignment.z,
            int resolution = 60,
            string name = "")
            : this(new Path(points, close: true), height, twistDegrees, slices, scale, centered, alignment, resolution, name)
        {
        }

        public TwistExtrude(
            Vector2[] points,
            double height,
            double twistDegrees = 360,
            int slices = 30,
            double scale = 1.0,
            bool centered = false,
            Alignment alignment = Alignment.z,
            int resolution = 60,
            string name = "")
            : this(new Path(points, close: true), height, twistDegrees, slices, scale, centered, alignment, resolution, name)
        {
        }

        private static Mesh BuildMesh(
            I2DPrimitive shape,
            double height,
            double twistDegrees,
            int slices,
            double scale,
            bool centered,
            int resolution)
        {
            // 1. Sample base 2D profile at z = 0 with seam alignment
            List<Vector2> baseLoop = Loft.SampleLoop(shape, resolution, 0, out Vector2 center);
            int n = baseLoop.Count;

            var mesh = new Mesh();

            // 2. Generate slices along Z
            int sliceCount = slices;
            var sliceLoops = new List<List<Vector2>>(sliceCount + 1);

            for (int k = 0; k <= sliceCount; k++)
            {
                double t = (double)k / sliceCount;
                double angleDeg = twistDegrees * t;
                double angleRad = MathHelper.DegreesToRadians(angleDeg);
                double s = 1.0 + (scale - 1.0) * t;

                double cosA = Math.Cos(angleRad);
                double sinA = Math.Sin(angleRad);

                var loop = new List<Vector2>(n);
                for (int i = 0; i < n; i++)
                {
                    double dx = (baseLoop[i].X - center.X) * s;
                    double dy = (baseLoop[i].Y - center.Y) * s;
                    double x = center.X + dx * cosA - dy * sinA;
                    double y = center.Y + dx * sinA + dy * cosA;
                    loop.Add(new Vector2(x, y));
                }
                sliceLoops.Add(loop);
            }

            // 3. Generate side wall quads / triangles between consecutive slices
            for (int k = 0; k < sliceCount; k++)
            {
                double zBottom = (double)k / sliceCount * height;
                double zTop = (double)(k + 1) / sliceCount * height;

                var bottomLoop = sliceLoops[k];
                var topLoop = sliceLoops[k + 1];

                bool topIsPoint = (k == sliceCount - 1) && (scale <= 1e-6);

                if (topIsPoint)
                {
                    var apex = new Vector3(center.X, center.Y, height);
                    for (int i = 0; i < n; i++)
                    {
                        int next = (i + 1) % n;
                        var b0 = new Vector3(bottomLoop[i], zBottom);
                        var b1 = new Vector3(bottomLoop[next], zBottom);
                        mesh.CreateFace(b0, b1, apex);
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        int next = (i + 1) % n;
                        var b0 = new Vector3(bottomLoop[i], zBottom);
                        var b1 = new Vector3(bottomLoop[next], zBottom);
                        var t1 = new Vector3(topLoop[next], zTop);
                        var t0 = new Vector3(topLoop[i], zTop);

                        mesh.CreateFace(b0, b1, t1);
                        mesh.CreateFace(b0, t1, t0);
                    }
                }
            }

            // 4. Generate bottom cap at z = 0 (facing -Z)
            var bottomStorage = new VertexStorage();
            bottomStorage.MoveTo(sliceLoops[0][0]);
            for (int i = 1; i < n; i++)
            {
                bottomStorage.LineTo(sliceLoops[0][i]);
            }
            bottomStorage.ClosePolygon();

            Mesh bottomCapMesh = bottomStorage.TriangulateFaces(zHeight: 0);
            bottomCapMesh.ReverseFaces();
            mesh.CopyFaces(bottomCapMesh);

            // 5. Generate top cap at z = height (facing +Z) if scale > 0
            if (scale > 1e-6)
            {
                var topStorage = new VertexStorage();
                var topLoop = sliceLoops[sliceCount];
                topStorage.MoveTo(topLoop[0]);
                for (int i = 1; i < n; i++)
                {
                    topStorage.LineTo(topLoop[i]);
                }
                topStorage.ClosePolygon();

                Mesh topCapMesh = topStorage.TriangulateFaces(zHeight: height);
                mesh.CopyFaces(topCapMesh);
            }

            // 6. Centering along Z
            if (centered)
            {
                mesh.Translate(new Vector3(0, 0, -height / 2.0));
            }

            mesh.CleanAndMerge();
            return mesh;
        }
    }
}
