using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// Player skills. These can be attached to the player (or children), or to the team (or children).
/// A player can have multiple skills attached (e.g. 1 for human team, 1 for AI team, or multiple for different game difficulties).
/// The skills can be attached to a team, if you want multiple players to use the same skills.
/// The player first checks if there are skills attached to himself, and if none then he checks if skills are attached to the team.
/// But there must be at least 1 skill component available.
/// </summary>
public class SsPlayerSkills : MonoBehaviour
{
	// Enums
	//------
	// Specifies when skills should be used
	public enum whenToUseSkills
	{
		asDefault = 0,								// Default skills to use when no other skills are found.
		humanTeamAndAnyMatchDifficulty,				// Use when team is human controlled and for any match difficulty.
		computerTeamAndAnyMatchDifficulty,			// Use when team is compter controlled and for any match difficulty.
		humanTeamForSpecificMatchDifficulty,		// Use when team is human controlled and for a specific match difficulty (or higher).
		computerTeamForSpecificMatchDifficulty,		// Use when team is computer controlled and for a specific match difficulty (or higher).

		// REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.
	}


	// Classes
	//--------
	// AI skills: These only affect AI controlled players
	[System.Serializable]
	public class SsAiSkills
	{
		// Field has 8 vertical zones, 4 on each side. 0 is nearest to player's goals, and 7 nearest to other team's goals.

		// AI: Passing and Shooting
		//.........................
		[Header("AI: Passing and Shooting")]

		[Tooltip("The AI shoots when they are past this zone.")]
		[Range(0, SsFieldProperties.maxZones - 1)]
		public int shootPastZone = 6;

		[Tooltip("Can shoot when not facing the goal posts, when past zone 5.5.")]
		public bool ignoreAngleWhenShooting;

		[Tooltip("Chance to shoot when in the goal area.")]
		[Range(0.0f, 100.0f)]
		public float chanceShootInGoalArea = 100.0f;

		[Tooltip("Can shoot if foremost player, and past the half way mark.")]
		public bool canShootIfForemostPlayer;

		[Tooltip("Chance the baller will pass to the player at the front.")]
		[Range(0.0f, 100.0f)]
		public float chancePassToForemostPlayer = 20.0f;

		[Space(10)]
		[Tooltip("Min time between shooting attempts. A random value is selected between min and max.")]
		public float shootDelayMin = 0.5f;
		[Tooltip("Max time between shooting attempts. A random value is selected between min and max.")]
		public float shootDelayMax = 1.0f;

		[Space(10)]
		[Tooltip("Try to hold the ball for at least this long before passing.")]
		public float holdBallBeforePassTime = 1.0f;

		[Tooltip("Always try to pass forward first.")]
		public bool alwaysPassForward;

		[Space(10)]
		[Tooltip("Pass if had ball for longer than this time (min). A random value will be selected between min and max.")]
		public float passIfHadBallTimeMin = 5.0f;

		[Tooltip("Pass if had ball for longer than this time (max). A random value will be selected between min and max.")]
		public float passIfHadBallTimeMax = 10.0f;

		[Tooltip("Pass if opponent is sliding to me and closer than this distance.")]
		public float passIfOpponentSlideNear = 3.0f;


		// AI: Tackling
		//.............
		[Header("AI: Tackling")]
		[Tooltip("Ignore the \"Valid Slide Angle\" and slide in any direction.")]
		public bool ignoreSlideAngle = true;

		[Tooltip("Slide tackle success chance.")]
		[Range(0.0f, 100.0f)]
		public float slideTackleSuccessChance = 80.0f;

		[Tooltip("Intervals between slide tackles.")]
		public float slideIntervals = 1.0f;
		
		[Tooltip("Target must have the ball for this time before this player can slides to him from behind.")]
		public float slideFromBehindDelay = 3.0f;
		
		[Tooltip("Force a slide if the opponents pass the ball more than this amount. It prevents the opponents constantly passing to each " + 
		         "other, therefore never giving the AI time to slide.")]
		public int forceSlideAfterPasses = 4;

