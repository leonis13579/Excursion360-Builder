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
    public static StateItem EditableItem { get; private set; }
    static StateItemPlaceEditor()
    {
        SceneView.duringSceneGui += SceneGui;
    }

    public static void EnableEditing(State editableState, StateItem editableItem, UnityEngine.Color cubeColor)
    {
        if (!editableState)
            throw new ArgumentNullException(nameof(editableState));
        if (!editableItem)
            throw new ArgumentNullException(nameof(editableItem));
        StateItemPlaceEditor.EditableState = editableState;
        StateItemPlaceEditor.EditableItem = editableItem;
        StateItemPlaceEditor.CubeColor = cubeColor;
    }

    public static void CleadEditing()
    {
        EditableState = default;
        EditableItem = default;
        CubeColor = default;
    }

    private static void SceneGui(SceneView sceneView)
    {
        if (!EditableItem || !EditableState)
            return;

        Handles.color = CubeColor;
        Handles.DrawWireCube(
            EditableState.transform.position + EditableItem.orientation * Vector3.forward,
            Vector3.one * 0.2f
        );

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            Event.current.Use();

            SphereCollider collider = EditableState.GetComponent<SphereCollider>();
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            //if ((state.transform.position - ray.origin).magnitude <= collider.radius * collider.radius)
            ray.origin -= ray.direction * collider.radius * 4;

            if (collider.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                var toCenterDirection = EditableState.transform.position - hit.point;
                var rightDirection = Vector3.Cross(toCenterDirection, ray.direction);
                var normal = Vector3.Cross(rightDirection, ray.direction);
                var hitPosition = EditableState.transform.position + StateEditorWindow.ReflectDirection(toCenterDirection, normal);

                Undo.RecordObject(EditableItem, "Undo orientation change");

                EditableItem.orientation = Quaternion.FromToRotation(Vector3.forward,
                    hitPosition - EditableState.transform.position);
            }
        }
    }
}

