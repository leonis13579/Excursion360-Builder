using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class TextureSource : MonoBehaviour
{
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

    public abstract IEnumerator LoadTexture();

    protected virtual void OnStartUsing()
    {
    }

    protected virtual void OnStopUsing()
    {
    }
}
