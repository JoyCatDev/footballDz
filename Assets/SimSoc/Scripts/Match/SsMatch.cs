using UnityEngine;
using System.Collections;


/// <summary>
/// Match.
/// </summary>
public class SsMatch : MonoBehaviour {

	// Const/Static
	//-------------
	public const float defaultDuration = 10.0f;				// Default match duration (minutes)
	public const float demoDuration = 10.0f;					// Demo build match duration (minutes)
	public const float demoTournamentDuration = 0.34f;		// Demo build tournament match duration (minutes)


	// Enums
	//------
	// Match types
	public enum matchTypes
	{
		/// <summary>
		/// Friendly (i.e. Not part of a tournament).
		/// </summary>
		friendly = 0,

		/// <summary>
		/// Tournament match.
		/// </summary>
		tournament,
	}


	// Match states
	public enum states
	{
		/// <summary>
		/// Loading match resources. When loading is complete then it will change to loadingDone.
		/// </summary>
		loading = 0, 

		/// <summary>
		/// Waiting for each team to load their players.
		/// </summary>
		loadingPlayers,

		/// <summary>
		/// Loading is complete. After 1 frame it will change to intro.
		/// </summary>
		loadingDone, 

		/// <summary>
		/// Match intro animation sequence. A short delay before changing to kickOff.
		/// </summary>
		intro, 

		/// <summary>
		/// Kick off. At start of match or at half time. Player has the ball at the centre mark.
		/// </summary>
		kickOff, 

		/// <summary>
		/// Play. Ball is in play.
		/// </summary>
		play,  

		/// <summary>
		/// Pre-throw in. Short delay before changing to throwIn.
		/// </summary>
		preThrowIn, 

		/// <summary>
		/// Throw in. Player holds ball to throw in.
		/// </summary>
		throwIn,

		/// <summary>
		/// Pre-corner kick. Short delay before changing to cornerKick.
		/// </summary>
		preCornerKick, 

		/// <summary>
		/// Corner kick. Player has the ball in a corner.
		/// </summary>
		cornerKick, 

		/// <summary>
		/// Pre-goal kick. Short delay before changing to goalKick.
		/// </summary>
		preGoalKick, 

		/// <summary>
		/// Goal kick. Goalkeeper has the ball.
		/// </summary>
		goalKick, 

		/// <summary>
		/// Goal. Short delay before changing to goalCelebration.
		/// </summary>
		goal,

		/// <summary>
		/// Goal celebration. Players celebrate goal. Short delay before changing to kickOff.
		/// </summary>
		goalCelebration,

		/// <summary>
		/// End of half. Short delay before changin to kickOff.
		/// </summary>
		halfTime, 

		/// <summary>
		/// Full time. After 1 frame it will change to preMatchResults.
		/// </summary>
		fullTime, 

		/// <summary>
		/// Pre-match results. Short delay before changing to matchResults.
		/// </summary>
		preMatchResults,

		/// <summary>
		/// Showing match results.
		/// </summary>
		matchResults, 

	}


	// Loading steps
	public enum loadingSteps
	{
		loadTeam1,
		loadTeam2,
		loadBall,
		initTeams,
	}


	// Private
	//--------
	static private SsMatch instance;

	private SsFieldProperties field;				// Reference to the field
	private SsBall ball;							// Reference to the ball

	private states state = states.loading;			// Match state
	private float stateTime;						// Time the state started
	private int stateUpdateCount;					// Count how many updates have passed since the state changed
	private bool isDone;							// Is the match done?

	private SsNewMatch loadingMatch;				// Loading new match settings
	private loadingSteps loadingStep;				// Loading step
	private int loadingFrame;						// Loading frame count

	private float lastDt;							// Delta time of the last/previous Update method.

	private float duration;							// The length of the match duration (minutes)
	private int difficulty;							// Match difficulty
	private int matchHalf;							// The match half (0 = first half, 1 = second half)
	private float maxHalfTime;						// Max time for the half (seconds).
	private float halfTimeTimer;					// Keeps track of the current half's time. Set to zero at start of each half.
	private float matchTimer;						// Keeps track of the match time.
	private matchTypes matchType;					// Match type (e.g. friendly, tournament)
	private bool needsWinner;						// Does the match need a winner?

	private SsTeam[] teams;							// The 2 teams
	private SsTeam kickoffTeam;						// Team to kickoff
	private SsTeam originalKickoffTeam;				// Kickoff team at start of the match
	private SsTeam halfTimeKickoffTeam;				// Team to kickoff at half time
	private SsTeam leftTeam;						// Team playing on the left side (from -X to +X)
	private SsTeam rightTeam;						// Team playing on the right side (from +X to -X)
	private SsTeam ballTeam;						// Team who has the ball
	private SsTeam throwInTeam;						// Team that must throw in, goal kick, etc.

	private SsPlayer ballPlayer;					// Player who has the ball
	private SsPlayer prevBallPlayer;				// Previous player who had the ball
	private SsPlayer lastBallPlayer;				// Last player who touched the ball
	private int ballChangePlayerCounter;			// Counts how many times the ball changes players. Used by the AI.

	private SsTeam goalTeam;						// Team who scored a goal last
	private SsPlayer goalScorer;					// Player who scored the goal

	private bool halfTimeUiDidClose;				// Did the half time UI close?

	public event System.Action<states> onStateChanged;		// On state changed action event



	// Properties
	//-----------
	static public SsMatch Instance
	{
		get { return(instance); }
	}


	public states State
	{
		get { return(state); }
	}


	public float StateTime
	{
		get { return(stateTime); }
	}


	public bool IsDone
	{
		get { return(isDone); }
	}


	/// <summary>
	/// Is the match in a state in which play must be updated? (i.e. match timer, ball in play, and human and AI updates)
	/// </summary>
	/// <value>The state of the in play.</value>
	public bool InPlayState
	{
		get
		{
			if ((state == states.kickOff) || 
			    (state == states.play) || 
			    (state == states.preThrowIn) || 
			    (state == states.throwIn) || 
			    (state == states.preCornerKick) || 
			    (state == states.cornerKick) || 
			    (state == states.preGoalKick) || 
			    (state == states.goalKick) || 
			    (state == states.goal))
			{
				return (true);
			}

			return (false);
		}
	}


