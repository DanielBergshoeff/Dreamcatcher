using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float RotateSpeed = 3f;
    public float MaxRotation = 60f;
    public float BindRotationSpeed = 120f;
    public float BoostSpeed = 5f;

    [Header("Misc")]
    public float DownwardsSpeedBonus = 3f;
    public float MinDownwardsAngle = 80f;
    public GameObject TriangleCanvas;

    [Header("Collision aversion")]
    public float CheckDistance = 5f;
    public LayerMask CollisionLayer;
    public float AversionMultiplier = 3f;
    public float SphereSize = 0.7f;

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

    private float aversionStrength = 0f;
    private float aversionDirectionRight = 0f;
    private float aversionDirectionUp = 0f;

    private bool rotateAroundSelf = false;

    private void Awake() {
        Instance = this;
        body = transform.GetChild(0);
    }

    private void OnLeftShoulder() {
        if (aroundPillar)
            return;

        if (!rotateAroundSelf) {
            direction = -1;
            currentRot = 0f;
            rotateAroundSelf = true;
        }
        else {
            rotateAroundSelf = false;
        }
    }

    private void OnRightShoulder() {
        if (aroundPillar)
            return;

        if (!rotateAroundSelf) {
            direction = 1;
            currentRot = 0f;
            rotateAroundSelf = true;
        }
        else {
            rotateAroundSelf = false;
        }
    }

    public void OnR2() {
        FreeLookCam.Instance.OnR2();
    }

    public void OnL2() {
        FreeLookCam.Instance.OnL2();
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

        FreeLookCam.Instance.RotatingAroundPillar = true;
    }

    // Update is called once per frame
    void Update() {
        if (aroundPillar) {
            RotateAroundPillar();
            return;
        }

        ApplyMovement();

        if (rotateAroundSelf) {
            RotateAroundSelf();
            return;
        }

        CollisionAversion();
        ApplyRotation();
    }

    private void RotateAroundSelf() {
        transform.RotateAround(transform.position + transform.right * direction, Vector3.up, 1f * direction);
        currentRot += 1f;
    }

    private void CollisionAversion() {
        //Forward check
        RaycastHit hit;
        if (!Physics.SphereCast(transform.position, SphereSize, transform.forward, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
            aversionStrength = 0f;
            return;
        }

        float up = 0f;
        float right = 0f;

        if (Physics.SphereCast(transform.position, SphereSize, new Vector3(transform.forward.x, 0f, transform.forward.z), out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
            right = 1f;
        }
        else {
            up = 1f;
        }

        aversionStrength = Mathf.Clamp(1f - hit.distance / CheckDistance, 0.5f, 1f);
        aversionDirectionRight = 0f;
        aversionDirectionUp = 0f;
        float avgStrengthLeft = 0f;
        float avgStrengthRight = 0f;
        float avgStrengtDown = 0f;
        float avgStrengthUp = 0f;

        if (right > 0f) {
            for (float i = 0.1f; i <= 1f; i += 0.1f) {
                if (Physics.SphereCast(transform.position, SphereSize, transform.forward * (1f - i) - transform.right * i, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
                    avgStrengthLeft += hit.distance;
                }
                else {
                    aversionDirectionRight = -1f;
                    break;
                }

                if (Physics.SphereCast(transform.position, SphereSize, transform.forward * (1f - i) + transform.right * i, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
                    avgStrengthRight += hit.distance;
                }
                else {
                    aversionDirectionRight = 1f;
                    break;
                }
            }

            if (aversionDirectionRight == 0f) {
                if (avgStrengthLeft >= avgStrengthRight) {
                    aversionDirectionRight = -1f;
                }
                else {
                    aversionDirectionRight = 1f;
                }
            }
        }

        if (up > 0f) {
            for (float i = 0.1f; i <= 1f; i += 0.1f) {
                if (Physics.SphereCast(transform.position, SphereSize, transform.forward * (1f - i) - transform.up * i, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
                    avgStrengtDown += hit.distance;
                }
                else {
                    aversionDirectionUp = -1f;
                    break;
                }

                if (Physics.SphereCast(transform.position, SphereSize, transform.forward * (1f - i) + transform.up * i, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
                    avgStrengthUp += hit.distance;
                }
                else {
                    aversionDirectionUp = 1f;
                    break;
                }
            }

            if (aversionDirectionUp == 0f) {
                if (avgStrengtDown >= avgStrengthUp) {
                    aversionDirectionUp = -1f;
                }
                else {
                    aversionDirectionUp = 1f;
                }
            }
        }
    }

    private void ApplyRotation() {
        if (wingPosition > 1f * targetDirection.x && targetDirection.x < 0f)
            wingPosition += targetDirection.x * Time.deltaTime * 3f;
        else if (wingPosition < 1f * targetDirection.x && targetDirection.x > 0f)
            wingPosition += targetDirection.x * Time.deltaTime * 3f;
        else if (wingPosition > 0.03f && targetDirection.x <= 0.01f)
            wingPosition -= Time.deltaTime * 3f;
        else if (wingPosition < -0.03f && targetDirection.x >= -0.01f)
            wingPosition += Time.deltaTime * 3f;
        else if (Mathf.Abs(targetDirection.x) < 0.01f) {
            wingPosition = 0f;
        }

        body.rotation = Quaternion.Euler(new Vector3(body.rotation.eulerAngles.x, body.rotation.eulerAngles.y, wingPosition * -60f));
        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f));

        if (targetDirection.sqrMagnitude < 0.01f && aversionStrength < 0.01f)
            return;

        float rotationHorizontal = targetDirection.x * (1f - aversionStrength) + aversionDirectionRight * aversionStrength * AversionMultiplier;
        float rotationVertical = targetDirection.y * (1f - aversionStrength) - aversionDirectionUp * aversionStrength * AversionMultiplier;
        transform.Rotate(0f, rotationHorizontal * RotateSpeed, 0f);
        transform.Rotate(rotationVertical * RotateSpeed, 0f, 0f);
    }

    private void ApplyMovement() {
        if (bonusSpeed > 0f) {
            bonusSpeed -= Time.deltaTime;
            FreeLookCam.Instance.BonusMoveSpeed = (bonusSpeed / 5) * 4;
        }

        if (flapSpeed > 0f)
            flapSpeed -= Time.deltaTime;

        float angle = Vector3.Angle(-Vector3.up, transform.forward);
        float downward = 1f - Mathf.Clamp(angle, 0f, MinDownwardsAngle) / MinDownwardsAngle;
        bonusSpeed += downward * Time.deltaTime * DownwardsSpeedBonus;

        float speed = MoveSpeed + bonusSpeed + flapSpeed;
        transform.position = transform.position + transform.forward * Time.deltaTime * speed;
    }

    private void RotateAroundPillar() {
        transform.RotateAround(pillarPoint, Vector3.up, BindRotationSpeed * direction * Time.deltaTime);
        currentRot += BindRotationSpeed * Time.deltaTime;
        if (currentRot >= 360f) {
            aroundPillar = false;
            PillarManager.Instance.AddPillarToBind(currentPillar, pillarPoint);
            FreeLookCam.Instance.RotatingAroundPillar = false;
        }
    }

    private void OnJump() {
        flapSpeed = BoostSpeed;
    }

    private void OnRightStick(InputValue value) {
        FreeLookCam.Instance.RightStickValues = value.Get<Vector2>();
    }

    private void OnMove(InputValue context) {
        targetDirection = context.Get<Vector2>();
    }

    private void GoThroughPortal() {
        transform.position = PillarManager.Instance.PortalCam.transform.position;
        PillarManager.Instance.SwapPortals();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Speed")) {
            bonusSpeed = 10f;
        }

        if (other.CompareTag("Portal")) {
            GoThroughPortal();
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
