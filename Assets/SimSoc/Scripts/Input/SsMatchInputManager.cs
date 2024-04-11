using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Match input manager. Reads input from various hardware (e.g. keyboard, mouse, gamepad, accelerometer, touch, etc.).
/// You can override the relevant methods to add your own input (e.g. UpdateKeyboard, UpdateGamepad, etc.).
/// </summary>
public class SsMatchInputManager : MonoBehaviour {

	// Classes
	//--------
	// Input for a single user.
	[System.Serializable]
	public class SsUserInput
	{
		public SsInput.inputTypes type;			// Input type

		[System.NonSerialized]
		public Vector2 axes;					// Axes

		[System.NonSerialized]
		public Vector2 lookAxes;				// Look axes. The same as axes, but mainly used for rotating players (in case axes is set to zero elsewhere).

		[System.NonSerialized]
		public Vector3 mousePos;				// Mouse world position, usually on the ground (if mouse input is used)

		[System.NonSerialized]
		public bool button1;					// Is button 1 down?

		[System.NonSerialized]
		public bool button2;					// Is button 2 down?

		[System.NonSerialized]
		public bool button3;					// Is button 3 down?

		[System.NonSerialized]
		public bool wasPressedButton1;			// Was button 1 pressed down this frame?

		[System.NonSerialized]
		public bool wasPressedButton2;			// Was button 2 pressed down this frame?

		[System.NonSerialized]
		public bool wasPressedButton3;			// Was button 2 pressed down this frame?


		[System.NonSerialized]
		public SsTeam team;						// Team controlled by the user

		[System.NonSerialized]
		public SsPlayer player;					// Player controlled by the user

		[System.NonSerialized]
		public bool sprintActive;				// Is sprint active?

		[System.NonSerialized]
		public float wasPressedTimeButton1;		// Time.time when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public float wasPressedTimeButton2;		// Time.time when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public SsPlayer wasPressedPlayerButton1;	// Player that was active when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public SsPlayer wasPressedPlayerButton2;	// Player that was active when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public SsPlayer wasPressedPassPlayerButton1;	// Passed to player that was active when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.
		
		[System.NonSerialized]
		public SsPlayer wasPressedPassPlayerButton2;	// Passed to player that was active when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public Vector2 wasPressedAxesButton1;		// Axes when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.
		
		[System.NonSerialized]
		public Vector2 wasPressedAxesButton2;		// Axes when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.


		[System.NonSerialized]
		public Vector2 wasPressedPassAxesButton1;		// Axes from passed to player to the mouse, when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.

		[System.NonSerialized]
		public Vector2 wasPressedPassAxesButton2;		// Axes from passed to player to the mouse, when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.



		/// <summary>
		/// Clean up references and memory.
		/// </summary>
		/// <returns>The up.</returns>
		public void CleanUp()
		{
			ResetMe();
		}


		/// <summary>
		/// Reset the input.
		/// </summary>
		public void ResetMe()
		{
			ClearInput();

			wasPressedTimeButton1 = 0.0f;
			wasPressedTimeButton2 = 0.0f;
			wasPressedPlayerButton1 = null;
			wasPressedPlayerButton2 = null;
			wasPressedPassPlayerButton1 = null;
			wasPressedPassPlayerButton2 = null;
			wasPressedAxesButton1 = Vector2.zero;
			wasPressedAxesButton2 = Vector2.zero;
			wasPressedPassAxesButton1 = Vector2.zero;
			wasPressedPassAxesButton2 = Vector2.zero;

			if (player != null)
			{
				if (player.IsUserControlled)
				{
					SetControlPlayer(null, player.UserControlIndex);
				}
				player.UserControlIndex = -1;
			}
			player = null;
		}


		/// <summary>
		/// Clears the input. Must be called at the start of each update.
		/// </summary>
		/// <returns>The input.</returns>
		public void ClearInput()
		{
			axes = Vector2.zero;
			lookAxes = Vector2.zero;
			mousePos = Vector3.zero;
			button1 = false;
			button2 = false;
			button3 = false;
			wasPressedButton1 = false;
			wasPressedButton2 = false;
			wasPressedButton3 = false;
			sprintActive = false;
		}
	}



