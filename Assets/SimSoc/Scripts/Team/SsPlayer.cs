using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SsPlayerAnimations))]

/// <summary>
/// Player.
/// </summary>
public class SsPlayer : SsGameObject
{
    // Const/Static
    //-------------
    public const float averageSpeedMilesPerHour = 8.5f;     // Average run speed (miles per hour)
    public const float averageSpeedMetersPerSecond = (averageSpeedMilesPerHour * SsBall.milesToMetres) / (60.0f * 60.0f);

    public const float maxLastTimeHadBall = 0.5f;           // Player can only collect the ball if he has Not had it for longer than this time.
    public const float passToMinLastTimeHadBall = -1.0f;    // Pass to players who's lastTimeHadBall is greater than this value

    // Angle from horizontal line. Player must be facing within this angle towards goalposts to shoot at goal.
    public const float validFacingShootAngle = 70.0f;

    // At the start of the match, delay shooting for this duration, when player is before half way mark
    public const float delayShootingBeforeHalfway = 5.0f;

    public const float kickToNoOneAheadDistanceMin = 5.0f;      // Min distance to kick ahead to no-one
    public const float kickToNoOneAheadDistanceMax = 20.0f;     // Max distance to kick ahead to no-one

    public const float tripUpForce = 2.5f;                      // Upwards force added to player when he falls, when tackled
    public const float fallForwardDistance = 2.0f;              // Fall forward distance
    public const float maxDelaySlideChangeDirection = 0.1f;     // Delay changing direction when sliding

    public const float cannotTouchBallWhenReleaseTime = 0.5f;   // Cannot touch ball when released ball
    public const float cannotTouchBallWhenTackledTime = 2.0f;   // Cannot touch the ball for this time when tackled

    private const float dribbleBallExtraSpeed = 1.0f;           // Dribble ball this much faster than the player's run speed

    private const float diveSlowDownSpeedWithBall = 20.0f;      // Slow down speed for dive when goalkeeper has the ball

    private const float minStartBicycleKickDistanceSquared = 1.0f;  // Bicycle kick can start when ball is between the min and max distances from the player
    private const float maxStartBicycleKickDistanceSquared = 15.0f; // Bicycle kick can start when ball is between the min and max distances from the player
    private const float ballBicycleHeightMin = 1.0f;        // Ball height must be between min and max to perform a bicycle kick
    private const float ballBicycleHeightMax = 10.0f;       // Ball height must be between min and max to perform a bicycle kick
    private const float upBicycleForce = 4.0f;              // Up force when doing a bicycle kick
    private const float bicycleDelayGravity = 0.0f;         // Delay gravity when moving up for bicycle kick

    private const float punchBallBackValidHeight = 2.0f;    // Ball must be higher than this for the goalkeeper to punch it backwards.
    private const float punchBallHeightMin = 0.0f;          // Punch ball this height min
    private const float punchBallForwardHeightMax = 2.0f;   // Punch ball forward this height max
    private const float punchBallBackwardHeightMax = 3.0f;  // Punch ball backward this height max

    private const float headerUpForce = 10.0f;              // Min up force of the player when he heads the ball



    // Enums
    //------
    // Gender types
    public enum genders
    {
        female = 0,
        male,
        both,
        none,
        unknown,

        // REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.

        maxEnums
    }


    // Player positions
    public enum positions
    {
        forward = 0,
        midfielder,
        defender,
        goalkeeper,

        // REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.

        maxPositions
    }


    // Player states. These are linked up to animation states via SsAnimationProperties.
    public enum states
    {
        idle = 0,               // Idle
        run,                    // Run
        kickNear,               // Kick near
        kickMedium,             // Kick medium
        kickFar,                // Kick far

        slideTackle = 6,        // Slide tackle
        falling,                // Falling to the ground (e.g. after being slide tackled)
        inPain,                 // Lying on the ground in pain. Always played after falling.
        throwInHold,            // Hold ball for a throw in
        throwIn,                // Throw in (e.g. when ball went out the side of the field)
        bicycleKick,            // Bicycle kick
        header,                 // Header
        gk_diveForward,         // Goalkeeper: dive forward
        gk_diveUp,              // Goalkeeper: dive up
        gk_diveLeft,            // Goalkeeper: dive left
        gk_diveRight,           // Goalkeeper: dive right

        gk_standHoldBall = 21,  // Goalkeeper: stand holding the ball
        gk_throwBallNear,       // Goalkeeper: throw in ball near
        gk_throwBallMedium,     // Goalkeeper: throw in ball medium
        gk_throwBallFar,        // Goalkeeper: throw in ball far

        goalCelebration,        // Goal celebration
        ownGoal,                // Own goal

        // REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.

        maxStates
    }


    // Dribble states
    public enum dribbleStates
    {
        notDribbling,           // Not dribbling
        getBall,                // Player just got ball.
        idle,                   // Ball idle. Ball is standing still and/or player is moving towards ball.
        moveForward,            // Ball is moving forward after it has been kicked.
    }



    // Classes
    //--------

    // Position properties
    [System.Serializable]
    public class SsPositionProperties
    {
        [Tooltip("The position to play.")]
        public positions position;

        [Tooltip("Specify which AI component to use for the position. The component must be attached to the player, then dragged here. " +
                 "If this is null then a default AI will be attached and used (e.g. SsPlayerAiDefender).")]
        public SsPlayerAi ai;
    }


    // A delayed shoot (e.g. wait for kick animation to end before moving the ball).
    public class SsDelayShoot
    {
        public bool pending;                        // Is there a delayed shoot pending?
        public float delayTime;                     // Shoot will happen when this time is reached (e.g. Time.time + 1)

        // Parameters for the ball's Shoot method:
        public Vector3 targetPos;
        public bool shotAtGoal;
        public AudioClip clip;
    }


    // A delayed pass (e.g. wait for kick animation to end before moving the ball).
    public class SsDelayPass
    {
        public bool pending;                        // Is there a delayed pass pending?
        public float delayTime;                     // Pass will happen when this time is reached (e.g. Time.time + 1)

        // Parameters for the ball's Pass method:
        public SsPlayer toPlayer;
        public Vector3 targetPos;
        public bool clearBallTeam;
        public AudioClip clip;
    }



    // Public
    //-------
    [Tooltip("Unique ID. It is used to identify the resource. Do Not change this after the game has been released.")]
    public string id;

    // Personal Info
    [Header("Personal Info")]
    public string firstName;
    public string lastName;
    [Tooltip("Preferred name/nickname.")]
    public string preferredName;
    [Tooltip("Short name used on certain UIs.")]
    public string shortName;
    [Tooltip("3 letter name used on certain UIs.")]
    public string name3Letters;
    [Tooltip("Gender type.")]
    public genders gender = genders.female;
    public int age;
    public int jerseyNumber;
    [Tooltip("Height of player, from the ground to the top of his head.")]
    public float headHeight = 1.6f;


    [Space(10)]
    [Tooltip("All the positions which the player can play.")]
    public SsPositionProperties[] positionProperties;


    // Mesh
    [Header("Mesh")]
    [Tooltip("If the player has a mesh child then link it here, otherwise link to a mesh prefab which will be spawned and attached to the player (e.g. an fbx file).")]
    public GameObject meshChildOrPrefab;

    [Tooltip("Mesh position offset relative to the player game object. The player game object's pivot will be positioned on the ground. (Optional. Only used when Mesh Prefab is used.)")]
    public Vector3 meshOffsetPosition;

    [Tooltip("Mesh rotation offset relative to the player game object. (Optional. Only used when Mesh Prefab is used.)")]
    public Vector3 meshOffsetRotation;


    // Collission
    [Header("Collision")]
    [Tooltip("Collider used for detecting collision with the ball. Good idea to make this slightly larger than the player.")]
    public BoxCollider ballCollider;
    [Tooltip("Collider used for detecting collision with the ball when diving. Good idea to make this slightly larger than the player.")]
    public BoxCollider ballDiveCollider;
    [Tooltip("Collider used for detecting collision with other players. (Leave empty if the same as the ball collider.)")]
    public BoxCollider playerCollider;
    [Tooltip("Collder used for testing if the player is visible on screen. (Leave empty if the same as the ball or player collider.)")]
    public BoxCollider visibilityCollider;


    // Shadow
    [Header("Shadow")]
    [Tooltip("Fake shadow to use for all the players. Each player can override this and have their own shadow.")]
    public SsFakeShadow fakeShadowPrefab;


    // Private
    //--------
    protected SsBall ball;                          // Reference to the ball

    protected positions position;                   // Current playing position (e.g. defender). Use Position to change this.
    protected SsPlayerSkills skills;                // Skills
    protected SsPlayerHuman human;                  // Process human input
    protected SsPlayerAi ai;                        // Process AI decisions and input

    protected SsTeam team;                          // Team to which this player belongs
    protected SsTeam otherTeam;                     // Opponent team
    protected int index;                            // Index in the team list
    protected int formationIndex;                   // Index in formation's list of players

    protected GameObject mesh;                      // Mesh, was a child or spawned from a prefab
    protected Quaternion meshDefaultRot;            // Mesh default local rotation

    protected SsPlayerSounds sounds;                // Player sounds
    protected SsPlayerParticles particles;          // Player particles

    protected CharacterController controller;       // Character controller

    protected SsPlayerAnimations animations;        // Player animations

    protected SsPlayerStateMatrix stateMatrix;      // State matrix

    protected float ballBoxRadius = 1.0f;           // Radius of ballCollider
    protected float ballBoxHeight = 2.0f;           // Height of ballCollider
    protected float ballDiveBoxRadius = 1.0f;       // Radius of ballDiveCollider
    protected float ballDiveBoxHeight = 2.0f;       // Height of ballDiveCollider
    protected float playerBoxRadius = 1.0f;         // Radius of playerCollider
    protected float playerBoxHeight = 2.0f;         // Height of playerCollider
    protected float ballLostDistanceSqr = 3.0f * 3.0f;  // If ball further than this then player lost the ball (under certain conditions).
    protected float ballLostDistanceAlwaysSqr = 3.0f * 3.0f;    // Player always loses the ball if it is further than this distance.

    // Small empty space around player. Used to position player (e.g. near field edge, other players close to player, etc.)
    protected float personalSpaceRadius = 2.0f;

    protected int userControlIndex = -1;            // Index of user controlling the player (-1 = no user/AI)
    protected SsMatchInputManager.SsUserInput input = new SsMatchInputManager.SsUserInput();    // Input data for user/AI

    protected states state = states.idle;           // Current state
    protected float stateTime;                      // Time the state started

    protected float speed;                          // Movement speed. This is set at the end of the Update, so in most cases refers to the speed of the previous Update.
    protected float didMoveSpeed;                   // The actual speed the player moved. This is less than "speed", because it has delta time applied to it. This is the magnitude of didMoveVec, excluding Y.
    protected Vector3 moveVec;                      // Movement vector. Cleared at the start of each update. Contains deltaTime movement.
    protected Vector3 didMoveVec;                   // The actual distance the player moved.
    protected Vector3 prevMoveVec;                  // Previous update's movement vector
    protected float dynamicDiveSpeed;               // Dive speed (may slow down while diving)
    protected bool slowDownDive;                    // Must dive slow down?
    protected Vector3 lookVec;                      // Look vector (usually the direction rotating to)
    protected float velocityY;                      // Vertical velocity (e.g. used for jumping).
    protected Vector3 prevPos;                      // Position in the previous update, for calculating the move distance.
    protected float moveDistance;                   // Distance moved since starting a movement (e.g. running, sliding)
    protected float hangTime;                       // Hang time to delay gravity when reaching the crest of upward movement.
    protected float delayGravity;                   // Delay gravity.
    protected float trackMaxHeight;                 // Track max height during current air movement

    protected float lastTimeHadBall;                // The last time the player had the ball
    protected float haveBallTime;                   // Time when the player got the ball
    protected int haveBallFrame;                    // Frame count when the player got the ball
    protected float onGroundTime;                   // Count how long player is on the ground

    protected float validPassAngle;                 // Can only pass to players within this angle (relative to forward vector). (0=can Not pass)

    protected SsPlayer markPlayer;                  // The player to mark
                                                    // Is this player marked by other players? The array initially contains null elements, which are filled in as other players
                                                    // mark this player. Use GetMarkedByPlayer() to determine if the player is marked. (Sorted via distance)
    protected SsPlayer[] markedByPlayers = new SsPlayer[SsTeam.maxMarkedByPlayers];
    protected bool stopMarkingInNextUpdate;         // Stop marking a player in the next update.

    protected SsPlayer ballerSlideTarget;           // The opponent that is sliding towards this player
    protected SsPlayer slideTarget;                 // The opponent this player is sliding towards
    protected float slideSqrDistanceAtStart;        // Square distance at the start of the slide towards the target
    protected float dynamicChanceSideStep;          // Chance to sidestep. This is set whenever an opponent slides.
    protected float sideStepTime;                   // Timer to keep track of sidesteps
    protected bool slideDidStartAsAi;               // Did an AI start the sliding?
    protected bool slideTackleSuccess;              // Will slide tackle be a success? (Set when slide starts.)
    protected float slideTargetAngle;               // The angle sliding to the target, relative to the target's forward vector (e.g. tackle from front, side, back).
    protected float tackledAngle;                   // The angle this player was tackled at, relative to the forward vector (e.g. tackled from front, side, back). (Example: Front = less than 70, Side = 70 to 100, Back = greater than 100.)

    protected Vector3 targetDirection;              // Target direction to turn to (zero vector = none)

    protected SsGameObject targetObject;            // Target object to move to
    protected float targetObjectRadiusSquared;      // Min radius close to target object
    protected float targetObjectRadiusSquaredMax;   // Max radius close to target object
    protected float targetObjectUpdateDirectionTime;    // Timer to keep track of when the player must turn towards the target object

    protected bool gotoTargetPos;                   // Indicates is the player moves to targetPos
    protected Vector3 targetPos;                    // Target position to move to.
    protected float targetPosRadiusSqr;             // Target pos radius (squared). Reached pos when closer than this radius.
    protected float targetPosUpdateDirectionTime;   // Timer to keep track of when the player must turn towards the target position

    protected Vector3 initVecToTarget;              // Initial vector to target. Used to test if moved past target.


    // Delay to lie on the ground in pain, when the player falls down (e.g. tackled, hurt by bomb).
    // Default value is lieOnGroundDuration. (> 0 = lying on ground)
    protected float painDelay;

    protected float unTackleableTime;               // Time to be untackleable. Use SetUnTackleableTime() to change it.

    protected bool isBallPredator;                  // Is the player a ball predator? (Player tasked to get a loose ball)
    protected float ballPredatorTime;               // Time the player started to become a ball predator

    protected bool isBallerPredator;                // Is the player a baller predator?
    protected float ballerPredatorTime;             // Time the player started to become a baller predator
    protected float cannotTouchBallTimer;           // Timer to prevent player touching the ball for a while.
    protected float delaySlideChangeDirection;      // Timer to delay sliding change direction

    protected bool fallForward;                     // Is falling forward?
    protected bool fallForwardDone;                 // Is falling forward done?

    protected SsDelayShoot delayShoot = new SsDelayShoot();
    protected SsDelayPass delayPass = new SsDelayPass();

    protected bool saveDive;                        // Will dive save the ball?

    protected bool bicycleKickDidTouchBall;         // Did bicycle kick touch the ball to shoot it?

    protected dribbleStates dribbleState;
    protected bool isDribbling;
    protected float dribbleTime;
    protected float dribbleDuration;                // Duration to check if must change to idle
    protected float dribbleMinDurationToIdle;       // Min duration to check if must change to idle
    protected float dribbleAngle;                   // Direction facing angle, to detect changes in direction
    protected float ballDribbleDistanceHalf;        // Half-way between skills.ballDribbleDistanceMin and skills.ballDribbleDistanceMax
    protected int dribbleKickCount;                 // How many times the ball was kicked during the dribble


    // Markers
    protected GameObject markerControl;
    protected GameObject markerControlBall;
    protected GameObject markerControlBallDefender;
    protected GameObject markerControlBallMidfielder;
    protected GameObject markerControlBallForward;
    protected GameObject markerPass;
    protected GameObject markerAiBall;


#if UNITY_EDITOR
    protected string debugDefaultName;
#endif //UNITY_EDITOR


    // Properties
    //-----------
    /// <summary>
    /// Index of user currently controlling this player. <0 = AI controlled.
    /// </summary>
    /// <value>The index of the user control.</value>
    public int UserControlIndex
    {
        get { return (userControlIndex); }
        set
        {
            userControlIndex = value;
            SsMatchInputManager.OnPlayerControlIndexChanged(this, team.UserControlIndex);
        }
    }


    public SsMatchInputManager.SsUserInput Input
    {
        get { return (input); }
    }


    public bool IsUserControlled
    {
        get { return (userControlIndex >= 0); }
    }


    public bool IsAiControlled
    {
        get { return (userControlIndex < 0); }
    }


    public SsPlayerSkills Skills
    {
        get { return (skills); }
    }


    public SsPlayerAi Ai
    {
        get { return (ai); }
    }


    public positions Position
    {
        get { return (position); }
        set
        {
            position = value;

            // If team has no goalkeeper then set it now
            if ((value == positions.goalkeeper) && (team != null) && (team.GoalKeeper == null))
            {
                team.GoalKeeper = this;
            }

            SetAiComponent();

#if UNITY_EDITOR
            DebugSetGameObjectName();
#endif //UNITY_EDITOR
        }
    }


    public SsTeam Team
    {
        get { return (team); }
        set
        {
            team = value;

            if (team != null)
            {
                team.AddPlayer(this);
            }
        }
    }


    public SsTeam OtherTeam
    {
        get { return (otherTeam); }
        set
        {
            otherTeam = value;
        }
    }


    /// <summary>
    /// Index in the team list.
    /// </summary>
    /// <value>The index.</value>
    public int Index
    {
        get { return (index); }
        set
        {
            index = value;

#if UNITY_EDITOR
            DebugSetGameObjectName();
#endif //UNITY_EDITOR
        }
    }


    public int FormationIndex
    {
        get { return (formationIndex); }
        set
        {
            formationIndex = value;
        }
    }


    public states State
    {
        get { return (state); }
    }


