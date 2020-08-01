namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using MagicaVoxelToolbox;
	using MagicaVoxelToolbox.Saving;


	public class VoxelToUnityWindow : MoenenEditorWindow {




		#region --- SUB ---



		public struct PathData {
			public string Path;
			public string Extension;
			public string Root;
		}




		public enum Task {
			Prefab = 0,
			Lod = 1,
			Material = 2,
			Obj = 3,
			ToJson = 4,
			ToVox = 5,
			ToQb = 6,
		}




		public enum ExportMod {
			Specified = 0,
			OriginalPath = 1,
			AskEverytime = 2,
		}



		public enum PivotMod {
			Specified = 0,
			MagicaVoxelWorldMod = 1,
		}



		public enum _25DSpriteNum {
			_1 = 1,
			_4 = 4,
			_8 = 8,
			_16 = 16,
		}



		public enum FacingOption {
			PositiveZInUnity = 0,
			NegativeZInUnity = 1,
		}




		#endregion




		#region --- VAR ---



		// Global
		private static Vector2[] SHADER_VALUE_REMAP_SOURCE = new Vector2[SHADER_PROPERTY_NUM] {
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(1, 2),
			new Vector2(0, 100),
			new Vector2(0, 100),
			new Vector2(1, 5),
			new Vector2(0, 100),
		};
		private const int SHADER_NUM = VoxelData.MaterialData.SHADER_NUM;
		private const int SHADER_PROPERTY_NUM = VoxelData.MaterialData.SHADER_PROPERTY_NUM;
		private static readonly string[] SHADER_NAMES = new string[SHADER_NUM] { "Diffuse", "Metal", "Plastic", "Glass", "Emiss", };


		// Short
		private static ExportMod TheExportMod {
			get {
				return (ExportMod)ExportModIndex.Value;
			}
			set {
				ExportModIndex.Value = (int)value;
			}
		}

		private static Core_Voxel.LightMapSupportType LightMapSupportMode {
			get {
				return (Core_Voxel.LightMapSupportType)LightMapSupportTypeIndex.Value;
			}
			set {
				LightMapSupportTypeIndex.Value = (int)value;
			}
		}

		private static Shader TheDiffuseShader {
			get {
				return Shader.Find(Shader_Paths[0]);
			}
			set {
				Shader_Paths[0].Value = value ? value.name : "Mobile/Diffuse";
			}
		}

		private static Shader TheMetalShader {
			get {
				return Shader.Find(Shader_Paths[1]);
			}
			set {
				Shader_Paths[1].Value = value ? value.name : "Mobile/Diffuse";
			}
		}

		private static Shader ThePlasticShader {
			get {
				return Shader.Find(Shader_Paths[2]);
			}
			set {
				Shader_Paths[2].Value = value ? value.name : "Mobile/Diffuse";
			}
		}

		private static Shader TheGlassShader {
			get {
				return Shader.Find(Shader_Paths[3]);
			}
			set {
				Shader_Paths[3].Value = value ? value.name : "Mobile/Diffuse";
			}
		}

		private static Shader TheEmissionShader {
			get {
				return Shader.Find(Shader_Paths[4]);
			}
			set {
				Shader_Paths[4].Value = value ? value.name : "Mobile/Diffuse";
			}
		}

		private static Shader[] TheShaders {
			get {
				return new Shader[SHADER_NUM] {
					Shader.Find(Shader_Paths[0]),
					Shader.Find(Shader_Paths[1]),
					Shader.Find(Shader_Paths[2]),
					Shader.Find(Shader_Paths[3]),
					Shader.Find(Shader_Paths[4]),
				};
			}
		}

		private static string[] ShaderKeywords {
			get {
				return new string[SHADER_PROPERTY_NUM] {
					Shader_Keywords[0],
					Shader_Keywords[1],
					Shader_Keywords[2],
					Shader_Keywords[3],
					Shader_Keywords[4],
					Shader_Keywords[5],
					Shader_Keywords[6],
					Shader_Keywords[7],
					Shader_Keywords[8],
					Shader_Keywords[9],
					Shader_Keywords[10],
					Shader_Keywords[11],
					Shader_Keywords[12],
				};
			}
		}

		private static Vector2[] ShaderValueRemaps {
			get {
				return new Vector2[SHADER_PROPERTY_NUM] {
					Shader_ValueRemaps[0],
					Shader_ValueRemaps[1],
					Shader_ValueRemaps[2],
					Shader_ValueRemaps[3],
					Shader_ValueRemaps[4],
					Shader_ValueRemaps[5],
					Shader_ValueRemaps[6],
					Shader_ValueRemaps[7],
					Shader_ValueRemaps[8],
					Shader_ValueRemaps[9],
					Shader_ValueRemaps[10],
					Shader_ValueRemaps[11],
					Shader_ValueRemaps[12],
				};
			}
		}

		// Data
		private Vector2 MasterScrollPosition;
		private static bool AboutFold_Contact = false;
		private static bool AboutFold_Ad = false;

		// Selection
		private static Dictionary<Object, PathData> TaskMap = new Dictionary<Object, PathData>();
		private static int VoxNum = 0;
		private static int QbNum = 0;
		private static int FolderNum = 0;
		private static int ObjNum = 0;
		private static int JsonNum = 0;
		private static Texture2D VoxFileIcon = null;
		private static Texture2D QbFileIcon = null;
		private static Texture2D JsonFileIcon = null;


		// Saving
		private static EditorSavingBool ViewPanelOpen = new EditorSavingBool("V2U.ViewPanelOpen", true);
		private static EditorSavingBool CreatePanelOpen = new EditorSavingBool("V2U.CreatePanelOpen", true);
		private static EditorSavingBool SettingPanelOpen = new EditorSavingBool("V2U.SettingPanelOpen", false);
		private static EditorSavingBool AboutPanelOpen = new EditorSavingBool("V2U.AboutPanelOpen", false);
		private static EditorSavingBool ModelGenerationSettingPanelOpen = new EditorSavingBool("V2U.ModelGenerationSettingPanelOpen", false);
		private static EditorSavingBool OptimizationSettingPanelOpen = new EditorSavingBool("V2U.OptimizationSettingPanelOpen", false);
		private static EditorSavingBool ShaderSettingPanelOpen = new EditorSavingBool("V2U.ShaderSettingPanelOpen", false);
		private static EditorSavingBool SpriteGenerationSettingPanelOpen = new EditorSavingBool("V2U.SpriteGenerationSettingPanelOpen", false);
		private static EditorSavingBool SystemSettingPanelOpen = new EditorSavingBool("V2U.SystemSettingPanelOpen", false);
		private static EditorSavingBool ToolPanelOpen = new EditorSavingBool("V2U.ToolPanelOpen", true);
		private static EditorSavingBool ColorfulTitle = new EditorSavingBool("V2U.ColorfulTitle", true);
		private static EditorSavingString ExportPath = new EditorSavingString("V2U.ExportPath", "Assets");
		private static EditorSavingInt LightMapSupportTypeIndex = new EditorSavingInt("V2U.LightMapSupportTypeIndex", 0);
		private static EditorSavingInt ExportModIndex = new EditorSavingInt("V2U.ExportModIndex", 2);
		private static EditorSavingFloat ModelScale = new EditorSavingFloat("V2U.ModelScale", 0.1f);
		private static EditorSavingInt LodNum = new EditorSavingInt("V2U.LodNum", 2);
		private static EditorSavingBool LogMessage = new EditorSavingBool("V2U.LogMessage", true);
		private static EditorSavingBool ShowDialog = new EditorSavingBool("V2U.ShowDialog", true);
		private static EditorSavingBool EditorDockToScene = new EditorSavingBool("V2U.EditorDockToScene", true);
		private static EditorSavingVector3 ModelPivot = new EditorSavingVector3("V2U.ModelPivot", Vector3.one * 0.5f);
		private static EditorSavingBool OptimizeFront = new EditorSavingBool("V2U.OptimizeFront", true);
		private static EditorSavingBool OptimizeBack = new EditorSavingBool("V2U.OptimizeBack", true);
		private static EditorSavingBool OptimizeUp = new EditorSavingBool("V2U.OptimizeUp", true);
		private static EditorSavingBool OptimizeDown = new EditorSavingBool("V2U.OptimizeDown", true);
		private static EditorSavingBool OptimizeLeft = new EditorSavingBool("V2U.OptimizeLeft", true);
		private static EditorSavingBool OptimizeRight = new EditorSavingBool("V2U.OptimizeRight", true);
		private static EditorSavingBool FacingYPlusInMV = new EditorSavingBool("V2U.FacingYPlusInMV", true);
		private static EditorSavingInt CurrentShowingShaderIndex = new EditorSavingInt("V2U.CurrentShowingShaderIndex", 0);
		private static EditorSavingInt ReplacementModeIndex = new EditorSavingInt("V2U.ReplacementModeIndex", 0);
		private static EditorSavingString ShaderMainTextureKeyword = new EditorSavingString("V2U.ShaderMainTextureKeyword", "_MainTex");// Keyword Using in VEditor
		private static EditorSavingString[] Shader_Paths = new EditorSavingString[5] {
			new EditorSavingString("V2U.ShaderPath_Diffuse", "Mobile/Diffuse"),
			new EditorSavingString("V2U.ShaderPath_Matel", "Standard"),
			new EditorSavingString("V2U.ShaderPath_Plastic", "Standard"),
			new EditorSavingString("V2U.ShaderPath_Glass", "Sprites/Default"),
			new EditorSavingString("V2U.ShaderPath_Emission", "Sprites/Default"),
		};
		private static EditorSavingString[] Shader_Keywords = new EditorSavingString[SHADER_PROPERTY_NUM] {
			new EditorSavingString("V2U.ShaderKeyword_MetalWeight", "$c_Color"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_MetalRough", "_Metallic"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_MetalSpecular",""),   // 0-100 0-1

			new EditorSavingString("V2U.ShaderKeyword_PlasticWeight","$c_Color"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_PlasticRough","_Metallic"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_PlasticSpecular",""), // 0-100 0-1

			new EditorSavingString("V2U.ShaderKeyword_GlassWeight","$a_Color"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_GlassRough",""),			// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_GlassRefract",""),		// 1-2   0-1
			new EditorSavingString("V2U.ShaderKeyword_GlassAttenuate",""), // 0-100 0-1
			 
			new EditorSavingString("V2U.ShaderKeyword_EmitWeight","$c_Color"),		// 0-100 0-1
			new EditorSavingString("V2U.ShaderKeyword_EmitPower",""),			// 1-5   0-4
			new EditorSavingString("V2U.ShaderKeyword_EmitLdr",""),				// 0-100 0-1
		};
		private static EditorSavingVector2[] Shader_ValueRemaps = new EditorSavingVector2[SHADER_PROPERTY_NUM] {
			new EditorSavingVector2("V2U.ShaderValueRemap_MetalWeight", new Vector2(0.5f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_MetalRough", new Vector2(0f, 0.8f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_MetalSpecular", new Vector2(0f, 0.8f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_PlasticWeight", new Vector2(0.5f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_PlasticRough", new Vector2(0f, 0.2f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_PlasticSpecular", new Vector2(0f, 0.2f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_GlassWeight", new Vector2(0.8f, 0.3f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_GlassRough", new Vector2(0f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_GlassRefract", new Vector2(0f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_GlassAttenuate", new Vector2(0f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_EmitWeight", new Vector2(0.5f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_EmitPower", new Vector2(0f, 1f)),
			new EditorSavingVector2("V2U.ShaderValueRemap_EmitLdr", new Vector2(0f, 1f)),
		};


		#endregion




		#region --- MSG ---



		[MenuItem("Tools/MagicaVoxel Toolbox/Toolbox")]
		public static void OpenWindow () {
			var inspector = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			VoxelToUnityWindow window = inspector != null ?
				GetWindow<VoxelToUnityWindow>("Voxel To Unity", true, inspector) :
				GetWindow<VoxelToUnityWindow>("Voxel To Unity", true);
			window.minSize = new Vector2(275, 400);
			window.maxSize = new Vector2(600, 1000);
		}



		private void OnEnable () {
			Window_Enable();
		}



		private void OnFocus () {
			RefreshSelection();
			Repaint();
		}



		private void OnSelectionChange () {
			RefreshSelection();
			Repaint();
		}



		private void OnGUI () {

			MasterScrollPosition = GUILayout.BeginScrollView(MasterScrollPosition, GUI.skin.scrollView);

			TitleGUI();

			Window_Main();

			GUILayout.EndScrollView();

			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl(null);
				Repaint();
			}

		}



		private void TitleGUI () {

			const string MAIN_TITLE = "Voxel to Unity";
			const string MAIN_TITLE_RICH = "<color=#ff3333>V</color><color=#ffcc00>o</color><color=#ffff33>x</color><color=#33ff33>e</color><color=#33ccff>l</color><color=#eeeeee> to Unity</color>";

			Space(6);
			LayoutV(() => {
				GUIStyle style = new GUIStyle() {
					alignment = TextAnchor.LowerCenter,
					fontSize = 12,
					fontStyle = FontStyle.Bold
				};
				style.normal.textColor = Color.white;
				style.richText = true;
				Rect rect = GUIRect(0, 18);

				GUIStyle shadowStyle = new GUIStyle(style) {
					richText = false
				};

				EditorGUI.DropShadowLabel(rect, MAIN_TITLE, shadowStyle);
				GUI.Label(rect, ColorfulTitle ? MAIN_TITLE_RICH : MAIN_TITLE, style);

			});
			Space(6);
		}



		#endregion




		#region --- API ---



		public static void Window_Enable () {
			LoadSetting();
			RefreshSelection();
			RefreshMergeSetting();
		}



		public static void Window_Main () {

			ViewGUI();
			CreateGUI();
			ToolGUI();
			SettingGUI();
			AboutGUI();

			if (GUI.changed) {
				SaveSetting();
			}

		}




		#endregion




		#region --- GUI ---



		private static void ViewGUI () {

			LayoutF(() => {
				bool addSpaceFlag = true;
				Space(2);
				int iconSize = 26;

				LayoutH(() => {

					// Init
					GUIStyle labelStyle = new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleLeft,
						fontSize = 10
					};

					// Icons
					if (FolderNum > 0) {
						if (VoxNum + QbNum + JsonNum <= 0) {
							// None With Folder
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "There are NO .vox or .qb file in selecting folder.", MessageType.Warning);
							addSpaceFlag = false;
						}
					} else if (VoxNum + QbNum + JsonNum <= 0) {
						if (ObjNum > 0) {
							// Selecting Not Voxel File
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "The file selecting is NOT .vox or .qb file.", MessageType.Warning);
							addSpaceFlag = false;
						} else {
							// None
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "Select *.vox, *.qb, *.json or folder in Project View.", MessageType.Info);
							addSpaceFlag = false;
						}
					}

					Space(4);

					if (VoxNum > 0) {
						// Vox
						LayoutH(() => {
							if (VoxFileIcon) {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), VoxFileIcon);
							}
						}, true);
						GUI.Label(GUIRect(0, iconSize), ".vox\n× " + VoxNum.ToString(), labelStyle);
						Space(4);
					}

					if (QbNum > 0) {
						// Qb
						LayoutH(() => {
							if (QbFileIcon) {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), QbFileIcon);
							}
						}, true);
						GUI.Label(GUIRect(0, iconSize), ".qb\n× " + QbNum.ToString(), labelStyle);
					}

					if (JsonNum > 0) {
						// Json
						LayoutH(() => {
							if (JsonFileIcon) {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), JsonFileIcon);
							}
						}, true);
						GUI.Label(GUIRect(0, iconSize), ".json\n× " + JsonNum.ToString(), labelStyle);
					}

				});

				Space(addSpaceFlag ? 16 : 6);

				// Scale Too Small Warning
				if (ModelScale == 0) {
					EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "Model scale has been set to 0. Your model will be invisible.", MessageType.Error);
					Space(6);
				} else if (ModelScale <= 0.0001f) {
					EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "Model scale is too small. Your may not able to see them.", MessageType.Warning);
					Space(6);
				}

				// Combine Warning
				if (!OptimizeFront || !OptimizeBack || !OptimizeLeft || !OptimizeRight || !OptimizeUp || !OptimizeDown) {
					EditorGUI.HelpBox(GUIRect(0, iconSize + 14), "Faces in some direction will NOT be combine.\nSee \"Setting\" > \"Optimization\".", MessageType.Info);
					Space(6);
				}

			}, "Selecting Files", ViewPanelOpen, true);

		}



		private static void CreateGUI () {
			LayoutF(() => {
				bool oldEnable = GUI.enabled;
				GUI.enabled = VoxNum > 0 || QbNum > 0;
				int buttonHeight = 34;
				string s = VoxNum + QbNum > 1 ? "s" : "";

				var dotStyle = new GUIStyle(GUI.skin.label) {
					richText = true,
					alignment = TextAnchor.MiddleLeft,
				};

				Space(6);

				Rect rect = new Rect();
				if (GUI.Button(rect = GUIRect(0, buttonHeight), "Create Prefab" + s)) {
					// Create Prefab
					DoTask(Task.Prefab);
				}
				GUI.Label(rect, GUI.enabled ? "   <color=#33ccff>●</color>" : "", dotStyle);

				Space(4);
				if (GUI.Button(rect = GUIRect(0, buttonHeight), "Create LOD Prefab" + s)) {
					// Create LOD Prefab
					DoTask(Task.Lod);
				}
				GUI.Label(rect, GUI.enabled ? "   <color=#33ccff>●</color>" : "", dotStyle);

				Space(4);
				bool oldE = GUI.enabled;
				GUI.enabled = oldE && VoxNum > 0;
				if (GUI.Button(rect = GUIRect(0, buttonHeight), "Create Material Prefab" + s)) {
					// Create Material Prefab
					DoTask(Task.Material);
				}
				GUI.Label(rect, GUI.enabled ? "   <color=#33ccff>●</color>" : "", dotStyle);
				GUI.enabled = oldE;

				Space(4);
				if (GUI.Button(rect = GUIRect(0, buttonHeight), "Create Obj File" + s)) {
					// Create Obj File
					DoTask(Task.Obj);
				}
				GUI.Label(rect, GUI.enabled ? "   <color=#33ff66>●</color>" : "", dotStyle);


				Space(4);
				LayoutH(() => {

					GUI.enabled = VoxNum > 0 || QbNum > 0;
					if (GUI.Button(rect = GUIRect(0, buttonHeight), "   To Json")) {
						// To Json
						DoTask(Task.ToJson);
					}
					GUI.Label(rect, GUI.enabled ? "   <color=#cccccc>●</color>" : "", dotStyle);
					Space(2);

					GUI.enabled = JsonNum > 0 || QbNum > 0;
					if (GUI.Button(rect = GUIRect(0, buttonHeight), "  To Vox")) {
						// To Vox
						DoTask(Task.ToVox);
					}
					GUI.Label(rect, GUI.enabled ? "   <color=#cc66ff>●</color>" : "", dotStyle);
					Space(2);

					GUI.enabled = JsonNum > 0 || VoxNum > 0;
					if (GUI.Button(rect = GUIRect(0, buttonHeight), " To Qb")) {
						// To Qb
						DoTask(Task.ToQb);
					}
					GUI.Label(rect, GUI.enabled ? "   <color=#cc66ff>●</color>" : "", dotStyle);

				});

				Space(6);

				// Export To
				LayoutV(() => {
					GUI.enabled = true;

					Space(4);
					LayoutH(() => {
						GUI.Label(GUIRect(0, 18), "Export To:");
						TheExportMod = (ExportMod)EditorGUI.EnumPopup(GUIRect(110, 18), TheExportMod);
					});

					if (TheExportMod == ExportMod.Specified) {
						Space(4);
						LayoutH(() => {
							Space(6);
							EditorGUI.SelectableLabel(GUIRect(0, 18), ExportPath, GUI.skin.textField);
							if (GUI.Button(GUIRect(60, 18), "Browse", EditorStyles.miniButtonMid)) {
								BrowseExportPath();
							}
						});
						Space(2);
					}
					Space(2);

					GUI.enabled = oldEnable;
				}, true);

				Space(4);

				GUI.enabled = oldEnable;
			}, "Create", CreatePanelOpen, true);
		}



		private static void ToolGUI () {
			LayoutF(() => {


				var dotStyle = new GUIStyle(GUI.skin.label) {
					fontSize = 9,
					richText = true,
					alignment = TextAnchor.MiddleLeft,
				};


				// Rigging Editor
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Rigging Editor")) {
						VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.Rigging, EditorDockToScene);
					}
					GUI.Label(rect, "   <color=#33ccff>●</color>", dotStyle);
					GUIRect(16, 26);
				});

				Space(4);

				// Skeletal Animation Editor
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Skeletal Animation Editor")) {
						var window = VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.Skeletal, EditorDockToScene);
						if (window) {
							window.OpenSkeletal();
						}
					}
					GUI.Label(rect, "   <color=#33cccc>●</color>", dotStyle);
					GUIRect(16, 26);
				});

				Space(4);

				// Sprite Editor
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Sprite Editor")) {
						VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.Sprite, EditorDockToScene);
					}
					GUI.Label(rect, "   <color=#ffcc00>●</color>", dotStyle);
					GUIRect(16, 26);
				});

				Space(4);

				// Map Generator
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Map Generator")) {
						var window = VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.MapGenerator, EditorDockToScene);
						if (window) {
							window.OpenGenerator();
						}
					}
					GUI.Label(rect, "   <color=#cc66ff>●</color>", dotStyle);
					GUIRect(16, 26);
				});
				Space(4);


				// Character Generator
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Character Generator")) {
						var window = VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.CharacterGenerator, EditorDockToScene);
						if (window) {
							window.OpenGenerator();
						}
					}
					GUI.Label(rect, "   <color=#cc66ff>●</color>", dotStyle);
					GUIRect(16, 26);
				});
				Space(4);


				// Prefab Combiner
				LayoutH(() => {
					GUIRect(16, 26);
					Rect rect = new Rect();
					if (GUI.Button(rect = GUIRect(0, 28), " Prefab Combiner")) {
						var window = VoxelEditorWindow.OpenWindow(VoxelEditorWindow.EditorMode.PrefabCombiner, EditorDockToScene);
						if (window) {
							window.OpenCombiner();
						}
					}
					GUI.Label(rect, "   <color=#33ccff>●</color>", dotStyle);
					GUIRect(16, 26);
				});
				Space(4);



				Space(4);

			}, "Tools", ToolPanelOpen, true);
		}



		private static void SettingGUI () {
			LayoutF(() => {

				const int HEIGHT = 16;

				// Model Generation
				LayoutF(() => {

					Space(2);

					// Pivot
					LayoutH(() => {
						EditorGUI.LabelField(GUIRect(48, 18), "Pivot");
						ModelPivot.Value = EditorGUI.Vector3Field(GUIRect(0, 18), "", ModelPivot);
					});
					Space(2);

					// Scale
					ModelScale.Value = Mathf.Max(EditorGUI.FloatField(GUIRect(0, HEIGHT), "Scale (unit/voxel)", ModelScale), 0f);
					Space(4);

					// LOD
					LodNum.Value = Mathf.Clamp(EditorGUI.IntField(GUIRect(0, HEIGHT), "LOD Num", LodNum), 2, 4);
					Space(4);

					// Facing Y+
					FacingYPlusInMV.Value = (FacingOption)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), "Y+ in MagicaVoxel is", (FacingOption)(FacingYPlusInMV.Value ? 0 : 1)) == 0;
					Space(4);

					// LightMapSupportType
					LightMapSupportMode = (Core_Voxel.LightMapSupportType)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), "Lightmap", LightMapSupportMode);
					Space(4);

					// Replacement Mode
					ReplacementModeIndex.Value = (int)(Core_File.ReplacementMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), new GUIContent("Object Replacement", "Only for sub objects inside prefab"), (Core_File.ReplacementMode)ReplacementModeIndex.Value);
					Space(4);

				}, "Model Generation", ModelGenerationSettingPanelOpen, true);


				// Optimization
				LayoutF(() => {
					LayoutH(() => {
						OptimizeLeft.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine X-", OptimizeLeft);
						Space(2);
						OptimizeRight.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine X+", OptimizeRight);
					});
					Space(2);
					LayoutH(() => {
						OptimizeDown.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine Y-", OptimizeDown);
						Space(2);
						OptimizeUp.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine Y+", OptimizeUp);
					});
					Space(2);
					LayoutH(() => {
						OptimizeBack.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine Z-", OptimizeBack);
						Space(2);
						OptimizeFront.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), "Combine Z+", OptimizeFront);
					});
					Space(6);
					EditorGUI.HelpBox(
						GUIRect(0, 32),
						"The check boxes above will make faces only combines on specified directions.",
						MessageType.Info
					);
					Space(6);
				}, "Optimization", OptimizationSettingPanelOpen, true);


				// Shaders
				LayoutF(() => {

					// Bar
					LayoutH(() => {
						for (int i = 0; i < SHADER_NUM; i++) {
							var style = i == 0 ? EditorStyles.miniButtonLeft : i == SHADER_NUM - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
							if (CurrentShowingShaderIndex == i) {
								style = new GUIStyle(style) {
									normal = style.active,
								};
							}
							if (GUI.Button(GUIRect(0, 16), SHADER_NAMES[i], style)) {
								CurrentShowingShaderIndex.Value = i;
								GUI.FocusControl(null);
							}
						}
					});
					Space(4);

					switch (CurrentShowingShaderIndex) {

						default:
						case 0:
							// Diffuse
							LayoutV(() => {
								Space(2);
								TheDiffuseShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16), "Diffuse Shader", TheDiffuseShader, typeof(Shader), false);
								Space(2);
								ShaderMainTextureKeyword.Value = EditorGUI.TextField(GUIRect(0, 16), "MainText Keyword", ShaderMainTextureKeyword.Value);
								Space(2);
							}, true);
							Space(2);
							break;

						case 1:
							// Metal
							LayoutV(() => {
								Space(2);
								TheMetalShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16), "Metal Shader", TheMetalShader, typeof(Shader), false);

								// Prop
								ShaderPropertyGUI(-1, "");
								Space(2);
								ShaderPropertyGUI(0, "Weight");
								Space(6);
								ShaderPropertyGUI(1, "Rough");
								Space(6);
								ShaderPropertyGUI(2, "Specular");
								Space(6);

							}, true);
							Space(2);
							break;

						case 2:
							// Plastic
							LayoutV(() => {
								Space(2);
								ThePlasticShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16), "Plastic Shader", ThePlasticShader, typeof(Shader), false);

								// Prop
								ShaderPropertyGUI(-1, "");
								Space(2);
								ShaderPropertyGUI(3, "Weight");
								Space(6);
								ShaderPropertyGUI(4, "Rough");
								Space(6);
								ShaderPropertyGUI(5, "Specular");
								Space(6);

							}, true);
							Space(2);
							break;

						case 3:
							// Glass
							LayoutV(() => {
								Space(2);
								TheGlassShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16), "Glass Shader", TheGlassShader, typeof(Shader), false);

								// Prop
								ShaderPropertyGUI(-1, "");
								Space(2);
								ShaderPropertyGUI(6, "Weight");
								Space(6);
								ShaderPropertyGUI(7, "Rough");
								Space(6);
								ShaderPropertyGUI(8, "Refract");
								Space(6);
								ShaderPropertyGUI(9, "Attenuate");
								Space(6);

							}, true);
							Space(2);
							break;

						case 4:
							// Emission
							LayoutV(() => {
								Space(2);
								TheEmissionShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16), "Emission Shader", TheEmissionShader, typeof(Shader), false);

								// Prop
								ShaderPropertyGUI(-1, "");
								Space(2);
								ShaderPropertyGUI(10, "Weight");
								Space(6);
								ShaderPropertyGUI(11, "Power");
								Space(6);
								ShaderPropertyGUI(12, "LDR");
								Space(6);

							}, true);
							Space(2);
							break;
					}

					if (CurrentShowingShaderIndex != 0) {
						Space(2);
						EditorGUI.HelpBox(
							GUIRect(0, 72),
							"Add tag for keyword will make it represent color. No tag means float.\n" +
							"\"$a\" = alpha \"$r\" = r \"$g\" = g\n\"$b\" = b \"$c\" = rgb\n eg. \"$aTint\", \"$r_Color\", \"FloatValue\"",
							MessageType.Info
						);
						Space(6);
					}

				}, "Shader", ShaderSettingPanelOpen, true);


				// System
				LayoutF(() => {
					Space(2);
					LayoutH(() => {
						LogMessage.Value = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), LogMessage);
						GUI.Label(GUIRect(0, 18), "Log To Console");
						Space(2);
						ShowDialog.Value = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), ShowDialog);
						GUI.Label(GUIRect(0, 18), "Dialog Window");
					});
					Space(2);
					LayoutH(() => {
						ColorfulTitle.Value = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), ColorfulTitle);
						GUI.Label(GUIRect(0, 18), "Colorful Title");
						EditorDockToScene.Value = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), EditorDockToScene);
						GUI.Label(GUIRect(0, 18), "Dock Editor To Scene");
					});
					Space(2);
				}, "System", SystemSettingPanelOpen, true);



			}, "Setting", SettingPanelOpen, true);
		}



		private static void AboutGUI () {
			LayoutF(() => {

				// Content
				LayoutV(() => {
					GUI.Label(GUIRect(0, 18), "MagicaVoxel Toolbox II.");
					GUI.Label(GUIRect(0, 18), "Developed by 楠瓜Moenen.");
					Link(0, 18, "Give it ★★★★★.", @"http://u3d.as/tWS");
				}, true);
				Space(2);

				// Links
				LayoutF(() => {
					LayoutH(() => {
						GUI.Label(GUIRect(90, 18), "Twitter");
						Link(0, 18, "@_Moenen", @"https://twitter.com/_Moenen");
					});

					LayoutH(() => {
						GUI.Label(GUIRect(90, 18), "QQ");
						Link(0, 18, "1182032752", @"tencent://message/?Menu=yes&uin=1182032752&Service=300&sigT=45a1e5847943b64c6ff3990f8a9e644d2b31356cb0b4ac6b24663a3c8dd0f8aa12a595b1714f9d45");
					});

					LayoutH(() => {
						GUI.Label(GUIRect(90, 18), "Unity Store");
						Link(0, 18, "Moenen", @"https://assetstore.unity.com/publishers/15506");
					});

					LayoutH(() => {
						GUI.Label(GUIRect(90, 18), "Email");
						Link(0, 18, "moenenn@163.com", @"mailto:moenenn@163.com");
					});

					LayoutH(() => {
						GUI.Label(GUIRect(90, 18), "Google Photo");
						Link(0, 18, "Voxel Art", @"https://goo.gl/photos/cPpgGXN6PaHQsf6K8");
					});
				}, "Contact", ref AboutFold_Contact, true);
				Space(2);

				// AD
				LayoutF(() => {
					GUI.Label(GUIRect(0, 18), "Free Assets:");
					Space(2);
					LayoutH(() => {
						Link(74, 18, "Santa Claus", @"http://u3d.as/u5V");
						Space(8);
						Link(60, 18, "Pixel Man", @"http://u3d.as/XvH");
						Space(8);
						Link(68, 18, "Hierponent", @"http://u3d.as/C3X");
					});

					Space(4);
					GUI.Label(GUIRect(0, 18), "Voxel Assets:");
					Space(2);
					LayoutH(() => {
						Link(68, 18, "Character", @"http://u3d.as/w5V");
						Space(8);
						Link(80, 18, "Environment", @"http://u3d.as/w5X");
						Space(8);
						Link(40, 18, "Props", @"http://u3d.as/w64");
					});

					LayoutH(() => {
						Link(68, 18, "Vegetation", @"http://u3d.as/wa0");
						Space(8);
						Link(46, 18, "Vehicle", @"http://u3d.as/wa1");
						Space(8);
						Link(40, 18, "Tanks", @"http://u3d.as/DTX");
					});

					LayoutH(() => {
						Link(68, 18, "Spaceships", @"http://u3d.as/E8d");
						Space(8);
						Link(46, 18, "Turrets", @"http://u3d.as/GbL");
						Space(8);
						Link(46, 18, "Robots", @"http://u3d.as/MKK");
					});

					LayoutH(() => {
						Link(46, 18, "Blocks", @"http://u3d.as/Nha");
						Space(8);
						Link(50, 18, "Particles", @"http://u3d.as/Mq0");
					});


					Space(4);
					GUI.Label(GUIRect(0, 18), "Pixel Assets:");
					LayoutH(() => {
						Link(68, 18, "Character", @"http://u3d.as/Tjd");
						Space(8);
						Link(80, 18, "Environment", @"http://u3d.as/Tjg");
						Space(8);
						Link(40, 18, "Props", @"http://u3d.as/Tjh");
					});

					LayoutH(() => {
						Link(68, 18, "Vegetation", @"http://u3d.as/Tjj");
						Space(8);
						Link(46, 18, "Vehicle", @"http://u3d.as/Tjo");
						Space(8);
						Link(50, 18, "Particles", @"http://u3d.as/1hbh");
					});

					LayoutH(() => {
						Link(68, 18, "Poker Card", @"http://u3d.as/1kWc");
					});


					Space(4);
					GUI.Label(GUIRect(0, 18), "Toolkit:");
					LayoutH(() => {
						Link(120, 18, "Fleck Map Generator", @"http://u3d.as/Nfa");
						Space(8);
						Link(80, 18, "CMD Gomoku", @"http://u3d.as/14yU");
					});

					LayoutH(() => {
						Link(100, 18, "Kaleidoscope UI", @"http://u3d.as/1a6E");
						Space(8);
						Link(110, 18, "Simple Map Editor", @"http://u3d.as/AAw");
					});

					LayoutH(() => {
						Link(80, 18, "uGUI Plus", @"http://u3d.as/Yje");
						Space(8);
						Link(130, 18, "2.5D Sprite Converter", @"http://u3d.as/GTd");
					});

					LayoutH(() => {
						Link(110, 18, "Cyan Level Editor", @"http://u3d.as/FuS");
						Space(8);
						Link(100, 18, "Simple 2D Shape", @"http://u3d.as/CEg");
					});

					LayoutH(() => {
						Link(120, 18, "Character Movement Pro", @"http://u3d.as/15TG");
						Space(8);
						Link(90, 18, "U File Browser", @"http://u3d.as/iyn");
					});


				}, "More", ref AboutFold_Ad, true);

			}, "About", AboutPanelOpen, true);
		}



		#endregion




		#region --- TSK ---




		private static void DoTask (Task task) {

			if (TaskMap.Count == 0) { return; }

			if (TheExportMod == ExportMod.AskEverytime && !BrowseExportPath()) { return; }

			RefreshMergeSetting();

			string failMessage = "[Voxel] Failed to create model for {0} model{1}.";
			int successedNum = 0;
			int failedNum = 0;
			int taskCount = TaskMap.Count;
			bool useLOD = task == Task.Lod;
			bool useMaterial = task == Task.Material;
			var resultList = new List<Core_Voxel.Result>();
			Util.ProgressBar("Creating", "Starting task...", 0f);
			Util.StartWatch();
			ForAllSelection((pathData) => {

				try {
					string fileName = Util.GetNameWithoutExtension(pathData.Path);

					Util.ProgressBar("Creating", string.Format("[{1}/{2}] Creating {0}", fileName, successedNum + failedNum + 1, taskCount), (float)(successedNum + failedNum + 1) / (taskCount + 1));

					VoxelData voxelData = null;
					switch (task) {
						case Task.Prefab:
						case Task.Lod:
						case Task.Material:
						case Task.Obj:
							// Model
							if (pathData.Extension == ".vox" || pathData.Extension == ".qb") {
								voxelData = VoxelFile.GetVoxelData(
									Util.FileToByte(pathData.Path),
									pathData.Extension == ".vox"
								);

								// Rotate 180 in Y-Axis (No Rig Data)
								if (!FacingYPlusInMV) {
									voxelData.Rotate180InY();
								}

								// Create Result
								var result = Core_Voxel.CreateLodModel(
									voxelData,
									ModelScale,
									useLOD ? LodNum : 1,
									useMaterial,
									LightMapSupportMode,
									ModelPivot
								);
								if (TheExportMod == ExportMod.OriginalPath) {
									result.ExportRoot = Util.GetParentPath(pathData.Path);
									result.ExportSubRoot = "";
								} else {
									result.ExportRoot = ExportPath;
									result.ExportSubRoot = pathData.Root;
								}
								result.FileName = fileName;
								result.Extension = task == Task.Obj ? ".obj" : ".prefab";
								result.IsRigged = false;
								result.WithAvatar = false;
								resultList.Add(result);
							}
							break;
						case Task.ToJson:
							if (pathData.Extension == ".vox" || pathData.Extension == ".qb") {
								// Voxel To Json
								voxelData = VoxelFile.GetVoxelData(Util.FileToByte(pathData.Path), pathData.Extension == ".vox");
								var json = Core_Voxel.VoxelToJson(voxelData);
								string path = TheExportMod == ExportMod.OriginalPath ?
									Util.ChangeExtension(pathData.Path, ".json") :
									Util.CombinePaths(ExportPath, pathData.Root, fileName + ".json");
								Util.CreateFolder(Util.GetParentPath(path));
								Util.Write(json, path);
							}
							break;
						case Task.ToVox:
						case Task.ToQb:
							// Json To Voxel
							string aimEx = task == Task.ToVox ? ".vox" : ".qb";
							if (pathData.Extension == ".json") {
								voxelData = Core_Voxel.JsonToVoxel(Util.Read(pathData.Path));
							} else if (pathData.Extension == ".vox" || pathData.Extension == ".qb") {
								if (aimEx != pathData.Extension) {
									voxelData = VoxelFile.GetVoxelData(Util.FileToByte(pathData.Path), pathData.Extension == ".vox");
								}
							}
							if (voxelData) {
								string aimPath = TheExportMod == ExportMod.OriginalPath ?
									Util.ChangeExtension(pathData.Path, aimEx) :
									Util.CombinePaths(ExportPath, pathData.Root, fileName + aimEx);
								Util.ByteToFile(
									VoxelFile.GetVoxelByte(voxelData, task == Task.ToVox),
									aimPath
								);
							}
							break;
					}
					successedNum++;
				} catch (System.Exception ex) {
					failMessage += "\n[path] " + pathData.Path + "\n" + ex.Message;
					failedNum++;
				}

			});

			// File
			try {

				Core_File.CreateFileForResult(
					resultList,
					useMaterial ? TheShaders : new Shader[1] { TheDiffuseShader },
					ShaderKeywords,
					ShaderValueRemaps,
					ModelScale,
					ModelPivot,
					(Core_File.ReplacementMode)ReplacementModeIndex.Value,
					ShaderMainTextureKeyword.Value
				);

				double taskTime = Util.StopWatchAndGetTime();

				// Log Messages
				if (successedNum > 0) {
					string msg = string.Format("[Voxel] {0} model{1} created in {2}sec.", successedNum, (successedNum > 1 ? "s" : ""), taskTime.ToString("0.00"));
					if (LogMessage) {
						Debug.Log(msg);
					}
					if (ShowDialog) {
						Util.Dialog("Success", msg, "OK");
					}
				}
				if (failedNum > 0) {
					string msg = string.Format(failMessage, failedNum.ToString(), (failedNum > 1 ? "s" : ""));
					if (LogMessage) {
						Debug.LogWarning(msg);
					}
					if (ShowDialog) {
						Util.Dialog("Warning", msg, "OK");
					}
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex.Message);
			}

			Util.ClearProgressBar();

		}




		#endregion




		#region --- LGC ---



		private static void LoadSetting () {
			ViewPanelOpen.Load();
			CreatePanelOpen.Load();
			SettingPanelOpen.Load();
			AboutPanelOpen.Load();
			ModelGenerationSettingPanelOpen.Load();
			SpriteGenerationSettingPanelOpen.Load();
			SystemSettingPanelOpen.Load();
			ToolPanelOpen.Load();
			ColorfulTitle.Load();
			ExportPath.Load();
			ExportModIndex.Load();
			ModelScale.Load();
			LodNum.Load();
			LogMessage.Load();
			ShowDialog.Load();
			LightMapSupportTypeIndex.Load();
			OptimizationSettingPanelOpen.Load();
			ShaderSettingPanelOpen.Load();
			OptimizeFront.Load();
			OptimizeBack.Load();
			OptimizeUp.Load();
			OptimizeDown.Load();
			OptimizeLeft.Load();
			OptimizeRight.Load();
			EditorDockToScene.Load();
			ModelPivot.Load();
			FacingYPlusInMV.Load();
			CurrentShowingShaderIndex.Load();
			ReplacementModeIndex.Load();
			ShaderMainTextureKeyword.Load();
			for (int i = 0; i < Shader_Paths.Length; i++) {
				Shader_Paths[i].Load();
			}
			for (int i = 0; i < Shader_Keywords.Length; i++) {
				Shader_Keywords[i].Load();
			}
			for (int i = 0; i < Shader_ValueRemaps.Length; i++) {
				Shader_ValueRemaps[i].Load();
			}
		}



		private static void SaveSetting () {
			ViewPanelOpen.TrySave();
			CreatePanelOpen.TrySave();
			SettingPanelOpen.TrySave();
			AboutPanelOpen.TrySave();
			ModelGenerationSettingPanelOpen.TrySave();
			SpriteGenerationSettingPanelOpen.TrySave();
			SystemSettingPanelOpen.TrySave();
			ToolPanelOpen.TrySave();
			ColorfulTitle.TrySave();
			ExportPath.TrySave();
			ExportModIndex.TrySave();
			ModelScale.TrySave();
			LodNum.TrySave();
			LogMessage.TrySave();
			ShowDialog.TrySave();
			LightMapSupportTypeIndex.TrySave();
			OptimizationSettingPanelOpen.TrySave();
			ShaderSettingPanelOpen.TrySave();
			OptimizeFront.TrySave();
			OptimizeBack.TrySave();
			OptimizeUp.TrySave();
			OptimizeDown.TrySave();
			OptimizeLeft.TrySave();
			OptimizeRight.TrySave();
			EditorDockToScene.TrySave();
			ModelPivot.TrySave();
			FacingYPlusInMV.TrySave();
			CurrentShowingShaderIndex.TrySave();
			ReplacementModeIndex.TrySave();
			ShaderMainTextureKeyword.TrySave();
			for (int i = 0; i < Shader_Paths.Length; i++) {
				Shader_Paths[i].TrySave();
			}
			for (int i = 0; i < Shader_Keywords.Length; i++) {
				Shader_Keywords[i].TrySave();
			}
			for (int i = 0; i < Shader_ValueRemaps.Length; i++) {
				Shader_ValueRemaps[i].TrySave();
			}
		}



		private static void RefreshSelection () {

			VoxNum = 0;
			QbNum = 0;
			FolderNum = 0;
			JsonNum = 0;

			// Fix Selection
			var fixedSelection = new List<KeyValuePair<Object, string>>();
			for (int i = 0; i < Selection.objects.Length; i++) {
				fixedSelection.Add(new KeyValuePair<Object, string>(
					Selection.objects[i],
					AssetDatabase.GetAssetPath(Selection.objects[i]))
				);
			}

			for (int i = 0; i < fixedSelection.Count; i++) {
				if (!fixedSelection[i].Key) { continue; }
				var pathI = fixedSelection[i].Value;
				for (int j = 0; j < fixedSelection.Count; j++) {
					if (i == j || !fixedSelection[j].Key) { continue; }
					var pathJ = fixedSelection[j].Value;
					if (Util.IsChildPathCompair(pathJ, pathI)) {
						fixedSelection[j] = new KeyValuePair<Object, string>(null, null);
					}
				}
			}

			// Get Task Map
			TaskMap.Clear();
			for (int i = 0; i < fixedSelection.Count; i++) {
				if (!fixedSelection[i].Key) { continue; }
				var obj = fixedSelection[i].Key;
				var path = fixedSelection[i].Value;
				path = Util.FixPath(path);
				var ex = Util.GetExtension(path);
				if (AssetDatabase.IsValidFolder(path)) {
					FolderNum++;
					var files = Util.GetFilesIn(path, "*.vox", "*.qb", "*.json");
					for (int j = 0; j < files.Length; j++) {
						var filePath = Util.FixedRelativePath(files[j].FullName);
						var fileEx = Util.GetExtension(filePath);
						if (fileEx == ".vox" || fileEx == ".qb" || fileEx == ".json") {
							var fileObj = AssetDatabase.LoadAssetAtPath<Object>(filePath);
							if (fileObj && !TaskMap.ContainsKey(fileObj)) {
								TaskMap.Add(fileObj, new PathData() {
									Path = filePath,
									Extension = fileEx,
									Root = Util.FixPath(filePath.Substring(
										path.Length,
										filePath.Length - path.Length - Util.GetNameWithExtension(filePath).Length
									)),
								});

								if (fileEx == ".vox") {
									VoxNum++;
									FixVoxIcon(fileObj);
								} else if (fileEx == ".qb") {
									QbNum++;
									FixQbIcon(fileObj);
								} else if (fileEx == ".json") {
									JsonNum++;
									FixJsonIcon(fileObj);
								}
							}
						}
					}
				} else if (ex == ".vox" || ex == ".qb" || ex == ".json") {
					if (!TaskMap.ContainsKey(obj)) {
						TaskMap.Add(obj, new PathData() {
							Path = path,
							Extension = ex,
							Root = "",
						});
						if (ex == ".vox") {
							VoxNum++;
							FixVoxIcon(obj);
						} else if (ex == ".qb") {
							QbNum++;
							FixQbIcon(obj);
						} else if (ex == ".json") {
							JsonNum++;
							FixJsonIcon(obj);
						}
					}
				}
			}

			ObjNum = Selection.objects.Length;

		}



		private static void ForAllSelection (System.Action<PathData> action) {
			foreach (var key_Value in TaskMap) {
				action(key_Value.Value);
			}
		}



		private static void FixVoxIcon (Object vox) {
			if (!VoxFileIcon) {
				VoxFileIcon = AssetPreview.GetMiniThumbnail(vox);
			}
		}
		private static void FixQbIcon (Object qb) {
			if (!QbFileIcon) {
				QbFileIcon = AssetPreview.GetMiniThumbnail(qb);
			}
		}
		private static void FixJsonIcon (Object json) {
			if (!JsonFileIcon) {
				JsonFileIcon = AssetPreview.GetMiniThumbnail(json);
			}
		}



		private static bool BrowseExportPath () {
			string newPath = Util.FixPath(EditorUtility.OpenFolderPanel("Select Export Path", ExportPath, ""));
			if (!string.IsNullOrEmpty(newPath)) {
				newPath = Util.FixedRelativePath(newPath);
				if (!string.IsNullOrEmpty(newPath)) {
					ExportPath.Value = newPath;
					return true;
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
			return false;
		}





		#endregion




		#region --- UTL ---



		private static void RefreshMergeSetting () {
			Core_Voxel.SetMergeInDirection(OptimizeFront, OptimizeBack, OptimizeUp, OptimizeDown, OptimizeLeft, OptimizeRight);
		}



		private static void ShaderPropertyGUI (int index, string name) {

			const int GAP = 12;

			if (index < 0) {
				// Title
				LayoutH(() => {
					EditorGUI.LabelField(GUIRect(0, 16), "");
					EditorGUI.LabelField(GUIRect(0, 16), "Keyword", EditorStyles.centeredGreyMiniLabel);
					Space(GAP);
					EditorGUI.LabelField(GUIRect(18 * 4 + 30 + 2 + 2, 16), "Remap", EditorStyles.centeredGreyMiniLabel);
				});
			} else {
				// Content
				LayoutH(() => {

					EditorGUI.LabelField(GUIRect(0, 16), name);

					Shader_Keywords[index].Value = EditorGUI.TextField(GUIRect(0, 16), Shader_Keywords[index]);

					Space(GAP);

					bool oldE = GUI.enabled;
					GUI.enabled = false;
					EditorGUI.FloatField(GUIRect(18, 16), SHADER_VALUE_REMAP_SOURCE[index].x);
					Space(2);
					EditorGUI.FloatField(GUIRect(30, 16), SHADER_VALUE_REMAP_SOURCE[index].y);
					GUI.enabled = oldE;
					EditorGUI.LabelField(GUIRect(18, 16), "→", EditorStyles.centeredGreyMiniLabel);
					Vector2 newRemap = Vector2.zero;
					newRemap.x = EditorGUI.FloatField(GUIRect(18, 16), Shader_ValueRemaps[index].Value.x);
					Space(2);
					newRemap.y = EditorGUI.FloatField(GUIRect(18, 16), Shader_ValueRemaps[index].Value.y);
					Shader_ValueRemaps[index].Value = newRemap;
				});
			}

		}



		#endregion



	}







	[CustomEditor(typeof(DefaultAsset)), CanEditMultipleObjects]
	public class VoxelInspector : Editor {


		private void OnEnable () {
			if (HasVoxelTarget()) {
				VoxelToUnityWindow.Window_Enable();
			}
		}


		public override void OnInspectorGUI () {
			base.OnInspectorGUI();
			if (HasVoxelTarget()) {
				bool oldE = GUI.enabled;
				GUI.enabled = true;
				VoxelToUnityWindow.Window_Main();
				GUI.enabled = oldE;
			}
		}


		private bool HasVoxelTarget () {
			for (int i = 0; i < targets.Length; i++) {
				string path = AssetDatabase.GetAssetPath(targets[i]);
				string ex = Util.GetExtension(path);
				if (ex == ".vox" || ex == ".qb" || Util.HasFileIn(path, "*.vox", "*.qb")) {
					return true;
				}
			}
			return false;
		}

	}


}