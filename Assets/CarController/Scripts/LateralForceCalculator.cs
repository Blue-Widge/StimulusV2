using UnityEngine;

public class LateralForceCalculator : MonoBehaviour
{
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    
    public int CalculateLateralForce()
    {
        float steerAngle = (frontLeftWheel.steerAngle + frontRightWheel.steerAngle) * 0.5f;
        steerAngle*=frontLeftWheel.sidewaysFriction.stiffness;
        int lateralForce = (int) (steerAngle * Mathf.Lerp(1f, 20f, frontLeftWheel.attachedRigidbody.velocity.magnitude));
        return lateralForce;
    }
}