		[Tooltip("Can start sliding to an opponent when the opponent is busy kicking.")]
		public bool canSlideKickingOpponent = false;

		[Tooltip("Can chande direction while sliding?\n(NOTE: Override's the default setting.)")]
		public bool canChangeSlideDirection;


		// AI: Diving (Goalkeeper)
		//------------------------
		[Header("AI: Diving (Goalkeeper)")]
		[Tooltip("Chance to save a ball while diving.")]
		[Range(0.0f, 100.0f)]
		public float chanceDiveSave = 70.0f;

		[Tooltip("Goalkeeper can start diving at player who has the ball when he is closer than this distance.")]
		public float startDiveAtOpponentDistance = 10.0f;


		// AI: Loose Ball
		//...............
		[Header("AI: Loose Ball")]
		[Tooltip("Time it takes to react to collect a loose ball, when player is nearest to the ball.")]
		public float looseReactTime = 3.0f;

		[Tooltip("If a loose ball moves slower than this then try to collect it.")]
		public float looseReactSpeedMin = 5.0f;


		// AI: Marking/Marked
		//...................
		[Header("AI: Marking/Marked")]
		[Tooltip("Can this player mark other players?")]
		public bool canMarkPlayers = true;

		[Tooltip("Only start marking players within this distance.")]
		public float startMarkDistance = 10.0f;

		[Tooltip("Run away if opponent marking this player is nearer than this distance.")]
		public float markDistanceTooClose = 3.0f;

		[Tooltip("Intervals at which to check if this player is the nearest to the opponent who has the ball.")]
		public float checkNearestToBallerTime = 2.0f;

		[Space(10)]
		[Tooltip("Min radius to reach when running to a player to mark. A random value will be selected between min and max.")]
		public float markDistanceMin = 1.0f;

		[Tooltip("Max radius to reach when running to a player to mark. A random value will be selected between min and max.")]
		public float markDistanceMax = 2.0f;

		[Tooltip("Time to delay movement when too close to a target player.")]
		public float delayMoveTooCloseToTarget = 1.0f;


		// AI: Sidestep
		//.............
		[Header("AI: Sidestep")]
		[Tooltip("Chance to sidestep a sliding opponent.")]
		[Range(0.0f, 100.0f)]
		public float chanceSideStep = 60.0f;
		[Tooltip("Distance to sidestep")]
		public float sideStepDistance = 3.0f;
		[Tooltip("Can start sidestep when opponent nearer than this distance")]
		public float startSideStepDistance = 5.0f;
		[Tooltip("If opponent starts sliding nearer than this distance then reduce chance of sidestep by half.")]
		public float sideStepReduceDistance = 3.0f;
		[Tooltip("Sidestep duration.")]
		public float sideStepTimeMax = 2.0f;
		[Tooltip("Delay between sidesteps")]
		public float delaySideStepTimeMax = 3.0f;


		// AI: Searching
		//..............
		[Header("AI: Searching")]
		[Tooltip("When searching for an open spot, first try to find a spot with a radius this size. (Also affected by the field's grid size.)")]
		public float preferredOpenRadius = 5.0f;

		[Space(10)]
		[Tooltip("Arc angle to use when searching for an open spot ahead, when running in the direction of the opponent goal posts. If greater than 180 then the AI may run backwards.")]
		[Range(0.0f, 360.0f)]
		public float searchAheadArc = 100.0f;
		[Tooltip("Arc min radius when searching ahead/behind. Random radius will be selected between min and max.")]
		public float searchArcRadiusMin = 15.0f;
		[Tooltip("Arc max radius when searching ahead/behind. Random radius will be selected between min and max.")]
		public float searchArcRadiusMax = 30.0f;

		[Tooltip("Delays for goalkeeper to casually follow the ball, while he is in the goal area. (Goalkeeper)")]
		public float gkCasualFollowBallDelay = 0.3f;