	public int Difficulty
	{
		get { return(difficulty); }
	}


	public int MatchHalf
	{
		get { return(matchHalf); }
	}


	public float MaxHalfTime
	{
		get { return(maxHalfTime); }
	}


	public float HalfTimeTimer
	{
		get { return(halfTimeTimer); }
	}


	public float MatchTimer
	{
		get { return(matchTimer); }
	}


	/// <summary>
	/// Delta time of the last/previous Update method.
	/// </summary>
	/// <value>The last dt.</value>
	public float LastDt
	{
		get { return(lastDt); }
	}


	public SsTeam[] Teams
	{
		get { return(teams); }
	}


	public SsTeam KickoffTeam
	{
		get { return(kickoffTeam); }
	}


	public SsTeam OriginalKickoffTeam
	{
		get { return(originalKickoffTeam); }
	}


	public SsTeam HalfTimeKickoffTeam
	{
		get { return(halfTimeKickoffTeam); }
	}


	/// <summary>
	/// Team playing on the left side (from -X to +X).
	/// </summary>
	/// <value>The left team.</value>
	public SsTeam LeftTeam
	{
		get { return(leftTeam); }
	}


	/// <summary>
	/// Team playing on the right side (from +X to -X).
	/// </summary>
	/// <value>The right team.</value>
	public SsTeam RightTeam
	{
		get { return(rightTeam); }
	}


	public SsTeam BallTeam
	{
		get { return(ballTeam); }
	}


	/// <summary>
	/// Team that must throw in, goal kick, etc.
	/// </summary>
	/// <value>The throw in team.</value>
	public SsTeam ThrowInTeam
	{
		get { return(throwInTeam); }
		set
		{
			throwInTeam = value;
		}
	}


	/// <summary>
	/// Team who scored a goal last.
	/// </summary>
	/// <value>The goal team.</value>
	public SsTeam GoalTeam
	{
		get { return(goalTeam); }
	}


	public SsPlayer GoalScorer
	{
		get { return(goalScorer); }
	}


	public SsPlayer BallPlayer
	{
		get { return(ballPlayer); }
	}


	public SsPlayer PrevBallPlayer
	{
		get { return(prevBallPlayer); }
	}


	public SsPlayer LastBallPlayer
	{
		get { return(lastBallPlayer); }
	}


	public int BallChangePlayerCounter
	{
		get { return(ballChangePlayerCounter); }
		set
		{
			ballChangePlayerCounter = value;
		}
	}



	// Methods
	//--------

	/// <summary>
	/// Creates the match game object. NOTE: This method is called from SsNewMatch. Use StartMatch or StartMatchTest to start a new match.
	/// </summary>
	/// <returns>The match.</returns>
	static public SsMatch CreateMatch()
	{
		SsMatch match = null;
		GameObject go = new GameObject("Match");
		if (go != null)
		{
			match = go.AddComponent<SsMatch>();
			if (match == null)
			{
				// Failed to add component, no longer need the game object
				Destroy(go);
			}
		}

		return (match);
	}


	/// <summary>
	/// Starts the match.
	/// </summary>
	/// <returns>The match.</returns>
	/// <param name="teamResource1">Team resource1. The path and prefab name in the Resources folder, but excluding the "Resources" prefix (e.g. "Teams/Red Team/Red Team").</param>
	/// <param name="teamResource2">Team resource2. The path and prefab name in the Resources folder, but excluding the "Resources" prefix (e.g. "Teams/Red Team/Red Team").</param>
	/// <param name="teamInput1">Team input1.</param>
	/// <param name="teamInput2">Team input2.</param>
	/// <param name="ballResource">Ball resource. The path and prefab name in the Resources folder, but excluding the "Resources" prefix (e.g. "Balls/Generic Ball").</param>
	/// <param name="fieldScene">Field scene to load.</param>
	/// <param name="optionalLoadingScene">Optional loading scene.</param>
	/// <param name="overrideDuration">Override duration. -1 = Do Not override</param>
	/// <param name="needsWinner">Does the match need a winner?</param>
	/// <param name="overrideDifficulty">Override the match difficulty. -1 = Do Not override</param>
	/// <param name="destroyPersistentAudio">Destroy all persistent audio game objects (e.g. menu sounds).</param>
	static public bool StartMatch(string teamResource1, string teamResource2, 
	                              SsInput.inputTypes teamInput1, SsInput.inputTypes teamInput2, 
	                              string ballResource, 
	                              string fieldScene, 
	                              string optionalLoadingScene,
	                              float overrideDuration,
	                              bool needsWinner, 
	                              int overrideDifficulty = -1, 
	                              bool destroyPersistentAudio = true)
	{
		if ((string.IsNullOrEmpty(teamResource1)) || (string.IsNullOrEmpty(teamResource2)) || 
		    (string.IsNullOrEmpty(ballResource)))
		{
			return (false);
		}

		GameObject go = new GameObject("NewMatch");
		if (go == null)
		{
			return (false);
		}
		
		SsNewMatch newMatch = go.AddComponent<SsNewMatch>();
		if (newMatch == null)
		{
			Destroy(go);
			return (false);
		}
		
		newMatch.teamResources = new string[2];
		newMatch.teamResources[0] = teamResource1;
		newMatch.teamResources[1] = teamResource2;
		
		newMatch.teamInput = new SsInput.inputTypes[2];
		newMatch.teamInput[0] = teamInput1;
		newMatch.teamInput[1] = teamInput2;
		
		newMatch.ballResource = ballResource;
		newMatch.fieldScene = fieldScene;
		newMatch.optionalLoadingScene = optionalLoadingScene;
		newMatch.overrideDuration = overrideDuration;
		newMatch.needsWinner = needsWinner;
		newMatch.overrideDifficulty = overrideDifficulty;

		if (destroyPersistentAudio)
		{
			SsPersistentAudio.DestroyAll();
		}
		
		return (true);
	}


