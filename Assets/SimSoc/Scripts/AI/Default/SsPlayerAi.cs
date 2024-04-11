using UnityEngine;
using System.Collections;

/// <summary>
/// Player AI base class. Handles AI decisions and updates the input.
/// NOTE: Essentially the AI moves by setting the input data (e.g. axes).
/// </summary>
public class SsPlayerAi : MonoBehaviour {

	// Const/Static
	//-------------
	public const float kickOffDelay = 2.0f;					// Delay before kick off at first half
	public const float kickOffDelay2 = 4.0f;				// Delay before kick off at second half (or extra time)
	public const float throwInDelay = 1.0f;					// Delay before throwing in the ball
	public const float cornerKickDelay = 1.0f;				// Delay before kicking from a corner kick
	public const float goalKickDelay = 1.0f;				// Delay before kicking from a goal kick

	public const float delayThrowBallTimeMin = 0.6f;		// Min random time for goalkeeper to hod ball before throwing
	public const float delayThrowBallTimeMax = 0.8f;		// Max random time for goalkeeper to hod ball before throwing
	public const float delayWaitForOtherTeamToRunBackTimeMin = 3.0f; // Min random time for goalkeeper to wait for players to run back
	public const float delayWaitForOtherTeamToRunBackTimeMax = 3.2f; // Max random time for goalkeeper to wait for players to run back

	public const float forwardFindOpenSpotTimerMin = 2.5f;	// Min random time to delay searching for an open spot ahead
	public const float forwardFindOpenSpotTimerMax = 4.0f;	// Max random time to delay searching for an open spot ahead
	public const float backwardFindOpenSpotTimerMin = 2.5f;	// Min random time to delay searching for an open spot behind
	public const float backwardFindOpenSpotTimerMax = 4.0f;	// Max random time to delay searching for an open spot behind

	public const float checkBallPredatorIntervals = 2.0f;	// Intervals between checking if players should chase the ball
	public const float checkRunForwardIntervals = 3.0f;		// Intervals between checking if players should run forward
	public const float checkRunBackwardIntervals = 3.0f;	// Intervals between checking if players should run backward
	public const float markPlayerIntervals = 0.2f;			// Intervals between marking players
	public const float tooCloseToBallerIntervals = 3.0f;	// Intervals to check if too close to baller, of own team.

	// Delay for checking RunToOpenSpotBetweenZonesInRowIfGoalkeeperHoldsBall
	public const float delayRunToOpenSpotWhenGoalkeeperHoldsBall = 0.1f;

	// Delay for checking RunToOpenSpotBetweenZonesInRow
	public const float delayRunToOpenSpotBetweenZones = 1.0f;

	// Forward players runs backwards to before this zone when opponent goalkeeper has the ball.
	public const int runBackBeforeZoneWhenGoalkeerHasBall = 4;

	// Select a position in the field, but Not closer than this distance from the field edge. Used for some of the searches.
	public const float fieldSearchBorder = 1.0f;



	// Private
	//--------
	protected SsMatch match;							// Reference to the match
	protected SsFieldProperties field;					// Reference to the field
	protected SsBall ball;								// Reference to the ball

	protected SsPlayer player;							// Player to which this is attached
	protected SsTeam team;								// Team to which this player belongs
	protected SsTeam otherTeam;							// Opponent team

	protected SsMatchInputManager.SsUserInput input;	// Input data

	protected int defaultVerticalPos;					// Default row.
	protected int preferredVerticalPos;					// Preferred row. Affected by team formation.

	protected float forceDelayUpdateTime;				// Force delay AI update timer.
	protected float delaySlidingTime;					// Timer to delay sliding.
	protected float runForwardTimer;					// Timer to delay running forward.
	protected float runBackwardTimer;					// Timer to delay running backward
	protected float delayShootingTime;					// Timer to delay shooting attempts
	protected float passIfHadBallTooLongTime;			// Pass if had ball for longer than this time
	protected float delaySideStepTime;					// Timer to delay sidestepping.
	protected float delayThrowBallTime;					// Time to delay throwing a ball (goalkeeper)
	protected float delayWaitForOtherTeamToRunBackTime;	// Time to wait for the other team to run backwards (goalkeeper)
	protected float delayMarkTimer;						// Delay time before a sprite can mark another sprite
	protected float runToOpenSpotBetweenZonesInRowTimer;	// Time to delay running to an open spot between zones, in a row.
	protected float tooCloseToBallerTimer;				// Time to delay running when close to baller
	protected float forwardFindOpenSpotTimer;			// Delay trying to find an open spot ahead of the ball
	protected float backwardFindOpenSpotTimer;			// Delay trying to find an open spot behind of the ball
	protected float delayDiveTime;						// Time to delay diving
	protected float goalkeeperFollowBallInGoalkeeperArea;	// Time to delay following ball in goalkeeper area (goalkeeper)

	protected bool runningStraightForwards;				// Is running straight forward?
	protected bool runningStraightBackwards;			// Is running straight backward?
	protected bool runningToOpenSpotFromPredator;		// Is running to open spot from predator?
	protected bool runningToGoalkeeperArea;				// Is running to goalkeeper area?
	protected bool divingAtBaller;						// Is goalkeeper diving at the baller?
	protected bool passToForemost;						// Indicates if the baller must pass to the foremost player

	protected int trackBallChangePlayerCounter;			// Track how many times the ball changes players


	// Properties
	//-----------
	/// <summary>
	/// Preferred row on the field. Usually set based on the player's position in the team formation.
	/// </summary>
	/// <value>The preferred vertical position.</value>
	public int PreferredVerticalPos
	{
		get { return(preferredVerticalPos); }
		set
		{
			preferredVerticalPos = value;
		}
	}


	public int DefaultVerticalPos
	{
		get { return(defaultVerticalPos); }
		set
		{
			defaultVerticalPos = value;
		}
	}


	public float ForceDelayUpdateTime
	{
		get { return(forceDelayUpdateTime); }
		set
		{
			forceDelayUpdateTime = value;
		}
	}


	public float DelaySlidingTime
	{
		get { return(delaySlidingTime); }
		set
		{
			delaySlidingTime = value;
		}
	}


	public float RunForwardTimer
	{
		get { return(runForwardTimer); }
		set
		{
			runForwardTimer = value;
		}
	}


	public float RunBackwardTimer
	{
		get { return(runBackwardTimer); }
		set
		{
			runBackwardTimer = value;
		}
	}


	public float DelayShootingTime
	{
		get { return(delayShootingTime); }
		set
		{
			delayShootingTime = value;
		}
	}


	public float DelayThrowBallTime
	{
		get { return(delayThrowBallTime); }
		set
		{
			delayThrowBallTime = value;
		}
	}


	public float DelayWaitForOtherTeamToRunBackTime
	{
		get { return(delayWaitForOtherTeamToRunBackTime); }
		set
		{
			delayWaitForOtherTeamToRunBackTime = value;
		}
	}


	public float DelayDiveTime
	{
		get { return(delayDiveTime); }
		set
		{
			delayDiveTime = value;
		}
	}


	public bool RunningStraightForwards
	{
		get { return(runningStraightForwards); }
	}


	public bool RunningStraightBackwards
	{
		get { return(runningStraightBackwards); }
	}


	public bool RunningToGoalkeeperArea
	{
		get { return(runningToGoalkeeperArea); }
	}


	public bool DivingAtBaller
	{
		get { return(divingAtBaller); }
		set
		{
			divingAtBaller = value;
		}
	}


	public bool PassToForemost
	{
		get { return(passToForemost); }
		set
		{
			passToForemost = value;
		}
	}



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived classes must call the base method.
	/// </summary>
	public virtual void Awake()
	{
		player = gameObject.GetComponentInParent<SsPlayer>();
		if (player != null)
		{
			input = player.Input;
		}
	}


