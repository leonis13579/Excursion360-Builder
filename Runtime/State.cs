using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * @brief Represents some place
 */
[ExecuteInEditMode]
public class State : MonoBehaviour
{
    /**
     * @brief Name of this place
     */
    public string title;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;

    void Awake()
    {
        if (
#if UNITY_EDITOR
            EditorApplication.isPlaying ||
#endif
            Application.isPlaying)
        {
            GetComponent<Renderer>().enabled = false;
        }

#if UNITY_EDITOR
        TextureSource textureSource = GetComponent<TextureSource>();
        if (textureSource == null)
            gameObject.AddComponent<FileImageSource>();

        ReloadTexture();
#endif
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        var connections = GetComponents<Connection>();
        foreach (var connection in connections)
            if (connection.destination)
                Undo.DestroyObjectImmediate(connection.destination);
    }

    void Update()
    {
        name = title;
    }

    public void Reload()
    {
        var connections = GetComponents<Connection>();
        foreach (var connection in connections)
        {
            if (connection.destination)
                connection.destination.destination = connection;
        }

        ReloadTexture();
    }

    public void ReloadTexture()
    {
        TextureSource textureSource = GetComponent<TextureSource>();
        if (textureSource == null)
            return;

        StartCoroutine(LoadTexture(textureSource));
    }

    private IEnumerator LoadTexture(TextureSource textureSource)
    {
        yield return textureSource.LoadTexture();

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_materialProperties);
        _materialProperties.SetTexture("_MainTex", textureSource.loadedTexture);
        _renderer.SetPropertyBlock(_materialProperties);
    }
#endif
}
