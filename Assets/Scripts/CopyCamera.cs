using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCamera : MonoBehaviour
{
    public Camera ToCopy;
    public Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = ToCopy.transform.position + Offset;
        transform.rotation = ToCopy.transform.rotation;
    }
}
