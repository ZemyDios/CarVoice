using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private Wheel[] wheels;

    [Header("Steering Behavior")]
    [SerializeField] private float steeringSmoothSpeed = 5f; // smooth wheel turning

    private AudioSource audioSource;

    private float accelerationInput; // -1 to 1
    private float steeringInput;     // -1 to 1

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Subscribe to VoiceCommands events
        if (VoiceCommands.Instance != null)
            VoiceCommands.Instance.OnCommandDetected += OnVoiceCommand;
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (VoiceCommands.Instance != null)
            VoiceCommands.Instance.OnCommandDetected -= OnVoiceCommand;
    }

    /// <summary>
    /// Handles voice commands from VoiceCommands system.
    /// Applies acceleration and steering based on intensity.
    /// </summary>
    private void OnVoiceCommand(VoiceCommandType command, float intensity)
    {
        switch (command)
        {
            case VoiceCommandType.Accelerate:
                accelerationInput += intensity;
                break;

            case VoiceCommandType.Brake:
                accelerationInput -= intensity;
                break;

            case VoiceCommandType.Left:
                steeringInput -= intensity * 0.5f; // Make steering slower for more precision
                break;

            case VoiceCommandType.Right:
                steeringInput += intensity * 0.5f; // Make steering slower for more precision
                break;

            case VoiceCommandType.Straight:
                steeringInput = 0f;
                break;

            case VoiceCommandType.Stop:
                accelerationInput = 0f;
                break;
        }

        // Clamp to allowed range
        accelerationInput = Mathf.Clamp(accelerationInput, -1f, 1f);
        steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);

        audioSource.Play();
    }

    private void FixedUpdate()
    {
        // Target steering angle (-45 to 45)
        float targetAngle = steeringInput * 45f;

        foreach (Wheel wheel in wheels)
        {
            // Acceleration
            wheel.SetAcceleration(accelerationInput);

            // Steering (only wheels that can steer)
            if (wheel.GetSterable())
            {
                float currentY = wheel.transform.localEulerAngles.y;

                // Smooth interpolation for steering
                float newY = Mathf.LerpAngle(currentY, targetAngle, Time.fixedDeltaTime * steeringSmoothSpeed);

                Vector3 rot = wheel.transform.localEulerAngles;
                rot.y = newY;
                wheel.transform.localEulerAngles = rot;
            }
        }
    }
}
