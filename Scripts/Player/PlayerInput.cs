using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;
    [HideInInspector] public bool attackPressed;
    [HideInInspector] public bool reloadPressed;
    [HideInInspector] public bool interactPressed;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool sprintPressed;
    [HideInInspector] public bool crouchPressed;
    [HideInInspector] public bool escapePressed;
    [HideInInspector] public bool zoomPressed;
    [HideInInspector] public bool slot1;
    [HideInInspector] public bool slot2;
    [HideInInspector] public bool slot3;
    [HideInInspector] public bool dropPressed;
    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 moveInput;
    private PlayerSettings playerSettings;
    public bool canGiveInput;
    void OnEnable()
    {
        inputActions.Player.Enable();
    }
    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    void Start()
    {
        playerSettings = FindFirstObjectByType<PlayerSettings>();
    }
    void Update()
    {
        if(canGiveInput)
        {
        mouseX = inputActions.Player.Look.ReadValue<Vector2>().x * Time.deltaTime * playerSettings.sensitivity;
        mouseY = inputActions.Player.Look.ReadValue<Vector2>().y * Time.deltaTime * playerSettings.sensitivity;

        if(Time.timeScale != 0) attackPressed = inputActions.Player.Attack.WasPressedThisFrame();
        reloadPressed = inputActions.Player.Reload.WasPressedThisFrame();
        interactPressed = inputActions.Player.Interact.WasPressedThisFrame();
        jumpPressed = inputActions.Player.Jump.WasPressedThisFrame();
        sprintPressed = inputActions.Player.Sprint.IsPressed();
        crouchPressed = inputActions.Player.Crouch.IsPressed();
        escapePressed = inputActions.Player.Escape.WasPressedThisFrame();
        zoomPressed = inputActions.Player.Zoom.IsPressed();
        slot1 = inputActions.Player.Slot1.WasPressedThisFrame();
        slot2 = inputActions.Player.Slot2.WasPressedThisFrame();
        slot3 = inputActions.Player.Slot3.WasPressedThisFrame();
        dropPressed = inputActions.Player.Drop.WasPressedThisFrame();
        
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        }
    }
}
