using UnityEngine;
using System.Collections;

/// <summary>
/// Mobile hud. Contains mobile-specific controls.
/// </summary>
public class SsMobileHud : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public GameObject rightHanded;
	public GameObject leftHanded;
	public UnityEngine.UI.Button rightSprint;
	public UnityEngine.UI.Button leftSprint;


	// Private
	//--------
	private SsMatch match;
	private DynamicDpad rightHandedDpad;
	private DynamicDpad leftHandedDpad;
	private DynamicDpad activeDpad;
	private bool initDpad;

	
	
	// Methods
	//--------
	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		// Only show menu for accelerometer or touch controls
		bool showMenu = false;
		if (match != null)
		{
			if ((match.LeftTeam != null) && (match.LeftTeam.IsUserControlled))
			{
				if ((match.LeftTeam.InputType == SsInput.inputTypes.accelerometer) || 
				    (match.LeftTeam.InputType == SsInput.inputTypes.touch))
				{
					showMenu = true;
				}
			}
			if ((match.RightTeam != null) && (match.RightTeam.IsUserControlled))
			{
				if ((match.RightTeam.InputType == SsInput.inputTypes.accelerometer) || 
				    (match.RightTeam.InputType == SsInput.inputTypes.touch))
				{
					showMenu = true;
				}
			}
		}

		if (showMenu == false)
		{
			return;
		}

		base.Show(fromDirection, snap);

		// Show/hide the sprint buttons
		if (rightSprint != null)
		{
			rightSprint.gameObject.SetActive(SsSettings.canSprint);
		}
		if (leftSprint != null)
		{
			leftSprint.gameObject.SetActive(SsSettings.canSprint);
		}

		initDpad = true;
	}


	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		GetReferences();
	}


	/// <summary>
	/// Get the dpad on the game object or its children, even if children are disabled.
	/// </summary>
	/// <returns>The dpad.</returns>
	/// <param name="go">Go.</param>
	DynamicDpad GetDpad(GameObject go)
	{
		DynamicDpad[] dpads = go.GetComponentsInChildren<DynamicDpad>(true);
		int i;
		if ((dpads != null) && (dpads.Length > 0))
		{
			for (i = 0; i < dpads.Length; i++)
			{
				if (dpads[i] != null)
				{
					return (dpads[i]);
				}
			}
		}

		return (null);
	}

	
	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();
		
		match = null;
	}
	
	
	/// <summary>
	/// Gets the references.
	/// </summary>
	/// <returns>The references.</returns>
	void GetReferences()
	{
		if ((SsMatch.Instance == null) || (SsMatch.Instance.IsLoading(true)))
		{
			// Match is busy loading
			Invoke("GetReferences", 0.1f);
			return;
		}

		if (match == null)
		{
			match = SsMatch.Instance;
			if (match != null)
			{
				// Finally show the menu
				Show(fromDirections.invalid, false);
			}
		}
	}


	/// <summary>
	/// Show/hide the dpad.
	/// </summary>
	/// <returns>The dpad.</returns>
	void ShowDpad()
	{
		bool showDpad = false;

		activeDpad = null;

		if (rightHanded != null)
		{
			if (rightHandedDpad == null)
			{
				rightHandedDpad = GetDpad(rightHanded);
			}
			rightHanded.SetActive(!SsSettings.leftHanded);
			
			if ((SsMatchInputManager.Instance != null) && 
			    (SsSettings.leftHanded == false))
			{
				SsMatchInputManager.Instance.Dpad = rightHandedDpad;
			}
		}
		
		if (leftHanded != null)
		{
			if (leftHandedDpad == null)
			{
				leftHandedDpad = GetDpad(leftHanded);
			}
			leftHanded.SetActive(SsSettings.leftHanded);
			
			if ((SsMatchInputManager.Instance != null) && 
			    (SsSettings.leftHanded))
			{
				SsMatchInputManager.Instance.Dpad = leftHandedDpad;
			}
		}


		// Only show dpad for touch controls
		if (match != null)
		{
			if ((match.LeftTeam != null) && (match.LeftTeam.IsUserControlled))
			{
				if (match.LeftTeam.InputType == SsInput.inputTypes.touch)
				{
					showDpad = true;
				}
			}
			if ((match.RightTeam != null) && (match.RightTeam.IsUserControlled))
			{
				if (match.RightTeam.InputType == SsInput.inputTypes.touch)
				{
					showDpad = true;
				}
			}
		}

		if (rightHandedDpad != null)
		{
			rightHandedDpad.gameObject.SetActive(showDpad);
			if ((SsSettings.leftHanded == false) && (showDpad))
			{
				activeDpad = rightHandedDpad;
			}
		}

		if (leftHandedDpad != null)
		{
			leftHandedDpad.gameObject.SetActive(showDpad);
			if ((SsSettings.leftHanded) && (showDpad))
			{
				activeDpad = leftHandedDpad;
			}
		}


		if (activeDpad != null)
		{
			activeDpad.PositionControls();
		}
	}


	/// <summary>
	/// Update this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Update()
	{
		base.Update();

		if ((initDpad) && (IsAnimating == false) && (State == states.visibleAndIdle))
		{
			initDpad = false;
			ShowDpad();
		}
	}


	/// <summary>
	/// Raises the down button1 event.
	/// </summary>
	public void OnDownButton1()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonDown(0);
		}
	}


	/// <summary>
	/// Raises the up button1 event.
	/// </summary>
	public void OnUpButton1()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonUp(0);
		}
	}
	
	
	/// <summary>
	/// Raises the down button2 event.
	/// </summary>
	public void OnDownButton2()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonDown(1);
		}
	}
	
	
	/// <summary>
	/// Raises the up button2 event.
	/// </summary>
	public void OnUpButton2()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonUp(1);
		}
	}


	/// <summary>
	/// Raises the down button3 event.
	/// </summary>
	public void OnDownButton3()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonDown(2);
		}
	}
	
	
	/// <summary>
	/// Raises the up button3 event.
	/// </summary>
	public void OnUpButton3()
	{
		if (SsMatchInputManager.Instance != null)
		{
			SsMatchInputManager.Instance.OnMobileTouchButtonUp(2);
		}
	}
}
