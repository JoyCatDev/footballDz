using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimSoc;
using Random = UnityEngine.Random;


/// <summary>
/// Tournament/league manager.
/// </summary>
public class SsTournament : MonoBehaviour, ITournamentManager {

	// Const/Static
	//-------------
	public const int minTeams = 4;			// Min teams per tournament
	public const int maxTeams = 50;			// Max teams per tournament

	public const int pointsWin = 3;			// Points awarded when winning a match
	public const int pointsLose = 0;		// Points awarded when losing a match
	public const int pointsDraw = 1;		// Points awarded when drawing a match
	
	// Score limits for AI matches (Reminder: There are sometimes random adjustments based on AI skill to increase limits)
	// Winner score = random(random(winLowScoreMin, winLowScoreMax), winHighScoreMax)
	public const int winLowScoreMin = 1;	// Winner's random low min score
	public const int winLowScoreMax = 3;	// Winner's random low max score
	public const int winHighScoreMax = 5;	// Winner's random high max score
	public const int winHighScoreRealMax = 8;	// Score should rarely go more than this when adjusted for AI skill
	// Draw score = random(drawScoreMin, drawScoreMax)
	public const int drawScoreMin = 0;		// Draw score random min
	public const int drawScoreMax = 3;		// Draw score random max



	// Enums
	//------
	// Tournaments types
	public enum tournamentTypes
	{
		/// <summary>
		/// No tournament active.
		/// </summary>
		none = 0,

		/// <summary>
		/// Log based tournament (e.g. league). Each team will play the other team at least once.
		/// The two teams with the most points will play in the final.
		/// </summary>
		logTournament,


		/// <summary>
		/// Single elimination tournament. 1 team is eliminated in each match, until only 2 teams remain for the final.
		/// </summary>
		singleElimination,

		/// <summary>
		/// A custom tournament.
		/// </summary>
		custom,
		
		// REMINDER: DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.
		
		maxTypes
	}


	// How are fields selected
	public enum fieldSelectSequences
	{
		/// <summary>
		/// Alternate between home and away.
		/// </summary>
		alternateHomeAndAway = 0,

		/// <summary>
		/// Only play home games.
		/// </summary>
		homeOnly,

		/// <summary>
		/// Only play away games.
		/// </summary>
		awayOnly,

		/// <summary>
		/// Random fields.
		/// </summary>
		random,

		// REMINDER: DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.

		maxSequences
	}


	// Public
	//-------

	/// <summary>
	/// Fired just before a new tournament starts, or a tournament loads.
	/// </summary>
	public static event TournamentStartOrLoadEventHandler TournamentStartOrLoad;
	
	/// <summary>
	/// Fired when a new tournament started, or a tournament was loaded.
	/// </summary>
	public static event TournamentStartedOrLoadedEventHandler TournamentStartedOrLoaded;

	/// <summary>
	/// Fired when a match ended.
	/// </summary>
	public static event TournamentMatchEndedEventHandler TournamentMatchEnded;

	// Save/load the following:
	//-------------------------
	static public string tournamentId;										// Active tournament ID
	static public tournamentTypes tournamentType = tournamentTypes.none;	// Active tournament type
	static public bool tournamentDone = false;								// Indicates if the current tournament is done
	static public int tournamentMatch = 0;									// Current match (zero-based index)
	static public int tournamentDifficulty = 0;								// Tournament difficulty
	static public string[] tournamentPlayerTeams;							// Human player team IDs
	static public SsTournamentMatchInfo[] tournamentMatchInfos;				// Tournament match info
	static public readonly List<TournamentGroupInfo> groupsInfo = new List<TournamentGroupInfo>();
	
	// Team IDs (can also be used for the display order on the UIs).
	static public string[] tournamentTeamIds;
	
	// Random seed (e.g. for reproducing rankings).
	static public int tournamentRandomSeed;


	// Private
	//--------
	static private List<SsTeamStats> log;				// Sorted log of teams

	static private int finalMatchIndex = -1;			// Index of the final match

	static private string winTeamId;					// Team that won in the final (final always has a winner/loser)
	static private string loseTeamId;					// Team that lost in the final (final always has a winner/loser)
	static private int winTeamScore = -1;				// Winning team score in the final (final always has a winner/loser)
	static private int loseTeamScore = -1;				// Losing team score in the final (final always has a winner/loser)
	
	static private string fieldId;						// Field of the current match

	static private int tournamentCount;					// Count how many tournaments were started. Used by some UI to detect changes.
	static private int newOrLoadedCount;

	/// <summary>
	/// Active tournament settings.
	/// </summary>
	static private SsTournamentSettings activeSettings;


	// Properties
	//-----------

	public static SsTournament Instance { get; private set; }
	
	/// <summary>
	/// Is the tournament done?
	/// </summary>
	/// <value>The tournament done.</value>
	static public bool IsTournamentDone
	{
		get { return(tournamentDone); }
	}



	/// <summary>
	/// Is a tournament active? (NOTE: The tournament may be done, i.e. all the matches have been played.)
	/// </summary>
	/// <value><c>true</c> if is tournament active; otherwise, <c>false</c>.</value>
	static public bool IsTournamentActive
	{
		get { return(tournamentType != tournamentTypes.none); }
	}


	/// <summary>
	/// Is the tournament active and Not yet done?
	/// </summary>
	/// <value><c>true</c> if is tournament active and not done; otherwise, <c>false</c>.</value>
	static public bool IsTournamentActiveAndNotDone
	{
		get
		{
			if ((IsTournamentDone == false) && (IsTournamentActive))
			{
				return (true);
			}
			return (false);
		}
	}


	/// <summary>
	/// Is the tournament at the final match?
	/// </summary>
	/// <value><c>true</c> if is final match; otherwise, <c>false</c>.</value>
	static public bool IsFinalMatch
	{
		get { return ((finalMatchIndex >= 0) && (tournamentMatch >= finalMatchIndex)); }
	}


	/// <summary>
	/// Team that won in the final (final always has a winner/loser)
	/// </summary>
	/// <value>The window team identifier.</value>
	static public string WinTeamId
	{
		get { return(winTeamId); }
	}
	

	/// <summary>
	/// Team that lost in the final (final always has a winner/loser)
	/// </summary>
	/// <value>The lose team identifier.</value>
	static public string LoseTeamId
	{
		get { return(loseTeamId); }
	}
	

	/// <summary>
	/// Winning team score in the final (final always has a winner/loser)
	/// </summary>
	/// <value>The window team score.</value>
	static public int WinTeamScore
	{
		get { return(winTeamScore); }
	}
	

	/// <summary>
	/// Losing team score in the final (final always has a winner/loser).
	/// </summary>
	/// <value>The lose team score.</value>
	static public int LoseTeamScore
	{
		get { return(loseTeamScore); }
	}


	/// <summary>
	/// Gets the log, for a log based tournament. The log is only valid if a tournament has been initialised 
	/// and CalcTeamPointsAndLog has been called.
	/// </summary>
	/// <value>The log.</value>
	static public List<SsTeamStats> Log
	{
		get { return(log); }
	}


	/// <summary>
	/// Count how many tournaments were started. Used by some UI to detect changes.
	/// </summary>
	static public int TournamentCount
	{
		get { return(tournamentCount); }
	}
	
	/// <summary>
	/// Gets the number of match days in the group stage (if any).
	/// </summary>
	public static int GroupStageMatchDays { get; private set; }
	
	/// <summary>
	/// Gets the current match day in the group stage (if any).
	/// </summary>
	public static int GroupStageMatchDay { get; private set; }
	
	/// <inheritdoc/>
	public string TournamentId => tournamentId;
	
	/// <inheritdoc/>
	public tournamentTypes TournamentType => tournamentType;

	/// <inheritdoc/>
	public int RandomSeed => tournamentRandomSeed;

	/// <inheritdoc/>
	public int MatchIndex => tournamentMatch;

	/// <inheritdoc/>
	public string[] TeamIds => tournamentTeamIds;

	/// <inheritdoc/>
	public string[] PlayerTeamIds => tournamentPlayerTeams;

	/// <inheritdoc/>
	public SsTournamentSettings Settings => activeSettings;

	/// <inheritdoc/>
	public SsTournamentMatchInfo[] MatchesInfo => tournamentMatchInfos;

	/// <inheritdoc/>
	public List<TournamentGroupInfo> GroupsInfo => groupsInfo;

	/// <inheritdoc/>
	public int NewOrLoadedCount => newOrLoadedCount;

