using UnityEngine;
using GolfGame.Golf.Ball;
using GolfGame.Golf.ShotMechanic;

namespace GolfGame.Golf
{
    /// <summary>
    /// Builds a visible golfer character from Unity primitives and animates
    /// a simple swing cycle driven by ShotController state.
    /// </summary>
    public class GolferVisual : MonoBehaviour
    {
        [SerializeField] private GolfBall golfBall;
        [SerializeField] private ShotController shotController;

        // Body part references (built in Awake)
        private Transform body;
        private Transform head;
        private Transform clubArmPivot;

        // Swing animation state
        private float currentArmAngle = -80f;
        private bool isFrozenDuringFlight;
        private Vector3 frozenPosition;
        private Quaternion frozenRotation;

        // Target angles for each phase
        private const float AddressAngle = -80f;
        private const float BackswingAngle = 60f;
        private const float FollowThroughAngle = -20f;
        private const float CelebrationAngle = 90f;

        private void Awake()
        {
            BuildGolferModel();
            transform.localScale = Vector3.one * 0.6f;
        }

        // -----------------------------------------------------------------
        // Model construction
        // -----------------------------------------------------------------

        private void BuildGolferModel()
        {
            // ---- Body (capsule) ----
            body = CreatePrimitive("Body", PrimitiveType.Capsule, transform);
            body.localPosition = new Vector3(0f, 0.65f, 0f);
            body.localScale = new Vector3(0.4f, 0.6f, 0.25f);
            RemoveCollider(body);
            ApplyColor(body, new Color(0.2f, 0.4f, 0.8f)); // polo-shirt blue

            // ---- Left leg ----
            Transform leftLeg = CreatePrimitive("LeftLeg", PrimitiveType.Cylinder, transform);
            leftLeg.localPosition = new Vector3(-0.08f, 0.22f, 0f);
            leftLeg.localScale = new Vector3(0.12f, 0.22f, 0.12f);
            RemoveCollider(leftLeg);
            ApplyColor(leftLeg, new Color(0.25f, 0.25f, 0.3f)); // dark gray pants

            // ---- Right leg ----
            Transform rightLeg = CreatePrimitive("RightLeg", PrimitiveType.Cylinder, transform);
            rightLeg.localPosition = new Vector3(0.08f, 0.22f, 0f);
            rightLeg.localScale = new Vector3(0.12f, 0.22f, 0.12f);
            RemoveCollider(rightLeg);
            ApplyColor(rightLeg, new Color(0.25f, 0.25f, 0.3f));

            // ---- Head (sphere) ----
            head = CreatePrimitive("Head", PrimitiveType.Sphere, body);
            // Parent-relative: body scale is (0.4, 0.6, 0.25), so we need to
            // counter-scale so the head ends up 0.22 world units across.
            head.localPosition = new Vector3(0f, 0.7f, 0f);
            head.localScale = new Vector3(
                0.22f / 0.4f,
                0.22f / 0.6f,
                0.22f / 0.25f
            );
            RemoveCollider(head);
            ApplyColor(head, new Color(0.9f, 0.75f, 0.6f)); // skin

            // ---- Hat / visor ----
            Transform hat = CreatePrimitive("Hat", PrimitiveType.Cylinder, head);
            // Hat sits on top of head. Head local scale already compensates for
            // body so we compensate again relative to head.
            hat.localPosition = new Vector3(0f, 0.55f, 0.1f);
            hat.localScale = new Vector3(
                0.25f / 0.22f * 0.4f,
                0.04f / 0.22f * 0.6f,
                0.25f / 0.22f * 0.25f
            );
            RemoveCollider(hat);
            ApplyColor(hat, Color.white);

            // ---- Club arm pivot (empty at right shoulder) ----
            GameObject pivotGO = new GameObject("ClubArmPivot");
            clubArmPivot = pivotGO.transform;
            clubArmPivot.SetParent(transform, false);
            // Shoulder height, offset to the right side of body
            clubArmPivot.localPosition = new Vector3(0.22f, 1.1f, 0f);

            // -- Arm --
            Transform arm = CreatePrimitive("Arm", PrimitiveType.Cylinder, clubArmPivot);
            arm.localPosition = new Vector3(0f, -0.17f, 0f);
            arm.localScale = new Vector3(0.06f, 0.35f, 0.06f);
            RemoveCollider(arm);
            ApplyColor(arm, new Color(0.9f, 0.75f, 0.6f)); // skin

            // -- Club shaft --
            Transform shaft = CreatePrimitive("ClubShaft", PrimitiveType.Cylinder, clubArmPivot);
            shaft.localPosition = new Vector3(0f, -0.7f, 0f);
            shaft.localScale = new Vector3(0.02f, 0.5f, 0.02f);
            RemoveCollider(shaft);
            ApplyColor(shaft, new Color(0.7f, 0.7f, 0.7f)); // light gray

            // -- Club head --
            Transform clubHead = CreatePrimitive("ClubHead", PrimitiveType.Cube, clubArmPivot);
            clubHead.localPosition = new Vector3(0f, -1.22f, 0.05f);
            clubHead.localScale = new Vector3(0.08f, 0.02f, 0.15f);
            RemoveCollider(clubHead);
            ApplyColor(clubHead, new Color(0.3f, 0.3f, 0.35f)); // dark gray

            // Set initial address pose
            currentArmAngle = AddressAngle;
            clubArmPivot.localRotation = Quaternion.Euler(currentArmAngle, 0f, 0f);
        }

