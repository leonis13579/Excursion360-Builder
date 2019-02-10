using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
class StateGraphMenu
{
    private const string MenuName = "VR Tour/Show state graph";
    private const string SettingName = "StateGraphHidden";

    static StateGraphMenu()
    {
        if (StateGraphRenderer.Instance == null)
            return;

        UpdateDelegate();
    }

    public static bool IsEnabled
    {
        get { return EditorPrefs.GetBool(SettingName, true); }
        set { EditorPrefs.SetBool(SettingName, value); }
    }

    [MenuItem(MenuName)]
    private static void ToggleAction()
    {
        IsEnabled = !IsEnabled;
        UpdateDelegate();
    }

    [MenuItem(MenuName, true)]
    private static bool ToggleActionValidate()
    {
        Menu.SetChecked(MenuName, IsEnabled);
        return true;
    }

    public static void UpdateDelegate()
    {
        if (StateGraphRenderer.Instance == null)
            return;

        SceneView.onSceneGUIDelegate -= StateGraphRenderer.Instance.RenderStateGraph;
        if (IsEnabled)
            SceneView.onSceneGUIDelegate += StateGraphRenderer.Instance.RenderStateGraph;

        SceneView.RepaintAll();
    }
}
#endif