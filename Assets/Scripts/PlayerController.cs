using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction toggleFreFlyAction;
    private bool isFlying = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        InputSystem_Actions InputSystem_Actions = new InputSystem_Actions();
        moveAction = InputSystem_Actions.FindAction("Move");
        jumpAction = InputSystem_Actions.FindAction("Jump");
        toggleFreFlyAction = InputSystem_Actions.FindAction("ToggleFreeFly");

        moveAction.Enable();
        jumpAction.Enable();
        toggleFreFlyAction.Enable();

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;

        toggleFreFlyAction.performed += OnToggleFreFlyActionPerformed;
    }

    void Update()
    {
        if(moveAction.IsInProgress())
            Move(moveAction.ReadValue<Vector2>());
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        Move(moveInput);
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        StopMove();
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        StopJump();
    }

    void Move(Vector2 moveInput)
    {
        Vector3 movement = (rb.transform.forward * moveInput.y + rb.transform.right * moveInput.x) * moveSpeed;
        rb.linearVelocity = new Vector3(movement.x, 0, movement.z);
    }

    void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void StopMove()
    {
        rb.linearVelocity = new Vector3(0, 0, 0);
    }

    void StopJump()
    {
        
    }

     bool IsGrounded()
    {
        return Physics.OverlapSphere(groundCheck.position, 2f, groundLayer).Length > 0;
    }

    void OnToggleFreFlyActionPerformed(InputAction.CallbackContext context)
    {
        isFlying = !isFlying;
        if(isFlying)
            moveAction.Disable();
        else
            moveAction.Enable();
    }

    void OnDestroy()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;

        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        moveAction.Disable();
        jumpAction.Disable();
    }
}
