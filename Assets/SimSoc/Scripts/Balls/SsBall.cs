using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Ball.
/// </summary>
public class SsBall : SsGameObject
{
	// Const / Static
	//---------------
	public const float milesToMetres = 1609.34f;			// Convert miles to metres

	public const float maxSpeedMilesPerHour = 131.0f;		// Max speed of a soccer ball (miles per hour).
	public const float maxSpeedMetersPerSecond = (maxSpeedMilesPerHour * milesToMetres) / (60.0f * 60.0f);

	public const float averageSpeedMilesPerHour = 60.0f;	// Average speed of soccer ball (miles per hour).
	public const float averageSpeedMetersPerSecond = (averageSpeedMilesPerHour * milesToMetres) / (60.0f * 60.0f);

	// If no player has the ball and it moves slower than this speed, then set the ball team to null (i.e. no team owns the ball)
	public const float minSpeedClearTeam = 2.0f;

	// If ball moves slower than this speed then it can be collected by a player (in various conditions)
	public const float slowSpeed = 1.0f;


	// The following variables control the forces applied to a ball when it is kicked, headed, thrown in, etc.
	// Distance to reach max speed (e.g. max pass speed, max shoot speed). Distance is from start pos to target pos. Value was 
	// derived via testing. (Smaller = faster)
	public const float distanceToMaxSpeed = 50.0f;
	public const float defaultMaxHeight = 30.0f;			// Default max height (replaced by value from Global Settings)
	public const float minForceSpeed = 8.0f;				// Min speed when a force is needed, so ball at least has some movement
	public const float maxShootSpeed = maxSpeedMetersPerSecond;
	public const float maxPassSpeed = averageSpeedMetersPerSecond;
	public const float maxHeaderSpeed = averageSpeedMetersPerSecond;
	public const float maxThrowSpeed = averageSpeedMetersPerSecond;

	public const float dribbleFriction = 30.0f;



	// Public
	//-------
	[Tooltip("Unique ID. It is used to identify the resource. Do Not change this after the game has been released.")]
	public string id;

	[Space(10)]
	[Tooltip("Name to display on UI.")]
	public string displayName;

	[Space(10)]
	[Tooltip("Scale the forces applied to the ball. Smaller = heavier (e.g. metal ball). Larger = lighter (e.g. foam ball).")]
	public float scaleForces = 1.0f;

	[Tooltip("Radius. If zero then it will automatically be set, based on the collider size.")]
	public float radius;


	// Mesh
	[Header("Mesh")]
	[Tooltip("Mesh prefab to clone and attach to the ball game object. (Optional. Not needed if ball already has a mesh child.)")]
	public GameObject meshPrefab;
	
	[Tooltip("Mesh position offset relative to the ball game object. The ball's pivot should be positioned in its centre. (Optional. Not needed if ball already has a mesh child.)")]
	public Vector3 meshOffsetPosition;
	
	[Tooltip("Mesh rotation offset relative to the ball game object. (Optional. Not needed if ball already has a mesh child.)")]
	public Vector3 meshOffsetRotation;


	// Collission
	[Header("Collision")]
	[Tooltip("Collder used for testing if the ball is visible on screen.")]
	public SphereCollider visibilityCollider;


	[Header("Shadow")]
	[Tooltip("Fake shadow to use.")]
	public SsFakeShadow fakeShadowPrefab;



	// Private
	//--------
	static private SsBall instance;

	protected Rigidbody rb;							// Rigidbody

	protected SsBallSounds sounds;					// Ball sounds
	protected SsBallParticles particles;			// Ball particles

	protected bool wasShotAtGoal;					// Was the ball shot at goal when last kicked?

	// Radius AI uses when they run to the ball.
	protected float aiRadiusMin;					// Min radius to reach
	protected float aiRadiusMax;					// Max radius to reach

	protected float maxHeight;						// Try to keep ball lower than this height, relative to ground (may pass it slightly sometimes)
	protected float trackMaxHeight;					// Track max height during current air movement

	protected CInterceptCalculator2d interceptCalculator = new CInterceptCalculator2d();	// For calculating if players can intercept ball.

	protected bool gotoTargetPos;					// Move to a target position? (For fake physics that need to be controlled precisely.)
	protected Vector3 targetPos;					// Target position to move to.
	protected bool gotoTargetPosSnap;				// Should snap to position in next update?

	protected bool hasFakeForce;					// Is there a fake force acting on the ball, over multiple frames?
	protected Vector3 fakeForce;					// The fake force.
	protected float fakeForceDeceleration;			// Deceleration to slow down the fake force.
	protected float fakeForceMinSpeed;				// Min speed of the fake force

	protected float inPlayTime;						// Timer to keep track how long ball has been in the play area
	protected Vector3 lastInPlayPosition;			// Position of when the ball was last in the play area
	protected Vector3 ballOutPosition;				// Position where the ball went out of the field

	protected Transform defaultParent;
	protected Vector3 defaultLocalScale = Vector3.one;

	protected Vector3 kickStartPosition;			// The position from where the ball was kicked, passed, shot at goal, etc.

	protected Vector3 prevVelocity;					// Velocity in previous update

	protected float bounceGroundSfxTime;			// Time when bounce on ground sfx was last played
	protected float trackBounceSfxMaxHeight;		// Track bounce max height for bounce sfx

	/// <summary>
	/// The Rigidbody's default collision detection mode.
	/// </summary>
	protected CollisionDetectionMode _defualtCollisionDetectionMode;


