using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShowPeople : MonoBehaviour
{
    public RectTransform container;

    public RectTransform tempKeypoint;

    public int numKeypoint = 17;

    private List<RectTransform> keypoints;

    private void Start()
    {
        keypoints = new List<RectTransform>(numKeypoint);
        for (int i = 0; i < numKeypoint; i++)
        {
            keypoints.Add(Instantiate(tempKeypoint, container));
        }
    }

    public void SetKeypoints(List<Vector2> keypointsData)
    {
        if (keypointsData.Count != numKeypoint)
        {
            Debug.Log("个数不匹配");
            return;
        }

        for (int i = 0; i < numKeypoint; i++)
        {
            keypoints[i].anchoredPosition = keypointsData[i] * container.rect.size;
        }
    }
}
