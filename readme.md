# QuestBoilerplate - DepthAPI test - Unity 2022.3.14f1 URP - Vulkan - Meta SDK v59


## Build and Run

* Run this in MQDH: `adb shell setprop debug.oculus.experimentalEnabled 1`

* if adb version error: in terminal run `/c/Program\ Files/Unity/Hub/Editor/2022.3.14f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb version` then build and run again


## Meta SDK v59

[Meta SDK Packages](https://developer.oculus.com/documentation/unity/unity-package-manager/#individual-sdks)
[Packages in Asset Store](https://assetstore.unity.com/publishers/25353)

[Project configuration](https://developer.oculus.com/documentation/unity/unity-conf-settings/#configuration-settings)


## Depth API


[Depth API](https://github.com/oculus-samples/Unity-DepthAPI)
[Depth API setup blog](https://blog.learnxr.io/xr-development/quest-3-mixed-reality-with-meta-depth-api-new-occlusion-features)


[Occlusion Shaders](https://github.com/oculus-samples/Unity-DepthAPI#for-urp)
[Custom Occlusion Shaders](https://github.com/oculus-samples/Unity-DepthAPI#8-implementing-occlusion-in-custom-shaders)

## Scene Mesh

[Scene Mesh](https://developer.oculus.com/documentation/unity/unity-scene-mesh/)


## URP


## AppSpaceWarp (not setup but we are using Vulkan)

[AppSpaceWarp Sample project](https://github.com/oculus-samples/Unity-AppSpaceWarp)


## TODO

* couldn't find MSAA settings:

> In the Anti Aliasing list, select 4x. Unlike non-VR apps, VR apps must set the multisample anti-aliasing (MSAA) level appropriately high to compensate for stereo rendering, which reduces the effective horizontal resolution by 50%. You can also let OVRManager automatically select the appropriate multisample anti-aliasing (MSAA) level based on the headset.

> Known Issue: If you are using Universal Render Pipeline (URP), you need to manually set the MSAA level to 4x. We are aware of the issue that URP does not set the MSAA level automatically. Once the fix is published, we will announce it on our Release Notes page.


* Meta XR Simulator - Synthetic Environment Builder package removed. It was throwing shader error missing vulkan imports: 
> Error building Player: Shader error in 'Custom/CopyDepthShader': undeclared identifier 'sampler_LastCameraDepthTexture' at line 66 (on vulkan)

* check out more [oculus-samples Unity projects](https://github.com/oculus-samples?q=unity&type=all&language=&sort=)

## Performance Optimization

https://developer.oculus.com/documentation/unity/unity-perf/?locale=en_US

https://developer.oculus.com/documentation/unity/po-per-frame-gpu/

https://developer.oculus.com/blog/how-to-optimize-your-oculus-quest-app-w-renderdoc-getting-started-frame-capture/

https://developer.oculus.com/blog/how-to-optimize-your-oculus-quest-app-w-renderdoc-quest-hardware-and-software-offerings/

https://developer.oculus.com/blog/pc-rendering-techniques-to-avoid-when-developing-for-mobile-vr/

https://developer.oculus.com/blog/understanding-gameplay-latency-for-oculus-quest-oculus-go-and-gear-vr/

https://developer.oculus.com/blog/common-rendering-mistakes-how-to-find-them-and-how-to-fix-them/

https://developer.oculus.com/documentation/unity/unity-mobile-performance-intro/

https://developer.oculus.com/documentation/unity/ts-ovrstats/

https://developer.oculus.com/documentation/unity/ts-ovrmetricstool/