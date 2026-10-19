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
    /// Common interface for 2D primitives that can be extruded or revolved into 3D shapes.
    /// </summary>
    public interface I2DPrimitive
    {
        /// <summary>
        /// Gets whether this 2D primitive forms a closed boundary suitable for 3D extrusion/revolution.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Validates that the primitive is closed for extrusion or revolution.
        /// Throws an InvalidOperationException with a detailed explanation if it is not closed.
        /// </summary>
        void ValidateClosed(string operationName = "extrude");

        /// <summary>
        /// Converts this 2D primitive into an Agg IVertexSource path.
        /// </summary>
        IVertexSource ToVertexSource();

        /// <summary>
        /// Extrudes this 2D primitive into a solid 3D shape along the Z axis (or specified alignment).
        /// </summary>
        CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z);

        /// <summary>
        /// Extrudes this 2D primitive with a rotational twist and optional scale along the Z axis.
        /// </summary>
        CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z);

        /// <summary>
        /// Extrudes this 2D primitive while twisting around its centroid into a helical 3D shape.
        /// </summary>
        CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z);

        /// <summary>
        /// Revolves this 2D primitive around an axis into a solid 3D shape.
        /// </summary>
        CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true);

        /// <summary>
        /// Revolves this 2D primitive using start and end angles in radians.
        /// </summary>
        CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true);

        /// <summary>
        /// Lofts this 2D primitive to another 2D primitive across the specified height into a 3D solid shape.
        /// </summary>
        CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z);

        /// <summary>
        /// Triangulates the 2D primitive into a flat planar mesh on the XY ground plane.
        /// </summary>
        Mesh ToMesh();

        /// <summary>
        /// Generates a thin wire/ribbon mesh from this primitive (useful for drawing open lines/paths in 3D).
        /// </summary>
        Mesh ToWireMesh(double width = 0.5);
    }

    internal static class Primitive2DHelpers
    {
        public static CsgObject ApplyAlignment(CsgObject root, Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.x:
                    return new Rotate(root, y: MathHelper.DegreesToRadians(90));
                case Alignment.negX:
                    return new Rotate(root, y: MathHelper.DegreesToRadians(-90));
                case Alignment.y:
                    return new Rotate(root, x: MathHelper.DegreesToRadians(90));
                case Alignment.negY:
                    return new Rotate(root, x: MathHelper.DegreesToRadians(-90));
                case Alignment.negZ:
                    return new Rotate(root, x: MathHelper.DegreesToRadians(180));
                case Alignment.z:
                default:
                    return root;
            }
        }
    }

    #region Point

    /// <summary>
    /// Represents a 2D or 3D coordinate point (X, Y, Z).
    /// Fully compatible with Vector2, Vector3, tuples (x, y) / (x, y, z), Dot, and Line.
    /// </summary>
    public class Point : I2DPrimitive, IEquatable<Point>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector2 Position2D
        {
            get => new Vector2(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        public Vector3 Position3D
        {
            get => new Vector3(X, Y, Z);
            set { X = value.X; Y = value.Y; Z = value.Z; }
        }

        public Vector2 Position
        {
            get => Position2D;
            set => Position2D = value;
        }

        public bool IsClosed => false;

        public Point(double x, double y, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point(double x, double y) : this(x, y, 0) { }

        public Point(Vector2 pos) : this(pos.X, pos.Y, 0) { }

        public Point(Vector3 pos) : this(pos.X, pos.Y, pos.Z) { }

        public Point() : this(0, 0, 0) { }

        // Distance & Line helpers
        public double DistanceTo(Point other) => (Position3D - (other?.Position3D ?? Vector3.Zero)).Length;
        public double DistanceTo(Vector3 other) => (Position3D - other).Length;
        public double DistanceTo(Vector2 other) => (Position2D - other).Length;

        public Line LineTo(Point other) => new Line(this, other);
        public Line LineTo(Vector3 other) => new Line(this, new Point(other));
        public Line LineTo(Vector2 other) => new Line(this, new Point(other));
        public Line LineTo(double x, double y, double z = 0) => new Line(this, new Point(x, y, z));

        // Implicit conversions
        public static implicit operator Vector2(Point p) => p == null ? Vector2.Zero : new Vector2(p.X, p.Y);
        public static implicit operator Vector3(Point p) => p == null ? Vector3.Zero : new Vector3(p.X, p.Y, p.Z);
        public static implicit operator Point(Vector2 v) => new Point(v.X, v.Y, 0);
        public static implicit operator Point(Vector3 v) => new Point(v.X, v.Y, v.Z);
        public static implicit operator Point((double x, double y) t) => new Point(t.x, t.y, 0);
        public static implicit operator Point((double x, double y, double z) t) => new Point(t.x, t.y, t.z);

        // Operators
        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Point operator *(Point a, double scale) => new Point(a.X * scale, a.Y * scale, a.Z * scale);
        public static Point operator /(Point a, double scale) => new Point(a.X / scale, a.Y / scale, a.Z / scale);
        public static Point operator -(Point a) => new Point(-a.X, -a.Y, -a.Z);

        // Deconstruction
        public void Deconstruct(out double x, out double y) { x = X; y = Y; }
        public void Deconstruct(out double x, out double y, out double z) { x = X; y = Y; z = Z; }

        public override bool Equals(object obj)
        {
            if (obj is Point p) return Equals(p);
            if (obj is Vector3 v3) return Math.Abs(X - v3.X) < 1e-6 && Math.Abs(Y - v3.Y) < 1e-6 && Math.Abs(Z - v3.Z) < 1e-6;
            if (obj is Vector2 v2) return Math.Abs(X - v2.X) < 1e-6 && Math.Abs(Y - v2.Y) < 1e-6 && Math.Abs(Z) < 1e-6;
            return false;
        }

        public bool Equals(Point other)
        {
            if (other is null) return false;
            return Math.Abs(X - other.X) < 1e-6 && Math.Abs(Y - other.Y) < 1e-6 && Math.Abs(Z - other.Z) < 1e-6;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Math.Round(X, 4).GetHashCode();
                hash = (hash * 397) ^ Math.Round(Y, 4).GetHashCode();
                hash = (hash * 397) ^ Math.Round(Z, 4).GetHashCode();
                return hash;
            }
        }

        public void ValidateClosed(string operationName = "extrude")
        {
            string typeName = GetType().Name;
            throw new InvalidOperationException(
                $"Cannot {operationName} a {typeName} at ({X:0.##}, {Y:0.##}): a {typeName.ToLower()} is a 0-dimensional point without area or boundary. " +
                $"A closed 2D shape (such as Circle, Rectangle, Square, or closed Path) is required.");
        }

        public IVertexSource ToVertexSource()
        {
            var s = new VertexStorage();
            s.MoveTo(X, Y);
            return s;
        }

        public CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            return null;
        }

        public CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            return null;
        }

        public CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            return null;
        }

        public CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("loft");
            return null;
        }

        public CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            return null;
        }

        public CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("twist extrude");
            return null;
        }

        public Mesh ToMesh() => ToWireMesh(0.5);

        public Mesh ToWireMesh(double width = 0.5)
        {
            if (Math.Abs(Z) > 1e-6)
            {
                return MatterHackers.RenderOpenGl.CsgToMesh.Convert(new Sphere(width).Translate(Position3D));
            }
            return new Ellipse(Position2D, width, width, 16).TriangulateFaces();
        }

        public Circle ToCircle(double radius) => new Circle(Position2D, radius);
        public CsgObject ToSphere(double radius) => new Sphere(radius).Translate(Position3D);

        public Path ToPath() => new Path().MoveTo(Position2D);

        public override string ToString() => Math.Abs(Z) > 1e-6 ? $"Point({X}, {Y}, {Z})" : $"Point({X}, {Y})";
    }

    #endregion Point

    #region Dot

    /// <summary>
    /// Represents a 2D point / dot primitive (specialization of Point).
    /// Note: A dot is 0-dimensional with zero area, so attempting to extrude or revolve it directly will fail validation.
    /// </summary>
    public class Dot : Point
    {
        public Dot(double x, double y) : base(x, y, 0) { }

        public Dot(Vector2 pos) : base(pos.X, pos.Y, 0) { }

        public Dot() : base(0, 0, 0) { }

        public override string ToString() => $"Dot({X}, {Y})";
    }

    #endregion Dot

    #region Line

    /// <summary>
    /// Represents a 2D or 3D line segment between two points.
    /// Supports length, height (compatible with Cylinder height), direction, midpoints, and cylinder conversion.
    /// Note: A line is an open 1D segment with zero enclosed area, so attempting to extrude or revolve it directly will fail validation.
    /// Combine lines in a closed Path (with path.Close()) to form extrudable boundaries.
    /// </summary>
    public class Line : I2DPrimitive
    {
        private Vector3 start3D;
        private Vector3 end3D;

        public Vector3 Start3D
        {
            get => start3D;
            set => start3D = value;
        }

        public Vector3 End3D
        {
            get => end3D;
            set => end3D = value;
        }

        public Point StartPoint
        {
            get => new Point(start3D);
            set => start3D = value != null ? value.Position3D : Vector3.Zero;
        }

        public Point EndPoint
        {
            get => new Point(end3D);
            set => end3D = value != null ? value.Position3D : Vector3.Zero;
        }

        public Vector2 Start
        {
            get => new Vector2(start3D.X, start3D.Y);
            set => start3D = new Vector3(value.X, value.Y, start3D.Z);
        }

        public Vector2 End
        {
            get => new Vector2(end3D.X, end3D.Y);
            set => end3D = new Vector3(value.X, value.Y, end3D.Z);
        }

        public double X1 => start3D.X;
        public double Y1 => start3D.Y;
        public double Z1 => start3D.Z;
        public double X2 => end3D.X;
        public double Y2 => end3D.Y;
        public double Z2 => end3D.Z;

        public double Dx => end3D.X - start3D.X;
        public double Dy => end3D.Y - start3D.Y;
        public double Dz => end3D.Z - start3D.Z;

        public double Length => (end3D - start3D).Length;

        /// <summary>
        /// The height/length of this line segment. Directly compatible with Cylinder height.
        /// </summary>
        public double Height => Length;

        public Vector3 MidPoint3D => (start3D + end3D) * 0.5;
        public Point MidPoint => new Point(MidPoint3D);

        public Vector3 Direction => Length > 1e-9 ? (end3D - start3D).GetNormal() : Vector3.UnitZ;

        public bool IsClosed => (start3D - end3D).LengthSquared < 1e-9;

        public Line(Point start, Point end)
        {
            start3D = start != null ? start.Position3D : Vector3.Zero;
            end3D = end != null ? end.Position3D : Vector3.Zero;
        }

        public Line(Vector3 start, Vector3 end)
        {
            start3D = start;
            end3D = end;
        }

        public Line(Vector2 start, Vector2 end)
        {
            start3D = new Vector3(start.X, start.Y, 0);
            end3D = new Vector3(end.X, end.Y, 0);
        }

        public Line(double x1, double y1, double x2, double y2)
            : this(new Vector2(x1, y1), new Vector2(x2, y2)) { }

        public Line(double x1, double y1, double z1, double x2, double y2, double z2)
            : this(new Vector3(x1, y1, z1), new Vector3(x2, y2, z2)) { }

        public Line(double length, double angleRadians = 0)
            : this(Vector2.Zero, new Vector2(length * Math.Cos(angleRadians), length * Math.Sin(angleRadians))) { }

        public Line() : this(Vector3.Zero, Vector3.Zero) { }

        // Implicit conversions
        public static implicit operator Line((Point start, Point end) t) => new Line(t.start, t.end);

        public void ValidateClosed(string operationName = "extrude")
        {
            if (!IsClosed)
            {
                throw new InvalidOperationException(
                    $"Cannot {operationName} an open Line from ({X1:0.##}, {Y1:0.##}) to ({X2:0.##}, {Y2:0.##}): " +
                    $"a line is a 1-dimensional open segment and does not enclose a 2D area. " +
                    $"Combine lines into a closed Path (using path.Close()), or use Rectangle/Polygon.");
            }
            else
            {
                throw new InvalidOperationException(
                    $"Cannot {operationName} a degenerate zero-length line: it has no 2D area.");
            }
        }

        public IVertexSource ToVertexSource()
        {
            var s = new VertexStorage();
            s.MoveTo(Start);
            s.LineTo(End);
            return s;
        }

        public CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            return null;
        }

        public CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            return null;
        }

        public CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            return null;
        }

        public CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("loft");
            return null;
        }

        public CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            return null;
        }

        public CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("twist extrude");
            return null;
        }

        public Mesh ToMesh() => ToWireMesh(0.5);

        public Mesh ToWireMesh(double width = 0.5)
        {
            if (Math.Abs(start3D.Z) > 1e-6 || Math.Abs(end3D.Z) > 1e-6)
            {
                return MatterHackers.RenderOpenGl.CsgToMesh.Convert(Cylinder.CreateBetween(start3D, end3D, width * 0.5, 16));
            }
            var stroke = new Stroke(ToVertexSource(), width);
            return stroke.TriangulateFaces();
        }

        public CsgObject ToCylinder(double radius, int sides = 16) => Cylinder.CreateBetween(start3D, end3D, radius, sides);

        public Path ToPath() => new Path().MoveTo(Start).LineTo(End);

        public override string ToString() => Math.Abs(Z1) > 1e-6 || Math.Abs(Z2) > 1e-6
            ? $"Line(({X1}, {Y1}, {Z1}) -> ({X2}, {Y2}, {Z2}))"
            : $"Line(({X1}, {Y1}) -> ({X2}, {Y2}))";
    }

    #endregion Line

    #region Path

    /// <summary>
    /// Represents a 2D path / polygon composed of sequential points, line segments, and curves.
    /// Can be explicitly closed via path.Close() or by matching the first and last points.
    /// </summary>
    public class Path : I2DPrimitive
    {
        private readonly List<Vector2> _points = new();
        private readonly VertexStorage _storage = new();
        private bool _isClosedExplicitly = false;

        public IReadOnlyList<Vector2> Points => _points;

        public bool IsClosed
        {
            get
            {
                if (_isClosedExplicitly) return true;
                if (_points.Count >= 3 && (_points[0] - _points[_points.Count - 1]).LengthSquared < 1e-6)
                {
                    return true;
                }
                return false;
            }
        }

        public Path() { }

        public Path(IEnumerable<Vector2> points, bool close = false)
        {
            if (points != null)
            {
                foreach (var pt in points)
                {
                    AddPoint(pt);
                }
            }
            if (close) Close();
        }

        public Path(params Vector2[] points) : this((IEnumerable<Vector2>)points, false) { }

        public Path(double[] coordinates, bool close = false)
        {
            if (coordinates != null)
            {
                if (coordinates.Length % 2 != 0)
                {
                    throw new ArgumentException("Coordinates array must contain an even number of values (pairs of X and Y).");
                }
                for (int i = 0; i < coordinates.Length; i += 2)
                {
                    AddPoint(coordinates[i], coordinates[i + 1]);
                }
            }
            if (close) Close();
        }

        public Path MoveTo(double x, double y)
        {
            var pt = new Vector2(x, y);
            _points.Add(pt);
            _storage.MoveTo(pt);
            return this;
        }

        public Path MoveTo(Vector2 point) => MoveTo(point.X, point.Y);

        public Path LineTo(double x, double y)
        {
            var pt = new Vector2(x, y);
            if (_points.Count == 0)
            {
                _points.Add(Vector2.Zero);
                _storage.MoveTo(Vector2.Zero);
            }
            _points.Add(pt);
            _storage.LineTo(pt);
            return this;
        }

        public Path LineTo(Vector2 point) => LineTo(point.X, point.Y);

        public Path AddPoint(double x, double y)
        {
            if (_points.Count == 0)
            {
                return MoveTo(x, y);
            }
            return LineTo(x, y);
        }

        public Path AddPoint(Vector2 point) => AddPoint(point.X, point.Y);

        public Path Add(Line line)
        {
            if (line == null) return this;
            if (_points.Count == 0)
            {
                MoveTo(line.Start);
                LineTo(line.End);
            }
            else
            {
                if ((_points[_points.Count - 1] - line.Start).LengthSquared > 1e-6)
                {
                    LineTo(line.Start);
                }
                LineTo(line.End);
            }
            return this;
        }

        public Path CurveTo(double ctrlX, double ctrlY, double endX, double endY)
        {
            if (_points.Count == 0)
            {
                MoveTo(0, 0);
            }
            var start = _points[_points.Count - 1];
            // Tessellate quadratic Bezier into segments
            const int steps = 16;
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double invT = 1.0 - t;
                double x = invT * invT * start.X + 2 * invT * t * ctrlX + t * t * endX;
                double y = invT * invT * start.Y + 2 * invT * t * ctrlY + t * t * endY;
                LineTo(x, y);
            }
            return this;
        }

        public Path CurveTo(double ctrl1X, double ctrl1Y, double ctrl2X, double ctrl2Y, double endX, double endY)
        {
            if (_points.Count == 0)
            {
                MoveTo(0, 0);
            }
            var start = _points[_points.Count - 1];
            // Tessellate cubic Bezier into segments
            const int steps = 20;
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double invT = 1.0 - t;
                double x = invT * invT * invT * start.X + 3 * invT * invT * t * ctrl1X + 3 * invT * t * t * ctrl2X + t * t * t * endX;
                double y = invT * invT * invT * start.Y + 3 * invT * invT * t * ctrl1Y + 3 * invT * t * t * ctrl2Y + t * t * t * endY;
                LineTo(x, y);
            }
            return this;
        }

        public Path Close()
        {
            if (!_isClosedExplicitly)
            {
                _isClosedExplicitly = true;
                if (_points.Count >= 2 && (_points[0] - _points[_points.Count - 1]).LengthSquared > 1e-6)
                {
                    _storage.LineTo(_points[0]);
                }
                _storage.ClosePolygon();
            }
            return this;
        }

        public void ValidateClosed(string operationName = "extrude")
        {
            if (_points.Count == 0)
            {
                throw new InvalidOperationException($"Cannot {operationName} an empty path: path contains no points.");
            }

            if (_points.Count < 3)
            {
                throw new InvalidOperationException(
                    $"Cannot {operationName} path: path only has {_points.Count} point(s). " +
                    $"At least 3 points forming a closed loop are required to define a 2D boundary.");
            }

            if (!IsClosed)
            {
                var start = _points[0];
                var end = _points[_points.Count - 1];
                throw new InvalidOperationException(
                    $"Cannot {operationName} an open path: the path has {_points.Count} points and is not closed. " +
                    $"Start point ({start.X:0.##}, {start.Y:0.##}) does not match end point ({end.X:0.##}, {end.Y:0.##}). " +
                    $"Call path.Close() or connect the last point back to the start point before {operationName}ing.");
            }
        }

        public IVertexSource ToVertexSource()
        {
            if (IsClosed)
            {
                var s = new VertexStorage();
                if (_points.Count > 0)
                {
                    s.MoveTo(_points[0]);
                    for (int i = 1; i < _points.Count; i++)
                    {
                        s.LineTo(_points[i]);
                    }
                    if ((_points[0] - _points[_points.Count - 1]).LengthSquared > 1e-6)
                    {
                        s.LineTo(_points[0]);
                    }
                    s.ClosePolygon();
                }
                return s;
            }
            return _storage;
        }

        public CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Extrusion height must be greater than zero.");
            }

            var mesh = VertexSourceToMesh.Extrude(ToVertexSource(), height);
            if (centered)
            {
                mesh.Translate(new Vector3(0, 0, -height / 2));
            }
            CsgObject result = new MeshContainer(mesh);
            return Primitive2DHelpers.ApplyAlignment(result, alignment);
        }

        public CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            double angleRad = MathHelper.DegreesToRadians(Math.Clamp(angleDegrees, 1, 360));
            return RevolveRadians(0, angleRad, angleSteps, axisOffset, revolveAroundZ);
        }

        public CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            IVertexSource source = ToVertexSource();
            if (Math.Abs(axisOffset) > 1e-6)
            {
                var shifted = new VertexStorage();
                foreach (var v in source.Vertices())
                {
                    if (v.IsMoveTo) shifted.MoveTo(v.Position.X + axisOffset, v.Position.Y);
                    else if (v.IsLineTo) shifted.LineTo(v.Position.X + axisOffset, v.Position.Y);
                    else if (v.IsClose) shifted.ClosePolygon();
                }
                source = shifted;
            }

            var mesh = VertexSourceToMesh.Revolve(source, angleSteps, angleStart, angleEnd, revolveAroundZ);
            return new MeshContainer(mesh);
        }

        public CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new Loft(this, top, height, twistDegrees, resolution, capped, centered, alignment);
        }

        public CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            if (Math.Abs(twistDegrees) > 1e-6 || Math.Abs(scale - 1.0) > 1e-6)
            {
                return TwistExtrude(height, twistDegrees, slices, scale, centered, alignment);
            }
            return Extrude(height, centered, alignment);
        }

        public CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new TwistExtrude(this, height, twistDegrees, slices, scale, centered, alignment);
        }

        public Mesh ToMesh()
        {
            ValidateClosed("triangulate");
            return ToVertexSource().TriangulateFaces();
        }

        public Mesh ToWireMesh(double width = 0.5)
        {
            var stroke = new Stroke(ToVertexSource(), width);
            return stroke.TriangulateFaces();
        }

        public override string ToString() => $"Path({_points.Count} points, Closed={IsClosed})";
    }

    #endregion Path

    #region Circle

    /// <summary>
    /// Represents a 2D circle with center and radius (or regular N-gon if sides > 2).
    /// </summary>
    public class Circle : I2DPrimitive
    {
        public Vector2 Center { get; set; }
        public double Radius { get; set; }
        public int Sides { get; set; }

        public double Diameter
        {
            get => Radius * 2;
            set => Radius = value / 2;
        }

        public bool IsClosed => true;

        public Circle(double radius, int sides = 0)
            : this(Vector2.Zero, radius, sides) { }

        public Circle(double x, double y, double radius, int sides = 0)
            : this(new Vector2(x, y), radius, sides) { }

        public Circle(Vector2 center, double radius, int sides = 0)
        {
            Center = center;
            Radius = radius;
            Sides = sides;
        }

        public void ValidateClosed(string operationName = "extrude")
        {
            if (Radius <= 0)
            {
                throw new InvalidOperationException($"Cannot {operationName} Circle: radius must be positive (was {Radius}).");
            }
        }

        public IVertexSource ToVertexSource()
        {
            int steps = Sides > 2 ? Sides : 40;
            return new Ellipse(Center, Radius, Radius, num_steps: steps);
        }

        public CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Extrusion height must be greater than zero.");
            }

            var mesh = VertexSourceToMesh.Extrude(ToVertexSource(), height);
            if (centered)
            {
                mesh.Translate(new Vector3(0, 0, -height / 2));
            }
            CsgObject result = new MeshContainer(mesh);
            return Primitive2DHelpers.ApplyAlignment(result, alignment);
        }

        public CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            double angleRad = MathHelper.DegreesToRadians(Math.Clamp(angleDegrees, 1, 360));
            return RevolveRadians(0, angleRad, angleSteps, axisOffset, revolveAroundZ);
        }

        public CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            int steps = Sides > 2 ? Sides : 40;
            var circleCenter = new Vector2(Center.X + axisOffset, Center.Y);
            var source = new Ellipse(circleCenter, Radius, Radius, num_steps: steps);
            var mesh = VertexSourceToMesh.Revolve(source, angleSteps, angleStart, angleEnd, revolveAroundZ);
            return new MeshContainer(mesh);
        }

        public CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new Loft(this, top, height, twistDegrees, resolution, capped, centered, alignment);
        }

        public CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            if (Math.Abs(twistDegrees) > 1e-6 || Math.Abs(scale - 1.0) > 1e-6)
            {
                return TwistExtrude(height, twistDegrees, slices, scale, centered, alignment);
            }
            return Extrude(height, centered, alignment);
        }

        public CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new TwistExtrude(this, height, twistDegrees, slices, scale, centered, alignment);
        }

        public Mesh ToMesh()
        {
            ValidateClosed("triangulate");
            return ToVertexSource().TriangulateFaces();
        }

        public Mesh ToWireMesh(double width = 0.5)
        {
            var stroke = new Stroke(ToVertexSource(), width);
            return stroke.TriangulateFaces();
        }

        public override string ToString() => $"Circle(Radius={Radius}, Center=({Center.X}, {Center.Y}))";
    }

    #endregion Circle

    #region Rectangle

    /// <summary>
    /// Represents a 2D rectangle (with optional rounded corners).
    /// </summary>
    public class Rectangle : I2DPrimitive
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public Vector2 Center { get; set; }
        public double CornerRadius { get; set; }

        public bool IsClosed => true;

        public Rectangle(double width, double height, bool centered = true)
            : this(width, height, 0, centered) { }

        public Rectangle(double x, double y, double width, double height, bool centered = false)
            : this(width, height, 0, centered)
        {
            if (!centered)
            {
                Center = new Vector2(x + width / 2, y + height / 2);
            }
            else
            {
                Center = new Vector2(x, y);
            }
        }

        public Rectangle(Vector2 size, bool centered = true)
            : this(size.X, size.Y, 0, centered) { }

        public Rectangle(double width, double height, double cornerRadius, bool centered = true)
        {
            Width = width;
            Height = height;
            CornerRadius = cornerRadius;
            Center = centered ? Vector2.Zero : new Vector2(width / 2, height / 2);
        }

        public void ValidateClosed(string operationName = "extrude")
        {
            if (Width <= 0 || Height <= 0)
            {
                throw new InvalidOperationException($"Cannot {operationName} Rectangle: width and height must be positive (was {Width}x{Height}).");
            }
            if (CornerRadius < 0)
            {
                throw new InvalidOperationException($"Cannot {operationName} Rectangle: cornerRadius cannot be negative (was {CornerRadius}).");
            }
        }

        public IVertexSource ToVertexSource()
        {
            double left = Center.X - Width / 2;
            double right = Center.X + Width / 2;
            double bottom = Center.Y - Height / 2;
            double top = Center.Y + Height / 2;

            if (CornerRadius > 0)
            {
                return new RoundedRect(left, bottom, right, top, CornerRadius);
            }

            var storage = new VertexStorage();
            storage.MoveTo(left, bottom);
            storage.LineTo(right, bottom);
            storage.LineTo(right, top);
            storage.LineTo(left, top);
            storage.ClosePolygon();
            return storage;
        }

        public CsgObject Extrude(double height, bool centered = false, Alignment alignment = Alignment.z)
        {
            ValidateClosed("extrude");
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Extrusion height must be greater than zero.");
            }

            if (CornerRadius <= 0)
            {
                CsgObject box = new Box(Width, Height, height);
                double zOffset = centered ? 0 : height / 2;
                box = new Translate(box, Center.X, Center.Y, zOffset);
                return Primitive2DHelpers.ApplyAlignment(box, alignment);
            }
            else
            {
                var mesh = VertexSourceToMesh.Extrude(ToVertexSource(), height);
                if (centered)
                {
                    mesh.Translate(new Vector3(0, 0, -height / 2));
                }
                CsgObject result = new MeshContainer(mesh);
                return Primitive2DHelpers.ApplyAlignment(result, alignment);
            }
        }

        public CsgObject Revolve(double angleDegrees = 360, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            double angleRad = MathHelper.DegreesToRadians(Math.Clamp(angleDegrees, 1, 360));
            return RevolveRadians(0, angleRad, angleSteps, axisOffset, revolveAroundZ);
        }

        public CsgObject RevolveRadians(double angleStart = 0, double angleEnd = MathHelper.Tau, int angleSteps = 30, double axisOffset = 0, bool revolveAroundZ = true)
        {
            ValidateClosed("revolve");
            IVertexSource source = ToVertexSource();
            if (Math.Abs(axisOffset) > 1e-6)
            {
                var shifted = new VertexStorage();
                foreach (var v in source.Vertices())
                {
                    if (v.IsMoveTo) shifted.MoveTo(v.Position.X + axisOffset, v.Position.Y);
                    else if (v.IsLineTo) shifted.LineTo(v.Position.X + axisOffset, v.Position.Y);
                    else if (v.IsClose) shifted.ClosePolygon();
                }
                source = shifted;
            }

            var mesh = VertexSourceToMesh.Revolve(source, angleSteps, angleStart, angleEnd, revolveAroundZ);
            return new MeshContainer(mesh);
        }

        public CsgObject Loft(I2DPrimitive top, double height, double twistDegrees = 0, int resolution = 60, bool capped = true, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new Loft(this, top, height, twistDegrees, resolution, capped, centered, alignment);
        }

        public CsgObject Extrude(double height, double twistDegrees, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            if (Math.Abs(twistDegrees) > 1e-6 || Math.Abs(scale - 1.0) > 1e-6)
            {
                return TwistExtrude(height, twistDegrees, slices, scale, centered, alignment);
            }
            return Extrude(height, centered, alignment);
        }

        public CsgObject TwistExtrude(double height, double twistDegrees = 360, int slices = 30, double scale = 1.0, bool centered = false, Alignment alignment = Alignment.z)
        {
            return new TwistExtrude(this, height, twistDegrees, slices, scale, centered, alignment);
        }

        public Mesh ToMesh()
        {
            ValidateClosed("triangulate");
            return ToVertexSource().TriangulateFaces();
        }

        public Mesh ToWireMesh(double width = 0.5)
        {
            var stroke = new Stroke(ToVertexSource(), width);
            return stroke.TriangulateFaces();
        }

        public override string ToString() => $"Rectangle({Width}x{Height}, Center=({Center.X}, {Center.Y}))";
    }

    #endregion Rectangle

    #region Square

    /// <summary>
    /// Represents a 2D square primitive (equal width and height).
    /// </summary>
    public class Square : Rectangle
    {
        public double Size
        {
            get => Width;
            set { Width = value; Height = value; }
        }

        public Square(double size, bool centered = true)
            : base(size, size, 0, centered) { }

        public Square(double x, double y, double size, bool centered = false)
            : base(x, y, size, size, centered) { }

        public Square(double size, double cornerRadius, bool centered = true)
            : base(size, size, cornerRadius, centered) { }

        public override string ToString() => $"Square(Size={Size}, Center=({Center.X}, {Center.Y}))";
    }

    /// <summary>
    /// Convenient alias for Square supporting alternate spelling.
    /// </summary>
    public class Sqare : Square
    {
        public Sqare(double size, bool centered = true) : base(size, centered) { }
        public Sqare(double x, double y, double size, bool centered = false) : base(x, y, size, centered) { }
        public Sqare(double size, double cornerRadius, bool centered = true) : base(size, cornerRadius, centered) { }
    }

    #endregion Square

    #region 2D Aliases

    public class Path2D : Path
    {
        public Path2D() : base() { }
        public Path2D(IEnumerable<Vector2> points, bool close = false) : base(points, close) { }
        public Path2D(params Vector2[] points) : base(points) { }
        public Path2D(double[] coordinates, bool close = false) : base(coordinates, close) { }
    }

    public class Line2D : Line
    {
        public Line2D(Vector2 start, Vector2 end) : base(start, end) { }
        public Line2D(Point start, Point end) : base(start, end) { }
        public Line2D(double x1, double y1, double x2, double y2) : base(x1, y1, x2, y2) { }
        public Line2D(double length, double angleRadians = 0) : base(length, angleRadians) { }
    }

    public class Dot2D : Dot
    {
        public Dot2D(double x, double y) : base(x, y) { }
        public Dot2D(Vector2 pos) : base(pos) { }
        public Dot2D() : base() { }
    }

    public class Point2D : Point
    {
        public Point2D(double x, double y) : base(x, y, 0) { }
        public Point2D(Vector2 pos) : base(pos) { }
        public Point2D() : base(0, 0, 0) { }
    }

    public class Point3D : Point
    {
        public Point3D(double x, double y, double z) : base(x, y, z) { }
        public Point3D(Vector3 pos) : base(pos) { }
        public Point3D() : base(0, 0, 0) { }
    }

    public class Circle2D : Circle
    {
        public Circle2D(double radius, int sides = 0) : base(radius, sides) { }
        public Circle2D(double x, double y, double radius, int sides = 0) : base(x, y, radius, sides) { }
        public Circle2D(Vector2 center, double radius, int sides = 0) : base(center, radius, sides) { }
    }

    public class Rectangle2D : Rectangle
    {
        public Rectangle2D(double width, double height, bool centered = true) : base(width, height, centered) { }
        public Rectangle2D(double x, double y, double width, double height, bool centered = false) : base(x, y, width, height, centered) { }
        public Rectangle2D(Vector2 size, bool centered = true) : base(size, centered) { }
        public Rectangle2D(double width, double height, double cornerRadius, bool centered = true) : base(width, height, cornerRadius, centered) { }
    }

    public class Square2D : Square
    {
        public Square2D(double size, bool centered = true) : base(size, centered) { }
        public Square2D(double x, double y, double size, bool centered = false) : base(x, y, size, centered) { }
        public Square2D(double size, double cornerRadius, bool centered = true) : base(size, cornerRadius, centered) { }
    }

    #endregion 2D Aliases
}
