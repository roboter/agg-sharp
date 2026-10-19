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

using MatterHackers.Csg.Transform;
using MatterHackers.VectorMath;
using System;

namespace MatterHackers.Csg.Solids
{
	/// <summary>
	/// Represents a 3D cylindrical or conical solid with specified radius/radii, height, number of facets (sides), and axis alignment.
	/// Supports regular cylinders, tapered cones, and creating cylinders directly from 2D/3D Line primitives.
	/// </summary>
	public class Cylinder : CsgObjectWrapper
	{
		private double height;

		public class CylinderPrimitive : Solid
		{
			public int Sides { get; }
			public double Radius1 { get; }
			public double Radius2 { get; }
			public double Height { get; }

			internal CylinderPrimitive(double radius1, double radius2, double height, int sides, string name)
				: base(name)
			{
				this.Radius1 = radius1;
				this.Radius2 = radius2;
				this.Height = height;
				this.Sides = sides;
			}

			internal CylinderPrimitive(CylinderPrimitive objectToCopy)
				: this(objectToCopy.Radius1, objectToCopy.Radius2, objectToCopy.Height, objectToCopy.Sides, objectToCopy.name)
			{
			}

			public override AxisAlignedBoundingBox GetAxisAlignedBoundingBox()
			{
				double maxRadius = Math.Max(Radius1, Radius2);
				return new AxisAlignedBoundingBox(new Vector3(-maxRadius, -maxRadius, -Height / 2), new Vector3(maxRadius, maxRadius, Height / 2));
			}
		}

		/// <summary>
		/// Creates a uniform 3D cylinder solid with equal top and bottom radius.
		/// </summary>
		/// <param name="radius">Radius of the cylinder.</param>
		/// <param name="height">Total height of the cylinder along its axis of alignment.</param>
		/// <param name="sides">Number of polygonal facets around the circumference (default: 30).</param>
		/// <param name="alignment">Orientation axis of the cylinder: Alignment.x, Alignment.y, or Alignment.z (default: Alignment.z).</param>
		/// <param name="name">Optional identifier name for the solid.</param>
		public Cylinder(double radius, double height, int sides = 30, Alignment alignment = Alignment.z, string name = "")
			: this(radius, radius, height, sides, alignment, name)
		{
		}

		/// <summary>
		/// Creates a 3D cylinder using the length/height of a Line primitive.
		/// </summary>
		/// <param name="line">Line primitive whose Height/Length determines the cylinder's height.</param>
		/// <param name="radius">Radius of the cylinder.</param>
		/// <param name="sides">Number of polygonal facets around the circumference (default: 30).</param>
		public Cylinder(Line line, double radius, int sides = 30)
			: this(radius, line != null ? line.Height : 0, sides)
		{
		}

		/// <summary>Bottom (or start) radius of the cylinder or truncated cone.</summary>
		public double Radius1 { get; }

		/// <summary>Top (or end) radius of the cylinder or truncated cone.</summary>
		public double Radius2 { get; }

		/// <summary>Radius of the cylinder (equals Radius1).</summary>
		public double Radius => Radius1;

		/// <summary>Total height of the cylinder along its alignment axis.</summary>
		public double Height => height;

		/// <summary>
		/// Creates a tapered 3D cylinder or cone with different bottom (radius1) and top (radius2) radii.
		/// </summary>
		/// <param name="radius1">Radius at the base / bottom.</param>
		/// <param name="radius2">Radius at the top (can be 0 for a sharp cone).</param>
		/// <param name="height">Total height along the alignment axis.</param>
		/// <param name="sides">Number of polygonal facets around the circumference (default: 30).</param>
		/// <param name="alignment">Orientation axis: Alignment.x, Alignment.y, or Alignment.z (default: Alignment.z).</param>
		/// <param name="name">Optional identifier name for the solid.</param>
		public Cylinder(double radius1, double radius2, double height, int sides = 30, Alignment alignment = Alignment.z, string name = "")
			: base(name)
		{
			this.height = height;
			this.Radius1 = radius1;
			this.Radius2 = radius2;
			root = new CylinderPrimitive(radius1, radius2, height, sides, name);
			switch (alignment)
			{
				case Alignment.x:
					root = new Rotate(root, y: MathHelper.DegreesToRadians(90));
					break;

				case Alignment.y:
					root = new Rotate(root, x: MathHelper.DegreesToRadians(90));
					break;
			}
		}

		/// <summary>
		/// Constructs a cylinder/strut directly along a Line segment.
		/// </summary>
		public static CsgObject CreateBetween(Line line, double radius, int sides = 16)
		{
			if (line == null) return new Sphere(radius);
			return CreateBetween(line.Start3D, line.End3D, radius, sides);
		}

		/// <summary>
		/// Constructs a cylinder/strut bridging directly between two Point objects.
		/// </summary>
		public static CsgObject CreateBetween(Point start, Point end, double radius, int sides = 16)
		{
			return CreateBetween(start?.Position3D ?? Vector3.Zero, end?.Position3D ?? Vector3.Zero, radius, sides);
		}

		/// <summary>
		/// Constructs a cylinder/strut bridging directly between two 3D coordinate points.
		/// </summary>
		public static CsgObject CreateBetween(Vector3 start, Vector3 end, double radius, int sides = 16)
		{
			Vector3 delta = end - start;
			double length = delta.Length;
			if (length < 1e-6)
			{
				return new Sphere(radius).Translate(start);
			}

			Vector3 mid = (start + end) * 0.5;

			if (Math.Abs(delta.X) < 1e-6 && Math.Abs(delta.Y) < 1e-6)
			{
				return new Cylinder(radius, length, sides, Alignment.z).Translate(mid);
			}
			if (Math.Abs(delta.Y) < 1e-6 && Math.Abs(delta.Z) < 1e-6)
			{
				return new Cylinder(radius, length, sides, Alignment.x).Translate(mid);
			}
			if (Math.Abs(delta.X) < 1e-6 && Math.Abs(delta.Z) < 1e-6)
			{
				return new Cylinder(radius, length, sides, Alignment.y).Translate(mid);
			}

			Vector3 zAxis = delta.GetNormal();
			Vector3 up = Math.Abs(zAxis.Z) < 0.99 ? Vector3.UnitZ : Vector3.UnitY;
			Vector3 xAxis = up.Cross(zAxis).GetNormal();
			Vector3 yAxis = zAxis.Cross(xAxis);

			Matrix4X4 mat = new Matrix4X4(
				new Vector4(xAxis, 0),
				new Vector4(yAxis, 0),
				new Vector4(zAxis, 0),
				new Vector4(mid, 1)
			);

			var baseCyl = new Cylinder(radius, length, sides, Alignment.z);
			return new TransformBase(baseCyl, mat);
		}
	}
}