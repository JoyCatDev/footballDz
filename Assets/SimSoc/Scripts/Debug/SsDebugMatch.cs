using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// DEBUG match. Put this in a field scene to create a test match (i.e. able to test the scene without having to go through the menus).
/// </summary>
public class SsDebugMatch : MonoBehaviour {

	// Const/Static
	//-------------
	private const float markerSize = 2.0f;
	private const float markerSizeBig = 3.0f;


	// Public
	//-------
	[Header("Team 1")]
	public SsTeam teamPrefab1;
	public SsInput.inputTypes teamInput1;


	[Header("Team 2")]
	public SsTeam teamPrefab2;
	public SsInput.inputTypes teamInput2;


	[Header("Ball")]
	public SsBall ballPrefab;


	[Header("Settings")]
	[Tooltip("Match duration (minutes).")]
	public float matchDuration = SsMatch.defaultDuration;

	[Tooltip("Does the match need a winner?")]
	public bool needsWinner;

	[Tooltip("Override the match difficulty?")]
	public bool overrideDifficulty = false;
	[Tooltip("Difficulty to override.")]
	public int difficulty = 0;


	[Header("SHOW DEBUG INFO")]
	[Tooltip("Show OnGUI info. SIMSOC_ENABLE_ONGUI must be defined in the project settings.")]
	public bool showOnGui = false;


	[Space(10)]
	[Tooltip("Show control and pass markers.")]
	public bool showPlayerMarkers = false;

	[Tooltip("Show lines to player's targets.")]
	public bool showPlayerTargets = false;

	[Tooltip("Show player search grid blocks.")]
	public bool showPlayerSearches = false;

	[Tooltip("Show circles when player sidesteps an opponent.")]
	public bool showPlayerSidesteps = false;

	[Tooltip("Show circle when player avoids a sliding opponent.")]
	public bool showPlayerAvoidSlide = false;

	[Tooltip("Show the player and ball's visible rects.")]
	public bool showVisibleRect = false;

	[Tooltip("Show what position the player is playing.")]
	public bool showPlayerPosition = false;

	[Tooltip("Show player's state.")]
	public bool showPlayerState = false;

	[Tooltip("Show distance at which player loses ownership of the ball.")]
	public bool showPlayerBallLostDistance = false;

	[Tooltip("Show the player's dribble distances.")]
	public bool showPlayerDribble = false;

	[Tooltip("Show player personal space. ")]
	public bool showPlayerPersonalSpace = false;

	[Tooltip("Show circles when a bad pass happens.")]
	public bool showBadPass = false;

	[Tooltip("Show circles when a bad shot happens.")]
	public bool showBadShot = false;

	[Tooltip("Show position ball is moving to, for small automated movements (e.g. dribbling).")]
	public bool showBallTargetPos = false;

	[Tooltip("Draw line between the ball and the ball player.")]
	public bool showBallToBallPlayer = false;


	[Space(10)]
	[Tooltip("Show field halves.")]
	public bool showHalves = false;

	[Tooltip("Show field zones.")]
	public bool showZones = false;

	[Tooltip("Show field rows.")]
	public bool showRows = false;

	[Tooltip("Show field grid.")]
	public bool showGrid = false;

	[Tooltip("Show field grid weights.")]
	public bool showGridWeights = false;



	// Private
	//--------
	static private SsDebugMatch instance;

	private SsMatch match;
	private SsBall ball;
	private List<SsMatchInputManager.SsUserInput> userInput;

	private float setUserControlDelay;


