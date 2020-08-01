namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;




	public enum Direction {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		Front = 4,
		Back = 5,
	}



	public class VoxelData {



		#region --- SUB ---


		[System.Serializable]
		public class MaterialData {


			public const int SHADER_NUM = 5;
			public const int SHADER_PROPERTY_NUM = 13;
			public static readonly Vector2[] SHADER_VALUE_REMAP_SOURCE = new Vector2[SHADER_PROPERTY_NUM] {
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 1),
				new Vector2(0, 4),
				new Vector2(0, 1),
			};



			public enum MaterialType {
				Diffuse = 0,
				Metal = 1,
				Glass = 2,
				Emit = 3,
			}


			public MaterialType Type;
			public int Index;
			public float Weight;
			public float Rough;
			public float Spec;
			public float Ior;
			public float Att;
			public float Flux;
			public float LDR;
			public int Plastic;


			public static MaterialType GetTypeFromString (string voxStr) {
				switch (voxStr) {
					default:
					case "_diffuse":
						return MaterialType.Diffuse;
					case "_metal":
						return MaterialType.Metal;
					case "_glass":
						return MaterialType.Glass;
					case "_emit":
						return MaterialType.Emit;
				}
			}
			public static string GetStringFromType (MaterialType type) {
				switch (type) {
					default:
					case MaterialType.Diffuse:
						return "_diffuse";
					case MaterialType.Metal:
						return "_metal";
					case MaterialType.Glass:
						return "_glass";
					case MaterialType.Emit:
						return "_emit";
				}
			}


			public bool IsSameWith (MaterialData other) {
				if (Type != other.Type) { return false; }
				switch (Type) {
					default:
					case MaterialType.Diffuse:
						return true;
					case MaterialType.Metal:
						return Weight == other.Weight && Rough == other.Rough && Spec == other.Spec && Plastic == other.Plastic;
					case MaterialType.Glass:
						return Weight == other.Weight && Rough == other.Rough && Ior == other.Ior && Att == other.Att;
					case MaterialType.Emit:
						return Weight == other.Weight && Flux == other.Flux && LDR == other.LDR;
				}
			}
		}


		[System.Serializable]
		public class TransformData {

			[System.Serializable]
			public class FrameData {
				public Vector3 Rotation;
				public Vector3 Position;
				public Vector3 Scale;
			}

			public int ChildID;
			public int LayerID;
			public string Name;
			public bool Hidden;
			public int Reserved;
			public FrameData[] Frames;

		}


		public class GroupData {
			public Dictionary<string, string> Attributes;
			public int[] ChildNodeId;
		}


		public class ShapeData {
			public Dictionary<string, string> Attributes;
			public KeyValuePair<int, Dictionary<string, string>>[] ModelData;
		}



		[System.Serializable]
		public class RigData {



			[System.Serializable]
			public class Bone {

				[System.NonSerialized] public Bone Parent = null;
				[System.NonSerialized] public int ChildCount = 0;

				public int ParentIndex;// root >> -1
				public string Name;
				public int PositionX;
				public int PositionY;
				public int PositionZ;

				public static implicit operator bool (Bone bone) {
					return bone != null;
				}

			}



			[System.Serializable]
			public class Weight {

				public int X;
				public int Y;
				public int Z;
				public int BoneIndexA = -1;
				public int BoneIndexB = -1;



				public Weight () {
					X = 0;
					Y = 0;
					Z = 0;
					BoneIndexA = -1;
					BoneIndexB = -1;
				}



				public Weight (int indexA, int indexB) {
					X = 0;
					Y = 0;
					Z = 0;
					BoneIndexA = indexA;
					BoneIndexB = indexB;
				}



				public float GetWeight (int boneIndex) {
					float weight = 0f;
					if (boneIndex == BoneIndexA || boneIndex == BoneIndexB) {
						weight = 0.5f;
						if (BoneIndexA == -1 || BoneIndexB == -1) {
							weight = 1f;
						}
					}
					return weight;
				}



				public void SetWeight (int boneIndex) {
					if (BoneIndexA == boneIndex || BoneIndexB == boneIndex) {
						return;
					} else if (BoneIndexA == -1) {
						BoneIndexA = boneIndex;
					} else if (BoneIndexB == -1) {
						BoneIndexB = boneIndex;
					} else {
						BoneIndexB = BoneIndexA;
						BoneIndexA = boneIndex;
					}
				}



				public bool IndexEqualsTo (Weight other) {
					return (BoneIndexA == other.BoneIndexA && BoneIndexB == other.BoneIndexB) ||
						(BoneIndexA == other.BoneIndexB && BoneIndexB == other.BoneIndexA);
				}




			}



			public const int CURRENT_VERSION = 1;


			public List<Bone> Bones;
			public List<Weight> Weights;
			public int Version = 0;



			public void FixVersion () {
				if (Version == 0) {
					if (Bones != null) {
						for (int i = 0; i < Bones.Count; i++) {
							var bone = Bones[i];
							bone.PositionX *= 2;
							bone.PositionY *= 2;
							bone.PositionZ *= 2;
						}
					}
				}
				Version = CURRENT_VERSION;
			}




			public static List<Bone> GetHumanBones (Vector3 size, bool fullSize) {


				var bones = new List<Bone>();


				// Body
				var hip = GetNewBone("Hips", null, 0.5f - 1f / size.x, 0.5f - 1f / size.y, 0.5f - 1f / size.z, size);
				var spine = GetNewBone("Spine", hip, 0, 0.05f, 0, size);
				var chest = GetNewBone("Chest", spine, 0, 0.05f, 0, size);
				var _upperChest = GetNewBone("UpperChest", chest, 0, 0.05f, 0, size);
				var neck = GetNewBone("Neck", fullSize ? _upperChest : chest, 0, 0.15f, 0, size);
				var head = GetNewBone("Head", neck, 0, 0.05f, 0, size);


				// Head
				var _eyeL = GetNewBone("LeftEye", head, -0.1f, 0.1f, 0, size);
				var _eyeR = GetNewBone("RightEye", head, 0.1f, 0.1f, 0, size);
				var _jaw = GetNewBone("Jaw", head, 0, 0.05f, 0, size);


				// Arm
				var _leftShoulder = GetNewBone("LeftShoulder", _upperChest, -0.05f, 0, 0, size);
				var _rightShoulder = GetNewBone("RightShoulder", _upperChest, 0.05f, 0, 0, size);

				var armUL = GetNewBone("LeftUpperArm", fullSize ? _leftShoulder : chest, -0.1f, 0.05f, 0, size);
				var armUR = GetNewBone("RightUpperArm", fullSize ? _rightShoulder : chest, 0.1f, 0.05f, 0, size);

				var armDL = GetNewBone("LeftLowerArm", armUL, -0.2f, 0, 0, size);
				var armDR = GetNewBone("RightLowerArm", armUR, 0.2f, 0, 0, size);

				var handL = GetNewBone("LeftHand", armDL, -0.2f, 0, 0, size);
				var handR = GetNewBone("RightHand", armDR, 0.2f, 0, 0, size);


				// Leg
				var legUL = GetNewBone("LeftUpperLeg", hip, -0.05f, 0, 0, size);
				var legUR = GetNewBone("RightUpperLeg", hip, 0.05f, 0, 0, size);

				var legDL = GetNewBone("LeftLowerLeg", legUL, 0, -0.25f, 0, size);
				var legDR = GetNewBone("RightLowerLeg", legUR, 0, -0.25f, 0, size);

				var footL = GetNewBone("LeftFoot", legDL, 0, -0.25f, 0, size);
				var footR = GetNewBone("RightFoot", legDR, 0, -0.25f, 0, size);

				var _toeL = GetNewBone("LeftToes", footL, 0, 0, -0.05f, size);
				var _toeR = GetNewBone("RightToes", footR, 0, 0, -0.05f, size);



				// Hand   
				// Proximal > Intermediate > Distal
				// Thumb Index Middle Ring Little
				var _thumbProximalL = GetNewBone("Left Thumb Proximal", handL, -0.05f, 0, 0, size);
				var _thumbProximalR = GetNewBone("Right Thumb Proximal", handR, 0.05f, 0, 0, size);
				var _thumbIntermediateL = GetNewBone("Left Thumb Intermediate", _thumbProximalL, -0.05f, 0, 0, size);
				var _thumbIntermediateR = GetNewBone("Right Thumb Intermediate", _thumbProximalR, 0.05f, 0, 0, size);
				var _thumbDistalL = GetNewBone("Left Thumb Distal", _thumbIntermediateL, -0.05f, 0, 0, size);
				var _thumbDistalR = GetNewBone("Right Thumb Distal", _thumbIntermediateR, 0.05f, 0, 0, size);

				var _indexProximalL = GetNewBone("Left Index Proximal", handL, -0.05f, 0, 0, size);
				var _indexProximalR = GetNewBone("Right Index Proximal", handR, 0.05f, 0, 0, size);
				var _indexIntermediateL = GetNewBone("Left Index Intermediate", _indexProximalL, -0.05f, 0, 0, size);
				var _indexIntermediateR = GetNewBone("Right Index Intermediate", _indexProximalR, 0.05f, 0, 0, size);
				var _indexDistalL = GetNewBone("Left Index Distal", _indexIntermediateL, -0.05f, 0, 0, size);
				var _indexDistalR = GetNewBone("Right Index Distal", _indexIntermediateR, 0.05f, 0, 0, size);

				var _middleProximalL = GetNewBone("Left Middle Proximal", handL, -0.05f, 0, 0, size);
				var _middleProximalR = GetNewBone("Right Middle Proximal", handR, 0.05f, 0, 0, size);
				var _middleIntermediateL = GetNewBone("Left Middle Intermediate", _middleProximalL, -0.05f, 0, 0, size);
				var _middleIntermediateR = GetNewBone("Right Middle Intermediate", _middleProximalR, 0.05f, 0, 0, size);
				var _middleDistalL = GetNewBone("Left Middle Distal", _middleIntermediateL, -0.05f, 0, 0, size);
				var _middleDistalR = GetNewBone("Right Middle Distal", _middleIntermediateR, -0.05f, 0, 0, size);

				var _ringProximalL = GetNewBone("Left Ring Proximal", handL, -0.05f, 0, 0, size);
				var _ringProximalR = GetNewBone("Right Ring Proximal", handR, 0.05f, 0, 0, size);
				var _ringIntermediateL = GetNewBone("Left Ring Intermediate", _ringProximalL, -0.05f, 0, 0, size);
				var _ringIntermediateR = GetNewBone("Right Ring Intermediate", _ringProximalR, 0.05f, 0, 0, size);
				var _ringDistalL = GetNewBone("Left Ring Distal", _ringIntermediateL, -0.05f, 0, 0, size);
				var _ringDistalR = GetNewBone("Right Ring Distal", _ringIntermediateR, -0.05f, 0, 0, size);

				var _littleProximalL = GetNewBone("Left Little Proximal", handL, -0.05f, 0, 0, size);
				var _littleProximalR = GetNewBone("Right Little Proximal", handR, 0.05f, 0, 0, size);
				var _littleIntermediateL = GetNewBone("Left Little Intermediate", _littleProximalL, -0.05f, 0, 0, size);
				var _littleIntermediateR = GetNewBone("Right Little Intermediate", _littleProximalR, 0.05f, 0, 0, size);
				var _littleDistalL = GetNewBone("Left Little Distal", _littleIntermediateL, -0.05f, 0, 0, size);
				var _littleDistalR = GetNewBone("Right Little Distal", _littleIntermediateR, -0.05f, 0, 0, size);


				bones.Add(hip);
				bones.Add(spine);
				bones.Add(chest);
				bones.Add(neck);
				bones.Add(head);
				bones.Add(legUL);
				bones.Add(legUR);
				bones.Add(legDL);
				bones.Add(legDR);
				bones.Add(footL);
				bones.Add(footR);
				bones.Add(armUL);
				bones.Add(armUR);
				bones.Add(armDL);
				bones.Add(armDR);
				bones.Add(handL);
				bones.Add(handR);

				if (fullSize) {

					bones.Add(_upperChest);
					bones.Add(_eyeL);
					bones.Add(_eyeR);
					bones.Add(_jaw);
					bones.Add(_leftShoulder);
					bones.Add(_rightShoulder);
					bones.Add(_toeL);
					bones.Add(_toeR);


					bones.Add(_thumbProximalL);
					bones.Add(_thumbProximalR);
					bones.Add(_thumbIntermediateL);
					bones.Add(_thumbIntermediateR);
					bones.Add(_thumbDistalL);
					bones.Add(_thumbDistalR);

					bones.Add(_indexProximalL);
					bones.Add(_indexProximalR);
					bones.Add(_indexIntermediateL);
					bones.Add(_indexIntermediateR);
					bones.Add(_indexDistalL);
					bones.Add(_indexDistalR);

					bones.Add(_middleProximalL);
					bones.Add(_middleProximalR);
					bones.Add(_middleIntermediateL);
					bones.Add(_middleIntermediateR);
					bones.Add(_middleDistalL);
					bones.Add(_middleDistalR);

					bones.Add(_ringProximalL);
					bones.Add(_ringProximalR);
					bones.Add(_ringIntermediateL);
					bones.Add(_ringIntermediateR);
					bones.Add(_ringDistalL);
					bones.Add(_ringDistalR);

					bones.Add(_littleProximalL);
					bones.Add(_littleProximalR);
					bones.Add(_littleIntermediateL);
					bones.Add(_littleIntermediateR);
					bones.Add(_littleDistalL);
					bones.Add(_littleDistalR);

				}

				return bones;
			}



			public static Bone GetNewBone (string name, Bone parent, float x = 0, float y = 0, float z = 0, Vector3 size = default(Vector3)) {
				x *= 2f;
				y *= 2f;
				z *= 2f;
				return new Bone() {
					Name = name,
					Parent = parent,
					PositionX = x > 0 ? Mathf.CeilToInt(x * size.x) : Mathf.FloorToInt(x * size.x),
					PositionY = y > 0 ? Mathf.CeilToInt(y * size.y) : Mathf.FloorToInt(y * size.y),
					PositionZ = z > 0 ? Mathf.CeilToInt(z * size.z) : Mathf.FloorToInt(z * size.z),
				};
			}



		}



		#endregion




		#region --- VAR ---



		public int Version = -1;
		public List<int[,,]> Voxels = new List<int[,,]>();
		public List<Color> Palette = new List<Color>();
		public List<MaterialData> Materials = new List<MaterialData>();
		public Dictionary<int, TransformData> Transforms = new Dictionary<int, TransformData>();
		public Dictionary<int, GroupData> Groups = new Dictionary<int, GroupData>();
		public Dictionary<int, ShapeData> Shapes = new Dictionary<int, ShapeData>();
		public Dictionary<int, RigData> Rigs = new Dictionary<int, RigData>();



		#endregion




		#region --- API ---




		public Color GetColorFromPalette (int index) {
			index--;
			return index >= 0 && index < Palette.Count ? Palette[index] : Color.clear;
		}



		public Vector3 GetModelSize (int index) {
			return new Vector3(
				Voxels[index].GetLength(0),
				Voxels[index].GetLength(1),
				Voxels[index].GetLength(2)
			);
		}



		public Vector3 GetFootPoint (int index) {

			var voxels = Voxels[index];

			int sizeX = voxels.GetLength(0);
			int sizeY = voxels.GetLength(1);
			int sizeZ = voxels.GetLength(2);

			for (int y = 0; y < sizeY; y++) {
				Vector3 foot = Vector3.zero;
				int footCount = 0;
				for (int x = 0; x < sizeX; x++) {
					for (int z = 0; z < sizeZ; z++) {
						var voxel = voxels[x, y, z];
						if (voxel != 0) {
							foot += new Vector3(x, y, z);
							footCount++;
						}
					}
				}
				if (footCount > 0) {
					return foot / footCount;
				}
			}
			return Vector3.zero;
		}



		public Vector3 GetBounds () {
			Vector3 bounds = Vector3.zero;
			for (int i = 0; i < Voxels.Count; i++) {
				var size = GetModelSize(i);
				bounds = Vector3.Max(bounds, size);
			}
			return bounds;
		}



		public VoxelData GetCopy () {
			return new VoxelData() {
				Version = Version,
				Voxels = new List<int[,,]>(Voxels),
				Palette = new List<Color>(Palette),
				Transforms = new Dictionary<int, TransformData>(Transforms),
				Groups = new Dictionary<int, GroupData>(Groups),
				Shapes = new Dictionary<int, ShapeData>(Shapes),
				Materials = new List<MaterialData>(Materials),
				Rigs = new Dictionary<int, RigData>(Rigs),
			};
		}



		public void GetModelTransform (int index, out Vector3 pos, out Vector3 rot, out Vector3 scale) {
			pos = Vector3.zero;
			rot = Vector3.zero;
			scale = Vector3.one;
			foreach (var tf in Transforms) {
				bool success = false;
				int id = tf.Value.ChildID;
				if (Shapes.ContainsKey(id)) {
					var shape = Shapes[id];
					for (int i = 0; i < shape.ModelData.Length; i++) {
						if (shape.ModelData[i].Key == index) {
							if (tf.Value.Frames.Length > 0) {
								var frame = tf.Value.Frames[0];
								pos = frame.Position;
								rot = frame.Rotation;
								scale = frame.Scale;
							}
							success = true;
							break;
						}
					}
				}
				if (success) {
					break;
				}
			}
		}



		public Texture2D GetThumbnail (int index, bool positiveZ = true) {
			if (Voxels == null || index >= Voxels.Count || index < 0) { return null; }
			var voxels = Voxels[index];
			int sizeX = voxels.GetLength(0);
			int sizeY = voxels.GetLength(1);
			int sizeZ = voxels.GetLength(2);
			Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
			var colors = new Color[sizeX * sizeY];
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					Color c = Color.clear;
					for (int z = positiveZ ? 0 : sizeZ - 1; positiveZ ? z < sizeZ : z >= 0; z += positiveZ ? 1 : -1) {
						if (voxels[x, y, z] != 0) {
							c = GetColorFromPalette(voxels[x, y, z]);
							break;
						}
					}
					colors[y * sizeX + (positiveZ ? x : sizeX - 1 - x)] = c;
				}
			}
			texture.filterMode = FilterMode.Point;
			texture.SetPixels(colors);
			texture.Apply();
			return texture;
		}



		public bool IsExposed (int index, int x, int y, int z, int sizeX, int sizeY, int sizeZ, Direction dir, List<MaterialData> materials = null) {
			return IsExposed(Voxels[index], x, y, z, sizeX, sizeY, sizeZ, dir, materials);
		}




		public void ResetToDefaultNode () {
			Transforms.Clear();
			Groups.Clear();
			Shapes.Clear();

			Transforms.Add(0, new TransformData() {
				ChildID = 1,
				Hidden = false,
				LayerID = -1,
				Name = "Root",
				Reserved = -1,
				Frames = new TransformData.FrameData[1] {
					new TransformData.FrameData() {
						Position = Vector3.zero,
						Rotation = Vector3.zero,
						Scale = Vector3.one,
					},
				},
			});

			Groups.Add(1, new GroupData() {
				Attributes = new Dictionary<string, string>(),
				ChildNodeId = new int[1] { 2 },
			});

			Transforms.Add(2, new TransformData() {
				ChildID = 3,
				Hidden = false,
				LayerID = -1,
				Name = "Root",
				Reserved = -1,
				Frames = new TransformData.FrameData[1] {
					new TransformData.FrameData() {
						Position = Vector3.zero,
						Rotation = Vector3.zero,
						Scale = Vector3.one,
					},
				},
			});

			Shapes.Add(3, new ShapeData() {
				Attributes = new Dictionary<string, string>(),
				ModelData = new KeyValuePair<int, Dictionary<string, string>>[1] {
					new KeyValuePair<int, Dictionary<string, string>>(0, new Dictionary<string, string>())
				},
			});

		}



		public void Rotate180InY () {
			if (Shapes.Count == 1) {
				for (int i = 0; i < Voxels.Count; i++) {
					var source = Voxels[i];
					int sizeX = source.GetLength(0);
					int sizeY = source.GetLength(1);
					int sizeZ = source.GetLength(2);
					var voxels = new int[sizeX, sizeY, sizeZ];
					for (int x = 0; x < sizeX; x++) {
						for (int y = 0; y < sizeY; y++) {
							for (int z = 0; z < sizeZ; z++) {
								voxels[x, y, z] = source[sizeX - x - 1, y, sizeZ - z - 1];
							}
						}
					}
					Voxels[i] = voxels;
				}
			} else if (Transforms.ContainsKey(0) && Transforms[0].Frames.Length > 0) {
				Transforms[0].Frames[0].Rotation.y += 180;
			}
		}



		// Global API
		public static VoxelData CreateNewData () {
			var data = new VoxelData() {
				Version = 150,
				Materials = new List<MaterialData>(),
				Palette = new List<Color>() { Color.white },
				Rigs = new Dictionary<int, RigData>(),
				Voxels = new List<int[,,]>() {
					new int[1,1,1]{ { { 1 } } },
				},
			};
			data.ResetToDefaultNode();
			return data;
		}



		public static bool IsExposed (int[,,] voxels, int x, int y, int z, int sizeX, int sizeY, int sizeZ, Direction dir, List<MaterialData> materials = null) {
			bool exposed = false;
			if (voxels[x, y, z] != 0) {
				switch (dir) {
					case Direction.Up:
						exposed = y == sizeY - 1 || voxels[x, y + 1, z] == 0;
						break;
					case Direction.Down:
						exposed = y == 0 || voxels[x, y - 1, z] == 0;
						break;
					case Direction.Left:
						exposed = x == 0 || voxels[x - 1, y, z] == 0;
						break;
					case Direction.Right:
						exposed = x == sizeX - 1 || voxels[x + 1, y, z] == 0;
						break;
					case Direction.Front:
						exposed = z == sizeZ - 1 || voxels[x, y, z + 1] == 0;
						break;
					case Direction.Back:
						exposed = z == 0 || voxels[x, y, z - 1] == 0;
						break;
				}
				if (materials != null && !exposed) {
					int voxel = voxels[x, y, z];
					var material = voxel >= 0 && voxel < materials.Count ? materials[voxel] : null;
					if (material != null && material.Type != MaterialData.MaterialType.Glass) {
						int blockingVoxel = 0;
						switch (dir) {
							case Direction.Up:
								blockingVoxel = y == sizeY - 1 ? -1 : voxels[x, y + 1, z];
								break;
							case Direction.Down:
								blockingVoxel = y == 0 ? -1 : voxels[x, y - 1, z];
								break;
							case Direction.Left:
								blockingVoxel = x == 0 ? -1 : voxels[x - 1, y, z];
								break;
							case Direction.Right:
								blockingVoxel = x == sizeX - 1 ? -1 : voxels[x + 1, y, z];
								break;
							case Direction.Front:
								blockingVoxel = z == sizeZ - 1 ? -1 : voxels[x, y, z + 1];
								break;
							case Direction.Back:
								blockingVoxel = z == 0 ? -1 : voxels[x, y, z - 1];
								break;
						}
						var blockingMaterial = blockingVoxel >= 0 && blockingVoxel < materials.Count ? materials[blockingVoxel] : null;
						if (blockingMaterial != null && blockingMaterial.Type == MaterialData.MaterialType.Glass) {
							exposed = true;
						}
					}
				}
			}
			return exposed;
		}



		public static Material GetMaterialFrom (MaterialData mat, Texture2D texture, Shader[] shaders, string[] keywords, Vector2[] remaps, string mainTexKeyword) {
			if (mat == null || shaders == null || keywords == null || remaps == null) {
				return new Material(shaders != null && shaders.Length > 0 ? shaders[0] : Shader.Find("Mobile/Diffuse")) { mainTexture = texture };
			}
			Material material;
			if (shaders.Length < 5) {
				var shader = shaders.Length > 0 ? shaders[0] : Shader.Find("Mobile/Diffuse");
				shaders = new Shader[5] { shader, shader, shader, shader, shader };
			}
			switch (mat.Type) {
				default:
				case MaterialData.MaterialType.Diffuse:
					material = new Material(shaders[0]);
					break;
				case MaterialData.MaterialType.Metal:
					if (mat.Plastic == 0) {
						material = new Material(shaders[1]);
						Util.SetMaterialFloatOrColor(material, keywords[0], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[0].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[0].y, remaps[0].x, remaps[0].y, mat.Weight));// Weight
						Util.SetMaterialFloatOrColor(material, keywords[1], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[1].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[1].y, remaps[1].x, remaps[1].y, mat.Rough));// Rough
						Util.SetMaterialFloatOrColor(material, keywords[2], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[2].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[2].y, remaps[2].x, remaps[2].y, mat.Spec));// Specular
					} else {
						material = new Material(shaders[2]);
						Util.SetMaterialFloatOrColor(material, keywords[3], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[3].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[3].y, remaps[3].x, remaps[3].y, mat.Weight));// Weight
						Util.SetMaterialFloatOrColor(material, keywords[4], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[4].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[4].y, remaps[4].x, remaps[4].y, mat.Rough));// Rough
						Util.SetMaterialFloatOrColor(material, keywords[5], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[5].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[5].y, remaps[5].x, remaps[5].y, mat.Spec));// Specular
					}
					break;
				case MaterialData.MaterialType.Glass:
					material = new Material(shaders[3]);
					Util.SetMaterialFloatOrColor(material, keywords[6], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[6].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[6].y, remaps[6].x, remaps[6].y, mat.Weight));// Weight
					Util.SetMaterialFloatOrColor(material, keywords[7], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[7].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[7].y, remaps[7].x, remaps[7].y, mat.Rough));// Rough
					Util.SetMaterialFloatOrColor(material, keywords[8], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[8].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[8].y, remaps[8].x, remaps[8].y, mat.Ior));// Refract
					Util.SetMaterialFloatOrColor(material, keywords[9], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[9].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[9].y, remaps[9].x, remaps[9].y, mat.Att));// Att
					break;
				case MaterialData.MaterialType.Emit:
					material = new Material(shaders[4]);
					Util.SetMaterialFloatOrColor(material, keywords[10], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[10].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[10].y, remaps[10].x, remaps[10].y, mat.Weight));// Weight
					Util.SetMaterialFloatOrColor(material, keywords[11], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[11].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[11].y, remaps[11].x, remaps[11].y, mat.Flux));// Power
					Util.SetMaterialFloatOrColor(material, keywords[12], Util.Remap(MaterialData.SHADER_VALUE_REMAP_SOURCE[12].x, MaterialData.SHADER_VALUE_REMAP_SOURCE[12].y, remaps[12].x, remaps[12].y, mat.LDR));// ldr
					break;
			}
			material.SetTexture(mainTexKeyword, texture);
			return material;
		}



		public static implicit operator bool (VoxelData data) {
			return data != null;
		}



		public static VoxelData GetSplitedData (VoxelData source) {


			const int SPLIT_SIZE = 126;
			if (source.Voxels.Count == 0) {
				return source;
			}

			var size = source.GetModelSize(0);
			int sizeX = Mathf.RoundToInt(size.x);
			int sizeY = Mathf.RoundToInt(size.y);
			int sizeZ = Mathf.RoundToInt(size.z);
			if (sizeX <= SPLIT_SIZE && sizeY <= SPLIT_SIZE && sizeZ <= SPLIT_SIZE) {
				return source;
			}

			int splitCountX = (sizeX / SPLIT_SIZE) + 1;
			int splitCountY = (sizeY / SPLIT_SIZE) + 1;
			int splitCountZ = (sizeZ / SPLIT_SIZE) + 1;

			var data = new VoxelData();

			// Nodes
			var childNodeId = new int[splitCountX * splitCountY * splitCountZ];
			for (int i = 0; i < childNodeId.Length; i++) {
				childNodeId[i] = i * 2 + 2;
			}

			data.Voxels = new List<int[,,]>();
			data.Transforms = new Dictionary<int, TransformData>() {
				{ 0, new TransformData(){
					Name = "",
					ChildID = 1,
					Hidden = false,
					LayerID = 0,
					Reserved = 0,
					Frames = new TransformData.FrameData[1]{
						new TransformData.FrameData(){
							Position = Vector3.zero,
							Rotation = Vector3.zero,
							Scale = Vector3.one,
						},
					},
				}},
			};
			data.Groups = new Dictionary<int, GroupData>() {
				{1, new GroupData(){
					Attributes = new Dictionary<string, string>(),
					ChildNodeId = childNodeId,
				}},
			};
			data.Shapes = new Dictionary<int, ShapeData>();
			data.Palette = new List<Color>(source.Palette);
			data.Version = source.Version;

			int _i = 0;
			for (int x = 0; x < splitCountX; x++) {
				for (int y = 0; y < splitCountY; y++) {
					for (int z = 0; z < splitCountZ; z++) {
						int splitSizeX = x < splitCountX - 1 ? SPLIT_SIZE : (sizeX - x * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeY = y < splitCountY - 1 ? SPLIT_SIZE : (sizeY - y * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeZ = z < splitCountZ - 1 ? SPLIT_SIZE : (sizeZ - z * SPLIT_SIZE) % SPLIT_SIZE;
						int childID = childNodeId[_i];
						data.Transforms.Add(childID, new TransformData() {
							Name = "Splited_Model_" + _i,
							Reserved = 0,
							LayerID = 0,
							Hidden = false,
							ChildID = childID + 1,
							Frames = new TransformData.FrameData[1] {
								new TransformData.FrameData(){
									Position = new Vector3(
										x * SPLIT_SIZE + splitSizeX / 2,
										y * SPLIT_SIZE + splitSizeY / 2,
										z * SPLIT_SIZE + splitSizeZ / 2
									),
									Rotation = Vector3.zero,
									Scale = Vector3.one,
								},
							},
						});
						data.Shapes.Add(childID + 1, new ShapeData() {
							Attributes = new Dictionary<string, string>(),
							ModelData = new KeyValuePair<int, Dictionary<string, string>>[] {
								new KeyValuePair<int, Dictionary<string, string>>(_i, new Dictionary<string, string>()),
							},
						});
						_i++;
					}
				}
			}


			// Split
			var sourceVoxels = source.Voxels[0];
			for (int x = 0; x < splitCountX; x++) {
				for (int y = 0; y < splitCountY; y++) {
					for (int z = 0; z < splitCountZ; z++) {
						int splitSizeX = x < splitCountX - 1 ? SPLIT_SIZE : (sizeX - x * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeY = y < splitCountY - 1 ? SPLIT_SIZE : (sizeY - y * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeZ = z < splitCountZ - 1 ? SPLIT_SIZE : (sizeZ - z * SPLIT_SIZE) % SPLIT_SIZE;
						var voxels = new int[splitSizeX, splitSizeY, splitSizeZ];
						for (int i = 0; i < splitSizeX; i++) {
							for (int j = 0; j < splitSizeY; j++) {
								for (int k = 0; k < splitSizeZ; k++) {
									voxels[i, j, k] = sourceVoxels[
										x * SPLIT_SIZE + i,
										y * SPLIT_SIZE + j,
										z * SPLIT_SIZE + k
									];
								}
							}
						}
						data.Voxels.Add(voxels);
					}
				}
			}

			return data;
		}



		#endregion




		#region --- LOD ---



		public void LodIteration (int lodLevel) {


			Rigs.Clear();

			for (int index = 0; index < Voxels.Count; index++) {
				var size = GetModelSize(index);
				int sizeX = (int)size.x;
				int sizeY = (int)size.y;
				int sizeZ = (int)size.z;
				int range = lodLevel + Mathf.Max(1, (sizeX + sizeY + sizeZ) / 42);
				bool allZero = true;
				int[,,] voxels = new int[sizeX, sizeY, sizeZ];
				int[,,] sourceVoxels = Voxels[index];
				for (int x = 0; x < sizeX - range; x++) {
					for (int y = 0; y < sizeY - range; y++) {
						for (int z = 0; z < sizeZ - range; z++) {
							if (sourceVoxels[x, y, z] == 0) { continue; }
							if (allZero) { allZero = false; }
							SetRange(
								ref voxels, sourceVoxels[x, y, z],
								Mathf.Max(0, x - range), Mathf.Min(sizeX - 1, x + range),
								Mathf.Max(0, y - range), Mathf.Min(sizeY - 1, y + range),
								Mathf.Max(0, z - range), Mathf.Min(sizeZ - 1, z + range)
							);
						}
					}
				}
				if (allZero) {
					voxels[0, 0, 0] = 1;
				}
				Voxels[index] = voxels;
			}

		}



		private void SetRange (ref int[,,] voxels, int v, int l, int r, int d, int u, int b, int f) {
			for (int x = l; x < r; x++) {
				for (int y = d; y < u; y++) {
					for (int z = b; z < f; z++) {
						voxels[x, y, z] = v;
					}
				}
			}
		}





		#endregion


	}





	public class QbData {


		public struct QbMatrix {

			public string Name;
			public int SizeX, SizeY, SizeZ;
			public int PosX, PosY, PosZ;
			public int[,,] Voxels;

		}



		public uint Version;
		public uint ColorFormat; // 0->RGBA 1->BGRA
		public uint ZAxisOrientation; // 0->Left Handed  1->Right Handed
		public uint Compressed; // 0->Normal  1->WithNumbers
		public uint NumMatrixes;
		public uint VisibleMask;



		public List<QbMatrix> MatrixList;



		public VoxelData GetVoxelData () {
			var data = new VoxelData {
				Version = 150,
				Palette = new List<Color>(),
				Transforms = new Dictionary<int, VoxelData.TransformData>() {
					{ 0, new VoxelData.TransformData(){
						ChildID = 1,
						Hidden = false,
						LayerID = -1,
						Name = "",
						Reserved = -1,
						Frames = new VoxelData.TransformData.FrameData[1] {new VoxelData.TransformData.FrameData() {
							 Position = Vector3.zero,
							 Rotation = Vector3.zero,
							 Scale = Vector3.one,
						}},
					}},
				},
				Shapes = new Dictionary<int, VoxelData.ShapeData>(),
				Groups = new Dictionary<int, VoxelData.GroupData>() {
					{1, new VoxelData.GroupData(){
						 Attributes = new Dictionary<string, string>(),
						 ChildNodeId = new int[MatrixList.Count],
					}}
				},
				Materials = new List<VoxelData.MaterialData>(),
				Voxels = new List<int[,,]>(),
			};

			bool leftHanded = ZAxisOrientation == 0;

			var palette = new Dictionary<Color, int>();
			for (int index = 0; index < MatrixList.Count; index++) {

				var m = MatrixList[index];
				int[,,] voxels = new int[m.SizeX, m.SizeY, m.SizeZ];

				for (int x = 0; x < m.SizeX; x++) {
					for (int y = 0; y < m.SizeY; y++) {
						for (int z = 0; z < m.SizeZ; z++) {
							int colorInt = m.Voxels[x, y, z];
							if (colorInt == 0) {
								if (leftHanded) {
									voxels[x, y, m.SizeZ - z - 1] = 0;
								} else {
									voxels[x, y, z] = 0;
								}
							} else {
								var color = GetColor(colorInt);
								int cIndex;
								if (palette.ContainsKey(color)) {
									cIndex = palette[color];
								} else {
									cIndex = palette.Count;
									palette.Add(color, cIndex);
									data.Palette.Add(color);
								}
								if (leftHanded) {
									voxels[x, y, m.SizeZ - z - 1] = cIndex + 1;
								} else {
									voxels[x, y, z] = cIndex + 1;
								}
							}
						}
					}
				}

				data.Voxels.Add(voxels);
				data.Groups[1].ChildNodeId[index] = index * 2 + 2;
				data.Transforms.Add(index * 2 + 2, new VoxelData.TransformData() {
					ChildID = index * 2 + 3,
					Hidden = false,
					LayerID = 0,
					Reserved = 0,
					Name = "",
					Frames = new VoxelData.TransformData.FrameData[1] {new VoxelData.TransformData.FrameData() {
						Position = leftHanded ?
							new Vector3(m.PosX + m.SizeX / 2, m.PosY + m.SizeY / 2, -(m.PosZ + Mathf.CeilToInt(m.SizeZ / 2f))) :
							new Vector3(m.PosX + m.SizeX / 2, m.PosY + m.SizeY / 2, m.PosZ + m.SizeZ / 2),
						Rotation = Vector3.zero,
						Scale = Vector3.one,
					}},
				});
				data.Shapes.Add(index * 2 + 3, new VoxelData.ShapeData() {
					Attributes = new Dictionary<string, string>(),
					ModelData = new KeyValuePair<int, Dictionary<string, string>>[1] {
						new KeyValuePair<int, Dictionary<string, string>>(index, new Dictionary<string, string>())
					},
				});
			}

			return data;
		}



		private Color GetColor (int color) {
			if (ColorFormat == 0) {
				int r = 0xFF & color;
				int g = 0xFF00 & color;
				g >>= 8;
				int b = 0xFF0000 & color;
				b >>= 16;
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			} else {
				int b = 0xFF & color;
				int g = 0xFF00 & color;
				g >>= 8;
				int r = 0xFF0000 & color;
				r >>= 16;
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			}
		}



	}




	[System.Serializable]
	public class VoxelJsonData {


		[System.Serializable]
		public class IntArray3 {
			public int[] Value;
			public int SizeX;
			public int SizeY;
			public int SizeZ;

			public IntArray3 (int sizeX, int sizeY, int sizeZ) {
				SizeX = sizeX;
				SizeY = sizeY;
				SizeZ = sizeZ;
				Value = new int[sizeX * sizeY * sizeZ];
			}

			public void Set (int x, int y, int z, int value) {
				Value[z * SizeX * SizeY + y * SizeX + x] = value;
			}

			public int Get (int x, int y, int z) {
				return Value[z * SizeX * SizeY + y * SizeX + x];
			}

		}


		[System.Serializable]
		public class SerGroupData {
			public int Index;
			public string[] AttKeys;
			public string[] Attributes;
			public int[] ChildNodeId;
		}


		[System.Serializable]
		public class SerShapeData {

			[System.Serializable]
			public class ModelMap {
				public string[] Key;
				public string[] Value;
			}

			public int Index;
			public string[] AttKeys;
			public string[] Attributes;
			public int[] ModelDataIndexs;
			public ModelMap[] ModelDatas;
		}


		public int Version;
		public IntArray3[] Voxels;
		public Color[] Palette;
		public VoxelData.MaterialData[] Materials;
		public int[] TransformIndexs;
		public VoxelData.TransformData[] Transforms;
		public SerGroupData[] Groups;
		public SerShapeData[] Shapes;
		public int[] RigIndexs;
		public VoxelData.RigData[] Rigs;




		public VoxelJsonData (VoxelData source) {

			// Version
			Version = source.Version;

			// Voxels
			Voxels = new IntArray3[source.Voxels.Count];
			for (int i = 0; i < Voxels.Length; i++) {
				int sizeX = source.Voxels[i].GetLength(0);
				int sizeY = source.Voxels[i].GetLength(1);
				int sizeZ = source.Voxels[i].GetLength(2);
				var voxels = new IntArray3(sizeX, sizeY, sizeZ);
				var sourceVoxels = source.Voxels[i];
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						for (int z = 0; z < sizeZ; z++) {
							voxels.Set(x, y, z, sourceVoxels[x, y, z]);
						}
					}
				}
				Voxels[i] = voxels;
			}

			// Palette
			Palette = source.Palette.ToArray();

			// Materials
			Materials = source.Materials.ToArray();

			// Transforms
			var tfMap = source.Transforms;
			TransformIndexs = new int[tfMap.Count];
			Transforms = new VoxelData.TransformData[tfMap.Count];
			int index = 0;
			foreach (var tf in tfMap) {
				TransformIndexs[index] = tf.Key;
				Transforms[index] = tf.Value;
				index++;
			}

			// Groups
			var gpMap = source.Groups;
			Groups = new SerGroupData[gpMap.Count];
			index = 0;
			foreach (var gp in gpMap) {
				Groups[index] = new SerGroupData() {
					Index = gp.Key,
					ChildNodeId = gp.Value.ChildNodeId,
					AttKeys = new string[gp.Value.Attributes.Count],
					Attributes = new string[gp.Value.Attributes.Count],
				};
				var attMap = gp.Value.Attributes;
				int i = 0;
				foreach (var att in attMap) {
					Groups[index].AttKeys[i] = att.Key;
					Groups[index].Attributes[i] = att.Value;
					i++;
				}
				index++;
			}

			// Shapes
			var shMap = source.Shapes;
			Shapes = new SerShapeData[shMap.Count];
			index = 0;
			foreach (var sh in shMap) {
				Shapes[index] = new SerShapeData {
					Index = sh.Key,
					ModelDataIndexs = new int[sh.Value.ModelData.Length],
					ModelDatas = new SerShapeData.ModelMap[sh.Value.ModelData.Length],
					AttKeys = new string[sh.Value.Attributes.Count],
					Attributes = new string[sh.Value.Attributes.Count],
				};
				var attMap = sh.Value.Attributes;
				int i = 0;
				foreach (var att in attMap) {
					Shapes[index].AttKeys[i] = att.Key;
					Shapes[index].Attributes[i] = att.Value;
					i++;
				}
				for (i = 0; i < sh.Value.ModelData.Length; i++) {
					Shapes[index].ModelDataIndexs[i] = sh.Value.ModelData[i].Key;
					Shapes[index].ModelDatas[i] = new SerShapeData.ModelMap() {
						Key = new string[sh.Value.ModelData[i].Value.Count],
						Value = new string[sh.Value.ModelData[i].Value.Count],
					};
					int j = 0;
					var map = sh.Value.ModelData[i].Value;
					foreach (var aj in map) {
						Shapes[index].ModelDatas[i].Key[j] = aj.Key;
						Shapes[index].ModelDatas[i].Value[j] = aj.Value;
						j++;
					}
				}
				index++;
			}

			// Rigs
			var rigMap = source.Rigs;
			RigIndexs = new int[rigMap.Count];
			Rigs = new VoxelData.RigData[rigMap.Count];
			index = 0;
			foreach (var rig in rigMap) {
				RigIndexs[index] = rig.Key;
				Rigs[index] = rig.Value;
				var bones = Rigs[index].Bones;
				for (int i = 0; i < bones.Count; i++) {
					bones[i].ParentIndex = bones.IndexOf(bones[i].Parent);
				}
				index++;
			}


		}



		public VoxelData GetVoxelData () {

			var data = new VoxelData() {
				Version = Version,
				Voxels = new List<int[,,]>(),
				Palette = new List<Color>(Palette),
				Materials = new List<VoxelData.MaterialData>(Materials),
				Transforms = new Dictionary<int, VoxelData.TransformData>(),
				Groups = new Dictionary<int, VoxelData.GroupData>(),
				Shapes = new Dictionary<int, VoxelData.ShapeData>(),
				Rigs = new Dictionary<int, VoxelData.RigData>(),
			};

			// Voxels
			for (int i = 0; i < Voxels.Length; i++) {
				var sourceV = Voxels[i];
				int[,,] vs = new int[sourceV.SizeX, sourceV.SizeY, sourceV.SizeZ];
				for (int x = 0; x < sourceV.SizeX; x++) {
					for (int y = 0; y < sourceV.SizeY; y++) {
						for (int z = 0; z < sourceV.SizeZ; z++) {
							vs[x, y, z] = sourceV.Get(x, y, z);
						}
					}
				}
				data.Voxels.Add(vs);
			}

			// Transforms
			for (int i = 0; i < Transforms.Length; i++) {
				data.Transforms.Add(TransformIndexs[i], Transforms[i]);
			}

			// Groups
			for (int i = 0; i < Groups.Length; i++) {
				var groupData = new VoxelData.GroupData() {
					ChildNodeId = Groups[i].ChildNodeId,
					Attributes = new Dictionary<string, string>(),
				};
				for (int j = 0; j < Groups[i].AttKeys.Length; j++) {
					groupData.Attributes.Add(Groups[i].AttKeys[j], Groups[i].Attributes[j]);
				}
				data.Groups.Add(Groups[i].Index, groupData);
			}

			// Shapes
			for (int i = 0; i < Shapes.Length; i++) {
				var sourceShape = Shapes[i];
				var shapeData = new VoxelData.ShapeData() {
					Attributes = new Dictionary<string, string>(),
					ModelData = new KeyValuePair<int, Dictionary<string, string>>[sourceShape.ModelDatas.Length],
				};
				for (int j = 0; j < sourceShape.Attributes.Length; j++) {
					shapeData.Attributes.Add(sourceShape.AttKeys[j], sourceShape.Attributes[j]);
				}
				for (int j = 0; j < sourceShape.ModelDatas.Length; j++) {
					shapeData.ModelData[j] = new KeyValuePair<int, Dictionary<string, string>>(
						sourceShape.ModelDataIndexs[j],
						new Dictionary<string, string>()
					);
					var sourceData = sourceShape.ModelDatas[j];
					for (int k = 0; k < sourceData.Value.Length; k++) {
						shapeData.ModelData[j].Value.Add(sourceData.Key[k], sourceData.Value[k]);
					}
				}
				data.Shapes.Add(sourceShape.Index, shapeData);
			}

			// Rig
			for (int i = 0; i < Rigs.Length; i++) {
				var bones = Rigs[i].Bones;
				for (int j = 0; j < bones.Count; j++) {
					bones[j].Parent = null;
					int pIndex = bones[j].ParentIndex;
					if (pIndex >= 0 && pIndex < bones.Count) {
						bones[j].Parent = bones[pIndex];
					}
				}
				data.Rigs.Add(RigIndexs[i], Rigs[i]);
			}

			return data;
		}


	}



}