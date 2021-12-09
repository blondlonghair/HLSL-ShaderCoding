using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, ImageEffectAllowedInSceneView]
public class PostProcessing : MonoBehaviour
{
    public Material mat;
    private RenderTexture rt = null;

    private void Start()
    {
        // rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //블러처리
        rt = RenderTexture.GetTemporary(64, 64, 0, src.format);
        
        Graphics.Blit(src, rt);
        Graphics.Blit(rt, dest, mat);
        
        RenderTexture.ReleaseTemporary(rt);
    }
}
