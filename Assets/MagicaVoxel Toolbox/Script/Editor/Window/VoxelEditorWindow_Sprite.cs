namespace MagicaVoxelToolbox {
	using MagicaVoxelToolbox;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using MagicaVoxelToolbox.Saving;


	// Sprite Part
	public partial class VoxelEditorWindow {




		#region --- VAR ---


		// Short
		private bool IsSpriting {
			get {
				return CurrentEditorMode == EditorMode.Sprite;
			}
		}


		// Saving
		private EditorSavingInt SpriteNum25DIndex = new EditorSavingInt("VEditor.SpriteNum25DIndex", 8);
		private EditorSavingInt SpriteNum2DIndex = new EditorSavingInt("VEditor.SpriteNum2DIndex", 6);
		private EditorSavingInt SpriteNum8bitIndex = new EditorSavingInt("VEditor.SpriteNum8bitIndex", 8);
		private EditorSavingFloat Sprite25DLight = new EditorSavingFloat("VEditor.Sprite25DLight", 0.6f);
		private EditorSavingFloat Sprite2DLight = new EditorSavingFloat("VEditor.Sprite2DLight", 0.6f);
		private EditorSavingFloat Sprite8bitLight = new EditorSavingFloat("VEditor.Sprite8bitLight", 0.6f);
		private EditorSavingVector2 Sprite25DPivot = new EditorSavingVector2("VEditor.Sprite25DPivot", Vector3.one * 0.5f);
		private EditorSavingVector2 Sprite8bitPivot = new EditorSavingVector2("VEditor.Sprite8bitPivot", Vector3.one * 0.5f);
		private EditorSavingVector2 Sprite2DPivot = new EditorSavingVector2("VEditor.Sprite2DPivot", Vector2.one * 0.5f);
		private EditorSavingFloat Sprite25DCameraScale = new EditorSavingFloat("VEditor.Sprite25DCameraScale", 8f);


		#endregion



		#region --- TSK ---




		private void CreateSprite (Core_Sprite.SpriteType type) {
			if (!Data) { return; }
			string path = Util.FixPath(EditorUtility.SaveFilePanel("Select Export Path", "Assets", Util.GetNameWithoutExtension(VoxelFilePath) + type.ToString(), "png"));
			if (!string.IsNullOrEmpty(path)) {
				path = Util.FixedRelativePath(path);
				if (!string.IsNullOrEmpty(path)) {

					bool oldShow = ShowBackgroundBox;
					CubeTF.gameObject.SetActive(false);
					BoxRoot.gameObject.SetActive(false);

					var result = Core_Sprite.CreateSprite(
						Data,
						CurrentModelIndex,
						type,
						GetSpriteNum(type),
						GetSpriteLight(type),
						GetSpritePivot(type),
						Camera,
						Sprite25DCameraScale
					);

					CubeTF.gameObject.SetActive(true);
					BoxRoot.gameObject.SetActive(oldShow);

					if (result.Texture) {
						Util.ByteToFile(result.Texture.EncodeToPNG(), path);
						VoxelPostprocessor.AddSprite(path, new VoxelPostprocessor.SpriteConfig() {
							width = result.Width,
							height = result.Height,
							Pivots = result.Pivots,
							spriteRects = result.Rects,
							Names = result.NameFixes,
						});
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
						EditorApplication.delayCall += VoxelPostprocessor.ClearAsset;
					}
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
		}



		private void CreateScreenShot () {
			string path = Util.FixPath(EditorUtility.SaveFilePanel("Select Export Path", "Assets", Util.GetNameWithoutExtension(VoxelFilePath) + "_screenShot", "png"));
			if (!string.IsNullOrEmpty(path)) {
				path = Util.FixedRelativePath(path);
				if (!string.IsNullOrEmpty(path)) {
					bool oldShow = ShowBackgroundBox;
					CubeTF.gameObject.SetActive(false);
					BoxRoot.gameObject.SetActive(false);
					var texture = Util.RenderTextureToTexture2D(Camera);
					if (texture) {
						texture = Util.TrimTexture(texture, 0.01f, 12);
						Util.ByteToFile(texture.EncodeToPNG(), path);
						VoxelPostprocessor.AddScreenshot(path);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorApplication.delayCall += VoxelPostprocessor.ClearAsset;
					}
					CubeTF.gameObject.SetActive(true);
					BoxRoot.gameObject.SetActive(oldShow);
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
		}



		#endregion



		#region --- GUI ---



		private void SpriteEditingGUI () {
			if (IsSpriting) {
				LayoutV(() => {

					const int HEIGHT = 16;
					const int LABEL_WIDTH = 90;
					const int ITEM_WIDTH = 140;
					const int GAP_HEIGHT = 6;

					Space(GAP_HEIGHT);

					LayoutH(() => {

						// 2.5D
						LayoutV(() => {

							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "2.5D Sprite");
							Space(GAP_HEIGHT);

							LayoutH(() => {
								// Sprite Num
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Sprite Num");
								SpriteNum25DIndex.Value = (int)(Core_Sprite.SpriteNum)EditorGUI.EnumPopup(
									GUIRect(60, HEIGHT),
									(Core_Sprite.SpriteNum)SpriteNum25DIndex.Value
								);
							});
							Space(GAP_HEIGHT);

							// Light
							LayoutH(() => {
								float light = Sprite25DLight;
								FloatField(LABEL_WIDTH, HEIGHT, "Light", ref light);
								Sprite25DLight.Value = Mathf.Clamp01(light);
							});
							Space(GAP_HEIGHT);

							// Scale
							LayoutH(() => {
								float scale = Sprite25DCameraScale;
								FloatField(LABEL_WIDTH, HEIGHT, "Scale", ref scale);
								Sprite25DCameraScale.Value = Mathf.Clamp(scale, 1f, 10f);
							});
							Space(GAP_HEIGHT);

							// Pivot
							LayoutH(() => {
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Pivot");
								var pivot = EditorGUI.Vector2Field(GUIRect(ITEM_WIDTH, HEIGHT), "", Sprite25DPivot);
								Sprite25DPivot.Value = pivot;
							});
							Space(GAP_HEIGHT);

						}, false, new GUIStyle(GUI.skin.box) {
							fixedWidth = LABEL_WIDTH + ITEM_WIDTH + 20,
						});
						Space(GAP_HEIGHT);


						// 8bit
						LayoutV(() => {

							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "8bit Sprite");
							Space(GAP_HEIGHT);

							// Sprite Num
							LayoutH(() => {
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Sprite Num");
								SpriteNum8bitIndex.Value = (int)(Core_Sprite.SpriteNum)EditorGUI.EnumPopup(
									GUIRect(60, HEIGHT),
									(Core_Sprite.SpriteNum)SpriteNum8bitIndex.Value
								);
							});
							Space(GAP_HEIGHT);

							// Light
							LayoutH(() => {
								float light = Sprite8bitLight;
								FloatField(LABEL_WIDTH, HEIGHT, "Light", ref light);
								Sprite8bitLight.Value = Mathf.Clamp01(light);
							});
							Space(GAP_HEIGHT);

							// Pivot
							LayoutH(() => {
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Pivot");
								var pivot = EditorGUI.Vector2Field(GUIRect(ITEM_WIDTH, HEIGHT), "", Sprite8bitPivot);
								Sprite8bitPivot.Value = pivot;
							});
							Space(GAP_HEIGHT);

						}, false, new GUIStyle(GUI.skin.box) {
							fixedWidth = LABEL_WIDTH + ITEM_WIDTH + 20,
						});
						Space(GAP_HEIGHT);


						// 2D
						LayoutV(() => {

							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "2D Sprite");
							Space(GAP_HEIGHT);

							// Sprite Num
							LayoutH(() => {
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Sprite Num");
								SpriteNum2DIndex.Value = (int)(Core_Sprite.Sprite2DNum)EditorGUI.EnumPopup(
									GUIRect(60, HEIGHT),
									(Core_Sprite.Sprite2DNum)SpriteNum2DIndex.Value
								);
							});
							Space(GAP_HEIGHT);

							// Light
							LayoutH(() => {
								float light = Sprite2DLight;
								FloatField(LABEL_WIDTH, HEIGHT, "Light", ref light);
								Sprite2DLight.Value = Mathf.Clamp01(light);
							});
							Space(GAP_HEIGHT);

							// Pivot
							LayoutH(() => {
								EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Pivot");
								var pivot = EditorGUI.Vector2Field(GUIRect(ITEM_WIDTH, HEIGHT), "", Sprite2DPivot);
								Sprite2DPivot.Value = pivot;
							});
							Space(GAP_HEIGHT);


						}, false, new GUIStyle(GUI.skin.box) {
							fixedWidth = LABEL_WIDTH + ITEM_WIDTH + 20,
						});
						Space(GAP_HEIGHT);

					});

					Space(GAP_HEIGHT);
					GUIRect(0, 1);

				}, false, new GUIStyle() {
					padding = new RectOffset(9, 6, 4, 4),
					margin = new RectOffset(14, 20, 8, 4),
				});
			}
		}



		private void SpritePanelGUI () {
			const int WIDTH = 158;
			const int HEIGHT = 30;
			const int GAP = 3;

			var buttonStyle = new GUIStyle(GUI.skin.button) {
				fontSize = 11,
			};
			var dotStyle = new GUIStyle(GUI.skin.label) {
				richText = true,
				alignment = TextAnchor.MiddleLeft,
			};

			Rect rect = new Rect(
				ViewRect.x + ViewRect.width - WIDTH,
				ViewRect.y + ViewRect.height - HEIGHT - GAP,
				WIDTH,
				HEIGHT
			);

			// Spriting
			if (GUI.Button(rect, "   Create Screenshot", buttonStyle)) {
				CreateScreenShot();
				Repaint();
			}
			GUI.Label(rect, "  <color=#ffcc00>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
			if (GUI.Button(rect, "   Create 2D Sprite", buttonStyle)) {
				CreateSprite(Core_Sprite.SpriteType._2D);
				Repaint();
			}

			GUI.Label(rect, "  <color=#ffcc00>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

			if (GUI.Button(rect, "   Create 8bit Sprite", buttonStyle)) {
				CreateSprite(Core_Sprite.SpriteType._8bit);
				Repaint();
			}
			GUI.Label(rect, "  <color=#ffcc00>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

			if (GUI.Button(rect, "   Create 2.5D Sprite", buttonStyle)) {
				CreateSprite(Core_Sprite.SpriteType._25D);
				Repaint();
			}
			GUI.Label(rect, "  <color=#ffcc00>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

		}



		#endregion



		#region --- LGC ---



		private void Load_Sprite () {
			SpriteNum25DIndex.Load();
			SpriteNum2DIndex.Load();
			SpriteNum8bitIndex.Load();
			Sprite25DLight.Load();
			Sprite8bitLight.Load();
			Sprite2DLight.Load();
			Sprite25DPivot.Load();
			Sprite8bitPivot.Load();
			Sprite2DPivot.Load();
			Sprite25DCameraScale.Load();
		}



		private void Save_Sprite () {
			SpriteNum25DIndex.TrySave();
			SpriteNum2DIndex.TrySave();
			SpriteNum8bitIndex.TrySave();
			Sprite25DLight.TrySave();
			Sprite8bitLight.TrySave();
			Sprite2DLight.TrySave();
			Sprite25DPivot.TrySave();
			Sprite8bitPivot.TrySave();
			Sprite2DPivot.TrySave();
			Sprite25DCameraScale.TrySave();
		}



		private int GetSpriteNum (Core_Sprite.SpriteType type) {
			switch (type) {
				default:
				case Core_Sprite.SpriteType._8bit:
				return SpriteNum8bitIndex;
				case Core_Sprite.SpriteType._2D:
				return SpriteNum2DIndex;
				case Core_Sprite.SpriteType._25D:
				return SpriteNum25DIndex;
			}
		}



		private float GetSpriteLight (Core_Sprite.SpriteType type) {
			switch (type) {
				default:
				case Core_Sprite.SpriteType._25D:
				return Sprite25DLight;
				case Core_Sprite.SpriteType._8bit:
				return Sprite8bitLight;
				case Core_Sprite.SpriteType._2D:
				return Sprite2DLight;
			}
		}



		private Vector2 GetSpritePivot (Core_Sprite.SpriteType type) {
			switch (type) {
				default:
				case Core_Sprite.SpriteType._25D:
				return Sprite25DPivot;
				case Core_Sprite.SpriteType._8bit:
				return Sprite8bitPivot;
				case Core_Sprite.SpriteType._2D:
				return Sprite2DPivot;
			}
		}




		#endregion



	}
}