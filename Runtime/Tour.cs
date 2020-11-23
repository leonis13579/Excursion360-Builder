﻿using Excursion360_Builder.Runtime.Markers;
using Excursion360_Builder.Shared.States.Items.Field;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public ConnectionMarker connectionMarkerPrefab;
    public GroupConnectionMarker groupConnectionMarkerPrefab;
    public FieldItemMarker baseFieldItemGameObject;

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

    private State _currentState = null;
    private TextureSource _currentTextureSource = null;

    private State _nextState = null;
    private TextureSource _nextTextureSource = null;

    private float _transition;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;
    private VideoPlayerPool _videoPlayerPool;

    private readonly List<Marker> _markers = new List<Marker>();

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        Assert.IsNotNull(_renderer);

        Assert.IsNotNull(firstState);

        Assert.IsTrue(colorSchemes.Length > 0, "Need minimum 1 element in colors collection");
        _currentState = firstState;
        _currentTextureSource = PrepareState(_currentState);

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
                _currentTextureSource.InUse = false;
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
        _nextTextureSource = PrepareState(_nextState);

        _transition = 0.0f;
    }

    public void SpawnConnections()
    {
        var connections = _currentState.GetComponents<Connection>();
        
        foreach (var connection in connections)
        {
            ConnectionMarker marker = Instantiate(connectionMarkerPrefab, transform);
            marker.name = "Marker to " + connection.GetDestenationTitle();
            marker.connection = connection;
            marker.transform.localPosition = connection.Orientation * Vector3.forward;
            var markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.SetColor("_Color", colorSchemes[connection.colorScheme].color);
            _markers.Add(marker);
        }

        var groupConnections = _currentState.GetComponents<GroupConnection>();

        foreach (var groupConnection in groupConnections)
        {
            GroupConnectionMarker marker = Instantiate(groupConnectionMarkerPrefab, transform);
            marker.name = "Group Marker to " + groupConnection.title;
            marker.groupConnection = groupConnection;
            marker.transform.localPosition = groupConnection.Orientation * Vector3.forward;
            var markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.SetColor("_Color", Color.blue);
            _markers.Add(marker);
        }

        var fieldItems = _currentState.GetComponents<FieldItem>();


        foreach (var fieldItem in fieldItems)
        {
            var fieldItemMarker = Instantiate(baseFieldItemGameObject, transform);
            fieldItemMarker.Init(fieldItem);
            _markers.Add(fieldItemMarker);
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
        _materialProperties.SetTexture("_MainTex", _currentTextureSource.LoadedTexture);

        var mr = _currentState.transform.rotation;
        _materialProperties.SetVector("_MainOrientation", new Vector4(
            mr.x, mr.y, mr.z, mr.w));

        if (_nextState != null && _nextTextureSource != null)
        {
            // Set next texture and orientation
            _materialProperties.SetTexture("_NextTex", _nextTextureSource.LoadedTexture);

            var nr = _nextState.transform.rotation;
            _materialProperties.SetVector("_NextOrientation", new Vector4(
                nr.x, nr.y, nr.z, nr.w));
        }

        // Set transition
        _materialProperties.SetFloat("_Transition", _transition);

        _renderer.SetPropertyBlock(_materialProperties);
    }

    private TextureSource PrepareState(State state)
    {
        var textureSource = state.GetComponent<TextureSource>();
        textureSource.InUse = true;
        StartCoroutine(textureSource.LoadTexture());

        var connections = state.GetComponents<Connection>();
        foreach (var connection in connections)
        {
            if (connection.Destination)
                StartCoroutine(connection.Destination.GetComponent<TextureSource>().LoadTexture());
        }
        return textureSource;
    }
}