	// Public
	//-------
	[Header("Mobile Input")]
	[Tooltip("Enable multi-touch on mobile devices when the match starts. It will restore it when the scene ends.")]
	public bool multiTouchEnabled = true;

	[Tooltip("Use touches to simulate mouse events on mobile devices, during the match.\n" + 
	         "Note: Usually this is disabled, so it does not interfere with the dpad. But you can enable it if you use a dpad other than " + 
	         "the default one.")]
	public bool simulateMouseWithTouches = false;

	[Tooltip("Keep this small, ideally not more than 0.1.")]
	public float accelerometerDeadzone = 0.0f;


	[Header("Desktop Input")]
	[Tooltip("Pass ball to player near the mouse position, within this radius. (Only used if \"pass to mouse\" is true in the global settings.)")]
	public float passToMousePlayerRadius = 2.0f;
	[Tooltip("Delays between changing the player to pass to near the mouse position. (Only used if \"pass to mouse\" is true in the global settings.)")]
	public float passToMousePlayerUpdateDelay = 0.5f;


	[Header("Named Buttons and Axes")]
	public string keyboardHorizontalAxes = "Horizontal";
	public string keyboardVerticalAxes = "Vertical";
	public string keyboardFire1 = "Fire1";
	public string keyboardFire2 = "Fire2";
	public string keyboardFire3 = "Fire3";
	[Space(10)]
	public string mouseFire1 = "Mouse_Fire1";
	public string mouseFire2 = "Mouse_Fire2";
	public string mouseFire3 = "Mouse_Fire3";
	[Space(10)]
	public string anyJoystickHorizontalAxes = "JX_Horizontal";
	public string anyJoystickVerticalAxes = "JX_Vertical";
	public string anyJoystickFire1 = "JX_Fire1";
	public string anyJoystickFire2 = "JX_Fire2";
	public string anyJoystickFire3 = "JX_Fire3";
	[Space(10)]
	public string joystick1HorizontalAxes = "J1_Horizontal";
	public string joystick1VerticalAxes = "J1_Vertical";
	public string joystick1Fire1 = "J1_Fire1";
	public string joystick1Fire2 = "J1_Fire2";
	public string joystick1Fire3 = "J1_Fire3";
	[Space(10)]
	public string joystick2HorizontalAxes = "J2_Horizontal";
	public string joystick2VerticalAxes = "J2_Vertical";
	public string joystick2Fire1 = "J2_Fire1";
	public string joystick2Fire2 = "J2_Fire2";
	public string joystick2Fire3 = "J2_Fire3";


	[Header("Sprint")]
	[Tooltip("For how long can player sprint (seconds)?")]
	public float sprintTimeMax = 2.0f;
	[Tooltip("Delay before refilling the sprint (seconds).")]
	public float sprintRefillDelayMax = 5.0f;
	[Tooltip("Speed at which to refill the sprint.")]
	public float sprintRefillSpeed = 10.0f;



	// Private
	//--------
	static private SsMatchInputManager instance;

	private bool restoreMultiTouch;
	private bool restoreSimulateMouse;
	private List<SsUserInput> userInput = new List<SsUserInput>();	// Input for each user. Must have at least 1 user to be able to play the game.


	// Screen touch buttons for mobile input. Assume there will only be 3 buttons, controlled by 1 user.
	private bool[] touchButton = new bool[3];
	private bool[] touchButtonWasPressed = new bool[3];
	private DynamicDpad dpad;		// Active dpad on mobile. Assume only 1 dpad will be controlled by 1 user.



	// Properties
	//-----------
	static public SsMatchInputManager Instance
	{
		get { return(instance); }
	}


	public List<SsUserInput> UserInput
	{
		get { return(userInput); }
	}


