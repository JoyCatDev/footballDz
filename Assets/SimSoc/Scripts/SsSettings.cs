using UnityEngine;
using System.Collections;

/// <summary>
/// Global settings. Most of these are saved/loaded to disk.
/// </summary>
public class SsSettings : MonoBehaviour {

	// Const/Static
	//-------------
	public const int maxPlayers = 2;		// Max human players. NOTE: Changing this will require code changes and menu changes.


	// Public
	//-------

	[Header("Builds")]
	[Tooltip("Is this a demo build?")]
	public bool demoBuild;
	[Tooltip("Player teams to use in demo.")]
	public string[] demoPlayerTeams;
	[Tooltip("Field to use for demo.")]
	public string demoFieldId;
	[Tooltip("Ball to use for demo.")]
	public string demoBallId;


	[Header("Prefabs")]
	public SsMatchSettings matchSettingsPrefab;


	[Header("Platforms")]
	[Tooltip("Platform groups. Specify which runtime platforms belong to each group.")]
	public SsPlatform.SsPlatformGroupsProperties[] platformGroups;

	[Space(10)]
	[Tooltip("Specify which player inputs are allowed per platform/platform group (e.g. keyboard on PC & Mac, accelerometer on phones).")]
	public SsPlatform.SsPlatformInput[] platformInputs;


	// DEBUG TESTING
	//--------------
	[Header("DEBUG TESTING IN EDITOR")]
	[Tooltip("Simulate runtime platform in the editor, for certain features (e.g. to test Android input in the editor).")]
	public bool simulateRuntimePlatform = false;
	[Tooltip("Runtime platform to simulate.")]
	public RuntimePlatform runtimePlatformToSimulate = RuntimePlatform.Android;

	[Space(10)]
	[Tooltip("Log info to console in the editor. This is mainly for non-critical, but possibly useful, info.\n" + 
	         "Warnings and errors are always logged in the editor.")]
	public bool logInfoToConsole = true;

	[Space(10)]
	[Tooltip("Human controls goalkeeper only. Mainly for testing diving.")]
	public bool controlGoalkeeperOnly;

	[Tooltip("Human team always kicks off.")]
	public bool humanKickoff;

	[Tooltip("Disable AI.")]
	public bool disableAi;


	[Header("DEBUG TESTING IN BUILDS & EDITOR")]
	[Space(10)]
	[Tooltip("Disable all lighting's shadows at the start of a match. Can be used to test if shadows slow down the game on mobile devices.")]
	public bool disableShadows;



	// Do NOT save/load the following:
	//--------------------------------
	static public int selectedNumPlayers = 1;						// Selected number of human players, via the menu (0, 1 or 2)
	static public SsMatch.matchTypes selectedMatchType = SsMatch.matchTypes.friendly;	// Selecte match type, via the menu.
	static public string selectedTournamentId = null;				// Selected tournament ID, via the menu
	static public bool showTournamentOverview;						// Show tournament overview next time main menu scene loads?



	// Save/load the following:
	//-------------------------
	static public bool leftHanded = false;							// Left-handed touch controls
	static private Vector3 calibration = new Vector3(0.0f, 0.0f, -1.0f);	// Calibration values to save/load.

	static public float volume = 0.5f;								// Volume. Use SetVolume().
	static public SsInput.inputTypes[] playerInput = new SsInput.inputTypes[maxPlayers];	// Player input type
	static public bool didCalibrate;								// Did calibrate at least once?
	static public bool canSprint = true;							// Can the players sprint?
	static public bool didShowControls;								// Did show controls help popup?
	static public bool showMiniMap = true;							// Show the mini-map?
	static public bool didShowDemoTournamentMsg;					// Did show demo tournament message?
	static public bool passToMouse = false;							// Pass the ball to the mouse position.

	static public string[] selectedTeamIds = {null, null};			// Selected team IDs, via the menu
	static public string selectedFieldId = null;					// Selected field ID, via the menu
	static public string[] selectedFormationIds = {null, null};		// Selected team IDs, via the menu
	static public string selectedBallId = null;						// Selected ball ID, via the menu


	// Private
	//--------
	static private SsSettings instance;


	// Properties
	//-----------
	static public SsSettings Instance
	{
		get { return(instance); }
	}


	static public bool LogInfoToConsole
	{
		get
		{

#if UNITY_EDITOR
			if (instance != null)
			{
				return (instance.logInfoToConsole);
			}

			// Log by default, in case there's something we need to see before the settings are loaded
			return (true);

#else
			// Not in editor: Do Not log
			return (false);

#endif //UNITY_EDITOR

		}
	}



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;

		DontDestroyOnLoad(gameObject);

