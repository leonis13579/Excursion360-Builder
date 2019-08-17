using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UrlImageSource : TextureSource
{
    [TextArea]
    public string textureUrl = "";

    private string _currentUrl = null;
    private Texture _currentTexture = null;

    public override SourceType GetSourceType()
    {
        return SourceType.Image;
    }

    public override IEnumerator LoadTexture()
    {
        if (textureUrl == "")
        {
            loadedTexture = null;
            _currentUrl = null;
            _currentTexture = null;
            yield break;
        }

        if (_currentUrl == null || _currentUrl != textureUrl)
        {
            _currentUrl = textureUrl;

            UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(textureUrl);

            yield return textureRequest.SendWebRequest();

            if (textureRequest.isHttpError || textureRequest.isNetworkError)
            {
                Debug.LogError("Unable to load texture: " + textureUrl);
                _currentTexture = null;
            }
            else
                _currentTexture = DownloadHandlerTexture.GetContent(textureRequest);
        }

        loadedTexture = _currentTexture;
    }

#if UNITY_EDITOR
    public override string Export(string destination, string stateName)
    {
        return textureUrl;
    }
#endif
}
