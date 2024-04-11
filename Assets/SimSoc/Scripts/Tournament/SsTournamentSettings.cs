using UnityEngine;
using System.Collections;
using SimSoc;

/// <summary>
/// Tournament settings.
/// </summary>
[System.Serializable]
public class SsTournamentSettings
{
	// Public
	//-------
	[Tooltip("Unique ID. It is used to identify the tournament. Do Not change this after the game has been released.")]
	public string id;

	[Tooltip("Name to display on UI.")]
	public string displayName;

	[Tooltip("Tournament type.")]
	public SsTournament.tournamentTypes type;

	[Tooltip("Max teams in the tournament. Must be at least 4, but not more than the number of teams in the game.\n" + 
	         "Must be a power of 2 for Single Elimination (e.g. 4, 8, 16, 32).")]
	[Range(SsTournament.minTeams, SsTournament.maxTeams)]	// Limit to prevent tournaments taking too long.
	public int maxTeams = 4;

	[Tooltip("The number of times each team must face each other (e.g. how many times must Team A face Team B).\n" + 
	         "This only applies to the Log Tournament.")]
	[Range(1, 5)]			// Limit to prevent tournaments taking too long.
	public int numFaceEachOther = 1;	// Also referred to as "legs".

	[Tooltip("Percentage chance that human teams can start in the same group, for Elimination tournaments.")]
	[Range(0, 100)]
	public float humansInSameGroupChance = 10.0f;

	[Tooltip("Percentage chance that human teams can start in the same match, if they start in the same group, for Elimination tournaments.")]
	[Range(0, 100)]
	public float humansInSameMatchChance = 10.0f;

	[Tooltip("How a team's field is selected in the tournament matches. It will try to stick to this, but may not always be possible.")]
	public SsTournament.fieldSelectSequences fieldSelectSequence;

	[Tooltip("Max AI difficulty for the tournament. Only valid if teams/players have skills setup for different difficulties.")]
	public int maxAiDifficulty;
	
	
	// Calculated variables: Each tournament type uses one or more of these
	[System.NonSerialized]
	public int minMatches;				// Min matches needed for each team to play every other at least once (i.e. in 1 leg).

	[System.NonSerialized]
	public int maxMatches;				// Max matches in the tournament.

	[System.NonSerialized]
	public int matchesPerRound;			// Number of matches per round. (Log Tournament)

	[System.NonSerialized]
	public int maxRounds;				// Max number of rounds. (Log Tournament, Single Elimination) (For Log Tournament this excludes the final match.)

	[System.NonSerialized]
	public int minRounds;				// Min rounds needed for each team to play every other at least once (i.e. in 1 leg).

	[System.NonSerialized]
	public int matchesInFirstRound;		// Number of matches in the first round. (Single Elimination)

	[System.NonSerialized]
	public int groupsInFirstRound;		// Number of groups in first round. (Single Elimination)

	[System.NonSerialized]
	public int[] matchesInRound;		// Number of matches in each round. (Single Elimination)

	[System.NonSerialized]
	public int[] winnerNextMatch;		// Index of match to which to move the winner

	/// <summary>
	/// Gets the custom tournament settings. Returns null if the tournament doesn't have custom settings.
	/// </summary>
	public ICustomTournamentSettings CustomSettings { get; private set; }
	

	// Methods
	//--------

	/// <summary>
	/// Constructor with initialization parameters.
	/// </summary>
	/// <param name="customSettings">Custom tournament settings, if any.</param>
	public SsTournamentSettings(ICustomTournamentSettings customSettings = null)
	{
		CustomSettings = customSettings;
	}
	
	/// <summary>
	/// Init this instance.
	/// </summary>
	public void Init()
	{
		SsTournament.InitSettings(this);
	}
}
