using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraControls : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public Transform onPlayerCameraPos;
    public float moveSpeed = 5f;

    private InputAction lookAction;
    private InputAction toggleFreFlyAction;
    private InputAction moveAction;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isFlying = false;
    

    void Awake()
    {
        InputSystem_Actions InputSystem_Actions = new InputSystem_Actions();
        lookAction = InputSystem_Actions.FindAction("Look");
        toggleFreFlyAction = InputSystem_Actions.FindAction("ToggleFreeFly");
        moveAction = InputSystem_Actions.FindAction("Move");
        lookAction.Enable();
        toggleFreFlyAction.Enable();

        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;

        toggleFreFlyAction.performed += OnToggleFreFlyActionPerformed;
    }

    void Update()
    {
        if(moveAction.IsInProgress())
            Move(moveAction.ReadValue<Vector2>());
    }

    void OnLookPerformed(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        Look(lookInput);
    }

    void OnLookCanceled(InputAction.CallbackContext context)
    {
        StopLook();
    }

    void Look(Vector2 lookInput)
    {  
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        yRotation += mouseX;
        // yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if(!isFlying)
            playerBody.Rotate(Vector3.up * mouseX);
        else
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void StopLook()
    {
        // Если необходимо, добавьте логику остановки обзора
    }

    void OnToggleFreFlyActionPerformed(InputAction.CallbackContext context)
    {
        isFlying = !isFlying;
        if(isFlying)
            moveAction.Enable();
        else
        {
            moveAction.Disable();
            transform.position = onPlayerCameraPos.transform.position;
            transform.rotation = playerBody.transform.rotation;
        }
    }

    void Move(Vector2 moveInput)
    {
        Vector3 movement = (transform.forward * moveInput.y + 
        transform.right * moveInput.x + 
        transform.up * (moveInput.y * (float)Math.Asin(Math.Clamp(transform.rotation.x,-90f,90f)))) * Time.deltaTime * moveSpeed;
        transform.position += movement;
    }

    void OnDestroy()
    {
        lookAction.performed -= OnLookPerformed;
        lookAction.canceled -= OnLookCanceled;

        lookAction.Disable();
    }
}
