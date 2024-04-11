using UnityEngine;
using System.Collections;

/// <summary>
/// Input screen.
/// </summary>
public class SsInputScreen : SsBaseMenu
{
	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;

	public UnityEngine.UI.Text heading;

	public UnityEngine.UI.Dropdown inputDropdown;
	public UnityEngine.UI.Button calibrate;
	public UnityEngine.UI.Toggle leftHanded;
	public UnityEngine.UI.Toggle canSprint;


	// Private
	//--------
	private int playerIndex = 0;


	// Methods
	//--------

	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();
		
		// Reset scroll view position, after all content has been added
		if (scrollView != null)
		{
			scrollView.normalizedPosition = Vector2.zero;
		}
	}


	/// <summary>
	/// Set the player input and show the menu.
	/// </summary>
	/// <returns>The player input.</returns>
	/// <param name="playerIndex">Player index.</param>
	public void ShowPlayerInput(int playerIndex)
	{
		if ((playerIndex < 0) || (playerIndex >= SsSettings.maxPlayers))
		{
#if UNITY_EDITOR
			Debug.LogError("ERROR: Tried to show invalid player input: " + playerIndex);
#endif //UNITY_EDITOR

			return;
		}

		this.playerIndex = playerIndex;

		Show(fromDirections.invalid, false);
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

		UpdateControls(true);
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	/// <param name="rebuildDropdown">Rebuild the dropdown lists.</param>
	public void UpdateControls(bool rebuildDropdown = false)
	{
		SsInput.inputTypes selectedInput = SsSettings.GetPlayerInput(playerIndex);

		if ((heading != null) && (rebuildDropdown))
		{
			heading.text = string.Format("P{0} INPUT", playerIndex + 1);
		}

		if (inputDropdown != null)
		{
			SsInput.inputTypes[] inputs = SsSettings.Instance.GetPlatformInputTypes();
			SsInput.inputTypes input;
			UnityEngine.UI.Dropdown.OptionData data;
			int i, oldValue;

			oldValue = inputDropdown.value;

			if (rebuildDropdown)
			{
				inputDropdown.options.Clear();
				inputDropdown.value = 0;

				if ((inputs != null) && (inputs.Length > 0))
				{
					for (i = 0; i < inputs.Length; i++)
					{
						input = inputs[i];
						data = new UnityEngine.UI.Dropdown.OptionData(GetInputText(input));

						inputDropdown.options.Add(data);

						if (input == selectedInput)
						{
							inputDropdown.value = i;
						}
					}
				}
			}
			else
			{
				if ((inputs != null) && (inputs.Length > 0))
				{
					for (i = 0; i < inputs.Length; i++)
					{
						input = inputs[i];
						if (input == selectedInput)
						{
							inputDropdown.value = i;
							break;
						}
					}
				}
			}

			if ((oldValue == inputDropdown.value) && 
			    (inputDropdown.captionText != null))
			{
				// Unity bug: It does Not update the caption when the value remains the same
				inputDropdown.captionText.text = GetInputText(selectedInput);
			}
		}

		if (calibrate != null)
		{
			calibrate.gameObject.SetActive(selectedInput == SsInput.inputTypes.accelerometer);
		}

		if (leftHanded != null)
		{
			leftHanded.isOn = SsSettings.leftHanded;
			leftHanded.gameObject.SetActive((selectedInput == SsInput.inputTypes.accelerometer) || 
			                                (selectedInput == SsInput.inputTypes.touch));
		}

		if (canSprint != null)
		{
			canSprint.isOn = SsSettings.canSprint;
		}
	}


	/// <summary>
	/// Get the text to display for the specified input type.
	/// </summary>
	/// <returns>The input text.</returns>
	/// <param name="inputType">Input type.</param>
	private string GetInputText(SsInput.inputTypes inputType)
	{
		return (inputType.ToString().ToUpper());
	}


	/// <summary>
	/// Raises the input changed event.
	/// </summary>
	public void OnInputChanged()
	{
		if (inputDropdown == null)
		{
			return;
		}

		SsInput.inputTypes[] inputs = SsSettings.Instance.GetPlatformInputTypes();
		int i;

		i = inputDropdown.value;
		if ((i >= 0) && (inputs != null) && (i < inputs.Length))
		{
			SsSettings.SetPlayerInput(playerIndex, inputs[i]);
		}

		UpdateControls();
	}


	/// <summary>
	/// Raises the calibrate event.
	/// </summary>
	public void OnCalibrate()
	{
		SsMatchInputManager.CalibrateAccelerometer(true);
	}


	/// <summary>
	/// Raises the left handed event.
	/// </summary>
	public void OnLeftHanded()
	{
		if (leftHanded != null)
		{
			SsSettings.leftHanded = leftHanded.isOn;
		}
		UpdateControls();
	}


	/// <summary>
	/// Raises the can sprint event.
	/// </summary>
	public void OnCanSprint()
	{
		if (canSprint != null)
		{
			SsSettings.canSprint = canSprint.isOn;
		}
		UpdateControls();
	}
}
