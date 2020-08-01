namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Core_Sprite {





		#region --- SUB ---



		public enum Direction {
			Up = 0,
			Down = 1,
			Front = 2,
			Back = 3,
			Left = 4,
			Right = 5,
		}



		public enum FaceType {

			Up_0 = 0,
			Up_22 = 1,
			Up_45 = 2,
			Up_67 = 3,

			Left_0 = 4,
			Left_22 = 5,
			Left_45 = 6,
			Left_67 = 7,

			Front_0 = 8,
			Front_22 = 9,
			Front_45 = 10,
			Front_67 = 11,
		}



		public enum SpriteType {
			_8bit = 0,
			_25D = 1,
			_2D = 2,
		}



		public enum SpriteNum {
			_1 = 1,
			_2 = 2,
			_4 = 4,
			_8 = 8,
		}


		public enum Sprite2DNum {
			_1 = 1,
			_2 = 2,
			_3 = 3,
			_6 = 6,
		}



		public class Result {
			public int Width;
			public int Height;
			public string[] NameFixes;
			public Texture2D Texture;
			public Vector2[] Pivots;
			public Rect[] Rects;
		}



		public struct Voxel {

			public int ColorIndex {
				get;
				set;
			}

			public bool IsEmpty {
				get {
					return ColorIndex == 0;
				}
			}

			public bool IsVisible {
				get {
					return Visible != null && Visible.Length > 5 && (Visible[0] || Visible[1] || Visible[2] || Visible[3] || Visible[4] || Visible[5]);
				}
				set {
					Visible[0] = value;
					Visible[1] = value;
					Visible[2] = value;
					Visible[3] = value;
					Visible[4] = value;
					Visible[5] = value;
				}
			}

			public bool VisibleLeft {
				get {
					return Visible[(int)Direction.Left];
				}
				set {
					Visible[(int)Direction.Left] = value;
				}
			}
			public bool VisibleRight {
				get {
					return Visible[(int)Direction.Right];
				}
				set {
					Visible[(int)Direction.Right] = value;
				}
			}
			public bool VisibleUp {
				get {
					return Visible[(int)Direction.Up];
				}
				set {
					Visible[(int)Direction.Up] = value;
				}
			}
			public bool VisibleDown {
				get {
					return Visible[(int)Direction.Down];
				}
				set {
					Visible[(int)Direction.Down] = value;
				}
			}
			public bool VisibleFront {
				get {
					return Visible[(int)Direction.Front];
				}
				set {
					Visible[(int)Direction.Front] = value;
				}
			}
			public bool VisibleBack {
				get {
					return Visible[(int)Direction.Back];
				}
				set {
					Visible[(int)Direction.Back] = value;
				}
			}
			public bool[] Visible {
				get;
				private set;
			}

			public void Init () {
				ColorIndex = 0;
				Visible = new bool[6] { false, false, false, false, false, false };
			}

		}



		private struct VoxelFace {
			public Vector3 Position;
			public Vector3 Normal;
			public Color Color;
			public FaceType Type;

			public static FaceType GetFaceType (int spriteAngleIndex, Direction dir, float normalX) {
				int type = dir == Direction.Up ? 0 : normalX < 0f ? 4 : 8;
				type +=
					spriteAngleIndex >= 0 && spriteAngleIndex <= 3 ? 2 :
					(spriteAngleIndex >= 4 && spriteAngleIndex <= 7 ? 0 :
					(spriteAngleIndex >= 8 && spriteAngleIndex <= 11 ? 1 : 3));
				if (type == (int)FaceType.Left_0) {
					type = (int)FaceType.Front_0;
				}
				return (FaceType)type;
			}

		}



		private struct Pixel {
			public Color Color;
			public int X;
			public int Y;
		}



		private class FaceSorter : IComparer<VoxelFace> {
			public int Compare (VoxelFace x, VoxelFace y) {
				return y.Position.z.CompareTo(x.Position.z);
			}
		}




		#endregion




		#region --- VAR ---



		public static readonly float[] SPRITE_ANGLE = new float[] {
			45f, 135f, 225f, 315f,
			0f, 90f, 180f, 270f
		};
		public static readonly float[] FIXED_SPRITE_ANGLE = new float[] {
			45f, 135f, 225f, 315f,
			0f, 90.001f, 180f, 270.001f
		};
		private static readonly string[] SPRITE_2D_NAMES = new string[] { "F", "L", "B", "R", "U", "D" };
		private static readonly Vector3[] VOX_CENTER_OFFSET = new Vector3[] {
			Vector3.up,
			Vector3.down,
			Vector3.back ,
			Vector3.forward,
			Vector3.left,
			Vector3.right,
		};
		private const int MAX_ROOM_NUM = 80 * 80 * 80;
		private static readonly float CAMERA_ANGLE = 35f;



		#endregion



		// API
		public static Result CreateSprite (VoxelData voxelData, int modelIndex, SpriteType type, int num, float light, Vector3 pivot, Camera screenShotCamera, float cameraScale) {

			// Voxels
			Voxel[,,] voxels = GetVoxels(voxelData, modelIndex);

			// Colorss
			int[] widths;
			int[] heights;
			Color[][] colorss;
			Result result;

			switch (type) {

				default:
				case SpriteType._25D:
					num = Mathf.Clamp(num, 1, 8);
					light = Mathf.Clamp01(light);
					colorss = Get25DColorss(
						voxels, voxelData.Palette.ToArray(), screenShotCamera,
						num, light, cameraScale,
						out widths, out heights
					);
					result = PackTextures(colorss, widths, heights, pivot);
					result.NameFixes = new string[result.Rects.Length];
					for (int i = 0; i < result.Rects.Length; i++) {
						result.NameFixes[i] = SPRITE_ANGLE[i].ToString("0");
					}
					break;

				case SpriteType._8bit:
					num = Mathf.Clamp(num, 1, 8);
					light = Mathf.Clamp01(light);
					colorss = Get8bitColorss(
						voxels, voxelData.Palette.ToArray(),
						num, light,
						out widths, out heights
					);
					result = PackTextures(colorss, widths, heights, pivot);
					result.NameFixes = new string[result.Rects.Length];
					for (int i = 0; i < result.Rects.Length; i++) {
						result.NameFixes[i] = SPRITE_ANGLE[i].ToString("0");
					}
					break;

				case SpriteType._2D:
					num = Mathf.Clamp(num, 1, 6);
					light = Mathf.Clamp01(light);
					colorss = Get2DColorss(voxels, voxelData.Palette.ToArray(), num, light, out widths, out heights);
					result = Pack2DTextures(colorss, widths, heights, pivot);
					result.NameFixes = new string[result.Rects.Length];
					for (int i = 0; i < result.Rects.Length; i++) {
						result.NameFixes[i] = SPRITE_2D_NAMES[i];
					}
					break;
			}
			return result;
		}




		// LGC
		private static Voxel[,,] GetVoxels (VoxelData voxelData, int modelIndex) {
			var size = voxelData.GetModelSize(modelIndex);
			int sizeX = (int)size.x;
			int sizeY = (int)size.z;
			int sizeZ = (int)size.y;
			Voxel[,,] voxels = new Voxel[sizeX, sizeY, sizeZ];
			for (int i = 0; i < sizeX; i++) {
				for (int j = 0; j < sizeY; j++) {
					for (int k = 0; k < sizeZ; k++) {
						voxels[i, j, k].Init();
						voxels[i, j, k].ColorIndex = voxelData.Voxels[modelIndex][i, k, j];
					}
				}
			}
			for (int i = 0; i < sizeX; i++) {
				for (int j = 0; j < sizeY; j++) {
					for (int k = 0; k < sizeZ; k++) {
						if (voxels[i, j, k].IsEmpty) {
							voxels[i, j, k].IsVisible = true;
							continue;
						}
						voxels[i, j, k].VisibleLeft = i > 0 ? voxels[i - 1, j, k].IsEmpty : true;
						voxels[i, j, k].VisibleRight = i < sizeX - 1 ? voxels[i + 1, j, k].IsEmpty : true;
						voxels[i, j, k].VisibleFront = j > 0 ? voxels[i, j - 1, k].IsEmpty : true;
						voxels[i, j, k].VisibleBack = j < sizeY - 1 ? voxels[i, j + 1, k].IsEmpty : true;
						voxels[i, j, k].VisibleDown = k > 0 ? voxels[i, j, k - 1].IsEmpty : true;
						voxels[i, j, k].VisibleUp = k < sizeZ - 1 ? voxels[i, j, k + 1].IsEmpty : true;
					}
				}
			}
			return voxels;
		}



		private static Color[][] Get8bitColorss (
			Voxel[,,] voxels, Color[] palette,
			int spriteNum, float lightIntensity,
			out int[] widths, out int[] heights
		) {

			int voxelSizeX = voxels.GetLength(0);
			int voxelSizeY = voxels.GetLength(1);
			int voxelSizeZ = voxels.GetLength(2);
			widths = new int[spriteNum];
			heights = new int[spriteNum];

			if (voxelSizeX * voxelSizeY * voxelSizeZ > MAX_ROOM_NUM) {
				if (!Util.Dialog(
					"Warning",
					"Model Is Too Large !\nIt may takes very long time to create this sprite.\nAre you sure to do that?",
					"Just Go ahead!",
					"Cancel"
				)) {
					return null;
				}
			}

			Color[][] colorss = new Color[spriteNum][];

			for (int index = 0; index < spriteNum; index++) {

				float angleY = FIXED_SPRITE_ANGLE[index];
				Quaternion cameraRot = Quaternion.Inverse(Quaternion.Euler(CAMERA_ANGLE, angleY, 0f));
				Vector3 minPos;
				Vector3 maxPos;


				// Get faces
				List<VoxelFace> faces = GetFaces(
					voxels, palette,
					new Vector3(voxelSizeX, voxelSizeY, voxelSizeZ), cameraRot,
					index, lightIntensity,
					out minPos, out maxPos
				);

				// Get Pixels
				int minPixelX;
				int minPixelY;
				int maxPixelX;
				int maxPixelY;

				// Get Pixels
				List<Pixel> pixels = GetPixels(
					faces,
					out minPixelX, out minPixelY,
					out maxPixelX, out maxPixelY
				);

				// W and H
				int width = maxPixelX - minPixelX + 1 + 2;
				int height = maxPixelY - minPixelY + 1 + 2;

				// Get Colorss
				colorss[index] = new Color[width * height];
				int len = pixels.Count;
				for (int i = 0; i < len; i++) {
					int id = (pixels[i].Y - minPixelY + 1) * width + (pixels[i].X - minPixelX + 1);
					colorss[index][id] = pixels[i].Color;
				}

				// Cheat
				{
					List<int> cheatPixels = new List<int>();
					List<Color> cheatColors = new List<Color>();
					for (int x = 0; x < width; x++) {
						for (int y = 0; y < height; y++) {
							Color c = CheckPixelsAround_Cheat(colorss[index], x, y, width, height);
							if (c != Color.clear) {
								cheatPixels.Add(y * width + x);
								cheatColors.Add(c);
							}
						}
					}
					int cheatCount = cheatPixels.Count;
					for (int i = 0; i < cheatCount; i++) {
						colorss[index][cheatPixels[i]] = cheatColors[i];
					}
				}

				// Final
				widths[index] = width;
				heights[index] = height;

			}
			return colorss;
		}



		private static Color[][] Get25DColorss (
			Voxel[,,] voxels, Color[] palette, Camera camera,
			int spriteNum, float lightIntensity, float cameraScale,
			out int[] widths, out int[] heights
		) {

			int voxelSizeX = voxels.GetLength(0);
			int voxelSizeY = voxels.GetLength(1);
			int voxelSizeZ = voxels.GetLength(2);

			widths = new int[spriteNum];
			heights = new int[spriteNum];

			if (!camera) { return null; }

			if (voxelSizeX * voxelSizeY * voxelSizeZ > MAX_ROOM_NUM) {
				if (!Util.Dialog(
					"Warning",
					"Model Is Too Large !\nIt may takes very long time to create this sprite.\nAre you sure to do that?",
					"Just Go ahead!",
					"Cancel"
				)) {
					return null;
				}
			}

			Color[][] colorss = new Color[spriteNum][];

			Transform cameraRoot = camera.transform.parent;
			Quaternion oldRot = cameraRoot.rotation;
			float oldSize = camera.orthographicSize;
			camera.orthographicSize = (11f - cameraScale) * Mathf.Max(voxelSizeX, voxelSizeY, voxelSizeZ);

			for (int index = 0; index < spriteNum; index++) {

				float angleY = FIXED_SPRITE_ANGLE[index];
				Quaternion cameraRot = Quaternion.Euler(
					CAMERA_ANGLE,
					angleY,
					0f
				);
				cameraRoot.rotation = cameraRot;

				var texture = Util.RenderTextureToTexture2D(camera);
				texture = Util.TrimTexture(texture);

				int width = texture.width;
				int height = texture.height;
				colorss[index] = texture.GetPixels();

				// Cheat
				{
					List<int> cheatPixels = new List<int>();
					List<Color> cheatColors = new List<Color>();
					for (int x = 0; x < width; x++) {
						for (int y = 0; y < height; y++) {
							Color c = CheckPixelsAround_Cheat(colorss[index], x, y, width, height);
							if (c != Color.clear) {
								cheatPixels.Add(y * width + x);
								cheatColors.Add(c);
							}
						}
					}
					int cheatCount = cheatPixels.Count;
					for (int i = 0; i < cheatCount; i++) {
						colorss[index][cheatPixels[i]] = cheatColors[i];
					}
				}

				// Alpha Fix
				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						Color c = colorss[index][y * width + x];
						if (c.a < 0.8f) {
							colorss[index][y * width + x] = Color.clear;
						}
					}
				}


				// Final
				widths[index] = width;
				heights[index] = height;
			}

			cameraRoot.rotation = oldRot;
			camera.orthographicSize = oldSize;

			return colorss;
		}



		private static Color[][] Get2DColorss (Voxel[,,] voxels, Color[] palette, int num, float light01, out int[] widths, out int[] heights) {
			widths = new int[num];
			heights = new int[num];
			Color[][] colorss = new Color[num][];
			int[] FACE_DIR_ID = new int[6] { 2, 4, 3, 5, 0, 1 };
			for (int i = 0; i < num; i++) {
				int width = voxels.GetLength(i == 1 || i == 3 ? 2 : 0);
				int height = voxels.GetLength(i == 0 || i == 2 ? 2 : 1);
				int depth = voxels.GetLength(i == 1 || i == 3 ? 0 : i == 0 || i == 2 ? 1 : 2);
				int faceDirIndex = FACE_DIR_ID[i];
				colorss[i] = new Color[(width + 1) * (height + 1)];
				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						Color c = Color.clear;
						for (int z = 0; z < depth; z++) {
							Voxel v =
								i == 0 ? voxels[x, z, y] :
								i == 1 ? voxels[z, height - y - 1, x] :
								i == 2 ? voxels[width - x - 1, depth - z - 1, y] :
								i == 3 ? voxels[depth - z - 1, y, x] :
								i == 4 ? voxels[x, y, depth - z - 1] :
										 voxels[x, height - y - 1, z];
							if (v.ColorIndex > 0 && v.Visible[faceDirIndex]) {
								c = Color.Lerp(
									palette[v.ColorIndex - 1],
									Color.black,
									light01 * z / depth
								);
								break;
							}
						}
						colorss[i][i == 1 || i == 3 ? x * (height + 1) + y : y * (width + 1) + x] = c;
					}
				}
				widths[i] = (i == 1 || i == 3 ? height : width) + 1;
				heights[i] = (i == 1 || i == 3 ? width : height) + 1;
			}
			return colorss;
		}



		private static List<VoxelFace> GetFaces (
			Voxel[,,] voxels, Color[] palette,
			Vector3 voxelSize, Quaternion cameraRot,
			int cameraAngleIndex, float lightIntensity,
			out Vector3 minPos, out Vector3 maxPos
		) {
			lightIntensity = 1f - lightIntensity;
			minPos = Vector3.one * float.MaxValue;
			maxPos = Vector3.one * float.MinValue;
			List<VoxelFace> faces = new List<VoxelFace>();
			for (int x = 0; x < voxelSize.x; x++) {
				for (int y = 0; y < voxelSize.y; y++) {
					for (int z = 0; z < voxelSize.z; z++) {
						Voxel vox = voxels[x, y, z];
						Color color = palette[vox.ColorIndex <= 0 ? 0 : vox.ColorIndex - 1];
						for (int i = 0; i < 6; i++) {
							if (!vox.IsEmpty && vox.Visible[i]) {
								Vector3 pos = cameraRot * (new Vector3(
									x - voxelSize.x,
									z - voxelSize.z,
									y - voxelSize.y
								) + VOX_CENTER_OFFSET[i] * 0.5f);
								minPos = Vector3.Min(minPos, pos);
								maxPos = Vector3.Max(maxPos, pos);
								Vector3 worldNormal = cameraRot * VOX_CENTER_OFFSET[i];
								// Normal Check
								if (Vector3.Angle(worldNormal, Vector3.back) >= 90f) {
									continue;
								}
								VoxelFace face = new VoxelFace() {
									Position = pos,
									Normal = worldNormal,
									Type = VoxelFace.GetFaceType(cameraAngleIndex, (Direction)i, worldNormal.x),
								};
								face.Color = Color.Lerp(
									color,// F L U
									(int)face.Type > 3 ? (int)face.Type > 7 ? Color.black : Color.black : Color.black,
									(int)face.Type > 3 ? (int)face.Type > 7 ? lightIntensity * 0.4f : lightIntensity * 0.8f : lightIntensity * 0.6f
								);
								faces.Add(face);
							}
						}
					}
				}
			}
			faces.Sort(new FaceSorter());
			return faces;
		}




		private static Color CheckPixelsAround_Cheat (Color[] colors, int x, int y, int w, int h) {
			// Self
			if (colors[y * w + x] != Color.clear) {
				return Color.clear;
			}

			Color c, color = Color.clear;

			// U
			if (y < h - 1) {
				c = colors[(y + 1) * w + x];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// D
			if (y > 0) {
				c = colors[(y - 1) * w + x];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// L
			if (x < w - 1) {
				c = colors[y * w + x + 1];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			// R
			if (x > 0) {
				c = colors[y * w + x - 1];
				if (c == Color.clear) {
					return Color.clear;
				} else {
					color = c;
				}
			}

			return color;
		}



		#region --- Colorss ---



		private static List<Pixel> GetPixels (
			List<VoxelFace> faces,
			out int minPixelX, out int minPixelY,
			out int maxPixelX, out int maxPixelY
		) {
			minPixelX = int.MaxValue;
			minPixelY = int.MaxValue;
			maxPixelX = 0;
			maxPixelY = 0;
			List<Pixel> pixels = new List<Pixel>();
			int count = faces.Count;
			for (int index = 0; index < count; index++) {
				VoxelFace face = faces[index];
				float pixelSize = 1f;
				Vector2 pos = new Vector2(
					(face.Position.x * pixelSize),
					(face.Position.y * pixelSize)
				);
				pos.x = Mathf.Floor((pos.x / pixelSize) * pixelSize);
				pos.y = Mathf.Floor((pos.y / pixelSize) * pixelSize);

				minPixelX = Mathf.Min(minPixelX, (int)pos.x);
				minPixelY = Mathf.Min(minPixelY, (int)pos.y);
				maxPixelX = Mathf.Max((int)pos.x, maxPixelX);
				maxPixelY = Mathf.Max((int)pos.y, maxPixelY);
				pixels.Add(new Pixel() { Color = face.Color, X = (int)pos.x, Y = (int)pos.y });
			}

			return pixels;
		}






		#endregion



		private static Result PackTextures (Color[][] colorss, int[] widths, int[] heights, Vector2 pivot) {

			int tCount = colorss.Length;
			int gapSize = 1;
			Result resultInfo = new Result() {
				Pivots = new Vector2[tCount],
			};

			// Single Size
			int singleWidth = 0;
			int singleHeight = 0;
			for (int i = 0; i < tCount; i++) {
				singleWidth = Mathf.Max(singleWidth, widths[i]);
				singleHeight = Mathf.Max(singleHeight, heights[i]);
				resultInfo.Pivots[i] = pivot;
			}

			// Size All
			int aimCountX = tCount > 4 ? 4 : tCount;
			int aimCountY = ((tCount - 1) / 4) + 1;
			int aimWidth = aimCountX * singleWidth + gapSize * (aimCountX + 1);
			int aimHeight = aimCountY * singleHeight + gapSize * (aimCountY + 1);

			resultInfo.Width = aimWidth;
			resultInfo.Height = aimHeight;
			resultInfo.Texture = new Texture2D(aimWidth, aimHeight, TextureFormat.ARGB32, false);
			resultInfo.Texture.SetPixels(new Color[aimWidth * aimHeight]);

			Rect[] spriteRects = new Rect[tCount];
			for (int i = 0; i < tCount; i++) {
				int width = widths[i];
				int height = heights[i];
				int globalOffsetX = (i % 4) * singleWidth + ((i % 4) + 1) * gapSize;
				int globalOffsetY = (i / 4) * singleHeight + ((i / 4) + 1) * gapSize;
				int offsetX = globalOffsetX + (singleWidth - width) / 2;
				int offsetY = globalOffsetY + (singleHeight - height) / 2;
				// Rect
				spriteRects[i] = new Rect(globalOffsetX, globalOffsetY, singleWidth, singleHeight);
				resultInfo.Texture.SetPixels(offsetX, offsetY, width, height, colorss[i]);
			}
			resultInfo.Texture.Apply();
			resultInfo.Rects = spriteRects;

			return resultInfo;
		}



		private static Result Pack2DTextures (Color[][] colorss, int[] widths, int[] heights, Vector2 pivot) {

			int tCount = colorss.Length;
			Result resultInfo = new Result() {
				Pivots = new Vector2[6] { pivot, pivot, pivot, pivot, pivot, pivot },
			};

			Texture2D[] textures = new Texture2D[colorss.Length];
			for (int i = 0; i < textures.Length; i++) {
				textures[i] = new Texture2D(widths[i], heights[i], TextureFormat.ARGB32, false);
				textures[i].SetPixels(colorss[i]);
				textures[i].Apply();
			}

			var packingList = new List<PackingData>();
			for (int i = 0; i < textures.Length; i++) {
				var t = textures[i];
				packingList.Add(new PackingData(t.width, t.height, t.GetPixels(), false));
			}

			Rect[] rects = RectPacking.PackTextures(out resultInfo.Texture, packingList, false, true);
			resultInfo.Width = resultInfo.Texture.width;
			resultInfo.Height = resultInfo.Texture.height;
			for (int i = 0; i < rects.Length; i++) {
				rects[i].x *= resultInfo.Width;
				rects[i].y *= resultInfo.Height;
				rects[i].width *= resultInfo.Width;
				rects[i].height *= resultInfo.Height;
				rects[i].width = rects[i].width - 1;
				rects[i].height = rects[i].height - 1;
			}
			resultInfo.Rects = rects;
			return resultInfo;
		}





	}

}