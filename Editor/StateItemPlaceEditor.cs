using Excursion360_Builder.Shared.States.Items;
using Packages.Excursion360_Builder.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Graphs.Styles;

[InitializeOnLoad]
public class StateItemPlaceEditor
{
    public static UnityEngine.Color CubeColor { get; private set; }
    public static State EditableState { get; private set; }
    public static IStateItem EditableItem { get; private set; }
    static StateItemPlaceEditor()
    {
        SceneView.duringSceneGui += SceneGui;
        Selection.selectionChanged += CleadEditing;
    }

    public static void EnableEditing(State editableState, IStateItem editableItem, UnityEngine.Color cubeColor)
    {
        if (!editableState)
            throw new ArgumentNullException(nameof(editableState));
        if (editableItem == null)
            throw new ArgumentNullException(nameof(editableItem));
        EditableState = editableState;
        EditableItem = editableItem;
        CubeColor = cubeColor;
    }

    public static void CleadEditing()
    {
        EditableState = default;
        EditableItem = default;
        CubeColor = default;
    }

    private static void SceneGui(SceneView sceneView)
    {
        if (EditableItem == null || !EditableState)
            return;

        Handles.color = CubeColor;
        Handles.DrawWireCube(
            EditableState.transform.position + EditableItem.Orientation * Vector3.forward,
            Vector3.one * 0.2f
        );

        if (InteractionHelper.GetStateClickPoint(EditableState, out var rotation))
        {
            EditableItem.Orientation = rotation;
        }
    }
}

