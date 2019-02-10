using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PlayerState>();

            return _instance;
        }
    }

    private static PlayerState _instance;
    
    public Marker markerPrefab;
    public Renderer panoramaSphere;

    public State currentState;
    public float transitionSpeed = 2.0f;

    private State _nextState;
    private float _transition;

    private MaterialPropertyBlock _materialProperties;

    private List<Marker> _markers = new List<Marker>();

    private void Start()
    {
        _nextState = null;
        _transition = 0.0f;

        SpawnConnections();
    }

    private void OnDestroy()
    {
        ClearConnections();
    }

    private void Update()
    {
        if (_nextState != null)
        {
            _transition += Time.deltaTime;
            if (_transition >= 1.0f) {
                currentState = _nextState;
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
        if (currentState == null)
            return;

        var connections = currentState.GetComponents<Connection>();
        
        foreach (var connection in connections)
        {
            Marker marker = Instantiate(markerPrefab, transform);
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
        if (currentState == null || _transition != 0.0f && _nextState == null)
            return;

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();

        panoramaSphere.GetPropertyBlock(_materialProperties);

        // Set main texture and orientation
        _materialProperties.SetTexture("_MainTex", currentState.panoramaTexture);

        var mr = currentState.transform.rotation;
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

        panoramaSphere.SetPropertyBlock(_materialProperties);
    }
}
