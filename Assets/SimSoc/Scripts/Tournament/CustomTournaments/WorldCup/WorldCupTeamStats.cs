namespace SimSoc
{
	/// <summary>
	/// A team's stats for the world cup. This includes group stage stats, which are used to calculate the team's rank. 
	/// </summary>
	public class WorldCupTeamStats
	{
		public string TeamId { get; }
	
		/// <summary>
		/// Gets the tournament's overall stats.
		/// </summary>
		public TournamentTeamStats OverallStats { get; }
	
		/// <summary>
		/// Gets the team's group stage stats.
		/// </summary>
		public TournamentTeamStats GroupStats { get; }

		/// <summary>
		/// Constructor with initialization parameters.
		/// </summary>
		public WorldCupTeamStats(string teamId)
		{
			TeamId = teamId;
			OverallStats = new TournamentTeamStats(teamId);
			GroupStats = new TournamentTeamStats(teamId);
		}

		public void ClearStats(string[] playerTeamIds)
		{
			OverallStats.ClearStats(playerTeamIds);
			GroupStats.ClearStats(playerTeamIds);
		}
	}
}
