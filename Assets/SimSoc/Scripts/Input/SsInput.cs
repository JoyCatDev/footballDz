using UnityEngine;
using System.Collections;

/// <summary>
/// Input. Enums and constants.
/// </summary>
public class SsInput : MonoBehaviour {

	// Enums
	//------
	// Input types
	public enum inputTypes
	{
		/// <summary>
		/// Invalid.
		/// </summary>
		invalid = 0,

		/// <summary>
		/// The AI/CPU will control the team.
		/// </summary>
		ai,


		/// <summary>
		/// Use mouse or keyboard.
		/// Default setup: It uses the named buttons and axes, i.e. "Horizontal", "Vertical", ""Fire1", "Fire2", "Mouse_Fire1", "Mouse_Fire2".
		/// </summary>
		mouseOrKeyboard,
										

		/// <summary>
		/// Use mouse.
		/// Default setup: Player runs/kicks towards the mouse cursor position.
		/// </summary>
		mouse,


		/// <summary>
		/// Use keyboard.
		/// Default setup: It uses the named buttons and axes, i.e. "Horizontal", "Vertical", "Fire1", "Fire2".
		/// </summary>
		keyboard,


		/// <summary>
		/// Use any gamepad.
		/// Default setup: It uses the buttons and axes, i.e. "JX_Horizontal", "JX_Vertical", "JX_Fire1", "JX_Fire2".
		/// </summary>
		gamepad,


		/// <summary>
		/// Use accelerometer and touch.
		/// Default setup: Accelerometer for movement, touch buttons for actions.
		/// </summary>
		accelerometer,


		/// <summary>
		/// Default setup: Dpad for movement, touch buttons for actions.
		/// </summary>
		touch,


		/// <summary>
		/// Use gamepad 1. Uses named buttons and axes, i.e. "J1_Horizontal", "J1_Vertical", "J1_Fire1", "J1_Fire2".
		/// </summary>
		gamepad1,


		/// <summary>
		/// Use gamepad 2. Uses named buttons and axes, i.e. "J2_Horizontal", "J2_Vertical", "J2_Fire1", "J2_Fire2".
		/// </summary>
		gamepad2,


		// REMINDER: ADD NEW ONES ABOVE THIS LINE. DO NOT CHANGE THE ORDER.
		
		maxInputTypes,
	}

}
