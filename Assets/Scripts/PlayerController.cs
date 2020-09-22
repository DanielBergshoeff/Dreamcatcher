using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float RotateSpeed = 3f;
    public float MaxRotation = 60f;

    private Vector2 targetDirection;

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + transform.forward * Time.deltaTime * MoveSpeed;

        if (targetDirection.sqrMagnitude < 0.01f)
            return;

        transform.Rotate(0f, targetDirection.x * RotateSpeed, 0f);
        transform.Rotate(targetDirection.y * RotateSpeed, 0f, 0f);


        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f));

        //transform.Rotate(0f, targetDirection.x, 0f);
        /*
        Quaternion targetDir = Quaternion.LookRotation(Camera.main.transform.forward * targetDirection.y + Camera.main.transform.right * targetDirection.x);
        Quaternion rot = Quaternion.Slerp(transform.rotation, targetDir, 0.01f);
        transform.rotation = rot;*/
    }

    public void OnRightStick(InputValue value) {
        //FreeLookCam.Instance.RightStickValues = value.Get<Vector2>();
    }

    private void OnMove(InputValue context) {
        targetDirection = context.Get<Vector2>();
    }
}
