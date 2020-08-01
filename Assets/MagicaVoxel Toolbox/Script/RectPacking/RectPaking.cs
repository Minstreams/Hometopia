namespace MagicaVoxelToolbox {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;




	public class PackingData {


		public int Width;
		public int Height;
		public Color[] TextureColors;


		public PackingData (int sizeU, int sizeV, Color[] colors, bool outline = true) {
			Width = sizeU;
			Height = sizeV;
			if (outline) {
				Width += 2;
				Height += 2;
			}
			TextureColors = new Color[Width * Height];
			if (outline) {
				for (int v = 1; v < Height - 1; v++) {
					TextureColors[v * Width] = colors[(v - 1) * sizeU];
					for (int u = 1; u < Width - 1; u++) {
						TextureColors[v * Width + u] = colors[(v - 1) * sizeU + (u - 1)];
					}
					TextureColors[v * Width + Width - 1] = colors[(v - 1) * sizeU + (Width - 3)];

				}
				for (int u = 0; u < Width; u++) {
					TextureColors[u] = TextureColors[Width + u];
					TextureColors[(Height - 1) * Width + u] = TextureColors[(Height - 2) * Width + u];
				}
			} else {
				for (int v = 0; v < Height; v++) {
					for (int u = 0; u < Width; u++) {
						TextureColors[v * Width + u] = colors[v * sizeU + u];
					}
				}
			}

		}


		public bool SameWith (PackingData other) {
			if (Width != other.Width || Height != other.Height) {
				return false;
			}
			int end = TextureColors.Length - Width;
			for (int i = Width; i < end; i++) {
				if (TextureColors[i] != other.TextureColors[i]) {
					return false;
				}
			}
			return true;
		}


	}



	public struct RectPacking {



		private class ItemSorter : IComparer<Item> {
			bool SortWithIndex;
			public ItemSorter (bool sortWithIndex) {
				SortWithIndex = sortWithIndex;
			}
			public int Compare (Item x, Item y) {
				return SortWithIndex ?
					x.Index.CompareTo(y.Index) :
					y.Height.CompareTo(x.Height);
			}
		}



		private struct Item {
			public int Index;
			public int X, Y;
			public int Width, Height;
			public Color[] TextureColors;
		}


		private class Shelf {

			public int Y;
			public int Width;
			public int Height;
			public int[] RoomHeight;


			public bool AddItem (ref Item item, ref int width, ref int height) {

				int currentFitWidth = 0;
				int maxRoomY = 0;
				for (int i = 0; i < RoomHeight.Length; i++) {
					if (RoomHeight[i] >= item.Height) {
						// fit
						currentFitWidth++;
						maxRoomY = Mathf.Max(maxRoomY, Height - RoomHeight[i]);
						if (currentFitWidth >= item.Width) {
							item.Y = Y + maxRoomY;
							item.X = i - currentFitWidth + 1;
							// set width height
							width = Mathf.Max(width, item.X + item.Width);
							height = Mathf.Max(height, item.Y + item.Height);
							// Set room height
							for (int j = item.X; j < item.X + item.Width; j++) {
								RoomHeight[j] = Height - maxRoomY - item.Height;
							}
							return true;
						}
					} else {
						// not fit
						currentFitWidth = 0;
						maxRoomY = 0;
					}
				}
				return false;
			}

		}



		public static Rect[] PackTextures (out Texture2D texture, List<PackingData> packingList, bool hasOutline, bool defaultClear) {

			// Check
			if (packingList.Count == 0) {
				texture = Texture2D.whiteTexture;
				return new Rect[1] { new Rect(0, 0, 1, 1) };
			}

			// Init
			int aimSize = 16;
			int minWidth = 16;
			int allArea = 0;
			List<Item> items = new List<Item>();
			for (int i = 0; i < packingList.Count; i++) {
				int w = packingList[i].Width;
				int h = packingList[i].Height;
				items.Add(new Item() {
					Index = i,
					Width = w,
					Height = h,
					TextureColors = packingList[i].TextureColors
				});
				allArea += items[i].Width * items[i].Height;
				minWidth = Mathf.Max(minWidth, items[i].Width);
			}
			while (aimSize < minWidth || aimSize * aimSize < allArea * 1.1f) {
				aimSize *= 2;
			}

			//Sort With Height
			items.Sort(new ItemSorter(false));

			// Pack
			int width = 0;
			int height = 0;

			List<Shelf> shelfs = new List<Shelf>();
			for (int i = 0; i < items.Count; i++) {

				// Try Add
				bool success = false;
				Item item = items[i];
				for (int j = 0; j < shelfs.Count; j++) {
					success = shelfs[j].AddItem(
						ref item, ref width, ref height
					);
					if (success) {
						items[i] = item;
						break;
					}
				}

				// Fail to Add
				if (!success) {

					// New shelf
					Shelf s = new Shelf() {
						Y = shelfs.Count == 0 ? 0 : shelfs[shelfs.Count - 1].Y + shelfs[shelfs.Count - 1].Height,
						Width = aimSize,
						Height = items[i].Height,
						RoomHeight = new int[aimSize],
					};
					for (int j = 0; j < aimSize; j++) {
						s.RoomHeight[j] = s.Height;
					}
					shelfs.Add(s);

					// Add Again
					success = shelfs[shelfs.Count - 1].AddItem(
						ref item, ref width, ref height
					);
					items[i] = item;

					// Error, this shouldn't be happen...
					if (!success) {
						throw new System.Exception("Fail to pack textures.");
					}
				}

			}

			// Set Texture
			width = aimSize;
			height = Mathf.Max(height, aimSize);
			texture = new Texture2D(width, height, TextureFormat.ARGB32, false, false) {
				filterMode = FilterMode.Point
			};

			// Default Color
			if (defaultClear) {
				var defualtColors = new Color[width * height];
				for (int i = 0; i < defualtColors.Length; i++) {
					defualtColors[i] = Color.clear;
				}
				texture.SetPixels(defualtColors);
			}


			for (int i = 0; i < items.Count; i++) {
				texture.SetPixels(
					items[i].X,
					items[i].Y,
					items[i].Width,
					items[i].Height,
					items[i].TextureColors
				);
			}
			texture.Apply();

			// Sort with Index
			items.Sort(new ItemSorter(true));
			Rect[] uvs = new Rect[items.Count];
			for (int i = 0; i < items.Count; i++) {
				if (hasOutline) {
					uvs[i] = new Rect(
						(float)(items[i].X + 1) / width,
						(float)(items[i].Y + 1) / height,
						(float)(items[i].Width - 2) / width,
						(float)(items[i].Height - 2) / height
					);
				} else {
					uvs[i] = new Rect(
						(float)(items[i].X) / width,
						(float)(items[i].Y) / height,
						(float)(items[i].Width) / width,
						(float)(items[i].Height) / height
					);
				}
			}

			return uvs;
		}


	}
}
