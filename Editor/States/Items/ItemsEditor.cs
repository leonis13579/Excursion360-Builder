using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

public class ItemsEditor
{
    private StateItem _currentStateItem;

    public void Draw(State state)
    {

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            Undo.AddComponent<StateItem>(state.gameObject);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        foreach (var item in state.GetComponents<StateItem>())
        {
            var titleName = string.IsNullOrEmpty(item.name) ? "NO TITLE" : item.name;
            if (titleName.Length > 30)
            {
                titleName = titleName.Substring(0, 30) + "...";
            }
            if (item.isFolded = EditorGUILayout.Foldout(item.isFolded, titleName, true))
            {
                Undo.RecordObject(item, "Edit state item name");
                item.name = EditorGUILayout.TextField("Name", item.name);

                var buttonStyle = Styles.ToggleButtonStyleNormal;
                if (_currentStateItem == item)
                    buttonStyle = Styles.ToggleButtonStyleToggled;

                if (GUILayout.Button("edit", buttonStyle))
                {
                    if (_currentStateItem == item)
                        _currentStateItem = null;
                    else
                        _currentStateItem = item;
                }

                if (GUILayout.Button("delete"))
                {
                    Undo.DestroyObjectImmediate(item);
                }
            }
            EditorGUILayout.Space();
        }
    }

    internal void OnSceneGUI(SceneView sceneView, State state)
    {
        if (_currentStateItem == null || !state)
            return;

        Handles.color = Color.green;
        Handles.DrawWireCube(
            state.transform.position + _currentStateItem.orientation * Vector3.forward,
            Vector3.one * 0.2f
        );

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            Event.current.Use();

            SphereCollider collider = state.GetComponent<SphereCollider>();
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            //if ((state.transform.position - ray.origin).magnitude <= collider.radius * collider.radius)
            ray.origin -= ray.direction * collider.radius * 4;

            if (collider.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                var toCenterDirection = state.transform.position - hit.point;
                var rightDirection = Vector3.Cross(toCenterDirection, ray.direction);
                var normal = Vector3.Cross(rightDirection, ray.direction);
                var hitPosition = state.transform.position + StateEditorWindow.ReflectDirection(toCenterDirection, normal);

                Undo.RecordObject(_currentStateItem, "Undo orientation change");

                _currentStateItem.orientation = Quaternion.FromToRotation(Vector3.forward,
                    hitPosition - state.transform.position);
            }
        }
    }
}

#endif
