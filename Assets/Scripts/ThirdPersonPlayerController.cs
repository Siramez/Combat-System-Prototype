using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonPlayerController : MonoBehaviour
{
    // Variables for player movement
    public float walkSpeed = 5f; // Controls the speed when walking
    public float walkBackwardsSpeed = 3f; // Controls the speed when walking backwards
    public float sprintSpeed = 10f; // Controls the speed when sprinting
    public float rotateSpeed = 200f; // Controls how fast the player rotates
    public float gravity = -10f; // Controls how strong gravity affects the player
    public float attackMovementSpeedModifier = 0.5f; // Speed modifier while attacking
    public Transform cameraTransform; // Refers to the camera, used for movement direction

    private CharacterController controller; // Reference to the CharacterController component
    private Vector3 velocity; // Stores the player's movement velocity
    private bool isGrounded; // Checks if the player is on the ground
    public bool isAttacking = false; // Flag to track if the player is attacking
    private float currentSpeed; // Tracks the player's current movement speed

    private PlayerControls inputActions; // Reference to the Input System actions
    private Vector2 movementInput; // Stores the WASD/arrow keys input
    private bool isSprinting; // Tracks if the player is sprinting

    void Awake()
    {
        inputActions = new PlayerControls(); // Initialize input actions
    }

    void OnEnable()
    {
        inputActions.Enable(); // Enable input actions

        // Movement inputs from the Movement action mapping
        inputActions.Movement.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputActions.Movement.Move.canceled += ctx => movementInput = Vector2.zero;

        inputActions.Movement.Sprint.performed += ctx => isSprinting = true;
        inputActions.Movement.Sprint.canceled += ctx => isSprinting = false;
    }

    void OnDisable()
    {
        inputActions.Disable(); // Disable input actions
    }

    void Start()
    {
        controller = GetComponent<CharacterController>(); // Get the CharacterController component from the player
        currentSpeed = walkSpeed; // Set the default speed to walk speed
    }

    void Update()
    {
        isGrounded = controller.isGrounded; // Check if the player is grounded (on the ground)

        // If grounded and falling, reset vertical velocity to keep the player on the ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps the player "sticking" to the ground
        }

        HandleMovement(); // Handle player's movement input
        ApplyGravity(); // Apply gravity
    }

    void HandleMovement()
    {
        // Calculate movement direction based on input (WASD/arrow keys) in 3D world
        Vector3 moveDirection = (cameraTransform.forward * movementInput.y + cameraTransform.right * movementInput.x);

        // Zero out the y-component to prevent vertical movement
        moveDirection.y = 0f;

        // Project the move direction onto the horizontal plane
        moveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up).normalized;

        // Determine speed based on input
        if (movementInput == Vector2.zero)
        {
            currentSpeed = 0f; // No movement
        }
        else if (isSprinting && isGrounded)
        {
            currentSpeed = sprintSpeed; // Sprint speed when holding sprint
        }
        else if (movementInput.y < 0) // Walking backward
        {
            currentSpeed = walkBackwardsSpeed;
        }
        else
        {
            currentSpeed = walkSpeed; // Normal walk speed
        }

        // Apply movement speed modifier if attacking
        if (isAttacking)
        {
            currentSpeed *= attackMovementSpeedModifier; // Slow down movement while attacking
        }

        // Send Speed and Direction parameters to Animator for Blend Tree control
        GetComponent<Animator>().SetFloat("Speed", currentSpeed);

        // Adjust the 'Direction' based on movement input
        if (movementInput.y < 0) // Backward
        {
            GetComponent<Animator>().SetFloat("Direction", -15f);
        }
        else if (movementInput.x != 0) // Strafing
        {
            GetComponent<Animator>().SetFloat("Direction", movementInput.x > 0 ? 5f : -5f); // Right or left
        }
        else // Forward
        {
            GetComponent<Animator>().SetFloat("Direction", 0f);
        }

        // Move the character based on the calculated direction and speed
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Rotate the character to face the camera direction when moving
        if (movementInput.y >= 0) // Forward and strafing
        {
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = targetRotation; // Instant rotation
            }
        }
        else // Backward movement
        {
            // When walking backward, rotate 180 degrees from the camera's direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-moveDirection); // Invert direction
                transform.rotation = targetRotation; // Instant rotation
            }
        }
    }

    void ApplyGravity()
    {
        isGrounded = controller.isGrounded; // Update grounded status

        if (isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f; // Keep the player "sticking" to the ground
            }
        }
        else
        {
            // Apply gravity if in the air
            velocity.y += gravity * Time.deltaTime; // Add gravity over time
        }

        // Move the character controller with the updated velocity
        controller.Move(velocity * Time.deltaTime);
    }
}
