namespace MagicaVoxelToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using UnityEditor.Animations;
	using System.Linq;
	using Saving;


	// Skeletal Part
	public partial class VoxelEditorWindow {





		#region --- SUB ---



		public enum BindingType {

			HeadNeck_Nod,
			HeadNeck_Tilt,
			HeadNeck_Turn,

			ChestSpine_Nod,
			ChestSpine_Tilt,
			ChestSpine_Turn,

			Root_Nod,
			Root_Tilt,
			Root_Turn,

			Root_X,
			Root_Y,
			Root_Z,

			L_Arm_UD,
			L_Arm_FB,
			L_Arm_Twist,

			L_Forearm_Stretch,
			L_Forearm_Tilt,
			L_Forearm_Twist,

			R_Arm_UD,
			R_Arm_FB,
			R_Arm_Twist,

			R_Forearm_Stretch,
			R_Forearm_Tilt,
			R_Forearm_Twist,

			L_Leg_UD,
			L_Leg_FB,
			L_Leg_Twist,

			L_ForeLeg_Stretch,
			L_ForeLeg_Tilt,
			L_ForeLeg_Twist,

			R_Leg_UD,
			R_Leg_FB,
			R_Leg_Twist,

			R_ForeLeg_Stretch,
			R_ForeLeg_Tilt,
			R_ForeLeg_Twist,


		}



		public enum SkeletalToolkitType {
			Ring = 0,
			Axis = 1,

		}



		#endregion



		#region --- VAR ---



		// Global
		private const int SKELETAL_LEFT_PANEL_WIDTH = 300;
		private const int SKELETAL_BINDING_HEIGHT = 20;
		private const int SKELETAL_TOOLKIT_COUNT = 2;
		private const int SKELETAL_HANDLE_TYPE_COUNT = 36;
		private const string SKELETAL_HANDLE_NAME = "Voxel To Unity Skeletal Handle";
		private readonly Color[] SKELETAL_RING_HANDLE_COLOR = new Color[3] {
			new Color(0.8f, 0.8f, 0.8f),
			new Color(1, 1, 1),
			new Color(0.3f, 0.8f, 0.4f),
		};
		private readonly Color[] SKELETAL_AXIS_HANDLE_COLOR = new Color[] {
			new Color(242f / 255f, 74f / 255f, 37f / 255f, 0.618f),		// x n
			new Color (115f / 255f, 191f / 255f, 53f / 255f, 0.618f),	// y n
			new Color (66f / 255f, 140f / 255f, 242f / 255f, 0.618f),	// z n
			new Color(0.8f, 0.8f, 0.8f),	// x h
			new Color (0.8f, 0.8f, 0.8f),	// y h
			new Color (0.8f, 0.8f, 0.8f),	// z h
			new Color(1f, 1f, 1f),	// x d
			new Color(1f, 1f, 1f),	// y d
			new Color(1f, 1f, 1f),	// z d
		};
		private readonly HumanBodyBones[][] BindingTypeParent = new HumanBodyBones[SKELETAL_TOOLKIT_COUNT][] {
			// Ring
			new HumanBodyBones[SKELETAL_HANDLE_TYPE_COUNT]{

				HumanBodyBones.Head, HumanBodyBones.Head, HumanBodyBones.Head,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,

				HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftUpperArm,
				HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftLowerArm,
				HumanBodyBones.RightUpperArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightUpperArm,
				HumanBodyBones.RightLowerArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightLowerArm,

				HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftUpperLeg,
				HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftLowerLeg,
				HumanBodyBones.RightUpperLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightUpperLeg,
				HumanBodyBones.RightLowerLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightLowerLeg,

			},
			// Axis
			new HumanBodyBones[SKELETAL_HANDLE_TYPE_COUNT]{

				HumanBodyBones.Head, HumanBodyBones.Head, HumanBodyBones.Head,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,
				HumanBodyBones.Chest, HumanBodyBones.Chest, HumanBodyBones.Chest,

				HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftLowerArm,
				HumanBodyBones.LeftHand, HumanBodyBones.LeftHand, HumanBodyBones.LeftHand,
				HumanBodyBones.RightLowerArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightLowerArm,
				HumanBodyBones.RightHand, HumanBodyBones.RightHand, HumanBodyBones.RightHand,

				HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftLowerLeg,
				HumanBodyBones.LeftFoot, HumanBodyBones.LeftFoot, HumanBodyBones.LeftFoot,
				HumanBodyBones.RightLowerLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightLowerLeg,
				HumanBodyBones.RightFoot, HumanBodyBones.RightFoot, HumanBodyBones.RightFoot,

			},
		};
		private readonly EditorCurveBinding[][] BindingTypeBinding = new EditorCurveBinding[SKELETAL_HANDLE_TYPE_COUNT][] {

			// Head Neck
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Head Nod Down-Up", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Neck Nod Down-Up", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Head Tilt Left-Right", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Neck Tilt Left-Right", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Head Turn Left-Right", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Neck Turn Left-Right", type = typeof(Animator) },
			},

			
			// Spine Chest
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Spine Front-Back", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Chest Front-Back", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Spine Left-Right", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Chest Left-Right", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Spine Twist Left-Right", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Chest Twist Left-Right", type = typeof(Animator) },
			},

			
			// Root Rot
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.x", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.y", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.z", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.w", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.x", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.y", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.z", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.w", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.x", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.y", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.z", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "RootQ.w", type = typeof(Animator) },
			},


			// Root Pos
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootT.x", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootT.y", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "RootT.z", type = typeof(Animator) },
			},

			
			
			// L Arm
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Arm Down-Up", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Arm Front-Back", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Arm Twist In-Out", type = typeof(Animator) },
			},
			

			// L Forearm
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Forearm Stretch", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Left Hand In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Arm Twist In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Forearm Twist In-Out", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Left Hand Down-Up", type = typeof(Animator) },
			},


			// R Arm
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Arm Down-Up", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Arm Front-Back", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Arm Twist In-Out", type = typeof(Animator) },
			},


			// R Forearm
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Forearm Stretch", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Right Hand In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Arm Twist In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Forearm Twist In-Out", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Right Hand Down-Up", type = typeof(Animator) },
			},

			

			// L Leg
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Upper Leg In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Upper Leg Front-Back", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Upper Leg Twist In-Out", type = typeof(Animator) },
			},

			
			// L Foreleg
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Lower Leg Stretch", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Left Foot Up-Down", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Left Lower Leg Twist In-Out", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Left Foot Twist In-Out", type = typeof(Animator) },
			},


			// R Leg
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Upper Leg In-Out", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Upper Leg Front-Back", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Upper Leg Twist In-Out", type = typeof(Animator) },
			},


			// R Foreleg
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Lower Leg Stretch", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Right Foot Up-Down", type = typeof(Animator) },
			},
			new EditorCurveBinding[]{},
			new EditorCurveBinding[]{
				new EditorCurveBinding(){ path = "", propertyName = "Right Lower Leg Twist In-Out", type = typeof(Animator) },
				new EditorCurveBinding(){ path = "", propertyName = "Right Foot Twist In-Out", type = typeof(Animator) },
			},


		};
		private Quaternion[] SkeletalTPoseBonesRotation = new Quaternion[0];
		private readonly int[][] SKELETAL_HANDLE_MOUSE_WORLD_AXIS = new int[SKELETAL_TOOLKIT_COUNT][] {
			// 1 2 3 -1 -2 -3
			// x y z -x -y -z
			// Ring
			new int[SKELETAL_HANDLE_TYPE_COUNT]{
				// HeadNeck
				-2, -1, -1,
				// Chest Spine
				-2, -1, -1,
				// Root
				2, 1, -1,
				1, 1, 1,
				// L Arm
				-2, 3, 3,
				// L FArm
				3, 3, 3,
				// R Arm
				-2, 3, 3,
				// R FArm
				3, 3, 3,
				// L Leg
				1, 3, 3,
				// L FLeg
				-3, -3, -3,
				// R Leg
				-1, 3, 3,
				// R FLeg
				-3, -3, -3,
			},
			// Axis
			new int[SKELETAL_HANDLE_TYPE_COUNT]{
				// HeadNeck
				-2, -1, -1,
				// Chest Spine
				-2, -1, -1,
				// Root
				2, 1, -1,
				1, 1, 1,
				// L Arm
				-2, 3, 3,
				// L FArm
				3, 3, 3,
				// R Arm
				-2, 3, 3,
				// R FArm
				3, 3, 3,
				// L Leg
				1, 3, 3,
				// L FLeg
				-3, -3, -3,
				// R Leg
				-1, 3, 3,
				// R FLeg
				-3, -3, -3,
			},
		};


		// Short Cut
		private bool IsSkeletaling {
			get {
				return CurrentEditorMode == EditorMode.Skeletal;
			}
		}

		private bool SkeletalAnimationLoop {
			get {
				return SkeletalAnimation && (SkeletalAnimation.wrapMode == WrapMode.Loop || SkeletalAnimation.wrapMode == WrapMode.Default);
			}
		}


		// Data
		private static Mesh[] _SkeletalHandleMesh = new Mesh[6];
		private AnimationClip SkeletalAnimation = null;
		private Animator SkeletalAni = null;
		private Transform[] SkeletalHandles = new Transform[SKELETAL_HANDLE_TYPE_COUNT];
		private Transform SkeletalHoveringHandle = null;
		private Transform SkeletalDraggingHandle = null;
		private Bounds? SkeletalCharacterBounds = null;
		private BindingType? SkeletalDraggingType = null;
		private SkeletalToolkitType SkeletalToolkit = SkeletalToolkitType.Axis;
		private string SkeletalAnimationPath = "";
		private float SkeletalAnimationTime01 = 0f;
		private float PrevSkeletalAnimationTime01 = -1f;
		private float SkeletalFrameWidth = 12;
		private float FrameRate = -1f;
		private int SkeletalStateHash = -1;
		private bool IsSkeletalPlaying = false;
		private bool RulerDragging = false;


		// Saving
		private EditorSavingFloat SkeletalHandleDistanceMuti = new EditorSavingFloat("VEditor.SkeletalHandleDistanceMuti", 1f);
		private EditorSavingFloat SkeletalHandleDistanceSensitivity = new EditorSavingFloat("VEditor.SkeletalHandleDistanceSensitivity", 2f);
		private EditorSavingBool ShowSkeletalHipHandles = new EditorSavingBool("VEditor.ShowSkeletalHipHandles", true);
		private EditorSavingBool ShowSkeletalArmHandles = new EditorSavingBool("VEditor.ShowSkeletalArmHandles", true);
		private EditorSavingBool ShowSkeletalLegHandles = new EditorSavingBool("VEditor.ShowSkeletalLegHandles", true);


		#endregion




		#region --- API ---



		public void OpenSkeletal () {
			CurrentModelIndex = 0;
			VoxelFilePath = "";
			Data = VoxelData.CreateNewData();
			if (Data) {
				// Node for Debug Data
				foreach (var gp in Data.Groups) {
					if (!NodeOpen.ContainsKey(gp.Key)) {
						NodeOpen.Add(gp.Key, false);
					}
				}
				// Size
				if (Data.Voxels.Count > 0) {
					Data.Voxels[0] = new int[10, 20, 10];
				}
			}
			SwitchModel(CurrentModelIndex);
			ClearScene();
			DataDirty = false;

			// Skeletal
			PickSkeletalCharacter(null);
			IsSkeletalPlaying = false;
			SetSkeletalAnimatorTime(0f);
			SkeletalAnimation = null;

			if (Selection.activeObject is AnimationClip) {
				ImportSkeletalAnimation(Selection.activeObject as AnimationClip);
				Repaint();
			}
		}



		#endregion




		#region --- MSG ---




		private void SkeletalBarGUI () {


			if (!SkeletalAni) {
				PickSkeletalCharacter(null);
			}

			if (!SkeletalAnimation || !SkeletalAni) { return; }


			const int BAR_HEIGHT = 18;
			const int INT_FIELD_WIDTH = 28;
			const int RULER_ITEM_HEIGHT = 18;


			var layoutStyle = new GUIStyle() {
				margin = new RectOffset(),
				padding = new RectOffset(),
			};
			var labelStyle = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft,
				fontSize = 8,
			};


			float duration = SkeletalAnimation.length;
			float newF = SkeletalAnimation.frameRate;
			if (FrameRate != newF) {
				FrameRate = newF;
				ResetSkeletalAnimation();
			}
			int frameCount = (int)(duration * FrameRate);
			float allFramesWidth = frameCount * SkeletalFrameWidth;

			SkeletalFrameWidth = Mathf.Clamp(
				(EditorGUIUtility.currentViewWidth - SKELETAL_LEFT_PANEL_WIDTH - 24) / frameCount,
				2, 12
			);


			LayoutH(() => {


				var oldC = GUI.color;

				var barRect = GUIRect(0, BAR_HEIGHT);
				barRect.x = 0;
				GUI.Box(barRect, GUIContent.none, EditorStyles.toolbar);


				// Loop
				var rect = new Rect(
					barRect.x + SKELETAL_LEFT_PANEL_WIDTH - INT_FIELD_WIDTH - 54,
					barRect.y,
					36,
					barRect.height
				);
				bool loop = SkeletalAnimation.wrapMode == WrapMode.Loop || SkeletalAnimation.wrapMode == WrapMode.Default;
				if (GUI.Button(rect, "Loop", EditorStyles.toolbarButton)) {
					loop = !loop;
					SkeletalAnimation.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
				}
				if (loop) {
					GUI.color = new Color(1, 1, 1, 0.1f);
					GUI.DrawTexture(rect, Texture2D.whiteTexture);
					GUI.color = oldC;
				}


				// Frame Num
				rect.x = barRect.x + SKELETAL_LEFT_PANEL_WIDTH - INT_FIELD_WIDTH - 8;
				rect.y += 2;
				rect.width = INT_FIELD_WIDTH;
				rect.height -= 4;
				int currentFrame = Mathf.Clamp(Mathf.RoundToInt(SkeletalAnimationTime01 * frameCount), 0, frameCount);
				int newFrame = EditorGUI.DelayedIntField(rect, currentFrame, EditorStyles.toolbarTextField);
				if (currentFrame != newFrame) {
					SetSkeletalAnimatorTime((float)newFrame / frameCount, true);
				}

				// Animation Path Label
				rect = barRect;
				rect.x += 6;
				rect.y += 2;
				rect.width = 180;
				GUI.Label(
					rect,
					SkeletalAnimationPath.Length > 36 ?
					"..." + SkeletalAnimationPath.Substring(SkeletalAnimationPath.Length - 36, 36) :
					SkeletalAnimationPath,
					EditorStyles.miniLabel
				);


				// Drag To Move
				if (Event.current.button <= 1) {
					// Left
					var type = Event.current.type;
					switch (type) {
						case EventType.MouseDown:
						case EventType.MouseDrag:

						// Down or Drag
						var r = new Rect(barRect) {
							x = 0,
							width = barRect.width,
						};

						if (type == EventType.MouseDown) {
							RulerDragging = (Event.current.button == 1 && Event.current.control && ViewRect.Contains(Event.current.mousePosition)) || r.Contains(Event.current.mousePosition);
						}

						if (RulerDragging) {
							if (SkeletalAni.gameObject.activeSelf) {
								float x01 = Mathf.Clamp01(
									(Event.current.mousePosition.x - r.x - SKELETAL_LEFT_PANEL_WIDTH) / (int)allFramesWidth
								);
								IsSkeletalPlaying = false;
								SetSkeletalAnimatorTime(x01, !Event.current.alt);
							}
						}
						break;
						case EventType.MouseMove:
						case EventType.MouseUp:
						RulerDragging = false;
						break;
					}
				}


				// Ruler
				for (int frame = 0; frame <= frameCount; frame++) {

					bool isKey = frame % 5 == 0;
					float height = isKey ? 0.4f : 0.1f;

					// Line
					GUI.color = Color.grey;
					GUI.DrawTexture(
						new Rect(
							SKELETAL_LEFT_PANEL_WIDTH + frame * SkeletalFrameWidth,
							barRect.y + RULER_ITEM_HEIGHT * (1f - height),
							1,
							RULER_ITEM_HEIGHT * height
						),
						Texture2D.whiteTexture
					);

					// Label
					if (isKey) {
						GUI.color = Color.white * 0.85f;
						EditorGUI.LabelField(new Rect(
							SKELETAL_LEFT_PANEL_WIDTH + frame * SkeletalFrameWidth + 2,
							barRect.y,
							SkeletalFrameWidth * 5,
							RULER_ITEM_HEIGHT
						), frame.ToString(), labelStyle);
					}


				}
				GUI.color = oldC;



				// Needle GUI
				GUI.color = new Color(242f / 255f, 74f / 255f, 37f / 255f);
				rect.x = SKELETAL_LEFT_PANEL_WIDTH + SkeletalAnimationTime01 * allFramesWidth;
				rect.y = barRect.y;
				rect.width = 1;
				rect.height = BAR_HEIGHT;
				GUI.DrawTexture(rect, Texture2D.whiteTexture);
				GUI.color = oldC;



			}, false, layoutStyle);

		}



		private void SkeletalGUI () {

			// Check
			if (!SkeletalAni) { return; }


			if (SkeletalCharacterRoot.childCount == 0) {
				PickSkeletalCharacter(null);
				Repaint();
			}

			Space(42);

			if (SkeletalAnimation) {
				LayoutH(() => {
					GUIRect(0, 1);
					EditorGUI.HelpBox(GUIRect(220, 42), "Dock Animation Window Here", MessageType.Info);
					GUIRect(0, 1);
				});
			} else {
				LayoutH(() => {
					GUIRect(0, 1);
					EditorGUI.HelpBox(GUIRect(220, 42), "Select Animation File in Project View.", MessageType.Info);
					GUIRect(0, 1);
				});
			}



			// Bottom Left
			if (SkeletalAnimation) {

				float time = SkeletalAnimationTime01 * SkeletalAnimation.length;

				var bindingX = BindingTypeBinding[(int)BindingType.Root_X][0];
				var bindingY = BindingTypeBinding[(int)BindingType.Root_Y][0];
				var bindingZ = BindingTypeBinding[(int)BindingType.Root_Z][0];

				var curveX = AnimationUtility.GetEditorCurve(SkeletalAnimation, bindingX);
				var curveY = AnimationUtility.GetEditorCurve(SkeletalAnimation, bindingY);
				var curveZ = AnimationUtility.GetEditorCurve(SkeletalAnimation, bindingZ);

				float rootX = curveX != null ? curveX.Evaluate(time) : 0f;
				float rootY = curveY != null ? curveY.Evaluate(time) : 0f;
				float rootZ = curveZ != null ? curveZ.Evaluate(time) : 0f;


				var rect = new Rect(ViewRect) {
					width = 42,
					height = 18,
					y = ViewRect.max.y - 20 * 3,
				};
				rect.x += 6;
				GUI.Label(rect, "Root.x");
				rect.y += rect.height + 2;
				GUI.Label(rect, "Root.y");
				rect.y += rect.height + 2;
				GUI.Label(rect, "Root.z");

				rect = new Rect(ViewRect) {
					x = 48,
					width = 36,
					height = 18,
					y = ViewRect.max.y - 20 * 3,
				};
				var newX = EditorGUI.DelayedFloatField(rect, rootX);
				rect.y += rect.height + 2;
				var newY = EditorGUI.DelayedFloatField(rect, rootY);
				rect.y += rect.height + 2;
				var newZ = EditorGUI.DelayedFloatField(rect, rootZ);


				bool needRefresh = false;
				if (newX != rootX) {
					SetSkeletalCurve(bindingX, newX, false, true);
					needRefresh = true;
				}
				if (newY != rootY) {
					SetSkeletalCurve(bindingY, newY, false, true);
					needRefresh = true;
				}
				if (newZ != rootZ) {
					SetSkeletalCurve(bindingZ, newZ, false, true);
					needRefresh = true;
				}


				if (needRefresh) {
					if (SkeletalAni) {
						SkeletalAni.Rebind();
					}
					UpdateSkeletalAnimator();
					Repaint();
				}

			}



			// Top Left
			{
				var rect = new Rect() {
					x = 24,
					y = ViewRect.y + 18,
					width = 52,
					height = 20,
				};
				if (GUI.Button(rect, "Body", ShowSkeletalHipHandles ? new GUIStyle(EditorStyles.miniButtonLeft) { normal = EditorStyles.miniButtonLeft.active } : EditorStyles.miniButtonLeft)) {
					ShowSkeletalHipHandles.Value = !ShowSkeletalHipHandles;
					RefreshSkeletalHandleActive();
					Repaint();
				}
				rect.x += rect.width;
				if (GUI.Button(rect, "Arm", ShowSkeletalArmHandles ? new GUIStyle(EditorStyles.miniButtonMid) { normal = EditorStyles.miniButtonMid.active } : EditorStyles.miniButtonMid)) {
					ShowSkeletalArmHandles.Value = !ShowSkeletalArmHandles;
					RefreshSkeletalHandleActive();
					Repaint();
				}
				rect.x += rect.width;
				if (GUI.Button(rect, "Leg", ShowSkeletalLegHandles ? new GUIStyle(EditorStyles.miniButtonRight) { normal = EditorStyles.miniButtonRight.active } : EditorStyles.miniButtonRight)) {
					ShowSkeletalLegHandles.Value = !ShowSkeletalLegHandles;
					RefreshSkeletalHandleActive();
					Repaint();
				}

				// Toolkit
				rect.y += rect.height + 6;
				rect.x = 24;
				for (int i = 0; i < SKELETAL_TOOLKIT_COUNT; i++) {
					var type = (SkeletalToolkitType)i;
					var style = i == 0 ? EditorStyles.miniButtonLeft : i == SKELETAL_TOOLKIT_COUNT - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButton;
					if (type == SkeletalToolkit) {
						style = new GUIStyle(style) {
							normal = style.active,
						};
					}
					if (GUI.Button(rect, type.ToString(), style)) {
						if (type != SkeletalToolkit) {
							SkeletalToolkit = type;
							IsSkeletalPlaying = false;
							RespawnAllSkeletalHandles();
							SkeletalRefresh();
							Repaint();
						}
					}
					rect.x += rect.width;
				}
			}



		}



		private void SkeletalViewGUI () {

			if (!SkeletalAnimation || !SkeletalAni || !SkeletalAni.avatar) { return; }

			bool needRepaint = false;
			Vector3 hitPoint = Vector3.zero;

			// Ray Cast
			switch (Event.current.type) {
				case EventType.MouseMove:
				case EventType.MouseDrag:
				case EventType.MouseDown:
				case EventType.MouseUp:

				if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) {
					if (Event.current.button != 0) {
						break;
					}
				}

					// Hover
					{
					bool hited = false;
					if (ViewRect.Contains(Event.current.mousePosition)) {
						ViewCastHit((hit) => {
							// Hit
							if (hit.transform.name == SKELETAL_HANDLE_NAME) {
								if (hit.transform != SkeletalHoveringHandle) {
									if (SkeletalHoveringHandle && SkeletalHoveringHandle != SkeletalDraggingHandle) {
										SetSkeletalHandleColor(SkeletalHoveringHandle, GetSkeletalHandleColor(SkeletalHoveringHandle, 0));
									}
									SkeletalHoveringHandle = hit.transform;
									if (SkeletalHoveringHandle != SkeletalDraggingHandle) {
										SetSkeletalHandleColor(SkeletalHoveringHandle, GetSkeletalHandleColor(SkeletalHoveringHandle, 1));
									}
									needRepaint = true;
								}
								hitPoint = hit.point;
								hited = true;
							}
						});
					}
					// None Hited
					if (!hited) {
						if (SkeletalHoveringHandle && (Event.current.type != EventType.MouseDrag || SkeletalHoveringHandle != SkeletalDraggingHandle)) {
							SetSkeletalHandleColor(SkeletalHoveringHandle, GetSkeletalHandleColor(SkeletalHoveringHandle, 0));
							SkeletalHoveringHandle = null;
							needRepaint = true;
						}
					}
				}

					// Drag
					{

					if (Event.current.type != EventType.MouseDrag) {
						// Move / Down
						if (SkeletalDraggingHandle) {
							SetSkeletalHandleColor(
								SkeletalDraggingHandle,
								SkeletalDraggingHandle == SkeletalHoveringHandle ?
									GetSkeletalHandleColor(SkeletalDraggingHandle, 1) :
									GetSkeletalHandleColor(SkeletalDraggingHandle, 0)
							);
							SkeletalDraggingHandle = null;
							needRepaint = true;
						}

					}

					if (Event.current.type == EventType.MouseDown) {
						// Down
						if (SkeletalHoveringHandle) {
							SkeletalDraggingHandle = SkeletalHoveringHandle;
							SkeletalDraggingType = null;
							for (int i = 0; SkeletalHandles != null && i < SkeletalHandles.Length; i++) {
								if (SkeletalHandles[i] == SkeletalDraggingHandle) {
									SkeletalDraggingType = (BindingType)i;
									break;
								}
							}
							SetSkeletalHandleColor(SkeletalDraggingHandle, GetSkeletalHandleColor(SkeletalDraggingHandle, 2));
							Undo.RegisterCompleteObjectUndo(SkeletalAnimation, "Skeletal Animation");
						}
					} else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0) {
						// Left Drag
						if (SkeletalDraggingHandle && SkeletalDraggingType.HasValue) {
							var type = SkeletalDraggingType.Value;
							var oldScreenPos = GetGUIScreenPosition(Event.current.mousePosition - Event.current.delta);
							var screenPos = GetGUIScreenPosition(Event.current.mousePosition);
							Vector3 worldDelta = Camera.ScreenToWorldPoint(oldScreenPos) - Camera.ScreenToWorldPoint(screenPos);
							int subIndex = type == BindingType.Root_Tilt ? 2 : type == BindingType.Root_Turn ? 1 : 0;
							int axis = SKELETAL_HANDLE_MOUSE_WORLD_AXIS[(int)SkeletalToolkit][(int)type];
							float delta = worldDelta[Mathf.Abs(axis) - 1] * (axis > 0 ? 1f : -1f);
							float sen = SkeletalHandleDistanceSensitivity * 0.1f;
							SkeletalHandleDrag(type, delta * sen, subIndex);
							SetDataDirty();
							if (SkeletalAni) {
								SkeletalAni.Rebind();
								UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
							}
							UpdateSkeletalAnimator();
							needRepaint = true;
						}
					}
				}
				break;
			}


			if (needRepaint) {
				Repaint();
			}

		}



		private void SkeletalPanelGUI () {

			const int WIDTH = 180;
			const int HEIGHT = 30;
			const int GAP = 3;

			var buttonStyle = new GUIStyle(GUI.skin.button) {
				fontSize = 11,
			};
			var buttonStyleMid = new GUIStyle(GUI.skin.button) {
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



			// Play Pause
			GUI.enabled = SkeletalAni;
			var oldRect = rect;
			LayoutH(() => {
				rect.width = HEIGHT;
				if (GUI.RepeatButton(rect, "◀", EditorStyles.miniButtonLeft)) {
					IsSkeletalPlaying = false;
					float newTime = SkeletalAnimationTime01 - SkeletalAnimation.length / FrameRate / 2f;
					SetSkeletalAnimatorTime(newTime < 0f ? 1f : newTime);
				}
				rect.x += HEIGHT;
				if (GUI.RepeatButton(rect, "▶", EditorStyles.miniButtonMid)) {
					IsSkeletalPlaying = false;
					float newTime = SkeletalAnimationTime01 + SkeletalAnimation.length / FrameRate / 2f;
					SetSkeletalAnimatorTime(newTime > 1f ? 0f : newTime);
				}
				rect.x += HEIGHT;
				rect.width = WIDTH - HEIGHT * 2;
				if (GUI.Button(rect, IsSkeletalPlaying ? "Pause" : "Play", buttonStyleMid)) {
					IsSkeletalPlaying = !IsSkeletalPlaying;
					if (IsSkeletalPlaying && !SkeletalAnimationLoop) {
						SetSkeletalAnimatorTime(0f);
					}
				}
			});
			rect = oldRect;
			rect.y -= HEIGHT + GAP;



			// Refresh
			GUI.enabled = SkeletalAni;
			oldRect = rect;
			LayoutH(() => {
				rect.width = HEIGHT;
				if (GUI.Button(rect, "◁", EditorStyles.miniButtonLeft)) {
					IsSkeletalPlaying = false;
					float newTime = SkeletalAnimationTime01 - 2f * SkeletalAnimation.length / FrameRate;
					SetSkeletalAnimatorTime(newTime < 0f ? 1f : newTime, true);
				}
				rect.x += HEIGHT;
				if (GUI.Button(rect, "▷", EditorStyles.miniButtonMid)) {
					IsSkeletalPlaying = false;
					float newTime = SkeletalAnimationTime01 + 2f * SkeletalAnimation.length / FrameRate;
					SetSkeletalAnimatorTime(newTime > 1f ? 0f : newTime, true);
				}
				rect.x += HEIGHT;
				rect.width = WIDTH - HEIGHT * 2;
				if (GUI.Button(rect, "Refresh", buttonStyleMid)) {
					if (SkeletalAni) {
						SkeletalRefresh();
						Repaint();
					}
				}
			});
			rect = oldRect;
			rect.y -= HEIGHT + GAP;



			// Pick Character
			GUI.enabled = true;
			oldRect = rect;
			LayoutH(() => {
				rect.width *= 0.5f;
				if (GUI.Button(rect, "Pick Char", buttonStyle)) {
					var path = Util.FixedRelativePath(EditorUtility.OpenFilePanel("Pick Character", "Assets", "prefab"));
					if (!string.IsNullOrEmpty(path)) {
						var character = AssetDatabase.LoadAssetAtPath<GameObject>(path);
						if (character) {
							PickSkeletalCharacter(character.transform);
						}
					}
				}
				rect.x += rect.width;
				if (GUI.Button(rect, "Default Char", buttonStyleMid)) {
					PickSkeletalCharacter(null);
				}
			});
			rect = oldRect;
			rect.y -= HEIGHT + GAP;



			// Export
			GUI.enabled = SkeletalCharacterRoot && SkeletalAnimation;
			if (GUI.Button(rect, "   Export Animation", buttonStyle)) {
				var path = EditorUtility.SaveFilePanel("Export Animation", "Assets", SkeletalAnimation.name, "anim");
				if (!string.IsNullOrEmpty(path)) {
					ExportSkeletalAnimation(path);
				}
				Repaint();
			}
			GUI.Label(rect, "  <color=#33cccc>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
			GUI.enabled = oldE;


			// Tip
			if (!SkeletalAnimation) {
				GUI.enabled = true;
				var oldC = GUI.color;
				GUI.color = Color.green;
				rect.x -= 40;
				rect.width += 40;
				GUI.Label(rect, "Select Animation File in Project View");
				GUI.color = oldC;
				GUI.enabled = oldE;
			}

		}



		private void SkeletalClose () {
			SkeletalAnimation = null;
			SkeletalHoveringHandle = null;
			SkeletalDraggingHandle = null;
			IsSkeletalPlaying = false;
			SkeletalHandles = null;
		}



		private void Update () {

			// Check
			if (focusedWindow != this || !SkeletalAnimation) { return; }

			if (SkeletalDraggingHandle) {
				IsSkeletalPlaying = false;
			}

			if (IsSkeletalPlaying) {
				var time01 = SkeletalAnimationTime01 + Mathf.Min(Time.deltaTime, 0.08f) / SkeletalAnimation.length;
				if (time01 > 1 && !SkeletalAnimationLoop) {
					IsSkeletalPlaying = false;
					SetSkeletalAnimatorTime(1f);
				} else {
					SetSkeletalAnimatorTime(time01 % 1f);
				}
			}


			if (PrevSkeletalAnimationTime01 != SkeletalAnimationTime01) {
				UpdateSkeletalAnimator();
				Repaint();
			}

		}




		#endregion




		#region --- LGC ---



		private void PickSkeletalCharacter (Transform prefab) {


			SkeletalAni = null;
			if (!prefab) {
				string rootPath = Util.GetRootPath(this);
				if (!string.IsNullOrEmpty(rootPath)) {
					string path = Util.CombinePaths(rootPath, "Data", "Default Skeletal Character.prefab");
					if (Util.FileExists(path)) {
						var _p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
						if (_p) {
							prefab = _p.transform;
						}
					}
				}
			}

			if (!prefab) { return; }

			Util.ClearChildrenImmediate(SkeletalCharacterRoot);
			Util.ClearChildrenImmediate(SkeletalHandleRoot);
			SkeletalHandles = null;

			Transform tf = Instantiate(prefab, SkeletalCharacterRoot);
			tf.localPosition = Vector3.zero;
			tf.localRotation = Quaternion.identity;
			tf.localScale = Vector3.one;


			// Renderer
			SkeletalCharacterBounds = null;
			var sr = tf.GetComponent<SkinnedMeshRenderer>();
			if (sr) {
				sr.gameObject.layer = LAYER_ID;
				var b = sr.bounds;
				b.Expand(sr.transform.lossyScale);
				SkeletalCharacterBounds = b;
			}
			var srs = tf.GetComponentsInChildren<SkinnedMeshRenderer>();
			if (!sr && srs.Length == 0) {
				Debug.LogWarning("[Voxel to Unity] No SkinnedMeshRenderer in " + tf.name);
				Util.ClearChildrenImmediate(SkeletalCharacterRoot);
				SkeletalHandles = null;
				Repaint();
				return;
			} else {
				for (int i = 0; i < srs.Length; i++) {
					srs[i].gameObject.layer = LAYER_ID;
					var b = srs[i].bounds;
					b.Expand(srs[i].transform.lossyScale);
					if (SkeletalCharacterBounds.HasValue) {
						SkeletalCharacterBounds.Value.Encapsulate(b);
					} else {
						SkeletalCharacterBounds = b;
					}
					if (!sr) {
						sr = srs[i];
					}
				}
			}
			var mr = tf.GetComponent<MeshRenderer>();
			if (mr) {
				mr.gameObject.layer = LAYER_ID;
			}
			var mrs = tf.GetComponentsInChildren<MeshRenderer>(true);
			for (int i = 0; i < mrs.Length; i++) {
				mrs[i].gameObject.layer = LAYER_ID;
			}


			// Animator
			Avatar avatar = null;
			var ani = tf.GetComponent<Animator>();
			if (!ani) {
				ani = tf.GetComponentInChildren<Animator>();
			}
			if (!ani) {
				ani = tf.gameObject.AddComponent<Animator>();
			} else {
				avatar = ani.avatar;
			}
			ani.updateMode = AnimatorUpdateMode.UnscaledTime;
			ani.applyRootMotion = false;

			// Avatar
			if (!avatar) {
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(prefab));
				foreach (var obj in objs) {
					if (obj is Avatar) {
						avatar = obj as Avatar;
						break;
					}
				}
			}
			if (!avatar) {
				Debug.LogWarning("[Voxel to Unity] No Avatar in " + tf.name);
				Util.ClearChildrenImmediate(SkeletalCharacterRoot);
				SkeletalHandles = null;
				Repaint();
				return;
			}
			ani.avatar = avatar;
			SkeletalAni = ani;


			// Set T Pose Position
			ResetSkeletalTPose();


			// Reset
			ResetSkeletalAnimation();


			// Resize
			Data.Voxels[0] = new int[
				(int)(SkeletalCharacterBounds.Value.size.x),
				(int)(SkeletalCharacterBounds.Value.size.y),
				(int)(SkeletalCharacterBounds.Value.size.z)
			];
			ModelSize = Data.GetModelSize(CurrentModelIndex);
			FixDirectionSignArrowPosition();
			SpawnBox();
			float maxSize = Mathf.Max(ModelSize.x, ModelSize.y, ModelSize.z, 12);
			CameraSizeMin = 0.5f;
			CameraSizeMax = maxSize * 2f;
			CameraRoot.rotation = Quaternion.Euler(36, 24 + 180, 0f);
			SetCameraSize((CameraSizeMin + CameraSizeMax) * 0.5f);
			RefreshCubeTransform();

			SetSkeletalAnimatorTime(SkeletalAnimationTime01);
			IsSkeletalPlaying = false;

			RespawnAllSkeletalHandles();


		}



		private void ImportSkeletalAnimation (AnimationClip ani) {
			if (ani) {
				SkeletalAnimation = ani;
				SkeletalAnimation.legacy = false;
				SkeletalAnimationPath = AssetDatabase.GetAssetPath(ani);
				ResetSkeletalAnimation();
				RespawnAllSkeletalHandles();
			}
			IsSkeletalPlaying = false;
		}



		private void ExportSkeletalAnimation (string path) {
			if (SkeletalAnimation != null) {
				try {
					var newAni = Util.CopyAnimation(SkeletalAnimation);
					AssetDatabase.CreateAsset(newAni, Util.FixedRelativePath(path));
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					ImportSkeletalAnimation(newAni);
				} catch { }
			}
			IsSkeletalPlaying = false;
		}



		private void SkeletalRefresh () {
			IsSkeletalPlaying = false;
			GotoSkeletalTPose();
			SkeletalAni.Rebind();
			SkeletalAni.gameObject.SetActive(false);
			SkeletalAni.gameObject.SetActive(true);
			UpdateSkeletalAnimator();
		}



		#endregion




		#region --- UTL ---



		// Animation
		private void ResetSkeletalAnimation () {
			if (!SkeletalAni || !SkeletalAnimation) { return; }
			var controller = new AnimatorController();
			controller.AddLayer("Default");
			controller.AddMotion(SkeletalAnimation, 0);
			controller.name = "Controller";
			SkeletalStateHash = Animator.StringToHash(SkeletalAnimation.name);
			AnimatorController.SetAnimatorController(SkeletalAni, controller);
			SkeletalAni.runtimeAnimatorController = controller;
			SkeletalAni.speed = 0f;
			SkeletalAni.logWarnings = false;
			SetSkeletalAnimatorTime(SkeletalAnimationTime01);
			RefreshSkeletalHandleRotation();
#if UNITY_2018_3_6
			Undo.ClearAll();
#endif
		}



		private void SetSkeletalAnimatorTime (float time01, bool snap = false) {
			time01 = Mathf.Clamp01(time01);
			if (snap) {
				int rate = (int)(SkeletalAnimation.length * SkeletalAnimation.frameRate);
				time01 = Mathf.Round(time01 * rate) / rate;
			}
			SkeletalAnimationTime01 = time01;
		}



		private void UpdateSkeletalAnimator () {
			if (SkeletalAni && SkeletalAnimation && SkeletalAni.isActiveAndEnabled && SkeletalAni.runtimeAnimatorController && SkeletalAni.isInitialized) {
				SkeletalAni.StartPlayback();
				SkeletalAni.Play(SkeletalStateHash, -1, SkeletalAnimationTime01);
				// ※※※  If you know why unity log that warning  ※※※
				// ※※※  Please email me (moenenn@163.com)       ※※※
				SkeletalAni.Update(0);
				PrevSkeletalAnimationTime01 = SkeletalAnimationTime01;
				RefreshSkeletalHandleRotation();
			}
		}



		private void ResetSkeletalTPose () {
			if (!SkeletalAni) { return; }
			int len = System.Enum.GetNames(typeof(HumanBodyBones)).Length;
			SkeletalTPoseBonesRotation = new Quaternion[len];
			for (int i = 0; i < len; i++) {
				if ((HumanBodyBones)i != HumanBodyBones.LastBone) {
					var bone = SkeletalAni.GetBoneTransform((HumanBodyBones)i);
					SkeletalTPoseBonesRotation[i] = bone ? bone.rotation : Quaternion.identity;
				}
			}
		}



		private void GotoSkeletalTPose () {
			if (!SkeletalAni) { return; }
			int len = System.Enum.GetNames(typeof(HumanBodyBones)).Length;
			for (int i = 0; i < len; i++) {
				if ((HumanBodyBones)i != HumanBodyBones.LastBone) {
					var bone = SkeletalAni.GetBoneTransform((HumanBodyBones)i);
					if (bone) {
						bone.rotation = SkeletalTPoseBonesRotation[i];
					}
				}
			}
		}



		// Handle
		private void RespawnAllSkeletalHandles () {

			if (SkeletalHandles != null) {
				foreach (var tf in SkeletalHandles) {
					if (tf && tf.gameObject) {
						DestroyImmediate(tf.gameObject, false);
					}
				}
				SkeletalHandles = null;
			}
			Util.ClearChildrenImmediate(SkeletalHandleRoot);


			if (!SkeletalAni) { return; }



			// Bones
			var bones = new Transform[SKELETAL_HANDLE_TYPE_COUNT];
			for (int i = 0; i < SKELETAL_HANDLE_TYPE_COUNT; i++) {
				bones[i] = SkeletalAni.GetBoneTransform(BindingTypeParent[(int)SkeletalToolkit][i]);
			}


			// Rebind
			GotoSkeletalTPose();


			// Spawn Rot Handles
			Vector3 pos;
			BindingType type;
			bool showHip = ShowSkeletalHipHandles;
			bool showArm = ShowSkeletalArmHandles;
			bool showLeg = ShowSkeletalLegHandles;
			switch (SkeletalToolkit) {

				case SkeletalToolkitType.Ring: {

					SkeletalHandles = new Transform[SKELETAL_HANDLE_TYPE_COUNT];

					var nodAngle = Quaternion.Euler(-45, 0, -90);
					var nodAngle180 = Quaternion.Euler(0, 0, -90);
					var tiltAngle = Quaternion.Euler(-90, 0, 0);
					var turnAngle = Quaternion.Euler(0, 0, 0);


					// Head Neck
					pos = new Vector3(0, 1.2f, 0);
					float size = 0.6f;
					type = BindingType.HeadNeck_Nod;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, nodAngle, size, 0, showHip);
					type = BindingType.HeadNeck_Tilt;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, tiltAngle, size, 1, showHip);
					type = BindingType.HeadNeck_Turn;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, turnAngle, size, 2, showHip);


					// Chest
					pos = new Vector3(0, 0.2f, 1f);
					size = 0.25f;
					type = BindingType.ChestSpine_Nod;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, nodAngle, size, 0, showHip);
					type = BindingType.ChestSpine_Tilt;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, tiltAngle, size, 1, showHip);
					type = BindingType.ChestSpine_Turn;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, turnAngle, size, 1, showHip);


					// Hip
					pos = new Vector3(0, 0, 0.8f);
					size = 0.15f;
					type = BindingType.Root_Nod;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, nodAngle, size, 0, showHip);
					type = BindingType.Root_Tilt;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, tiltAngle, size, 1, showHip);
					type = BindingType.Root_Turn;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, turnAngle, size, 1, showHip);


					// Arm L
					var rotFix = Quaternion.Euler(0, -90, 0);
					pos = new Vector3(-0.1f, 0, 0);
					size = 0.2f;
					type = BindingType.L_Arm_UD;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle, size, 0, showArm);
					type = BindingType.L_Arm_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * tiltAngle, size, 1, showArm);
					type = BindingType.L_Arm_FB;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * turnAngle, size, 1, showArm);


					// Arm R
					rotFix = Quaternion.Euler(0, 90, 0);
					pos = new Vector3(0.1f, 0, 0);
					size = 0.2f;
					type = BindingType.R_Arm_UD;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle, size, 0, showArm);
					type = BindingType.R_Arm_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * tiltAngle, size, 1, showArm);
					type = BindingType.R_Arm_FB;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * turnAngle, size, 1, showArm);


					// Fore Arm L
					rotFix = Quaternion.Euler(0, -90, 0);
					pos = new Vector3(-0.1f, 0, 0);
					size = 0.2f;
					type = BindingType.L_Forearm_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle, size, 0, showArm);
					type = BindingType.L_Forearm_Tilt;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * tiltAngle, size, 1, showArm);
					type = BindingType.L_Forearm_Stretch;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * turnAngle, size, 1, showArm);


					// Fore Arm R
					rotFix = Quaternion.Euler(0, 90, 0);
					pos = new Vector3(0.1f, 0, 0);
					size = 0.2f;
					type = BindingType.R_Forearm_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle, size, 0, showArm);
					type = BindingType.R_Forearm_Tilt;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * tiltAngle, size, 1, showArm);
					type = BindingType.R_Forearm_Stretch;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * turnAngle, size, 1, showArm);


					// Leg L
					rotFix = Quaternion.Euler(90, 0, 0);
					var rotFixTilt = Quaternion.Euler(90, -90, 0);
					var rotFixTwist = Quaternion.Euler(45, -90, 90);
					pos = new Vector3(0, -0.1f, 0);
					size = 0.2f;
					type = BindingType.L_Leg_FB;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle180, size, 1, showLeg);
					type = BindingType.L_Leg_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTilt * tiltAngle, size, 1, showLeg);
					type = BindingType.L_Leg_UD;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTwist * turnAngle, size, 0, showLeg);


					// Leg R
					rotFix = Quaternion.Euler(90, 0, 0);
					rotFixTilt = Quaternion.Euler(90, 90, 0);
					rotFixTwist = Quaternion.Euler(45, 90, 90);
					pos = new Vector3(0, -0.1f, 0);
					size = 0.2f;
					type = BindingType.R_Leg_FB;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle180, size, 1, showLeg);
					type = BindingType.R_Leg_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTilt * tiltAngle, size, 1, showLeg);
					type = BindingType.R_Leg_UD;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTwist * turnAngle, size, 0, showLeg);


					// F Leg L
					rotFix = Quaternion.Euler(90, 0, 0);
					rotFixTilt = Quaternion.Euler(90, -90, 0);
					rotFixTwist = Quaternion.Euler(45, -90, 90);
					pos = new Vector3(0, 0.1f, 0);
					size = 0.2f;
					type = BindingType.L_ForeLeg_Stretch;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle180, size, 1, showLeg);
					type = BindingType.L_ForeLeg_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTilt * tiltAngle, size, 1, showLeg);


					// F Leg R
					rotFix = Quaternion.Euler(90, 0, 0);
					rotFixTilt = Quaternion.Euler(90, 90, 0);
					rotFixTwist = Quaternion.Euler(45, 90, 90);
					pos = new Vector3(0, 0.1f, 0);
					size = 0.2f;
					type = BindingType.R_ForeLeg_Stretch;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFix * nodAngle180, size, 1, showLeg);
					type = BindingType.R_ForeLeg_Twist;
					SpawnSkeletalRingHandle(type, bones[(int)type], pos, rotFixTilt * tiltAngle, size, 1, showLeg);

				}
				break;

				case SkeletalToolkitType.Axis: {
					SkeletalHandles = new Transform[SKELETAL_HANDLE_TYPE_COUNT];


					// Head Neck
					pos = new Vector3(0, 1.3f, 1.3f);
					type = BindingType.HeadNeck_Nod;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 1, pos, 0.3f, showHip);
					type = BindingType.HeadNeck_Turn;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 0, pos, 0.3f, showHip);

					// Chest Spine
					pos = new Vector3(0, 0.3f, 1.8f);
					type = BindingType.ChestSpine_Nod;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 1, pos, 0.3f, showHip);
					type = BindingType.ChestSpine_Turn;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 0, pos, 0.3f, showHip);

					// Hip
					type = BindingType.Root_Nod;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 1, pos, -0.15f, showHip);
					type = BindingType.Root_Turn;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 0, pos, -0.15f, showHip);

					// Arm
					pos = new Vector3(0, 0, 0);
					type = BindingType.L_Arm_UD;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 1, pos, 0.3f, showArm);
					type = BindingType.L_Arm_FB;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, 0.3f, showArm);

					pos = new Vector3(0, 0, 0);
					type = BindingType.R_Arm_UD;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 1, pos, 0.3f, showArm);
					type = BindingType.R_Arm_FB;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, 0.3f, showArm);

					// F Arm
					pos = new Vector3(0, 0, 0);
					type = BindingType.L_Forearm_Stretch;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, -0.3f, showArm);

					pos = new Vector3(0, 0, 0);
					type = BindingType.R_Forearm_Stretch;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, -0.3f, showArm);

					// Leg
					pos = new Vector3(0, 0, 0);
					type = BindingType.L_Leg_FB;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, 0.3f, showLeg);
					type = BindingType.L_Leg_UD;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 0, pos, -0.3f, showLeg);

					pos = new Vector3(0, 0, 0);
					type = BindingType.R_Leg_FB;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, 0.3f, showLeg);
					type = BindingType.R_Leg_UD;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 0, pos, 0.3f, showLeg);

					// F Leg
					pos = new Vector3(0, 0f, 0);
					type = BindingType.L_ForeLeg_Stretch;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, -0.3f, showLeg);

					pos = new Vector3(0, 0f, 0);
					type = BindingType.R_ForeLeg_Stretch;
					SpawnSkeletalAxisHandle(type, bones[(int)type], 2, pos, -0.3f, showLeg);


					ResetSkeletalTPose();
				}
				break;

			}

			RefreshSkeletalHandleRotation();
		}



		private void SpawnSkeletalRingHandle (BindingType type, Transform parent, Vector3 pos, Quaternion rot, float size, int meshIndex, bool active) {

			var handle = new GameObject(SKELETAL_HANDLE_NAME, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)) {
				layer = LAYER_ID,
			}.transform;
			handle.SetParent(parent);
			handle.position = parent.position + pos * SkeletalHandleDistanceMuti;
			handle.rotation = rot;
			handle.localScale = Vector3.one * size * SkeletalHandleDistanceMuti;

			handle.GetComponent<MeshFilter>().mesh = GetSkeletalHandleMesh(meshIndex);

			var mr = handle.GetComponent<MeshRenderer>();
			var mat = new Material(QUAD_SHADER);
			mat.SetColor(COLOR_SHADER_ID, SKELETAL_RING_HANDLE_COLOR[0]);
			mr.material = mat;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = false;
			mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			handle.GetComponent<MeshCollider>().sharedMesh = GetSkeletalHandleMesh(meshIndex + 3);

			SkeletalHandles[(int)type] = handle;

			handle.gameObject.SetActive(active);

		}



		private void SpawnSkeletalAxisHandle (BindingType type, Transform parent, int axis, Vector3 pos, float size, bool active) {
			var handle = SpawnAxis(axis, SKELETAL_HANDLE_NAME, parent);
			handle.position = parent.position + pos * SkeletalHandleDistanceMuti;
			handle.localScale = Vector3.one * size * SkeletalHandleDistanceMuti;
			SkeletalHandles[(int)type] = handle;
			handle.gameObject.SetActive(active);
		}



		private void SetSkeletalHandleColor (Transform handle, Color color) {
			var mr = handle.GetComponent<MeshRenderer>();
			if (mr) {
				mr.sharedMaterial.SetColor(COLOR_SHADER_ID, color);
			} else {
				var tf = handle.GetChild(0);
				if (tf) {
					mr = tf.GetComponent<MeshRenderer>();
					if (mr) {
						mr.sharedMaterial.SetColor(COLOR_SHADER_ID, color);
					}
				}
				tf = handle.GetChild(1);
				if (tf) {
					mr = tf.GetComponent<MeshRenderer>();
					if (mr) {
						mr.sharedMaterial.SetColor(COLOR_SHADER_ID, color);
					}
				}
			}
		}



		private void SkeletalHandleDrag (BindingType type, float delta, int subIndex = 0) {

			if (!SkeletalAnimation) { return; }
			var bindings = BindingTypeBinding[(int)type];
			if (bindings.Length == 0) { return; }

			if (type == BindingType.Root_Nod || type == BindingType.Root_Tilt || type == BindingType.Root_Turn) {
				// Root
				SetSkeletalCurve(bindings[subIndex], delta, true);
			} else {
				// Other
				if (type == BindingType.ChestSpine_Nod || type == BindingType.ChestSpine_Tilt || type == BindingType.ChestSpine_Turn) {
					for (int i = 0; i < bindings.Length; i++) {
						delta = SetSkeletalCurve(bindings[i], delta, i == 0);
						delta *= 0.6f;
					}
				} else {
					for (int i = 0; i < bindings.Length; i++) {
						SetSkeletalCurve(bindings[i], i == 0 ? delta : 0, i == 0);
					}
				}
			}
		}



		private float SetSkeletalCurve (EditorCurveBinding binding, float value, bool isDelta, bool forceLiner = false) {
			float timeL = SkeletalAnimationTime01 * SkeletalAnimation.length - 0.5f / FrameRate;
			float timeR = SkeletalAnimationTime01 * SkeletalAnimation.length + 0.5f / FrameRate;
			var curve = AnimationUtility.GetEditorCurve(SkeletalAnimation, binding);
			if (curve == null) {
				curve = AnimationCurve.Linear(0f, 0f, SkeletalAnimation.length, 0f);
			}
			int index = -1;
			for (int i = 0; i < curve.keys.Length; i++) {
				var key = curve.keys[i];
				if (key.time > timeR) {
					break;
				} else if (key.time < timeL) {
					continue;
				}
				index = i;
			}

			float newValue = value;
			if (index < 0) {
				float time = SkeletalAnimationTime01 * SkeletalAnimation.length;
				newValue = isDelta ? curve.Evaluate(time) + value : value;
				curve.AddKey(time, newValue);
			} else {
				var key = curve.keys[index];
				newValue = isDelta ? key.value + value : value;
				key.value = newValue;
				curve.MoveKey(index, key);
			}

			// End
			if (forceLiner) {
				Util.CurveAllLiner(curve);
			}
			AnimationUtility.SetEditorCurve(SkeletalAnimation, binding, curve);
			return newValue;
		}



		private static Mesh GetSkeletalHandleMesh (int index) {
			if (!_SkeletalHandleMesh[index]) {
				bool isCollider = index > 2;
				float angle = index % 3 == 0 ? 90 : index % 3 == 1 ? 180 : 359.9f;
				_SkeletalHandleMesh[index] = Util.CreateSectorMesh(
					isCollider ? 0.9f : 0.98f,
					isCollider ? 1.1f : 1.02f,
					isCollider ? 0.1f : 0.04f,
					angle,
					isCollider ? 12 : 24
				);
			}
			return _SkeletalHandleMesh[index];
		}



		private void RefreshSkeletalHandleRotation () {
			if (SkeletalToolkit == SkeletalToolkitType.Axis) {
				for (int i = 0; SkeletalHandles != null && i < SkeletalHandles.Length; i++) {
					if (!SkeletalHandles[i]) { continue; }
					char c = SkeletalHandles[i].GetChild(0).name[0];
					SkeletalHandles[i].rotation = c == '0' ? Quaternion.Euler(0, 0, -90) :
						c == '1' ? Quaternion.Euler(0, 0, 0) :
						Quaternion.Euler(90, 0, 0);
				}
			}
		}



		private void RefreshSkeletalHandleActive () {
			if (SkeletalHandles != null && SkeletalHandles.Length >= SKELETAL_HANDLE_TYPE_COUNT) {
				for (int i = 0; i < SkeletalHandles.Length; i++) {
					var h = SkeletalHandles[i];
					if (!h) { continue; }
					switch ((BindingType)i) {
						case BindingType.HeadNeck_Nod:
						case BindingType.HeadNeck_Tilt:
						case BindingType.HeadNeck_Turn:
						case BindingType.ChestSpine_Nod:
						case BindingType.ChestSpine_Tilt:
						case BindingType.ChestSpine_Turn:
						case BindingType.Root_Nod:
						case BindingType.Root_Tilt:
						case BindingType.Root_Turn:
						case BindingType.Root_X:
						case BindingType.Root_Y:
						case BindingType.Root_Z:
						h.gameObject.SetActive(ShowSkeletalHipHandles);
						break;
						case BindingType.L_Arm_UD:
						case BindingType.L_Arm_FB:
						case BindingType.L_Arm_Twist:
						case BindingType.L_Forearm_Stretch:
						case BindingType.L_Forearm_Tilt:
						case BindingType.L_Forearm_Twist:
						case BindingType.R_Arm_UD:
						case BindingType.R_Arm_FB:
						case BindingType.R_Arm_Twist:
						case BindingType.R_Forearm_Stretch:
						case BindingType.R_Forearm_Tilt:
						case BindingType.R_Forearm_Twist:
						h.gameObject.SetActive(ShowSkeletalArmHandles);
						break;
						case BindingType.L_Leg_UD:
						case BindingType.L_Leg_FB:
						case BindingType.L_Leg_Twist:
						case BindingType.L_ForeLeg_Stretch:
						case BindingType.L_ForeLeg_Tilt:
						case BindingType.L_ForeLeg_Twist:
						case BindingType.R_Leg_UD:
						case BindingType.R_Leg_FB:
						case BindingType.R_Leg_Twist:
						case BindingType.R_ForeLeg_Stretch:
						case BindingType.R_ForeLeg_Tilt:
						case BindingType.R_ForeLeg_Twist:
						h.gameObject.SetActive(ShowSkeletalLegHandles);
						break;
					}
				}
			}
		}



		private Color GetSkeletalHandleColor (Transform handle, int type) {
			switch (SkeletalToolkit) {
				default:
				case SkeletalToolkitType.Ring:
				return SKELETAL_RING_HANDLE_COLOR[type];
				case SkeletalToolkitType.Axis:
				char c = handle.GetChild(0).name[0];
				return c == '0' ? SKELETAL_AXIS_HANDLE_COLOR[type * 3 + 0] :
				c == '1' ? SKELETAL_AXIS_HANDLE_COLOR[type * 3 + 1] :
				SKELETAL_AXIS_HANDLE_COLOR[type * 3 + 2];
			}
		}



		#endregion



	}
}