	/// <summary>
	/// Use this for initialization.
	/// NOTE: Derived classes must call the base method.
	/// </summary>
	public virtual void Start()
	{
		match = SsMatch.Instance;
		field = SsFieldProperties.Instance;
		ball = SsBall.Instance;

		if (player != null)
		{
			team = player.Team;
			otherTeam = player.OtherTeam;
		}

		ResetMe();
	}


	/// <summary>
	/// Reset this instance.
	/// NOTE: Derived classes must call the base method.
	/// </summary>
	public virtual void ResetMe()
	{
		divingAtBaller = false;

		delayShootingTime = 0.0f;
		passIfHadBallTooLongTime = Random.Range(player.Skills.ai.passIfHadBallTimeMin, player.Skills.ai.passIfHadBallTimeMax);
		delaySideStepTime = 0.0f;
		player.SideStepTime = 0.0f;
		delayThrowBallTime = 0.0f;
		delayWaitForOtherTeamToRunBackTime = 0.0f;
		delayDiveTime = 0.0f;

		ClearTimers();
	}


	/// <summary>
	/// Clear AI related timers. Mainly used when the control player changes so that the previous control player (now an AI) reacts immediately.
	/// </summary>
	/// <returns>The timers.</returns>
	public virtual void ClearTimers()
	{
		trackBallChangePlayerCounter = 0;
		tooCloseToBallerTimer = 0.0f;
		forwardFindOpenSpotTimer = 0.0f;
		backwardFindOpenSpotTimer = 0.0f;
		runForwardTimer = 0.0f;
		runBackwardTimer = 0.0f;
		runToOpenSpotBetweenZonesInRowTimer = 0.0f;
		delayMarkTimer = 0.0f;
		runningToGoalkeeperArea = false;
		goalkeeperFollowBallInGoalkeeperArea = 0.0f;
		runningToOpenSpotFromPredator = false;
		runningStraightBackwards = false;
		runningStraightForwards = false;
		forceDelayUpdateTime = 0.0f;
		passToForemost = false;

		delaySlidingTime = 0.0f;
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived classes must call the base method.
	/// </summary>
	public virtual void OnDestroy()
	{
		CleanUp();
	}
	
	
	/// <summary>
	/// Clean up. Includes freeing resources and clearing references.
	/// NOTE: Derived classes must call the base method.
	/// </summary>
	/// <returns>The up.</returns>
	public virtual void CleanUp()
	{
		match = null;
		field = null;
		ball = null;

		player = null;
		team = null;
	}


	/// <summary>
	/// Test if the AI is in a state where it can be updated (i.e. can it make decisions and perform actions).
	/// </summary>
	/// <returns><c>true</c> if this instance can update ai; otherwise, <c>false</c>.</returns>
	public virtual bool CanUpdateAi()
	{
		if (match == null)
		{
			return (false);
		}


#if UNITY_EDITOR
		// DEBUG
		if ((SsSettings.Instance != null) && (SsSettings.Instance.disableAi))
		{
			return (false);
		}
#endif //UNITY_EDITOR


		// Check match's state
		if ((match.InPlayState) || 
		    (match.State == SsMatch.states.goal) || 
		    (match.State == SsMatch.states.halfTime) || 
		    (match.State == SsMatch.states.fullTime))
		{
			// Check player's state
			if ((player.State != SsPlayer.states.slideTackle) && 
			    //(player.State != SsPlayer.states.tackle) &&
			    (player.State != SsPlayer.states.falling) && 
			    (player.State != SsPlayer.states.inPain) && 
			    (player.IsDiving == false) && 
			    (player.State != SsPlayer.states.bicycleKick) &&
			    (player.GkIsThrowingIn == false) && 
			    (player.DelayPass.pending == false) && 
			    (player.DelayShoot.pending == false))
			{
				// Check timers
				if ((forceDelayUpdateTime <= Time.time) &&
				    (player.Team.DelayAiUpdateTime <= Time.time))
				{
					return (true);
				}
			}
		}

		return (false);
	}


	/// <summary>
	/// Raises the stop moving event.
	/// </summary>
	public virtual void OnStopMoving(bool wasMoving)
	{
		// Clear movement timers (only timers which update while the player is moving)
		tooCloseToBallerTimer = 0.0f;
		runForwardTimer = 0.0f;
		runBackwardTimer = 0.0f;
		
		runningToGoalkeeperArea = false;
		runningToOpenSpotFromPredator = false;
		runningStraightBackwards = false;
		runningStraightForwards = false;


		if ((player.WasMoving()) || (wasMoving))
		{
			// Timers that should Not update while the player is moving

			forwardFindOpenSpotTimer = Time.time + Random.Range(forwardFindOpenSpotTimerMin, forwardFindOpenSpotTimerMax);
			backwardFindOpenSpotTimer = Time.time + Random.Range(backwardFindOpenSpotTimerMin, backwardFindOpenSpotTimerMax);
			runToOpenSpotBetweenZonesInRowTimer = Time.time + delayRunToOpenSpotBetweenZones;
			goalkeeperFollowBallInGoalkeeperArea = Time.time + player.Skills.ai.gkCasualFollowBallDelay;
		}
	}


	//---------------
	// Update Methods
	//---------------
	
	/// <summary>
	/// Update is called once per frame. It is recommended that you override UpdateAi() instead of Update().
	/// </summary>
	public virtual void Update()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		float dt = Time.deltaTime;

		if (player.IsAiControlled)
		{
			input.ClearInput();

			PreUpdate(dt);

			if (CanUpdateAi())
			{
				UpdateAi(dt);
			}
		}
	}


	/// <summary>
	/// Pre-update the AI. It updates some timers.
	/// </summary>
	/// <returns>The update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual void PreUpdate(float dt)
	{
		// Did a different player get the ball?
		if (trackBallChangePlayerCounter != match.BallChangePlayerCounter)
		{
			forwardFindOpenSpotTimer = 0.0f;
			backwardFindOpenSpotTimer = 0.0f;
			runToOpenSpotBetweenZonesInRowTimer = 0.0f;
		}
	}


	/// <summary>
	/// Post-update the AI.
	/// </summary>
	/// <returns>The update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual void PostUpdate(float dt)
	{
	}