		[Tooltip("Try to stay near the centre of the goal area. (Goalkeeper)")]
		public bool gkStayNearCentreOfGoalArea = true;


		// AI: Misc
		//.........
		[Header("AI: Misc")]
		[Tooltip("Intervals at which to change direction when running to a target.")]
		public float changeDirectionIntervals = 0.2f;

		[Tooltip("If player with ball (in same team) is closer than this distance to AI, then AI runs away.")]
		public float tooCloseToBallerDist = 1.5f;

	} //ai


	// Public
	//-------
	[Space(10)]
	[Tooltip("Helpful description to help identify the various skills. (If this is empty then it is automatically set to the name of the game object, in the editor.)")]
	public string description;

	// When To Use
	//............
	[Header("When To Use")]
	[Tooltip("Specify when the player must use this skills component. This is ignored if only one skills component is attached to the player. \n" + 
	         "For example: The player can use different skills for the human and computer controlled teams, and for different match difficulties. \n" + 
	         "So you can attach multiple skills components to the player, and the game will use the relevant one.")]
	public whenToUseSkills whenToUseSkill = whenToUseSkills.asDefault;
	[Tooltip("This skills component will only used for this specific match difficulty. Only valid if \"When To Use Skill\" makes use of a difficulty, \n" +
	         "otherwise this is ignored.")]
	public int useForDifficulty = 0;



	// Human and AI: These affect user controlled and AI controlled players
	//---------------------------------------------------------------------

	// Speeds
	//.......
	[Header("Speeds")]
	[Tooltip("Run speed.")]
	public float runSpeed = 6.0f;

	[Tooltip("Sprint speed (running with sprint button held down).")]
	public float sprintSpeed = 7.5f;

	[Tooltip("Sprint animation speed scale (i.e. makes run animation faster when sprinting).")]
	public float sprintAnimationSpeedScale = 2.0f;

	[Tooltip("Run speed, when running from goalkeeper who has the ball.")]
	public float runFromGoalkeeperSpeed = 10.0f;

	[Tooltip("Slide tackle speed.")]
	public float slideTackleSpeed = 8.0f;

	[Tooltip("Dive speed. (Goalkeeper)")]
	public float diveSpeed = 10.0f;

	[Tooltip("Max dive up speed. (Goalkeeper)")]
	public float maxDiveUpSpeed = 7.0f;

	[Tooltip("Speed at which to turn (degrees per second). This mainly affects the AI, because user controlled player turns instantly.")]
	public float turnSpeed = 400.0f;


	// Passing and Shooting
	//.....................
	[Header("Passing and Shooting")]

	public float maxShootSpeed = SsBall.maxSpeedMetersPerSecond;
	public float maxPassSpeed = SsBall.averageSpeedMetersPerSecond;
	public float maxHeaderSpeed = SsBall.averageSpeedMetersPerSecond;
	public float maxThrowSpeed = SsBall.averageSpeedMetersPerSecond;

	[Space(10)]
	[Tooltip("Can only pass to another player within this angle, relative to this player's forward direction.")]
	[Range(0.0f, 360.0f)]
	public float canPassWithinAngle = 90.0f;

	[Tooltip("Chance for a bad pass. If the pass is bad then it will use Pass Accuracy Radius to determine the position to pass to.")]
	[Range(0.0f, 100.0f)]
	public float passBadChance = 0.0f;

	[Tooltip("Radius of how close to pass to a target player/location. This is only used when a bad pass happens. A random position is selected within the radius. Bigger radius is less accurate, zero is most accurate.")]
	public float passAccuracyRadius = 0.0f;


	[Space(10)]
	[Tooltip("Chance to randomly aim when shooting at the goal. If he aims randomly then he might shoot straight at the goalkeeper.")]
	[Range(0.0f, 100.0f)]
	public float chanceRandomAimAtGoal = 10.0f;

