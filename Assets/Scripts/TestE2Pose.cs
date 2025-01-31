using UnityEngine;
using UnityEngine.UI;

public class TestE2Pose : MonoBehaviour
{
    public Texture inputTexture;
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;
    public TestShowPeople testShowPeople;

    private DemoE2Pose e2Pose;

    private void Start()
    {
        e2Pose = new DemoE2Pose();
    }

    private void OnDestroy()
    {
        e2Pose?.Dispose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Exec();
        }
    }

    private void Exec()
    {
        Debug.Log("Exec!");

        var result = e2Pose.Run(inputTexture);

        if (result.Count > 0)
        {
            testShowPeople.SetKeypoints(result[0]);
        }

        rawImage.texture = inputTexture;
        aspectRatioFitter.aspectRatio = (float)inputTexture.width / inputTexture.height;
    }
}
