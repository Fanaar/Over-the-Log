using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [HideInInspector] public float MoveX;
    [HideInInspector] public float MoveZ;
    [HideInInspector] public bool JumpPressed;
    [HideInInspector] public bool SprintHeld;
    [HideInInspector] public bool DivePressed;
    [HideInInspector] public bool InteractPressed;

    public void ReadInput()
    {
        MoveX = Keyboard.current.aKey.isPressed ? -1f : Keyboard.current.dKey.isPressed ? 1f : 0f;
        MoveZ = Keyboard.current.wKey.isPressed ? 1f : Keyboard.current.sKey.isPressed ? -1f : 0f;

        JumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        SprintHeld = Keyboard.current.leftShiftKey.isPressed;
        DivePressed = Mouse.current.leftButton.wasPressedThisFrame;
        InteractPressed = Keyboard.current.eKey.wasPressedThisFrame;
    }
}