		SpawnObjects();
		LoadSettings();


#if UNITY_EDITOR
		if (demoBuild)
		{
			Debug.LogWarning("WARNING: This is a demo build. (You can change it on the Global Settings prefab.)");
		}
		if (simulateRuntimePlatform)
		{
			Debug.LogWarning("WARNING: Simulating runtime platform in the editor: " + runtimePlatformToSimulate + 
			                 "      (You can turn it off on the Global Settings prefab.)");
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		SaveSettings();
		instance = null;
	}


	/// <summary>
	/// Raises the application focus event.
	/// </summary>
	/// <param name="focusStatus">Focus status.</param>
	void OnApplicationFocus(bool focusStatus)
	{
		if (focusStatus == false)
		{
			SaveSettings();
		}
	}



	/// <summary>
	/// Loads the settings. This must be called before accessing any settings, ideally in the first scene. 
	/// It will clone the Global Settings prefab.
	/// </summary>
	/// <returns>The settings.</returns>
	static public void LoadSettings()
	{
		int i;
		string key;

		// Load match settings
		if (SsMatchSettings.Instance != null)
		{
			SsMatchSettings.Instance.LoadSettings();
		}

		// Load team stats
		SsTeamStats.LoadSettings();

		// Load tournament
		SsTournament.LoadSettings();


		if ((PlayerPrefs.HasKey("calX")) && 
		    (PlayerPrefs.HasKey("calY")) && 
		    (PlayerPrefs.HasKey("calZ")))
		{
			calibration.x = Mathf.Clamp(PlayerPrefs.GetFloat("calX"), -1.0f, 1.0f);
			calibration.y = Mathf.Clamp(PlayerPrefs.GetFloat("calY"), -1.0f, 1.0f);
			calibration.z = Mathf.Clamp(PlayerPrefs.GetFloat("calZ"), -1.0f, 1.0f);

			FunkyCalibrate.RestoreCalibration(calibration);
		}

		if (PlayerPrefs.HasKey("didCalibrate"))
		{
			didCalibrate = (PlayerPrefs.GetInt("didCalibrate") != 0);
		}

		if (PlayerPrefs.HasKey("canSprint"))
		{
			canSprint = (PlayerPrefs.GetInt("canSprint") != 0);
		}

		if (PlayerPrefs.HasKey("didShowControls"))
		{
			didShowControls = (PlayerPrefs.GetInt("didShowControls") != 0);
		}

		if (PlayerPrefs.HasKey("showMiniMap"))
		{
			showMiniMap = (PlayerPrefs.GetInt("showMiniMap") != 0);
		}

		if (PlayerPrefs.HasKey("didShowDemoTournamentMsg"))
		{
			didShowDemoTournamentMsg = (PlayerPrefs.GetInt("didShowDemoTournamentMsg") != 0);
		}
	
		//if (PlayerPrefs.HasKey("passToMouse"))
		//{
			//passToMouse = (PlayerPrefs.GetInt("passToMouse") != 0);
		//}

		if (PlayerPrefs.HasKey("volume"))
		{
			SetVolume(PlayerPrefs.GetFloat("volume"));
		}

		for (i = 0; i < playerInput.Length; i++)
		{
			key = string.Format("pinput{0}", i);
			if (PlayerPrefs.HasKey(key))
			{
				SetPlayerInput(i, (SsInput.inputTypes)PlayerPrefs.GetInt(key));
			}
		}


		if ((selectedTeamIds != null) && (selectedTeamIds.Length > 0))
		{
			for (i = 0; i < selectedTeamIds.Length; i++)
			{
				key = string.Format("sel_team{0}", i);
				if (PlayerPrefs.HasKey(key))
				{
					selectedTeamIds[i] = PlayerPrefs.GetString(key);
				}
			}
		}
		if (PlayerPrefs.HasKey("sel_field"))
		{
			selectedFieldId = PlayerPrefs.GetString("sel_field");
		}
		if ((selectedFormationIds != null) && (selectedFormationIds.Length > 0))
		{
			for (i = 0; i < selectedFormationIds.Length; i++)
			{
				key = string.Format("sel_formation{0}", i);
				if (PlayerPrefs.HasKey(key))
				{
					selectedFormationIds[i] = PlayerPrefs.GetString(key);
				}
			}
		}
		if (PlayerPrefs.HasKey("sel_ball"))
		{
			selectedBallId = PlayerPrefs.GetString("sel_ball");
		}


		SsTournament.OnNewOrLoaded(true);

		ValidateInput();
	}

	
	/// <summary>
	/// Save the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	static public void SaveSettings()
	{
		int i;
		string key;

		// Save match settings
		if (SsMatchSettings.Instance != null)
		{
			SsMatchSettings.Instance.SaveSettings();
		}


		// Save team stats
		SsTeamStats.SaveSettings();

		// Save tournament
		SsTournament.SaveSettings();


		PlayerPrefs.SetFloat("calX", calibration.x);
		PlayerPrefs.SetFloat("calY", calibration.y);
		PlayerPrefs.SetFloat("calZ", calibration.z);

		PlayerPrefs.SetInt("didCalibrate", GetRandomIntForBool(didCalibrate));
		PlayerPrefs.SetInt("canSprint", GetRandomIntForBool(canSprint));
		PlayerPrefs.SetInt("didShowControls", GetRandomIntForBool(didShowControls));
		PlayerPrefs.SetInt("showMiniMap", GetRandomIntForBool(showMiniMap));
		PlayerPrefs.SetInt("didShowDemoTournamentMsg", GetRandomIntForBool(didShowDemoTournamentMsg));
		//PlayerPrefs.SetInt("passToMouse", GetRandomIntForBool(passToMouse));

		PlayerPrefs.SetFloat("volume", volume);

		for (i = 0; i < playerInput.Length; i++)
		{
			key = string.Format("pinput{0}", i);
			PlayerPrefs.SetInt(key, (int)playerInput[i]);
		}


		if ((selectedTeamIds != null) && (selectedTeamIds.Length > 0))
		{
			for (i = 0; i < selectedTeamIds.Length; i++)
			{
				key = string.Format("sel_team{0}", i);
				PlayerPrefs.SetString(key, selectedTeamIds[i]);
			}
		}
		PlayerPrefs.SetString("sel_field", selectedFieldId);
		if ((selectedFormationIds != null) && (selectedFormationIds.Length > 0))
		{
			for (i = 0; i < selectedFormationIds.Length; i++)
			{
				key = string.Format("sel_formation{0}", i);
				PlayerPrefs.SetString(key, selectedFormationIds[i]);
			}
		}
		PlayerPrefs.SetString("sel_ball", selectedBallId);


		PlayerPrefs.Save();
	}


	
	/// <summary>
	/// Use a random int value to store bools, instead of 1, to make it more difficult to hack.
	/// </summary>
	/// <returns>The random int for bool.</returns>
	static public int GetRandomIntForBool(bool value)
	{
		if (value == false)
		{
			return (0);
		}
		return (Random.Range(100, 10000));
	}


