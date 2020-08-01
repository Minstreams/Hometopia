namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Core_Combine {




		#region --- SUB ---



		public class Result {

			public List<Material> Materials;
			public List<Mesh> Meshs;
			public Texture2D Texture;
			public Transform Root;

		}



		public struct MaterialData {

			public struct PropertyData {

				public string Name;
				public UnityEditor.ShaderUtil.ShaderPropertyType Type;
				public Color ColorValue;
				public float FloatValue;
				public Vector3 Vector3Value;


				public bool SameProperty (PropertyData prop) {
					if (Name != prop.Name || Type != prop.Type) {
						return false;
					}
					switch (Type) {
						default:
							return false;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
							return ColorValue == prop.ColorValue;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
						case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
							return FloatValue == prop.FloatValue;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
							return Vector3Value == prop.Vector3Value;
					}
				}


			}



			public string ShaderName;
			public PropertyData[] Properties;


			public MaterialData (Material mat) {

				ShaderName = "";
				Properties = null;

				if (!mat || !mat.shader) { return; }

				ShaderName = mat.shader.name;
				var propList = new List<PropertyData>();
				int count = UnityEditor.ShaderUtil.GetPropertyCount(mat.shader);
				for (int i = 0; i < count; i++) {
					var propData = new PropertyData {
						Name = UnityEditor.ShaderUtil.GetPropertyName(mat.shader, i),
						Type = UnityEditor.ShaderUtil.GetPropertyType(mat.shader, i),
					};
					switch (propData.Type) {
						default:
							continue;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
							propData.ColorValue = mat.GetColor(propData.Name);
							break;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
						case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
							propData.FloatValue = mat.GetFloat(propData.Name);
							break;
						case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
							propData.Vector3Value = mat.GetVector(propData.Name);
							break;
					}
					propList.Add(propData);
				}
				Properties = propList.ToArray();

			}

			public bool SameMaterial (MaterialData data) {
				if (ShaderName != data.ShaderName || (Properties == null) != (data.Properties == null)) {
					return false;
				}
				if (Properties != null && Properties.Length == data.Properties.Length) {
					for (int i = 0; i < Properties.Length; i++) {
						if (!Properties[i].SameProperty(data.Properties[i])) {
							return false;
						}
					}
				}
				return true;
			}

			public Material GetMaterial (Texture2D texture, string mainTexKeyword) {
				var mat = new Material(Shader.Find(ShaderName));
				if (Properties != null) {
					for (int i = 0; i < Properties.Length; i++) {
						var prop = Properties[i];
						if (mat.HasProperty(prop.Name)) {
							switch (prop.Type) {
								default:
									continue;
								case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
									mat.SetColor(prop.Name, prop.ColorValue);
									break;
								case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
								case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
									mat.SetFloat(prop.Name, prop.FloatValue);
									break;
								case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
									mat.SetVector(prop.Name, prop.Vector3Value);
									break;
							}
						}
					}
				}
				mat.SetTexture(mainTexKeyword, texture);
				return mat;
			}

		}



		public class RendererList {



			public class RendererData {

				public Transform Object;
				public Mesh Mesh;
				public Texture2D Texture;
				public int PackingIndex;
				public Rect RemapUV;

				public RendererData (Renderer rd, string mainTexKeyword) {

					Object = null;
					Mesh = null;
					Texture = null;

					if (!rd) { return; }

					Mesh mesh = null;
					Texture2D texture = null;

					if (rd is SkinnedMeshRenderer) {
						mesh = (rd as SkinnedMeshRenderer).sharedMesh;
					} else if (rd is MeshRenderer) {
						var mf = rd.GetComponent<MeshFilter>();
						if (mf) {
							mesh = mf.sharedMesh;
						}
					}

					var mat = rd.sharedMaterial;
					texture = mat ? mat.GetTexture(mainTexKeyword) as Texture2D : null;

					if (mesh && mat) {
						Object = rd.transform;
						Texture = texture;
						Mesh = mesh;
					}
				}

			}



			public List<RendererData> List;


		}




		#endregion




		#region --- API ---



		public static Result Combine (Transform root, bool combineMesh, System.Action<string, float> progressLog, string mainTexKeyword) {

			var oldScale = root.localScale;
			var oldPosition = root.position;
			var oldRotation = root.rotation;
			root.localScale = Vector3.one;
			root.position = Vector3.zero;
			root.rotation = Quaternion.identity;

			var mrs = root.GetComponentsInChildren<MeshRenderer>(false);
			var srs = root.GetComponentsInChildren<SkinnedMeshRenderer>(false);
			if (mrs.Length + srs.Length == 0) { return null; }

			// Init Map
			var matMap = new Dictionary<MaterialData, RendererList>();
			AddToMap(matMap, mrs, mainTexKeyword);
			AddToMap(matMap, srs, mainTexKeyword);

			// Get Packed Texture
			progressLog.Invoke("Packing Textures", 0.333f);
			Texture2D texture;
			var newUV = GetPackedTexture(matMap, out texture);

			// Get New Materials and Meshs
			progressLog.Invoke("Creating Meshes", 0.666f);
			var result = GetResult(matMap, texture, newUV, combineMesh, mainTexKeyword);

			root.localScale = oldScale;
			root.position = oldPosition;
			root.rotation = oldRotation;

			return result;
		}



		#endregion




		#region --- LGC ---



		private static void AddToMap (Dictionary<MaterialData, RendererList> matMap, Renderer[] renders, string mainTexKeyword) {
			for (int i = 0; i < renders.Length; i++) {
				var mat = renders[i].sharedMaterial;
				if (!mat) { continue; }
				var matData = new MaterialData(mat);
				bool hasSame = false;
				foreach (var pair in matMap) {
					if (pair.Key.SameMaterial(matData)) {
						var rData = new RendererList.RendererData(renders[i], mainTexKeyword);
						if (rData.Object) {
							pair.Value.List.Add(rData);
						}
						hasSame = true;
						break;
					}
				}
				if (!hasSame) {
					matMap.Add(
						matData,
						new RendererList() {
							List = new List<RendererList.RendererData>() {
								new RendererList.RendererData(renders[i],mainTexKeyword)
							}
						}
					);
				}
			}
		}



		private static Rect[] GetPackedTexture (Dictionary<MaterialData, RendererList> matMap, out Texture2D texture) {

			var packingList = new List<PackingData>();

			foreach (var pair in matMap) {
				for (int index = 0; index < pair.Value.List.Count; index++) {
					var rData = pair.Value.List[index];
					if (rData.Mesh == null) {
						continue;
					}
					var uvs = rData.Mesh.uv;
					// Get Remap UV
					var minmax = new Vector4(1, 1, 0, 0);
					Vector2 uv;
					for (int i = 0; i < uvs.Length; i++) {
						uv = uvs[i];
						minmax.x = Mathf.Min(minmax.x, uv.x);
						minmax.y = Mathf.Min(minmax.y, uv.y);
						minmax.z = Mathf.Max(minmax.z, uv.x);
						minmax.w = Mathf.Max(minmax.w, uv.y);
					}
					var remapUV = new Rect(minmax.x, minmax.y, minmax.z - minmax.x, minmax.w - minmax.y);
					rData.RemapUV = remapUV;

					// Get New Texture
					if (rData.Texture) {
						int sourceWidth = rData.Texture.width;
						int sourceHeight = rData.Texture.height;
						int width = (int)(sourceWidth * remapUV.width);
						int height = (int)(sourceHeight * remapUV.height);
						var colors = rData.Texture.GetPixels(
							(int)(remapUV.x * sourceWidth),
							(int)(remapUV.y * sourceHeight),
							width,
							height
						);
						rData.PackingIndex = packingList.Count;
						packingList.Add(new PackingData(width, height, colors));
					} else {
						rData.PackingIndex = packingList.Count;
						packingList.Add(new PackingData(1, 1, new Color[1]));
					}
				}
			}

			return RectPacking.PackTextures(out texture, packingList, true, false);
		}



		private static Result GetResult (Dictionary<MaterialData, RendererList> matMap, Texture2D texture, Rect[] newUV, bool combineMesh, string mainTexKeyword) {
			var result = new Result() {
				Meshs = new List<Mesh>(),
				Materials = new List<Material>(),
				Texture = texture,
				Root = new GameObject("Root").transform,
			};

			result.Root.position = Vector3.zero;
			result.Root.rotation = Quaternion.identity;
			result.Root.localScale = Vector3.one;
			texture.name = "Texture";
			int matIndex = 0;

			foreach (var pair in matMap) {

				// New Material
				var mat = pair.Key.GetMaterial(texture, mainTexKeyword);
				mat.name = "Material " + matIndex.ToString();
				result.Materials.Add(mat);

				// Mesh Data
				List<Vector3> _verts = null;
				List<Vector2> _uvs = null;
				if (combineMesh) {
					_verts = new List<Vector3>();
					_uvs = new List<Vector2>();
				}

				for (int index = 0; index < pair.Value.List.Count; index++) {

					var rData = pair.Value.List[index];

					// New Mesh
					var remap = newUV[rData.PackingIndex];
					var sourceUV = rData.Mesh.uv;
					var uvs = new Vector2[sourceUV.Length];
					for (int i = 0; i < sourceUV.Length; i++) {
						uvs[i] = new Vector2(
							((sourceUV[i].x - rData.RemapUV.x) / rData.RemapUV.width) * remap.width + remap.x,
							((sourceUV[i].y - rData.RemapUV.y) / rData.RemapUV.height) * remap.height + remap.y
						);
					}

					if (combineMesh) {
						var sourceVerts = rData.Mesh.vertices;
						var vs = new Vector3[sourceVerts.Length];
						for (int i = 0; i < vs.Length; i++) {
							vs[i] = rData.Object.TransformPoint(sourceVerts[i]);
						}
						_verts.AddRange(vs);
						_uvs.AddRange(uvs);
					} else {

						// Mesh
						var mesh = new Mesh {
							name = "Mesh " + matIndex.ToString() + "-" + index.ToString(),
							vertices = rData.Mesh.vertices,
							triangles = rData.Mesh.triangles,
							colors = rData.Mesh.colors,
						};
						mesh.uv = uvs;
						mesh.RecalculateNormals();
						mesh.UploadMeshData(false);
						result.Meshs.Add(mesh);

						// New Object in Root
						var tf = new GameObject(rData.Object.name, typeof(MeshRenderer), typeof(MeshFilter)).transform;
						tf.SetParent(result.Root);
						tf.position = rData.Object.position;
						tf.rotation = rData.Object.rotation;
						tf.localScale = rData.Object.lossyScale;

						// Renderer
						var mr = tf.GetComponent<MeshRenderer>();
						var mf = tf.GetComponent<MeshFilter>();
						mr.material = mat;
						mf.mesh = mesh;

					}

				}

				if (combineMesh) {
					var mesh = new UnlimitiedMesh(_verts, _uvs);
					for (int i = 0; i < mesh.Count; i++) {

						var _m = mesh.GetMeshAt(i);
						_m.name = "Mesh " + matIndex.ToString() + "-" + i.ToString();
						result.Meshs.Add(_m);

						// New Object in Root
						var tf = new GameObject("Combined Model " + i.ToString(), typeof(MeshRenderer), typeof(MeshFilter)).transform;
						tf.SetParent(result.Root);
						tf.position = Vector3.zero;
						tf.rotation = Quaternion.identity;
						tf.localScale = Vector3.one;

						// Renderer
						var mr = tf.GetComponent<MeshRenderer>();
						var mf = tf.GetComponent<MeshFilter>();
						mr.material = mat;
						mf.mesh = _m;

					}
				}

				matIndex++;

			}
			return result;
		}





		#endregion



	}
}