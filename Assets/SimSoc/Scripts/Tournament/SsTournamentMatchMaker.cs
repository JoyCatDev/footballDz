using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Tournament match maker. Generates the matches for tournaments.
/// </summary>
public class SsTournamentMatchMaker : MonoBehaviour {

	// Enums
	//------
	// Sort states
	private enum sortStates
	{
		unsorted = 0,		// Un-sorted
		temp,				// Temp sorted, still busy
		sorted,				// Sorted, done
	}


	// Class
	//------
	/// <summary>
	/// Match data used for sorting the matches
	/// </summary>
	private class SsMatchSortData
	{
		// REMINDER: Add new variables to CopyFrom(), and team variables to SwapTeams() and ClearTeams().

		public sortStates sortState = sortStates.unsorted;	// Sort state
		public bool hasPlayer;								// Does the match have a human player team?

		public string leftTeam;								// Left team ID (team in left column)
		public string rightTeam;							// Right team ID (team in right column)
		public bool leftIsPlayer;							// Is left team a human player team?
		public bool rightIsPlayer;							// Is right team a human player team?

		// REMINDER: Add new variables to CopyFrom(), and team variables to SwapTeams() and ClearTeams().


		// Methods
		//--------
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="copyFrom">Copy from.</param>
		public SsMatchSortData(SsMatchSortData copyFrom = null)
		{
			if (copyFrom != null)
			{
				CopyFrom(copyFrom);
			}
		}


		/// <summary>
		/// Copy variables from the source.
		/// </summary>
		/// <returns>The from.</returns>
		/// <param name="source">Source.</param>
		public void CopyFrom(SsMatchSortData source)
		{
			sortState = source.sortState;
			hasPlayer = source.hasPlayer;

			leftTeam = source.leftTeam;
			rightTeam = source.rightTeam;
			leftIsPlayer = source.leftIsPlayer;
			rightIsPlayer = source.rightIsPlayer;
		}


		/// <summary>
		/// Swaps the 2 teams from left/right.
		/// </summary>
		/// <returns>The teams.</returns>
		public void SwapTeams()
		{
			string temp = leftTeam;
			bool tempPlayer = leftIsPlayer;

			leftTeam = rightTeam;
			rightTeam = temp;

			leftIsPlayer = rightIsPlayer;
			rightIsPlayer = tempPlayer;
		}


		/// <summary>
		/// Does match contain the specified team?
		/// </summary>
		/// <returns><c>true</c> if this instance has team the specified id; otherwise, <c>false</c>.</returns>
		/// <param name="id">Identifier.</param>
		public bool HasTeam(string id)
		{
			if ((leftTeam == id) || (rightTeam == id))
			{
				return (true);
			}
			return (false);
		}


		/// <summary>
		/// Clears the teams.
		/// </summary>
		/// <returns>The teams.</returns>
		public void ClearTeams()
		{
			hasPlayer = false;

			leftTeam = "";
			rightTeam = "";
			leftIsPlayer = false;
			rightIsPlayer = false;
		}


		/// <summary>
		/// Sets the has player.
		/// </summary>
		/// <returns>The has player.</returns>
		/// <param name="playerTeamIds">Player team identifiers.</param>
		public void SetHasPlayer(string[] playerTeamIds)
		{
			hasPlayer = false;
			leftIsPlayer = false;
			rightIsPlayer = false;

			if ((playerTeamIds != null) && (playerTeamIds.Length > 0))
			{
				for (int i = 0; i < playerTeamIds.Length; i++)
				{
					if (leftTeam == playerTeamIds[i])
					{
						hasPlayer = true;
						leftIsPlayer = true;
					}

					if (rightTeam == playerTeamIds[i])
					{
						hasPlayer = true;
						rightIsPlayer = true;
					}

					if ((leftIsPlayer) && (rightIsPlayer))
					{
						return;
					}
				}
			}
		}
	}



	// Private
	//--------
	static private List<SsMatchSortData> possible;			// All possible matches for 1 leg
	static private SsMatchSortData swapBuffer;				// Buffer for swapping matches



	// Methods
	//--------

