using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime.Unity;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Text;

public class DemoE2Pose : IDisposable
{
    private E2Pose e2pose;

    public DemoE2Pose()
    {
        e2pose = new E2Pose(ConfigScriptablObject.Instance.e2poseModel.bytes, ConfigScriptablObject.Instance.e2poseOptions);
    }

    public void Dispose()
    {
        e2pose?.Dispose();
    }

    public List<List<Vector2>> Run(Texture texture)
    {
        // Run model
        e2pose.Run(texture);
        var result = e2pose.Detections;

        // Log result
        Debug.Log("Result count: " + result.Length);
        foreach (var resultItem in result)
        {
            var sb = new StringBuilder();
            sb.AppendLine(resultItem.probability.ToString());
            for (int i = 0; i < resultItem.keypoints.Length; i++)
            {
                sb.AppendLine(resultItem.keypoints[i].coordinate.ToString());
            }
            Debug.Log(sb.ToString());
        }

        // Extract results
        List<List<Vector2>> finalResult = new(result.Length);
        for (int i = 0; i < result.Length; i++)
        {
            List<Vector2> tempRes = new(result[i].keypoints.Length);
            for (int j = 0; j < result[i].keypoints.Length; j++)
            {
                tempRes.Add(result[i].keypoints[j].coordinate);
            }
            finalResult.Add(tempRes);
        }
        return finalResult;
    }
}

public class E2Pose : ImageInference<float>
{
    [Serializable]
    public class Options : ImageInferenceOptions
    {
        [Range(1, 20)]
        public int maxDetections = 10;
        [Range(0f, 1f)]
        public float probThreshold = 0.5f;
        [Range(1, 40)]
        public int keypointCount = 17;
    }

    public struct Detection : IComparable<Detection>, IDisposable
    {
        public NativeArray<Keypoint> keypoints;
        public float probability;

        public Detection(NativeArray<Keypoint> keypoints, float probability)
        {
            this.keypoints = keypoints;
            this.probability = probability;
        }

        public int CompareTo(Detection other)
        {
            return other.probability.CompareTo(probability);
        }

        public void Dispose()
        {
            if (keypoints.IsCreated)
            {
                keypoints.Dispose();
            }
        }
    }

    public readonly struct Keypoint
    {
        public readonly Vector2 coordinate;
        public readonly float probability;

        public Keypoint(Vector2 coordinate, float probability)
        {
            this.coordinate = coordinate;
            this.probability = probability;
        }
    }

    private Options options;

    private NativeArray<Detection> poseList;

    public ReadOnlySpan<Detection> Detections => poseList.AsReadOnlySpan()[..selectCount];

    private int[] selectIdx;
    private int selectCount;

    public E2Pose(byte[] model, Options options) : base(model, options)
    {
        this.options = options;

        selectIdx = new int[outputs[1].GetTensorDataAsSpan<float>().Length];

        poseList = new NativeArray<Detection>(options.maxDetections, Allocator.Persistent);
    }

    protected override void PostProcess()
    {
        var kpt = outputs[0].GetTensorDataAsSpan<float>();
        var pv = outputs[1].GetTensorDataAsSpan<float>();

        // 过滤出超过阈值的索引
        // Filter out indexes that exceed the threshold
        selectCount = FilterIndex(pv, selectIdx, options.probThreshold, options.maxDetections);

        // 根据索引取出对应的检测结果
        // Extract the corresponding detection results according to the index
        FilterPoseData(kpt, pv);
    }

    public override void Dispose()
    {
        base.Dispose();
        poseList.Dispose();
    }

    /// <summary>
    /// 筛选出合适的索引
    /// </summary>
    /// <param name="dataList"></param>
    /// <param name="resultList"></param>
    /// <param name="threshold"></param>
    /// <param name="maxK"></param>
    /// <returns></returns>
    private int FilterIndex(ReadOnlySpan<float> dataList, int[] resultList, float threshold, int maxK)
    {
        // 筛选出超过阈值的索引
        int resultCount = 0;
        var sb = new StringBuilder();
        sb.AppendLine("All probabilities:");
        for (int i = 0; i < dataList.Length; i++)
        {
            sb.Append(i);
            sb.Append(" : ");
            sb.Append(dataList[i]);
            sb.AppendLine();
            if (dataList[i] > threshold)
            {
                resultList[resultCount++] = i;
            }
        }
        Debug.Log(sb.ToString());

        // 保留最大的前k个索引
        // TODO 进行一个排序

        return math.min(maxK, resultCount);
    }

    private void FilterPoseData(in ReadOnlySpan<float> keypoints, in ReadOnlySpan<float> probabilities)
    {
        // 根据索引取出对应的检测结果
        int idx = 0;
        for (int i = 0; i < selectCount; i++)
        {
            var kpts = new NativeArray<Keypoint>(options.keypointCount, Allocator.Persistent);
            for (int j = 0; j < options.keypointCount; j++)
            {
                idx = (options.keypointCount * 3) * i + j * 3;
                var p = keypoints[idx];
                var x = keypoints[idx + 1];
                var y = keypoints[idx + 2];
                kpts[j] = new Keypoint(new Vector2(x, y), p);
            }
            poseList[i] = new Detection(kpts, probabilities[selectIdx[i]]);
        }
    }
}