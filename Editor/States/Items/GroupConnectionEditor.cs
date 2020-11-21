﻿using System;
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
        if (GUILayout.Button("Add"))
        {
            Undo.AddComponent<GroupConnection>(state.gameObject);
        }
        EditorGUILayout.Space();
        var groupConnections = state.GetComponents<GroupConnection>();
        foreach (var groupConnection in groupConnections)
        {
            var groupConnectionTitle = GetTitleStringOf(groupConnection.title);
            GUILayout.Label(groupConnectionTitle, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            Undo.RecordObject(groupConnection, "Edit group connection title");
            groupConnection.title = EditorGUILayout.TextField("Title:", groupConnection.title);

            var buttonStyle = Styles.ToggleButtonStyleNormal;
            if (StateItemPlaceEditor.EditableItem == (object)groupConnection)
                buttonStyle = Styles.ToggleButtonStyleToggled;

            if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), "edit", buttonStyle))
            {
                if (StateItemPlaceEditor.EditableItem == (object)groupConnection)
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
                if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), $"Add connection"))
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
            EditorGUILayout.LabelField("State references:");
            EditorGUI.indentLevel++;
            foreach (var stateReference in groupConnection.states)
            {
                EditorGUILayout.LabelField(stateReference.title);
                var line = EditorGUILayout.BeginHorizontal();

                GUILayout.Space(40);

                if (GUILayout.Button("Move to"))
                {
                    StateEditorWindow.FocusCamera(stateReference.gameObject);
                }

                if (GUILayout.Button("To simple connection"))
                {
                    Undo.RecordObject(groupConnection, "Delete state reference");
                    groupConnection.states.Remove(stateReference);
                    var addedConnection = Undo.AddComponent<Connection>(state.gameObject);
                    addedConnection.Destination = stateReference;
                    break;
                }

                if (GUILayout.Button("Delete", Styles.DeleteButtonStyle))
                {
                    Undo.RecordObject(groupConnection, "Delete state reference");
                    groupConnection.states.Remove(stateReference);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }
    }
}
#endif
