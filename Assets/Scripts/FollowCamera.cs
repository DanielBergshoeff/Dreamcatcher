﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera Instance;

    public Transform Target;
    public float RotateSpeed = 2f;
    public float MoveSpeed = 2f;
    public float BonusMoveSpeed = 0f;
    public float Distance = 5f;

    private void Awake() {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotating towards Target
        Vector3 lookHeading = Target.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(lookHeading);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, RotateSpeed * Time.deltaTime);

        //Moving towards target
        Vector3 followPosition = Target.position - Target.forward * Distance;
        Vector3 followHeading = followPosition - transform.position;
        float dist = followHeading.magnitude;
        Vector3 followDir = followHeading / dist;

        dist = Mathf.Clamp(dist, 0f, 1f);

        transform.position = transform.position + followDir * Time.deltaTime * (MoveSpeed + BonusMoveSpeed) * dist;

    }
}