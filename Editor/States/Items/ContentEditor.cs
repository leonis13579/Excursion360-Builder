using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

class ContentEditor : EditorBase
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
            var titleName = GetTitleStringOf(item.name);
            if (item.isOpened = EditorGUILayout.Foldout(item.isOpened, titleName, true))
            {
                Undo.RecordObject(item, "Edit state item name");
                item.name = EditorGUILayout.TextField("Name", item.name);

                var buttonStyle = Styles.ToggleButtonStyleNormal;
                if (StateItemPlaceEditor.EditableItem as UnityEngine.Object == item)
                    buttonStyle = Styles.ToggleButtonStyleToggled;

                if (GUILayout.Button("edit", buttonStyle))
                {
                    if (StateItemPlaceEditor.EditableItem as UnityEngine.Object == item)
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
}

#endif
