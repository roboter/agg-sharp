/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using MatterHackers.Csg.Operations;
using MatterHackers.Csg.Processors;
using MatterHackers.Csg.Transform;
using MatterHackers.VectorMath;
using System;
using System.Collections.Generic;

namespace MatterHackers.Csg
{
	public enum Alignment { x, y, z, negX, negY, negZ };

	[Flags]
	public enum Face
	{
		Left = 0x01,
		Right = 0x02,
		Front = 0x04,
		Back = 0x08,
		Bottom = 0x10,
		Top = 0x20,
	};

	[Flags]
	public enum Edge
	{
		LeftFront = Face.Left | Face.Front,
		LeftBack = Face.Left | Face.Back,
		LeftBottom = Face.Left | Face.Bottom,
		LeftTop = Face.Left | Face.Top,
		RightFront = Face.Right | Face.Front,
		RightBack = Face.Right | Face.Back,
		RightBottom = Face.Right | Face.Bottom,
		RightTop = Face.Right | Face.Top,
		FrontBottom = Face.Front | Face.Bottom,
		FrontTop = Face.Front | Face.Top,
		BackBottom = Face.Back | Face.Bottom,
		BackTop = Face.Back | Face.Top
	}

	public static class Helpers
	{
		public static string FormatWith(this string format, params object[] args)
		{
			if (format == null)
				throw new ArgumentNullException("format");

			return string.Format(format, args);
		}

		public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
		{
			if (format == null)
				throw new ArgumentNullException("format");

			return string.Format(provider, format, args);
		}
	}

	public abstract class CsgObject
	{
		private Dictionary<string, string> properties = new Dictionary<string, string>();

		public Dictionary<string, string> Properties
		{
			get
			{
				return properties;
			}
		}

		protected string name
		{
			get
			{
				if (properties.ContainsKey("name"))
				{
					return properties["name"];
				}
				return "";
			}

			set
			{
				properties["name"] = value;
			}
		}

		public CsgObject(string name)
		{
			//UnitTests.Run();
			this.name = name;
		}

		public CsgObject(Dictionary<string, string> properties)
		{
			//UnitTests.Run();
			if (properties != null)
			{
				this.properties = properties;
			}
		}

		#region Member Functions

		abstract public AxisAlignedBoundingBox GetAxisAlignedBoundingBox();

		public String Name { get { return name; } }

		public Vector3 GetCenter()
		{
			AxisAlignedBoundingBox bounds = GetAxisAlignedBoundingBox();
			return new Vector3((bounds.MaxXYZ + bounds.MinXYZ) / 2);
		}

		#region Size Functions

		public virtual Vector3 Size
		{
			get
			{
				AxisAlignedBoundingBox bounds = GetAxisAlignedBoundingBox();
				return new Vector3(
					bounds.MaxXYZ.X - bounds.MinXYZ.X,
					bounds.MaxXYZ.Y - bounds.MinXYZ.Y,
					bounds.MaxXYZ.Z - bounds.MinXYZ.Z);
			}

			set
			{
				throw new NotImplementedException("You have to implement this in the derived class.");
			}
		}

		public double XSize { get { return Size.X; } }

		public double YSize { get { return Size.Y; } }

		public double ZSize { get { return Size.Z; } }

		#endregion Size Functions

		#region Geometric Properties & Inspection

		public AxisAlignedBoundingBox Bounds => GetAxisAlignedBoundingBox();
		public AxisAlignedBoundingBox BoundingBox => GetAxisAlignedBoundingBox();
		public Vector3 Center => GetCenter();
		public Vector3 Min => Bounds.MinXYZ;
		public Vector3 Max => Bounds.MaxXYZ;

		// Face center anchor points
		public Vector3 TopCenter => new Vector3(Center.X, Center.Y, Max.Z);
		public Vector3 BottomCenter => new Vector3(Center.X, Center.Y, Min.Z);
		public Vector3 LeftCenter => new Vector3(Min.X, Center.Y, Center.Z);
		public Vector3 RightCenter => new Vector3(Max.X, Center.Y, Center.Z);
		public Vector3 FrontCenter => new Vector3(Center.X, Min.Y, Center.Z);
		public Vector3 BackCenter => new Vector3(Center.X, Max.Y, Center.Z);

