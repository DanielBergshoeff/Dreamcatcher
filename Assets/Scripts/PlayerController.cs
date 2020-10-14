using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float BodyRotateSpeed = 0.5f;
    public float RotateSpeed = 3f;
    public float MaxRotation = 60f;
    public float MaxRotationVertical = 40f;
    public float MaxRotationVerticalBody = 40f;
    public float ReturnToNeutralBoost = 2.0f;
    public float BoostSpeed = 5f;
    public float ReduceSpeedMultiplier = 5f;
    public float BoostCooldown = 3f;
    public float BounceBack = 1f;

    [Header("Misc")]
    public float DownwardsSpeedBonus = 3f;
    public float MinDownwardsAngle = 80f;
    public GameObject TriangleCanvas;

    [Header("Collision aversion")]
    public float CheckDistance = 5f;
    public LayerMask CollisionLayer;
    public float AversionMultiplier = 3f;
    public float SphereSize = 0.7f;

    [Header("Pillars")]
    public float BindRotationSpeed = 120f;
    public bool BindRotation = false;

    private float bonusSpeed = 0f;
    private float flapSpeed = 0f;
    private float pathBonusSpeed = 0f;

    private Vector2 targetDirection;
    private float wingPosition = 0f;
    private float wingPositionVertical = 0f;
    private Transform body;

    private bool inPillar = false;
    private Pillar currentPillar;
    private bool aroundPillar = false;
    private Vector3 pillarPoint;
    private float currentRot = 0f;
    private float direction = 0f;

    private float aversionStrength = 0f;
    private float aversionStrengthVertical = 0f;
    private float aversionDirectionRight = 0f;
    private float aversionDirectionUp = 0f;

    private bool rotateAroundSelf = false;

    public float PathBonusSpeed = 10f;

    private BezierSpline PathSpline;
    private bool inPath = false;
    private Transform pathParent;
    private float pathCurrentPosition = 0f;
    private int pathDir = 1;
    private float pathCoolDown = 0f;
    private float boostCooldown = 0f;

    private PlayerInput playerInput;

    private AudioSource myAudioSource;

    private void Awake() {
        Instance = this;
        body = transform.GetChild(0);
        myAudioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.FindAction("LeftShoulder").started += LeftShoulder;
        playerInput.actions.FindAction("RightShoulder").started += RightShoulder;
        playerInput.actions.FindAction("LeftShoulder").canceled += LeftShoulder;
        playerInput.actions.FindAction("RightShoulder").canceled += RightShoulder;

        playerInput.actions.FindAction("Bind").started += Bind;

    }

    private void LeftShoulder(InputAction.CallbackContext context) {
        if(rotateAroundSelf && context.started) //If started, but there's already rotation
            return;

        if (context.canceled && !rotateAroundSelf) //If stopped, but there's no rotation going
            return;

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

    private void RightShoulder(InputAction.CallbackContext context) {
        if (rotateAroundSelf && context.started) //If started, but there's already rotation
            return;

        if (context.canceled && !rotateAroundSelf) //If stopped, but there's no rotation going
            return;

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

    private void Bind(InputAction.CallbackContext context) {
        if (!inPillar)
            return;

        pillarPoint = new Vector3(currentPillar.transform.position.x, transform.position.y, currentPillar.transform.position.z);

        if (!BindRotation) {
            switch(PillarManager.Instance.AddPillarToBind(currentPillar, pillarPoint)) {
                case PillarBoundType.Bound:
                    myAudioSource.PlayOneShot(AudioManager.Instance.ConnectPillar);
                    TriangleCanvas.SetActive(false);
                    inPillar = false;
                    break;
                case PillarBoundType.Last:
                    myAudioSource.PlayOneShot(AudioManager.Instance.CompleteForm);
                    break;
                case PillarBoundType.Portal:
                    myAudioSource.PlayOneShot(AudioManager.Instance.CompleteForm);
                    myAudioSource.PlayOneShot(AudioManager.Instance.CompleteArea);
                    TriangleCanvas.SetActive(false);
                    inPillar = false;
                    break;
            }
            return;
        }

        aroundPillar = true;

        currentRot = 0f;
        Vector3 dir = pillarPoint - transform.position;
        float angleRight = Vector3.Angle(dir, transform.right);
        float angleLeft = Vector3.Angle(dir, -transform.right);

        direction = angleRight > angleLeft ? -1f : 1f;
        FreeLookCam.Instance.RotatingAroundPillar = true;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (pathCoolDown > 0f)
            pathCoolDown -= Time.deltaTime;

        if (boostCooldown > 0f)
            boostCooldown -= Time.deltaTime;

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
            aversionStrengthVertical = 0f;
            return;
        }

        float up = 0f;
        float right = 0f;

        if (Physics.SphereCast(transform.position, SphereSize, new Vector3(transform.forward.x, 0f, transform.forward.z), out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
            right = 1f;
            aversionStrength = Mathf.Clamp(1f - hit.distance / CheckDistance, 0, 1f);
        }
        else {
            up = 1f;
            aversionStrengthVertical = Mathf.Clamp(1f - hit.distance / CheckDistance, 0, 1f);
        }

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
                    Debug.DrawRay(transform.position, (transform.forward * (1f - i) - transform.right * i) * CheckDistance, Color.yellow);
                    break;
                }

                if (Physics.SphereCast(transform.position, SphereSize, transform.forward * (1f - i) + transform.right * i, out hit, CheckDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
                    avgStrengthRight += hit.distance;
                }
                else {
                    aversionDirectionRight = 1f;
                    Debug.DrawRay(transform.position, (transform.forward * (1f - i) + transform.right * i) * CheckDistance, Color.yellow);
                    break;
                }
            }

            if (aversionDirectionRight == 0f) {
                if (avgStrengthLeft >= avgStrengthRight) {
                    Debug.DrawRay(transform.position, (transform.forward - transform.right) * CheckDistance, Color.red);
                    aversionDirectionRight = -1f;
                }
                else {
                    Debug.DrawRay(transform.position, (transform.forward + transform.right) * CheckDistance, Color.red);
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
        float rotation = 0f;
        float vertical = 0f;
        bool boost = false;
        bool verticalBoost = false;

        float rotPlayer = 0f;
        float rotAversion = 0f;
        float rotPlayerVertical = 0f;
        float rotAversionVertical = 0f;

        //HORIZONTAL ROTATION
        if ((wingPosition > 1f * targetDirection.x && targetDirection.x < 0f)
            || (wingPosition < 1f * targetDirection.x && targetDirection.x > 0f)) {
            rotPlayer = targetDirection.x * (1f - aversionStrength);
        }
        else if (wingPosition < 0f && wingPosition < -0.05f) {
            rotPlayer = BounceBack * (1f - aversionStrength);
        }
        else if (wingPosition > 0f && wingPosition > 0.05f) {
            rotPlayer = -BounceBack * (1f - aversionStrength);
        }

        rotAversion = aversionDirectionRight * aversionStrength;
        
        float totalRot = rotPlayer + rotAversion;

        if ((totalRot > 0f && wingPosition < 0f) || (totalRot < 0f && wingPosition > 0f))
            boost = true;
        
        if((totalRot > 0f && wingPosition < 1f) || (totalRot < 0f && wingPosition > -1f))
            rotation = totalRot * Time.deltaTime * (boost ? ReturnToNeutralBoost : BodyRotateSpeed);

        //VERTICAL ROTATION
        if ((wingPositionVertical > 1f * targetDirection.y && targetDirection.y < 0f)
            || (wingPositionVertical < 1f * targetDirection.y && targetDirection.y > 0f)) {
            rotPlayerVertical = targetDirection.y * (1f - aversionStrengthVertical);
        }
        else if (wingPositionVertical < 0f && wingPositionVertical < -0.05f) {
            rotPlayerVertical = BounceBack * (1f - aversionStrengthVertical);
        }
        else if (wingPositionVertical > 0f && wingPositionVertical > 0.05f) {
            rotPlayerVertical = -BounceBack * (1f - aversionStrengthVertical);
        }

        rotAversionVertical = aversionDirectionUp * aversionStrengthVertical;
        //Debug.Log("Vertical: " + rotAversionVertical);
        float totalRotVertical = rotPlayerVertical - rotAversionVertical;

        if ((totalRotVertical > 0f && wingPositionVertical < 0f) || (totalRotVertical < 0f && wingPositionVertical > 0f))
            verticalBoost = true;

        if ((totalRotVertical > 0f && wingPositionVertical < 1f) || (totalRotVertical < 0f && wingPositionVertical > -1f))
            vertical = totalRotVertical * Time.deltaTime * (verticalBoost ? ReturnToNeutralBoost : BodyRotateSpeed);

        wingPosition += rotation;
        wingPositionVertical += vertical;

        body.localRotation = Quaternion.identity * Quaternion.Euler(new Vector3(wingPositionVertical * MaxRotationVerticalBody, 0f, wingPosition * -MaxRotation));
        transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f));
        float speed = MoveSpeed + bonusSpeed + pathBonusSpeed + flapSpeed;

        if (!inPath) {
            float f = wingPosition * RotateSpeed * (speed / 8f);
            float f2 = f * aversionStrength * AversionMultiplier;
            transform.Rotate(0f, f + f2, 0f);
            //Debug.Log("Horizontal position: " + wingPosition);
            float f3 = wingPositionVertical * RotateSpeed * (speed / 8f);
            float f4 = f3 * aversionStrengthVertical * AversionMultiplier;
            float total = transform.eulerAngles.x + f3 + f4;
            if((Mathf.Abs(total) < MaxRotationVertical && Mathf.Abs(total) < 180f) || (Mathf.Abs(total) > 360f - MaxRotationVertical &&  Mathf.Abs(total) > 180f))
                transform.Rotate(f3 + f4, 0f, 0f);
            //Debug.Log("Vertical position: " + wingPositionVertical);
        }
        else {
            float percentage = (pathCurrentPosition + 1f * pathDir) / PathSpline.TotalDistance;
            pathBonusSpeed = percentage * PathBonusSpeed;
            Vector3 targetDir = PathSpline.GetPoint(percentage) - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * 3f, 0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    private void ApplyMovement() {
        if (bonusSpeed > 0f) 
            bonusSpeed -= bonusSpeed * 0.01f * Time.deltaTime * ReduceSpeedMultiplier;
        

        if (pathBonusSpeed > 0f)
            pathBonusSpeed -= pathBonusSpeed * 0.01f * Time.deltaTime * ReduceSpeedMultiplier;

        FreeLookCam.Instance.BonusMoveSpeed = ((bonusSpeed + pathBonusSpeed) / 5) * 4;

        if (flapSpeed > 0f)
            flapSpeed -= flapSpeed * 0.01f * Time.deltaTime * ReduceSpeedMultiplier;

        float angle = Vector3.Angle(-Vector3.up, transform.forward);
        float downward = 1f - Mathf.Clamp(angle, 0f, MinDownwardsAngle) / MinDownwardsAngle;
        bonusSpeed += downward * Time.deltaTime * DownwardsSpeedBonus;

        float speed = MoveSpeed + bonusSpeed + pathBonusSpeed + flapSpeed;

        if (inPath) { 
            pathCurrentPosition += speed * Time.deltaTime * pathDir;
            if(pathCurrentPosition > PathSpline.TotalDistance || pathCurrentPosition < 0f) {
                inPath = false;
                pathCoolDown = 1f;
            }
        }

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

    private void OnBoost() {
        if (boostCooldown > 0f)
            return;

        boostCooldown = BoostCooldown;
        flapSpeed += BoostSpeed;
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
            bonusSpeed += BoostSpeed;
        }

        if (other.CompareTag("Portal")) {
            GoThroughPortal();
        }

        if (other.CompareTag("Path")) {
            if (!inPath && pathCoolDown <= 0f) {
                pathParent = other.transform.parent;
                PathSpline = pathParent.GetComponent<BezierSpline>();

                if (pathParent.GetChild(0) == other.transform) {
                    pathCurrentPosition = 0f;
                    pathDir = 1;
                }
                else {
                    pathCurrentPosition = PathSpline.TotalDistance;
                    pathDir = -1;
                }

                bonusSpeed += PathBonusSpeed;
                inPath = true;
            }
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
