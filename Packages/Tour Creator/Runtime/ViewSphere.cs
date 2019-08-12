using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * @brief Main object
 */
public class ViewSphere : MonoBehaviour
{
    public static ViewSphere Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ViewSphere>();

            return _instance;
        }
    }

    private static ViewSphere _instance;

    public State startState;
    public Marker markerPrefab;

    /**
     * @brief Transition speed in seconds
     */
    public float transitionSpeed = 2.0f;

    private State _currentState;
    private State _nextState;
    private float _transition;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;

    private List<Marker> _markers = new List<Marker>();

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        Assert.IsNotNull(_renderer);

        Assert.IsNotNull(startState);
        _currentState = startState;
        _nextState = null;

        SpawnConnections();
    }

    void OnDestroy()
    {
        ClearConnections();
    }

    void Update()
    {
        if (_nextState != null)
        {
            _transition += Time.deltaTime;
            if (_transition >= 1.0f) {
                _currentState = _nextState;
                _nextState = null;
                _transition = 0.0f;

                SpawnConnections();
            }
        }

        UpdateMaterial();
    }

    public void StartTransition(State nextState)
    {
        if (_nextState != null || _transition > 0.0f)
            return;

        ClearConnections();

        _nextState = nextState;
        _transition = 0.0f;
    }

    public void SpawnConnections()
    {
        var connections = _currentState.GetComponents<Connection>();
        
        foreach (var connection in connections)
        {
            Marker marker = Instantiate(markerPrefab, transform);
            marker.name = "Marker to " + connection.destination.origin.title;
            marker.connection = connection;
            marker.transform.localPosition = connection.orientation * Vector3.forward;

            _markers.Add(marker);
        }
    }

    public void ClearConnections()
    {
        foreach (var marker in _markers)
        {
            Destroy(marker.gameObject);
        }
        _markers.Clear();
    }

    private void UpdateMaterial()
    {
        if (_currentState == null || _transition != 0.0f && _nextState == null)
            return;

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_materialProperties);

        // Set main texture and orientation
        _materialProperties.SetTexture("_MainTex", _currentState.panoramaTexture);

        var mr = _currentState.transform.rotation;
        _materialProperties.SetVector("_MainOrientation", new Vector4(
            mr.x, mr.y, mr.z, mr.w));

        if (_nextState != null)
        {
            // Set next texture and orientation
            _materialProperties.SetTexture("_NextTex", _nextState.panoramaTexture);

            var nr = _nextState.transform.rotation;
            _materialProperties.SetVector("_NextOrientation", new Vector4(
                nr.x, nr.y, nr.z, nr.w));
        }

        // Set transition
        _materialProperties.SetFloat("_Transition", _transition);

        _renderer.SetPropertyBlock(_materialProperties);
    }
}
