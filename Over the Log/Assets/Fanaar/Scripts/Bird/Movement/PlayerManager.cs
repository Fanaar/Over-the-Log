using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public enum MovementState { Human, Bird }
    public MovementState currentState = MovementState.Human;

    public PlayerInputHandler input;
    public HumanMovement humanMovement;
    public BirdMovement birdMovement;
    public PlayerCamera cameraController;
    public DiveHandler diveHandler;

    void Update()
    {
        input.ReadInput();
        cameraController.HandleLook(currentState, input);

        if (currentState == MovementState.Human)
        {
            humanMovement.HandleMovement(input);
            humanMovement.HandleJumpFlap(input);

            if (humanMovement.flightUnlocked && input.SprintHeld)
                currentState = MovementState.Bird;
        }
        else if (currentState == MovementState.Bird)
        {
            birdMovement.HandleMovement(input);
            diveHandler.HandleDive(input); // <-- call dive logic here
        }
    }
}
