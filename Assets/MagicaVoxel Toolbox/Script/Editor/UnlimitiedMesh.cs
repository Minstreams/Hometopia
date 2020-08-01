namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class UnlimitiedMesh {


		// Global
		private const int MAX_VERTEX_COUNT = 65532;


		// API
		public int Count {
			get {
				return Meshs.Count;
			}
		}

		public Mesh this[int index] {
			get {
				return GetMeshAt(index);
			}
		}


		// Data
		private List<Mesh> Meshs = new List<Mesh>();
		private List<int> MaterialIndex = new List<int>();



		// API
		public UnlimitiedMesh (List<List<Vector3>> verticess, List<List<Vector2>> uvss, List<BoneWeight> boneWeights = null) {

			if (verticess == null || uvss == null || verticess.Count != uvss.Count) { return; }

			Meshs = new List<Mesh>();
			MaterialIndex = new List<int>();

			for (int matIndex = 0; matIndex < verticess.Count; matIndex++) {

				var vertices = verticess[matIndex];
				var uvs = uvss[matIndex];
				int vCount = vertices.Count;
				int meshNum = vCount / MAX_VERTEX_COUNT + 1;

				for (int index = 0; index < meshNum; index++) {

					var mesh = new Mesh();

					// Vertices
					int vertCount = Mathf.Min(MAX_VERTEX_COUNT, vertices.Count - index * MAX_VERTEX_COUNT);
					mesh.SetVertices(vertices.GetRange(index * MAX_VERTEX_COUNT, vertCount));

					// UV
					mesh.SetUVs(0, uvs.GetRange(
						index * MAX_VERTEX_COUNT,
						Mathf.Min(MAX_VERTEX_COUNT, uvs.Count - index * MAX_VERTEX_COUNT)
					));

					// Tri
					mesh.SetTriangles(GetDefaultTriangleData(vertCount), 0);
					mesh.UploadMeshData(false);

					// Color
					mesh.colors = GetWhiteColors(vertCount);

					// BoneWeights
					if (boneWeights != null && boneWeights.Count > 0) {
						mesh.boneWeights = boneWeights.GetRange(index * MAX_VERTEX_COUNT, vertCount).ToArray();
					}

					mesh.RecalculateNormals();
					mesh.RecalculateTangents();
					mesh.RecalculateBounds();
					mesh.UploadMeshData(false);

					Meshs.Add(mesh);
					MaterialIndex.Add(matIndex);

				}
				
			}

		}



		public UnlimitiedMesh (List<Vector3> vertices, List<Vector2> uvs, List<BoneWeight> boneWeights = null) : this(
			new List<List<Vector3>>() { vertices }, new List<List<Vector2>>() { uvs }, boneWeights
		) { }



		public Mesh GetMeshAt (int index) {
			return Meshs[index];
		}



		public int GetMaterialIndexAt (int index) {
			return MaterialIndex[index];
		}


		private int[] GetDefaultTriangleData (int verCount) {
			int quadCount = verCount / 4;
			int[] result = new int[quadCount * 6];
			for (int i = 0; i < quadCount; i++) {
				result[i * 6] = i * 4;
				result[i * 6 + 1] = i * 4 + 1;
				result[i * 6 + 2] = i * 4 + 2;
				result[i * 6 + 3] = i * 4;
				result[i * 6 + 4] = i * 4 + 2;
				result[i * 6 + 5] = i * 4 + 3;
			}
			return result;
		}



		private Color[] GetWhiteColors (int verCount) {
			var colors = new Color[verCount];
			Color c = Color.white;
			for (int i = 0; i < verCount; i++) {
				colors[i] = c;
			}
			return colors;
		}




	}
}