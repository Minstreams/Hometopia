namespace MagicaVoxelToolbox {
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	public struct Util {



		#region --- File ---



		public static string Read (string path) {
			path = FixPath(path, false);
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}



		public static void Write (string data, string path) {
			path = FixPath(path, false);
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}




		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				path = FixPath(path, false);
				Directory.CreateDirectory(path);
			}
		}





		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				path = FixPath(path, false);
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}



		public static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			path = FixPath(path, false);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}




		public static bool HasFileIn (string path, params string[] searchPattern) {
			if (PathIsDirectory(path)) {
				for (int i = 0; i < searchPattern.Length; i++) {
					path = FixPath(path, false);
					if (new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories).Length > 0) {
						return true;
					}
				}
			}
			return false;
		}



		public static FileInfo[] GetFilesIn (string path, params string[] searchPattern) {
			List<FileInfo> allFiles = new List<FileInfo>();
			path = FixPath(path, false);
			if (PathIsDirectory(path)) {
				if (searchPattern.Length > 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories));
					}
				}
			}
			return allFiles.ToArray();
		}



		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				path = FixPath(path, false);
				File.Delete(path);
			}
		}



		#endregion



		#region --- Path ---



		private const string ROOT_NAME = "MagicaVoxel Toolbox";


		public static string GetRootPath (ScriptableObject scriptObj) {
			var rootPath = "";
			var script = MonoScript.FromScriptableObject(scriptObj);
			if (script) {
				var path = AssetDatabase.GetAssetPath(script);
				string rootName = ROOT_NAME;
				if (!string.IsNullOrEmpty(path)) {
					int index = path.LastIndexOf(rootName);
					if (index >= 0) {
						rootPath = path.Substring(0, index + rootName.Length);
					}
				}
			}
			return rootPath;
		}



		public static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[path.Length - 1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}



		public static string GetParentPath (string path) {
			path = FixPath(path, false);
			return FixedRelativePath(Directory.GetParent(path).FullName);
		}




		public static string FixedRelativePath (string path) {
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path.Substring(fixedDataPath.Length);
			} else {
				return "";
			}
		}




		public static string GetFullPath (string path) {
			path = FixPath(path, false);
			return new FileInfo(path).FullName;
		}



		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}



		public static string GetExtension (string path) {
			return Path.GetExtension(path);//.txt
		}



		public static string GetNameWithoutExtension (string path) {
			return Path.GetFileNameWithoutExtension(path);
		}


		public static string GetNameWithExtension (string path) {
			return Path.GetFileName(path);
		}


		public static string ChangeExtension (string path, string newEx) {
			return Path.ChangeExtension(path, newEx);
		}



		public static bool DirectoryExists (string path) {
			path = FixPath(path, false);
			return Directory.Exists(path);
		}



		public static bool FileExists (string path) {
			path = FixPath(path, false);
			return File.Exists(path);
		}



		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			path = FixPath(path, false);
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}



		public static bool IsChildPath (string pathA, string pathB) {
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathCompair(pathA, pathB);
			} else {
				return IsChildPathCompair(pathB, pathA);
			}
		}



		public static bool IsChildPathCompair (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}



		#endregion



		#region --- Message ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {
			//EditorApplication.Beep();
			PauseWatch();
			if (string.IsNullOrEmpty(cancel)) {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok);
				RestartWatch();
				return sure;
			} else {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok, cancel);
				RestartWatch();
				return sure;
			}
		}


		public static int DialogComplex (string title, string msg, string ok, string cancel, string alt) {
			//EditorApplication.Beep();
			PauseWatch();
			int index = EditorUtility.DisplayDialogComplex(title, msg, ok, cancel, alt);
			RestartWatch();
			return index;
		}


		public static void ProgressBar (string title, string msg, float value) {
			value = Mathf.Clamp01(value);
			EditorUtility.DisplayProgressBar(title, msg, value);
		}


		public static void ClearProgressBar () {
			EditorUtility.ClearProgressBar();
		}



		#endregion



		#region --- Watch ---


		private static System.Diagnostics.Stopwatch TheWatch;


		public static void StartWatch () {
			TheWatch = new System.Diagnostics.Stopwatch();
			TheWatch.Start();
		}


		public static void PauseWatch () {
			if (TheWatch != null) {
				TheWatch.Stop();
			}
		}


		public static void RestartWatch () {
			if (TheWatch != null) {
				TheWatch.Start();
			}
		}


		public static double StopWatchAndGetTime () {
			if (TheWatch != null) {
				TheWatch.Stop();
				return TheWatch.Elapsed.TotalSeconds;
			}
			return 0f;
		}


		#endregion



		#region --- Misc ---


		public static bool IsTypingInGUI () {
			return GUIUtility.keyboardControl != 0;
		}



		public static bool NoFuncKeyPressing () {
			return !Event.current.alt && !Event.current.control && !Event.current.shift;
		}



		public static Mesh CreateConeMesh (float radius, float height, int subdivisions = 12) {
			Mesh mesh = new Mesh();

			Vector3[] vertices = new Vector3[subdivisions + 2];
			Vector2[] uv = new Vector2[vertices.Length];
			int[] triangles = new int[(subdivisions * 2) * 3];

			vertices[0] = Vector3.zero;
			uv[0] = new Vector2(0.5f, 0f);
			for (int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
				float ratio = (float)i / n;
				float r = ratio * (Mathf.PI * 2f);
				float x = Mathf.Cos(r) * radius;
				float z = Mathf.Sin(r) * radius;
				vertices[i + 1] = new Vector3(x, 0f, z);
				uv[i + 1] = new Vector2(ratio, 0f);
			}
			vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
			uv[subdivisions + 1] = new Vector2(0.5f, 1f);

			// construct bottom

			for (int i = 0, n = subdivisions - 1; i < n; i++) {
				int offset = i * 3;
				triangles[offset] = 0;
				triangles[offset + 1] = i + 1;
				triangles[offset + 2] = i + 2;
			}

			// construct sides

			int bottomOffset = subdivisions * 3;
			for (int i = 0, n = subdivisions - 1; i < n; i++) {
				int offset = i * 3 + bottomOffset;
				triangles[offset] = i + 1;
				triangles[offset + 1] = subdivisions + 1;
				triangles[offset + 2] = i + 2;
			}

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return mesh;
		}



		public static Mesh CreateSectorMesh (float radiusA, float radiusB, float height, float angle, int subDivisions = 12) {

			subDivisions = Mathf.Clamp(subDivisions, 2, 128);
			angle = Mathf.Repeat(angle, 360f);

			Mesh mesh = new Mesh();

			int triLen = (8 * (subDivisions - 1) + 4) * 3;
			int vLen = subDivisions * 4;
			var vs = new Vector3[vLen];
			var uv = new Vector2[vLen];
			var tris = new int[triLen];

			float currentAngle = -angle / 2f;
			for (int i = 0; i < subDivisions; i++, currentAngle += angle / (subDivisions - 1)) {

				float rAngle = currentAngle * Mathf.Deg2Rad;

				vs[i * 4 + 0] = new Vector3(Mathf.Sin(rAngle) * radiusA, -height * 0.5f, Mathf.Cos(rAngle) * radiusA);
				vs[i * 4 + 1] = new Vector3(Mathf.Sin(rAngle) * radiusB, -height * 0.5f, Mathf.Cos(rAngle) * radiusB);
				vs[i * 4 + 2] = new Vector3(Mathf.Sin(rAngle) * radiusA, height * 0.5f, Mathf.Cos(rAngle) * radiusA);
				vs[i * 4 + 3] = new Vector3(Mathf.Sin(rAngle) * radiusB, height * 0.5f, Mathf.Cos(rAngle) * radiusB);

				uv[i * 4 + 0] = new Vector2(0, 0);
				uv[i * 4 + 1] = new Vector2(0, 1);
				uv[i * 4 + 2] = new Vector2(1, 0);
				uv[i * 4 + 3] = new Vector2(1, 1);

				if (i < subDivisions - 1) {

					tris[i * 24 + 0] = i * 4 + 2;
					tris[i * 24 + 1] = i * 4 + 3;
					tris[i * 24 + 2] = i * 4 + 7;

					tris[i * 24 + 3] = i * 4 + 2;
					tris[i * 24 + 4] = i * 4 + 7;
					tris[i * 24 + 5] = i * 4 + 6;


					tris[i * 24 + 6] = i * 4 + 0;
					tris[i * 24 + 7] = i * 4 + 2;
					tris[i * 24 + 8] = i * 4 + 6;

					tris[i * 24 + 9] = i * 4 + 0;
					tris[i * 24 + 10] = i * 4 + 6;
					tris[i * 24 + 11] = i * 4 + 4;


					tris[i * 24 + 12] = i * 4 + 0;
					tris[i * 24 + 13] = i * 4 + 5;
					tris[i * 24 + 14] = i * 4 + 1;

					tris[i * 24 + 15] = i * 4 + 0;
					tris[i * 24 + 16] = i * 4 + 4;
					tris[i * 24 + 17] = i * 4 + 5;


					tris[i * 24 + 18] = i * 4 + 1;
					tris[i * 24 + 19] = i * 4 + 7;
					tris[i * 24 + 20] = i * 4 + 3;

					tris[i * 24 + 21] = i * 4 + 1;
					tris[i * 24 + 22] = i * 4 + 5;
					tris[i * 24 + 23] = i * 4 + 7;
				}

			}

			tris[triLen - 12] = 0;
			tris[triLen - 11] = 1;
			tris[triLen - 10] = 3;

			tris[triLen - 9] = 0;
			tris[triLen - 8] = 3;
			tris[triLen - 7] = 2;

			tris[triLen - 6] = vLen - 4;
			tris[triLen - 5] = vLen - 2;
			tris[triLen - 4] = vLen - 1;

			tris[triLen - 3] = vLen - 4;
			tris[triLen - 2] = vLen - 1;
			tris[triLen - 1] = vLen - 3;

			mesh.vertices = vs;
			mesh.triangles = tris;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return mesh;
		}



		public static bool InRange (int x, int y, int z, int sizeX, int sizeY, int sizeZ) {
			return x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0 && z < sizeZ;
		}




		public static Vector2 VectorAbs (Vector2 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			return v;
		}



		public static Vector3 VectorAbs (Vector3 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			v.z = Mathf.Abs(v.z);
			return v;
		}


		public static Vector3 SwipYZ (Vector3 v) {
			float tempZ = v.z;
			v.z = v.y;
			v.y = tempZ;
			return v;
		}


		public static float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? 0 : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		public static int MaxAxis (Vector3 v) {
			if (Mathf.Abs(v.x) >= Mathf.Abs(v.y)) {
				return Mathf.Abs(v.x) >= Mathf.Abs(v.z) ? 0 : 2;
			} else {
				return Mathf.Abs(v.y) >= Mathf.Abs(v.z) ? 1 : 2;
			}
		}


		public static void CopyToClipboard (string containt) {
			TextEditor te = new TextEditor { text = containt };
			te.SelectAll();
			te.Copy();
		}



		public static string GetObj (Mesh m) {

			StringBuilder sb = new StringBuilder();

			sb.Append("g ").Append(m.name).Append("\n");
			foreach (Vector3 v in m.vertices) {
				sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in m.normals) {
				sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in m.uv) {
				sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
			}

			sb.Append("\n");
			sb.Append("usemtl ").Append(m.name).Append("\n");
			sb.Append("usemap ").Append(m.name).Append("\n");

			int[] triangles = m.triangles;
			for (int i = 0; i < triangles.Length; i += 3) {
				sb.Append(
					string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
					triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1)
				);
			}

			return sb.ToString();
		}



		public static Texture2D RenderTextureToTexture2D (Camera renderCamera) {
			var rTex = renderCamera.targetTexture;
			if (!rTex) { return null; }
			RenderTexture.active = rTex;
			Texture2D texture = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false, false) {
				filterMode = FilterMode.Bilinear
			};
			var oldColor = renderCamera.backgroundColor;
			renderCamera.backgroundColor = Color.clear;
			renderCamera.Render();
			texture.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0, false);
			texture.Apply();
			renderCamera.backgroundColor = oldColor;
			RenderTexture.active = null;
			return texture;
		}



		public static Texture2D TrimTexture (Texture2D texture, float alpha = 0.01f, int gap = 0) {
			int width = texture.width;
			int height = texture.height;
			var colors = texture.GetPixels();
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					var c = colors[y * width + x];
					if (c.a > alpha) {
						minX = Mathf.Min(minX, x);
						minY = Mathf.Min(minY, y);
						maxX = Mathf.Max(maxX, x);
						maxY = Mathf.Max(maxY, y);
					}
				}
			}

			// Gap
			minX = Mathf.Clamp(minX - gap, 0, width - 1);
			minY = Mathf.Clamp(minY - gap, 0, height - 1);
			maxX = Mathf.Clamp(maxX + gap, 0, width - 1);
			maxY = Mathf.Clamp(maxY + gap, 0, height - 1);

			int newWidth = maxX - minX + 1;
			int newHeight = maxY - minY + 1;
			if (newWidth != width || newHeight != height) {
				texture.Resize(newWidth, newHeight);
				var newColors = new Color[newWidth * newHeight];
				for (int y = 0; y < newHeight; y++) {
					for (int x = 0; x < newWidth; x++) {
						newColors[y * newWidth + x] = colors[(y + minY) * width + (x + minX)];
					}
				}
				texture.SetPixels(newColors);
				texture.Apply();
			}
			return texture;
		}




		public static bool GetBit (int value, int index) {
			if (index < 0 || index > 31) { return false; }
			var val = 1 << index;
			return (value & val) == val;
		}



		public static int SetBitValue (int value, int index, bool bitValue) {
			if (index < 0 || index > 31) { return value; }
			var val = 1 << index;
			return bitValue ? (value | val) : (value & ~val);
		}



		public static void SetMaterialFloatOrColor (Material mat, string keyword, float value) {
			try {
				if (!string.IsNullOrEmpty(keyword)) {
					if (keyword[0] == '$' && keyword.Length > 2) {
						Color color = Color.white;
						switch (keyword[1]) {
							case 'r':
								color.r = value;
								break;
							case 'g':
								color.g = value;
								break;
							case 'b':
								color.b = value;
								break;
							case 'a':
								color.a = value;
								break;
							case 'c':
								color.r = value;
								color.g = value;
								color.b = value;
								break;
						}
						mat.SetColor(keyword.Substring(2, keyword.Length - 2), color);
					} else {
						mat.SetFloat(keyword, value);
					}
				}
			} catch { }
		}



		public static string GetPrefabAssetPath (GameObject g) {
#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
			return g ? AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(g)) : "";
