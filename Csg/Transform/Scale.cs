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

namespace MatterHackers.Csg.Transform
{
	/// <summary>
	/// Represents a 3D scaling (resizing) transformation applied to a CSG object uniformly or non-uniformly along X, Y, and Z.
	/// </summary>
	public class Scale : TransformBase
	{
		private Vector3 scaleCache;

		/// <summary>Gets the 3D scale factors (X, Y, Z).</summary>
		public Vector3 GetScale()
		{
			return scaleCache;
		}

		/// <summary>
		/// Scales a CSG object uniformly by the same multiplier along all 3 axes (X, Y, Z).
		/// </summary>
		/// <param name="objectToScale">The CSG object to resize.</param>
		/// <param name="scale">Uniform scale factor (e.g. 2.0 = double size, 0.5 = half size).</param>
		/// <param name="name">Optional identifier name.</param>
		public Scale(CsgObject objectToScale, double scale, string name = "")
			: this(objectToScale, scale, scale, scale, name)
		{
		}

		/// <summary>
		/// Scales a CSG object non-uniformly with independent scale multipliers along X, Y, and Z.
		/// </summary>
		/// <param name="objectToScale">The CSG object to resize.</param>
		/// <param name="x">Scale factor along the X axis.</param>
		/// <param name="y">Scale factor along the Y axis.</param>
		/// <param name="z">Scale factor along the Z axis.</param>
		/// <param name="name">Optional identifier name.</param>
		public Scale(CsgObject objectToScale, double x, double y, double z, string name = "")
			: this(objectToScale, new Vector3(x, y, z), name)
		{
		}

		/// <summary>
		/// Scales a CSG object using a Vector3 containing the (X, Y, Z) scale factors.
		/// </summary>
		/// <param name="objectToScale">The CSG object to resize.</param>
		/// <param name="scale">Vector containing the (X, Y, Z) scale factors.</param>
		/// <param name="name">Optional identifier name.</param>
		public Scale(CsgObject objectToScale, Vector3 scale, string name = "")
			: base(objectToScale, name)
		{
			this.scaleCache = scale;
			transform = Matrix4X4.CreateScale(scale);
		}
	}
}