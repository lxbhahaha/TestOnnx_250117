# 1 Preparatory work
## 1.1 onnxruntime

Since it cannot be downloaded from NPM, refer to `manifest.json` and unzip [com.github.asus4.onnxruntime.win-x64-gpu-0.2.6](https://github.com/asus4/onnxruntime-unity/releases/tag/v0.2.6) to directory `/Packages/`.

## 1.2 model file
There are two model files (`yolox_nano_320x320.with_runtime_opt.ort`、`hrnet_coco_w32_256x192.with_runtime_opt.ort`) that require additional downloads. After you put them into the unity project, make sure that **YoloxModel** and **hrnet_256_192_Model** in file `Assets/Resources/Config` are set correctly.

By the way, the HRNet model setup is **not necessary**, I just wanted to incorporate the use of HRNet, but it hasn't been fully implemented yet, so it will not have any effect. I'm currently just checking to see if the model loads successfully.

### 1.2.1 yolox-nano
File `yolox_nano_320x320.with_runtime_opt.ort` uses the example provided in [onnxruntime-unity-examples](https://github.com/asus4/onnxruntime-unity-examples).

### 1.2.2 hrnet
File `hrnet_coco_w32_256x192.with_runtime_opt.ort` is the onnx file downloaded in [PINTO_model_zoo](https://github.com/PINTO0309/PINTO_model_zoo/tree/main/271_HRNet) and converted by the instructions in [onnxruntime-unity-examples](https://github.com/asus4/onnxruntime-unity-examples?tab=readme-ov-file#how-to-convert-onnx-to-ort-format).

# 2 Run demo
Play `Assets/Scenes/SampleScene`, if all goes well you should be able to see the camera picture, and the person in the picture will be framed in green.