	/// <summary>
	/// Start a match to test.
	/// </summary>
	/// <returns>The match test.</returns>
	/// <param name="teamPrefab1">Team prefab1.</param>
	/// <param name="teamPrefab2">Team prefab2.</param>
	/// <param name="teamInput1">Input team1.</param>
	/// <param name="teamInput2">Input team2.</param>
	/// <param name="ballPrefab">Ball prefab.</param>
	/// <param name="fieldScene">Field scene to load.</param>
	/// <param name="optionalLoadingScene">Optional loading scene.</param>
	/// <param name="overrideDuration">Override duration. -1 = Do Not override</param>
	/// <param name="needsWinner">Does the match need a winner?</param>
	/// <param name="overrideDifficulty">Override the match difficulty. -1 = Do Not override</param>
	/// <param name="destroyPersistentAudio">Destroy all persistent audio game objects (e.g. menu sounds).</param>
	static public bool StartMatchTest(SsTeam teamPrefab1, SsTeam teamPrefab2, 
	                                  SsInput.inputTypes teamInput1, SsInput.inputTypes teamInput2, 
	                                  SsBall ballPrefab, 
	                                  string fieldScene, 
	                                  string optionalLoadingScene,
	                                  float overrideDuration,
	                                  bool needsWinner, 
	                                  int overrideDifficulty = -1, 
	                                  bool destroyPersistentAudio = true)
	{
		if ((teamPrefab1 == null) || (teamPrefab2 == null))
		{
			return (false);
		}

		GameObject go = new GameObject("NewMatch");
		if (go == null)
		{
			return (false);
		}

		SsNewMatch newMatch = go.AddComponent<SsNewMatch>();
		if (newMatch == null)
		{
			Destroy(go);
			return (false);
		}

		newMatch.teamPrefabs = new SsTeam[2];
		newMatch.teamPrefabs[0] = teamPrefab1;
		newMatch.teamPrefabs[1] = teamPrefab2;

		newMatch.teamInput = new SsInput.inputTypes[2];
		newMatch.teamInput[0] = teamInput1;
		newMatch.teamInput[1] = teamInput2;

		newMatch.ballPrefab = ballPrefab;
		newMatch.fieldScene = fieldScene;
		newMatch.optionalLoadingScene = optionalLoadingScene;
		newMatch.overrideDuration = overrideDuration;
		newMatch.needsWinner = needsWinner;
		newMatch.overrideDifficulty = overrideDifficulty;

		if (destroyPersistentAudio)
		{
			SsPersistentAudio.DestroyAll();
		}

		return (true);
	}


	/// <summary>
	/// Starts the match from the menus. It checks if demo is active and limits resources.
	/// </summary>
	/// <returns>The match from menus.</returns>
	/// <param name="teamIds">Team identifiers. null = random.</param>
	/// <param name="leftIsPlayer">Is left team a player team? (i.e. teamIds[0])</param>
	/// <param name="rightIsPlayer">Is right team a player team? (i.e. teamIds[1])</param>
	/// <param name="leftPlayerInput">Left player input type. Ignored if left team is AI.</param>
	/// <param name="rightPlayerInput">Right player input type. Ignored if right team is AI.</param>
	/// <param name="fieldId">Field identifier. null = random.</param>
	/// <param name="ballId">Ball identifier. null = random.</param>
	/// <param name="needsWinner">Does the match need a winner?</param>
	static public bool StartMatchFromMenus(string[] teamIds, 
											bool leftIsPlayer, bool rightIsPlayer,
	                                       SsInput.inputTypes leftPlayerInput, SsInput.inputTypes rightPlayerInput, 
	                                       string fieldId, string ballId,
	                                       bool needsWinner)
	{
		if ((SsResourceManager.Instance == null) || 
		    (SsSceneManager.Instance == null) || 
		    (SsSettings.Instance == null))
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Failed to start the match.");
			#endif //UNITY_EDITOR
			
			return (false);
		}
		
		SsResourceManager.SsTeamResource team1, team2;
		SsResourceManager.SsBallResource ball;
		SsInput.inputTypes[] input = new SsInput.inputTypes[2];
		SsSceneManager.SsSceneResource field;
		bool result;


		// Players
		//--------
		if ((teamIds == null) || (teamIds.Length < 1) || (string.IsNullOrEmpty(teamIds[0])))
		{
			if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
			{
			 	// DEMO
				if (leftIsPlayer)
				{
					team1 = SsResourceManager.Instance.GetRandomTeam(null, SsSettings.Instance.demoPlayerTeams);
				}
				else
				{
					team1 = SsResourceManager.Instance.GetRandomTeam(null);
				}
			}
			else
			{
				team1 = SsResourceManager.Instance.GetRandomTeam(null);
			}
		}
		else
		{
			team1 = SsResourceManager.Instance.GetTeam(teamIds[0]);
		}
		if (team1 == null)
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Resource Manager does not have team ID: " + teamIds[0]);
			#endif //UNITY_EDITOR
			