	[Tooltip("Chance to shoot from own goal posts. (Increases to \"Chance Shoot From Halfway\" when player moves closer to the halfway mark.)" + 
	         "\nNote: For AI this is limited by \"Shoot Past Zone\".")]
	[Range(0.0f, 100.0f)]
	public float chanceShootFromOwnGoal = 0.0f;
	[Tooltip("Chance to shoot from halfway. (Increases to \"Chance Shoot From Zone 6\" when " + 
	         "player moves past halfway and closer to zone 6.) " + 
	         "\nNote: For AI this is limited by \"Shoot Past Zone\".")]
	[Range(0.0f, 100.0f)]
	public float chanceShootFromHalfway = 2.0f;

	[Tooltip("Chance to shoot when past zone 6. (Increases to \"Chance Shoot Before Zone 8\" when player moves closer to zone 8.)" + 
	         "\nNote: For AI this is limited by \"Shoot Past Zone\".")]
	[Range(0.0f, 100.0f)]
	public float chanceShootFromZone6 = 5.0f;

	[Tooltip("Chance to shoot before zone 8." + 
	         "\nNote: For AI this is limited by \"Shoot Past Zone\".")]
	[Range(0.0f, 100.0f)]
	public float chanceShootBeforeZone8 = 20.0f;

	[Tooltip("Chance for a bad shot to goal. If the shot is bad then it will use Shoot Accuracy Radius to determine the position to shoot to.")]
	[Range(0.0f, 100.0f)]
	public float shootBadChance = 0.0f;
	
	[Tooltip("Radius of how close to shoot to a target location. This is only used when a bad shot happens. A random position is selected within the radius. Bigger radius is less accurate, zero is most accurate.")]
	public float shootAccuracyRadius = 0.0f;


	// Tackling
	//.........
	[Header("Tackling")]
	public bool canSlideTackle = true;

	[Tooltip("How far to slide.")]
	public float slideTackleDistance = 4.0f;

	[Tooltip("The player can start sliding if he is closer than this distance to the target player. (Increase this if the player's slide speed is fast.)")]
	public float startSlideDistance = 10.0f;

	[Tooltip("Player must face target within this angle to start sliding towards the target, otherwise just slide forward. \n" + 
	         "Note: AI has the \"Ignore Slide Angle\" property which will ignore this angle if it is set.")]
	[Range(0.0f, 360.0f)]
	public float validSlideAngle = 90.0f;

	[Tooltip("Can collect ball while sliding?")]
	public bool canGetBallWhileSliding = true;

	[Tooltip("Can chande direction while sliding?\n(NOTE: AI also has this property to allow just the AI to change direction.)")]
	public bool canChangeSlideDirection;

	[Tooltip("Can start sliding to goalkeeper who is holding the ball.")]
	public bool canSlideGoalkeeperWithBall = false;
	


	// Diving (Goalkeeper)
	//....................
	[Header("Diving (Goalkeeper)")]
	[Tooltip("Time to hang in the air when diving (delay before falling down). (Goalkeeper)")]
	public float diveHangTime = 0.1f;

	[Tooltip("Max dive distance. Might dive further if high in the air. (Goalkeeper)")]
	public float maxDiveDistance = 5.0f;
	
	[Tooltip("Intervals at which to try diving to a ball. (Goalkeeper)")]
	public float diveIntervals = 0.1f;

	[Tooltip("Delay before trying the next dive, after the current dive has ended.")]
	public float nextDiveDelay = 1.0f;

	[Tooltip("Human player: Chance to save a ball while diving. (AI player chance is under the AI skills.)")]
	[Range(0.0f, 100.0f)]
	public float humanChanceDiveSave = 90.0f;

	[Tooltip("Chance goalkeeper will punch the ball backwards, towards the net.")]
	[Range(0.0f, 100.0f)]
	public float chanceGoalkeeperPunchBack = 10.0f;
	[Tooltip("Chance goalkeeper will punch the ball forwards, away from the net.")]
	[Range(0.0f, 100.0f)]
	public float chanceGoalkeeperPunchForward = 20.0f;