	// Methods
	//--------

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}
	
	/// <inheritdoc/>
	public int? GetGroupIndex(string teamId)
	{
		if (groupsInfo == null || groupsInfo.Count <= 0 || string.IsNullOrEmpty(teamId))
		{
			return null;
		}

		for (int i = 0, len = groupsInfo.Count; i < len; i++)
		{
			var info = groupsInfo[i];
			if (info == null || info.TeamIds == null || info.TeamIds.Length <= 0)
			{
				continue;
			}
			for (int t = 0, lenT = info.TeamIds.Length; t < lenT; t++)
			{
				if (teamId.Equals(info.TeamIds[t], StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}
		}
		
		return null;
	}

	/// <inheritdoc/>
	public TournamentGroupInfo GetGroup(string teamId)
	{
		var index = GetGroupIndex(teamId);
		return index.HasValue ? groupsInfo[index.Value] : null;
	}

	/// <summary>
	/// Loads the settings.
	/// </summary>
	/// <returns>The settings.</returns>
	static public void LoadSettings()
	{
		int i, n, numPlayers, numTeams, numMatches, numGroups;
		string key, keyPrefix;
		SsTournamentMatchInfo matchInfo;

		numPlayers = 0;
		numTeams = 0;
		numMatches = 0;
		numGroups = 0;

		if (PlayerPrefs.HasKey("tnt_id"))
		{
			tournamentId = PlayerPrefs.GetString("tnt_id");
		}

		if (PlayerPrefs.HasKey("tnt_type"))
		{
			tournamentType = (tournamentTypes)PlayerPrefs.GetInt("tnt_type");
		}

		if (PlayerPrefs.HasKey("tnt_done"))
		{
			tournamentDone = (PlayerPrefs.GetInt("tnt_done") != 0);
		}

		if (PlayerPrefs.HasKey("tnt_match"))
		{
			tournamentMatch = PlayerPrefs.GetInt("tnt_match");
		}

		if (PlayerPrefs.HasKey("tnt_difficulty"))
		{
			tournamentDifficulty = PlayerPrefs.GetInt("tnt_difficulty");
		}

		if (PlayerPrefs.HasKey("tnt_numPlayers"))
		{
			numPlayers = PlayerPrefs.GetInt("tnt_numPlayers");
		}

		if (PlayerPrefs.HasKey("tnt_numTeams"))
		{
			numTeams = PlayerPrefs.GetInt("tnt_numTeams");
		}

		if (PlayerPrefs.HasKey("tnt_numMatches"))
		{
			numMatches = PlayerPrefs.GetInt("tnt_numMatches");
		}
		
		if (PlayerPrefs.HasKey("tnt_numGroups"))
		{
			numGroups = PlayerPrefs.GetInt("tnt_numGroups");
		}
		
		if (PlayerPrefs.HasKey("tnt_rseed"))
		{
			tournamentRandomSeed = PlayerPrefs.GetInt("tnt_rseed");
		}

		// Player IDs
		tournamentPlayerTeams = new string[numPlayers];
		if ((tournamentPlayerTeams != null) && (tournamentPlayerTeams.Length > 0))
		{
			for (i = 0; i < tournamentPlayerTeams.Length; i++)
			{
				key = string.Format("tnt_playerId_{0:D2}", i);
				if (PlayerPrefs.HasKey(key))
				{
					tournamentPlayerTeams[i] = PlayerPrefs.GetString(key);
				}
			}
		}
		
		
		// Team IDs
		tournamentTeamIds = new string[numTeams];
		if ((tournamentTeamIds != null) && (tournamentTeamIds.Length > 0))
		{
			for (i = 0; i < tournamentTeamIds.Length; i++)
			{
				key = string.Format("tnt_teamId_{0:D2}", i);
				if (PlayerPrefs.HasKey(key))
				{
					tournamentTeamIds[i] = PlayerPrefs.GetString(key);
				}
			}
		}
		
		
		// Matches
		tournamentMatchInfos = new SsTournamentMatchInfo[numMatches];
		if ((tournamentMatchInfos != null) && (tournamentMatchInfos.Length > 0))
		{
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				keyPrefix = string.Format("tnt_match_{0:D3}", i);

				tournamentMatchInfos[i] = new SsTournamentMatchInfo();
				matchInfo = tournamentMatchInfos[i];
				if (matchInfo != null)
				{
					key = $"{keyPrefix}_matchDay";
					if (PlayerPrefs.HasKey(key))
					{
						matchInfo.matchDay = PlayerPrefs.GetInt(key);
					}
					
					for (n = 0; n < matchInfo.teamScore.Length; n++)
					{
						key = string.Format("{0}_teamScore_{1:D2}", keyPrefix, n);
						if (PlayerPrefs.HasKey(key))
						{
							matchInfo.teamScore[n] = PlayerPrefs.GetInt(key);
						}

						key = string.Format("{0}_teamId_{1:D2}", keyPrefix, n);
						if (PlayerPrefs.HasKey(key))
						{
							matchInfo.teamId[n] = PlayerPrefs.GetString(key);
						}
					}
				}
			}
		}
		
		// Groups
		groupsInfo.Clear();
		if (numGroups > 0)
		{
			for (i = 0; i < numGroups; i++)
			{
				var info = new TournamentGroupInfo();
				groupsInfo.Add(info);

				keyPrefix = $"tnt_group_{i:D3}";
				
				// Number of teams
				var tempNumTeams = 0;
				key = $"{keyPrefix}_numTeams";
				if (PlayerPrefs.HasKey(key))
				{
					tempNumTeams = PlayerPrefs.GetInt(key);
				}
				if (tempNumTeams <= 0)
				{
					continue;
				}
				
				info.TeamIds = new string[tempNumTeams];
				for (n = 0; n < tempNumTeams; n++)
				{
					// Team IDs
					key = $"{keyPrefix}_teamId_{n:D2}";
					if (PlayerPrefs.HasKey(key))
					{
						info.TeamIds[n] = PlayerPrefs.GetString(key);
					}
				}
			}
		}
	}


	/// <summary>
	/// Saves the settings. Rather call SsSettings.SaveSettings().
	/// </summary>
	/// <returns>The settings.</returns>
	static public void SaveSettings()
	{
		int i, n;
		string key, keyPrefix;
		SsTournamentMatchInfo matchInfo;


		PlayerPrefs.SetString("tnt_id", tournamentId);
		PlayerPrefs.SetInt("tnt_type", (int)tournamentType);
		PlayerPrefs.SetInt("tnt_done", SsSettings.GetRandomIntForBool(tournamentDone));
		PlayerPrefs.SetInt("tnt_match", tournamentMatch);
		PlayerPrefs.SetInt("tnt_difficulty", tournamentDifficulty);

		PlayerPrefs.SetInt("tnt_numPlayers", (tournamentPlayerTeams != null) ? tournamentPlayerTeams.Length : 0);
		PlayerPrefs.SetInt("tnt_numTeams", (tournamentTeamIds != null) ? tournamentTeamIds.Length : 0);
		PlayerPrefs.SetInt("tnt_numMatches", (tournamentMatchInfos != null) ? tournamentMatchInfos.Length : 0);
		PlayerPrefs.SetInt("tnt_numGroups", groupsInfo != null ? groupsInfo.Count : 0);
		
		PlayerPrefs.SetInt("tnt_rseed", tournamentRandomSeed);

		// Player IDs
		if ((tournamentPlayerTeams != null) && (tournamentPlayerTeams.Length > 0))
		{
			for (i = 0; i < tournamentPlayerTeams.Length; i++)
			{
				key = string.Format("tnt_playerId_{0:D2}", i);
				if (string.IsNullOrEmpty(tournamentPlayerTeams[i]) == false)
				{
					PlayerPrefs.SetString(key, tournamentPlayerTeams[i]);
				}
				else
				{
					PlayerPrefs.SetString(key, "");
				}
			}
		}


		// Team IDs
		if ((tournamentTeamIds != null) && (tournamentTeamIds.Length > 0))
		{
			for (i = 0; i < tournamentTeamIds.Length; i++)
			{
				key = string.Format("tnt_teamId_{0:D2}", i);
				if (string.IsNullOrEmpty(tournamentTeamIds[i]) == false)
				{
					PlayerPrefs.SetString(key, tournamentTeamIds[i]);
				}
				else
				{
					PlayerPrefs.SetString(key, "");
				}
			}
		}


		// Matches
		if ((tournamentMatchInfos != null) && (tournamentMatchInfos.Length > 0))
		{
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				keyPrefix = string.Format("tnt_match_{0:D3}", i);

				matchInfo = tournamentMatchInfos[i];
				if (matchInfo != null)
				{
					key = $"{keyPrefix}_matchDay";
					PlayerPrefs.SetInt(key, matchInfo.matchDay);
					
					for (n = 0; n < matchInfo.teamScore.Length; n++)
					{
						key = string.Format("{0}_teamScore_{1:D2}", keyPrefix, n);
						PlayerPrefs.SetInt(key, matchInfo.teamScore[n]);

						key = string.Format("{0}_teamId_{1:D2}", keyPrefix, n);
						if (string.IsNullOrEmpty(matchInfo.teamId[n]) == false)
						{
							PlayerPrefs.SetString(key, matchInfo.teamId[n]);
						}
						else
						{
							PlayerPrefs.SetString(key, "");
						}
					}
				}
			}
		}
		
		// Groups
		if (groupsInfo != null && groupsInfo.Count > 0)
		{
			var len = groupsInfo.Count;
			for (i = 0; i < len; i++)
			{
				var info = groupsInfo[i];
				if (info == null)
				{
					continue;
				}
				
				keyPrefix = $"tnt_group_{i:D3}";
				
				// Number of teams
				var tempNumTeams = info.TeamIds != null ? info.TeamIds.Length : 0;
				key = $"{keyPrefix}_numTeams";
				PlayerPrefs.SetInt(key, tempNumTeams);
				if (tempNumTeams <= 0)
				{
					continue;
				}
				
				for (n = 0; n < tempNumTeams; n++)
				{
					// Team IDs
					key = $"{keyPrefix}_teamId_{n:D2}";
					var teamId = info.TeamIds[n];
					PlayerPrefs.SetString(key, string.IsNullOrEmpty(teamId) ? string.Empty : teamId);
				}
			}
		}
	}


	/// <summary>
	/// Start a new tournament.
	/// </summary>
	/// <returns>The new tournament.</returns>
	/// <param name="id">Tournament ID.</param>
	/// <param name="playerTeamIds">Human player team IDs. Must contain at least 1 human player.</param>
	/// <param name="teamIds">Team IDs. These can be generated by a UI to preview the teams before a tournament starts.</param>
	static public bool StartNewTournament(string id, 
	                                      string[] playerTeamIds, 
	                                      string[] teamIds = null)
	{
		return (StartTournamentInternal(id, playerTeamIds, teamIds));
	}


	/// <summary>
	/// End the tournament. This will cancel the active tournament if it is still in progress.
	/// </summary>
	/// <returns>The tournament.</returns>
	static public void EndTournament()
	{
		int i;

		switch (tournamentType)
		{
			case tournamentTypes.custom:
				var settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
				if (settings == null)
				{
					break;
				}
				var customController = settings.CustomSettings.CustomController;
				if (customController != null)
				{
					customController.OnTournamentEnded();
				}
				
				var matchMaker = settings.CustomSettings.MatchMaker;
				if (matchMaker != null)
				{
					matchMaker.OnTournamentEnded();
				}
				break;
		}

		tournamentType = tournamentTypes.none;

		tournamentDone = false;
		tournamentMatch = 0;
		tournamentId = null;
		activeSettings = null;

		tournamentPlayerTeams = null;
		tournamentTeamIds = null;

		if (log != null)
		{
			log.Clear();
		}

		if ((tournamentMatchInfos != null) && (tournamentMatchInfos.Length > 0))
		{
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				tournamentMatchInfos[i] = null;
			}
		}
		tournamentMatchInfos = null;

		groupsInfo.Clear();

		UpdateMatchDays();
		
		SsSettings.SaveSettings();


#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			Debug.Log("End tournament");
		}
#endif //UNITY_EDITOR
	}


	/// <summary>
	/// Start the next tournament match that contains human players.
	/// </summary>
	/// <returns>The next tournament match.</returns>
	static public bool StartNextTournamentMatch()
	{
		if ((SsResourceManager.Instance == null) || 
		    (SsSceneManager.Instance == null) || 
		    (SsSettings.Instance == null) || 
		    (IsTournamentDone) || (tournamentType == tournamentTypes.none))
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Failed to start the tournament match.");
			#endif //UNITY_EDITOR
			
			return (false);
		}


		SsTournamentMatchInfo matchInfo;
		int i, numPlayers;
		bool result, leftIsPlayer, rightIsPlayer;
		SsInput.inputTypes leftPlayerInput, rightPlayerInput;


		EndAiMatches(false);

		matchInfo = GetMatchInfo();
		if (matchInfo == null)
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Failed to start the tournament match. No match info.");
			#endif //UNITY_EDITOR

			return (false);
		}

		numPlayers = 0;
		leftIsPlayer = false;
		rightIsPlayer = false;
		leftPlayerInput = SsInput.inputTypes.ai;
		rightPlayerInput = SsInput.inputTypes.ai;
		for (i = 0; i < tournamentPlayerTeams.Length; i++)
		{
			if (matchInfo.teamId[0] == tournamentPlayerTeams[i])
			{
				numPlayers ++;
				leftIsPlayer = true;
				leftPlayerInput = SsSettings.GetPlayerInput(i);
			}

			if (matchInfo.teamId[1] == tournamentPlayerTeams[i])
			{
				numPlayers ++;
				rightIsPlayer = true;
				rightPlayerInput = SsSettings.GetPlayerInput(i);
			}
		}

		UpdateMatchHomeTeams();
		SetField();

#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			var teamsString = $"{matchInfo.teamId[0]}  -  {matchInfo.teamId[1]}";
			var fieldString = $"homeTeam: {matchInfo.homeTeam} (field: {GetField()})";
			Debug.Log($"TOURNAMENT: Start match: {tournamentMatch},     {teamsString},     numPlayers: {numPlayers},     {fieldString}");
		}
