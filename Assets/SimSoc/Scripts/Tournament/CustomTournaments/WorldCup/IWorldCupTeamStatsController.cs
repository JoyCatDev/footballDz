using System.Collections.Generic;

namespace SimSoc
{
	/// <summary>
	/// Controls the team stats in a world cup.
	/// </summary>
	public interface IWorldCupTeamStatsController
	{
		/// <summary>
		/// Fired when the stats are updated.
		/// </summary>
		event UpdatedWorldCupStatsEventHandler UpdatedWorldCupStats;
	
		/// <summary>
		/// Gets all the team stats. The dictionary's key is the team ID.
		/// </summary>
		Dictionary<string, WorldCupTeamStats> Stats { get; }
	
		/// <summary>
		/// Gets the overall stats, sorted by rank.
		/// </summary>
		List<TournamentTeamStats> OverallStats { get; }
	
		/// <summary>
		/// Gets the group stage stats, sorted by rank.
		/// </summary>
		List<TournamentTeamStats> GroupStats { get; }

		/// <summary>
		/// Gets a team's stats.
		/// </summary>
		WorldCupTeamStats GetStats(string teamId);

		/// <summary>
		/// Gets the stats for a specific group. It will be sorted via rank.
		/// </summary>
		/// <param name="groupIndex">The group to get.</param>
		/// <param name="addToList">Add them to this list.</param>
		void GetGroupStats(int groupIndex, List<TournamentTeamStats> addToList);

		/// <summary>
		/// Updates the stats based on the matches played.
		/// </summary>
		/// <param name="isFirstTime">Is this the first time for a new (or loaded) tournament?</param>
		void CalculateStats(bool isFirstTime);
	}
}
