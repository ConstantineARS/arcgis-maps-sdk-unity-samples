using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCharcterController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigidBody;
    private IA_ThirdPersonCharacter thirdPersonControls;
    private InputAction move;

    [Header ("Movement Variables")]
    [SerializeField] private float MoveForce;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float upSpeed;
    private bool flying;
    private bool sprinting;

    private Vector3 forceDirection = Vector3.zero;
    [SerializeField] private Camera playerCamera;

    private Vector2 movementInput;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        thirdPersonControls = new IA_ThirdPersonCharacter();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        thirdPersonControls.ThirdPersonCharacter.Flying.started += StartFlying;
        thirdPersonControls.ThirdPersonCharacter.Jump.started += Jump;
        thirdPersonControls.ThirdPersonCharacter.Sprint.started += Sprint;
        move = thirdPersonControls.ThirdPersonCharacter.Move;
        thirdPersonControls.Enable();
    }

    private void OnDisable()
    {
        thirdPersonControls.ThirdPersonCharacter.Flying.started -= StartFlying;
        thirdPersonControls.ThirdPersonCharacter.Jump.started -= Jump;
        thirdPersonControls.ThirdPersonCharacter.Sprint.started -= Sprint;
        thirdPersonControls.Disable();
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (IsGrounded() && !flying)
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        sprinting = !sprinting;
        animator.SetBool("Sprinting", sprinting);

        if (sprinting)
        {
            maxSpeed = 100f;
        }
        else
        {
            maxSpeed = 25f;
        }
    }

    private void Update()
    {
        animator.SetFloat("Speed", rigidBody.velocity.magnitude / maxSpeed);

        if (thirdPersonControls.ThirdPersonCharacter.Jump.inProgress && flying)
        {
            rigidBody.MovePosition(rigidBody.position + transform.up * Time.deltaTime * upSpeed);
        }

        if (thirdPersonControls.ThirdPersonCharacter.WalkGoDown.inProgress && flying)
        {
            rigidBody.MovePosition(rigidBody.position + transform.up * Time.deltaTime * -upSpeed);
        }
    }

    private void FixedUpdate()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * MoveForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * MoveForce;
        rigidBody.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rigidBody.velocity.y < 0f) 
        {
            rigidBody.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }

        var horizontalVelocity = rigidBody.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rigidBody.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rigidBody.velocity.y;
        }

        LookAt();
    }

    private void LookAt()
    {
        var direction = rigidBody.velocity;
        direction.y = 0;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            rigidBody.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rigidBody.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        var forward = playerCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        var right = playerCamera.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    private bool IsGrounded()
    {
        var ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void StartFlying(InputAction.CallbackContext context)
    {
        flying = !flying;
        animator.SetBool("Flying", flying);
    }
}
