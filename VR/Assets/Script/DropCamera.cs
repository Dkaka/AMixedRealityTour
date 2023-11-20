using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCamera : MonoBehaviour
{
    // Elevate the camera to a bird's-eye view/descend the camera to a local view.

    private GameObject Player;
    public bool isDrop; //false->up; true->drop
    public bool isTriggered; // true -> start camera position change

    private Vector3 upPosition = new Vector3(1000, 10000, -10000);
    private Quaternion upRotation = Quaternion.Euler(50, 0, 0);
    private Vector3 downPosition = new Vector3(0, 0, -2.03f);
    private Quaternion downRotation = Quaternion.identity;
    public float moveSpeed;
    public float rotationSpeed;
    public bool isDoneChange = true; //camera is not in the process of elevating or descending -> true
    public bool isOnGround = true; //camera is at down position-> true

    void Start()
    {
        Player = GameObject.Find("Player");
    }

    void Update()
    {
        ChangeCamera();
    }

    public void BPressed(bool pressed)
    {
        if (UserMode.Instance.userMode) // Camera position can only be change in VR mode
        {
            if (pressed && isDoneChange)
            {
                isTriggered = true;
                isDoneChange = false;
                if (!isDrop)
                {
                    isOnGround = false;
                }
            }
        }
    }

    private void ChangeCamera()
    {
        if (isTriggered && !isDoneChange)
        {
            var currPos = Player.transform.position;
            var currRot = Player.transform.rotation;

            // Check if the camera has reached desired position
            if ((Vector3.Distance(currPos, upPosition) < 0.1 && Quaternion.Angle(currRot, upRotation) < 0.1 && !isDrop)
                || (Vector3.Distance(currPos, downPosition) < 0.1 && Quaternion.Angle(currRot, downRotation) < 0.1 && isDrop)) 
            {
                isDoneChange = true;
                isTriggered = false;
                if (isDrop)
                {
                    Player.transform.position = downPosition;
                    Player.transform.rotation = downRotation;
                    isOnGround = true;
                }
                else
                {
                    Player.transform.position = upPosition;
                    Player.transform.rotation = upRotation;
                }
                isDrop = !isDrop;
                
                return;
            }

            // Change camera position
            if (isDrop)
            {
                Player.transform.position = Vector3.Lerp(currPos, downPosition, Time.deltaTime * moveSpeed);
                Player.transform.rotation = Quaternion.Lerp(currRot, downRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                Player.transform.position = Vector3.Lerp(currPos, upPosition, Time.deltaTime * moveSpeed);
                Player.transform.rotation = Quaternion.Lerp(currRot, upRotation, Time.deltaTime * rotationSpeed);
            }
            
        }
        
    }
}
