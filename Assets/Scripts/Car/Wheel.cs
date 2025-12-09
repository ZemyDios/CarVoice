using System;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    Rigidbody carRigidBody;

    [Header("Suspension")]
    [SerializeField] float restDistance;
    [SerializeField] float springStrength;
    [SerializeField] float springDamper;

    [Header("Steering")]
    [SerializeField] bool steerable;
    [SerializeField] float gripFactor; // 0 -> No grip | 1 -> full grip
    [SerializeField] float lateralGripStrength;
    float direction;

    [Header("Acceleration")]
    [SerializeField] bool powered;
    [SerializeField] float carTopSpeed;
    [SerializeField] AnimationCurve powerCurve;
    float accelerationInput;
    [SerializeField] float maxTorque = 1500f;   // Motor torque
    [SerializeField] float pedalResponsiveness = 3f; // How fast engine reacts to input changes
    [SerializeField] float engineBrake = 0.3f;

    private float engineInput = 0f;  // Smoothed input

    private void Start()
    {
        carRigidBody = transform.parent.gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -transform.up);
        bool isTouchingGround = Physics.Raycast(ray, out hit, restDistance);

        if (isTouchingGround)
        {
            Suspension(hit);
            Steering(hit);
            if (powered) Acceleration();
        }

        Debug.DrawRay(transform.position, -transform.up * restDistance, isTouchingGround ? Color.green : Color.red);
    }

    private void Suspension(RaycastHit hit)
    {
        // World-space direction of the spring force.
        Vector3 springDir = transform.up;

        // World-space velocity of the wheel.
        Vector3 wheelWorldVelocity = carRigidBody.GetPointVelocity(transform.position);

        // Calculate offset form the raycast.
        float offset = restDistance - hit.distance;

        // Calculate velocity along the spring direction.
        float velocity = Vector3.Dot(springDir, wheelWorldVelocity);

        // Calculate the magnitude of the dampened spring force.
        float force = (offset * springStrength) - (velocity * springDamper);

        // Apply the force at the location of the wheel.
        carRigidBody.AddForceAtPosition(springDir * force, transform.position, ForceMode.Acceleration);
    }

    private void Steering(RaycastHit hit)
    {
        // World-space direction of the spring force.
        Vector3 steeringDir = transform.right;

        // World-space velocity of the wheel.
        Vector3 wheelWorldVelocity = carRigidBody.GetPointVelocity(transform.position);

        // Calculate velocity in the steering direction.
        float steeringVelocity = Vector3.Dot(steeringDir, wheelWorldVelocity);

        // The change in velocity.
        float desiredVelocityChange = -steeringVelocity * gripFactor;

        // Turn change in velocity into acceleration.
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        // Apply the force at the location of the wheel.
        carRigidBody.AddForceAtPosition(steeringDir * lateralGripStrength * desiredAcceleration, transform.position, ForceMode.Acceleration);
    }

    private void Acceleration()
    {
        Vector3 accelerationDir = transform.forward;

        // Smooth engine response
        engineInput = Mathf.MoveTowards(
            engineInput,
            accelerationInput,
            pedalResponsiveness * Time.fixedDeltaTime
        );

        float carSpeed = Vector3.Dot(transform.parent.transform.forward, carRigidBody.linearVelocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);

        float torqueFactor = powerCurve.Evaluate(normalizedSpeed);

        float wheelTorque = maxTorque * torqueFactor * engineInput;

        // Apply torque
        carRigidBody.AddForceAtPosition(accelerationDir * wheelTorque, transform.position, ForceMode.Force);

        // Engine braking (no input)
        if (Mathf.Abs(accelerationInput) < 0.1f)
        {
            Vector3 brakeForce = -carRigidBody.linearVelocity * engineBrake;
            carRigidBody.AddForce(brakeForce, ForceMode.Acceleration);
        }
    }


    public void SetAcceleration(float acceleration)
    {
        accelerationInput = acceleration;
    }

    public bool GetSterable()
    {
        return steerable;
    }

}
