namespace SimSoc
{
	/// <summary>
	/// Generates the matches for a custom tournament.
	/// </summary>
	public interface ICustomTournamentMatchMaker
	{
		/// <summary>
		/// Creates the initial matches (or all the matches) for a new tournament. Returns true when successful, false 
		/// otherwise.
		/// </summary>
		/// <remarks>
		/// This is not called when a tournament is loaded.
		/// </remarks>
		bool CreateMatches();
	
		/// <summary>
		/// Called after a new tournament started or a tournament was loaded.
		/// </summary>
		/// <remarks>
		/// This is called after <see cref="CreateMatches"/> for a new tournament.
		/// </remarks>
		/// <param name="wasLoaded">Was the tournament loaded?</param>
		void OnNewOrLoaded(bool wasLoaded);

		/// <summary>
		/// Called when a match ended. Returns true when successful, false otherwise.
		/// </summary>
		bool OnMatchEnded();
	
		/// <summary>
		/// Called when a tournament was ended.
		/// </summary>
		void OnTournamentEnded();
	}
}
