using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMode : MonoBehaviour
{
    public bool userMode; // true -> VR(local guide); false -> AR(user)
    public static UserMode Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance);
        }
    }
}
