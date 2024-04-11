namespace SimSoc
{
	/// <summary>
	/// Delegate for the <see cref="SsTournament.TournamentStartOrLoad"/> event.
	/// </summary>
	/// <param name="tournamentId">ID of the tournament that's about to start or load.</param>
	/// <param name="wasLoaded">Was the tournament loaded?</param>
	public delegate void TournamentStartOrLoadEventHandler(string tournamentId, bool wasLoaded);

	/// <summary>
	/// Delegate for the <see cref="SsTournament.TournamentStartedOrLoaded"/> event.
	/// </summary>
	/// <param name="tournamentId">ID of the tournament that was started or loaded.</param>
	/// <param name="wasLoaded">Was the tournament loaded?</param>
	public delegate void TournamentStartedOrLoadedEventHandler(string tournamentId, bool wasLoaded);

	/// <summary>
	/// Delegate for the <see cref="SsTournament.TournamentMatchEnded"/> event.
	/// </summary>
	/// <param name="tournamentId">ID of the tournament whose match ended.</param>
	/// <param name="hadError">Was there an error when the match ended?</param>
	public delegate void TournamentMatchEndedEventHandler(string tournamentId, bool hadError);
}