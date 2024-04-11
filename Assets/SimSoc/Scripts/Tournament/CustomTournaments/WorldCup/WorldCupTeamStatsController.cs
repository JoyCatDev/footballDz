using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimSoc
{
	/// <inheritdoc cref="IWorldCupTeamStatsController"/>
	public class WorldCupTeamStatsController : IWorldCupTeamStatsController
	{
		/// <inheritdoc/>
		public event UpdatedWorldCupStatsEventHandler UpdatedWorldCupStats;
	
		protected readonly ITournamentManager _tournamentManager;
		protected WorldCupTournamentSettings _worldCupSettings;
		protected readonly Dictionary<string, WorldCupTeamStats> _stats = new Dictionary<string, WorldCupTeamStats>();
		protected readonly List<TournamentTeamStats> _overallStats = new List<TournamentTeamStats>();
		protected readonly List<TournamentTeamStats> _groupStats = new List<TournamentTeamStats>();

		/// <inheritdoc/>
		public virtual Dictionary<string, WorldCupTeamStats> Stats => _stats;

		/// <inheritdoc/>
		public virtual List<TournamentTeamStats> OverallStats => _overallStats;

		/// <inheritdoc/>
		public virtual List<TournamentTeamStats> GroupStats => _groupStats;
	
		public WorldCupTeamStatsController(ITournamentManager tournamentManager)
		{
			_tournamentManager = tournamentManager;
		}

		/// <inheritdoc/>
		public virtual WorldCupTeamStats GetStats(string teamId)
		{
			return !string.IsNullOrEmpty(teamId) && _stats != null && _stats.ContainsKey(teamId)
				? _stats[teamId]
				: null;
		}

		/// <inheritdoc/>
		public virtual void GetGroupStats(int groupIndex, List<TournamentTeamStats> addToList)
		{
			if (addToList == null)
			{
				return;
			}
		
			addToList.Clear();
			if (_groupStats == null || _groupStats.Count <= 0)
			{
				return;
			}

			for (int i = 0, len = _groupStats.Count; i < len; i++)
			{
				var stats = _groupStats[i];
				if (stats != null && stats.GroupIndex == groupIndex)
				{
					addToList.Add(stats);
				}
			}

			if (addToList.Count > 0)
			{
				addToList.Sort((x, y) => x.Rank.CompareTo(y.Rank));
			}
		}

		/// <inheritdoc/>
		public virtual void CalculateStats(bool isFirstTime)
		{
			var matchesInfo = _tournamentManager.MatchesInfo;
			if (matchesInfo == null || matchesInfo.Length <= 0)
			{
				return;
			}

			Initialize(isFirstTime);

			var worldCupSettings = (WorldCupTournamentSettings)_tournamentManager.Settings.CustomSettings;
			var worldCupController = (IWorldCupTournament)worldCupSettings.CustomController;
			var updatedOverallStats = false;
			var updatedGroupStats = false;
			for (int i = 0, len = matchesInfo.Length; i < len; i++)
			{
				var matchInfo = matchesInfo[i];
			
				// Skip matches that haven't been played or setup
				if (matchInfo == null || matchInfo.teamScore[0] < 0 || matchInfo.teamScore[1] < 0 ||
				    string.IsNullOrEmpty(matchInfo.teamId[0]) || string.IsNullOrEmpty(matchInfo.teamId[1]))
				{
					continue;
				}

				var stats0 = GetStats(matchInfo.teamId[0]);
				var stats1 = GetStats(matchInfo.teamId[1]);
				if (stats0 == null || stats1 == null)
				{
					continue;
				}

				// Overall stats
				updatedOverallStats = true;
				UpdateTeamStats(stats0.OverallStats, stats1.OverallStats, matchInfo, worldCupSettings.PointsWin,
					worldCupSettings.PointsDraw, worldCupSettings.PointsLose);

				// Group stats
				var state = worldCupController.GetStateForMatch(i);
				if (!state.HasValue || state.Value != WorldCupState.GroupStage)
				{
					continue;
				}
				updatedGroupStats = true;
				UpdateTeamStats(stats0.GroupStats, stats1.GroupStats, matchInfo, worldCupSettings.PointsWin,
					worldCupSettings.PointsDraw, worldCupSettings.PointsLose);
			}

			if (updatedOverallStats)
			{
				SortTeamStats(_overallStats);
			}
		
			if (updatedGroupStats)
			{
				SortTeamStats(_groupStats);
			}
		
			UpdatedWorldCupStats?.Invoke(this);
		}
	
		/// <summary>
		/// Initializes (or clears) the stats.
		/// </summary>
		/// <param name="isFirstTime">Is this the first time for a new (or loaded) tournament?</param>
		protected virtual void Initialize(bool isFirstTime)
		{
			_worldCupSettings = (WorldCupTournamentSettings)_tournamentManager.Settings.CustomSettings;

			if (isFirstTime)
			{
				_stats.Clear();
				_overallStats.Clear();
				_groupStats.Clear();
			}
		
			var teamIds = _tournamentManager.TeamIds;
			if (teamIds == null || teamIds.Length <= 0)
			{
				return;
			}

			var playerTeamIds = _tournamentManager.PlayerTeamIds;
			for (int i = 0, len = teamIds.Length; i < len; i++)
			{
				var stats = AddTeam(teamIds[i]);
				if (stats == null)
				{
					continue;
				}
			
				stats.ClearStats(playerTeamIds);
				InitializeTeamStats(stats.OverallStats);
				InitializeTeamStats(stats.GroupStats);
			}
		}

		protected virtual void UpdateTeamStats(TournamentTeamStats stats0, TournamentTeamStats stats1, 
			SsTournamentMatchInfo matchInfo, int pointsWin, int pointsDraw, int pointsLose)
		{
			var score0 = matchInfo.teamScore[0];
			var score1 = matchInfo.teamScore[1];
			var points0 = 0;
			var points1 = 0;
			var goalDifference0 = 0;
			var goalDifference1 = 0;

			if (score0 > score1)
			{
				// Team[0] wins
				stats0.MatchesWon ++;
				points0 += pointsWin;
				goalDifference0 += score0 - score1;

				stats1.MatchesLost ++;
				points1 += pointsLose;
				goalDifference1 -= score0 - score1;
			}
			else if (score1 > score0)
			{
				// Team[1] wins
				stats1.MatchesWon ++;
				points1 += pointsWin;
				goalDifference1 += score1 - score0;

				stats0.MatchesLost ++;
				points0 += pointsLose;
				goalDifference0 -= score1 - score0;
			}
			else if (score0 == score1)
			{
				// Draw
				stats0.MatchesDrawn ++;
				points0 += pointsDraw;

				stats1.MatchesDrawn ++;
				points1 += pointsDraw;
			}

			stats0.Goals += score0;
			stats0.Points += points0;
			stats0.GoalDifference += goalDifference0;
			stats0.MatchesPlayed ++;
			stats0.AddHeadToHeadStats(stats1.TeamId, points0, goalDifference0, score0);
		
			stats1.Goals += score1;
			stats1.Points += points1;
			stats1.GoalDifference += goalDifference1;
			stats1.MatchesPlayed ++;
			stats1.AddHeadToHeadStats(stats0.TeamId, points1, goalDifference1, score1);
		}

		protected virtual void InitializeTeamStats(TournamentTeamStats teamStats)
		{
			if (teamStats == null)
			{
				return;
			}
		
			teamStats.GroupIndex = GetTeamGroupIndex(teamStats.TeamId);
			var teamResource = SsResourceManager.Instance != null
				? SsResourceManager.Instance.GetTeam(teamStats.TeamId)
				: null;
			if (teamResource != null)
			{
				teamStats.TeamName = teamResource.teamName;
			}
		}

		protected virtual int GetTeamGroupIndex(string teamId)
		{
			if (_tournamentManager == null)
			{
				return -1;
			}
			var groupIndex = _tournamentManager.GetGroupIndex(teamId);
			return groupIndex.HasValue ? groupIndex.Value : -1;
		}

		protected virtual void SortTeamStats(List<TournamentTeamStats> teamStats)
		{
			if (teamStats == null || teamStats.Count <= 0)
			{
				return;
			}
			
			// First sort via team ID so that the initial order is always the same, before we use the random seed.
			teamStats.Sort(CompareTeamStatsById);
			// Set random seed so that the sort order is the same (e.g. when the sort needs to select a random team).
			Random.InitState(_tournamentManager.RandomSeed);
			teamStats.Sort(CompareTeamStats);
			Random.InitState((int)DateTime.Now.Ticks);

			// Update the ranks
			for (int i = 0, len = teamStats.Count; i < len; i++)
			{
				var stats = teamStats[i];
				if (stats != null)
				{
					stats.Rank = i + 1;
				}
			}
		}

		protected virtual int CompareTeamStatsById(TournamentTeamStats x, TournamentTeamStats y)
		{
			return string.Compare(x.TeamId, y.TeamId, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Compares the two teams' stats to determine each team's rank.
		/// </summary>
		protected virtual int CompareTeamStats(TournamentTeamStats x, TournamentTeamStats y)
		{
			// Points
			var result = y.Points.CompareTo(x.Points);
			if (result != 0)
			{
				return result;
			}

			// Goal difference
			result = y.GoalDifference.CompareTo(x.GoalDifference);
			if (result != 0)
			{
				return result;
			}
		
			// Goals scored
			result = y.Goals.CompareTo(x.Goals);
			if (result != 0)
			{
				return result;
			}

			// Points in head-to-head matches
			result = y.GetHeadToHeadPoints(x.TeamId).CompareTo(x.GetHeadToHeadPoints(y.TeamId));
			if (result != 0)
			{
				return result;
			}
		
			// Goal difference in head-to-head matches
			result = y.GetHeadToHeadGoalDifference(x.TeamId).CompareTo(x.GetHeadToHeadGoalDifference(y.TeamId));
			if (result != 0)
			{
				return result;
			}

			// Goals scored in head-to-head matches
			result = y.GetHeadToHeadGoals(x.TeamId).CompareTo(x.GetHeadToHeadGoals(y.TeamId));
			if (result != 0)
			{
				return result;
			}

			// Note: If you implement red/yellow cards then here you can compare fair play points, defined by the
			// number of yellow and red cards received.

			// Drawing of lots (i.e. random)
			result = Random.Range(0, 100) < 50 ? -1 : 1;
		
			// Give preference to human teams?
			if (_worldCupSettings != null && _worldCupSettings.PreferHumanRank)
			{
				// If both are human then randomly select one
				if (x.HumanIndex >= 0 && y.HumanIndex >= 0)
				{
					return result;
				}
				if (x.HumanIndex >= 0)
				{
					return -1;
				}
				if (y.HumanIndex >= 0)
				{
					return 1;
				}
			}
		
			return result;
		}

		protected virtual WorldCupTeamStats AddTeam(string teamId)
		{
			var stats = GetStats(teamId);
			if (stats == null && !string.IsNullOrEmpty(teamId))
			{
				stats = new WorldCupTeamStats(teamId);
				_stats.Add(teamId, stats);
				_overallStats.Add(stats.OverallStats);
				_groupStats.Add(stats.GroupStats);
			}
			return stats;
		}
	}
}
