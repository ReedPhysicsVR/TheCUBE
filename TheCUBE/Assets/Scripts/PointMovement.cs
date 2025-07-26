using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the motion information for the point charge.
/// </summary>
public class PointMovement : MonoBehaviour
{
    private Rigidbody rb;
    private PresetControl preset;
    public Vector3 position { get; private set; }
    public Vector3 velocity { get; private set; }
    public Vector3 acceleration { get; private set; }
    private Vector3 prevVel;
    public Vector3 origin;
    public InputActionReference retrievePoint;

    void Awake()
    {
        // Adds the RetrievePoint function to the controller input action profile
        if (!retrievePoint.action.enabled)
        {
            retrievePoint.action.Enable();
        }
        retrievePoint.action.performed += RetrievePoint;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        preset = GetComponent<PresetControl>();
        GetComponent<Transform>().localPosition = origin;
    }

    // When preset movement is enabled, pull the motion information from PresetControl
    // Otherwise, pull position/velocity from RigidBody and approximate the acceleration.
    void Update()
    {
        position = rb.position;
        velocity = preset.enabled ? preset.velocity : rb.velocity;

        if (preset.enabled) acceleration = preset.accel;
        else if (velocity != Vector3.zero && velocity != prevVel) acceleration = (velocity - prevVel) / Time.deltaTime;
        else acceleration = Vector3.zero;

        if (velocity != prevVel) prevVel = velocity;
    }

    void OnDestroy()
    {
        retrievePoint.action.Disable();
        retrievePoint.action.performed -= RetrievePoint;
    }

    /// <summary>
    /// Function for controller button. Returns the point charge to its origin position.
    /// </summary>
    /// <param name="context"></param>
    public void RetrievePoint(InputAction.CallbackContext context)
    {
        RetrievePoint();
    }

    /// <summary>
    /// Function for UI button. Returns the point charge to its origin position.
    /// </summary>
    public void RetrievePoint()
    {
        if (!preset.enabled)
        {
            rb.velocity = Vector3.zero;
            velocity = Vector3.zero;
            Transform trans = GetComponent<Transform>();
            trans.localPosition = origin;
            position = trans.position;
        }
    }

}
