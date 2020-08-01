namespace MagicaVoxelToolbox {
	using MagicaVoxelToolbox;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;



	// Combiner Part
	public partial class VoxelEditorWindow : MoenenEditorWindow {




		// SUB
		public class PrefabData {
			public Transform Prefab;
			public Transform Object;
			public Vector3 Rotation;
			public Texture2D Preview;
		}



		// Short Cut
		private bool IsCombining {
			get {
				return CurrentEditorMode == EditorMode.PrefabCombiner;
			}
		}



		// Data
		private List<PrefabData> CombineList = new List<PrefabData>();
		private int SelectingPrefabIndex = -1;



		public void OpenCombiner () {
			CurrentModelIndex = 0;
			VoxelFilePath = "";
			Data = VoxelData.CreateNewData();
			// Node Open
			if (Data) {
				foreach (var gp in Data.Groups) {
					if (!NodeOpen.ContainsKey(gp.Key)) {
						NodeOpen.Add(gp.Key, false);
					}
				}
			}
			SwitchModel(CurrentModelIndex);
			ClearScene();
			CombineList.Clear();
			SelectingPrefabIndex = -1;
			DataDirty = false;
		}



		private void CombinerGUI () {

			const int ITEM_SIZE = 72;
			const int ITEM_GAP = 4;
			const int CONTENT_WIDTH = 536;
			const int COUNT_X = (CONTENT_WIDTH - ITEM_GAP) / (ITEM_SIZE + ITEM_GAP);

			// Prefabs
			LayoutH(() => {

				// Content
				LayoutV(() => {

					Space(8);

					var style = new GUIStyle() { fixedWidth = ITEM_SIZE, };
					int currentIndex = 0;
					while (currentIndex < CombineList.Count) {
						LayoutH(() => {
							Space(ITEM_GAP);
							for (int x = 0; x < COUNT_X && currentIndex < CombineList.Count; x++) {
								bool continueFlag = false;
								// Item
								LayoutV(() => {
									var prefab = CombineList[currentIndex];
									if (prefab.Prefab) {

										// Preview
										var rect = GUIRect(ITEM_SIZE, ITEM_SIZE);
										var preview = prefab.Preview ?? (prefab.Preview = AssetPreview.GetAssetPreview(prefab.Prefab.gameObject));
										GUI.DrawTexture(rect, preview ?? Texture2D.whiteTexture, ScaleMode.ScaleToFit);

										// Name
										GUI.Label(GUIRect(ITEM_SIZE, 18), prefab.Prefab.name);

										// Highlight
										if (currentIndex == SelectingPrefabIndex) {
											var oldColor = GUI.color;
											GUI.color = Color.green;
											GUI.Box(rect, GUIContent.none);
											GUI.color = oldColor;
										}

										// Click
										if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
											SelectingPrefabIndex = currentIndex;
											Event.current.Use();
										}

										currentIndex++;
									} else {
										CombineList.RemoveAt(currentIndex);
										currentIndex--;
										continueFlag = true;
									}
								}, false, style);
								Space(ITEM_GAP);
								if (continueFlag) { continue; }
							}
						});
						Space(ITEM_GAP);
					}
					if (CombineList.Count == 0) {
						Space(4);
						LayoutH(() => {
							GUIRect(12, 36);
							EditorGUI.HelpBox(GUIRect(420, 36), "No prefab in list.\nClick \"+ Prefab\" button or drag prefab into this window to add prefab.", MessageType.Info);
						});
						Space(4);
					}
					Space(4);

					// Add Button
					var _rect = GUIRect(82, 24);
					_rect.x = 0;
					if (GUI.Button(_rect, "+ Prefab", EditorStyles.miniButtonRight)) {
						AddCombinePrefab(
							Util.FixedRelativePath(EditorUtility.OpenFilePanel("Pick Prefab", "Assets", "prefab"))
						);
					}

					// Clear Button
					Space(2);
					_rect = GUIRect(82, 24);
					_rect.x = 0;
					if (GUI.Button(_rect, "Clear", EditorStyles.miniButtonRight)) {
						if (Util.Dialog("", "Remove all prefabs in the list?", "Remove All", "Cancel")) {
							ClearScene();
							CombineList.Clear();
							SelectingPrefabIndex = -1;
							Repaint();
						}
					}

					Space(8);

				}, false, new GUIStyle() { fixedWidth = CONTENT_WIDTH, });

				// Panel
				if (SelectingPrefabIndex >= 0 && SelectingPrefabIndex < CombineList.Count) {
					LayoutV(() => {
						var prefab = CombineList[SelectingPrefabIndex];
						if (prefab.Prefab) {

							// Name
							GUI.Label(GUIRect(0, 18), SelectingPrefabIndex + ", " + prefab.Prefab.name);
							Space(6);

							// Transform
							LayoutH(() => {
								GUI.Label(GUIRect(67, 18), "Position");
								prefab.Object.localPosition = EditorGUI.Vector3Field(GUIRect(0, 18), "", prefab.Object.localPosition);
							});
							LayoutH(() => {
								GUI.Label(GUIRect(67, 18), "Rotation");
								prefab.Rotation = EditorGUI.Vector3Field(GUIRect(0, 18), "", prefab.Rotation);
								prefab.Object.localRotation = Quaternion.Euler(prefab.Rotation);
							});
							LayoutH(() => {
								GUI.Label(GUIRect(67, 18), "Scale");
								prefab.Object.localScale = EditorGUI.Vector3Field(GUIRect(0, 18), "", prefab.Object.localScale);
							});
							Space(8);

							// Delete Button
							LayoutH(() => {
								GUIRect(0, 1);
								if (GUI.Button(GUIRect(72, 18), "Remove") && Util.Dialog("", "Remove prefab" + prefab.Prefab.name + " from list?", "Remove", "Cancel")) {
									if (prefab.Object) {
										DestroyImmediate(prefab.Object.gameObject, false);
									}
									CombineList.RemoveAt(SelectingPrefabIndex);
									SelectingPrefabIndex = CombineList.Count == 0 ? -1 : Mathf.Clamp(SelectingPrefabIndex, 0, CombineList.Count - 1);
									Repaint();
								}
							});
							Space(8);

						}
					}, true);
				} else {
					LayoutV(() => {
						GUIRect(0, 1);
					});
				}

			});

		}



		private void CombinerPanelGUI () {
			const int WIDTH = 240;
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

			bool oldE = GUI.enabled;
			GUI.enabled = CombinerEditingRoot && CombinerEditingRoot.childCount > 0;
			// Combiner
			if (GUI.Button(rect, "   Combine Mesh + Material + Texture", buttonStyle)) {
				CombinePrefab(true, EditorPrefs.GetString("V2U.ShaderMainTextureKeyword", "_MainTex"));
				Repaint();
			}
			GUI.Label(rect, "  <color=#33ccff>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

			if (GUI.Button(rect, "   Combine Material + Texture", buttonStyle)) {
				CombinePrefab(false, EditorPrefs.GetString("V2U.ShaderMainTextureKeyword", "_MainTex"));
				Repaint();
			}
			GUI.Label(rect, "  <color=#33ccff>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

			GUI.enabled = oldE;
		}



		private void ClearScene () {
			if (CombinerEditingRoot) {
				int len = CombinerEditingRoot.childCount;
				for (int i = 0; i < len; i++) {
					DestroyImmediate(CombinerEditingRoot.GetChild(0).gameObject, false);
				}
			}
		}



		private void AddCombinePrefab (string path) {
			if (!string.IsNullOrEmpty(path)) {
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if (prefab) {
					var tf = Instantiate(prefab, CombinerEditingRoot).transform;
					tf.localPosition = Vector3.zero;
					tf.localRotation = Quaternion.identity;
					tf.localScale = Vector3.one;
					tf.gameObject.layer = LAYER_ID;
					var allMRs = tf.GetComponentsInChildren<MeshRenderer>(false);
					var allSMRs = tf.GetComponentsInChildren<SkinnedMeshRenderer>(false);
					for (int i = 0; i < allMRs.Length; i++) {
						allMRs[i].gameObject.layer = LAYER_ID;
					}
					for (int i = 0; i < allSMRs.Length; i++) {
						allSMRs[i].gameObject.layer = LAYER_ID;
					}
					CombineList.Add(new PrefabData() {
						Prefab = prefab.transform,
						Object = tf,
						Rotation = Vector3.zero,
						Preview = null,
					});
					Repaint();
				}
			}
		}



		private void CombinePrefab (bool combineMesh, string mainTexKeyword) {

			if (!CombinerEditingRoot || CombinerEditingRoot.childCount == 0) { return; }
			var path = Util.FixedRelativePath(EditorUtility.SaveFilePanel("Save Combined Prefab", "Assets", "Combined Prefab", "prefab"));
			if (string.IsNullOrEmpty(path)) { return; }
			try {
				var result = Core_Combine.Combine(
					CombinerEditingRoot,
					combineMesh,
					(msg, progress) => {
						Util.ProgressBar("Combining", msg, progress);
					},
					mainTexKeyword
				);
				if (result != null) {

					// Empty Prefab
					if (Util.FileExists(path)) {
						Object[] things = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
						foreach (Object o in things) {
							DestroyImmediate(o, true);
						}
					} else {
						var tempObject = new GameObject();
						SavePrefab(tempObject, path, true);
						DestroyImmediate(tempObject, false);
					}

					// Add Sub Objects In
					AssetDatabase.AddObjectToAsset(result.Texture, path);
					for (int i = 0; i < result.Meshs.Count; i++) {
						AssetDatabase.AddObjectToAsset(result.Meshs[i], path);
					}
					for (int i = 0; i < result.Materials.Count; i++) {
						AssetDatabase.AddObjectToAsset(result.Materials[i], path);
					}

					// Override Prefab
					SavePrefab(result.Root.gameObject, path);
					DestroyImmediate(result.Root.gameObject, false);

					// Done
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
					AssetDatabase.SaveAssets();
					Resources.UnloadUnusedAssets();
				}
			} catch (System.Exception ex) { Debug.LogError(ex.Message); }

			Util.ClearProgressBar();
		}




		private static Object SavePrefab (GameObject obj, string path, bool createEmpty = false) {
#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
			if (createEmpty) {
				return PrefabUtility.CreateEmptyPrefab(path);
			} else {
				return PrefabUtility.ReplacePrefab(obj, AssetDatabase.LoadAssetAtPath<Object>(path), ReplacePrefabOptions.ReplaceNameBased);
			}
#else  // 2018.3+
			return PrefabUtility.SaveAsPrefabAsset(obj, path);
#endif
		}


	}


}