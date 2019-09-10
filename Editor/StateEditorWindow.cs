using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

public class StateEditorWindow : EditorWindow
{
    private Connection _currentConnection;
    private List<State> _selectedStates = new List<State>();

    private bool _connectionsEditMode = false;

    private TextureSourceEditor _textureSourceEditor = new TextureSourceEditor();

    private Vector2 _connectionsListScroll = Vector2.zero;

    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        // Create styles if not exists
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        // Draw pages
        if (_selectedStates.Count == 0 || _selectedStates.Count > 2)
        {
            DrawIdlePageGUI();
        }
        else if (_selectedStates.Count == 1)
        {
            DrawStateEditPageGUI();
        }
        else
        {
            DrawConnectionEditGUI();
        }

        // Force update scene on change (for savig)
        if (GUI.changed)
        {
            foreach (var selection in _selectedStates)
            {
                EditorUtility.SetDirty(selection);
                EditorSceneManager.MarkSceneDirty(selection.gameObject.scene);
            }
        }

        // Draw state creation button
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create new state", GUILayout.Height(50)))
        {
            var newObject = PrefabUtility.InstantiatePrefab(TourEditor.StatePrefab) as GameObject;
            SelectObject(newObject);
            Undo.RegisterCreatedObjectUndo(newObject, "Undo state creation");
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
            return _currentConnection != null && s == _currentConnection.origin;
        })) {
            _currentConnection = null;
        }

        if (_currentConnection == null)
            return;

        var state = _currentConnection.origin;

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

            if (collider.Raycast(ray, out RaycastHit hit, 100.0f))
            {
                var toCenterDirection = state.transform.position - hit.point;
                var rightDirection = Vector3.Cross(toCenterDirection, ray.direction);
                var normal = Vector3.Cross(rightDirection, ray.direction);
                var hitPosition = state.transform.position + ReflectDirection(toCenterDirection, normal);

                Undo.RecordObject(_currentConnection, "Undo orientation change");

                _currentConnection.orientation = Quaternion.FromToRotation(Vector3.forward,
                    hitPosition - state.transform.position);
            }
        }
    }

    private void DrawIdlePageGUI()
    {
        GUILayout.Label("Select one state for editing it", EditorStyles.boldLabel);
        GUILayout.Label("Select two states for editing connections", EditorStyles.boldLabel);
    }

    private void DrawStateEditPageGUI()
    {
        if (EditorApplication.isPlaying)
            return;

        var state = _selectedStates[0];

        if (_connectionsEditMode)
            TourEditor.StateGraphRenderer.targetState = state;

        // Draw title edit field
        GUILayout.Label("State title: ", EditorStyles.boldLabel);
        state.title = EditorGUILayout.TextField(state.title);
        EditorGUILayout.Space();

        // Draw panorama texture edit field
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Panorama: ", EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Change texture source"))
            _textureSourceEditor.ShowContextMenu(state);

        EditorGUILayout.EndHorizontal();

        _textureSourceEditor.Draw(state);

        EditorGUILayout.Space();

        // Draw panorama preview

        var previewTexture = state.GetComponent<TextureSource>().loadedTexture;
        if (previewTexture == null)
        {
            previewTexture = EditorGUIUtility.whiteTexture;
        }
        EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, 150.0f), previewTexture, null, ScaleMode.ScaleToFit);


        EditorGUILayout.Space();

        GUILayout.Label("Actions: ", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Focus camera", GUILayout.Height(50)))
        {
            FocusCamera(state.gameObject);
            SelectObject(state.gameObject);
        }

        // Draw edit mode toggle
        GUIStyle editModeButtonStyle = _connectionsEditMode? ToggleButtonStyleToggled : ToggleButtonStyleNormal;
        if (GUILayout.Button("Edit mode", editModeButtonStyle, GUILayout.Height(50)))
        { 
            if (_connectionsEditMode)
            {
                TourEditor.StateGraphRenderer.targetState = null;
                _currentConnection = null;
            }
            else
            {
                TourEditor.StateGraphRenderer.targetState = state;
            }

            _connectionsEditMode = !_connectionsEditMode;
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Draw connections list
        GUILayout.Label("Connections: ", EditorStyles.boldLabel);

        _connectionsListScroll = EditorGUILayout.BeginScrollView(_connectionsListScroll);

        var connections = state.GetComponents<Connection>();

        foreach (var connection in connections)
        {
            if (connection.destination == null)
                continue;

            GUIStyle buttonStyle = ToggleButtonStyleNormal;
            if (_connectionsEditMode && _currentConnection == connection)
                buttonStyle = ToggleButtonStyleToggled;

            if (GUILayout.Button(connection.destination.origin.title, buttonStyle))
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
                    FocusCamera(connection.destination.origin.gameObject);
                    SelectObject(connection.destination.origin.gameObject);
                }
            }

            var schemes = Tour.Instance.colorSchemes;
            var schemeNames = schemes.Select(s => s.name).ToArray();
            connection.colorScheme = EditorGUILayout.Popup("Color scheme", connection.colorScheme, schemeNames);
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        GUILayout.FlexibleSpace();
    }

    void DrawConnectionEditGUI()
    {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Toggle connection", GUILayout.Height(50)))
        {
            CreateConnection(_selectedStates[0], _selectedStates[1]);
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
            if (connecton.destination == null)
                continue;

            if (connecton.destination.origin == secondState)
                connectionFirst = connecton;
        }

        Connection connectionSecond = null;
        foreach (var connecton in seconsStateConnections)
        {
            if (connecton.destination == null)
                continue;

            if (connecton.destination.origin == firstState)
                connectionSecond = connecton;
        }

        if (connectionFirst == null && connectionSecond == null)
        {
            var directionToSecond = secondState.transform.position - firstState.transform.position;

            connectionFirst = Undo.AddComponent<Connection>(firstState.gameObject);
            connectionSecond = Undo.AddComponent<Connection>(secondState.gameObject);

            connectionFirst.destination = connectionSecond;
            connectionFirst.orientation = Quaternion.FromToRotation(Vector3.forward, directionToSecond);

            connectionSecond.destination = connectionFirst;
            connectionSecond.orientation = Quaternion.FromToRotation(Vector3.forward, -directionToSecond);
        }
        else
        {
            if (connectionFirst != null)
                Undo.DestroyObjectImmediate(connectionFirst);

            if (connectionSecond != null)
                Undo.DestroyObjectImmediate(connectionSecond);
        }
    }
}

#endif