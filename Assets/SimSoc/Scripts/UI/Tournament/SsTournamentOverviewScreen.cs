using UnityEngine;
using System.Collections;
using SimSoc;

/// <summary>
/// Tournament overview screen.
/// </summary>
public class SsTournamentOverviewScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.Button nextButton;

	public UnityEngine.UI.Text heading;
	public UnityEngine.UI.Button logButton;
	public UnityEngine.UI.Button treeButton;
	public UnityEngine.UI.Button groupsButton;
	public UnityEngine.UI.Button knockoutButton;

	[Space(10)]
	public RectTransform nextMatchInfo;
	public UnityEngine.UI.Text nextMatchHeading;
	public UnityEngine.UI.Image leftTeamIcon;
	public UnityEngine.UI.Image rightTeamIcon;
	public UnityEngine.UI.Text leftTeamName;
	public UnityEngine.UI.Text rightTeamName;

	[Space(10)]
	public RectTransform finalResultsInfo;
	public UnityEngine.UI.Image winTeamIcon;
	public UnityEngine.UI.Text winTeamScore;
	public UnityEngine.UI.Image loseTeamIcon;
	public UnityEngine.UI.Text loseTeamScore;


#if UNITY_EDITOR
	private bool didShowOnce;
	private int tournamentCount = -1;					// Keep track if new tournaments have started.
	private int tournamentMatch = -1;					// Keep track if tournaments matches have changed.
