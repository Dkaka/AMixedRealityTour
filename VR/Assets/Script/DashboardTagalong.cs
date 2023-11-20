// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class DashboardTagalong : MonoBehaviour
{

    public float maxAngle = 20.0f;
    public float lerpTime = 0.1f;

    Vector3 originalPosition;
    Quaternion originalRotation;

    Quaternion currentRotation;
    GameObject quad;

    void Start()
    {
        var fg = GameObject.Find("FollowCamera");
        originalPosition = fg.transform.localPosition;
        originalRotation = fg.transform.localRotation;
        currentRotation = Quaternion.identity;
        
    }


    void Update()
    {
        if (quad == null)
        {
            quad = GameObject.Find("Quad");
        }
        Vector3 camPosition = Camera.main.transform.position; // GazeManager.Instance.Stabilizer.StablePosition;
        Quaternion camRotation = Camera.main.transform.rotation; // GazeManager.Instance.Stabilizer.StableRotation;

        float cameraAngle = camRotation.eulerAngles.y;
        float currentAngle = currentRotation.eulerAngles.y;

        float diffAngle = currentAngle - cameraAngle;
        while (diffAngle > 180) diffAngle -= 360;
        while (diffAngle < -180) diffAngle += 360;

        diffAngle = Math.Min(diffAngle, maxAngle);
        diffAngle = Math.Max(diffAngle, -maxAngle);

        float targetAngle = cameraAngle + diffAngle;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.unscaledDeltaTime / lerpTime);

        if (quad != null)
        {
            //Debug.Log(camPosition + currentRotation * originalPosition);
            quad.transform.position = camPosition + currentRotation * originalPosition;
            quad.transform.rotation = currentRotation * originalRotation;
        }
        
    }
}
