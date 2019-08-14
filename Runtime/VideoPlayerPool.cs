using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerPool : MonoBehaviour
{
    private class Pair
    {
        public VideoPlayer videoPlayer;
        public bool available;
    }

    private List<Pair> _videoPlayers = new List<Pair>();
    
    public VideoPlayer Aquire()
    {
        var pair = _videoPlayers.Find(p => p.available);
        if (pair != null)
        {
            pair.available = false;
            return pair.videoPlayer;
        }

        var videoPlayer = gameObject.AddComponent<VideoPlayer>();

        pair = new Pair
        {
            videoPlayer = videoPlayer,
            available = false
        };
        _videoPlayers.Add(pair);

        return videoPlayer;
    }

    public void Release(VideoPlayer videoPlayer)
    {
        if (videoPlayer == null)
            return;

        var pair = _videoPlayers.Find(p => p.videoPlayer == videoPlayer);
        if (pair != null)
            pair.available = true;
    }
}
