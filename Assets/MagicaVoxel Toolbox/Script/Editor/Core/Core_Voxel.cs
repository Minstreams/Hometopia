namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Core_Voxel {




		#region --- SUB ---


		public enum LightMapSupportType {
			SmallTextureButNoLightmap = 0,
			SupportLightmapButLargeTexture = 1,
		}



		public struct Int3 {

			public int X;
			public int Y;
			public int Z;

			public int Max {
				get {
					return Mathf.Max(X, Mathf.Max(Y, Z));
				}
			}

			public Int3 (Vector3 v) {
				X = (int)v.x;
				Y = (int)v.y;
				Z = (int)v.z;
			}

			public Int3 (int x, int y, int z) {
				X = x;
				Y = y;
				Z = z;
			}

			public Vector3 ToVector3 (float offset = 0f) {
				return new Vector3(X + offset, Y + offset, Z + offset);
			}

			public static Int3 Round3 (float x, float y, float z) {
				return new Int3(
					Mathf.RoundToInt(x + 0.501f),
					Mathf.RoundToInt(y + 0.501f),
					Mathf.RoundToInt(z + 0.501f)
				);
			}

			public override string ToString () {
				return string.Format("({0},{1},{2})", X, Y, Z);
			}
		}



		public class Result {

			public class VoxelModel {
				public UnlimitiedMesh[] Meshs = new UnlimitiedMesh[0];
				public Texture2D[] Textures = new Texture2D[0];
				public VoxelData.MaterialData[] Materials = new VoxelData.MaterialData[0];
				public Bone[] RootBones = new Bone[0];
				public VoxelNode RootNode = new VoxelNode();
				public Vector3[] ModelSize = new Vector3[0];
				public Vector3[] FootPoints = new Vector3[0];
				public int MaxModelBounds;
			}



			public class VoxelNode {

				public UnlimitiedMesh Model = null;
				public Texture2D Texture = null;

				public Int3 Position = new Int3(0, 0, 0);
				public Quaternion Rotation = Quaternion.identity;
				public Int3 Scale = new Int3(1, 1, 1);
				public Int3 Size = new Int3(1, 1, 1);

				public VoxelNode[] Children = null;
				public string Name = "";
				public bool Active = true;
			}



			public string ExportRoot;
			public string ExportSubRoot;
			public string FileName;
			public string Extension;
			public bool IsRigged;
			public bool WithAvatar;
			public VoxelModel[] VoxelModels = new VoxelModel[0];

			public static implicit operator bool (Result result) { return result != null; }

		}



		public class Face {


			// Data
			public int X;
			public int Y;
			public int Z;
			public Direction Direction;
			public bool Aread;
			public int MaterialMapIndex;


			// CRT
			public Face (int x, int y, int z, Direction dir) {
				X = x;
				Y = y;
				Z = z;
				Direction = dir;
				Aread = false;
			}



		}



		public class Bone {
			public int Index;
			public string Name;
			public Vector3 Position { get; set; }
			public List<Bone> ChildBones;
		}



		public class Area {

			public Vector3 A;
			public Vector3 B;
			public Vector3 C;
			public Vector3 D;
			public int SizeU;
			public int SizeV;
			public bool IsLUB;
			public Direction Direction;
			public int MaterialMapIndex;


			public void Init (Int3 centerMin, Int3 centerMax, int dir) {

				Vector3 min = centerMin.ToVector3(-0.5f);
				Vector3 max = centerMax.ToVector3(0.5f);

				Direction = (Direction)dir;

				switch (Direction) {
					default:

					case Direction.Left:// x min

						A = min;
						B = new Vector3(min.x, min.y, max.z);
						C = new Vector3(min.x, max.y, max.z);
						D = new Vector3(min.x, max.y, min.z);

						SizeU = (int)(max.z - min.z);// z
						SizeV = (int)(max.y - min.y);// y
						IsLUB = true;
						break;
					case Direction.Down:// y min

						A = min;
						B = new Vector3(max.x, min.y, min.z);
						C = new Vector3(max.x, min.y, max.z);
						D = new Vector3(min.x, min.y, max.z);

						SizeU = (int)(max.z - min.z);// z
						SizeV = (int)(max.x - min.x);// x
						IsLUB = false;
						break;
					case Direction.Back:// z min
						A = min;
						B = new Vector3(min.x, max.y, min.z);
						C = new Vector3(max.x, max.y, min.z);
						D = new Vector3(max.x, min.y, min.z);

						SizeU = (int)(max.y - min.y);// y
						SizeV = (int)(max.x - min.x);// x
						IsLUB = true;
						break;

					case Direction.Right:// x max
						A = new Vector3(max.x, min.y, min.z);
						B = new Vector3(max.x, max.y, min.z);
						C = max;
						D = new Vector3(max.x, min.y, max.z);

						SizeU = (int)(max.z - min.z);// z
						SizeV = (int)(max.y - min.y);// y
						IsLUB = false;
						break;
					case Direction.Up:// y max

						A = new Vector3(min.x, max.y, min.z);
						B = new Vector3(min.x, max.y, max.z);
						C = max;
						D = new Vector3(max.x, max.y, min.z);

						SizeU = (int)(max.z - min.z);// z
						SizeV = (int)(max.x - min.x);// x
						IsLUB = true;
						break;
					case Direction.Front:// z max

						A = new Vector3(min.x, min.y, max.z);
						B = new Vector3(max.x, min.y, max.z);
						C = max;
						D = new Vector3(min.x, max.y, max.z);

						SizeU = (int)(max.y - min.y);// y
						SizeV = (int)(max.x - min.x);// x
						IsLUB = false;
						break;
				}
			}


		}




		#endregion




		#region --- VAR ---


		private static readonly bool[] MERGE_IN_DIRECTION = new bool[6] { true, true, true, true, true, true, };
		private static readonly int[] WEIGHT_COUNT_CACHE = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
		private static readonly int[] WEIGHT_BONE_INDEX_CACHE = new int[16] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, };
		private static readonly BoneWeight[] Weight_Point_Cache = new BoneWeight[8] { default(BoneWeight), default(BoneWeight), default(BoneWeight), default(BoneWeight), default(BoneWeight), default(BoneWeight), default(BoneWeight), default(BoneWeight), };


		private static VoxelData Data;
		private static int ModelIndex;
		private static float ModelScale;
		private static bool SupportRig;
		private static bool SupportMaterial;
		private static LightMapSupportType SupportLightMap;
		private static int SizeX, SizeY, SizeZ;
		private static int[,,] Voxels;
		private static Face[,,,] Faces;
		private static Color[,,] Colors;
		private static BoneWeight[,,] PointWeights;
		private static Vector3 Pivot;
		private static Bone[] RootBones;
		private static List<Area> AreaList = new List<Area>();
		private static List<PackingData> PackingList = new List<PackingData>();
		private static List<int> AreaPackingMap = new List<int>();
		private static Dictionary<VoxelData.MaterialData, int> MaterialMap = new Dictionary<VoxelData.MaterialData, int>();
		private static List<VoxelData.MaterialData> MaterialPalette = new List<VoxelData.MaterialData>();


		#endregion




		#region --- API ---



		// Model
		public static Result CreateLodModel (VoxelData data, float scale, int lodNum, bool material, LightMapSupportType supportLightMap, Vector3 pivot) {
			var result = new Result() {
				VoxelModels = new Result.VoxelModel[lodNum],
			};
			for (int lodLevel = 0; lodLevel < lodNum; lodLevel++) {
				var res = CreateModel(data, scale, material, supportLightMap, false, pivot);
				if (res && res.VoxelModels.Length > 0) {
					result.VoxelModels[lodLevel] = res.VoxelModels[0];
				}
				data.LodIteration(lodLevel);
			}
			return result;
		}




		public static Result CreateModel (VoxelData data, float scale, bool material, LightMapSupportType supportLightMap, bool supportRig, Vector3 pivot) {

			if (data == null) { return null; }

			Data = data;
			ModelScale = scale;
			SupportLightMap = supportLightMap;
			SupportRig = supportRig;
			SupportMaterial = material;
			Pivot = pivot;
			MaterialMap.Clear();
			MaterialPalette.Clear();

			var result = new Result {
				VoxelModels = new Result.VoxelModel[1] { new Result.VoxelModel() {
					 Meshs = new UnlimitiedMesh[data.Voxels.Count],
					 Textures = new Texture2D[data.Voxels.Count],
					 ModelSize = new Vector3[data.Voxels.Count],
					 FootPoints = new Vector3[data.Voxels.Count],
					 MaxModelBounds = new Int3(data.GetBounds()).Max,
				}}
			};

			GetMaterials();

			for (int index = 0; index < data.Voxels.Count; index++) {
				UnlimitiedMesh mesh;
				Texture2D texture;
				PointWeights = null;
				RootBones = null;
				Voxels = data.Voxels[index];
				SizeX = Voxels.GetLength(0);
				SizeY = Voxels.GetLength(1);
				SizeZ = Voxels.GetLength(2);
				ModelIndex = index;

				GetFaces();
				GetColors();
				if (SupportRig) {
					var rootBoneList = GetChildBones(null);
					if (rootBoneList != null) {
						RootBones = rootBoneList.ToArray();
					}
					GetWeights();
				}
				GetAreas();
				GetResultFromArea(out mesh, out texture);

				result.VoxelModels[0].Meshs[index] = mesh;
				result.VoxelModels[0].Textures[index] = texture;
				result.VoxelModels[0].ModelSize[index] = data.GetModelSize(index);
				result.VoxelModels[0].FootPoints[index] = data.GetFootPoint(index);
				result.VoxelModels[0].RootBones = RootBones;
			}

			result.VoxelModels[0].Materials = MaterialPalette.ToArray();
			result.VoxelModels[0].RootNode = GetTransformData(result.VoxelModels[0], result.VoxelModels[0].Textures, 0);

			Data = null;
			Voxels = null;
			Faces = null;
			PointWeights = null;
			RootBones = null;
			Colors = null;
			AreaList.Clear();
			PackingList.Clear();
			AreaPackingMap.Clear();
			MaterialMap.Clear();
			MaterialPalette.Clear();

			return result;
		}



		// Json
		public static string VoxelToJson (VoxelData data) {
			return JsonUtility.ToJson(new VoxelJsonData(data), false);
		}



		public static VoxelData JsonToVoxel (string json) {
			var jData = JsonUtility.FromJson<VoxelJsonData>(json);
			return jData != null ? jData.GetVoxelData() : null;
		}



		// Setting
		public static void SetMergeInDirection (bool f, bool b, bool u, bool d, bool l, bool r) {
			MERGE_IN_DIRECTION[(int)Direction.Front] = f;
			MERGE_IN_DIRECTION[(int)Direction.Back] = b;
			MERGE_IN_DIRECTION[(int)Direction.Up] = u;
			MERGE_IN_DIRECTION[(int)Direction.Down] = d;
			MERGE_IN_DIRECTION[(int)Direction.Left] = l;
			MERGE_IN_DIRECTION[(int)Direction.Right] = r;
		}



		#endregion




		#region --- LGC ---



		private static void GetMaterials () {

			MaterialMap.Clear();
			MaterialPalette.Clear();

			if (SupportMaterial) {
				var materials = Data.Materials.ToArray();
				//if (materials.Length > 0) {
				//	materials[0] = diffuseMat;
				//}
				for (int i = 0; i < materials.Length; i++) {
					var mat = materials[i];
					if (mat == null) { continue; }
					if (!MaterialMap.ContainsKey(mat)) {
						MaterialMap.Add(mat, MaterialPalette.Count);
						MaterialPalette.Add(mat);
					}
					for (int j = i + 1; j < materials.Length; j++) {
						var matOther = materials[j];
						if (matOther == null) { continue; }
						if (mat.IsSameWith(matOther)) {
							materials[j] = null;
							if (!MaterialMap.ContainsKey(matOther)) {
								MaterialMap.Add(matOther, MaterialMap[mat]);
							}
						}
					}
				}
			} else {
				var diffuseMat = new VoxelData.MaterialData() { Type = VoxelData.MaterialData.MaterialType.Diffuse };
				MaterialMap.Add(diffuseMat, 0);
				MaterialPalette.Add(diffuseMat);
			}

		}




		private static void GetFaces () {
			Faces = new Face[SizeX, SizeY, SizeZ, 6];
			List<VoxelData.MaterialData> materials = null;
			if (SupportMaterial) {
				materials = new List<VoxelData.MaterialData>(Data.Materials);
			}
			for (int x = 0; x < SizeX; x++) {
				for (int y = 0; y < SizeY; y++) {
					for (int z = 0; z < SizeZ; z++) {
						int voxel = Voxels[x, y, z];
						if (voxel == 0) { continue; }
						var matIndex = 0;
						if (SupportMaterial) {
							var mat = materials != null && voxel >= 0 && voxel < materials.Count ? materials[voxel] : null;
							if (mat != null && MaterialMap.ContainsKey(mat)) {
								matIndex = MaterialMap[mat];
							}
						}
						for (int i = 0; i < 6; i++) {
							Face face = null;
							if (VoxelData.IsExposed(Voxels, x, y, z, SizeX, SizeY, SizeZ, (Direction)i, materials)) {
								face = new Face(x, y, z, (Direction)i) { MaterialMapIndex = matIndex };
							}
							Faces[x, y, z, i] = face;
						}
					}
				}
			}
		}




		private static void GetColors () {
			Colors = new Color[SizeX, SizeY, SizeZ];
			for (int x = 0; x < SizeX; x++) {
				for (int y = 0; y < SizeY; y++) {
					for (int z = 0; z < SizeZ; z++) {
						Colors[x, y, z] = Data.GetColorFromPalette(Voxels[x, y, z]);
					}
				}
			}
		}




		private static List<Bone> GetChildBones (VoxelData.RigData.Bone parent) {
			if (Data.Rigs.ContainsKey(ModelIndex)) {
				var rigData = Data.Rigs[ModelIndex];
				if (rigData.Bones.Count == 0) { return null; }
				var boneList = new List<Bone>();
				for (int i = 0; i < rigData.Bones.Count; i++) {
					var sourceBone = rigData.Bones[i];
					if (sourceBone && sourceBone.Parent == parent) {
						var bone = new Bone() {
							Index = i,
							Name = sourceBone.Name,
							Position = new Vector3(
								sourceBone.PositionX / 2f,
								sourceBone.PositionY / 2f,
								sourceBone.PositionZ / 2f
							),
							ChildBones = GetChildBones(sourceBone),
						};
						boneList.Add(bone);
					}
				}
				return boneList;
			}
			return null;
		}




		private static void GetWeights () {
			int sizeX = SizeX;
			int sizeY = SizeY;
			int sizeZ = SizeZ;
			var weights = new VoxelData.RigData.Weight[sizeX, sizeY, sizeZ];

			// Weights
			if (Data.Rigs.ContainsKey(ModelIndex)) {
				var rigData = Data.Rigs[ModelIndex];
				if (rigData.Bones.Count > 0 && rigData.Weights.Count > 0) {
					for (int i = 0; i < rigData.Weights.Count; i++) {
						var source = rigData.Weights[i];
						if ((source.BoneIndexA >= 0 || source.BoneIndexB >= 0) && source.X >= 0 && source.X < sizeX && source.Y >= 0 && source.Y < sizeY && source.Z >= 0 && source.Z < sizeZ) {
							weights[source.X, source.Y, source.Z] = new VoxelData.RigData.Weight(source.BoneIndexA, source.BoneIndexB);
						}
					}
				}
			}


			// Point Weights
			PointWeights = new BoneWeight[sizeX + 1, sizeY + 1, sizeZ + 1];
			for (int x = 0; x < sizeX + 1; x++) {
				for (int y = 0; y < sizeY + 1; y++) {
					for (int z = 0; z < sizeZ + 1; z++) {
						PointWeights[x, y, z] = GetBoneWeightFromVoxelWeights(
							new Vector3(x - 0.5f, y - 0.5f, z - 0.5f),
							weights
						);
					}
				}
			}

		}




		private static void GetAreas () {

			AreaList.Clear();
			PackingList.Clear();
			AreaPackingMap.Clear();

			Int3 min, max;

			for (int d = 0; d < 6; d++) {
				for (int x = 0; x < SizeX; x++) {
					for (int y = 0; y < SizeY; y++) {
						for (int z = 0; z < SizeZ; z++) {
							var face = Faces[x, y, z, d];
							if (face != null && !face.Aread) {

								GetFaceRange(x, y, z, d, out min, out max);

								// Area
								var area = new Area() { MaterialMapIndex = face.MaterialMapIndex };
								area.Init(min, max, d);
								AreaList.Add(area);

								// Texture
								var colors = new Color[area.SizeU * area.SizeV];
								int currentIndex = 0;
								for (int u = min.X; u <= max.X; u++) {
									for (int v = min.Y; v <= max.Y; v++) {
										for (int w = min.Z; w <= max.Z; w++) {
											colors[currentIndex] = Colors[u, v, w];
											currentIndex++;
										}
									}
								}

								// Lightmap
								if (SupportLightMap != LightMapSupportType.SupportLightmapButLargeTexture) {
									var packingData = new PackingData(area.SizeU, area.SizeV, colors);
									bool hasSame = false;
									for (int i = 0; i < PackingList.Count; i++) {
										if (packingData.SameWith(PackingList[i])) {
											hasSame = true;
											AreaPackingMap.Add(i);
											break;
										}
									}
									if (!hasSame) {
										AreaPackingMap.Add(PackingList.Count);
										PackingList.Add(packingData);
									}
								} else {
									PackingList.Add(new PackingData(area.SizeU, area.SizeV, colors));
								}
							}
						}

					}
				}
			}

		}




		private static void GetFaceRange (int x, int y, int z, int dir, out Int3 min, out Int3 max) {

			min = new Int3(x, y, z);
			max = new Int3(x, y, z);

			bool breakFlag = false;

			var sourceFace = Faces[x, y, z, dir];
			sourceFace.Aread = true;

			// Allow Check
			if (!MERGE_IN_DIRECTION[dir]) {
				return;
			}

			if (SupportMaterial) {

				// Material
				var matIndex = sourceFace.MaterialMapIndex;

				// X
				if (dir != 2 && dir != 3) {
					for (int i = x - 1; i >= 0; i--) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
							break;
						}
						min.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
					for (int i = x + 1; i < SizeX; i++) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
							break;
						}
						max.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
				}

				// Y
				if (dir != 0 && dir != 1) {
					breakFlag = false;
					for (int i = y - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
								breakFlag = true;
							}
						}
						if (!breakFlag) {
							min.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}

					breakFlag = false;
					for (int i = y + 1; i < SizeY && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
								breakFlag = true;
							}
						}
						if (!breakFlag) {
							max.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}
				}

				// Z
				if (dir != 4 && dir != 5) {
					breakFlag = false;
					for (int i = z - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
									breakFlag = true;
								}
							}
						}
						if (!breakFlag) {
							min.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}

					breakFlag = false;
					for (int i = z + 1; i < SizeZ && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread || matIndex != face.MaterialMapIndex) {
									breakFlag = true;
								}
							}
						}
						if (!breakFlag) {
							max.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}
				}


			} else if (PointWeights == null) {

				// No Weight

				// X
				if (dir != 2 && dir != 3) {
					for (int i = x - 1; i >= 0; i--) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread) {
							break;
						}
						min.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
					for (int i = x + 1; i < SizeX; i++) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread) {
							break;
						}
						max.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
				}

				// Y
				if (dir != 0 && dir != 1) {
					breakFlag = false;
					for (int i = y - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread) {
								breakFlag = true;
							}
						}
						if (!breakFlag) {
							min.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}

					breakFlag = false;
					for (int i = y + 1; i < SizeY && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread) {
								breakFlag = true;
							}
						}
						if (!breakFlag) {
							max.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}
				}

				// Z
				if (dir != 4 && dir != 5) {
					breakFlag = false;
					for (int i = z - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread) {
									breakFlag = true;
								}
							}
						}
						if (!breakFlag) {
							min.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}

					breakFlag = false;
					for (int i = z + 1; i < SizeZ && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread) {
									breakFlag = true;
								}
							}
						}
						if (!breakFlag) {
							max.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}
				}

			} else {

				// Have Weight
				SetWeightPointCache(sourceFace.X, sourceFace.Y, sourceFace.Z, (Direction)dir, false);

				// X
				if (dir != 2 && dir != 3) {
					for (int i = x - 1; i >= 0; i--) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
							break;
						}
						min.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
					for (int i = x + 1; i < SizeX; i++) {
						var face = Faces[i, y, z, dir];
						if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
							break;
						}
						max.X = i;
						Faces[i, y, z, dir].Aread = true;
					}
				}

				// Y
				if (dir != 0 && dir != 1) {
					breakFlag = false;
					for (int i = y - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
								breakFlag = true;
							}

						}
						if (!breakFlag) {
							min.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}

					breakFlag = false;
					for (int i = y + 1; i < SizeY && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							var face = Faces[j, i, z, dir];
							if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
								breakFlag = true;
							}
						}
						if (!breakFlag) {
							max.Y = i;
							for (int j = min.X; j <= max.X; j++) {
								Faces[j, i, z, dir].Aread = true;
							}
						}
					}
				}

				// Z
				if (dir != 4 && dir != 5) {
					breakFlag = false;
					for (int i = z - 1; i >= 0 && !breakFlag; i--) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
									breakFlag = true;
								}
							}
						}
						if (!breakFlag) {
							min.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}

					breakFlag = false;
					for (int i = z + 1; i < SizeZ && !breakFlag; i++) {
						for (int j = min.X; j <= max.X && !breakFlag; j++) {
							for (int k = min.Y; k <= max.Y && !breakFlag; k++) {
								var face = Faces[j, k, i, dir];
								if (face == null || face.Aread || !CacheBoneWeightsEquals(face)) {
									breakFlag = true;
								}

							}
						}
						if (!breakFlag) {
							max.Z = i;
							for (int j = min.X; j <= max.X; j++) {
								for (int k = min.Y; k <= max.Y; k++) {
									Faces[j, k, i, dir].Aread = true;
								}
							}
						}
					}
				}
			}

		}




		private static void GetResultFromArea (out UnlimitiedMesh uMesh, out Texture2D texture) {

			// Texture
			var rects = RectPacking.PackTextures(out texture, PackingList, true, false);

			// Mesh
			int matCount = MaterialPalette.Count;
			var vertss = new List<List<Vector3>>();
			var uvss = new List<List<Vector2>>();
			var boneWeights = new List<BoneWeight>();
			Vector3 modelOffset = new Vector3(
				0.5f - SizeX * Pivot.x,
				0.5f - SizeY * Pivot.y,
				0.5f - SizeZ * Pivot.z
			);

			for (int i = 0; i < matCount; i++) {
				vertss.Add(new List<Vector3>());
				uvss.Add(new List<Vector2>());
			}

			for (int i = 0; i < AreaList.Count; i++) {
				var area = AreaList[i];
				int matIndex = area.MaterialMapIndex;

				// Vert
				vertss[matIndex].Add((area.A + modelOffset) * ModelScale);
				vertss[matIndex].Add((area.B + modelOffset) * ModelScale);
				vertss[matIndex].Add((area.C + modelOffset) * ModelScale);
				vertss[matIndex].Add((area.D + modelOffset) * ModelScale);

				// Weight
				if (PointWeights != null) {
					boneWeights.Add(PointWeights[Mathf.RoundToInt(area.A.x + 0.5f), Mathf.RoundToInt(area.A.y + 0.5f), Mathf.RoundToInt(area.A.z + 0.5f)]);
					boneWeights.Add(PointWeights[Mathf.RoundToInt(area.B.x + 0.5f), Mathf.RoundToInt(area.B.y + 0.5f), Mathf.RoundToInt(area.B.z + 0.5f)]);
					boneWeights.Add(PointWeights[Mathf.RoundToInt(area.C.x + 0.5f), Mathf.RoundToInt(area.C.y + 0.5f), Mathf.RoundToInt(area.C.z + 0.5f)]);
					boneWeights.Add(PointWeights[Mathf.RoundToInt(area.D.x + 0.5f), Mathf.RoundToInt(area.D.y + 0.5f), Mathf.RoundToInt(area.D.z + 0.5f)]);
				}

				// UV
				Rect uvRect = SupportLightMap == LightMapSupportType.SupportLightmapButLargeTexture ? rects[i] : rects[AreaPackingMap[i]];
				if (area.IsLUB) {
					uvss[matIndex].Add(uvRect.min);
					uvss[matIndex].Add(new Vector2(uvRect.x + uvRect.width, uvRect.y));
					uvss[matIndex].Add(uvRect.max);
					uvss[matIndex].Add(new Vector2(uvRect.x, uvRect.y + uvRect.height));
				} else {
					uvss[matIndex].Add(uvRect.min);
					uvss[matIndex].Add(new Vector2(uvRect.x, uvRect.y + uvRect.height));
					uvss[matIndex].Add(uvRect.max);
					uvss[matIndex].Add(new Vector2(uvRect.x + uvRect.width, uvRect.y));
				}

			}

			// Clear 
			//for (int i = 0; i < vertss.Count; i++) {
			//	if (vertss[i].Count == 0) {
			//		vertss.RemoveAt(i);
			//		uvss.RemoveAt(i);
			//		i--;
			//	}
			//} Fuuuuuuuuck!!!!!!!!

			// Create Mesh
			uMesh = new UnlimitiedMesh(vertss, uvss, boneWeights);

		}





		#endregion




		#region --- UTL ---




		// Data
		private static Result.VoxelNode GetTransformData (Result.VoxelModel vModel, Texture2D[] textures, int nodeID) {
			var node = new Result.VoxelNode();
			var meshs = vModel.Meshs;
			if (Data.Transforms.ContainsKey(nodeID)) {
				var tfData = Data.Transforms[nodeID];
				var frames = tfData.Frames;
				if (frames.Length > 0) {
					node.Position = new Int3(frames[0].Position);
					node.Rotation = Quaternion.Euler(frames[0].Rotation);
					node.Scale = new Int3(frames[0].Scale);
				}
				if (!string.IsNullOrEmpty(tfData.Name)) {
					node.Name = tfData.Name;
				}
				node.Active = !tfData.Hidden;
				if (Data.Groups.ContainsKey(tfData.ChildID)) {
					var groupData = Data.Groups[tfData.ChildID];
					if (groupData.ChildNodeId != null) {
						node.Children = new Result.VoxelNode[groupData.ChildNodeId.Length];
						for (int i = 0; i < groupData.ChildNodeId.Length; i++) {
							node.Children[i] = GetTransformData(vModel, textures, groupData.ChildNodeId[i]);
						}
					}
				} else if (Data.Shapes.ContainsKey(tfData.ChildID)) {
					var shapeData = Data.Shapes[tfData.ChildID];
					if (shapeData.ModelData != null && shapeData.ModelData.Length > 0) {
						var mData = shapeData.ModelData[0];
						node.Model = meshs[mData.Key];
						node.Texture = textures[mData.Key];
						node.Size = new Int3(vModel.ModelSize[mData.Key]);
					}
				}
			}

			return node;
		}




		// Bone Weight
		private static bool CacheBoneWeightsEquals (Face face) {
			SetWeightPointCache(face.X, face.Y, face.Z, face.Direction, true);
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					if (!Weight_Point_Cache[i].Equals(Weight_Point_Cache[j + 4])) {

						return false;
					}
				}
			}

			return true;
		}




		private static void SetWeightPointCache (int x, int y, int z, Direction dir, bool isAlt) {
			int index = isAlt ? 4 : 0;
			switch (dir) {
				case Direction.Back:
					Weight_Point_Cache[index + 0] = PointWeights[x, y, z];
					Weight_Point_Cache[index + 1] = PointWeights[x + 1, y, z];
					Weight_Point_Cache[index + 2] = PointWeights[x, y + 1, z];
					Weight_Point_Cache[index + 3] = PointWeights[x + 1, y + 1, z];
					break;
				case Direction.Front:
					Weight_Point_Cache[index + 0] = PointWeights[x, y, z + 1];
					Weight_Point_Cache[index + 1] = PointWeights[x + 1, y, z + 1];
					Weight_Point_Cache[index + 2] = PointWeights[x, y + 1, z + 1];
					Weight_Point_Cache[index + 3] = PointWeights[x + 1, y + 1, z + 1];
					break;
				case Direction.Up:
					Weight_Point_Cache[index + 0] = PointWeights[x, y + 1, z];
					Weight_Point_Cache[index + 1] = PointWeights[x + 1, y + 1, z];
					Weight_Point_Cache[index + 2] = PointWeights[x, y + 1, z + 1];
					Weight_Point_Cache[index + 3] = PointWeights[x + 1, y + 1, z + 1];
					break;
				case Direction.Down:
					Weight_Point_Cache[index + 0] = PointWeights[x, y, z];
					Weight_Point_Cache[index + 1] = PointWeights[x + 1, y, z];
					Weight_Point_Cache[index + 2] = PointWeights[x, y, z + 1];
					Weight_Point_Cache[index + 3] = PointWeights[x + 1, y, z + 1];
					break;
				case Direction.Left:
					Weight_Point_Cache[index + 0] = PointWeights[x, y, z];
					Weight_Point_Cache[index + 1] = PointWeights[x, y + 1, z];
					Weight_Point_Cache[index + 2] = PointWeights[x, y, z + 1];
					Weight_Point_Cache[index + 3] = PointWeights[x, y + 1, z + 1];
					break;
				case Direction.Right:
					Weight_Point_Cache[index + 0] = PointWeights[x + 1, y, z];
					Weight_Point_Cache[index + 1] = PointWeights[x + 1, y + 1, z];
					Weight_Point_Cache[index + 2] = PointWeights[x + 1, y, z + 1];
					Weight_Point_Cache[index + 3] = PointWeights[x + 1, y + 1, z + 1];
					break;
			}
		}



		private static BoneWeight GetBoneWeightFromVoxelWeights (Vector3 pos, VoxelData.RigData.Weight[,,] weights) {

			Int3 floor = new Int3(
				Mathf.Clamp(Mathf.FloorToInt(pos.x), 0, SizeX - 1),
				Mathf.Clamp(Mathf.FloorToInt(pos.y), 0, SizeY - 1),
				Mathf.Clamp(Mathf.FloorToInt(pos.z), 0, SizeZ - 1)
			);
			Int3 ceil = new Int3(
				Mathf.Clamp(Mathf.CeilToInt(pos.x), 0, SizeX - 1),
				Mathf.Clamp(Mathf.CeilToInt(pos.y), 0, SizeY - 1),
				Mathf.Clamp(Mathf.CeilToInt(pos.z), 0, SizeZ - 1)
			);

			// Clear Cache
			for (int i = 0; i < 16; i++) {
				WEIGHT_COUNT_CACHE[i] = 0;
				WEIGHT_BONE_INDEX_CACHE[i] = -1;
			}

			// Get Counts
			for (int x = floor.X; x <= ceil.X; x++) {
				for (int y = floor.Y; y <= ceil.Y; y++) {
					for (int z = floor.Z; z <= ceil.Z; z++) {
						var w = weights[x, y, z];
						if (w == null) { continue; }
						CountWeight(w.BoneIndexA);
						CountWeight(w.BoneIndexB);
					}
				}
			}

			// Get Index And Weight
			int maxCacheIndex0 = -1;
			int maxCacheIndex1 = -1;
			int maxCacheIndex2 = -1;
			int maxCacheIndex3 = -1;
			int maxCacheCount0 = 0;
			int maxCacheCount1 = 0;
			int maxCacheCount2 = 0;
			int maxCacheCount3 = 0;

			for (int i = 0; i < 16; i++) {
				if (WEIGHT_COUNT_CACHE[i] >= maxCacheCount0) {
					maxCacheCount3 = maxCacheCount2;
					maxCacheCount2 = maxCacheCount1;
					maxCacheCount1 = maxCacheCount0;
					maxCacheCount0 = WEIGHT_COUNT_CACHE[i];
					maxCacheIndex3 = maxCacheIndex2;
					maxCacheIndex2 = maxCacheIndex1;
					maxCacheIndex1 = maxCacheIndex0;
					maxCacheIndex0 = i;
				}
			}

			// Get Index and Weight
			int sumCount = maxCacheCount0 + maxCacheCount1 + maxCacheCount2 + maxCacheCount3;
			if (sumCount == 0) {
				sumCount = 1;
				maxCacheCount0 = 1;
			}

			// Fix -1 Value
			for (int i = 0; i < 16; i++) {
				if (WEIGHT_BONE_INDEX_CACHE[i] == -1) {
					WEIGHT_BONE_INDEX_CACHE[i] = 0;
				}
			}

			// Sort
			int boneIndex0 = maxCacheIndex0 >= 0 && maxCacheIndex0 < 16 ? WEIGHT_BONE_INDEX_CACHE[maxCacheIndex0] : 0;
			int boneIndex1 = maxCacheIndex1 >= 0 && maxCacheIndex1 < 16 ? WEIGHT_BONE_INDEX_CACHE[maxCacheIndex1] : 0;
			int boneIndex2 = maxCacheIndex2 >= 0 && maxCacheIndex2 < 16 ? WEIGHT_BONE_INDEX_CACHE[maxCacheIndex2] : 0;
			int boneIndex3 = maxCacheIndex3 >= 0 && maxCacheIndex3 < 16 ? WEIGHT_BONE_INDEX_CACHE[maxCacheIndex3] : 0;

			if (maxCacheCount0 == maxCacheCount1) {
				if (boneIndex0 > boneIndex1) {
					int temp = boneIndex0;
					boneIndex0 = boneIndex1;
					boneIndex1 = temp;
				}
			}

			if (maxCacheCount0 == maxCacheCount2) {
				if (boneIndex0 > boneIndex2) {
					int temp = boneIndex0;
					boneIndex0 = boneIndex2;
					boneIndex2 = temp;
				}
			}

			if (maxCacheCount0 == maxCacheCount3) {
				if (boneIndex0 > boneIndex3) {
					int temp = boneIndex0;
					boneIndex0 = boneIndex3;
					boneIndex3 = temp;
				}
			}


			return new BoneWeight() {
				boneIndex0 = boneIndex0,
				boneIndex1 = boneIndex1,
				boneIndex2 = boneIndex2,
				boneIndex3 = boneIndex3,
				weight0 = (float)maxCacheCount0 / sumCount,
				weight1 = (float)maxCacheCount1 / sumCount,
				weight2 = (float)maxCacheCount2 / sumCount,
				weight3 = (float)maxCacheCount3 / sumCount,
			};
		}



		private static void CountWeight (int boneIndex) {
			if (boneIndex == -1) { return; }
			for (int i = 0; i < 16; i++) {
				if (WEIGHT_BONE_INDEX_CACHE[i] == boneIndex) {
					WEIGHT_COUNT_CACHE[i]++;
					return;
				}
			}
			for (int i = 0; i < 16; i++) {
				if (WEIGHT_BONE_INDEX_CACHE[i] == -1) {
					WEIGHT_BONE_INDEX_CACHE[i] = boneIndex;
					WEIGHT_COUNT_CACHE[i]++;
					return;
				}
			}
		}





		#endregion





	}
}