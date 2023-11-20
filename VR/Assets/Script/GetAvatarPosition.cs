using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAvatarPosition : MonoBehaviour
{
    //Get the position of local avatar
    public GameObject Head;
    public Vector3 position;
    void Start()
    {
    }
    void Update()
    {
        position = Head.transform.position;
        
    }




}