	/// <summary>
	/// Creates the matches for a tournament.
	/// </summary>
	/// <returns>The matches.</returns>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	static public bool CreateMatches(SsTournament.tournamentTypes type,
	                                 string[] teamIds, 
	                                 string[] playerTeamIds, 
	                                 SsTournamentSettings settings,
	                                 SsTournamentMatchInfo[] matchInfos)
	{
		bool results = false;

		Init(type, teamIds, playerTeamIds, settings, matchInfos);

		switch (type)
		{
			case SsTournament.tournamentTypes.logTournament:
				// Log based tournament
				results = CreateMatchesLog(type, teamIds, playerTeamIds, settings, matchInfos);
				break;
			case SsTournament.tournamentTypes.singleElimination:
				// Single Elimination
				results = CreateMatchesSingleElimination(type, teamIds, playerTeamIds, settings, matchInfos);
				break;
#if UNITY_EDITOR
			default:
				Debug.LogError($"Unsupported tournament type: {type}");
				break;
#endif
		}

		CleanUp();

		return (results);
	}


	/// <summary>
	/// Init the match maker.
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	static private void Init(SsTournament.tournamentTypes type,
	                         string[] teamIds, 
	                         string[] playerTeamIds, 
	                         SsTournamentSettings settings,
	                         SsTournamentMatchInfo[] matchInfos)
	{
		CleanUp();

		int left, right;
		SsMatchSortData sortData;

		swapBuffer = new SsMatchSortData();


		// Create all the possible matches for 1 leg (i.e. each team will face every other team at least once)
		possible = new List<SsMatchSortData>(settings.minMatches);
		for (left = 0; left < teamIds.Length; left++)
		{
			for (right = left + 1; right < teamIds.Length; right++)
			{
				sortData = new SsMatchSortData();
				sortData.leftTeam = teamIds[left];
				sortData.rightTeam = teamIds[right];
				sortData.SetHasPlayer(playerTeamIds);
				possible.Add(sortData);
			}
		}

	}


	/// <summary>
	/// Clean up the match maker.
	/// </summary>
	/// <returns>The up.</returns>
	static private void CleanUp()
	{
		swapBuffer = null;

		if ((possible != null) && (possible.Count > 0))
		{
			possible.Clear();
		}
		possible = null;
	}


	/// <summary>
	/// Copies the matches.
	/// </summary>
	/// <returns>The matches.</returns>
	/// <param name="source">Source.</param>
	static private List<SsMatchSortData> CopyMatches(List<SsMatchSortData> source)
	{
		if ((source == null) || (source.Count <= 0))
		{
			return (null);
		}
		List<SsMatchSortData> list = new List<SsMatchSortData>(source.Count);
		int i;
		for (i = 0; i < source.Count; i++)
		{
			list.Add(new SsMatchSortData(source[i]));
		}
		return (list);
	}


	/// <summary>
	/// Swap the 2 matches.
	/// </summary>
	/// <returns>The matches.</returns>
	/// <param name="match1">Match1.</param>
	/// <param name="match2">Match2.</param>
	static private void SwapMatches(SsMatchSortData match1, SsMatchSortData match2)
	{
		swapBuffer.CopyFrom(match1);
		match1.CopyFrom(match2);
		match2.CopyFrom(swapBuffer);
	}


	/// <summary>
	/// Randomizes the matches. It creates a copy then randomizes the copy.
	/// </summary>
	/// <returns>The matches.</returns>
	/// <param name="source">Source.</param>
	static private List<SsMatchSortData> RandomizeMatches(List<SsMatchSortData> source)
	{
		if ((source == null) || (source.Count <= 0))
		{
			return (null);
		}
		List<SsMatchSortData> list = CopyMatches(source);
		if (list != null)
		{
			list.Shuffle();
		}
		return (list);
	}


	/// <summary>
	/// Get a random match that contains a human player team.
	/// </summary>
	/// <returns>The random player team match.</returns>
	/// <param name="list">List to select from.</param>
	/// <param name="avoidTeam1">ID of a team to avoid.</param>
	/// <param name="avoidTeam2">ID of a team to avoid.</param>
	/// <param name="onePlayerOnly">Match must have 1 human team only.</param>
	static private SsMatchSortData GetRandomPlayerTeamMatch(List<SsMatchSortData> list,
	                                                        string avoidTeam1 = null, string avoidTeam2 = null,
	                                                        bool onePlayerOnly = false)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return (null);
		}

		int i;
		SsMatchSortData sortData;

		i = 0;
		while (i < list.Count)
		{
			sortData = list[Random.Range(0, list.Count)];
			if ((sortData != null) && (sortData.hasPlayer))
			{
				if ((onePlayerOnly) && (sortData.leftIsPlayer) && (sortData.rightIsPlayer))
				{
					// Avoid match with 2 players
				}
				else if ((string.IsNullOrEmpty(avoidTeam1) == false) && 
				    	 ((sortData.leftTeam == avoidTeam1) || (sortData.rightTeam == avoidTeam1)))
				{
					// Avoid match
				}
				else if ((string.IsNullOrEmpty(avoidTeam2) == false) && 
				         ((sortData.leftTeam == avoidTeam2) || (sortData.rightTeam == avoidTeam2)))
				{
					// Avoid match
				}
				else
				{
					return (sortData);
				}
			}
			i ++;
		}

		// Use first one we find
		for (i = 0; i < list.Count; i++)
		{
			sortData = list[i];
			if ((sortData != null) && (sortData.hasPlayer))
			{
				if ((string.IsNullOrEmpty(avoidTeam1) == false) && 
				    ((sortData.leftTeam == avoidTeam1) || (sortData.rightTeam == avoidTeam1)))
				{
					// Avoid match
				}
				else if ((string.IsNullOrEmpty(avoidTeam2) == false) && 
				         ((sortData.leftTeam == avoidTeam2) || (sortData.rightTeam == avoidTeam2)))
				{
					// Avoid match
				}
				else
				{
					return (sortData);
				}
			}
		}

		return (null);
	}


	/// <summary>
	/// Get a random match that contains the 2 specified teams. If either of the specified teams are null then get 
	/// a match that contains the other team.
	/// </summary>
	/// <returns>The random match with teams.</returns>
	/// <param name="list">List.</param>
	/// <param name="team1">Team 1 ID.</param>
	/// <param name="team2">Team 2 ID.</param>
	/// <param name="unsortedOnly">Check unsorted matches only.</param>
	static private SsMatchSortData GetRandomMatchWithTeams(List<SsMatchSortData> list, 
	                                                       string team1, string team2,
	                                                       bool unsortedOnly)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return (null);
		}
		
		int i, needCount, count;
		SsMatchSortData sortData;

		needCount = 0;
		if (string.IsNullOrEmpty(team1) == false)
		{
			needCount ++;
		}
		if (string.IsNullOrEmpty(team2) == false)
		{
			needCount ++;
		}
		if (needCount <= 0)
		{
			return (null);
		}

		i = 0;
		while (i < list.Count)
		{
			sortData = list[Random.Range(0, list.Count)];
			if ((sortData != null) && 
			    ((unsortedOnly == false) || (sortData.sortState == sortStates.unsorted)))
			{
				count = 0;
				if ((sortData.leftTeam == team1) || (sortData.leftTeam == team2))
				{
					count ++;
				}
				if ((sortData.rightTeam == team1) || (sortData.rightTeam == team2))
				{
					count ++;
				}

				if (count >= needCount)
				{
					return (sortData);
				}
			}

			i ++;
		}


		// Use first one we find
		for (i = 0; i < list.Count; i++)
		{
			sortData = list[i];
			if ((sortData != null) && 
			    ((unsortedOnly == false) || (sortData.sortState == sortStates.unsorted)))
			{
				count = 0;
				if ((sortData.leftTeam == team1) || (sortData.leftTeam == team2))
				{
					count ++;
				}
				if ((sortData.rightTeam == team1) || (sortData.rightTeam == team2))
				{
					count ++;
				}
				
				if (count >= needCount)
				{
					return (sortData);
				}
			}
		}
		
		return (null);
	}


	/// <summary>
	/// Set the sorted state for all matches in the specified list.
	/// </summary>
	/// <returns>The sort states.</returns>
	/// <param name="list">List.</param>
	/// <param name="newState">New state.</param>
	static private void SetSortStates(List<SsMatchSortData> list, sortStates newState,
	                                  bool clearTeams = false)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return;
		}
		int i;
		SsMatchSortData sortData;
		for (i = 0; i < list.Count; i++)
		{
			sortData = list[i];
			if (sortData != null)
			{
				sortData.sortState = newState;
				if (clearTeams)
				{
					sortData.ClearTeams();
				}
			}
		}
	}


	/// <summary>
	/// Set the sorted state for all matches that contain the team, in the specified list.
	/// </summary>
	/// <returns>The sort states.</returns>
	/// <param name="list">List.</param>
	/// <param name="newState">New state.</param>
	static private void SetSortStatesForTeam(List<SsMatchSortData> list, sortStates newState, string teamId, 
	                                         bool clearTeams = false)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return;
		}
		int i;
		SsMatchSortData sortData;
		for (i = 0; i < list.Count; i++)
		{
			sortData = list[i];
			if ((sortData != null) && (sortData.HasTeam(teamId)))
			{
				sortData.sortState = newState;
				if (clearTeams)
				{
					sortData.ClearTeams();
				}
			}
		}
	}


	/// <summary>
	/// Replace the sort state for all the matches in the list.
	/// </summary>
	/// <returns>The sort states.</returns>
	/// <param name="list">List.</param>
	/// <param name="toReplace">To replace.</param>
	/// <param name="replaceWith">Replace with.</param>
	static private void ReplaceSortStates(List<SsMatchSortData> list, sortStates toReplace, sortStates replaceWith,
	                                      bool clearTeams = false)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return;
		}
		int i;
		SsMatchSortData sortData;
		for (i = 0; i < list.Count; i++)
		{
			sortData = list[i];
			if ((sortData != null) && (sortData.sortState == toReplace))
			{
				sortData.sortState = replaceWith;
				if (clearTeams)
				{
					sortData.ClearTeams();
				}
			}
		}
	}


	/// <summary>
	/// Get the first match with the specified sort state.
	/// </summary>
	/// <returns>The first match.</returns>
	/// <param name="list">List.</param>
	/// <param name="state">State.</param>
	/// <param name="startIndex">Start index in the list. -1 = start from beginning.</param>
	/// <param name="endIndex">End index in the list. -1 = end at the end of the list.</param>
	/// <param name="excludeTeamId">Exclude matches that contain this team ID.</param>
	static private int GetFirstMatch(List<SsMatchSortData> list, sortStates state, 
	                                 int startIndex = -1, int endIndex = -1,
	                                 string excludeTeamId = null)
	{
		if ((list == null) || (list.Count <= 0))
		{
			return (-1);
		}
		int i;
		SsMatchSortData sortData;

		if (startIndex < 0)
		{
			startIndex = 0;
		}
		if ((endIndex < 0) || (endIndex > list.Count - 1))
		{
			endIndex = list.Count - 1;
		}

		for (i = startIndex; i <= endIndex; i++)
		{
			sortData = list[i];
			if ((sortData != null) && (sortData.sortState == state))
			{
				if ((string.IsNullOrEmpty(excludeTeamId) == false) && (sortData.HasTeam(excludeTeamId)))
				{
					// Exlude this match
				}
				else
				{
					return (i);
				}
			}
		}

		return (-1);
	}


	/// <summary>
	/// Test if the list of matches contain any of the 2 teams in "match".
	/// </summary>
	/// <returns>The matches contain teams.</returns>
	/// <param name="match">Match.</param>
	/// <param name="matches">Matches.</param>
	/// <param name="start">List start index.</param>
	/// <param name="end">List end index.</param>
	static private bool DoesMatchesContainTeams(SsMatchSortData match, List<SsMatchSortData> matches,
	                                            int start, int end)
	{
		if ((match == null) || (matches == null) || (start < 0) || (end < 0) || (start > end) || 
		    (start >= matches.Count) || (end >= matches.Count))
		{
			return (false);
		}
		
		int i;
		SsMatchSortData sortData;
		for (i = start; i <= end; i++)
		{
			sortData = matches[i];
			if (sortData != null)
			{
				if ((sortData.HasTeam(match.leftTeam)) || (sortData.HasTeam(match.rightTeam)))
				{
					return (true);
				}
			}
		}
		
		return (false);
	}


	/// <summary>
	/// Count the number of consecutive columns in the matches, in which the team appears. The search is 
	/// from the bottom-up. It only counts consecutive left or right columns, and stops when the column changes.
	/// </summary>
	/// <returns>The team columns.</returns>
	/// <param name="matches">Matches.</param>
	/// <param name="startBottomIndex">Start bottom index.</param>
	/// <param name="teamId">Team identifier.</param>
	/// <param name="leftCount">Left count.</param>
	/// <param name="rightCount">Right count.</param>
	static private void CountTeamColumns(List<SsMatchSortData> matches, int startBottomIndex, string teamId, 
	                                     out int leftCount, out int rightCount)
	{
		leftCount = 0;
		rightCount = 0;
		if ((matches == null) || (startBottomIndex < 0) || (startBottomIndex >= matches.Count))
		{
			return;
		}

		SsMatchSortData match;
		int i;

		for (i = startBottomIndex; i >= 0; i--)
		{
			match = matches[i];
			if ((match != null) && (match.HasTeam(teamId)))
			{
				// Did column change?
				if (((leftCount > 0) && (match.rightTeam == teamId)) || 
				    ((rightCount > 0) && (match.leftTeam == teamId)))
				{
					return;
				}

				if (match.leftTeam == teamId)
				{
					leftCount ++;
				}
				else if (match.rightTeam == teamId)
				{
					rightCount ++;
				}
			}
		}
	}



	/// <summary>
	/// Create the matches for a log based tournament.
	/// </summary>
	/// <returns>The create matches.</returns>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	static private bool CreateMatchesLog(SsTournament.tournamentTypes type,
	                                     string[] teamIds, 
	                                     string[] playerTeamIds, 
	                                     SsTournamentSettings settings,
	                                     SsTournamentMatchInfo[] matchInfos)
	{
		// NOTE: The term "leg" refers to a set of matches in which each team faces every other team at least once.
		bool result = true;
		int maxLegs = settings.numFaceEachOther;
		int maxMatches = settings.minMatches * maxLegs;	// Should be same length as matchInfos
		List<SsMatchSortData>[] legMatches = new List<SsMatchSortData>[maxLegs];
		List<SsMatchSortData> allMatches = new List<SsMatchSortData>(maxMatches);
		int i, n, t, max, retry, maxRetry, destMatchStart, destMatchEnd;
		bool ok;
		SsMatchSortData match;
		SsTournamentMatchInfo matchInfo;
		string firstHuman = playerTeamIds[0];

		// Fill list with empty matches
		for (i = 0; i < maxMatches; i++)
		{
			allMatches.Add(new SsMatchSortData());
		}

		// Point the leg matches to the relevant matches in the all matches
		t = 0;
		for (i = 0; i < legMatches.Length; i++)
		{
			legMatches[i] = new List<SsMatchSortData>(settings.minMatches);

			for (n = 0; n < settings.minMatches; n++)
			{
				legMatches[i].Add(allMatches[t]);
				t ++;
			}
		}


		// Step: Create the matches for each leg
		//--------------------------------------
		maxRetry = 20;
		for (i = 0; i < legMatches.Length; i++)
		{
			for (retry = 0; retry < maxRetry; retry++)
			{
				ok = LogCreateLegMatches(type, teamIds, playerTeamIds, settings, matchInfos, 
				                         legMatches, i);
				if (ok == false)
				{
					if (retry >= maxRetry - 1)
					{
#if UNITY_EDITOR
						Debug.LogWarning("WARNING: Failed to create leg matches. Retried:" + retry);
#endif //UNITY_EDITOR
						result = false;
						break;
					}
					else
					{
						// Reset the matches and try again
						SetSortStates(legMatches[i], sortStates.unsorted, true);
					}
				}
				else
				{
					break;
				}
			}
		}


		if (result)
		{
			// Get the first human team in the first match (should be the left team)
			match = allMatches[0];
			if (match.leftTeam == playerTeamIds[0])
			{
				firstHuman = playerTeamIds[0];
			}
			else if (match.leftTeam == playerTeamIds[1])
			{
				firstHuman = playerTeamIds[1];
			}
			else if (match.rightTeam == playerTeamIds[0])
			{
				firstHuman = playerTeamIds[0];
			}
			else if (match.rightTeam == playerTeamIds[1])
			{
				firstHuman = playerTeamIds[1];
			}


			// Step: For each round, move a match to the first row which contains teams not in the previous match
			//---------------------------------------------------------------------------------------------------
			destMatchStart = 0;
			destMatchEnd = destMatchStart + settings.matchesPerRound - 1;
			max = allMatches.Count / settings.matchesPerRound;
			for (i = 0; i < max; i++)
			{
				// This is Not a critical step, so it is ok if it fails
				LogSetRoundFirstMatch(allMatches, destMatchStart, destMatchEnd);

				destMatchStart = destMatchEnd + 1;
				destMatchEnd += settings.matchesPerRound;
			}


			// Set all as unsorted
			SetSortStates(allMatches, sortStates.unsorted);


			// Step: Alternate the columns of the first human team (that was in the first match)
			//----------------------------------------------------------------------------------
			destMatchStart = 0;
			destMatchEnd = destMatchStart + settings.matchesPerRound - 1;
			max = allMatches.Count / settings.matchesPerRound;
			for (i = 0; i < max; i++)
			{
				// This is Not a critical step, so it is ok if it fails
				LogRoundAlternateTeamColumn(allMatches, destMatchStart, destMatchEnd, firstHuman, true);
				
				destMatchStart = destMatchEnd + 1;
				destMatchEnd += settings.matchesPerRound;
			}


			// Step: Alternate all team columns for the unsorted matches
			//----------------------------------------------------------
			max = allMatches.Count;
			for (i = 0; i < max; i++)
			{
				// This is Not a critical step, so it is ok if it fails
				AlternateTeamColumn(allMatches, i, true, true);
			}
		}



		// Finally
		//--------
		// If success then fill matchInfos with the matches
		if (result)
		{
			for (i = 0; i < allMatches.Count; i++)
			{
				match = allMatches[i];
				matchInfo = matchInfos[i];

				if ((match != null) && (matchInfo != null))
				{
					matchInfo.teamId[0] = match.leftTeam;
					matchInfo.teamId[1] = match.rightTeam;

					matchInfo.isPlayer[0] = match.leftIsPlayer;
					matchInfo.isPlayer[1] = match.rightIsPlayer;
				}
			}

#if UNITY_EDITOR
			DebugDisplayMatches("All Matches", allMatches, settings.matchesPerRound);
#endif //UNITY_EDITOR
		}


		// Clean up
		for (i = 0; i < legMatches.Length; i++)
		{
			if ((legMatches[i] != null) && (legMatches[i].Count > 0))
			{
				legMatches[i].Clear();
			}
			legMatches[i] = null;
		}
		legMatches = null;

		if ((allMatches != null) && (allMatches.Count > 0))
		{
			allMatches.Clear();
		}
		allMatches = null;

		return (result);
	}





	/// <summary>
	/// Create the matches for the specified leg, for a log based tournament.
	/// NOTE: The term "leg" refers to a set of matches in which each team faces every other team at least once.
	/// </summary>
	/// <returns>The create leg matches.</returns>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	/// <param name="legMatches">Leg matches.</param>
	/// <param name="legIndex">Leg index.</param>
	static private bool LogCreateLegMatches(SsTournament.tournamentTypes type,
	                                        string[] teamIds, 
	                                        string[] playerTeamIds, 
	                                        SsTournamentSettings settings,
	                                        SsTournamentMatchInfo[] matchInfos,
	                                        List<SsMatchSortData>[] legMatches, int legIndex)
	{
		List<SsMatchSortData> srcMatches = RandomizeMatches(possible);	// Randomize all possible matches, makes a copy
		List<SsMatchSortData> destMatches = legMatches[legIndex];
		SsMatchSortData match;
		int i, srcIndex, destMatchStart, destMatchEnd;
		bool ok;

		// Reset the sorted states
		SetSortStates(srcMatches, sortStates.unsorted);


		// Step: Randomly select a match with a human team and put it first
		//-----------------------------------------------------------------
		match = GetRandomPlayerTeamMatch(srcMatches);
		if (match == null)
		{
			srcMatches.Clear();
			return (false);
		}

		// First match can be marked as sorted, we are Not going to move it again
		match.sortState = sortStates.sorted;
		destMatches[0].CopyFrom(match);


		// Step: Sort matches to fill dest rounds with unique matches.
		//------------------------------------------------------------
		destMatchStart = 0;
		destMatchEnd = destMatchStart + settings.matchesPerRound - 1;
		for (i = 0; i < settings.minRounds; i++)
		{
			// Get the first unused match
			srcIndex = GetFirstMatch(srcMatches, sortStates.unsorted);
			if (srcIndex < 0)
			{
				srcMatches.Clear();
				return (false);
			}

			// Fill the round with matches
			ok = LogFillRound(srcMatches, srcIndex, destMatches, destMatchStart, destMatchEnd);
			if (ok == false)
			{
				srcMatches.Clear();
				return (false);
			}

			destMatchStart = destMatchEnd + 1;
			destMatchEnd += settings.matchesPerRound;
		}

		return (true);
	}


	/// <summary>
	/// Fill the round with matches, for a log based tournament. It takes matches from the source list and copies them to the 
	/// destination list.
	/// </summary>
	/// <returns>The fill round.</returns>
	/// <param name="srcList">Source list.</param>
	/// <param name="srcIndex">Source index.</param>
	/// <param name="destList">Destination list.</param>
	/// <param name="destMatchStart">Destination start index.</param>
	/// <param name="destMatchEnd">Destination end index.</param>
	static private bool LogFillRound(List<SsMatchSortData> srcList, int srcIndex,
	                                 List<SsMatchSortData> destList, int destMatchStart, int destMatchEnd)
	{
		int i, s, d;
		bool filled;
		SsMatchSortData srcMatch, destMatch;

		s = srcIndex;

		filled = false;
		while (filled == false)
		{
			for (i = s; i < srcList.Count; i++)
			{
				srcMatch = srcList[i];
				if (srcMatch.sortState == sortStates.sorted)
				{
					continue;
				}

				if (DoesMatchesContainTeams(srcMatch, destList, destMatchStart, destMatchEnd) == false)
				{
					d = GetFirstMatch(destList, sortStates.unsorted, destMatchStart, destMatchEnd);
					if ((d >= 0) && (d < destList.Count))
					{
						destMatch = destList[d];
						destMatch.CopyFrom(srcMatch);
						destMatch.sortState = sortStates.temp;

						srcMatch.sortState = sortStates.temp;

						// Is the round filled?
						d = GetFirstMatch(destList, sortStates.unsorted, destMatchStart, destMatchEnd);
						if (d < 0)
						{
							filled = true;

							ReplaceSortStates(srcList, sortStates.temp, sortStates.sorted);
							ReplaceSortStates(destList, sortStates.temp, sortStates.sorted);
							break;
						}
					}
				}
			}

			if (filled)
			{
				break;
			}

			s ++;
			if (s >= srcList.Count)
			{
				break;
			}

			ReplaceSortStates(srcList, sortStates.temp, sortStates.unsorted);
			ReplaceSortStates(destList, sortStates.temp, sortStates.unsorted, true);
		}


		// Clear the temporary used matches
		ReplaceSortStates(srcList, sortStates.temp, sortStates.unsorted);
		ReplaceSortStates(destList, sortStates.temp, sortStates.unsorted, true);

		return (filled);
	}



	/// <summary>
	/// Set the round's first match, for a log based tournament.
	/// </summary>
	/// <returns>The set round first match.</returns>
	/// <param name="destList">Destination list.</param>
	/// <param name="destMatchStart">Destination match start.</param>
	/// <param name="destMatchEnd">Destination match end.</param>
	static private bool LogSetRoundFirstMatch(List<SsMatchSortData> destList, int destMatchStart, int destMatchEnd)
	{
		SsMatchSortData lastMatch;
		SsMatchSortData match;
		int i, foundMatch;

		// Is this the first match in the first round?
		if (destMatchStart == 0)
		{
			// No matches before this, so no need to do anything
			return (true);
		}

		lastMatch = destList[destMatchStart - 1];
		if (lastMatch == null)
		{
			return(false);
		}

		// Find match which does Not contain teams in the previous match
		foundMatch = -1;
		for (i = destMatchStart; i <= destMatchEnd; i++)
		{
			match = destList[i];
			if ((match != null) && 
			    (lastMatch.HasTeam(match.leftTeam) == false) && 
			    (lastMatch.HasTeam(match.rightTeam) == false))
			{
				foundMatch = i;
				break;
			}
		}

		if (foundMatch == -1)
		{
			return (false);
		}

		if (foundMatch != destMatchStart)
		{
			// Move the match to the first position
			SwapMatches(destList[destMatchStart], destList[foundMatch]);
		}

		return (true);
	}


	/// <summary>
	/// Alternate the team's columns in the round, for a log based tournament.
	/// </summary>
	/// <returns>The round alternate team column.</returns>
	/// <param name="destList">Destination list.</param>
	/// <param name="destMatchStart">Destination match start.</param>
	/// <param name="destMatchEnd">Destination match end.</param>
	/// <param name="teamId">Team identifier.</param>
	static private bool LogRoundAlternateTeamColumn(List<SsMatchSortData> destList, int destMatchStart, int destMatchEnd,
	                                                string teamId, bool markAsSorted)
	{
		SsMatchSortData match;
		int i, foundMatch, leftCount, rightCount;
		
		// Is this the first match in the first round?
		if (destMatchStart == 0)
		{
			// No matches before this, so no need to alternate

			if (markAsSorted)
			{
				// Mark the match as sorted
				match = destList[0];
				match.sortState = sortStates.sorted;
			}

			return (true);
		}
		
		// Find match which contains the team
		foundMatch = -1;
		for (i = destMatchStart; i <= destMatchEnd; i++)
		{
			match = destList[i];
			if ((match != null) && (match.HasTeam(teamId)))
			{
				foundMatch = i;
				break;
			}
		}
		
		if (foundMatch == -1)
		{
			return (false);
		}
		
		CountTeamColumns(destList, foundMatch - 1, teamId, out leftCount, out rightCount);

		match = destList[foundMatch];
		if (((match.leftTeam == teamId) && (leftCount > 0)) || 
		    ((match.rightTeam == teamId) && (rightCount > 0)))
		{
			match.SwapTeams();
		}

		if (markAsSorted)
		{
			// Mark the match as sorted
			match.sortState = sortStates.sorted;
		}

		return (true);
	}



	/// <summary>
	/// Create the matches for a single elimination tournament.
	/// </summary>
	/// <returns>The create matches.</returns>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	static private bool CreateMatchesSingleElimination(SsTournament.tournamentTypes type,
	                                                   string[] teamIds, 
	                                                   string[] playerTeamIds, 
	                                                   SsTournamentSettings settings,
	                                                   SsTournamentMatchInfo[] matchInfos)
	{
		bool result = true;
		int maxMatches = settings.maxMatches;
		List<SsMatchSortData> allMatches = new List<SsMatchSortData>(maxMatches);
		int i;
		SsMatchSortData match;
		SsTournamentMatchInfo matchInfo;
		bool ok;


		// Fill list with empty matches
		for (i = 0; i < maxMatches; i++)
		{
			allMatches.Add(new SsMatchSortData());
		}


		// Step: Fill the first round of matches
		//--------------------------------------
		ok = SingleEliminationCreateFirstRound(type, teamIds, playerTeamIds, settings, matchInfos, allMatches);
		if (ok == false)
		{
			result = false;
		}


		// Finally
		//--------
		// If success then fill matchInfos with the matches
		if (result)
		{
			for (i = 0; i < allMatches.Count; i++)
			{
				match = allMatches[i];
				matchInfo = matchInfos[i];
				
				if ((match != null) && (matchInfo != null))
				{
					matchInfo.teamId[0] = match.leftTeam;
					matchInfo.teamId[1] = match.rightTeam;
					
					matchInfo.isPlayer[0] = match.leftIsPlayer;
					matchInfo.isPlayer[1] = match.rightIsPlayer;
				}
			}
			
#if UNITY_EDITOR
			DebugDisplayMatches("All Matches", allMatches);
#endif //UNITY_EDITOR
		}
#if UNITY_EDITOR
		else
		{
			DebugDisplayMatches("Failed Matches", allMatches);
		}
#endif //UNITY_EDITOR

		
		// Clean up
		if ((allMatches != null) && (allMatches.Count > 0))
		{
			allMatches.Clear();
		}
		allMatches = null;
		
		return (result);
	}


	/// <summary>
	/// Replace the team ID in the array with the specified ID.
	/// </summary>
	/// <returns>The team identifier.</returns>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="replaceId">Replace identifier.</param>
	/// <param name="replaceWithId">Replace with identifier.</param>
	static private void ReplaceTeamId(string[] teamIds, string replaceId, string replaceWithId)
	{
		int i;
		for (i = 0; i < teamIds.Length; i++)
		{
			if (teamIds[i] == replaceId)
			{
				teamIds[i] = replaceWithId;
			}
		}
	}


	/// <summary>
	/// Get a random ID from the array.
	/// </summary>
	/// <returns>The random team identifier.</returns>
	/// <param name="teamIds">Team identifiers.</param>
	static private string GetRandomTeamId(string[] teamIds, string excludeId = null)
	{
		if ((teamIds == null) || (teamIds.Length <= 0))
		{
			return (null);
		}
		
		int i;
		string id;

		i = 0;
		while (i < teamIds.Length)
		{
			id = teamIds[Random.Range(0, teamIds.Length)];
			if ((string.IsNullOrEmpty(id) == false) && 
			    (id != excludeId))
			{
				return (id);
			}
			i ++;
		}
		
		// Use first one we find
		for (i = 0; i < teamIds.Length; i++)
		{
			id = teamIds[i];
			if ((string.IsNullOrEmpty(id) == false) && 
			    (id != excludeId))
			{
				return (id);
			}
		}
		
		return (null);
	}



	/// <summary>
	/// Singles the elimination create first round.
	/// </summary>
	/// <returns>The elimination create first round.</returns>
	/// <param name="type">Type.</param>
	/// <param name="teamIds">Team identifiers.</param>
	/// <param name="playerTeamIds">Player team identifiers.</param>
	/// <param name="settings">Settings.</param>
	/// <param name="matchInfos">Match infos.</param>
	/// <param name="allMatches">All matches.</param>
	static private bool SingleEliminationCreateFirstRound(SsTournament.tournamentTypes type,
	                                                      string[] teamIds, 
	                                                      string[] playerTeamIds, 
	                                                      SsTournamentSettings settings,
	                                                      SsTournamentMatchInfo[] matchInfos,
	                                                      List<SsMatchSortData> allMatches)
	{
		List<SsMatchSortData> destMatches = allMatches;
		SsMatchSortData match;
		int i, groupIndex;
		bool humansCanBeInSameGroup, humansCanBeInSameMatch;
		string[] srcTeamIds;
		string unusedPlayerTeamId, leftTeam, rightTeam;

		// Copy the teams
		srcTeamIds = new string[teamIds.Length];
		for (i = 0; i < srcTeamIds.Length; i++)
		{
			srcTeamIds[i] = teamIds[i];
		}

		humansCanBeInSameGroup = false;
		humansCanBeInSameMatch = false;
		if ((playerTeamIds.Length == 2) && 
		    (string.IsNullOrEmpty(playerTeamIds[1]) == false))
		{
			humansCanBeInSameGroup = (Random.Range(0.0f, 100.0f) < settings.humansInSameGroupChance);
			humansCanBeInSameMatch = ((Random.Range(0.0f, 100.0f) < settings.humansInSameMatchChance) && (humansCanBeInSameGroup));
		}

		unusedPlayerTeamId = null;


		// Randomly select a human player for the first match
		if ((playerTeamIds.Length == 2) && 
		    (string.IsNullOrEmpty(playerTeamIds[1]) == false))
		{
			i = Random.Range(0, 2);
			leftTeam = playerTeamIds[i];
			if (i == 0)
			{
				unusedPlayerTeamId = playerTeamIds[1];
			}
			else
			{
				unusedPlayerTeamId = playerTeamIds[0];
			}
		}
		else
		{
			leftTeam = playerTeamIds[0];
		}
		ReplaceTeamId(srcTeamIds, leftTeam, "");

		if (humansCanBeInSameMatch)
		{
			// Add the second human to the match
			rightTeam = unusedPlayerTeamId;
			ReplaceTeamId(srcTeamIds, rightTeam, "");
			unusedPlayerTeamId = null;
		}
		else
		{
			// Select a random AI team
			rightTeam = GetRandomTeamId(srcTeamIds, unusedPlayerTeamId);
			ReplaceTeamId(srcTeamIds, rightTeam, "");
		}

		match = destMatches[0];
		match.leftTeam = leftTeam;
		match.rightTeam = rightTeam;
		match.SetHasPlayer(playerTeamIds);

		// Fill remaining matches with teams
		groupIndex = 0;
		for (i = 1; i < settings.matchesInFirstRound; i++)
		{
			if ((i % 2) == 0)
			{
				groupIndex ++;
			}

			match = destMatches[i];
			leftTeam = null;
			rightTeam = null;

			if ((i == 1) && (humansCanBeInSameGroup) && (humansCanBeInSameMatch == false) && 
			    (string.IsNullOrEmpty(unusedPlayerTeamId) == false))
			{
				// Add the other human team to match 2
				leftTeam = unusedPlayerTeamId;
				unusedPlayerTeamId = null;
				ReplaceTeamId(srcTeamIds, leftTeam, "");

				// Select a random AI team
				rightTeam = GetRandomTeamId(srcTeamIds);
				ReplaceTeamId(srcTeamIds, rightTeam, "");
			}
			else if ((groupIndex == settings.groupsInFirstRound - 1) && 
			         (string.IsNullOrEmpty(unusedPlayerTeamId) == false))
			{
				// Add the other human team to the last group
				leftTeam = unusedPlayerTeamId;
				unusedPlayerTeamId = null;
				ReplaceTeamId(srcTeamIds, leftTeam, "");
				
				// Select a random AI team
				rightTeam = GetRandomTeamId(srcTeamIds);
				ReplaceTeamId(srcTeamIds, rightTeam, "");
			}
			else if ((groupIndex < settings.groupsInFirstRound - 1) && 
			         (string.IsNullOrEmpty(unusedPlayerTeamId) == false))
			{
				// Not the last group, so do Not add the other human team.
				// We put the other human team in the last group to increase the chance of the humans facing each other in the final.

				// Select random teams
				leftTeam = GetRandomTeamId(srcTeamIds, unusedPlayerTeamId);
				ReplaceTeamId(srcTeamIds, leftTeam, "");

				rightTeam = GetRandomTeamId(srcTeamIds, unusedPlayerTeamId);
				ReplaceTeamId(srcTeamIds, rightTeam, "");
			}
			else
			{
				// Select random teams
				leftTeam = GetRandomTeamId(srcTeamIds);
				ReplaceTeamId(srcTeamIds, leftTeam, "");
				
				rightTeam = GetRandomTeamId(srcTeamIds);
				ReplaceTeamId(srcTeamIds, rightTeam, "");
			}
			
			if ((match != null) && 
			    (string.IsNullOrEmpty(leftTeam) == false) && 
			    (string.IsNullOrEmpty(rightTeam) == false))
			{
				match.leftTeam = leftTeam;
				match.rightTeam = rightTeam;
				match.SetHasPlayer(playerTeamIds);
			}
			else
			{
				return (false);
			}
		}

		
#if UNITY_EDITOR
		DebugDisplayMatches("Elimination Round 1 Matches", destMatches, -1, 2);
#endif //UNITY_EDITOR
		
		return (true);
	}


	/// <summary>
	/// Alternate the teams' columns in the match, if possible.
	/// Alternate the team with the most consecutive columns in the same column.
	/// </summary>
	/// <returns>The team column.</returns>
	/// <param name="destList">Destination list.</param>
	/// <param name="index">Index.</param>
	/// <param name="unsortedOnly">Unsorted only.</param>
	/// <param name="markAsSorted">Mark as sorted.</param>
	static private bool AlternateTeamColumn(List<SsMatchSortData> destList, int index, bool unsortedOnly, bool markAsSorted)
	{
		SsMatchSortData match = destList[index];
		int leftTeamLeftCount, leftTeamRightCount, rightTeamLeftCount, rightTeamRightCount;
		bool swap;

		if (match == null)
		{
			return (false);
		}

		if ((unsortedOnly) && (match.sortState != sortStates.unsorted))
		{
			// Already sorted
			return (true);
		}

		CountTeamColumns(destList, index - 1, match.leftTeam, out leftTeamLeftCount, out leftTeamRightCount);
		CountTeamColumns(destList, index - 1, match.rightTeam, out rightTeamLeftCount, out rightTeamRightCount);

		swap = false;

		// Determine if the teams must be swapped
		if ((leftTeamLeftCount <= 0) && (leftTeamRightCount <= 0) && 
		    (rightTeamLeftCount <= 0) && (rightTeamRightCount <= 0))
		{
			// This is the first match that either team appears in, no need to swap them.
		}
		else if ((leftTeamLeftCount <= 0) && (rightTeamRightCount <= 0))
		{
			// Both teams are in new columns, no need to swap them.
		}
		else
		{
			if ((leftTeamLeftCount > 0) && (rightTeamRightCount > 0))
			{
				// Both teams in the same columns
				if (leftTeamLeftCount == rightTeamRightCount)
				{
					// They have been in the same columns for the same amount of matches
					swap = true;
				}
				else if (leftTeamLeftCount > rightTeamRightCount)
				{
					// Left team has been in column for longer than right team
					swap = true;
				}
				else if (rightTeamRightCount > leftTeamLeftCount)
				{
					// Right team has been in column for longer than left team
					swap = true;
				}
			}
			else if (leftTeamLeftCount > 0)
			{
				// Left team in the same column (right team was in left column in previous matches)
				if (leftTeamLeftCount >= rightTeamLeftCount + 1)
				{
					// Even if right team moves back to left columns, it will have been in left column for less/equal than left team
					swap = true;
				}
			}
			else if (rightTeamRightCount > 0)
			{
				// Right team in the same column (left team was in right column in previous matches)
				if (rightTeamRightCount >= leftTeamRightCount + 1)
				{
					// Even if left team moves back to right columns, it will have been in right column for less/equal than right team
					swap = true;
				}
			}
		}


		if (swap)
		{
			match.SwapTeams();
		}

		if (markAsSorted)
		{
			// Mark the match as sorted
			match.sortState = sortStates.sorted;
		}

		return (true);
	}


