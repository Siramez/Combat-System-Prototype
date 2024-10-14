using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    private Animator animator;
    private ThirdPersonPlayerController playerController;
    public ComboList comboList; // Assume this holds an array of ComboData

    private float[] attackCooldowns;
    private bool[] isAbilityOnCooldown; // Track cooldown status of each ability
    private bool isWeaponEquipped = false; // Track if the weapon is equipped
    private bool isAnimating = false; // Track if an ability is currently animating
    private bool isLMBHeld = false; // Track if LMB is being held
    private bool isRMBHeld = false; // Track if RMB is being held
    private string lastAbilityTrigger; // Track the last ability that was triggered

    private PlayerControls inputActions;

    private void Awake()
    {
        inputActions = new PlayerControls();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;
        playerController = GetComponent<ThirdPersonPlayerController>();
        InitializeCooldowns();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Basic Attacks
        inputActions.Combat.LMB.started += ctx => StartLMBAbility();
        inputActions.Combat.LMB.canceled += ctx => StopLMBAbility();

        inputActions.Combat.RMB.started += ctx => StartRMBAbility();
        inputActions.Combat.RMB.canceled += ctx => StopRMBAbility();

        // Other attacks as press
        inputActions.Combat.E.performed += ctx => TryPlayAbility(comboList.combos[2], 2);
        inputActions.Combat.F.performed += ctx => TryPlayAbility(comboList.combos[3], 3);
        inputActions.Combat.Q.performed += ctx => TryPlayAbility(comboList.combos[4], 4);
        inputActions.Combat.R.performed += ctx => TryPlayAbility(comboList.combos[5], 5);

        // Special Attacks
        inputActions.Combat.ShiftQ.performed += ctx => TryPlayAbility(comboList.combos[6], 6);
        inputActions.Combat.ShiftE.performed += ctx => TryPlayAbility(comboList.combos[7], 7);
        inputActions.Combat.ShiftF.performed += ctx => TryPlayAbility(comboList.combos[8], 8);
        inputActions.Combat.ShiftLMB.performed += ctx => TryPlayAbility(comboList.combos[9], 9);
        inputActions.Combat.ShiftRMB.performed += ctx => TryPlayAbility(comboList.combos[10], 10);
        inputActions.Combat.WE.performed += ctx => TryPlayAbility(comboList.combos[11], 11);
        inputActions.Combat.WQ.performed += ctx => TryPlayAbility(comboList.combos[12], 12);
        inputActions.Combat.WF.performed += ctx => TryPlayAbility(comboList.combos[13], 13);
        inputActions.Combat.SE.performed += ctx => TryPlayAbility(comboList.combos[14], 14);
        inputActions.Combat.SQ.performed += ctx => TryPlayAbility(comboList.combos[15], 15);
        inputActions.Combat.SF.performed += ctx => TryPlayAbility(comboList.combos[16], 16);

        // Weapon Toggle
        inputActions.Combat.C.performed += ctx => ToggleWeapon();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        // If the weapon is not equipped, disable all attack inputs
        if (!isWeaponEquipped)
        {
            ResetAttackStates(); // Ensure no inputs are held
            return; // Exit the Update function to prevent any ability play
        }

        // Handle LMB and RMB attacks
        HandleComboInput();
    }

    private void HandleComboInput()
    {
        bool isShiftHeld = Keyboard.current.shiftKey.isPressed;
        bool isWHeld = Keyboard.current.wKey.isPressed;
        bool isSHeld = Keyboard.current.sKey.isPressed;

        // Check for Shift combinations
        if (isShiftHeld)
        {
            if (isLMBHeld)
            {
                TryPlayAbility(comboList.combos[9], 9); // Shift+LMB
                return; // Exit to prevent playing LMB animation
            }

            if (isRMBHeld)
            {
                TryPlayAbility(comboList.combos[10], 10); // Shift+RMB
                return; // Exit to prevent playing RMB animation
            }
        }

        // Check W/S combinations
        if (isWHeld)
        {
            if (isLMBHeld)
            {
                TryPlayAbility(comboList.combos[11], 11); // W+E
                return; // Exit to prevent playing LMB animation
            }

            if (isRMBHeld)
            {
                TryPlayAbility(comboList.combos[12], 12); // W+Q
                return; // Exit to prevent playing RMB animation
            }
        }

        if (isSHeld)
        {
            if (isLMBHeld)
            {
                TryPlayAbility(comboList.combos[14], 14); // S+E
                return; // Exit to prevent playing LMB animation
            }

            if (isRMBHeld)
            {
                TryPlayAbility(comboList.combos[15], 15); // S+Q
                return; // Exit to prevent playing RMB animation
            }
        }

        // Handle abilities for LMB and RMB independently
        if (isLMBHeld)
        {
            TryPlayAbility(comboList.combos[0], 0); // Handle LMB combo
        }

        if (isRMBHeld)
        {
            TryPlayAbility(comboList.combos[1], 1); // Handle RMB combo
        }
    }

    private void ResetAttackStates()
    {
        isLMBHeld = false; // Ensure LMB is not held
        isRMBHeld = false; // Ensure RMB is not held
        animator.SetBool("IsLMBHeld", false);
        animator.SetBool("IsRMBHeld", false);
    }

    private void ToggleWeapon()
    {
        isWeaponEquipped = !isWeaponEquipped; // Toggle the weapon state

        if (isWeaponEquipped)
        {
            animator.SetTrigger("drawWeapon");
        }
        else
        {
            animator.SetTrigger("unequipWeapon");
            playerController.isAttacking = false; // Ensure attacking state is reset
            ResetAttackStates(); // Reset attack inputs when weapon is unequipped
        }
    }

    private void InitializeCooldowns()
    {
        attackCooldowns = new float[comboList.combos.Length];
        isAbilityOnCooldown = new bool[comboList.combos.Length]; // Initialize cooldown tracker
    }

    private void StopLMBAbility()
    {
        isLMBHeld = false;
        animator.SetBool("IsLMBHeld", false); // Update animator parameter
        InterruptCurrentAnimation(); // Stop ability immediately
    }

    private void StopRMBAbility()
    {
        isRMBHeld = false;
        animator.SetBool("IsRMBHeld", false); // Update animator parameter
        InterruptCurrentAnimation(); // Stop ability immediately
    }

    private void StartLMBAbility()
    {
        if (!isWeaponEquipped) return; // Ensure weapon is equipped
        isLMBHeld = true; // Set hold state for LMB
        animator.SetBool("IsLMBHeld", true); // Update animator parameter
    }

    private void StartRMBAbility()
    {
        if (!isWeaponEquipped) return; // Ensure weapon is equipped
        isRMBHeld = true; // Set hold state for RMB
        animator.SetBool("IsRMBHeld", true); // Update animator parameter
    }

    private void TryPlayAbility(ComboData comboData, int index)
    {
        // Check if the weapon is equipped and ability is not on cooldown
        if (!isWeaponEquipped || isAbilityOnCooldown[index]) return;

        // Prevent re-triggering the same animation if it's already playing
        if (IsAbilityAnimating(comboData.animationTriggers[0]))
        {
            // Allow for an early transition to the next attack
            if (index < comboData.animationTriggers.Length - 1)
            {
                // Play the next ability in the combo
                TryPlayAbility(comboData, index + 1);
                return; // Exit to avoid resetting the animation
            }
        }
        else
        {
            // If not animating, reset previous animation triggers
            InterruptCurrentAnimation();
        }

        // Set the attacking state
        playerController.isAttacking = true;

        // Play the animation
        animator.SetTrigger(comboData.animationTriggers[0]);
        attackCooldowns[index] = comboData.attackCooldowns[0];
        isAbilityOnCooldown[index] = true; // Set cooldown state
        lastAbilityTrigger = comboData.animationTriggers[0]; // Store the last trigger

        // Start the cooldown coroutine
        StartCoroutine(CooldownRoutine(index));
    }

    private IEnumerator CooldownRoutine(int index)
    {
        // Wait for the specified cooldown time
        yield return new WaitForSeconds(attackCooldowns[index]);
        isAbilityOnCooldown[index] = false; // Mark ability as available again
    }

    private void InterruptCurrentAnimation()
    {
        animator.ResetTrigger(lastAbilityTrigger); // Reset the last triggered animation
        playerController.isAttacking = false; // Reset the attacking state
    }

    private bool IsAbilityAnimating(string trigger)
    {
        // Check if the animator is currently playing an animation with the specified trigger
        return animator.GetCurrentAnimatorStateInfo(0).IsName(trigger);
    }
}
