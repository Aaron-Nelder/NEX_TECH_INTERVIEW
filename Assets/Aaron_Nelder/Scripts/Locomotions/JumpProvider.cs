using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class JumpProvider : LocomotionProvider
{
    public XROriginMovement transformation { get; set; } = new XROriginMovement();

    [Header("Gravity")]
    [SerializeField] AnimationCurve m_gravityCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] float m_gravity = 9.81f;
    [SerializeField] float m_stickToGroundForce = 5.0f;

    [SerializeField] LayerMask m_groundedLayers;
    [SerializeField] float m_characterRadius = 0.3f;

    [Header("Jump")]
    [SerializeField] AnimationCurve m_jumpForceCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] XRInputButtonReader m_jumpButton;
    [SerializeField] float m_jumpForce = 5.0f;

    CharacterController m_CharacterController;
    bool m_AttemptedGetCharacterController = false;
    bool m_IsJumping = false;
    float m_progressionTime = 0.0f;

    void Start()
    {
        FindCharacterController();
        TryPrepareLocomotion();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_jumpButton.inputActionReferenceValue.action.IsPressed() && IsGrounded() && !m_IsJumping)
        {
            m_progressionTime = 0;
            m_IsJumping = true;
        }
        else if (IsGrounded())
        {
            transformation.motion = Vector3.down * m_stickToGroundForce * Time.deltaTime;
        }

        if (m_IsJumping)
        {
            float endTime = m_jumpForceCurve.keys[m_jumpForceCurve.length - 1].time;
            float progress = m_jumpForceCurve.Evaluate(m_progressionTime / endTime);
            transformation.motion = Vector3.up * m_jumpForce * progress * Time.deltaTime;
            m_progressionTime += Time.deltaTime;

            if (m_progressionTime > endTime)
            {
                m_progressionTime = 0;
                m_IsJumping = false;
            }
        }
        if (!IsGrounded() && !m_IsJumping)
        {
            float endTime = m_gravityCurve.keys[m_gravityCurve.length - 1].time;
            float progress = m_gravityCurve.Evaluate(m_progressionTime / endTime);
            transformation.motion = Vector3.down * m_gravity * progress * Time.deltaTime;
            m_progressionTime += Time.deltaTime;
            m_progressionTime = Mathf.Min(m_progressionTime, endTime);
        }

        TryQueueTransformation(transformation);
    }

    bool IsGrounded()
    {
        XROrigin xrOrigin = mediator.xrOrigin;
        if (xrOrigin is null)
            return false;

        bool hasCharacterController = m_CharacterController is not null;

        Vector3 sphereOrigin = xrOrigin.Origin.transform.position;
        sphereOrigin.y = xrOrigin.CameraFloorOffsetObject.transform.position.y;

        float radius = hasCharacterController ? m_CharacterController.radius - 0.025f : m_characterRadius;
        return Physics.OverlapSphereNonAlloc(sphereOrigin, radius, new Collider[1], m_groundedLayers, QueryTriggerInteraction.Ignore) > 0;
    }

    void FindCharacterController()
    {
        var xrOrigin = mediator.xrOrigin?.Origin;
        if (xrOrigin == null)
            return;

        // Save a reference to the optional CharacterController on the rig GameObject
        // that will be used to move instead of modifying the Transform directly.
        if (m_CharacterController == null && !m_AttemptedGetCharacterController)
        {
            // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
            if (!xrOrigin.TryGetComponent(out m_CharacterController) && xrOrigin != mediator.xrOrigin.gameObject)
                mediator.xrOrigin.TryGetComponent(out m_CharacterController);

            m_AttemptedGetCharacterController = true;
        }
    }
}
