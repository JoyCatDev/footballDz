namespace SimSoc
{
	/// <summary>
	/// Controls some logic for a custom tournament.
	/// </summary>
	public interface ICustomTournamentController
	{
		/// <summary>
		/// Does validation before starting a tournament. Returns true when successful, false otherwise.
		/// </summary>
		bool StartTournamentValidation();

		/// <summary>
		/// Called just after the team IDs have been generated. This can be used to change the order of the team IDs. 
		/// </summary>
		void OnGeneratedTeamIds(ref string[] teamIds);

		/// <summary>
		/// Sets up the tournament groups. Returns true when successful, false otherwise.
		/// </summary>
		bool SetupGroups();

		/// <summary>
		/// Called when the initial matches have been setup.
		/// </summary>
		void OnSetupMatches();

		/// <summary>
		/// Called after a new tournament started or a tournament was loaded. It does some calculations and sets up data
		/// that is not saved/loaded.
		/// </summary>
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
