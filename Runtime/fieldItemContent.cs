using Excursion360_Builder.Shared.States.Items.Field;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fieldItemContent : Marker
{
    private FieldItem fieldItem;

    private FileImageSource imageSource;
    private FileVideoSource videoSource;

    public override string Title => "";

    public void Init(FieldItem fieldItem)
    {
        this.fieldItem = fieldItem;

        GetComponentInChildren<Canvas>().worldCamera = Camera.current;
        imageSource = GetComponentInChildren<FileImageSource>();
        videoSource = GetComponentInChildren<FileVideoSource>();

        switch (fieldItem.contentType)
        {
            case FieldItem.ContentType.Photo:
                StartCoroutine(CreatePhoto());
                break;

            case FieldItem.ContentType.Video:
                break;
        }
    }

    private IEnumerator CreatePhoto()
    {
        imageSource.texture = fieldItem.texture;
        yield return StartCoroutine(imageSource.LoadTexture());

        var rawImage = GetComponentInChildren<RawImage>();
        rawImage.texture = imageSource.LoadedTexture;
        rawImage.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(imageSource.LoadedTexture.width, imageSource.LoadedTexture.height);

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(imageSource.LoadedTexture.width * 0.001f, imageSource.LoadedTexture.height * 0.001f);
    }

    public override void HandleInteract()
    {
        Destroy(gameObject);
    }
}
