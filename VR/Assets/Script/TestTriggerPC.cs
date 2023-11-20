using System.Collections;
using System.Collections.Generic;
using Ubiq.Avatars;
using Unity.VisualScripting;
//using Unity.XR.OpenVR;
using UnityEngine;

public class TestTriggerPC : MonoBehaviour
{
    //Testing script for pc controller

    public bool BPressed;
    public bool XPressed;
    public bool YPressed;
    public bool isPC;


    void Update()
    {
        if (isPC)
        {
            var dropCamera = GameObject.Find("Player").GetComponent<DropCamera>();
            //dropCamera.prevPressed = false;
            dropCamera.BPressed(BPressed);

            var selector = GameObject.Find("Selector Panel").GetComponent<OpenSelector>();
            //selector.prevPressed = false;
            selector.XPressed(XPressed);
            
        }

    }
}

