using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

public class ContentEditor
{
    public void Draw(State state)
    {

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            Undo.AddComponent<ContentItem>(state.gameObject);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        foreach (var item in state.GetComponents<ContentItem>())
        {
            var titleName = string.IsNullOrEmpty(item.name) ? "NO TITLE" : item.name;
            if (titleName.Length > 30)
            {
                titleName = titleName.Substring(0, 30) + "...";
            }
            if (item.isOpened = EditorGUILayout.Foldout(item.isOpened, titleName, true))
            {
                Undo.RecordObject(item, "Edit state item name");
                item.name = EditorGUILayout.TextField("Name", item.name);

                var buttonStyle = Styles.ToggleButtonStyleNormal;
                if (StateItemPlaceEditor.EditableItem == item)
                    buttonStyle = Styles.ToggleButtonStyleToggled;

                if (GUILayout.Button("edit", buttonStyle))
                {
                    if (StateItemPlaceEditor.EditableItem == item)
                    {
                        StateItemPlaceEditor.CleadEditing();
                    }
                    else
                    {
                        StateItemPlaceEditor.EnableEditing(state, item, Color.green);
                    }
                }

                if (GUILayout.Button("delete", Styles.DeleteButtonStyle))
                {
                    Undo.DestroyObjectImmediate(item);
                }
            }
            EditorGUILayout.Space();
        }
    }

    internal void OnSceneGUI(SceneView sceneView, State state)
    {
    }
}

#endif
