using System.Collections;

#if UNITY_EDITOR
using UnityEngine;
#endif

[DisallowMultipleComponent]
public abstract class TextureSource : MonoBehaviour
{
    public enum SourceType
    {
        Image,
        Video,
        Stream
    }

    public Texture loadedTexture { 
        protected set
        {
            _loadedTexture = value;
        }
        get
        {
            if (_loadedTexture == null)
                return Tour.Instance.defaultTexture;

            return _loadedTexture;
        }
    }

    public bool inUse { 
        set
        {
            if (value == _inUse)
                return;
            _inUse = value;

            if (value)
                OnStartUsing();
            else
                OnStopUsing();
        }
        get
        {
            return _inUse;
        }
    }

    private bool _inUse = false;
    private Texture _loadedTexture = null;

    public abstract SourceType GetSourceType();

    public abstract IEnumerator LoadTexture();

    protected virtual void OnStartUsing()
    {
    }

    protected virtual void OnStopUsing()
    {
    }

#if UNITY_EDITOR
    public abstract string Export(string destination, string stateName);
#endif
}
