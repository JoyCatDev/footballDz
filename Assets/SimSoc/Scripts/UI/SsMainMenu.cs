using UnityEngine;
using System.Collections;

/// <summary>
/// Main menu.
/// </summary>
public class SsMainMenu : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;



	// Private
	//--------
	static private SsMainMenu instance;


	// Properties
	//-----------
	static public SsMainMenu Instance
	{
		get { return(instance); }
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Awake()
	{
		base.Awake();

		instance = this;

		SsSpawnPersistentPrefabs.SpawnPrefabs();

		if ((SsSettings.selectedMatchType == SsMatch.matchTypes.tournament) && 
		    (SsTournament.IsTournamentActive))
		{
			SsSettings.showTournamentOverview = true;
		}


#if UNITY_EDITOR
		// Editor warnings
		//----------------
		if (SsResourceManager.Instance == null)
		{
			Debug.LogError("ERROR: The Resource Manager is not loaded.");
		}
		if (SsSceneManager.Instance == null)
		{
			Debug.LogError("ERROR: The Scene Manager is not loaded. Make sure it is added to Spawn Persistent Prefabs.");
		}
		if (SsSettings.Instance == null)
		{
			Debug.LogError("ERROR: The Global Settings is not loaded. Make sure it is added to Spawn Persistent Prefabs.");
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();

		instance = null;
	}



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
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		if (SsSettings.showTournamentOverview)
		{
			SsSettings.showTournamentOverview = false;
			if (SsTournamentSelectScreen.ShowTournamentOverviewScreen())
			{
				HideImmediate();
				return;
			}
		}

		base.Show(fromDirection, snap);
	}


	/// <summary>
	/// Raises the exit event.
	/// </summary>
	public void OnExit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
	

	/// <summary>
	/// Raises the p1 event.
	/// </summary>
	public void On1P()
	{
		SsSettings.selectedNumPlayers = 1;
		SsSettings.selectedMatchType = SsMatch.matchTypes.friendly;
	}
	

	/// <summary>
	/// Raises the p2 event.
	/// </summary>
	public void On2P()
	{
		SsSettings.selectedNumPlayers = 2;
		SsSettings.selectedMatchType = SsMatch.matchTypes.friendly;
	}
	

	/// <summary>
	/// Raises the spectate event.
	/// </summary>
	public void OnSpectate()
	{
		SsSettings.selectedNumPlayers = 0;
		SsSettings.selectedMatchType = SsMatch.matchTypes.friendly;

		Invoke("StartSpectate", outInvokeDelay);
	}


	/// <summary>
	/// Starts the spectate match.
	/// </summary>
	/// <returns>The spectate.</returns>
	private void StartSpectate()
	{
		SsMatch.StartMatchFromMenus(null, 
		                            false, false,
		                            SsInput.inputTypes.invalid, SsInput.inputTypes.invalid, 
		                            null, null, false);
	}


	/// <summary>
	/// Raises the tournament event.
	/// </summary>
	public void OnTournament()
	{
		SsSettings.selectedMatchType = SsMatch.matchTypes.tournament;

		// Is there an active tournament? (Even if it already done)
		if (SsTournament.IsTournamentActive)
		{
			// Show the relevant tournament overview screen.
			SsTournamentSelectScreen.ShowTournamentOverviewScreen();
		}
		else
		{
			// Show tournament select screen

			SsSettings.selectedNumPlayers = Mathf.Clamp(SsSettings.selectedNumPlayers, 1, SsSettings.maxPlayers);

			if (SsTournamentSelectScreen.Instance != null)
			{
				SsTournamentSelectScreen.Instance.ShowFromRight();
			}
		}
	}


}