    public SsPlayerAnimations Animations
    {
        get { return (animations); }
    }


    public SsPlayerParticles Particles
    {
        get { return (particles); }
    }


    public CharacterController Controller
    {
        get { return (controller); }
    }


    public SsPlayerStateMatrix StateMatrix
    {
        get { return (stateMatrix); }
    }


    /// <summary>
    /// Movement speed. This is set at the end of the Update, so in most cases refers to the speed of the previous Update.
    /// </summary>
    /// <value>The speed.</value>
    public float Speed
    {
        get { return (speed); }
    }



    /// <summary>
    /// The actual speed the player moved. This is less than "speed", because it has delta time applied to it. 
    /// This is the magnitude of didMoveVec, excluding Y.
    /// </summary>
    /// <value>The did move speed.</value>
    public float DidMoveSpeed
    {
        get { return (didMoveSpeed); }
    }


    /// <summary>
    /// Movement vector. Cleared at the start of each update. Contains deltaTime movement.
    /// </summary>
    /// <value>The move vec.</value>
    public Vector3 MoveVec
    {
        get { return (moveVec); }
    }


    /// <summary>
    /// The actual distance the player moved.
    /// </summary>
    /// <value>The did move vec.</value>
    public Vector3 DidMoveVec
    {
        get { return (didMoveVec); }
    }


    /// <summary>
    /// Is the sprint active?
    /// </summary>
    /// <value>The sprint active.</value>
    public bool SprintActive
    {
        get { return (input.sprintActive); }
    }


    /// <summary>
    /// Look vector (usually the direction rotating to). This should rarely be set outside the player's UpdateMovementAndRotation method.
    /// </summary>
    /// <value>The look vec.</value>
    public Vector3 LookVec
    {
        get { return (lookVec); }
        set
        {
            lookVec = value;
        }
    }


    public float VelocityY
    {
        get { return (velocityY); }
        set
        {
            velocityY = value;
        }
    }


    public float MoveDistance
    {
        get { return (moveDistance); }
    }


    public float HangTime
    {
        get { return (hangTime); }
        set
        {
            hangTime = value;
        }
    }


    public float DelayGravity
    {
        get { return (delayGravity); }
        set
        {
            delayGravity = value;
        }
    }


    public float BallBoxRadius
    {
        get { return (ballBoxRadius); }
    }


    public float BallBoxHeight
    {
        get { return (ballBoxHeight); }
    }


    public float BallDiveBoxRadius
    {
        get { return (ballDiveBoxRadius); }
    }


    public float BallDiveBoxHeight
    {
        get { return (ballDiveBoxHeight); }
    }


    public float PlayerBoxRadius
    {
        get { return (playerBoxRadius); }
    }


    public float PlayerBoxHeight
    {
        get { return (playerBoxHeight); }
    }


    public float BallLostDistanceSqr
    {
        get { return (ballLostDistanceSqr); }
    }


    public float BallDribbleDistanceMin
    {
        get { return (skills.ballDribbleDistanceMin); }
    }


    public float BallDribbleDistanceMax
    {
        get { return (skills.ballDribbleDistanceMax); }
    }


    public float PersonalSpaceRadius
    {
        get { return (personalSpaceRadius); }
    }


    public float HeadHeight
    {
        get { return (headHeight); }
    }


    /// <summary>
    /// Track max height during current air movement.
    /// </summary>
    /// <value>The height of the track max.</value>
    public float TrackMaxHeight
    {
        get { return (trackMaxHeight); }
    }


    public float LastTimeHadBall
    {
        get { return (lastTimeHadBall); }
        set
        {
            lastTimeHadBall = value;
        }
    }


    public float HaveBallTime
    {
        get { return (haveBallTime); }
        set
        {
            haveBallTime = value;
        }
    }


    public float OnGroundTime
    {
        get { return (onGroundTime); }
    }


    public float ValidPassAngle
    {
        get { return (validPassAngle); }
        set
        {
            validPassAngle = value;
        }
    }


    public SsPlayer MarkPlayer
    {
        get { return (markPlayer); }
    }


    /// <summary>
    /// Gets the players marking this player. In most cases, use GetMarkedByPlayer to get the nearest one.
    /// </summary>
    /// <value>The marked by players.</value>
    public SsPlayer[] MarkedByPlayers
    {
        get { return (markedByPlayers); }
    }


    public bool StopMarkingInNextUpdate
    {
        get { return (stopMarkingInNextUpdate); }
        set
        {
            stopMarkingInNextUpdate = value;
        }
    }


    public SsPlayer BallerSlideTarget
    {
        get { return (ballerSlideTarget); }
        set
        {
            ballerSlideTarget = value;
        }
    }


    public float SlideSqrDistanceAtStart
    {
        get { return (slideSqrDistanceAtStart); }
    }


    public float DynamicChanceSideStep
    {
        get { return (dynamicChanceSideStep); }
        set
        {
            dynamicChanceSideStep = value;
        }
    }


    public float SideStepTime
    {
        get { return (sideStepTime); }
        set
        {
            sideStepTime = value;
            SetUnTackleableTime(value + 1.0f, false);
        }
    }


    /// <summary>
    /// The angle this player was tackled from, relative to the forward vector (e.g. tackled from front, side, back).
    /// Example: Front = less than 70, Side = 70 to 100, Back = greater than 100.
    /// </summary>
    /// <value>The tackled angle.</value>
    public float TackledAngle
    {
        get { return (tackledAngle); }
        set
        {
            tackledAngle = value;
        }
    }


    public SsGameObject TargetObject
    {
        get { return (targetObject); }
    }


    public float TargetObjectUpdateDirectionTime
    {
        get { return (targetObjectUpdateDirectionTime); }
        set
        {
            targetObjectUpdateDirectionTime = value;
        }
    }


    public Vector3 TargetPos
    {
        get { return (targetPos); }
    }


    public bool GotoTargetPos
    {
        get { return (gotoTargetPos); }
    }


    public float PainDelay
    {
        get { return (painDelay); }
        set
        {
            painDelay = value;
        }
    }


    /// <summary>
    /// Get the unTackleable time. Use SetUnTackleableTime() to change it.
    /// </summary>
    /// <value>The un tackleable time.</value>
    public float UnTackleableTime
    {
        get { return (unTackleableTime); }
    }


    /// <summary>
    /// Is the player in a hurt state (e.g falling, lying in pain, etc.)?
    /// </summary>
    /// <value>The state of the in hurt.</value>
    public bool IsHurt
    {
        get
        {
            if ((painDelay > 0.0f) ||
                (state == states.falling) ||
                (state == states.inPain))
            {
                return (true);
            }
            return (false);
        }
    }


    /// <summary>
    /// Is busy kicking? Exclude bicycle kick.
    /// </summary>
    /// <value><c>true</c> if this instance is kicking; otherwise, <c>false</c>.</value>
    public bool IsKicking
    {
        get
        {
            if ((state == states.kickNear) ||
                (state == states.kickMedium) ||
                (state == states.kickFar))
            {
                return (true);
            }
            return (false);
        }
    }


    /// <summary>
    /// Is busy diving? (i.e. in one of the dive states: dive left, right, etc.)
    /// </summary>
    /// <value><c>true</c> if this instance is diving; otherwise, <c>false</c>.</value>
    public bool IsDiving
    {
        get
        {
            if ((state == states.gk_diveForward) ||
                (state == states.gk_diveUp) ||
                (state == states.gk_diveLeft) ||
                (state == states.gk_diveRight))
            {
                return (true);
            }
            return (false);
        }
    }


    /// <summary>
    /// Is the goalkeeper throwing the ball?
    /// </summary>
    /// <value>The gk is throwing in.</value>
    public bool GkIsThrowingIn
    {
        get
        {
            if ((state == states.gk_throwBallNear) ||
                (state == states.gk_throwBallMedium) ||
                (state == states.gk_throwBallFar))
            {
                return (true);
            }
            return (false);
        }
    }


    /// <summary>
    /// Is the player on the ground?
    /// </summary>
    /// <value><c>true</c> if this instance is on ground; otherwise, <c>false</c>.</value>
    public bool IsOnGround
    {
        get
        {
            if ((controller != null) && (controller.isGrounded))
            {
                return (true);
            }
            return (false);
        }
    }


    public bool IsBallPredator
    {
        get { return (isBallPredator); }
    }


    public float BallPredatorTime
    {
        get { return (ballPredatorTime); }
        set
        {
            ballPredatorTime = value;
        }
    }


    public bool IsBallerPredator
    {
        get { return (isBallerPredator); }
    }


    public float BallerPredatorTime
    {
        get { return (ballerPredatorTime); }
        set
        {
            ballerPredatorTime = value;
        }
    }


    public float CannotTouchBallTimer
    {
        get { return (cannotTouchBallTimer); }
        set
        {
            cannotTouchBallTimer = value;
        }
    }


    public SsDelayShoot DelayShoot
    {
        get { return (delayShoot); }
    }


    public SsDelayPass DelayPass
    {
        get { return (delayPass); }
    }


    /// <summary>
    /// Get the dribble state.
    /// </summary>
    /// <value>The state of the dribble.</value>
    public dribbleStates DribbleState
    {
        get { return (dribbleState); }
    }


    /// <summary>
    /// Is the ball in a position where it is being dribbled?
    /// </summary>
    /// <value><c>true</c> if this instance is dribbling; otherwise, <c>false</c>.</value>
    public bool IsDribbling
    {
        get { return (isDribbling); }
    }



    // Methods
    //--------

    /// <summary>
    /// Creates a player from the prefab.
    /// </summary>
    /// <returns>The player.</returns>
    /// <param name="prefab">Prefab.</param>
    static public SsPlayer CreatePlayer(SsPlayer prefab)
    {
        if (prefab == null)
        {
            return (null);
        }

        SsPlayer player = (SsPlayer)Instantiate(prefab);

        return (player);
    }


    /// <summary>
    /// Awake this instance.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public override void Awake()
    {
        base.Awake();

        animations = gameObject.GetComponent<SsPlayerAnimations>();
        if (animations == null)
        {
            animations = gameObject.AddComponent<SsPlayerAnimations>();
        }

        human = gameObject.GetComponentInChildren<SsPlayerHuman>();
        if (human == null)
        {
            human = gameObject.AddComponent<SsPlayerHuman>();
        }

        ai = gameObject.GetComponentInChildren<SsPlayerAi>();

        stateMatrix = gameObject.GetComponentInChildren<SsPlayerStateMatrix>();
        if (stateMatrix == null)
        {
            stateMatrix = gameObject.AddComponent<SsPlayerStateMatrix>();
        }

        sounds = gameObject.GetComponentInChildren<SsPlayerSounds>();
        if (sounds == null)
        {
            sounds = gameObject.AddComponent<SsPlayerSounds>();
        }

        particles = gameObject.GetComponentInChildren<SsPlayerParticles>();
        if (particles == null)
        {
            particles = gameObject.AddComponent<SsPlayerParticles>();
        }


        // Mesh
        if (meshChildOrPrefab != null)
        {
            // Is it a child?
            if (meshChildOrPrefab.transform.IsChildOf(transform))
            {
                mesh = meshChildOrPrefab;
            }
            else
            {
                mesh = (GameObject)Instantiate(meshChildOrPrefab);
                if (mesh != null)
                {
                    mesh.transform.parent = transform;
                    mesh.transform.localPosition = meshOffsetPosition;
                    mesh.transform.localRotation = Quaternion.Euler(meshOffsetRotation);
                }
            }
            if (mesh != null)
            {
                meshDefaultRot = mesh.transform.localRotation;
            }
        }


        // Colliders
        //----------
        if (ballCollider == null)
        {
            ballCollider = ballDiveCollider;
            if (ballCollider == null)
            {
                ballCollider = playerCollider;
                if (ballCollider == null)
                {
                    ballCollider = visibilityCollider;
                }
            }
        }
        if (ballCollider != null)
        {
            // Disable the collider
            ballCollider.enabled = false;

            // The radius is the largest of X or Z
            ballBoxRadius = Mathf.Max(Mathf.Abs(ballCollider.size.x * ballCollider.transform.lossyScale.x),
                                      Mathf.Abs(ballCollider.size.z * ballCollider.transform.lossyScale.z));

            ballBoxHeight = Mathf.Abs(ballCollider.size.y * ballCollider.transform.lossyScale.y);
        }


        if (ballDiveCollider == null)
        {
            ballDiveCollider = ballCollider;
        }
        if (ballDiveCollider != null)
        {
            // Disable the collider
            ballDiveCollider.enabled = false;

            // The radius is the largest of X or Z
            ballDiveBoxRadius = Mathf.Max(Mathf.Abs(ballDiveCollider.size.x * ballDiveCollider.transform.lossyScale.x),
                                      Mathf.Abs(ballDiveCollider.size.z * ballDiveCollider.transform.lossyScale.z));

            ballDiveBoxHeight = Mathf.Abs(ballDiveCollider.size.y * ballDiveCollider.transform.lossyScale.y);
        }


        if (playerCollider == null)
        {
            playerCollider = ballCollider;
        }
        if (playerCollider != null)
        {
            // Disable the collider
            playerCollider.enabled = false;

            // The radius is the largest of X or Z
            playerBoxRadius = Mathf.Max(Mathf.Abs(playerCollider.size.x * playerCollider.transform.lossyScale.x),
                                      Mathf.Abs(playerCollider.size.z * playerCollider.transform.lossyScale.z));

            playerBoxHeight = Mathf.Abs(playerCollider.size.y * playerCollider.transform.lossyScale.y);
        }

        if (visibilityCollider == null)
        {
            visibilityCollider = ballCollider;
        }
        if (visibilityCollider != null)
        {
            // Disable the collider
            visibilityCollider.enabled = false;
        }


        // Editor checks
        //--------------
#if UNITY_EDITOR
        if (meshChildOrPrefab == null)
        {
            Debug.LogError("ERROR: Player Mesh Child Or Prefab is null: " + GetAnyName());
        }

        if ((positionProperties == null) || (positionProperties.Length <= 0))
        {
            Debug.LogError("ERROR: Player does not have Position Properties: " + GetAnyName());
        }

        if (ballCollider == null)
        {
            Debug.LogError("ERROR: Player does not have a ball collider: " + GetAnyName());
        }

        if (visibilityCollider == null)
        {
            Debug.LogError("ERROR: Player does not have a Visibility Collider set: " + GetAnyName());
        }

        if (stateMatrix == null)
        {
            Debug.LogError("ERROR: Player does not have a Player State Matrix component: " + GetAnyName());
        }

        debugDefaultName = name;
#endif //UNITY_EDITOR

    }


    /// <summary>
    /// Start this instance.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public override void Start()
    {
        base.Start();

        ball = SsBall.Instance;

        controller = gameObject.GetComponentInChildren<CharacterController>();

        SpawnObjects();

        CalcVisibleRect(visibilityCollider);

        OnPositionSet();

        SetAiComponent();


        // Disable until intro/kickoff
        gameObject.SetActive(false);


        // Editor checks
        //--------------
#if UNITY_EDITOR
        if (controller == null)
        {
            Debug.LogError("ERROR: Player does not have a character controller: " + name);
        }
        if (ai == null)
        {
            Debug.LogError("ERROR: Player does not have an AI component: " + name);
        }

        if (skills == null)
        {
            Debug.LogError("ERROR: Player (or childen) does not have a Player Skills component: " + GetAnyName());
        }
        else if (skills.canPassWithinAngle <= 1.0f)
        {
            Debug.LogWarning("WARNING: Player has a small pass angle, Can Pass Within Angle (" +
                             skills.canPassWithinAngle.ToString("f2") + "), which will make it difficult to pass: " + GetAnyName());
        }
#endif //UNITY_EDITOR

    }


    /// <summary>
    /// Reset at start of match, or half time.
    /// REMINDER: This also clears the user control index. So make sure you set it after calling reset, if needed.
    /// NOTE: Derived method must call the base method.
    /// </summary>
    public override void ResetMe()
    {
        base.ResetMe();

        // Do Not clear user controll index during start of game/half time, because it may have just been set
        if ((match == null) ||
            ((match.State != SsMatch.states.loading) &&
              (match.State != SsMatch.states.intro) &&
              (match.State != SsMatch.states.kickOff)))
        {
            UserControlIndex = -1;
        }

        SelectSkills();

        animations.ResetMe();

        dynamicDiveSpeed = 0.0f;
        slowDownDive = false;
        hangTime = 0.0f;
        delayGravity = 0.0f;
        lastTimeHadBall = 0.0f;
        haveBallTime = 0.0f;
        haveBallFrame = 0;
        onGroundTime = 0.0f;
        painDelay = 0.0f;
        SetUnTackleableTime(0.0f, true);
        validPassAngle = skills.canPassWithinAngle;
        trackMaxHeight = (field != null) ? field.GroundY : transform.position.y;

        markPlayer = null;
        ClearAllMarkedByPlayers(false);
        stopMarkingInNextUpdate = false;

        ballerSlideTarget = null;
        slideTarget = null;
        slideSqrDistanceAtStart = 0.0f;
        dynamicChanceSideStep = 0.0f;
        sideStepTime = 0.0f;
        slideDidStartAsAi = false;
        slideTargetAngle = 0.0f;
        tackledAngle = 0.0f;

        ballerPredatorTime = 0.0f;
        cannotTouchBallTimer = 0.0f;
        delaySlideChangeDirection = 0.0f;
        fallForwardDone = false;
        fallForward = false;

        targetDirection = Vector3.zero;

        targetObject = null;
        targetObjectRadiusSquared = 0.0f;
        targetObjectRadiusSquaredMax = 0.0f;
        targetObjectUpdateDirectionTime = 0.0f;

        gotoTargetPos = false;
        targetPos = Vector3.zero;
        targetPosRadiusSqr = 0.0f;
        targetPosUpdateDirectionTime = 0.0f;

        ballLostDistanceSqr = skills.ballDribbleDistanceMax * 1.1f;
        ballLostDistanceAlwaysSqr = skills.ballDribbleDistanceMax * 1.5f;
        ballLostDistanceSqr *= ballLostDistanceSqr;
        ballLostDistanceAlwaysSqr *= ballLostDistanceAlwaysSqr;
        personalSpaceRadius = Mathf.Max(ballBoxRadius, playerBoxRadius) * 1.1f;
        ballDribbleDistanceHalf = skills.ballDribbleDistanceMin + ((skills.ballDribbleDistanceMax - skills.ballDribbleDistanceMin) / 2.0f);

        particles.StopAllParticles();

        SetBallPredator(false, false);
        SetBallerPredator(false, false);

        StopMoving(true, true, true, false);

        SetState(states.idle, true, SsPlayer.states.idle, null, true);
    }


