using Objects.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonReporting
{
	//thanks ChatGPT
	public class MeshCalcs
	{

		public static double CalculateVolume(Mesh mesh)
		{
			if (mesh.applicationId == "rhino" || mesh.applicationId == "grasshopper") throw new NotImplementedException();

			double volume = 0;

			var vertices = mesh.vertices;
			var faces = mesh.faces;


			int vertexSize = 3; 

			for (int i = 0; i < faces.Count; i++)
			{
				int faceSize = faces[i];
				int[] faceIndices = new int[faceSize];

				faces.CopyTo(i + 1, faceIndices, 0, faceSize);

				for (int j = 0; j < faceSize - 2; j++)
				{
					int index1 = faceIndices[0];
					int index2 = faceIndices[j + 1];
					int index3 = faceIndices[j + 2];

					double[] p1 = GetVertex(index1, vertices, vertexSize);
					double[] p2 = GetVertex(index2, vertices, vertexSize);
					double[] p3 = GetVertex(index3, vertices, vertexSize);

					volume += TetrahedronVolume(p1, p2, p3);
				}

				i += faceSize;
			}

			return volume;
		}

		private static double[] GetVertex(int index, List<double> vertices, int vertexSize)
		{
			double[] vertex = new double[vertexSize];
			int startIndex = index * vertexSize;

			for (int i = 0; i < vertexSize; i++)
			{
				vertex[i] = vertices[startIndex + i];
			}

			return vertex;
		}

		private static double TetrahedronVolume(double[] p1, double[] p2, double[] p3)
		{
			double volume = 0;

			for (int i = 0; i < 3; i++) 
			{
				volume += (p1[i] * p2[(i + 1) % 3] * p3[(i + 2) % 3]) -
						  (p1[i] * p3[(i + 1) % 3] * p2[(i + 2) % 3]);
			}

			return Math.Abs(volume) / 6.0;
		}
	}

}
