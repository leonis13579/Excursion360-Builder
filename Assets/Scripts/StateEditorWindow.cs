using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

public class StateEditorWindow : EditorWindow
{
    private Connection _currentConnection;
    private List<State> _selectedStates = new List<State>();

    private bool _connectionsEditMode = false;

    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    [MenuItem("VR Tour/State editor")]
    static void CreateWizard()
    {
        var window = GetWindow<StateEditorWindow>();
        window.Show();
    }

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnGUI()
    {
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        if (_selectedStates.Count == 0 || _selectedStates.Count > 2)
        {
            GUILayout.Label("Select one state for editing it", EditorStyles.boldLabel);
            GUILayout.Label("Select two states for editing connections", EditorStyles.boldLabel);
        }
        else if (_selectedStates.Count == 1)
        {
            var state = _selectedStates[0];

            // Draw title edit field
            GUILayout.Label("State title: ", EditorStyles.boldLabel);
            state.title = EditorGUILayout.TextField(state.title);
            EditorGUILayout.Space();

            // Draw panorama texture edit field
            GUILayout.Label("State panorama: ", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            state.panoramaTexture = EditorGUILayout.ObjectField(state.panoramaTexture, typeof(Texture), true) as Texture;
            state.UpdateTexture();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Draw panorama preview
            var previewTexture = state.panoramaTexture;
            if (previewTexture == null) {
                previewTexture = EditorGUIUtility.whiteTexture;
            }
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, 150.0f), previewTexture, null, ScaleMode.ScaleToFit);
            EditorGUILayout.Space();

            if (GUILayout.Button("Focus camera"))
            {
                FocusCamera(state.gameObject);
                SelectObject(state.gameObject);
            }
            EditorGUILayout.Space();

            // Draw connections list
            GUILayout.Label("Connections: ", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            // Draw edit mode toggle
            _connectionsEditMode = EditorGUILayout.Toggle("Edit mode", _connectionsEditMode);
            if (_connectionsEditMode)
            {
                StateGraphRenderer.Instance.currentState = state;
            }
            else
            {
                StateGraphRenderer.Instance.currentState = null;
                _currentConnection = null;
                GUIUtility.GetControlID(FocusType.Keyboard);
            }

            // Draw labels toggle
            StateGraphRenderer.Instance.showLabels = EditorGUILayout.Toggle("Show labels", StateGraphRenderer.Instance.showLabels);

            GUILayout.EndHorizontal();

            var connections = state.GetComponents<Connection>();

            foreach (var connection in connections)
            {
                GUIStyle buttonStyle = ToggleButtonStyleNormal;
                if (_connectionsEditMode && _currentConnection == connection)
                    buttonStyle = ToggleButtonStyleToggled;

                if (GUILayout.Button(connection.destination.state.title, buttonStyle))
                {
                    if (_connectionsEditMode)
                    {
                        if (_currentConnection == connection)
                            _currentConnection = null;
                        else
                            _currentConnection = connection;
                    }
                    else
                    {
                        FocusCamera(connection.destination.state.gameObject);
                        SelectObject(connection.destination.state.gameObject);
                    }
                }
            }
            EditorGUILayout.Space();

            GUILayout.FlexibleSpace();
        }
        else
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Toggle connection", GUILayout.Height(50)))
            {
                CreateConnection(_selectedStates[0], _selectedStates[1]);
            }
        }

        if (GUI.changed)
        {
            foreach (var selection in _selectedStates)
            {
                EditorUtility.SetDirty(selection);
                EditorSceneManager.MarkSceneDirty(selection.gameObject.scene);
            }
        }

        // Draw button for adding new states
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create new state"))
        {
            var newObject = Instantiate(StateGraphRenderer.Instance.statePrefab, Vector3.zero, Quaternion.identity);
            SelectObject(newObject);
        }

        EditorGUILayout.Space();

        // Force redraw all

        Repaint();
        SceneView.RepaintAll();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        _selectedStates = GetSlectedStates();
        if (!_selectedStates.Find((State s) => 
        {
            return _currentConnection != null && s == _currentConnection.state;
        })) {
            _currentConnection = null;
        }

        if (_currentConnection == null)
            return;

        var state = _currentConnection.state;

        Handles.color = Color.green;
        Handles.DrawWireCube(
            state.transform.position + _currentConnection.orientation * Vector3.forward,
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

            RaycastHit hit;
            if (collider.Raycast(ray, out hit, 100.0f))
            {
                var toCenterDirection = state.transform.position - hit.point;
                var rightDirection = Vector3.Cross(toCenterDirection, ray.direction);
                var normal = Vector3.Cross(rightDirection, ray.direction);
                var hitPosition = state.transform.position + ReflectDirection(toCenterDirection, normal);
                
                _currentConnection.orientation = Quaternion.FromToRotation(Vector3.forward, 
                    hitPosition - state.transform.position);
            }
        }

    }

    public static Vector3 ReflectDirection(Vector3 inDirection, Vector3 normal)
    {
        var length = inDirection.magnitude;
        inDirection.Normalize();
        normal.Normalize();

        return (2 * (Vector3.Dot(inDirection, normal) * normal) - inDirection).normalized * -length;
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }

    private void FocusCamera(GameObject obj)
    {
        var sceneView = SceneView.lastActiveSceneView;

        var offset = sceneView.pivot - sceneView.camera.transform.position;
        var cameraDistance = offset.magnitude;
        sceneView.pivot = obj.transform.position + sceneView.camera.transform.forward * cameraDistance;
    }

    private void SelectObject(GameObject obj)
    {
        Selection.objects = new Object[] { obj };
    }

    private List<State> GetSlectedStates()
    {
        var result = new List<State>();

        foreach (var selection in Selection.gameObjects)
        {
            var state = selection.GetComponent<State>();
            if (state != null)
                result.Add(state);
        }

        return result;
    }

    private void CreateConnection(State firstState, State secondState)
    {
        var firstStateConnections = firstState.GetComponents<Connection>();
        var seconsStateConnections = secondState.GetComponents<Connection>();

        Connection connectionFirst = null;
        foreach (var connecton in firstStateConnections)
        {
            if (connecton.destination.state == secondState)
                connectionFirst = connecton;
        }

        Connection connectionSecond = null;
        foreach (var connecton in seconsStateConnections)
        {
            if (connecton.destination.state == firstState)
                connectionSecond = connecton;
        }

        if (connectionFirst == null && connectionSecond == null)
        {
            var directionToSecond = secondState.transform.position - firstState.transform.position;

            connectionFirst = firstState.gameObject.AddComponent<Connection>();
            connectionSecond = secondState.gameObject.AddComponent<Connection>();

            connectionFirst.state = firstState;
            connectionFirst.destination = connectionSecond;
            connectionFirst.orientation = Quaternion.FromToRotation(Vector3.forward, directionToSecond);

            connectionSecond.state = secondState;
            connectionSecond.destination = connectionFirst;
            connectionSecond.orientation = Quaternion.FromToRotation(Vector3.forward, -directionToSecond);
        }
        else
        {
            if (connectionFirst != null)
                DestroyImmediate(connectionFirst);

            if (connectionSecond != null)
                DestroyImmediate(connectionSecond);
        }
    }
}

#endif