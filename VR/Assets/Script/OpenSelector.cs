using System.Collections;
using System.Collections.Generic;
using Ubiq.Avatars;
using Ubiq.Spawning;
using Ubiq.XR;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine;

public class OpenSelector : MonoBehaviour
{
    // To open/close the selector for the local guide to bring additional objects or 3d pen into the scene

    public GameObject selector;
    public GameObject ImageArchPrefab;
    public GameObject ImageLayoutPrefab;
    public GameObject VideoPrefab;
    public GameObject ThreeDPenPrefab;
    public Hand controller;
    private bool toFalse = true; // For desktop mode, force to only response after pressed turned false and back to true
                                 // Because on the controller, "pressed" changes only at the moment the button is pressed,
                                 // whereas on desktop, "pressed" needs to be manually unselected, so it doesn't occur instantly.


    void Update()
    {
        var currActive = selector.activeSelf;
        if (currActive) // if selector is opened, then let it follow the location of the hand
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
        
    }


    public void XPressed(bool pressed)
    {
        if (UserMode.Instance.userMode) // Selector can only be opened in VR mode
        {
            if (pressed  && toFalse)
            {
                var isActive = selector.activeSelf;
                selector.SetActive(!isActive); // close or open the selector
                toFalse = false;               
            }
            else if (!pressed)
            {
                toFalse = true;
            }
        }
    }

    //Bring images into the scene and send the location of the image through network
    public void Images()
    {
        var n = NetworkSpawnManager.Find(this).SpawnWithPeerScope(ImageArchPrefab);
        //var nt = NetworkSpawnManager.Find(this).SpawnWithPeerScope(ImageLayoutPrefab);
        n.GetComponent<NetworkGraspableObject>().transform.position = controller.transform.position;
        //nt.GetComponent<NetworkGraspableObject>().transform.position = controller.transform.position;
    }

    public void Pigeon()
    {
        var nt = NetworkSpawnManager.Find(this).SpawnWithPeerScope(ImageLayoutPrefab);
        nt.GetComponent<NetworkGraspableObject>().transform.position = controller.transform.position;
    }

    public void Videos()
    {
        var n = NetworkSpawnManager.Find(this).SpawnWithPeerScope(VideoPrefab);
        n.GetComponent<NetworkGraspableObject>().transform.position = controller.transform.position;
    }

    public void ThreeDPen()
    {
        var n = NetworkSpawnManager.Find(this).SpawnWithPeerScope(ThreeDPenPrefab);
        n.GetComponent<Pen>().transform.position = controller.transform.position;

    }


}
