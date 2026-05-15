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

using MatterHackers.Csg;
using MatterHackers.Csg.Operations;
using MatterHackers.Csg.Solids;
using MatterHackers.Csg.Transform;
using MatterHackers.PolygonMesh;
using MatterHackers.VectorMath;
using System;
using System.Threading;

namespace MatterHackers.RenderOpenGl
{
	public class CsgToMesh
	{
		public static PolygonMesh.Mesh Convert(CsgObject objectToProcess, bool cleanMeshAfterConvert = false)
		{
			CsgToMesh visitor = new CsgToMesh();
			var mesh = visitor.CsgToMeshRecursive((dynamic)objectToProcess);
			if(cleanMeshAfterConvert)
			{
				mesh.CleanAndMergMesh(CancellationToken.None);
			}

			return mesh;
		}

		public CsgToMesh()
		{
		}

		#region Visitor Pattern Functions

		public PolygonMesh.Mesh CsgToMeshRecursive(CsgObject objectToProcess)
		{
			throw new Exception("You must write the specialized function for this type.");
		}

		#region PrimitiveWrapper

		public PolygonMesh.Mesh CsgToMeshRecursive(CsgObjectWrapper objectToProcess)
		{
			return CsgToMeshRecursive((dynamic)objectToProcess.Root);
		}

		#endregion PrimitiveWrapper

		#region Box

		public static PolygonMesh.Mesh CreateBox(AxisAlignedBoundingBox aabb)
		{
			PolygonMesh.Mesh cube = new PolygonMesh.Mesh();
			cube.Vertices.Add(new Vector3(aabb.MinXYZ.X, aabb.MinXYZ.Y, aabb.MaxXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MaxXYZ.X, aabb.MinXYZ.Y, aabb.MaxXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MaxXYZ.X, aabb.MaxXYZ.Y, aabb.MaxXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MinXYZ.X, aabb.MaxXYZ.Y, aabb.MaxXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MinXYZ.X, aabb.MinXYZ.Y, aabb.MinXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MaxXYZ.X, aabb.MinXYZ.Y, aabb.MinXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MaxXYZ.X, aabb.MaxXYZ.Y, aabb.MinXYZ.Z));
			cube.Vertices.Add(new Vector3(aabb.MinXYZ.X, aabb.MaxXYZ.Y, aabb.MinXYZ.Z));

			cube.Faces.Add(0, 1, 2, cube.Vertices);
			cube.Faces.Add(0, 2, 3, cube.Vertices);
			cube.Faces.Add(4, 0, 3, cube.Vertices);
			cube.Faces.Add(4, 3, 7, cube.Vertices);
			cube.Faces.Add(1, 5, 6, cube.Vertices);
			cube.Faces.Add(1, 6, 2, cube.Vertices);
			cube.Faces.Add(4, 7, 6, cube.Vertices);
			cube.Faces.Add(4, 6, 5, cube.Vertices);
			cube.Faces.Add(3, 2, 6, cube.Vertices);
			cube.Faces.Add(3, 6, 7, cube.Vertices);
			cube.Faces.Add(4, 5, 1, cube.Vertices);
			cube.Faces.Add(4, 1, 0, cube.Vertices);

			return cube;
		}

		public PolygonMesh.Mesh CsgToMeshRecursive(Csg.Solids.MeshContainer objectToPrecess)
		{
			return objectToPrecess.GetMesh();
		}

		public PolygonMesh.Mesh CsgToMeshRecursive(BoxPrimitive objectToProcess)
		{
			return CreateBox(objectToProcess.GetAxisAlignedBoundingBox());
		}

		#endregion Box

		#region Cylinder

