using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTextureResponder : MonoBehaviour
{
    public ARCameraBackground m_ARCameraBackground;
    public RenderTexture targetRenderTexture;

    // Update is called once per frame
    void Update()
    {
        Graphics.Blit(null, targetRenderTexture, m_ARCameraBackground.material);
    }
}