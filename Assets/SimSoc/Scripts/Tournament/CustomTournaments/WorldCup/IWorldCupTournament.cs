namespace SimSoc
{
	/// <inheritdoc cref="ICustomTournamentController"/>
	/// <remarks>
	/// World cup tournament controller.
	/// </remarks>
	public interface IWorldCupTournament : ICustomTournamentController
	{
		/// <summary>
		/// Gets the state of the world cup.
		/// </summary>
		WorldCupState State { get; }
	
		/// <summary>
		/// Gets the current match index. (Zero based index.)
		/// </summary>
		int MatchIndex { get; }
	
		/// <summary>
		/// Gets the stats controller.
		/// </summary>
		IWorldCupTeamStatsController StatsController { get; }

		/// <summary>
		/// Is the state one of the knockout stages' states?
		/// </summary>
		bool IsKnockoutStageState(WorldCupState state);

		/// <summary>
		/// Gets the next knockout stage's state after the specified state. Returns null if it could not be determined, or
		/// if the specified state is the last knockout state.
		/// </summary>
		WorldCupState? GetNextKnockoutState(WorldCupState state);
	
		/// <summary>
		/// Gets the world cup state for the specified match index. Returns null if it could not be determined.
		/// </summary>
		/// <param name="matchIndex">Match index (zero-based).</param>
		WorldCupState? GetStateForMatch(int matchIndex);

		/// <summary>
		/// Gets the knockout round index for the state. Returns null if it could not be determined.
		/// </summary>
		int? GetKnockoutRoundIndexForState(WorldCupState state);

		/// <summary>
		/// Gets the indices of the first and last match for the specified world cup state. Returns null if it could not be 
		/// determined.
		/// </summary>
		void GetMatchesForState(WorldCupState state, out int? firstMatchIndex, out int? lastMatchIndex);

		/// <summary>
		/// Gets the heading to display for the state. Returns null if it could not be determined.
		/// </summary>
		string GetStateDisplayHeading(WorldCupState state);
	}
}
