using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;

/**
 * @brief Main object
 */
public class Tour : MonoBehaviour
{
    public static Tour Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Tour>();

            return _instance;
        }
    }

    private static Tour _instance;

    public State firstState;
    public Marker markerPrefab;

    public Texture defaultTexture;
    public Texture logoTexture;


    public VideoPlayerPool videoPlayerPool 
    { 
        get
        {
            if (_videoPlayerPool == null)
            {
                var pool = new GameObject();
                pool.name = "__video_pool__";
                _videoPlayerPool = pool.AddComponent<VideoPlayerPool>();
            }

            return _videoPlayerPool;
        }
    }

    /**
     * @brief Transition speed in seconds
     */
    public float transitionSpeed = 2.0f;
    public ColorScheme[] colorSchemes = new ColorScheme[] { new ColorScheme { color = Color.red, name = "default" } };
    public string linkPrefix;

    private State _currentState = null;
    private TextureSource _currentTextureSource = null;

    private State _nextState = null;
    private TextureSource _nextTextureSource = null;

    private float _transition;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;
    private VideoPlayerPool _videoPlayerPool;

    private List<Marker> _markers = new List<Marker>();

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        Assert.IsNotNull(_renderer);

        Assert.IsNotNull(firstState);

        Assert.IsTrue(colorSchemes.Length > 0, "Need minimum 1 element in colors collection");
        _currentState = firstState;
        PrepareState(_currentState, ref _currentTextureSource);

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
                _currentTextureSource.inUse = false;
                _currentState = _nextState;
                _currentTextureSource = _nextTextureSource;
                _nextState = null;
                _nextTextureSource = null;
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
        PrepareState(_nextState, ref _nextTextureSource);

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
            var markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.SetColor("_Color", colorSchemes[connection.colorScheme].color);
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
        _materialProperties.SetTexture("_MainTex", _currentTextureSource.loadedTexture);

        var mr = _currentState.transform.rotation;
        _materialProperties.SetVector("_MainOrientation", new Vector4(
            mr.x, mr.y, mr.z, mr.w));

        if (_nextState != null && _nextTextureSource != null)
        {
            // Set next texture and orientation
            _materialProperties.SetTexture("_NextTex", _nextTextureSource.loadedTexture);

            var nr = _nextState.transform.rotation;
            _materialProperties.SetVector("_NextOrientation", new Vector4(
                nr.x, nr.y, nr.z, nr.w));
        }

        // Set transition
        _materialProperties.SetFloat("_Transition", _transition);

        _renderer.SetPropertyBlock(_materialProperties);
    }

    private void PrepareState(State state, ref TextureSource textureSource)
    {
        textureSource = state.GetComponent<TextureSource>();
        textureSource.inUse = true;
        StartCoroutine(textureSource.LoadTexture());

        var connections = state.GetComponents<Connection>();
        foreach (var connection in connections)
        {
            if (connection.destination)
                StartCoroutine(connection.destination.GetComponent<TextureSource>().LoadTexture());
        }
    }
}
