using UnityEngine;

public class AccelerationCalculator : MonoBehaviour
{
    private Vector3 previousVelocity;
    public float acceleration { get; private set; }
    public new Rigidbody rigidbody;
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;

    private void Start()
    {
        // Initialize previous velocity
        previousVelocity = Vector3.zero;
    }

    private void Update()
    {
        // Calculate acceleration
        acceleration = CalculateAcceleration();
        
        // Update previous velocity
        previousVelocity = rigidbody.velocity;
    }

    private float CalculateAcceleration()
    {
        Vector3 currentVelocity = rigidbody.velocity;

        // Calculate the change in velocity
        Vector3 velocityChange = currentVelocity - previousVelocity;

        // Calculate acceleration using the magnitude of the velocity change
        float acceleration = velocityChange.magnitude / Time.deltaTime;

        return acceleration;
    }
    
    public int CalculateLateralForce()
    {
        float steerAngle = (frontLeftWheel.steerAngle + frontRightWheel.steerAngle) * 0.5f;
        steerAngle*=frontLeftWheel.sidewaysFriction.stiffness;
        steerAngle *= Mathf.Lerp(1f, 20f, frontLeftWheel.attachedRigidbody.velocity.magnitude);
        return (int)steerAngle;
    }
}