using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Hud. In-game screen.
/// </summary>
public class SsHud : SsBaseMenu {

	// Const/Static
	//-------------
	// Number strings for updating text (e.g. match time). Used to prevent string memory leaks, especially frequent updates.
	static public string[] numberStrings = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

	static public int maxMinutes = 99;		// Max minutes to display on the hud


	// Public
	//-------
	[Header("Prefabs")]
	[Tooltip("Mini-map player icon prefab.")]
	public UnityEngine.UI.Image miniMapPlayerPrefab;


	[Header("Elements")]
	public UnityEngine.UI.Text half;
	//public UnityEngine.UI.Text time;
	[Tooltip("Time minutes left digit.")]
	public UnityEngine.UI.Text timeMinuteLeft;
	[Tooltip("Time minutes right digit.")]
	public UnityEngine.UI.Text timeMinuteRight;
	[Tooltip("Time seconds left digit.")]
	public UnityEngine.UI.Text timeSecondLeft;
	[Tooltip("Time seconds right digit.")]
	public UnityEngine.UI.Text timeSecondRight;

	[Space(10)]
	[Tooltip("Auto swap the icons and score based on the camera's play direction.")]
	public bool autoSwapIconsForCam = true;

	[Space(10)]
	public UnityEngine.UI.Text leftScore;
	public UnityEngine.UI.Image leftIcon;

	[Space(10)]
	public UnityEngine.UI.Text rightScore;
	public UnityEngine.UI.Image rightIcon;

	[Space(10)]
	[Tooltip("Goal popup to show when a goal is scored.")]
	public SsBaseMenu goal;
	[Tooltip("How long should goal popup be visible?")]
	public float goalVisibleDuration = 2.0f;

	[Space(10)]
	[Tooltip("Half time popup to show at half time.")]
	public SsBaseMenu halfTime;

	[Tooltip("Results popup to show at the end of the game.")]
	public SsBaseMenu results;

	[Space(10)]
	[Tooltip("Mini-map panel.")]
	public GameObject miniMap;

	[Tooltip("Mini-map play area panel. It will hold the ball & player icons, and difines the size of the play area.")]
	public RectTransform miniMapPlayArea;

	[Tooltip("Mini-map ball icon.")]
	public UnityEngine.UI.Image miniMapBall;

	[Tooltip("Auto rotate the mini-map based on the camera's play direction.")]
	public bool autoRotateMiniMapForCam = true;
	[Tooltip("Auto adjust the mini-map position for portrait orientation based on the camera's play direction. Only adjusted when auto rotate is set.")]
	public Vector2 autoAdjustPortraitPos = new Vector2(0.0f, -63.0f);



	// Private
	//--------
	static private SsHud instance;

	private SsMatch match;
	private bool addedStateAction;								// Added match state change action
	
	private UnityEngine.UI.Image[] arrowPass;
	private UnityEngine.UI.Image[] arrowControl;

	private SsBaseMenu.toDirections goalOutDirection;
	private float goalVisibleTime;

	private UnityEngine.UI.Image[,] miniMapPlayers;
	private Vector3 miniMapDefaultLocalRotation;				// Mini-map default local rotation
	private Vector3 miniMapDefaultLocalPosition;				// Mini-map default local position



	// Properties
	//-----------
	static public SsHud Instance
	{
		get { return(instance); }
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

		if (miniMap != null)
		{
			miniMapDefaultLocalRotation = miniMap.transform.localRotation.eulerAngles;
			miniMapDefaultLocalPosition = miniMap.transform.localPosition;
		}
	}

	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		GetReferences();
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();

		instance = null;