	// Properties
	//-----------
	static public SsBall Instance
	{
		get { return(instance); }
	}


	public Rigidbody Rb
	{
		get { return(rb); }
	}


	public SsBallSounds Sounds
	{
		get { return(sounds); }
	}


	public Vector3 MoveVec
	{
		get
		{
			if (rb != null)
			{
				return (rb.velocity);
			}
			return(Vector3.zero);
		}
	}


	public float Speed
	{
		get
		{
			if (rb != null)
			{
				return (rb.velocity.magnitude);
			}
			return (0.0f);
		}
	}


	public bool WasShotAtGoal
	{
		get { return(wasShotAtGoal); }
		set
		{
			wasShotAtGoal = value;

			if (wasShotAtGoal)
			{
				particles.Play(particles.shotAtGoal);
			}
			else
			{
				particles.Stop(particles.shotAtGoal);
			}
		}
	}


	public float AiRadiusMin
	{
		get { return(aiRadiusMin); }
	}


	public float AiRadiusMax
	{
		get { return(aiRadiusMax); }
	}


	/// <summary>
	/// Track max height during current air movement.
	/// </summary>
	/// <value>The height of the track max.</value>
	public float TrackMaxHeight
	{
		get { return(trackMaxHeight); }
	}


	public Vector3 BallOutPosition
	{
		get { return(ballOutPosition); }
	}


	public Vector3 KickStartPosition
	{
		get { return(kickStartPosition); }
	}

	//Is there a fake force acting on the ball, over multiple frames?
	public bool HasFakeForce
	{
		get { return(hasFakeForce); }
	}

	// The fake force.
	public Vector3 FakeForce
	{
		get { return(fakeForce); }
		set
		{
			fakeForce = value;
		}
	}



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Awake()
	{
		base.Awake();

		instance = this;

		rb = gameObject.GetComponent<Rigidbody>();
		if (rb != null)
		{
			_defualtCollisionDetectionMode = rb.collisionDetectionMode;
		}

		sounds = gameObject.GetComponent<SsBallSounds>();
		if (sounds == null)
		{
			sounds = gameObject.AddComponent<SsBallSounds>();
		}

		particles = gameObject.GetComponent<SsBallParticles>();
		if (particles == null)
		{
			particles = gameObject.AddComponent<SsBallParticles>();
		}


		// Mesh
		if (meshPrefab != null)
		{
			GameObject go = (GameObject)Instantiate(meshPrefab);
			if (go != null)
			{
				go.transform.parent = transform;
				go.transform.localPosition = meshOffsetPosition;
				go.transform.localRotation = Quaternion.Euler(meshOffsetRotation);
			}
		}

		if (visibilityCollider == null)
		{
			visibilityCollider = gameObject.GetComponentInChildren<SphereCollider>();
		}

		// Shadow
		if (fakeShadowPrefab != null)
		{
			SsFakeShadow shadow = (SsFakeShadow)Instantiate(fakeShadowPrefab);
			if (shadow != null)
			{
				shadow.transform.parent = transform;
			}
		}

		defaultParent = transform.parent;
		defaultLocalScale = transform.localScale;

		CalcRadius();


#if UNITY_EDITOR
		// Editor warnings
		//----------------
		if (rb == null)
		{
			Debug.LogError("ERROR: The ball does not have a rigid body: " + name);
		}
		else
		{
			if (rb.drag != 0)
			{
				Debug.LogWarning("WARNING: Ball drag is not zero. (" + name + ") It will affect pass/shoot accuracy. Try changing Scale Forces instead.");
			}
		}

		if (gameObject.GetComponentInChildren<Collider>() == null)
		{
			Debug.LogError("ERROR: The ball does not have a collider: " + name);
		}
		if (visibilityCollider == null)
		{
			Debug.LogError("ERROR: The ball does not have a Visibility Collider set: " + name);
		}
#endif //UNITY_EDITOR
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		instance = null;
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
	}


	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		if ((SsMatchCamera.Instance != null) && (SsMatchCamera.Instance.Target == null))
		{
			SsMatchCamera.Instance.SetTarget(gameObject, true);
		}

		CalcVisibleRect(visibilityCollider);


