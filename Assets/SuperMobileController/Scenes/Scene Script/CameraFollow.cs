using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("General Settings")]
    public float followTime = 0.2f;
    public Transform target;
    public bool alwaysAtBack = false;
    public float angularMovingSpeed = 1;
    [Header("Camera Offset Position Settings")]
    public float offsetY = 0;
    public float offsetZ = 0;
    [Header("Look At Offset Settings")]
    public Vector3 targetOffset = Vector3.zero;

    public float angularDisplacement = 0;

    private float maxOffsetY = 0;
    // Start is called before the first frame update
    void Start()
    {
        maxOffsetY = offsetY * 1.3f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Calculate new position
        Vector3 targetPosition = (target.position + targetOffset);
        if (alwaysAtBack)
        {
            transform.position = targetPosition + (target.forward.normalized * -offsetZ) + new Vector3(0, offsetY, 0);
        } else
        {
            var newPosition = (Quaternion.AngleAxis(angularDisplacement, Vector3.up) * new Vector3(0, offsetY, -offsetZ)) + (targetPosition);
            transform.position = newPosition;
        }
        transform.forward = targetPosition - transform.position;
    }

    public void MoveAngle(string skillName, Vector2 input, float speed)
    {
        //Move camera angle
        if (Mathf.Abs(input.x) < Mathf.Abs(input.y))
        {
            input.x = 0;
        } else
        {
            input.y = 0;
        }

        offsetY += input.y / 25;
        if (offsetY < 0)
        {
            offsetY = 0;
        }
        else if (offsetY > maxOffsetY)
        {
            offsetY = maxOffsetY;
        }
           
        angularDisplacement = Mathf.Lerp(angularDisplacement, (angularDisplacement + (input.x * angularMovingSpeed)), 0.5f);
    }
}
