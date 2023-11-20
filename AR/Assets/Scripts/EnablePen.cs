using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Spawning;

public class EnablePen : MonoBehaviour
{
    public bool isdraw;
    public bool enablePen;
    private GameObject pen;
    private Pen penComponent;
    public GameObject arCamera;
    public GameObject ThreeDPenPrefab;
    private bool istoggled;
    //private GameObject spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        //spawnManager = GameObject.Find("Spawn Manager");

        //penComponent = pen.GetComponent<Pen>();
    }

    // Update is called once per frame
    void Update()
    {
        pen = GameObject.Find("3DPen(Clone)");
        
        if (istoggled)
        {
            penComponent = pen.GetComponent<Pen>();
            penComponent.transform.position = arCamera.transform.position;
            penComponent.transform.rotation = arCamera.transform.rotation;
        }
        //
    }


    

    public void PenToggle()
    {
        var n = NetworkSpawnManager.Find(this).SpawnWithPeerScope(ThreeDPenPrefab);
        n.GetComponent<Pen>().transform.position = arCamera.transform.position;
        istoggled = true;


        //enablePen = !enablePen;
        //if (enablePen)
        //{
        //    pen.SetActive(true);
        //}
        //else
        //{
        //    pen.SetActive(false);
        //}
    }

    public void Stroke()
    {
        isdraw = !isdraw;
        if (isdraw)
        {
            penComponent.owner = true;
            penComponent.BeginDrawing();
        }
        else
        {
            penComponent.owner = false;
            penComponent.EndDrawing();
        }
    }
}
