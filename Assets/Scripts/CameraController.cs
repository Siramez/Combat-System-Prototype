using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Vector3 offset; // Offset distance between player and camera
    public float distance = 5f; // Base distance from player
    public float mouseSensitivity = 2f; // Sensitivity of mouse movement
    public float smoothTime = 0.1f; // Smoothing time for camera movement
    public float minVerticalAngle = -20f; // Minimum vertical angle (to limit looking too low)
    public float maxVerticalAngle = 45f; // Maximum vertical angle (for combat range)
    private float currentYaw = 0f; // Horizontal camera rotation
    private float currentPitch = 0f; // Vertical camera rotation
    private Vector3 currentVelocity = Vector3.zero; // Velocity for smooth damping

    private void Start()
    {
        // Initialize the camera offset based on the initial position
        offset = transform.position - player.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        HandleCameraRotation();
        UpdateCameraPosition();
    }

    // Handle mouse input for camera rotation
    private void HandleCameraRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Adjust the yaw and pitch based on mouse movement
        currentYaw += mouseX;
        currentPitch -= mouseY;

        // Clamp the pitch to prevent the camera from flipping or going too high/low
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
    }

    // Update the camera's position and rotation smoothly
    private void UpdateCameraPosition()
    {
        // Calculate the new camera rotation based on yaw and pitch
        Quaternion cameraRotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Calculate desired position based on the player's position, camera rotation, and offset
        Vector3 desiredPosition = player.position + cameraRotation * offset;

        // Smoothly move the camera towards the desired position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // Rotate the camera to always look at the player
        transform.LookAt(player.position);

        // Adjust camera distance based on player movement speed
        AdjustCameraDistance();
    }

    // Adjust the camera distance based on player movement speed (more dynamic feel)
    private void AdjustCameraDistance()
    {
        float speed = player.GetComponent<CharacterController>().velocity.magnitude;

        // Change camera distance based on speed (you can tweak these values)
        if (speed > 0.1f) // If the player is moving
        {
            distance = Mathf.Lerp(distance, 4f, Time.deltaTime); // Closer distance when moving
        }
        else
        {
            distance = Mathf.Lerp(distance, 5f, Time.deltaTime); // Default distance when idle
        }

        // Update the offset with the adjusted distance
        offset = offset.normalized * distance;
    }
}
