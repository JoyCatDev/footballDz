using UnityEngine;
using System.Collections;

/// <summary>
/// Match camera.
/// </summary>
public class SsMatchCamera : MonoBehaviour {

	// Enums
	//------
	// Camera states
	public enum states
	{
		loading = 0,					// Scene is busy loading.
		intro,							// Intro sequence.
		followBallOrBallPlayer,			// Follow the ball or the player who has the ball.
		followGoalScorer,				// Follow the player who scored a goal.
	}

	// Play directions
	public enum playDirections
	{
		right = 0,						// Play right (default)
		left,							// Play left
		up,								// Play up
		down,							// Play down

		// DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.
	}


	// Public
	//-------
	[Tooltip("Direction to play, based on camera position relative to the field. This affects the user input axes.")]
	public playDirections playDirection = playDirections.left;

	[Space(10)]
	[Tooltip("Position offset when following a target (e.g. player, ball)")]
	public Vector3 followOffset;

	[Space(10)]
	[Tooltip("Use smooth movement (otherwise snap to target position).")]
	public bool useSmooth = true;
	[Tooltip("Approximately the time it will take to reach the target. A smaller value will reach the target faster.")]
	public float smoothTime = 0.2f;

	[Space(10)]
	[Tooltip("Rotate camera to always look at the target (e.g. player, ball)")]
	public bool lookAtTarget;
	[Tooltip("Rotation speed when looking at target.")]
	public float lookDampening = 4.0f;
	[Tooltip("Restore default rotation at kickoff. Useful when \"Look At Target\" is not set.")]
	public bool restoreRotationAtKickOff;

	[Space(10)]
	[Tooltip("Delay between changing targets (e.g. changing from player to ball).")]
	public float delayChangeTargets = 0.5f;



	// Private
	//--------
	static private SsMatchCamera instance;

	// The following are true when the object being followed has a rigidbody attached.

	static private bool useFixedUpdate = false;				// Use FixedUpdate to update the camera for general movement (otherwise use LateUpdate).
	static private bool useFixedUpdateForBall = true;		// "Use FixedUpdate when following the ball (otherwise use LateUpdate).
	static private bool useFixedUpdateForPlayer = false;	// Use FixedUpdate when following a player (otherwise use LateUpdate).


	private bool dynamicUseFixedUpdate;			// Dynamically change if camera must use FixedUpdate (otherwise use LateUpdate).

	private SsMatch match;						// Reference to the match
	private SsFieldProperties field;			// Reference to the field
	private SsBall ball;						// Reference to the ball

	private bool addedStateAction;				// Added match state change action

	private states state = states.loading;		// Camera state

	private Vector3 velocity = Vector3.zero;	// Velocity for smooth movement

	private Camera cam;

	private GameObject target;					// Target. Use SetTarget() to change the target during a match.
	private float targetTime;					// Time.time when the target was set.

	private Quaternion defaultRot;				// Default rotation



	// Properties
	//-----------
	static public SsMatchCamera Instance
	{
		get { return(instance); }
	}


	public Camera Cam
	{
		get { return(cam); }
	}


	public bool DynamicUseFixedUpdate
	{
		get { return(dynamicUseFixedUpdate); }
	}


	/// <summary>
	/// Get the current target.
	/// </summary>
	/// <value>The target.</value>
	public GameObject Target
	{
		get { return(target); }
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
		dynamicUseFixedUpdate = useFixedUpdate;
		cam = gameObject.GetComponentInChildren<Camera>();

		defaultRot = transform.rotation;
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
		match = null;
		field = null;
		ball = null;
	}


	/// <summary>
	/// Raises the enable event.
	/// </summary>
	public void OnEnable()
	{
		AddCallbacks();
	}
	
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	void OnDisable()
	{
		RemoveCallbacks();
	}
	
	
	/// <summary>
	/// Adds the callbacks.
	/// </summary>
	/// <returns>The callbacks.</returns>
	private void AddCallbacks()
	{
		if ((SsMatch.Instance != null) && (addedStateAction == false))
		{
			SsMatch.Instance.onStateChanged += OnMatchStateChanged;
			addedStateAction = true;
		}
	}
	
	
	/// <summary>
	/// Removes the callbacks.
	/// </summary>
	/// <returns>The callbacks.</returns>
	private void RemoveCallbacks()
	{
		if ((SsMatch.Instance != null) && (addedStateAction))
		{
			SsMatch.Instance.onStateChanged -= OnMatchStateChanged;
			addedStateAction = false;
		}
	}


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		GetReferences();

