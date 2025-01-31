using Microsoft.ML.OnnxRuntime.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "CreatScriptableObject/Config")]
public class ConfigScriptablObject : ScriptableObject
{
    #region 单例

    public static ConfigScriptablObject Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ConfigScriptablObject>("Config");
            }
            return instance;
        }
    }
    private static ConfigScriptablObject instance = null;

    #endregion

    [SerializeField]
    public OrtAsset yoloxModel;
    [SerializeField]
    public Yolox.Options yoloxOptions;

    [SerializeField]
    public OrtAsset hrnet_256_192_Model;
    [SerializeField]
    public DemoHRNet.OneTexHrnetPoseDetect.Options hrnetOptions;

    [SerializeField]
    public OrtAsset e2poseModel;
    [SerializeField]
    public E2Pose.Options e2poseOptions;


    [Space]
    [SerializeField]
    public UIBoundingBox uiBoundingBox;
}
