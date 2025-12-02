using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTransform;
    public Transform cameraPivot;
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;

    private float cameraPitch;
    private float cameraYaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks cursor to center
        Cursor.visible = false;                   // Hides cursor
    }

    public void HandleLook(PlayerManager.MovementState state, PlayerInputHandler input)
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity;
        float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -cameraClampAngle, cameraClampAngle);
        playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        if (state == PlayerManager.MovementState.Human)
            playerTransform.Rotate(Vector3.up * mouseX);
        else
        {
            cameraYaw += mouseX;
            cameraPivot.localEulerAngles = new Vector3(0f, cameraYaw, 0f);
        }
    }
}