			return (false);
		}


		if ((teamIds == null) || (teamIds.Length < 2) || (string.IsNullOrEmpty(teamIds[1])))
		{
			if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
			{
				// DEMO
				if (rightIsPlayer)
				{
					team2 = SsResourceManager.Instance.GetRandomTeam(team1.id, SsSettings.Instance.demoPlayerTeams);
				}
				else
				{
					team2 = SsResourceManager.Instance.GetRandomTeam(team1.id);
				}
			}
			else
			{
				team2 = SsResourceManager.Instance.GetRandomTeam(team1.id);
			}
		}
		else
		{
			team2 = SsResourceManager.Instance.GetTeam(teamIds[1]);
		}
		if (team2 == null)
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Resource Manager does not have team ID: " + teamIds[1]);
			#endif //UNITY_EDITOR
			
			return (false);
		}

		
		// Input
		//------
		if (leftIsPlayer)
		{
			input[0] = leftPlayerInput;
		}
		else
		{
			input[0] = SsInput.inputTypes.ai;
		}

		if (rightIsPlayer)
		{
			input[1] = rightPlayerInput;
		}
		else
		{
			input[1] = SsInput.inputTypes.ai;
		}


		// Calibrate at least once
		if (SsSettings.didCalibrate == false)
		{
			SsMatchInputManager.CalibrateAccelerometer(true);
		}
		
		
		// Field
		//------
		if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
		{
			// DEMO: Only play first field
			field = SsSceneManager.Instance.GetField(SsSettings.Instance.demoFieldId);
		}
		else
		{
			field = SsSceneManager.Instance.GetField(fieldId);
		}
		
		
		// Ball
		//-----
		if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
		{
			// DEMO: Only play with first ball
			ball = SsResourceManager.Instance.GetBall(SsSettings.Instance.demoBallId);
		}
		else
		{
			ball = SsResourceManager.Instance.GetBall(ballId);
		}
		if (ball == null)
		{
			#if UNITY_EDITOR
			Debug.LogError("ERROR: Resource Manager could not select a ball. ID: " + ballId);
			#endif //UNITY_EDITOR
			
			return (false);
		}
		
		
		// Start the match
		//----------------
		result = SsMatch.StartMatch(team1.path, team2.path, 
		                            input[0], 
		                            input[1], 
		                            ball.path, 
		                            (field != null) ? field.sceneName : null, 
		                            SsSceneManager.Instance.defaultLoadingScene, 
		                            -1,
		                            needsWinner);
		#if UNITY_EDITOR
		if (result == false)
		{
			Debug.LogError("ERROR: Failed to start the match.");
		}
		#endif //UNITY_EDITOR

		if (result)
		{
			SsVolumeController.FadeVolumeOut(SsVolumeController.menuMusicFadeOutDuration);
		}

		return (result);
	}


	/// <summary>
	/// Starts the match with the new settings.
	/// </summary>
	/// <returns>The match with settings.</returns>
	/// <param name="newMatch">New match.</param>
	public bool StartMatchWithSettings(SsNewMatch newMatch)
	{
		SsNewMatch nm = newMatch;

		loadingMatch = nm;

		// Will load resources if there are no prefabs
		if ((nm.teamPrefabs == null) || (nm.teamPrefabs.Length < 2))
		{
			nm.teamPrefabs = new SsTeam[2];
		}

		teams = new SsTeam[2];


		// Stats
		//------
		matchType = SsSettings.selectedMatchType;
		needsWinner = nm.needsWinner;
		
		if (nm.overrideDuration > 0.0f)
		{
			duration = nm.overrideDuration;
		}
		else if (SsMatchSettings.Instance != null)
		{
			duration = SsMatchSettings.Instance.matchDuration;
		}
		else
		{
			duration = defaultDuration;
		}
		
		if (nm.overrideDifficulty >= 0)
		{
			difficulty = nm.overrideDifficulty;
		}
		else
		{
			if (matchType == matchTypes.tournament)
			{
				difficulty = SsTournament.tournamentDifficulty;
			}
			else
			{
				difficulty = (SsMatchSettings.Instance != null) ? SsMatchSettings.Instance.matchDifficulty : 0;
			}
		}
		
		
		if ((SsSettings.Instance != null) && (SsSettings.Instance.demoBuild))
		{
			// DEMO
			if (nm.overrideDuration > 0.0f)
			{
				#if UNITY_EDITOR
				if (SsSettings.LogInfoToConsole)
				{
					Debug.Log("Game is in demo mode, but the match duration has been overrided to " + nm.overrideDuration + "." + 
					          " Possibly caused by running the field scene in the editor.");
				}
				#endif //UNITY_EDITOR
			}
			else
			{
				if (matchType == matchTypes.tournament)
				{
					duration = demoTournamentDuration;
				}
				else
				{
					duration = demoDuration;
				}
			}
		}
		
		
		maxHalfTime = (duration / 2.0f) * 60.0f;
		halfTimeTimer = 0.0f;
		matchTimer = 0.0f;


#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			Debug.Log("START MATCH:  Duration: " + duration + 
			          "      Type: " + matchType + 
			          "      Needs Winner: " + needsWinner + 
			          "      Difficulty: " + difficulty);
		}
