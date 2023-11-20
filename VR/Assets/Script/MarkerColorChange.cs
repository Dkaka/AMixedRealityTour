using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerColorChange : MonoBehaviour
{
    // To make the marker flash (color change red <-> yellow);
    // The marker assists the local guide in observing the user's position on the map from a bird's-eye view.

    private new Renderer renderer;
    private Color originalColor;
    public float waitTimeRed=1.5f;
    public float waitTimeYellow = 0.5f;

    void Start()
    {
        renderer = transform.GetComponentInChildren<Renderer>();
        originalColor = renderer.material.color;
        StartCoroutine(ChangeColor());
    }

    IEnumerator ChangeColor()
    {
        while (true)
        {
            renderer.material.color = Color.yellow;
            yield return new WaitForSeconds(waitTimeYellow);
            renderer.material.color = originalColor;
            yield return new WaitForSeconds(waitTimeRed);
        }
        
    }
}