#endif //UNITY_EDITOR

	
	// Private
	//--------
	static private SsTournamentOverviewScreen instance;
	
	
	// Properties
	//-----------
	static public SsTournamentOverviewScreen Instance
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
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, bool snap)
	{
		base.Show(fromDirection, snap);
		

		// Initialise selected options, for when this menu is shown before the team select screen
		SsSettings.selectedTournamentId = SsTournament.tournamentId;
		if ((SsTournament.tournamentPlayerTeams != null) && (SsTournament.tournamentPlayerTeams.Length > 0))
		{
			SsSettings.selectedNumPlayers = Mathf.Clamp(SsTournament.tournamentPlayerTeams.Length, 1, SsSettings.maxPlayers);
		}
		
		// End AI matches
		if (!SsTournament.EndAiMatches(true))
		{
			SsMsgBox.ShowMsgBox("FAILED TO END THE AI MATCHES. PLEASE TRY STARTING A NEW TOURNAMENT.", null, "OK", null,
				null);
		}

		// Hide the next button if the tournament is finished
		bool visible = !SsTournament.IsTournamentDone;
		if ((nextButton != null) && (nextButton.gameObject.activeInHierarchy != visible))
		{
			nextButton.gameObject.SetActive(visible);
		}

		UpdateControls(true);


#if UNITY_EDITOR
		if ((didShowOnce == false) || 
		    (tournamentCount != SsTournament.TournamentCount) || 
		    (tournamentMatch != SsTournament.tournamentMatch))
		{
			SsTournament.DebugShowTournamentDetails("OVERVIEW");
		}
		
		didShowOnce = true;
		tournamentCount = SsTournament.TournamentCount;
		tournamentMatch = SsTournament.tournamentMatch;
#endif //UNITY_EDITOR
		
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	/// <param name="updateLogos">Update logos.</param>
	public void UpdateControls(bool updateLogos)
	{
		SsTournamentMatchInfo matchInfo;
		bool visible;
		var settings = SsMatchSettings.Instance != null
			? SsMatchSettings.Instance.GetTournamentSettings(SsTournament.tournamentId)
			: null;
		string newTitle;

		if ((updateLogos) && (heading != null))
		{
			newTitle = "TOURNAMENT OVERVIEW";
			if (SsMatchSettings.Instance != null)
			{
				if ((settings != null) && (string.IsNullOrEmpty(settings.displayName) == false))
				{
					newTitle = settings.displayName.ToUpper();
				}
			}
			heading.text = newTitle;
		}


		if (SsTournament.IsTournamentDone)
		{
			matchInfo = SsTournament.GetFinalMatchInfo();
			if (matchInfo != null)
			{
				if ((updateLogos) && (SsMenuResources.Instance != null))
				{
					if (winTeamIcon != null)
					{
						winTeamIcon.sprite = SsMenuResources.Instance.GetBigTeamLogo(SsTournament.WinTeamId);
					}
					if (loseTeamIcon != null)
					{
						loseTeamIcon.sprite = SsMenuResources.Instance.GetBigTeamLogo(SsTournament.LoseTeamId);
					}
				}
				
				if (winTeamScore != null)
				{
					winTeamScore.text = SsTournament.WinTeamScore.ToString();
				}
				if (loseTeamScore != null)
				{
					loseTeamScore.text = SsTournament.LoseTeamScore.ToString();
				}
			}
		}
		else
		{
			matchInfo = SsTournament.GetMatchInfo();
			if (matchInfo != null)
			{
				if (nextMatchHeading != null)
				{
					if (SsTournament.IsFinalMatch)
					{
						nextMatchHeading.text = "FINAL MATCH";
					}
					else
					{
						nextMatchHeading.text = "NEXT MATCH";
					}
				}

				if ((updateLogos) && (SsMenuResources.Instance != null))
				{
					if (leftTeamIcon != null)
					{
						leftTeamIcon.sprite = SsMenuResources.Instance.GetBigTeamLogo(matchInfo.teamId[0]);
					}
					if (rightTeamIcon != null)
					{
						rightTeamIcon.sprite = SsMenuResources.Instance.GetBigTeamLogo(matchInfo.teamId[1]);
					}
				}

				if (leftTeamName != null)
				{
					leftTeamName.text = matchInfo.teamName[0];
				}
				if (rightTeamName != null)
				{
					rightTeamName.text = matchInfo.teamName[1];
				}
			}
		}


		visible = !SsTournament.IsTournamentDone;
		if ((nextMatchInfo != null) && (nextMatchInfo.gameObject.activeInHierarchy != visible))
		{
			nextMatchInfo.gameObject.SetActive(visible);
		}

		visible = SsTournament.IsTournamentDone;
		if ((finalResultsInfo != null) && (finalResultsInfo.gameObject.activeInHierarchy != visible))
		{
			finalResultsInfo.gameObject.SetActive(visible);
		}

		visible = (SsTournament.tournamentType == SsTournament.tournamentTypes.logTournament);
		if ((logButton != null) && (logButton.gameObject.activeInHierarchy != visible))
		{
			logButton.gameObject.SetActive(visible);
		}

		visible = (SsTournament.tournamentType == SsTournament.tournamentTypes.singleElimination);
		if ((treeButton != null) && (treeButton.gameObject.activeInHierarchy != visible))
		{
			treeButton.gameObject.SetActive(visible);
		}

		var customSettings = settings != null ? settings.CustomSettings : null;
		var worldCup = customSettings != null ? customSettings.CustomController as IWorldCupTournament : null;
		visible = worldCup != null && worldCup.StatsController != null && worldCup.StatsController.GroupStats != null;
		if (groupsButton != null && groupsButton.gameObject.activeSelf != visible)
		{
			groupsButton.gameObject.SetActive(visible);
		}
		
		visible = worldCup != null && 
		          (worldCup.IsKnockoutStageState(worldCup.State) || worldCup.State == WorldCupState.Done);
		if (knockoutButton != null && knockoutButton.gameObject.activeSelf != visible)
		{
			knockoutButton.gameObject.SetActive(visible);
		}
	}


	
	/// <summary>
	/// Raises the back event.
	/// </summary>
	public void OnBack()
	{
		// Has a match been played or the tournament select has Not been shown?
		if ((SsTournament.tournamentMatch > 0) || 
		    ((SsTournamentSelectScreen.Instance != null) && (SsTournamentSelectScreen.Instance.ShowCount <= 0)))
		{
			// Show the main menu
			if (SsMainMenu.Instance != null)
			{
				SsMainMenu.Instance.ShowFromLeft();
			}
		}
		else
		{
			// Allow player to change the tournament settings.
			if (SsTeamSelectScreen.Instance != null)
			{
				SsTeamSelectScreen.Instance.ShowFromLeft();
			}
		}
	}
	
	
	/// <summary>
	/// Raises the new event.
	/// </summary>
	public void OnNew()
	{
		SsMsgBox.ShowMsgBox("START NEW TOURNAMENT?", null, "OK", OnNewYes, null);
	}
	
	
	/// <summary>
	/// Raises the new yes event.
	/// </summary>
	void OnNewYes()
	{
		SsTournament.EndTournament();
		
		HideToRight();
		
		// Show the tournament select screen
		if (SsTournamentSelectScreen.Instance != null)
		{
			SsTournamentSelectScreen.Instance.ShowFromLeft();
		}
	}
	
	
	/// <summary>
	/// Raises the next event.
	/// </summary>
	public void OnNext()
	{
		SsTournament.StartNextTournamentMatch();
	}
}