		/// <summary>
		/// Returns the 8 corner points of the bounding box.
		/// </summary>
		public Vector3[] GetCorners()
		{
			var min = Min;
			var max = Max;
			return new Vector3[]
			{
				new Vector3(min.X, min.Y, min.Z), // 0: Bottom-Front-Left
				new Vector3(max.X, min.Y, min.Z), // 1: Bottom-Front-Right
				new Vector3(max.X, max.Y, min.Z), // 2: Bottom-Back-Right
				new Vector3(min.X, max.Y, min.Z), // 3: Bottom-Back-Left
				new Vector3(min.X, min.Y, max.Z), // 4: Top-Front-Left
				new Vector3(max.X, min.Y, max.Z), // 5: Top-Front-Right
				new Vector3(max.X, max.Y, max.Z), // 6: Top-Back-Right
				new Vector3(min.X, max.Y, max.Z), // 7: Top-Back-Left
			};
		}

		/// <summary>
		/// Returns the 12 edges of the bounding box as line segments (Start, End).
		/// </summary>
		public (Vector3 Start, Vector3 End)[] GetBoundingEdges()
		{
			var c = GetCorners();
			return new (Vector3, Vector3)[]
			{
				// Bottom 4 edges
				(c[0], c[1]), (c[1], c[2]), (c[2], c[3]), (c[3], c[0]),
				// Top 4 edges
				(c[4], c[5]), (c[5], c[6]), (c[6], c[7]), (c[7], c[4]),
				// Vertical 4 pillars
				(c[0], c[4]), (c[1], c[5]), (c[2], c[6]), (c[3], c[7])
			};
		}

		/// <summary>
		/// Returns the 12 bounding edges as Line primitive objects.
		/// </summary>
		public Solids.Line[] GetLines()
		{
			var edges = GetBoundingEdges();
			var lines = new Solids.Line[edges.Length];
			for (int i = 0; i < edges.Length; i++)
			{
				lines[i] = new Solids.Line(edges[i].Start, edges[i].End);
			}
			return lines;
		}

		public Solids.Line[] Lines => GetLines();

		/// <summary>
		/// Returns unique vertices as Point primitive objects.
		/// </summary>
		public Solids.Point[] GetPoints(double tolerance = 1e-4)
		{
			var unique = GetUniqueVertices(tolerance);
			var pts = new Solids.Point[unique.Count];
			for (int i = 0; i < unique.Count; i++)
			{
				pts[i] = new Solids.Point(unique[i]);
			}
			return pts;
		}

		public Solids.Point[] CornerPoints => GetPoints();

		/// <summary>
		/// Evaluates the CSG tree into a triangulated 3D mesh.
		/// </summary>
		public PolygonMesh.Mesh ToMesh()
		{
			return MatterHackers.RenderOpenGl.CsgToMesh.Convert(this);
		}

		/// <summary>
		/// Gets all vertices of the evaluated 3D mesh.
		/// </summary>
		public List<Vector3> GetVertices()
		{
			var mesh = ToMesh();
			var result = new List<Vector3>(mesh.Vertices.Count);
			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				var v = mesh.Vertices[i];
				result.Add(new Vector3(v.X, v.Y, v.Z));
			}
			return result;
		}

		/// <summary>
		/// Gets unique vertices of the evaluated 3D mesh (e.g. 8 corner points for a cube).
		/// </summary>
		public List<Vector3> GetUniqueVertices(double tolerance = 1e-4)
		{
			var raw = GetVertices();
			var unique = new List<Vector3>();
			double tolSq = tolerance * tolerance;
			foreach (var pt in raw)
			{
				bool found = false;
				for (int i = 0; i < unique.Count; i++)
				{
					if ((unique[i] - pt).LengthSquared <= tolSq)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					unique.Add(pt);
				}
			}
			return unique;
		}

