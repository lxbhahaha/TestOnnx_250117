using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoTest : MonoBehaviour
{
    public int maxFps = 30;
    private float frameInterval;
    private float intervalTimer;

    public RawImage background;
    public RectTransform bgRect;
    public AspectRatioFitter aspectRatioFitter;

    private DemoYoloxHumanDetect demoYolo;

    private List<UIBoundingBox> bboxList = new();
    private Rect[] cropRects;
    private float[] scroes;

    private DemoYoloxHumanDetect yolo1;
    private DemoYoloxHumanDetect yolo2;
    private DemoHRNet hrnet1;
    private DemoHRNet hrnet2;

    private void Awake()
    {
        frameInterval = 1f / maxFps;

        // yolo1 = new DemoYoloxHumanDetect();
        // hrnet1 = new DemoHRNet();

        ImageAcquisiton.Instance.InitAndPlay();
        demoYolo= new DemoYoloxHumanDetect();

        // yolo2 = new DemoYoloxHumanDetect();
        // hrnet2 = new DemoHRNet();
    }

    private void OnDestroy()
    {
        demoYolo?.Dispose();
        yolo1?.Dispose();
        yolo2?.Dispose();
        hrnet1?.Dispose();
        hrnet2?.Dispose();
    }

    private void Update()
    {
        intervalTimer += Time.deltaTime;
        if (intervalTimer > frameInterval)
        {
            intervalTimer -= frameInterval;
            Run();
        }
    }

    private void Run()
    {
        // Get image
        Texture tex = ImageAcquisiton.Instance.GetCurrentTexture();

        // Set image
        background.texture = tex;
        aspectRatioFitter.aspectRatio = (float)ImageAcquisiton.Instance.Width / ImageAcquisiton.Instance.Height;

        // Inference
        (cropRects, scroes) = demoYolo.Run(tex);

        // Show
        foreach (var bbox in bboxList)
        {
            bbox.gameObject.SetActive(false);
            ObjectPoolManager.Instance.ReleaseUIBoundingBox(bbox);
        }
        bboxList.Clear();
        for (int i = 0; i < cropRects.Length; i++)
        {
            var bbox = ObjectPoolManager.Instance.GetUIBoundingBox();
            bbox.transform.SetParent(bgRect);
            bbox.Set(cropRects[i], bgRect);
            bbox.gameObject.SetActive(true);
            bboxList.Add(bbox);
        }

        // Release
        ObjectPoolManager.Instance.ReleaseTexture(tex);
    }
}
