using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime.Unity;
using Unity.Collections;
using UnityEngine;

public class DemoHRNet
{
    private OneTexHrnetPoseDetect liteHrnet;

    public DemoHRNet()
    {
        liteHrnet = new OneTexHrnetPoseDetect(
            ConfigScriptablObject.Instance.hrnet_256_192_Model.bytes,
            ConfigScriptablObject.Instance.hrnetOptions);
    }

    public void Dispose()
    {
        liteHrnet.Dispose();
    }

    public (float[,,], float[]) Run(Texture[] textures)
    {
        // TODO 后需要看看怎么动态配置骨骼的大小
        float[,,] skeletons = new float[textures.Length, 17, 2];
        float[] scores = new float[textures.Length];

        for (int i = 0; i < textures.Length; i++)
        {
            // 推理
            liteHrnet.Run(textures[i]);

            // 读取数据

        }

        return (skeletons, scores);
    }

    public class OneTexHrnetPoseDetect : ImageInference<float>
    {
        [Serializable]
        public class Options : ImageInferenceOptions
        {
            [Header("Lite-HRnet options")]
            public int maxBatch;
        }

        public readonly struct Detection // : IComparable<Detection>
        {
            // public readonly float[] output;
        }

        Options options;

        private NativeArray<Detection> detectionsArray;
        private int detectionCount = 0;
        public ReadOnlySpan<Detection> Detections => detectionsArray.AsReadOnlySpan()[..detectionCount];

        public OneTexHrnetPoseDetect(byte[] model, Options options) : base(model, options)
        {
            this.options = options;

            // detectionsArray = new NativeArray<Detection>(options.maxBatch, Allocator.Persistent);
        }

        protected override void PostProcess()
        {
            var output = outputs[0].GetTensorDataAsSpan<float>();
            for (int i = 0; i < output.Length; i++)
            {
                Debug.Log(output[i]);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            detectionsArray.Dispose();
        }
    }
}
