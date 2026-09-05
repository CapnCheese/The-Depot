using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private float tilt;
    private float offset;
    public Transform orientation;
    public PlayerMovement player;
    [HideInInspector] public float xRotation;
    [HideInInspector] public float yRotation;
    private float xSpring;
    private float zSpring;
    private float xSpringVelocity;
    private float zSpringVelocity;
    private float fovVelocity;
    public Camera mainCamera;
    public Camera overlayCamera;
    public PlayerInput playerInput;
    private float zoomAmount;
    private float zoomMultiplier;
    public Transform cameraPos;
    void FixedUpdate()
    {
    }
    void Update()
    {
        transform.position = cameraPos.position;
        
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            yRotation += playerInput.mouseX * zoomMultiplier;
            xRotation -= playerInput.mouseY * zoomMultiplier;
        }
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        transform.rotation = Quaternion.Euler(xSpring + xRotation, yRotation,zSpring);

        xSpring = Mathf.SmoothDampAngle(xSpring, player.rb.linearVelocity.y, ref xSpringVelocity, 0.2f);
        zSpring = Mathf.SmoothDampAngle(zSpring, -player.localVel.x * 0.5f, ref zSpringVelocity, 0.2f);

        zoomAmount = playerInput.zoomPressed ? 50 : 0;
        zoomMultiplier = playerInput.zoomPressed ? 0.25f : 1;
        
        mainCamera.fieldOfView = Mathf.SmoothDamp(mainCamera.fieldOfView, 70 + player.rb.linearVelocity.magnitude - zoomAmount, ref fovVelocity, 0.1f);
        overlayCamera.fieldOfView = Mathf.SmoothDamp(overlayCamera.fieldOfView, 70 + player.rb.linearVelocity.magnitude - zoomAmount, ref fovVelocity, 0.1f);
    }
}