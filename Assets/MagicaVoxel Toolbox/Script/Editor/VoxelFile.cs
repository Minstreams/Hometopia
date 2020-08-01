namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System.IO;
	using System.Text;


	public static class VoxelFile {



		#region --- SUB ---


		public delegate void OnProgressHandler (float progress);


		#endregion




		#region --- API ---



		public static VoxelData GetVoxelData (byte[] voxelBytes, bool isVox, OnProgressHandler onProgress = null) {
			return isVox ? GetVoxelDataFromVox(voxelBytes, onProgress) : GetVoxelDataFromQb(voxelBytes);
		}



		public static byte[] GetVoxelByte (VoxelData data, bool isVox, OnProgressHandler onProgress = null) {
			return isVox ? GetVoxFromVoxelData(data, onProgress) : GetQbFromVoxelData(data);
		}




		#endregion




		#region --- VOX ---



		#region --- Read ---


		private static VoxelData GetVoxelDataFromVox (byte[] voxBytes, OnProgressHandler onProgress) {

			VoxelData data = new VoxelData();

			if (!CheckID(voxBytes, "VOX ")) {
				Debug.LogError("Error with Magic Number. The file is not a vox file.");
				return null;
			}

			using (MemoryStream ms = new MemoryStream(voxBytes)) {
				using (BinaryReader br = new BinaryReader(ms)) {

					// VOX_
					br.ReadInt32();

					// VERSION
					data.Version = System.BitConverter.ToInt32(br.ReadBytes(4), 0);

					// MAIN
					byte[] chunkId = br.ReadBytes(4);
					int mainChunkSize = br.ReadInt32();
					int mainChildrenSize = br.ReadInt32();
					br.ReadBytes(mainChunkSize);
					if (!CheckID(chunkId, "MAIN")) {
						Debug.LogError("Error with Main Chunk ID");
						return null;
					}

					// Containt
					int readSize = 0;
					Vector3 tempSize = new Vector3();

					while (readSize < mainChildrenSize) {

						string id = GetID(br.ReadBytes(4));
						readSize += 4;

						switch (id) {
							case "PACK":
								readSize += ReadPackChunk(br);
								break;
							case "SIZE":
								readSize += ReadSizeChunk(br, out tempSize);
								break;
							case "XYZI":
								int[,,] tempVoxels = new int[(int)tempSize.x, (int)tempSize.y, (int)tempSize.z];
								readSize += ReadVoxelChunk(br, ref tempVoxels);
								data.Voxels.Add(tempVoxels);
								break;
							case "RGBA":
								readSize += ReadPalattee(br, ref data.Palette);
								break;
							case "nTRN":
								readSize += ReadTransform(br, ref data);
								break;
							case "nGRP":
								readSize += ReadGroup(br, ref data);
								break;
							case "nSHP":
								readSize += ReadShape(br, ref data);
								break;
							case "MATL":
								readSize += ReadMaterial(br, ref data);
								break;
							case "RIGG":
								readSize += ReadRig(br, ref data, 0);
								break;
							case "_RIG":
								readSize += ReadRig(br, ref data, VoxelData.RigData.CURRENT_VERSION);
								break;
							default:
								int chunkSize = br.ReadInt32();
								int childrenSize = br.ReadInt32();
								br.ReadBytes(chunkSize + childrenSize);
								readSize += chunkSize + childrenSize + 4 + 4;
								break;
						}
					}

					if (onProgress != null) { onProgress.Invoke((float)readSize / mainChildrenSize); }

					// Add Default Node if No Node
					if (data.Transforms.Count == 0) {
						data.ResetToDefaultNode();
					}

					// Mat Fix
					if (data.Materials != null) {
						data.Materials.Insert(0, new VoxelData.MaterialData());
					}

				}
			}
			return data;
		}



		// Chunk Reader
		private static int ReadPackChunk (BinaryReader _br) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			_br.ReadInt32();

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadSizeChunk (BinaryReader _br, out Vector3 size) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			int x = _br.ReadInt32();
			int z = _br.ReadInt32();
			int y = _br.ReadInt32();
			size = new Vector3(x, y, z);
			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadVoxelChunk (BinaryReader _br, ref int[,,] tempVoxels) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			int voxelNum = _br.ReadInt32();
			for (int i = 0; i < voxelNum; ++i) {
				int x = _br.ReadByte();
				int z = _br.ReadByte();
				int y = _br.ReadByte();
				tempVoxels[x, y, z] = _br.ReadByte();
			}
			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadPalattee (BinaryReader _br, ref List<Color> colors) {
			colors = new List<Color>();
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();
			for (int i = 0; i < 256; i++) {
				colors.Add(new Color(
					_br.ReadByte() / 255f,
					_br.ReadByte() / 255f,
					_br.ReadByte() / 255f,
					_br.ReadByte() / 255f)
				);
			}
			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadMaterial (BinaryReader _br, ref VoxelData data) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			int id = _br.ReadInt32();
			var dic = ReadDictionary(_br);

			data.Materials.Add(new VoxelData.MaterialData() {
				Index = id,
				Type = VoxelData.MaterialData.GetTypeFromString(TryGetString(dic, "_type", "_diffuse")),
				Weight = TryGetFloat(dic, "_weight", 0f),
				Rough = TryGetFloat(dic, "_rough", 0f),
				Spec = TryGetFloat(dic, "_spec", 0f),
				Ior = TryGetFloat(dic, "_ior", 0f),
				Att = TryGetFloat(dic, "_att", 0f),
				Flux = TryGetFloat(dic, "_flux", 0f),
				LDR = TryGetFloat(dic, "_ldr", 0f),
				Plastic = TryGetInt(dic, "_plastic", 0),
			});

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadTransform (BinaryReader _br, ref VoxelData data) {

			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			int id = _br.ReadInt32();
			var dic = ReadDictionary(_br);
			int childID = _br.ReadInt32();
			int reservedID = _br.ReadInt32();
			int layerID = _br.ReadInt32();
			int frameNum = _br.ReadInt32();
			var frameData = new VoxelData.TransformData.FrameData[frameNum];

			for (int i = 0; i < frameNum; i++) {
				var frameDic = ReadDictionary(_br);
				Vector3 rot;
				Vector3 scale;
				VoxMatrixByteToTransform(TryGetByte(frameDic, "_r", 4), out rot, out scale);
				frameData[i] = new VoxelData.TransformData.FrameData() {
					Position = TryGetVector3(frameDic, "_t", Vector3.zero),
					Rotation = rot,
					Scale = scale,
				};
				// Fix Y-Z
				frameData[i].Position = frameData[i].Position;
				frameData[i].Scale = frameData[i].Scale;
			}

			if (!data.Transforms.ContainsKey(id)) {
				data.Transforms.Add(id, new VoxelData.TransformData() {
					Name = TryGetString(dic, "_name", ""),
					Hidden = TryGetString(dic, "_hidden", "0") == "1",
					ChildID = childID,
					LayerID = layerID,
					Reserved = reservedID,
					Frames = frameData,
				});
			}

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadGroup (BinaryReader _br, ref VoxelData data) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			int id = _br.ReadInt32();
			var nodeAttr = ReadDictionary(_br);
			int childNum = _br.ReadInt32();
			var childData = new int[childNum];

			for (int i = 0; i < childNum; i++) {
				childData[i] = _br.ReadInt32();
			}

			if (!data.Groups.ContainsKey(id)) {
				data.Groups.Add(id, new VoxelData.GroupData() {
					Attributes = nodeAttr,
					ChildNodeId = childData,
				});
			}

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}



		private static int ReadShape (BinaryReader _br, ref VoxelData data) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			int id = _br.ReadInt32();
			var nodeAttr = ReadDictionary(_br);
			int modelNum = _br.ReadInt32();
			var modelData = new KeyValuePair<int, Dictionary<string, string>>[modelNum];

			for (int i = 0; i < modelNum; i++) {
				int modelId = _br.ReadInt32();
				var modelAttr = ReadDictionary(_br);
				modelData[i] = new KeyValuePair<int, Dictionary<string, string>>(modelId, modelAttr);
			}

			if (!data.Shapes.ContainsKey(id)) {
				data.Shapes.Add(id, new VoxelData.ShapeData() {
					Attributes = nodeAttr,
					ModelData = modelData,
				});
			}

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}
			return chunkSize + childrenSize + 4 + 4;
		}




		private static int ReadRig (BinaryReader _br, ref VoxelData data, int version) {
			int chunkSize = _br.ReadInt32();
			int childrenSize = _br.ReadInt32();

			int id = _br.ReadInt32();

			var rigData = new VoxelData.RigData() {
				Bones = new List<VoxelData.RigData.Bone>(),
				Weights = new List<VoxelData.RigData.Weight>(),
				Version = version,
			};

			// Bone
			int boneCount = _br.ReadInt32();
			for (int i = 0; i < boneCount; i++) {
				var bone = new VoxelData.RigData.Bone {
					Name = ReadString(_br),
					ParentIndex = _br.ReadInt32(),
					PositionX = _br.ReadInt32(),
					PositionY = _br.ReadInt32(),
					PositionZ = _br.ReadInt32(),
				};
				rigData.Bones.Add(bone);
			}

			for (int i = 0; i < rigData.Bones.Count; i++) {
				int pIndex = rigData.Bones[i].ParentIndex;
				if (pIndex >= 0 && pIndex < rigData.Bones.Count) {
					rigData.Bones[i].Parent = rigData.Bones[pIndex];
				}
			}

			// Weight
			int WeightCount = _br.ReadInt32();
			for (int i = 0; i < WeightCount; i++) {
				var weight = new VoxelData.RigData.Weight {
					X = _br.ReadInt32(),
					Y = _br.ReadInt32(),
					Z = _br.ReadInt32(),
					BoneIndexA = _br.ReadInt32(),
					BoneIndexB = _br.ReadInt32(),
				};
				rigData.Weights.Add(weight);
			}

			// Version
			rigData.FixVersion();

			// End
			if (!data.Rigs.ContainsKey(id)) {
				data.Rigs.Add(id, rigData);
			}

			if (childrenSize > 0) {
				_br.ReadBytes(childrenSize);
			}

			return chunkSize + childrenSize + 4 + 4;
		}




		#endregion



		#region --- Write ---



		private static byte[] GetVoxFromVoxelData (VoxelData data, OnProgressHandler onProgress) {
			if (!data) { return null; }
			List<byte> voxByte = new List<byte>();
			byte[] mainChrunk = WriteMain(data, onProgress);
			voxByte.AddRange(Encoding.Default.GetBytes("VOX "));
			voxByte.AddRange(GetBytes(data.Version));
			voxByte.AddRange(Encoding.Default.GetBytes("MAIN"));
			voxByte.AddRange(GetBytes(0));
			voxByte.AddRange(GetBytes(mainChrunk.Length));
			voxByte.AddRange(mainChrunk);
			return voxByte.ToArray();
		}



		private static byte[] WriteMain (VoxelData data, OnProgressHandler onProgress) {

			const float STEP_COUNT = 5f;

			List<byte> bytes = new List<byte>();

			if (data.Voxels.Count > 1) {
				// PACK
				//bytes.AddRange(WritePack(data));
			}

			for (int i = 0; i < data.Voxels.Count; i++) {
				if (onProgress != null) { onProgress.Invoke((i + 1) / STEP_COUNT / data.Voxels.Count); }

				// SIZE
				bytes.AddRange(WriteSize(data.Voxels[i]));
				// XYZI
				bytes.AddRange(WriteVoxels(data.Voxels[i]));
			}

			// RGBA
			if (onProgress != null) { onProgress.Invoke((2 / STEP_COUNT)); }

			bytes.AddRange(WritePalette(data.Palette));


			// TGS
			var tgsList = new SortedList<int, byte[]>();

			if (onProgress != null) { onProgress.Invoke(3 / STEP_COUNT); }

			// nTRN
			foreach (var t in data.Transforms) {
				tgsList.Add(t.Key, WriteTransform(t.Key, t.Value));
			}

			// nGRP
			foreach (var g in data.Groups) {
				tgsList.Add(g.Key, WriteGroup(g.Key, g.Value));
			}

			// nSHP
			foreach (var s in data.Shapes) {
				tgsList.Add(s.Key, WriteShape(s.Key, s.Value));
			}

			for (int i = 0; i < tgsList.Keys.Count; i++) {
				bytes.AddRange(tgsList[tgsList.Keys[i]]);
			}

			if (onProgress != null) { onProgress.Invoke(4 / STEP_COUNT); }
			// MATL
			for (int i = 0; i < data.Materials.Count; i++) {
				bytes.AddRange(WriteMaterial(data.Materials[i]));
			}

			if (onProgress != null) { onProgress.Invoke(5 / STEP_COUNT); }
			// RIGG
			foreach (var r in data.Rigs) {
				bytes.AddRange(WriteRig(r.Key, r.Value));
			}

			return bytes.ToArray();
		}




		// Chrunk Writer
		private static byte[] WritePack (VoxelData data) {
			List<byte> bytes = new List<byte>();
			bytes.AddRange(GetBytes(data.Voxels.Count));
			return ToChrunkByte(bytes, "PACK");
		}



		private static byte[] WriteSize (int[,,] voxels) {
			List<byte> bytes = new List<byte>();
			int size0 = Mathf.Clamp(voxels.GetLength(0), byte.MinValue, byte.MaxValue);
			int size1 = Mathf.Clamp(voxels.GetLength(1), byte.MinValue, byte.MaxValue);
			int size2 = Mathf.Clamp(voxels.GetLength(2), byte.MinValue, byte.MaxValue);
			bytes.AddRange(GetBytes(size0));
			bytes.AddRange(GetBytes(size2));
			bytes.AddRange(GetBytes(size1));
			return ToChrunkByte(bytes, "SIZE");
		}



		private static byte[] WriteVoxels (int[,,] voxels) {
			List<byte> bytes = new List<byte>();
			int lenX = Mathf.Clamp(voxels.GetLength(0), byte.MinValue, byte.MaxValue);
			int lenY = Mathf.Clamp(voxels.GetLength(1), byte.MinValue, byte.MaxValue);
			int lenZ = Mathf.Clamp(voxels.GetLength(2), byte.MinValue, byte.MaxValue);
			int voxelNum = 0;
			for (byte x = 0; x < lenX; x++) {
				for (byte y = 0; y < lenY; y++) {
					for (byte z = 0; z < lenZ; z++) {
						if (voxels[x, y, z] != 0) {
							bytes.Add(x);
							bytes.Add(z);
							bytes.Add(y);
							bytes.Add((byte)voxels[x, y, z]);
							voxelNum++;
						}
					}
				}
			}
			bytes.InsertRange(0, GetBytes(voxelNum));
			return ToChrunkByte(bytes, "XYZI");
		}



		private static byte[] WritePalette (List<Color> palette) {
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < 256; i++) {
				Color color = i < palette.Count ? palette[i] : Color.black;
				bytes.Add((byte)(color.r * 255f));
				bytes.Add((byte)(color.g * 255f));
				bytes.Add((byte)(color.b * 255f));
				bytes.Add((byte)(color.a * 255f));
			}
			return ToChrunkByte(bytes, "RGBA");
		}



		private static byte[] WriteTransform (int id, VoxelData.TransformData transform) {

			List<byte> bytes = new List<byte>();

			bytes.AddRange(GetBytes(id));
			// Dic Name Hidden
			bytes.AddRange(GetBytes(2));
			AddVoxStringBytes(ref bytes, "_name");
			AddVoxStringBytes(ref bytes, transform.Name);
			AddVoxStringBytes(ref bytes, "_hidden");
			AddVoxStringBytes(ref bytes, transform.Hidden ? "1" : "0");
			// child node id
			bytes.AddRange(GetBytes(transform.ChildID));
			bytes.AddRange(GetBytes(transform.Reserved));
			bytes.AddRange(GetBytes(transform.LayerID));
			bytes.AddRange(GetBytes(transform.Frames.Length));
			// frame dic
			for (int i = 0; i < transform.Frames.Length; i++) {
				var frame = transform.Frames[i];
				bytes.AddRange(GetBytes(2));
				AddVoxStringBytes(ref bytes, "_r");
				AddVoxStringBytes(ref bytes, TransformToVoxMatrixByteString(frame.Rotation, frame.Scale));
				AddVoxStringBytes(ref bytes, "_t");
				AddVoxStringBytes(ref bytes, string.Format(
					"{0} {1} {2}",
					((int)frame.Position.x).ToString(),
					((int)frame.Position.z).ToString(),
					((int)frame.Position.y).ToString()
				));

			}

			return ToChrunkByte(bytes, "nTRN");
		}



		private static byte[] WriteGroup (int id, VoxelData.GroupData group) {
			List<byte> bytes = new List<byte>();
			bytes.AddRange(GetBytes(id));
			bytes.AddRange(GetBytes(group.Attributes.Count));
			foreach (var att in group.Attributes) {
				AddVoxStringBytes(ref bytes, att.Key);
				AddVoxStringBytes(ref bytes, att.Value);
			}
			bytes.AddRange(GetBytes(group.ChildNodeId.Length));
			for (int i = 0; i < group.ChildNodeId.Length; i++) {
				bytes.AddRange(GetBytes(group.ChildNodeId[i]));
			}
			return ToChrunkByte(bytes, "nGRP");
		}



		private static byte[] WriteShape (int id, VoxelData.ShapeData shape) {
			List<byte> bytes = new List<byte>();
			bytes.AddRange(GetBytes(id));
			bytes.AddRange(GetBytes(shape.Attributes.Count));
			foreach (var att in shape.Attributes) {
				AddVoxStringBytes(ref bytes, att.Key);
				AddVoxStringBytes(ref bytes, att.Value);
			}
			bytes.AddRange(GetBytes(shape.ModelData.Length));
			for (int i = 0; i < shape.ModelData.Length; i++) {
				var pair = shape.ModelData[i];
				bytes.AddRange(GetBytes(pair.Key));
				bytes.AddRange(GetBytes(pair.Value.Count));
				foreach (var att in pair.Value) {
					AddVoxStringBytes(ref bytes, att.Key);
					AddVoxStringBytes(ref bytes, att.Value);
				}
			}
			return ToChrunkByte(bytes, "nSHP");
		}



		private static byte[] WriteMaterial (VoxelData.MaterialData material) {
			List<byte> bytes = new List<byte>();

			bytes.AddRange(GetBytes(material.Index));
			bytes.AddRange(GetBytes(9));
			AddVoxStringBytes(ref bytes, "_type");
			AddVoxStringBytes(ref bytes, VoxelData.MaterialData.GetStringFromType(material.Type));
			AddVoxStringBytes(ref bytes, "_weight");
			AddVoxStringBytes(ref bytes, material.Weight.ToString());
			AddVoxStringBytes(ref bytes, "_rough");
			AddVoxStringBytes(ref bytes, material.Rough.ToString());
			AddVoxStringBytes(ref bytes, "_spec");
			AddVoxStringBytes(ref bytes, material.Spec.ToString());
			AddVoxStringBytes(ref bytes, "_ior");
			AddVoxStringBytes(ref bytes, material.Ior.ToString());
			AddVoxStringBytes(ref bytes, "_att");
			AddVoxStringBytes(ref bytes, material.Att.ToString());
			AddVoxStringBytes(ref bytes, "_flux");
			AddVoxStringBytes(ref bytes, material.Flux.ToString());
			AddVoxStringBytes(ref bytes, "_ldr");
			AddVoxStringBytes(ref bytes, material.LDR.ToString());
			AddVoxStringBytes(ref bytes, "_plastic");
			AddVoxStringBytes(ref bytes, material.Plastic.ToString());
			return ToChrunkByte(bytes, "MATL");
		}



		private static byte[] WriteRig (int id, VoxelData.RigData rig) {

			List<byte> bytes = new List<byte>();

			bytes.AddRange(GetBytes(id));

			// Bones
			bytes.AddRange(GetBytes(rig.Bones.Count));
			for (int i = 0; i < rig.Bones.Count; i++) {
				var bone = rig.Bones[i];
				AddVoxStringBytes(ref bytes, bone.Name);
				bytes.AddRange(GetBytes(bone.ParentIndex));
				bytes.AddRange(GetBytes(bone.PositionX));
				bytes.AddRange(GetBytes(bone.PositionY));
				bytes.AddRange(GetBytes(bone.PositionZ));
			}

			// Weights
			bytes.AddRange(GetBytes(rig.Weights.Count));
			for (int i = 0; i < rig.Weights.Count; i++) {
				var weight = rig.Weights[i];
				bytes.AddRange(GetBytes(weight.X));
				bytes.AddRange(GetBytes(weight.Y));
				bytes.AddRange(GetBytes(weight.Z));
				bytes.AddRange(GetBytes(weight.BoneIndexA));
				bytes.AddRange(GetBytes(weight.BoneIndexB));
			}

			return ToChrunkByte(bytes, "_RIG");
		}




		#endregion



		#endregion




		#region --- QB ---




		private static VoxelData GetVoxelDataFromQb (byte[] qbBytes) {
			QbData qData = new QbData();
			using (MemoryStream ms = new MemoryStream(qbBytes)) {
				using (BinaryReader br = new BinaryReader(ms)) {


					int index;
					int data;
					uint count;
					const uint CODEFLAG = 2;
					const uint NEXTSLICEFLAG = 6;

					qData.Version = br.ReadUInt32();
					qData.ColorFormat = br.ReadUInt32();
					qData.ZAxisOrientation = br.ReadUInt32();
					qData.Compressed = br.ReadUInt32();
					qData.VisibleMask = br.ReadUInt32();
					qData.NumMatrixes = br.ReadUInt32();

					qData.MatrixList = new List<QbData.QbMatrix>();

					for (int i = 0; i < qData.NumMatrixes; i++) {

						QbData.QbMatrix qm = new QbData.QbMatrix();

						// read matrix name
						int nameLength = br.ReadByte();
						qm.Name = br.ReadChars(nameLength).ToString(); // Name

						// read matrix size
						qm.SizeX = br.ReadInt32();
						qm.SizeY = br.ReadInt32();
						qm.SizeZ = br.ReadInt32();

						// read matrix position
						qm.PosX = br.ReadInt32();
						qm.PosY = br.ReadInt32();
						qm.PosZ = br.ReadInt32();

						// create matrix and add to matrix list
						qm.Voxels = new int[qm.SizeX, qm.SizeY, qm.SizeZ];

						int x, y, z;
						if (qData.Compressed == 0) {
							for (z = 0; z < qm.SizeZ; z++) {
								for (y = 0; y < qm.SizeY; y++) {
									for (x = 0; x < qm.SizeX; x++) {
										qm.Voxels[x, y, z] = (int)br.ReadUInt32();
									}
								}
							}
						} else {
							z = 0;
							while (z < qm.SizeZ) {
								index = 0;
								while (true) {
									data = (int)br.ReadUInt32();
									if (data == NEXTSLICEFLAG)
										break;
									else if (data == CODEFLAG) {
										count = br.ReadUInt32();
										data = (int)br.ReadUInt32();
										for (int j = 0; j < count; j++) {
											x = index % qm.SizeX;
											y = index / qm.SizeX;
											index++;
											qm.Voxels[x, y, z] = data;
										}
									} else {
										x = index % qm.SizeX;
										y = index / qm.SizeX;
										index++;
										qm.Voxels[x, y, z] = data;
									}
								}
								z++;
							}
						}
						qData.MatrixList.Add(qm);
					}

				}
			}
			return qData.GetVoxelData();
		}



		private static byte[] GetQbFromVoxelData (VoxelData data) {


			List<byte> bytes = new List<byte>();

			bytes.AddRange(System.BitConverter.GetBytes((uint)257));
			bytes.AddRange(System.BitConverter.GetBytes((uint)0)); // Always RGBA
			bytes.AddRange(System.BitConverter.GetBytes((uint)1)); // Always Right Handed
			bytes.AddRange(System.BitConverter.GetBytes(0)); // Always Not Compressed
			bytes.AddRange(System.BitConverter.GetBytes((uint)1));
			bytes.AddRange(System.BitConverter.GetBytes((uint)data.Voxels.Count));

			for (int index = 0; index < data.Voxels.Count; index++) {

				var voxels = data.Voxels[index];

				// Get Size
				int sizeX = voxels.GetLength(0);
				int sizeY = voxels.GetLength(1);
				int sizeZ = voxels.GetLength(2);

				// Get Position
				Vector3 size = data.GetModelSize(index);
				Vector3 pos, rot, scl;
				data.GetModelTransform(index, out pos, out rot, out scl);
				int posX = (int)pos.x - (int)size.x / 2;
				int posY = (int)pos.y - (int)size.y / 2;
				int posZ = (int)pos.z - (int)size.z / 2;

				// name
				bytes.Add(0);

				// size
				bytes.AddRange(System.BitConverter.GetBytes(sizeX));
				bytes.AddRange(System.BitConverter.GetBytes(sizeY));
				bytes.AddRange(System.BitConverter.GetBytes(sizeZ));

				// pos
				bytes.AddRange(System.BitConverter.GetBytes(posX));
				bytes.AddRange(System.BitConverter.GetBytes(posY));
				bytes.AddRange(System.BitConverter.GetBytes(posZ));

				// voxels
				for (int z = 0; z < sizeZ; z++) {
					for (int y = 0; y < sizeY; y++) {
						for (int x = 0; x < sizeX; x++) {
							int v = voxels[x, y, z];
							bytes.AddRange(System.BitConverter.GetBytes(
								v == 0 ? 0 : ColorToInt(data.GetColorFromPalette(v))
							));
						}
					}
				}


			}


			return bytes.ToArray();
		}




		#endregion




		#region --- UTL ---



		private static byte[] GetBytes (int i) {
			return System.BitConverter.GetBytes(i);
		}



		private static void AddVoxStringBytes (ref List<byte> bytes, string str) {
			var strBytes = Encoding.Default.GetBytes(str);
			bytes.AddRange(GetBytes(strBytes.Length));
			bytes.AddRange(strBytes);
		}



		private static bool CheckID (byte[] bytes, string id) {
			for (int i = 0; i < bytes.Length && i < id.Length; i++) {
				if (id[i] != bytes[i]) {
					return false;
				}
			}
			return true;
		}



		private static string GetID (byte[] bytes) {
			string id = "";
			for (int i = 0; i < bytes.Length; i++) {
				id += (char)bytes[i];
			}
			return id;
		}



		private static string ReadString (BinaryReader br) {
			int len = br.ReadInt32();
			byte[] bytes = br.ReadBytes(len);
			string str = "";
			for (int i = 0; i < bytes.Length; i++) {
				str += (char)bytes[i];
			}
			return str;
		}



		// Dic
		private static Dictionary<string, string> ReadDictionary (BinaryReader br) {
			var dic = new Dictionary<string, string>();
			int len = br.ReadInt32();
			for (int i = 0; i < len; i++) {
				string key = ReadString(br);
				string value = ReadString(br);
				dic.Add(key, value);
			}
			return dic;
		}



		private static float TryGetFloat (Dictionary<string, string> dic, string key, float defaultValue) {
			float res;
			if (dic.ContainsKey(key) && float.TryParse(dic[key], out res)) {
				return res;
			}
			return defaultValue;
		}



		private static int TryGetInt (Dictionary<string, string> dic, string key, int defaultValue) {
			int res;
			if (dic.ContainsKey(key) && int.TryParse(dic[key], out res)) {
				return res;
			}
			return defaultValue;
		}



		private static string TryGetString (Dictionary<string, string> dic, string key, string defaultValue) {
			return dic.ContainsKey(key) ? dic[key] : defaultValue;
		}



		private static byte TryGetByte (Dictionary<string, string> dic, string key, byte defaultValue) {
			byte res;
			if (dic.ContainsKey(key) && byte.TryParse(dic[key], out res)) {
				return res;
			}
			return defaultValue;
		}



		private static Vector3 TryGetVector3 (Dictionary<string, string> dic, string key, Vector3 defaultValue) {
			if (dic.ContainsKey(key)) {
				string[] valueStr = dic[key].Split(' ');
				Vector3 vector = Vector3.zero;
				if (valueStr.Length == 3) {
					for (int i = 0; i < 3; i++) {
						int value;
						if (int.TryParse(valueStr[i], out value)) {
							vector[i] = value;
						} else {
							return defaultValue;
						}
					}
					return Util.SwipYZ(vector);
				}
			}
			return defaultValue;
		}



		// Chrunk
		private static byte[] ToChrunkByte (List<byte> source, string id) {
			int len = source.Count;
			source.InsertRange(0, GetBytes(0));
			source.InsertRange(0, GetBytes(len));
			source.InsertRange(0, Encoding.Default.GetBytes(id));
			return source.ToArray();
		}



		// Matrix
		public static void VoxMatrixByteToTransform (byte mByte, out Vector3 rotation, out Vector3 scale) {
			Util.VoxMatrixByteToTransform(mByte, out rotation, out scale);
		}



		private static string TransformToVoxMatrixByteString (Vector3 rot, Vector3 scale) {
			return Util.TransformToVoxMatrixByte(rot, scale).ToString();
		}



		// Qb
		private static int ColorToInt (Color color) {
			return (
				((int)(color.a * 255) << 24) |
				((int)(color.b * 255) << 16) |
				((int)(color.g * 255) << 8) |
				((int)(color.r * 255) << 0)
			);
		}




		#endregion



	}
}