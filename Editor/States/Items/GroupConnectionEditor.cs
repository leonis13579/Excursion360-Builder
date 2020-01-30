using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

class GroupConnectionEditor : EditorBase
{
    public void Draw(State state)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            Undo.AddComponent<GroupConnection>(state.gameObject);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        var groupConnections = state.GetComponents<GroupConnection>();
        foreach (var groupConnection in groupConnections)
        {
            GUILayout.Label(GetTitleStringOf(groupConnection.title), EditorStyles.boldLabel);
            Undo.RecordObject(groupConnection, "Edit group connection title");
            groupConnection.title = EditorGUILayout.TextField("Title:", groupConnection.title);

            var buttonStyle = Styles.ToggleButtonStyleNormal;
            if (StateItemPlaceEditor.EditableItem == groupConnection)
                buttonStyle = Styles.ToggleButtonStyleToggled;

            if (GUILayout.Button("edit", buttonStyle))
            {
                if (StateItemPlaceEditor.EditableItem == groupConnection)
                {
                    StateItemPlaceEditor.CleadEditing();
                }
                else
                {
                    StateItemPlaceEditor.EnableEditing(state, groupConnection, Color.green);
                }
            }

            EditorGUILayout.Space();


            var connections = state.GetComponents<Connection>();
            
            if (connections.Length > 0)
            {
                if (GUILayout.Button($"Add connection"))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (var connection in connections)
                    {
                        menu.AddItem(new GUIContent(connection.Destination.title), false, o =>
                        {
                            var selectedConnection = o as Connection;
                            Undo.RecordObject(groupConnection, "Add state reference");
                            groupConnection.states.Add(selectedConnection.Destination);

                            Undo.DestroyObjectImmediate(selectedConnection);

                        }, connection);
                    }
                    menu.ShowAsContext();
                }
            }
            else
            {
                GUILayout.Label("No available connections to add");
            }
            EditorGUILayout.Space();
            foreach (var stateReference in groupConnection.states)
            {
                GUILayout.Label(stateReference.title);
            }
        }
    }
}
#endif
