using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Team stats that can be saved/loaded. Also includes the active tournament stats.
/// </summary>
public class SsTeamStats {


	// Public
	//-------

	static public List<SsTeamStats> teamStats;
	static private bool didCreateStats;


	// Save/load the following:
	//-------------------------
	public string teamId;

	public int tournamentAi = 0;					// Tournament AI level/difficulty

	// NOTE: Here you can add team stats to save/load (e.g. how many times has a team been selected, how many times did team win, etc.)


	// Do Not save/load the following:
	//--------------------------------
	// Tournament: These are setup/calculated.
	public int tournamentPoints = 0;				// Tournament points
	public int tournamentLogPosition = -1;			// Tournament log position
	public int tournamentGoalDifference = 0;		// Tournament goal difference
	public int tournamentLogPlayed = 0;				// Tournament matches played
	public int tournamentLogWon = 0;				// Tournament matches won
	public int tournamentLogLost = 0;				// Tournament matches lost
	public int tournamentLogDraw = 0;				// Tournament matches draw
	public int tournamentHumanIndex = -1;			// Tournament human player index (e.g. 0 = player 1, 1 = player 2, -1 = AI team)

	public string teamName;							// Team name used on UI

	
	
	// Methods
	//--------

	/// <summary>
	/// Loads the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	static public void LoadSettings()
	{
		int i;
		SsTeamStats stats;
		string key, keyPrefix, teamId;

		CreateStats();

		if ((teamStats != null) && (teamStats.Count > 0))
		{
			for (i = 0; i < teamStats.Count; i++)
			{
				keyPrefix = string.Format("teamStat_{0:D2}", i);

				stats = null;

				key = string.Format("{0}_teamId", keyPrefix);
				if (PlayerPrefs.HasKey(key))
				{
					teamId = PlayerPrefs.GetString(key);
					stats = GetTeamStats(teamId);
				}

				if (stats == null)
				{
					continue;
				}

				key = string.Format("{0}_tournAi", keyPrefix);
				if (PlayerPrefs.HasKey(key))
				{
					stats.tournamentAi = PlayerPrefs.GetInt(key);
				}
			}
		}
	}


	/// <summary>
	/// Save the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	static public void SaveSettings()
	{
		int i;
		SsTeamStats stats;
		string key, keyPrefix;

		if ((teamStats != null) && (teamStats.Count > 0))
		{
			for (i = 0; i < teamStats.Count; i++)
			{
				stats = teamStats[i];
				if (stats == null)
				{
					continue;
				}
				
				keyPrefix = string.Format("teamStat_{0:D2}", i);
				
				key = string.Format("{0}_teamId", keyPrefix);
				PlayerPrefs.SetString(key, stats.teamId);
				
				key = string.Format("{0}_tournAi", keyPrefix);
				PlayerPrefs.SetInt(key, stats.tournamentAi);
			}
		}
	}


	/// <summary>
	/// Create the stats if they are Not yet created.
	/// </summary>
	/// <returns>The stats.</returns>
	static public void CreateStats()
	{
		if ((teamStats != null) && (didCreateStats))
		{
			return;
		}

		int max = 10;
		if ((SsResourceManager.Instance != null) && (SsResourceManager.Instance.teams != null))
		{
			max = Mathf.Max(max, SsResourceManager.Instance.teams.Length);
		}

		if (teamStats == null)
		{
			teamStats = new List<SsTeamStats>(max);
		}
		else
		{
			teamStats.Clear();
		}

		if ((SsResourceManager.Instance == null) || (SsResourceManager.Instance.teams.Length <= 0))
		{
			return;
		}

		int i;
		SsResourceManager.SsTeamResource teamRes;
		SsTeamStats stats;

		for (i = 0; i < SsResourceManager.Instance.teams.Length; i++)
		{
			teamRes = SsResourceManager.Instance.teams[i];
			if (teamRes == null)
			{
				continue;
			}

			stats = new SsTeamStats();
			if (stats != null)
			{
				stats.teamId = teamRes.id;
				stats.teamName = teamRes.teamName;

				teamStats.Add(stats);
			}
		}

		didCreateStats = true;
	}


	/// <summary>
	/// Gets the team stats.
	/// </summary>
	/// <returns>The team stats, or null if there is none.</returns>
	/// <param name="teamId">Team identifier.</param>
	static public SsTeamStats GetTeamStats(string teamId)
	{
		if ((teamStats == null) || (teamStats.Count <= 0) || (string.IsNullOrEmpty(teamId)))
		{
			return (null);
		}

		int i;
		SsTeamStats stats;
		for (i = 0; i < teamStats.Count; i++)
		{
			stats = teamStats[i];
			if ((stats != null) && (stats.teamId == teamId))
			{
				return (stats);
			}
		}

		return (null);
	}


	/// <summary>
	/// Init stats that are used in the tournament log.
	/// </summary>
	/// <returns>The tournament log.</returns>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	public void InitTournamentLog(string[] playerTeamIds)
	{
		tournamentPoints = 0;
		tournamentLogPosition = -1;
		tournamentGoalDifference = 0;
		tournamentLogPlayed = 0;
		tournamentLogWon = 0;
		tournamentLogLost = 0;
		tournamentLogDraw = 0;

		InitHumanIndex(playerTeamIds);
	}


	/// <summary>
	/// Inits the human index.
	/// </summary>
	/// <returns>The human index.</returns>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	public void InitHumanIndex(string[] playerTeamIds)
	{
		tournamentHumanIndex = -1;
		if ((playerTeamIds != null) && (playerTeamIds.Length > 0))
		{
			int i;
			for (i = 0; i < playerTeamIds.Length; i++)
			{
				if (teamId == playerTeamIds[i])
				{
					tournamentHumanIndex = i;
					break;
				}
			}
		}
	}
}