		public static PolygonMesh.Mesh CreateCylinder(Cylinder.CylinderPrimitive cylinderToMeasure)
		{
			if (cylinderToMeasure.Sides < 3)
			{
				throw new ArgumentOutOfRangeException(nameof(cylinderToMeasure), "Cylinder must have at least 3 sides.");
			}

			PolygonMesh.Mesh cylinder = new PolygonMesh.Mesh();
			int sides = cylinderToMeasure.Sides;
			double bottomZ = -cylinderToMeasure.Height / 2;
			double topZ = cylinderToMeasure.Height / 2;
			bool hasBottomRadius = Math.Abs(cylinderToMeasure.Radius1) > double.Epsilon;
			bool hasTopRadius = Math.Abs(cylinderToMeasure.Radius2) > double.Epsilon;

			int bottomStart = cylinder.Vertices.Count;
			for (int i = 0; i < sides; i++)
			{
				Vector2 radialPosition = Vector2.Rotate(new Vector2(cylinderToMeasure.Radius1, 0), MathHelper.Tau * i / sides);
				cylinder.Vertices.Add(new Vector3(radialPosition.X, radialPosition.Y, bottomZ));
			}

			int topStart = cylinder.Vertices.Count;
			for (int i = 0; i < sides; i++)
			{
				Vector2 radialPosition = Vector2.Rotate(new Vector2(cylinderToMeasure.Radius2, 0), MathHelper.Tau * i / sides);
				cylinder.Vertices.Add(new Vector3(radialPosition.X, radialPosition.Y, topZ));
			}

			int bottomCenter = cylinder.Vertices.Count;
			cylinder.Vertices.Add(new Vector3(0, 0, bottomZ));

			int topCenter = cylinder.Vertices.Count;
			cylinder.Vertices.Add(new Vector3(0, 0, topZ));

			for (int i = 0; i < sides; i++)
			{
				int next = (i + 1) % sides;
				int bottom = bottomStart + i;
				int nextBottom = bottomStart + next;
				int top = topStart + i;
				int nextTop = topStart + next;

				if (hasBottomRadius && hasTopRadius)
				{
					cylinder.Faces.Add(top, bottom, nextBottom, cylinder.Vertices);
					cylinder.Faces.Add(top, nextBottom, nextTop, cylinder.Vertices);
				}
				else if (hasBottomRadius)
				{
					cylinder.Faces.Add(top, bottom, nextBottom, cylinder.Vertices);
				}
				else if (hasTopRadius)
				{
					cylinder.Faces.Add(top, bottom, nextTop, cylinder.Vertices);
				}

				if (hasBottomRadius)
				{
					cylinder.Faces.Add(bottomCenter, nextBottom, bottom, cylinder.Vertices);
				}

				if (hasTopRadius)
				{
					cylinder.Faces.Add(topCenter, top, nextTop, cylinder.Vertices);
				}
			}

			return cylinder;
		}

		public PolygonMesh.Mesh CsgToMeshRecursive(Cylinder.CylinderPrimitive objectToProcess)
		{
			return CreateCylinder(objectToProcess);
		}

		#endregion Cylinder

		#region NGonExtrusion

		public PolygonMesh.Mesh CsgToMeshRecursive(NGonExtrusion.NGonExtrusionPrimitive objectToProcess)
		{
			int sides = (int)Math.Round(objectToProcess.NumSides);
			var cylinder = new Cylinder.CylinderPrimitive(
				objectToProcess.Radius1,
				objectToProcess.Radius1,
				objectToProcess.Height,
				sides,
				string.Empty);

			return CreateCylinder(cylinder);
		}

		#endregion NGonExtrusion

		#region Sphere

