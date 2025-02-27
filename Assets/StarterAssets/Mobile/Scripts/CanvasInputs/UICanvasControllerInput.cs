using UnityEngine;

//namespace StarterAssets

    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

    // I dont think this does anything
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
        print("3");
        starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }


        public void VirtualJumpHoldInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpHoldInput(virtualJumpState);
        }
    public void VirtualInteractInput(bool virtualJumpState)
    {
        starterAssetsInputs.InteractInput(virtualJumpState);
    }

    public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }
        
    }


