using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public sealed class Vignette : MonoBehaviour
{
    public Material Mat;

    [Space, Range(0f, 1.414f + 0.1f)]
    public float VDistance = 0.1f;
    [Range(0f, 1f)]
    public float VSmooth = 0.5f;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Mat.SetFloat("_VDistance", VDistance);
        Mat.SetFloat("_VSmooth", VSmooth);

        Graphics.Blit(source, destination, Mat);
    }
}
