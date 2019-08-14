using System.Collections;
using UnityEngine;

public class FileTextureSource : TextureSource
{
    public Texture texture = null;

    public override IEnumerator LoadTexture()
    {
        loadedTexture = texture;
        yield break;
    }
}
