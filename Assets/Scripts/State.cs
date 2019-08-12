using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Texture panoramaTexture;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;

    void Start()
    {
        if (
#if UNITY_EDITOR
            EditorApplication.isPlaying ||
#endif
            Application.isPlaying)
        {
            GetComponent<Renderer>().enabled = false;
        }

        UpdateTexture();
    }

    void Update()
    {
#if UNITY_EDITOR
        name = title;
#endif
    }

    public void UpdateTexture()
    {
        if (panoramaTexture == null)
            return;

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();
        
        _renderer.GetPropertyBlock(_materialProperties);
        _materialProperties.SetTexture("_MainTex", panoramaTexture);
        _renderer.SetPropertyBlock(_materialProperties);
    }
}
