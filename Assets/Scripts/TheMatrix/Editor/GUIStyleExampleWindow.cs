using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class GUIStyleExampleWindow : EditorWindow
{
    private readonly string[] dList =
    {
        "box",
        "button",
        "label",
        "scrollView",
        "textArea",
        "textField",
        "toggle",
        "window",
        "horizontalScrollbar",
        "horizontalScrollbarThumb",
        "horizontalScrollbarLeftButton",
        "horizontalScrollbarRightButton",
        "verticalScrollbar",
        "verticalScrollbarThumb",
        "verticalScrollbarUpButton",
        "verticalScrollbarDownButton",
        "horizontalSlider",
        "horizontalSliderThumb",
        "verticalSlider",
        "verticalSliderThumb"
    };
    private GUIStyle[] sList = null;
    private int length = 0;
    private void OnEnable()
    {
        GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        sList = skin.customStyles;
        length = dList.Length + sList.Length;
    }

    private Vector2 mScrollPos;

    [MenuItem("MatrixTool/GUIStyle 样例窗口")]
    private static void Example()
    {
        var w = GetWindow<GUIStyleExampleWindow>();
        w.wantsMouseEnterLeaveWindow = false;
        w.wantsMouseMove = false;
        w.autoRepaintOnSceneChange = false;
    }

    private int page = 0;
    private int itemsPerPage = 30;
    private string text = "Test测试";
    private void OnGUI()
    {
        mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
        for (int i = page * itemsPerPage; i < length && i < (page + 1) * itemsPerPage; ++i)
        {
            string name = i < dList.Length ? dList[i] : sList[i - dList.Length].name;
            GUIStyle style = i < dList.Length ? dList[i] : sList[i - dList.Length];
            GUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(name, GUILayout.MaxWidth(250));
            GUILayout.Label(text, style);
            GUILayout.EndHorizontal();
            GUILayout.Box(
                string.Empty,
                GUILayout.Width(position.width - 24),
                GUILayout.Height(1)
            );
        }

        EditorGUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("prev"))
        {
            page--;
            if (page < 0) page = 0;
        }
        itemsPerPage = EditorGUILayout.IntField(itemsPerPage);
        GUILayout.Label(page.ToString());
        if (GUILayout.Button("next"))
        {
            page++;
            while (page * itemsPerPage > length) page--;
        }
        GUILayout.EndHorizontal();
        text = EditorGUILayout.TextField(text);
    }
}

