using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[ExecuteInEditMode]
public class VideoPreviewGenerator : MonoBehaviour
{
    public VideoClip videoClip;
    public RenderTexture renderTexture;

    private VideoPlayer _videoPlayer;
    private bool _shouldDie = false;

    void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        _videoPlayer.clip = videoClip;
        _videoPlayer.targetTexture = renderTexture;

        _videoPlayer.sendFrameReadyEvents = true;
        _videoPlayer.frameReady += (VideoPlayer source, long index) =>
        {
            source.Pause();
            _shouldDie = true;
        };

        _videoPlayer.Play();
    }

    void Update()
    {
        if (_shouldDie)
        {
            _shouldDie = false;
            DestroyImmediate(gameObject);
        }
    }
}
