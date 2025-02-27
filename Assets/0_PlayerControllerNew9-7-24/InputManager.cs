using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
public class InputManager : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool jumpHold; //
	public bool sprint;
	public bool interact;
	public bool goBack;
	public bool pause;
	public bool one, two, three, four;
	public Vector2 mousePosition;
	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;


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

	/*
	 * Rules 9/7/24
	 * add input system from package manager
	 * create actions on specific action map
	 * Create function called "On" + {nameOfAction} (Ex: action is called Jump, then function is called OnJump
	 * keep track of it with InputValue
	 * 
	 */


	public void OnBack(InputValue value)///
	{
		print("Q");
		goBack = value.isPressed;
	}
	

	public void OnMousePosition(InputValue value)///
	{
		mousePosition = value.Get<Vector2>();
	}

	public void OnF(InputValue value)///
	{
		interact = value.isPressed;
	}

	public void OnMove(InputValue value)
	{
        move = (value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		if (cursorInputForLook)
		{
            look = (value.Get<Vector2>());
		}
	}

	public void OnJump(InputValue value)
	{
		jump = (value.isPressed);
	}
	public void OnJumpHold(InputValue value)///
	{
        //print("JumpHold In OnJumpHold");
        //print(value);
        jumpHold = (value.isPressed);

	}

	public void OnInteract(InputValue value)///
	{
        //print("JumpHold In OnJumpHold");
        //print(value);
        interact = (value.isPressed);


    }
	public void OnSprint(InputValue value)
	{
        sprint = (value.isPressed);
	}
	public void OnPause(InputValue value)
	{
        pause = (value.isPressed);
	}











	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	public void SetCursorState(bool isMouseLocked)
	{
		print($"set cursor state{isMouseLocked}");
        //Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;

        if (isMouseLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }


    public void TurnOffMouse()
    {
		SetCursorState(true);
    }

    public void TurnOnMouse()
    {
        SetCursorState(false);
    }

}