#endif //UNITY_EDITOR

		result = SsMatch.StartMatchFromMenus(matchInfo.teamId, 
		                                     leftIsPlayer, rightIsPlayer,
		                                     leftPlayerInput, rightPlayerInput, 
		                                     GetField(), SsSettings.selectedBallId,
		                                     matchInfo.needsWinner);

		return (result);
	}


	/// <summary>
	/// Init the specified tournament settings.
	/// </summary>
	/// <returns>The settings.</returns>
	/// <param name="settings">Settings.</param>
	static public void InitSettings(SsTournamentSettings settings)
	{
		if (settings == null)
		{
			return;
		}

		switch (settings.type)
		{
		case tournamentTypes.logTournament:
		{
			// Log based tournament
			//---------------------

			// Min matches needed for each team to play every other at least once
			settings.minMatches = (settings.maxTeams * (settings.maxTeams - 1)) / 2;

			// Multiply by number of times each team must face each other
			settings.maxMatches = settings.minMatches * settings.numFaceEachOther;

			// 1 extra for the final match.
			settings.maxMatches += 1;

			// In each round, every team plays a match.
			settings.matchesPerRound = settings.maxTeams / 2;

			// Max number of rounds, excludes the final match.
			settings.maxRounds = (settings.maxMatches - 1) / settings.matchesPerRound;

			// Min rounds needed for each team to play every other at least once
			settings.minRounds = settings.minMatches / settings.matchesPerRound;

			break;
		}
		case tournamentTypes.singleElimination:
		{
			// Single Elimination
			//-------------------
			settings.maxMatches = settings.maxTeams - 1;

			settings.matchesInFirstRound = settings.maxTeams / 2;
			settings.groupsInFirstRound = settings.matchesInFirstRound / 2;


			// Setup the matches per round, and the next matches indices
			List<int> mpr = new List<int>(settings.maxMatches);
			int numMatches = settings.maxMatches;
			int i, n;

			i = 0;
			n = settings.matchesInFirstRound;
			while ((numMatches > 0) && (n > 0))
			{
				mpr.Add(n);
				numMatches -= n;
				n /= 2;
			}

			settings.maxRounds = mpr.Count;
			settings.matchesInRound = new int[mpr.Count];
			for (i = 0; i < mpr.Count; i++)
			{
				settings.matchesInRound[i] = mpr[i];
			}

			mpr.Clear();

			if (settings.maxRounds > 1)
			{
				n = settings.matchesInRound[0];
				for (i = 0; i < settings.maxMatches - 1; i++)
				{
					mpr.Add(n);
					if ((i % 2) == 1)
					{
						n ++;
					}
				}

				settings.winnerNextMatch = new int[mpr.Count];
				for (i = 0; i < mpr.Count; i++)
				{
					settings.winnerNextMatch[i] = mpr[i];
				}
			}

			break;
		}
		case tournamentTypes.custom:
		{
			settings.CustomSettings.Initialize(Instance);
			break;
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		default:
		{
			Debug.LogError("ERROR: Unsupported tournament type: " + settings.type);
			break;
		}
#endif //UNITY_EDITOR
		}
	}


	/// <summary>
	/// Called after a new tournament started or a tournament was loaded.
	/// It does some calculations and sets up data that is Not saved/loaded.
	/// </summary>
	/// <param name="wasLoaded">Was the tournament loaded?</param>
	static public void OnNewOrLoaded(bool wasLoaded)
	{
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		if (settings == null)
		{
			return;
		}
		
		TournamentStartOrLoad?.Invoke(tournamentId, wasLoaded);
		
		activeSettings = settings;
		newOrLoadedCount++;

		SsTeamStats stats;
		SsTournamentMatchInfo matchInfo;
		int i, n, max;
		string[] playerTeamIds = tournamentPlayerTeams;


		finalMatchIndex = tournamentMatchInfos.Length - 1;


		SsTeamStats.CreateStats();
		for (i = 0; i < tournamentTeamIds.Length; i++)
		{
			stats = SsTeamStats.GetTeamStats(tournamentTeamIds[i]);
			if (stats != null)
			{
				stats.InitHumanIndex(playerTeamIds);
			}
		}

		max = tournamentMatchInfos.Length - 1;
		for (i = 0; i <= max; i++)
		{
			matchInfo = tournamentMatchInfos[i];
			if (matchInfo == null)
			{
				continue;
			}


			// Does match need a winner?
			matchInfo.needsWinner = false;
			if (tournamentType == tournamentTypes.logTournament)
			{
				if (i == max)
				{
					// Final match
					matchInfo.needsWinner = true;
				}
			}
			else if (tournamentType == tournamentTypes.singleElimination)
			{
				matchInfo.needsWinner = true;
			}


			for (n = 0; n < 2; n++)
			{
				matchInfo.humanIndex[n] = -1;

				stats = SsTeamStats.GetTeamStats(matchInfo.teamId[n]);
				if (stats != null)
				{
					matchInfo.humanIndex[n] = stats.tournamentHumanIndex;
					matchInfo.teamName[n] = stats.teamName;
				}
			}


			// Is player
			matchInfo.isPlayer[0] = false;
			matchInfo.isPlayer[1] = false;
			if ((playerTeamIds != null) && (playerTeamIds.Length > 0))
			{
				for (n = 0; n < playerTeamIds.Length; n++)
				{
					if (matchInfo.teamId[0] == playerTeamIds[n])
					{
						matchInfo.isPlayer[0] = true;
					}
					if (matchInfo.teamId[1] == playerTeamIds[n])
					{
						matchInfo.isPlayer[1] = true;
					}
				}
			}
		}

		UpdateMatchHomeTeams();

		switch (tournamentType)
		{
		case tournamentTypes.singleElimination:
		{
			break;
		}
		case tournamentTypes.logTournament:
		{
			// Log based tournament
			//---------------------

			// Calculate the team points and log positions
			CalcTeamPointsAndLog();
			
			break;
		}
		case tournamentTypes.custom:
		{
			var customController = settings.CustomSettings.CustomController;
			if (customController != null)
			{
				customController.OnNewOrLoaded(wasLoaded);
			}
			
			var matchMaker = settings.CustomSettings.MatchMaker;
			if (matchMaker != null)
			{
				matchMaker.OnNewOrLoaded(wasLoaded);
			}

			break;
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		default:
		{
			Debug.LogError($"Unsupported tournament type: {tournamentType}");
			break;
		}
#endif
		} //switch


		// End any outstanding AI matches (in case game shut down without saving previous match results)
		EndAiMatches();

		if (IsFinalMatch)
		{
			SetupFinalMatch();
		}
		
		SetField();

		UpdateMatchDays();
		
		TournamentStartedOrLoaded?.Invoke(tournamentId, wasLoaded);
	}


	/// <summary>
	/// Start a tournament. This must be called from StartTournament.
	/// </summary>
	/// <returns>The new tournament.</returns>
	/// <param name="continueTournament">Are we continuing a tournament?</param>
	/// <param name="id">Tournament ID.</param>
	/// <param name="playerTeamIds">Human player team IDs. Must contain at least 1 human player.</param>
	/// <param name="teamIds">Team IDs. These can be generated by a UI to preview the teams before a tournament starts, or the teams 
	/// from a loaded tournament that must continue. If null then it will be generated.</param>
	static private bool StartTournamentInternal(string id, 
	                                            string[] playerTeamIds, 
	                                            string[] teamIds)
	{
		if ((SsMatchSettings.Instance == null) || (SsResourceManager.Instance == null))
		{
#if UNITY_EDITOR
			if (SsMatchSettings.Instance == null)
			{
				Debug.LogError("ERROR: Failed to start a tournament. Match Settings is null.");
			}
			if (SsResourceManager.Instance == null)
			{
				Debug.LogError("ERROR: Failed to start a tournament. Resource Manager is null.");
			}
#endif //UNITY_EDITOR

			return (false);
		}


		int i;
		SsTeamStats stats;
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(id);
		tournamentTypes type;

		if (settings == null)
		{
#if UNITY_EDITOR
			Debug.LogError("ERROR: Did not find tournament settings for tournament ID: " + id);
#endif //UNITY_EDITOR

			return (false);
		}

		EndTournament();

		type = settings.type;

		tournamentId = id;
		activeSettings = settings;
		tournamentType = type;
		tournamentDone = false;
		tournamentMatch = 0;
		tournamentMatchInfos = null;
		tournamentDifficulty = (SsMatchSettings.Instance != null) ? SsMatchSettings.Instance.matchDifficulty : 0;
		tournamentRandomSeed = Random.Range(int.MinValue, int.MaxValue);
		groupsInfo.Clear();

		tournamentPlayerTeams = new string[playerTeamIds.Length];
		for (i = 0; i < playerTeamIds.Length; i++)
		{
			tournamentPlayerTeams[i] = playerTeamIds[i];
		}

		finalMatchIndex = -1;

		winTeamId = null;
		loseTeamId = null;
		winTeamScore = -1;
		loseTeamScore = -1;

		if (log != null)
		{
			log.Clear();
		}

		tournamentCount ++;


		// Clear the teams' tournament info
		SsTeamStats.CreateStats();
		for (i = 0; i < SsTeamStats.teamStats.Count; i++)
		{
			stats = SsTeamStats.teamStats[i];
			if (stats == null)
			{
				continue;
			}

			stats.tournamentAi = Random.Range(0, settings.maxAiDifficulty + 1);
			if (stats.tournamentAi > tournamentDifficulty)
			{
				stats.tournamentAi = tournamentDifficulty;
			}

			stats.tournamentPoints = 0;
			stats.tournamentLogPosition = -1;
			stats.tournamentGoalDifference = 0;
		}


		switch (tournamentType)
		{
		case tournamentTypes.logTournament:
		{
			// Log based tournament
			//---------------------

			// Clear the matches info/create array
			ClearMatches(id, settings);

			// Set the order of the teams
			if ((tournamentTeamIds == null) || (tournamentTeamIds.Length != settings.maxTeams))
			{
				tournamentTeamIds = new string[settings.maxTeams];
			}
			
			if ((teamIds == null) || (teamIds.Length != settings.maxTeams))
			{
				teamIds = GenerateTeamIDs(id, playerTeamIds, settings);
			}
			
			// Copy the team IDs
			for (i = 0; i < teamIds.Length; i++)
			{
				tournamentTeamIds[i] = teamIds[i];
			}
			
			// Set up the matches
			if (SetupMatches() == false)
			{
#if UNITY_EDITOR
				Debug.LogError("ERROR: Failed to create matches for tournament (id: " + settings.id + ").");
#endif //UNITY_EDITOR

				EndTournament();
				return (false);
			}

			finalMatchIndex = tournamentMatchInfos.Length - 1;

			break;
		}
		case tournamentTypes.singleElimination:
		{
			// Single Elimination
			//-------------------

			if ((settings.maxTeams < 4) || (Mathf.IsPowerOfTwo(settings.maxTeams) == false))
			{
#if UNITY_EDITOR
				Debug.LogError("ERROR: Tournament (id: " + settings.id + ") has invalid max teams. It must be more than 4 and a power of two.");
#endif //UNITY_EDITOR

				EndTournament();
				return (false);
			}


			// Clear the matches info/create array
			ClearMatches(id, settings);
			
			// Set the order of the teams
			if ((tournamentTeamIds == null) || (tournamentTeamIds.Length != settings.maxTeams))
			{
				tournamentTeamIds = new string[settings.maxTeams];
			}
			
			if ((teamIds == null) || (teamIds.Length != settings.maxTeams))
			{
				teamIds = GenerateTeamIDs(id, playerTeamIds, settings);
			}
			
			// Copy the team IDs
			for (i = 0; i < teamIds.Length; i++)
			{
				tournamentTeamIds[i] = teamIds[i];
			}
			
			// Set up the matches
			if (SetupMatches() == false)
			{
#if UNITY_EDITOR
				Debug.LogError("ERROR: Failed to create matches for tournament (id: " + settings.id + ").");
#endif //UNITY_EDITOR
				
				EndTournament();
				return (false);
			}
			
			finalMatchIndex = tournamentMatchInfos.Length - 1;

			break;
		}
		case tournamentTypes.custom:
		{
			var customController = settings.CustomSettings.CustomController;
			if (customController != null && !customController.StartTournamentValidation())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"ERROR: Failed to validate the tournament (id: {settings.id}).");
#endif
				EndTournament();
				return false;
			}
			
			// Clear the matches info/create array
			ClearMatches(id, settings);
			
			// Set the order of the teams
			if (tournamentTeamIds == null || tournamentTeamIds.Length != settings.maxTeams)
			{
				tournamentTeamIds = new string[settings.maxTeams];
			}
			if (teamIds == null || teamIds.Length != settings.maxTeams)
			{
				teamIds = GenerateTeamIDs(id, playerTeamIds, settings);
			}
			
			// Copy the team IDs
			for (i = 0; i < teamIds.Length; i++)
			{
				tournamentTeamIds[i] = teamIds[i];
			}

			if (customController != null && !customController.SetupGroups())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"ERROR: Failed to setup groups for tournament (id: {settings.id}).");
#endif
				EndTournament();
				return false;
			}
			
			if (!SetupMatches())
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError($"ERROR: Failed to create matches for tournament (id: {settings.id}).");
#endif

				EndTournament();
				return false;
			}

			finalMatchIndex = tournamentMatchInfos.Length - 1;
			
			break;
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		default:
			Debug.LogError($"Unsupported tournament type: {tournamentType}");
			break;
