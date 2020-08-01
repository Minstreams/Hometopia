using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GameSystem;
using System.IO;

public class GameEditorExtension : EditorWindow
{
    //这里是一些编辑器方法
    [MenuItem("MatrixTool/Open Tool Window 打开工具箱 #F1")]
    public static void OpenToolWindow()
    {
        var comfirmWindow = EditorWindow.GetWindow<GameEditorExtension>("Minstreams工具箱");
    }
    /// <summary>
    /// 导航到系统配置文件
    /// </summary>
    [MenuItem("MatrixTool/System Config 系统配置 _F2")]
    public static void NavToSystemConfig()
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/System/TheMatrixSetting.asset");
    }
    /// <summary>
    /// 测试当前场景
    /// </summary>
    [MenuItem("MatrixTool/Debug Current Scene 测试当前场景 #F5")]
    public static void DebugCurrent()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        var sysScene = EditorSceneManager.OpenScene("Assets/Scenes/System.unity", OpenSceneMode.Additive);
        sysScene.GetRootGameObjects()[0].GetComponent<TheMatrix>().testAll = false;
        EditorApplication.isPlaying = true;
    }
    /// <summary>
    /// 测试全部场景
    /// </summary>
    [MenuItem("MatrixTool/Debug All Scenea 测试全部场景 _F5")]
    public static void DebugAll()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }
        EditorSceneManager.SaveOpenScenes();
        var sysScene = EditorSceneManager.OpenScene("Assets/Scenes/System.unity", OpenSceneMode.Single);
        sysScene.GetRootGameObjects()[0].GetComponent<TheMatrix>().testAll = true;
        foreach (string sceneName in TheMatrix.Setting.gameSceneMap.list)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.AdditiveWithoutLoading);
        }
        EditorApplication.isPlaying = true;
    }
    /// <summary>
    /// 添加子系统
    /// </summary>
    public static void AddSubSystem(string name, string comment)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!name.EndsWith("System")) name += "System";
        if (AssetDatabase.IsValidFolder("Assets/Scripts/SubSystem/" + name))
        {
            Debug.LogAssertion(name + " already Exists!");
            return;
        }
        AssetDatabase.CreateFolder("Assets/Scripts/SubSystem", name);
        //Setting-------------------------------
        var fSetting = File.CreateText("Assets/Scripts/SubSystem/" + name + "/" + name + "Setting.cs");
        fSetting.Write(
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = " + "\"" + name + "Setting\", menuName = \"系统配置文件/" + name + "Setting\"" + @")]
        public class " + name + @"Setting : ScriptableObject
        {
            //data definition here
        }
    }
}");
        fSetting.Close();

        var fSystem = File.CreateText("Assets/Scripts/SubSystem/" + name + "/" + name + ".cs");
        fSystem.Write(
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.Setting;

namespace GameSystem
{
    /// <summary>
    /// " + (string.IsNullOrEmpty(comment) ? name : comment) + @"
    /// </summary>
    public class " + name + @" : SubSystem<" + name + @"Setting>
    {
        //Your code here


        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInit()
        {
            //用于控制Action初始化
            TheMatrix.onGameAwake += OnGameAwake;
            TheMatrix.onGameStart += OnGameStart;
        }
        private static void OnGameAwake()
        {
            //在进入游戏第一个场景时调用
        }
        private static void OnGameStart()
        {
            //在主场景游戏开始时和游戏重新开始时调用
        }


        //API---------------------------------
        //public static void SomeFunction(){}
    }
}
");
        fSystem.Close();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Scripts/SubSystems/" + name);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Minstreams工具箱", name + " created!", "Cool~");
    }
    public void CreateSettingAsset()
    {
        if (string.IsNullOrWhiteSpace(subSystemName)) return;
        if (!subSystemName.EndsWith("System")) subSystemName += "System";
        if (!AssetDatabase.IsValidFolder("Assets/Scripts/SubSystem/" + subSystemName))
        {
            Debug.LogAssertion(subSystemName + " doesn't exist!");
            return;
        }
        if (File.Exists("Assets/Resources/System/" + subSystemName + "Setting.asset"))
        {
            Debug.LogAssertion(subSystemName + " Setting Asset already exist!");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/System/" + subSystemName + "Setting.asset");
            return;
        }
        Selection.selectionChanged += _CreateSettingAsset;
        NavToSystemConfig();
    }
    private void _CreateSettingAsset()
    {
        Selection.selectionChanged -= _CreateSettingAsset;
        EditorApplication.ExecuteMenuItem("Assets/Create/系统配置文件/" + subSystemName + "Setting");
        EditorUtility.DisplayDialog("Minstreams工具箱", subSystemName + "  Setting Asset created!", "Cool~");
    }
    private void ScanSubSystem()
    {
        subSystemSettings = AssetDatabase.FindAssets("SystemSetting t:ScriptableObject");
        for (int i = 0; i < subSystemSettings.Length; ++i)
        {
            subSystemSettings[i] = AssetDatabase.GUIDToAssetPath(subSystemSettings[i]);
        }
    }
    /// <summary>
    /// 添加Linker
    /// </summary>
    public static void AddLinker(string name, string comment, string subSystemName = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!string.IsNullOrWhiteSpace(subSystemName))
        {
            if (!subSystemName.EndsWith("System")) subSystemName += "System";
            if (!AssetDatabase.IsValidFolder("Assets/Scripts/SubSystem/" + subSystemName + "/Linker")) AssetDatabase.CreateFolder("Assets/Scripts/SubSystem/" + subSystemName, "Linker");
        }
        var f = File.CreateText("Assets/Scripts/" + (string.IsNullOrWhiteSpace(subSystemName) ? "" : ("SubSystem/" + subSystemName + "/")) + "Linker/" + name + ".cs");
        f.Write(
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        " + (string.IsNullOrEmpty(comment) ? "" : (
        @"/// <summary>
        /// " + comment + @"
        /// </summary>")) + @"
        [AddComponentMenu(" + "\"Linker/" + (string.IsNullOrWhiteSpace(subSystemName) ? "" : (subSystemName + "/")) + name + "\"" + @")]
        public class " + name + @" : MonoBehaviour
        {
            //Inner code here

            //Output
            public SimpleEvent output;

            //Input
            [ContextMenu(" + "\"Invoke\"" + @")]
            public void Invoke()
            {
                output?.Invoke();
            }
        }
    }
}");
        f.Close();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Minstreams工具箱", (string.IsNullOrWhiteSpace(subSystemName) ? "" : (subSystemName + "/")) + name + " created!", "Cool~");
    }
    /// <summary>
    /// 添加Operator
    /// </summary>
    public static void AddOperator(string name, string comment, string subSystemName = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!string.IsNullOrWhiteSpace(subSystemName))
        {
            if (!subSystemName.EndsWith("System")) subSystemName += "System";
            if (!AssetDatabase.IsValidFolder("Assets/Scripts/SubSystem/" + subSystemName + "/Operator")) AssetDatabase.CreateFolder("Assets/Scripts/SubSystem/" + subSystemName, "Operator");
        }
        var f = File.CreateText("Assets/Scripts/" + (string.IsNullOrWhiteSpace(subSystemName) ? "" : ("SubSystem/" + subSystemName + "/")) + "Operator/" + name + ".cs");
        f.Write(
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        " + (string.IsNullOrEmpty(comment) ? "" : (
        @"/// <summary>
        /// " + comment + @"
        /// </summary>")) + @"
        [AddComponentMenu(" + "\"Operator/" + (string.IsNullOrWhiteSpace(subSystemName) ? "" : (subSystemName + "/")) + name + "\"" + @")]
        public class " + name + @" : MonoBehaviour
        {
            //Inner code here

            //Input
            //public void SomeFuntion(){}
        }
    }
}");
        f.Close();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Minstreams工具箱", (string.IsNullOrWhiteSpace(subSystemName) ? "" : (subSystemName + "/")) + name + " created!", "Cool~");
    }
    /// <summary>
    /// 添加Savable
    /// </summary>
    public static void AddSavable(string name, string comment, string subSystemName = "")
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!string.IsNullOrWhiteSpace(subSystemName))
        {
            if (!subSystemName.EndsWith("System")) subSystemName += "System";
            if (!AssetDatabase.IsValidFolder("Assets/Scripts/SubSystem/" + subSystemName + "/Savable")) AssetDatabase.CreateFolder("Assets/Scripts/SubSystem/" + subSystemName, "Savable");
        }
        var f = File.CreateText("Assets/Scripts/" + (string.IsNullOrWhiteSpace(subSystemName) ? "" : ("SubSystem/" + subSystemName + "/")) + "Savable/" + name + ".cs");
        f.Write(
@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Savable
    {
        " + (string.IsNullOrEmpty(comment) ? "" : (
        @"/// <summary>
        /// " + comment + @"
        /// </summary>")) + @"
        [CreateAssetMenu(fileName = " + "\"" + name + "\", menuName = \"Savable/" + name + "\"" + @")]
        public class " + name + @" : SavableObject
        {
            //Your Data here
            //public float data1;

            //APPLY the data to game
            public override void ApplyData()
            {
                //apply(data1);
            }

            //Collect and UPDATE data
            public override void UpdateData()
            {
                //data1 = ...
            }
        }
    }
}");
        f.Close();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Minstreams工具箱", (string.IsNullOrWhiteSpace(subSystemName) ? "" : (subSystemName + "/")) + name + " created!", "Cool~");

    }

    private string[] subSystemSettings = null;
    public enum EditorMode
    {
        SubSystem,
        Linker,
        Operator,
        Savable
    }
    private EditorMode editorMode;
    private string subSystemName = "";
    private string subSystemComment = "";
    private string linkerName = "";
    private string linkerComment = "";
    private string operatorName = "";
    private string operatorComment = "";
    private string savableName = "";
    private string savableComment = "";
    private GUIStyle headerStyle;
    private GUIStyle HeaderStyle
    {
        get
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle("ProfilerBadge");
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.fontSize = 18;
                headerStyle.fixedHeight = 32;
                headerStyle.margin = new RectOffset(2, 2, 2, 2);
            }
            return headerStyle;
        }
    }
    private GUIStyle btnStyle;
    private GUIStyle BtnStyle
    {
        get
        {
            if (btnStyle == null)
            {
                btnStyle = new GUIStyle("toolbarbutton");
                headerStyle.alignment = TextAnchor.MiddleCenter;
                btnStyle.fixedHeight = 20;
                btnStyle.margin = new RectOffset(4, 4, 4, 4);
            }
            return btnStyle;
        }
    }
    private GUIStyle labelStyle;
    private GUIStyle LabelStyle
    {
        get
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle("ColorPickerBackground");
                labelStyle.padding = new RectOffset(8, 8, 4, 4);
                labelStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            }
            return labelStyle;
        }
    }
    private GUIStyle tabBGStyle;
    private GUIStyle TabBGStyle
    {
        get
        {
            if (tabBGStyle == null)
            {
                tabBGStyle = new GUIStyle("IN ThumbnailShadow");
                tabBGStyle.fixedHeight = 0;
            }
            return tabBGStyle;
        }
    }
    private GUIStyle tabStyle;
    private GUIStyle TabStyle
    {
        get
        {
            if (tabStyle == null)
            {
                tabStyle = new GUIStyle("PreLabel");
                tabStyle.padding = TabLabelStyle.padding;
                tabStyle.stretchHeight = false;
                tabStyle.alignment = TextAnchor.MiddleCenter;
                tabStyle.contentOffset = Vector2.zero;
                tabStyle.margin = new RectOffset(2, 2, 2, 2);
                tabStyle.fontStyle = FontStyle.Normal;
                tabStyle.normal.textColor = Color.black;
            }
            return tabStyle;
        }
    }
    private GUIStyle tabLabelStyle;
    private GUIStyle TabLabelStyle
    {
        get
        {
            if (tabLabelStyle == null)
            {
                tabLabelStyle = new GUIStyle("ShurikenEffectBg");
                tabLabelStyle.stretchHeight = false;
                tabLabelStyle.alignment = TextAnchor.MiddleCenter;
                tabLabelStyle.contentOffset = Vector2.zero;
                tabLabelStyle.margin = new RectOffset(2, 2, 2, 2);
                tabLabelStyle.fontStyle = FontStyle.Bold;
                tabLabelStyle.normal.textColor = Color.white;
            }
            return tabLabelStyle;
        }
    }
    /// <summary>
    /// 分隔符
    /// </summary>
    private void Separator()
    {
        GUILayout.Label("", "RL DragHandle", GUILayout.ExpandWidth(true));
    }
    private void SectionHeader(string title)
    {
        Separator();
        GUILayout.Label(title, HeaderStyle, GUILayout.ExpandWidth(true));
    }
    private string TextArea(string name, string target)
    {
        string result;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(name, "ProfilerSelectedLabel", GUILayout.ExpandWidth(false), GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.2f));
            result = GUILayout.TextField(target, 24, "SearchTextField");
        }
        GUILayout.EndHorizontal();
        return result;
    }
    private void Label(string text)
    {
        GUILayout.Label(text, LabelStyle);
    }



    private void OnEnable()
    {
        ScanSubSystem();
        Input.imeCompositionMode = IMECompositionMode.On;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical("GameViewBackground", GUILayout.ExpandHeight(true));
        SectionHeader("数据导航");
        if (GUILayout.Button("系统配置", BtnStyle)) NavToSystemConfig();
        if (subSystemSettings != null)
        {
            for (int i = 0; i < subSystemSettings.Length; ++i)
            {
                int start = Mathf.Max(subSystemSettings[i].LastIndexOf('/') + 1, 0);
                string subName = subSystemSettings[i].Substring(start, subSystemSettings[i].Length - start - 19);
                if (GUILayout.Button(subName, BtnStyle))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(subSystemSettings[i]);
                    subSystemName = subName;
                    subSystemComment = "";
                }
            }
        }
        if (GUILayout.Button("刷新", BtnStyle)) ScanSubSystem();

        SectionHeader("自动测试");
        if (GUILayout.Button("测试全部场景", BtnStyle)) DebugAll();
        if (GUILayout.Button("测试当前场景", BtnStyle)) DebugCurrent();

        SectionHeader("自动化代码生成");
        GUILayout.BeginHorizontal(TabBGStyle);
        {
            if (GUILayout.Button("SubSystem", editorMode == EditorMode.SubSystem ? TabLabelStyle : TabStyle)) editorMode = EditorMode.SubSystem;
            if (GUILayout.Button("Linker", editorMode == EditorMode.Linker ? TabLabelStyle : TabStyle)) editorMode = EditorMode.Linker;
            if (GUILayout.Button("Operator", editorMode == EditorMode.Operator ? TabLabelStyle : TabStyle)) editorMode = EditorMode.Operator;
            if (GUILayout.Button("Savable", editorMode == EditorMode.Savable ? TabLabelStyle : TabStyle)) editorMode = EditorMode.Savable;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);
        switch (editorMode)
        {
            case EditorMode.SubSystem:
                subSystemName = TextArea("Sub System Name", subSystemName);
                subSystemComment = TextArea("Comment", subSystemComment);
                Label("自动生成一个子系统 Sub System");
                Label("由于实在无法把生成代码与生成配置文件功能做到一起，生成新系统时，请依次点这两个按钮。");
                if (GUILayout.Button("Add", BtnStyle)) AddSubSystem(subSystemName, subSystemComment);
                if (GUILayout.Button("Create Setting Asset", BtnStyle)) CreateSettingAsset();
                break;
            case EditorMode.Linker:
                linkerName = TextArea("Linker Name", linkerName);
                linkerComment = TextArea("Comment", linkerComment);
                Label("自动生成一个连接节点 Linker，用于连接和处理数据。");
                if (GUILayout.Button("Add", BtnStyle)) AddLinker(linkerName, linkerComment);
                if (GUILayout.Button("Add To Current SubSystem", BtnStyle)) AddLinker(linkerName, linkerComment, subSystemName);
                break;
            case EditorMode.Operator:
                operatorName = TextArea("Operator Name", operatorName);
                operatorComment = TextArea("Comment", operatorComment);
                Label("自动生成一个操作节点 Operator，用于执行具体动作。");
                if (GUILayout.Button("Add", BtnStyle)) AddOperator(operatorName, operatorComment);
                if (GUILayout.Button("Add To Current SubSystem", BtnStyle)) AddOperator(operatorName, operatorComment, subSystemName);
                break;
            case EditorMode.Savable:
                savableName = TextArea("Savable Name", savableName);
                savableComment = TextArea("Comment", savableComment);
                Label("自动生成一个可存储对象SavableObject，用于持久化存储数据。");
                if (GUILayout.Button("Add", BtnStyle)) AddSavable(savableName, savableComment);
                if (GUILayout.Button("Add To Current SubSystem", BtnStyle)) AddSavable(savableName, savableComment, subSystemName);
                break;
        }
        Separator();
        GUILayout.EndVertical();
    }
}

#region EnumMap Drawer Definition
[CustomPropertyDrawer(typeof(GameSceneMap), true)]
public class GameSceneMapDrawer : EnumMapDrawer<TheMatrix.GameScene> { }
[CustomPropertyDrawer(typeof(SoundClipMap), true)]
public class AudioClipMapDrawer : EnumMapDrawer<AudioSystem.Sound> { }
[CustomPropertyDrawer(typeof(InputKeyMap), true)]
public class InputKeyMapDrawer : EnumMapDrawer<InputSystem.InputKey> { }
#endregion