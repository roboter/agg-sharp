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
using System.Collections.Generic;

namespace MatterHackers.Csg.Operations
{
	/// <summary>
	/// Represents a CSG Boolean Union (addition / merge) operation combining two or more 3D objects into a single solid.
	/// In code, you can also use the '+' operator: objectA + objectB.
	/// </summary>
	public class Union : CsgObject
	{
		internal List<CsgObject> allObjects = new List<CsgObject>();

		/// <summary>Gets the list of all participating CSG objects in this union.</summary>
		public List<CsgObject> AllObjects { get { return allObjects; } }

		/// <summary>
		/// Creates an empty Union container.
		/// </summary>
		/// <param name="name">Optional identifier name.</param>
		public Union(string name = "")
			: base(name)
		{
		}

		/// <summary>
		/// Combines two CSG objects into a unified solid (A + B).
		/// </summary>
		/// <param name="a">The first solid.</param>
		/// <param name="b">The second solid.</param>
		/// <param name="name">Optional identifier name.</param>
		public Union(CsgObject a, CsgObject b, string name = "")
			: base(name)
		{
			if (a is Union)
			{
				foreach (CsgObject objectsFromUnion in ((Union)a).allObjects)
				{
					allObjects.Add(objectsFromUnion);
				}
			}
			else
			{
				allObjects.Add(a);
			}
			if (b is Union)
			{
				foreach (CsgObject objectsFromUnion in ((Union)b).allObjects)
				{
					allObjects.Add(objectsFromUnion);
				}
			}
			else
			{
				allObjects.Add(b);
			}
		}

		public override AxisAlignedBoundingBox GetAxisAlignedBoundingBox()
		{
			AxisAlignedBoundingBox allBounds = allObjects[0].GetAxisAlignedBoundingBox();
			for (int i = 1; i < allObjects.Count; i++)
			{
				allBounds = AxisAlignedBoundingBox.Union(allObjects[i].GetAxisAlignedBoundingBox(), allBounds);
			}
			return allBounds;
		}

		public void Add(CsgObject objectToAdd)
		{
			allObjects.Add(objectToAdd);
		}
	}
}