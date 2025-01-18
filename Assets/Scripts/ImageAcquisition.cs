using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class ImageAcquisiton : MonoBehaviour
{
    #region 单例

    public static ImageAcquisiton Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("ImageAcquisitonModule");
                instance = go.AddComponent<ImageAcquisiton>();
            }
            return instance;
        }
    }
    private static ImageAcquisiton instance = null;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    #region 成员变量

    /// <summary>
    /// 当前摄像头画面的高度
    /// </summary>
    public int Height
    {
        get { return webcamTexture == null ? 0 : webcamTexture.height; }
    }
    /// <summary>
    /// 当前摄像头画面的宽度
    /// </summary>
    public int Width
    {
        get { return webcamTexture == null ? 0 : webcamTexture.width; }
    }

    /// <summary>
    /// 当前相机画面是否正在播放
    /// </summary>
    public bool isPlaying
    {
        get { return webcamTexture == null ? false : webcamTexture.isPlaying; }
    }

    /// <summary>
    /// 当前的web摄像头的图像
    /// </summary>
    private WebCamTexture webcamTexture = null;

    /// <summary>
    /// 是否在Init函数执行后立即播放
    /// </summary>
    private bool playAfterInit = true;

    /// <summary>
    /// 所需要初始化的相机的名称
    /// </summary>
    private string cameraName = "";

    /// <summary>
    /// 返回WebCamDevices的回调
    /// </summary>
    Action<WebCamDevice[]> callbackReturnDevices = null;

    #endregion

    #region 成员函数

    /// <summary>
    /// 获取当前的摄像头画面
    /// </summary>
    /// <returns>摄像头画面</returns>
    public RenderTexture GetCurrentTexture()
    {
        RenderTexture resultTex = ObjectPoolManager.Instance.GetRenderTexture(webcamTexture.width, webcamTexture.height);
        Graphics.CopyTexture(webcamTexture, resultTex);

        return resultTex;
    }

    /// <summary>
    /// 初始化相机，同时申请权限
    /// </summary>
    /// <param name="play">是否初始化完时立即开始播放</param>
    /// <param name="cameraName">所要初始化的相机名称，对应WebCamTexture.devices的name</param>
    public void InitAndPlay(bool play = true, string cameraName = "")
    {
        playAfterInit = play;
        this.cameraName = cameraName;
#if UNITY_IOS || UNITY_WEBGL
        StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => { Init(); }));
        return;
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
            callbacks.PermissionGranted += PermissionCallbackGrantedInitCamera;
            AskCameraPermission(callbacks);
            return;
        }
#endif
        Init();
    }

    /// <summary>
    /// 获取相机的Devices。
    /// Tips:
    /// 因为没有相机权限时无法获取结果，如果要申请权限需要等待回调，所以统一用回调来返回结果。
    /// 如果在已经确认有权限的情况下，可以直接使用`WebCamTexture.devices`来获取则不需要使用回调。
    /// </summary>
    /// <param name="callback">获取到时执行的回调函数</param>
    public void GetWebCamDevice(Action<WebCamDevice[]> callback)
    {
        callbackReturnDevices = callback;
#if UNITY_IOS || UNITY_WEBGL
        StartCoroutine(AskForPermissionIfRequired(UserAuthorization.WebCam, () => { callbackReturnDevices(WebCamTexture.devices); }));
        return;
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacksPermissionDenied;
            callbacks.PermissionGranted += PermissionCallbackGrantedReturnDevices;
            AskCameraPermission(callbacks);
            return;
        }
#endif
        callbackReturnDevices(WebCamTexture.devices);
    }

    /// <summary>
    /// 初始化，并且初始化之后直接播放（根据设置）
    /// </summary>
    private void Init()
    {
        if (cameraName == "")
            webcamTexture = new WebCamTexture();
        else
            webcamTexture = new WebCamTexture(cameraName);

        if (playAfterInit) Play();
    }

    /// <summary>
    /// 开启摄像头播放
    /// </summary>
    /// <returns>是否成功开启</returns>
    public bool Play()
    {
        if (webcamTexture == null) return false;

        webcamTexture.Play();
        return true;
    }

    /// <summary>
    /// 暂停摄像头
    /// </summary>
    public void Pause()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Pause();
        }
    }

    /// <summary>
    /// 停止摄像头
    /// </summary>
    public void Stop()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }

    #region 申请权限相关

#if UNITY_IOS || UNITY_WEBGL
    private bool CheckPermissionAndRaiseCallbackIfGranted(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (Application.HasUserAuthorization(authenticationType))
        {
            if (authenticationGrantedAction != null)
                authenticationGrantedAction();

            return true;
        }
        return false;
    }

    private IEnumerator AskForPermissionIfRequired(UserAuthorization authenticationType, Action authenticationGrantedAction)
    {
        if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
        {
            yield return Application.RequestUserAuthorization(authenticationType);
            if (!CheckPermissionAndRaiseCallbackIfGranted(authenticationType, authenticationGrantedAction))
                Debug.LogWarning($"Permission {authenticationType} Denied");
        }
    }
#elif UNITY_ANDROID

    /// <summary>
    /// 权限申请成功时的回调函数（执行相机初始化）
    /// </summary>
    /// <param name="permissionName"></param>
    private void PermissionCallbackGrantedInitCamera(string permissionName)
    {
        StartCoroutine(DelayedCameraInitialization());
    }

    /// <summary>
    /// 权限申请成功时的回调函数（返回相机设备）
    /// </summary>
    /// <param name="permissionName"></param>
    private void PermissionCallbackGrantedReturnDevices(string permissionName)
    {
        StartCoroutine(DelayedReturnDevices());
    }

    /// <summary>
    /// 延时一帧初始化相机
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedCameraInitialization()
    {
        yield return null;
        Init();
    }

    /// <summary>
    /// 延时一帧返回相机设备
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedReturnDevices()
    {
        yield return null;
        callbackReturnDevices(WebCamTexture.devices);
    }

    /// <summary>
    /// 权限申请失败时的回调函数
    /// </summary>
    /// <param name="permissionName"></param>
    private void PermissionCallbacksPermissionDenied(string permissionName)
    {
        Debug.LogWarning($"Permission {permissionName} Denied");
    }

    /// <summary>
    /// 申请相机权限
    /// </summary>
    /// <param name="callbacks">申请结束的时候执行的回调函数</param>
    private void AskCameraPermission(PermissionCallbacks callbacks)
    {
        Permission.RequestUserPermission(Permission.Camera, callbacks);
    }
#endif

    #endregion

    #endregion
}
