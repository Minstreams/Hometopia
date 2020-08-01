namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class Core_MapGeneration {




		#region --- SUB ---


		public struct Int3 {
			public int A;
			public int B;
			public int C;
			public Int3 (int a, int b, int c) {
				A = a;
				B = b;
				C = c;
			}
		}


		[System.Serializable]
		public class Preset {


			public const int MAX_COLOR_COUNT = 31;
			public const char SEED_START = (char)1;

			public int Iteration = 3;
			public float IterationRadius = 2f;
			public float IterationLerp = 1f;
			public Vector2 GroundHeight = new Vector2(0f, 16f);
			public Vector2 WaterHeight = new Vector2(1f, 6f);
			public int SizeX = 128;
			public int SizeY = 128;
			public float GroundBump = 2;
			public float WaterBump = 1;
			public float CaveBump = 1;
			public float CaveRadius = 2f;
			public Vector2 CaveHeight = new Vector2(0f, 6f);
			public bool Island = true;
			public bool Tint = true;
			public bool Water = true;
			public bool Land = true;
			public bool Cave = true;
			public List<Color> GroundColors = new List<Color>() { Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, };
			public List<Color> GrassColors = new List<Color>() { Color.green, Color.green, };
			public Color WaterColor = Color.blue;
			public string Seeds;
			public int PerlinSeed;
			public int CaveSeed;



			public void FixGenerationValues () {//
				Iteration = Mathf.Clamp(Iteration, 1, 6);
				IterationRadius = Mathf.Clamp(IterationRadius, 0.1f, 3f);
				IterationLerp = Mathf.Clamp01(IterationLerp);
				SizeX = Mathf.Clamp(SizeX, 8, 512);
				SizeY = Mathf.Clamp(SizeY, 8, 512);
				GroundBump = Mathf.Clamp(GroundBump, 0, 10);
				WaterBump = Mathf.Clamp(WaterBump, 0, 10);
				CaveBump = Mathf.Clamp(CaveBump, 0, 10);
				CaveRadius = Mathf.Clamp(CaveRadius, 0, SizeX * 0.25f);
				GroundHeight = new Vector2(
					Mathf.Clamp(GroundHeight.x, -125, 125),
					Mathf.Clamp(GroundHeight.y, Mathf.Clamp(GroundHeight.x, -125, 125), 125)
				);
				CaveHeight = new Vector2(
					Mathf.Clamp(CaveHeight.x, 0f, GroundHeight.y),
					Mathf.Clamp(CaveHeight.y, Mathf.Clamp(CaveHeight.x, 0f, GroundHeight.y), GroundHeight.y)
				);
				WaterHeight = new Vector2(
					Mathf.Clamp(WaterHeight.x, 0, 125),
					Mathf.Clamp(WaterHeight.y, Mathf.Clamp(WaterHeight.x, 0, 125), 125)
				);
				// Seed
				if (Seeds == null || Seeds.Length != SizeX * SizeY) {
					CreateSeed();
				}
				// Color
				if (GroundColors.Count > MAX_COLOR_COUNT) {
					GroundColors.RemoveRange(0, GroundColors.Count - MAX_COLOR_COUNT);
				}
				if (GrassColors.Count > MAX_COLOR_COUNT) {
					GrassColors.RemoveRange(0, GrassColors.Count - MAX_COLOR_COUNT);
				}
				for (int i = 0; i < GroundColors.Count; i++) {
					var c = GroundColors[i];
					c.a = 1f;
					GroundColors[i] = c;
				}
				for (int i = 0; i < GrassColors.Count; i++) {
					var c = GrassColors[i];
					c.a = 1f;
					GrassColors[i] = c;
				}
				WaterColor.a = 1f;
			}



			public void CreateSeed () {
				var b = new System.Text.StringBuilder();
				for (int x = 0; x < SizeX; x++) {
					for (int y = 0; y < SizeY; y++) {
						b.Append((char)Random.Range(SEED_START, SEED_START + 256));
					}
				}
				Seeds = b.ToString();
				PerlinSeed = Random.Range(int.MinValue, int.MaxValue);
				CaveSeed = Random.Range(int.MinValue, int.MaxValue);
			}



			public void LoadFromJson (string json) {
				try {
					if (string.IsNullOrEmpty(json)) { return; }
					JsonUtility.FromJsonOverwrite(json, this);
				} catch { }
			}



			public string ToJson () {
				return JsonUtility.ToJson(this, true);
			}


		}



		#endregion




		#region --- API ---



		public static VoxelData Generate (Preset preset, System.Action<float, int> onProgress = null) {
			try {
				if (preset == null) { return VoxelData.CreateNewData(); }

				var data = VoxelData.CreateNewData();
				data.Voxels = new List<int[,,]> { };
				int sizeX = preset.SizeX;
				int sizeY = preset.SizeY;
				int height = Mathf.Max(
					Mathf.CeilToInt(preset.GroundHeight.y),
					Mathf.CeilToInt(preset.WaterHeight.y)
				);
				var voxels = new int[sizeX, height, sizeY];
				const float PERLIN_ZOOM = 10f;
				float perlinOffset = (Mathf.Abs((float)preset.PerlinSeed) / int.MaxValue) * PERLIN_ZOOM;

				// Palette
				data.Palette = new List<Color>();
				int gColorLen = preset.GroundColors.Count + preset.GrassColors.Count;
				int paletteLoopCount = 128 / (gColorLen + 1);
				for (int loop = 0; loop <= paletteLoopCount && data.Palette.Count < 256; loop++) {
					for (int i = 0; i < preset.GroundColors.Count; i++) {
						data.Palette.Add(Color.Lerp(preset.GroundColors[i], preset.GroundColors[i] * 0.618f, ((float)loop) / paletteLoopCount));
					}
					for (int i = 0; i < preset.GrassColors.Count; i++) {
						data.Palette.Add(Color.Lerp(preset.GrassColors[i], preset.GrassColors[i] * 0.618f, ((float)loop) / paletteLoopCount));
					}
					data.Palette.Add(Color.Lerp(preset.WaterColor, preset.WaterColor * 0.618f, ((float)loop) / paletteLoopCount));
				}
				for (int i = 0; data.Palette.Count < 256; i++) {
					data.Palette.Add(new Color(0.25f, 0.25f, 0.25f));
				}

				// Seeds
				var seeds = new float[sizeX, sizeY];
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						seeds[x, y] = (preset.Seeds[y * sizeX + x] - Preset.SEED_START) / 255f;
					}
				}

				// Island
				if (preset.Island) {
					for (int x = 0; x < sizeX; x++) {
						seeds[x, 0] = 0f;
						seeds[x, sizeY - 1] = 0f;
					}
					for (int y = 0; y < sizeY; y++) {
						seeds[0, y] = 0f;
						seeds[sizeX - 1, y] = 0f;
					}
				}

				// Iteration
				for (int iter = 0; iter < preset.Iteration; iter++) {
					var temp = new float[sizeX, sizeY];
					for (int x = 0; x < sizeX; x++) {
						if (onProgress != null) {
							onProgress(((float)x / (sizeX - 1) / preset.Iteration) + ((float)iter / preset.Iteration), 0);
						}
						for (int y = 0; y < sizeY; y++) {
							temp[x, y] = GetIterationValue(seeds, x, y, preset.IterationRadius, preset.IterationLerp, sizeX, sizeY);
						}
					}
					seeds = temp;
				}

				// Min Max
				float seedMin = 1f;
				float seedMax = 0f;
				int minMaxOffset = preset.Island ? (int)(preset.IterationRadius * 2f) : 0;
				for (int x = minMaxOffset; x < sizeX - minMaxOffset; x++) {
					for (int y = minMaxOffset; y < sizeY - minMaxOffset; y++) {
						seedMin = Mathf.Min(seedMin, seeds[x, y]);
					}
				}
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						seedMax = Mathf.Max(seedMax, seeds[x, y]);
					}
				}

				// Island
				if (preset.Island) {
					for (int x = 0; x < sizeX; x++) {
						seeds[x, 0] = 0f;
						seeds[x, sizeY - 1] = 0f;
					}
					for (int y = 0; y < sizeY; y++) {
						seeds[0, y] = 0f;
						seeds[sizeX - 1, y] = 0f;
					}
				}

				// Ground
				if (preset.Land && preset.GroundHeight.y > 0.01f) {
					float bumpMuti = preset.GroundBump / 5f;
					for (int x = 0; x < sizeX; x++) {
						if (onProgress != null) {
							onProgress(((float)x / (sizeX - 1) / preset.Iteration), 1);
						}
						for (int y = 0; y < sizeY; y++) {
							int h = (int)Util.Remap(
								seedMin, seedMax,
								preset.GroundHeight.x,
								preset.GroundHeight.y + 0.999f,
								seeds[x, y]
							);
							int colorIndex = Mathf.Clamp((int)(Mathf.PerlinNoise(
								(1f - Mathf.Repeat((x / (sizeX - 1f)) * bumpMuti, 0.999f)) * PERLIN_ZOOM + perlinOffset,
								(1f - Mathf.Repeat((y / (sizeY - 1f)) * bumpMuti, 0.999f)) * PERLIN_ZOOM + perlinOffset
							) * gColorLen), 0, gColorLen - 1);
							bool isGrass = colorIndex >= preset.GroundColors.Count;
							int basicGroundIndex = Mathf.Clamp(colorIndex - preset.GroundColors.Count, 0, preset.GroundColors.Count - 1);
							h = Mathf.Clamp(h, 0, height - 1);
							for (int i = 0; i < h; i++) {
								int index = colorIndex;
								if (isGrass && i < h - 1) {
									index = basicGroundIndex;
								}
								if (preset.Tint) {
									index = Mathf.RoundToInt(Mathf.Clamp(
										Util.Remap(preset.GroundHeight.x, preset.GroundHeight.y - 1f, paletteLoopCount * 0.5f, 0f, i),
										0f, paletteLoopCount - 1f
									)) * (gColorLen + 1) + index;
								}
								voxels[x, i, y] = index + 1;
							}
						}
					}

				}


				// Cave
				if (preset.Cave) {
					if (onProgress != null) {
						onProgress(0f, 2);
					}
					// Get Cave Voxel
					var caveVoxels = new int[sizeX, height, sizeY];
					float bumpMuti = preset.CaveBump / 5f;
					float cavePerlinOffset = (Mathf.Abs((float)preset.CaveSeed) / int.MaxValue) * PERLIN_ZOOM;
					float rotSpeedMuti = preset.CaveBump * 5f;

					int minHeight = Mathf.Clamp(Mathf.RoundToInt(preset.CaveHeight.x), 0, height - 1);
					int maxHeight = Mathf.Clamp(Mathf.RoundToInt(preset.CaveHeight.y), 0, height - 1);
					float posX = Mathf.Repeat(preset.CaveSeed, sizeX - 0.01f);
					float posY = Mathf.Repeat(-preset.CaveSeed, sizeY - 0.01f);
					float speedAngle = 0f;
					Vector2 speed = Vector2.up;

					for (int i = 0; i < sizeX * sizeY; i++) {

						float perlin = Mathf.Clamp01(Mathf.PerlinNoise(
							Mathf.Repeat((posX / (sizeX - 1f)) * bumpMuti, 0.999f) * PERLIN_ZOOM + cavePerlinOffset,
							Mathf.Repeat((posY / (sizeY - 1f)) * bumpMuti, 0.999f) * PERLIN_ZOOM + cavePerlinOffset
						));
						int h = (int)Util.Remap(0f, 1f, minHeight, maxHeight + 0.999f, perlin);

						int xMin = Mathf.Max(Mathf.RoundToInt(posX) - Mathf.CeilToInt(preset.CaveRadius), 0);
						int xMax = Mathf.Min(Mathf.RoundToInt(posX) + Mathf.CeilToInt(preset.CaveRadius), sizeX - 1);
						int yMin = Mathf.Max(Mathf.RoundToInt(posY) - Mathf.CeilToInt(preset.CaveRadius), 0);
						int yMax = Mathf.Min(Mathf.RoundToInt(posY) + Mathf.CeilToInt(preset.CaveRadius), sizeY - 1);
						Vector2 centerPos = new Vector2(posX, posY);
						for (int x = xMin; x <= xMax; x++) {
							for (int y = yMin; y <= yMax; y++) {
								if (Vector2.Distance(new Vector2(x, y), centerPos) <= preset.CaveRadius) {
									int z = Mathf.Clamp((int)(h - preset.CaveRadius), 0, height - 1);
									int zMax = Mathf.Clamp((int)(h + preset.CaveRadius), 0, height - 1);
									for (; z < zMax; z++) {
										caveVoxels[x, z, y] = 1;
									}
								}
							}
						}

						float seed01 = (preset.Seeds[i] - Preset.SEED_START) / 255f;
						speedAngle = Mathf.Repeat(speedAngle + Util.Remap(0, 1, -rotSpeedMuti, rotSpeedMuti, seed01), 360f);
						speed = Quaternion.Euler(0, 0, speedAngle) * speed;
						posX = Mathf.Repeat(posX + speed.x, sizeX);
						posY = Mathf.Repeat(posY + speed.y, sizeY);
					}

					data.Voxels.Add(caveVoxels);

					// Fix Into Ground
					if (preset.Land) {
						for (int x = 0; x < sizeX; x++) {
							for (int y = 0; y < sizeY; y++) {
								for (int h = 0; h < height; h++) {
									if (caveVoxels[x, h, y] > 0 && voxels[x, h, y] > 0) {
										voxels[x, h, y] = 0;
									}
								}
							}
						}
					}
				}


				// Water
				var waterStack = new Stack<Int3>();
				if (preset.Water) {
					float bumpMuti = preset.WaterBump / 5f;
					for (int x = 0; x < sizeX; x++) {
						if (onProgress != null) {
							onProgress(((float)x / (sizeX - 1) / preset.Iteration), 3);
						}
						for (int y = 0; y < sizeY; y++) {
							float perlin = Mathf.PerlinNoise(
								Mathf.Repeat((x / (sizeX - 1f)) * bumpMuti, 0.999f) * PERLIN_ZOOM + perlinOffset,
								Mathf.Repeat((y / (sizeY - 1f)) * bumpMuti, 0.999f) * PERLIN_ZOOM + perlinOffset
							);
							int h = (int)Mathf.Lerp(
								preset.WaterHeight.x,
								preset.WaterHeight.y + 0.999f,
								perlin
							);
							for (int i = 0; i < h; i++) {
								if (voxels[x, i, y] > 0) { continue; }
								int v = preset.Tint ?
									Mathf.RoundToInt(Mathf.Clamp(
										Util.Remap(preset.WaterHeight.x, preset.WaterHeight.y - 1f, paletteLoopCount * 0.5f, 0f, i),
										0f, paletteLoopCount - 1f
									)) * (gColorLen + 1) + gColorLen + 1
									: gColorLen + 1;
								voxels[x, i, y] = -v;
								waterStack.Push(new Int3(x, i, y));
							}
						}
					}

					// Fix Water Physics
					if (onProgress != null) {
						onProgress(0.5f, 4);
					}
					if (preset.Land) {
						int _a, _b, _c, _d;
						for (int safeCount = 1000000; safeCount > 0 && waterStack.Count > 0; safeCount--) {
							var pos = waterStack.Pop();
							if (voxels[pos.A, pos.B, pos.C] < 0) {
								_a = pos.A > 0 ? voxels[pos.A - 1, pos.B, pos.C] : 1;
								_b = pos.A < sizeX - 1 ? voxels[pos.A + 1, pos.B, pos.C] : 1;
								_c = pos.C > 0 ? voxels[pos.A, pos.B, pos.C - 1] : 1;
								_d = pos.C < sizeY - 1 ? voxels[pos.A, pos.B, pos.C + 1] : 1;
								if (_a == 0 || _b == 0 || _c == 0 || _d == 0) {
									voxels[pos.A, pos.B, pos.C] = 0;
									if (_a < 0) {
										waterStack.Push(new Int3(pos.A - 1, pos.B, pos.C));
									}
									if (_b < 0) {
										waterStack.Push(new Int3(pos.A + 1, pos.B, pos.C));
									}
									if (_c < 0) {
										waterStack.Push(new Int3(pos.A, pos.B, pos.C - 1));
									}
									if (_d < 0) {
										waterStack.Push(new Int3(pos.A, pos.B, pos.C + 1));
									}
								}
							}
						}
					}
					// Fix -Voxels
					for (int x = 0; x < sizeX; x++) {
						for (int y = 0; y < sizeY; y++) {
							for (int h = 0; h < height; h++) {
								if (voxels[x, h, y] < 0) {
									voxels[x, h, y] = -voxels[x, h, y];
								}
							}
						}
					}
				}


				// End
				data.Voxels.Insert(0, voxels);
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



		#endregion




		#region --- UTL ---



		private static float GetIterationValue (float[,] source, int x, int y, float iterRadius, float lerp, int sizeX, int sizeY) {
			int range = Mathf.CeilToInt(iterRadius);
			int l = Mathf.Max(x - range, 0);
			int d = Mathf.Max(y - range, 0);
			int r = Mathf.Min(x + range, sizeX - 1);
			int u = Mathf.Min(y + range, sizeY - 1);
			float sum = source[x, y];
			float count = 1;
			Vector2 center = new Vector2(x, y);
			for (int i = l; i <= r; i++) {
				for (int j = d; j <= u; j++) {
					if (i == x && j == y) { continue; }
					if (Vector2.Distance(center, new Vector2(i, j)) <= iterRadius) {
						sum += source[i, j];
						count++;
					}
				}
			}
			return Mathf.Clamp01(Mathf.Lerp(source[x, y], sum / count, lerp));
		}





		#endregion




	}
}



