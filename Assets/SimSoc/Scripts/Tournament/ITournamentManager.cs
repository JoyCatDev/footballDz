using System.Collections.Generic;

namespace SimSoc
{
	/// <summary>
	/// The tournament manager.
	/// </summary>
	public interface ITournamentManager
	{
		/// <summary>
		/// Active tournament ID.
		/// </summary>
		/// <remarks>
		/// Can be empty/null if no tournament is active/loaded.
		/// </remarks>
		string TournamentId { get; }
	
		/// <summary>
		/// Active tournament type.
		/// </summary>
		SsTournament.tournamentTypes TournamentType { get; }

		/// <summary>
		/// Random seed (e.g. for reproducing rankings).
		/// </summary>
		int RandomSeed { get; }
	
		/// <summary>
		/// Current match index (zero-based).
		/// </summary>
		int MatchIndex { get; }
	
		/// <summary>
		/// IDs of the teams in the active tournament.
		/// </summary>
		string[] TeamIds { get; }
	
		/// <summary>
		/// Human team IDs in the active tournament.
		/// </summary>
		string[] PlayerTeamIds { get; }
	
		/// <summary>
		/// Active tournament settings.
		/// </summary>
		/// <remarks>
		/// Can return null if no tournament is active/loaded.
		/// </remarks>
		SsTournamentSettings Settings { get; }
	
		/// <summary>
		/// Matches in the active tournament.
		/// </summary>
		SsTournamentMatchInfo[] MatchesInfo { get; }
	
		/// <summary>
		/// Groups info of the active tournament.
		/// </summary>
		List<TournamentGroupInfo> GroupsInfo { get; }
	
		/// <summary>
		/// Counts how many times a new or loaded tournament started.
		/// </summary>
		int NewOrLoadedCount { get; }

		/// <summary>
		/// Gets the group index in which the team has been placed. Can return null (e.g. groups haven't been setup,
		/// team not found, etc.)
		/// </summary>
		int? GetGroupIndex(string teamId);
	
		/// <summary>
		/// Gets the group info in which the team has been placed. Can return null (e.g. groups haven't been setup,
		/// team not found, etc.)
		/// </summary>
		TournamentGroupInfo GetGroup(string teamId);
	}
}
