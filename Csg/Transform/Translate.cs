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
	/// Represents a spatial translation (offset/movement) transformation applied to a 3D CSG object along the X, Y, and Z axes.
	/// </summary>
	public class Translate : TransformBase
	{
		private Vector3 translateCache;

		/// <summary>The current 3D translation offset vector.</summary>
		public Vector3 Translation
		{
			get
			{
				return translateCache;
			}
			set
			{
				translateCache = value;
				transform = Matrix4X4.CreateTranslation(value);
			}
		}

		/// <summary>
		/// Translates a CSG object by explicit X, Y, and Z offset distances.
		/// </summary>
		/// <param name="objectToTranslate">The CSG object to move in 3D space.</param>
		/// <param name="x">Translation distance along the X axis (default: 0).</param>
		/// <param name="y">Translation distance along the Y axis (default: 0).</param>
		/// <param name="z">Translation distance along the Z axis (default: 0).</param>
		/// <param name="name">Optional identifier name.</param>
		public Translate(CsgObject objectToTranslate, double x = 0, double y = 0, double z = 0, string name = "")
			: this(objectToTranslate, new Vector3(x, y, z), name)
		{
		}

		/// <summary>
		/// Translates a CSG object by a Vector3 displacement vector.
		/// </summary>
		/// <param name="objectToTranslate">The CSG object to move in 3D space.</param>
		/// <param name="translation">Vector containing the (X, Y, Z) translation offsets.</param>
		/// <param name="name">Optional identifier name.</param>
		public Translate(CsgObject objectToTranslate, Vector3 translation, string name = "")
			: base(objectToTranslate, name)
		{
			translateCache = translation;
			transform = Matrix4X4.CreateTranslation(translation);
		}
	}
}