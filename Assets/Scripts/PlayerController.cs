using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float RotateSpeed = 3f;
    public float MaxRotation = 60f;

    private float bonusSpeed = 0f;
    private float flapSpeed = 0f;

    private Vector2 targetDirection;

    // Update is called once per frame
    void Update()
    {
        if (bonusSpeed > 0f)
            bonusSpeed -= Time.deltaTime;

        if (flapSpeed > 0f)
            flapSpeed -= Time.deltaTime;

        float speed = MoveSpeed + bonusSpeed + flapSpeed;
        transform.position = transform.position + transform.forward * Time.deltaTime * speed;

        if (targetDirection.sqrMagnitude < 0.01f)
            return;

        transform.Rotate(0f, targetDirection.x * RotateSpeed, 0f);
        transform.Rotate(targetDirection.y * RotateSpeed, 0f, 0f);

        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f));
    }

    private void OnJump() {
        flapSpeed = 5f;
    }

    private void OnRightStick(InputValue value) {
        //FreeLookCam.Instance.RightStickValues = value.Get<Vector2>();
    }

    private void OnMove(InputValue context) {
        targetDirection = context.Get<Vector2>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Speed")) {
            bonusSpeed = 10f;
        }
    }
}
