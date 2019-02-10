using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
public class StateGraphRenderer : MonoBehaviour
{
    public GameObject statePrefab; // TODO: move it to state manager

    public static StateGraphRenderer Instance {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<StateGraphRenderer>();

            return _instance;
        }
    }

    private static StateGraphRenderer _instance = null;

    public State currentState;
    public bool showLabels;

    private GUIStyle _labelsStyle;

    private void Awake()
    {
        if (FindObjectsOfType<StateGraphRenderer>().Length > 1)
        {
            DestroyImmediate(this);
            return;
        }

        StateGraphMenu.UpdateDelegate();
    }

    public void RenderStateGraph(SceneView sceneview)
    {
        if (_labelsStyle == null)
        {
            _labelsStyle = new GUIStyle();
            _labelsStyle.normal.textColor = Color.green;
        }


        var states = FindObjectsOfType<State>();

        foreach (var state in states)
        {
            if (currentState != null && state != currentState)
                continue;

            var connections = state.GetComponents<Connection>();

            foreach (var connection in connections)
            {
                var firstConnectionPosition = connection.transform.position + connection.orientation * Vector3.forward;
                var secondConnectionPosition = connection.destination.state.transform.position +
                    connection.destination.orientation * Vector3.forward;

                Handles.color = Color.red;
                Handles.DotHandleCap(
                    0,
                    firstConnectionPosition,
                    connection.transform.rotation * Quaternion.LookRotation(Vector3.right),
                    0.05f,
                    EventType.Repaint
                );

                if (showLabels)
                {
                    Handles.color = Color.blue;
                    Handles.Label(firstConnectionPosition, connection.destination.state.title, _labelsStyle);
                }

                Handles.color = Color.yellow;
                Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);
            }
        }
    }
}
#endif
