using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimSoc
{
	/// <inheritdoc cref="ICustomTournamentController"/>
	/// <remarks>
	/// World cup tournament controller.
	/// </remarks>
	public class WorldCupTournament : IWorldCupTournament
	{
		// String format for displaying the group name.
		protected const string GroupFormat = "GROUP {0}";
		// Group name letters.
		protected const string GroupLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		
		protected readonly ITournamentManager _tournamentManager;
		protected WorldCupTournamentSettings _worldCupSettings;

		// Lists to re-use.
		protected readonly List<int> _tempGroupIndices = new List<int>();
		protected readonly List<int> _teamIndices = new List<int>();
		protected readonly List<string> _tempTeamIds = new List<string>();
	
		// Initialized a new tournament, or a tournament that was loaded.
		protected bool _initializedTournament;

		// Knockout settings. The dictionary key is the WorldCupState.
		protected readonly Dictionary<int, WorldCupKnockoutStageSettings> _knockoutSettings =
			new Dictionary<int, WorldCupKnockoutStageSettings>();
		
		/// <inheritdoc/>
		public virtual WorldCupState State { get; protected set; }

		/// <inheritdoc/>
		public virtual int MatchIndex { get; protected set; }

		/// <inheritdoc/>
		public virtual IWorldCupTeamStatsController StatsController { get; }
	
		public WorldCupTournament(ITournamentManager tournamentManager)
		{
			_tournamentManager = tournamentManager;
			StatsController = new WorldCupTeamStatsController(tournamentManager);
		}
	
#region ICustomTournamentController
		/// <inheritdoc/>
		public virtual bool StartTournamentValidation()
		{
			SetState(WorldCupState.NotStarted);
			InitializeTournament();
			return true;
		}

		/// <inheritdoc/>
		public virtual void OnGeneratedTeamIds(ref string[] teamIds)
		{
			var playerTeamIds = _tournamentManager.PlayerTeamIds;
			var hasTwoPlayerTeams = playerTeamIds != null && playerTeamIds.Length == 2 &&
			                        !string.IsNullOrEmpty(playerTeamIds[0]) && !string.IsNullOrEmpty(playerTeamIds[1]);
			if (teamIds == null || teamIds.Length <= 0 || !hasTwoPlayerTeams)
			{
				return;
			}
		
			// Test if humans teams can be in the first match
			var groupStageSettings = _worldCupSettings != null ? _worldCupSettings.GroupStage : null;
			var humansCanBeInFirstMatch = groupStageSettings != null && hasTwoPlayerTeams &&
			                              Random.Range(0.0f, 100.0f) <= groupStageSettings.HumansInSameMatchChance;
			if (humansCanBeInFirstMatch)
			{
				// Human teams can be in the first match, so don't randomize the teams array (i.e. leave human teams at
				// the start of the array). 
				return;
			}

			// Randomize all the teams, so that human teams don't always face each other in the same order.
			_tempTeamIds.Clear();
			_tempTeamIds.AddRange(teamIds);
			_tempTeamIds.Shuffle();
			teamIds = _tempTeamIds.ToArray();

			// Put one of the human teams at index 0
			var playerTeamId0 = playerTeamIds[0];
			var playerTeamId1 = playerTeamIds[1];
			for (int i = 0, len = teamIds.Length; i < len; i++)
			{
				var teamId = teamIds[i];
				if (string.IsNullOrEmpty(teamId))
				{
					continue;
				}
			
				if (teamId.Equals(playerTeamId0, StringComparison.InvariantCultureIgnoreCase) ||
				    teamId.Equals(playerTeamId1, StringComparison.InvariantCultureIgnoreCase))
				{
					if (i != 0)
					{
						teamIds[i] = teamIds[0];
						teamIds[0] = teamId;
					}
					break;
				}
			}
		}

		/// <inheritdoc/>
		public virtual bool SetupGroups()
		{
			var teamIds = _tournamentManager.TeamIds;
			var settings = _tournamentManager.Settings;
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (teamIds == null || teamIds.Length <= 0 || settings == null || groupsInfo == null)
			{
				return false;
			}
		
			var worldCupSettings = (WorldCupTournamentSettings)settings.CustomSettings;
			var groupStageSettings = worldCupSettings.GroupStage;
		
			// Not enough teams for groups?
			if (teamIds.Length < worldCupSettings.NumTeams)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.LogError("Not enough teams for the groups.");
#endif
				return false;
			}

			var playerTeamIds = _tournamentManager.PlayerTeamIds;
			var hasTwoPlayerTeams = playerTeamIds != null && playerTeamIds.Length == 2 && 
			                        !string.IsNullOrEmpty(playerTeamIds[0]) && !string.IsNullOrEmpty(playerTeamIds[1]);
			var humansCanBeInSameGroup = false;
		
			if (hasTwoPlayerTeams)
			{
				humansCanBeInSameGroup = Random.Range(0.0f, 100.0f) <= groupStageSettings.HumansInSameGroupChance;
			}
		
			// Place the teams in sequential order in the groups.
			groupsInfo.Clear();
			var numTeamsPerGroup = groupStageSettings.NumTeamsPerGroup;
			var teamIndex = 0;
			int? teamGroup0 = null;
			int? teamGroup1 = null;
			var teamIndex0 = -1;
			var teamIndex1 = -1;
			for (int groupIndex = 0, lenG = groupStageSettings.NumGroups; groupIndex < lenG; groupIndex++)
			{
				var groupInfo = new TournamentGroupInfo
				{
					DisplayName = CreateGroupDisplayName(groupIndex),
					TeamIds = new string[numTeamsPerGroup]
				};
				for (var t = 0; t < numTeamsPerGroup; t++)
				{
					var teamId = teamIds[teamIndex++];
					groupInfo.TeamIds[t] = teamId;
					if (!hasTwoPlayerTeams)
					{
						continue;
					}

					if (teamId.Equals(playerTeamIds[0], StringComparison.InvariantCultureIgnoreCase))
					{
						teamGroup0 = groupIndex;
						teamIndex0 = t;
					}
					if (teamId.Equals(playerTeamIds[1], StringComparison.InvariantCultureIgnoreCase))
					{
						teamGroup1 = groupIndex;
						teamIndex1 = t;
					}
				}
				groupsInfo.Add(groupInfo);
			}
		
			if (!hasTwoPlayerTeams || groupsInfo.Count < 2)
			{
				return true;
			}
		
			// Are the human teams in the same group, but they shouldn't be?
			if (!humansCanBeInSameGroup && teamGroup0.HasValue && teamGroup1.HasValue && 
			    teamGroup0.Value == teamGroup1.Value)
			{
				// Randomly decide to move team 0 or 1 to a different group
				if (Random.Range(0, 100) < 50)
				{
					MoveTeamToRandomGroup(teamGroup0.Value, teamIndex0);
				}
				else
				{
					MoveTeamToRandomGroup(teamGroup1.Value, teamIndex1);
				}
			}
			else if (humansCanBeInSameGroup && teamGroup0.HasValue && teamGroup1.HasValue && 
			         teamGroup0.Value != teamGroup1.Value)
			{
				// Human teams are not in the same group, but they should be.
				// Move the team from the bottom group the top group (should be group zero).
				if (teamGroup1.Value < teamGroup0.Value)
				{
					MoveTeamToGroup(teamGroup0.Value, teamGroup1.Value, teamIndex0, teamIndex1);
				}
				else
				{
					MoveTeamToGroup(teamGroup1.Value, teamGroup0.Value, teamIndex1, teamIndex0);
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public virtual void OnSetupMatches()
		{
			CalculateStats(true);
		}

		/// <inheritdoc/>
		public virtual void OnNewOrLoaded(bool wasLoaded)
		{
			InitializeTournament();
		
			OnMatchIndexChanged(false);

			// Update matches info
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (matchesInfo != null && matchesInfo.Length > 0)
			{
				for (int i = 0, len = matchesInfo.Length; i < len; i++)
				{
					var info = matchesInfo[i];
					if (info == null)
					{
						continue;
					}
				
					if (string.IsNullOrEmpty(info.displayHeading))
					{
						info.displayHeading = GetMatchDisplayHeading(i);
					}

					info.needsWinner = false;
					info.knockoutRoundIndex = -1;
					info.knockoutRoundMatchIndex = -1;
				
					var state = GetStateForMatch(i);
					if (!state.HasValue)
					{
						continue;
					}
				
					var isKnockoutStage = IsKnockoutStageState(state.Value);
					info.needsWinner = isKnockoutStage;
					if (!isKnockoutStage)
					{
						continue;
					}

					var roundIndex = GetKnockoutRoundIndexForState(state.Value);
					if (!roundIndex.HasValue)
					{
						continue;
					}
					info.knockoutRoundIndex = roundIndex.Value;
				
					GetMatchesForState(state.Value, out var firstMatchIndex, out var lastMatchIndex);
					if (!firstMatchIndex.HasValue || !lastMatchIndex.HasValue || i < firstMatchIndex.Value || 
					    i > lastMatchIndex.Value)
					{
						continue;
					}

					info.knockoutRoundMatchIndex = i - firstMatchIndex.Value;
				}
			}
		
			// Update groups info
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (groupsInfo != null && groupsInfo.Count > 0)
			{
				for (int i = 0, len = groupsInfo.Count; i < len; i++)
				{
					var info = groupsInfo[i];
					if (info != null && string.IsNullOrEmpty(info.DisplayName))
					{
						info.DisplayName = CreateGroupDisplayName(i);
					}
				}
			}

			CalculateStats(wasLoaded);
		}

		/// <inheritdoc/>
		public virtual bool OnMatchEnded()
		{
			OnMatchIndexChanged(true);
			return true;
		}

		/// <inheritdoc/>
		public virtual void OnTournamentEnded()
		{
			_initializedTournament = false;
			SetState(WorldCupState.NotStarted);
		}
#endregion

		/// <inheritdoc/>
		public virtual bool IsKnockoutStageState(WorldCupState state)
		{
			return _knockoutSettings != null && _knockoutSettings.ContainsKey((int)state);
		}

		/// <inheritdoc/>
		public virtual WorldCupState? GetNextKnockoutState(WorldCupState state)
		{
			if (_knockoutSettings == null || _knockoutSettings.Count <= 0)
			{
				return null;
			}

			var foundState = state == WorldCupState.GroupStage;
			foreach (var keyValuePair in _knockoutSettings)
			{
				var tempState = (WorldCupState)keyValuePair.Key;
				var tempSettings = keyValuePair.Value;
				if (foundState && tempSettings.NumMatches > 0)
				{
					return tempState;
				}
				if (state == tempState)
				{
					foundState = true;
				}
			}

			return null;
		}

		/// <inheritdoc/>
		public virtual WorldCupState? GetStateForMatch(int matchIndex)
		{
			var worldCupSettings = (WorldCupTournamentSettings)_tournamentManager.Settings.CustomSettings;
		
			var min = 0;
			var num = worldCupSettings.GroupStage.NumMatches;
			var max = min + num;
			if (num > 0 && matchIndex >= min && matchIndex < max)
			{
				return WorldCupState.GroupStage;
			}

			foreach (var keyValuePair in _knockoutSettings)
			{
				var settings = keyValuePair.Value;
				min += num;
				num = settings.NumMatches;
				max = min + num;
				if (num > 0 && matchIndex >= min && matchIndex < max)
				{
					return (WorldCupState)keyValuePair.Key;
				}
			}
		
			if (matchIndex >= max)
			{
				return WorldCupState.Done;
			}
		
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.LogError($"Could not determine the world cup state from the match index {matchIndex}. Were new states added?");
#endif
		
			return null;
		}

		/// <inheritdoc/>
		public virtual int? GetKnockoutRoundIndexForState(WorldCupState state)
		{
			if (_knockoutSettings == null || _knockoutSettings.Count <= 0)
			{
				return null;
			}
		
			var roundIndex = 0;
			foreach (var keyValuePair in _knockoutSettings)
			{
				var tempState = (WorldCupState)keyValuePair.Key;
				var tempSettings = keyValuePair.Value;
				if (tempSettings.NumMatches <= 0)
				{
					continue;
				}
			
				if (state == tempState)
				{
					return roundIndex;
				}
				roundIndex++;
			}

			return null;
		}

		/// <inheritdoc/>
		public virtual void GetMatchesForState(WorldCupState state, out int? firstMatchIndex, out int? lastMatchIndex)
		{
			firstMatchIndex = null;
			lastMatchIndex = null;
		
			var worldCupSettings = (WorldCupTournamentSettings)_tournamentManager.Settings.CustomSettings;
			var min = 0;
			var num = worldCupSettings.GroupStage.NumMatches;
			var max = min + num;
			if (state == WorldCupState.GroupStage)
			{
				if (num > 0)
				{
					firstMatchIndex = min;
					lastMatchIndex = max - 1;	
				}
				return;
			}

			foreach (var keyValuePair in _knockoutSettings)
			{
				var settings = keyValuePair.Value;
				min += num;
				num = settings.NumMatches;
				max = min + num;
				if (state == (WorldCupState)keyValuePair.Key)
				{
					if (num > 0)
					{
						firstMatchIndex = min;
						lastMatchIndex = max - 1;	
					}
					return;
				}
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Debug.LogError($"Could not determine the match index from the world cup state {state}. Were new states added?");
#endif
		}

		/// <inheritdoc/>
		public virtual string GetStateDisplayHeading(WorldCupState state)
		{
			var heading = _worldCupSettings.GetStateHeading(state);
			return string.IsNullOrEmpty(heading) ? state.ToString() : heading;
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

			if (_worldCupSettings == null)
			{
				return;
			}
		
			// Build the list of knockout settings
			_knockoutSettings.Clear();
			_knockoutSettings.Add((int)WorldCupState.RounfOf32, _worldCupSettings.RoundOf32);
			_knockoutSettings.Add((int)WorldCupState.RoundOf16, _worldCupSettings.RoundOf16);
			_knockoutSettings.Add((int)WorldCupState.QuarterFinals, _worldCupSettings.QuarterFinals);
			_knockoutSettings.Add((int)WorldCupState.SemiFinals, _worldCupSettings.SemiFinals);
			_knockoutSettings.Add((int)WorldCupState.ThirdPlacePlayOff, _worldCupSettings.ThirdPlace);
			_knockoutSettings.Add((int)WorldCupState.Final, _worldCupSettings.Final);
		}
	
		/// <summary>
		/// Helper method used by <see cref="SetupGroups"/> to move a human team to a random group.
		/// </summary>
		/// <param name="fromGroup">Move team from this group.</param>
		/// <param name="teamIndex">Team index in the <paramref name="fromGroup"/>.</param>
		protected virtual void MoveTeamToRandomGroup(int fromGroup, int teamIndex)
		{
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (groupsInfo == null || fromGroup < 0 || fromGroup >= groupsInfo.Count || groupsInfo.Count < 2)
			{
				return;
			}
		
			// Build a list of possible groups
			_tempGroupIndices.Clear();
			for (int i = 0, len = groupsInfo.Count; i < len; i++)
			{
				if (i != fromGroup)
				{
					_tempGroupIndices.Add(i);
				}
			}

			const int toTeamIndex = 0;
			var fromGroupInfo = groupsInfo[fromGroup];
			var toGroupInfo = groupsInfo[_tempGroupIndices[Random.Range(0, _tempGroupIndices.Count)]];
			var fromTeamId = fromGroupInfo.TeamIds[teamIndex];
			var toTeamId = toGroupInfo.TeamIds[toTeamIndex];
			fromGroupInfo.TeamIds[teamIndex] = toTeamId;
			toGroupInfo.TeamIds[toTeamIndex] = fromTeamId;
		}

		/// <summary>
		/// Helper method used by <see cref="SetupGroups"/> to move a human team to the specified group.
		/// </summary>
		/// <param name="fromGroup">Move team from this group.</param>
		/// <param name="toGroup">Move the team to this group.</param>
		/// <param name="teamIndex">Team index in the <paramref name="fromGroup"/>.</param>
		/// <param name="ignoreIndex">Don't put the team in this index in the <paramref name="toGroup"/>.</param>
		protected virtual void MoveTeamToGroup(int fromGroup, int toGroup, int teamIndex, int ignoreIndex)
		{
			var groupsInfo = _tournamentManager.GroupsInfo;
			if (groupsInfo == null || fromGroup < 0 || fromGroup >= groupsInfo.Count || toGroup < 0 || 
			    toGroup >= groupsInfo.Count || groupsInfo.Count < 2)
			{
				return;
			}
			
			var toGroupInfo = groupsInfo[toGroup];
			if (toGroupInfo == null || toGroupInfo.TeamIds == null || toGroupInfo.TeamIds.Length <= 0)
			{
				return;
			}
		
			// Build a list of possible indices
			_teamIndices.Clear();
			for (int i = 0, len = toGroupInfo.TeamIds.Length; i < len; i++)
			{
				if (i != ignoreIndex)
				{
					_teamIndices.Add(i);
				}
			}

			if (_teamIndices.Count <= 0)
			{
				return;
			}
		
			var toTeamIndex = _teamIndices[Random.Range(0, _teamIndices.Count)];
			var fromGroupInfo = groupsInfo[fromGroup];
			var fromTeamId = fromGroupInfo.TeamIds[teamIndex];
			var toTeamId = toGroupInfo.TeamIds[toTeamIndex];
			fromGroupInfo.TeamIds[teamIndex] = toTeamId;
			toGroupInfo.TeamIds[toTeamIndex] = fromTeamId;
		}

		protected virtual void SetState(WorldCupState newState)
		{
			if (State == newState)
			{
				return;
			}

			State = newState;
			switch (newState)
			{
				case WorldCupState.NotStarted:
					MatchIndex = 0;
					break;
			}
		}

		/// <summary>
		/// Updates the state when the match index changed.
		/// </summary>
		protected virtual void OnMatchIndexChanged(bool calculateStats)
		{
			var matchIndex = _tournamentManager.MatchIndex;
			MatchIndex = matchIndex;
			if (calculateStats)
			{
				CalculateStats(false);	
			}

			var newState = GetStateForMatch(matchIndex);
			if (newState.HasValue)
			{
				SetState(newState.Value);
			}
		}

		/// <summary>
		/// Updates the stats based on the matches played.
		/// </summary>
		protected virtual void CalculateStats(bool isFirstTime)
		{
			if (StatsController != null)
			{
				StatsController.CalculateStats(isFirstTime);	
			}
		}

		protected virtual string CreateGroupDisplayName(int groupIndex)
		{
			return groupIndex < GroupLetters.Length
				? string.Format(GroupFormat, GroupLetters[groupIndex])
				: string.Format(GroupFormat, groupIndex + 1);
		}
	
		protected virtual string GetMatchDisplayHeading(int matchIndex)
		{
			var state = GetStateForMatch(matchIndex);
			return state.HasValue ? GetStateDisplayHeading(state.Value) : null;
		}
	}
}