	/// <summary>
	/// The active DynamicDpad.
	/// </summary>
	/// <value>The dpad.</value>
	public DynamicDpad Dpad
	{
		get { return(dpad); }
		set
		{
			dpad = value;
		}
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void Awake()
	{
		instance = this;

		restoreMultiTouch = Input.multiTouchEnabled;
		Input.multiTouchEnabled = multiTouchEnabled;

		restoreSimulateMouse = Input.simulateMouseWithTouches;
		Input.simulateMouseWithTouches = simulateMouseWithTouches;

		FunkyCalibrate.SetDeadZone(new Vector3(accelerometerDeadzone, accelerometerDeadzone, accelerometerDeadzone));
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void OnDestroy()
	{
		instance = null;

		CleanUp();

		Input.multiTouchEnabled = restoreMultiTouch;
		Input.simulateMouseWithTouches = restoreSimulateMouse;
	}


	/// <summary>
	/// Use this for initialization.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void Start()
	{
	}


	/// <summary>
	/// Clean up references and memory.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	/// <returns>The up.</returns>
	public virtual void CleanUp()
	{
		int i;
		SsUserInput ui;

		if ((userInput != null) && (userInput.Count > 0))
		{
			for (i = 0; i < userInput.Count; i++)
			{
				ui = userInput[i];
				if (ui == null)
				{
					continue;
				}
				ui.CleanUp();
			}
			userInput.Clear();
		}
		userInput = null;
	}


	/// <summary>
	/// Adds the user input to the list.
	/// </summary>
	/// <returns>The user input.</returns>
	/// <param name="index">Index.</param>
	/// <param name="inputType">Input type.</param>
	/// <param name="team">Team that is controlled.</param>
	private SsUserInput AddUserInput(int index, SsInput.inputTypes inputType, SsTeam team)
	{
		if (userInput == null)
		{
			userInput = new List<SsUserInput>();
		}

		SsUserInput newInput = new SsUserInput();

		newInput.type = inputType;
		newInput.team = team;

		// Does slot already exist?
		if ((index >= 0) && (index < userInput.Count))
		{
			// Replace input
			userInput[index] = newInput;
		}
		else
		{
			// Fill slots until we reach the needed amount
			while (userInput.Count < index)
			{
				userInput.Add(null);
			}

			userInput.Add(newInput);
		}

		return (newInput);
	}


	/// <summary>
	/// A user's input changed. Call this at the start of the match, or when input changes.
	/// </summary>
	/// <param name="index">Index.</param>
	/// <param name="inputType">Input type.</param>
	/// <param name="team">Team that is controlled.</param>
	public virtual void OnUserInputChanged(int index, SsInput.inputTypes inputType, SsTeam team)
	{
		if (index < 0)
		{
			return;
		}

		SsUserInput input = null;

		if ((index >= 0) && (userInput != null) && (index < userInput.Count))
		{
			input = userInput[index];
		}

		if (input == null)
		{
			// New input

			// AI does Not need user input
			if (inputType == SsInput.inputTypes.ai)
			{
				return;
			}

			input = AddUserInput(index, inputType, team);
		}
		else
		{
			// Update exiting input

			// AI does Not need user input
			if (inputType == SsInput.inputTypes.ai)
			{
				// Clear the input slot
				userInput[index] = null;
				return;
			}

			input.type = inputType;
			input.team = team;
			input.ResetMe();
		}
	}


	/// <summary>
	/// Mobile touch button down.
	/// </summary>
	/// <param name="buttonIndex">Button index.</param>
	public void OnMobileTouchButtonDown(int buttonIndex)
	{
		if ((buttonIndex >= 0) && (buttonIndex < touchButton.Length))
		{
			touchButtonWasPressed[buttonIndex] = true;
			touchButton[buttonIndex] = true;
		}
	}


	/// <summary>
	/// Mobile touch button up.
	/// </summary>
	/// <param name="buttonIndex">Button index.</param>
	public void OnMobileTouchButtonUp(int buttonIndex)
	{
		if ((buttonIndex >= 0) && (buttonIndex < touchButton.Length))
		{
			touchButtonWasPressed[buttonIndex] = false;
			touchButton[buttonIndex] = false;
		}
	}





	/// <summary>
	/// Update is called once per frame.
	/// NOTE: Derived method should at least call PreUpdateInput and PostUpdateInput, as done in this method.
	/// </summary>
	public virtual void Update()
	{
		float dt = Time.deltaTime;

		PreUpdateInput();
		UpdateInput(dt);
		PostUpdateInput();
	}


	/// <summary>
	/// Called before UpdateInput. Clears the input.
	/// </summary>
	/// <returns>The update input.</returns>
	public virtual void PreUpdateInput()
	{
		if ((userInput == null) || (userInput.Count <= 0))
		{
			return;
		}
		
		int i;
		SsUserInput ui;
		
		for (i = 0; i < userInput.Count; i++)
		{
			ui = userInput[i];
			if (ui == null)
			{
				continue;
			}
			ui.ClearInput();
		}
	}


	/// <summary>
	/// Called after UpdateInput.
	/// </summary>
	/// <returns>The update input.</returns>
	public virtual void PostUpdateInput()
	{
		if ((userInput == null) || (userInput.Count <= 0))
		{
			return;
		}

		int i;
		SsUserInput ui;
		Vector3 vec;
		
		for (i = 0; i < userInput.Count; i++)
		{
			ui = userInput[i];
			if (ui == null)
			{
				continue;
			}


			// Change input based on play direction (i.e. camera orientation)
			if ((ui.type != SsInput.inputTypes.mouse) && (SsMatchCamera.Instance != null) && 
			    (SsMatchCamera.Instance.playDirection != SsMatchCamera.playDirections.right))
			{
				vec = new Vector3(ui.axes.x, 0.0f, ui.axes.y);

				if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.up)
				{
					// Play up
					vec = Quaternion.Euler(0.0f, 90.0f, 0.0f) * vec;
				}
				else if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.down)
				{
					// Play down
					vec = Quaternion.Euler(0.0f, 270.0f, 0.0f) * vec;
				}
				else if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.left)
				{
					// Play left
					vec = Quaternion.Euler(0.0f, 180.0f, 0.0f) * vec;
				}

				ui.axes.x = vec.x;
				ui.axes.y = vec.z;
			}

			ui.lookAxes = ui.axes;


			if ((ui.team.PassPlayer != null) && 
			    ((ui.wasPressedButton1) || (ui.wasPressedButton2)))
			{
				vec = ui.mousePos - ui.team.PassPlayer.transform.position;
				vec.y = 0.0f;	// Ignore height
				vec.Normalize();
			}
			else
			{
				vec = Vector3.zero;
			}

			if (ui.wasPressedButton1)
			{
				ui.wasPressedTimeButton1 = Time.time;
				ui.wasPressedPlayerButton1 = ui.player;
				ui.wasPressedPassPlayerButton1 = ui.team.PassPlayer;
				ui.wasPressedAxesButton1 = ui.axes;

				ui.wasPressedPassAxesButton1.x = vec.x;
				ui.wasPressedPassAxesButton1.y = vec.z;
			}

			if (ui.wasPressedButton2)
			{
				ui.wasPressedTimeButton2 = Time.time;
				ui.wasPressedPlayerButton2 = ui.player;
				ui.wasPressedPassPlayerButton2 = ui.team.PassPlayer;
				ui.wasPressedAxesButton2 = ui.axes;

				ui.wasPressedPassAxesButton2.x = vec.x;
				ui.wasPressedPassAxesButton2.y = vec.z;
			}
		}