		public PolygonMesh.Mesh CsgToMeshRecursive(Sphere objectToProcess)
		{
			const int longitudeSegments = 40;
			const int latitudeSegments = longitudeSegments / 2;
			double radius = objectToProcess.Radius;
			PolygonMesh.Mesh sphere = new PolygonMesh.Mesh();

			int top = sphere.Vertices.Count;
			sphere.Vertices.Add(new Vector3(0, 0, radius));

			for (int latitude = 1; latitude < latitudeSegments; latitude++)
			{
				double theta = Math.PI * latitude / latitudeSegments;
				double ringRadius = radius * Math.Sin(theta);
				double z = radius * Math.Cos(theta);

				for (int longitude = 0; longitude < longitudeSegments; longitude++)
				{
					double phi = MathHelper.Tau * longitude / longitudeSegments;
					sphere.Vertices.Add(new Vector3(ringRadius * Math.Cos(phi), ringRadius * Math.Sin(phi), z));
				}
			}

			int bottom = sphere.Vertices.Count;
			sphere.Vertices.Add(new Vector3(0, 0, -radius));

			int RingIndex(int latitude, int longitude)
			{
				return 1 + (latitude - 1) * longitudeSegments + longitude % longitudeSegments;
			}

			for (int longitude = 0; longitude < longitudeSegments; longitude++)
			{
				int next = (longitude + 1) % longitudeSegments;
				sphere.Faces.Add(top, RingIndex(1, longitude), RingIndex(1, next), sphere.Vertices);
			}

			for (int latitude = 1; latitude < latitudeSegments - 1; latitude++)
			{
				for (int longitude = 0; longitude < longitudeSegments; longitude++)
				{
					int next = (longitude + 1) % longitudeSegments;
					int upper = RingIndex(latitude, longitude);
					int upperNext = RingIndex(latitude, next);
					int lower = RingIndex(latitude + 1, longitude);
					int lowerNext = RingIndex(latitude + 1, next);

					sphere.Faces.Add(upper, lower, lowerNext, sphere.Vertices);
					sphere.Faces.Add(upper, lowerNext, upperNext, sphere.Vertices);
				}
			}

			for (int longitude = 0; longitude < longitudeSegments; longitude++)
			{
				int next = (longitude + 1) % longitudeSegments;
				sphere.Faces.Add(bottom, RingIndex(latitudeSegments - 1, next), RingIndex(latitudeSegments - 1, longitude), sphere.Vertices);
			}

			return sphere;
		}

		#endregion Sphere

		#region Transform

		public PolygonMesh.Mesh CsgToMeshRecursive(TransformBase objectToProcess)
		{
			PolygonMesh.Mesh mesh = CsgToMeshRecursive((dynamic)objectToProcess.ObjectToTransform);
			mesh.Transform(objectToProcess.ActiveTransform);
			return mesh;
		}

		#endregion Transform

		#region Union

		public PolygonMesh.Mesh CsgToMeshRecursive(Union objectToProcess)
		{
			PolygonMesh.Mesh primary = CsgToMeshRecursive((dynamic)objectToProcess.AllObjects[0]);
			for (int i = 1; i < objectToProcess.AllObjects.Count; i++)
			{
				PolygonMesh.Mesh add = CsgToMeshRecursive((dynamic)objectToProcess.AllObjects[i]);
				primary = PolygonMesh.Csg.CsgOperations.Union(primary, add);
			}

			return primary;
		}

		#endregion Union

		#region Difference

		public PolygonMesh.Mesh CsgToMeshRecursive(Difference objectToProcess)
		{
			PolygonMesh.Mesh primary = CsgToMeshRecursive((dynamic)objectToProcess.Primary);
			foreach (CsgObject objectToOutput in objectToProcess.AllSubtracts)
			{
				PolygonMesh.Mesh remove = CsgToMeshRecursive((dynamic)objectToOutput);
				primary = PolygonMesh.Csg.CsgOperations.Subtract(primary, remove);
			}

			return primary;
		}

		#endregion Difference

		#region Intersection

		public PolygonMesh.Mesh CsgToMeshRecursive(Intersection objectToProcess)
		{
			PolygonMesh.Mesh a = CsgToMeshRecursive((dynamic)objectToProcess.a);
			PolygonMesh.Mesh b = CsgToMeshRecursive((dynamic)objectToProcess.b);
			return PolygonMesh.Csg.CsgOperations.Intersect(a, b);
		}

		#endregion Intersection

		#endregion Visitor Pattern Functions
	}
}
