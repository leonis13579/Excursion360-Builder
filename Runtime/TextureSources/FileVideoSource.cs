using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FileVideoSource : TextureSource
{
    public VideoClip videoClip;

    private RenderTexture _renderTexture = null;
    private VideoClip _currentVideoClip = null;
    private bool _isPlaying = false;

    public override IEnumerator LoadTexture()
    {
        if (videoClip == null)
        {
            loadedTexture = null;
            _renderTexture = null;
            _currentVideoClip = null;
            yield break;
        }

        if (_currentVideoClip == null || _currentVideoClip != videoClip)
        { 
            _renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0,
                RenderTextureFormat.ARGB32);
            _currentVideoClip = videoClip;

#if UNITY_EDITOR
            GameObject previewGeneratorObject = new GameObject();
            previewGeneratorObject.name = "__preview_generator__";
            previewGeneratorObject.SetActive(false);

            var previewGenerator = previewGeneratorObject.AddComponent<VideoPreviewGenerator>();
            previewGenerator.videoClip = videoClip;
            previewGenerator.renderTexture = _renderTexture;

            previewGeneratorObject.SetActive(true);
#endif
        }

        loadedTexture = _renderTexture;
        Debug.Log("Updating");

        if (inUse && _renderTexture != null && !_isPlaying)
        {
            var videoPlayer = Tour.Instance.videoPlayer;

            videoPlayer.targetTexture = _renderTexture;
            videoPlayer.Play();
            Debug.Log("Play");

            _isPlaying = true;
        }
    }

    protected override void OnStartUsing()
    {
        var videoPlayer = Tour.Instance.videoPlayer;

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.isLooping = true;

        videoPlayer.clip = videoClip;
        videoPlayer.Pause();

        Debug.Log("Start using");
    }

    protected override void OnStopUsing()
    {
        Tour.Instance.videoPlayer.Pause();
        _isPlaying = false;

        Debug.Log("Stop using");
    }
}