#endif //UNITY_EDITOR


		return (true);
	}


	/// <summary>
	/// End the match and load the next scene.
	/// </summary>
	/// <returns>The match and load next scene.</returns>
	public void EndMatchAndLoadNextScene()
	{
		SsVolumeController.FadeVolumeOut(SsVolumeController.matchMusicFadeOutTime);

		SsNewMatch.DestroyAll();
		if (SsSceneManager.Instance != null)
		{
			SsSceneLoader.LoadScene(SsSceneManager.Instance.matchEndScene, 
			                        SsSceneManager.Instance.defaultLoadingScene);
		}
	}


	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
		SetState(states.loading);
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;

		CleanUp();
	}


	/// <summary>
	/// Clean up. Includes freeing resources and clearing references.
	/// </summary>
	/// <returns>The up.</returns>
	void CleanUp()
	{
		int i;

		field = null;
		ball = null;

		if (teams != null)
		{
			for (i = 0; i < teams.Length; i++)
			{
				teams[i] = null;
			}
		}
		teams = null;

		kickoffTeam = null;
		originalKickoffTeam = null;
		halfTimeKickoffTeam = null;
		leftTeam = null;
		rightTeam = null;
		ballTeam = null;
		throwInTeam = null;

		goalTeam = null;
		goalScorer = null;
		
		ballPlayer = null;
		prevBallPlayer = null;
		lastBallPlayer = null;

		loadingMatch = null;
	}


	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start()
	{
		field = SsFieldProperties.Instance;
		ball = SsBall.Instance;

		ResetMe();
	}


	/// <summary>
	/// Reset this instance.
	/// </summary>
	public void ResetMe()
	{
		matchHalf = 0;
		halfTimeTimer = 0.0f;
		matchTimer = 0.0f;
	}


	/// <summary>
	/// Test if the match is busy loading.
	/// </summary>
	/// <returns>True if match is busy loading.</returns>
	/// <param name="checkIfAllPlayersAreLoaded">Check if all players are loaded as well.</param>
	public bool IsLoading(bool checkIfAllPlayersAreLoaded)
	{
		if (state == states.loading)
		{
			return (true);
		}

		if (checkIfAllPlayersAreLoaded)
		{
			if (state == states.loadingPlayers)
			{
				return (true);
			}
		}

		return (false);
	}


	/// <summary>
	/// Test if match is in a state that allows a player to shoot or pass the ball.
	/// </summary>
	/// <returns><c>true</c> if this instance can shoot or pass; otherwise, <c>false</c>.</returns>
	public bool CanShootOrPass()
	{
		if ((state == states.play) || 
		    (state == states.kickOff) || 
		    (state == states.throwIn) || 
		    (state == states.cornerKick) || 
		    (state == states.goalKick))
		{
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Called whenever the ball is passed.
	/// </summary>
	public void OnBallPass()
	{
		if ((State == states.kickOff) || 
		    (state == states.throwIn) || 
		    (state == states.cornerKick) ||
		    (state == states.goalKick))
		{
			// Delay sliding tackle for both teams
			leftTeam.SetDelaySlidingTime(Time.time + 2.0f, false);
			rightTeam.SetDelaySlidingTime(Time.time + 2.0f, false);

			SetState(states.play);
		}
	}


	/// <summary>
	/// Clear both teams' pass to players.
	/// </summary>
	/// <returns>The pass players.</returns>
	public void ClearPassPlayers(bool clearPrevPassPlayer = true)
	{
		if (leftTeam != null)
		{
			leftTeam.SetPassPlayer(null, clearPrevPassPlayer);
		}
		if (rightTeam != null)
		{
			rightTeam.SetPassPlayer(null, clearPrevPassPlayer);
		}
	}


	/// <summary>
	/// Sets the ball team.
	/// </summary>
	/// <returns>The ball team.</returns>
	/// <param name="team">Team.</param>
	public void SetBallTeam(SsTeam team)
	{
		ballTeam = team;

		// Clear the last pass players
		if (leftTeam != null)
		{
			leftTeam.LastPassPlayer = null;
		}
		if (rightTeam != null)
		{
			rightTeam.LastPassPlayer = null;
		}
	}
	
	
	/// <summary>
	/// Sets the ball player.
	/// </summary>
	/// <returns>The ball player.</returns>
	/// <param name="player">Player.</param>
	/// <param name="stopMovingBall">Stop the ball's movement.</param>
	public void SetBallPlayer(SsPlayer player, bool stopMovingBall = true)
	{
		if (ball == null)
		{
			return;
		}

		SsPlayer currentPlayer = ballPlayer;
		bool ballWasMoving = ball.IsMoving();
		Vector3 ballOldVel = ball.Rb.velocity;
		bool wasShotAtGoal = ball.WasShotAtGoal;

		ball.WasShotAtGoal = false;

		// Current player who has the ball
		if (currentPlayer != null)
		{
			currentPlayer.OnLostBall();

			// Stop ball movement if it was taken away from another player
			stopMovingBall = true;
		}

		if ((ballPlayer != player) && (player != null))
		{
			ballChangePlayerCounter ++;
		}

		prevBallPlayer = ballPlayer;
		ballPlayer = player;

		ClearPassPlayers();

		if (stopMovingBall)
		{
			// Stop the ball, it will also make it start falling down
			ball.StopMoving();
		}
		else
		{
			// Clear target pos
			ball.SetTargetPosition(false, Vector3.zero);
			ball.StopFakeForce();
		}

		
		// New player who has the ball
		if (player != null)
		{
			lastBallPlayer = player;
			StopBallPredators(null);
			StopBallerPredator(null);
			SetBallTeam(player.Team);

			player.OnGetBall(ballWasMoving, wasShotAtGoal, ballOldVel);
		}
	}


	/// <summary>
	/// Set the goalkeeper to stand and hold the ball.
	/// </summary>
	/// <returns>The goalkeeper hold ball.</returns>
	/// <param name="player">Player.</param>
	public void SetGoalkeeperHoldBall(SsPlayer player)
	{
		player.SetState(SsPlayer.states.gk_standHoldBall);

		if ((player.Team.IsUserControlled == false) && 
		    (player.OtherTeam.IsUserControlled))
		{
			// Disable user input for other team, while players run from goalkeeper
			SsMatchInputManager.SetControlPlayer(null, player.OtherTeam.UserControlIndex);
		}

		if (leftTeam != null)
		{
			leftTeam.StopPlayersWhenGoalkeeperGetsBall();
		}
		if (rightTeam != null)
		{
			rightTeam.StopPlayersWhenGoalkeeperGetsBall();
		}
	}


	/// <summary>
	/// Set the team's start positions (e.g. for kickoff, throw-ins, corner kicks. etc.).
	/// </summary>
	/// <returns>The teams start positions.</returns>
	/// <param name="teamWithBall">Team who will have the ball.</param>
	/// <param name="forState">The state for which to set the start positions.</param>
	public void SetTeamsStartPositions(SsTeam teamWithBall, states forState)
	{
		SsPlayer player;

		SetBallPlayer(null);
		SetBallTeam(null);
		ball.ResetMe();

		lastBallPlayer = null;

		if (forState == states.kickOff)
		{
			// Has the match just started?
			if ((halfTimeTimer > 0.0f) || (matchHalf > 0))
			{
				// Set/change formation. Potentially spawn new players if the formation requires different players.
				leftTeam.SetFormationAtKickOff();
				rightTeam.SetFormationAtKickOff();
			}

			// Place ball at centre
			ball.PositionAtCentre();
		}


		if (teamWithBall != null)
		{
			if (forState == states.goalKick)
			{
				// Give the ball to the goalkeeper
				SetBallPlayer(teamWithBall.GoalKeeper);
			}
			else
			{
				// Give the ball to a forward player
				player = teamWithBall.GetFirstPlayer(SsPlayer.positions.forward);
				if (player == null)
				{
					// Give the ball to a midfielder
					player = teamWithBall.GetFirstPlayer(SsPlayer.positions.midfielder);
					if (player == null)
					{
						// Give the ball to a defender
						player = teamWithBall.GetFirstPlayer(SsPlayer.positions.defender);
					}
				}
				SetBallPlayer(player);

#if UNITY_EDITOR
				if (player == null)
				{
					Debug.LogError("ERROR: Failed to give the ball to a player.");
				}
#endif //UNITY_EDITOR
			}
		}
#if UNITY_EDITOR
		else
		{
			Debug.LogError("ERROR: teamWithBall is null.");
		}
#endif //UNITY_EDITOR


		// Position the teams' players
		leftTeam.PositionPlayers(teamWithBall, forState);
		rightTeam.PositionPlayers(teamWithBall, forState);


		// Let the ball know its position has changed
		ball.OnPositionSet();
	}


	/// <summary>
	/// Sets the state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="newState">New state.</param>
	public void SetState(states newState)
	{
		// Process the old state
		PreSetState(newState);

		state = newState;
		stateTime = Time.time;
		stateUpdateCount = 0;

		// Process the new state
		switch (state)
		{
		case states.loading:
		{
			// Loading
			//--------
			loadingStep = loadingSteps.loadTeam1;
			loadingFrame = 0;
			break;
		}
		case states.loadingDone:
		{
			// Loading done
			//-------------

			// DEBUG
			if ((SsSettings.Instance != null) && (SsSettings.Instance.disableShadows))
			{
				Light[] lights = FindObjectsOfType<Light>();
				int i;
				for (i = 0; i < lights.Length; i++)
				{
					if (lights[i] != null)
					{
						lights[i].shadows = LightShadows.None;
					}
				}

				#if UNITY_EDITOR
				Debug.LogWarning("WARNING: Disabling shadows. Change the setting on the Global Settings prefab.");
				#endif //UNITY_EDITOR
			}

			break;
		}
		case states.kickOff:
		{
			// Kick off
			//---------

			SetTeamsStartPositions(kickoffTeam, states.kickOff);

			field.PlaySfx(field.matchPrefabs.sfxWhistleKickoff);

			break;
		}
		case states.cornerKick:
		{
			// Corner kick
			//------------

			// Delay the other team's AI
			throwInTeam.OtherTeam.DelayAiUpdateTime = Time.time + 1.0f;

			SetTeamsStartPositions(throwInTeam, state);

			break;
		}
		case states.throwIn:
		{
			// Throw in
			//---------

			// Delay the other team's AI
			throwInTeam.OtherTeam.DelayAiUpdateTime = Time.time + 1.0f;
			
			SetTeamsStartPositions(throwInTeam, state);

			break;
		}
		case states.goalKick:
		{
			// Goal kick
			//----------
			
			// Delay the other team's AI
			throwInTeam.OtherTeam.DelayAiUpdateTime = Time.time + 1.0f;
			
			SetTeamsStartPositions(throwInTeam, state);
			
			break;
		}
		case states.halfTime:
		{
			// Half time
			//----------

			halfTimeUiDidClose = false;

			break;
		}
		case states.fullTime:
		{
			// Full time
			//----------

			isDone = true;


			if ((SsSettings.Instance.demoBuild) && 
			    (needsWinner) && (leftTeam.Score == rightTeam.Score))
			{
				// DEMO
				// Match is a draw. Select a random winner.
				if (Random.Range(0, 100) < 50)
				{
					leftTeam.Score ++;
				}
				else
				{
					rightTeam.Score ++;
				}

#if UNITY_EDITOR
				if (SsSettings.LogInfoToConsole)
				{
					Debug.Log("Demo match ended in a draw. Selecting a random winner.");
				}
#endif //UNITY_EDITOR
			}


			if (matchType == SsMatch.matchTypes.tournament)
			{
				// End the tournament match
				SsTournament.EndMatch(LeftTeam.id, LeftTeam.Score, 
				                      RightTeam.id, RightTeam.Score);
			}

			break;
		}
		case states.goalCelebration:
		{
			if ((goalScorer != null) && (goalScorer.Team == goalTeam))
			{
				goalScorer.SetState(SsPlayer.states.goalCelebration);
			}
			else if ((goalScorer != null) && (goalScorer.Team != goalTeam))
			{
				goalScorer.SetState(SsPlayer.states.ownGoal);
			}
			break;
		}
		} //switch


		if (onStateChanged != null)
		{
			onStateChanged(newState);
		}
	}


	/// <summary>
	/// IMPORTANT: This is only called from within SetState.
	/// It processes/cleans up the old state before setting the new state.
	/// </summary>
	/// <returns>The set state.</returns>
	/// <param name="newState">New state.</param>
	public void PreSetState(states newState)
	{
		// Process the old state
		if (state == states.halfTime)
		{
			SwitchTeamSides();
		}
	}


	/// <summary>
	/// Switch the team sides during half time.
	/// </summary>
	/// <returns>The team sides.</returns>
	public void SwitchTeamSides()
	{
		SsTeam tempTeam;

		kickoffTeam = halfTimeKickoffTeam;
		matchHalf = 1;
		halfTimeTimer = 0.0f;
		leftTeam.PlayDirection = -leftTeam.PlayDirection;
		rightTeam.PlayDirection = -rightTeam.PlayDirection;

		tempTeam = leftTeam;
		leftTeam = rightTeam;
		rightTeam = tempTeam;
	}


	/// <summary>
	/// Test if a goalkeeper is holding the ball.
	/// </summary>
	/// <returns><c>true</c> if this instance is goalkeeper holding the ball; otherwise, <c>false</c>.</returns>
	public bool IsGoalkeeperHoldingTheBall()
	{
		if ((ballPlayer != null) && 
		    (ballPlayer.Position == SsPlayer.positions.goalkeeper))
		{
			if ((ballPlayer.IsDiving) || 
			    (ballPlayer.State == SsPlayer.states.gk_standHoldBall) || 
			    (ballPlayer.GkIsThrowingIn))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Test if the player's team's goalkeeper is holding the ball.
	/// </summary>
	/// <returns><c>true</c> if this instance is team goalkeeper holding the ball the specified player; otherwise, <c>false</c>.</returns>
	/// <param name="player">Player.</param>
	public bool IsTeamGoalkeeperHoldingTheBall(SsPlayer player)
	{
		if ((ballTeam == player.Team) && 
		    (IsGoalkeeperHoldingTheBall()))
		{
			return (true);
		}
		return (false);
	}
	
	
	/// <summary>
	/// Test if the other team's goalkeeper is holding the ball.
	/// </summary>
	/// <returns><c>true</c> if this instance is other team goalkeeper holding the ball the specified player; otherwise, <c>false</c>.</returns>
	/// <param name="player">Player.</param>
	public bool IsOtherTeamGoalkeeperHoldingTheBall(SsPlayer player)
	{
		if ((ballTeam == player.OtherTeam) && 
		    (IsGoalkeeperHoldingTheBall()))
		{
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Stop the players chasing after the loose ball.
	/// </summary>
	/// <returns>The ball predators.</returns>
	/// <param name="team">The team to stop. Set to null to stop both teams.</param>
	public void StopBallPredators(SsTeam team)
	{
		if (team != null)
		{
			team.StopAllBallPredators();
		}
		else
		{
			if (leftTeam != null)
			{
				leftTeam.StopAllBallPredators();
			}
			if (rightTeam != null)
			{
				rightTeam.StopAllBallPredators();
			}
		}
	}


	/// <summary>
	/// Stop the players chasing after the player who has the ball.
	/// </summary>
	/// <returns>The baller predator.</returns>
	/// <param name="team">The team to stop. Set to null to stop both teams.</param>
	public void StopBallerPredator(SsTeam team)
	{
		if (team != null)
		{
			team.StopAllBallerPredators();
		}
		else
		{
			if (leftTeam != null)
			{
				leftTeam.StopAllBallerPredators();
			}
			if (rightTeam != null)
			{
				rightTeam.StopAllBallerPredators();
			}
		}
	}


	/// <summary>
	/// A team scored a goal.
	/// </summary>
	/// <param name="team">Team who scored the goal.</param>
	public void OnScoreGoal(SsTeam team)
	{
		goalTeam = team;
		goalScorer = lastBallPlayer;
		if (team == leftTeam)
		{
			kickoffTeam = rightTeam;
		}
		else
		{
			kickoffTeam = leftTeam;
		}
		team.Score ++;

		SetState(states.goal);
		
		field.PlaySfx(field.matchPrefabs.sfxScoreGoal);
	}


	/// <summary>
	/// Raises the half time user interface closed event.
	/// </summary>
	public void OnHalfTimeUiClosed()
	{
		halfTimeUiDidClose = true;
	}
	

	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		float dt = Time.deltaTime;

		lastDt = dt;

		UpdateState(dt);
	}


	/// <summary>
	/// Updates the state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="dt">Dt.</param>
	void UpdateState(float dt)
	{

		stateUpdateCount ++;

		switch (state)
		{
		case states.loading:
		{
			// Loading
			//--------
			UpdateLoading(dt);
			break;
		}
		case states.loadingPlayers:
		{
			// Loading players
			//----------------
			if ((leftTeam != null) && (leftTeam.Players != null) && (leftTeam.Players.Count > 0) && 
			    (rightTeam != null) && (rightTeam.Players != null) && (rightTeam.Players.Count > 0))
			{
				SetState(states.loadingDone);
			}
			break;
		}
		case states.loadingDone:
		{
			// Loading done
			//-------------
			if (stateUpdateCount > 1)
			{
				SetState(states.intro);
			}
			break;
		}
		case states.intro:
		{
			// Intro
			//------
			// NOTE: Here can add support for camera/player animation during intro
			if (stateTime + 1.0f <= Time.time)
			{
				SetState(states.kickOff);
			}
			break;
		}
		case states.kickOff:
		case states.play:
		case states.throwIn:
		case states.cornerKick:
		case states.goalKick:
		{
			break;
		}
		case states.preThrowIn:
		{
			// Pre-throw in
			//-------------
			if (stateTime + 1.0f <= Time.time)
			{
				SetState(states.throwIn);
			}
			break;
		}
		case states.preCornerKick:
		{
			// Pre-corner kick
			//----------------
			if (stateTime + 1.0f <= Time.time)
			{
				SetState(states.cornerKick);
			}
			break;
		}
		case states.preGoalKick:
		{
			// Pre-goal kick
			//--------------
			if (stateTime + 1.0f <= Time.time)
			{
				SetState(states.goalKick);
			}
			break;
		}
		case states.goal:
		{
			// Goal
			//-----
			if ((ball.WasShotAtGoal) && (stateTime + 1.0f <= Time.time))
			{
				ball.WasShotAtGoal = false;
			}

			if (stateTime + 3.0f <= Time.time)
			{
				SetState(states.goalCelebration);
			}
			break;
		}
		case states.goalCelebration:
		{
			// Goal celebration
			//-----------------
			if ((stateTime + 3.0f <= Time.time) && 
			    ((goalScorer == null) || (goalScorer.Team != goalTeam) || (goalScorer.State == SsPlayer.states.idle) || 
			 	 (goalScorer.Animations.IsAnimationPlaying() == false)))
			{
				SetState(states.kickOff);
			}
			break;
		}
		case states.halfTime:
		{
			// Half time
			//----------
			if ((halfTimeUiDidClose) && (stateTime + 4.0f <= Time.time))
			{
				SetState(states.kickOff);
			}
			break;
		}
		case states.fullTime:
		{
			// Full time
			//----------
			if (stateUpdateCount > 1)
			{
				SetState(states.preMatchResults);
			}
			break;
		}
		case states.preMatchResults:
		{
			// Pre-match results
			//------------------
			if (stateTime + 3.0f <= Time.time)
			{
				SetState(states.matchResults);
			}
			break;
		}
		case states.matchResults:
		{
			// Match results
			//--------------
			// Wait for match result UI
			break;
		}
		} //switch


		if (InPlayState)
		{
			UpdatePlay(dt);
		}
	}


	/// <summary>
	/// Update state: Loading.
	/// </summary>
	/// <returns>The loading.</returns>
	/// <param name="dt">Dt.</param>
	void UpdateLoading(float dt)
	{
		// Allow 2 frame updates to pass before going to the next loading step
		loadingFrame ++;
		if (loadingFrame < 2)
		{
			return;
		}
		loadingFrame = 0;


		SsNewMatch nm = loadingMatch;
		SsTeam team;
		int i, userIndex, playDirection, kickOffTeamIndex;
		SsInput.inputTypes input;
		GameObject go;

		if ((loadingStep == loadingSteps.loadTeam1) || 
		    (loadingStep == loadingSteps.loadTeam2))
		{
			// Load team 1/2
			//--------------
			if (loadingStep == loadingSteps.loadTeam1)
			{
				i = 0;
			}
			else
			{
				i = 1;
			}

			if (nm.teamPrefabs[i] == null)
			{
				go = GeUtils.LoadGameObject(nm.teamResources[i]);
				if (go != null)
				{
					nm.teamPrefabs[i] = go.GetComponent<SsTeam>();
				}
				#if UNITY_EDITOR
				else
				{
					Debug.LogError("ERROR: Failed to load the team: " + nm.teamResources[i]);
				}
				#endif //UNITY_EDITOR
			}

			teams[i] = (SsTeam)Instantiate(nm.teamPrefabs[i]);


			// Go to next loading step
			if (loadingStep == loadingSteps.loadTeam1)
			{
				loadingStep = loadingSteps.loadTeam2;
			}
			else
			{
				loadingStep = loadingSteps.loadBall;
			}
		}
		else if (loadingStep == loadingSteps.loadBall)
		{
			// Load ball
			//----------
			if (nm.ballPrefab == null)
			{
				go = GeUtils.LoadGameObject(nm.ballResource);
				if (go != null)
				{
					nm.ballPrefab = go.GetComponent<SsBall>();
				}
				#if UNITY_EDITOR
				else
				{
					Debug.LogError("ERROR: Failed to load the ball: " + nm.ballResource);
				}
				#endif //UNITY_EDITOR
			}

			if ((nm.ballPrefab != null) && (SsBall.Instance == null))
			{
				Instantiate(nm.ballPrefab);
			}
			ball = SsBall.Instance;


			// Go to next loading step
			loadingStep = loadingSteps.initTeams;
		}
		else if (loadingStep == loadingSteps.initTeams)
		{
			// Init teams
			//-----------
			userIndex = 0;
			playDirection = (Random.Range(0, 2) == 0) ? -1 : 1;
			kickOffTeamIndex = Random.Range(0, 2);
			
			
			#if UNITY_EDITOR
			// DEBUG
			if ((SsSettings.Instance != null) && (SsSettings.Instance.humanKickoff))
			{
				// Human team kickoff
				for (i = 0; i < 2; i++)
				{
					input = nm.teamInput[i];
					if ((input != SsInput.inputTypes.invalid) && (input != SsInput.inputTypes.ai))
					{
						kickOffTeamIndex = i;
						
						Debug.LogWarning("WARNING: Human team is forced to kick off (in the editor). You can change it on the Global Settings.");
						break;
					}
				}
			}
			#endif //UNITY_EDITOR


			for (i = 0; i < 2; i++)
			{
				input = nm.teamInput[i];
				team = teams[i];
				
				if (team == null)
				{
					continue;
				}
				
				team.Index = i;
				
				if ((input != SsInput.inputTypes.invalid) && (input != SsInput.inputTypes.ai))
				{
					team.UserControlIndex = userIndex ++;
				}
				else
				{
					team.UserControlIndex = -1;
				}
				team.InputType = input;
				
				
				team.SetPlayDirection(playDirection, true);
				playDirection = -playDirection;

				
				if (i == kickOffTeamIndex)
				{
					originalKickoffTeam = team;
					kickoffTeam = originalKickoffTeam;
				}
				
				if (team.PlayDirection == 1)
				{
					leftTeam = team;
				}
				else
				{
					rightTeam = team;
				}
			}
			
			
			if (kickoffTeam == leftTeam)
			{
				halfTimeKickoffTeam = rightTeam;
			}
			else
			{
				halfTimeKickoffTeam = leftTeam;
			}
			
			
			if (leftTeam != null)
			{
				leftTeam.OtherTeam = rightTeam;
			}
			if (rightTeam != null)
			{
				rightTeam.OtherTeam = leftTeam;
			}


			// First stage of loading is done, now wait for teams to load their players
			nm = null;
			loadingMatch = null;
			SetState(states.loadingPlayers);
		}

	}


	/// <summary>
	/// Update state: Ball is in play.
	/// </summary>
	/// <returns>The play.</returns>
	/// <param name="dt">Dt.</param>
	void UpdatePlay(float dt)
	{
		if ((state == states.play) && 
		    (IsGoalkeeperHoldingTheBall() == false))
		{
			UpdateMatchTimers(dt);
		}
	}


	/// <summary>
	/// Updates the match timers.
	/// </summary>
	/// <returns>The match timers.</returns>
	/// <param name="dt">Dt.</param>
	void UpdateMatchTimers(float dt)
	{
		bool suddenDeath = false;

		// If match needs a winner and scores are tied then go to sudden death
		if ((needsWinner) && (leftTeam.Score == rightTeam.Score))
		{
			if (SsSettings.Instance.demoBuild)
			{
				// DEMO: Demo does Not have sudden death. A random winner will be selected.
			}
			else
			{
				suddenDeath = true;
			}
		}

		
		if ((matchHalf == 0) || (matchHalf == 1))
		{
			// Update timers for 1st and 2nd half
			halfTimeTimer += dt;
			matchTimer += dt;

			if (suddenDeath == false)
			{
				if (matchTimer > maxHalfTime * (matchHalf + 1.0f))
				{
					matchTimer = maxHalfTime * (matchHalf + 1.0f);
				}
			}

			if (halfTimeTimer > maxHalfTime)
			{
				if (suddenDeath == false)
				{
					halfTimeTimer = maxHalfTime;
				}

				// Do Not end half while ball is flying towards goal posts OR the player is busy kicking
				if ((ball.WasShotAtGoal == false) && 
				    ((ballPlayer == null) || (ballPlayer.IsKicking == false)))
				{
					if (matchHalf == 0)
					{
						// End the half
						SetState(states.halfTime);
						
						field.PlaySfx(field.matchPrefabs.sfxWhistleHalfTime);
					}
					else
					{
						if (suddenDeath == false)
						{
							// End the match
							SetState(states.fullTime);
							
							field.PlaySfx(field.matchPrefabs.sfxWhistleFullTime);
						}
					}
				}
			}
		}

	}

}