		PositionAtCentre();
	}


	/// <summary>
	/// Reset this instance.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public override void ResetMe()
	{
		base.ResetMe();

		WasShotAtGoal = false;
		inPlayTime = 0.0f;
		lastInPlayPosition = transform.position;

		if (SsMatchSettings.Instance != null)
		{
			maxHeight = SsMatchSettings.Instance.maxBallHeight;
		}
		else
		{
			maxHeight = defaultMaxHeight;
		}

		prevVelocity = Vector3.zero;

		bounceGroundSfxTime = 0.0f;
		trackBounceSfxMaxHeight = 0.0f;

		DetachFromObject(true);

		StopMoving();
	}


	/// <summary>
	/// Called when the balls's position has been set (e.g. start of match, corner kick, half time, etc.).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void OnPositionSet()
	{
		UpdateZoneAndVisibility(0.0f, true, null, field);
	}


	/// <summary>
	/// Calculates the ball's radius.
	/// </summary>
	/// <returns>The radius.</returns>
	public virtual void CalcRadius()
	{
		float maxScale = Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
		SphereCollider sc = gameObject.GetComponentInChildren<SphereCollider>();
		BoxCollider bc;
		float newRadius = radius;

		if (sc != null)
		{
			newRadius = sc.radius * maxScale;
		}
		else
		{
			bc = gameObject.GetComponentInChildren<BoxCollider>();
			if (bc != null)
			{
				newRadius = Mathf.Max(bc.bounds.max.x, Mathf.Max(bc.bounds.max.y, bc.bounds.max.z)) * maxScale;
			}
		}

		if (radius <= 0.0f)
		{
			radius = newRadius;
		}

		aiRadiusMin = radius;
		aiRadiusMax = radius * 3.0f;
	}


	/// <summary>
	/// Position at the centre mark.
	/// </summary>
	/// <returns>The at centre.</returns>
	public virtual void PositionAtCentre()
	{
		if (field != null)
		{
			transform.position = field.CentreMark + (Vector3.up * radius);

			// Clear velocity
			StopMoving();

			OnPositionSet();
		}
	}
	
	/// <summary>
	/// Enable/disable the Rigidbody's physics.
	/// </summary>
	public void EnablePhysics(bool enable, bool clearVelocities = true)
	{
		if (rb == null)
		{
			return;
		}

		if (enable)
		{
			rb.isKinematic = false;
			rb.collisionDetectionMode = _defualtCollisionDetectionMode;
		}
		else
		{
			rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			rb.isKinematic = true;
		}

		if (clearVelocities)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}


	/// <summary>
	/// Attach the ball to an object (e.g. a goalkeeper's hand).
	/// </summary>
	/// <returns>The to object.</returns>
	/// <param name="attachTo">Object to attach to.</param>
	/// <param name="offsetPos">Offset position.</param>
	/// <param name="radiusVec">The direction in which to adjust the position by the ball's radius.</param>
	public virtual void AttachToObject(GameObject attachTo, Vector3 offsetPos, Vector3 radiusVec)
	{
		if (attachTo == null)
		{
			return;
		}

		transform.parent = attachTo.transform;
		transform.localRotation = Quaternion.identity;

		float max = Mathf.Max(transform.localScale.x, Mathf.Max(transform.localScale.y, transform.localScale.z));
		transform.localPosition = offsetPos + (radiusVec * (radius * max));

		if (rb != null)
		{
			// Disable the rigidbody's physics
			EnablePhysics(false);
		}
	}


	/// <summary>
	/// Detach the ball from an object.
	/// </summary>
	/// <returns>The from object.</returns>
	/// <param name="clearVelocity">Clear the rigidbody's velocity and angular velocity.</param>
	public virtual void DetachFromObject(bool clearVelocity)
	{
		if (transform.parent == defaultParent)
		{
			return;
		}

		transform.parent = defaultParent;
		transform.localScale = defaultLocalScale;

		if (rb != null)
		{
			EnablePhysics(true, clearVelocity);
		}
	}


	/// <summary>
	/// Update is called once per frame.
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

		// If no ball player and ball moves slow, then clear the ball team (i.e. no team owns the ball any more)
		if ((match != null) && (match.BallPlayer == null) && (match.BallTeam != null) && 
		    (Speed < minSpeedClearTeam))
		{
			match.SetBallTeam(null);
		}

		if (UpdateCheckGoal() == false)
		{
			UpdateCheckBallOut(dt);
		}
	}


	/// <summary>
	/// Check if a goal was scored.
	/// </summary>
	/// <returns>True if goal.</returns>
	protected virtual bool UpdateCheckGoal()
	{
		if (match.State == SsMatch.states.play)
		{
			// Test if ball in the goal posts
			if (field.IsBallInGoalpost(match.LeftTeam))
			{
				match.OnScoreGoal(match.RightTeam);
				return (true);
			}
			else if (field.IsBallInGoalpost(match.RightTeam))
			{
				match.OnScoreGoal(match.LeftTeam);
				return (true);
			}
		}

		return (false);
	}


	/// <summary>
	/// Check if the ball went out.
	/// </summary>
	/// <returns>The check ball out.</returns>
	/// <param name="dt">Dt.</param>
	protected virtual bool UpdateCheckBallOut(float dt)
	{
		if (match.State != SsMatch.states.play)
		{
			return (false);
		}


		// Test if ball goes out
		if ((field.IsBallInPlayArea(IsMoving(true)) == false) && 
		    (field.IsInGoalposts(gameObject, match.LeftTeam, 0.0f) == false) && 
		    (field.IsInGoalposts(gameObject, match.RightTeam, 0.0f) == false))
		{
			// Must be in play for at least some time
			if (inPlayTime > 0.0f)
			{
				SsMatch.states nextState = SsMatch.states.preThrowIn;
				Vector2 pos;
				Bounds bounds = field.PlayArea;
				Rect rect = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
				SsPlayer lastTouchPlayer = match.LastBallPlayer;	// Player who touched the ball last
				bool isGoalKick = false;
				SsTeam team = null;

				// Get the position where the ball went out
				if (GeUtils.LineRectangleIntersect(new Vector2(lastInPlayPosition.x, lastInPlayPosition.z), 
				                                   new Vector2(transform.position.x, transform.position.z), 
				                                   rect,
				                                   out pos))
				{
					ballOutPosition = new Vector3(pos.x, 
					                              field.GroundY + SsMarkersManager.offsetY, 
					                              pos.y);
				}
				else
				{
					ballOutPosition = new Vector3(transform.position.x, 
					                              field.GroundY + SsMarkersManager.offsetY, 
					                              transform.position.z);
				}


				// Test if the ball went out the sides past the goal posts
				if ((transform.position.x < field.PlayArea.min.x) || 
				    (transform.position.x > field.PlayArea.max.x))
				{
					nextState = SsMatch.states.preCornerKick;
				}
				else
				{
					nextState = SsMatch.states.preThrowIn;
				}


				// Determine which team must get the ball, and what the next state should be
				if (lastTouchPlayer == null)
				{
					// NOTE: This should never happen, because a player must have touched the ball before it went out
					lastTouchPlayer = match.BallPlayer;
					if (lastTouchPlayer == null)
					{
						if (nextState == SsMatch.states.preCornerKick)
						{
							// Give the ball to the goalkeeper
							isGoalKick = true;
							if (ballOutPosition.x < field.CentreMark.x)
							{
								team = match.LeftTeam;
							}
							else
							{
								team = match.RightTeam;
							}
						}
						else if (match.KickoffTeam != null)
						{
							// Give the ball to the kickoff team
							team = match.KickoffTeam;
						}
						else
						{
							// Give the ball to a random team
							if (Random.Range(0, 100) < 50)
							{
								team = match.LeftTeam;
							}
							else
							{
								team = match.RightTeam;
							}
						}

						lastTouchPlayer = team.OtherTeam.GetFirstPlayer();
					}
				}

				if ((team == null) && (lastTouchPlayer != null))
				{
					// Test if it is a goal kick
					if (nextState == SsMatch.states.preCornerKick)
					{
						if (((ballOutPosition.x < field.CentreMark.x) && (lastTouchPlayer.Team == match.RightTeam)) || 
						    ((ballOutPosition.x >= field.CentreMark.x) && (lastTouchPlayer.Team == match.LeftTeam)))
						{
							isGoalKick = true;
						}
					}
					team = lastTouchPlayer.OtherTeam;
				}

				if (isGoalKick)
				{
					nextState = SsMatch.states.preGoalKick;
				}

				match.ThrowInTeam = team;
				match.SetState(nextState);

				field.PlaySfx(field.matchPrefabs.sfxBallOut);

				// Finally, position and show the out marker
				if (SsMarkersManager.Instance != null)
				{
					SsMarkersManager.Instance.SetOutPosition(ballOutPosition);
				}

				return (true);
			}
			
			inPlayTime = 0.0f;
		}
		else
		{
			inPlayTime += dt;
			if (field.IsBallInPlayArea(false))
			{
				lastInPlayPosition = transform.position;
			}
		}

		return (false);
	}


	/// <summary>
	/// Fixed update.
	/// </summary>
	/// <returns>The update.</returns>
	public override void FixedUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		base.FixedUpdate();

		float dt = Time.deltaTime;

		PreUpdateBall(dt);
		UpdateBall(dt);
		PostUpdateBall(dt);

	}


	/// <summary>
	/// Pre-update the ball. Called before the UpdateBall method.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The update ball.</returns>
	/// <param name="dt">Dt.</param>
	protected virtual void PreUpdateBall(float dt)
	{
	}


	/// <summary>
	/// Post-update ball. Called after the UpdateBall method.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The update ball.</returns>
	/// <param name="dt">Dt.</param>
	protected virtual void PostUpdateBall(float dt)
	{
		// Update zone in FixedUpdate, because ball uses a rigidbody for movement
		UpdateZoneAndVisibility(dt, false, null, field);
		
		
		if (trackMaxHeight < transform.position.y)
		{
			trackMaxHeight = transform.position.y;
		}
		if (trackBounceSfxMaxHeight < transform.position.y)
		{
			trackBounceSfxMaxHeight = transform.position.y;
		}

		prevVelocity = rb.velocity;
	}


	/// <summary>
	/// Update the ball.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The ball.</returns>
	/// <param name="dt">Dt.</param>
	protected virtual void UpdateBall(float dt)
	{
		if (rb != null)
		{
			float speed;

			if (gotoTargetPos)
			{
				// Move to a target position
				if (match.BallPlayer != null)
				{
					// Only used when there is a ball player
					Vector3 pos = new Vector3(targetPos.x, rb.position.y, targetPos.z);
					speed = Mathf.Max(10.0f, match.BallPlayer.Skills.runSpeed + 6.0f);
					
					if (gotoTargetPosSnap)
					{
						gotoTargetPosSnap = false;
						rb.MovePosition(pos);
					}
					else
					{
						rb.MovePosition(Vector3.MoveTowards(rb.position, pos, speed * dt));
					}
					
					if (rb.position == pos)
					{
						gotoTargetPos = false;
					}
					
					#if UNITY_EDITOR
					// DEBUG
					if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showBallTargetPos))
					{
						Debug.DrawLine(rb.position, pos);
					}
					#endif //UNITY_EDITOR
				}
				else
				{
					// Only move to a target position when there is a ball player
					gotoTargetPos = false;
				}
			}

			if (hasFakeForce)
			{
				// Move via a fake force
				rb.MovePosition(rb.position + (fakeForce * dt));

				if (fakeForceDeceleration != 0.0f)
				{
					speed = fakeForce.magnitude;

					// If player stopped moving then reduce speed faster
					if ((match.BallPlayer != null) && (match.BallPlayer.IsMoving() == false))
					{
						speed = Mathf.MoveTowards(speed, fakeForceMinSpeed, fakeForceDeceleration * 3.0f * dt);
					}
					else
					{
						speed = Mathf.MoveTowards(speed, fakeForceMinSpeed, fakeForceDeceleration * dt);
					}

					if (speed <= 0.0f)
					{
						hasFakeForce = false;
					}
					else
					{
						fakeForce = fakeForce.normalized * speed;
					}
				}
			}


			// Check if ball bounces on ground
			if (bounceGroundSfxTime <= Time.time)
			{
				if ((rb.velocity.x >= 0.0) && (prevVelocity.y < 0.0) && 
				    (rb.position.y <= field.GroundY + radius) && 
				    (trackBounceSfxMaxHeight >= field.GroundY + sounds.minHeightForBounceSfx))
				{
					bounceGroundSfxTime = Time.time + 0.2f;
					trackBounceSfxMaxHeight = transform.position.y;
					sounds.PlaySfx(sounds.bounceGround);
				}
			}
		}
	}



	/// <summary>
	/// Clamps the horizontal force.
	/// </summary>
	/// <returns>The horizontal force.</returns>
	/// <param name="force">Force.</param>
	public virtual Vector3 ClampHorizontalForce(Vector3 force)
	{
		float distance;
		force.y = 0.0f;

		distance = force.magnitude;
		if (distance > maxSpeedMetersPerSecond)
		{
			force = force.normalized * maxSpeedMetersPerSecond;
		}

		return (force);
	}


	/// <summary>
	/// Test if the ball is moving.
	/// </summary>
	/// <returns><c>true</c> if this instance is moving the specified horizontalOnly; otherwise, <c>false</c>.</returns>
	/// <param name="horizontalOnly">Horizontal only (e.g. ignore gravity, upward movement).</param>
	public virtual bool IsMoving(bool horizontalOnly = false)
	{
		if (rb != null)
		{
			if (horizontalOnly == false)
			{
				return ((rb.velocity.x != 0.0f) || (rb.velocity.y != 0.0f) || (rb.velocity.z != 0.0f));
			}
			return ((rb.velocity.x != 0.0f) || (rb.velocity.z != 0.0f));
		}
		return (false);
	}


	/// <summary>
	/// Stop the ball's movement.
	/// </summary>
	/// <returns>The moving.</returns>
	public virtual void StopMoving()
	{
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
		gotoTargetPos = false;
		targetPos = Vector3.zero;
		WasShotAtGoal = false;
		StopFakeForce();
	}


	/// <summary>
	/// Sets the target position to move to.
	/// IMPORTANT: This is only used when there is a ball player (e.g. for dribbling).
	/// </summary>
	/// <returns>The target position.</returns>
	/// <param name="enabled">Enabled.</param>
	/// <param name="newTargetPos">New target position.</param>
	public virtual void SetTargetPosition(bool enabled, Vector3 newTargetPos, bool snap = false)
	{
		gotoTargetPos = enabled;
		targetPos = newTargetPos;
		gotoTargetPosSnap = snap;
	}


	/// <summary>
	/// Add a force to the ball. It adds a ForceMode.VelocityChange force. (In this mode, the unit of the force parameter is applied to the 
	/// rigidbody as distance/time.)
	/// </summary>
	/// <returns>The force.</returns>
	/// <param name="force">Force.</param>
	/// <param name="clearVelocity">Clear velocity.</param>
	/// <param name="randomSpin">Add a random spin to the ball.</param>
	/// <param name="applyScaleForces">Apply the "scaleForces" to the force.</param>
	/// <param name="clearMoveToTarget">Clear the move to target.</param>
	public virtual void AddForce(Vector3 force, bool clearVelocity = true, bool randomSpin = true, 
	                             bool applyScaleForces = true, bool clearMoveToTarget = true)
	{
		if (clearMoveToTarget)
		{
			SetTargetPosition(false, Vector3.zero);
		}

		hasFakeForce = false;

		trackMaxHeight = transform.position.y;
		trackBounceSfxMaxHeight = transform.position.y;

		if (rb != null)
		{
			Vector3 finalForce;

			if (clearVelocity)
			{
				rb.velocity = Vector3.zero;
			}

			if (applyScaleForces)
			{
				finalForce = force * scaleForces;
			}
			else
			{
				finalForce = force;
			}

			if (finalForce != Vector3.zero)
			{
				rb.AddForce(finalForce, ForceMode.VelocityChange);
			}

			if (randomSpin)
			{
				RandomSpin();
			}
		}
	}


	/// <summary>
	/// Add a fake force. A fake force is applied to the ball each frame until the force decelerates to zero.
	/// </summary>
	/// <returns>The fake force.</returns>
	/// <param name="fakeForce">Fake force.</param>
	/// <param name="fakeForceDeceleration">Friction that will be used to slow down the force over time.</param>
	public virtual void AddFakeForce(Vector3 force, float deceleration, float minSpeed = 0.0f, 
	                                 bool clearVelocity = true, 
	                                 bool randomSpin = true, 
	                                 bool applyScaleForces = true, bool clearMoveToTarget = true,
	                                 bool spinInForceDirection = false, float spinScale = 1.0f)
	{
		if (clearMoveToTarget)
		{
			SetTargetPosition(false, Vector3.zero);
		}
		
		trackMaxHeight = transform.position.y;
		trackBounceSfxMaxHeight = transform.position.y;
		
		if (rb != null)
		{
			Vector3 finalForce;
			
			if (clearVelocity)
			{
				rb.velocity = Vector3.zero;
			}
			
			if (applyScaleForces)
			{
				finalForce = force * scaleForces;
			}
			else
			{
				finalForce = force;
			}
			
			hasFakeForce = true;
			fakeForce = finalForce;
			fakeForceDeceleration = Mathf.Abs(deceleration);
			fakeForceMinSpeed = minSpeed;

			if (randomSpin)
			{
				RandomSpin();
			}
			else if (spinInForceDirection)
			{
				// Apply angular force in direction of the fake force
				// REMINDER: The angular force is the rotation around each axis. So we set each to affect how the ball will roll
				// 	on the ground: x-axis rolls in z direction, y-axis affects nothing, z-axis rolls in opposite x direction
				rb.angularVelocity = new Vector3(finalForce.z * spinScale, 
				                                 0.0f, 
				                                 -finalForce.x * spinScale);
			}
		}
	}


	/// <summary>
	/// Stop the current fake force on the ball.
	/// </summary>
	/// <returns>The fake force.</returns>
	public virtual void StopFakeForce()
	{
		hasFakeForce = false;
	}


	/// <summary>
	/// Shoot the ball to a target position.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="player">Player who kicked it.</param>
	/// <param name="targetPos">Target position.</param>
	/// <param name="shotAtGoal">Indicates if the ball was shot at goal.</param>
	public virtual void Shoot(SsPlayer player, Vector3 targetPos, bool shotAtGoal, AudioClip clip = null)
	{
		Vector3 force;

		// Match stuff
		//------------
		match.SetBallTeam(null);
		match.SetBallPlayer(null);


		// Ball forces and movement
		//-------------------------
		DetachFromObject(true);
		SetTargetPosition(false, Vector3.zero);
		kickStartPosition = transform.position;

		force = CalcForce(transform.position, targetPos, minForceSpeed, maxShootSpeed, player.Skills.maxShootSpeed);

		WasShotAtGoal = shotAtGoal;

		AddForce(force);


		// Player
		//-------
		if (player != null)
		{
			// NOTE: Most of the code was moved to the player's Shoot method.

			player.DelayPass.pending = false;
			player.DelayShoot.pending = false;
		}

		sounds.PlaySfx(clip);

		field.PlaySfx(field.matchPrefabs.sfxShootBall);
	}


	/// <summary>
	/// Pass the ball to a target player or position.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="player">Player.</param>
	/// <param name="toPlayer">To player.</param>
	/// <param name="targetPos">Target position. (Usually the player's position, but might be slightly infront or next to the player.)</param>
	/// <param name="clearBallTeam">Indicates if team who owns ball must be cleared, so no one owns ball.</param>
	public virtual void Pass(SsPlayer player, SsPlayer toPlayer, Vector3 targetPos, 
	                         bool clearBallTeam = false, AudioClip clip = null)
	{
		Vector3 force;

		// Match stuff
		//------------
		match.SetBallPlayer(null);
		if ((toPlayer != null) && (toPlayer != toPlayer.Team.PassPlayer))
		{
			toPlayer.Team.SetPassPlayer(toPlayer, true);
		}
		if (clearBallTeam)
		{
			match.SetBallTeam(null);
		}
		match.StopBallPredators(null);



		// Ball forces and movement
		//-------------------------
		DetachFromObject(true);
		SetTargetPosition(false, Vector3.zero);
		kickStartPosition = transform.position;

		force = CalcForce(transform.position, targetPos, minForceSpeed, maxPassSpeed, player.Skills.maxPassSpeed);
		
		WasShotAtGoal = false;

		AddForce(force);


		// Player
		//-------
		if (player != null)
		{
			// NOTE: Most of the code was moved to the player's Pass method.
			
			player.DelayPass.pending = false;
			player.DelayShoot.pending = false;

			// If other team is human controlled and there is no control player, then make nearest the control player
			if ((player.OtherTeam.IsUserControlled) && (player.OtherTeam.ControlPlayer == null))
			{
				player.OtherTeam.MakeNearestPlayerToBallTheControlPlayer(true);
			}
		}

		sounds.PlaySfx(clip);

		// Check if the game state must change
		match.OnBallPass();
	}


	/// <summary>
	/// Give the ball a random spin.
	/// </summary>
	/// <returns>The spin.</returns>
	/// <param name="minAngle">Minimum angle.</param>
	/// <param name="maxAngle">Max angle.</param>
	public virtual void RandomSpin(float minAngle = 0.0f, float maxAngle = 360.0f)
	{
		if (rb != null)
		{
			rb.angularVelocity = new Vector3(Random.Range(minAngle, maxAngle),
			                                 Random.Range(minAngle, maxAngle),
			                                 Random.Range(minAngle, maxAngle));
		}
	}


	/// <summary>
	/// Calc the fake force needed to move to the position, starting with the velocity within the specified time.
	/// A fake force is applied to the ball each frame until the force decelerates to zero.
	/// </summary>
	/// <returns>The fake force.</returns>
	/// <param name="position">Position to move to.</param>
	/// <param name="velocity">Initial velocity.</param>
	/// <param name="time">Time it takes to move to the position.</param>
	/// <param name="deceleration">Get the deceleration needed to slow down the force.</param>
	public virtual Vector3 CalcFakeForce(Vector3 position, float velocityStart, float velocityEnd, 
	                             out float time, out float deceleration)
	{
		// Movement calculations:
		// http://www.physicsclassroom.com/class/vectors/Lesson-2/Horizontally-Launched-Projectiles-Problem-Solving
		// Vertical arc (horizontal is same, just replace y with x):
		//	y = vert. displacement
		//	ay = vert. acceleration
		//	t = time
		//	vfy = final vert. velocity
		//	viy = initial vert. velocity
		//		
		//	y = (viy * t) + (0.5 * ay * square(t))
		//		
		//	vfy = viy + (ay * t)
		//		
		//	square(vfy) = square(viy) + (2 * ay * y)

		Vector3 vec = position - transform.position;
		float distance = vec.magnitude;

		deceleration = 0.0f;
		time = 0.0f;

		if (distance == 0.0f)
		{
			return (Vector3.zero);
		}

		// square(vfy) = square(viy) + (2 * ay * y)
		// (velocityEnd * velocityEnd) = (velocityStart * velocityStart) + (2.0f * deceleration * distance)
		// (2.0f * deceleration * distance) = (velocityEnd * velocityEnd) - (velocityStart * velocityStart)
		deceleration = ((velocityEnd * velocityEnd) - (velocityStart * velocityStart)) / (2.0f * distance);

		// vfy = viy + (ay * t)
		// velocityEnd = velocityStart + (deceleration * time)
		// (deceleration * time) = velocityEnd - velocityStart
		time = (velocityEnd - velocityStart) / deceleration;

		return (vec.normalized * velocityStart);
	}


	/// <summary>
	/// Calc the force needed to move the ball from startPos to endPos. It tries to make it move in a nice arc, based on gravity.
	/// </summary>
	/// <returns>The force.</returns>
	/// <param name="startPos">Start position.</param>
	/// <param name="endPos">End position.</param>
	/// <param name="horizontalSpeedMin">Horizontal speed min.</param>
	/// <param name="horizontalSpeedMax">Horizontal speed max.</param>
	/// <param name="playerHorizontalSpeedMax">Player's horizontal speed max.</param>
	public virtual Vector3 CalcForce(Vector3 startPos, Vector3 endPos, 
	                                 float horizontalSpeedMin, float horizontalSpeedMax,
	                                 float playerHorizontalSpeedMax, 
	                                 bool allowRandomness = false)
	{
		// Movement/arc calculations:
		// http://www.physicsclassroom.com/class/vectors/Lesson-2/Horizontally-Launched-Projectiles-Problem-Solving
		// Vertical arc (horizontal is same, just replace y with x):
		//	y = vert. displacement
		//	ay = vert. acceleration (in this case, gravity)
		//	t = time
		//	vfy = final vert. velocity
		//	viy = initial vert. velocity
		//		
		//	y = (viy * t) + (0.5 * ay * square(t))
		//		
		//	vfy = viy + (ay * t)
		//		
		//	square(vfy) = square(viy) + (2 * ay * y)
		
		
		Vector3 force = Vector3.zero;
		
		// Is there no horizontal movement?
		if ((startPos.x == endPos.x) && (startPos.z == endPos.z))
		{
			if (endPos.y > startPos.y)
			{
				// Calc upward vertical force needed
				force.y = Mathf.Clamp(GeUtils.CalcVerticalVelocity(endPos.y - startPos.y), horizontalSpeedMin, horizontalSpeedMax);
			}
			return (force);
		}

		Vector3 vec = endPos - startPos;
		Vector3 hVec = new Vector3(vec.x, 0.0f, vec.z);		// Horizontal vector
		float horizontalDistance = hVec.magnitude;			// Horizontal distance between startPos and endPos.
		float horizontalSpeed;								// Horizontal speed.
		float horizontalTime;								// Time ball will move horizontally.
		float magicSpeed;									// Magic speed (see below).
		float estimatedHeight, maxSpeed;



		// Step 1: Calc speed in ideal conditions (i.e. not limit to distance, so ball always reaches target pos)
		//--------
		// If player's max speed is more than the default, then use the player's max speed.
		// This ensures kicks feel faster when the player has a max speed.
		// (And if the player's speed is less than default, then use the default, so kick does not feel too slow.)
		maxSpeed = Mathf.Max(horizontalSpeedMax, playerHorizontalSpeedMax);

		// The speed it will take to travel the horizontal distance over distanceToMaxSpeed seconds.
		// It ranges from 0 to 1 when less than/equal to distanceToMaxSpeed.
		magicSpeed = horizontalDistance / distanceToMaxSpeed;

		horizontalSpeed = Mathf.Lerp(horizontalSpeedMin, maxSpeed, magicSpeed);

		horizontalTime = horizontalDistance / horizontalSpeed;


		if (allowRandomness)
		{
			// Randomly slow down the time to make the ball go higher
			if (Random.Range(0, 100) < 20)
			{
				horizontalTime *= Random.Range(1.0f, 1.4f);
				if (Random.Range(0, 100) < 20)
				{
					// Randomly increase distance
					horizontalDistance += Random.Range(0.0f, 1.0f);
				}
				horizontalSpeed = horizontalDistance / horizontalTime;
			}
			else if (Random.Range(0, 100) < 20)
			{
				// Randomly increase distance
				horizontalDistance += Random.Range(0.0f, 1.0f);
				horizontalTime = horizontalDistance / horizontalSpeed;
			}
		}


		// Step 2: Now that we have speed and time, check if the speed is more than the max speed
		//--------
		if (horizontalSpeed > playerHorizontalSpeedMax)
		{
			// We need to limit the speed to the player's max speed.
			// But we want to maintain the same feel and time (based on Step 1's calcs above)

			// Calc the max distance that will be reached by the player's max speed

			// Reversed equation to find the distance:
			// horizontalDistance = magicSpeed * distanceToMaxSpeed;

			maxSpeed = playerHorizontalSpeedMax;
			magicSpeed = GeUtils.Lerp(maxSpeed, horizontalSpeedMin, horizontalSpeedMax, 0.0f, 1.0f);
			horizontalDistance = magicSpeed * distanceToMaxSpeed;
			horizontalSpeed = maxSpeed;
			horizontalTime = horizontalDistance / horizontalSpeed;
		}


		// Step 3: Finally, we can calc the force
		//--------
		force = hVec.normalized * horizontalSpeed;
		
		// Ideally, the ball's arc will reach the crest at half the time.
		// At the arc's crest the velocity will be zero, which we can use to calc the initial vert velocity needed to reach the height:
		//	vfy = viy + (ay * t)
		//	viy = vfy - (ay * t)
		force.y = 0.0f - (Physics.gravity.y * (horizontalTime * 0.5f));


		// Is the target position higher up?
		if (vec.y > 0.0f)
		{
			// Add force to reach the target position at max time
			// At the max time the velocity will be zero
			//	vfy = viy + (ay * t)
			//	viy = vfy - (ay * t)
			force.y = 0.0f - (Physics.gravity.y * horizontalTime);
		}

		
		// Limit the upward force to the max height allowed
		estimatedHeight = GeUtils.CalcHeight(force.y);
		if (estimatedHeight > maxHeight)
		{
			force.y = GeUtils.CalcVerticalVelocity(maxHeight);
		}
		
		
		return (force);
	}


	/// <summary>
	/// Test if a player can intercept the ball.
	/// </summary>
	/// <returns>The ball.</returns>
	/// <param name="player">Player.</param>
	/// <param name="playerSpeed">Player speed.</param>
	/// <param name="getIntersectPoint">Get intersect point. NOTE: Y will be 0.</param>
	/// <param name="getInterceptTime">Get intercept time.</param>
	/// <param name="getPlayerVelocity">Get the velocity the player should have to reach the intercept point.</param>
	/// <param name="getJumpForce">Get jump force.</param>
	/// <param name="getFailPoint">If interception fails, this returns the nearest point to the ball.</param>
	/// <param name="getRelativeHeight">Get height ball will reach, relative to the player.</param>
	public virtual bool InterceptBall(SsPlayer player, float playerSpeed, out Vector3 getInterceptPoint, out float getInterceptTime, 
	                                  out Vector3 getPlayerVelocity, out float getJumpForce, 
	                                  out Vector3 getFailPoint, out float getRelativeHeight)
	{
		getInterceptPoint = Vector3.zero;
		getInterceptTime = 0.0f;
		getPlayerVelocity = Vector3.zero;
		getJumpForce = 0.0f;
		getFailPoint = Vector3.zero;
		getRelativeHeight = 0.0f;

		bool interceptionPossible;
		float height;

		interceptCalculator.ChaserPosition = new SVector2d(player.transform.position.x, player.transform.position.z);
		interceptCalculator.ChaserSpeed = playerSpeed;
		interceptCalculator.RunnerPosition = new SVector2d(transform.position.x, transform.position.z);
		interceptCalculator.RunnerVelocity = new SVector2d(rb.velocity.x, rb.velocity.z);
		
		interceptionPossible = interceptCalculator.InterceptionPossible;
		if (interceptionPossible)
		{
			// Can intercept
			SVector2d tempVec;
			tempVec = interceptCalculator.InterceptionPoint;
			getInterceptPoint = new Vector3((float)tempVec.X, 0.0f, (float)tempVec.Y);
			tempVec = interceptCalculator.ChaserVelocity;
			getPlayerVelocity = new Vector3((float)tempVec.X, 0.0f, (float)tempVec.Y);
			getInterceptTime = (float)interceptCalculator.TimeToInterception;

			if (rb.velocity.y > 0.0f)
			{
				// Ball is moving up
				// Height at intersection time
				height = transform.position.y + GeUtils.CalcHeightAtTime(rb.velocity.y, getInterceptTime);
				height -= player.transform.position.y;	// Height relative to player
				if (height > 0.0f)
				{
					getRelativeHeight = height;
					getJumpForce = GeUtils.CalcVerticalVelocity(height);
				}
			}
		}
		else
		{
			// No intercept
			getFailPoint = GeUtils.ClosestPointOnLine(transform.position, 
			                                          transform.position + (rb.velocity.normalized * 10000.0f), 
			                                          player.transform.position);
			if (rb.velocity.y > 0.0f)
			{
				// Ball is moving up
				// How high will ball go?
				height = transform.position.y + GeUtils.CalcHeight(rb.velocity.y);
				height -= player.transform.position.y;	// Height relative to player
				if (height > 0.0f)
				{
					getRelativeHeight = height;
					getJumpForce = GeUtils.CalcVerticalVelocity(height);
				}
			}
		}

		return (interceptionPossible);
	}


	/// <summary>
	/// Test if ball collides with the box collider.
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

		if (((boxTransform.position.y - halfHeight) <= transform.position.y + radius) && 
		    ((boxTransform.position.y + halfHeight) >= transform.position.y - radius) && 
		    ((boxTransform.position.x - boxRadius) <= transform.position.x + radius) && 
		    ((boxTransform.position.x + boxRadius) >= transform.position.x - radius) && 
		    ((boxTransform.position.z - boxRadius) <= transform.position.z + radius) && 
		    ((boxTransform.position.z + boxRadius) >= transform.position.z - radius))
		{
			return (true);
		}

		return (false);
	}


}
