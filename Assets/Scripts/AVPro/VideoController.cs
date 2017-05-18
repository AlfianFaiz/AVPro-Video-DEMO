using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{
    public static VideoController instance;

    #region members
    // AVPro Video
    public MediaPlayer mediaPlayer;

    // flag
    private bool isPlaying = true;// 视频是否正在播放
    private float _setVideoSeekSliderValue;// 校验标识
    private bool isPlayingWhenDragging;// 拖动进度条时视频是否正在播放

    // UI
    public Button btnPlayPause;// 播放/暂停按钮
    //public Button btnStop;// 停止按钮
    public Slider sliderVideoProgress;// 视频进度条
    public Text textCurrentVideoTime;// 当前播放时长
    public Text textFullVideoTime;// 视频总时长
    public Sprite iconPlay;// 播放图标
    public Sprite iconPause;// 暂停图标
    //public Sprite iconStop;// 停止图标
    #endregion

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        if (mediaPlayer)
        {
            mediaPlayer.Events.AddListener(OnVideoEvent);
            // TODO get video path;
            OnOpenVideoFile("BigBuckBunny_720p30.mp4");
        }
    }

    void Update()
    {
        // 时长信息
        float currentTime = mediaPlayer.Control.GetCurrentTimeMs();
        float durationTime = mediaPlayer.Info.GetDurationMs();

        // 设置界面时长文本
        textCurrentVideoTime.text = TimeConvert(currentTime);
        textFullVideoTime.text = TimeConvert(durationTime);

        // 计算播放进度
        if (mediaPlayer && mediaPlayer.Info != null && durationTime > 0f)
        {
            float ratio = currentTime / durationTime;
            _setVideoSeekSliderValue = ratio;
            sliderVideoProgress.value = ratio;
        }

    }

    /// <summary>
    /// AVPro Video打开视频文件
    /// </summary>
    public void OnOpenVideoFile(string videoPath)
    {
        mediaPlayer.m_VideoPath = string.Empty;
        mediaPlayer.m_VideoPath = videoPath;
        if (string.IsNullOrEmpty(mediaPlayer.m_VideoPath))
        {
            mediaPlayer.CloseVideo();
        }
        else
        {
            mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, mediaPlayer.m_VideoPath/*, AutoStartToggle.isOn*/);
        }
    }

    /// <summary>
    /// 视频进度条数值发生变化
    /// On Value Changed
    /// </summary>
    public void OnVideoSeekSlider()
    {
        if (mediaPlayer && sliderVideoProgress && sliderVideoProgress.value != _setVideoSeekSliderValue)
        {
            // 设置视频进度
            mediaPlayer.Control.Seek(sliderVideoProgress.value * mediaPlayer.Info.GetDurationMs());
        }
    }

    /// <summary>
    /// 开始拖动视频进度条
    /// Begin Drag
    /// </summary>
    public void OnVideoSliderDown()
    {
        if (mediaPlayer)
        {
            // 拖动进度条时应该暂停视频播放，否则一边拖进度条一边自己走
            isPlayingWhenDragging = mediaPlayer.Control.IsPlaying();
            if (isPlayingWhenDragging)
            {
                mediaPlayer.Control.Pause();
            }

            // 处理拖动造成的数值变化
            // 这句话貌似没用，但是官方示例这样写了
            OnVideoSeekSlider();
        }
    }

    /// <summary>
    /// 结束拖动视频进度条
    /// End Drag
    /// </summary>
    public void OnVideoSliderUp()
    {
        // 拖动结束后恢复视频的播放状态
        if (mediaPlayer && isPlayingWhenDragging)
        {
            mediaPlayer.Control.Play();
            isPlayingWhenDragging = false;
        }
    }

    /// <summary>
    /// 播放/暂停按钮的点击事件
    /// </summary>
    public void OnPlayPauseButtonClick()
    {
        if (isPlaying)// 正在播放
        {
            mediaPlayer.Control.Pause();
            isPlaying = false;
            btnPlayPause.GetComponent<Image>().sprite = iconPlay;
        }
        else// 正暂停播放
        {
            // 兼容低版本Unity
            if (mediaPlayer.Control.IsFinished())
            {
                mediaPlayer.Control.Rewind();
            }
            else
            {
                mediaPlayer.Control.Play();
            }

            isPlaying = true;
            btnPlayPause.GetComponent<Image>().sprite = iconPause;
        }
    }

    /// <summary>
    /// 停止按钮的点击事件
    /// </summary>
    public void OnStopButtonClick()
    {
        mediaPlayer.CloseVideo();
        mediaPlayer.m_VideoPath = string.Empty;
    }

    /// <summary>
    /// AVPro Video状态变化事件回调
    /// </summary>
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                break;
            case MediaPlayerEvent.EventType.Started:
                // 视频开始播放
                isPlaying = true;
                btnPlayPause.GetComponent<Image>().sprite = iconPause;
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                // 视频播放结束
                isPlaying = false;
                btnPlayPause.GetComponent<Image>().sprite = iconPlay;
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }

    /// <summary>
    /// 计算时长
    /// </summary>
    /// <param name="ms">毫秒</param>
    /// <returns>hh:mm:ss格式的时长字符串</returns>
    private string TimeConvert(float ms)
    {
        int _seconds = (int)(ms / 1000);
        int hours = _seconds / (60 * 60);
        int minutes = (_seconds % (60 * 60)) / 60;
        int seconds = _seconds % (60 * 60 * 60);

        return (string.Format("{0:00}", hours) + ":" + string.Format("{0:00}", minutes) + ":" + string.Format("{0:00}", seconds));
    }
}