#endif
		} //switch


		// Calc some data
		OnNewOrLoaded(false);


		// Save the tournament
		SsSettings.SaveSettings();



#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			string msg = "TOURNAMENT START:" + 
					"    id: " + id + 
					"    teams: " + settings.maxTeams + 
					"    matches: " + settings.maxMatches + " (min: " + settings.minMatches + ")" + 
					"    rounds: " + settings.maxRounds + 
					"    matches/round: " + settings.matchesPerRound + 
					"    minRounds: " + settings.minRounds;

			if ((playerTeamIds != null) && (playerTeamIds.Length > 0))
			{
				msg += "     players: ";
				for (i = 0; i < playerTeamIds.Length; i++)
				{
					msg += playerTeamIds[i];
					if (i < playerTeamIds.Length - 1)
					{
						msg += ", ";
					}
				}
			}

			if ((tournamentTeamIds != null) && (tournamentTeamIds.Length > 0))
			{
				msg += "     teams: ";
				for (i = 0; i < tournamentTeamIds.Length; i++)
				{
					msg += tournamentTeamIds[i];
					if (i < tournamentTeamIds.Length - 1)
					{
						msg += ", ";
					}
				}
			}


			Debug.Log(msg);
			
			DebugShowTournamentDetails("START");
		}
#endif //UNITY_EDITOR

		return (true);
	}


	/// <summary>
	/// Clears the matches, before starting a new tournament. It also creates the matches array 
	/// if it does Not exist (or the number of matches have changed).
	/// </summary>
	/// <returns>The matches.</returns>
	/// <param name="id">Tournament ID. Used to determine the size of the matches array.</param>
	/// <param name="tournamentSettings">Tournament settings. Optional. If null then settings will be
	/// found via tournament ID.</param>
	static private void ClearMatches(string id, 
	                                 SsTournamentSettings tournamentSettings = null)
	{
		if ((SsResourceManager.Instance == null) || 
		    (SsMatchSettings.Instance == null))
		{
			return;
		}

		int i;
		SsTournamentMatchInfo matchInfo;
		SsTournamentSettings settings;

		if (tournamentSettings != null)
		{
			settings = tournamentSettings;
		}
		else
		{
			settings = SsMatchSettings.Instance.GetTournamentSettings(id);
		}

		if ((settings != null) && 
		    ((tournamentMatchInfos == null) || 
		     (tournamentMatchInfos.Length != settings.maxMatches)))
		{
			tournamentMatchInfos = new SsTournamentMatchInfo[settings.maxMatches];
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				tournamentMatchInfos[i] = new SsTournamentMatchInfo();
			}
		}
		else if ((tournamentMatchInfos != null) && (tournamentMatchInfos.Length > 0))
		{
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				matchInfo = tournamentMatchInfos[i];
				if (matchInfo != null)
				{
					matchInfo.Init();
				}
				else
				{
					tournamentMatchInfos[i] = new SsTournamentMatchInfo();
				}
			}
		}
	}


	/// <summary>
	/// Generates the team IDs for a tournament. It selects random teams from the Resource Manager.
	/// </summary>
	/// <returns>The team IDs.</returns>
	/// <param name="id">Tournament ID.</param>
	/// <param name="playerTeamIds">Human player team IDs. Must contain at least 1 human player.</param>
	/// <param name="tournamentSettings">Tournament settings. Optional. If null then settings will be 
	/// found via tournament ID.</param>
	static public string[] GenerateTeamIDs(string id, string[] playerTeamIds,
	                                       SsTournamentSettings tournamentSettings = null)
	{
		if ((SsResourceManager.Instance == null) || (SsMatchSettings.Instance == null))
		{
			return (null);
		}

		SsTournamentSettings settings;

		if (tournamentSettings != null)
		{
			settings = tournamentSettings;
		}
		else
		{
			settings = SsMatchSettings.Instance.GetTournamentSettings(id);
		}

		if (settings == null)
		{
			return (null);
		}

		SsResourceManager.SsResource[] teams = SsResourceManager.Instance.teams;
		string[] teamIds = new string[settings.maxTeams];
		int i, n, t, retry;
		bool found, foundDuplicate;

#if UNITY_EDITOR
		if ((teams == null) || (teams.Length < settings.maxTeams))
		{
			Debug.LogError("ERROR: Trying to create a tournament with " + settings.maxTeams + 
			                 " teams, but the Resource Manager only has " + 
			                 ((teams != null) ? teams.Length.ToString() : "0") + " teams." + 
			                 "  Tournament ID: " + settings.id);
		}
#endif //UNITY_EDITOR
		
		for (i = 0; i < teamIds.Length; i++)
		{
			if (i < playerTeamIds.Length)
			{
				// Players are always first
				teamIds[i] = playerTeamIds[i];
			}
			else
			{
				// Select random teams
				found = false;
				retry = 0;
				while (retry < 10)
				{
					teamIds[i] = teams[Random.Range(0, teams.Length)].id;
					foundDuplicate = false;
					for (n = 0; n < i; n++)
					{
						if (teamIds[i] == teamIds[n])
						{
							foundDuplicate = true;
							break;
						}
					}
					if (foundDuplicate == false)
					{
						found = true;
						break;
					}
					retry ++;
				} //while
				
				if (found == false)
				{
					// Use the first available one
					for (t = 0; t < teams.Length; t++)
					{
						teamIds[i] = teams[t].id;
						foundDuplicate = false;
						for (n = 0; n < i; n++)
						{
							if (teamIds[i] == teamIds[n])
							{
								foundDuplicate = true;
								break;
							}
						}
						if (foundDuplicate == false)
						{
							found = true;
							break;
						}
					}
				}
			}
		} //for i

		var customController = settings.CustomSettings != null ? settings.CustomSettings.CustomController : null;
		if (customController != null)
		{
			customController.OnGeneratedTeamIds(ref teamIds);
		}
		
		return (teamIds);
	}


	/// <summary>
	/// Test if the team is one of the human player teams in the active tournament.
	/// </summary>
	/// <returns><c>true</c> if is player team the specified teamId; otherwise, <c>false</c>.</returns>
	/// <param name="teamId">Team identifier.</param>
	static public bool IsPlayerTeam(string teamId)
	{
		if ((string.IsNullOrEmpty(teamId)) || (tournamentPlayerTeams == null) || (tournamentPlayerTeams.Length <= 0))
		{
			return (false);
		}
		int i;
		for (i = 0; i < tournamentPlayerTeams.Length; i++)
		{
			if (tournamentPlayerTeams[i] == teamId)
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Gets the team's player team index. Returns -1 if the team is not a player team.
	/// </summary>
	public static int GetPlayerTeamIndex(string teamId)
	{
		if (string.IsNullOrEmpty(teamId) || tournamentPlayerTeams == null || tournamentPlayerTeams.Length <= 0)
		{
			return -1;
		}
		int i;
		for (i = 0; i < tournamentPlayerTeams.Length; i++)
		{
			if (teamId.Equals(tournamentPlayerTeams[i], StringComparison.InvariantCultureIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}


	/// <summary>
	/// Sets up the matches for the new tournament.
	/// </summary>
	/// <returns>The matches.</returns>
	static private bool SetupMatches()
	{
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		if (settings == null)
		{
			return (false);
		}

		switch (tournamentType)
		{
		case tournamentTypes.logTournament:
		{
			// Log based tournament
			//---------------------

			if (SsTournamentMatchMaker.CreateMatches(tournamentType, tournamentTeamIds, tournamentPlayerTeams,
				                                     settings, tournamentMatchInfos) == false)
			{
				return (false);
			}

			// Calculate the team points and log positions
			CalcTeamPointsAndLog();

			break;
		}
		case tournamentTypes.singleElimination:
		{
			// Single Elimination
			//-------------------

			if (SsTournamentMatchMaker.CreateMatches(tournamentType, tournamentTeamIds, tournamentPlayerTeams,
			                                         settings, tournamentMatchInfos) == false)
			{
				return (false);
			}

			break;
		}
		case tournamentTypes.custom:
		{
			var matchMaker = settings.CustomSettings.MatchMaker;
			if (matchMaker != null)
			{
				if (!matchMaker.CreateMatches())
				{
					return false;
				}
			}
			else if (!SsTournamentMatchMaker.CreateMatches(tournamentType, tournamentTeamIds, tournamentPlayerTeams, 
				settings, tournamentMatchInfos))
			{
				return false;
			}
			
			var customController = settings.CustomSettings.CustomController;
			if (customController != null)
			{
				customController.OnSetupMatches();
			}

			break;
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		default:
			Debug.LogError($"Unsupported tournament type: {tournamentType}");
			break;
#endif
		} //switch


		if (IsFinalMatch)
		{
			SetupFinalMatch();
		}
		
		SetField();

		return (true);
	}


	/// <summary>
	/// Gets the match info for the specified match, or for the current match.
	/// </summary>
	/// <returns>The match info.</returns>
	/// <param name="index">Index. (-1 = get current match)</param>
	static public SsTournamentMatchInfo GetMatchInfo(int index = -1)
	{
		if ((tournamentMatchInfos == null) || (tournamentMatchInfos.Length <= 0) || 
		    (tournamentTeamIds == null) || (tournamentTeamIds.Length <= 0))
		{
			return (null);
		}
		
		if (index < 0)
		{
			index = tournamentMatch;
		}
		if ((index < 0) || (index >= tournamentMatchInfos.Length))
		{
			return (null);
		}
		
		return (tournamentMatchInfos[index]);
	}


	/// <summary>
	/// Gets the final match info.
	/// </summary>
	/// <returns>The final match info.</returns>
	static public SsTournamentMatchInfo GetFinalMatchInfo()
	{
		return (GetMatchInfo(finalMatchIndex));
	}


	/// <summary>
	/// Ends all AI matches from the current match onwards until a match which contains a human team.
	/// </summary>
	/// <returns>False when an error occurs.</returns>
	/// <param name="includeFinalMatch">Indicates if the final match must also end, if it contains AI teams.</param>
	/// <param name="finalMatchLeftTeam">Left team ID of the final match currently in progress when user quits.</param>
	/// <param name="finalMatchLeftTeamScore">Left team score of the final match currently in progress when user quits.</param>
	/// <param name="finalMatchRightTeam">Right team ID of the final match currently in progress when user quits.</param>
	/// <param name="finalMatchRightTeamScore">Right team score of the final match currently in progress when user quits.</param>
	static public bool EndAiMatches(bool includeFinalMatch = false,
	                                string finalMatchLeftTeam = null, int finalMatchLeftTeamScore = -1,
	                                string finalMatchRightTeam = null, int finalMatchRightTeamScore = -1,
	                                bool logDetailsToConsole = true)
	{
		// NOTE: If new tournament types only contain matches with human teams, then test for the type here and return.

		if ((tournamentMatchInfos == null) || (tournamentMatchInfos.Length <= 0) || 
		    (tournamentTeamIds == null) || (tournamentTeamIds.Length <= 0))
		{
			return true;
		}
		
		if ((includeFinalMatch == false) && (IsFinalMatch))
		{
			return true;
		}

		// Is final match done?
		if ((IsFinalMatch) && (IsTournamentDone))
		{
			return true;
		}
		
		SsTournamentMatchInfo matchInfo;
		int i;
		SsTeamStats[] stats = new SsTeamStats[2];
		int[] minScore = new int[2];
		int[] maxScore = new int[2];
		int winner, loser, tempScore, adjustMin, adjustDraw, aiDiff, random;
		bool randomlySelectWinner, canUseLogPosition;
		int hi, lo, tempDiff, tempChance, forceWinMin, forceWinMax;


		// Can AI use their position in the log to determine if they win?
		canUseLogPosition = false;
		if (tournamentType == tournamentTypes.logTournament)
		{
			canUseLogPosition = true;
		}

		
		// Loop through the matches
		var foundError = false;
		for (i = tournamentMatch; i < tournamentMatchInfos.Length; i++)
		{
			if ((includeFinalMatch == false) && (IsFinalMatch))
			{
				break;
			}
			
			matchInfo = GetMatchInfo(i);
			if (matchInfo == null)
			{
				continue;
			}
			
			// Are there human players in the match?
			if (matchInfo.HasOneOfTeams(tournamentPlayerTeams))
			{
				break;
			}

			// Is the match done?
			if (matchInfo.MatchDone)
			{
				continue;
			}
			
			stats[0] = SsTeamStats.GetTeamStats(matchInfo.teamId[0]);
			stats[1] = SsTeamStats.GetTeamStats(matchInfo.teamId[1]);
			
			// The stats can also be null if the teams don't exist in the resources manager.
			if ((stats[0] == null) || (stats[1] == null))
			{
				foundError |= true;
				continue;
			}
			
			
			// NOTE: The average goal difference in soccer is about 2. In most matches, rarely does a team score 
			//		 more than 3 goals. It is very rare for a team to score more than 8 goals in a match.
			
			winner = -1;
			loser = -1;
			adjustMin = 0;
			adjustDraw = 0;
			randomlySelectWinner = false;
			aiDiff = 0;
			forceWinMin = -1;
			forceWinMax = -1;
			
			
			// Is this the final match and it is in progress?
			if ((IsFinalMatch) && 
			    (string.IsNullOrEmpty(finalMatchLeftTeam) == false) && (finalMatchLeftTeamScore >= 0) && 
			    (string.IsNullOrEmpty(finalMatchRightTeam) == false) && (finalMatchRightTeamScore >= 0))
			{
				// Final match
				//------------
				
				// Add 1 score to either of the teams
				if (Random.Range(0, 100) < 50)
				{
					finalMatchLeftTeamScore ++;
				}
				else
				{
					finalMatchRightTeamScore ++;
				}
				
				if (finalMatchLeftTeamScore == finalMatchRightTeamScore)
				{
					// There must be a winner
					if (Random.Range(0, 100) < 50)
					{
						finalMatchLeftTeamScore ++;
					}
					else
					{
						finalMatchRightTeamScore ++;
					}
				}
				
				if (finalMatchLeftTeam == stats[0].teamId)
				{
					// Left team = team[0]
					if (finalMatchLeftTeamScore > finalMatchRightTeamScore)
					{
						// Win: team[0]
						winner = 0;
						loser = 1;
						minScore[winner] = finalMatchLeftTeamScore;
						minScore[loser] = finalMatchRightTeamScore;
					}
					else
					{
						// Win: team[1]
						winner = 1;
						loser = 0;
						minScore[winner] = finalMatchRightTeamScore;
						minScore[loser] = finalMatchLeftTeamScore;
					}
				}
				else
				{
					// Right team = team[0]
					if (finalMatchRightTeamScore > finalMatchLeftTeamScore)
					{
						// Win: team[0]
						winner = 0;
						loser = 1;
						minScore[winner] = finalMatchRightTeamScore;
						minScore[loser] = finalMatchLeftTeamScore;
					}
					else
					{
						// Win: team[1]
						winner = 1;
						loser = 0;
						minScore[winner] = finalMatchLeftTeamScore;
						minScore[loser] = finalMatchRightTeamScore;
					}
				}

				foundError |= !EndMatch(stats[winner].teamId, minScore[winner], stats[loser].teamId, minScore[loser], 
					null, logDetailsToConsole, false);
				
				break;
			}
			else if (Random.Range(0, 100) < 5)
			{
				// Random winner
				//--------------
				
				// Every once in a while, any of the 2 teams can win (e.g. underdogs defeat champions)
				randomlySelectWinner = true;
			}
			else
			{
				// Select a winner, based on skill
				//--------------------------------
				
				// Stronger AI have a greater chance of winning
				hi = -1;
				lo = -1;
				
				if (stats[0].tournamentAi == stats[1].tournamentAi)
				{
					// Equal skills
					if (Random.Range(0, 100) < 50)
					{
						randomlySelectWinner = true;
					}
					else
					{
						// Team with a higher log position stands a better chance of winning
						random = Random.Range(0, 100);
						if ((canUseLogPosition) && 
						    (stats[0].tournamentLogPosition > stats[1].tournamentLogPosition) && 
						    (random < 10) &&
						    (stats[0].tournamentLogPosition >= 0) && 
						    (stats[1].tournamentLogPosition >= 0))
						{
							// Win: team[0]
							winner = 0;
							loser = 1;
							aiDiff = stats[0].tournamentAi - stats[1].tournamentAi;
						}
						else if ((canUseLogPosition) && 
						         (stats[1].tournamentLogPosition > stats[0].tournamentLogPosition) && 
						         (random < 10) && 
						         (stats[0].tournamentLogPosition >= 0) && 
						         (stats[1].tournamentLogPosition >= 0))
						{
							// Win: team[1]
							winner = 1;
							loser = 0;
							aiDiff = stats[1].tournamentAi - stats[0].tournamentAi;
						}
						else if (matchInfo.needsWinner)
						{
							// Match must have a winner
							randomlySelectWinner = true;
						}
						else
						{
							// Draw
						}
					}
				}
				else if (stats[0].tournamentAi > stats[1].tournamentAi)
				{
					// team[0] has more skill
					hi = 0;
					lo = 1;
				}
				else if (stats[1].tournamentAi > stats[0].tournamentAi)
				{
					// team[1] has more skill
					hi = 1;
					lo = 0;
				}
				
				if ((hi != -1) && (lo != -1))
				{
					// Increase the chance that the stronger team will win
					tempDiff = stats[hi].tournamentAi - stats[lo].tournamentAi;
					if (tempDiff == 1)
					{
						tempChance = 70;
					}
					else if (tempDiff == 2)
					{
						tempChance = 80;
					}
					else 
					{
						tempChance = 95;
					}
					
					if (Random.Range(0, 100) < tempChance)
					{
						//  Stronger team wins
						winner = hi;
						loser = lo;
						aiDiff = stats[hi].tournamentAi - stats[lo].tournamentAi;
					}
					else if (Random.Range(0, 100) < 30)
					{
						// Weaker team barely wins
						winner = lo;
						loser = hi;
						aiDiff = 0;
						forceWinMin = winLowScoreMin;
						forceWinMax = winLowScoreMin + 1;
					}
					else if (matchInfo.needsWinner)
					{
						// Match must have a winner
						randomlySelectWinner = true;
					}
					else
					{
						// Draw
					}
				}
			}
			
			
			if ((winner != loser) && 
			    (winner >= 0) && (winner <= 1) && 
			    (loser >= 0) && (loser <= 1))
			{
				// Found a winner & loser
			}
			else if (matchInfo.needsWinner)
			{
				// Match must have a winner
				randomlySelectWinner = true;
			}
			
			
			if (randomlySelectWinner)
			{
				// Randomly select a winner
				if (Random.Range(0, 100) < 50)
				{
					// Win: team[0]
					winner = 0;
					loser = 1;
					aiDiff = stats[0].tournamentAi - stats[1].tournamentAi;
				}
				else
				{
					// Win: team[1]
					winner = 1;
					loser = 0;
					aiDiff = stats[1].tournamentAi - stats[0].tournamentAi;
				}
			}
			
			
			if ((winner != loser) && 
			    (winner >= 0) && (winner <= 1) && 
			    (loser >= 0) && (loser <= 1))
			{
				// If winner's AI is stronger then make the score slightly bigger
				if (aiDiff > 0)
				{
					random = Random.Range(0, 100);
					if (random < 50)
					{
						// Potentially higher score
						adjustMin = Random.Range(0, Mathf.Min(aiDiff, (winHighScoreRealMax - winHighScoreMax)) + 1);
					}
					else if (random < 55)
					{
						// Higher score
						adjustMin = Mathf.Min(aiDiff, (winHighScoreRealMax - winHighScoreMax));
					}
				}
				
				// Set scores' random ranges
				if ((forceWinMin != -1) && (forceWinMax != -1))
				{
					minScore[winner] = forceWinMin;
					maxScore[winner] = forceWinMax;
				}
				else
				{
					minScore[winner] = Random.Range(winLowScoreMin, 
					                                (winLowScoreMax + 1) + adjustMin);  // e.g. range 1-3 (without adjustment)
					maxScore[winner] = Random.Range(minScore[winner], 
					                                minScore[winner] + (winHighScoreMax - winLowScoreMax + 1));  // e.g. range 1-5 (without adjustment)
				}
				minScore[loser] = 0;
				if (minScore[winner] > winLowScoreMax)
				{
					// Winner's score is high
					if (Random.Range(0, 100) < 30)
					{
						// Increase the loser's score as the winner's score is increased
						maxScore[loser] = minScore[winner] - 1;
					}
					else
					{
						// Keep the loser's score low
						maxScore[loser] = winLowScoreMax - 1;
					}
				}
				else
				{
					maxScore[loser] = minScore[winner] - 1;
				}

				foundError |= !EndMatch(stats[0].teamId, Random.Range(minScore[0], maxScore[0] + 1), stats[1].teamId, 
				         Random.Range(minScore[1], maxScore[1] + 1), null, logDetailsToConsole, false);
			}
			else
			{
				// Draw
				tempScore = Random.Range(drawScoreMin, (drawScoreMax + 1) + adjustDraw);	// e.g. range 0-3 (without adjustment)

				foundError |= !EndMatch(stats[0].teamId, tempScore, stats[1].teamId, tempScore, null,
					logDetailsToConsole, false);
			}

			// This can happen if there's an error while ending a match
			if (tournamentMatchInfos == null)
			{
				break;
			}
		}
		
		if (IsFinalMatch)
		{
			SetupFinalMatch();
		}
		
		SsSettings.SaveSettings();

		return !foundError;
	}


	/// <summary>
	/// Ends the current match. It also sets up the final match info when the last round has finished.
	/// </summary>
	/// <returns>False when an error occurs.</returns>
	/// <param name="teamId1">Team id1.</param>
	/// <param name="teamScore1">Team score1.</param>
	/// <param name="teamId2">Team id2.</param>
	/// <param name="teamScore2">Team score2.</param>
	/// <param name="forfeitTeam">Team that forfeited the match. Null if none.</param>
	static public bool EndMatch(string teamId1, int teamScore1, 
	                            string teamId2, int teamScore2,
	                            string forfeitTeam = null,
	                            bool logDetailsToConsole = true,
	                            bool saveSettings = true)
	{
		if ((tournamentMatchInfos == null) || (tournamentMatchInfos.Length <= 0) || 
		    (tournamentTeamIds == null) || (tournamentTeamIds.Length <= 0))
		{
			return true;
		}
		
		if ((tournamentMatch < 0) || (tournamentMatch >= tournamentMatchInfos.Length))
		{
#if UNITY_EDITOR
			Debug.LogError("ERROR: Invalid tournament match index (" + tournamentMatch + ") in EndMatch!");
#endif //UNITY_EDITOR

			return false;
		}
		
		
		SsTournamentMatchInfo matchInfo = tournamentMatchInfos[tournamentMatch];
		if ((matchInfo == null) || (matchInfo.MatchDone))
		{
			return true;
		}

		if (string.IsNullOrEmpty(teamId1) || string.IsNullOrEmpty(teamId2) ||
		    string.IsNullOrEmpty(matchInfo.teamId[0]) || string.IsNullOrEmpty(matchInfo.teamId[1]))
		{
#if UNITY_EDITOR
			Debug.LogError($"Invalid team IDs. Failed to end the match for tournament: {tournamentId}");
#endif
			return false;
		}
		
		var oldTournamentId = tournamentId;
		
		// Update the match info
		if (matchInfo.teamId[0] == teamId1)
		{
			matchInfo.teamScore[0] = teamScore1;
			matchInfo.teamScore[1] = teamScore2;
		}
		else
		{
			matchInfo.teamScore[0] = teamScore2;
			matchInfo.teamScore[1] = teamScore1;
		}
		
		
		// Did a team forfeit the match?
		if (string.IsNullOrEmpty(forfeitTeam) == false)
		{
			int goalDifference;

			// Are both player teams?
			if ((matchInfo.isPlayer[0]) && (matchInfo.isPlayer[1]) && (matchInfo.needsWinner == false))
			{
				// End in a draw
				matchInfo.teamScore[0] = 0;
				matchInfo.teamScore[1] = 0;
			}
			else
			{
				// Give the other team a 3-0 victory, or higher if the goal difference is higher
				if (matchInfo.teamId[0] == forfeitTeam)
				{
					// Forfeit: team[0]
					goalDifference = matchInfo.teamScore[1] - matchInfo.teamScore[0];
					if (goalDifference <= 3)
					{
						matchInfo.teamScore[0] = 0;
						matchInfo.teamScore[1] = 3;
					}
				}
				else
				{
					// Forfeit: team[1]
					goalDifference = matchInfo.teamScore[0] - matchInfo.teamScore[1];
					if (goalDifference <= 3)
					{
						matchInfo.teamScore[0] = 3;
						matchInfo.teamScore[1] = 0;
					}
				}
			}
		}


		if (tournamentType == tournamentTypes.singleElimination)
		{
			// Single Elimination
			PutWinnerInNextMatch(matchInfo, tournamentMatch);
		}

		
		// Go to the next match
		tournamentMatch ++;
		
		
		// Was this the final match?
		if (tournamentMatch > finalMatchIndex)
		{
			// Final match complete
			tournamentDone = true;
			
			if (matchInfo.teamScore[0] > matchInfo.teamScore[1])
			{
				winTeamId = matchInfo.teamId[0];
				winTeamScore = matchInfo.teamScore[0];
				
				loseTeamId = matchInfo.teamId[1];
				loseTeamScore = matchInfo.teamScore[1];
			}
			else
			{
				winTeamId = matchInfo.teamId[1];
				winTeamScore = matchInfo.teamScore[1];
				
				loseTeamId = matchInfo.teamId[0];
				loseTeamScore = matchInfo.teamScore[0];
			}
		}
		else
		{
			// Is the next match the final match?
			if (IsFinalMatch)
			{
				SetupFinalMatch();
			}
			
			SetField();
		}

		if (tournamentType == tournamentTypes.logTournament)
		{
			// Calculate the team points and log positions.
			CalcTeamPointsAndLog();
		}

		var foundError = false;
		var settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		var customSettings = settings != null ? settings.CustomSettings : null;
		var customController = customSettings != null ? customSettings.CustomController : null;
		if (customController != null && !customController.OnMatchEnded())
		{
			foundError = true;
		}

		var matchMaker = customSettings != null ? customSettings.MatchMaker : null;
		if (!foundError && matchMaker != null && !matchMaker.OnMatchEnded())
		{
			foundError = true;
		}
		
		if (foundError)
		{
#if UNITY_EDITOR
			Debug.LogError($"Failed to end the match for tournament: {tournamentId}");
#endif
			EndTournament();
		}
		
		UpdateMatchDays();

		if (saveSettings)
		{
			SsSettings.SaveSettings();
		}
		
#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			if (logDetailsToConsole)
			{
				DebugShowTournamentDetails("END MATCH");
			}
		}
#endif //UNITY_EDITOR
		
		TournamentMatchEnded?.Invoke(oldTournamentId, foundError);

		return !foundError;
	}

	/// <summary>
	/// Puts the winner in next match, for the single elimination tournament.
	/// </summary>
	/// <returns>The winner in next match.</returns>
	/// <param name="srcMatch">Source match.</param>
	/// <param name="srcMatchIndex">Source match index.</param>
	static public void PutWinnerInNextMatch(SsTournamentMatchInfo srcMatch, int srcMatchIndex)
	{
		if (SsMatchSettings.Instance == null)
		{
			return;
		}

		int i;
		SsTournamentMatchInfo nextMatch;
		int winner;
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		if (settings == null)
		{
			return;
		}

		nextMatch = null;
		if ((srcMatchIndex >= 0) && (settings.winnerNextMatch != null) && 
		    (srcMatchIndex < settings.winnerNextMatch.Length))
		{
			i = settings.winnerNextMatch[srcMatchIndex];
			if ((i >= 0) && (i < tournamentMatchInfos.Length))
			{
				nextMatch = tournamentMatchInfos[i];
			}
		}

		if (nextMatch == null)
		{
			return;
		}

		if (srcMatch.teamScore[0] > srcMatch.teamScore[1])
		{
			winner = 0;
		}
		else
		{
			winner = 1;
		}

		i = -1;
		if (string.IsNullOrEmpty(nextMatch.teamId[0]))
		{
			i = 0;
		}
		else if (string.IsNullOrEmpty(nextMatch.teamId[1]))
		{
			i = 1;
		}
		if (i == -1)
		{
			return;
		}

		nextMatch.CopyTeam(srcMatch, winner, i);
		nextMatch.teamScore[i] = -1;
	}


	/// <summary>
	/// Setup the final match.
	/// </summary>
	/// <returns>The final match.</returns>
	static private void SetupFinalMatch()
	{
		if ((tournamentMatchInfos == null) || (tournamentMatchInfos.Length <= 0) || 
		    (tournamentTeamIds == null) || (tournamentTeamIds.Length <= 0))
		{
			return;
		}
		
		int matchIndex = tournamentMatchInfos.Length - 1;
		SsTeamStats[] stats = new SsTeamStats[2];
		SsTournamentMatchInfo matchInfo = tournamentMatchInfos[matchIndex];
		int i;

		if (tournamentType == tournamentTypes.logTournament)
		{
			// Setup the final match between the 2 top teams in the log.
			if ((log != null) && (log.Count > 0))
			{
				for (i = 0; i < 2; i++)
				{
					stats[i] = log[i];
					if (stats[i] != null)
					{
						matchInfo.teamId[i] = stats[i].teamId;
						matchInfo.teamName[i] = stats[i].teamName;
						matchInfo.humanIndex[i] = stats[i].tournamentHumanIndex;

						if (matchInfo.MatchDone == false)
						{
							matchInfo.teamScore[i] = -1;
						}
					}
				}
			}
		}
		
		if (matchInfo.MatchDone)
		{
			// Final match complete
			if (matchInfo.teamScore[0] > matchInfo.teamScore[1])
			{
				winTeamId = matchInfo.teamId[0];
				winTeamScore = matchInfo.teamScore[0];
				
				loseTeamId = matchInfo.teamId[1];
				loseTeamScore = matchInfo.teamScore[1];
			}
			else
			{
				winTeamId = matchInfo.teamId[1];
				winTeamScore = matchInfo.teamScore[1];
				
				loseTeamId = matchInfo.teamId[0];
				loseTeamScore = matchInfo.teamScore[0];
			}
		}
		
		// Final match will be played on a random field
		matchInfo.homeTeam = -1;
		
		SetField();
	}


	/// <summary>
	/// Calculate the team points and log positions.
	/// </summary>
	/// <returns>The team points.</returns>
	static public void CalcTeamPointsAndLog()
	{
		if ((tournamentMatchInfos == null) || (tournamentMatchInfos.Length <= 0) || 
		    (tournamentTeamIds == null) || (tournamentTeamIds.Length <= 0))
		{
			return;
		}
		
		SsTeamStats[] stats = new SsTeamStats[2];
		SsTournamentMatchInfo matchInfo;
		int i, n;
		bool foundAtLeastOneMatch = false;	// Found at least 1 match that was played
		int foundIndex, foundGoalDifference, foundSame;
		bool added;
		
		if (log == null)
		{
			log = new List<SsTeamStats>(tournamentTeamIds.Length);
		}
		else
		{
			log.Clear();
		}
		
		// Clear the team stats
		for (i = 0; i < tournamentTeamIds.Length; i++)
		{
			// stats[0] is used as a temp ref
			stats[0] = SsTeamStats.GetTeamStats(tournamentTeamIds[i]);
			if (stats[0] != null)
			{
				stats[0].InitTournamentLog(tournamentPlayerTeams);
			}
		}
		
		// Loop through the matches
		for (i = 0; i < tournamentMatch; i++)
		{
			// We do Not include the final match
			if ((i >= tournamentMatchInfos.Length) || 
			    (i >= finalMatchIndex))
			{
				break;
			}

			matchInfo = tournamentMatchInfos[i];
			if (matchInfo == null)
			{
				continue;
			}
			
			// Was the match Not yet played?
			if ((matchInfo.teamScore[0] < 0) || (matchInfo.teamScore[1] < 0))
			{
				break;
			}
			
			// Are teams Not yet setup?
			if ((string.IsNullOrEmpty(matchInfo.teamId[0])) || 
			    (string.IsNullOrEmpty(matchInfo.teamId[1])))
			{
				continue;
			}
			
			stats[0] = SsTeamStats.GetTeamStats(matchInfo.teamId[0]);
			stats[1] = SsTeamStats.GetTeamStats(matchInfo.teamId[1]);
			if ((stats[0] != null) && (stats[1] != null))
			{
				foundAtLeastOneMatch = true;
				if (matchInfo.teamScore[0] > matchInfo.teamScore[1])
				{
					// Win: team[0]
					stats[0].tournamentLogWon ++;
					stats[0].tournamentPoints += pointsWin;
					stats[0].tournamentGoalDifference += (matchInfo.teamScore[0] - matchInfo.teamScore[1]);
					stats[0].tournamentLogPlayed ++;
					
					stats[1].tournamentLogLost ++;
					stats[1].tournamentPoints += pointsLose;
					stats[1].tournamentGoalDifference -= (matchInfo.teamScore[0] - matchInfo.teamScore[1]);
					stats[1].tournamentLogPlayed ++;
				}
				else if (matchInfo.teamScore[1] > matchInfo.teamScore[0])
				{
					// Win: team[1]
					stats[1].tournamentLogWon ++;
					stats[1].tournamentPoints += pointsWin;
					stats[1].tournamentGoalDifference += (matchInfo.teamScore[1] - matchInfo.teamScore[0]);
					stats[1].tournamentLogPlayed ++;
					
					stats[0].tournamentLogLost ++;
					stats[0].tournamentPoints += pointsLose;
					stats[0].tournamentGoalDifference -= (matchInfo.teamScore[1] - matchInfo.teamScore[0]);
					stats[0].tournamentLogPlayed ++;
				}
				else if (matchInfo.teamScore[0] == matchInfo.teamScore[1])
				{
					// Draw
					stats[0].tournamentLogDraw ++;
					stats[0].tournamentPoints += pointsDraw;
					stats[0].tournamentLogPlayed ++;
					
					stats[1].tournamentLogDraw ++;
					stats[1].tournamentPoints += pointsDraw;
					stats[1].tournamentLogPlayed ++;
				}
			}
		}


		if (foundAtLeastOneMatch == false)
		{
			InitLog(tournamentTeamIds);
			return;
		}


		// Sort the teams for the log
		for (i = 0; i < tournamentTeamIds.Length; i++)
		{
			// stats[0] is used as a temp ref
			stats[0] = SsTeamStats.GetTeamStats(tournamentTeamIds[i]);
			if (stats[0] != null)
			{
				if (i == 0)
				{
					// First item added to the list
					log.Add(stats[0]);
				}
				else
				{
					added = false;
					foundIndex = -1;
					foundGoalDifference = -1;
					foundSame = -1;
					for (n = 0; n < log.Count; n++)
					{
						// stats[1] is used as a temp ref to compare to
						stats[1] = log[n];
						if (stats[1] != null)
						{
							if (stats[0].tournamentPoints > stats[1].tournamentPoints)
							{
								// More points, so insert above this item
								foundIndex = n;
								break;
							}
							else if ((foundGoalDifference < 0) && 
							         (stats[0].tournamentPoints == stats[1].tournamentPoints) && 
							         (stats[0].tournamentGoalDifference > stats[1].tournamentGoalDifference))
							{
								// Same points, but higher goal difference
								foundGoalDifference = n;
							}
							else if ((foundSame < 0) && 
							         (stats[0].tournamentPoints == stats[1].tournamentPoints) && 
							         (stats[0].tournamentGoalDifference == stats[1].tournamentGoalDifference))
							{
								// Same points and same goal difference
								foundSame = n;
							}
						}
					} //for n


					// Note: We use the smallest one of foundIndex, foundSame and foundGoalDifference.
					
					if ((foundIndex >= 0) && 
					    ((foundSame < 0) || (foundSame > foundIndex)) && 
					    ((foundGoalDifference < 0) || (foundGoalDifference > foundIndex)))
					{
						added = true;
						log.Insert(foundIndex, stats[0]);
					}
					
					if ((added == false) && (foundSame >= 0) && 
					    ((foundGoalDifference < 0) || (foundGoalDifference > foundSame)))
					{
						stats[1] = log[foundSame];
						
						if (IsPlayerTeam(stats[0].teamId))
						{
							// The player gets preference when 2 teams have the same points and goal difference
							added = true;
							log.Insert(foundSame, stats[0]);
						}
						else if (IsPlayerTeam(stats[1].teamId))
						{
							// The player gets preference when 2 teams have the same points and goal difference

							// Add it after the found item
							if (foundSame >= log.Count - 1)
							{
								// Add it to the end of the list
								added = true;
								log.Add(stats[0]);
							}
							else
							{
								added = true;
								log.Insert(foundSame + 1, stats[0]);
							}
						}
						else
						{
							// 2 AI teams, randomly select one
							if (Random.Range(0, 100) < 50)
							{
								// Add it before the found item
								added = true;
								log.Insert(foundSame, stats[0]);
							}
							else
							{
								// Add it after the found item
								if (foundSame >= log.Count - 1)
								{
									// Add it to the end of the list
									added = true;
									log.Add(stats[0]);
								}
								else
								{
									added = true;
									log.Insert(foundSame + 1, stats[0]);
								}
							}
						}
					}
					
					if ((added == false) && (foundGoalDifference >= 0))
					{
						added = true;
						log.Insert(foundGoalDifference, stats[0]);
					}
					
					if (added == false)
					{
						// Add it to the end of the list
						added = true;
						log.Add(stats[0]);
					}
				}
			}
		} //for i


		if ((log != null) && (log.Count > 0))
		{
			for (i = 0; i < log.Count; i++)
			{
				if (log[i] != null)
				{
					log[i].tournamentLogPosition = i;
				}
			}
		}
	}


	/// <summary>
	/// Init the log with the supplied team IDs.
	/// started in a league.
	/// </summary>
	/// <returns>The log.</returns>
	/// <param name="useTeamIds">Use team identifiers.</param>
	static private void InitLog(string[] useTeamIds)
	{
		if ((useTeamIds == null) || (useTeamIds.Length < 0))
		{
			return;
		}
		
		SsTeamStats stats;
		int i;
		
		if (log == null)
		{
			log = new List<SsTeamStats>(useTeamIds.Length);
		}
		else
		{
			log.Clear();
		}
		
		// Add the teams in the order of the IDs
		for (i = 0; i < useTeamIds.Length; i++)
		{
			stats = SsTeamStats.GetTeamStats(useTeamIds[i]);
			if (stats != null)
			{
				stats.InitTournamentLog(tournamentPlayerTeams);
				log.Add(stats);
			}
		}
		
		if ((log != null) && (log.Count > 0))
		{
			for (i = 0; i < log.Count; i++)
			{
				if (log[i] != null)
				{
					log[i].tournamentLogPosition = i;
				}
			}
		}
	}


	/// <summary>
	/// Get the field ID for the current match.
	/// </summary>
	/// <returns>The field.</returns>
	static public string GetField()
	{
		if (string.IsNullOrEmpty(fieldId))
		{
			SetField();
		}
		
		return (fieldId);
	}


	/// <summary>
	/// Set the field ID for the current match.
	/// </summary>
	/// <returns>The field.</returns>
	static public string SetField()
	{
		if ((SsSceneManager.Instance == null) || (SsResourceManager.Instance == null))
		{
			return (null);
		}

		SsTournamentMatchInfo matchInfo;
		SsSceneManager.SsFieldSceneResource fieldRes;
		SsResourceManager.SsTeamResource[] teamRes = new SsResourceManager.SsTeamResource[2];
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		var customSettings = settings != null ? settings.CustomSettings : null;
		
		// Default to the first scene
		fieldRes = SsSceneManager.Instance.GetField(0);
		if (fieldRes != null)
		{
			fieldId = fieldRes.id;
		}
		
		if (tournamentType == tournamentTypes.logTournament || 
		    (customSettings != null && customSettings.UseFieldSelectSequence))
		{
			// Log based tournament, or custom tournament that uses the field sequence
			//---------------------
			matchInfo = GetMatchInfo();
			if (matchInfo != null)
			{
				teamRes[0] = SsResourceManager.Instance.GetTeam(matchInfo.teamId[0]);
				teamRes[1] = SsResourceManager.Instance.GetTeam(matchInfo.teamId[1]);

				fieldRes = null;

				if (IsFinalMatch)
				{
					// Final match

					// Use a random field that does Not belong to one of the two teams
					if ((teamRes[0] != null) && (teamRes[1] != null))
					{
						fieldRes = SsSceneManager.Instance.GetRandomField(teamRes[0].homeFieldId, teamRes[1].homeFieldId);
					}
				}
				else
				{
					if (matchInfo.homeTeam == 0)
					{
						// Left team's field
						if (teamRes[0] != null)
						{
							fieldRes = SsSceneManager.Instance.GetField(teamRes[0].homeFieldId);
						}
					}
					else if (matchInfo.homeTeam == 1)
					{
						// Right team's field
						if (teamRes[1] != null)
						{
							fieldRes = SsSceneManager.Instance.GetField(teamRes[1].homeFieldId);
						}
					}
					else if (settings != null)
					{
						if (settings.fieldSelectSequence == fieldSelectSequences.awayOnly)
						{
							// Use a random field that does Not belong to one of the two teams
							if ((teamRes[0] != null) && (teamRes[1] != null))
							{
								fieldRes = SsSceneManager.Instance.GetRandomField(teamRes[0].homeFieldId, teamRes[1].homeFieldId);
							}
						}
					}
				}

				if (fieldRes == null)
				{
					fieldRes = SsSceneManager.Instance.GetRandomField();
				}

				if (fieldRes != null)
				{
					fieldId = fieldRes.id;
				}
			}
		}

		return (fieldId);
	}

	/// <summary>
	/// Updated the matches' home teams.
	/// </summary>
	private static void UpdateMatchHomeTeams()
	{
		var settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		if (settings == null)
		{
			return;
		}
		
		var customSettings = settings.CustomSettings;
		if (customSettings != null && !customSettings.UseFieldSelectSequence)
		{
			return;
		}
		
		// Variables for keeping track of when a human team played at home.
		// These indices are for the lastHome and playedCount arrays.
		const int leftPlayerArrayIndex = 0;
		const int rightPlayerArrayIndex = 1;
		var lastHome = new[]{false, false};
		var playedCount = new[]{0, 0};
		
		for (int i = 0, max = tournamentMatchInfos.Length; i < max; i++)
		{
			var matchInfo = tournamentMatchInfos[i];
			if (matchInfo == null)
			{
				continue;
			}
			
			// Variables for keeping track of when a human team played at home.
			int? leftPlayer = null;
			int? rightPlayer = null;
			var playerIndex = GetPlayerTeamIndex(matchInfo.teamId[0]);
			if (playerIndex == 0)
			{
				leftPlayer = 0;
			}
			else if (playerIndex == 1)
			{
				rightPlayer = 0;
			}
			playerIndex = GetPlayerTeamIndex(matchInfo.teamId[1]);
			if (playerIndex == 0)
			{
				leftPlayer = 1;
			}
			else if (playerIndex == 1)
			{
				rightPlayer = 1;
			}
			
			if (settings.fieldSelectSequence == fieldSelectSequences.alternateHomeAndAway)
			{
				if (tournamentType != tournamentTypes.custom)
				{
					// Left team always plays at home. Matches has been sorted to alternate teams on left/right sides
					// (as much as possible).
					matchInfo.homeTeam = 0;
				}
				else
				{
					// Custom tournament
					int? selectedPlayer = null;
					int? playerArrayIndex = null;
					if (leftPlayer.HasValue && rightPlayer.HasValue)
					{
						// Both are human teams
						int usePlayer;
						if (playedCount[leftPlayerArrayIndex] == playedCount[rightPlayerArrayIndex])
						{
							// Both teams played the same amount of games, so select a team based on the match index (so
							// that it's always in the same order).
							usePlayer = i % 2 == 0 ? leftPlayer.Value : rightPlayer.Value;
						}
						else
						{
							// Select the team that has played the least amount of games.
							usePlayer = playedCount[leftPlayerArrayIndex] > playedCount[rightPlayerArrayIndex]
								? rightPlayer.Value
								: leftPlayer.Value;
						}

						// If the team played at home the last time then select the other team's field.
						if (usePlayer == leftPlayer.Value)
						{
							matchInfo.homeTeam = lastHome[leftPlayerArrayIndex] ? rightPlayer.Value : leftPlayer.Value;
						}
						else
						{
							matchInfo.homeTeam = lastHome[rightPlayerArrayIndex] ? leftPlayer.Value : rightPlayer.Value;
						}

						lastHome[leftPlayerArrayIndex] = matchInfo.homeTeam == leftPlayer.Value;
						lastHome[rightPlayerArrayIndex] = matchInfo.homeTeam == rightPlayer.Value;

						playedCount[leftPlayerArrayIndex]++;
						playedCount[rightPlayerArrayIndex]++;
					}
					else if (leftPlayer.HasValue)
					{
						selectedPlayer = leftPlayer.Value;
						playerArrayIndex = leftPlayerArrayIndex;
					}
					else if (rightPlayer.HasValue)
					{
						selectedPlayer = rightPlayer.Value;
						playerArrayIndex = rightPlayerArrayIndex;
					}

					if (selectedPlayer.HasValue && playerArrayIndex.HasValue)
					{
						var otherTeam = selectedPlayer.Value == 0 ? 1 : 0;
						matchInfo.homeTeam = lastHome[playerArrayIndex.Value] ? otherTeam : selectedPlayer.Value;
						lastHome[playerArrayIndex.Value] = matchInfo.homeTeam == selectedPlayer.Value;
						playedCount[playerArrayIndex.Value]++;
					}
				}
			}
			else if (settings.fieldSelectSequence == fieldSelectSequences.awayOnly)
			{
				// Will select a random fields that does Not belong to either team
				matchInfo.homeTeam = -1;
			}
			else if (settings.fieldSelectSequence == fieldSelectSequences.homeOnly)
			{
				if (matchInfo.isPlayer[0] && matchInfo.isPlayer[1])
				{
					// Both are players, select a random one to play at home
					matchInfo.homeTeam = Random.Range(0, 2);
				}
				else if (matchInfo.isPlayer[1])
				{
					matchInfo.homeTeam = 1;
				}
				else
				{
					// Default: left team plays at home
					matchInfo.homeTeam = 0;
				}
			}
			else if (settings.fieldSelectSequence == fieldSelectSequences.random)
			{
				matchInfo.homeTeam = -1;
			}
		}
	}

	/// <summary>
	/// Updates the match days properties.
	/// </summary>
	private static void UpdateMatchDays()
	{
		GroupStageMatchDays = -1;
		GroupStageMatchDay = -1;

		if (tournamentMatchInfos == null || tournamentMatchInfos.Length <= 0 || groupsInfo == null || 
		    groupsInfo.Count <= 0)
		{
			return;
		}

		var lastMatchDays = -1;
		for (int i = 0, len = tournamentMatchInfos.Length; i < len; i++)
		{
			var info = tournamentMatchInfos[i];
			if (info == null || info.matchDay < 0)
			{
				continue;
			}

			if (GroupStageMatchDays < info.matchDay)
			{
				GroupStageMatchDays = info.matchDay;
				lastMatchDays = i;
			}
			if (tournamentMatch == i)
			{
				GroupStageMatchDay = info.matchDay;
			}
		}

		// Have we played past the group stage?
		if (GroupStageMatchDay < 0 && GroupStageMatchDays >= 0 && tournamentMatch >= 0 && 
		    tournamentMatch >= lastMatchDays)
		{
			GroupStageMatchDay = GroupStageMatchDays;
		}
	}


#if UNITY_EDITOR
	/// <summary>
	/// DEBUG: Test the tournament system. It generates matches and results.
	/// WARNING: It will end the current active tournament.
	/// </summary>
	/// <returns>The test tournament.</returns>
	/// <param name="id">Tournament ID.</param>
	/// <param name="justEndAiMatches">Indicates if the first few AI matches must end. Otherwise all the matches will end, including 
	/// human team matches. This can be used to create a tournament and play a human match.</param>
	/// <param name="showResultsInConsole">Log the results to the console.</param>
	/// <param name="maxHumanTeams">Max number of human teams (0 to 2).</param>
	static public void DebugTestTournament(string id, bool justEndAiMatches = false,
	                                       bool showResultsInConsole = true, int maxHumanTeams = 2)
	{
		maxHumanTeams = Mathf.Clamp(maxHumanTeams, 0, 2);
		int playerCount = maxHumanTeams > 0 ? Random.Range(1, maxHumanTeams + 1) : 0;
		string[] playerTeamIds = new string[playerCount];
		int i, max;
		SsResourceManager.SsResource teamRes;
		SsTournamentMatchInfo matchInfo;
		string forfeitTeam;

		if (IsTournamentActiveAndNotDone)
		{
			Debug.LogWarning("WARNING: Creating a debug test tournament while a previous tournament is not yet done. " + 
			                 "Previous tournament will be ended. It may cause errors if a tournament match is busy.");
		}


		// Create random human players
		for (i = 0; i < playerTeamIds.Length; i++)
		{
			if (i == 0)
			{
				teamRes = SsResourceManager.Instance.GetRandomTeam(null);
			}
			else
			{
				teamRes = SsResourceManager.Instance.GetRandomTeam(playerTeamIds[0]);
			}

			if (teamRes != null)
			{
				playerTeamIds[i] = teamRes.id;
			}
		}


		// Start the tournament
		if (StartNewTournament(id, playerTeamIds, null) == false)
		{
			return;
		}


		if (justEndAiMatches)
		{
			EndAiMatches(true, null, -1, null, -1, false);
		}
		else
		{
			EndAiMatches(true, null, -1, null, -1, false);

			if (IsTournamentDone == false)
			{
				// Loop through the matches and end them
				max = tournamentMatchInfos.Length;
				for (i = 0; i < max; i++)
				{
					matchInfo = GetMatchInfo(tournamentMatch);
					if (matchInfo == null)
					{
						continue;
					}
					
					// Randomly forfeit matches
					if (Random.Range(0, 100) < 30)
					{
						if (Random.Range(0, 100) < 50)
						{
							forfeitTeam = matchInfo.teamId[0];
						}
						else
						{
							forfeitTeam = matchInfo.teamId[1];
						}
					}
					else
					{
						forfeitTeam = null;
					}
					
					if (matchInfo.needsWinner)
					{
						// Match must have a winner
						if (Random.Range(0, 100) < 50)
						{
							EndMatch(matchInfo.teamId[0], Random.Range(5, 10), 
							         matchInfo.teamId[1], Random.Range(0, 5),
							         forfeitTeam, false, false);
						}
						else
						{
							EndMatch(matchInfo.teamId[0], Random.Range(0, 5), 
							         matchInfo.teamId[1], Random.Range(5, 10),
							         forfeitTeam, false, false);
						}
					}
					else
					{
						EndMatch(matchInfo.teamId[0], Random.Range(0, 10), 
						         matchInfo.teamId[1], Random.Range(0, 10),
						         forfeitTeam, false, false);
					}
					
					EndAiMatches(true, null, -1, null, -1, false);
				}
			}
		}

		if (showResultsInConsole)
		{
			DebugShowTournamentDetails("RESULTS");
		}
	}


	/// <summary>
	/// DEBUG: Show active tournament details.
	/// </summary>
	/// <returns>The show tournament details.</returns>
	static public void DebugShowTournamentDetails(string title = null)
	{
		if (SsSettings.LogInfoToConsole == false)
		{
			return;
		}

		string msg = "TOURNAMENT DETAILS: " + title + "\n";
		int i, r, startNextRound;
		SsTournamentMatchInfo matchInfo;
		SsTeamStats stats;
		SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
		var tournamentManager = (ITournamentManager)Instance;
		var customSettings = settings != null ? settings.CustomSettings : null;
		var worldCup = customSettings != null ? customSettings.CustomController as IWorldCupTournament : null;

		// ID, type
		msg += string.Format("ID: {0}       Type: {1}\n", tournamentId, tournamentType);


		// Players
		if ((tournamentPlayerTeams != null) && (tournamentPlayerTeams.Length > 0))
		{
			msg += "Player teams:\n";
			msg += "   ";
			for (i = 0; i < tournamentPlayerTeams.Length; i++)
			{
				msg += string.Format("{0}", tournamentPlayerTeams[i]);
				if (i < tournamentPlayerTeams.Length - 1)
				{
					msg += ",  ";
				}
			}
			msg += "\n";
		}
		else
		{
			msg += "No player teams.\n";
		}


		// All teams
		if ((tournamentTeamIds != null) && (tournamentTeamIds.Length > 0))
		{
			msg += "All teams:\n";
			msg += "   ";
			for (i = 0; i < tournamentTeamIds.Length; i++)
			{
				msg += string.Format("{0}", tournamentTeamIds[i]);
				if (i < tournamentTeamIds.Length - 1)
				{
					msg += ",  ";
				}
			}
			msg += "\n";
		}
		else
		{
			msg += "No teams.\n";
		}


		// Match
		msg += string.Format("Match index: {0}\n", tournamentMatch + 1);
		if (GroupStageMatchDays > 0 && GroupStageMatchDay >= 0)
		{
			msg += $"Group match day: {GroupStageMatchDay + 1} / {GroupStageMatchDays + 1}\n";	
		}

		if (worldCup != null)
		{
			msg += $"World Cup State: {worldCup.State}\n";
		}

		// Is done
		if (IsTournamentDone)
		{
			msg += "Tournament is done.\n";
		}
		
		// Groups
		if (groupsInfo != null && groupsInfo.Count > 0)
		{
			msg += "\nGroups:\n";
			for (i = 0; i < groupsInfo.Count; i++)
			{
				var info = groupsInfo[i];
				if (info == null || info.TeamIds == null || info.TeamIds.Length <= 0)
				{
					continue;
				}
				msg += $"{info.DisplayName}\n";
				for (int t = 0, lenT = info.TeamIds.Length; t < lenT; t++)
				{
					msg += $"   {info.TeamIds[t]}\n";
				}
			}
			msg += "\n";
		}
		else
		{
			msg += "No groups.\n";
		}

		// Matches
		if ((tournamentMatchInfos != null) && (tournamentMatchInfos.Length > 0))
		{
			msg += "Matches info:\n";

			startNextRound = 0;
			if ((tournamentType == tournamentTypes.singleElimination) && (settings != null) && 
			    (settings.matchesInRound != null) && (settings.matchesInRound.Length > 0))
			{
				startNextRound = settings.matchesInRound[0];
			}

			r = 0;
			string heading = null;
			var lastMatchDay = -1;
			for (i = 0; i < tournamentMatchInfos.Length; i++)
			{
				if ((settings != null) && (settings.matchesPerRound > 0))
				{
					if ((i % settings.matchesPerRound) == 0)
					{
						msg += string.Format("    Rnd {0:D2}\n", r + 1);
						r ++;
					}
				}
				else if ((tournamentType == tournamentTypes.singleElimination) && (settings != null))
				{
					if ((settings.matchesInRound != null) && (r < settings.matchesInRound.Length))
					{
						if ((i == 0) || (i == startNextRound))
						{
							if (i == startNextRound)
							{
								r++;
								startNextRound += settings.matchesInRound[r];
							}
							msg += string.Format("    Rnd {0:D2}\n", r + 1);
						}
					}
				}

				matchInfo = tournamentMatchInfos[i];
				if (matchInfo != null)
				{
					if (!string.IsNullOrEmpty(matchInfo.displayHeading) && 
					    (string.IsNullOrEmpty(heading) || heading != matchInfo.displayHeading))
					{
						heading = matchInfo.displayHeading;
						msg += $" {heading}\n";
					}

					var teamId0 = matchInfo.teamId[0];
					var teamId1 = matchInfo.teamId[1];
					var group0 = tournamentManager.GetGroupIndex(teamId0);
					var group1 = tournamentManager.GetGroupIndex(teamId1);
					var groupString = group0.HasValue && group1.HasValue && group0.Value == group1.Value
						? $"     (group: {group0.Value})"
						: string.Empty;
					var matchDay = matchInfo.matchDay;
					var matchDayString = matchDay >= 0 ? $"     (mDay: {matchDay})" : string.Empty;
					var knockoutMatchIndex = matchInfo.knockoutRoundIndex >= 0 && matchInfo.knockoutRoundMatchIndex >= 0
						? $"     (knRn: {matchInfo.knockoutRoundIndex}-{matchInfo.knockoutRoundMatchIndex})"
						: string.Empty;
					var arrow = tournamentMatch == i ? "   <==" : string.Empty;
					if (lastMatchDay != matchDay && matchDay >= 0)
					{
						lastMatchDay = matchDay;
						msg += $" (Matchday: {matchDay + 1})\n";
					}
					var player0 = GetPlayerTeamIndex(teamId0);
					var player1 = GetPlayerTeamIndex(teamId1);
					var teamName0 = player0 >= 0 ? $"{teamId0} (P{player0 + 1})" : teamId0;
					var teamName1 = player1 >= 0 ? $"{teamId1} (P{player1 + 1})" : teamId1;
					msg += string.Format("      {0:D2} ({1:D2}):    {2,18} {3,3} - {4,-3} {5,-18}    {6}     (needWin:{7}){8}{9}{10}{11}\n",
					                     i + 1,
					                     i,

					                     teamName0, 
					                     matchInfo.teamScore[0], 

					                     matchInfo.teamScore[1], 
					                     teamName1, 

					                     matchInfo.homeTeam,
					                     matchInfo.needsWinner,
					                     groupString,
					                     matchDayString,
					                     knockoutMatchIndex,
					                     arrow);
				}
				else
				{
					msg += "   null\n";
				}
			}
		}
		else
		{
			msg += "No matches.";
		}


		// Log
		if ((log != null) && (log.Count > 0))
		{
			msg += "\nLog:\n";
			for (i = 0; i < log.Count; i++)
			{
				stats = log[i];
				if (stats == null)
				{
					continue;
				}

				//                    Pos     Team    Hmn      P          W          D          L          GD          PTS
				msg += string.Format("{0,-2}  {1,-15} {2,-4}   P:{3,-2}   W:{4,-2}   D:{5,-2}   L:{6,-2}   GD:{7,-2}   PTS:{8,-2}\n",
				                     stats.tournamentLogPosition,
				                     stats.teamId,
				                     (stats.tournamentHumanIndex != -1) ? "[P" + (stats.tournamentHumanIndex + 1) + "]"  : "   ",
				                     stats.tournamentLogPlayed,
				                     stats.tournamentLogWon,
				                     stats.tournamentLogDraw,
				                     stats.tournamentLogLost,
				                     stats.tournamentGoalDifference,
				                     stats.tournamentPoints);
			}
		}
		else
		{
			msg += "\nNo log available.\n";
		}


		Debug.Log(msg);
	}
#endif //UNITY_EDITOR
}