	/// <summary>
	/// Updates the AI decisions and input. This is the main AI update method.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateAi(float dt)
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return (false);
		}


		// Kick off
		if (match.State == SsMatch.states.kickOff)
		{
			return (UpdateKickOff(dt));
		}


		// Throw in
		if (match.State == SsMatch.states.throwIn)
		{
			return (UpdateThrowIn(dt));
		}


		// Corner kick
		if (match.State == SsMatch.states.cornerKick)
		{
			return (UpdateCornerKick(dt));
		}


		// Goal kick
		if (match.State == SsMatch.states.goalKick)
		{
			return (UpdateGoalKick(dt));
		}


		// Goal
		if (match.State == SsMatch.states.goal)
		{
			return (UpdateGoal(dt));
		}


		// Half time
		if (match.State == SsMatch.states.halfTime)
		{
			return (UpdateHalfTime(dt));
		}


		// Full time
		if (match.State == SsMatch.states.fullTime)
		{
			return (UpdateFullTime(dt));
		}


		// Team goalkeeper has the ball
		if (match.IsTeamGoalkeeperHoldingTheBall(player))
		{
			return (UpdateTeamGoalkeeperHasBall(dt));
		}


		// Other team goalkeeper has the ball
		if (match.IsOtherTeamGoalkeeperHoldingTheBall(player))
		{
			return (UpdateOtherTeamGoalkeeperHasBall(dt));
		}


		// Team has the ball (Not goalkeeper)
		if (match.BallTeam == team)
		{
			return (UpdateTeamHasBallNotGoalkeeper(dt));
		}


		// Other team has the ball (Not goalkeeper)
		if (match.BallTeam == player.OtherTeam)
		{
			return (UpdateOtherTeamHasBallNotGoalkeeper(dt));
		}


		// No team has the ball
		if (match.BallTeam == null)
		{
			return (UpdateNoTeamHasBall(dt));
		}


		return (false);
	}


	/// <summary>
	/// Update AI: Kick off match state.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateKickOff(float dt)
	{
		float delay = (match.MatchHalf == 0) ? kickOffDelay : kickOffDelay2;
		if ((player == match.BallPlayer) && (player.HaveBallTime + delay < Time.time))
		{
			return (Pass(true, false, false));
		}
		return (false);
	}


	/// <summary>
	/// Update AI: Throw in match state.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateThrowIn(float dt)
	{
		if ((player == match.BallPlayer) && (player.HaveBallTime + throwInDelay < Time.time))
		{
			return (Pass(true, false, false));
		}
		return (false);
	}


	/// <summary>
	/// Update AI: Corner kick match state.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateCornerKick(float dt)
	{
		if ((player == match.BallPlayer) && (player.HaveBallTime + cornerKickDelay < Time.time))
		{
			return (Pass(true, false, false));
		}
		return (false);
	}


	/// <summary>
	/// Update AI: Goal kick match state.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateGoalKick(float dt)
	{
		if ((player == match.BallPlayer) && (player.HaveBallTime + goalKickDelay < Time.time))
		{
			return (Pass(true, false, false));
		}
		return (false);
	}


	/// <summary>
	/// Update AI: Goal match state.
	/// </summary>
	/// <returns>The goal.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateGoal(float dt)
	{
		if (player.IsMoving())
		{
			player.StopMoving(true, true, true);
		}

		// Do nothing else
		return (true);
	}


	/// <summary>
	/// Update AI: half time match state.
	/// </summary>
	/// <returns>The half time.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateHalfTime(float dt)
	{
		if (player.IsMoving())
		{
			player.StopMoving(true, true, true);
		}
		
		// Do nothing else
		return (true);
	}


	/// <summary>
	/// Update AI: full time match state.
	/// </summary>
	/// <returns>The full time.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateFullTime(float dt)
	{
		if (player.IsMoving())
		{
			player.StopMoving(true, true, true);
		}
		
		// Do nothing else
		return (true);
	}


	/// <summary>
	/// Update AI: Team goalkeeper has ball.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateTeamGoalkeeperHasBall(float dt)
	{
		return (false);
	}


	/// <summary>
	/// Update AI: Other team goalkeeper has the ball
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateOtherTeamGoalkeeperHasBall(float dt)
	{
		return (false);
	}


	/// <summary>
	/// Update AI: Team has the ball (Not goalkeeper).
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateTeamHasBallNotGoalkeeper(float dt)
	{
		return (false);
	}


	/// <summary>
	/// Update AI: Other team has the ball (Not goalkeeper).
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateOtherTeamHasBallNotGoalkeeper(float dt)
	{
		return (false);
	}


	/// <summary>
	/// Update AI: No team has the ball.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public virtual bool UpdateNoTeamHasBall(float dt)
	{
		return (false);
	}



	//---------------
	// Action Methods
	//---------------

	/// <summary>
	/// Pass to another player.
	/// </summary>
	/// <param name="setControlPlayer">Indicates if the other team's control player must be set (if they user controlled).</param>
	/// <param name="passForward">Indicates if AI must try to pass to a forward player first.</param>
	/// <param name="predictPass">Indicates if AI is allowed to predict pass.</param>
	protected bool Pass(bool setControlPlayer, bool passForward, bool predictPass)
	{
		SsPlayer target = null;

		if ((match.State == SsMatch.states.play) && 
		    (passToForemost) && 
		    (player != team.ForemostPlayer) && 
		    (team.ForemostPlayer != null) && 
		    (team.ForemostPlayer.IsHurt == false))
		{
			target = team.ForemostPlayer;
		}
		else if ((match.State == SsMatch.states.play) && 
		         (passToForemost) && 
		         (player != team.SecondForemostPlayer) && 
		         (team.SecondForemostPlayer != null) && 
		         (team.SecondForemostPlayer.IsHurt == false))
		{
			target = team.SecondForemostPlayer;
		}
		else
		{
			// Do Not pass to the goalkeeper during kickoff
			target = team.GetPotentialPassPlayer(player, 
			                                     (match.State == SsMatch.states.kickOff ? team.GoalKeeper : null), 
			                                     null, -1.0f, SsPlayer.passToMinLastTimeHadBall, passForward);
		}

		if (target != null)
		{
			passIfHadBallTooLongTime = Random.Range(player.Skills.ai.passIfHadBallTimeMin, player.Skills.ai.passIfHadBallTimeMax);
			player.Pass(target, Vector3.zero);

			if (setControlPlayer)
			{
				otherTeam.MakeNearestPlayerToBallTheControlPlayer(true);
			}

			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Pass the ball is an opponent player is nearby.
	/// </summary>
	/// <returns>The if opponent close.</returns>
	protected bool PassIfOpponentClose()
	{
		if (player.HaveBallTime + player.Skills.ai.holdBallBeforePassTime > Time.time)
		{
			return (false);
		}
		
		SsPlayer nearestOpponent = null;
		float distance = GetOpponentDistance(true, out nearestOpponent);
		if ((distance <= player.Skills.ai.markDistanceTooClose) && 
		    ((player.OtherTeam.IsUserControlled == false) || 
		 	 ((nearestOpponent != null) && (nearestOpponent.IsUserControlled))))
		{
			return (Pass(false, player.Skills.ai.alwaysPassForward, true));
		}
		
		return (false);
	}


	/// <summary>
	/// Pass if player had the ball too long.
	/// </summary>
	/// <returns>The if ran long distance.</returns>
	protected bool PassIfRanLongDistance()
	{
		if (player.HaveBallTime + passIfHadBallTooLongTime < Time.time)
		{
			return (Pass(false, player.Skills.ai.alwaysPassForward, true));
		}
		return (false);
	}


	/// <summary>
	/// Side step when an opponent is sliding towards the player.
	/// </summary>
	/// <returns>The step.</returns>
	protected bool SideStep()
	{
		if (player.SideStepTime > Time.time)
		{
			// Did sidestep end?
			if (player.GotoTargetPos == false)
			{
				player.SideStepTime = 0.0f;
				return (false);
			}
			return (true);
		}
		else if (delaySideStepTime > Time.time)
		{
			return (false);
		}

		if (player.BallerSlideTarget != null)
		{
			float chance = player.DynamicChanceSideStep;
			
			// Reduce chance of sidestep when opponent started sliding nearby
			if (player.BallerSlideTarget.SlideSqrDistanceAtStart < (player.Skills.ai.sideStepReduceDistance * player.Skills.ai.sideStepReduceDistance))
			{
				chance *= 2.0f;
			}
			if (chance > player.Skills.ai.chanceSideStep)
			{
				return (false);
			}
			
			Vector2 vec = new Vector2(transform.position.x - player.BallerSlideTarget.transform.position.x, 
			                          transform.position.z - player.BallerSlideTarget.transform.position.z);

			if (vec.sqrMagnitude > (player.Skills.ai.startSideStepDistance * player.Skills.ai.startSideStepDistance))
			{
				return (false);
			}
			
			vec.Normalize();
			
			Vector2 leftVec = GeUtils.RotateVector(vec, -70.0f);	// Sidestep left
			Vector2 rightVec = GeUtils.RotateVector(vec, 70.0f);	// Sidestep right
			Vector3 pos;
			
			// Determine the best direction to sidestep
			if (transform.position.z >= field.PlayArea.max.z - (player.Skills.ai.sideStepDistance * 1.5f))
			{
				// Near top of field, turn down
				if (vec.x > 0.0f)
				{
					vec = rightVec;
				}
				else
				{
					vec = leftVec;
				}
			}
			else if (transform.position.z <= field.PlayArea.min.z + (player.Skills.ai.sideStepDistance * 1.5f))
			{
				// Near bottom of field, turn up
				if (vec.x > 0.0f)
				{
					vec = leftVec;
				}
				else
				{
					vec = rightVec;
				}
			}
			else if (Random.Range(0, 100) < 80)
			{
				// Sidestep towards opponent goal post
				if (team.PlayDirection > 0.0f)
				{
					if (vec.y > 0.0f)
					{
						vec = rightVec;
					}
					else
					{
						vec = leftVec;
					}
				}
				else
				{
					if (vec.y > 0.0f)
					{
						vec = leftVec;
					}
					else
					{
						vec = rightVec;
					}
				}
			}
			else
			{
				// Random direction
				if (Random.Range(0, 100) < 50)
				{
					vec = leftVec;
				}
				else
				{
					vec = rightVec;
				}
			}

			pos = transform.position + (new Vector3(vec.x, 0.0f, vec.y) * player.Skills.ai.sideStepDistance);

			// Make sure spot is Not too close to field edges
			Bounds fieldBounds = field.PlayArea;
			pos.x = Mathf.Clamp(pos.x, fieldBounds.min.x + fieldSearchBorder, fieldBounds.max.x - fieldSearchBorder);
			pos.z = Mathf.Clamp(pos.z, fieldBounds.min.z + fieldSearchBorder, fieldBounds.max.z - fieldSearchBorder);

			player.SetTargetPosition(true, pos, 1.0f, 2.0f);
			player.SideStepTime = Time.time + player.Skills.ai.sideStepTimeMax;
			delaySideStepTime = Time.time + player.Skills.ai.delaySideStepTimeMax;

#if UNITY_EDITOR
			// DEBUG
			if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSidesteps))
			{
				Debug.DrawLine(player.transform.position, player.TargetPos, Color.cyan, 5.0f);
				SsDebugMatch.DrawCircleUp(player.transform.position, player.Skills.ai.startSideStepDistance, 10, Color.cyan, 5.0f);
				SsDebugMatch.DrawCircleUp(player.transform.position, player.Skills.ai.sideStepReduceDistance, 10, Color.blue, 5.0f);
			}
#endif //UNITY_EDITOR

			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Passes if opponent slide towards the player.
	/// </summary>
	/// <returns>The if opponent slides.</returns>
	protected bool PassIfOpponentSlides()
	{
		if ((player.HaveBallTime + player.Skills.ai.holdBallBeforePassTime <= Time.time) && 
		    (player.BallerSlideTarget != null) && 
		    (player.DistanceToObjectSquared(player.BallerSlideTarget.gameObject) < (player.Skills.ai.passIfOpponentSlideNear * player.Skills.ai.passIfOpponentSlideNear)))
		{
			if (Pass(false, player.Skills.ai.alwaysPassForward, true))
			{

#if UNITY_EDITOR
				// DEBUG
				if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerAvoidSlide))
				{
					SsDebugMatch.DrawCircleUp(player.transform.position, player.Skills.ai.passIfOpponentSlideNear, 10, 
					                          new Color(247.0f / 255.0f, 148.0f / 255.0f, 29.0f / 255.0f), 5.0f);
				}
#endif //UNITY_EDITOR

				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Get the distance to the opponent who is marking this player, or any opponent if the player is Not marked.
	/// </summary>
	/// <returns>The opponent distance.</returns>
	/// <param name="getNearestPlayerIfNotMarked">Indicates if any near opponent must be returned if the player is Not marked.</param>
	/// <param name="getNearestOpponent">Get nearest opponent.</param>
	protected float GetOpponentDistance(bool getNearestPlayerIfNotMarked, out SsPlayer getNearestOpponent)
	{
		getNearestOpponent = null;
		float distance = 10000.0f;
		SsPlayer tempPlayer = player.GetMarkedByPlayer();
		if (tempPlayer != null)
		{
			distance = player.DistanceToObject(tempPlayer.gameObject);
			getNearestOpponent = tempPlayer;
		}
		else if (getNearestPlayerIfNotMarked)
		{
			// Get the nearest free opponent who could possibly mark this player in the near future
			tempPlayer = player.OtherTeam.GetNearestPlayer(null, player, null, null, null, 
			                                                   -1.0f, false, -1, -1, null, false, false, -1.0f, -1.0f, false);
			if (tempPlayer != null)
			{
				distance = player.DistanceToObject(tempPlayer.gameObject);
				getNearestOpponent = tempPlayer;
			}
		}
		return (distance);
	}


	/// <summary>
	/// Test if the player becomes the ball predator (i.e. has to collect a loose ball).
	/// </summary>
	/// <returns>The on ball.</returns>
	protected bool PreyOnBall()
	{
		if ((match.BallPlayer == null) && (team.BallPredatorsCount <= 0) && 
		    (team.BallPredatorDelayTime <= Time.time) && 
		    (team.NearestUnhurtAiPlayerToBall == player))
		{
			match.StopBallPredators(team);
			player.StopMoving();
			player.SetBallPredator(true, false);

			// Delay multiple players constantly trying to get the ball
			team.BallPredatorDelayTime = Time.time + checkBallPredatorIntervals;

			player.SetTargetObject(ball, ball.AiRadiusMin, ball.AiRadiusMax);

			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Test if the predator is taking too long to get to the ball and there is another player closer to the ball.
	/// </summary>
	/// <returns>The predator taking too long.</returns>
	protected bool BallPredatorTakingTooLong()
	{
		if (player.IsBallPredator)
		{
			// Is player Not the nearest unhurt to the ball, or player is hurt?
			if ((player != team.NearestUnhurtAiPlayerToBall) || (player.IsHurt))
			{
				if ((player.BallPredatorTime + player.Skills.ai.looseReactTime < Time.time) || 
				    (ball.Speed < player.Skills.ai.looseReactSpeedMin) || 
				    (player.IsHurt))
				{
					match.StopBallPredators(team);
					team.BallPredatorDelayTime = 0.0f;	// Change instantly to another player
					return (true);
				}
			}
		}

		return (false);
	}


	/// <summary>
	/// Run forward.
	/// </summary>
	/// <returns>The forward.</returns>
	protected bool RunForward()
	{
		return (RunForwardIfBeforeZone(SsFieldProperties.maxZones));
	}


	/// <summary>
	/// Run forward if before the specified zone.
	/// </summary>
	/// <returns>The forward if before zone.</returns>
	/// <param name="zone">Zone.</param>
	protected bool RunForwardIfBeforeZone(int zone)
	{
		if ((runForwardTimer <= Time.time) && 
		    (field.PlayerBeforeZone(player, zone, null, false)))
		{
			if (FindOpenSpotInDirection(team.PlayDirection, transform.position, 
			                            player.Skills.ai.searchArcRadiusMin, player.Skills.ai.searchArcRadiusMax, 
			                            ((match.IsTeamGoalkeeperHoldingTheBall(player)) || (player == match.BallPlayer))))
			{
				runForwardTimer = Time.time + checkRunForwardIntervals;
				return (true);
			}
			else if (player.IsMoving() == false)
			{
				// Just run forward if the player is standing still, to prevent AI standing around too long.
				if (team.PlayDirection > 0.0f)
				{
					SelectPointInCircle(transform.position + (Vector3.right * 10.0f), 2.5f);
				}
				else
				{
					SelectPointInCircle(transform.position - (Vector3.right * 10.0f), 2.5f);
				}

				runForwardTimer = Time.time + checkRunForwardIntervals;
				return (true);
			}
		}

		return (false);
	}


	/// <summary>
	/// Select a random point in a circle to move to.
	/// </summary>
	/// <returns>The point in circle.</returns>
	/// <param name="centre">Centre of circle.</param>
	/// <param name="maxRadius">Max radius.</param>
	protected void SelectPointInCircle(Vector3 centre, float maxRadius)
	{
		Vector3 pos = new Vector3(centre.x + Random.Range(-maxRadius, maxRadius), 
		                          field.GroundY,
		                          centre.z + Random.Range(-maxRadius, maxRadius));

		// Make sure spot is Not too close to field edges
		Bounds fieldBounds = field.PlayArea;
		pos.x = Mathf.Clamp(pos.x, fieldBounds.min.x + fieldSearchBorder, fieldBounds.max.x - fieldSearchBorder);
		pos.z = Mathf.Clamp(pos.z, fieldBounds.min.z + fieldSearchBorder, fieldBounds.max.z - fieldSearchBorder);

		player.SetTargetPosition(true, pos, 1.0f, 2.0f);
	}


	/// <summary>
	/// Run straight backward if past zone.
	/// </summary>
	/// <returns>The straight backward if past zone.</returns>
	/// <param name="zone">Zone.</param>
	protected bool RunStraightBackwardIfPastZone(int zone)
	{
		if (runningStraightBackwards == false)
		{
			if (field.PlayerPastZone(player, zone, null))
			{
				SsFieldProperties.SsGrid grid = null;
				Vector3 pos = transform.position;
				float x;
				if (team.PlayDirection > 0.0f)
				{
					x = field.GetZoneLeftEdge(zone);
					pos.x = x - 1.0f;
					grid = field.GetGridAtPoint(pos, true);
				}
				else
				{
					x = field.GetZoneRightEdge(SsFieldProperties.maxZones - zone - 1);
					pos.x = x + 1.0f;
					grid = field.GetGridAtPoint(pos, true);
				}

				if (grid != null)
				{
					SelectPointInGrid(grid, -team.PlayDirection);
					runningStraightBackwards = true;
					return (true);
				}
			}
		}

		return (runningStraightBackwards);
	}


	/// <summary>
	/// Run backwards if past the specified zone.
	/// </summary>
	/// <returns>The backward if past zone.</returns>
	/// <param name="zone">Zone.</param>
	protected bool RunBackwardIfPastZone(float zone)
	{
		if (runBackwardTimer <= Time.time)
		{
			if (field.PlayerPastZone(player, zone, null))
			{
				if (FindOpenSpotInDirection(-team.PlayDirection, transform.position, 
				                            player.Skills.ai.searchArcRadiusMin, player.Skills.ai.searchArcRadiusMax, 
				                            match.IsOtherTeamGoalkeeperHoldingTheBall(player)))
				{
					runBackwardTimer = Time.time + checkRunBackwardIntervals;
					return (true);
				}
			}
		}
		return (false);
	}


	/// <summary>
	/// Shoot if close to goal.
	/// </summary>
	/// <returns>The if close to goal.</returns>
	protected bool ShootIfCloseToGoal()
	{
		if (field.PlayerPastZone(player, player.Skills.ai.shootPastZone, null, false))
		{
			return (Shoot());
		}
		return (false);
	}


	/// <summary>
	/// Shoot at the goal posts.
	/// </summary>
	protected bool Shoot()
	{
		if (delayShootingTime > Time.time)
		{
			return (false);
		}
		delayShootingTime = Time.time + Random.Range(player.Skills.ai.shootDelayMin, player.Skills.ai.shootDelayMax);

		Vector3 shootPos;
		bool canShoot = player.CanShootToGoal(out shootPos, false, 
		                                      player.Skills.ai.ignoreAngleWhenShooting,
		                                      false, false, false);
		if (canShoot)
		{
			player.Shoot(shootPos, true);
			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Test if the player (who was passed to) is busy running to a position where the ball is going to land.
	/// If he has reached the position then start running to the ball.
	/// </summary>
	/// <returns>The pass prey on ball.</returns>
	protected bool PredictPassPreyOnBall()
	{
		if ((team.PredictPass == false) || (player != team.PassPlayer) || (player.GotoTargetPos))
		{
			return (false);
		}
		if (player.TargetObject == ball)
		{
			// Still busy running towards the ball
			return (true);
		}
		return (PreyOnBall());
	}


	/// <summary>
	/// The AI waits for the ball if it has been passed to him.
	/// </summary>
	/// <returns>The for passed ball if player is receiver.</returns>
	protected bool WaitForPassedBallIfPlayerIsReceiver()
	{
		if (player == team.PassPlayer)
		{
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Run forward if in own penalty area.
	/// </summary>
	/// <returns>The straight forward if in penalty area.</returns>
	protected bool RunStraightForwardIfInPenaltyArea()
	{
		if (runningStraightForwards == false)
		{
			if (field.IsInPenaltyArea(gameObject, team))
			{
				SsFieldProperties.SsGrid grid = null;
				Vector3 pos = transform.position;
				if (team.PlayDirection > 0.0f)
				{
					pos.x = field.LeftPenaltyArea.max.x + (field.LeftPenaltyArea.size.x / 2.0f);
					grid = field.GetGridAtPoint(pos, true);
				}
				else
				{
					pos.x = field.RightPenaltyArea.min.x - (field.RightPenaltyArea.size.x / 2.0f);
					grid = field.GetGridAtPoint(pos, true);
				}

				if (grid != null)
				{
					SelectPointInGrid(grid, 0.0f);
					runningStraightForwards = true;
					return (true);
				}
			}
		}

		return (runningStraightForwards);
	}


	/// <summary>
	/// Test if the goalkeeper must hold the ball.
	/// </summary>
	/// <returns>The ball.</returns>
	protected bool HoldBall()
	{
		if ((player.State == SsPlayer.states.gk_standHoldBall) && (delayThrowBallTime > Time.time))
		{
			return (true);
		}

		// Wait for the foremost opponent to run backwards past zone "runBackBeforeZoneWhenGoalkeerHasBall"
		if ((delayWaitForOtherTeamToRunBackTime > Time.time) && (otherTeam.ForemostPlayer != null) && 
		    (field.PlayerPastZone(otherTeam.ForemostPlayer, runBackBeforeZoneWhenGoalkeerHasBall, null, true)))
		{
			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Mark the nearest unmarked opponent within the specified zones.
	/// </summary>
	/// <returns>The mark nearest unmarked opponent in zone.</returns>
	/// <param name="beforeZone">Mark players before this zone. Set to -1 if Not used.</param>
	/// <param name="pastZone">Mark players past this zone. Set to -1 if Not used.</param>
	protected bool MarkNearestUnmarkedOpponentInZone(int beforeZone, int pastZone)
	{
		// Do Not mark if the team cannot mark OR player himself is marked
		if ((player.Skills.ai.canMarkPlayers == false) || (player.GetMarkedByPlayer() != null))
		{
			return (false);
		}
		
		// Test if the player is already marking a player in the zone
		if (player.MarkPlayer != null)
		{
			if ((beforeZone >= 0) && (pastZone >= 0))
			{
				if ((field.PlayerBeforeZone(player.MarkPlayer, beforeZone, team, false)) &&
				    (field.PlayerPastZone(player.MarkPlayer, pastZone, team, false)))
				{
					return (true);
				}
			}
			else if (beforeZone >= 0)
			{
				if (field.PlayerBeforeZone(player.MarkPlayer, beforeZone, team, false))
				{
					return (true);
				}
			}
			else if (pastZone >= 0)
			{
				if (field.PlayerPastZone(player.MarkPlayer, pastZone, team, false))
				{
					return (true);
				}
			}

			// Stop marking the previous player
			player.StopMoving(true, true, true);
			delayMarkTimer = Time.time + markPlayerIntervals;

			return (false);
		}
		
		if (delayMarkTimer <= Time.time)
		{
			// Find the nearest player in the zone
			SsPlayer controlPlayer = null;
			SsPlayer tempPlayer = otherTeam.GetNearestPlayer(null, player, null, 
			                                                 controlPlayer, null, 
			                                                 -1.0f, true, beforeZone, pastZone, team, 
			                                                 true, false, -1.0f, -1.0f, false);
			if ((tempPlayer != null) && (player.DistanceToObject(tempPlayer.gameObject) < player.Skills.ai.startMarkDistance))
			{
				player.StopMoving(true, true, true);
				player.SetMarkPlayer(tempPlayer, -1.0f, -1.0f);
				return (true);
			}
		}
		
		return (false);
	}


	/// <summary>
	/// Run to an open spot between the two zones in the row if an opponent is close. It includes the two zones.
	/// </summary>
	/// <returns>The to open spot between zones in row if opponent close.</returns>
	/// <param name="zoneStart">Zone start.</param>
	/// <param name="zoneEnd">Zone end.</param>
	/// <param name="row">Row.</param>
	protected bool RunToOpenSpotBetweenZonesInRowIfOpponentClose(int zoneStart, int zoneEnd, int row)
	{
		if (runningToOpenSpotFromPredator)
		{
			return (true);
		}

		SsPlayer nearestOpponent = null;
		float distance = GetOpponentDistance(true, out nearestOpponent);

		if (distance <= player.Skills.ai.markDistanceTooClose)
		{
			if (RunToOpenSpotBetweenZonesInRow(zoneStart, zoneEnd, row))
			{
				// Briefly delay the predator's movement
				nearestOpponent = player.GetMarkedByPlayer();
				if (nearestOpponent != null)
				{
					nearestOpponent.StopMoving(true, true, false);
				}

				runningToOpenSpotFromPredator = true;
				return (true);
			}
		}
		
		return (false);
	}


	/// <summary>
	/// Run to an open spot between the zones, and in the row, if goalkeeper holds ball.
	/// </summary>
	/// <returns>The to open spot between zones in row if goalkeeper holds ball.</returns>
	/// <param name="zoneStart">Zone start.</param>
	/// <param name="zoneEnd">Zone end.</param>
	/// <param name="row">Row.</param>
	protected bool RunToOpenSpotBetweenZonesInRowIfGoalkeeperHoldsBall(int zoneStart, int zoneEnd, int row)
	{
		if ((match.IsGoalkeeperHoldingTheBall()) && 
		    (match.BallPlayer.HaveBallTime + delayRunToOpenSpotWhenGoalkeeperHoldsBall <= Time.time) && 
		    (player.IsMoving() == false) && 
		    (RunToOpenSpotBetweenZonesInRow(zoneStart, zoneEnd, row)))
		{
			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Run to an open spot between the two zones, and in the row. It includes the two zones.
	/// </summary>
	/// <returns>The to open spot between zones in row.</returns>
	/// <param name="zoneStart">Zone start.</param>
	/// <param name="zoneEnd">Zone end.</param>
	/// <param name="row">Row.</param>
	protected bool RunToOpenSpotBetweenZonesInRow(int zoneStart, int zoneEnd, int row)
	{
		if (runToOpenSpotBetweenZonesInRowTimer <= Time.time)
		{
			Bounds boundsRow = field.GetRowBounds(row);
			Bounds boundsZoneStart;
			Bounds boundsZoneEnd;
			Rect rect;
			SsFieldProperties.SsGridSearchData data;

			if (team.PlayDirection > 0)
			{
				boundsZoneStart = field.GetZoneBounds(zoneStart);
				boundsZoneEnd = field.GetZoneBounds(zoneEnd);
			}
			else
			{
				boundsZoneStart = field.GetZoneBounds(SsFieldProperties.maxZones - zoneEnd - 1);
				boundsZoneEnd = field.GetZoneBounds(SsFieldProperties.maxZones - zoneStart - 1);
			}
			
			rect = new Rect(new Vector2(boundsZoneStart.min.x, boundsRow.min.z), 
			                new Vector2(boundsZoneEnd.max.x - boundsZoneStart.min.x, boundsRow.size.z));


			data = field.FindOpenGridInRect(ref rect, player.Skills.ai.preferredOpenRadius, 1, 2);
			if (player.Skills.ai.preferredOpenRadius > 0.0f)
			{
				if ((data == null) || (data.grid == null) || (data.gridWeight > 0))
				{
					// Try a smaller area
					data = field.FindOpenGridInRect(ref rect, player.Skills.ai.preferredOpenRadius * 0.5f, 1, 1);
					if ((data == null) || (data.grid == null) || (data.gridWeight > 0))
					{
						// Try an even smaller area
						data = field.FindOpenGridInRect(ref rect, 0.0f, 1, 1);
					}
				}
			}

			if ((data != null) && (data.grid != null) && (data.gridWeight <= 0))
			{
				SelectPointInGrid(data.grid, 0.0f);

				// REMINDER: This time also affects a marked player running to an open spot.
				runToOpenSpotBetweenZonesInRowTimer = Time.time + delayRunToOpenSpotBetweenZones;

				trackBallChangePlayerCounter = match.BallChangePlayerCounter;

				return (true);
			}
		}

		return (false);
	}


	/// <summary>
	/// Select a random point in a grid block to move to.
	/// </summary>
	/// <returns>The point in grid.</returns>
	/// <param name="grid">Grid.</param>
	/// <param name="preferDirection">Preferred X direction in the grid. (-1 = prefer area in left side of grid, 1 = right side, 0 = no preference).</param>
	protected bool SelectPointInGrid(SsFieldProperties.SsGrid grid, float preferDirection)
	{
		if (grid == null)
		{
			return (false);
		}
		Vector3 pos;

		if (preferDirection == 0)
		{
			// Anywhere in the grid block
			pos = new Vector3(Random.Range(grid.bounds.min.x, grid.bounds.max.x), 
			                  field.GroundY, 
			                  Random.Range(grid.bounds.min.z, grid.bounds.max.z));
		}
		else if (preferDirection < 0)
		{
			// Prefer a position in the left side of the grid block
			pos = new Vector3(Random.Range(grid.bounds.min.x, grid.bounds.min.x + (grid.bounds.size.x * 0.65f)), 
			                  field.GroundY, 
			                  Random.Range(grid.bounds.min.z, grid.bounds.max.z));
		}
		else
		{
			// Prefer a position in the right side of the grid block
			pos = new Vector3(Random.Range(grid.bounds.min.x + (grid.bounds.size.x * 0.35f), grid.bounds.max.x), 
			                  field.GroundY, 
			                  Random.Range(grid.bounds.min.z, grid.bounds.max.z));
		}

		// Make sure spot is Not too close to field edges
		Bounds fieldBounds = field.PlayArea;
		pos.x = Mathf.Clamp(pos.x, fieldBounds.min.x + fieldSearchBorder, fieldBounds.max.x - fieldSearchBorder);
		pos.z = Mathf.Clamp(pos.z, fieldBounds.min.z + fieldSearchBorder, fieldBounds.max.z - fieldSearchBorder);

		player.SetTargetPosition(true, pos, 1.0f, 2.0f);

		return (true);
	}


	/// <summary>
	/// Run up/down to an open spot if the baller is too close.
	/// </summary>
	/// <returns>The up down to open spot if too close to baller.</returns>
	protected bool RunUpDownToOpenSpotIfTooCloseToBaller()
	{
		if ((match.BallPlayer != null) && (tooCloseToBallerTimer <= Time.time) && 
		    (player.DistanceToObjectSquared(match.BallPlayer.gameObject) < (player.Skills.ai.tooCloseToBallerDist * player.Skills.ai.tooCloseToBallerDist)) && 
		    (player.Grid != null))
		{
			SsFieldProperties.SsGrid grid;
			int direction;

			if (transform.position.z < match.BallPlayer.transform.position.z)
			{
				// Down
				direction = -1;
			}
			else
			{
				// Up
				direction = 1;
			}
			grid = field.FindFirstOpenGridInColumn(field.GetGrid(player.Grid.x, player.Grid.y - direction), direction, field.gridHeight);
			if (grid != null)
			{
				SelectPointInGrid(grid, team.PlayDirection);
				tooCloseToBallerTimer = Time.time + tooCloseToBallerIntervals;
				return (true);
			}
			else
			{
				// No spots found, so try to run right/left
				if (transform.position.x < match.BallPlayer.transform.position.x)
				{
					// Left
					direction = -1;
				}
				else
				{
					// Right
					direction = 1;
				}
				grid = field.FindFirstOpenGridInRow(field.GetGrid(player.Grid.x + direction, player.Grid.y), direction, 2);
				if (grid != null)
				{
					SelectPointInGrid(grid, 0.0f);
					tooCloseToBallerTimer = Time.time + tooCloseToBallerIntervals;
					return (true);
				}
			}
		}


		return (false);
	}


	/// <summary>
	/// Find an open spot ahead and close to the ball.
	/// </summary>
	/// <returns>The open spot ahead and close to ball.</returns>
	protected bool FindOpenSpotAheadAndCloseToBall()
	{
		if (forwardFindOpenSpotTimer <= Time.time)
		{
			if (FindOpenSpotInDirection(team.PlayDirection, ball.transform.position, 
			                            player.Skills.ai.searchArcRadiusMin, player.Skills.ai.searchArcRadiusMax, 
			                            false))
			{
				forwardFindOpenSpotTimer = Time.time + Random.Range(forwardFindOpenSpotTimerMin, forwardFindOpenSpotTimerMax);

				trackBallChangePlayerCounter = match.BallChangePlayerCounter;

				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Find an open spot behind and close to the ball.
	/// </summary>
	/// <returns>The open spot behind and close to ball.</returns>
	protected bool FindOpenSpotBehindAndCloseToBall()
	{
		if (backwardFindOpenSpotTimer <= Time.time)
		{
			if (FindOpenSpotInDirection(-team.PlayDirection, ball.transform.position, 
			                            player.Skills.ai.searchArcRadiusMin, player.Skills.ai.searchArcRadiusMax, 
			                            false))
			{
				backwardFindOpenSpotTimer = Time.time + Random.Range(backwardFindOpenSpotTimerMin, backwardFindOpenSpotTimerMax);

				trackBallChangePlayerCounter = match.BallChangePlayerCounter;

				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Find an open spot in the play direction, using an arc which is divided into sub-arcs to test (one at a time).
	/// </summary>
	/// <returns>The open spot in direction.</returns>
	/// <param name="playDirection">Play direction.</param>
	/// <param name="startPos">Start position.</param>
	/// <param name="minRadius">Minimum radius.</param>
	/// <param name="maxRadius">Max radius.</param>
	/// <param name="useCurrentRow">Indicates if the player must try to use the row at startPos.</param>
	protected bool FindOpenSpotInDirection(float playDirection, Vector3 startPos, float minRadius, float maxRadius,
	                                       bool useCurrentRow)
	{
		SsFieldProperties.SsGridSearchData data, bestData;
		int i, arcIndex, maxArcs;
		float angle, angleStep, startAngle;

		// Number of sub-arcs into which to split the main arc
		maxArcs = 5;

		angleStep = player.Skills.ai.searchAheadArc / (float)maxArcs;

		// Randomly select which sub-arc to use first, so player does Not always start with the same one
		arcIndex = Random.Range(0, maxArcs);

		// REMINDER: Angle 0 is up and increases clockwise
		if (playDirection > 0)
		{
			// Right: forward angle = 90
			startAngle = 90.0f - (player.Skills.ai.searchAheadArc / 2.0f);
		}
		else
		{
			// Left: forward angle = 270
			startAngle = 270.0f - (player.Skills.ai.searchAheadArc / 2.0f);
		}

		// Try to find an open spot in one of the sub-arcs.
		bestData = null;
		for (i = 0; i < maxArcs; i++)
		{
			angle = (float)arcIndex * angleStep;

			data = field.FindOpenGridInArc(startPos, 
			                               startAngle + angle, 
			                               startAngle + angle + angleStep, 
			                               minRadius, maxRadius, 
			                               player.Skills.ai.preferredOpenRadius, 1, 1);
			
			if ((data.grid != null) && (data.grid.playerWeight <= 0))
			{
				if (data.gridWeight <= 0)
				{
					// No players in the group of grid blocks. No need to look for a better place.
					bestData = data;
					break;
				}
				// Select the grid with the lowest grid weight
				if ((bestData == null) || (bestData.gridWeight > data.gridWeight))
				{
					bestData = data;
				}
			}

			arcIndex ++;
			if (arcIndex >= maxArcs)
			{
				arcIndex = 0;
			}
		}


		// Found a grid block. It may Not be empty, but has the lowest gridWeight.
		if (bestData != null)
		{
			SelectPointInGrid(bestData.grid, 0.0f);

#if UNITY_EDITOR
			// DEBUG
			if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches) && 
			    (bestData.gridWeight > 0))
			{
				SsDebugMatch.DrawCircleUp(player.TargetPos, 1.0f, 10, Color.yellow, 1.0f);
				SsFieldProperties.SsGrid grid = bestData.grid;
				if (grid != null)
				{
					SsDebugMatch.DrawRectUp(grid.bounds.center, 
					                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
					                        Color.yellow, 1.0f);
				}
			}
#endif //UNITY_EDITOR

			return (true);
		}
		
		return (false);
	}


	/// <summary>
	/// Goalkeeper dives towards the ball if it is flying towards the goalpost and it is before the specified zone.
	/// </summary>
	/// <returns>The to ball if before zone.</returns>
	/// <param name="zone">Zone.</param>
	protected bool DiveToBallIfBeforeZone(int zone)
	{
		if ((match.BallPlayer != null) || (delayDiveTime > Time.time))
		{
			return (false);
		}

		Vector3 intersectPoint = ball.transform.position;
		bool validDive = false;

		delayDiveTime = Time.time + player.Skills.diveIntervals;

		if (player.IsDiving == false)
		{
			if ((player.IsHurt == false) && 
			    (player.IsOnGround) && 
				(field.IsBallBeforeZone(team, zone)))
			{
				validDive = ((field.IsInPenaltyArea(gameObject, team)) && 
				             (field.IsBallMovingToGoalpost(team, out intersectPoint, true, 2.0f)));

				if ((validDive == false) && (player.IsUserControlled))
				{
					// Human controlled goalkeeper can dive when ball is anywhere in the penalty area
					if (field.IsInPenaltyArea(ball.gameObject, team))
					{
						validDive = true;
					}
				}
			}
		}
		
		if (validDive)
		{
			return (player.Dive(true, false));
		}

		return (false);
	}


	/// <summary>
	/// Run vertically up/down in the goalkeeper area.
	/// </summary>
	/// <returns>The ball vertically and stay in goalkeeper area.</returns>
	protected bool FollowBallVerticallyAndStayInGoalkeeperArea()
	{
		if (goalkeeperFollowBallInGoalkeeperArea <= Time.time)
		{
			if (field.IsInGoalArea(gameObject, team))
			{
				SelectSpotInGoalkeeperArea();
				goalkeeperFollowBallInGoalkeeperArea = Time.time + player.Skills.ai.gkCasualFollowBallDelay;
				return (true);
			}
			else
			{
				// Run towards the goalkeeper area
				return (StandStillInGoalkeeperArea(team));
			}
		}
		
		return (false);
	}


	/// <summary>
	/// Select a spot in the goalkeeper area, based on the vertical position of the ball.
	/// </summary>
	/// <returns>The spot in goalkeeper area.</returns>
	protected bool SelectSpotInGoalkeeperArea()
	{
		Vector3 pos = new Vector3(0.0f, field.GroundY, 0.0f);
		Bounds bounds = field.GetGoalArea(team);
		float border;

		if (player.Skills.ai.gkStayNearCentreOfGoalArea)
		{
			// Use the Z of the goal posts area, so goalkeeper stays closer to goal posts centre
			Bounds tempBounds = field.GetGoalPosts(team);
			bounds.min = new Vector3(bounds.min.x, bounds.min.y, tempBounds.min.z);
			bounds.max = new Vector3(bounds.max.x, bounds.max.y, tempBounds.max.z);
		}

		border = bounds.size.z * 0.05f;	// Stay away from area's edge by 5% its length

		// Prefer the front 2/3 of the goal area, so Not too close to the posts
		if (team.PlayDirection > 0)
		{
			pos.x = Random.Range(bounds.min.x + (bounds.size.x * 0.3f), bounds.max.x);
		}
		else
		{
			pos.x = Random.Range(bounds.min.x, bounds.max.x - (bounds.size.x * 0.3f));
		}

		if (ball.transform.position.z > bounds.max.z - border)
		{
			// Ball past the top of the area
			pos.z = bounds.max.z - border;
		}
		else if (ball.transform.position.z < bounds.min.z + border)
		{
			// Ball past the bottom of the area
			pos.z = bounds.min.z + border;
		}
		else
		{
			// Ball within the area
			pos.z = ball.transform.position.z;
		}

		// Some randomness
		pos.z += Random.Range(-1.0f, 1.0f);

		// Make sure spot is Not too close to field edges
		Bounds fieldBounds = field.PlayArea;
		pos.x = Mathf.Clamp(pos.x, fieldBounds.min.x + fieldSearchBorder, fieldBounds.max.x - fieldSearchBorder);
		pos.z = Mathf.Clamp(pos.z, fieldBounds.min.z + fieldSearchBorder, fieldBounds.max.z - fieldSearchBorder);

		player.SetTargetPosition(true, pos, 1.0f, 2.0f);

		return (true);
	}


	/// <summary>
	/// Stand still in the goalkeeper area. Run towards the area if Not inside the area.
	/// </summary>
	/// <returns>The still in goalkeeper area.</returns>
	protected bool StandStillInGoalkeeperArea(SsTeam team)
	{
		if (field.IsInGoalArea(gameObject, team))
		{
			return (true);
		}
		
		if (runningToGoalkeeperArea == false)
		{
			SelectSpotInGoalkeeperArea();
			runningToGoalkeeperArea = true;
		}
		
		return (runningToGoalkeeperArea);
	}


	/// <summary>
	/// Tackle the baller if he is close.
	/// </summary>
	/// <returns>The baller.</returns>
	protected bool TackleBaller()
	{
		Vector3 vec;

		// NOTE: We do Not allow the human team's AI players to slide. Only the player controlled by the human can slide, so that 
		//	all the "hard" work is done by the human.
		if ((team.IsUserControlled == false) && 
		    ((team.OpponentPassesSinceLastSlide > player.Skills.ai.forceSlideAfterPasses) || 
		 	 ((team.GetDelaySlidingTime() <= Time.time) && (DelaySlidingTime <= Time.time))) && 
		    (player.CanSlide(out vec)))
		{
			// Start slide tackle
			player.SetState(SsPlayer.states.slideTackle, true, SsPlayer.states.idle, vec);

			team.SetDelaySlidingTime(Time.time + player.Skills.ai.slideIntervals, false);
			team.OpponentPassesSinceLastSlide = 0;

			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Test if the player becomes the baller predator (i.e. has to chase the player with the ball).
	/// </summary>
	/// <returns>The on baller.</returns>
	protected bool PreyOnBaller()
	{
		if ((match.BallPlayer != null) && 
		    (player == team.NearestUnhurtAiPlayerToBall) && 
		    (team.BallerPredatorDelayTime <= Time.time) && 
		    ((team.BallerPredatorsCount <= 0) || (team.BallerPredatorsMovingCount <= 0)))
		{
			MakeBallerPredator();
			return (true);
		}

		return (false);
	}


	/// <summary>
	/// Make the player the baller predator.
	/// </summary>
	/// <returns>The baller predator.</returns>
	protected void MakeBallerPredator()
	{
		if ((match.BallPlayer != null) && (match.BallPlayer.Team != team))
		{
			team.StopAllBallerPredators();
			player.StopMoving(true, true, true);
			
			// Is the ball player already marked?
			if (match.BallPlayer.GetMarkedByPlayer() != null)
			{
				// Stop all the other players marking the ball player
				match.BallPlayer.ClearAllMarkedByPlayers(true);
			}
			
			player.SetMarkPlayer(match.BallPlayer, 0.0f, 0.0f);	// Zero radius, because player must get the baller

			player.SetBallerPredator(true, false);

			team.BallerPredatorDelayTime = Time.time + SsTeam.defaultBallerPredatorDelayTime;
		}
	}


	/// <summary>
	/// Test if the predator is taking too long to get to the baller and there is another player closer to the baller.
	/// </summary>
	/// <returns>The predator taking too long.</returns>
	protected bool BallerPredatorTakingTooLong()
	{
		if (player.IsBallerPredator)
		{
			// Is player Not the nearest unhurt to the ball, or player is hurt?
			if ((player != team.NearestUnhurtAiPlayerToBall) || (player.IsHurt))
			{
				if ((player.BallerPredatorTime + player.Skills.ai.checkNearestToBallerTime < Time.time) || 
				    (player.IsHurt))
				{
					match.StopBallerPredator(team);

					team.BallerPredatorDelayTime = 0.0f;	// Change instantly to another player

					return (true);
				}
			}
		}

		return (false);
	}


	/// <summary>
	/// Test if the goalkeeper must dive at the baller in the penalty area.
	/// </summary>
	/// <returns>The dive at opponent in penalty area.</returns>
	protected bool DiveAtOpponentInPenaltyArea()
	{
		if ((match.BallPlayer != null) && (delayDiveTime <= Time.time) && 
		    (player.DistanceToObjectSquared(match.BallPlayer.gameObject) < (player.Skills.ai.startDiveAtOpponentDistance * player.Skills.ai.startDiveAtOpponentDistance)) && 
		    (field.IsInPenaltyArea(match.BallPlayer.gameObject, team)))
		{
			if (player.Dive(false, true))
			{
				divingAtBaller = true;
				return (true);
			}
		}

		return (false);
	}
}