		/// <summary>
		/// Gets all unique edges of the evaluated 3D mesh as (Start, End) pairs.
		/// </summary>
		public List<(Vector3 Start, Vector3 End)> GetEdges(double tolerance = 1e-4)
		{
			var mesh = ToMesh();
			var edges = new List<(Vector3 Start, Vector3 End)>();
			if (mesh == null || mesh.Faces.Count == 0) return edges;

			var uniquePoints = new List<Vector3>();
			var canonicalIndex = new int[mesh.Vertices.Count];

			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				var v = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);
				int found = -1;
				for (int u = 0; u < uniquePoints.Count; u++)
				{
					if (Math.Abs(uniquePoints[u].X - v.X) <= tolerance &&
					    Math.Abs(uniquePoints[u].Y - v.Y) <= tolerance &&
					    Math.Abs(uniquePoints[u].Z - v.Z) <= tolerance)
					{
						found = u;
						break;
					}
				}
				if (found == -1)
				{
					canonicalIndex[i] = uniquePoints.Count;
					uniquePoints.Add(v);
				}
				else
				{
					canonicalIndex[i] = found;
				}
			}

			var edgeSet = new HashSet<(int, int)>();
			void AddEdge(int i1, int i2)
			{
				if (i1 < 0 || i2 < 0 || i1 >= mesh.Vertices.Count || i2 >= mesh.Vertices.Count)
					return;
				int c1 = canonicalIndex[i1];
				int c2 = canonicalIndex[i2];
				if (c1 == c2) return;
				int min = Math.Min(c1, c2);
				int max = Math.Max(c1, c2);
				if (edgeSet.Add((min, max)))
				{
					edges.Add((uniquePoints[c1], uniquePoints[c2]));
				}
			}

			foreach (var face in mesh.Faces)
			{
				AddEdge(face.v0, face.v1);
				AddEdge(face.v1, face.v2);
				AddEdge(face.v2, face.v0);
			}

			return edges;
		}

		#region Fluent Transformations

		public CsgObject Translate(double x, double y, double z) => new Translate(this, x, y, z);
		public CsgObject Translate(Vector3 offset) => new Translate(this, offset);
		public CsgObject Rotate(double x = 0, double y = 0, double z = 0) => new Rotate(this, x, y, z);
		public CsgObject RotateDegrees(double x = 0, double y = 0, double z = 0) =>
			new Rotate(this, MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(z));
		public CsgObject Scale(double scale) => new Scale(this, scale);
		public CsgObject Scale(double x, double y, double z) => new Scale(this, x, y, z);
		public CsgObject Scale(Vector3 scale) => new Scale(this, scale);

		#endregion Fluent Transformations

		#endregion Geometric Properties & Inspection

		#region Mirror Functions

		public CsgObject NewMirrorAccrossX(double offsetFromOrigin = 0, string name = "")
		{
			if (offsetFromOrigin != 0)
			{
				return new Translate(new Scale(this, new Vector3(-1, 1, 1)), new Vector3(offsetFromOrigin * 2, 0, 0), name);
			}
			return new Scale(this, new Vector3(-1, 1, 1), name);
		}

		public CsgObject NewMirrorAccrossY(double offsetFromOrigin = 0, string name = "")
		{
			return new Translate(new Scale(this, new Vector3(1, -1, 1)), new Vector3(0, offsetFromOrigin * 2, 0), name);
		}

		public CsgObject NewMirrorAccrossZ(double offsetFromOrigin = 0, string name = "")
		{
			return new Translate(new Scale(this, new Vector3(1, 1, -1)), new Vector3(0, 0, offsetFromOrigin * 2), name);
		}

		#endregion Mirror Functions

		#endregion Member Functions

		#region Operators

		public static CsgObject operator +(CsgObject left, CsgObject right)
		{
			if (left == null)
			{
				return right;
			}
			if (right == null)
			{
				return left;
			}
			return new Union(left, right);
		}

		public static Union operator +(Union left, CsgObject right)
		{
			left.Add(right);
			return left;
		}

		public static Union operator +(CsgObject left, Union right)
		{
			return right + left;
		}

		public static CsgObject operator -(CsgObject left, CsgObject right)
		{
			return new Difference(left, right);
		}

		public static Difference operator -(Difference left, CsgObject right)
		{
			left.AddToSubtractList(right);
			return left;
		}

		#endregion Operators

		#region Static Functions

		/// <summary>
		/// Normally ObjectCSG tree can be directed acyclic graphs and have instances appear more than once in the tree.
		/// This function will ensure that every Object is a unique instance in the structure.
		/// </summary>
		/// <param name="dagRoot"></param>
		/// <returns>A new ObjectCSG root that is a new tree of all the objects in the original tree.</returns>
		public static CsgObject Flatten(CsgObject dagRoot)
		{
			CopyAndFlatten flattener = new CopyAndFlatten();
			return flattener.DoCopyAndFlatten((dynamic)dagRoot);
		}

		public static Face GetOposite(Face face)
		{
			Face oppositeFace = face;
			switch (face)
			{
				case Face.Left:
					oppositeFace = Face.Right;
					break;

				case Face.Right:
					oppositeFace = Face.Left;
					break;

				case Face.Front:
					oppositeFace = Face.Back;
					break;

				case Face.Back:
					oppositeFace = Face.Front;
					break;

				case Face.Bottom:
					oppositeFace = Face.Top;
					break;

				case Face.Top:
					oppositeFace = Face.Bottom;
					break;

				default:
					throw new NotImplementedException();
			}

			return oppositeFace;
		}

		#region Structure Optimizations

		public void OptomizeTransforms()
		{
		}

		#endregion Structure Optomizations

		#endregion Static Functions
	}
}