#if UNITY_EDITOR
	/// <summary>
	/// DEBUG: Display the matches in the list, to the console.
	/// </summary>
	/// <returns>The display matches.</returns>
	/// <param name="title">Title.</param>
	/// <param name="matches">List of matches.</param>
	static private void DebugDisplayMatches(string title, List<SsMatchSortData> matches, int matchesPerRound = -1,
	                                        int matchesPerGroup = -1)
	{
		if (SsSettings.LogInfoToConsole == false)
		{
			return;
		}

		if (matches == null)
		{
			return;
		}

		string msg = "TOURNAMENT MATCH MAKER: " + title + "\n";
		int i, round, group;
		SsMatchSortData sortData;

		round = 0;
		group = 0;
		for (i = 0; i < matches.Count; i++)
		{
			if ((matchesPerRound > 0) && ((i % matchesPerRound) == 0))
			{
				msg += string.Format("  Round: {0}\n", round + 1);
				round ++;
			}

			if ((matchesPerGroup > 0) && ((i % matchesPerGroup) == 0))
			{
				msg += string.Format("  Group: {0}\n", group + 1);
				group ++;
			}

			sortData = matches[i];
			if (sortData == null)
			{
				continue;
			}
			msg += string.Format("    {0:D2}: {1}{2} - {3}{4}          [{5}]\n", 
			                     i + 1, 
			                     sortData.leftTeam, 
			                     sortData.leftIsPlayer ? "*" : " ",
			                     sortData.rightIsPlayer ? "*" : " ",
			                     sortData.rightTeam,
			                     sortData.sortState);
		}

		Debug.Log(msg);
	}
#endif //UNITY_EDITOR

}
