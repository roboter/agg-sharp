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

using MatterHackers.VectorMath;

namespace MatterHackers.Csg.Solids
{
	/// <summary>
	/// Represents a 3D spherical solid centered at the origin (0, 0, 0) with a specified radius.
	/// </summary>
	public class Sphere : Solid
	{
		internal double radius;

		/// <summary>Radius of the sphere.</summary>
		public double Radius { get { return radius; } }

		/// <summary>
		/// Creates a new 3D sphere solid with specified radius.
		/// </summary>
		/// <param name="radius">Radius of the sphere from the origin.</param>
		/// <param name="name">Optional identifier name for the solid.</param>
		public Sphere(double radius, string name = "")
			: base(name)
		{
			this.radius = radius;
		}

		/// <summary>
		/// Creates a copy of an existing 3D sphere solid.
		/// </summary>
		/// <param name="objectToCopy">The source sphere to duplicate.</param>
		public Sphere(Sphere objectToCopy)
			: this(objectToCopy.radius, objectToCopy.name)
		{
		}

		public override AxisAlignedBoundingBox GetAxisAlignedBoundingBox()
		{
			return new AxisAlignedBoundingBox(new Vector3(-radius, -radius, -radius), new Vector3(radius, radius, radius));
		}
	}
}