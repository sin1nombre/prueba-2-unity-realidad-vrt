using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class GraphicsAPITextController : MonoBehaviour
{
    void Start()
    {
        TextMesh tm = gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
        switch (SystemInfo.graphicsDeviceType)
        {
#if !UNITY_2023_1_OR_NEWER
            case GraphicsDeviceType.OpenGLES2:
                tm.text = "OpenGL ES 2";
                break;
#endif
            case GraphicsDeviceType.OpenGLES3:
                tm.text = "OpenGL ES 3";
                break;
#if UNITY_IOS
            case GraphicsDeviceType.Metal:
                tm.text = "Metal";
                break;

#endif
#if UNITY_ANDROID
            case GraphicsDeviceType.Vulkan:
                tm.text = "Vulkan";
                break;
#endif
            default:
                tm.text = "Unrecognized Graphics API";
                break;
        }
    }
}