	// Dribbling
	//..........
	[Header("Dribbling")]
	[Tooltip("Min dribble distance. Kick ball if it comes nearer than this distance.")]
	public float ballDribbleDistanceMin = 0.5f;
	[Tooltip("Max dribble distance. Kick ball to this distance when dribbling.")]
	public float ballDribbleDistanceMax = 1.5f;
	[Tooltip("Distance ball can move to the sides while dribbling.")]
	public float ballDribbleSideDistance = 0.2f;
	[Tooltip("Kick forward speed while dribbling.")]
	public float ballDribbleKickSpeed = 15.0f;


	// Misc
	//.....
	[Header("Misc")]
	[Tooltip("Duration to lie on the ground when tackled/hurt. Set to zero to make player get up as soon as fall animation is done.")]
	public float lieOnGroundDuration = 1.0f;



	// AI: These only affect AI controlled players
	//--------------------------------------------
	[Header("AI")]
	[Tooltip("These only affect AI controlled players.")]
	public SsAiSkills ai;




	// Private
	//--------

	// IMPORTANT NOTE: The skills component can be shared by multiple players, therefore do Not add any variables that are 
	//					used by a single player. All variables might be shared by multiple players.

	private int searchScore;				// Score when searching for the best skills to use.


	// Properties
	//-----------
	public int SearchScore
	{
		get { return(searchScore); }
		set
		{
			searchScore = value;
		}
	}

	

	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		ResetMe();
	}
	
	
	/// <summary>
	/// Reset the skills. Calculates some skills and sets some random skills.
	/// </summary>
	public void ResetMe()
	{
	}
	
	
	/// <summary>
	/// Adjusts the pass to position based on the skill of the player (i.e. good pass or bad pass).
	/// </summary>
	/// <returns>The pass position.</returns>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	public Vector3 AdjustPassPosition(Vector3 from, Vector3 to)
	{
		bool passBad = (Random.Range(0, 100) < passBadChance);
		if ((passAccuracyRadius <= 0.0f) || (passBad == false))
		{
			return (to);
		}

		float radius = Mathf.Min(Vector3.Distance(from, to), passAccuracyRadius);
		Vector2 offset = Random.insideUnitCircle * radius;


#if UNITY_EDITOR
		// DEBUG
		if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showBadPass))
		{
			SsDebugMatch.DrawCrossUp(to, 0.5f, Color.yellow, 3.0f);
			SsDebugMatch.DrawCircleUp(to, radius, 10, Color.yellow, 3.0f);
			SsDebugMatch.DrawCrossUp(to + new Vector3(offset.x, 0.0f, offset.y), 0.5f, Color.red, 3.0f);
		}
#endif //UNITY_EDITOR


		return (to + new Vector3(offset.x, 0.0f, offset.y));
	}


	/// <summary>
	/// Adjusts the position to shoot to based on the skill of the player (i.e. good shot or bad shot).
	/// </summary>
	/// <returns>The shot position.</returns>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	public Vector3 AdjustShootPosition(Vector3 from, Vector3 to)
	{
		bool shootBad = (Random.Range(0, 100) < shootBadChance);
		if ((shootAccuracyRadius <= 0.0f) || (shootBad == false))
		{
			return (to);
		}
		
		float radius = Mathf.Min(Vector3.Distance(from, to), shootAccuracyRadius);
		Vector3 offset = Random.insideUnitSphere * radius;
		
		
#if UNITY_EDITOR
		// DEBUG
		if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showBadShot))
		{
			SsDebugMatch.DrawCrossUp(to, 0.5f, Color.yellow, 3.0f);
			SsDebugMatch.DrawCircleUp(to, radius, 10, Color.yellow, 3.0f);
			SsDebugMatch.DrawCrossUp(to + offset, 0.5f, Color.red, 3.0f);
		}
#endif //UNITY_EDITOR

		return (to + offset);
	}


#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		// Update description in the editor
		if (string.IsNullOrEmpty(description))
		{
			description = name;
		}
	}
#endif //UNITY_EDITOR

}
