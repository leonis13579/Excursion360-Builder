using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[ExecuteInEditMode]
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

    public Texture defaultTexture;

    public VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    }
}
