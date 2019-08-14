using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

public class StateGraphRenderer
{
    /**
     * @brief If it is not null, connections will only be rendered for this state
     */
    public State targetState = null;

    /**
     * @brief If true, labels will be rendered
     */
    public bool showLabels;

    /**
     * @brief Is true, connections will be rendered
     */
    public bool showConnections;

    private GUIStyle _labelsStyle;

    public void RenderStateGraph(SceneView sceneview)
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null || EditorApplication.isPlaying)
            return;


        if (_labelsStyle == null)
        {
            _labelsStyle = new GUIStyle();
            _labelsStyle.normal.textColor = Color.green;
        }

        var states = UnityEngine.Object.FindObjectsOfType<State>();

        foreach (var state in states)
        {
            if (targetState != null && state != targetState)
                continue;

            var connections = state.GetComponents<Connection>();

            foreach (var connection in connections)
            {
                if (connection.destination == null)
                    continue;

                var firstConnectionPosition = connection.transform.position + connection.orientation * Vector3.forward;
                var secondConnectionPosition = connection.destination.origin.transform.position +
                    connection.destination.orientation * Vector3.forward;

                if (showConnections)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);

                    Handles.color = Color.red;
                    Handles.DotHandleCap(
                        0,
                        firstConnectionPosition,
                        connection.transform.rotation * Quaternion.LookRotation(Vector3.right),
                        0.05f,
                        EventType.Repaint
                    );
                }

                // Draw label on connections when target is selected
                if (targetState != null && showLabels)
                {
                    Handles.color = Color.blue;
                    Handles.Label(firstConnectionPosition, connection.destination.origin.title, _labelsStyle);
                }
            }

            // Draw all labels when no target selected
            if (targetState == null && showLabels)
            {
                Handles.color = Color.blue;
                Handles.Label(state.transform.position, state.title, _labelsStyle);
            }
        }
    }
}

#endif
