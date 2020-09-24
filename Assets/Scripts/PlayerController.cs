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

    public float BindRotationSpeed = 120f;

    public GameObject TriangleCanvas;

    private float bonusSpeed = 0f;
    private float flapSpeed = 0f;
    private float downwardSpeed = 0f;

    private Vector2 targetDirection;
    private float wingPosition = 0f;
    private Transform body;

    private bool inPillar = false;
    private Pillar currentPillar;
    private bool aroundPillar = false;
    private Vector3 pillarPoint;
    private float currentRot = 0f;
    private float direction = 0f;

    private void Awake() {
        body = transform.GetChild(0);
    }

    private void OnBind() {
        if (!inPillar)
            return;

        aroundPillar = true;
        pillarPoint = new Vector3(currentPillar.transform.position.x, transform.position.y, currentPillar.transform.position.z);
        TriangleCanvas.SetActive(false);
        currentRot = 0f;

        Vector3 dir = pillarPoint - transform.position;
        float angleRight = Vector3.Angle(dir, transform.right);
        float angleLeft = Vector3.Angle(dir, -transform.right);

        direction = angleRight > angleLeft ? -1f : 1f;

        inPillar = false;
    }

    // Update is called once per frame
    void Update() {
        if (aroundPillar) {
            transform.RotateAround(pillarPoint, Vector3.up, BindRotationSpeed * direction * Time.deltaTime);
            currentRot += BindRotationSpeed * Time.deltaTime;
            if (currentRot >= 360f) {
                aroundPillar = false;
                PillarManager.Instance.AddPillarToBind(currentPillar, pillarPoint);
            }
            return;
        }


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
        else if (wingPosition > 0.03f && targetDirection.x <= 0.01f)
            wingPosition -= Time.deltaTime * 3f;
        else if (wingPosition < -0.03f && targetDirection.x >= -0.01f)
            wingPosition += Time.deltaTime * 3f;
        else if(Mathf.Abs(targetDirection.x) < 0.01f){
            wingPosition = 0f;
        }


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

        if (inPillar == true)
            return;

        if (other.CompareTag("Pillar")) {
            inPillar = true;
            TriangleCanvas.SetActive(true);
            currentPillar = other.GetComponent<Pillar>();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (inPillar == false)
            return;

        if (other.CompareTag("Pillar")) {
            inPillar = false;
            TriangleCanvas.SetActive(false);
            currentPillar = null;
        }
    }
}