	/// <summary>
	/// Sets the volume.
	/// </summary>
	/// <returns>The volume.</returns>
	/// <param name="newVolume">New volume.</param>
	static public void SetVolume(float newVolume)
	{
		volume = Mathf.Clamp(newVolume, 0.0f, 1.0f);
		SsVolumeController.SetVolume(newVolume);
	}


	/// <summary>
	/// Sets the player input.
	/// </summary>
	/// <returns>The player input.</returns>
	/// <param name="playerIndex">Player index.</param>
	/// <param name="type">Type.</param>
	static public void SetPlayerInput(int playerIndex, SsInput.inputTypes typeType)
	{
		if ((playerIndex >= 0) && (playerIndex < playerInput.Length))
		{
			if (instance != null)
			{
				playerInput[playerIndex] = instance.GetValidPlayerInput(typeType);
			}
			else
			{
				playerInput[playerIndex] = typeType;
			}
		}
	}


	/// <summary>
	/// Gets the player input.
	/// </summary>
	/// <returns>The player input.</returns>
	/// <param name="playerIndex">Player index.</param>
	static public SsInput.inputTypes GetPlayerInput(int playerIndex)
	{
		if ((playerIndex >= 0) && (playerIndex < playerInput.Length))
		{
			return (playerInput[playerIndex]);
		}
		return (SsInput.inputTypes.invalid);
	}


	/// <summary>
	/// Make sure the player input types are valid for the current platform.
	/// </summary>
	/// <returns>The input.</returns>
	static public void ValidateInput()
	{
		if (instance == null)
		{
			return;
		}

		int i;
		for (i = 0; i < playerInput.Length; i++)
		{
			playerInput[i] = instance.GetValidPlayerInput(playerInput[i]);
		}
	}



