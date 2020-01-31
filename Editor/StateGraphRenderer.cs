using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
            RenderGroupConnections(state);
            RenderItems(state);
        }
    }

    private void RenderGroupConnections(State state)
    {
        var groupConnections = state.GetComponents<GroupConnection>();
        foreach (var item in groupConnections)
        {
            var itemPosition = item.transform.position + item.orientation * Vector3.forward;
            Handles.color = Color.blue;
            Handles.DotHandleCap(
                0,
                itemPosition,
                item.transform.rotation,
                0.05f,
                EventType.Repaint
            );
            foreach (var targetState in item.states)
            {
                var firstConnectionPosition = state.transform.position + item.orientation * Vector3.forward;
                var secondConnectionPosition = targetState.transform.position;

                var backConnection = targetState.GetComponents<Connection>().FirstOrDefault(c => c.Destination == state);

                if (backConnection == null)
                {
                    var backGroupConnection = targetState.GetComponents<GroupConnection>().FirstOrDefault(gc => gc.states.Any(s => s == state));
                    if (backGroupConnection != null)
                    {
                        secondConnectionPosition += backGroupConnection.orientation * Vector3.forward;
                    }
                }
                else
                {
                    secondConnectionPosition += backConnection.orientation * Vector3.forward;
                }

                if (showConnections)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);
                }
            }
        }
    }

    private void RenderItems(State state)
    {
        var items = state.GetComponents<ContentItem>();
        foreach (var item in items)
        {
            var itemPosition = item.transform.position + item.orientation * Vector3.forward;
            Handles.color = Color.cyan;
            Handles.DotHandleCap(
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

            var firstConnectionPosition = connection.transform.position + connection.orientation * Vector3.forward;
            var secondConnectionPosition = connection.Destination.transform.position;
            var backConnection = connection.Destination.GetComponents<Connection>().FirstOrDefault(c => c.Destination == state);

            if (backConnection == null)
            {
                var backGroupConnection = connection.Destination.GetComponents<GroupConnection>().FirstOrDefault(gc => gc.states.Any(s => s == state));
                if (backGroupConnection != null)
                {
                    secondConnectionPosition += backGroupConnection.orientation * Vector3.forward;
                }
            }
            else
            {
                secondConnectionPosition += backConnection.orientation * Vector3.forward;
            }

            if (showConnections)
            {
                Handles.color = Color.yellow;
                Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);

                Handles.color = Color.red;
                Handles.DotHandleCap(
                    0,
                    firstConnectionPosition,
                    Quaternion.identity,
                    0.07f,
                    EventType.Repaint
                );
            }

            // Draw label on connections when target is selected
            if (targetState != null && showLabels)
            {
                Handles.Label(firstConnectionPosition, connection.Origin.title, _labelsEditModeStyle);
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
