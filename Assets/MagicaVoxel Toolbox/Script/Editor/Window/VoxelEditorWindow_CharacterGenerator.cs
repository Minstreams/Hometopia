namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using MagicaVoxelToolbox.Saving;


	// Character Generator Part
	public partial class VoxelEditorWindow {




		#region --- SUB ---




		private enum CharacterOrganGroup {
			Head = 0,
			Body = 1,
			ArmL = 2,
			ArmR = 3,
			LegL = 4,
			LegR = 5,
		}



		private enum CharacterGeneratorPanelType {
			Organ = 0,
			OrganTable = 1,
			Attachment = 2,
		}




		#endregion




		#region --- VAR ---


		// Global
		private static readonly string[] CHARACTER_STEP_LABELS = new string[] {
			"{0}/{1} Generating physical Body {2}%",
			"{0}/{1} Generating attachment {2}%",
		};
		private static readonly string[] CHARACTER_GENERATOR_PANEL_LANEL = new string[] {
			"Organ",
			"Organ Table",
			"Attachment",
		};
		private static readonly int[] CHARACTER_GENERATOR_PANEL_WIDTH = new int[] {
			52, 82, 82
		};
		private static readonly string[] CHARACTER_ORGAN_TYPE_MASK_LABELS = new string[] {
			"Head",
			"Neck",
			"Body",
			"Hip",
			"Left Leg Upper",
			"Left Leg Lower",
			"Left Foot",
			"Left Arm Upper",
			"Left Arm Lower",
			"Left Hand",
			"Right Leg Upper",
			"Right Leg Lower",
			"Right Foot",
			"Right Arm Upper",
			"Right Arm Lower",
			"Right Hand",
		};


		// Data
		private Core_CharacterGeneration.Preset CGConfig = new Core_CharacterGeneration.Preset();
		private Dictionary<Core_CharacterGeneration.AttachmentData, Texture2D> CharacterAttachmentThumbnailMap = new Dictionary<Core_CharacterGeneration.AttachmentData, Texture2D>();
		private CharacterOrganGroup CurrentOrganGroup = CharacterOrganGroup.Head;
		private CharacterGeneratorPanelType TheCharacterGeneratorPanelType = CharacterGeneratorPanelType.Organ;
		private int CurrentAttachmentIndex = 0;


		// Saving
		private EditorSavingBool ShowAllAttachment = new EditorSavingBool("VEditor.ShowAllAttachment", false);



		#endregion




		#region --- GUI ---



		private void CharacterGeneratorGUI () {

			// Switch Button
			LayoutV(() => {

				Space(6);

				int count = CHARACTER_GENERATOR_PANEL_LANEL.Length;
				LayoutH(() => {
					for (int i = 0; i < count; i++) {
						var type = (CharacterGeneratorPanelType)i;
						if (type == CharacterGeneratorPanelType.OrganTable) {
							continue;
						}
						string typeName = CHARACTER_GENERATOR_PANEL_LANEL[i];
						GUIStyle style;
						if (type == TheCharacterGeneratorPanelType) {
							style = new GUIStyle(i == count - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid);
							style.normal = style.active;
						} else {
							style = i == count - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
						}
						if (GUI.Button(GUIRect(CHARACTER_GENERATOR_PANEL_WIDTH[i], 20), typeName, style)) {
							TheCharacterGeneratorPanelType = type;
							Repaint();
						}
					}
				});

			});

			Space(4);

			// Content
			LayoutV(() => {

				bool changed = false;

				switch (TheCharacterGeneratorPanelType) {
					default:
					case CharacterGeneratorPanelType.Organ:
						// Physical Body Controller
						LayoutH(() => {
							CharacterHumanMapGUI();
							changed = PhysicalBodyControllerContentGUI() || changed;
						});
						break;
					case CharacterGeneratorPanelType.OrganTable:
						// Physical Body Table
						changed = OrganTableGUI() || changed;
						break;
					case CharacterGeneratorPanelType.Attachment:
						// Attachment Controller
						changed = CharacterAttachmentGUI() || changed;
						break;
				}

				Space(2);

				if (changed) {
					CGConfig.FixGenerationValues();
					Save_Generation();
					SetDataDirty();
				}

			}, false, new GUIStyle(GUI.skin.box) {
				padding = new RectOffset(9, 6, 4, 4),
				margin = new RectOffset(14, 20, 8, 4),
			});
		}



		private void CharacterHumanMapGUI () {
			LayoutV(() => {

				var tintColor = new Color(0.8f, 0.85f, 0.8f);
				var disableColor = new Color(0.8f, 0.85f, 0.8f, 0.9f);
				var buttonStyle = EditorStyles.miniButton;
				var rect = new Rect();
				var allRect = GUIRect(90, 120);
				var oldE = GUI.enabled;
				var oldC = GUI.color;

				// Head
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.Head;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				rect.width = allRect.width * 0.618f;
				rect.height = allRect.height * 0.309f;
				rect.x = allRect.x + (allRect.width - rect.width) / 2f;
				rect.y = allRect.y + 2;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.Head;
				}

				// Body
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.Body;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				rect.y += rect.height + 2;
				rect.height = allRect.height * 0.32f;
				rect.width = allRect.width * 0.618f * 0.618f;
				rect.x = allRect.x + (allRect.width - rect.width) / 2f;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.Body;
				}

				// Leg
				rect.y += rect.height + 2;
				rect.height = allRect.height * 0.309f;
				rect.width = allRect.width * 0.618f * 0.618f * 0.5f;
				rect.x = allRect.x + (allRect.width - rect.width * 2f) / 2f;
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.LegL;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.LegL;
				}
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.LegR;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				rect.x += rect.width;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.LegR;
				}

				// Arm
				rect.height = allRect.height * 0.309f + 20f;
				rect.width = allRect.width * 0.618f * 0.618f * 0.5f;
				rect.x = allRect.x + allRect.width * 0.1f;
				rect.y = allRect.y + allRect.height * 0.309f + 6f;
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.ArmL;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.ArmL;
				}
				GUI.enabled = oldE && CurrentOrganGroup != CharacterOrganGroup.ArmR;
				GUI.color = GUI.enabled ? tintColor : disableColor;
				rect.x = allRect.width - rect.width + 2f * allRect.x - rect.x;
				if (GUI.Button(rect, "", buttonStyle)) {
					CurrentOrganGroup = CharacterOrganGroup.ArmR;
				}

				GUI.enabled = oldE;
				GUI.color = oldC;

			}, false, new GUIStyle() {
				fixedWidth = 90,
			});
		}



		// Controller
		private bool PhysicalBodyControllerContentGUI () {

			const int GAP_WIDTH = 12;

			bool changed = false;

			Space(GAP_WIDTH);

			switch (CurrentOrganGroup) {
				case CharacterOrganGroup.Head:
					changed = OrganControllerItemGUI(CGConfig.Head, "Head") || changed;
					changed = OrganControllerItemGUI(CGConfig.Neck, "Neck") || changed;
					break;
				case CharacterOrganGroup.Body:
					changed = OrganControllerItemGUI(CGConfig.Body, "Body") || changed;
					changed = OrganControllerItemGUI(CGConfig.Hip, "Hip") || changed;
					break;
				case CharacterOrganGroup.ArmL:
					changed = OrganControllerItemGUI(CGConfig.ArmU_L, "Left Upper Arm", CGConfig.ArmU_R) || changed;
					changed = OrganControllerItemGUI(CGConfig.ArmD_L, "Left Lower Arm", CGConfig.ArmD_R) || changed;
					changed = OrganControllerItemGUI(CGConfig.Hand_L, "Left Hand", CGConfig.Hand_R) || changed;
					break;
				case CharacterOrganGroup.ArmR:
					changed = OrganControllerItemGUI(CGConfig.ArmU_R, "Right Upper Arm", CGConfig.ArmU_L) || changed;
					changed = OrganControllerItemGUI(CGConfig.ArmD_R, "Right Lower Arm", CGConfig.ArmD_L) || changed;
					changed = OrganControllerItemGUI(CGConfig.Hand_R, "Right Hand", CGConfig.Hand_L) || changed;
					break;
				case CharacterOrganGroup.LegL:
					changed = OrganControllerItemGUI(CGConfig.LegU_L, "Left Upper Leg", CGConfig.LegU_R) || changed;
					changed = OrganControllerItemGUI(CGConfig.LegD_L, "Left Lower Leg", CGConfig.LegD_R) || changed;
					changed = OrganControllerItemGUI(CGConfig.Foot_L, "Left Foot", CGConfig.Foot_R) || changed;
					break;
				case CharacterOrganGroup.LegR:
					changed = OrganControllerItemGUI(CGConfig.LegU_R, "Right Upper Leg", CGConfig.LegU_L) || changed;
					changed = OrganControllerItemGUI(CGConfig.LegD_R, "Right Lower Leg", CGConfig.LegD_L) || changed;
					changed = OrganControllerItemGUI(CGConfig.Foot_R, "Right Foot", CGConfig.Foot_L) || changed;
					break;
			}

			return changed;
		}



		private bool OrganControllerItemGUI (Core_CharacterGeneration.OrganData organ, string name, Core_CharacterGeneration.OrganData mirrorOrgan = null) {

			bool changed = false;

			const int HEIGHT = 22;
			const int LABEL_WIDTH = 52;
			const int LABEL_WIDTH_ALT = 24;
			const int BUTTON_WIDTH = 24;
			const int GAP_HEIGHT = 4;
			const int GAP_WIDTH_ALT = 4;

			LayoutV(() => {

				// Name
				GUI.Label(GUIRect(0, HEIGHT), name);

				// Size
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Size");
					// X
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.SizeX) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.SizeX) || changed;
					Space(GAP_WIDTH_ALT);
					// Y
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.SizeY) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.SizeY) || changed;
					Space(GAP_WIDTH_ALT);
					// Z
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.SizeZ) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.SizeZ) || changed;
				});

				Space(GAP_HEIGHT);

				// Position
				LayoutH(() => {

					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Position");

					// X
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.X) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.X) || changed;
					Space(GAP_WIDTH_ALT);

					// Y
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.Y) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.Y) || changed;
					Space(GAP_WIDTH_ALT);

					// Z
					changed = IntField(LABEL_WIDTH_ALT, HEIGHT, "", ref organ.Z) || changed;
					changed = AddReduceButtons(BUTTON_WIDTH, HEIGHT, ref organ.Z) || changed;

				});
				Space(GAP_HEIGHT);

				LayoutH(() => {

					// Visible
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Visible");
					var newVisible = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), organ.Visible);
					if (newVisible != organ.Visible) {
						organ.Visible = newVisible;
						changed = true;
					}
					Space(12);

					// Mirror
					var oldE = GUI.enabled;
					GUI.enabled = mirrorOrgan;
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Mirror");
					var newMirror = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), organ.Mirror);
					GUI.enabled = oldE;
					if (mirrorOrgan) {
						mirrorOrgan.Mirror = newMirror;
						if (newMirror != organ.Mirror) {
							organ.Mirror = newMirror;
							mirrorOrgan.Mirror = newMirror;
							changed = true;
						}

						if (changed && organ.Mirror) {
							mirrorOrgan.CopyFrom(organ, true);
						}
					}

				});

				LayoutH(() => {
					// Skin Color
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Skin");
					var color = ColorField(0, HEIGHT, "", organ.SkinColor);
					if (color != organ.SkinColor) {
						organ.SkinColor = color;
						changed = true;
					}
				});

			}, false, new GUIStyle(GUI.skin.box) {
				fixedWidth = LABEL_WIDTH + LABEL_WIDTH_ALT * 3f + BUTTON_WIDTH * 3f + GAP_WIDTH_ALT * 2f + 12,
			});

			return changed;
		}



		// Table
		private bool OrganTableGUI () {

			var tintColor = new Color(0.8f, 0.85f, 0.8f);
			const int HEIGHT = 16;
			const int LABEL_WIDTH = 80;
			const int ITEM_WIDTH = 32;
			const int TABLE_LINE_COUNT = 3;
			const int TABLE_GAP = 14;

			var textFieldStyle = new GUIStyle(GUI.skin.textField) {
				alignment = TextAnchor.MiddleCenter,
			};

			var oldC = GUI.color;
			GUI.color = tintColor;

			Space(4);

			// Title
			LayoutH(() => {

				for (int i = 0; i < TABLE_LINE_COUNT; i++) {

					Space(TABLE_GAP);

					// Organ
					Rect rect = GUIRect(LABEL_WIDTH, HEIGHT);
					rect.height *= 2;
					EditorGUI.LabelField(rect, "Organ", textFieldStyle);

					// Size Pos
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH * 3, HEIGHT), "Size", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH * 3, HEIGHT), "Position", textFieldStyle);

					// Skin
					rect = GUIRect(18, HEIGHT);
					rect.height *= 2;
					EditorGUI.LabelField(rect, "S", textFieldStyle);

				}

			});


			// Sub Title
			LayoutH(() => {
				for (int i = 0; i < TABLE_LINE_COUNT; i++) {

					Space(TABLE_GAP);

					// Organ Space
					GUIRect(LABEL_WIDTH, HEIGHT);

					// Size Pos
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "X", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "Y", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "Z", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "X", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "Y", textFieldStyle);
					EditorGUI.LabelField(GUIRect(ITEM_WIDTH, HEIGHT), "Z", textFieldStyle);

					// Skin Space
					GUIRect(18, HEIGHT);
				}
			});

			GUI.color = oldC;

			bool changed = false;

			// Head
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.ArmU_L, "Upper Arm L", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.LegU_L, "Upper Leg L", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Head, "Head", textFieldStyle) || changed;

			});
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.ArmU_R, "Upper Arm R", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.LegU_R, "Upper Leg R", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Body, "Body", textFieldStyle) || changed;
			});
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.ArmD_L, "Lower Arm L", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.LegD_L, "Lower Leg L", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Neck, "Neck", textFieldStyle) || changed;
			});
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.ArmD_R, "Lower Arm R", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.LegD_R, "Lower Leg R", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Hip, "Hip", textFieldStyle) || changed;

			});
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.Hand_L, "Hand L", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Foot_L, "Foot L", textFieldStyle) || changed;
			});
			LayoutH(() => {
				changed = OrganTableItemGUI(CGConfig.Hand_R, "Hand R", textFieldStyle) || changed;
				changed = OrganTableItemGUI(CGConfig.Foot_R, "Foot R", textFieldStyle) || changed;
			});

			return changed;
		}



		private bool OrganTableItemGUI (Core_CharacterGeneration.OrganData organ, string name, GUIStyle labelStyle) {

			bool changed = false;

			const int HEIGHT = 18;
			const int LABEL_WIDTH = 80;
			const int ITEM_WIDTH = 32;
			const int TABLE_GAP = 14;

			Space(TABLE_GAP);

			// Name
			var oldC = GUI.color;
			GUI.color = new Color(0.8f, 0.85f, 0.8f);
			EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), name, labelStyle);
			GUI.color = oldC;

			// Size
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.SizeX, true, labelStyle) || changed;
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.SizeY, true, labelStyle) || changed;
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.SizeZ, true, labelStyle) || changed;

			// Pos
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.X, true, labelStyle) || changed;
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.Y, true, labelStyle) || changed;
			changed = IntField(ITEM_WIDTH, HEIGHT, "", ref organ.Z, true, labelStyle) || changed;

			// Skin
			var color = ColorField(0, HEIGHT, "", organ.SkinColor);
			if (color != organ.SkinColor) {
				organ.SkinColor = color;
				changed = true;
			}


			return changed;
		}



		// Attachment
		private bool CharacterAttachmentGUI () {

			bool changed = false;
			if (CGConfig == null) { return changed; }

			const int HEIGHT = 18;

			var attachments = CGConfig.Attachments;
			if (attachments == null) {
				CGConfig.Attachments = new List<Core_CharacterGeneration.AttachmentData>();
			}

			LayoutH(() => {

				LayoutV(() => {

					// Map
					var oldE = GUI.enabled;
					GUI.enabled = !ShowAllAttachment;
					CharacterHumanMapGUI();
					GUI.enabled = oldE;
					Space(4);

					// + Empty Button
					if (GUI.Button(GUIRect(0, HEIGHT), "+ Empty", EditorStyles.miniButton)) {
						var organ = GetFirstOrganInGroup(CurrentOrganGroup);
						var att = new Core_CharacterGeneration.AttachmentData() {
							Name = "New Attachment " + attachments.Count,
							TargetMask = Util.SetBitValue(0, (int)organ, true),
						};
						attachments.Add(att);
						ReloadCharacterAttachmentThumbnailMap();
						changed = true;
						Repaint();
					}
					Space(2);

					// Load Button
					if (GUI.Button(GUIRect(0, HEIGHT), "+ VOX", EditorStyles.miniButton)) {
						string path;
						var data = TryPickVoxelData(out path);
						if (data && data.Voxels != null) {
							var organ = GetFirstOrganInGroup(CurrentOrganGroup);
							for (int i = 0; i < data.Voxels.Count; i++) {
								string fix = data.Voxels.Count > 1 ? "_" + i.ToString() : "";
								var att = new Core_CharacterGeneration.AttachmentData() {
									Name = Util.GetNameWithoutExtension(path) + fix,
									TargetMask = Util.SetBitValue(0, (int)organ, true),
								};
								att.LoadFrom(data, i);
								attachments.Add(att);
								ReloadCharacterAttachmentThumbnailMap();
								changed = true;
								Repaint();
							}
						}
					}

				}, false, new GUIStyle() { fixedWidth = 90, });

				Space(22);

				// List
				changed = CharacterAttachmentListGUI() || changed;
				Space(4);

				// Content
				if (CurrentAttachmentIndex >= 0 && CurrentAttachmentIndex < attachments.Count) {
					var att = attachments[CurrentAttachmentIndex];
					if (att != null) {
						if (ShowAllAttachment || CheckAttachmentOrganGroup(att, CurrentOrganGroup)) {
							changed = CharacterAttachmentContentGUI(att) || changed;
						}
					}
				}
			});

			Space(8);

			return changed;
		}



		private bool CharacterAttachmentListGUI () {

			var attachments = CGConfig.Attachments;
			bool changed = false;

			LayoutV(() => {

				if (attachments == null || attachments.Count == 0) {
					EditorGUI.HelpBox(GUIRect(0, 64), "No attachment data.\nUse \"+ VOX\" button to add attachment.", MessageType.Info);
				} else {

					const int HEIGHT = 18;

					LayoutH(() => {
						// Show All
						var newShowAllAttachment = EditorGUI.ToggleLeft(GUIRect(90, HEIGHT), " Show All", ShowAllAttachment);
						if (newShowAllAttachment != ShowAllAttachment) {
							ShowAllAttachment.Value = newShowAllAttachment;
							ShowAllAttachment.TrySave();
						}

						// Move Button
						if (ShowAllAttachment && CurrentAttachmentIndex >= 0) {
							var oldE = GUI.enabled;
							GUI.enabled = CurrentAttachmentIndex > 0;
							if (GUI.Button(GUIRect(24, HEIGHT), "▲", EditorStyles.miniButtonLeft)) {
								var att = attachments[CurrentAttachmentIndex];
								attachments[CurrentAttachmentIndex] = attachments[CurrentAttachmentIndex - 1];
								attachments[CurrentAttachmentIndex - 1] = att;
								CurrentAttachmentIndex--;
								changed = true;
								Repaint();
							}
							GUI.enabled = CurrentAttachmentIndex < attachments.Count - 1;
							if (GUI.Button(GUIRect(24, HEIGHT), "▼", EditorStyles.miniButtonRight)) {
								var att = attachments[CurrentAttachmentIndex];
								attachments[CurrentAttachmentIndex] = attachments[CurrentAttachmentIndex + 1];
								attachments[CurrentAttachmentIndex + 1] = att;
								CurrentAttachmentIndex++;
								changed = true;
								Repaint();
							}
							GUI.enabled = oldE;
						}

					});
					Space(4);


					// List
					CurrentAttachmentIndex = Mathf.Clamp(CurrentAttachmentIndex, -1, attachments.Count - 1);
					if (!ShowAllAttachment && CurrentAttachmentIndex >= 0 && CurrentAttachmentIndex < attachments.Count && !CheckAttachmentOrganGroup(attachments[CurrentAttachmentIndex], CurrentOrganGroup)) {
						CurrentAttachmentIndex = -1;
					}

					bool hasItem = false;
					bool pressing = Event.current.type == EventType.MouseDown;
					var position = Event.current.mousePosition;
					for (int i = 0; i < attachments.Count; i++) {
						var att = attachments[i];
						if (att == null || (!ShowAllAttachment && !CheckAttachmentOrganGroup(att, CurrentOrganGroup))) { continue; }

						var rect = GUIRect(0, HEIGHT);
						rect.width -= rect.height;

						// highlight
						if (CurrentAttachmentIndex == i) {
							var oldC = GUI.color;
							GUI.color = PaintingBoneIndex == -1 ? new Color(0.5f, 1f, 1f, 0.08f) : new Color(1f, 0.5f, 1f, 0.08f);
							GUI.DrawTexture(rect, Texture2D.whiteTexture);
							GUI.color = oldC;
						}

						// Label
						GUI.Label(rect, att.Name);
						if (pressing && rect.Contains(position)) {
							int index = i;
							EditorApplication.delayCall += () => {
								CurrentAttachmentIndex = index;
								Repaint();
							};
						}

						// Visible
						rect.x += rect.width;
						rect.width = rect.height;
						var newVisible = EditorGUI.Toggle(rect, att.Visible);
						if (att.Visible != newVisible) {
							att.Visible = newVisible;
							changed = true;
						}

						// Icon
						rect.x -= rect.width + 6;
						if (CharacterAttachmentThumbnailMap.ContainsKey(att)) {
							var texture = CharacterAttachmentThumbnailMap[att];
							if (texture) {
								GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
							}
						}

						hasItem = true;
					}
					if (!hasItem) {
						EditorGUI.HelpBox(GUIRect(0, 42), "No attachment", MessageType.Info);
					}
				}

				Space(4);

			}, false, new GUIStyle() {
				fixedWidth = 180,
			});

			return changed;
		}



		private bool CharacterAttachmentContentGUI (Core_CharacterGeneration.AttachmentData att) {

			if (att == null) { return false; }

			const int HEIGHT = 18;
			const int LABEL_WIDTH = 72;
			const int THUMB_SIZE = 120;
			const int LAYOUT_WIDTH = 220;

			bool changed = false;

			var attachments = CGConfig.Attachments;
			if (attachments == null) {
				CGConfig.Attachments = new List<Core_CharacterGeneration.AttachmentData>();
			}
			LayoutV(() => {

				// Detail
				LayoutH(() => {

					LayoutV(() => {

						Space(4);

						LayoutH(() => {
							// Name
							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Name");
							changed = StringField(0, HEIGHT, ref att.Name) || changed;
							Space(2);
							// Visible
							var newVisible = EditorGUI.Toggle(GUIRect(HEIGHT, HEIGHT), att.Visible);
							if (newVisible != att.Visible) {
								att.Visible = newVisible;
								changed = true;
							}
						});
						Space(2);


						// Targets
						// Label
						LayoutH(() => {
							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Targets");
							int newMask = EditorGUI.MaskField(GUIRect(0, HEIGHT), att.TargetMask, CHARACTER_ORGAN_TYPE_MASK_LABELS);
							if (newMask != att.TargetMask) {
								att.TargetMask = newMask;
								changed = true;
							}
						});
						Space(2);


						// Placement Type
						LayoutH(() => {
							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Placement");
							var newPlacement = (Core_CharacterGeneration.PlacementType)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), att.PlacementType);
							if (att.PlacementType != newPlacement) {
								att.PlacementType = newPlacement;
								changed = true;
							}
						});
						Space(2);

						// Lower Panel
						// Size
						LayoutH(() => {
							EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Size");
							if (att.Voxels != null && att.Voxels.Length > 0) {
								var oldC = GUI.color;
								GUI.color = new Color(1, 1, 1, 0.5f);
								EditorGUI.LabelField(GUIRect(0, HEIGHT), att.SizeX.ToString(), GUI.skin.textField);
								EditorGUI.LabelField(GUIRect(0, HEIGHT), att.SizeY.ToString(), GUI.skin.textField);
								EditorGUI.LabelField(GUIRect(0, HEIGHT), att.SizeZ.ToString(), GUI.skin.textField);
								GUI.color = oldC;
							}
						});
						Space(2);

						// Anchor Border Min Max
						switch (att.PlacementType) {
							default:
							case Core_CharacterGeneration.PlacementType.Position:
								// Position
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Position");

									int newInt;
									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.PositionX);
									if (newInt != att.PositionX) {
										att.PositionX = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.PositionY);
									if (newInt != att.PositionY) {
										att.PositionY = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.PositionZ);
									if (newInt != att.PositionZ) {
										att.PositionZ = newInt;
										changed = true;
									}
								});
								Space(2);

								// Anchor
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Anchor");

									float newFloat;
									newFloat = EditorGUI.FloatField(GUIRect(0, HEIGHT), att.Anchor.x);
									if (newFloat != att.Anchor.x) {
										att.Anchor.x = newFloat;
										changed = true;
									}
									Space(2);

									newFloat = EditorGUI.FloatField(GUIRect(0, HEIGHT), att.Anchor.y);
									if (newFloat != att.Anchor.y) {
										att.Anchor.y = newFloat;
										changed = true;
									}
									Space(2);

									newFloat = EditorGUI.FloatField(GUIRect(0, HEIGHT), att.Anchor.z);
									if (newFloat != att.Anchor.z) {
										att.Anchor.z = newFloat;
										changed = true;
									}

								});

								Space(HEIGHT * 2 + 4);

								break;
							case Core_CharacterGeneration.PlacementType.Stretch:
							case Core_CharacterGeneration.PlacementType.Repeat:
								// Stretch
								// Border Min
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Border Min");

									int newInt;
									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMinX);
									if (newInt != att.BorderMinX) {
										att.BorderMinX = Mathf.Clamp(newInt, 0, att.SizeX - att.BorderMaxX);
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMinY);
									if (newInt != att.BorderMinY) {
										att.BorderMinY = Mathf.Clamp(newInt, 0, att.SizeY - att.BorderMaxY);
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMinZ);
									if (newInt != att.BorderMinZ) {
										att.BorderMinZ = Mathf.Clamp(newInt, 0, att.SizeZ - att.BorderMaxZ);
										changed = true;
									}
								});
								Space(2);

								// Border Max
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Border Max");

									int newInt;
									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMaxX);
									if (newInt != att.BorderMaxX) {
										att.BorderMaxX = Mathf.Clamp(newInt, 0, att.SizeX - att.BorderMinX);
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMaxY);
									if (newInt != att.BorderMaxY) {
										att.BorderMaxY = Mathf.Clamp(newInt, 0, att.SizeY - att.BorderMinY);
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.BorderMaxZ);
									if (newInt != att.BorderMaxZ) {
										att.BorderMaxZ = Mathf.Clamp(newInt, 0, att.SizeZ - att.BorderMinZ);
										changed = true;
									}
								});
								Space(2);

								// Offset Min
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Offset Min");

									int newInt;
									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMinX);
									if (newInt != att.OffsetMinX) {
										att.OffsetMinX = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMinY);
									if (newInt != att.OffsetMinY) {
										att.OffsetMinY = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMinZ);
									if (newInt != att.OffsetMinZ) {
										att.OffsetMinZ = newInt;
										changed = true;
									}
								});
								Space(2);

								// Offset Max
								LayoutH(() => {
									EditorGUI.LabelField(GUIRect(LABEL_WIDTH, HEIGHT), "Offset Max");

									int newInt;
									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMaxX);
									if (newInt != att.OffsetMaxX) {
										att.OffsetMaxX = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMaxY);
									if (newInt != att.OffsetMaxY) {
										att.OffsetMaxY = newInt;
										changed = true;
									}
									Space(2);

									newInt = EditorGUI.IntField(GUIRect(0, HEIGHT), att.OffsetMaxZ);
									if (newInt != att.OffsetMaxZ) {
										att.OffsetMaxZ = newInt;
										changed = true;
									}
								});
								break;
						}
						Space(8);

					}, false, new GUIStyle() {
						fixedWidth = LAYOUT_WIDTH,
						padding = new RectOffset(8, 8, 4, 4),
					});
					Space(4);


					// Thumbnail
					LayoutV(() => {
						Space(6);
						GUI.Label(GUIRect(THUMB_SIZE, HEIGHT), "Thumbnail");
						Space(2);
						var thumb = CharacterAttachmentThumbnailMap.ContainsKey(att) ? CharacterAttachmentThumbnailMap[att] : null;
						if (!thumb) {
							thumb = Texture2D.blackTexture;
						}
						var rect = GUIRect(THUMB_SIZE, THUMB_SIZE);
						GUI.Box(rect, GUIContent.none);
						float newSize = Mathf.Min(THUMB_SIZE, Mathf.Max(att.SizeX * 12, att.SizeY * 12));
						rect.x += (THUMB_SIZE - newSize) / 2f;
						rect.y += (THUMB_SIZE - newSize) / 2f;
						rect.width = newSize;
						rect.height = newSize;
						GUI.DrawTexture(rect, thumb, ScaleMode.ScaleToFit);
						Space(6);
					}, false, new GUIStyle() {
						fixedWidth = THUMB_SIZE + 4,
					});


				}, true);


				// Buttons
				LayoutH(() => {

					const int BUTTON_WIDTH = 62;

					Space(96);

					// Duplicate Button
					if (GUI.Button(GUIRect(BUTTON_WIDTH, HEIGHT), "Duplicate", EditorStyles.miniButtonLeft)) {
						if (att != null) {
							attachments.Add(att.GetCopy());
							ReloadCharacterAttachmentThumbnailMapFor(attachments[attachments.Count - 1]);
							changed = true;
							Repaint();
						}
					}

					// Save Button
					if (GUI.Button(GUIRect(BUTTON_WIDTH, HEIGHT), "Save As", EditorStyles.miniButtonMid)) {
						if (att != null && att.Voxels != null && att.Voxels.Length != 0) {
							try {
								var data = att.GetVoxelData();
								if (data) {
									var path = Util.FixedRelativePath(EditorUtility.SaveFilePanel("Save Attachment As Vox", "Assets", att.Name, "vox"));
									if (!string.IsNullOrEmpty(path)) {
										var bytes = VoxelFile.GetVoxelByte(data, true);
										Util.ByteToFile(bytes, path);
										AssetDatabase.SaveAssets();
										AssetDatabase.Refresh();
									}
								}
							} catch { }
						}
					}

					// Load Button
					if (GUI.Button(GUIRect(BUTTON_WIDTH, HEIGHT), "Load", EditorStyles.miniButtonMid)) {
						var data = TryPickVoxelData();
						if (data && data.Voxels != null && data.Voxels.Count > 0) {
							att.LoadFrom(data, 0);
							ReloadCharacterAttachmentThumbnailMapFor(att);
							changed = true;
							Repaint();
						}
					}

					// Delete Button
					var oldE = GUI.enabled;
					GUI.enabled = CurrentAttachmentIndex >= 0 && CurrentAttachmentIndex < attachments.Count;
					if (GUI.Button(GUIRect(BUTTON_WIDTH, HEIGHT), "Delete", EditorStyles.miniButtonRight)) {
						if (CurrentAttachmentIndex >= 0 && CurrentAttachmentIndex < attachments.Count) {
							if (att != null && Util.Dialog("", "Delete attachment " + att.Name + "?", "Delete", "Cancel")) {
								attachments.RemoveAt(CurrentAttachmentIndex);
								CurrentAttachmentIndex = Mathf.Clamp(CurrentAttachmentIndex, -1, attachments.Count - 1);
								changed = true;
								Repaint();
							}
						}
					}
					GUI.enabled = oldE;
				});

			});

			Space(8);
			return changed;

		}




		#endregion




		#region --- LGC ---




		private void ReloadCharacterAttachmentThumbnailMap (bool forceReload = false) {
			if (forceReload) {
				CharacterAttachmentThumbnailMap.Clear();
			}
			if (CGConfig == null || CGConfig.Attachments == null || CGConfig.Attachments.Count == 0) { return; }
			for (int i = 0; i < CGConfig.Attachments.Count; i++) {
				var att = CGConfig.Attachments[i];
				if (att == null) { continue; }
				if (!CharacterAttachmentThumbnailMap.ContainsKey(att)) {
					ReloadCharacterAttachmentThumbnailMapFor(att);
				}
			}
		}



		private void ReloadCharacterAttachmentThumbnailMapFor (Core_CharacterGeneration.AttachmentData att) {
			var voxelData = att.GetVoxelData();
			if (!voxelData || voxelData.Voxels == null || voxelData.Voxels.Count == 0) { return; }
			if (!CharacterAttachmentThumbnailMap.ContainsKey(att)) {
				CharacterAttachmentThumbnailMap.Add(att, voxelData.GetThumbnail(0, false));
			} else {
				CharacterAttachmentThumbnailMap[att] = voxelData.GetThumbnail(0, false);
			}
		}



		private CharacterOrganGroup GetOrganGroup (Core_CharacterGeneration.OrganType organ) {
			switch (organ) {
				case Core_CharacterGeneration.OrganType.Head:
				case Core_CharacterGeneration.OrganType.Neck:
					return CharacterOrganGroup.Head;
				default:
				case Core_CharacterGeneration.OrganType.Body:
				case Core_CharacterGeneration.OrganType.Hip:
					return CharacterOrganGroup.Body;
				case Core_CharacterGeneration.OrganType.ArmU_L:
				case Core_CharacterGeneration.OrganType.ArmD_L:
				case Core_CharacterGeneration.OrganType.Hand_L:
					return CharacterOrganGroup.ArmL;
				case Core_CharacterGeneration.OrganType.ArmU_R:
				case Core_CharacterGeneration.OrganType.ArmD_R:
				case Core_CharacterGeneration.OrganType.Hand_R:
					return CharacterOrganGroup.ArmR;
				case Core_CharacterGeneration.OrganType.LegU_L:
				case Core_CharacterGeneration.OrganType.LegD_L:
				case Core_CharacterGeneration.OrganType.Foot_L:
					return CharacterOrganGroup.LegL;
				case Core_CharacterGeneration.OrganType.LegU_R:
				case Core_CharacterGeneration.OrganType.LegD_R:
				case Core_CharacterGeneration.OrganType.Foot_R:
					return CharacterOrganGroup.LegR;
			}
		}



		private Core_CharacterGeneration.OrganType GetFirstOrganInGroup (CharacterOrganGroup group) {
			int len = System.Enum.GetNames(typeof(Core_CharacterGeneration.OrganType)).Length;
			for (int i = 0; i < len; i++) {
				var o = (Core_CharacterGeneration.OrganType)i;
				if (GetOrganGroup(o) == group) {
					return o;
				}
			}
			return Core_CharacterGeneration.OrganType.Head;
		}



		private bool CheckAttachmentOrganGroup (Core_CharacterGeneration.AttachmentData att, CharacterOrganGroup group) {
			if (att != null && att.TargetMask != 0) {
				for (int i = 0; i < Core_CharacterGeneration.ORGAN_TYPE_LENGTH; i++) {
					if (att.CheckTargetOrgan(i) && GetOrganGroup((Core_CharacterGeneration.OrganType)i) == CurrentOrganGroup) {
						return true;
					}
				}
			}
			return false;
		}




		#endregion



	}
}