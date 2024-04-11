namespace SimSoc
{
	/// <summary>
	/// Tournament group info.
	/// </summary>
	public class TournamentGroupInfo
	{
		// Save/load the following:
	
		// Team IDs.
		public string[] TeamIds;
	
		// Do not save the following, they are dynamically updated:
	
		// The group's name to display on UI.
		public string DisplayName;
	}
}
