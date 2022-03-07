using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public sealed class Pixelation : MonoBehaviour
{
    public Material Mat;
    [Space]
    public uint PixelDesity = 100;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Mat.SetFloat("_AspektRatio", (float)Screen.width / Screen.height);
        Mat.SetInt("_PixelDensity", (int)PixelDesity);

        Graphics.Blit(source, destination, Mat);
    }
}