		match = null;
	}


	/// <summary>
	/// Raises the enable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnEnable()
	{
		base.OnEnable();

		AddCallbacks();
	}


	/// <summary>
	/// Raises the disable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDisable()
	{
		base.OnDisable();

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
	/// Gets the references.
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
			if (match != null)
			{
				if (autoRotateMiniMapForCam)
				{
					RotateMiniMap();
				}

				SpawnObjects();

				// Finally show the menu
				Show(fromDirections.invalid, false);
			}
		}

	}


	/// <summary>
	/// Spawns the objects.
	/// </summary>
	/// <returns>The objects.</returns>
	private void SpawnObjects()
	{
		SsFieldProperties field = SsFieldProperties.Instance;
		SsMatchPrefabs mps = (field != null) ? field.matchPrefabs : null;
		float z;
		int i, n, max;

		// Arrows
		if (mps != null)
		{
			if (mps.arrowPass != null)
			{
				arrowPass = new UnityEngine.UI.Image[2];
				for (i = 0; i < 2; i++)
				{
					arrowPass[i] = (UnityEngine.UI.Image)Instantiate(mps.arrowPass);
					if (arrowPass[i] != null)
					{
						z = arrowPass[i].transform.localPosition.z;
						if (rectTransform != null)
						{
							arrowPass[i].rectTransform.SetParent(rectTransform, false);
						}
						else
						{
							arrowPass[i].rectTransform.SetParent(transform, false);
						}
						arrowPass[i].transform.localPosition = new Vector3(0.0f, 0.0f, z);

						// Draw the arrow behind the other HUD elements
						arrowPass[i].transform.SetAsFirstSibling();

						arrowPass[i].gameObject.SetActive(false);
					}
				}
			}

			if (mps.arrowControl != null)
			{
				arrowControl = new UnityEngine.UI.Image[2];
				for (i = 0; i < 2; i++)
				{
					arrowControl[i] = (UnityEngine.UI.Image)Instantiate(mps.arrowControl);
					if (arrowControl[i] != null)
					{
						z = arrowControl[i].transform.localPosition.z;
						if (rectTransform != null)
						{
							arrowControl[i].rectTransform.SetParent(rectTransform, false);
						}
						else
						{
							arrowControl[i].rectTransform.SetParent(transform, false);
						}
						arrowControl[i].transform.localPosition = new Vector3(0.0f, 0.0f, z);

						// Draw the arrow behind the other HUD elements
						arrowControl[i].transform.SetAsFirstSibling();

						arrowControl[i].gameObject.SetActive(false);
					}
				}
			}
		}


		// Mini-map
		if ((miniMap != null) && (miniMapPlayArea != null) && (miniMapPlayerPrefab != null) && 
		    (match != null) && (match.Teams != null) && (match.Teams.Length >= 2))
		{
			List<SsPlayer> players;
			SsTeam team;

			miniMapPlayers = new UnityEngine.UI.Image[2, field.NumPlayersPerSide];

			for (i = 0; i < 2; i++)
			{
				team = match.Teams[i];
				if (team == null)
				{
					continue;
				}

#if UNITY_EDITOR
				if (team.miniMapColour.a <= 0.0f)
				{
					Debug.LogWarning("WARNING: Team mini-map colour alpha is zero. Team: " + team.GetAnyName());
				}
#endif //UNITY_EDITOR

				players = team.Players;

				if ((players != null) && (players.Count > 0))
				{
					max = Mathf.Min(players.Count, miniMapPlayers.GetLength(1));

#if UNITY_EDITOR
					if (players.Count > field.NumPlayersPerSide)
					{
						Debug.LogWarning("WARNING: Number of players in team is more than amount specified in the match settings/field properties. Team: " + team.GetAnyName());
					}
#endif //UNITY_EDITOR

					for (n = 0; n < max; n++)
					{
						miniMapPlayers[i, n] = Instantiate(miniMapPlayerPrefab);
						if (miniMapPlayers[i, n] != null)
						{
							miniMapPlayers[i, n].rectTransform.SetParent(miniMapPlayArea, false);
							miniMapPlayers[i, n].color = team.miniMapColour;

							miniMapPlayers[i, n].gameObject.SetActive(false);
						}
					}
				}
			}

			// Draw the ball last
			if (miniMapBall != null)
			{
				miniMapBall.transform.SetAsLastSibling();
			}
		}


		if (miniMap != null)
		{
			miniMap.SetActive(SsSettings.showMiniMap);
		}
	}


	/// <summary>
	/// Rotate the mini-map based on the camera's play direction.
	/// </summary>
	public void RotateMiniMap()
	{
		if ((miniMap == null) || (SsMatchCamera.Instance == null) || (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.right))
		{
			return;
		}

		if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.left)
		{
			// Play left
			miniMap.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f) * Quaternion.Euler(miniMapDefaultLocalRotation);
		}
		else if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.up)
		{
			// Play up
			miniMap.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f) * Quaternion.Euler(miniMapDefaultLocalRotation);
			miniMap.transform.localPosition = miniMapDefaultLocalPosition + new Vector3(autoAdjustPortraitPos.x, autoAdjustPortraitPos.y, 0.0f);
		}
		else if (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.down)
		{
			// Play down
			miniMap.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f) * Quaternion.Euler(miniMapDefaultLocalRotation);
			miniMap.transform.localPosition = miniMapDefaultLocalPosition + new Vector3(autoAdjustPortraitPos.x, autoAdjustPortraitPos.y, 0.0f);
		}
	}


	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		base.Show(fromDirection, snap);

		AddCallbacks();

		UpdateControls(true);
	}


	/// <summary>
	/// Raises the match state changed event.
	/// REMINDER: May Not receive the "loading" state change, because the "loading" state set before this game object registers this method.
	/// </summary>
	/// <param name="newState">New state.</param>
	private void OnMatchStateChanged(SsMatch.states newState)
	{
		bool updateUi = true;
		bool updateIcons = false;

		if (newState == SsMatch.states.kickOff)
		{
			updateIcons = true;
		}
		else if (newState == SsMatch.states.goal)
		{
			ShowGoal();
		}
		else if (newState == SsMatch.states.halfTime)
		{
			ShowHalfTime();
		}
		else if (newState == SsMatch.states.matchResults)
		{
			ShowResults();
		}

		if (updateUi)
		{
			UpdateControls(updateIcons);
		}
	}


	/// <summary>
	/// Shows the goal popup.
	/// </summary>
	/// <returns>The goal.</returns>
	public void ShowGoal()
	{
		if (goal != null)
		{
			if (match.GoalTeam == match.LeftTeam)
			{
				goal.Show(fromDirections.fromRight, false);
				goalOutDirection = toDirections.toLeft;
			}
			else
			{
				goal.Show(fromDirections.fromLeft, false);
				goalOutDirection = toDirections.toRight;
			}
			goalVisibleTime = goalVisibleDuration;
		}
	}


	/// <summary>
	/// Hides the goal.
	/// </summary>
	/// <returns>The goal.</returns>
	public void HideGoal()
	{
		if ((goal != null) && (goal.IsVisible))
		{
			goal.Hide(false, goalOutDirection);
		}
	}


	/// <summary>
	/// Shows the half time.
	/// </summary>
	/// <returns>The half time.</returns>
	public void ShowHalfTime()
	{
		if (halfTime != null)
		{
			halfTime.Show(fromDirections.invalid, false);
		}
		else if (match != null)
		{
			match.OnHalfTimeUiClosed();
		}
	}


	/// <summary>
	/// Shows the results.
	/// </summary>
	/// <returns>The results.</returns>
	public void ShowResults()
	{
		if (results != null)
		{
			results.Show(fromDirections.invalid, false);
		}
		else if (match != null)
		{
			match.EndMatchAndLoadNextScene();
		}
	}



	/// <summary>
	/// Update this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Update()
	{
		base.Update();

		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

		float dt = lastDt;

		UpdateArrows(dt);
		UpdateMiniMap(dt);
		UpdatePopups(dt);
		UpdateTime();
	}


	/// <summary>
	/// Gets the team sides for in-game UI (e.g. HUD, half time, match results). It takes into account the camera's play direction.
	/// </summary>
	/// <param name="leftTeam">Left team.</param>
	/// <param name="rightTeam">Right team.</param>
	static public void GetTeamSidesForUI(out SsTeam leftTeam, out SsTeam rightTeam)
	{
		if ((SsMatchCamera.Instance == null) || (SsMatch.Instance == null))
		{
			leftTeam = null;
			rightTeam = null;
			return;
		}

		if ((SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.left) || 
		    (SsMatchCamera.Instance.playDirection == SsMatchCamera.playDirections.down))
		{
			// Change teams based on camera's play direction
			leftTeam = SsMatch.Instance.RightTeam;
			rightTeam = SsMatch.Instance.LeftTeam;
		}
		else
		{
			leftTeam = SsMatch.Instance.LeftTeam;
			rightTeam = SsMatch.Instance.RightTeam;
		}
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The user interface.</returns>
	/// <param name="updateIcons">Update team icons.</param>
	public void UpdateControls(bool updateIcons)
	{
		if ((match == null) || (match.LeftTeam == null) || (match.RightTeam == null))
		{
			return;
		}

		Sprite sprite;
		int i;
		SsTeam leftTeam, rightTeam;

		if (autoSwapIconsForCam)
		{
			// Get the team's based on the camera's play direction
			GetTeamSidesForUI(out leftTeam, out rightTeam);
		}
		else
		{
			leftTeam = match.LeftTeam;
			rightTeam = match.RightTeam;
		}

		if (half != null)
		{
			i = match.MatchHalf + 1;
			if ((i >= 0) && (i < 10))
			{
				// Use numberStrings to prevent string memory leaks.
				half.text = numberStrings[i];
			}
			else
			{
				half.text = i.ToString();
			}
		}

		UpdateTime();

		if (leftScore != null)
		{
			i = leftTeam.Score;
			if ((i >= 0) && (i < 10))
			{
				// Use numberStrings to prevent string memory leaks.
				leftScore.text = numberStrings[i];
			}
			else
			{
				leftScore.text = i.ToString();
			}
		}
		if (rightScore != null)
		{
			i = rightTeam.Score;
			if ((i >= 0) && (i < 10))
			{
				// Use numberStrings to prevent string memory leaks.
				rightScore.text = numberStrings[i];
			}
			else
			{
				rightScore.text = i.ToString();
			}
		}

		if (updateIcons)
		{
			if (leftIcon != null)
			{
				sprite = leftTeam.hudIcon;
				if (sprite != null)
				{
					leftIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Hud Icon not set. Team: " + leftTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}
			if (rightIcon != null)
			{
				sprite = rightTeam.hudIcon;
				if (sprite != null)
				{
					rightIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Hud Icon not set. Team: " + rightTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}
		}
	}


	/// <summary>
	/// Updates the time.
	/// </summary>
	/// <returns>The time.</returns>
	public void UpdateTime()
	{
		if (match == null)
		{
			return;
		}

		int min, sec, left, right;

		min = (int)match.MatchTimer / 60;
		sec = (int)match.MatchTimer % 60;

		// Minutes
		if (min > maxMinutes)
		{
			min = maxMinutes;
		}
		left = min / 10;
		right = min - (left * 10);

		if (timeMinuteLeft != null)
		{
			if ((left >= 0) && (left < 10))
			{
				timeMinuteLeft.text = numberStrings[left];
			}
		}
		if (timeMinuteRight != null)
		{
			if ((right >= 0) && (right < 10))
			{
				timeMinuteRight.text = numberStrings[right];
			}
		}

		// Seconds
		left = sec / 10;
		right = sec - (left * 10);

		if (timeSecondLeft != null)
		{
			if ((left >= 0) && (left < 10))
			{
				timeSecondLeft.text = numberStrings[left];
			}
		}
		if (timeSecondRight != null)
		{
			if ((right >= 0) && (right < 10))
			{
				timeSecondRight.text = numberStrings[right];
			}
		}
	}


	/// <summary>
	/// Updates the arrows.
	/// </summary>
	/// <returns>The arrows.</returns>
	/// <param name="dt">Dt.</param>
	public void UpdateArrows(float dt)
	{
		int i, n;
		SsTeam team;
		UnityEngine.UI.Image arrow;
		Vector2 localPoint, pos, edgePos, screenCentre;
		bool visible, gotEdge;
		float border;
		SsPlayer player;
		UnityEngine.UI.Image[] arrows;

		screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);

		// First loop is for pass arrows, second loop is for control arrows
		for (n = 0; n < 2; n++)
		{
			if (n == 0)
			{
				arrows = arrowPass;
			}
			else
			{
				arrows = arrowControl;
			}

			if ((arrows == null) || (arrows.Length <= 0))
			{
				continue;
			}

			// Loop through the arrows
			for (i = 0; i < 2; i++)
			{
				arrow = arrows[i];

				player = null;
				team = null;
				if ((match != null) && (match.Teams != null) && (i < match.Teams.Length))
				{
					team = match.Teams[i];

					if (n == 0)
					{
						player = team.PotentialPassPlayer;
					}
					else
					{
						player = team.ControlPlayer;
					}
				}

				if (arrow == null)
				{
					continue;
				}

				visible = false;

				border = Mathf.Max(arrow.rectTransform.rect.width, arrow.rectTransform.rect.height) / 2.0f;
				if (parentCanvas != null)
				{
					if (parentCanvas.scaleFactor > 0.0f)
					{
						border *= parentCanvas.scaleFactor;
					}
				}


				if ((rectTransform != null) && (player != null) && 
				    (player.IsPartiallyOffScreen))
				{
					gotEdge = false;
					pos = new Vector2(player.ScreenRect.center.x, player.ScreenRect.center.y);

					// Find which screen edge to attach the arrow
					if (GeUtils.LineSegmentIntersection(new Vector2(border, Screen.height - border),
					                                    new Vector2(Screen.width - border, Screen.height - border),
					                                    screenCentre, pos, out edgePos))
					{
						// Top
						gotEdge = true;
					}
					else if (GeUtils.LineSegmentIntersection(new Vector2(Screen.width - border, Screen.height - border),
					                                         new Vector2(Screen.width - border, border),
					                                         screenCentre, pos, out edgePos))
					{
						// Right
						gotEdge = true;
					}
					else if (GeUtils.LineSegmentIntersection(new Vector2(border, border),
					                                         new Vector2(Screen.width - border, border),
					                                         screenCentre, pos, out edgePos))
					{
						// Bottom
						gotEdge = true;
					}
					else if (GeUtils.LineSegmentIntersection(new Vector2(border, Screen.height - border),
					                                         new Vector2(border, border),
					                                         screenCentre, pos, out edgePos))
					{
						// Left
						gotEdge = true;
					}


					if ((gotEdge) && 
					    (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, 
					                                                         edgePos, 
					                                                         null, out localPoint)))
					{
						arrow.rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, arrow.rectTransform.localPosition.z);
						arrow.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, GeUtils.GetAngleFromVector(screenCentre - edgePos));
						visible = true;
					}
				}

				if (visible != arrow.gameObject.activeInHierarchy)
				{
					arrow.gameObject.SetActive(visible);
				}
			}
		}
	}


	/// <summary>
	/// Updates the popups.
	/// </summary>
	/// <returns>The goal.</returns>
	/// <param name="dt">Dt.</param>
	void UpdatePopups(float dt)
	{
		if ((goal != null) && (goal.IsVisible) && (goal.IsAnimating == false) && (goalVisibleTime > 0.0f))
		{
			goalVisibleTime -= dt;
			if (goalVisibleTime <= 0.0f)
			{
				goalVisibleTime = 0.0f;
				HideGoal();
			}
		}

		if ((IsVisible) && (IsAnimating == false) && 
		    (SsSettings.didShowControls == false) && 
		    (SsMsgBox.Instance != null) && (SsMsgBox.Instance.IsVisible == false) && 
		    (SsMsgBox.Instance.IsAnimating == false))
		{
			SsPauseMenu.ShowHelpPopup();
		}
	}


	/// <summary>
	/// Updates the mini map.
	/// </summary>
	/// <returns>The mini map.</returns>
	/// <param name="dt">Dt.</param>
	void UpdateMiniMap(float dt)
	{
		if ((miniMap == null) || (miniMapPlayArea == null) || 
		    (match == null) || (match.Teams == null))
		{
			return;
		}

		// Did the showMiniMap state change?
		if (miniMap.activeInHierarchy != SsSettings.showMiniMap)
		{
			miniMap.SetActive(SsSettings.showMiniMap);
		}

		if (miniMap.activeInHierarchy == false)
		{
			return;
		}

		Vector2 pos, scale;
		List<SsPlayer> players;
		SsPlayer player;
		SsTeam team;
		int i, n, max;
		UnityEngine.UI.Image icon;
		bool visible;

		scale.x = (miniMapPlayArea.rect.width / 2.0f);
		scale.y = (miniMapPlayArea.rect.height / 2.0f);

		if ((SsBall.Instance != null) && (miniMapBall != null))
		{
			pos = SsBall.Instance.RadarPos;
			pos.x = (miniMapPlayArea.rect.center.x / scale.x) + (pos.x * scale.x);
			pos.y = (miniMapPlayArea.rect.center.y / scale.y) + (pos.y * scale.y);
			miniMapBall.rectTransform.localPosition = new Vector3(pos.x, pos.y, miniMapBall.rectTransform.localPosition.z);
		}


		for (i = 0; i < 2; i++)
		{
			team = match.Teams[i];
			if (team == null)
			{
				continue;
			}
			players = team.Players;
			if ((players != null) && (players.Count > 0))
			{
				max = miniMapPlayers.GetLength(1);

				for (n = 0; n < max; n++)
				{
					icon = miniMapPlayers[i, n];
					player = players[n];

					if (icon != null)
					{
						if (player != null)
						{
							visible = player.gameObject.activeInHierarchy;

							pos = player.RadarPos;
							pos.x = (miniMapPlayArea.rect.center.x / scale.x) + (pos.x * scale.x);
							pos.y = (miniMapPlayArea.rect.center.y / scale.y) + (pos.y * scale.y);
							icon.rectTransform.localPosition = new Vector3(pos.x, pos.y, icon.rectTransform.localPosition.z);
						}
						else
						{
							visible = false;
						}

						if (icon.gameObject.activeInHierarchy != visible)
						{
							icon.gameObject.SetActive(visible);
						}
					}
				}
			}
		}
	}
}
