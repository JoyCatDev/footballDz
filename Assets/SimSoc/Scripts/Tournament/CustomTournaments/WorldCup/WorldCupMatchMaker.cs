using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimSoc
{
	/// <inheritdoc cref="ICustomTournamentMatchMaker"/>
	/// <remarks>
	/// Creates the matches for a world cup.
	/// </remarks>
	public class WorldCupMatchMaker : ICustomTournamentMatchMaker
	{
		/// <summary>
		/// Container for team info.
		/// </summary>
		protected class TeamInfo
		{
			protected readonly List<TeamInfo> _opponents = new List<TeamInfo>();
		
			public string TeamId { get; }
			public bool IsHumanTeam { get; }
			public int GroupIndex { get; }
		
			/// <summary>
			/// Number of matches the team was added to.
			/// </summary>
			/// <remarks>
			/// This is used as a temp variable while we're building the group stage matches.
			/// </remarks>
			public int MatchesCount { get; private set; }

			public TeamInfo(string teamId, bool isHumanTeam, int groupIndex)
			{
				TeamId = teamId;
				IsHumanTeam = isHumanTeam;
				GroupIndex = groupIndex;
			}

			public void AddedMatch(TeamInfo opponent)
			{
				if (!_opponents.Contains(opponent))
				{
					_opponents.Add(opponent);
				}
				MatchesCount++;
			}

			public bool HasOpponent(TeamInfo opponent)
			{
				return _opponents.Contains(opponent);
			}
		}

		/// <summary>
		/// Container for match info.
		/// </summary>
		/// <remarks>
		/// This is used as temp objects when creating the list of matches.
		/// </remarks>
		protected class TempMatchInfo
		{
			public TeamInfo[] TeamsInfo { get; } = new TeamInfo[2];
			public int MatchDay { get; set; } = -1;

			public TempMatchInfo(TeamInfo team0, TeamInfo team1)
			{
				TeamsInfo[0] = team0;
				TeamsInfo[1] = team1;
			}
		}

		protected readonly ITournamentManager _tournamentManager;
		protected WorldCupTournamentSettings _worldCupSettings;
		protected IWorldCupTournament _worldCup;
		protected IWorldCupTeamStatsController _statsController;
		protected readonly Dictionary<string, TeamInfo> _teamsInfo = new Dictionary<string, TeamInfo>();
		protected readonly Dictionary<int, List<TempMatchInfo>> _tempGroupMatches =
			new Dictionary<int, List<TempMatchInfo>>();
		protected readonly Dictionary<int, List<TempMatchInfo>> _tempDayMatches =
			new Dictionary<int, List<TempMatchInfo>>();
		// Lists to re-use.
		protected readonly List<TournamentTeamStats> _reuseTeamStats = new List<TournamentTeamStats>();
		protected readonly List<SsTournamentMatchInfo> _reuseMatchesInfo = new List<SsTournamentMatchInfo>();
	
		/// <summary>
		/// Initialized a new tournament, or a tournament that was loaded.
		/// </summary>
		protected bool _initializedTournament;

		public WorldCupMatchMaker(ITournamentManager tournamentManager)
		{
			_tournamentManager = tournamentManager;
		}

		/// <inheritdoc/>
		public virtual bool CreateMatches()
		{
			InitializeTournament();
			return CreateGroupStageMatches();
		}

		/// <inheritdoc/>
		public virtual void OnNewOrLoaded(bool wasLoaded)
		{
			InitializeTournament();
		}

		/// <inheritdoc/>
		public virtual bool OnMatchEnded()
		{
			if (_worldCup == null || _statsController == null)
			{
				return false;
			}
		
			var state = _worldCup.GetStateForMatch(_worldCup.MatchIndex);
			if (!state.HasValue)
			{
				return true;
			}
		
			switch (state.Value)
			{
				case WorldCupState.NotStarted:
				case WorldCupState.GroupStage:
				case WorldCupState.Done:
					break;
				case WorldCupState.RounfOf32:
				case WorldCupState.RoundOf16:
				case WorldCupState.QuarterFinals:
				case WorldCupState.SemiFinals:
				case WorldCupState.ThirdPlacePlayOff:
				case WorldCupState.Final:
					var oldState = _worldCup.GetStateForMatch(_worldCup.MatchIndex - 1);
					if (!oldState.HasValue)
					{
						return false;
					}

					var movedToANewState = oldState.Value != state.Value;
					if (!movedToANewState)
					{
						// Create partial matches for the next state (i.e. move the winners to the next state's matches).
						var nextState = _worldCup.GetNextKnockoutState(state.Value);
						if (nextState.HasValue)
						{
							if (!CreateKnockoutMatches(oldState.Value, nextState.Value, true))
							{
								return false;
							}
						}
					
						// If busy with semi-finals then also update the final match.
						if (state.Value == WorldCupState.SemiFinals && 
						    (nextState == null || nextState.Value != WorldCupState.Final))
						{
							if (!CreateKnockoutMatches(oldState.Value, WorldCupState.Final, true))
							{
								return false;
							}
						}
					
						break;
					}

					// Create matches for the new state.
					if (!CreateKnockoutMatches(oldState.Value, state.Value, false))
					{
						return false;
					}

					// When we reach the third place play-off then we create the final match as well.
					if (state.Value == WorldCupState.ThirdPlacePlayOff && 
					    !CreateKnockoutMatches(oldState.Value, WorldCupState.Final, false))
					{
						return false;
					}
					break;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				default:
					Debug.LogError($"Unsupported world cup state: {state.Value}");
					break;
#endif
			}

			return true;
		}

		/// <inheritdoc/>
		public virtual void OnTournamentEnded()
		{
			_initializedTournament = false;
			_teamsInfo.Clear();
		}

		/// <summary>
		/// Initialize a new tournament, or a tournament that was loaded.
		/// </summary>
		protected virtual void InitializeTournament()
		{
			if (_initializedTournament)
			{
				return;
			}

			_initializedTournament = true;
		
			var settings = _tournamentManager.Settings;
			_worldCupSettings = settings != null ? (WorldCupTournamentSettings)settings.CustomSettings : null;
			_worldCup = _worldCupSettings != null ? (IWorldCupTournament)_worldCupSettings.CustomController : null;
			_statsController = _worldCup != null ? _worldCup.StatsController : null;
		
			_teamsInfo.Clear();
		}

		/// <summary>
		/// Adds all the teams to the <see cref="_teamsInfo"/> list. Returns true when successful, false otherwise.
		/// </summary>
		protected virtual bool BuildTeamsInfo()
		{
			var teamIds = _tournamentManager.TeamIds;
			if (teamIds == null || teamIds.Length <= 0)
			{
				return false;
			}
		
			// Has the info already been built?
			if (_teamsInfo.Count == teamIds.Length)
			{
				return true;
			}

			var playerTeamIds = _tournamentManager.PlayerTeamIds;
			var groupsInfo = _tournamentManager.GroupsInfo;
		
			_teamsInfo.Clear();

			for (int i = 0, len = teamIds.Length; i < len; i++)
			{
				var teamId = teamIds[i];
				if (string.IsNullOrEmpty(teamId))
				{
					continue;
				}
			
				// Determine if the team's a human team
				var isHumanTeam = false;
				if (playerTeamIds != null && playerTeamIds.Length > 0)
				{
					for (int n = 0, lenN = playerTeamIds.Length; n < lenN; n++)
					{
						if (!teamId.Equals(playerTeamIds[n], StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						isHumanTeam = true;
						break;
					}
				}

				// Get the team's group
				var groupIndex = -1;
				if (groupsInfo != null && groupsInfo.Count > 0)
				{
					for (int n = 0, lenN = groupsInfo.Count; n < lenN; n++)
					{
						var info = groupsInfo[n];
						if (info == null || info.TeamIds == null || info.TeamIds.Length <= 0)
						{
							continue;
						}

						for (int t = 0, lenT = info.TeamIds.Length; t < lenT; t++)
						{
							if (!teamId.Equals(info.TeamIds[t], StringComparison.InvariantCultureIgnoreCase))
							{
								continue;
							}
							groupIndex = n;
							break;
						}
					}
				}
			
				_teamsInfo.Add(teamId, new TeamInfo(teamId, isHumanTeam, groupIndex));
			}

			return _teamsInfo.Count > 0;
		}

		/// <summary>
		/// Gets the team's info from the <see cref="_teamsInfo"/> list.
		/// </summary>
		protected virtual TeamInfo GetTeamInfo(string teamId)
		{
			if (_teamsInfo == null || _teamsInfo.Count <= 0 || string.IsNullOrEmpty(teamId) || 
			    !_teamsInfo.ContainsKey(teamId))
			{
				return null;
			}
			return _teamsInfo[teamId];
		}

		/// <summary>
		/// Creates the group stage matches. Returns true when successful, false otherwise.
		/// </summary>
		protected virtual bool CreateGroupStageMatches()
		{
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (_worldCupSettings == null || groupsInfo == null || groupsInfo.Count <= 0)
			{
				return false;
			}
		
			if (!BuildTeamsInfo())
			{
				return false;
			}

			// First pass: Create the matches for the groups.
			_tempGroupMatches.Clear();
			var groupStageSettings = _worldCupSettings.GroupStage;
			var maxGroups = groupStageSettings.NumGroups;
			var maxMatchesPerGroup = groupStageSettings.NumMatchesPerGroup;
			for (var groupIndex = 0; groupIndex < maxGroups; groupIndex++)
			{
				if (!_tempGroupMatches.ContainsKey(groupIndex))
				{
					_tempGroupMatches[groupIndex] = new List<TempMatchInfo>();
				}
			
				var list = _tempGroupMatches[groupIndex];
				for (var match = 0; match < maxMatchesPerGroup; match++)
				{
					var team0 = GetTeamForGroupMatch(null, groupIndex);
					var team1 = GetTeamForGroupMatch(team0, groupIndex);
					if (team0 == null || team1 == null)
					{
						return false;
					}
				
					team0.AddedMatch(team1);
					team1.AddedMatch(team0);
					list.Add(new TempMatchInfo(team0, team1));
				}
			}

			// Second pass: Break the matches up into match days.
			_tempDayMatches.Clear();
			var matchesInfo = _tournamentManager.MatchesInfo;
			var matchDay = 0;
			var matchIndex = 0;
			var maxMatches = groupStageSettings.NumMatches;
			while (matchIndex < maxMatches)
			{
				if (!_tempDayMatches.ContainsKey(matchDay))
				{
					_tempDayMatches[matchDay] = new List<TempMatchInfo>();
				}
				var dayList = _tempDayMatches[matchDay];
			
				foreach (var keyValuePair in _tempGroupMatches)
				{
					var groupList = keyValuePair.Value;
					int start;
					int end;
					int direction;
				
					// Alternate the search direction (up/down) for each match day, so that teams at the bottom of the 
					// group can also play before the top teams play again.
					if (matchDay % 2 == 0)
					{
						start = 0;
						end = groupList.Count - 1;
						direction = 1;
					}
					else
					{
						start = groupList.Count - 1;
						end = 0;
						direction = -1;
					}

					for (var i = start; (direction > 0 && i <= end) || (direction < 0 && i >= end); i += direction)
					{
						var tempInfo = groupList[i];
						if (tempInfo == null || tempInfo.MatchDay != -1)
						{
							continue;
						}
					
						// Each team can only play 1 match per day.
						if (DoesListContainTeams(dayList, tempInfo))
						{
							continue;
						}

						tempInfo.MatchDay = matchDay;
						dayList.Add(tempInfo);

						var team0 = tempInfo.TeamsInfo[0];
						var team1 = tempInfo.TeamsInfo[1];
						var matchInfo = matchesInfo[matchIndex];
				
						matchInfo.matchDay = matchDay;
				
						matchInfo.teamId[0] = team0.TeamId;
						matchInfo.isPlayer[0] = team0.IsHumanTeam;

						matchInfo.teamId[1] = team1.TeamId;
						matchInfo.isPlayer[1] = team1.IsHumanTeam;

						matchIndex++;
					}
				}

				matchDay++;
			}

			return true;
		}

		/// <summary>
		/// Does the list of matches contain any of the 2 teams in the match?
		/// </summary>
		protected virtual bool DoesListContainTeams(List<TempMatchInfo> list, TempMatchInfo match)
		{
			if (list == null || list.Count <= 0 || match == null || match.TeamsInfo == null || match.TeamsInfo.Length < 2)
			{
				return false;
			}

			var teamId0 = match.TeamsInfo[0].TeamId;
			var teamId1 = match.TeamsInfo[1].TeamId;
			if (string.IsNullOrEmpty(teamId0) || string.IsNullOrEmpty(teamId1))
			{
				return false;
			}
		
			for (int i = 0, len = list.Count; i < len; i++)
			{
				var tempMatch = list[i];
				if (tempMatch == null || tempMatch.TeamsInfo == null || tempMatch.TeamsInfo.Length < 2)
				{
					continue;
				}

				var tempTeamId0 = tempMatch.TeamsInfo[0].TeamId;
				var tempTeamId1 = tempMatch.TeamsInfo[1].TeamId;
				if (teamId0.Equals(tempTeamId0, StringComparison.InvariantCultureIgnoreCase) ||
				    teamId0.Equals(tempTeamId1, StringComparison.InvariantCultureIgnoreCase) ||
				    teamId1.Equals(tempTeamId0, StringComparison.InvariantCultureIgnoreCase) ||
				    teamId1.Equals(tempTeamId1, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
		
			return false;
		}
	
		/// <summary>
		/// Gets a team to use for a group match.
		/// </summary>
		protected virtual TeamInfo GetTeamForGroupMatch(TeamInfo opponentTeam, int groupIndex)
		{
			TeamInfo foundTeam = null;
			foreach (var keyValuePair in _teamsInfo)
			{
				var teamInfo = keyValuePair.Value;
				if (teamInfo == null || teamInfo.GroupIndex != groupIndex || teamInfo == opponentTeam || 
				    teamInfo.HasOpponent(opponentTeam))
				{
					continue;
				}

				if (foundTeam == null || foundTeam.MatchesCount > teamInfo.MatchesCount)
				{
					foundTeam = teamInfo;	
				}
			}
		
			return foundTeam;
		}

		/// <summary>
		/// Creates the knockout stage matches. Returns true when successful, false otherwise.
		/// </summary>
		protected virtual bool CreateKnockoutMatches(WorldCupState oldState, WorldCupState state, 
			bool creatingPartialMatches)
		{
			if (!BuildTeamsInfo())
			{
				return false;
			}
			
			return oldState == WorldCupState.GroupStage
				? CreateKnockoutMatchesAfterGroup(oldState, state)
				: CreateNextKnockoutMatches(oldState, state, creatingPartialMatches);
		}

		/// <summary>
		/// Creates the knockout stage matches after the group stage ended. Returns true when successful, false
		/// otherwise.
		/// </summary>
		protected virtual bool CreateKnockoutMatchesAfterGroup(WorldCupState oldState, WorldCupState state)
		{
			Assert.IsTrue(_worldCupSettings.GroupStage.NumAdvanceTeamsPerGroup == 2,
				"World cup only supports 2 teams to advance from each group.");

			_worldCup.GetMatchesForState(state, out var firstMatch, out var lastMatch);
			if (!firstMatch.HasValue || !lastMatch.HasValue)
			{
				return false;
			}
		
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (groupsInfo == null || groupsInfo.Count <= 0)
			{
				return false;
			}
		
			// The winner of each group plays against the runner-up of another group.
			_tempGroupMatches.Clear();
			var newMatchIndex = firstMatch.Value;
			var lastMatchIndex = lastMatch.Value;
			string lastWinner = null;
			string lastRunnerUp = null;
			for (int i = 0, len = groupsInfo.Count; i < len; i++)
			{
				var info = groupsInfo[i];
				if (info == null)
				{
					continue;
				}
			
				var winner = GetGroupWinner(info);
				var runnerUp = GetGroupRunnerUp(info);
				if (i % 2 == 0)
				{
					lastWinner = winner;
					lastRunnerUp = runnerUp;
					continue;
				}

				if (!_tempGroupMatches.ContainsKey(i))
				{
					_tempGroupMatches[i] = new List<TempMatchInfo>();
				}

				var list = _tempGroupMatches[i];
				var team0 = GetTeamInfo(lastWinner);
				var team1 = GetTeamInfo(runnerUp);
			
				list.Add(new TempMatchInfo(team0, team1));
				newMatchIndex++;
				if (newMatchIndex <= lastMatchIndex)
				{
					team0 = GetTeamInfo(winner);
					team1 = GetTeamInfo(lastRunnerUp);
			
					list.Add(new TempMatchInfo(team0, team1));
					newMatchIndex++;
				}

				if (newMatchIndex > lastMatchIndex)
				{
					break;
				}
			}

			if (_tempGroupMatches.Count <= 0)
			{
				return false;
			}
		
			// Put human matches first
			foreach (var keyValuePair in _tempGroupMatches)
			{
				var list = keyValuePair.Value;
				int? foundHumanIndex = null;
				int? foundAiIndex = null;
				for (int i = 0, len = list.Count; i < len; i++)
				{
					var info = list[i];
					if (info.TeamsInfo[0].IsHumanTeam || info.TeamsInfo[1].IsHumanTeam)
					{
						if (!foundHumanIndex.HasValue)
						{
							foundHumanIndex = i;	
						}
					}
					else if (!foundAiIndex.HasValue)
					{
						foundAiIndex = i;
					}
					if (foundHumanIndex.HasValue && foundAiIndex.HasValue)
					{
						break;
					}
				}

				if (foundHumanIndex.HasValue && foundAiIndex.HasValue && foundHumanIndex.Value > foundAiIndex.Value)
				{
					var tempInfo = list[foundHumanIndex.Value];
					list[foundHumanIndex.Value] = list[foundAiIndex.Value];
					list[foundAiIndex.Value] = tempInfo;
				}
			
				// We only need it for the first group
				break;
			}

			// We take the first match from each group, then the second match from each group.
			newMatchIndex = firstMatch.Value;
			for (var pass = 0; pass < 2; pass++)
			{
				foreach (var keyValuePair in _tempGroupMatches)
				{
					var groupIndex = keyValuePair.Key;
					var list = keyValuePair.Value;
					for (int i = 0, len = list.Count; i < len; i++)
					{
						if (i != pass)
						{
							continue;
						}
						var info = list[i];
						CreateKnockoutMatch(newMatchIndex++, info.TeamsInfo[0].TeamId, info.TeamsInfo[1].TeamId);
						if (newMatchIndex > lastMatchIndex)
						{
							break;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Gets the group's winning team ID. 
		/// </summary>
		protected virtual string GetGroupWinner(TournamentGroupInfo info)
		{
			return GetTeamWithRank(0, info);
		}
	
		/// <summary>
		/// Gets the group's runner-up team ID. 
		/// </summary>
		protected virtual string GetGroupRunnerUp(TournamentGroupInfo info)
		{
			return GetTeamWithRank(1, info);
		}

		protected virtual string GetTeamWithRank(int rankIndex, TournamentGroupInfo info)
		{
			GetGroupStats(info, _reuseTeamStats);
			return _reuseTeamStats != null && _reuseTeamStats.Count > rankIndex && _reuseTeamStats[rankIndex] != null
				? _reuseTeamStats[rankIndex].TeamId
				: null;
		}

		/// <summary>
		/// Gets the stats for all the teams in the group.
		/// </summary>
		/// <param name="info">Group info.</param>
		/// <param name="addToList">Add the stats to this list. This list will first be cleared.</param>
		protected virtual void GetGroupStats(TournamentGroupInfo info, List<TournamentTeamStats> addToList)
		{
			if (addToList == null)
			{
				return;
			}
		
			addToList.Clear();
			var groupStats = _statsController != null ? _statsController.GroupStats : null;
			if (groupStats == null || groupStats.Count <= 0 || info == null)
			{
				return;
			}

			for (int i = 0, len = groupStats.Count; i < len; i++)
			{
				var stats = groupStats[i];
				if (string.IsNullOrEmpty(stats.TeamId))
				{
					continue;
				}

				for (int t = 0, lenT = info.TeamIds.Length; t < lenT; t++)
				{
					var teamId = info.TeamIds[t];
					if (stats.TeamId.Equals(teamId, StringComparison.InvariantCultureIgnoreCase))
					{
						addToList.Add(stats);
					}
				}
			}
		}
	
		/// <summary>
		/// Creates a single knockout match.
		/// </summary>
		/// <param name="matchIndex">Match index.</param>
		/// <param name="teamId0">First team.</param>
		/// <param name="teamId1">Second team.</param>
		protected virtual void CreateKnockoutMatch(int matchIndex, string teamId0, string teamId1)
		{
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (matchesInfo == null || matchIndex < 0 || matchIndex >= matchesInfo.Length)
			{
				return;
			}
		
			var matchInfo = matchesInfo[matchIndex];
			if (matchInfo == null)
			{
				return;
			}
		
			// Has the match already been created?
			if (HasMatchBeenCreated(matchInfo))
			{
				return;
			}

			var team0 = GetTeamInfo(teamId0);
			var team1 = GetTeamInfo(teamId1);
			if (team0 == null && team1 == null)
			{
				return;
			}

			var oldMatch0 = GetPreviousMatch(teamId0, matchIndex, out var oldMatchIndex0);
			var oldMatch1 = GetPreviousMatch(teamId1, matchIndex, out var oldMatchIndex1);

			if (oldMatch0 != null)
			{
				matchInfo.CopyTeam(oldMatch0, oldMatchIndex0, 0);
				matchInfo.teamScore[0] = -1;
			}
			else
			{
				matchInfo.teamId[0] = team0 != null ? team0.TeamId : null;
				matchInfo.isPlayer[0] = team0 != null && team0.IsHumanTeam;
			}

			if (oldMatch1 != null)
			{
				matchInfo.CopyTeam(oldMatch1, oldMatchIndex1, 1);
				matchInfo.teamScore[1] = -1;
			}
			else
			{
				matchInfo.teamId[1] = team1 != null ? team1.TeamId : null;
				matchInfo.isPlayer[1] = team1 != null && team1.IsHumanTeam;	
			}
		}

		/// <summary>
		/// Tests if <paramref name="matchInfo"/> has been setup.
		/// </summary>
		protected virtual bool HasMatchBeenCreated(SsTournamentMatchInfo matchInfo)
		{
			return matchInfo != null && matchInfo.teamId != null && matchInfo.teamId.Length > 1 &&
			       !string.IsNullOrEmpty(matchInfo.teamId[0]) && !string.IsNullOrEmpty(matchInfo.teamId[1]);
		}

		/// <summary>
		/// Creates the next knockout stage's matches. Returns true when successful, false otherwise.
		/// </summary>
		protected virtual bool CreateNextKnockoutMatches(WorldCupState oldState, WorldCupState state, 
			bool creatingPartialMatches)
		{
			_worldCup.GetMatchesForState(oldState, out var oldFirstMatch, out var oldLastMatch);
			if (!oldFirstMatch.HasValue || !oldLastMatch.HasValue)
			{
				return false;
			}

			_worldCup.GetMatchesForState(state, out var firstMatch, out var lastMatch);
			if (!firstMatch.HasValue || !lastMatch.HasValue)
			{
				return false;
			}
		
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (groupsInfo == null || groupsInfo.Count <= 0)
			{
				return false;
			}
		
			// For every 2 matches we create 1 new match. We take the winners from the 2 matches (or losers when creating 
			// the third-place match).
			var newMatchIndex = firstMatch.Value;
			var maxNewMatchIndex = lastMatch.Value;
			string teamId0 = null;
			for (int i = 0, len = oldLastMatch.Value - oldFirstMatch.Value + 1; i < len; i++)
			{
				var oldMatchIndex = i + oldFirstMatch.Value;
				if (i % 2 == 0)
				{
					// Use the loser for the third place play-off, otherwise use the winner
					teamId0 = state == WorldCupState.ThirdPlacePlayOff
						? GetMatchLoser(oldMatchIndex)
						: GetMatchWinner(oldMatchIndex);
					continue;
				}

				// Use the loser for the third place play-off, otherwise use the winner
				var teamId1 = state == WorldCupState.ThirdPlacePlayOff
					? GetMatchLoser(oldMatchIndex)
					: GetMatchWinner(oldMatchIndex);
				if (string.IsNullOrEmpty(teamId0) && string.IsNullOrEmpty(teamId1))
				{
					if (creatingPartialMatches)
					{
						return true;
					}
					return false;
				}
			
				CreateKnockoutMatch(newMatchIndex++, teamId0, teamId1);
				if (newMatchIndex > maxNewMatchIndex)
				{
					break;
				}
			}
		
			return true;
		}

		/// <summary>
		/// Gets a previous match the team played in.
		/// </summary>
		/// <param name="teamId">Team ID to find.</param>
		/// <param name="beforeMatchIndex">Find a match before this match index.</param>
		/// <param name="teamIndex">Gets the team index (0 or 1, but -1 when invalid).</param>
		protected virtual SsTournamentMatchInfo GetPreviousMatch(string teamId, int beforeMatchIndex, out int teamIndex)
		{
			teamIndex = -1;
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (string.IsNullOrEmpty(teamId) || matchesInfo == null || matchesInfo.Length <= 0 || beforeMatchIndex <= 0)
			{
				return null;
			}

			for (var i = beforeMatchIndex - 1; i >= 0; i--)
			{
				var info = matchesInfo[i];
				if (info == null || info.teamId == null || info.teamId.Length < 2)
				{
					continue;
				}
				if (teamId.Equals(info.teamId[0], StringComparison.CurrentCultureIgnoreCase))
				{
					teamIndex = 0;
					return info;
				}
				if (teamId.Equals(info.teamId[1], StringComparison.CurrentCultureIgnoreCase))
				{
					teamIndex = 1;
					return info;
				}
			}

			return null;
		}
	
		protected virtual string GetMatchWinner(int matchIndex)
		{
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (matchesInfo == null || matchIndex >= matchesInfo.Length || matchIndex < 0)
			{
				return null;
			}

			var info = matchesInfo[matchIndex];
			if (info == null || info.teamId == null || info.teamId.Length < 2 || info.teamScore == null || 
			    info.teamScore.Length < 2)
			{
				return null;
			}

			return info.teamScore[0] > info.teamScore[1]
				? info.teamId[0]
				: info.teamScore[1] > info.teamScore[0]
					? info.teamId[1]
					: null;
		}
	
		protected virtual string GetMatchLoser(int matchIndex)
		{
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (matchesInfo == null || matchIndex >= matchesInfo.Length || matchIndex < 0)
			{
				return null;
			}

			var info = matchesInfo[matchIndex];
			if (info == null || info.teamId == null || info.teamId.Length < 2 || info.teamScore == null || 
			    info.teamScore.Length < 2)
			{
				return null;
			}

			return info.teamScore[0] > info.teamScore[1]
				? info.teamId[1]
				: info.teamScore[1] > info.teamScore[0]
					? info.teamId[0]
					: null;
		}
	}
}
