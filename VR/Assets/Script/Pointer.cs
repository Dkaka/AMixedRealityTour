using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    // enable and disable the pointer which local guide could point to somewhere with laser emitted from their hands
    private bool isEnabled;
    private LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        isEnabled = lineRenderer.enabled;
    }

    

    public void SetLineRenderer(bool b)
    {
        lineRenderer.enabled = b;
    }

    public bool GetLineRenderer()
    {
        return isEnabled;
    }


}
