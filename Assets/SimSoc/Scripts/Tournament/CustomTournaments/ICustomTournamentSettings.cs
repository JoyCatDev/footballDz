namespace SimSoc
{
	/// <summary>
	/// Settings for a custom tournament.
	/// </summary>
	public interface ICustomTournamentSettings
	{
		/// <summary>
		/// Gets the base tournament settings.
		/// </summary>
		SsTournamentSettings TournamentSettings { get; }
	
		/// <summary>
		/// Gets the custom controller for the tournament. Returns null if no controller is needed.
		/// </summary>
		ICustomTournamentController CustomController { get; }
	
		/// <summary>
		/// Gets the custom match maker for the tournament. Returns null if no match maker is needed.
		/// </summary>
		ICustomTournamentMatchMaker MatchMaker { get; }
	
		/// <summary>
		/// Gets whether the <see cref="SsTournamentSettings.fieldSelectSequence"/> must be used for selecting fields.
		/// </summary>
		bool UseFieldSelectSequence { get; }
	
		/// <summary>
		/// Creates the <see cref="SsTournamentSettings"/> from the custom settings.
		/// </summary>
		SsTournamentSettings CreateSettings();

		/// <summary>
		/// Initializes the settings.
		/// </summary>
		void Initialize(ITournamentManager tournamentManager);
	}
}
