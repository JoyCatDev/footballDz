using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimSoc;

/// <summary>
/// Match settings.
/// </summary>
public class SsMatchSettings : MonoBehaviour {

	// Public
	//-------

	// Do NOT save/load the following:
	//--------------------------------
	[Tooltip("Save/load the match settings to disk. Useful if you want the user to be able to change some of the settings via a menu.")]
	public bool saveLoadSettings;			// Indicates if the match settings must be saved/loaded

	[Header("Ball")]
	[Tooltip("Try to keep ball lower than this height, relative to ground. It also limits vertical forces applied to the ball.")]
	public float maxBallHeight = SsBall.defaultMaxHeight;


	[Header("Tournaments")]
	public SsTournamentSettings[] tournamentSettings;



	// Save/load the following:
	//-------------------------
	[Header("Settings that are saved/loaded:")]
	[Tooltip("Number of players per team to have in the match (e.g. can have a 6 per side match, or a 11 per side match).")]
	[Range(SsTeam.minPlayers, SsTeam.maxPlayers)]
	public int numPlayersPerSide = SsTeam.maxPlayers;	// NOTE: Use field.NumPlayersPerSide to get the players, because field may override this value.

	[Tooltip("Match difficulty. (Optional. Only needed if you have set up multiple player skills for different difficulties.)")]
	public int matchDifficulty;
	// Note: This is used to initialise a match/tournament difficulty. During the match use match.Difficulty or team.Difficulty to get the difficulty.

	[Tooltip("Match duration (minutes).")]
	public float matchDuration = SsMatch.defaultDuration;



	// Private
	//--------
	static private SsMatchSettings instance;

	/// <summary>
	/// All tournament settings. It holds the <see cref="tournamentSettings"/> array and the attached
	/// <see cref="ICustomTournamentSettings"/> components.
	/// </summary>
	private readonly List<SsTournamentSettings> _allSettings = new List<SsTournamentSettings>();


	// Properties
	//-----------
	static public SsMatchSettings Instance
	{
		get { return(instance); }
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;

		if (gameObject.GetComponent<SsTournament>() == null)
		{
			gameObject.AddComponent<SsTournament>();
		}
		
		InitTournamentSettings();
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Loads the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	public void LoadSettings()
	{
		if (saveLoadSettings == false)
		{
			return;
		}

		if (PlayerPrefs.HasKey("matchDifficulty"))
		{
			matchDifficulty = PlayerPrefs.GetInt("matchDifficulty");
		}

		if (PlayerPrefs.HasKey("matchDuration"))
		{
			matchDuration = PlayerPrefs.GetFloat("matchDuration");
		}

	}


	/// <summary>
	/// Saves the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	public void SaveSettings()
	{
		if (saveLoadSettings == false)
		{
			return;
		}

		PlayerPrefs.SetInt("matchDifficulty", matchDifficulty);
		PlayerPrefs.SetFloat("matchDuration", matchDuration);

	}


	/// <summary>
	/// Inits the tournament settings.
	/// </summary>
	/// <returns>The tournament settings.</returns>
	private void InitTournamentSettings()
	{
		if (tournamentSettings != null && tournamentSettings.Length > 0)
		{
			_allSettings.AddRange(tournamentSettings);
		}

		var customSettings = GetComponentsInChildren<ICustomTournamentSettings>(true);
		if (customSettings != null && customSettings.Length > 0)
		{
			for (int i = 0, len = customSettings.Length; i < len; i++)
			{
				var custom = customSettings[i];
				if (custom != null)
				{
					_allSettings.Add(custom.CreateSettings());
				}
			}
		}
		
		for (int i = 0, len = _allSettings.Count; i < len; i++)
		{
			var tournament = _allSettings[i];
			if (tournament != null)
			{
				tournament.Init();
			}
		}
	}


	/// <summary>
	/// Gets the tournament settings for the specified tournament id.
	/// </summary>
	public SsTournamentSettings GetTournamentSettings(string id)
	{
		if (_allSettings == null || _allSettings.Count <= 0 || string.IsNullOrEmpty(id))
		{
			return null;
		}
		
		for (int i = 0, len = _allSettings.Count; i < len; i++)
		{
			var tournament = _allSettings[i];
			if (tournament != null && string.IsNullOrEmpty(tournament.id) == false && 
			    tournament.id.Equals(id, StringComparison.InvariantCultureIgnoreCase))
			{
				return tournament;
			}
		}

		return null;
	}
}
