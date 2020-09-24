using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float RotateSpeed = 3f;
    public float MaxRotation = 60f;

    public float DownwardsSpeedBonus = 3f;
    public float MinDownwardsAngle = 80f;

    private float bonusSpeed = 0f;
    private float flapSpeed = 0f;
    private float downwardSpeed = 0f;

    private Vector2 targetDirection;
    private float wingPosition = 0f;
    private Transform body;

    private void Awake() {
        body = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update() {
        if (bonusSpeed > 0f) {
            bonusSpeed -= Time.deltaTime;
            FollowCamera.Instance.BonusMoveSpeed = (bonusSpeed / 5) * 4;
        }

        if (flapSpeed > 0f)
            flapSpeed -= Time.deltaTime;

        if (wingPosition > 1f * targetDirection.x && targetDirection.x < 0f)
            wingPosition += targetDirection.x * Time.deltaTime * 3f;
        else if (wingPosition < 1f  * targetDirection.x && targetDirection.x > 0f)
            wingPosition += targetDirection.x * Time.deltaTime * 3f;
        else if (wingPosition > 0.05f && targetDirection.x <= 0.01f)
            wingPosition -= Time.deltaTime * 3f;
        else if (wingPosition < -0.05f && targetDirection.x >= -0.01f)
            wingPosition += Time.deltaTime * 3f;


        body.rotation = Quaternion.Euler(new Vector3(body.rotation.eulerAngles.x, body.rotation.eulerAngles.y, wingPosition * -60f));

        float angle = Vector3.Angle(-Vector3.up, transform.forward);
        float downward = 1f - Mathf.Clamp(angle, 0f, MinDownwardsAngle) / MinDownwardsAngle;
        bonusSpeed += downward * Time.deltaTime * DownwardsSpeedBonus;

        float speed = MoveSpeed + bonusSpeed + flapSpeed;
        transform.position = transform.position + transform.forward * Time.deltaTime * speed;

        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f));

        if (targetDirection.sqrMagnitude < 0.01f)
            return;

        transform.Rotate(0f, targetDirection.x * RotateSpeed, 0f);
        transform.Rotate(targetDirection.y * RotateSpeed, 0f, 0f);
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
