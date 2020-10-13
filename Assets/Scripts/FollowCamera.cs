using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class FollowCamera : PivotBasedCameraRig
{
    public static FollowCamera Instance;

    public new Transform Target;
    public float RotateSpeed = 2f;
    public float MoveSpeed = 2f;
    public float BonusMoveSpeed = 0f;
    public float Distance = 5f;
    public float Height = 1f;

    [HideInInspector] public Vector2 RightStickValues;

    private Vector3 offset;
    private Vector2 camDirection;

    protected override void FollowTarget(float deltaTime) {
        //Moving towards target
        Vector3 followPosition = Target.forward * Distance + Target.up * Height;
        followPosition = Quaternion.Euler(camDirection.y, camDirection.x, 0f) * followPosition;
        followPosition = Target.position - followPosition;

        Vector3 followHeading = followPosition - transform.position;
        float dist = followHeading.magnitude;
        Vector3 followDir = followHeading / dist;

        dist = Mathf.Clamp(dist, 0f, 1f);

        transform.position = transform.position + followDir * Time.deltaTime * (MoveSpeed + BonusMoveSpeed) * dist;
    }

    private new void Awake() {
        base.Awake();
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotating towards Target
        Vector3 lookHeading = Target.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(lookHeading);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, RotateSpeed * Time.deltaTime);

        camDirection += RightStickValues;
    }
}
