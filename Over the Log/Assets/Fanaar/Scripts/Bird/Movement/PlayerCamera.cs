using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTransform;   // for Human rotation
    public Transform cameraPivot;       // pivot for Bird rotation
    public Transform playerCamera;      // actual camera
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;

    [Header("Bird Camera Limits")]
    public float maxYawOffset = 45f;    // max yaw offset for Bird mode

    private float cameraPitch;
    private float cameraYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HandleLook(PlayerManager.MovementState state, PlayerInputHandler input)
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
        float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

        // --- PITCH ---
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -cameraClampAngle, cameraClampAngle);
        playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        if (state == PlayerManager.MovementState.Human)
        {
            // Human: rotate the whole player
            playerTransform.Rotate(Vector3.up * mouseX);
        }
        else
        {
            // Bird: rotate pivot but clamp yaw
            cameraYaw += mouseX;
            cameraYaw = Mathf.Clamp(cameraYaw, -maxYawOffset, maxYawOffset);
            cameraPivot.localEulerAngles = new Vector3(0f, cameraYaw, 0f);
        }
    }
}
