using UnityEngine;

namespace SimSoc.Debugs
{
	/// <summary>
	/// DEBUG: This allows you to test the tournaments.
	/// </summary>
	/// <remarks>
	/// Add this to a temp GameObject in the start up scene. Remember to delete the GameObject when done testing.
	/// </remarks>
	public class DebugTestTournaments : MonoBehaviour
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	
		[SerializeField, Tooltip("Don't destroy this game object when scenes change. It only works if this object " +
		                         "doesn't have a parent.")]
		protected bool _dontDestroyOnLoad = true;
	
		[SerializeField, Tooltip("ID of the tournament to test.")]
		protected string _tournamentId;

		[SerializeField, Tooltip("Only end the first few matches if they are AI matches. If this is false then it will " +
		                         "end all matches, including human matches. Set it to true if you want to play human " +
		                         "matches.")]
		protected bool _justEndAiMatches;

		[SerializeField, Tooltip("Log the results (of starting the tournament) to the console.")]
		protected bool _showResultsInConsole = true;

		[SerializeField, Tooltip("Max number of human teams (0 to 2).")]
		protected int _maxHumanTeams = 2;
	
		[Header("Input")]
		[SerializeField, Tooltip("Press this key to start a new tournament.")]
		protected KeyCode _startKey = KeyCode.F4;

		[SerializeField, Tooltip("Press this key to end the current match and let team 1 win.")]
		protected KeyCode _endMatchWinTeam1 = KeyCode.F5;
	
		[SerializeField, Tooltip("Press this key to end the current match and let team 2 win.")]
		protected KeyCode _endMatchWinTeam2 = KeyCode.F6;
	
		[SerializeField, Tooltip("Press this key to end the current match and let a random team win.")]
		protected KeyCode _endMatchWinRandomTeam = KeyCode.F7;

		private void Awake()
		{
			if (_dontDestroyOnLoad && transform.parent == null)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		private void Update()
		{
			UpdateInput();
		}
	
		private void UpdateInput()
		{
			if (Input.GetKeyDown(_startKey))
			{
				StartTournament();
			}
		
			if (Input.GetKeyDown(_endMatchWinTeam1))
			{
				EndMatch(0);
			}
			if (Input.GetKeyDown(_endMatchWinTeam2))
			{
				EndMatch(1);
			}
			if (Input.GetKeyDown(_endMatchWinRandomTeam))
			{
				EndMatch(Random.Range(0, 2));
			}
		}

		private void StartTournament()
		{
			if (string.IsNullOrEmpty(_tournamentId))
			{
				LogInfo("id is null or empty.");
				return;
			}

			LogInfo($"Starting tournament: {_tournamentId}");
			SsTournament.DebugTestTournament(_tournamentId, _justEndAiMatches, _showResultsInConsole, _maxHumanTeams);
		}

		private void EndMatch(int winTeamIndex)
		{
			if (SsTournament.Instance == null || !SsTournament.IsTournamentActive || SsTournament.IsTournamentDone ||
			    winTeamIndex < 0 || winTeamIndex > 1)
			{
				LogInfo("Could not end the current match.");
				return;
			}

			var matchInfo = SsTournament.GetMatchInfo();
			if (matchInfo == null)
			{
				LogInfo("Could not end the current match.");
				return;
			}

			var teamIds = matchInfo.teamId;
			if (teamIds == null || teamIds.Length < 2 || string.IsNullOrEmpty(teamIds[0]) ||
			    string.IsNullOrEmpty(teamIds[1]))
			{
				LogInfo("Could not end the current match.");
				return;
			}
		
			var loseTeamIndex = winTeamIndex == 0 ? 1 : 0;
			var winner = teamIds[winTeamIndex];
			var loser = teamIds[loseTeamIndex];
		
			LogInfo("<color=magenta>--------------------------------------------------</color>");
			LogInfo($"About to end match:     WIN: {winner} [{winTeamIndex}]     LOSE: {loser} [{loseTeamIndex}]");
		
			SsTournament.EndMatch(winner, Random.Range(3, 10), loser, Random.Range(0, 3));
		}
	
		/// <summary>
		/// Wrapper for Debug.Log. 
		/// </summary>
		private static void LogInfo(string message, params object[] args)
		{
			message = $"<color=green>[{nameof(DebugTestTournaments)}]</color> {message}     ({Time.frameCount} / {Time.realtimeSinceStartup})";
			Debug.LogFormat(message, args);
		}
	
#endif
	}
}
