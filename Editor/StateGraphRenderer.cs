using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

public class StateGraphRenderer
{
    /// <summary>
    /// If it is not null, connections will only be rendered for this state
    /// </summary>
    public State targetState = null;

    /// <summary>
    /// If true, labels will be rendered
    /// </summary>
    public bool showLabels;

    /// <summary>
    /// If true, connections will be rendered
    /// </summary>
    public bool showConnections;

    /// <summary>
    /// If true, items will be rendered
    /// </summary>
    public bool showItems;

    private GUIStyle _labelsStyle;
    private GUIStyle _labelsEditModeStyle;

    public void RenderStateGraph(SceneView sceneview)
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null || EditorApplication.isPlaying)
            return;


        if (_labelsStyle == null)
        {
            _labelsStyle = new GUIStyle();
            _labelsStyle.normal.textColor = Color.green;
        }

        if (_labelsEditModeStyle == null)
        {
            _labelsEditModeStyle = new GUIStyle();
            _labelsEditModeStyle.normal.textColor = Color.blue;
        }

        var states = UnityEngine.Object.FindObjectsOfType<State>();

        foreach (var state in states)
        {
            if (targetState != null && state != targetState)
                continue;

            RenderConnections(state);
            RenderItems(state);
        }
    }

    private void RenderItems(State state)
    {
        var items = state.GetComponents<StateItem>();
        foreach (var item in items)
        {
            var itemPosition = item.transform.position + item.orientation * Vector3.forward;
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(
                0,
                itemPosition,
                item.transform.rotation,
                0.05f,
                EventType.Repaint
            );
        }

    }

    private void RenderConnections(State state)
    {
        var connections = state.GetComponents<Connection>();

        foreach (var connection in connections)
        {
            if (connection.destination == null)
                continue;

            var firstConnectionPosition = connection.transform.position + connection.orientation * Vector3.forward;
            var secondConnectionPosition = connection.destination.Origin.transform.position +
                connection.destination.orientation * Vector3.forward;

            if (showConnections)
            {
                Handles.color = Color.yellow;
                Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);

                Handles.color = Color.red;
                Handles.DotHandleCap(
                    0,
                    firstConnectionPosition,
                    Quaternion.identity,
                    0.05f,
                    EventType.Repaint
                );
            }

            // Draw label on connections when target is selected
            if (targetState != null && showLabels)
            {
                Handles.Label(firstConnectionPosition, connection.destination.Origin.title, _labelsEditModeStyle);
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

#endif
