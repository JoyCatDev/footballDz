using UnityEngine;
using System.Collections;

/// <summary>
/// Team select screen.
/// </summary>
public class SsTeamSelectScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Link buttons to teams")]
	public SsUiManager.SsButtonToResource[] teamButtons;

	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;

	[Tooltip("Team highlight selectors.")]
	public UnityEngine.UI.Image[] selectors;

	public UnityEngine.UI.Button nextButton;

	public UnityEngine.UI.Dropdown difficultyDropdown;

	[Header("Misc")]
	[Tooltip("Automatically go to the next screen when the teams are selected (i.e. user does not have to click the next button, it will click it automatically).")]
	public bool autoNextWhenTeamsSelected;


	// Private
	//--------
	static private SsTeamSelectScreen instance;

	private int activeUser = 0;							// Active user who has to select a team (0 or 1)
	private int maxActiveUsers = 2;						// Max number of active users


	// Properties
	//-----------
	static public SsTeamSelectScreen Instance
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
	public override void Show(fromDirections fromDirection, bool snap)
	{
		base.Show(fromDirection, snap);

		maxActiveUsers = SsSettings.selectedNumPlayers;
		activeUser = 0;

		UpdateControls();
	}


	/// <summary>
	/// Get the team ID from the button. null if Not found.
	/// </summary>
	/// <returns>The team identifier.</returns>
	/// <param name="button">Button.</param>
	private string GetTeamId(UnityEngine.UI.Button button)
	{
		return (SsUiManager.SsButtonToResource.GetIdFromButton(button, teamButtons));
	}


	/// <summary>
	/// Update the controls.
	/// </summary>
	/// <returns>The selectors.</returns>
	public void UpdateControls()
	{
		int i;

		if ((difficultyDropdown != null) && (SsMatchSettings.Instance != null))
		{
			difficultyDropdown.value = SsMatchSettings.Instance.matchDifficulty;
		}

		for (i = 0; i < 2; i++)
		{
			if ((i >= 0) && 
			    (selectors != null) && (i < selectors.Length) && 
			    (SsSettings.selectedTeamIds != null) && (i < SsSettings.selectedTeamIds.Length))
			{
				UpdateSelector(selectors[i], SsSettings.selectedTeamIds[i], (i <= maxActiveUsers - 1), (i == activeUser));
			}
		}
	}


	/// <summary>
	/// Update the selector.
	/// </summary>
	/// <returns>The selector.</returns>
	/// <param name="selector">Selector.</param>
	/// <param name="selectedTeam">Selected team ID.</param>
	/// <param name="enable">Enable.</param>
	/// <param name="animate">Animate.</param>
	public void UpdateSelector(UnityEngine.UI.Image selector, string selectedTeam, bool enable, bool animate)
	{
		if (selector == null)
		{
			return;
		}

		if ((teamButtons == null) || (teamButtons.Length <= 0) || (enable == false))
		{
			selector.gameObject.SetActive(false);
			return;
		}

		int i;
		SsUiManager.SsButtonToResource res;
		RectTransform rectTransform;
		float z;
		string selectedTeamLower = (string.IsNullOrEmpty(selectedTeam) == false) ? selectedTeam.ToLower() : null;

		for (i = 0; i < teamButtons.Length; i++)
		{
			res = teamButtons[i];
			if ((res == null) || (res.button == null))
			{
				continue;
			}
			if (((string.IsNullOrEmpty(res.id)) && (string.IsNullOrEmpty(selectedTeam))) || 
			    ((string.IsNullOrEmpty(res.id) == false) && (res.id.ToLower() == selectedTeamLower)))
			{
				rectTransform = res.GetButtonRectTransform();
				if (rectTransform != null)
				{
					if (selector.gameObject.activeInHierarchy == false)
					{
						selector.gameObject.SetActive(true);
					}

					z = selector.transform.localPosition.z;
					selector.rectTransform.SetParent(rectTransform, false);
					selector.transform.localPosition = new Vector3(0.0f, 0.0f, z);

					if (animate)
					{
						selector.color = new Color(selector.color.r, selector.color.g, selector.color.b, 1.0f);
					}
					else
					{
						selector.color = new Color(selector.color.r, selector.color.g, selector.color.b, 0.5f);
					}
					return;
				}
			}
		}

		selector.gameObject.SetActive(false);
	}


	/// <summary>
	/// Raises the team event.
	/// </summary>
	/// <param name="button">Button.</param>
	public void OnTeam(UnityEngine.UI.Button button)
	{
		if ((activeUser >= 0) && 
		    (selectors != null) && (activeUser < selectors.Length) && 
		    (SsSettings.selectedTeamIds != null) && (activeUser < SsSettings.selectedTeamIds.Length))
		{
			bool gotoNextScreen = false;

			SsSettings.selectedTeamIds[activeUser] = GetTeamId(button);

			if (maxActiveUsers > 1)
			{
				// Switch to the next user
				if (activeUser == 0)
				{
					activeUser = 1;
				}
				else
				{
					activeUser = 0;
					gotoNextScreen = autoNextWhenTeamsSelected;
				}
			}
			else
			{
				gotoNextScreen = autoNextWhenTeamsSelected;
			}

			UpdateControls();

			if ((gotoNextScreen) && (nextButton != null) && (nextButton.onClick != null))
			{
				nextButton.onClick.Invoke();
			}
		}
	}


	/// <summary>
	/// Raises the back event.
	/// </summary>
	public void OnBack()
	{
		if (SsSettings.selectedMatchType == SsMatch.matchTypes.friendly)
		{
			// Return to Main Menu
			if (SsMainMenu.Instance != null)
			{
				SsMainMenu.Instance.Show(fromDirections.fromLeft, false);
			}
		}
		else if (SsSettings.selectedMatchType == SsMatch.matchTypes.tournament)
		{
			// Return to Tournament Select
			if (SsTournamentSelectScreen.Instance != null)
			{
				SsTournamentSelectScreen.Instance.Show(fromDirections.fromLeft, false);
			}
		}
	}


	/// <summary>
	/// Area the teams the same?
	/// </summary>
	/// <returns>The teams the same.</returns>
	/// <param name="showMessage">Show message.</param>
	public bool AreTeamsTheSame(bool showMessage)
	{
		// Ignore 1 player, or if either player has the random selected
		if ((maxActiveUsers < 2) || 
		    (string.IsNullOrEmpty(SsSettings.selectedTeamIds[0])) ||
		    (string.IsNullOrEmpty(SsSettings.selectedTeamIds[1])) || 
		    (SsSettings.selectedTeamIds[0] != SsSettings.selectedTeamIds[1]))
		{
			return (false);
		}

		if (showMessage)
		{
			SsMsgBox.ShowMsgBox("YOU CAN NOT SELECT THE SAME TEAMS IN A TOURNAMENT.", null, "OK", null, null);
		}

		return (true);
	}


	/// <summary>
	/// Raises the next event.
	/// </summary>
	public void OnNext()
	{
		// Do Not allow the same teams for tournament matches
		if ((SsSettings.selectedMatchType == SsMatch.matchTypes.tournament) && (AreTeamsTheSame(true)))
		{
			return;
		}

		HideToLeft();
		if (SsSettings.selectedMatchType == SsMatch.matchTypes.friendly)
		{
			// Start a friendly match
			Invoke("StartFriendlyMatch", outInvokeDelay);
		}
		else
		{
			// Start the tournament and show the relevant tournament overview screen
			Invoke("StartTournament", outInvokeDelay);
		}
	}


	/// <summary>
	/// Raises the difficulty changed event.
	/// </summary>
	public void OnDifficultyChanged()
	{
		if (difficultyDropdown == null)
		{
			return;
		}

		if (SsMatchSettings.Instance != null)
		{
			SsMatchSettings.Instance.matchDifficulty = difficultyDropdown.value;
		}
		
		UpdateControls();
	}


	/// <summary>
	/// Start a friendly match.
	/// </summary>
	/// <returns>The match.</returns>
	private void StartFriendlyMatch()
	{
		SsMatch.StartMatchFromMenus(SsSettings.selectedTeamIds, 
		                            true, (maxActiveUsers > 1), 
		                            SsSettings.GetPlayerInput(0), SsSettings.GetPlayerInput(1), 
		                            SsSettings.selectedFieldId, SsSettings.selectedBallId,
		                            false);
	}


	/// <summary>
	/// Start the tournament and show the relevant tournament overview screen.
	/// </summary>
	/// <returns>The tournament.</returns>
	private void StartTournament()
	{
		int i;
		string[] teams = new string[maxActiveUsers];
		SsResourceManager.SsTeamResource teamRes;
		string excludeId;
		bool foundEmpty;


		// Copy team IDs
		foundEmpty = false;
		for (i = 0; i < maxActiveUsers; i++)
		{
			teams[i] = SsSettings.selectedTeamIds[i];
			if (string.IsNullOrEmpty(teams[i]))
			{
				foundEmpty = true;
			}
		}


		// Replace empty teams with random teams
		if (foundEmpty)
		{
			for (i = 0; i < maxActiveUsers; i++)
			{
				if ((i == 0) && (maxActiveUsers > 1))
				{
					excludeId = teams[1];
				}
				else if (i == 1)
				{
					excludeId = teams[0];
				}
				else
				{
					excludeId = null;
				}

				if (string.IsNullOrEmpty(teams[i]))
				{
					if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
					{
						// DEMO
						teamRes = SsResourceManager.Instance.GetRandomTeam(excludeId, SsSettings.Instance.demoPlayerTeams);
					}
					else
					{
						teamRes = SsResourceManager.Instance.GetRandomTeam(excludeId);
					}
					if (teamRes != null)
					{
						teams[i] = teamRes.id;
					}
				}
			}
		}


		if (SsTournament.StartNewTournament(SsSettings.selectedTournamentId, teams, null))
		{
			SsTournamentSelectScreen.ShowTournamentOverviewScreen();
		}
		else
		{
			SsMsgBox.ShowMsgBox("FAILED TO START THE TOURNAMENT. PLEASE TRY AGAIN.", null, "OK", OnBack, OnBack);
		}
	}


}
