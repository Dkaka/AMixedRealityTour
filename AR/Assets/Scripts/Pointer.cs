using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    
    //private Ubiq.Avatars.Avatar avatar;

    public bool isEnabled;
    private LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        //avatar = GetComponentInParent<Ubiq.Avatars.Avatar>();
    }

    // Update is called once per frame
    void Update()
    {
        isEnabled = lineRenderer.enabled;
    }

    

    public void SetLineRenderer(bool b)
    {
        //if (!avatar.IsLocal)
        //{
            lineRenderer.enabled = b;
        //}
    }

    public bool GetLineRenderer()
    {
        //if (!avatar.IsLocal)
        //{
        return isEnabled;
        //}
    }


}
