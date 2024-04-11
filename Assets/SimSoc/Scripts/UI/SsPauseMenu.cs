using UnityEngine;
using System.Collections;

/// <summary>
/// Pause menu.
/// </summary>
public class SsPauseMenu : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;

	public UnityEngine.UI.Slider volumeSlider;
	public UnityEngine.UI.Text volumeValue;

	public UnityEngine.UI.Button calibrateButton;

	public UnityEngine.UI.Toggle miniMapToggle;


	// Private
	//--------
	private bool didShowOnce;


	// Methods
	//--------

	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		UpdateControls(true);
		
		// Reset scroll view position, after all content has been added
		if (scrollView != null)
		{
			scrollView.normalizedPosition = Vector2.zero;
		}
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();

		Time.timeScale = 1.0f;
	}


	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		base.Show(fromDirection, snap);

		Time.timeScale = 0.0f;

		UpdateControls(!didShowOnce);
		didShowOnce = true;
	}


	/// <summary>
	/// Hide the menu immediately (no animation).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The immediate.</returns>
	protected override void HideImmediate()
	{
		base.HideImmediate();

		Time.timeScale = 1.0f;
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	void UpdateControls(bool initialise = false)
	{
		bool enable;
		SsMatch match = SsMatch.Instance;

		if ((calibrateButton != null) && (initialise))
		{
			// Only enable if using accelerometer
			enable = false;

			if (match != null)
			{
				if ((match.LeftTeam != null) && (match.LeftTeam.IsUserControlled))
				{
					if (match.LeftTeam.InputType == SsInput.inputTypes.accelerometer)
					{
						enable = true;
					}
				}
				if ((match.RightTeam != null) && (match.RightTeam.IsUserControlled))
				{
					if (match.RightTeam.InputType == SsInput.inputTypes.accelerometer)
					{
						enable = true;
					}
				}
			}
			
			calibrateButton.gameObject.SetActive(enable);
		}


		if (volumeSlider != null)
		{
			volumeSlider.value = SsSettings.volume;
		}

		if (volumeValue != null)
		{
			volumeValue.text = ((int)(SsSettings.volume * 100)).ToString();
		}




	/*	if (miniMapToggle != null)
		{
			// If HUD has no mini-map then hide the button
			if ((SsHud.Instance != null) && (SsHud.Instance.miniMap == null))
			{
				miniMapToggle.gameObject.SetActive(false);
			}
			else
			{
				if (miniMapToggle.gameObject.activeInHierarchy == false)
				{
					//miniMapToggle.gameObject.SetActive(true);
				}

				miniMapToggle.isOn = SsSettings.showMiniMap;
			}
		}*/


		// Keep this at the bottom of the method
		if (initialise)
		{
			// Reset scroll view position, after all content has been added
			if (scrollView != null)
			{
				scrollView.normalizedPosition = Vector2.zero;
			}
		}
	}


	/// <summary>
	/// Raises the volume event.
	/// </summary>
	public void OnVolume()
	{
		if (volumeSlider != null)
		{
			SsSettings.SetVolume(volumeSlider.value);
		}
		UpdateControls();
	}


	/// <summary>
	/// Raises the quit event.
	/// </summary>
	public void OnQuit()
	{
		SsMatch match = SsMatch.Instance;
		if ((SsSettings.selectedMatchType == SsMatch.matchTypes.tournament) && 
		    (match != null) && (match.IsDone == false) && 
		    ((match.LeftTeam.IsUserControlled) || (match.RightTeam.IsUserControlled)))
		{
			SsMsgBox.ShowMsgBox("YOU WILL LOSE THE TOURNAMENT MATCH IF YOU QUIT.\n\nQUIT?", null, null, OnQuitYes, null);
		}
		else
		{
			SsMsgBox.ShowMsgBox("QUIT?", null, null, OnQuitYes, null);
		}
	}


	/// <summary>
	/// Raises the quit yes event.
	/// </summary>
	public void OnQuitYes()
	{
		SsMatch match = SsMatch.Instance;
		if (match != null)
		{
			if ((SsSettings.selectedMatchType == SsMatch.matchTypes.tournament) && 
			    (match.IsDone == false))
			{
				// Player quit tournament match, so forfeit it
				string forfeitTeam = "";
				if (match.LeftTeam.IsUserControlled)
				{
					forfeitTeam = match.LeftTeam.id;
				}
				else if (match.RightTeam.IsUserControlled)
				{
					forfeitTeam = match.RightTeam.id;
				}

				SsTournament.EndMatch(match.LeftTeam.id, match.LeftTeam.Score, 
				                      match.RightTeam.id, match.RightTeam.Score,
				                      forfeitTeam);
			}

			match.EndMatchAndLoadNextScene();
		}
	}


	/// <summary>
	/// Raises the restart event.
	/// </summary>
	public void OnRestart()
	{
		SsMsgBox.ShowMsgBox("RESTART MATCH?", null, null, OnRestartYes, null);
	}


	/// <summary>
	/// Raises the restart yes event.
	/// </summary>
	public void OnRestartYes()
	{
		SsNewMatch.RestartMatch();
	}


	/// <summary>
	/// Raises the calibrate event.
	/// </summary>
	public void OnCalibrate()
	{
		SsMatchInputManager.CalibrateAccelerometer(true);
	}


	/// <summary>
	/// Raises the mini map event.
	/// </summary>
	public void OnMiniMap()
	{
		/*if (miniMapToggle != null)
		{
			SsSettings.showMiniMap = miniMapToggle.isOn;
		}*/
	}


	/// <summary>
	/// Raises the help event.
	/// </summary>
	public void OnHelp()
	{
		ShowHelpPopup();
	}


	/// <summary>
	/// Shows the help popup for the controls.
	/// </summary>
	/// <returns>The help popup.</returns>
	static public void ShowHelpPopup()
	{
		SsInput.inputTypes[] inputs = SsSettings.Instance.GetPlatformInputTypes();
		SsInput.inputTypes input;
		int i;
		string msg = "YOU CAN CHANGE THE CONTROLS IN THE OPTIONS MENU.\n\n";
		bool didAddGamepad = false;

		SsSettings.didShowControls = true;

		if ((inputs != null) && (inputs.Length > 0))
		{
			for (i = 0; i < inputs.Length; i++)
			{
				input = inputs[i];
				switch (input)
				{
				case SsInput.inputTypes.accelerometer:
				{
					msg += "ACCELEROMETER:\n" + 
							"BUTTON 1 = PASS, CHANGE PLAYER.\n" + 
							"BUTTON 2 = SHOOT, TACKLE.\n" + 
							"HOLD DOWN BUTTON 3 = SPRINT.\n" + 
							"ACCELEROMETER = MOVE PLAYER.\n\n";
					break;
				}
				case SsInput.inputTypes.gamepad:
				case SsInput.inputTypes.gamepad1:
				case SsInput.inputTypes.gamepad2:
				{
					if (didAddGamepad == false)
					{
						didAddGamepad = true;
						msg += "GAMEPAD:\n" + 
								"BUTTON 1 = PASS, CHANGE PLAYER.\n" + 
								"BUTTON 2 = SHOOT, TACKLE.\n" + 
								"HOLD DOWN BUTTON 3 = SPRINT.\n" + 
								"THUMB STICK = MOVE PLAYER.\n\n";
					}
					break;
				}
				case SsInput.inputTypes.keyboard:
				{
					msg += "KEYBOARD:\n" + 
							"LEFT CTRL = PASS, CHANGE PLAYER.\n" + 
							"LEFT ALT = SHOOT, TACKLE.\n" + 
							"HOLD DOWN LEFT SHIFT = SPRINT.\n" + 
							"ARROWS = MOVE PLAYER.\n\n";
					break;
				}
				case SsInput.inputTypes.mouse:
				{
					msg += "MOUSE:\n" + 
							"LEFT CLICK = PASS, CHANGE PLAYER.\n" + 
							"RIGHT CLICK = SHOOT, TACKLE.\n" + 
							"HOLD DOWN MIDDLE BUTTON = SPRINT.\n" + 
							"PLAYERS RUN/KICK TOWARDS THE LOCATION OF THE BLUE CURSOR ON THE GROUND (LOCATED BENEATH THE MOUSE CURSOR).\n\n";
					break;
				}
				case SsInput.inputTypes.touch:
				{
					msg += "TOUCH:\n" + 
							"BUTTON 1 = PASS, CHANGE PLAYER.\n" + 
							"BUTTON 2 = SHOOT, TACKLE.\n" + 
							"HOLD DOWN BUTTON 3 = SPRINT.\n" + 	
							"DPAD = MOVE PLAYER.\n\n";
					break;
				}
				} //switch
			}
		}

		SsMsgBox.ShowMsgBox(msg, "CONTROLS", "OK", null, null, true, (Time.timeScale != 0.0f));
	}
}
