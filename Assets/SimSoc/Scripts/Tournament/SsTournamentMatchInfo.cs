using UnityEngine;
using System.Collections;

/// <summary>
/// Tournament match info (e.g. team IDs and results).
/// </summary>
public class SsTournamentMatchInfo
{
	// Public
	//-------

	// REMINDER: Add team variables to CopyTeam().

	// Save/load the following:
	public int[] teamScore = new int[2];			// Teams score (-1 indicates match has Not been played)
	public string[] teamId = new string[2];			// Team IDs
	public int matchDay = -1;						// Match day index


	// Do Not save the following, they are dynamically calculated:
	public int homeTeam;							// Index of team that plays at home (0 = left, 1 = right, -1 = none).
	public bool[] isPlayer = new bool[2];			// Are the teams player teams?
	public int[] humanIndex = {-1, -1};				// Human player index (e.g. 0 = player 1, 1 = player 2, -1 = AI team)
	public string[] teamName = new string[2];		// Team name for UI
	public bool needsWinner;						// Indicates if the match needs a winner.
	public string displayHeading;					// Heading to display above the match.
	public int knockoutRoundIndex = -1;				// Knockout round index.
	public int knockoutRoundMatchIndex = -1;		// Match index in the knockout round. 

	// REMINDER: Add team variables to CopyTeam().


	// Properties
	//-----------
	public bool MatchDone
	{
		get { return((teamScore[0] >= 0) && (teamScore[1] >= 0)); }
	}
	


	// Methods
	//--------
	/// <summary>
	/// Constructor.
	/// </summary>
	public SsTournamentMatchInfo()
	{
		Init();
	}


	/// <summary>
	/// Init this instance.
	/// </summary>
	public void Init()
	{
		homeTeam = -1;
		needsWinner = false;

		for (int i = 0; i < 2; i++)
		{
			teamScore[i] = -1;
			teamId[i] = null;

			isPlayer[i] = false;
			humanIndex[i] = -1;
			teamName[i] = null;
		}
	}


	/// <summary>
	/// Gets the team that won. Returns null if match has not been played, or it was a draw.
	/// </summary>
	public string GetWinner()
	{
		if (!MatchDone || teamScore[0] == teamScore[1])
		{
			return null;
		}

		return teamScore[0] > teamScore[1] ? teamId[0] : teamId[1];
	}


	/// <summary>
	/// Does the match contain any of the specified teams?
	/// </summary>
	/// <returns>The teams.</returns>
	/// <param name="teamIds">Team identifiers.</param>
	public bool HasOneOfTeams(string[] teamIds)
	{
		if ((teamIds == null) || (teamIds.Length <= 0))
		{
			return (false);
		}
		int i;
		for (i = 0; i < teamIds.Length; i++)
		{
			if ((teamId[0] == teamIds[i]) || 
			    (teamId[1] == teamIds[i]))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Copy team settings from another match, for the specified team.
	/// </summary>
	/// <returns>The team.</returns>
	/// <param name="fromInfo">From info.</param>
	/// <param name="fromIndex">From index.</param>
	/// <param name="toIndex">To index.</param>
	public void CopyTeam(SsTournamentMatchInfo fromInfo, int fromIndex, int toIndex)
	{
		teamScore[toIndex] = fromInfo.teamScore[fromIndex];
		teamId[toIndex] = fromInfo.teamId[fromIndex];
		isPlayer[toIndex] = fromInfo.isPlayer[fromIndex];
		humanIndex[toIndex] = fromInfo.humanIndex[fromIndex];
		teamName[toIndex] = fromInfo.teamName[fromIndex];
	}
}
