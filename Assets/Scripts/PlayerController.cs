using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float RotateSpeed = 3f;

    private Vector2 targetDirection;

    // Update is called once per frame
    void Update()
    {
        if (targetDirection.sqrMagnitude < 0.01f)
            return;

        transform.position = transform.position + (Camera.main.transform.forward * targetDirection.y + Camera.main.transform.right * targetDirection.x) * Time.deltaTime * MoveSpeed;

        float f = transform.rotation.eulerAngles.z + targetDirection.x;
        if (f >= 360)
            f -= 360;
        else if (f <= -360)
            f += 360;
        Debug.Log(f);
        if (f < 40f && f > -40f)
            transform.Rotate(0f, 0f, targetDirection.x);
        /*
        Quaternion targetDir = Quaternion.LookRotation(Camera.main.transform.forward * targetDirection.y + Camera.main.transform.right * targetDirection.x);
        Quaternion rot = Quaternion.Slerp(transform.rotation, targetDir, 0.01f);
        transform.rotation = rot;*/
    }

    public void OnRightStick(InputValue value) {
        FreeLookCam.Instance.RightStickValues = value.Get<Vector2>();
    }

    private void OnMove(InputValue context) {
        targetDirection = context.Get<Vector2>();
    }
}