		for (i = 0; i < touchButtonWasPressed.Length; i++)
		{
			touchButtonWasPressed[i] = false;
		}
	}


	/// <summary>
	/// Updates the input. Reads input from the various hardware.
	/// </summary>
	/// <returns>The input.</returns>
	public virtual void UpdateInput(float dt)
	{
		if ((userInput == null) || (userInput.Count <= 0))
		{
			return;
		}

		int i;
		SsUserInput ui;

		for (i = 0; i < userInput.Count; i++)
		{
			ui = userInput[i];
			if (ui == null)
			{
				continue;
			}

			switch (ui.type)
			{
			case SsInput.inputTypes.ai:
			{
				// Nothing happening here. AI is updated via the relevant AI script.
				break;
			}
			case SsInput.inputTypes.mouseOrKeyboard:
			{
				UpdateMouseOrKeyboard(dt, ui);
				break;
			}
			case SsInput.inputTypes.mouse:
			{
				UpdateMouse(dt, ui);
				break;
			}
			case SsInput.inputTypes.keyboard:
			{
				UpdateKeyboard(dt, ui);
				break;
			}
			case SsInput.inputTypes.gamepad:
			{
				UpdateGamepad(dt, ui);
				break;
			}
			case SsInput.inputTypes.accelerometer:
			{
				UpdateAccelerometer(dt, ui);
				break;
			}
			case SsInput.inputTypes.touch:
			{
				UpdateTouch(dt, ui);
				break;
			}
			case SsInput.inputTypes.gamepad1:
			{
				UpdateGamepad1(dt, ui);
				break;
			}
			case SsInput.inputTypes.gamepad2:
			{
				UpdateGamepad2(dt, ui);
				break;
			}
			} //switch
		}

	}


	/// <summary>
	/// Read input from the mouse or keyboard. 
	/// This uses the default named axes and buttons (i.e. "Horizontal", "Vertical", "Fire1", "Fire2", "Mouse_Fire1", "Mouse_Fire2"). Change it as needed.
	/// </summary>
	/// <returns>The mouse keyboard or gamepad.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateMouseOrKeyboard(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes.x = Input.GetAxis(keyboardHorizontalAxes);
		ui.axes.y = Input.GetAxis(keyboardVerticalAxes);

		
		// Actions
		//--------
		ui.button1 = (Input.GetButton(keyboardFire1)) || (Input.GetButton(mouseFire1));
		ui.wasPressedButton1 = ((Input.GetButtonDown(keyboardFire1)) || (Input.GetButtonDown(mouseFire1)));
		
		ui.button2 = ((Input.GetButton(keyboardFire2)) || (Input.GetButton(mouseFire2)));
		ui.wasPressedButton2 = ((Input.GetButtonDown(keyboardFire2)) || (Input.GetButtonDown(mouseFire2)));

		ui.button3 = ((Input.GetButton(keyboardFire3)) || (Input.GetButton(mouseFire3)));
		ui.wasPressedButton3 = ((Input.GetButtonDown(keyboardFire3)) || (Input.GetButtonDown(mouseFire3)));
	}


	/// <summary>
	/// Read input from the mouse. Player will move/kick towards the mouse cursor.
	/// </summary>
	/// <returns>The mouse.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateMouse(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		// Movement is a vector from the player to the mouse cursor
		if ((ui.player != null) && (SsMatchCamera.Instance != null) && (SsMatchCamera.Instance.Cam != null) && 
		    (SsFieldProperties.Instance != null))
		{
			Ray ray = SsMatchCamera.Instance.Cam.ScreenPointToRay(Input.mousePosition);
			Plane hPlane;
			float distance; 
			Vector3 vec;

			// Plane on ground, normal points up
			hPlane = new Plane(Vector3.up, new Vector3(0.0f, SsFieldProperties.Instance.GroundY, 0.0f));

			if (hPlane.Raycast(ray, out distance))
			{
				ui.mousePos = ray.GetPoint(distance);

				vec = ui.mousePos - ui.player.transform.position;
				vec.y = 0.0f;	// Ignore height
				vec.Normalize();
				ui.axes.x = vec.x;
				ui.axes.y = vec.z;

				if (SsMarkersManager.Instance != null)
				{
					SsMarkersManager.Instance.SetMousePosition(ui.mousePos);
				}

			}
		}


		// Actions
		//--------
		ui.button1 = Input.GetMouseButton(0);
		ui.wasPressedButton1 = Input.GetMouseButtonDown(0);
		
		ui.button2 = Input.GetMouseButton(1);
		ui.wasPressedButton2 = Input.GetMouseButtonDown(1);

		ui.button3 = Input.GetMouseButton(2);
		ui.wasPressedButton3 = Input.GetMouseButtonDown(2);
	}


	/// <summary>
	/// Read input from the keyboard.
	/// This uses the default named axes and buttons (i.e. "Horizontal", "Vertical", "Fire1", "Fire2"). Change it as needed.
	/// </summary>
	/// <returns>The keyboard.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateKeyboard(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes.x = Input.GetAxis(keyboardHorizontalAxes);
		ui.axes.y = Input.GetAxis(keyboardVerticalAxes);
		
		
		// Actions
		//--------
		ui.button1 = Input.GetButton(keyboardFire1);
		ui.wasPressedButton1 = Input.GetButtonDown(keyboardFire1);
		
		ui.button2 = Input.GetButton(keyboardFire2);
		ui.wasPressedButton2 = Input.GetButtonDown(keyboardFire2);

		ui.button3 = Input.GetButton(keyboardFire3);
		ui.wasPressedButton3 = Input.GetButtonDown(keyboardFire3);
	}


	/// <summary>
	/// Read input from accelerometer and touch controls.
	/// </summary>
	/// <returns>The accelerometer and touch.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateAccelerometer(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes = FunkyCalibrate.GetAccelerometerXY();
		if (ui.axes != Vector2.zero)
		{
			ui.axes.Normalize();
		}


		// Actions
		//--------
		ui.button1 = touchButton[0];
		ui.wasPressedButton1 = touchButtonWasPressed[0];
		
		ui.button2 = touchButton[1];
		ui.wasPressedButton2 = touchButtonWasPressed[1];

		ui.button3 = touchButton[2];
		ui.wasPressedButton3 = touchButtonWasPressed[2];
	}


	/// <summary>
	/// Read input from touch controls.
	/// </summary>
	/// <returns>The touch.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateTouch(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		if ((dpad != null) && (dpad.HasTouch))
		{
			ui.axes = dpad.Axes;
		}


		// Actions
		//--------
		ui.button1 = touchButton[0];
		ui.wasPressedButton1 = touchButtonWasPressed[0];
		
		ui.button2 = touchButton[1];
		ui.wasPressedButton2 = touchButtonWasPressed[1];

		ui.button3 = touchButton[2];
		ui.wasPressedButton3 = touchButtonWasPressed[2];
	}


	/// <summary>
	/// Read input from a gamepad. 
	/// This uses the named axes and buttons (i.e. "JX_Horizontal", "JX_Vertical", "JX_Fire1", "JX_Fire2"). Change it as needed.
	/// </summary>
	/// <returns>The gamepad.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateGamepad(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes.x = Input.GetAxis(anyJoystickHorizontalAxes);
		ui.axes.y = Input.GetAxis(anyJoystickVerticalAxes);

		
		// Actions
		//--------
		ui.button1 = Input.GetButton(anyJoystickFire1);
		ui.wasPressedButton1 = Input.GetButtonDown(anyJoystickFire1);
		
		ui.button2 = Input.GetButton(anyJoystickFire2);
		ui.wasPressedButton2 = Input.GetButtonDown(anyJoystickFire2);

		ui.button3 = Input.GetButton(anyJoystickFire3);
		ui.wasPressedButton3 = Input.GetButtonDown(anyJoystickFire3);
	}


	/// <summary>
	/// Read input from gamepad 1. 
	/// This uses the named axes and buttons (i.e. "J1_Horizontal", "J1_Vertical", "J1_Fire1", "J1_Fire2"). Change it as needed.
	/// </summary>
	/// <returns>The gamepad.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateGamepad1(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes.x = Input.GetAxis(joystick1HorizontalAxes);
		ui.axes.y = Input.GetAxis(joystick1VerticalAxes);
		
		
		// Actions
		//--------
		ui.button1 = Input.GetButton(joystick1Fire1);
		ui.wasPressedButton1 = Input.GetButtonDown(joystick1Fire1);
		
		ui.button2 = Input.GetButton(joystick1Fire2);
		ui.wasPressedButton2 = Input.GetButtonDown(joystick1Fire2);

		ui.button3 = Input.GetButton(joystick1Fire3);
		ui.wasPressedButton3 = Input.GetButtonDown(joystick1Fire3);
	}


	/// <summary>
	/// Read input from gamepad 2. 
	/// This uses the named axes and buttons (i.e. "J2_Horizontal", "J2_Vertical", "J2_Fire1", "J2_Fire2"). Change it as needed.
	/// </summary>
	/// <returns>The gamepad.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="ui">User interface.</param>
	public virtual void UpdateGamepad2(float dt, SsUserInput ui)
	{
		// Movement
		//---------
		ui.axes.x = Input.GetAxis(joystick2HorizontalAxes);
		ui.axes.y = Input.GetAxis(joystick2VerticalAxes);
		
		
		// Actions
		//--------
		ui.button1 = Input.GetButton(joystick2Fire1);
		ui.wasPressedButton1 = Input.GetButtonDown(joystick2Fire1);
		
		ui.button2 = Input.GetButton(joystick2Fire2);
		ui.wasPressedButton2 = Input.GetButtonDown(joystick2Fire2);

		ui.button3 = Input.GetButton(joystick2Fire3);
		ui.wasPressedButton3 = Input.GetButtonDown(joystick2Fire3);
	}



	// Static Methods
	//---------------

	/// <summary>
	/// Calibrates the accelerometer.
	/// </summary>
	/// <param name="saveSettings">If set to <c>true</c> save settings.</param>
	static public void CalibrateAccelerometer(bool saveSettings)
	{
		SsSettings.OnCalibrateAccelerometer(saveSettings);
	}
	
	
	/// <summary>
	/// Set the player that is controlled by the user.
	/// </summary>
	/// <returns>The control player.</returns>
	/// <param name="player">Player.</param>
	/// <param name="userIndex">User index.</param>
	static public void SetControlPlayer(SsPlayer player, int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			SsPlayer prevPlayer = instance.userInput[userIndex].player;
			
			instance.userInput[userIndex].player = null;
			if (prevPlayer != null)
			{
				// Change previous player's user input
				prevPlayer.UserControlIndex = -1;

				prevPlayer.Team.SelectReceiverAfterPass = false;

				prevPlayer.Team.ControlPlayer = null;
			}


#if UNITY_EDITOR
			// DEBUG
			if ((SsSettings.Instance != null) && (SsSettings.Instance.controlGoalkeeperOnly) && 
			    (player != null) && (player.Position != SsPlayer.positions.goalkeeper))
			{
				// Only control the goalkeeper
				player = player.Team.GoalKeeper;
			}
#endif //UNITY_EDITOR

			
			instance.userInput[userIndex].player = player;
			if (player != null)
			{
				player.UserControlIndex = userIndex;
				
				if (player.Team.PassPlayer == player)
				{
					player.Team.SetPassPlayer(null, false);
				}
				
				if (player.IsBallPredator)
				{
					player.SetBallPredator(false, false);
				}
				
				player.ClearTargets(true, true, true);

				player.Team.SelectReceiverAfterPass = false;

				player.Team.ControlPlayer = player;
			}
		}
	}
	
	
	/// <summary>
	/// Gets the player controlled by the user.
	/// </summary>
	/// <returns>The control player.</returns>
	/// <param name="userIndex">User index.</param>
	static public SsPlayer GetControlPlayer(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].player);
		}
		return (null);
	}
	
	
	/// <summary>
	/// Called when a player's control index changed. Check if any user inputs must change.
	/// </summary>
	/// <param name="player">Player.</param>
	static public void OnPlayerControlIndexChanged(SsPlayer player, int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null) && 
		    (instance.userInput[userIndex].player == player))
		{
			// Is player now controlled by an AI?
			if (player.IsAiControlled)
			{
				instance.userInput[userIndex].player = null;
			}
		}
	}
	
	
	/// <summary>
	/// Gets the axes for the specified user.
	/// </summary>
	/// <returns>The axes.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetAxes(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].axes);
		}
		return (Vector2.zero);
	}
	
	
	/// <summary>
	/// Gets the look axes for the specified user.
	/// </summary>
	/// <returns>The look axes.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetLookAxes(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].lookAxes);
		}
		return (Vector2.zero);
	}
	
	
	/// <summary>
	/// Gets the mouse position for the specified user. Mouse position is in world, usually on the ground.
	/// </summary>
	/// <returns>The mouse position.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector3 GetMousePos(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].mousePos);
		}
		return (Vector3.zero);
	}
	
	
	/// <summary>
	/// Is button1 held down, for the specified user?
	/// </summary>
	/// <returns>The button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].button1);
		}
		return (false);
	}
	
	
	/// <summary>
	/// Is button2 held down, for the specified user?
	/// </summary>
	/// <returns>The button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].button2);
		}
		return (false);
	}


	/// <summary>
	/// Is button3 held down, for the specified user?
	/// </summary>
	/// <returns>The button3.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetButton3(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].button3);
		}
		return (false);
	}
	
	
	/// <summary>
	/// Was button1 pressed down this frame, for the specified user?
	/// </summary>
	/// <returns>The button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetWasPressedButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedButton1);
		}
		return (false);
	}
	
	
	/// <summary>
	/// Was button2 pressed down this frame, for the specified user?
	/// </summary>
	/// <returns>The button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetWasPressedButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedButton2);
		}
		return (false);
	}


	/// <summary>
	/// Was button3 pressed down this frame, for the specified user?
	/// </summary>
	/// <returns>The button3.</returns>
	/// <param name="userIndex">User index.</param>
	static public bool GetWasPressedButton3(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedButton3);
		}
		return (false);
	}


	/// <summary>
	/// Get Time.time when button 1 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed time button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public float GetWasPressedTimeButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedTimeButton1);
		}
		return (0.0f);
	}


	/// <summary>
	/// Get Time.time when button 2 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed time button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public float GetWasPressedTimeButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedTimeButton2);
		}
		return (0.0f);
	}


	/// <summary>
	/// Get player that was active when button 1 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed player button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public SsPlayer GetWasPressedPlayerButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPlayerButton1);
		}
		return (null);
	}


	/// <summary>
	/// Get player that was active when button 2 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed player button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public SsPlayer GetWasPressedPlayerButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPlayerButton2);
		}
		return (null);
	}


	/// <summary>
	/// Get passed player that was active when button 1 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed pass player button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public SsPlayer GetWasPressedPassPlayerButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPassPlayerButton1);
		}
		return (null);
	}


	/// <summary>
	/// Get passed player that was active when button 2 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed pass player button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public SsPlayer GetWasPressedPassPlayerButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPassPlayerButton2);
		}
		return (null);
	}


	/// <summary>
	/// Get axes when button 1 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed axes button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetWasPressedAxesButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedAxesButton1);
		}
		return (Vector2.zero);
	}


	/// <summary>
	/// Get axes when button 2 was last pressed, for the specified user. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed axes button2.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetWasPressedAxesButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedAxesButton2);
		}
		return (Vector2.zero);
	}


	/// <summary>
	/// Get axes from passed to player to the mouse, when button 1 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed pass axes button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetWasPressedPassAxesButton1(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPassAxesButton1);
		}
		return (Vector2.zero);
	}


	/// <summary>
	/// Get axes from passed to player to the mouse, when button 2 was last pressed. Mainly used to keep track of buttons pressed just before getting the ball.
	/// </summary>
	/// <returns>The was pressed pass axes button1.</returns>
	/// <param name="userIndex">User index.</param>
	static public Vector2 GetWasPressedPassAxesButton2(int userIndex = 0)
	{
		if ((instance != null) && 
		    (userIndex >= 0) && (instance.userInput != null) && (userIndex < instance.userInput.Count) && 
		    (instance.userInput[userIndex] != null))
		{
			return (instance.userInput[userIndex].wasPressedPassAxesButton2);
		}
		return (Vector2.zero);
	}

}
