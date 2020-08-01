namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class Core_CharacterGeneration {




		#region --- SUB ---



		public const int ORGAN_TYPE_LENGTH = 16;
		public enum OrganType {
			Head = 0,
			Neck = 1,
			Body = 2,
			Hip = 3,
			LegU_L = 4,
			LegD_L = 5,
			Foot_L = 6,
			ArmU_L = 7,
			ArmD_L = 8,
			Hand_L = 9,
			LegU_R = 10,
			LegD_R = 11,
			Foot_R = 12,
			ArmU_R = 13,
			ArmD_R = 14,
			Hand_R = 15,
		}




		public enum PlacementType {
			Position = 0,
			Stretch = 1,
			Repeat = 2,
		}




		[System.Serializable]
		public class OrganData {

			public int X;
			public int Y;
			public int Z;
			public int SizeX;
			public int SizeY;
			public int SizeZ;
			public bool Mirror = true;
			public bool Visible = true;
			public Color SkinColor = new Color(231f / 255f, 181f / 255f, 145f / 255f);


			public OrganData (int sizeX, int sizeY, int sizeZ, int x, int y, int z) {
				X = x;
				Y = y;
				Z = z;
				SizeX = sizeX;
				SizeY = sizeY;
				SizeZ = sizeZ;
			}

			public void CopyFrom (OrganData other, bool mirror = false) {
				X = mirror ? -other.X : other.X;
				Y = other.Y;
				Z = other.Z;
				SizeX = other.SizeX;
				SizeY = other.SizeY;
				SizeZ = other.SizeZ;
				SkinColor = other.SkinColor;
				Mirror = other.Mirror;
			}

			public static implicit operator bool (OrganData organ) {
				return organ != null;
			}
		}




		[System.Serializable]
		public class AttachmentData {



			// VAR
			public int SizeZ {
				get {
					return Voxels.Length / (SizeX * SizeY);
				}
			}


			public string Name;
			public bool Visible = true;
			public int SizeX;
			public int SizeY;
			public int[] Voxels;
			public int TargetMask = 0;
			public PlacementType PlacementType = PlacementType.Stretch;
			// Position
			public int PositionX;
			public int PositionY;
			public int PositionZ;
			public Vector3 Anchor;
			// Stretch
			public int BorderMinX;
			public int BorderMinY;
			public int BorderMinZ;
			public int BorderMaxX;
			public int BorderMaxY;
			public int BorderMaxZ;
			public int OffsetMinX;
			public int OffsetMinY;
			public int OffsetMinZ;
			public int OffsetMaxX;
			public int OffsetMaxY;
			public int OffsetMaxZ;



			// API
			public void LoadFrom (VoxelData data, int index) {
				if (!data || data.Voxels == null || index < 0 || index >= data.Voxels.Count) { return; }
				int[,,] SourceVoxels = data.Voxels[index];
				int sizeX = SourceVoxels.GetLength(0);
				int sizeY = SourceVoxels.GetLength(1);
				int sizeZ = SourceVoxels.GetLength(2);
				SizeX = sizeX;
				SizeY = sizeY;
				Voxels = new int[sizeX * sizeY * sizeZ];
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						for (int z = 0; z < sizeZ; z++) {
							var vIndex = SourceVoxels[x, y, z];
							if (vIndex > 0) {
								var color = data.GetColorFromPalette(vIndex);
								var cInt = Color2Int(color);
								SetVoxelAt(cInt, x, y, z);
							} else {
								var cInt = Color2Int(Color.clear);
								SetVoxelAt(cInt, x, y, z);
							}
						}
					}
				}
			}



			public VoxelData GetVoxelData () {
				if (Voxels == null || SizeX == 0 || SizeY == 0 || SizeZ == 0) { return null; }
				var data = VoxelData.CreateNewData();
				data.Voxels = new List<int[,,]>() { new int[SizeX, SizeY, SizeZ], };
				int sizeZ = SizeZ;

				// Palette
				Dictionary<int, int> indexMap = new Dictionary<int, int>() { { 0, 0 } };
				var palette = new List<Color>() { };
				for (int i = 0; i < Voxels.Length; i++) {
					var c = Int2Color(Voxels[i]);
					if (!palette.Contains(c)) {
						palette.Add(c);
						if (!indexMap.ContainsKey(Voxels[i])) {
							indexMap.Add(Voxels[i], palette.Count);
						}
					}
				}
				data.Palette = palette;

				// Voxels
				for (int x = 0; x < SizeX; x++) {
					for (int y = 0; y < SizeY; y++) {
						for (int z = 0; z < sizeZ; z++) {
							int vIndex = GetVoxelAt(x, y, z);
							data.Voxels[0][x, y, z] = indexMap.ContainsKey(vIndex) ? indexMap[vIndex] : 0;
						}
					}
				}
				return data;
			}



			public AttachmentData GetCopy () {
				int[] voxels = null;
				if (Voxels != null) {
					voxels = new int[Voxels.Length];
					Voxels.CopyTo(voxels, 0);
				}
				return new AttachmentData() {
					Name = Name,
					Visible = Visible,
					SizeX = SizeX,
					SizeY = SizeY,
					Voxels = voxels,
					TargetMask = TargetMask,
					PlacementType = PlacementType,
					PositionX = PositionX,
					PositionY = PositionY,
					PositionZ = PositionZ,
					Anchor = Anchor,
					BorderMinX = BorderMinX,
					BorderMinY = BorderMinY,
					BorderMinZ = BorderMinZ,
					BorderMaxX = BorderMaxX,
					BorderMaxY = BorderMaxY,
					BorderMaxZ = BorderMaxZ,
					OffsetMinX = OffsetMinX,
					OffsetMinY = OffsetMinY,
					OffsetMinZ = OffsetMinZ,
					OffsetMaxX = OffsetMaxX,
					OffsetMaxY = OffsetMaxY,
					OffsetMaxZ = OffsetMaxZ,
				};
			}



			public bool CheckTargetOrgan (int targetIndex) {
				return Util.GetBit(TargetMask, targetIndex);
			}




			public OrganType[] GetTargetsFromMaskValue () {
				var targets = new List<OrganType>();
				for (int i = 0; i < ORGAN_TYPE_LENGTH; i++) {
					if (Util.GetBit(TargetMask, i)) {
						targets.Add((OrganType)i);
					}
				}
				return targets.ToArray();
			}


			// LGC
			public static int Color2Int (Color color) {
				return (
					((int)(color.a * 255) << 24) |
					((int)(color.b * 255) << 16) |
					((int)(color.g * 255) << 8) |
					((int)(color.r * 255) << 0)
				);
			}



			public static Color Int2Color (int color) {
				int r = 0xFF & color;
				int g = 0xFF00 & color;
				g >>= 8;
				int b = 0xFF0000 & color;
				b >>= 16;
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			}



			public int GetVoxelAt (int x, int y, int z) {
				return Voxels[z * SizeY * SizeX + y * SizeX + x];
			}



			public void SetVoxelAt (int value, int x, int y, int z) {
				Voxels[z * SizeY * SizeX + y * SizeX + x] = value;
			}



		}




		public class OrganTransformInfo {
			public OrganType Organ;
			public int PositionX;
			public int PositionY;
			public int PositionZ;
			public OrganTransformInfo (OrganType organ) {
				Organ = organ;
			}
			public static implicit operator bool (OrganTransformInfo info) {
				return info != null;
			}
		}




		[System.Serializable]
		public class Preset : ISerializationCallbackReceiver {



			// VAR
			public OrganData Head { get { return TryGetData(OrganType.Head); } }
			public OrganData Neck { get { return TryGetData(OrganType.Neck); } }
			public OrganData Body { get { return TryGetData(OrganType.Body); } }
			public OrganData Hip { get { return TryGetData(OrganType.Hip); } }
			public OrganData ArmU_L { get { return TryGetData(OrganType.ArmU_L); } }
			public OrganData ArmU_R { get { return TryGetData(OrganType.ArmU_R); } }
			public OrganData ArmD_L { get { return TryGetData(OrganType.ArmD_L); } }
			public OrganData ArmD_R { get { return TryGetData(OrganType.ArmD_R); } }
			public OrganData Hand_L { get { return TryGetData(OrganType.Hand_L); } }
			public OrganData Hand_R { get { return TryGetData(OrganType.Hand_R); } }
			public OrganData LegU_L { get { return TryGetData(OrganType.LegU_L); } }
			public OrganData LegU_R { get { return TryGetData(OrganType.LegU_R); } }
			public OrganData LegD_L { get { return TryGetData(OrganType.LegD_L); } }
			public OrganData LegD_R { get { return TryGetData(OrganType.LegD_R); } }
			public OrganData Foot_L { get { return TryGetData(OrganType.Foot_L); } }
			public OrganData Foot_R { get { return TryGetData(OrganType.Foot_R); } }



			public Dictionary<OrganType, OrganData> OrganMap = GetNewOrganMap();
			public List<AttachmentData> Attachments = new List<AttachmentData>();
			[SerializeField] private OrganData[] _OrganList;





			// MSG
			public void OnBeforeSerialize () {
				_OrganList = new OrganData[System.Enum.GetNames(typeof(OrganType)).Length];
				for (int i = 0; i < _OrganList.Length; i++) {
					if (OrganMap.ContainsKey((OrganType)i)) {
						_OrganList[i] = OrganMap[(OrganType)i];
					}
				}
			}



			public void OnAfterDeserialize () {
				OrganMap = new Dictionary<OrganType, OrganData>();
				if (_OrganList != null) {
					for (int i = 0; i < _OrganList.Length; i++) {
						OrganMap.Add((OrganType)i, _OrganList[i]);
					}
				}
			}




			// API
			public void LoadFromJson (string json) {
				try {
					if (string.IsNullOrEmpty(json)) { return; }
					JsonUtility.FromJsonOverwrite(json, this);
				} catch { }
			}



			public string ToJson () {
				return JsonUtility.ToJson(this, true);
			}



			public void FixGenerationValues () {
				ForAllOrgans((type, data) => {
					if (data) {
						data.SizeX = Mathf.Max(data.SizeX, 0);
						data.SizeY = Mathf.Max(data.SizeY, 0);
						data.SizeZ = Mathf.Max(data.SizeZ, 0);
						data.SkinColor.a = 1f;
					}
				});
				int len = System.Enum.GetNames(typeof(OrganType)).Length;
				if (OrganMap == null) {
					OrganMap = GetNewOrganMap();
				} else if (OrganMap.Count < len) {
					Debug.LogWarning("[Voxel Character Generator] Lack of map data. " + OrganMap.Count + "/" + len);
					var defaultMap = GetNewOrganMap();
					foreach (var defaultPair in defaultMap) {
						if (!OrganMap.ContainsKey(defaultPair.Key)) {
							OrganMap.Add(defaultPair.Key, defaultPair.Value);
						}
					}
				}
				if (Attachments != null) {
					for (int i = 0; i < Attachments.Count; i++) {
						var att = Attachments[i];
						att.Anchor.x = Mathf.Clamp01(att.Anchor.x);
						att.Anchor.y = Mathf.Clamp01(att.Anchor.y);
						att.Anchor.z = Mathf.Clamp01(att.Anchor.z);
						if (att.Voxels != null && att.Voxels.Length > 0) {
							att.BorderMaxX = Mathf.Clamp(att.BorderMaxX, 0, att.SizeX);
							att.BorderMaxY = Mathf.Clamp(att.BorderMaxY, 0, att.SizeY);
							att.BorderMaxZ = Mathf.Clamp(att.BorderMaxZ, 0, att.SizeZ);
							att.BorderMinX = Mathf.Clamp(att.BorderMinX, 0, att.SizeX - att.BorderMaxX);
							att.BorderMinY = Mathf.Clamp(att.BorderMinY, 0, att.SizeY - att.BorderMaxY);
							att.BorderMinZ = Mathf.Clamp(att.BorderMinZ, 0, att.SizeZ - att.BorderMaxZ);
						}
					}
				}
			}



			public void ForAllOrgans (System.Action<OrganType, OrganData> action) {
				foreach (var pair in OrganMap) {
					action(pair.Key, pair.Value);
				}
			}



			public OrganType? GetParent (OrganType organ) {
				switch (organ) {
					default:
					case OrganType.Hip:
						return null;
					case OrganType.Head:
						return OrganType.Neck;
					case OrganType.Neck:
						return OrganType.Body;
					case OrganType.Body:
					case OrganType.LegU_L:
					case OrganType.LegU_R:
						return OrganType.Hip;
					case OrganType.ArmU_L:
					case OrganType.ArmU_R:
						return OrganType.Body;
					case OrganType.ArmD_L:
						return OrganType.ArmU_L;
					case OrganType.ArmD_R:
						return OrganType.ArmU_R;
					case OrganType.LegD_L:
						return OrganType.LegU_L;
					case OrganType.LegD_R:
						return OrganType.LegU_R;
					case OrganType.Hand_L:
						return OrganType.ArmD_L;
					case OrganType.Hand_R:
						return OrganType.ArmD_R;
					case OrganType.Foot_L:
						return OrganType.LegD_L;
					case OrganType.Foot_R:
						return OrganType.LegD_R;
				}
			}



			public OrganTransformInfo GetTransformInfo (OrganType organ) {

				if (!OrganMap.ContainsKey(organ)) { return null; }
				OrganData data = OrganMap[organ];

				// Hip
				if (organ == OrganType.Hip) {
					return new OrganTransformInfo(organ) {
						PositionX = data.X,
						PositionY = data.Y,
						PositionZ = data.Z
					};
				}

				var _parent = GetParent(organ);
				if (!_parent.HasValue) { return null; }
				var parent = _parent.Value;
				var pData = OrganMap[parent];

				// Other
				var pInfo = GetTransformInfo(parent);
				OrganTransformInfo info = null;
				if (pInfo) {
					info = new OrganTransformInfo(organ) {
						PositionX = pInfo.PositionX + data.X,
						PositionY = pInfo.PositionY + data.Y,
						PositionZ = pInfo.PositionZ + data.Z,
					};

					switch (organ) {
						case OrganType.Head:
						case OrganType.Neck:
						case OrganType.Body:
							info.PositionY += GetHalfSize(pData.SizeY, true) + GetHalfSize(data.SizeY, false);
							break;
						case OrganType.ArmU_L:
						case OrganType.ArmD_L:
						case OrganType.Hand_L:
							info.PositionX -= GetHalfSize(pData.SizeX, false) + GetHalfSize(data.SizeX, true);
							break;
						case OrganType.ArmU_R:
						case OrganType.ArmD_R:
						case OrganType.Hand_R:
							info.PositionX += GetHalfSize(pData.SizeX, true) + GetHalfSize(data.SizeX, false);
							break;
						default:
							info.PositionY -= GetHalfSize(pData.SizeY, false) + GetHalfSize(data.SizeY, true);
							break;
					}
				}
				return info;
			}




			// LGC
			private static Dictionary<OrganType, OrganData> GetNewOrganMap () {
				return new Dictionary<OrganType, OrganData>() {
					{OrganType.Head, new OrganData(7, 7, 7, 0, 0, 0)},
					{OrganType.Neck, new OrganData(3, 1, 3, 0, 0, 0)},
					{OrganType.Body, new OrganData (5, 4, 3, 0, 0, 0)},
					{OrganType.Hip, new OrganData (5, 1, 3, 0, 0, 0)},
					{OrganType.LegU_L, new OrganData(1, 3, 1, -1, 0, 0) },
					{OrganType.LegU_R, new OrganData(1, 3, 1, 1, 0, 0) },
					{OrganType.LegD_L, new OrganData(1, 2, 1, 0, 0, 0)},
					{OrganType.LegD_R, new OrganData(1, 2, 1, 0, 0, 0)},
					{OrganType.Foot_L, new OrganData(1, 1, 1, 0, 0, 0)},
					{OrganType.Foot_R, new OrganData(1, 1, 1, 0, 0, 0)},
					{OrganType.ArmU_L, new OrganData(1, 3, 1, 0, 2, 0)},
					{OrganType.ArmU_R, new OrganData(1, 3, 1, 0, 2, 0)},
					{OrganType.ArmD_L, new OrganData(1, 2, 1, 0, 0, 0)},
					{OrganType.ArmD_R, new OrganData(1, 2, 1, 0, 0, 0)},
					{OrganType.Hand_L, new OrganData(1, 1, 1, 0, 0, 0)},
					{OrganType.Hand_R, new OrganData(1, 1, 1, 0, 0, 0)},
				};
			}



			private OrganData TryGetData (OrganType organ) {
				OrganData result;
				if (OrganMap.TryGetValue(organ, out result)) {
					return result;
				}
				return null;
			}



			private int GetHalfSize (int size, bool positive) {
				return positive ? size / 2 + 1 : (size - 1) / 2;
			}



		}




		#endregion




		public static VoxelData Generate (Preset preset, System.Action<float, int> onProgress = null) {
			try {
				if (preset == null) { return VoxelData.CreateNewData(); }

				int minX, minY, minZ, maxX, maxY, maxZ;

				var infoMap = Generate_GetMinMax(preset, out minX, out minY, out minZ, out maxX, out maxY, out maxZ);
				var data = Generate_FillVoxels(preset, infoMap, onProgress, minX, minY, minZ, maxX, maxY, maxZ);

				if (onProgress != null) {
					onProgress(1f, int.MaxValue);
				}
				return data;
			} catch (System.Exception ex) {
				if (onProgress != null) {
					onProgress(1f, int.MaxValue);
				}
				throw ex;
			}
		}



		// LGC
		private static Dictionary<OrganType, OrganTransformInfo> Generate_GetMinMax (
			Preset preset,
			out int minX, out int minY, out int minZ, out int maxX, out int maxY, out int maxZ
		) {

			int _minX = int.MaxValue;
			int _minY = int.MaxValue;
			int _minZ = int.MaxValue;
			int _maxX = int.MinValue;
			int _maxY = int.MinValue;
			int _maxZ = int.MinValue;

			// Body
			var infoMap = new Dictionary<OrganType, OrganTransformInfo>();
			preset.ForAllOrgans((organ, oData) => {
				var info = preset.GetTransformInfo(organ);
				if (info) {
					_minX = Mathf.Min(_minX, info.PositionX - GetOrganHalfSize(oData.SizeX, false));
					_minY = Mathf.Min(_minY, info.PositionY - GetOrganHalfSize(oData.SizeY, false));
					_minZ = Mathf.Min(_minZ, info.PositionZ - GetOrganHalfSize(oData.SizeZ, false));
					_maxX = Mathf.Max(_maxX, info.PositionX + GetOrganHalfSize(oData.SizeX, true));
					_maxY = Mathf.Max(_maxY, info.PositionY + GetOrganHalfSize(oData.SizeY, true));
					_maxZ = Mathf.Max(_maxZ, info.PositionZ + GetOrganHalfSize(oData.SizeZ, true));
					infoMap.Add(organ, info);
				}
			});

			minX = _minX;
			minY = _minY;
			minZ = _minZ;
			maxX = _maxX;
			maxY = _maxY;
			maxZ = _maxZ;

			// Attachment
			if (preset.Attachments != null) {
				for (int i = 0; i < preset.Attachments.Count; i++) {

					var att = preset.Attachments[i];
					if (att == null || !att.Visible || att.Voxels == null || att.Voxels.Length == 0 || att.TargetMask == 0) { continue; }

					var targets = att.GetTargetsFromMaskValue();
					for (int tIndex = 0; tIndex < targets.Length; tIndex++) {
						var organ = targets[tIndex];

						if (!infoMap.ContainsKey(organ) || !preset.OrganMap.ContainsKey(organ)) { continue; }

						var info = infoMap[organ];
						var oData = preset.OrganMap[organ];
						if (!info || !oData) { continue; }

						switch (att.PlacementType) {
							default:
							case PlacementType.Position:
								int attPosX = info.PositionX + att.PositionX + GetAttachmentAnchorPositionFix(att.Anchor.x, oData.SizeX);
								int attPosY = info.PositionY + att.PositionY + GetAttachmentAnchorPositionFix(att.Anchor.y, oData.SizeY);
								int attPosZ = info.PositionZ + att.PositionZ + GetAttachmentAnchorPositionFix(att.Anchor.z, oData.SizeZ);
								minX = Mathf.Min(minX, attPosX - GetOrganHalfSize(att.SizeX, false));
								minY = Mathf.Min(minY, attPosY - GetOrganHalfSize(att.SizeY, false));
								minZ = Mathf.Min(minZ, attPosZ - GetOrganHalfSize(att.SizeZ, false));
								maxX = Mathf.Max(maxX, attPosX + GetOrganHalfSize(att.SizeX, true));
								maxY = Mathf.Max(maxY, attPosY + GetOrganHalfSize(att.SizeY, true));
								maxZ = Mathf.Max(maxZ, attPosZ + GetOrganHalfSize(att.SizeZ, true));
								break;
							case PlacementType.Stretch:
							case PlacementType.Repeat:
								minX = Mathf.Min(minX, info.PositionX - att.OffsetMinX - GetOrganHalfSize(oData.SizeX, false));
								minY = Mathf.Min(minY, info.PositionY - att.OffsetMinY - GetOrganHalfSize(oData.SizeY, false));
								minZ = Mathf.Min(minZ, info.PositionZ - att.OffsetMinZ - GetOrganHalfSize(oData.SizeZ, false));
								maxX = Mathf.Max(maxX, info.PositionX + att.OffsetMaxX + GetOrganHalfSize(oData.SizeX, true));
								maxY = Mathf.Max(maxY, info.PositionY + att.OffsetMaxY + GetOrganHalfSize(oData.SizeY, true));
								maxZ = Mathf.Max(maxZ, info.PositionZ + att.OffsetMaxZ + GetOrganHalfSize(oData.SizeZ, true));
								break;
						}
					}
				}
			}
			return infoMap;

		}



		private static VoxelData Generate_FillVoxels (
			Preset preset,
			Dictionary<OrganType, OrganTransformInfo> infoMap,
			System.Action<float, int> onProgress,
			int minX, int minY, int minZ, int maxX, int maxY, int maxZ
		) {

			VoxelData data = VoxelData.CreateNewData();

			if (preset == null || preset.OrganMap == null) { return data; }

			int sizeX = maxX - minX + 1;
			int sizeY = maxY - minY + 1;
			int sizeZ = maxZ - minZ + 1;

			// --- Rig
			List<VoxelData.RigData.Bone> boneList;
			var boneMap = GetBoneMap(preset, infoMap, out boneList, minX, minY, minZ, maxX, maxY, maxZ);
			var weights = new int[sizeX, sizeY, sizeZ];
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					for (int z = 0; z < sizeZ; z++) {
						weights[x, y, z] = -1;
					}
				}
			}
			int chestIndex = -1;
			for (int i = 0; i < boneList.Count; i++) {
				var b = boneList[i];
				if (b && b.Name == "Chest") {
					chestIndex = i;
					break;
				}
			}

			// --- Body
			var voxels = new int[sizeX, sizeY, sizeZ];
			var palette = new List<Color>();

			int index = 0;
			foreach (var info in infoMap) {

				if (!preset.OrganMap.ContainsKey(info.Key)) { continue; }

				var oData = preset.OrganMap[info.Key];
				if (!oData) { continue; }

				int vIndex = palette.Count + 1;
				palette.Add(oData.SkinColor);

				if (!oData.Visible) { continue; }


				int boneIndexU = -1;
				int boneIndexD = -1;
				if (boneMap.ContainsKey(info.Key)) {
					boneIndexU = boneIndexD = boneList.IndexOf(boneMap[info.Key]);
					if (info.Key == OrganType.Body && chestIndex >= 0) {
						boneIndexU = chestIndex;
					}
				}

				var tf = info.Value;
				int l = Mathf.Clamp(tf.PositionX - GetOrganHalfSize(oData.SizeX, false) - minX, 0, sizeX - 1);
				int d = Mathf.Clamp(tf.PositionY - GetOrganHalfSize(oData.SizeY, false) - minY, 0, sizeY - 1);
				int b = Mathf.Clamp(tf.PositionZ - GetOrganHalfSize(oData.SizeZ, false) - minZ, 0, sizeZ - 1);
				int r = Mathf.Clamp(tf.PositionX + GetOrganHalfSize(oData.SizeX, true) - minX, 0, sizeX - 1);
				int u = Mathf.Clamp(tf.PositionY + GetOrganHalfSize(oData.SizeY, true) - minY, 0, sizeY - 1);
				int f = Mathf.Clamp(tf.PositionZ + GetOrganHalfSize(oData.SizeZ, true) - minZ, 0, sizeZ - 1);
				for (int x = l; x < r; x++) {
					for (int y = d; y < u; y++) {
						int bIndex = y > (u + d) * 0.5f ? boneIndexU : boneIndexD;
						for (int z = b; z < f; z++) {
							voxels[x, y, z] = vIndex;
							weights[x, y, z] = bIndex;
						}
					}
				}

				// Progress
				if (onProgress != null) {
					onProgress(index / (infoMap.Count - 1f), 0);
				}
				index++;
			}


			// --- Attachment
			if (preset.Attachments != null) {
				for (int i = 0; i < preset.Attachments.Count; i++) {
					var att = preset.Attachments[i];
					if (att == null || !att.Visible || att.TargetMask == 0) { continue; }

					var targets = att.GetTargetsFromMaskValue();
					for (int tIndex = 0; tIndex < targets.Length; tIndex++) {
						var target = targets[tIndex];
						if (!infoMap.ContainsKey(target) || !preset.OrganMap.ContainsKey(target)) { continue; }

						var info = infoMap[target];
						var oData = preset.OrganMap[target];
						var voxs = att.Voxels;

						if (!oData || voxs == null || voxs.Length == 0 || oData.SizeX == 0 || oData.SizeY == 0 || oData.SizeZ == 0) { continue; }

						int boneIndexU = -1;
						int boneIndexD = -1;
						if (boneMap.ContainsKey(target)) {
							boneIndexU = boneIndexD = boneList.IndexOf(boneMap[target]);
							if (target == OrganType.Body && chestIndex >= 0) {
								boneIndexU = chestIndex;
							}
						}

						switch (att.PlacementType) {
							default:
							case PlacementType.Position:

								// Pos Offset
								int posX = info.PositionX + att.PositionX + GetAttachmentAnchorPositionFix(att.Anchor.x, oData.SizeX) - GetOrganHalfSize(att.SizeX, false);
								int posY = info.PositionY + att.PositionY + GetAttachmentAnchorPositionFix(att.Anchor.y, oData.SizeY) - GetOrganHalfSize(att.SizeY, false);
								int posZ = info.PositionZ + att.PositionZ + GetAttachmentAnchorPositionFix(att.Anchor.z, oData.SizeZ) - GetOrganHalfSize(att.SizeZ, false);

								posX = Mathf.Clamp(posX - minX, 0, sizeX - att.SizeX);
								posY = Mathf.Clamp(posY - minY, 0, sizeY - att.SizeY);
								posZ = Mathf.Clamp(posZ - minZ, 0, sizeZ - att.SizeZ);

								// Fill
								int attSizeZ = att.SizeZ;
								for (int x = 0; x < att.SizeX; x++) {
									for (int y = 0; y < att.SizeY; y++) {
										int bIndex = y > att.SizeY * 0.5f ? boneIndexU : boneIndexD;
										for (int z = 0; z < attSizeZ; z++) {

											var v = att.GetVoxelAt(x, y, z);
											if (v == 0) { continue; }

											// Palette
											var c = AttachmentData.Int2Color(v);
											int vIndex = palette.IndexOf(c);
											if (vIndex < 0) {
												palette.Add(c);
												vIndex = palette.Count - 1;
											}

											// Voxel
											voxels[posX + x, posY + y, posZ + z] = vIndex + 1;
											weights[posX + x, posY + y, posZ + z] = bIndex;

										}
									}
								}
								break;
							case PlacementType.Stretch:
							case PlacementType.Repeat:
								// Pos Offset
								int l = info.PositionX - att.OffsetMinX - GetOrganHalfSize(oData.SizeX, false);
								int d = info.PositionY - att.OffsetMinY - GetOrganHalfSize(oData.SizeY, false);
								int b = info.PositionZ - att.OffsetMinZ - GetOrganHalfSize(oData.SizeZ, false);
								int r = info.PositionX + att.OffsetMaxX + GetOrganHalfSize(oData.SizeX, true);
								int u = info.PositionY + att.OffsetMaxY + GetOrganHalfSize(oData.SizeY, true);
								int f = info.PositionZ + att.OffsetMaxZ + GetOrganHalfSize(oData.SizeZ, true);

								l = Mathf.Clamp(l - minX, 0, sizeX - 1);
								d = Mathf.Clamp(d - minY, 0, sizeY - 1);
								b = Mathf.Clamp(b - minZ, 0, sizeZ - 1);
								r = Mathf.Clamp(r - minX, 0, sizeX - 1);
								u = Mathf.Clamp(u - minY, 0, sizeY - 1);
								f = Mathf.Clamp(f - minZ, 0, sizeZ - 1);

								bool isRepeat = att.PlacementType == PlacementType.Repeat;
								int _attSizeZ = att.SizeZ;
								for (int x = l; x < r; x++) {
									int _x = GetAttachmentStretchIndex(x - l, r - l - 1, att.SizeX, att.BorderMinX, att.BorderMaxX, isRepeat);
									for (int y = d; y < u; y++) {
										int _y = GetAttachmentStretchIndex(y - d, u - d - 1, att.SizeY, att.BorderMinY, att.BorderMaxY, isRepeat);
										int bIndex = y > (u + d) * 0.5f ? boneIndexU : boneIndexD;
										for (int z = b; z < f; z++) {
											int _z = GetAttachmentStretchIndex(z - b, f - b - 1, att.SizeZ, att.BorderMinZ, att.BorderMaxZ, isRepeat);

											if (_x < 0 || _y < 0 | _z < 0) { continue; }

											var v = att.GetVoxelAt(_x, _y, _z);
											if (v == 0) { continue; }

											// Palette
											var c = AttachmentData.Int2Color(v);
											int vIndex = palette.IndexOf(c);
											if (vIndex < 0) {
												palette.Add(c);
												vIndex = palette.Count - 1;
											}

											// Voxel
											voxels[x, y, z] = vIndex + 1;
											weights[x, y, z] = bIndex;

										}
									}
								}

								break;
						}

					}
					// Progress
					if (onProgress != null) {
						onProgress(i / (preset.Attachments.Count - 1f), 1);
					}
				}
			}


			// --- Rig
			var weightList = new List<VoxelData.RigData.Weight>();
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					for (int z = 0; z < sizeZ; z++) {
						var w = weights[x, y, z];
						if (w >= 0) {
							weightList.Add(new VoxelData.RigData.Weight() {
								X = x,
								Y = y,
								Z = z,
								BoneIndexA = w,
								BoneIndexB = -1,
							});
						}
					}
				}
			}
			var rig = new VoxelData.RigData {
				Version = VoxelData.RigData.CURRENT_VERSION,
				Bones = boneList,
				Weights = weightList,
			};

			// End
			data.Voxels = new List<int[,,]>() { voxels };
			data.Palette = palette;
			data.Rigs = new Dictionary<int, VoxelData.RigData>() { { 0, rig } };

			// Progress
			if (onProgress != null) {
				onProgress(1f, int.MaxValue);
			}

			return data;
		}



		private static Dictionary<OrganType, VoxelData.RigData.Bone> GetBoneMap (
			Preset preset,
			Dictionary<OrganType, OrganTransformInfo> infoMap,
			out List<VoxelData.RigData.Bone> boneList,
			int minX, int minY, int minZ, int maxX, int maxY, int maxZ
		) {

			int sizeX = maxX - minX + 1;
			int sizeY = maxY - minY + 1;
			int sizeZ = maxZ - minZ + 1;

			boneList = VoxelData.RigData.GetHumanBones(new Vector3(sizeX, sizeY, sizeZ), false);
			var boneMap = new Dictionary<OrganType, VoxelData.RigData.Bone>();

			for (int i = 0; i < boneList.Count; i++) {

				var bone = boneList[i];
				if (!bone) { continue; }

				bone.ParentIndex = bone.Parent ? boneList.IndexOf(bone.Parent) : -1;
				bone.PositionX = 0;
				bone.PositionY = 0;
				bone.PositionZ = 0;
				OrganData oData, pData;
				OrganType? type = null;

				switch (bone.Name) {

					// Body
					case "Hips":
						type = OrganType.Hip;
						if (!infoMap.ContainsKey(type.Value)) { break; }

						var tf = infoMap[type.Value];
						if (tf == null) { break; }

						bone.PositionX = tf.PositionX - minX;
						bone.PositionY = tf.PositionY - minY;
						bone.PositionZ = tf.PositionZ - minZ;

						break;
					case "Spine":
						type = OrganType.Body;
						oData = preset.Body;
						pData = preset.Hip;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y + pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;
					case "Chest":
						type = null;
						oData = preset.Body;
						pData = preset.Body;
						if (oData && pData) {
							bone.PositionX = 0;
							bone.PositionY = GetOrganHalfSize(pData.SizeY, false);
							bone.PositionZ = 0;
						}
						break;

					// Head
					case "Neck":
						type = OrganType.Neck;
						oData = preset.Neck;
						pData = preset.Body;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y + GetOrganHalfSize(pData.SizeY, true);
							bone.PositionZ = oData.Z;
						}
						break;
					case "Head":
						type = OrganType.Head;
						oData = preset.Head;
						pData = preset.Neck;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y + pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;

					// Arm
					case "LeftUpperArm":
						type = OrganType.ArmU_L;
						oData = preset.ArmU_L;
						pData = preset.Body;
						if (oData && pData) {
							bone.PositionX = oData.X - GetOrganHalfSize(pData.SizeX, false) - 1;
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightUpperArm":
						type = OrganType.ArmU_R;
						oData = preset.ArmU_R;
						pData = preset.Body;
						if (oData && pData) {
							bone.PositionX = oData.X + GetOrganHalfSize(pData.SizeX, true);
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;
					case "LeftLowerArm":
						type = OrganType.ArmD_L;
						oData = preset.ArmD_L;
						pData = preset.ArmU_L;
						if (oData && pData) {
							bone.PositionX = oData.X - pData.SizeX;
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightLowerArm":
						type = OrganType.ArmD_R;
						oData = preset.ArmD_R;
						pData = preset.ArmU_R;
						if (oData && pData) {
							bone.PositionX = oData.X + pData.SizeX;
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;
					case "LeftHand":
						type = OrganType.Hand_L;
						oData = preset.Hand_L;
						pData = preset.ArmD_L;
						if (oData && pData) {
							bone.PositionX = oData.X - pData.SizeX;
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightHand":
						type = OrganType.Hand_R;
						oData = preset.Hand_R;
						pData = preset.ArmD_R;
						if (oData && pData) {
							bone.PositionX = oData.X + pData.SizeX;
							bone.PositionY = oData.Y;
							bone.PositionZ = oData.Z;
						}
						break;

					// Leg
					case "LeftUpperLeg":
						type = OrganType.LegU_L;
						oData = preset.LegU_L;
						pData = preset.Hip;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - 1;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightUpperLeg":
						type = OrganType.LegU_R;
						oData = preset.LegU_R;
						pData = preset.Hip;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - 1;
							bone.PositionZ = oData.Z;
						}
						break;
					case "LeftLowerLeg":
						type = OrganType.LegD_L;
						oData = preset.LegD_L;
						pData = preset.LegU_L;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightLowerLeg":
						type = OrganType.LegD_R;
						oData = preset.LegD_R;
						pData = preset.LegU_R;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;
					case "LeftFoot":
						type = OrganType.Foot_L;
						oData = preset.Foot_L;
						pData = preset.LegD_L;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;
					case "RightFoot":
						type = OrganType.Foot_R;
						oData = preset.Foot_R;
						pData = preset.LegD_R;
						if (oData && pData) {
							bone.PositionX = oData.X;
							bone.PositionY = oData.Y - pData.SizeY;
							bone.PositionZ = oData.Z;
						}
						break;
				}

				bone.PositionX *= 2;
				bone.PositionY *= 2;
				bone.PositionZ *= 2;

				if (type.HasValue) {
					boneMap.Add(type.Value, bone);
				}

			}
			return boneMap;
		}




		// UTL
		private static int GetOrganHalfSize (int size, bool positive) {
			if (size <= 0) { return 0; }
			return positive ? size / 2 + 1 : (size - 1) / 2;
		}



		private static int GetAttachmentStretchIndex (int x, int len, int attLen, int bMin, int bMax, bool isRepeat) {
			if (x < bMin) {
				return x;
			} else if (x > len - bMax) {
				return attLen - len + x - 1;
			} else if (bMin + bMax < attLen) {
				return Mathf.Clamp(
					isRepeat ?
						(int)Mathf.Repeat(x - bMin, attLen - bMax - bMin - 0.001f) + bMin :
						bMin + (int)((((float)x - bMin) * (attLen - bMax - bMin)) / (len - bMax - bMin)),
					bMin,
					attLen - bMax - 1
				);
			} else {
				return -1;
			}
		}



		private static int GetAttachmentAnchorPositionFix (float anchor, int size) {
			int result = anchor < 0.5f ?
				Mathf.FloorToInt((anchor - 0.5f) * size) :
				Mathf.CeilToInt((anchor - 0.5f) * size);
			if (anchor >= 0.5f && size % 2 == 0) {
				result++;
			}
			return result;
		}



	}
}