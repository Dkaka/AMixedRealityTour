using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.UI;

public class PanelSwitch : MonoBehaviour
{
    public GameObject panel;
    private int counter;

    public void ShowHidePanel()
    {
        counter++;
        if (counter % 2 == 1)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    
    
    
    
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
