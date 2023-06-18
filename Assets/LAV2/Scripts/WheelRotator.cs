using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public float wheelControllerRotation = 0.0f;
    public float maxDegree = 505f;
    void Update()
    {
        Vector3 currentEuler = transform.localRotation.eulerAngles;
        Quaternion rotationGoal = Quaternion.Euler(currentEuler.x, currentEuler.y, -(wheelControllerRotation / 36768.0f * maxDegree));
        transform.localRotation = rotationGoal;
    }
}