		if ((target == null) && (ball != null))
		{
			SetTarget(ball.gameObject, true);
		}
	}


	/// <summary>
	/// Get references to objects (e.g. match, field).
	/// </summary>
	/// <returns>The references.</returns>
	void GetReferences()
	{
		if ((SsMatch.Instance == null) || (SsMatch.Instance.IsLoading(true)))
		{
			// Match is busy loading
			Invoke("GetReferences", 0.1f);
			return;
		}

		if (match == null)
		{
			match = SsMatch.Instance;
		}
		if (field == null)
		{
			field = SsFieldProperties.Instance;
		}
		if (ball == null)
		{
			ball = SsBall.Instance;
		}

		AddCallbacks();
	}


	/// <summary>
	/// Raises the match state changed event.
	/// REMINDER: May Not receive the "loading" state change, because the "loading" state set before this game object registers this method.
	/// </summary>
	/// <param name="newState">New state.</param>
	private void OnMatchStateChanged(SsMatch.states newState)
	{
		switch (newState)
		{
		case SsMatch.states.intro:
		{
			SetState(states.intro);
			break;
		}
		case SsMatch.states.kickOff:
		{
			SetState(states.followBallOrBallPlayer);

			if (restoreRotationAtKickOff)
			{
				transform.rotation = defaultRot;
			}

			break;
		}
		case SsMatch.states.goalCelebration:
		{
			if (match.GoalScorer != null)
			{
				SetState(states.followGoalScorer);
			}
			break;
		}
		case SsMatch.states.cornerKick:
		case SsMatch.states.throwIn:
		case SsMatch.states.goalKick:
		{
			UpdateCamera(0.0f, true);
			break;
		}
		} //switch
	}


	/// <summary>
	/// Set the camera's state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="newState">New state.</param>
	public void SetState(states newState)
	{
		state = newState;

		switch (state)
		{
		case states.followBallOrBallPlayer:
		{
			// Snap to position
			UpdateCamera(0.0f, true);
			break;
		}
		case states.followGoalScorer:
		{
			// Snap to position
			UpdateCamera(0.0f, true);
			break;
		}
		} //switch
	}


	/// <summary>
	/// Sets the target.
	/// </summary>
	/// <returns>The target.</returns>
	/// <param name="newTarget">New target.</param>
	/// <param name="snap">Snap.</param>
	public void SetTarget(GameObject newTarget, bool snap = false)
	{
		target = newTarget;
		targetTime = Time.time;
		if (snap)
		{
			velocity = Vector3.zero;
			UpdateCamera(0.0f, true);
		}
	}


	/// <summary>
	/// Late update.
	/// </summary>
	/// <returns>The update.</returns>
	void LateUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		float dt = Time.deltaTime;

		if (dynamicUseFixedUpdate == false)
		{
			UpdateCamera(dt);
		}
	}


	/// <summary>
	/// Fixed update.
	/// </summary>
	/// <returns>The update.</returns>
	void FixedUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		float dt = Time.deltaTime;

		if (dynamicUseFixedUpdate)
		{
			UpdateCamera(dt);
		}
	}


	/// <summary>
	/// Updates the camera.
	/// </summary>
	/// <returns>The camera.</returns>
	/// <param name="dt">Delta time.</param>
	/// <param name="snap">Should movement/rotation snap to the target. If relevant.</param>
	public void UpdateCamera(float dt, bool snap = false)
	{
		if ((instance == null) || (match == null))
		{
			return;
		}

		// Update the state
		switch (state)
		{
		case states.loading:
		{
			break;
		}
		case states.intro:
		{
			break;
		}
		case states.followBallOrBallPlayer:
		{
			UpdateFollowBallOrBallPlayer(dt, snap);
			break;
		}
		case states.followGoalScorer:
		{
			UpdateFollowGoalScorer(dt, snap);
			break;
		}
		} //switch

	}


	/// <summary>
	/// Update: Follow the current target, which is a player or the ball.
	/// </summary>
	/// <returns>The follow target player or ball.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="snal">Snal.</param>
	void UpdateFollowTarget(GameObject target, float dt, bool snap = false)
	{
		if (target == null)
		{
			return;
		}

		Vector3 targetPos;

		targetPos = target.transform.position + followOffset;
		if ((useSmooth) && (snap == false))
		{
			transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime,
			                                        Mathf.Infinity, dt);
		}
		else
		{
			transform.position = targetPos;
		}
		
		if (lookAtTarget)
		{
			Quaternion rotation = Quaternion.LookRotation(target.transform.position - transform.position);
			if (snap)
			{
				transform.rotation = rotation;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, rotation, lookDampening * dt);
			}
		}
	}


	/// <summary>
	/// Update state: follow ball or ball player.
	/// </summary>
	/// <returns>The follow ball or ball player.</returns>
	/// <param name="dt">Delta time.</param>
	/// <param name="snap">Should movement/rotation snap to the target. If relevant.</param>
	void UpdateFollowBallOrBallPlayer(float dt, bool snap = false)
	{
		GameObject newTarget;
		
		if (match.BallPlayer != null)
		{
			// Follow player who has the ball
			newTarget = match.BallPlayer.gameObject;
			dynamicUseFixedUpdate = useFixedUpdateForPlayer;
		}
		else if (ball != null)
		{
			// Follow ball
			newTarget = ball.gameObject;
			dynamicUseFixedUpdate = useFixedUpdateForBall;
		}
		else
		{
			newTarget = null;
			dynamicUseFixedUpdate = useFixedUpdate;
		}

		// Only change the target after delayChangeTargets seconds, in case target is set back to previous target, to avoid jerk
		if ((newTarget != target) && (targetTime + delayChangeTargets <= Time.time))
		{
			SetTarget(newTarget);
		}

		UpdateFollowTarget(newTarget, dt, snap);
	}


	/// <summary>
	/// Updates the follow goal scorer.
	/// </summary>
	/// <returns>The follow goal scorer.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="snap">Snap.</param>
	void UpdateFollowGoalScorer(float dt, bool snap = false)
	{
		GameObject target;
		
		if (match.GoalScorer != null)
		{
			// Follow goal scorer
			target = match.GoalScorer.gameObject;
			dynamicUseFixedUpdate = useFixedUpdateForPlayer;
		}
		else
		{
			target = null;
			dynamicUseFixedUpdate = useFixedUpdate;
		}
		
		UpdateFollowTarget(target, dt, snap);
	}

}
