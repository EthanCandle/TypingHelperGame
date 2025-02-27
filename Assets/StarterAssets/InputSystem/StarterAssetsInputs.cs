using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

//namespace StarterAssets

	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool jumpHold; //
		public bool sprint;
		public bool interact;
		public bool goBack;
	public bool one, two, three, four;
		public Vector2 mousePosition;
		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	/*
The way this works,
On the StartAssets Input Actions actions tab, name of the Action becomes a function called "public void On{ActionName}(InputValue value)"
Then below one called "public void {ActionName}Input({var} {nameOfVar})"
then {varFromVeryTop} = {nameOfVar}
then call the input in another script with _input.{veryTopVar} == true;

	Add varaible above to reference

// on Action, use Botton, on Interactions use Press -> Press and Release


	// don't forget to save the asset manager popup
	// can't name inputs after numbers
*/

	public void OnBack(InputValue value)///
	{
		print("Q");
		BackInput(value.isPressed);
	}

	public void BackInput(bool value)
	{
		goBack = value;
	}

	public void OnOne(InputValue value)///
	{
		print("1");
		OneInput(value.isPressed);
	}

	public void OneInput(bool value)
	{
		one = value;
	}
	public void OnTwo(InputValue value)///
	{
		print("2");
		TwoInput(value.isPressed);
	}

	public void TwoInput(bool value)
	{
		two = value;
	}
	public void OnThree(InputValue value)///
	{
		print("3");
		ThreeInput(value.isPressed);
	}

	public void ThreeInput(bool value)
	{
		three = value;
	}
	public void OnFour(InputValue value)///
	{
		print("4");
		FourInput(value.isPressed);
	}
	public void FourInput(bool value)
	{
		four = value;
	}

	public void OnGold(InputValue value)///
	{
		print("Gold");
		GoldInput(value.isPressed);
	}
	public void GoldInput(bool value)
	{
		four = value;
	}


	public void OnMousePosition(InputValue value)///
	{
		MousePositionInput(value.Get<Vector2>());
	}

	public void MousePositionInput(Vector2 mousePos)
	{
		mousePosition = mousePos;
	}

	public void OnF(InputValue value)///
	{
		FInput(value.isPressed);
	}

	public void FInput(bool isF)
	{
		interact = isF;
	}

	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if(cursorInputForLook)
		{
			LookInput(value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}
	public void OnJumpHold(InputValue value)///
	{
		//print("JumpHold In OnJumpHold");
		//print(value);
		JumpHoldInput(value.isPressed);
			
	}

	public void OnInteract(InputValue value)///
	{
		//print("JumpHold In OnJumpHold");
		//print(value);
		InteractInput(value.isPressed);

	}
	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}




#endif


	public void MoveInput(Vector2 newMoveDirection)
		{
		//print("2");
		move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}
		public void JumpHoldInput(bool newJumpState) ///
		{
			jumpHold = newJumpState;
		}

		public void InteractInput(bool newJumpState) ///
		{
			print("Interacted");
			interact = newJumpState;
		}
		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}


	private void OnApplicationFocus(bool hasFocus)
	{
		//SetCursorState(cursorLocked);
	}

	public void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
}
	
