using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SimSoc.Debugs
{
	/// <summary>
	/// DEBUG: Logs world cup info to the console.
	/// </summary>
	/// <remarks>
	/// Add this to a temp GameObject in the start up scene. Remember to delete the GameObject when done testing.
	/// </remarks>
	public class DebugLogWorldCupInfo : MonoBehaviour
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	
		[SerializeField, Tooltip("Don't destroy this game object when scenes change. It only works if this object " +
		                         "doesn't have a parent.")]
		protected bool _dontDestroyOnLoad = true;

		[Header("What to log")]
		[SerializeField]
		protected bool _logOverallStats;
	
		[SerializeField]
		protected bool _logGroupStats;

		[SerializeField]
		protected bool _logGroups;

		[Header("Input")]
		[SerializeField, Tooltip("Press this key to log all info.")]
		protected KeyCode _logAllInfoKey = KeyCode.F8;
	
		private IWorldCupTeamStatsController _statsController;
		// List to re-use.
		private readonly List<TournamentTeamStats> _groupStats = new List<TournamentTeamStats>();

		private void Start()
		{
			if (_dontDestroyOnLoad && transform.parent == null)
			{
				DontDestroyOnLoad(gameObject);
			}
		
			SubscribeToEvents(true);
		}

		private void OnDestroy()
		{
			SubscribeToEvents(false);
		}

		private void Update()
		{
			if (Input.GetKeyDown(_logAllInfoKey))
			{
				LogAllInfo();
			}
		}

		private void SubscribeToEvents(bool subscribe)
		{
			SsTournament.TournamentStartOrLoad -= OnTournamentStartOrLoad;
			if (subscribe)
			{
				SsTournament.TournamentStartOrLoad += OnTournamentStartOrLoad;
			}

			if (_statsController != null)
			{
				_statsController.UpdatedWorldCupStats -= OnUpdatedWorldCupStats;
				if (subscribe)
				{
					_statsController.UpdatedWorldCupStats += OnUpdatedWorldCupStats;
				}
			}
		}

		private void OnTournamentStartOrLoad(string tournamentId, bool wasLoaded)
		{
			SubscribeToEvents(false);
		
			var settings = SsMatchSettings.Instance.GetTournamentSettings(tournamentId);
			var worldCupSettings = settings != null ? (WorldCupTournamentSettings)settings.CustomSettings : null;
			var worldCup = worldCupSettings != null ? (IWorldCupTournament)worldCupSettings.CustomController : null;
			_statsController = worldCup != null ? worldCup.StatsController : null;
		
			SubscribeToEvents(true);
		}

		private void OnUpdatedWorldCupStats(IWorldCupTeamStatsController statsController)
		{
			LogAllInfo();
		}

		private void LogAllInfo()
		{
			if (_statsController == null)
			{
				return;
			}
		
			if (_logOverallStats)
			{
				LogTeamStats("Overall Stats", _statsController.OverallStats);	
			}

			if (_logGroupStats)
			{
				LogTeamStats("Group Stats", _statsController.GroupStats);	
			}

			if (_logGroups)
			{
				LogGroups();
			}
		}

		private void LogTeamStats(string heading, List<TournamentTeamStats> teamStats)
		{
			if (teamStats == null || teamStats.Count <= 0)
			{
				LogInfo($"{heading}: No stats to log");
				return;
			}
		
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine(heading);

			for (int i = 0, len = teamStats.Count; i < len; i++)
			{
				var stats = teamStats[i];
				if (stats == null)
				{
					continue;
				}

				sb.Append($"{stats.Rank,-2}   {stats.TeamId,-16}  P:{stats.MatchesPlayed,-2}  ");
				sb.Append($"W:{stats.MatchesWon,-2}  D:{stats.MatchesDrawn,-2}  L:{stats.MatchesLost,-2}  ");
				sb.Append($"GD:{stats.GoalDifference,-2}  PTS:{stats.Points,-2}          GLS: {stats.Goals}");
				var groupName = GetGroupName(stats.GroupIndex);
				sb.Append($"          {groupName}");
				var human = (stats.HumanIndex != -1) ? "[P" + (stats.HumanIndex + 1) + "]" : string.Empty;
				sb.AppendLine($"     {human}");
			}
		
			LogInfo(sb.ToString());
		}
	
		private void LogGroups()
		{
			var tournamentManager = (ITournamentManager)SsTournament.Instance;
			if (tournamentManager == null || tournamentManager.GroupsInfo == null || 
			    tournamentManager.GroupsInfo.Count <= 0)
			{
				return;
			}

			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine("Groups");
			var maxGroups = tournamentManager.GroupsInfo.Count;
			for (var g = 0; g < maxGroups; g++)
			{
				_groupStats.Clear();
				_statsController.GetGroupStats(g, _groupStats);
				if (_groupStats.Count <= 0)
				{
					continue;
				}
			
				var teamStats = _groupStats;
				sb.AppendLine($"{GetGroupName(g)}");
				for (int i = 0, len = teamStats.Count; i < len; i++)
				{
					var stats = teamStats[i];
					if (stats == null)
					{
						continue;
					}

					sb.Append($"{i + 1,-2}   {stats.TeamId,-16}  P:{stats.MatchesPlayed,-2}  ");
					sb.Append($"W:{stats.MatchesWon,-2}  D:{stats.MatchesDrawn,-2}  L:{stats.MatchesLost,-2}  ");
					sb.Append($"GD:{stats.GoalDifference,-2}  PTS:{stats.Points,-2}          GLS: {stats.Goals}");
					sb.Append($"          (OverallRank: {stats.Rank,-2})");
					var groupName = GetGroupName(stats.GroupIndex);
					sb.Append($"          {groupName}");
					var human = (stats.HumanIndex != -1) ? "[P" + (stats.HumanIndex + 1) + "]" : string.Empty;
					sb.AppendLine($"     {human}");
				}
			}

			LogInfo(sb.ToString());
		}

		private string GetGroupName(int groupIndex)
		{
			var tournamentManager = (ITournamentManager)SsTournament.Instance;
			var info = tournamentManager != null && tournamentManager.GroupsInfo != null && groupIndex >= 0 &&
			           groupIndex < tournamentManager.GroupsInfo.Count
				? tournamentManager.GroupsInfo[groupIndex]
				: null;
			return info != null ? info.DisplayName : null;
		}

		/// <summary>
		/// Wrapper for Debug.Log. 
		/// </summary>
		private static void LogInfo(string message, params object[] args)
		{
			message = $"<color=green>[{nameof(DebugLogWorldCupInfo)}]</color> {message}     ({Time.frameCount} / {Time.realtimeSinceStartup})";
			Debug.LogFormat(message, args);
		}
#endif
	}
}
