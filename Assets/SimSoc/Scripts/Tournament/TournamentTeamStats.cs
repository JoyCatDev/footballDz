using System;
using System.Collections.Generic;

namespace SimSoc
{
	/// <summary>
	/// A team's stats for a tournament. This can be used for a whole tournament, and/or for a specific stage of a
	/// tournament (e.g. group stage of a world cup).
	/// </summary>
	public class TournamentTeamStats
	{
		// Stats for head-to-head matches against other teams. Dictionary keys are the other team's ID.
		private readonly Dictionary<string, int> _headToHeadPoints = new Dictionary<string, int>();
		private readonly Dictionary<string, int> _headToHeadGoalDifference = new Dictionary<string, int>();
		private readonly Dictionary<string, int> _headToHeadGoals = new Dictionary<string, int>();
	
		public string TeamId { get; }
		/// <summary>
		/// Name to display on UI.
		/// </summary>
		public string TeamName { get; set; }
		public int GroupIndex { get; set; }
	
		public int Rank { get; set; }
		public int Points { get; set;}
		public int Goals { get; set;}
		public int GoalDifference { get; set; }
		public int MatchesPlayed { get; set; }
		public int MatchesWon { get; set; }
		public int MatchesDrawn { get; set; }
		public int MatchesLost { get; set; }
	
		/// <summary>
		/// Human player index (e.g. 0 = Player1, 1 = Player2, -1 = AI team)
		/// </summary>
		public int HumanIndex { get; private set; } = -1;

		/// <summary>
		/// Constructor with initialization parameters.
		/// </summary>
		public TournamentTeamStats(string teamId)
		{
			TeamId = teamId;
		}

		public void ClearStats(string[] playerTeamIds)
		{
			_headToHeadPoints.Clear();
			_headToHeadGoalDifference.Clear();
			_headToHeadGoals.Clear();
		
			Rank = 0;
			Points = 0;
			Goals = 0;
			GoalDifference = 0;
			MatchesPlayed = 0;
			MatchesWon = 0;
			MatchesDrawn = 0;
			MatchesLost = 0;

			SetHumanIndex(playerTeamIds);
		}

		/// <summary>
		/// Sets <see cref="HumanIndex"/> based on the player teams.
		/// </summary>
		public void SetHumanIndex(string[] playerTeamIds)
		{
			HumanIndex = -1;
			if (playerTeamIds == null || playerTeamIds.Length <= 0)
			{
				return;
			}
			for (int i = 0, len = playerTeamIds.Length; i < len; i++)
			{
				if (TeamId.Equals(playerTeamIds[i], StringComparison.InvariantCultureIgnoreCase))
				{
					HumanIndex = i;
					return;
				}
			}
		}

		/// <summary>
		/// Add stats received in a match against another team.
		/// </summary>
		/// <param name="opponentTeamId">Other team's ID.</param>
		/// <param name="points">Points received in the match</param>
		/// <param name="goalDifference">Goal difference received in the match.</param>
		/// <param name="goals">Goals received in the match.</param>
		public void AddHeadToHeadStats(string opponentTeamId, int points, int goalDifference, int goals)
		{
			if (_headToHeadPoints.ContainsKey(opponentTeamId))
			{
				_headToHeadPoints[opponentTeamId] += points;
			}
			else
			{
				_headToHeadPoints[opponentTeamId] = points;
			}
		
			if (_headToHeadGoalDifference.ContainsKey(opponentTeamId))
			{
				_headToHeadGoalDifference[opponentTeamId] += goalDifference;
			}
			else
			{
				_headToHeadGoalDifference[opponentTeamId] = goalDifference;
			}
		
			if (_headToHeadGoals.ContainsKey(opponentTeamId))
			{
				_headToHeadGoals[opponentTeamId] += goals;
			}
			else
			{
				_headToHeadGoals[opponentTeamId] = goals;
			}
		}

		public int GetHeadToHeadPoints(string opponentTeamId)
		{
			return _headToHeadPoints.ContainsKey(opponentTeamId) ? _headToHeadPoints[opponentTeamId] : 0;
		}
	
		public int GetHeadToHeadGoalDifference(string opponentTeamId)
		{
			return _headToHeadGoalDifference.ContainsKey(opponentTeamId) ? _headToHeadGoalDifference[opponentTeamId] : 0;
		}
	
		public int GetHeadToHeadGoals(string opponentTeamId)
		{
			return _headToHeadGoals.ContainsKey(opponentTeamId) ? _headToHeadGoals[opponentTeamId] : 0;
		}
	}
}