    /// <summary>
    /// Raises the destroy event.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }


    /// <summary>
    /// Clean up. Includes freeing resources and clearing references.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The up.</returns>
    public override void CleanUp()
    {
        base.CleanUp();

        ball = null;

        skills = null;
        team = null;
        otherTeam = null;
        markPlayer = null;

        ballerSlideTarget = null;
        slideTarget = null;
        targetObject = null;

        markerControl = null;
        markerControlBall = null;
        markerControlBallDefender = null;
        markerControlBallMidfielder = null;
        markerControlBallForward = null;
        markerPass = null;
        markerAiBall = null;

        ClearAllMarkedByPlayers(false);
        markedByPlayers = null;
    }

    /// <summary>
    /// Set the player's position. Use this instead of setting transform.position directly.
    /// </summary>
    /// <remarks>
    /// This ensures the character controller uses the new position. A change in Unity 2018.3.0f2 causes the 
    /// CharacterController to cache the position. So we disable and enable the controller to force it to use the new 
    /// position.
    /// </remarks>
    public virtual void SetPosition(Vector3 newPosition)
    {
        if (controller != null)
        {
            controller.enabled = false;
        }
        transform.position = newPosition;
        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    /// <summary>
    /// Spawns the objects (e.g. markers, shadows).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The objects.</returns>
    protected virtual void SpawnObjects()
    {
        SsMatchPrefabs mps = (field != null) ? field.matchPrefabs : null;


        // Markers
        //--------
        if (mps != null)
        {
            if (team.IsUserControlled)
            {
                if (mps.markerControl != null)
                {
                    markerControl = Instantiate(mps.markerControl);
                    if (markerControl != null)
                    {
                        markerControl.transform.parent = transform;
                        markerControl.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerControl.SetActive(false);
                    }
                }

                if (mps.markerControlBall != null)
                {
                    markerControlBall = Instantiate(mps.markerControlBall);
                    if (markerControlBall != null)
                    {
                        markerControlBall.transform.parent = transform;
                        markerControlBall.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerControlBall.SetActive(false);
                    }
                }

                if (mps.markerControlBallDefender != null)
                {
                    markerControlBallDefender = Instantiate(mps.markerControlBallDefender);
                    if (markerControlBallDefender != null)
                    {
                        markerControlBallDefender.transform.parent = transform;
                        markerControlBallDefender.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerControlBallDefender.SetActive(false);
                    }
                }

                if (mps.markerControlBallMidfielder != null)
                {
                    markerControlBallMidfielder = Instantiate(mps.markerControlBallMidfielder);
                    if (markerControlBallMidfielder != null)
                    {
                        markerControlBallMidfielder.transform.parent = transform;
                        markerControlBallMidfielder.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerControlBallMidfielder.SetActive(false);
                    }
                }

                if (mps.markerControlBallForward != null)
                {
                    markerControlBallForward = Instantiate(mps.markerControlBallForward);
                    if (markerControlBallForward != null)
                    {
                        markerControlBallForward.transform.parent = transform;
                        markerControlBallForward.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerControlBallForward.SetActive(false);
                    }
                }

                if (mps.markerPass != null)
                {
                    markerPass = Instantiate(mps.markerPass);
                    if (markerPass != null)
                    {
                        markerPass.transform.parent = transform;
                        markerPass.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerPass.SetActive(false);
                    }
                }
            }
            else
            {
                if (mps.markerAiBall != null)
                {
                    markerAiBall = Instantiate(mps.markerAiBall);
                    if (markerAiBall != null)
                    {
                        markerAiBall.transform.parent = transform;
                        markerAiBall.transform.localPosition = new Vector3(0.0f, SsMarkersManager.offsetY, 0.0f);
                        markerAiBall.SetActive(false);
                    }
                }
            }
        }


        // Shadow
        //-------
        if ((fakeShadowPrefab != null) || (team.fakeShadowPrefab != null))
        {
            SsFakeShadow shadow = (SsFakeShadow)Instantiate((fakeShadowPrefab != null) ? fakeShadowPrefab : team.fakeShadowPrefab);
            if (shadow != null)
            {
                shadow.transform.parent = transform;
            }
        }
    }


    /// <summary>
    /// Selects the relevant skills from all the ones attached to the player (or the player's children), or from the team.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The skills.</returns>
    public virtual void SelectSkills()
    {
        SsPlayerSkills[] allSkills = gameObject.GetComponentsInChildren<SsPlayerSkills>(true);
        if ((allSkills == null) || (allSkills.Length <= 0))
        {
            // Check if the team has player skills
            allSkills = team.gameObject.GetComponentsInChildren<SsPlayerSkills>(true);
        }
        if ((allSkills == null) || (allSkills.Length <= 0))
        {
            return;
        }
        if (allSkills.Length == 1)
        {
            // Only 1 skills attached
            skills = allSkills[0];
            return;
        }

        // Find the relevant skills to use
        int i, highestDifficulty;
        SsPlayerSkills tempSkills;


        // Set the search scores based on the conditions
        highestDifficulty = -1;
        for (i = 0; i < allSkills.Length; i++)
        {
            tempSkills = allSkills[i];
            if (tempSkills == null)
            {
                continue;
            }

            tempSkills.SearchScore = 0;

            switch (tempSkills.whenToUseSkill)
            {
                case SsPlayerSkills.whenToUseSkills.asDefault:
                    {
                        // Default skills to use when no other skills are found.
                        tempSkills.SearchScore += 1;
                        break;
                    }
                case SsPlayerSkills.whenToUseSkills.humanTeamAndAnyMatchDifficulty:
                case SsPlayerSkills.whenToUseSkills.computerTeamAndAnyMatchDifficulty:
                    {
                        // Use when team is human controlled and for any match difficulty.
                        // OR
                        // Use when team is computer controlled and for any match difficulty.
                        if (((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.humanTeamAndAnyMatchDifficulty) && (team.IsUserControlled)) ||
                            ((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.computerTeamAndAnyMatchDifficulty) && (team.IsAiControlled)))
                        {
                            tempSkills.SearchScore += 2;
                        }
                        break;
                    }
                case SsPlayerSkills.whenToUseSkills.humanTeamForSpecificMatchDifficulty:
                case SsPlayerSkills.whenToUseSkills.computerTeamForSpecificMatchDifficulty:
                    {
                        // Use when team is human controlled and for a specific match difficulty (or higher)
                        // OR
                        // Use when team is computer controlled and for a specific match difficulty (or higher).
                        if (((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.humanTeamForSpecificMatchDifficulty) &&
                             (team.IsUserControlled) && (team.Difficulty >= tempSkills.useForDifficulty)) ||
                            ((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.computerTeamForSpecificMatchDifficulty) &&
                              (team.IsAiControlled) && (team.Difficulty >= tempSkills.useForDifficulty)))
                        {
                            if (team.Difficulty == tempSkills.useForDifficulty)
                            {
                                tempSkills.SearchScore += 1000;
                            }
                            else if (highestDifficulty < tempSkills.useForDifficulty)
                            {
                                tempSkills.SearchScore += 6 + tempSkills.useForDifficulty;
                                highestDifficulty = tempSkills.useForDifficulty;
                            }
                            else
                            {
                                tempSkills.SearchScore += 4;
                            }
                        }
                        break;
                    }
            } //switch
        }


        // Select the skills with the most score
        skills = null;
        for (i = 0; i < allSkills.Length; i++)
        {
            tempSkills = allSkills[i];
            if (tempSkills == null)
            {
                continue;
            }

            if ((skills == null) || (tempSkills.SearchScore > skills.SearchScore))
            {
                skills = tempSkills;
            }
        }

    }


    /// <summary>
    /// Called when the player's position has been set (e.g. start of match, corner kick, half time, etc.).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void OnPositionSet()
    {
        prevPos = transform.position;
        StopMoving(true, true, true, false);
        UpdateZoneAndVisibility(0.0f, true, team, field);
    }


    /// <summary>
    /// Called when the player gets the ball. Primiraliy called from SsMatch.SetBallPlayer().
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void OnGetBall(bool ballWasMoving, bool wasShotAtGoal, Vector3 ballOldVel)
    {
        LastTimeHadBall = Time.time;
        HaveBallTime = Time.time;
        haveBallFrame = Time.frameCount;
        dribbleState = dribbleStates.getBall;
        dribbleTime = 0.0f;
        dribbleDuration = 0.0f;
        dribbleMinDurationToIdle = 0.0f;
        dribbleKickCount = 0;
        isDribbling = false;


        ClearTargets(true, true, true); // Prevent baller trying to reach a target object.

        // Turn towards the direction from where the ball came
        if ((State != SsPlayer.states.slideTackle) &&
            (State != SsPlayer.states.gk_standHoldBall) &&
            (IsDiving == false) &&
            (State != SsPlayer.states.bicycleKick) &&
            (ballWasMoving))
        {
            RotateToDirection(-ballOldVel);
        }


        // If human team then make this the control player
        if (Team.IsUserControlled)
        {
            // Human team's player becomes the control player
            SsMatchInputManager.SetControlPlayer(this, Team.UserControlIndex);
        }


        //rafikbenamara switch player
        else if ((OtherTeam.IsUserControlled) &&
                 (OtherTeam.NearestHumanPlayerToBall != OtherTeam.NearestPlayerToBall) &&
                 (OtherTeam.NearestPlayerToBall != null) &&
                 (OtherTeam.NearestPlayerToBall.IsHurt == false))
        {
            // If other team is human then make the nearest player the control player
            /*SsMatchInputManager.SetControlPlayer(OtherTeam.NearestPlayerToBall, OtherTeam.UserControlIndex);*/
        }

        if (OtherTeam.IsUserControlled == false)
        {
            // Delay other, AI team's sliding
            OtherTeam.SetDelaySlidingTime(Time.time + OtherTeam.Skills.ai.delaySlidingWhenOpponentGetsBall, false);
            OtherTeam.OpponentPassesSinceLastSlide++;
        }

        if (Team.IsAiControlled)
        {
            // AI team
            Team.OpponentPassesSinceLastSlide = 0;

            if (Ai != null)
            {
                Ai.DelayShootingTime = Time.time + Random.Range(Skills.ai.shootDelayMin, Skills.ai.shootDelayMax);
                Ai.PassToForemost = (Random.Range(0.0f, 100.0f) < Skills.ai.chancePassToForemostPlayer);

                // Make sure player who gets the ball can always run forward immediately
                Ai.RunForwardTimer = 0.0f;
            }

            SideStepTime = 0.0f;
        }

        if (IsDiving)
        {
            // Diving, goalkeeper caught ball
            if (wasShotAtGoal)
            {
                ball.Sounds.PlaySfx(ball.Sounds.goalkeeperCatch);
            }

            AttachBallToPlayer(State);
        }
        else
        {
            // Make sure player starts falling down immediately
            HangTime = 0.0f;
            DelayGravity = 0.0f;
            if (VelocityY > 0.0f)
            {
                VelocityY = 0.0f;
            }
        }


        // Shoot if the player is doing a bicycle kick
        if ((state == states.bicycleKick) && (bicycleKickDidTouchBall == false))
        {
            bicycleKickDidTouchBall = true;
            Shoot(field.GetShootPos(this), true);
        }
    }


    /// <summary>
    /// Called when the player no longer has the ball. Primiraliy called from SsMatch.SetBallPlayer().
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void OnLostBall()
    {
        LastTimeHadBall = Time.time;
        HaveBallTime = 0.0f;
        haveBallFrame = 0;
        BallerSlideTarget = null;
        SideStepTime = 0.0f;
        dribbleState = dribbleStates.notDribbling;
        isDribbling = false;


        DelayPass.pending = false;
        DelayShoot.pending = false;

        // Delay getting the ball again
        CannotTouchBallTimer = Time.time + cannotTouchBallWhenReleaseTime;
    }


    /// <summary>
    /// Test if the player can play the position.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns><c>true</c> if this instance can play position the specified position; otherwise, <c>false</c>.</returns>
    /// <param name="position">Position.</param>
    public virtual bool CanPlayPosition(positions position)
    {
        if ((positionProperties == null) || (positionProperties.Length <= 0))
        {
            return (false);
        }

        int i;
        SsPositionProperties pp;

        for (i = 0; i < positionProperties.Length; i++)
        {
            pp = positionProperties[i];
            if ((pp != null) && (pp.position == position))
            {
                return (true);
            }
        }

        return (false);
    }


    /// <summary>
    /// Make sure player has the relevant AI components attached, and enable the correct one and disable the rest.
    /// If none attached then the default ones will be attached.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ai component.</returns>
    protected virtual void SetAiComponent()
    {
        if ((positionProperties == null) || (positionProperties.Length <= 0))
        {
            return;
        }

        SsPlayerAi found = null;
        int i;
        SsPositionProperties pp;
        SsPlayerAi[] ais = gameObject.GetComponents<SsPlayerAi>();
        SsPlayerAiForward[] fors = gameObject.GetComponents<SsPlayerAiForward>();
        SsPlayerAiMidfielder[] mids = gameObject.GetComponents<SsPlayerAiMidfielder>();
        SsPlayerAiDefender[] defs = gameObject.GetComponents<SsPlayerAiDefender>();
        SsPlayerAiGoalkeeper[] gols = gameObject.GetComponents<SsPlayerAiGoalkeeper>();

        for (i = 0; i < positionProperties.Length; i++)
        {
            pp = positionProperties[i];
            if ((pp != null) && (pp.position == position))
            {
                found = pp.ai;
                break;
            }
        }

        if (found == null)
        {
            // Find relevant component, if Not found then attach a new one
            if (position == positions.forward)
            {
                // Forward
                if ((fors != null) && (fors.Length > 0))
                {
                    found = (SsPlayerAi)fors[0];
                }
                if (found == null)
                {
                    found = (SsPlayerAi)gameObject.AddComponent<SsPlayerAiForward>();
                }
            }
            else if (position == positions.midfielder)
            {
                // Midfielder
                if ((mids != null) && (mids.Length > 0))
                {
                    found = (SsPlayerAi)mids[0];
                }
                if (found == null)
                {
                    found = (SsPlayerAi)gameObject.AddComponent<SsPlayerAiMidfielder>();
                }
            }
            else if (position == positions.defender)
            {
                // Defender
                if ((defs != null) && (defs.Length > 0))
                {
                    found = (SsPlayerAi)defs[0];
                }
                if (found == null)
                {
                    found = (SsPlayerAi)gameObject.AddComponent<SsPlayerAiDefender>();
                }
            }
            else if (position == positions.goalkeeper)
            {
                // Goalkeeper
                if ((gols != null) && (gols.Length > 0))
                {
                    found = (SsPlayerAi)gols[0];
                }
                if (found == null)
                {
                    found = (SsPlayerAi)gameObject.AddComponent<SsPlayerAiGoalkeeper>();
                }
            }
        }

        if (found != null)
        {
            ai = found;
            ai.enabled = true;
        }


        // Disable the other AI components
        if ((ais != null) && (ais.Length > 0))
        {
            for (i = 0; i < ais.Length; i++)
            {
                if ((ais[i] != null) && (ais[i] != found))
                {
                    ais[i].enabled = false;
                }
            }
        }

    }



    /// <summary>
    /// Get the the first non-empty name from first name, last name, etc. If none are found then return the game object's name.
    /// </summary>
    /// <returns>The any name.</returns>
    public virtual string GetAnyName()
    {
        if (string.IsNullOrEmpty(firstName) == false)
        {
            return (firstName);
        }
        if (string.IsNullOrEmpty(lastName) == false)
        {
            return (lastName);
        }
        if (string.IsNullOrEmpty(preferredName) == false)
        {
            return (preferredName);
        }
        if (string.IsNullOrEmpty(shortName) == false)
        {
            return (shortName);
        }
        if (string.IsNullOrEmpty(name3Letters) == false)
        {
            return (name3Letters);
        }

        return (name);
    }


    /// <summary>
    /// Sets the un-tackleable time. 
    /// IMPORTANT: "time" is usually in the format: Time.time + delay value
    /// </summary>
    /// <returns>The un0tackleble time.</returns>
    /// <param name="time">Time. Usually in the format: Time.time + delay value</param>
    /// <param name="allowLess">Allow setting the time if it is less than the current delay time.</param>
    public virtual void SetUnTackleableTime(float time, bool allowLess)
    {
        if ((allowLess) || (time > unTackleableTime))
        {
            unTackleableTime = time;
        }
    }


    /// <summary>
    /// Clears all marked by players.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The all marked by players.</returns>
    /// <param name="tellThemToStopMarkingMe">Tell them to stop marking me.</param>
    public virtual void ClearAllMarkedByPlayers(bool tellThemToStopMarkingMe)
    {
        if ((markedByPlayers == null) || (markedByPlayers.Length <= 0))
        {
            return;
        }

        int i;
        for (i = 0; i < markedByPlayers.Length; i++)
        {
            if ((tellThemToStopMarkingMe) && (markedByPlayers[i] != null))
            {
                markedByPlayers[i].StopMarkingInNextUpdate = true;
            }
            markedByPlayers[i] = null;
        }
    }


    /// <summary>
    /// Gets the nearest player that is marking this player, if any.
    /// </summary>
    /// <returns>The marked by player.</returns>
    public virtual SsPlayer GetMarkedByPlayer()
    {
        if ((markedByPlayers == null) || (markedByPlayers.Length <= 0))
        {
            return (null);
        }
        return (markedByPlayers[0]);
    }


    /// <summary>
    /// This player is being marked by another player. Add the other player to the list.
    /// </summary>
    /// <returns>The marked by player.</returns>
    /// <param name="otherPlayer">Player that is marking this player.</param>
    public virtual void AddMarkedByPlayer(SsPlayer otherPlayer)
    {
        if ((otherPlayer == null) || (markedByPlayers == null) || (markedByPlayers.Length <= 0))
        {
            return;
        }

        int i, emptyIndex, nearestIndex, furthestIndex, newIndex;
        float nearestDist, furthestDist, compareDist;
        Vector3 vec;
        SsPlayer comparePlayer;
        bool found = false;

        newIndex = -1;
        emptyIndex = -1;
        nearestIndex = -1;
        furthestIndex = -1;
        nearestDist = 0.0f;
        furthestDist = 0.0f;

        vec = transform.position - otherPlayer.transform.position;

        // Find empty slot, nearest player, and furthest player
        for (i = 0; i < markedByPlayers.Length; i++)
        {
            comparePlayer = markedByPlayers[i];
            if (comparePlayer == null)
            {
                if (emptyIndex == -1)
                {
                    emptyIndex = i;
                }
            }
            else
            {
                vec = transform.position - comparePlayer.transform.position;
                compareDist = vec.sqrMagnitude;

                if ((nearestIndex == -1) || (nearestDist > compareDist))
                {
                    nearestIndex = i;
                    nearestDist = compareDist;
                }

                if ((furthestIndex == -1) || (furthestDist > compareDist))
                {
                    furthestIndex = i;
                    furthestDist = compareDist;
                }
            }
        }

        if (emptyIndex == -1)
        {
            // No empty slot
            if (furthestIndex != -1)
            {
                // Replace furthest player (and tell that player he is no longer marking)
                markedByPlayers[furthestIndex].SetMarkPlayer(null, -1.0f, -1.0f);
                newIndex = furthestIndex;
                markedByPlayers[newIndex] = otherPlayer;
                found = true;
            }
        }
        else
        {
            // Insert into empty slot
            newIndex = emptyIndex;
            markedByPlayers[newIndex] = otherPlayer;
            found = true;
        }

        if (found)
        {
            UpdateMarkedByPlayers(0.0f);
        }
    }


    /// <summary>
    /// The other player stopped marking this player. Remove the other player from the list.
    /// </summary>
    /// <returns>The marked by player.</returns>
    /// <param name="otherPlayer">The player that stopped marking this player.</param>
    public virtual void RemoveMarkedByPlayer(SsPlayer otherPlayer)
    {
        if ((otherPlayer == null) || (markedByPlayers == null) || (markedByPlayers.Length <= 0))
        {
            return;
        }

        int i;
        SsPlayer comparePlayer;
        bool found = false;

        // Find the player in the slots and remove him
        for (i = 0; i < markedByPlayers.Length; i++)
        {
            comparePlayer = markedByPlayers[i];
            if (comparePlayer == otherPlayer)
            {
                markedByPlayers[i] = null;
                found = true;
                break;
            }
        }

        if (found)
        {
            UpdateMarkedByPlayers(0.0f);
        }
    }


    /// <summary>
    /// Rotates to direction. Immediate change in direction.
    /// </summary>
    /// <returns>The to direction.</returns>
    /// <param name="forward">Forward.</param>
    public virtual void RotateToDirection(Vector3 forward)
    {
        forward.y = 0.0f;
        if (forward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }


    /// <summary>
    ///  Rotates to face the game object.
    /// </summary>
    /// <returns>The to object.</returns>
    /// <param name="go">Go.</param>
    public virtual void RotateToObject(GameObject go)
    {
        RotateToDirection(go.transform.position - transform.position);
    }


    /// <summary>
    /// Test if the player is moving.
    /// </summary>
    /// <returns><c>true</c> if this instance is moving the specified horizontalOnly; otherwise, <c>false</c>.</returns>
    /// <param name="horizontalOnly">Horizontal only (i.e. ignore jumping/gravity).</param>
    public virtual bool IsMoving(bool horizontalOnly = true)
    {
        if (horizontalOnly == false)
        {
            return ((moveVec.x != 0.0f) || (moveVec.y != 0.0f) || (moveVec.z != 0.0f));
        }
        return ((moveVec.x != 0.0f) || (moveVec.z != 0.0f));
    }


    /// <summary>
    /// Test if the player was moving in the previous Update.
    /// </summary>
    /// <returns>The moving.</returns>
    /// <param name="horizontalOnly">Horizontal only (i.e. ignore jumping/gravity).</param>
    public virtual bool WasMoving(bool horizontalOnly = true)
    {
        if (horizontalOnly == false)
        {
            return ((prevMoveVec.x != 0.0f) || (prevMoveVec.y != 0.0f) || (prevMoveVec.z != 0.0f));
        }
        return ((prevMoveVec.x != 0.0f) || (prevMoveVec.z != 0.0f));
    }


    /// <summary>
    /// Stop moving.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The moving.</returns>
    /// <param name="clearTargetPos">Clear target position the player is moving to.</param>
    /// <param name="clearTargetObject">Clear target object the player is moving to.</param>
    /// <param name="clearMarkPlayer">Clear the player marked by this player.</param>
    /// <param name="horizontalOnly">Stop horizontal movement only (i.e. leave jumping/gravity).</param>
    public virtual void StopMoving(bool clearTargetPos = true, bool clearTargetObject = true, bool clearMarkPlayer = true,
                                   bool horizontalOnly = true)
    {
        bool wasMoving = IsMoving();

        if (horizontalOnly)
        {
            moveVec = new Vector3(0.0f, moveVec.y, 0.0f);
        }
        else
        {
            moveVec = Vector3.zero;
            velocityY = 0.0f;
        }
        didMoveVec = Vector3.zero;
        didMoveSpeed = 0.0f;

        speed = 0.0f;
        moveDistance = 0.0f;

        if ((clearTargetPos) || (clearTargetObject) || (clearMarkPlayer))
        {
            ClearTargets(clearTargetPos, clearTargetObject, clearMarkPlayer);
        }

        if (clearTargetPos)
        {
            // Clear sidestepping
            sideStepTime = 0.0f;
        }

      /*  SetFootDistance();*/

        if (ai != null)
        {
            ai.OnStopMoving(wasMoving);
        }
    }

    public float lerpSpeed = 0.5f; // Adjust the speed as needed

    public void SetFootDistance()
    {
        Transform leftFoot = gameObject.GetComponentInChildren<leftfoot>().transform;
        Transform rightFoot = gameObject.GetComponentInChildren<rightfoot>().transform;

        if (leftFoot != null && rightFoot != null)
        {
            // Define the target positions for the left and right feet
            Vector3 leftTargetPosition = new Vector3(leftFoot.position.x, -3, leftFoot.position.z);
            Vector3 rightTargetPosition = new Vector3(rightFoot.position.x, 3, rightFoot.position.z);

            // Smoothly move the left foot towards the target position using lerp
            leftFoot.position = Vector3.Lerp(leftFoot.position, leftTargetPosition, lerpSpeed * Time.deltaTime);

            // Smoothly move the right foot towards the target position using lerp
            rightFoot.position = Vector3.Lerp(rightFoot.position, rightTargetPosition, lerpSpeed * Time.deltaTime);

            Debug.Log("Feet position updated.");
        }
        else
        {
            Debug.LogError("Left foot or right foot transform not found.");
        }
    }

    /// <summary>
    /// Clears the targets.
    /// </summary>
    /// <returns>The targets.</returns>
    /// <param name="clearTargetPos">Clear target position.</param>
    /// <param name="clearTargetObject">Clear target object.</param>
    /// <param name="clearMarkPlayer">Clear mark player.</param>
    public virtual void ClearTargets(bool clearTargetPos, bool clearTargetObject, bool clearMarkPlayer)
    {
        targetDirection = Vector3.zero;

        if (clearTargetPos)
        {
            SetTargetPosition(false, Vector3.zero, 0.0f, 0.0f);
        }
        if (clearTargetObject)
        {
            SetTargetObject(null, 0.0f, 0.0f);
        }
        if (clearMarkPlayer)
        {
            SetMarkPlayer(null, -1.0f, -1.0f);
        }
    }


    /// <summary>
    /// Sets the target object to move to.
    /// </summary>
    /// <returns>The target object.</returns>
    /// <param name="newTarget">New target. Set to null to clear the current target.</param>
    /// <param name="radiusMin">Min radius close to target. A random radius will be used between min and max.</param>
    /// <param name="radiusMax">Max radius close to target. A random radius will be used between min and max.</param>
    public virtual void SetTargetObject(SsGameObject newTarget, float radiusMin, float radiusMax)
    {
        if (newTarget != null)
        {
            Vector3 vec = newTarget.transform.position - transform.position;
            vec.y = 0.0f;   // Ignore height

            initVecToTarget = vec.normalized;

            targetObject = newTarget;

            targetObjectRadiusSquared = Random.Range(radiusMin, radiusMin + ((radiusMax - radiusMin) * 0.5f));
            targetObjectRadiusSquaredMax = radiusMax;

            targetObjectRadiusSquared *= targetObjectRadiusSquared;
            targetObjectRadiusSquaredMax *= targetObjectRadiusSquaredMax;

            targetObjectUpdateDirectionTime = 0.0f;
        }
        else
        {
            // REMINDER: Do Not clear the marked player target. It gets cleared explicitly with SetMarkPlayer().

            if (IsBallPredator)
            {
                SetBallPredator(false, false);
            }
            if (IsBallerPredator)
            {
                SetBallerPredator(false, false);
            }

            targetObject = null;
        }
    }


    /// <summary>
    /// Sets the target position to move to.
    /// </summary>
    /// <returns>The target position.</returns>
    /// <param name="enabled">Enabled/disable target position.</param>
    /// <param name="newTargetPos">New target position.</param>
    /// <param name="radiusMin">Min radius close to target. A random radius will be used between min and max.</param>
    /// <param name="radiusMax">Max radius close to target. A random radius will be used between min and max.</param>
    public virtual void SetTargetPosition(bool enabled, Vector3 newTargetPos, float radiusMin, float radiusMax)
    {
        if (enabled)
        {
            Vector3 vec = newTargetPos - transform.position;
            vec.y = 0.0f;   // Ignore height

            initVecToTarget = vec.normalized;

            gotoTargetPos = true;
            targetPos = newTargetPos;

            targetPosRadiusSqr = Random.Range(radiusMin, radiusMax);
            targetPosRadiusSqr *= targetPosRadiusSqr;
        }
        else
        {
            gotoTargetPos = false;
        }
        targetPosUpdateDirectionTime = 0.0f;
    }


    /// <summary>
    /// Sets player to mark.
    /// </summary>
    /// <returns>The mark player.</returns>
    /// <param name="playerToMark">Player to mark.</param>
    /// <param name="distanceMin">Min mark distance. Set to -1 to use player default distance.</param>
    /// <param name="distanceMax">Max mark distance. Set to -1 to use player default distance.</param>
    public virtual void SetMarkPlayer(SsPlayer playerToMark, float distanceMin, float distanceMax)
    {
        stopMarkingInNextUpdate = false;

        // Clear current marked player's reference to this player
        if (markPlayer != null)
        {
            markPlayer.RemoveMarkedByPlayer(this);
        }
        markPlayer = null;

        // Clear the list of players marking me, and tell them to stop marking me
        ClearAllMarkedByPlayers(true);

        SetTargetObject(null, 0.0f, 0.0f);

        markPlayer = playerToMark;
        if (playerToMark != null)
        {
            // Tell the other player he is being marked
            playerToMark.AddMarkedByPlayer(this);

            if (distanceMin < 0.0f)
            {
                distanceMin = skills.ai.markDistanceMin;
            }
            if (distanceMax < 0.0f)
            {
                distanceMax = skills.ai.markDistanceMax;
            }

            // Add target's personal space
            distanceMin += playerToMark.PersonalSpaceRadius;
            distanceMax += playerToMark.PersonalSpaceRadius;

            SetTargetObject(playerToMark, distanceMin, distanceMax);
        }
    }



    /// <summary>
    /// Set whether this player chases the ball.
    /// </summary>
    /// <returns>The ball predator.</returns>
    /// <param name="isPredator">Is predator.</param>
    /// <param name="stopMoving">Stop moving. It will also clear the target object and marked player, but Not the target position.</param>
    /// <param name="updateTeamList">Update the team's list of ball predators. In most cases this is true, except when calling this method
    /// while clearing the list.</param>
    public virtual void SetBallPredator(bool isPredator, bool stopMoving, bool updateTeamList = true)
    {
        isBallPredator = isPredator;
        if (isBallPredator)
        {
            ballPredatorTime = Time.time;
            if (stopMoving)
            {
                StopMoving(false, true, true);
            }

            if ((team != null) && (updateTeamList))
            {
                team.AddBallPredator(this);
            }
        }
        else
        {
            ballPredatorTime = 0.0f;
            if (stopMoving)
            {
                StopMoving(false, true, true);
            }

            if ((team != null) && (updateTeamList))
            {
                team.RemoveBallPredator(this);
            }
        }
    }


    /// <summary>
    /// Set whether this player chases the player who has the ball.
    /// </summary>
    /// <returns>The baller predator.</returns>
    /// <param name="isPredator">Is predator.</param>
    /// <param name="stopMoving">Stop moving.</param>
    /// <param name="updateTeamList">Update team list.</param>
    public virtual void SetBallerPredator(bool isPredator, bool stopMoving, bool updateTeamList = true)
    {
        isBallerPredator = isPredator;
        if (isBallerPredator)
        {
            ballerPredatorTime = Time.time;
            if (stopMoving)
            {
                StopMoving(false, true, true);
            }

            if ((team != null) && (updateTeamList))
            {
                team.AddBallerPredator(this);
            }
        }
        else
        {
            ballerPredatorTime = 0.0f;
            if (stopMoving)
            {
                StopMoving(false, true, true);
            }

            if ((team != null) && (updateTeamList))
            {
                team.RemoveBallerPredator(this);
            }
        }
    }


    /// <summary>
    /// Get the distance to the game object. Ignores height.
    /// </summary>
    /// <returns>The to object.</returns>
    /// <param name="go">Go.</param>
    public virtual float DistanceToObject(GameObject go)
    {
        if (go == null)
        {
            return (float.MaxValue);
        }
        Vector3 vec = go.transform.position - transform.position;
        vec.y = 0.0f;   // Ignore height
        return (vec.magnitude);
    }


    /// <summary>
    /// Get the square distance to the game object. Ignores height.
    /// </summary>
    /// <returns>The to object squared.</returns>
    /// <param name="go">Go.</param>
    public virtual float DistanceToObjectSquared(GameObject go)
    {
        if (go == null)
        {
            return (float.MaxValue);
        }
        Vector3 vec = go.transform.position - transform.position;
        vec.y = 0.0f;   // Ignore height
        return (vec.sqrMagnitude);
    }


    /// <summary>
    /// Attach the ball to the player.
    /// </summary>
    /// <returns>The ball to player.</returns>
    /// <param name="forState">For this state. It will check if the state has attach info. If Not then it will Not attach the ball.</param>
    public virtual void AttachBallToPlayer(states forState)
    {
        SsPlayerAnimations.SsAnimationProperties props = animations.GetAnimationProperties(forState);
        if ((props != null) && (props.attachBallToGo != null))
        {
            ball.AttachToObject(props.attachBallToGo, props.attachBallOffset, props.attachBallRadiusDir);
        }
    }


    /// <summary>
    /// Sets the state and plays the relevant animation.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The state.</returns>
    /// <param name="newState">New state.</param>
    /// <param name="playAnimation">Indicates if animation should be played.</param>
    /// <param name="alternateAnimation">If newState's animation could Not be played then play this alternate state's animation. Default is idle.</param>
    /// <param name="vec">Vector used for certain state (e.g. move to, look at, etc.). Null if not needed.</param>
    /// <param name="snapAnimation">Snap the animation (i.e. no blending).</param>
    public virtual void SetState(states newState, bool playAnimation = true,
                                 states alternateAnimation = states.idle,
                                 System.Nullable<Vector3> vec = null,
                                 bool snapAnimation = false)
    {
        if (state == newState)
        {
            return;
        }

        float chance;
        bool shouldDetachBall;

        // Process the OLD state
        PreSetState(newState, playAnimation, alternateAnimation, vec, out shouldDetachBall);


        state = newState;
        stateTime = Time.time;

        delayPass.pending = false;
        delayShoot.pending = false;


        // Process the NEW state
        switch (state)
        {
            case states.idle:
                {
                    // Idle
                    //-----

                    StopMoving(true, true, false);
                    break;
                }
            case states.kickNear:
            case states.kickMedium:
            case states.kickFar:
                {
                    // Kick
                    //-----

                    StopMoving(true, true, true);
                    break;
                }
            case states.slideTackle:
                {
                    // Slide tackle
                    //-------------

                    moveDistance = 0.0f;

                    StopMoving(true, false, false);

                    if (vec != null)
                    {
                        RotateToDirection(vec.Value);
                    }

                    delaySlideChangeDirection = Time.time + maxDelaySlideChangeDirection;

                    if (IsAiControlled)
                    {
                        slideTackleSuccess = (Random.Range(0, 100) < Skills.ai.slideTackleSuccessChance);
                    }
                    else
                    {
                        // Human slide tackle is always a success
                        slideTackleSuccess = true;
                    }

                    particles.Play(particles.slideTackle);
                    sounds.PlaySfx(sounds.slideTackle);

                    break;
                }
            case states.falling:
                {
                    // Falling
                    //--------

                    if (this == match.BallPlayer)
                    {
                        ball.DetachFromObject(false);
                    }

                    if (IsOnGround)
                    {
                        // Add small upward force
                        Jump(tripUpForce, 0.0f);
                    }

                    painDelay = skills.lieOnGroundDuration;
                    fallForwardDone = false;
                    fallForward = IsMoving();

                    ClearTargets(true, true, true);
                    moveDistance = 0.0f;

                    sounds.PlaySfx(sounds.fall);

                    break;
                }
            case states.inPain:
                {
                    // In pain
                    //--------

                    if (this == match.BallPlayer)
                    {
                        ball.DetachFromObject(false);
                    }

                    break;
                }
            case states.gk_standHoldBall:
                {
                    // GK stand hold ball
                    //-------------------

                    StopMoving(true, true, true);

                    if (ai != null)
                    {
                        ai.DelayThrowBallTime = Time.time + Random.Range(SsPlayerAi.delayThrowBallTimeMin,
                                                                         SsPlayerAi.delayThrowBallTimeMax);
                        ai.DelayWaitForOtherTeamToRunBackTime = Time.time + Random.Range(SsPlayerAi.delayWaitForOtherTeamToRunBackTimeMin,
                                                                                         SsPlayerAi.delayWaitForOtherTeamToRunBackTimeMax);
                    }

                    // Turn towards centre of field
                    if (team.PlayDirection > 0)
                    {
                        RotateToDirection(Vector3.right);
                    }
                    else
                    {
                        RotateToDirection(-Vector3.right);
                    }

                    team.MakeTheHoldPlayerTheControlPlayer(this);

                    shouldDetachBall = false;
                    AttachBallToPlayer(state);

                    break;
                }
            case states.gk_diveForward:
            case states.gk_diveUp:
            case states.gk_diveRight:
            case states.gk_diveLeft:
                {
                    // GK dive
                    //--------

                    // Allowed to touch the ball immediately
                    cannotTouchBallTimer = 0.0f;

                    StopMoving(true, true, true);

                    if (ai != null)
                    {
                        ai.DelayDiveTime = Time.time + skills.nextDiveDelay;
                    }

                    if (vec != null)
                    {
                        RotateToDirection(vec.Value);
                    }


                    // Dive save chance
                    chance = (IsUserControlled) ? skills.humanChanceDiveSave : skills.ai.chanceDiveSave;

                    // If player shot from outside the penalty area then the chance to catch the ball increases.
                    // The change increase from the % value specified in the skills, to 100%.
                    // In the penalty area it is the skills % value, and at half way it is 100%.
                    if ((chance < 100.0f) && (field.IsInPenaltyArea(ball.KickStartPosition, team) == false))
                    {
                        Bounds penaltyArea = field.GetPenaltyArea(team);
                        float distance, maxDistance, penaltyX;
                        if (team.PlayDirection > 0)
                        {
                            // Left team/penalty area (playing to right)
                            penaltyX = penaltyArea.max.x;
                            distance = Mathf.Max(ball.KickStartPosition.x - penaltyX, 0.0f);
                            maxDistance = Mathf.Abs(field.CentreMark.x - penaltyX);
                        }
                        else
                        {
                            // Right team/penalty area (playing to left)
                            penaltyX = penaltyArea.min.x;
                            distance = Mathf.Max(penaltyX - ball.KickStartPosition.x, 0.0f);
                            maxDistance = Mathf.Abs(penaltyX - field.CentreMark.x);
                        }

                        chance = GeUtils.Lerp(distance, 0.0f, maxDistance, chance, 100.0f);
                    }

                    saveDive = (Random.Range(0.0f, 100.0f) < chance);

                    break;
                }
            case states.throwInHold:
                {
                    // Throw in hold
                    //--------------

                    ClearTargets(true, true, true);

                    if (transform.position.z < field.CentreMark.z)
                    {
                        // Bottom of field, look up
                        RotateToDirection(Vector3.forward);
                    }
                    else
                    {
                        // Top of field, look down
                        RotateToDirection(-Vector3.forward);
                    }

                    shouldDetachBall = false;
                    AttachBallToPlayer(state);

                    break;
                }
            case states.bicycleKick:
                {
                    // Bicycle kick
                    //-------------
                    bicycleKickDidTouchBall = false;
                    StopMoving(true, true, true);
                    break;
                }
            case states.header:
                {
                    // Header
                    //-------
                    // Add force needed to reach the ball
                    chance = Mathf.Max(headerUpForce,
                                       GeUtils.CalcVerticalVelocity(ball.transform.position.y - (transform.position.y + headHeight - ball.radius)));
                    Jump(chance, 0.0f);
                    ball.Sounds.PlaySfx(ball.Sounds.header);
                    break;
                }
        } //switch



        if (shouldDetachBall)
        {
            ball.DetachFromObject(false);
        }


        if (playAnimation)
        {
            if (snapAnimation == false)
            {
                if (animations.PlayAnimation(animations.GetAnimationName(state), animations.GetAnimationHashCode(state)) == false)
                {
                    animations.PlayAnimation(animations.GetAnimationName(alternateAnimation), animations.GetAnimationHashCode(alternateAnimation));
                }
            }
            else
            {
                if (animations.PlayAnimation(animations.GetAnimationName(state), animations.GetAnimationHashCode(state), 0.0f) == false)
                {
                    animations.PlayAnimation(animations.GetAnimationName(alternateAnimation), animations.GetAnimationHashCode(alternateAnimation), 0.0f);
                }
            }
        }
    }


    /// <summary>
    /// IMPORTANT: This is only called from within SetState.
    /// It processes/cleans up the old state before setting the new state.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The set state.</returns>
    /// <param name="newState">New state.</param>
    /// <param name="playAnimation">Play animation.</param>
    /// <param name="alternateAnimation">Alternate animation.</param>
    /// <param name="vec">Vec.</param>
    /// <param name="shouldDetachBall">Get: should the ball be detached from the player?</param>
    protected virtual void PreSetState(states newState,
                                       bool playAnimation,
                                       states alternateAnimation,
                                       System.Nullable<Vector3> vec,
                                       out bool shouldDetachBall)
    {
        shouldDetachBall = false;

        // Process the OLD state
        switch (state)
        {
            case states.slideTackle:
                {
                    // Slide tackle
                    //-------------

                    if (ai != null)
                    {
                        ai.DelaySlidingTime = Time.time + skills.ai.slideIntervals;
                    }

                    ballerSlideTarget = null;

                    if ((slideTarget != null) && (slideTarget.BallerSlideTarget == this))
                    {
                        slideTarget.BallerSlideTarget = null;
                    }
                    slideTarget = null;
                    slideDidStartAsAi = false;

                    particles.Stop(particles.slideTackle);

                    break;
                }
            case states.falling:
                {
                    // Falling
                    //--------

                    if (ai != null)
                    {
                        ai.DelaySlidingTime = Time.time + skills.ai.slideIntervals;
                    }

                    ballerSlideTarget = null;

                    if ((slideTarget != null) && (slideTarget.BallerSlideTarget == this))
                    {
                        slideTarget.BallerSlideTarget = null;
                    }
                    slideTarget = null;

                    painDelay = 0.0f;
                    break;
                }
            case states.gk_diveForward:
            case states.gk_diveUp:
            case states.gk_diveRight:
            case states.gk_diveLeft:
                {
                    // GK dive
                    //--------

                    if (ai != null)
                    {
                        ai.DelayDiveTime = Time.time + skills.nextDiveDelay;
                        ai.DivingAtBaller = false;
                    }

                    if (mesh != null)
                    {
                        mesh.transform.localRotation = meshDefaultRot;
                    }

                    if (this == match.BallPlayer)
                    {
                        shouldDetachBall = true;
                    }

                    break;
                }
        } //switch

    }


    /// <summary>
    /// Shoot the ball.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <param name="targetPos">Target position.</param>
    /// <param name="shotAtGoal">Shot at goal.</param>
    public virtual void Shoot(Vector3 targetPos, bool shotAtGoal)
    {
        float shootDelayDuration, distance;
        Vector3 vec;

        targetPos = skills.AdjustShootPosition(ball.transform.position, targetPos);

        shootDelayDuration = 0.0f;

        vec = targetPos - ball.transform.position;
        distance = vec.magnitude;

        delayShoot.clip = null;

        ball.StopFakeForce();

        if (State == SsPlayer.states.bicycleKick)
        {
            // No animation change
        }
        else if (ball.transform.position.y >= transform.position.y + headHeight - ball.radius)
        {
            // Header
            SetState(states.header);
        }
        else
        {
            // Kick
            Kick(distance, ref delayShoot.clip);
            shootDelayDuration = animations.GetReleaseBallTime(state);
        }

        if (State != SsPlayer.states.bicycleKick)
        {
            // Turn in the direction of the kick
            RotateToDirection(vec);
        }


        BallerSlideTarget = null;
        SideStepTime = 0.0f;


        if (shootDelayDuration <= 0.0f)
        {
            // Move ball immediately
            delayShoot.pending = false;
            ball.Shoot(this, targetPos, shotAtGoal, delayShoot.clip);
        }
        else
        {
            // Delay ball movement
            delayShoot.pending = true;
            delayShoot.delayTime = Time.time + shootDelayDuration;
            delayShoot.targetPos = targetPos;
            delayShoot.shotAtGoal = shotAtGoal;
        }
    }


    /// <summary>
    /// Pass the ball.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <param name="toPlayer">To player.</param>
    /// <param name="targetPos">Target position.</param>
    /// <param name="clearBallTeam">Clear ball team.</param>
    public virtual void Pass(SsPlayer toPlayer, Vector3 targetPos, bool clearBallTeam = false)
    {
        float passDelayDuration, distance;
        bool kick;
        Vector3 vec;

        passDelayDuration = 0.0f;
        kick = true;

        ball.StopMoving();

        if (toPlayer != null)
        {
            toPlayer.Team.SetPassPlayer(toPlayer, true);
        }

        if (toPlayer != null)
        {
            targetPos = Skills.AdjustPassPosition(ball.transform.position, toPlayer.transform.position);
        }
        else
        {
            targetPos = Skills.AdjustPassPosition(ball.transform.position, targetPos);
        }

        vec = targetPos - ball.transform.position;
        distance = vec.magnitude;

        delayPass.clip = null;

        if (State == SsPlayer.states.gk_standHoldBall)
        {
            // Goalkeeper throw in
            //--------------------
            if ((distance > animations.gkFarThrowDistance) &&
                (animations.HasAnimation(SsPlayer.states.gk_throwBallFar)))
            {
                // Throw far
                SetState(SsPlayer.states.gk_throwBallFar);
                passDelayDuration = animations.GetReleaseBallTime(state);
                kick = false;

                if (ball.Sounds.goalkeeperThrowFar != null)
                {
                    delayPass.clip = ball.Sounds.goalkeeperThrowFar;
                }
                else
                {
                    delayPass.clip = ball.Sounds.goalkeeperThrowMedium;
                }
            }
            else if ((distance <= animations.gkMediumThrowDistance) &&
                     (animations.HasAnimation(SsPlayer.states.gk_throwBallNear)))
            {
                // Throw near
                SetState(SsPlayer.states.gk_throwBallNear);
                passDelayDuration = animations.GetReleaseBallTime(state);
                kick = false;

                if (ball.Sounds.goalkeeperThrowNear != null)
                {
                    delayPass.clip = ball.Sounds.goalkeeperThrowNear;
                }
                else
                {
                    delayPass.clip = ball.Sounds.goalkeeperThrowMedium;
                }
            }
            else
            {
                // Throw medium (default)
                SetState(SsPlayer.states.gk_throwBallMedium);
                passDelayDuration = animations.GetReleaseBallTime(state);
                kick = false;

                delayPass.clip = ball.Sounds.goalkeeperThrowMedium;
            }
        }
        else if (State == SsPlayer.states.throwInHold)
        {
            // Player throw in
            //----------------
            if (animations.HasAnimation(SsPlayer.states.throwIn))
            {
                SetState(SsPlayer.states.throwIn);
                passDelayDuration = animations.GetReleaseBallTime(state);
                kick = false;

                delayPass.clip = ball.Sounds.throwIn;
            }
        }
        else if (ball.transform.position.y >= transform.position.y + headHeight - ball.radius)
        {
            // Header
            //-------
            SetState(states.header);
            kick = false;
        }

        if (kick)
        {
            // Kick
            //-----
            Kick(distance, ref delayPass.clip);
            passDelayDuration = animations.GetReleaseBallTime(state);
        }


        if (State != SsPlayer.states.bicycleKick)
        {
            // Turn in the direction of the kick/throw
            RotateToDirection(vec);
        }

        BallerSlideTarget = null;
        SideStepTime = 0.0f;


        if (passDelayDuration <= 0.0f)
        {
            // Move ball immediately
            delayPass.pending = false;
            ball.Pass(this, toPlayer, targetPos, clearBallTeam, delayPass.clip);
        }
        else
        {
            // Delay ball movement
            delayPass.pending = true;
            delayPass.delayTime = Time.time + passDelayDuration;
            delayPass.toPlayer = toPlayer;
            delayPass.targetPos = targetPos;
            delayPass.clearBallTeam = clearBallTeam;
        }
    }


    /// <summary>
    /// Change to the kick state based on the distance (e.g. near, medium, far kick).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <param name="distance">Distance.</param>
    public virtual void Kick(float distance, ref AudioClip clip)
    {
        // Use distance to determine whether to use soft or hard kick, then play the animation after setting the state.
        if ((distance > animations.farKickDistance) && (animations.HasAnimation(states.kickFar)))
        {
            // Far kick
            SetState(SsPlayer.states.kickFar);

            if (ball.Sounds.kickFar != null)
            {
                clip = ball.Sounds.kickFar;
            }
            else
            {
                clip = ball.Sounds.kickMedium;
            }
        }
        else if ((distance <= animations.mediumKickDistance) && (animations.HasAnimation(states.kickNear)))
        {
            // Near kick
            SetState(SsPlayer.states.kickNear);

            if (ball.Sounds.kickNear != null)
            {
                clip = ball.Sounds.kickNear;
            }
            else
            {
                clip = ball.Sounds.kickMedium;
            }
        }
        else
        {
            // Medium kick (default)
            SetState(SsPlayer.states.kickMedium);

            clip = ball.Sounds.kickMedium;
        }
    }


    /// <summary>
    /// Test if the goalkeeper punches the ball, when he touches the ball.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball.</returns>
    public virtual bool PunchBall()
    {
        if ((Position != positions.goalkeeper) || (IsDiving == false))
        {
            return (false);
        }

        bool punchBackwards = false;
        float chance = Random.Range(0.0f, 100.0f);

        if ((chance < skills.chanceGoalkeeperPunchBack) &&
            (ball.transform.position.y > field.GroundY + punchBallBackValidHeight) &&
            (field.IsInPenaltyArea(gameObject, team)))
        {
            punchBackwards = true;
        }
        else if (chance < skills.chanceGoalkeeperPunchForward)
        {
            punchBackwards = false;
        }
        else
        {
            return (false);
        }

        Vector3 force;

        match.SetBallPlayer(null);
        match.SetBallTeam(null);    // Set ball team to null so that AI start chasing the loose ball

        if (punchBackwards)
        {
            // Punch backwards (towards net)
            if (team.PlayDirection > 0.0f)
            {
                force = GeUtils.GetVectorFromAngle(Random.Range(225.0f, 315.0f));
            }
            else
            {
                force = GeUtils.GetVectorFromAngle(Random.Range(45.0f, 135.0f));
            }
        }
        else
        {
            // Punch forward (away from net)
            if (team.PlayDirection > 0.0f)
            {
                force = GeUtils.GetVectorFromAngle(Random.Range(45.0f, 135.0f));
            }
            else
            {
                force = GeUtils.GetVectorFromAngle(Random.Range(225.0f, 315.0f));
            }
        }

        force *= Random.Range(skills.maxPassSpeed * 0.25f, skills.maxPassSpeed * 0.7f);

        if (punchBackwards)
        {
            force = new Vector3(force.x,
                                GeUtils.CalcVerticalVelocity(Random.Range(punchBallHeightMin, punchBallBackwardHeightMax)),
                                force.y);
        }
        else
        {
            force = new Vector3(force.x,
                                GeUtils.CalcVerticalVelocity(Random.Range(punchBallHeightMin, punchBallForwardHeightMax)),
                                force.y);
        }

        ball.DetachFromObject(true);
        ball.AddForce(force);

        slowDownDive = true;

        ball.Sounds.PlaySfx(ball.Sounds.punch);

        return (false);
    }


    /// <summary>
    /// Jump up by applying an upward force.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <param name="jumpForce">Jump force.</param>
    /// <param name="hangTime">Delay gravity when reaching the crest of the jump arc.</param>
    public virtual void Jump(float jumpForce, float hangTime)
    {
        velocityY = jumpForce;
        this.hangTime = hangTime;

        // Slighy delay to gravity, to allow leaving the ground
        delayGravity = 0.01f;

        onGroundTime = 0.0f;

        trackMaxHeight = transform.position.y;
    }


    /// <summary>
    /// Start diving. It will change the player's state and animation. If toBall and toBaller are false then the player
    /// will dive forward.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <param name="toBall">Should automatically dive towards the ball?</param>
    /// <param name="toBaller">Should automatically dive towards the player who has ball?</param>
    public virtual bool Dive(bool toBall, bool toBaller)
    {
        Vector3 point, vec, needVel, failPoint, intersectPoint;
        float jumpForce, time, relativeHeight, angle;
        states newState = states.gk_diveForward;
        bool foundState = false;

        jumpForce = 0.0f;
        relativeHeight = 0.0f;
        dynamicDiveSpeed = Skills.diveSpeed;
        slowDownDive = false;

        if (toBall)
        {
            // To ball
            if (ball.InterceptBall(this, dynamicDiveSpeed, out point, out time, out needVel, out jumpForce,
                                   out failPoint, out relativeHeight))
            {
                intersectPoint = point;
            }
            else
            {
                intersectPoint = failPoint;
            }
        }
        else if (toBaller)
        {
            // To baller
            if (match.BallPlayer == null)
            {
                return (false);
            }

            intersectPoint = match.BallPlayer.transform.position;
        }
        else
        {
            // Straight ahead
            intersectPoint = transform.position + (transform.forward * Skills.maxDiveDistance);
        }


        // Direction to dive
        vec = intersectPoint - transform.position;

        if (IsUserControlled == false)
        {
            // Is distance too far? (Slightly further than max distance.)
            vec.y = 0.0f;   // Ignore height
            if (vec.magnitude > Skills.maxDiveDistance * 1.1f)
            {
                return (false);
            }
        }


        // Only add upwards force if target is higher than player's head
        if (relativeHeight > HeadHeight)
        {
            if (jumpForce > Skills.maxDiveUpSpeed)
            {
                jumpForce = Skills.maxDiveUpSpeed;
            }
        }
        else
        {
            jumpForce = 0.0f;
        }


        // Determine if should dive left, right, forward or up
        //----------------------------------------------------

        if (toBall)
        {
            // To ball
            // Preferred facing direction
            if (ball.IsMoving(true))
            {
                // Opposite to ball velocity
                needVel = new Vector3(ball.Rb.velocity.x, 0.0f, ball.Rb.velocity.z);
                needVel = -needVel;
            }
            else
            {
                // Face towards ball. Not ideal, but ok if ball is Not moving.
                needVel = vec;
            }
        }
        else
        {
            // To baller/straight ahead: face straight ahead
            needVel = vec;
        }


        if ((animations.HasAnimation(states.gk_diveForward)) ||
            (animations.HasAnimation(states.gk_diveUp)))
        {
            angle = Vector3.Angle(needVel, vec);
            if (angle < 5.0f)
            {
                if ((jumpForce > 0.0f) && (animations.HasAnimation(states.gk_diveUp)))
                {
                    newState = states.gk_diveUp;
                    foundState = true;
                }
                else if (animations.HasAnimation(states.gk_diveForward))
                {
                    newState = states.gk_diveForward;
                    foundState = true;
                }
            }
        }

        if (foundState == false)
        {
            angle = GeUtils.GetAngleBetween(needVel, vec);
            if (angle <= 0)
            {
                // Left
                newState = states.gk_diveLeft;
                foundState = true;

                // Angle mesh must face, relative to the dive direction
                angle = 90.0f;
            }
            else
            {
                // Right
                newState = states.gk_diveRight;
                foundState = true;

                // Angle mesh must face, relative to the dive direction
                angle = -90.0f;
            }

            // Rotate the mesh
            if ((mesh != null) && (angle != 0.0f))
            {
                mesh.transform.Rotate(transform.up, angle, Space.World);
            }
        }

        SetState(newState, true, states.idle, vec);


        if (jumpForce > 0.0f)
        {
            Jump(jumpForce, Skills.diveHangTime);
        }

        return (true);
    }


    /// <summary>
    /// Test if the player is in a state that allows movement from user/AI input.
    /// </summary>
    /// <returns><c>true</c> if this instance can move; otherwise, <c>false</c>.</returns>
    public virtual bool CanMoveFromInput()
    {
        if ((IsKicking == false) &&
            (state != states.header) &&
            (state != states.bicycleKick) &&
            (state != states.throwIn) &&
            (GkIsThrowingIn == false) &&
            (delayPass.pending == false) &&
            (delayShoot.pending == false))
        {
            return (true);
        }
        return (false);
    }


    /// <summary>
    /// Test if player is in a state that allows them to shoot or pass the ball. Also tests if the player has the ball.
    /// </summary>
    /// <returns><c>true</c> if this instance can shoot or pass; otherwise, <c>false</c>.</returns>
    public virtual bool CanShootOrPass()
    {
        if ((match != null) && (this == match.BallPlayer) && (match.CanShootOrPass()) &&
            (state != states.slideTackle) &&
            (IsDiving == false) &&
            (delayPass.pending == false) &&
            (delayShoot.pending == false))
        {
            return (true);
        }
        return (false);
    }



    /// <summary>
    /// Test if the player can shoot to goal. Also set the shoot position and reduce the player's pass angle to prevent him 
    /// passing to players to the sides of the goalpost.
    /// </summary>
    /// <returns><c>true</c> if this instance can shoot to goal the specified getShootPos forceShoot ignoreAngle shootIfAngleSmall
    /// useLookVectorAngle wantToBicycleKick; otherwise, <c>false</c>.</returns>
    /// <param name="getShootPos">Get the position to shoot at.</param>
    /// <param name="forceShootPastZone5">Always shoot if player is facing towards the goal area and he is past zone 5.</param>
    /// <param name="ignoreAnglePastZone55">Ignore facing angle when player past zone 5.5.</param>
    /// <param name="shootIfAngleSmallPastZone55">Player must shoot if facing directly at the goalposts when he is past zone 5.5</param>
    /// <param name="useLookVectorAngle">Indicates if the player's look vector angle must be used, if the direction or movement angle fails.</param>
    /// <param name="wantToBicycleKick">Indicates if the player wants to bicycle kick.</param>
    public virtual bool CanShootToGoal(out Vector3 getShootPos, bool forceShootPastZone5,
                                       bool ignoreAnglePastZone55, bool shootIfAngleSmallPastZone55,
                                       bool useLookVectorAngle, bool wantToBicycleKick)
    {
        getShootPos = Vector3.zero;

        bool inGoalkeeperArea = field.IsInGoalArea(gameObject, otherTeam);
        Vector3 playerVec, intersectPoint;
        float angle, chance, newValidPassAngle;
        bool angleIsValid = false;

        newValidPassAngle = skills.canPassWithinAngle;

        // Get the player's facing angle
        if (IsMoving())
        {
            playerVec = moveVec;
        }
        else
        {
            playerVec = transform.forward;
        }
        angle = GeUtils.GetAngleFromVector(new Vector2(playerVec.x, playerVec.z));

        if (wantToBicycleKick)
        {
            // Inverse the angle (should face away from goal posts when doing a bicycle kick)
            angle += 180.0f;
            if (angle >= 360.0f)
            {
                angle -= 360.0f;
            }
        }


        // Player must be facing within a valid angle towards goalposts to shoot at goal.
        if (((team.PlayDirection > 0) && (angle >= (90.0f - validFacingShootAngle)) && (angle <= (90.0f + validFacingShootAngle))) ||
            ((team.PlayDirection < 0) && (angle >= (270.0f - validFacingShootAngle)) && (angle <= (270.0f + validFacingShootAngle))))
        {
            angleIsValid = true;
        }
        else if (useLookVectorAngle)
        {
            angle = GeUtils.GetAngleFromVector(new Vector2(lookVec.x, lookVec.z));
            if (((team.PlayDirection > 0) && (angle >= (90.0f - validFacingShootAngle)) && (angle <= (90.0f + validFacingShootAngle))) ||
                ((team.PlayDirection < 0) && (angle >= (270.0f - validFacingShootAngle)) && (angle <= (270.0f + validFacingShootAngle))))
            {
                angleIsValid = true;
            }
        }


        // Test if player is facing towards the goalposts
        if ((angleIsValid) ||
            ((ignoreAnglePastZone55) && (field.PlayerPastZone(this, 5.5f, team))) ||
            (inGoalkeeperArea))
        {
            chance = -1.0f;

            if (inGoalkeeperArea)
            {
                if (IsUserControlled)
                {
                    chance = 1000.0f;
                }
                else
                {
                    chance = skills.ai.chanceShootInGoalArea;
                }
            }
            else if (((forceShootPastZone5) && (field.PlayerPastZone(this, 5, team, true))) ||
                     ((shootIfAngleSmallPastZone55) && (field.PlayerPastZone(this, 5.5f, team)) &&
                       (field.IsPlayerMovingToGoalpost(otherTeam, this, out intersectPoint, false, true, useLookVectorAngle, 0.5f))))
            {
                chance = 1000.0f;
            }
            else if ((this == team.ForemostPlayer) &&
                     ((IsUserControlled) || (skills.ai.canShootIfForemostPlayer)) &&
                     (field.PlayerPastZone(this, 4, team, true)))
            {
                chance = 1000.0f;
            }
            else
            {
                // Determine the chance of shooting, based on the player's horizontal position
                if (team.PlayDirection > 0.0f)
                {
                    if (field.PlayerBeforeZone(this, 4, team, true))
                    {
                        // Before half way
                        // Do Not allow shooting from before the halfway line in the first few seconds of the match
                        if (match.HalfTimeTimer > delayShootingBeforeHalfway)
                        {
                            chance = GeUtils.Lerp(transform.position.x, field.PlayArea.min.x, field.CentreMark.x,
                                                  skills.chanceShootFromOwnGoal, skills.chanceShootFromHalfway);
                        }
                    }
                    else if (field.PlayerBeforeZone(this, 6, team, true))
                    {
                        // Before zone 6
                        chance = GeUtils.Lerp(transform.position.x, field.CentreMark.x, field.GetZoneLeftEdge(6),
                                              skills.chanceShootFromHalfway, skills.chanceShootFromZone6);
                    }
                    else
                    {
                        // Past zone 6
                        chance = GeUtils.Lerp(transform.position.x, field.GetZoneLeftEdge(6), field.GetZoneLeftEdge(8),
                                              skills.chanceShootFromZone6, skills.chanceShootBeforeZone8);
                    }
                }
                else if (team.PlayDirection < 0.0f)
                {
                    if (field.PlayerBeforeZone(this, 4, team, true))
                    {
                        // Before half way
                        // Do Not allow shooting from before the halfway line in the first few seconds of the match
                        if (match.HalfTimeTimer > delayShootingBeforeHalfway)
                        {
                            chance = GeUtils.Lerp(transform.position.x, field.PlayArea.max.x, field.CentreMark.x,
                                                  skills.chanceShootFromOwnGoal, skills.chanceShootFromHalfway);
                        }
                    }
                    else if (field.PlayerBeforeZone(this, 6, team, true))
                    {
                        // Before zone 6
                        chance = GeUtils.Lerp(transform.position.x, field.CentreMark.x, field.GetZoneRightEdge(1),
                                              skills.chanceShootFromHalfway, skills.chanceShootFromZone6);
                    }
                    else
                    {
                        // Past zone 6
                        chance = GeUtils.Lerp(transform.position.x, field.GetZoneRightEdge(1), field.GetZoneRightEdge(-1),
                                              skills.chanceShootFromZone6, skills.chanceShootBeforeZone8);
                    }
                }
            }


            if (Random.Range(0.0f, 100.0f) < chance)
            {
                getShootPos = field.GetShootPos(this);

                // Adjust the valid pass angle, which will affect if the player passes to another player (for the current Update)
                validPassAngle = newValidPassAngle;

                return (true);
            }
        }

        return (false);
    }


    /// <summary>
    /// Test if the player can start a slide tackle (e.g. check for target, distance, angle, etc.).
    /// </summary>
    /// <returns><c>true</c> if this instance can slide the specified slideDirection; otherwise, <c>false</c>.</returns>
    /// <param name="slideDirection">Slide direction. Note: If angle to target is greater than the valid slide angle then the player
    /// will just slide forward, Not towards the target (unless tha AI ignores the valid angle).</param>
    public virtual bool CanSlide(out Vector3 slideDirection)
    {
        slideDirection = Vector3.zero;

        if ((match.State != SsMatch.states.play) ||
            (this == match.BallPlayer) || (skills.canSlideTackle == false) ||
            (stateMatrix.CanChange(state, states.slideTackle) == false) ||
            (delayPass.pending) ||
            (delayShoot.pending))
        {
            return (false);
        }

        SsPlayer targetPlayer = match.BallPlayer;

        if ((targetPlayer == null) || (targetPlayer.Team == team) || (match.BallTeam == team) || (targetPlayer == this))
        {
            return (false);
        }

        if (IsAiControlled)
        {
            // Is AI Not allowed to slide an opponent who is busy kicking?
            if ((targetPlayer.IsKicking) && (skills.ai.canSlideKickingOpponent == false))
            {
                return (false);
            }
        }

        // Not allowed to slide goalkeeper holding the ball?
        if ((skills.canSlideGoalkeeperWithBall == false) && (targetPlayer.Position == positions.goalkeeper) &&
            (targetPlayer == match.BallPlayer))
        {
            if ((targetPlayer.GkIsThrowingIn) || (targetPlayer.State == states.gk_standHoldBall) ||
                (targetPlayer.IsDiving))
            {
                return (false);
            }
        }


        Vector3 vec, facing;
        float angle;
        bool angleTooWide = false;

        if (IsMoving())
        {
            facing = moveVec;
        }
        else
        {
            facing = transform.forward;
        }

        // Vector to target
        vec = targetPlayer.transform.position - transform.position;
        angle = Vector3.Angle(facing, vec);
        if ((angle > skills.validSlideAngle) &&
            ((IsUserControlled) || (skills.ai.ignoreSlideAngle == false)))
        {
            angleTooWide = true;
        }


        // Test if AI tries to slide from behind within the first few seconds the target has the ball
        if ((IsAiControlled) && (targetPlayer.haveBallTime + skills.ai.slideFromBehindDelay > Time.time))
        {
            Vector2 targetFacing;
            if (targetPlayer.IsMoving())
            {
                targetFacing = targetPlayer.MoveVec;
            }
            else
            {
                targetFacing = targetPlayer.transform.forward;
            }
            angle = Vector3.Angle(targetFacing, vec);
            if (angle < 90.0f)
            {
                return (false);
            }
        }


        // Test if the nearest player to the baller is much closer than this player. (Note: Updated this to only apply to AI.)
        if ((IsAiControlled) &&
            (team.NearestPlayerToBall != null) && (this != team.NearestPlayerToBall) &&
            (DistanceToObjectSquared(targetPlayer.gameObject) > team.NearestPlayerToBall.DistanceToObjectSquared(targetPlayer.gameObject) * 1.5f))
        {
            return (false);
        }


        // Test if player closer than the valid slide distance, OR the player (human only) is the nearest to the baller
        if ((vec.sqrMagnitude <= (skills.startSlideDistance * skills.startSlideDistance)) ||
            ((IsUserControlled) && (this == team.NearestPlayerToBall)))
        {
            if (angleTooWide)
            {
                slideDirection = facing.normalized;
            }
            else
            {
                slideDirection = vec.normalized;
            }

            slideTarget = targetPlayer;
            slideSqrDistanceAtStart = vec.sqrMagnitude;
            targetPlayer.BallerSlideTarget = this;
            targetPlayer.DynamicChanceSideStep = Random.Range(0.0f, 100.0f);
            slideDidStartAsAi = IsAiControlled;

            // Determine slide angle (e.g. sliding from front, side, behind)
            vec = transform.position - targetPlayer.transform.position;
            slideTargetAngle = Vector3.Angle(targetPlayer.transform.forward, vec);

            return (true);
        }

        return (false);
    }


    /// <summary>
    /// Test if the player can bicycle kick and start it.
    /// </summary>
    /// <returns><c>true</c> if this instance can bicycle kick; otherwise, <c>false</c>.</returns>
    public virtual bool CanBicycleKick()
    {
        if ((match.BallPlayer == null) &&
            (ball.transform.position.y > Mathf.Min(ballBicycleHeightMin, headHeight / 2.0f)) &&
             (ball.transform.position.y < Mathf.Max(ballBicycleHeightMax, headHeight + 1.0f)) &&
            ((this == team.PassPlayer) || (IsUserControlled)) &&
            (stateMatrix.CanChange(state, states.bicycleKick)) &&
            (DistanceToObjectSquared(ball.gameObject) >= minStartBicycleKickDistanceSquared) &&
            (DistanceToObjectSquared(ball.gameObject) <= maxStartBicycleKickDistanceSquared))
        {
            Vector3 shootPos;
            bool canShoot;
            if (team.IsUserControlled)
            {
                canShoot = CanShootToGoal(out shootPos, (team.PotentialPassPlayer == null), false, true, false, true);
            }
            else
            {
                canShoot = CanShootToGoal(out shootPos, false, skills.ai.ignoreAngleWhenShooting, false, false, true);
            }

            if (canShoot)
            {
                if ((team.PlayDirection > 0) &&
                    (ball.transform.position.x < transform.position.x) &&
                    (transform.forward.x < 0))
                {
                    // Team playing right, ball coming from left and player facing left (i.e. away from opponent goals)
                    StopMoving(true, true, true);
                    RotateToDirection(-Vector3.right);
                    SetState(states.bicycleKick);
                    Jump(upBicycleForce, bicycleDelayGravity);
                    return (true);
                }
                else if ((team.PlayDirection < 0) &&
                         (ball.transform.position.x > transform.position.x) &&
                         (transform.forward.x > 0))
                {
                    // Team playing left, ball coming from right and player facing right (i.e. away from opponent goals)
                    StopMoving(true, true, true);
                    RotateToDirection(Vector3.right);
                    SetState(states.bicycleKick);
                    Jump(upBicycleForce, bicycleDelayGravity);
                    return (true);
                }
            }
        }

        return (false);
    }


    /// <summary>
    /// Test if player collides with the box collider.
    /// </summary>
    /// <returns>The with box collider.</returns>
    /// <param name="boxTransform">Box transform.</param>
    /// <param name="boxRadius">Box radius.</param>
    /// <param name="boxHeight">Box height.</param>
    public virtual bool CollideWithBoxCollider(Transform boxTransform, float boxRadius, float boxHeight)
    {
        if (boxTransform == null)
        {
            return (false);
        }

        float halfHeight = boxHeight / 2.0f;

        if (((boxTransform.position.y - halfHeight) <= transform.position.y + (playerBoxHeight / 2.0f)) &&
            ((boxTransform.position.y + halfHeight) >= transform.position.y - (playerBoxHeight / 2.0f)) &&
            ((boxTransform.position.x - boxRadius) <= transform.position.x + playerBoxRadius) &&
            ((boxTransform.position.x + boxRadius) >= transform.position.x - playerBoxRadius) &&
            ((boxTransform.position.z - boxRadius) <= transform.position.z + playerBoxRadius) &&
            ((boxTransform.position.z + boxRadius) >= transform.position.z - playerBoxRadius))
        {
            return (true);
        }

        return (false);
    }


    /// <summary>
    /// Test if this player collides with another player.
    /// </summary>
    /// <returns>The with player.</returns>
    /// <param name="player">Player.</param>
    public virtual bool CollideWithPlayer(SsPlayer player)
    {
        return ((player != null) && (CollideWithBoxCollider(player.transform, player.PlayerBoxRadius, player.PlayerBoxHeight)));
    }


    /// <summary>
    /// Update.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public override void Update()
    {
        if ((match == null) || (match.IsLoading(true)))
        {
            // Match is busy loading
            return;
        }

        base.Update();

        float dt = Time.deltaTime;
        float speedScale = 1.0f;

        // Preparations before updating the player (e.g. clear movement vector, reset speed, etc.).
        PreUpdatePlayer(dt, ref speedScale);

        // Bulk of player update
        UpdatePlayer(dt, ref speedScale);

        // Post-update player (e.g. record previous position, previous movement vec, calc distance moved, etc.).
        PostUpdatePlayer(dt, ref speedScale);
    }



    /// <summary>
    /// Preparations for player update (e.g. clear movement vector, reset speed, etc.).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The update player.</returns>
    /// <param name="dt">Dt.</param>
    /// <param name="speedScale">Speed scale.</param>
    protected virtual void PreUpdatePlayer(float dt, ref float speedScale)
    {
        float gravity = Physics.gravity.y * dt;
        float prevVelocityY = velocityY;

        if (delayGravity > 0.0f)
        {
            gravity = 0.0f;
            delayGravity -= dt;
            if (delayGravity < 0.0f)
            {
                delayGravity = 0.0f;
            }
        }


        // Reset movement speed. Movement speed and speedscale can be affected by various things.
        speed = skills.runSpeed;    // Default to run speed, because its the most common movement action.

        // Restore animation speed, potentially changed by a sprint
        if ((state != states.run) && (animations.AnimationSpeed != 1.0f))
        {
            animations.SetAnimationSpeed(1.0f);
        }

        if (IsOnGround)
        {
            onGroundTime += dt;
        }
        else
        {
            onGroundTime = 0.0f;
        }


        // Clear movement
        moveVec = Vector3.zero;
        didMoveVec = Vector3.zero;
        didMoveSpeed = 0.0f;


        // Apply gravity
        // NOTE: Player's gravity will never by 100% like Unity's rigidbody's, because it is updated in Update instead of FixedUpdate.
        if (onGroundTime > 1.0f)
        {
            // Standing on the ground
            velocityY = gravity;
        }
        else
        {
            // Apply gravity
            velocityY += gravity;

            if ((prevVelocityY > 0.0f) && (velocityY <= 0.0f))
            {
                // Started moving down
                if (hangTime > 0.0f)
                {
                    // Pause in the air
                    delayGravity = Mathf.Max(delayGravity, hangTime);
                    hangTime = 0.0f;
                    velocityY = 0.0f;
                }
            }
        }


        if ((match.IsGoalkeeperHoldingTheBall()) ||
            (match.State == SsMatch.states.kickOff) ||
            (match.State == SsMatch.states.throwIn) ||
            (match.State == SsMatch.states.cornerKick) ||
            (match.State == SsMatch.states.goalKick))
        {
            // Can pass in any direction
            validPassAngle = 360.0f;
        }
        else
        {
            validPassAngle = skills.canPassWithinAngle;
        }
    }


    /// <summary>
    /// Post-update player (e.g. record previous position, previous movement vec, calc distance moved, etc.).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The update player.</returns>
    /// <param name="dt">Dt.</param>
    /// <param name="speedScale">Speed scale.</param>
    protected virtual void PostUpdatePlayer(float dt, ref float speedScale)
    {
        if (IsMoving())
        {
            moveDistance += new Vector2(transform.position.x - prevPos.x, transform.position.z - prevPos.z).magnitude;
        }
        else
        {
            moveDistance = 0.0f;
        }

        if (trackMaxHeight < transform.position.y)
        {
            trackMaxHeight = transform.position.y;
        }

        prevPos = transform.position;
        prevMoveVec = moveVec;
    }


    /// <summary>
    /// Updates the player. The main update method. Handles calculations for speed, processing input results, movement, rotation, etc.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The player.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdatePlayer(float dt, ref float speedScale)
    {
        // Is there a pending shoot?
        if ((delayShoot.pending) && (delayShoot.delayTime <= Time.time))
        {
            delayShoot.pending = false;
            delayPass.pending = false;
            ball.Shoot(this, delayShoot.targetPos, delayShoot.shotAtGoal, delayShoot.clip);
        }

        // Is there a pending pass?
        if ((delayPass.pending) && (delayPass.delayTime <= Time.time))
        {
            delayPass.pending = false;
            delayShoot.pending = false;
            ball.Pass(this, delayPass.toPlayer, delayPass.targetPos, delayPass.clearBallTeam, delayPass.clip);
        }


        CollectLooseBall(dt);

        if (CanMoveFromInput())
        {
            UpdateSpeedAndAutoMovement(dt, ref speedScale);
            UpdateMoveToTarget(dt);
            UpdateMovementAndRotation(dt, speedScale);
        }


        UpdateZoneAndVisibility(dt, true, team, field);

        UpdateState(dt);

        if (stopMarkingInNextUpdate)
        {
            SetMarkPlayer(null, -1.0f, -1.0f);
        }

        UpdateMarkedByPlayers(dt);

        if (match.BallPlayer == this)
        {
            UpdateMoveWithBall(dt);
        }

        UpdateMarkers(dt);
    }


    /// <summary>
    /// Set the movement speed based on the player's state. 
    /// It also sets the input.axes for automatic movement (e.g. move forward while sliding, diving).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The movement speed.</returns>
    /// <param name="dt">Delta time.</param>
    /// <param name="speedScale">Speed scale.</param>
    protected virtual void UpdateSpeedAndAutoMovement(float dt, ref float speedScale)
    {
        bool autoMoveForward = false;   // Automatically move forward (e.g. sliding)
        float max;

        if ((targetObject != null) || (gotoTargetPos))
        {
            autoMoveForward = true;
        }


        switch (state)
        {
            case states.run:
                {
                    if (SsSettings.canSprint)
                    {
                        if (input.sprintActive)
                        {
                            speed = skills.sprintSpeed;
                            animations.SetAnimationSpeed(skills.sprintAnimationSpeedScale);
                        }
                        else
                        {
                            animations.SetAnimationSpeed(1.0f);
                        }
                    }

                    if (ai != null)
                    {
                        // Run away fast from goalkeeper holding the ball
                        if (((ai.RunningStraightBackwards) && (match.IsOtherTeamGoalkeeperHoldingTheBall(this))) ||
                            ((ai.RunningStraightForwards) && (match.IsTeamGoalkeeperHoldingTheBall(this))))
                        {
                            speed = skills.runFromGoalkeeperSpeed;
                        }
                        else if ((position == positions.goalkeeper) && (ai.RunningToGoalkeeperArea))
                        {
                            // Goalkeeper runs slightly faster back to the goalkeeper area
                            speedScale = 1.3f;
                        }
                    }
                    break;
                }
            case states.slideTackle:
                {
                    speed = skills.slideTackleSpeed;
                    autoMoveForward = true;
                    break;
                }
            case states.gk_diveForward:
            case states.gk_diveUp:
            case states.gk_diveRight:
            case states.gk_diveLeft:
                {
                    speed = dynamicDiveSpeed;
                    autoMoveForward = true;
                    break;
                }
            case states.falling:
                {
                    if (fallForward)
                    {
                        // Move forward while in the air, or move certain distance across the ground
                        max = fallForwardDistance * field.frictionScale;
                        if ((IsOnGround == false) ||
                            ((field.IsSlippery) && (moveDistance < max)))
                        {
                            if (IsOnGround)
                            {
                                // Slow down
                                if (field.frictionScale > 0.0f)
                                {
                                    speedScale = Mathf.Max(1.0f - (moveDistance / (5.0f * field.frictionScale)), 0.0f);
                                }
                                else
                                {
                                    speedScale = 1.0f;
                                }
                                if (speedScale <= 0.01f)
                                {
                                    fallForwardDone = true;
                                    fallForward = false;
                                }
                            }
                            autoMoveForward = true;
                        }
                    }
                    break;
                }
        } //switch


        if (autoMoveForward)
        {
            input.axes = new Vector2(transform.forward.x, transform.forward.z);
            input.lookAxes = input.axes;
        }
    }


    /// <summary>
    /// Update movement to a target object, position. Also rotates to a target direction.
    /// </summary>
    /// <returns>The move to target.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdateMoveToTarget(float dt)
    {
        SsGameObject target = (markPlayer != null) ? markPlayer : targetObject;
        Vector3 vec;
        float distance;
        bool behind, collided;

        if (target != null)
        {
            // Move to a target object
            vec = target.transform.position - transform.position;
            vec.y = 0.0f;   // Ignore height
            distance = vec.sqrMagnitude;

            behind = (Vector3.Dot(initVecToTarget, vec.normalized) < 0.0f);

            collided = false;
            if ((target.ObjectType == objectTypes.player) && (CollideWithPlayer((SsPlayer)target)))
            {
                collided = true;
            }

            if (((distance <= targetObjectRadiusSquared) || (collided)) &&
                (IsMoving()) &&
                ((team.IsUserControlled) || (isBallPredator == false)))
            {
                // Delay movement to give the prey some time to get away.
                targetObjectUpdateDirectionTime = Time.time + skills.ai.delayMoveTooCloseToTarget;
                StopMoving(true, true, false);
            }
            else if (targetObjectUpdateDirectionTime <= Time.time)
            {
                targetObjectUpdateDirectionTime = Time.time + skills.ai.changeDirectionIntervals;

                if (state != states.slideTackle)
                {
                    // Turn towards target
                    if ((targetObject == ball) || (IsMoving()) ||
                        ((IsMoving() == false) && (distance > targetObjectRadiusSquaredMax)))
                    {
                        targetDirection = vec;
                        targetDirection.Normalize();
                    }
                }
            }
        }
        else if (gotoTargetPos)
        {
            // Move to a target position
            vec = targetPos - transform.position;
            vec.y = 0.0f;   // Ignore height
            distance = vec.sqrMagnitude;

            behind = (Vector3.Dot(initVecToTarget, vec.normalized) < 0.0f);

            // Did reach position?
            if ((distance <= targetPosRadiusSqr) || (behind))
            {
                if (ai != null)
                {
                    if ((team.IsUserControlled) && (match.IsTeamGoalkeeperHoldingTheBall(this)))
                    {
                        // Human team's AI pause longer to make it easier for goalie to select a player to pass to
                        ai.ForceDelayUpdateTime = Time.time + 3.0f;
                    }
                    else if ((ai.RunningStraightBackwards) || (ai.RunningStraightForwards))
                    {
                        // Brief pause before moving again
                        ai.ForceDelayUpdateTime = Time.time + 0.1f;
                    }
                }

                StopMoving(true, false, false);
            }
            else
            {
                if (targetPosUpdateDirectionTime <= Time.time)
                {
                    // Turn towards target
                    targetPosUpdateDirectionTime = Time.time + 1.0f;

                    targetDirection = vec;
                    targetDirection.Normalize();
                }
            }
        }


        // Rotate to a target direction
        if (targetDirection != Vector3.zero)
        {
            vec = Vector3.RotateTowards(transform.forward,
                                        targetDirection,
                                        skills.turnSpeed * Mathf.Deg2Rad * dt, 1.0f);
            input.lookAxes = new Vector2(vec.x, vec.z);
        }
    }


    /// <summary>
    /// Updates the movement based on the input vector, speed and velocityY.
    /// Updates the rotation based on the look axes.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The movement.</returns>
    /// <param name="dt">Dt.</param>
    /// <param name="speedScale">Scale movement speed.</param>
    protected virtual void UpdateMovementAndRotation(float dt, float speedScale)
    {
        speed = speed * speedScale;

        Vector2 vec = input.axes.normalized;
        moveVec.x += (vec.x * speed * dt);
        moveVec.z += (vec.y * speed * dt);

        moveVec.y += (velocityY * dt);

        lookVec = new Vector3(input.lookAxes.x, 0.0f, input.lookAxes.y);

        if (controller != null)
        {
            RotateToDirection(lookVec);

            Vector3 oldPos = transform.position;

            controller.Move(moveVec);

            didMoveVec = transform.position - oldPos;
            oldPos = didMoveVec;
            oldPos.y = 0.0f;
            didMoveSpeed = oldPos.magnitude;
        }
    }


    /// <summary>
    /// Updates the state and animations.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The state.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdateState(float dt)
    {
        float distance;

        switch (state)
        {
            case states.slideTackle:
                {
                    // Slide tackle
                    //-------------
                    distance = skills.slideTackleDistance * field.frictionScale;

                    // Test if player slid far enough. Only need to slide 1/2 the distance if he has the ball.
                    if ((moveDistance > distance) ||
                        ((this == match.BallPlayer) && (moveDistance > distance / 2.0f)) ||
                        ((speed > 0.0f) && (stateTime + (distance / speed) < Time.time)))
                    {
                        // Stop sliding
                        SetState(states.idle);
                    }
                    else if ((match.State == SsMatch.states.play) &&
                             (match.BallPlayer != null) &&
                             (match.BallPlayer.Team != team) &&
                             (match.BallPlayer != this) &&
                             (slideTackleSuccess) &&
                             (match.BallPlayer.UnTackleableTime <= Time.time) &&
                             (CollideWithPlayer(match.BallPlayer)))
                    {
                        // The player tackled the baller
                        match.BallPlayer.TackledAngle = slideTargetAngle;
                        match.BallPlayer.SetState(states.falling);

                        match.BallPlayer.CannotTouchBallTimer = Time.time + cannotTouchBallWhenTackledTime;

                        match.SetBallPlayer(this);
                    }
                    else if ((skills.canChangeSlideDirection) ||
                             ((IsAiControlled) && (skills.ai.canChangeSlideDirection) && (slideDidStartAsAi)))
                    {
                        // Change direction while sliding
                        if ((slideTarget != null) &&
                            ((slideTarget == match.BallPlayer) || (slideTarget == otherTeam.PassPlayer)) &&
                            (delaySlideChangeDirection <= Time.time))
                        {
                            delaySlideChangeDirection = Time.time + maxDelaySlideChangeDirection;
                            RotateToObject(slideTarget.gameObject);
                        }
                    }

                    break;
                }

            case states.idle:
                {
                    // Idle
                    //-----

                    if (IsMoving())
                    {
                        SetState(states.run);
                    }
                    else if ((IsUserControlled == false) && (this != match.BallPlayer) &&
                             (targetObject == null) && (gotoTargetPos == false))
                    {
                        // Turn towards the ball
                        RotateToObject(ball.gameObject);
                    }

                    break;
                }
            case states.gk_diveForward:
            case states.gk_diveUp:
            case states.gk_diveRight:
            case states.gk_diveLeft:
                {
                    // Dive
                    //-----
                    distance = skills.maxDiveDistance * field.frictionScale;

                    if ((this == match.BallPlayer) || (slowDownDive))
                    {
                        // Slow down when holding the ball
                        dynamicDiveSpeed = Mathf.MoveTowards(dynamicDiveSpeed, 0.0f, diveSlowDownSpeedWithBall * dt);

                        // Start falling down
                        if (velocityY > 0.0f)
                        {
                            velocityY = 0.0f;
                        }
                    }

                    // Test if player dived far enough. Only need to dive 1/3 the distance if he has the ball.
                    if ((IsOnGround) &&
                        ((moveDistance > distance) ||
                          ((this == match.BallPlayer) && (moveDistance > distance / 3.0f)) ||
                          ((IsMoving() == false) && (onGroundTime > 0.1f))))
                    {
                        if ((this == match.BallPlayer) && (field.IsInPenaltyArea(gameObject, team)))
                        {
                            // Goalkeeper lands while holding the ball
                            match.SetGoalkeeperHoldBall(this);
                        }
                        else
                        {
                            SetState(states.idle);
                        }
                    }
                    else if ((match.State == SsMatch.states.play) &&
                             (match.BallPlayer != null) && (match.BallPlayer.team != team) &&
                             (match.BallPlayer != this) && (CollideWithPlayer(match.BallPlayer)))
                    {
                        // The player dive-tackled the baller
                        if (ball.WasShotAtGoal == false)
                        {
                            ball.Sounds.PlaySfx(ball.Sounds.goalkeeperCatch);
                        }

                        match.BallPlayer.CannotTouchBallTimer = Time.time + cannotTouchBallWhenTackledTime;

                        match.BallPlayer.StopMoving(true, true, true);

                        match.SetBallPlayer(this);
                        match.SetGoalkeeperHoldBall(this);
                    }

                    break;
                }

            case states.run:
                {
                    // Run
                    //----

                    if (IsMoving() == false)
                    {
                        SetState(states.idle);
                    }

                    break;
                }
            case states.kickNear:
            case states.kickMedium:
            case states.kickFar:
                {
                    // Kick
                    //-----

                    if (IsMoving() == false)
                    {
                        if (animations.IsAnimationPlaying() == false)
                        {
                            SetState(states.idle);
                        }
                    }
                    break;
                }
            case states.falling:
                {
                    // Falling
                    //--------

                    if ((IsMoving() == false) || (fallForwardDone))
                    {
                        if ((stateTime + 1.0f <= Time.time) && (animations.IsAnimationPlaying() == false))
                        {
                            SetState(states.inPain);
                        }
                    }
                    break;
                }
            case states.inPain:
                {
                    // In pain
                    //--------

                    if ((IsMoving() == false) && (animations.IsAnimationPlaying() == false))
                    {
                        painDelay -= dt;
                        if (painDelay <= 0.0f)
                        {
                            painDelay = 0.0f;
                            SetState(states.idle);
                        }
                    }
                    break;
                }
            case states.throwIn:
                {
                    // Throw in
                    //---------
                    if (animations.IsAnimationPlaying() == false)
                    {
                        SetState(states.idle);
                    }
                    break;
                }
            case states.gk_throwBallNear:
            case states.gk_throwBallMedium:
            case states.gk_throwBallFar:
                {
                    // Goalkeeper Throw in
                    //--------------------
                    if (animations.IsAnimationPlaying() == false)
                    {
                        SetState(states.idle);
                    }
                    break;
                }
            case states.bicycleKick:
                {
                    // Bicycle kick
                    //-------------
                    if ((IsOnGround) && (onGroundTime > 0.1f) && (velocityY <= 0.0f) &&
                        (animations.IsAnimationPlaying() == false))
                    {
                        // Stop bicycle kick on on the ground
                        SetState(states.idle);
                    }
                    break;
                }
            case states.header:
                {
                    // Header
                    //-------
                    if ((IsOnGround) && (onGroundTime > 0.1f) && (velocityY <= 0.0f) &&
                        (animations.IsAnimationPlaying() == false))
                    {
                        // Stop header on on the ground
                        SetState(states.idle);
                    }
                    break;
                }
        } //switch
    }


    /// <summary>
    /// Update the ball movement when the player has the ball (e.g. dribble, goalkeeper hold ball, throw in, etc.).
    /// NOTE: Derived method must call the base method.
    /// </summary>
    /// <returns>The move with ball.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdateMoveWithBall(float dt)
    {
        Vector3 pos, vec;
        float angle, distance;
        bool changePos, lowEnough, ignoreLostBall, snapBall;

        // Is ball low enough for the player to push it to a desired position?
        lowEnough = (ball.transform.position.y < transform.position.y + headHeight);

        // Did the player just get the ball?
        ignoreLostBall = (haveBallFrame >= Time.frameCount - 2);
        snapBall = ignoreLostBall;

        isDribbling = false;

        if ((Position == positions.goalkeeper) &&
            (IsDiving))
        {
            // Goalkeeper diving with the ball
            //--------------------------------

            // Ball Not attached to the player?
            if (ball.transform.IsChildOf(transform) == false)
            {
                // Position in front of player
                ball.transform.position = transform.position + (transform.forward * 0.5f) + transform.up;
            }
        }
        else if ((Position == positions.goalkeeper) &&
                 (state == states.gk_standHoldBall))
        {
            // Goalkeeper standing and holding the ball
            //-----------------------------------------

            // Ball Not attached to the player?
            if (ball.transform.IsChildOf(transform) == false)
            {
                // Position in front of player
                ball.transform.position = transform.position + (transform.forward * 0.5f) + transform.up;
            }
        }
        else if (GkIsThrowingIn)
        {
            // Goalkeeper throwing in ball
            //----------------------------

            // Ball Not attached to the player?
            if (ball.transform.IsChildOf(transform) == false)
            {
                // Position above player
                ball.transform.position = transform.position + (transform.up * headHeight);
            }
        }
        else if (state == states.throwInHold)
        {
            // Player standing about to throw in the ball
            //-------------------------------------------

            // Ball Not attached to the player?
            if (ball.transform.IsChildOf(transform) == false)
            {
                // Position in front of player
                ball.transform.position = transform.position + (transform.forward * 0.5f) + transform.up;
            }
        }
        else if (state == states.throwIn)
        {
            // Player throwing in ball
            //------------------------

            // Ball Not attached to the player?
            if (ball.transform.IsChildOf(transform) == false)
            {
                // Position above player
                ball.transform.position = transform.position + (transform.up * headHeight);
            }
        }
        else if ((match.State == SsMatch.states.play) ||
                 (match.State == SsMatch.states.preCornerKick) ||
                 (match.State == SsMatch.states.preGoalKick) ||
                 (match.State == SsMatch.states.preMatchResults) ||
                 (match.State == SsMatch.states.preThrowIn))
        {
            if ((state == states.run) ||
                (state == states.idle) ||
                (state == states.slideTackle))
            {
                // Running/standing/sliding with the ball
                //---------------------------------------

                // Is ball low enough?
                if (lowEnough)
                {
                    ignoreLostBall = UpdateDribble(dt, ref snapBall);

                    isDribbling = true;
                }
            }
            else if ((IsKicking) &&
                     ((delayPass.pending) || (delayShoot.pending)))
            {
                // Busy kicking
                //-------------

                // Is ball low enough?
                if (lowEnough)
                {
                    // Position ball infront of player
                    changePos = false;
                    pos = ball.transform.position;

                    vec = pos - transform.position;
                    vec.y = 0.0f;   // Ignore height
                    distance = vec.magnitude;

                    // Angle between player forward direction and ball
                    angle = Vector3.Angle(vec, transform.forward);
                    if ((angle > 5.0f) || (distance > skills.ballDribbleDistanceMin))
                    {
                        // Rotate ball on an arc to move it in front of the player
                        distance = skills.ballDribbleDistanceMin;
                        pos = transform.position + (transform.forward * distance);
                        vec = pos - transform.position;
                        changePos = true;
                    }

                    if (changePos)
                    {
                        // Move ball to position
                        ball.RandomSpin();
                        ball.SetTargetPosition(true, new Vector3(pos.x, ball.transform.position.y, pos.z), snapBall);
                    }
                }
            }


            if (snapBall == false)
            {
                vec = ball.transform.position - transform.position;
                vec.y = 0.0f;   // Ignore height
                distance = vec.sqrMagnitude;
                if (ignoreLostBall == false)
                {
                    // Is ball too high or too far?
                    if ((lowEnough == false) ||
                        (distance > ballLostDistanceSqr))
                    {
                        if ((delayPass.pending == false) &&
                            (delayShoot.pending == false))
                        {
                            // Player lost the ball
                            match.SetBallPlayer(null, false);
                        }
                    }
                }
                else if (distance > ballLostDistanceAlwaysSqr)
                {
                    // Ball is very far, so ball must be lost
                    match.SetBallPlayer(null, false);
                }
            }
        }
    }


    /// <summary>
    /// Updates the dribble (i.e. running with the ball).
    /// </summary>
    /// <returns>True if we must Not test for a lost ball.</returns>
    /// <param name="dt">Dt.</param>
    public virtual bool UpdateDribble(float dt, ref bool snapBall)
    {
        Vector3 pos = ball.transform.position;
        bool changePos, spin, ignoreLostBall, applyFakeForce;
        float originalDistance, distance, sideDistance, percent, fakeForceDeceleration, fakeForceMinSpeed;
        Vector3 vec, fakeForce;

        changePos = false;
        spin = false;
        ignoreLostBall = false;

        applyFakeForce = false;
        fakeForce = Vector3.zero;
        fakeForceDeceleration = SsBall.dribbleFriction;
        fakeForceMinSpeed = 0.0f;

        vec = pos - transform.position;
        vec.y = 0.0f;   // Ignore height
        distance = vec.magnitude;
        originalDistance = distance;

        if (dribbleState == dribbleStates.getBall)
        {
            // Just got the ball
            // Snap to the front of the player
            // If distance is outside the dribble range then select a random forward distance
            if ((distance < skills.ballDribbleDistanceMin) || (distance > skills.ballDribbleDistanceMax))
            {
                distance = Random.Range(skills.ballDribbleDistanceMin, ballDribbleDistanceHalf);
            }
            // Random sideways distance
            sideDistance = Random.Range(-skills.ballDribbleSideDistance, skills.ballDribbleSideDistance);
            pos = transform.position + (transform.forward * distance) + (transform.right * sideDistance);
            changePos = true;
            snapBall = true;
            ignoreLostBall = true;

            dribbleState = dribbleStates.idle;
            dribbleTime = 0.0f;
            dribbleDuration = 0.0f;
            dribbleMinDurationToIdle = 0.0f;
        }
        else if (dribbleState == dribbleStates.idle)
        {
            vec = transform.InverseTransformPoint(pos);     // Ball position relative to the player

            if ((distance < skills.ballDribbleDistanceMin) ||
                ((vec.z < 0.0f) && (distance < skills.ballDribbleDistanceMax)))
            {
                // Ball is close (or behind), so kick it forward

                // Make sure ball is Not too far to the sides
                vec.x = Mathf.Clamp(vec.x, -skills.ballDribbleSideDistance, skills.ballDribbleSideDistance);

                dribbleState = dribbleStates.moveForward;
                dribbleTime = 0.0f;
                sideDistance = Random.Range(-skills.ballDribbleSideDistance, skills.ballDribbleSideDistance);

                // If player just got ball, and it is behind then first kick it to the player (to avoid kicking it too far)
                if ((dribbleKickCount <= 0) && (vec.z < 0.0f))
                {
                    distance = 0.0f;
                }
                else if (dribbleKickCount <= 0)
                {
                    // First kick, keep it nearer to avoid kicking too far
                    distance = ballDribbleDistanceHalf;
                }
                else
                {
                    distance = skills.ballDribbleDistanceMax;
                }

                pos = transform.position + (transform.forward * distance) + (transform.right * sideDistance);

                // Apply a fake force to the ball
                pos.y = ball.transform.position.y;
                fakeForce = ball.CalcFakeForce(pos, skills.ballDribbleKickSpeed, 0.0f, out dribbleDuration, out fakeForceDeceleration);
                dribbleMinDurationToIdle = dribbleDuration;

                // If player is moving then increase the distance to kick
                if (IsMoving())
                {
                    // How far will player move in dribble duration time?
                    vec = moveVec;
                    vec.y = 0.0f; // Ignore height
                    vec = vec.normalized * (speed * dribbleDuration);
                    distance += vec.magnitude;

                    pos = transform.position + (transform.forward * distance) + (transform.right * sideDistance);
                    pos.y = ball.transform.position.y;
                    fakeForceMinSpeed = speed * 0.5f;
                    fakeForce = ball.CalcFakeForce(pos, skills.ballDribbleKickSpeed, fakeForceMinSpeed, out dribbleDuration, out fakeForceDeceleration);
                }

                dribbleAngle = GeUtils.GetAngleFromVector3(transform.forward);
                dribbleKickCount++;

                applyFakeForce = true;
                ignoreLostBall = true;
            }
            else if ((vec.z < 0.0f) || (vec.x < -skills.ballDribbleSideDistance) || (vec.x > skills.ballDribbleSideDistance))
            {
                // Ball is behind or too far to the sides
                // Position in front of the player

                sideDistance = Mathf.Clamp(vec.x, -skills.ballDribbleSideDistance, skills.ballDribbleSideDistance);

                if (vec.z < 0.0f)
                {
                    distance = skills.ballDribbleDistanceMin;
                }

                pos = transform.position + (transform.forward * distance) + (transform.right * sideDistance);
                changePos = true;
                snapBall = (originalDistance * originalDistance < ballLostDistanceAlwaysSqr);
                ignoreLostBall = true;
            }

            // Always keep the ball while dribbling
            ignoreLostBall = true;
        }
        else if (dribbleState == dribbleStates.moveForward)
        {
            // Ball is moving forward

            vec = transform.InverseTransformPoint(pos);     // Ball position relative to the player

            // Is dribble time over or ball moved behind player?
            dribbleTime += dt;
            if ((dribbleTime > dribbleDuration) ||
                ((dribbleTime > dribbleMinDurationToIdle) && (vec.z < -ballDribbleDistanceHalf)))
            {
                dribbleTime = dribbleDuration;
                dribbleState = dribbleStates.idle;
            }

            ignoreLostBall = true;

            // Did direction (angle) change?
            percent = GeUtils.GetAngleFromVector3(transform.forward);
            if (Mathf.Abs(dribbleAngle - percent) > 1.0f)
            {
                // Rotate the ball's fake force
                if (ball.HasFakeForce)
                {
                    vec = ball.FakeForce;
                    vec.y = 0.0f;
                    vec = Quaternion.AngleAxis(dribbleAngle - percent, Vector3.up) * vec;
                    ball.FakeForce = new Vector3(vec.x, ball.FakeForce.y, vec.z);
                }

                // Large change in direction
                if (Mathf.Abs(dribbleAngle - percent) > 5.0f)
                {
                    // Make sure ball Not too far
                    distance = Mathf.Clamp(distance, skills.ballDribbleDistanceMin, ballDribbleDistanceHalf);
                }

                dribbleAngle = percent;

                // Snap ball to front of the player
                sideDistance = 0.0f;
                pos = transform.position + (transform.forward * distance) + (transform.right * sideDistance);
                changePos = true;
                snapBall = (originalDistance * originalDistance < ballLostDistanceAlwaysSqr);
            }
        }


        // Finally, update the ball's position if needed
        if (changePos)
        {
            // Move ball to position
            if (spin)
            {
                ball.RandomSpin();
            }
            ball.SetTargetPosition(true, new Vector3(pos.x, ball.transform.position.y, pos.z), snapBall);
        }
        else if (applyFakeForce)
        {
            ball.AddFakeForce(fakeForce, fakeForceDeceleration, fakeForceMinSpeed, true, true, false);
        }

        return (ignoreLostBall);
    }


    /// <summary>
    /// Updates the marked by players. It makes sure the nearest one is first in the list.
    /// </summary>
    /// <returns>The marked by players.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdateMarkedByPlayers(float dt)
    {
        if ((markedByPlayers == null) || (markedByPlayers.Length <= 0))
        {
            return;
        }

        int i, nearestIndex;
        Vector3 vec;
        float nearestDist, compareDist;
        SsPlayer otherPlayer;

        nearestIndex = -1;
        nearestDist = 0.0f;

        // Find the nearest player. If he is not in slot 0 then put him in slot 0.
        for (i = 0; i < markedByPlayers.Length; i++)
        {
            otherPlayer = markedByPlayers[i];
            if (otherPlayer != null)
            {
                vec = transform.position - otherPlayer.transform.position;
                compareDist = vec.sqrMagnitude;
                if ((nearestIndex == -1) || (nearestDist > compareDist))
                {
                    nearestIndex = i;
                    nearestDist = compareDist;
                }
            }
        }

        if ((nearestIndex != -1) && (nearestIndex != 0))
        {
            if (markedByPlayers[0] == null)
            {
                // Slot 0 is empty, just move the player to the slot
                markedByPlayers[0] = markedByPlayers[nearestIndex];
                markedByPlayers[nearestIndex] = null;
            }
            else
            {
                // Slot 0 is Not empty, swap the players
                otherPlayer = markedByPlayers[0];
                markedByPlayers[0] = markedByPlayers[nearestIndex];
                markedByPlayers[nearestIndex] = otherPlayer;
            }
        }
    }


    /// <summary>
    /// Updates the markers.
    /// </summary>
    /// <returns>The markers.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual void UpdateMarkers(float dt)
    {
        bool enable, foundControlBall;

        foundControlBall = false;

        if (markerControl != null)
        {
            enable = false;
            if ((IsUserControlled) && (match.BallPlayer != this))
            {
                enable = true;
            }
            if (markerControl.activeInHierarchy != enable)
            {
                markerControl.SetActive(enable);
            }
        }

        if (markerControlBallDefender != null)
        {
            enable = false;
            if ((IsUserControlled) && (match.BallPlayer == this) && (foundControlBall == false) &&
                (position == positions.defender))
            {
                enable = true;
                foundControlBall = true;
            }
            if (markerControlBallDefender.activeInHierarchy != enable)
            {
                markerControlBallDefender.SetActive(enable);
            }
        }

        if (markerControlBallMidfielder != null)
        {
            enable = false;
            if ((IsUserControlled) && (match.BallPlayer == this) && (foundControlBall == false) &&
                (position == positions.midfielder))
            {
                enable = true;
                foundControlBall = true;
            }
            if (markerControlBallMidfielder.activeInHierarchy != enable)
            {
                markerControlBallMidfielder.SetActive(enable);
            }
        }

        if (markerControlBallForward != null)
        {
            enable = false;
            if ((IsUserControlled) && (match.BallPlayer == this) && (foundControlBall == false) &&
                (position == positions.forward))
            {
                enable = true;
                foundControlBall = true;
            }
            if (markerControlBallForward.activeInHierarchy != enable)
            {
                markerControlBallForward.SetActive(enable);
            }
        }

        if (markerControlBall != null)
        {
            enable = false;
            if ((IsUserControlled) && (match.BallPlayer == this) && (foundControlBall == false))
            {
                enable = true;
                foundControlBall = true;
            }
            if (markerControlBall.activeInHierarchy != enable)
            {
                markerControlBall.SetActive(enable);
            }
        }


        if (markerPass != null)
        {
            enable = false;
            if ((IsUserControlled == false) &&
                ((team.PotentialPassPlayer == this) || (team.PassPlayer == this)))
            {
                enable = true;
            }
            if (markerPass.activeInHierarchy != enable)
            {
                markerPass.SetActive(enable);
            }
        }

        if (markerAiBall != null)
        {
            enable = false;
            if ((IsUserControlled == false) && (match.BallPlayer == this))
            {
                enable = true;
            }
            if (markerAiBall.activeInHierarchy != enable)
            {
                markerAiBall.SetActive(enable);
            }
        }
    }


    /// <summary>
    /// Test if player can collect the loose ball (i.e. ball Not belonging to a player).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The loose ball.</returns>
    /// <param name="dt">Dt.</param>
    protected virtual bool CollectLooseBall(float dt)
    {
        // Use a process of elimination to determine if player can Not collect the ball

        if ((match == null) ||
            (match.State != SsMatch.states.play) ||
            (match.BallPlayer != null) || (ball == null) ||
            (lastTimeHadBall + maxLastTimeHadBall > Time.time) ||
            (cannotTouchBallTimer > Time.time) ||
            (IsHurt) ||
            (delayPass.pending) ||
            (delayShoot.pending))
        {
            return (false);
        }

        if ((state == states.slideTackle) && (skills.canGetBallWhileSliding == false))
        {
            return (false);
        }

        if (IsDiving)
        {
            if (ball.CollideWithBoxCollider(transform, BallDiveBoxRadius, BallDiveBoxHeight) == false)
            {
                return (false);
            }
        }
        else if (ball.CollideWithBoxCollider(transform, BallBoxRadius, BallBoxHeight) == false)
        {
            return (false);
        }


        // The human players and receivers always intercept the ball.
        if ((IsUserControlled) || (this == team.PassPlayer))
        {
            // Will get ball
        }
        else if ((Position == positions.goalkeeper) && (IsDiving) && (saveDive == false))
        {
            // Goalkeeper dives miss
            return (false);
        }



        // Finally, a valid intercept
        match.SetBallPlayer(this);
        otherTeam.MakeNearestPlayerToBallTheControlPlayer(true);

        if (PunchBall() == false)
        {
            if ((Position == SsPlayer.positions.goalkeeper) &&
                (state != states.slideTackle) &&
                (IsDiving == false) &&
                (field.IsInGoalArea(gameObject, team)) &&
                (field.IsInGoalArea(ball.gameObject, team)))
            {
                // Goalkeeper catches and holds the ball, while he is standing/running
                if (ball.WasShotAtGoal == false)
                {
                    ball.Sounds.PlaySfx(ball.Sounds.goalkeeperCatch);
                }

                match.SetGoalkeeperHoldBall(this);
            }
            else if (state == states.slideTackle)
            {
                SetState(states.idle);
            }
        }

        return (true);
    }


#if UNITY_EDITOR
    /// <summary>
    /// DEBUG: Set the game object name so we can easily identify a player in the editor.
    /// </summary>
    /// <returns>The set game object name.</returns>
    protected virtual void DebugSetGameObjectName()
    {
        name = debugDefaultName + "  " + GeUtils.FirstLetterToUpper(Position.ToString())[0];
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Head height
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3.up * headHeight), new Vector3(1.0f, 0.01f, 1.0f));
    }
#endif //UNITY_EDITOR

}
