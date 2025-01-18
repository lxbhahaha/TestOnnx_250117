using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIBoundingBox : MonoBehaviour
{
    private Image image;
    private RectTransform detectionContainer;
    private RectTransform rt;
    private StringBuilder sb = new();

    private void Awake()
    {
        image = GetComponentInChildren<Image>();
        rt = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 设置Rect
    /// </summary>
    /// <param name="rect"></param>
    public void Set(Rect rect, RectTransform detectionContainer = null)
    {
        if (detectionContainer != null)
        {
            this.detectionContainer = detectionContainer;
        }

        // 设置Rect
        Vector2 viewportSize = detectionContainer.rect.size;
        rt.anchoredPosition = rect.min * viewportSize;
        rt.sizeDelta = rect.size * viewportSize;
    }

    /// <summary>
    /// 设置颜色
    /// </summary>
    /// <param name="color">颜色</param>
    public void SetColor(Color color)
    {
        image.color = color;
    }
}
