using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchVideo : MonoBehaviour
{
    public Renderer quad;

#if UNITY_SWITCH && !UNITY_EDITORs
    UnityEngine.Switch.SwitchVideoPlayer _video;
    UnityEngine.Switch.SwitchFMVTexture _lumaTex;
    UnityEngine.Switch.SwitchFMVTexture _chromaTex;

    void OnMovieEvent(UnityEngine.Switch.SwitchVideoPlayer.Event FMVevent)
    {
        Debug.Log("script has received FMV event :" + FMVevent);
    }

    void OnPreRender()
    {
        _video.Update();
        float normalized_progress = _video.GetCurrentTime() / _video.GetVideoLength();
    }

    public void VideoStart()
    {
        var moviePath = Application.streamingAssetsPath + "/Intro.mp4";
        _video = new UnityEngine.Switch.SwitchVideoPlayer(OnMovieEvent);

        int width = 1980;
        int height = 1080;
        //{   // if the resolution has already known, this should be removed so that the redundant file access could be suppressed.
        //    _video.GetTrackInfo(moviePath, out width, out height);
        //}
        {
            // when you didn't call this, system would guess a container type with the file extension.
            _video.SetContainerType(UnityEngine.Switch.SwitchVideoPlayer.ContainerType.Mpeg4);
        }

        _lumaTex = new UnityEngine.Switch.SwitchFMVTexture();
        _lumaTex.Create(width, height, UnityEngine.Switch.SwitchFMVTexture.Type.R8);
        _chromaTex = new UnityEngine.Switch.SwitchFMVTexture();
        _chromaTex.Create(width / 2, height / 2, UnityEngine.Switch.SwitchFMVTexture.Type.R8G8);
        _video.Init(_lumaTex, _chromaTex);

        quad.material.mainTexture = _lumaTex.GetTexture();
        quad.material.SetTexture("_ChromaTex", _chromaTex.GetTexture());

        _video.Play(moviePath);
    }

    public void VideoStop()
    {
        _video.Stop();
    }

    private void OnDestroy()
    {
        _video = null;
        _lumaTex = null;
        _chromaTex = null;
    }
#else
    public void VideoStart() { }
    public void VideoStop() { }
#endif
}