	// Properties
	//-----------
	static public SsDebugMatch Instance
	{
		get { return(instance); }
	}

		
	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;

#if UNITY_EDITOR
		if (gameObject.tag != "EditorOnly")
		{
			Debug.LogError("ERROR: A Debug Match game object's tag is not set to EditorOnly. This means it will be included in the build. " + 
			               "Scene: " + GeUtils.GetLoadedLevelName());
		}
#endif //UNITY_EDITOR
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		Invoke("PostStart", 0.5f);
	}


	/// <summary>
	/// Gets references to the match, ball, etc.
	/// </summary>
	/// <returns>The references.</returns>
	void GetReferences()
	{
		if (match == null)
		{
			match = SsMatch.Instance;
		}
		if (ball == null)
		{
			ball = SsBall.Instance;
		}
		if (userInput == null)
		{
			userInput = (SsMatchInputManager.Instance != null) ? SsMatchInputManager.Instance.UserInput : null;
		}
	}


	/// <summary>
	/// Posts the start.
	/// </summary>
	/// <returns>The start.</returns>
	void PostStart()
	{
		// If no match has been set up, then create a test match
		bool createMatch = false;

#if UNITY_EDITOR
		bool result = false;
#endif //UNITY_EDITOR

		GetReferences();

		if ((SsMatch.Instance == null) && (SsNewMatch.Instance == null))
		{
			createMatch = true;
		}
		else
		{
			SsPlayer[] players = FindObjectsOfType(typeof(SsPlayer)) as SsPlayer[];
			if ((players == null) || (players.Length <= 0))
			{
				createMatch = true;
			}
		}

		if (createMatch)
		{
#if UNITY_EDITOR
			result = CreateTestMatch();
#else
			CreateTestMatch();
#endif //UNITY_EDITOR
		}


#if UNITY_EDITOR
		if (SsSettings.LogInfoToConsole)
		{
			if (result)
			{
				Debug.Log("Creating a test match via a Debug Match object, in the scene: " + GeUtils.GetLoadedLevelName());
			}
			else
			{
				Debug.Log("Please note: Scene [" + GeUtils.GetLoadedLevelName() + "] contains a Debug Match object, but it was not used to create the match. " + 
			          "Therefore Debug Match's settings are ignored. (Match was probably created via the menus.)");
			}
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Creates a test match.
	/// </summary>
	/// <returns>The test match.</returns>
	bool CreateTestMatch()
	{
		if ((SsMatch.Instance == null) && (SsNewMatch.Instance == null))
		{
			int newDifficulty = -1;

			if (overrideDifficulty)
			{
				newDifficulty = difficulty;
			}

			SsMatch.StartMatchTest(teamPrefab1, teamPrefab2, 
			                       teamInput1, teamInput2,
			                       ballPrefab,
			                       "",
			                       "",
			                       matchDuration,
			                       needsWinner,
			                       newDifficulty);
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		float dt = Time.deltaTime;

		GetReferences();


		if (setUserControlDelay > 0.0f)
		{
			setUserControlDelay -= dt;
			if (setUserControlDelay <= 0.0f)
			{
				setUserControlDelay = 0.0f;


				// Set the user controlled player
				if (match.LeftTeam.IsUserControlled)
				{
					SsMatchInputManager.SetControlPlayer(match.LeftTeam.Players[0], match.LeftTeam.UserControlIndex);

				}
				else if (match.RightTeam.IsUserControlled)
				{
					SsMatchInputManager.SetControlPlayer(match.RightTeam.Players[0], match.RightTeam.UserControlIndex);
				}


				// Set the ball player
				if (match.KickoffTeam == match.LeftTeam)
				{
					match.SetBallPlayer(match.LeftTeam.Players[0]);
				}
				else if (match.KickoffTeam == match.RightTeam)
				{
					match.SetBallPlayer(match.RightTeam.Players[0]);
				}
			}
		}

	}


	/// <summary>
	/// Lates the update.
	/// </summary>
	/// <returns>The update.</returns>
	void LateUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		if ((SsMatchCamera.Instance != null) && (SsMatchCamera.Instance.DynamicUseFixedUpdate == false))
		{
			UpdateDraw();
		}
	}


	/// <summary>
	/// Fixeds the update.
	/// </summary>
	/// <returns>The update.</returns>
	void FixedUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		if ((SsMatchCamera.Instance != null) && (SsMatchCamera.Instance.DynamicUseFixedUpdate))
		{
			UpdateDraw();
		}
	}


	/// <summary>
	/// Update debug drawing in the scene (e.g. draw lines, rectangles, etc.).
	/// </summary>
	/// <returns>The draw.</returns>
	void UpdateDraw()
	{
		if (showPlayerMarkers)
		{
			DrawPlayerMarkers();
		}

		DrawPlayerStuff();

		DrawBallStuff();
	}


	/// <summary>
	/// Draw a rect which has a normal facing up (e.g. a player marker on the ground).
	/// </summary>
	/// <returns>The rect.</returns>
	/// <param name="centre">Centre.</param>
	/// <param name="size">Size.</param>
	/// <param name="colour">Colour.</param>
	static public void DrawRectUp(Vector3 centre, Vector2 size, Color colour, float duration = 0.0f)
	{
		// Far
		Debug.DrawLine(new Vector3(centre.x - (size.x / 2), centre.y, centre.z + (size.y / 2)),
		               new Vector3(centre.x + (size.x / 2), centre.y, centre.z + (size.y / 2)),
		               colour, duration);
		// Right
		Debug.DrawLine(new Vector3(centre.x + (size.x / 2), centre.y, centre.z + (size.y / 2)),
		               new Vector3(centre.x + (size.x / 2), centre.y, centre.z - (size.y / 2)),
		               colour, duration);
		// Bottom
		Debug.DrawLine(new Vector3(centre.x + (size.x / 2), centre.y, centre.z - (size.y / 2)),
		               new Vector3(centre.x - (size.x / 2), centre.y, centre.z - (size.y / 2)),
		               colour, duration);
		// Left
		Debug.DrawLine(new Vector3(centre.x - (size.x / 2), centre.y, centre.z - (size.y / 2)),
		               new Vector3(centre.x - (size.x / 2), centre.y, centre.z + (size.y / 2)),
		               colour, duration);
	}


	/// <summary>
	/// Draw a circle which has a normal facing up (e.g. lying on the ground).
	/// </summary>
	/// <returns>The circle up.</returns>
	/// <param name="centre">Centre.</param>
	/// <param name="radius">Radius.</param>
	/// <param name="maxSides">Max sides. Range 6 to 20.</param>
	/// <param name="colour">Colour.</param>
	static public void DrawCircleUp(Vector3 centre, float radius, int maxSides, Color colour, float duration = 0.0f)
	{
		maxSides = Mathf.Clamp(maxSides, 6, 20);

		Vector3 vec, pos, prevPos;
		int i;
		float angle, angleStep;

		angleStep = (float)360.0f / (float)maxSides;
		angle = 0.0f;
		prevPos = Vector3.zero;

		for (i = 0; i <= maxSides; i++)
		{
			vec = Quaternion.Euler(0.0f, angle, 0.0f) * (Vector3.forward * radius);

			pos = centre + vec;
			if (i > 0)
			{
				Debug.DrawLine(pos, prevPos, colour, duration);
			}
			prevPos = pos;

			angle += angleStep;
		}
	}


	/// <summary>
	/// Draws the player markers (e.g. control player, player being passed to).
	/// </summary>
	/// <returns>The player markers.</returns>
	void DrawPlayerMarkers()
	{
		if (match == null)
		{
			return;
		}

		int i;
		SsMatchInputManager.SsUserInput ui;

		if ((userInput != null) && (userInput.Count > 0))
		{
			// Show the user controlled players
			for (i = 0; i < userInput.Count; i++)
			{
				ui = userInput[i];
				if (ui == null)
				{
					continue;
				}
				
				if (ui.player != null)
				{
					if ((SsMatch.Instance != null) && (SsMatch.Instance.BallPlayer == ui.player))
					{
						// Has ball
						DrawRectUp(ui.player.transform.position, new Vector2(markerSize, markerSize), 
						           new Color(1.0f, 1.0f, 0.0f, 1.0f));
						DrawRectUp(ui.player.transform.position, new Vector2(markerSizeBig, markerSizeBig), 
						           new Color(1.0f, 1.0f, 0.0f, 1.0f));
					}
					else
					{
						DrawCircleUp(ui.player.transform.position, markerSize / 2.0f, 10, new Color(1.0f, 1.0f, 0.0f, 1.0f));
					}
				}
			}
		}


		// Potential pass player
		if ((match.BallPlayer != null) && (match.BallPlayer.Team.PotentialPassPlayer != null))
		{
			DrawRectUp(match.BallPlayer.Team.PotentialPassPlayer.transform.position, new Vector2(markerSize, markerSize), 
			           Color.cyan);
		}


		// Player being passed to
		if ((match.LeftTeam != null) && (match.LeftTeam.PassPlayer != null))
		{
			DrawRectUp(match.LeftTeam.PassPlayer.transform.position, new Vector2(markerSize, markerSize), 
			           new Color(0.0f, 0.0f, 1.0f, 0.5f));
		}
		if ((match.RightTeam != null) && (match.RightTeam.PassPlayer != null))
		{
			DrawRectUp(match.RightTeam.PassPlayer.transform.position, new Vector2(markerSize, markerSize), 
			           new Color(0.0f, 0.0f, 1.0f, 0.5f));
		}

	}


	/// <summary>
	/// Draws the player stuff.
	/// </summary>
	/// <returns>The player stuff.</returns>
	void DrawPlayerStuff()
	{
		if ((match == null) || (match.LeftTeam == null) || (match.RightTeam == null))
		{
			return;
		}

		int t, i;
		SsTeam team;
		SsPlayer player;
		Vector3 pos, tl, tr, bl, br;
		bool foundTarget;
		Color redLineColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		Rect rect;
		Camera cam = (SsMatchCamera.Instance != null) ? SsMatchCamera.Instance.Cam : null;
		float z;
		Color colour;


		pos = Vector3.zero;

		for (t = 0; t < 2; t++)
		{
			if (t == 0)
			{
				team = match.LeftTeam;
			}
			else
			{
				team = match.RightTeam;
			}

			if ((team.Players == null) || (team.Players.Count <= 0))
			{
				continue;
			}

			for (i = 0; i < team.Players.Count; i++)
			{
				player = team.Players[i];
				if (player == null)
				{
					continue;
				}

				if (showPlayerTargets)
				{
					foundTarget = false;
					if (player.TargetObject != null)
					{
						foundTarget = true;
						pos = player.TargetObject.transform.position;
					}
					else if (player.GotoTargetPos)
					{
						foundTarget = true;
						pos = player.TargetPos;
					}

					if (foundTarget)
					{
						Debug.DrawLine(player.transform.position, pos, redLineColor);
						DrawCrossUp(pos, 0.5f, redLineColor);
					}
				}


				if ((showVisibleRect) && (player.IsVisible) && (player.BehindCamera == false))
				{
					rect = player.VisibleRect;
					tl = player.transform.position + new Vector3(rect.center.x - (rect.width / 2.0f), 
					                                             rect.center.y + (rect.height / 2.0f), 
					                                             0.0f);
					tr = player.transform.position + new Vector3(rect.center.x + (rect.width / 2.0f), 
					                                             rect.center.y + (rect.height / 2.0f), 
					                                             0.0f);
					br = player.transform.position + new Vector3(rect.center.x + (rect.width / 2.0f), 
					                                             rect.center.y - (rect.height / 2.0f), 
					                                             0.0f);
					bl = player.transform.position + new Vector3(rect.center.x - (rect.width / 2.0f), 
					                                             rect.center.y - (rect.height / 2.0f), 
					                                             0.0f);
					if (player.IsVisible)
					{
						colour = Color.yellow;
					}
					else
					{
						colour = Color.red;
					}
					Debug.DrawLine(tl, tr, colour);
					Debug.DrawLine(tr, br, colour);
					Debug.DrawLine(br, bl, colour);
					Debug.DrawLine(bl, tl, colour);

					pos = player.transform.position + new Vector3(rect.center.x, rect.center.y, 0.0f);
					DrawCrossUp(pos, 0.1f, Color.yellow);

					if (cam != null)
					{
						rect = player.ScreenRect;
						z = Mathf.Abs(player.transform.position.z - cam.transform.position.z);
						tl = cam.ScreenToWorldPoint(new Vector3(rect.min.x, rect.max.y, z));
						tr = cam.ScreenToWorldPoint(new Vector3(rect.max.x, rect.max.y, z));
						br = cam.ScreenToWorldPoint(new Vector3(rect.max.x, rect.min.y, z));
						bl = cam.ScreenToWorldPoint(new Vector3(rect.min.x, rect.min.y, z));

						if (player.IsVisible)
						{
							colour = Color.blue;
						}
						else
						{
							colour = Color.red;
						}
						Debug.DrawLine(tl, tr, colour);
						Debug.DrawLine(tr, br, colour);
						Debug.DrawLine(br, bl, colour);
						Debug.DrawLine(bl, tl, colour);
					}
				}

				if (showPlayerBallLostDistance)
				{
					DrawCircleUp(player.transform.position, Mathf.Sqrt(player.BallLostDistanceSqr), 10, Color.red);
				}

				if (showPlayerDribble)
				{
					DrawCircleUp(player.transform.position, player.BallDribbleDistanceMin, 10, Color.green);
					DrawCircleUp(player.transform.position, player.BallDribbleDistanceMax, 10, Color.blue);
				}

				if (showPlayerPersonalSpace)
				{
					DrawCircleUp(player.transform.position, player.PersonalSpaceRadius, 10, Color.yellow);
				}
			}
		}
	}


	/// <summary>
	/// Draws the ball stuff.
	/// </summary>
	/// <returns>The ball stuff.</returns>
	void DrawBallStuff()
	{
		if ((match == null) || (match.LeftTeam == null) || (match.RightTeam == null))
		{
			return;
		}

		Camera cam = (SsMatchCamera.Instance != null) ? SsMatchCamera.Instance.Cam : null;
		SsBall ball = SsBall.Instance;
		Rect rect;
		Vector3 pos, tl, tr, bl, br;
		float z;
		Color colour;

		if ((showVisibleRect) && (ball.IsVisible) && (ball.BehindCamera == false))
		{
			rect = ball.VisibleRect;
			tl = ball.transform.position + new Vector3(rect.center.x - (rect.width / 2.0f), 
			                                             rect.center.y + (rect.height / 2.0f), 
			                                             0.0f);
			tr = ball.transform.position + new Vector3(rect.center.x + (rect.width / 2.0f), 
			                                             rect.center.y + (rect.height / 2.0f), 
			                                             0.0f);
			br = ball.transform.position + new Vector3(rect.center.x + (rect.width / 2.0f), 
			                                             rect.center.y - (rect.height / 2.0f), 
			                                             0.0f);
			bl = ball.transform.position + new Vector3(rect.center.x - (rect.width / 2.0f), 
			                                             rect.center.y - (rect.height / 2.0f), 
			                                             0.0f);
			Debug.DrawLine(tl, tr, Color.yellow);
			Debug.DrawLine(tr, br, Color.yellow);
			Debug.DrawLine(br, bl, Color.yellow);
			Debug.DrawLine(bl, tl, Color.yellow);
			
			pos = ball.transform.position + new Vector3(rect.center.x, rect.center.y, 0.0f);
			DrawCrossUp(pos, 0.1f, Color.yellow);
			
			if (cam != null)
			{
				rect = ball.ScreenRect;
				z = Mathf.Abs(ball.transform.position.z - cam.transform.position.z);
				tl = cam.ScreenToWorldPoint(new Vector3(rect.min.x, rect.max.y, z));
				tr = cam.ScreenToWorldPoint(new Vector3(rect.max.x, rect.max.y, z));
				br = cam.ScreenToWorldPoint(new Vector3(rect.max.x, rect.min.y, z));
				bl = cam.ScreenToWorldPoint(new Vector3(rect.min.x, rect.min.y, z));
				
				Debug.DrawLine(tl, tr, Color.blue);
				Debug.DrawLine(tr, br, Color.blue);
				Debug.DrawLine(br, bl, Color.blue);
				Debug.DrawLine(bl, tl, Color.blue);
			}
		}

		if ((showBallToBallPlayer) && (match.BallPlayer != null))
		{
			// Draw a thick line
			if ((match.BallPlayer.DribbleState == SsPlayer.dribbleStates.idle))
			{
				colour = Color.white;
			}
			else if ((match.BallPlayer.DribbleState == SsPlayer.dribbleStates.moveForward))
			{
				colour = Color.blue;
			}
			else
			{
				colour = new Color(0.4f, 1.0f, 0.4f);
			}

			Debug.DrawLine(ball.transform.position, 
			               match.BallPlayer.transform.position, 
			              colour);

			Debug.DrawLine(ball.transform.position + (Vector3.right * 0.01f), 
			               match.BallPlayer.transform.position + (Vector3.right * 0.01f), 
			               colour);

			Debug.DrawLine(ball.transform.position + (Vector3.right * 0.02f), 
			               match.BallPlayer.transform.position + (Vector3.right * 0.02f), 
			               colour);
		}
	}


	/// <summary>
	/// Draw a cross which has a normal pointing up (e.g. lying on the ground).
	/// </summary>
	/// <returns>The cross.</returns>
	/// <param name="centre">Centre.</param>
	/// <param name="radius">Radius.</param>
	/// <param name="colour">Colour.</param>
	/// <param name="duration">Duration.</param>
	static public void DrawCrossUp(Vector3 centre, float radius, Color colour, float duration = 0.0f)
	{
		Debug.DrawLine(centre + (Vector3.right * radius) + (Vector3.forward * radius),
		               centre - (Vector3.right * radius) - (Vector3.forward * radius),
		               colour, duration);
		Debug.DrawLine(centre - (Vector3.right * radius) + (Vector3.forward * radius),
		               centre + (Vector3.right * radius) - (Vector3.forward * radius),
		               colour, duration);
	}

#if SIMSOC_ENABLE_ONGUI
	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		if ((match == null) || (showOnGui == false))
		{
			return;
		}

		Vector2 pos = new Vector2(5, 5);
		float addY = 25.0f;
		SsPlayer player;
		SsBall ball = SsBall.Instance;
		SsFieldProperties field = SsFieldProperties.Instance;
		int i, n;
		SsTeam team;
		Rect rect;
		Vector2 oldPos;


		// Loading
		if (match.IsLoading(true))
		{
			GUI.Box(new Rect((Screen.width / 2) - 50, (Screen.height / 2) - 15, 100, 30), "");
			GUI.Box(new Rect((Screen.width / 2) - 50, (Screen.height / 2) - 15, 100, 30), "Loading...");
		}


		// State
		GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "" + match.State);
		pos.y += addY;


		// Half Timer
		GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "" + match.HalfTimeTimer.ToString("f0") + " / " + 
		          match.MaxHalfTime.ToString("f0"));
		pos.y += addY;


		// Ball team
		if (match.BallTeam != null)
		{
			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "ballTeam: " + match.BallTeam.GetAnyName());
			pos.y += addY;
			
			if (match.BallTeam.PotentialPassPlayer != null)
			{
				GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "passTo: " + match.BallTeam.PotentialPassPlayer.GetAnyName());
				pos.y += addY;
			}
			
			// Space
			pos.y += addY;
		}

		
		// Player with ball
		player = match.BallPlayer;
		if ((player != null) && (field != null))
		{
			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), player.GetAnyName());
			pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "dist: " + player.MoveDistance);
			//pos.y += addY;

			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "state: " + player.State);
			pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "onGrnd: " + player.OnGroundTime);
			//pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "velY: " + player.MoveVec.y);
			//pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "zone: " + player.Zone + "   (" + player.ZoneTransformed + ")" + 
			//          "    row: " + player.Row);
			//pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "past: " + field.PlayerPastZone(player, player.Skills.ai.shootPastZone, null, false));
			//pos.y += addY;

			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "sprint: " + player.Input.sprintActive);
			pos.y += addY;

			// Space
			pos.y += addY;
		}


		// Ball
		if (ball != null)
		{
			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "bspd: " + ball.Speed.ToString("f2"));
			pos.y += addY;

			//GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "bzn: " + ball.Zone);
			//pos.y += addY;

			GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "maxH: " + ball.TrackMaxHeight);
			pos.y += addY;

			// Space
			pos.y += addY;
		}


		// Teams
		if ((match != null) && (match.Teams != null) && (match.Teams.Length > 0))
		{
			for (i = 0; i < match.Teams.Length; i++)
			{
				team = match.Teams[i];
				if (team == null)
				{
					continue;
				}

				GUI.Label(new Rect(pos.x, pos.y, 1000, addY), "tc: " + team.BallPredatorsCount + "    " + team.name);
				pos.y += addY;

				oldPos = pos;

				// Players
				if ((team.Players != null) && (team.Players.Count > 0))
				{
					for (n = 0; n < team.Players.Count; n++)
					{
						player = team.Players[n];
						if (player == null)
						{
							continue;
						}

						if ((player.IsVisible) && (player.BehindCamera == false))
						{
							rect = player.ScreenRect;
							pos.y = addY;

							if (showPlayerPosition)
							{
								GUI.Label(new Rect(rect.min.x, Screen.height - rect.max.y - pos.y, 100, addY), 
								          "" + GeUtils.FirstLetterToUpper(player.Position.ToString())[0]);
								pos.y += addY;
							}

							if (showPlayerState)
							{
								GUI.Label(new Rect(rect.min.x, Screen.height - rect.max.y - pos.y, 100, addY), 
								          "" + player.State + "   " + 
								          GeUtils.FirstLetterToUpper(player.Animations.IsAnimationPlaying().ToString())[0] + 
								          "  " + player.Animations.GetAnimationTime().ToString("f2"));
								pos.y += addY;
							}

							//if (player.Position == SsPlayer.positions.goalkeeper)
							//{
							//	GUI.Label(new Rect(rect.min.x, Screen.height - rect.max.y - pos.y, 100, addY), 
							//          "" + player.MoveVec.y);
							//	pos.y += addY;
							//}

							//GUI.Label(new Rect(rect.min.x, Screen.height - rect.max.y - pos.y, 100, addY), 
							//          "tmh: " + player.TrackMaxHeight.ToString("f2"));
							//pos.y += addY;
						}
					}
				}

				pos = oldPos;
			}


			// Space
			pos.y += addY;
		}

	}
#endif
}
