using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTarget : MonoBehaviour
{
    [Header("General Settings")]
    public int dropPriority = 0;
    public string[] allowedDropGroup;
    public Color allowedDropTint = new Color32(1,120,11,255);
    public Color disallowedDropTint = new Color32(90, 0, 0, 255);
    public bool dropAtFixedPoint = false;
    public Vector3 fixedPoint = Vector3.zero;
    // Start is called before the first frame update

}
