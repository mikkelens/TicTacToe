using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BGAnimator : MonoBehaviour
{
    private Material BGM;

    public Vector2 Direction;
    public float Speed;

    private void Awake()
    {
        BGM = this.GetComponent<MeshRenderer>().material;
    }

    public void OnRenderObject()
    {
        BGM.SetTextureOffset("_MainTex", Direction * Speed * Time.timeSinceLevelLoad);
    }
}