        // -----------------------------------------------------------------
        // Update loops
        // -----------------------------------------------------------------

        private void Update()
        {
            if (shotController == null) return;

            AnimateSwing();
        }

        private void LateUpdate()
        {
            if (golfBall == null || shotController == null) return;

            UpdatePositionAndRotation();
        }

        // -----------------------------------------------------------------
        // Positioning
        // -----------------------------------------------------------------

        private void UpdatePositionAndRotation()
        {
            ShotState state = shotController.CurrentState;

            bool ballInFlight = state == ShotState.Flying;

            if (ballInFlight)
            {
                // Freeze golfer at the shot position during flight
                if (!isFrozenDuringFlight)
                {
                    frozenPosition = transform.position;
                    frozenRotation = transform.rotation;
                    isFrozenDuringFlight = true;
                }

                transform.position = frozenPosition;
                transform.rotation = frozenRotation;
                return;
            }

            // Ball is not in flight: position golfer near the ball
            isFrozenDuringFlight = false;

            Vector3 aimDir = shotController.AimDirection.normalized;
            if (aimDir.sqrMagnitude < 0.001f)
                aimDir = Vector3.forward;

            Vector3 targetPos = golfBall.transform.position
                                + Vector3.up * 0.1f;

            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(aimDir);
        }

        // -----------------------------------------------------------------
        // Swing animation
        // -----------------------------------------------------------------

        private void AnimateSwing()
        {
            ShotState state = shotController.CurrentState;
            float targetAngle = currentArmAngle;

            switch (state)
            {
                case ShotState.Idle:
                case ShotState.Aiming:
                case ShotState.Landed:
                    targetAngle = AddressAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 5f);
                    break;

                case ShotState.PowerSetting:
                    targetAngle = BackswingAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 3f);
                    break;

                case ShotState.AccuracySetting:
                    // Hold backswing
                    targetAngle = BackswingAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 5f);
                    break;

                case ShotState.Launching:
                    targetAngle = FollowThroughAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 15f);
                    break;

                case ShotState.Flying:
                    // Hold follow-through, turn head to track ball
                    targetAngle = FollowThroughAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 5f);

                    if (head != null && golfBall != null)
                    {
                        head.LookAt(golfBall.transform.position);
                    }
                    break;

                case ShotState.HoleComplete:
                    targetAngle = CelebrationAngle;
                    currentArmAngle = Mathf.Lerp(currentArmAngle, targetAngle, Time.deltaTime * 4f);
                    break;
            }

            if (clubArmPivot != null)
            {
                clubArmPivot.localRotation = Quaternion.Euler(currentArmAngle, 0f, 0f);
            }
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static Transform CreatePrimitive(string name, PrimitiveType type, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void RemoveCollider(Transform t)
        {
            Collider col = t.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
        }

        private static void ApplyColor(Transform t, Color color)
        {
            Renderer rend = t.GetComponent<Renderer>();
            if (rend == null) return;

            Material mat = new Material(GetLitShader());
            mat.color = color;
            rend.material = mat;
        }

        private static Shader GetLitShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null) return shader;

            shader = Shader.Find("Standard");
            return shader;
        }
    }
}
