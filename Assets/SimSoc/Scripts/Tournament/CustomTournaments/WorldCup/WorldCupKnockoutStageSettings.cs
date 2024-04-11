namespace SimSoc
{
	/// <summary>
	/// World cup knockout stage settings.
	/// </summary>
	public class WorldCupKnockoutStageSettings
	{
		/// <summary>
		/// Number of matches in the knockout stage. (0 if this stage is not used.)
		/// </summary>
		public int NumMatches { get; private set; }
	
		/// <summary>
		/// The number of matches this knockout stage usually has.
		/// </summary>
		public int DefaultNumMatches { get; }

		/// <summary>
		/// The number of teams needed for all the matches combined.
		/// </summary>
		public int TeamsNeededForAllMatches => DefaultNumMatches * 2;

		public WorldCupKnockoutStageSettings(int defaultNumberOfMatches = 0)
		{
			DefaultNumMatches = defaultNumberOfMatches;
			NumMatches = defaultNumberOfMatches;
		}

		/// <summary>
		/// Updates the value of <see cref="NumMatches"/> based on how many teams will advance to the knockout stage.
		/// </summary>
		/// <param name="numberOfAdvanceTeams">The total number of teams from the group stage to advance to the knockout
		/// stage.</param>
		/// <param name="forceToZero">Force the number of matches to zero.</param>
		public virtual void UpdateNumberOfMatches(int numberOfAdvanceTeams, bool forceToZero = false)
		{
			NumMatches = !forceToZero && TeamsNeededForAllMatches <= numberOfAdvanceTeams ? DefaultNumMatches : 0;
		}
	}
}
