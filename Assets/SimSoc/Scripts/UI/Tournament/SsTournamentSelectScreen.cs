using UnityEngine;
using System.Collections;

/// <summary>
/// Tournament select screen.
/// </summary>
public class SsTournamentSelectScreen : SsBaseMenu {
	
	// Public
	//-------
	[Header("Link buttons to tournament")]
	public SsUiManager.SsButtonToResource[] tournamentButtons;


	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;

	public UnityEngine.UI.Dropdown playersDropdown;



	// Private
	//--------
	static private SsTournamentSelectScreen instance;



	// Properties
	//-----------
	static public SsTournamentSelectScreen Instance
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
		base.Show(fromDirection, snap);


		if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild) && 
		    (SsSettings.didShowDemoTournamentMsg == false))
		{
			// DEMO: Show message to explain tournaments in the demo
			ShowDemoMessage();
		}


		UpdateControls();
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	public void UpdateControls()
	{
		int i;
		UnityEngine.UI.Text text;
		SsUiManager.SsButtonToResource buttonRes;
		SsTournamentSettings settings;


		if (playersDropdown != null)
		{
			playersDropdown.value = SsSettings.selectedNumPlayers - 1;
		}


		// Set the tournament buttons' text
		if ((tournamentButtons != null) && (tournamentButtons.Length > 0) && 
		    (SsMatchSettings.Instance != null))
		{
			for (i = 0; i < tournamentButtons.Length; i++)
			{
				buttonRes = tournamentButtons[i];
				if ((buttonRes != null) && (buttonRes.button != null) && 
				    (string.IsNullOrEmpty(buttonRes.id) == false))
				{
					text = buttonRes.button.gameObject.GetComponentInChildren<UnityEngine.UI.Text>();
					if (text != null)
					{
						settings = SsMatchSettings.Instance.GetTournamentSettings(buttonRes.id);
						if ((settings != null) && (string.IsNullOrEmpty(settings.displayName) == false))
						{
							text.text = settings.displayName.ToUpper();
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// Get the tournament ID from the button. null if Not found.
	/// </summary>
	/// <returns>The tournament identifier.</returns>
	/// <param name="button">Button.</param>
	private string GetTournamentId(UnityEngine.UI.Button button)
	{
		return (SsUiManager.SsButtonToResource.GetIdFromButton(button, tournamentButtons));
	}


	/// <summary>
	/// Raises the players changed event.
	/// </summary>
	public void OnPlayersChanged()
	{
		if (playersDropdown == null)
		{
			return;
		}

		SsSettings.selectedNumPlayers = Mathf.Clamp(playersDropdown.value + 1, 1, SsSettings.maxPlayers);

		UpdateControls();
	}


	/// <summary>
	/// Raises the tournament event.
	/// </summary>
	/// <param name="button">Button.</param>
	public void OnTournament(UnityEngine.UI.Button button)
	{
		SsSettings.selectedTournamentId = GetTournamentId(button);

#if UNITY_EDITOR
		if ((string.IsNullOrEmpty(SsSettings.selectedTournamentId)) || 
		    ((SsMatchSettings.Instance != null) && 
		 	 (SsMatchSettings.Instance.GetTournamentSettings(SsSettings.selectedTournamentId) == null)))
		{
			Debug.LogError("ERROR: Invalid tournament ID [" + SsSettings.selectedTournamentId + "]. Make sure the button is linked to a valid tournament ID.");
		}
#endif //UNITY_EDITOR
	}


	/// <summary>
	/// Raises the demo info event.
	/// </summary>
	public void OnDemoInfo()
	{
		ShowDemoMessage();
	}


	/// <summary>
	/// Shows the demo message to explain the tournaments.
	/// </summary>
	/// <returns>The demo message.</returns>
	private void ShowDemoMessage()
	{
		int seconds = (int)(SsMatch.demoTournamentDuration * 60.0f);

		SsSettings.didShowDemoTournamentMsg = true;

		SsMsgBox.ShowMsgBox("IN THIS DEMO:\n" + 
		                    "THE TOURNAMENT MATCHES ARE ONLY " + seconds + " SECONDS LONG.\n" + 
		                    "IF YOU PLAY IN A MATCH THAT REQUIRES A WINNER AND IT ENDS IN A DRAW, THEN A RANDOM WINNER WILL BE SELECTED (E.G. FINAL MATCH OR AN ELIMINATION MATCH).",
		                    "", "OK", null, null);
	}


	/// <summary>
	/// Show the relevant tournament overview screen.
	/// </summary>
	/// <returns>The tournament overview screen.</returns>
	static public bool ShowTournamentOverviewScreen()
	{
		if (SsTournamentOverviewScreen.Instance != null)
		{
			SsTournamentOverviewScreen.Instance.ShowFromRight();
			return (true);
		}

		return (false);
	}

}