#else
			return g ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(g) : "";
#endif
		}



		public static void OverrideMesh (Mesh instance, Mesh data) {

			instance.Clear();

			instance.vertices = data.vertices;
			instance.uv = data.uv;
			instance.triangles = data.triangles;
			instance.colors = data.colors;
			instance.boneWeights = data.boneWeights;
			instance.bindposes = data.bindposes;
			instance.name = data.name;

			instance.RecalculateNormals();
			instance.RecalculateTangents();
			instance.RecalculateBounds();

			instance.UploadMeshData(false);
		}



		public static void OverrideTexture (Texture2D instance, Texture2D data) {
			instance.Resize(data.width, data.height, data.format, false);
			instance.SetPixels(data.GetPixels());
			instance.Apply();
		}



		public static void OverrideMaterial (Material instance, Material data, string mainTexKeyword) {
			instance.shader = data.shader;
			instance.SetTexture(mainTexKeyword, data.GetTexture(mainTexKeyword));
			instance.CopyPropertiesFromMaterial(data);
		}



		public static AnimationClip CopyAnimation (AnimationClip source) {
			// Init
			var animation = new AnimationClip() {
				frameRate = source.frameRate,
				name = source.name,
				wrapMode = source.wrapMode,
				legacy = source.legacy,
				hideFlags = source.hideFlags,
				localBounds = source.localBounds,
			};
			// Data
			var bindings = AnimationUtility.GetCurveBindings(source);
			for (int i = 0; i < bindings.Length; i++) {
				var binding = bindings[i];
				var curve = AnimationUtility.GetEditorCurve(source, binding);
				var keys = new Keyframe[curve.length];
				for (int j = 0; j < keys.Length; j++) {
					keys[j] = curve.keys[j];
				}
				animation.SetCurve(binding.path, binding.type, binding.propertyName, new AnimationCurve(keys) {
					postWrapMode = curve.postWrapMode,
					preWrapMode = curve.preWrapMode,
				});
			}
			return animation;
		}



		public static void ClearChildrenImmediate (Transform tf) {
			if (!tf) { return; }
			int len = tf.childCount;
			for (int i = 0; i < len; i++) {
				Object.DestroyImmediate(tf.GetChild(0).gameObject, false);
			}
		}



		public static void CurveAllLiner (AnimationCurve curve) {
			for (int i = 0; i < curve.keys.Length; i++) {
				var key = curve.keys[i];
				key.inTangent = 0f;
				key.outTangent = 0f;
#if UNITY_2018_3_6
				key.inWeight = 0f;
				key.outWeight = 0f;
				key.weightedMode = WeightedMode.Both;
#endif
				curve.MoveKey(i, key);
			}
		}



		#endregion



		#region --- MagicaVoxel ---




		private static readonly Dictionary<byte, Vector4> MAGIC_BYTE_TO_TRANSFORM_MAP = new Dictionary<byte, Vector4>() {

			{ 40 , new Vector4(3,0,0,0)},
			{ 2  , new Vector4(3,3,0,0)},
			{ 24 , new Vector4(3,2,0,0)},
			{ 50 , new Vector4(3,1,0,0)},
			{ 120, new Vector4(1,0,2,0)},
			{ 98 , new Vector4(1,0,3,0)},
			{ 72 , new Vector4(1,0,0,0)},
			{ 82 , new Vector4(1,0,1,0)},
			{ 4  , new Vector4(0,0,0,0)},
			{ 22 , new Vector4(0,0,1,0)},
			{ 84 , new Vector4(0,0,2,0)},
			{ 70 , new Vector4(0,0,3,0)},
			{ 52 , new Vector4(0,2,0,0)},
			{ 118, new Vector4(0,2,3,0)},
			{ 100, new Vector4(0,2,2,0)},
			{ 38 , new Vector4(0,2,1,0)},
			{ 17 , new Vector4(0,3,0,0)},
			{ 89 , new Vector4(0,3,3,0)},
			{ 113, new Vector4(0,3,2,0)},
			{ 57 , new Vector4(0,3,1,0)},
			{ 33 , new Vector4(0,1,0,0)},
			{ 9  , new Vector4(0,1,1,0)},
			{ 65 , new Vector4(0,1,2,0)},
			{ 105, new Vector4(0,1,3,0)},

			{ 56 , new Vector4(3,0,0,1)},
			{ 34 , new Vector4(3,3,0,1)},
			{ 8  , new Vector4(3,2,0,1)},
			{ 18 , new Vector4(3,1,0,1)},
			{ 104, new Vector4(1,0,2,1)},
			{ 66 , new Vector4(1,0,3,1)},
			{ 88 , new Vector4(1,0,0,1)},
			{ 114, new Vector4(1,0,1,1)},
			{ 20 , new Vector4(0,0,0,1)},
			{ 86 , new Vector4(0,0,1,1)},
			{ 68 , new Vector4(0,0,2,1)},
			{ 6  , new Vector4(0,0,3,1)},
			{ 36 , new Vector4(0,2,0,1)},
			{ 54 , new Vector4(0,2,3,1)},
			{ 116, new Vector4(0,2,2,1)},
			{ 102, new Vector4(0,2,1,1)},
			{ 49 , new Vector4(0,3,0,1)},
			{ 25 , new Vector4(0,3,3,1)},
			{ 81 , new Vector4(0,3,2,1)},
			{ 121, new Vector4(0,3,1,1)},
			{ 1  , new Vector4(0,1,0,1)},
			{ 73 , new Vector4(0,1,1,1)},
			{ 97 , new Vector4(0,1,2,1)},
			{ 41 , new Vector4(0,1,3,1)},


		};

		private static readonly Dictionary<Vector4, byte> TRANSFORM_TO_MAGIC_BYTE_MAP = new Dictionary<Vector4, byte>() {

			{new Vector4(3,0,0,0), 40 },
			{new Vector4(3,3,0,0), 2  },
			{new Vector4(3,2,0,0), 24 },
			{new Vector4(3,1,0,0), 50 },
			{new Vector4(1,0,2,0), 120},
			{new Vector4(1,0,3,0), 98 },
			{new Vector4(1,0,0,0), 72 },
			{new Vector4(1,0,1,0), 82 },
			{new Vector4(0,0,0,0), 4  },
			{new Vector4(0,0,1,0), 22 },
			{new Vector4(0,0,2,0), 84 },
			{new Vector4(0,0,3,0), 70 },
			{new Vector4(0,2,0,0), 52 },
			{new Vector4(0,2,3,0), 118},
			{new Vector4(0,2,2,0), 100},
			{new Vector4(0,2,1,0), 38 },
			{new Vector4(0,3,0,0), 17 },
			{new Vector4(0,3,3,0), 89 },
			{new Vector4(0,3,2,0), 113},
			{new Vector4(0,3,1,0), 57 },
			{new Vector4(0,1,0,0), 33 },
			{new Vector4(0,1,1,0), 9  },
			{new Vector4(0,1,2,0), 65 },
			{new Vector4(0,1,3,0), 105},

			{new Vector4(3,0,0,1), 56 },
			{new Vector4(3,3,0,1), 34 },
			{new Vector4(3,2,0,1), 8  },
			{new Vector4(3,1,0,1), 18 },
			{new Vector4(1,0,2,1), 104},
			{new Vector4(1,0,3,1), 66 },
			{new Vector4(1,0,0,1), 88 },
			{new Vector4(1,0,1,1), 114},
			{new Vector4(0,0,0,1), 20 },
			{new Vector4(0,0,1,1), 86 },
			{new Vector4(0,0,2,1), 68 },
			{new Vector4(0,0,3,1), 6  },
			{new Vector4(0,2,0,1), 36 },
			{new Vector4(0,2,3,1), 54 },
			{new Vector4(0,2,2,1), 116},
			{new Vector4(0,2,1,1), 102},
			{new Vector4(0,3,0,1), 49 },
			{new Vector4(0,3,3,1), 25 },
			{new Vector4(0,3,2,1), 81 },
			{new Vector4(0,3,1,1), 121},
			{new Vector4(0,1,0,1), 1  },
			{new Vector4(0,1,1,1), 73 },
			{new Vector4(0,1,2,1), 97 },
			{new Vector4(0,1,3,1), 41 },

		};


		public static void VoxMatrixByteToTransform (byte the_Byte_Which_Wasted_My_While_Afternoon, out Vector3 rotation, out Vector3 scale) {
			if (MAGIC_BYTE_TO_TRANSFORM_MAP.ContainsKey(the_Byte_Which_Wasted_My_While_Afternoon)) {
				var v4 = MAGIC_BYTE_TO_TRANSFORM_MAP[the_Byte_Which_Wasted_My_While_Afternoon];
				rotation = new Vector3(v4.x * 90f, v4.y * 90f, v4.z * 90f);
				scale = v4.w < 0.5f ? Vector3.one : new Vector3(-1, 1, 1);
			} else {
				rotation = Vector3.zero;
				scale = Vector3.one;
			}
		}


		public static byte TransformToVoxMatrixByte (Vector3 rot, Vector3 scale) {
			var v4 = new Vector4(
				 (byte)(Mathf.RoundToInt((Mathf.Repeat(rot.x, 360f) / 90f) % 4)),
				 (byte)(Mathf.RoundToInt((Mathf.Repeat(rot.y, 360f) / 90f) % 4)),
				 (byte)(Mathf.RoundToInt((Mathf.Repeat(rot.z, 360f) / 90f) % 4)),
				 (byte)(scale.x > 0f ? 0 : 1)
			);
			if (TRANSFORM_TO_MAGIC_BYTE_MAP.ContainsKey(v4)) {
				return TRANSFORM_TO_MAGIC_BYTE_MAP[v4];
			} else {
				return 4;
			}
		}



		#endregion




	}


}