	/// <summary>
	/// Check if the player input type is valid for the current platform. If Not then returns the first valid type.
	/// </summary>
	/// <returns>The valid player input.</returns>
	/// <param name="inputType">Input type.</param>
	public SsInput.inputTypes GetValidPlayerInput(SsInput.inputTypes inputType)
	{
		if ((platformInputs == null) || (platformInputs.Length <= 0))
		{
			return (inputType);
		}

		SsPlatform.SsPlatformInput platformInput;
		int i;

		// First try the group
		platformInput = GetPlatformGroupInput(SsPlatform.GetCurrentPlatformGroups());
		if (platformInput == null)
		{
			// Try the platform
			platformInput = GetPlatformInput(SsPlatform.GetCurrentPlatform());
		}

		if (platformInput != null)
		{
			for (i = 0; i < platformInput.inputTypes.Length; i++)
			{
				if (platformInput.inputTypes[i] == inputType)
				{
					return (inputType);
				}
			}
			
			// Return the first one
			return (platformInput.inputTypes[0]);
		}
		
		return (inputType);
	}
	
	
	/// <summary>
	/// Gets the platform input object for the specified platform.
	/// </summary>
	/// <returns>The platform input. Null if it could Not be found, or the platform contains no input.</returns>
	/// <param name="platform">Platform.</param>
	public SsPlatform.SsPlatformInput GetPlatformInput(RuntimePlatform platform)
	{
		if ((platformInputs == null) || (platformInputs.Length <= 0))
		{
			return (null);
		}
		
		int i, n;
		SsPlatform.SsPlatformInput platformInput;
		
		// Find the platform
		for (i = 0; i < platformInputs.Length; i++)
		{
			platformInput = platformInputs[i];
			if ((platformInput == null) || 
			    (platformInput.platforms == null) || (platformInput.platforms.Length <= 0) || 
			    (platformInput.inputTypes == null) || (platformInput.inputTypes.Length <= 0))
			{
				continue;
			}
			for (n = 0; n < platformInput.platforms.Length; n++)
			{
				if (platformInput.platforms[n] == platform)
				{
					return (platformInput);
				}
			}
		}
		
		return (null);
	}


	/// <summary>
	/// Gets the platform input object for one of the specified platform groups.
	/// </summary>
	/// <returns>The platform input. Null if it could Not be found, or the platform contains no input.</returns>
	/// <param name="platform">Platform.</param>
	public SsPlatform.SsPlatformInput GetPlatformGroupInput(SsPlatform.platformGroups[] groups)
	{
		if ((platformInputs == null) || (platformInputs.Length <= 0) || 
		    (groups == null) || (groups.Length <= 0))
		{
			return (null);
		}
		
		int i, n, g;
		SsPlatform.SsPlatformInput platformInput;
		
		// Find the platform group
		for (i = 0; i < platformInputs.Length; i++)
		{
			platformInput = platformInputs[i];
			if ((platformInput == null) || 
			    (platformInput.groups == null) || (platformInput.groups.Length <= 0) || 
			    (platformInput.inputTypes == null) || (platformInput.inputTypes.Length <= 0))
			{
				continue;
			}
			for (n = 0; n < platformInput.groups.Length; n++)
			{
				for (g = 0; g < groups.Length; g++)
				{
					if (platformInput.groups[n] == groups[g])
					{
						return (platformInput);
					}
				}
			}
		}
		
		return (null);
	}


	/// <summary>
	/// Get the input types for the current platform/platform group.
	/// </summary>
	/// <returns>The platform input types.</returns>
	public SsInput.inputTypes[] GetPlatformInputTypes()
	{
		SsPlatform.SsPlatformInput platformInput;

		platformInput = GetPlatformGroupInput(SsPlatform.GetCurrentPlatformGroups());
		if (platformInput == null)
		{
			platformInput = GetPlatformInput(SsPlatform.GetCurrentPlatform());
		}

		if (platformInput != null)
		{
			return (platformInput.inputTypes);
		}

		return (null);
	}


	/// <summary>
	/// Raises the calibrate accelerometer event.
	/// </summary>
	/// <param name="saveSettings">If set to <c>true</c> save settings.</param>
	static public void OnCalibrateAccelerometer(bool saveSettings)
	{
		didCalibrate = true;
		calibration = FunkyCalibrate.Calibrate();
		if (saveSettings)
		{
			SaveSettings();
		}
	}


	/// <summary>
	/// Spawn objects.
	/// </summary>
	/// <returns>The objects.</returns>
	private void SpawnObjects()
	{
		// Load the resource manager
		SsResourceManager.Load();

		if (matchSettingsPrefab != null)
		{
			SsMatchSettings ms = (SsMatchSettings)Instantiate(matchSettingsPrefab);
			if (ms != null)
			{
				ms.transform.parent = transform;
			}
		}


		// Editor checks
		//--------------
#if UNITY_EDITOR
		if (matchSettingsPrefab == null)
		{
			Debug.LogError("ERROR: The match settings prefab has not been set on the global settings.");
		}
#endif //UNITY_EDITOR
	}
}
