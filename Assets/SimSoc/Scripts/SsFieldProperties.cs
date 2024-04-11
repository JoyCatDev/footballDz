using UnityEngine;
using System.Collections;

/// <summary>
/// Soccer field properties. Must be in the scene that contains the field/pitch/stadium. It contains properties for the 
/// field (e.g. kick off centre, playing area size, position of penalty kick, etc.).
/// </summary>
public class SsFieldProperties : MonoBehaviour {

	// Const/Static
	//-------------
	public const int maxZones = 8;			// Max number of vertical zones. NOTE: DO NOT CHANGE THIS!

	public const int maxRows = 11;			// Max number of horizontal rows. Must be an odd number, so there's one in the middle.
	public const float rowOverlapPercent = 30.0f;	// Percent rows overlap

	public const int gridSizeMin = 4;		// Min width/height of grid
	public const int gridSizeMax = 30;		// Max width/height of grid

	private const int gridSearchDataPoolSize = 50;		// Size of the gridSearchDataPool. Should at least be large enough for all players.


	// Classes
	//--------
	// Grid for AI
	public class SsGrid
	{
		public Bounds bounds;					// World bounds
		public Rect rect;						// World rect (on XZ axis, so Y/height is used as Z)
		public int playerWeight;				// Number of players in the grid
		public int x;							// x index in grids array
		public int y;							// y index in grids array
	}

	// Grid search results.
	// IMPORTANT: Avoid creating new ones during the match, rather use GetGridSearchDataFromPool to reduce garbage collection.
	public class SsGridSearchData
	{
		public SsGrid grid;						// The found grid
		public int gridWeight;					// Grid weight including the surrounding grids' weights

		// Methods
		//--------
		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			grid = null;
			gridWeight = 0;
		}
	}


	// Public
	//-------
	// Prefabs
	[Header("Prefabs")]
	[Tooltip("Match prefabs.")]
	public SsMatchPrefabs matchPrefabs;

	[Header("Players")]
	[Tooltip("Override the default number of players per side, which is specified in the Match Settings. This can only be less than the default.")]
	public bool overrideNumPlayersPerSide = false;
	[Tooltip("Override the number of players per team to have in the match (e.g. can have a 6 per side match, or a 11 per side match).")]
	[Range(SsTeam.minPlayers, SsTeam.maxPlayers)]
	public int numPlayersPerSide = SsTeam.maxPlayers;	// NOTE: Use field.NumPlayersPerSide to read this. It checks if override or if match settings value must be used.

	// Dimensions and Areas
	[Header("Dimensions and Areas")]
	[Tooltip("Width of the goal bars. These affect aim calculations when players shoot towards the posts. They are used in combination with the goal post area colliders.")]
	public float goalBarWidth = 0.3f;

	[Tooltip("Corner circle radius.")]
	public float cornerRadius = 0.91f;

	[Tooltip("Box collider that defines the play area.")]
	public BoxCollider playArea;

	[Tooltip("Sphere collider that defines the centre mark and centre radius.")]
	public SphereCollider centre;


	[Tooltip("Box collider that defines the left goal posts area. If ball goes into this area then it is a goal.")]
	public BoxCollider leftGoalPosts;

	[Tooltip("Box collider that defines the left goal area.")]
	public BoxCollider leftGoalArea;

	[Tooltip("Box collider that defines the left penalty area.")]
	public BoxCollider leftPenaltyArea;

	[Tooltip("Position of the left penalty mark.")]
	public GameObject leftPenaltyMark;


	[Tooltip("Box collider that defines the right goal posts area. If ball goes into this area then it is a goal.")]
	public BoxCollider rightGoalPosts;

	[Tooltip("Box collider that defines the right goal area.")]
	public BoxCollider rightGoalArea;

	[Tooltip("Box collider that defines the right penalty area.")]
	public BoxCollider rightPenaltyArea;

	[Tooltip("Position of the right penalty mark.")]
	public GameObject rightPenaltyMark;

	[Space(10)]
	[Tooltip("Allow the ball to go out the top of the field, for this distance. Only if no player has the ball.")]
	public float allowBallOutTop;
	[Tooltip("Allow the ball to go out the bottom of the field, for this distance. Only if no player has the ball.")]
	public float allowBallOutBottom;
	[Tooltip("Allow the ball to go out the left of the field, for this distance. Only if no player has the ball.")]
	public float allowBallOutLeft;
	[Tooltip("Allow the ball to go out the right of the field, for this distance. Only if no player has the ball.")]
	public float allowBallOutRight;


	// Audio
	[Header("Audio")]
	[Tooltip("Music to play during the match. This overrides the default music specified in the Match Prefabs.")]
	public AudioClip music;
	[Tooltip("Override the default music volume.")]
	public bool overrideDefaultMusicVolume = false;
	[Tooltip("Music volume override.")]
	public float musicVolume = 1.0f;
	
	[Tooltip("Override the default sfx volume.")]
	public bool overrideDefaultSfxVolume = false;
	[Tooltip("Sfx volume override.")]
	public float sfxVolume = 1.0f;
	
	[Tooltip("AudioSource for music. If empty then one will be added.")]
	public AudioSource audioSource;
	[Tooltip("AudioSource for SFX. If empty then one will be added.")]
	public AudioSource audioSourceSfx;
	
	[Space(10)]
	[Tooltip("Ambient sounds.")]
	public AudioClip[] ambientSounds;
	[Tooltip("Volume of the ambient sounds.")]
	public float ambientVolume = 1.0f;
	[Tooltip("Loop the ambient sounds.")]
	public bool loopAmbientSounds = true;


	// Particles
	[Header("Particles")]
	[Tooltip("Particle for slide tackle.")]
	public ParticleSystem slideTacklePrefab;


	// Misc
	[Header("Misc")]
	[Tooltip("Affects how far players slide/skid. \n" + 
	         "Example: 1 = normal, 0.5 = half normal distance (sand paper), 2 = double normal distance (ice)")]
	public float frictionScale = 1.0f;

	[Tooltip("Camera will be positioned and rotated the same as this game object, at the start of the scene.")]
	public GameObject cameraStartPosition;


	// Grid
	[Header("Grid")]
	[Tooltip("Number of grid blocks across. The grid is used by AI to find empty spots. A larger number may slow down AI.")]
	[Range(gridSizeMin, gridSizeMax)]
	public int gridWidth = 16;
	[Tooltip("Number of grid blocks down. The grid is used by AI to find empty spots. A larger number may slow down AI.")]
	[Range(gridSizeMin, gridSizeMax)]
	public int gridHeight = 10;



	// Private
	//--------
	static private SsFieldProperties instance;

	private SsMatch match;							// Reference to the match
	private SsBall ball;							// Reference to the ball

	private float groundY;					// Ground Y position.
	
	private Bounds _playArea;				// Field play area. Ball is considered out if it leaves this area.
	private Bounds leftHalfArea;			// Field left half play area. X decreases to the left and increases to right.
	private Bounds rightHalfArea;			// Field right half play area. X decreases to the left and increases to right.
	
	private Vector3 centreMark;				// Kick off centre of field
	private float centreRadius;				// Radius of circle around the centre mark (for positioning players)
	
	private Bounds _leftGoalPosts;			// Left goal posts area. If ball goes into this area then it is a goal.
	private Bounds _rightGoalPosts;			// Right goal posts area. If ball goes into this area then it is a goal.
	
	private Bounds _leftGoalArea;			// Left goal area
	private Bounds _rightGoalArea;			// Right goal area
	
	private Bounds _leftPenaltyArea;		// Left penalty area
	private Bounds _rightPenaltyArea;		// Right penalty area
	
	private Vector3 _leftPenaltyMark;		// Left penalty mark (position to place the ball for penalty kick)
	private Vector3 _rightPenaltyMark;		// Right penalty mark (position to place the ball for penalty kick)

	// Field is split into 8 vertical zones. Zones run from left to right. Helps with AI and general location logic.
	private Bounds[] zones = new Bounds[maxZones];
	private Vector3 zoneSize;				// Size of a zone

	private Bounds[] rows;						// Rows used for AI and formations. Field is divided into horizontal rows.
	private Vector3 rowSize;					// Row size, excluding the overlap.
	private Vector3 rowSizeOverlap;				// Row size, including the overlap. (NOTE: Rows overlap slightly.)

	private SsGrid [,]grids;				// Grid used for AI. Field is divided into a 16x10 (width x height) grid.
	private Vector3 gridStartPosTopLeft;	// Position of first grid's top-left corner, in top-left corner of field.
	private Vector3 gridSize;				// Grid size.

	// Array to use in FindGridsInCircle. We re-use the array, instead of a local variable, to reduce garbage collection.
	private SsGrid[] gridSearchArray;

	// Array of SsGridSearchData to re-use, so it is Not allocated during the match, to reduce garbage collection.
	private SsGridSearchData[] gridSearchDataPool;
	private int gridSearchDataPoolIndex = -1;




	// Properties
	//-----------
	static public SsFieldProperties Instance
	{
		get { return(instance); }
	}


	/// <summary>
	/// Get the number players per side. This checks if the field overrides the default number of players and returns the relevant number.
	/// </summary>
	/// <value>The number players per side.</value>
	public int NumPlayersPerSide
	{
		get
		{
			int num = SsMatchSettings.Instance.numPlayersPerSide;
			if ((overrideNumPlayersPerSide) && (numPlayersPerSide < SsMatchSettings.Instance.numPlayersPerSide))
			{
				num = numPlayersPerSide;
			}
			return (Mathf.Clamp(num, SsTeam.minPlayers, SsTeam.maxPlayers));
		}
	}


	public float GroundY
	{
		get { return(groundY); }
	}


	public Bounds PlayArea
	{
		get { return(_playArea); }
	}


	public Bounds LeftHalfArea
	{
		get { return(leftHalfArea); }
	}


	public Bounds RightHalfArea
	{
		get { return(rightHalfArea); }
	}
	
	
	public Vector3 CentreMark
	{
		get { return(centreMark); }
	}
	
	
	public float CentreRadius
	{
		get { return(centreRadius); }
	}
	
	
	public Bounds LeftGoalPosts
	{
		get { return(_leftGoalPosts); }
	}
	
	
	public Bounds RightGoalPosts
	{
		get { return(_rightGoalPosts); }
	}
	
	
	public Bounds LeftGoalArea
	{
		get { return(_leftGoalArea); }
	}
	
	
	public Bounds RightGoalArea
	{
		get { return(_rightGoalArea); }
	}
	
	
	public Bounds LeftPenaltyArea
	{
		get { return(_leftPenaltyArea); }
	}
	
	
	public Bounds RightPenaltyArea
	{
		get { return(_rightPenaltyArea); }
	}
	
	
	public Vector3 LeftPenaltyMark
	{
		get { return(_leftPenaltyMark); }
	}
	
	
	public Vector3 RightPenaltyMark
	{
		get { return(_rightPenaltyMark); }
	}


	public bool IsSlippery
	{
		get { return(frictionScale > 1.0f); }
	}


	public float ZoneWidth
	{
		get { return(zoneSize.x); }
	}



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;

		if (SsSettings.Instance == null)
		{
			// Possibly running the scene in the editor, so make sure persistent prefabs have been spawned (e.g. settings).
			SsSpawnPersistentPrefabs.SpawnPrefabs();
		}

		Init();



		// Editor warnings
		//----------------
#if UNITY_EDITOR
		if (SsSettings.Instance == null)
		{
			Debug.LogError("ERROR: Could not find the settings. Make sure the Spawn Persistent Prefabs is in the scene.");
		}

		if (matchPrefabs == null)
		{
			Debug.LogError("ERROR: Match Prefabs has not been set on the field properties.");
		}

		int m = (maxRows % 2);
		if (m != 1)
		{
			Debug.LogError("ERROR: The field's Max Rows (" + maxRows + ") must be an odd number: " + GeUtils.GetLoadedLevelName());
		}

		if ((SsSettings.Instance != null) && (SsSettings.Instance.disableAi))
		{
			Debug.LogWarning("WARNING: AI has been disabled (in the editor). You can change it on the Global Settings.");
		}

		if (SsSettings.LogInfoToConsole)
		{
			int level = QualitySettings.GetQualityLevel();
			string[] names = QualitySettings.names;
			string msg = ((names != null) && (level >= 0) && (level < names.Length)) ? names[level] : "";
			Debug.Log("Quality settings level: " + level + " (" + msg + ")");

			SsSceneManager.SsFieldSceneResource fieldRes = null;
			if (SsSceneManager.Instance != null)
			{
				fieldRes = SsSceneManager.Instance.GetLoadedField();
			}
			Debug.Log("Field: Scene: " + GeUtils.GetLoadedLevelName() + 
			          ",      Display Name: " + ((fieldRes != null) ? fieldRes.displayName : "") + 
			          ",      ID: " + ((fieldRes != null) ? fieldRes.id : ""));
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		GetReferences();

		SpawnObjects();
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
		}
		if (ball == null)
		{
			ball = SsBall.Instance;
		}

		if (SsFormationManager.Instance != null)
		{
			SsFormationManager.Instance.OnPreMatchStart();
		}
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
		ball = null;

		CleanupGrid();
	}


	/// <summary>
	/// Clean up the grid memory resources.
	/// </summary>
	/// <returns>The grid.</returns>
	void CleanupGrid()
	{
		int i, x, y;

		ClearGridWeights();

		if (grids != null)
		{
			for (x = 0; x < grids.GetLength(0); x++)
			{
				for (y = 0; y < grids.GetLength(1); y++)
				{
					grids[x, y] = null;
				}
			}
		}
		grids = null;

		if ((gridSearchArray != null) && (gridSearchArray.Length > 0))
		{
			for (i = 0; i < gridSearchArray.Length; i++)
			{
				gridSearchArray[i] = null;
			}
		}
		gridSearchArray = null;


		if ((gridSearchDataPool != null) && (gridSearchDataPool.Length > 0))
		{
			for (i = 0; i < gridSearchDataPool.Length; i++)
			{
				gridSearchDataPool[i] = null;
			}
		}
		gridSearchDataPool = null;
	}


	/// <summary>
	/// Init this instance.
	/// </summary>
	void Init()
	{
		RaycastHit hitInfo;
		int i;
		Transform child;
		bool foundGround = false;
		Vector3 size;

		groundY = 0.0f;


		// Disable all children
		for (i = 0; i < transform.childCount; i++)
		{
			child = transform.GetChild(i);
			if (child != null)
			{
				child.gameObject.SetActive(false);
			}
		}


		if (centre != null)
		{
			centreMark = centre.transform.position + centre.center;
			centreRadius = centre.radius * Mathf.Max(centre.transform.lossyScale.x, 
			                                         Mathf.Max(centre.transform.lossyScale.y, centre.transform.lossyScale.z));

			if (Physics.Raycast(centre.transform.position + centre.center + Vector3.up, -Vector3.up, out hitInfo, 1000.0f))
			{
				foundGround = true;
				groundY = hitInfo.point.y;
				centreMark.y = GroundY;
			}
		}


		if (playArea != null)
		{
			size = new Vector3(playArea.size.x * playArea.transform.lossyScale.x,
			                   playArea.size.y * playArea.transform.lossyScale.y, 
			                   playArea.size.z * playArea.transform.lossyScale.z);

			_playArea = new Bounds(new Vector3(playArea.transform.position.x + playArea.center.x, 
			                                   GroundY, 
			                                   playArea.transform.position.z + playArea.center.z), 
			                       				size);

			leftHalfArea = new Bounds(new Vector3(_playArea.center.x - (_playArea.size.x / 4.0f), 
			                                      _playArea.center.y, 
			                                      _playArea.center.z),
			                          new Vector3(_playArea.size.x / 2.0f, _playArea.size.y, _playArea.size.z));
			rightHalfArea = new Bounds(new Vector3(_playArea.center.x + (_playArea.size.x / 4.0f), 
			                                       _playArea.center.y, 
			                                       _playArea.center.z),
			                           new Vector3(_playArea.size.x / 2.0f, _playArea.size.y, _playArea.size.z));

			CreateZones();
			CreateRows();
			CreateGrid();
		}

		
		if (leftGoalPosts != null)
		{
			size = new Vector3(leftGoalPosts.size.x * leftGoalPosts.transform.lossyScale.x,
			                   leftGoalPosts.size.y * leftGoalPosts.transform.lossyScale.y, 
			                   leftGoalPosts.size.z * leftGoalPosts.transform.lossyScale.z);

			_leftGoalPosts = new Bounds(leftGoalPosts.transform.position + leftGoalPosts.center, size);
		}

		
		if (leftGoalArea != null)
		{
			size = new Vector3(leftGoalArea.size.x * leftGoalArea.transform.lossyScale.x,
			                   leftGoalArea.size.y * leftGoalArea.transform.lossyScale.y, 
			                   leftGoalArea.size.z * leftGoalArea.transform.lossyScale.z);

			_leftGoalArea = new Bounds(leftGoalArea.transform.position + leftGoalArea.center, size);
		}

		
		if (leftPenaltyArea != null)
		{
			size = new Vector3(leftPenaltyArea.size.x * leftPenaltyArea.transform.lossyScale.x,
			                   leftPenaltyArea.size.y * leftPenaltyArea.transform.lossyScale.y, 
			                   leftPenaltyArea.size.z * leftPenaltyArea.transform.lossyScale.z);

			_leftPenaltyArea = new Bounds(leftPenaltyArea.transform.position + leftPenaltyArea.center, size);
		}

		
		if (leftPenaltyMark != null)
		{
			if (foundGround)
			{
				_leftPenaltyMark = new Vector3(leftPenaltyMark.transform.position.x, 
				                               GroundY, 
				                               leftPenaltyMark.transform.position.z);
			}
			else
			{
				_leftPenaltyMark = leftPenaltyMark.transform.position;
			}
		}
		
		
		if (rightGoalPosts != null)
		{
			size = new Vector3(rightGoalPosts.size.x * rightGoalPosts.transform.lossyScale.x,
			                   rightGoalPosts.size.y * rightGoalPosts.transform.lossyScale.y, 
			                   rightGoalPosts.size.z * rightGoalPosts.transform.lossyScale.z);

			_rightGoalPosts = new Bounds(rightGoalPosts.transform.position + rightGoalPosts.center, size);
		}

		
		if (rightGoalArea != null)
		{
			size = new Vector3(rightGoalArea.size.x * rightGoalArea.transform.lossyScale.x,
			                   rightGoalArea.size.y * rightGoalArea.transform.lossyScale.y, 
			                   rightGoalArea.size.z * rightGoalArea.transform.lossyScale.z);

			_rightGoalArea = new Bounds(rightGoalArea.transform.position + rightGoalArea.center, size);
		}

		
		if (rightPenaltyArea != null)
		{
			size = new Vector3(rightPenaltyArea.size.x * rightPenaltyArea.transform.lossyScale.x,
			                   rightPenaltyArea.size.y * rightPenaltyArea.transform.lossyScale.y, 
			                   rightPenaltyArea.size.z * rightPenaltyArea.transform.lossyScale.z);

			_rightPenaltyArea = new Bounds(rightPenaltyArea.transform.position + rightPenaltyArea.center, size);
		}

		
		if (rightPenaltyMark != null)
		{
			if (foundGround)
			{
				_rightPenaltyMark = new Vector3(rightPenaltyMark.transform.position.x, 
				                                GroundY, 
				                                rightPenaltyMark.transform.position.z);
			}
			else
			{
				_rightPenaltyMark = rightPenaltyMark.transform.position;
			}
		}



		// Editor checks
		//--------------
#if UNITY_EDITOR
		string debugMsg;
		float leftDiff, rightDiff;
		leftDiff = Mathf.Abs(centreMark.x - leftHalfArea.max.x);
		rightDiff = Mathf.Abs(centreMark.x - rightHalfArea.min.x);
		if ((leftDiff > 0.5f) || (rightDiff > 0.5f))
		{
			debugMsg = string.Format("Please note: The field's centre mark is more than half a metre from the centre of the " + 
			                         "play area. The centre mark will be used as the centre of the field. " + 
			                         "(Distance from centre: {0},      Field scene: {1})", 
			                         Mathf.Max(leftDiff, rightDiff),
			                         GeUtils.GetLoadedLevelName());
			Debug.LogWarning(debugMsg);
		}

#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Inits the audio.
	/// </summary>
	/// <returns>The audio.</returns>
	void InitAudio()
	{
		SsVolumeController.FadeVolumeIn(SsSettings.volume, 
		                                SsVolumeController.matchMusicFadeInTime, 
		                                SsVolumeController.matchMusicFadeInDelay);

		if (matchPrefabs == null)
		{
			return;
		}

		AudioClip selectedMusic = null;
		int i;
		AudioSource tempAudioSource;
		AudioClip clip;


		// Sfx
		//----
		if (audioSourceSfx == null)
		{
			audioSourceSfx = gameObject.AddComponent<AudioSource>();
			if (audioSourceSfx != null)
			{
				audioSourceSfx.playOnAwake = false;

				if (overrideDefaultSfxVolume)
				{
					audioSourceSfx.volume = sfxVolume;
				}
				else
				{
					audioSourceSfx.volume = matchPrefabs.sfxVolume;
				}

				if (matchPrefabs.use2dAudio)
				{
					// 2D audio
					audioSourceSfx.spatialBlend = 0.0f;
				}
			}
		}


		// Music
		//------
		if ((matchPrefabs.music == null) && (music == null))
		{
			// No music
		}
		else if (matchPrefabs.music == null)
		{
			selectedMusic = music;
		}
		else if (music == null)
		{
			selectedMusic = matchPrefabs.music;
		}
		else
		{
			// Override the default music
			selectedMusic = music;

#if UNITY_EDITOR
			if ((music != matchPrefabs.music) && (matchPrefabs.music.preloadAudioData))
			{
				Debug.LogError("SERIOUS WARNING: The default match music clip's audio data is pre-loaded, and a field overrides the default music. " + 
				               "This means both are loaded into memory. It is recommended that you untick Preload Audio Data on the default clip [" + matchPrefabs.music + "].");
			}
#endif //UNITY_EDITOR
		}

		if (selectedMusic != null)
		{
			bool didExist = (audioSource != null);
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
			}
			if (audioSource != null)
			{
				if (selectedMusic.preloadAudioData == false)
				{
					selectedMusic.LoadAudioData();
				}

				if (didExist == false)
				{
					audioSource.priority = 0;	// Music has the highest priority
				}

				audioSource.clip = selectedMusic;
				audioSource.loop = matchPrefabs.loopMusic;

				if (overrideDefaultMusicVolume)
				{
					audioSource.volume = musicVolume;
				}
				else
				{
					audioSource.volume = matchPrefabs.musicVolume;
				}

				if (matchPrefabs.use2dAudio)
				{
					// 2D audio
					audioSource.spatialBlend = 0.0f;
				}

				audioSource.Play();
			}
		}


		// Ambient
		//--------
		if ((ambientSounds != null) && (ambientSounds.Length > 0))
		{
			for (i = 0; i < ambientSounds.Length; i++)
			{
				clip = ambientSounds[i];
				if (clip != null)
				{
					tempAudioSource = gameObject.AddComponent<AudioSource>();
					if (tempAudioSource != null)
					{
						tempAudioSource.clip = clip;
						tempAudioSource.loop = loopAmbientSounds;
						tempAudioSource.volume = ambientVolume;

						if (matchPrefabs.use2dAudio)
						{
							// 2D audio
							tempAudioSource.spatialBlend = 0.0f;
						}

						tempAudioSource.Play();
					}
				}
			}
		}
	}


	/// <summary>
	/// Play the sfx.
	/// </summary>
	/// <returns>The sfx.</returns>
	/// <param name="clip">Clip.</param>
	public void PlaySfx(AudioClip clip)
	{
		if ((audioSourceSfx != null) && (clip != null))
		{
			//audioSourceSfx.Stop();
			audioSourceSfx.PlayOneShot(clip);
		}
	}


	/// <summary>
	/// Creates the zones.
	/// </summary>
	/// <returns>The zones.</returns>
	void CreateZones()
	{
		int i;
		Vector3 pos;

		zoneSize = new Vector3(_playArea.size.x / (float)maxZones, _playArea.size.y, _playArea.size.z);
		pos = new Vector3(_playArea.min.x + (zoneSize.x / 2.0f), GroundY, _playArea.center.z);
		for (i = 0; i < maxZones; i++)
		{
			zones[i] = new Bounds(pos, zoneSize);
			pos.x += zoneSize.x;
		}
	}


	/// <summary>
	/// Creates the rows.
	/// </summary>
	/// <returns>The rows.</returns>
	void CreateRows()
	{
		int i, half;
		Vector3 pos, tempSize;

		rows = new Bounds[maxRows];
		tempSize = new Vector3(_playArea.size.x, _playArea.size.y, _playArea.size.z / (float)maxRows);
		rowSize = tempSize;
		rowSizeOverlap = new Vector3(tempSize.x, tempSize.y, tempSize.z + (tempSize.z * (rowOverlapPercent / 100.0f)));

		half = (int)((float)maxRows / 2.0f);
		pos = new Vector3(_playArea.center.x, 
		                  GroundY, 
		                  _playArea.center.z + (tempSize.z * half));
		for (i = 0; i < maxRows; i++)
		{
			rows[i] = new Bounds(pos, rowSizeOverlap);

			// Clamp rows at top and bottom of field
			if (rows[i].max.z > _playArea.max.z)
			{
				rows[i].max = new Vector3(rows[i].max.x, rows[i].max.y, _playArea.max.z);
			}
			if (rows[i].min.z < _playArea.min.z)
			{
				rows[i].min = new Vector3(rows[i].min.x, rows[i].min.y, _playArea.min.z);
			}

			pos.z -= tempSize.z;
		}
	}


	/// <summary>
	/// Creates the grid.
	/// </summary>
	/// <returns>The grid.</returns>
	void CreateGrid()
	{
		int i, x, y;
		Vector3 pos, startPos;
		SsGrid grid;

		CleanupGrid();

		gridWidth = Mathf.Clamp(gridWidth, gridSizeMin, gridSizeMax);
		gridHeight = Mathf.Clamp(gridHeight, gridSizeMin, gridSizeMax);

		grids = new SsGrid[gridWidth, gridHeight];
		gridSearchArray = new SsGrid[gridWidth * gridHeight];

		gridSize = new Vector3(_playArea.size.x / (float)gridWidth, 
		                       1.0f,
		                       _playArea.size.z / (float)gridHeight);

		// Start pos, top-left corner of first grid in top-left corner of field
		gridStartPosTopLeft = new Vector3(_playArea.min.x, GroundY, _playArea.max.z);

		// Start pos, centre of first grid in top-left corner of field
		startPos = new Vector3(_playArea.min.x + (gridSize.x / 2.0f), GroundY, _playArea.max.z - (gridSize.z / 2.0f));

		pos = startPos;
		for (y = 0; y < gridHeight; y++)
		{
			pos.x = startPos.x;
			for (x = 0; x < gridWidth; x++)
			{
				grids[x, y] = new SsGrid();
				grid = grids[x, y];
				if (grid != null)
				{
					grid.bounds = new Bounds(pos, gridSize);
					// REMINDER: Rect first parameter is the rect's top-left corner
					grid.rect = new Rect(new Vector2(pos.x - (gridSize.x / 2.0f), pos.z - (gridSize.z / 2.0f)), 
					                     new Vector2(gridSize.x, gridSize.z));
					grid.playerWeight = 0;
					grid.x = x;
					grid.y = y;
				}

				pos.x += gridSize.x;
			}
			pos.z -= gridSize.z;
		}


		// Grid search data pool
		gridSearchDataPool = new SsGridSearchData[gridSearchDataPoolSize];
		for (i = 0; i < gridSearchDataPool.Length; i++)
		{
			gridSearchDataPool[i] = new SsGridSearchData();
		}
	}


	/// <summary>
	/// Spawns the objects.
	/// </summary>
	/// <returns>The objects.</returns>
	private void SpawnObjects()
	{
		SsMarkersManager.CreateInstance();

		if (matchPrefabs != null)
		{
			SsMatchPrefabs.SpawnObjects(matchPrefabs);
		}

		// Position match camera
		if ((cameraStartPosition != null) && (SsMatchCamera.Instance != null))
		{
			SsMatchCamera.Instance.transform.position = cameraStartPosition.transform.position;
			SsMatchCamera.Instance.transform.rotation = cameraStartPosition.transform.rotation;
		}

		InitAudio();
	}


	/// <summary>
	/// Get a SsGridSearchData object from the pool, to re-use, and reduce garbage collection.
	/// Use this instead of creating a new object.
	/// </summary>
	/// <returns>The grid search data from pool.</returns>
	public SsGridSearchData GetGridSearchDataFromPool()
	{
		if ((gridSearchDataPool == null) || (gridSearchDataPool.Length <= 0))
		{
			// Fail safe
			return (new SsGridSearchData());
		}

		gridSearchDataPoolIndex ++;

		if ((gridSearchDataPoolIndex < 0) || 
		    (gridSearchDataPoolIndex >= gridSearchDataPool.Length))
		{
			gridSearchDataPoolIndex = 0;
		}

		gridSearchDataPool[gridSearchDataPoolIndex].Init();

		return (gridSearchDataPool[gridSearchDataPoolIndex]);
	}


	/// <summary>
	/// Clears the grid weights. Also clears both teams' players' grids.
	/// </summary>
	/// <returns>The grid weights.</returns>
	public void ClearGridWeights()
	{
		int x, y;

		if (match != null)
		{
			if (match.LeftTeam != null)
			{
				match.LeftTeam.ClearPlayersGrid();
			}
			if (match.RightTeam != null)
			{
				match.RightTeam.ClearPlayersGrid();
			}
		}

		if (grids != null)
		{
			for (x = 0; x < grids.GetLength(0); x++)
			{
				for (y = 0; y < grids.GetLength(1); y++)
				{
					grids[x, y].playerWeight = 0;
				}
			}
		}
	}


	/// <summary>
	/// Test if the game object is inside the penalty area.
	/// </summary>
	/// <returns><c>true</c> if this instance is in penalty area the specified go team; otherwise, <c>false</c>.</returns>
	/// <param name="go">Go.</param>
	/// <param name="team">Team whose penalty area should be used.</param>
	public bool IsInPenaltyArea(GameObject go, SsTeam team)
	{
		if ((go == null) || (team == null))
		{
			return (false);
		}

		if (team.PlayDirection > 0)
		{
			if ((go.transform.position.x < _leftPenaltyArea.min.x) || (go.transform.position.x > _leftPenaltyArea.max.x) || 
			    (go.transform.position.z < _leftPenaltyArea.min.z) || (go.transform.position.z > _leftPenaltyArea.max.z))
			{
				return (false);
			}
		}
		else
		{
			if ((go.transform.position.x < _rightPenaltyArea.min.x) || (go.transform.position.x > _rightPenaltyArea.max.x) || 
			    (go.transform.position.z < _rightPenaltyArea.min.z) || (go.transform.position.z > _rightPenaltyArea.max.z))
			{
				return (false);
			}
		}

		return (true);
	}


	/// <summary>
	/// Test if the point is inside the penalty area.
	/// </summary>
	/// <returns><c>true</c> if this instance is in penalty area the specified point team; otherwise, <c>false</c>.</returns>
	/// <param name="point">Point.</param>
	/// <param name="team">Team.</param>
	public bool IsInPenaltyArea(Vector3 point, SsTeam team)
	{
		if (team == null)
		{
			return (false);
		}
		
		if (team.PlayDirection > 0)
		{
			if ((point.x < _leftPenaltyArea.min.x) || (point.x > _leftPenaltyArea.max.x) || 
			    (point.z < _leftPenaltyArea.min.z) || (point.z > _leftPenaltyArea.max.z))
			{
				return (false);
			}
		}
		else
		{
			if ((point.x < _rightPenaltyArea.min.x) || (point.x > _rightPenaltyArea.max.x) || 
			    (point.z < _rightPenaltyArea.min.z) || (point.z > _rightPenaltyArea.max.z))
			{
				return (false);
			}
		}
		
		return (true);
	}


	/// <summary>
	/// Test if the game object is inside the goal area (the area infront of the goal posts).
	/// </summary>
	/// <returns><c>true</c> if this instance is in goal area the specified go team; otherwise, <c>false</c>.</returns>
	/// <param name="go">Go.</param>
	/// <param name="team">Team whose penalty area should be used.</param>
	public bool IsInGoalArea(GameObject go, SsTeam team)
	{
		if ((go == null) || (team == null))
		{
			return (false);
		}
		
		if (team.PlayDirection > 0)
		{
			if ((go.transform.position.x < _leftGoalArea.min.x) || (go.transform.position.x > _leftGoalArea.max.x) || 
			    (go.transform.position.z < _leftGoalArea.min.z) || (go.transform.position.z > _leftGoalArea.max.z))
			{
				return (false);
			}
		}
		else
		{
			if ((go.transform.position.x < _rightGoalArea.min.x) || (go.transform.position.x > _rightGoalArea.max.x) || 
			    (go.transform.position.z < _rightGoalArea.min.z) || (go.transform.position.z > _rightGoalArea.max.z))
			{
				return (false);
			}
		}
		
		return (true);
	}


	/// <summary>
	/// Test if the ball is before the halfway mark.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball before halfway the specified team; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team whose play direction should be used.</param>
	public bool IsBallBeforeHalfway(SsTeam team)
	{
		if ((ball != null) && (team != null))
		{
			if (((team.PlayDirection > 0) && (ball.transform.position.x < centreMark.x)) || 
			    ((team.PlayDirection < 0) && (ball.transform.position.x >= centreMark.x)))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Test if the ball is past the halfway mark.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball past halfway the specified team; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team whose play direction should be used.</param>
	public bool IsBallPastHalfway(SsTeam team)
	{
		if ((ball != null) && (team != null))
		{
			if (((team.PlayDirection > 0) && (ball.transform.position.x >= centreMark.x)) || 
			    ((team.PlayDirection < 0) && (ball.transform.position.x < centreMark.x)))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Test if the ball is before the zone.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball before zone the specified team zone; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team whose play direction should be used.</param>
	/// <param name="zone">Zone.</param>
	public bool IsBallBeforeZone(SsTeam team, int zone)
	{
		if ((ball != null) && (team != null))
		{
			if (((team.PlayDirection > 0) && (ball.Zone < zone)) ||
			    ((team.PlayDirection < 0) && (ball.Zone >= maxZones - zone)))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Test if the ball is past the zone.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball past zone the specified team zone; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team whose play direction should be used.</param>
	/// <param name="zone">Zone.</param>
	public bool IsBallPastZone(SsTeam team, int zone)
	{
		if ((ball != null) && (team != null))
		{
			if (((team.PlayDirection > 0) && (ball.Zone >= zone)) ||
			    ((team.PlayDirection < 0) && (ball.Zone < maxZones - zone)))
			{
				return (true);
			}
		}
		return (false);
	}


	/// <summary>
	/// Test if the ball is in the play area.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball in play area; otherwise, <c>false</c>.</returns>
	/// <param name="includeAllowOut">Include the field's "allowBallOut" properties in the test, which allows ball to go out slightly. Ignored when a player has the ball.</param>
	/// <param name="includeRadius">Include the ball's radius in the test.</param>
	public bool IsBallInPlayArea(bool includeAllowOut = true, bool includeRadius = false)
	{
		GameObject go = ball.gameObject;

		if (match.BallPlayer != null)
		{
			includeAllowOut = false;
		}

		if ((includeAllowOut) && (includeRadius == false))
		{
			if ((go.transform.position.x < _playArea.min.x - allowBallOutLeft) || 
			    (go.transform.position.x > _playArea.max.x + allowBallOutRight) || 
			    (go.transform.position.z < _playArea.min.z - allowBallOutBottom) || 
			    (go.transform.position.z > _playArea.max.z + allowBallOutTop))
			{
				return (false);
			}
		}
		else if ((includeAllowOut == false) && (includeRadius == false))
		{
			if ((go.transform.position.x < _playArea.min.x) || 
			    (go.transform.position.x > _playArea.max.x) || 
			    (go.transform.position.z < _playArea.min.z) || 
			    (go.transform.position.z > _playArea.max.z))
			{
				return (false);
			}
		}
		else if ((includeAllowOut == false) && (includeRadius))
		{
			if ((go.transform.position.x - ball.radius < _playArea.min.x) || 
			    (go.transform.position.x + ball.radius > _playArea.max.x) || 
			    (go.transform.position.z - ball.radius < _playArea.min.z) || 
			    (go.transform.position.z + ball.radius > _playArea.max.z))
			{
				return (false);
			}
		}
		else if ((includeAllowOut) && (includeRadius))
		{
			if ((go.transform.position.x - ball.radius < _playArea.min.x - allowBallOutLeft) || 
			    (go.transform.position.x + ball.radius > _playArea.max.x + allowBallOutRight) || 
			    (go.transform.position.z - ball.radius < _playArea.min.z - allowBallOutBottom) || 
			    (go.transform.position.z + ball.radius > _playArea.max.z + allowBallOutTop))
			{
				return (false);
			}
		}

		return (true);
	}


	/// <summary>
	/// Test if the object is in the play area.
	/// </summary>
	/// <returns><c>true</c> if this instance is in play area the specified go radius; otherwise, <c>false</c>.</returns>
	/// <param name="go">Go.</param>
	/// <param name="radius">The object's radius. Set to 0 to just use the object's position.</param>
	public bool IsInPlayArea(GameObject go, float radius)
	{
		if ((go == null) || 
			(go.transform.position.x - radius < _playArea.min.x) || 
		    (go.transform.position.x + radius > _playArea.max.x) || 
		    (go.transform.position.z - radius < _playArea.min.z) || 
		    (go.transform.position.z + radius > _playArea.max.z))
		{
			return (false);
		}
		return (true);
	}


	/// <summary>
	/// Test if the game object is inside the play area.
	/// </summary>
	/// <returns><c>true</c> if this instance is in play area the specified go; otherwise, <c>false</c>.</returns>
	/// <param name="go">Go.</param>
	public bool IsInPlayArea(GameObject go)
	{
		if ((go == null) || 
		    (go.transform.position.x < _playArea.min.x) || 
		    (go.transform.position.x > _playArea.max.x) || 
		    (go.transform.position.z < _playArea.min.z) || 
		    (go.transform.position.z > _playArea.max.z))
		{
			return (false);
		}
		return (true);
	}


	/// <summary>
	/// Test if the ball is in the goal posts.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball in goalpost the specified team; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team who's goalpost to use.</param>
	public bool IsBallInGoalpost(SsTeam team)
	{
		return (IsInGoalposts(ball.gameObject, team, ball.radius));
	}


	/// <summary>
	/// Test if the object is in the goal posts.
	/// </summary>
	/// <returns><c>true</c> if this instance is in goalposts the specified go team radius; otherwise, <c>false</c>.</returns>
	/// <param name="go">Game object.</param>
	/// <param name="team">The team who's goalpost to use.</param>
	/// <param name="radius">The object's radius. Set to 0 to just use the object's position.</param>
	public bool IsInGoalposts(GameObject go, SsTeam team, float radius)
	{
		if ((go == null) || (team == null))
		{
			return (false);
		}

		// REMINDER: We test Y as well, unlike all the other bound area tests. But we do Not test the bottom, because 
		//			 the ground is at the bottom.

		if (team.PlayDirection > 0)
		{
			if ((go.transform.position.x - radius < _leftGoalPosts.min.x) || 
			    (go.transform.position.x + radius > _leftGoalPosts.max.x) || 
			    (go.transform.position.y + radius > _leftGoalPosts.max.y) || 
			    (go.transform.position.z - radius < _leftGoalPosts.min.z) || 
			    (go.transform.position.z + radius > _leftGoalPosts.max.z))
			{
				return (false);
			}
		}
		else
		{
			if ((go.transform.position.x - radius < _rightGoalPosts.min.x) || 
			    (go.transform.position.x + radius > _rightGoalPosts.max.x) || 
			    (go.transform.position.y + radius > _rightGoalPosts.max.y) || 
			    (go.transform.position.z - radius < _rightGoalPosts.min.z) || 
			    (go.transform.position.z + radius > _rightGoalPosts.max.z))
			{
				return (false);
			}
		}
		
		return (true);
	}


	/// <summary>
	/// Get the radar position of the specified object.
	/// </summary>
	/// <returns>The radar position.</returns>
	/// <param name="go">Go.</param>
	public Vector2 GetRadarPos(SsGameObject go)
	{
		return (new Vector2((go.transform.position.x - _playArea.center.x) / _playArea.extents.x,
		                    (go.transform.position.z - _playArea.center.z) / _playArea.extents.z));
	}


	/// <summary>
	/// Get the zone at the point. Returns a zone on the boundary if the point does Not lie in any zone.
	/// </summary>
	/// <returns>The zone at point.</returns>
	/// <param name="pos">Position.</param>
	public int GetZoneAtPoint(Vector3 pos)
	{
		int zone = -1;
		
		// If outside zones then use the zones on the boundaries
		if (pos.x < zones[0].min.x)
		{
			zone = 0;
		}
		else if (pos.x > zones[maxZones - 1].max.x)
		{
			zone = maxZones - 1;
		}
		else
		{
			// Calc zone from the play area's left edge
			zone = (int)((float)((pos.x - zones[0].min.x) / zoneSize.x));
			if (zone < 0)
			{
				zone = 0;
			}
			if (zone > maxZones - 1)
			{
				zone = maxZones - 1;
			}
		}

		return (zone);
	}


	/// <summary>
	/// Get the bounds of the zone. Returns a zone on the boundary is the specified zone is invalid.
	/// </summary>
	/// <returns>The zone bounds.</returns>
	/// <param name="zone">Zone.</param>
	public Bounds GetZoneBounds(int zone)
	{
		if (zone < 0)
		{
			zone = 0;
		}
		if (zone > maxZones - 1)
		{
			zone = maxZones - 1;
		}
		return (zones[zone]);
	}


	/// <summary>
	/// Get the right edge of the zone's bounds. Returns a zone on the boundary is the specified zone is invalid.
	/// Returns the first zone's left edge if trying to get a zone before the first one.
	/// </summary>
	/// <returns>The zone right edge.</returns>
	/// <param name="zone">Zone.</param>
	public float GetZoneRightEdge(int zone)
	{
		if (zone < 0)
		{
			zone = 0;
			// Return first zone's left edge
			return (zones[zone].min.x);
		}
		if (zone > maxZones - 1)
		{
			zone = maxZones - 1;
		}
		return (zones[zone].max.x);
	}
	
	
	/// <summary>
	/// Get the left edge of the zone's bounds. Returns a zone on the boundary is the specified zone is invalid.
	/// Returns the last zone's right edge if trying to get a zone past the last one.
	/// </summary>
	/// <returns>The zone left edge.</returns>
	/// <param name="zone">Zone.</param>
	public float GetZoneLeftEdge(int zone)
	{
		if (zone < 0)
		{
			zone = 0;
		}
		if (zone > maxZones - 1)
		{
			zone = maxZones - 1;
			// Return last zone's right edge
			return (zones[zone].max.x);
		}
		return (zones[zone].min.x);
	}


	/// <summary>
	/// Get the row at the point. Returns a row on the boundary if the point does Not lie in any row.
	/// NOTE: This uses the row size excluding the overlaps.
	/// </summary>
	/// <returns>The row at point.</returns>
	/// <param name="pos">Position.</param>
	public int GetRowAtPoint(Vector3 pos)
	{
		int row = -1;

		// REMINDER: Rows increase down, but Z increases up

		// If outside rows then use the rows on the boundaries
		if (pos.z > rows[0].max.z)
		{
			row = 0;
		}
		else if (pos.z < rows[maxRows - 1].min.x)
		{
			row = maxRows - 1;
		}
		else
		{
			// Calc row from the play area's top edge
			row = (int)((float)((rows[0].max.z - pos.z) / rowSize.z));
			if (row < 0)
			{
				row = 0;
			}
			if (row > maxRows - 1)
			{
				row = maxRows - 1;
			}
		}
		
		return (row);
	}


	/// <summary>
	/// Get the bounds of the row. Returns a row on the boundary is the specified row is invalid.
	/// </summary>
	/// <returns>The zone bounds.</returns>
	/// <param name="zone">Zone.</param>
	public Bounds GetRowBounds(int row)
	{
		if (row < 0)
		{
			row = 0;
		}
		if (row > maxRows - 1)
		{
			row = maxRows - 1;
		}
		return (rows[row]);
	}


	/// <summary>
	/// Gets the zone and row bounds. It returns the width (X) and height (Y) of the combined zones, and the depth (Z) of the combined rows.
	/// </summary>
	/// <returns>The zone and row bounds.</returns>
	/// <param name="zoneStart">Zone start. Zones start on left side of field.</param>
	/// <param name="zoneEnd">Zone end. Zones start on left side of field.</param>
	/// <param name="rowStart">Row start. Rows start at top of field.</param>
	/// <param name="rowEnd">Row end. Rows start at top of field.</param>
	public Bounds GetZoneAndRowBounds(int zoneStart, int zoneEnd, int rowStart, int rowEnd)
	{
		Bounds zoneBounds1, zoneBounds2, rowBounds1, rowBounds2, resultBounds;
		
		resultBounds = new Bounds();

		zoneBounds1 = GetZoneBounds(zoneStart);
		zoneBounds2 = GetZoneBounds(zoneEnd);
		rowBounds1 = GetRowBounds(rowStart);
		rowBounds2 = GetRowBounds(rowEnd);

		// REMINDER: Rows increase down, but Z increases up
		resultBounds.min = new Vector3(zoneBounds1.min.x, zoneBounds1.min.y, rowBounds2.min.z);
		resultBounds.max = new Vector3(zoneBounds2.max.x, zoneBounds2.max.y, rowBounds1.max.z);

		return (resultBounds);
	}



	/// <summary>
	/// Get the grid at the point.
	/// </summary>
	/// <returns>The grid at point.</returns>
	/// <param name="pos">The point at which to find the grid at.</param>
	/// <param name="clampToField">Indicates if a grid on the boundary must be returned if the point does Not lie in any grid.</param>
	public SsGrid GetGridAtPoint(Vector3 pos, bool clampToField)
	{
		int x, y;

		// Calc array indices (x increases left, z increases up)
		x = (int)((float)((pos.x - gridStartPosTopLeft.x) / gridSize.x));
		y = (int)((float)((gridStartPosTopLeft.z - pos.z) / gridSize.z));
		
		if (clampToField)
		{
			// If outside field then use the grids on the boundaries
			if (x < 0)
			{
				x = 0;
			}
			if (x > gridWidth - 1)
			{
				x = gridWidth - 1;
			}
			if (y < 0)
			{
				y = 0;
			}
			if (y > gridHeight - 1)
			{
				y = gridHeight - 1;
			}
		}
		
		if ((x < 0) || (x > gridWidth - 1) || (y < 0) || (y > gridHeight - 1))
		{
			return (null);
		}
		
		return (grids[x, y]);
	}


	/// <summary>
	/// Finds the grids in the circle.
	/// NOTE: The results are stored in gridSearchArray.
	/// </summary>
	/// <returns>The number of grids found, which are stored in gridSearchArray.</returns>
	/// <param name="centre">Centre.</param>
	/// <param name="radius">Radius. If 0 or less then will only search for 1 grid block at the centre.</param>
	private int FindGridsInCircle(Vector3 centre, float radius)
	{
		SsGrid grid;

		if (radius <= 0.0f)
		{
			// Only need 1 grid at the centre
			grid = GetGridAtPoint(centre, false);
			if (grid != null)
			{
				gridSearchArray[0] = grid;
				return (1);
			}
			return (0);
		}

		int count = 0;
		int x, y, startX, startY, endX, endY, maxCount;
		Vector2 circleCentre = new Vector2(centre.x, centre.z);

		// Calc array indices (x increases left, z increases up)
		startX = (int)((float)((centre.x - radius - gridStartPosTopLeft.x) / gridSize.x));
		startY = (int)((float)((gridStartPosTopLeft.z - centre.z - radius) / gridSize.z));
		endX = (int)((float)((centre.x + radius - gridStartPosTopLeft.x) / gridSize.x));
		endY = (int)((float)((gridStartPosTopLeft.z - centre.z + radius) / gridSize.z));

		if ((startX == endX) && (startY == endY))
		{
			// Only need 1 grid at the centre
			grid = GetGridAtPoint(centre, false);
			if (grid != null)
			{
				gridSearchArray[0] = grid;
				return (1);
			}
			return (0);
		}

		maxCount = gridSearchArray.Length;
		for (y = startY; y <= endY; y++)
		{
			for (x = startX; x <= endX; x++)
			{
				grid = GetGrid(x, y);
				if ((grid != null) && 
				    (GeUtils.CircleRectCollision(circleCentre, radius, grid.rect)))
				{
					gridSearchArray[count] = grid;
					count ++;
					if (count >= maxCount)
					{
						break;
					}
				}
			}
			if (count >= maxCount)
			{
				break;
			}
		}

		return (count);
	}


	/// <summary>
	/// Get the grid in the array.
	/// </summary>
	/// <returns>The grid. Returns null if indices (x, y) are outside the array bounds.</returns>
	/// <param name="x">x index.</param>
	/// <param name="y">y index.</param>
	public SsGrid GetGrid(int x, int y)
	{
		if ((x < 0) || (x > gridWidth - 1) || (y < 0) || (y > gridHeight - 1))
		{
			return (null);
		}
		return (grids[x, y]);
	}


	/// <summary>
	/// Search through the grid's columns and return the first open grid
	/// </summary>
	/// <returns>The first open grid in column.</returns>
	/// <param name="startGrid">Start grid.</param>
	/// <param name="direction">Direction. 1 = up, -1 = down, 0 = no direction (check only startGrid)</param>
	/// <param name="maxSearch">Max search iterations. It will always search at least the first grid.</param>
	public SsGrid FindFirstOpenGridInColumn(SsGrid startGrid, int direction, int maxSearch)
	{
		if (startGrid == null)
		{
			return (null);
		}

		SsGrid grid = startGrid;
		int search = 0;

		if (direction != 0)
		{
			// Invert direction, because grid increases down, but Z increases up.
			direction = -direction;
		}

		while (grid != null)
		{
			if (grid.playerWeight == 0)
			{

#if UNITY_EDITOR
				// DEBUG
				if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
				{
					if (grid != null)
					{
						SsDebugMatch.DrawRectUp(grid.bounds.center, 
						                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
						                        Color.yellow, 1.0f);
					}
				}
#endif //UNITY_EDITOR

				return (grid);
			}
			if (direction == 0)
			{
				break;
			}
			grid = GetGrid(grid.x, grid.y + direction);
			search ++;
			if (search >= maxSearch)
			{
				break;
			}
		}
		return (null);
	}


	/// <summary>
	/// Search through the grid's rows and return the first open grid.
	/// </summary>
	/// <returns>The first open grid in row.</returns>
	/// <param name="startGrid">Start grid.</param>
	/// <param name="direction">Direction. -1 = left, 1 = right, 0 = no direction (check only startGrid)</param>
	/// <param name="maxSearch">Max search iterations. It will always search at least the first grid.</param>
	public SsGrid FindFirstOpenGridInRow(SsGrid startGrid, int direction, int maxSearch)
	{
		if (startGrid == null)
		{
			return (null);
		}
		SsGrid grid = startGrid;
		int search = 0;
		while (grid != null)
		{
			if (grid.playerWeight == 0)
			{
#if UNITY_EDITOR
				// DEBUG
				if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
				{
					if (grid != null)
					{
						SsDebugMatch.DrawRectUp(grid.bounds.center, 
						                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
						                        Color.yellow, 1.0f);
					}
				}
#endif //UNITY_EDITOR

				return (grid);
			}
			if (direction == 0)
			{
				break;
			}
			grid = GetGrid(grid.x + direction, grid.y);
			search ++;
			if (search >= maxSearch)
			{
				break;
			}
		}
		return (null);
	}
	

	/// <summary>
	/// Find a random open grid within the arc.
	/// </summary>
	/// <returns>The open grid in arc.</returns>
	/// <param name="centre">Centre of the arc.</param>
	/// <param name="angleStart">Arc start angle to search. 0 is up and increases clockwise. A random angle will be selected between start and end.</param>
	/// <param name="angleEnd">Arc end angle to search. 0 is up and increases clockwise. A random angle will be selected between start and end.</param>
	/// <param name="minRadius">Arc minimum radius. A random radius will be selected between min and max.</param>
	/// <param name="maxRadius">Arc max radius. A random radius will be selected between min and max.</param>
	/// <param name="searchRadius">Search for an open spot this large, using the random point (selected in the arc) as a centre. Set to 0 to only 
	/// use the single grid block at the random point.</param>
	/// <param name="outsideBoundaryWeight">Weight to add when invalid grid blocks are found (e.g. give this a high value if trying to 
	/// search at the edge of the field and you do Not want player to select a point near the edge). Set to 0 if you do Not want
	/// invalid blocks to affect the search.</param>
	/// <param name="maxSearches">Max search itterations.</param>
	public SsGridSearchData FindOpenGridInArc(Vector3 centre, float angleStart, float angleEnd, 
	                                          float minRadius, float maxRadius, 
	                                          float searchRadius, 
	                                          int outsideBoundaryWeight, int maxSearches)
	{
		SsGridSearchData result = GetGridSearchDataFromPool();

		float angle;
		float radius;
		Vector2 vec;
		Vector3 pos;
		SsGrid grid;
		int search, i, count;
		
		result.grid = null;
		result.gridWeight = 0;
		search = maxSearches;
		while (search > 0)
		{
			// Select a random point in the arc
			angle = Random.Range(angleStart, angleEnd);
			radius = Random.Range(minRadius, maxRadius);
			vec = GeUtils.GetVectorFromAngle(angle);
			pos = centre + (new Vector3(vec.x, 0.0f, -vec.y) * radius);

			if (searchRadius <= 0)
			{
				// Search a single grid block
				grid = GetGridAtPoint(pos, false);
				if (grid != null)
				{
					// Add the single grid block
					if ((result.grid == null) || 
					    ((result.grid.playerWeight > 0) && (grid.playerWeight <= 0)))
					{
						result.grid = grid;
					}
					result.gridWeight += grid.playerWeight;
					
					if (result.gridWeight <= 0)
					{
#if UNITY_EDITOR
						// DEBUG
						if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
						{
							SsDebugMatch.DrawCircleUp(pos, 1.0f, 10, Color.yellow, 1.0f);
							grid = result.grid;
							if (grid != null)
							{
								SsDebugMatch.DrawRectUp(grid.bounds.center, 
								                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
								                        Color.yellow, 1.0f);
							}
						}
#endif //UNITY_EDITOR
						
						// Found an empty grid area
						return (result);
					}
				}
				else
				{
					result.gridWeight += outsideBoundaryWeight;
				}
			}
			else
			{
				// Search multiple grid blocks within the radius
				count = FindGridsInCircle(pos, searchRadius);
				if (count > 0)
				{
					for (i = 0; i < count; i++)
					{
						grid = gridSearchArray[i];
						if (grid != null)
						{
							if ((result.grid == null) || 
							    ((result.grid.playerWeight > 0) && (grid.playerWeight <= 0)) ||
							    ((grid.playerWeight <= 0) && (Random.Range(0, 2) == 0)))
							{
								result.grid = grid;
							}
							result.gridWeight += grid.playerWeight;
						}
						else
						{
							result.gridWeight += outsideBoundaryWeight;
						}
					}
					
					if ((result.grid != null) && (result.gridWeight <= 0))
					{
						// Found an empty grid area

#if UNITY_EDITOR
						// DEBUG
						if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
						{
							SsDebugMatch.DrawCircleUp(pos, searchRadius, 10, Color.yellow, 1.0f);
							for (i = 0; i < count; i++)
							{
								grid = gridSearchArray[i];
								if (grid != null)
								{
									SsDebugMatch.DrawRectUp(grid.bounds.center, 
									                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
									                        Color.yellow, 1.0f);
								}
							}
						}
#endif //UNITY_EDITOR
						
						return (result);
					}
				}
			}

			search --;
		}


		return (result);
	}


	/// <summary>
	/// Find a random open grid within the rect.
	/// </summary>
	/// <returns>The open grid in rect.</returns>
	/// <param name="rect">Rect in which to select a random point.</param>
	/// <param name="searchRadius">Search for an open spot this large, using the random point as a centre. Set to 0 to only 
	/// use the single grid block at the random point.</param>
	/// <param name="outsideBoundaryWeight">Weight to add when invalid grid blocks are found (e.g. give this a high value if trying to 
	/// search at the edge of the field, but do not want player to select a point near the edge). Set to 0 if you do Not want
	/// invalid blocks to affect the search.</param>
	/// <param name="maxSearches">Max search itterations.</param>
	public SsGridSearchData FindOpenGridInRect(ref Rect rect, float searchRadius, int outsideBoundaryWeight, int maxSearches)
	{
		SsGridSearchData result = GetGridSearchDataFromPool();

		Vector3 pos;
		SsGrid grid;
		int search, i, count;

		pos = Vector3.zero;

		result.grid = null;
		result.gridWeight = 0;
		search = maxSearches;

		while (search > 0)
		{
			// Select a random point in the rect
			pos.x = Random.Range(rect.min.x, rect.max.x);
			pos.z = Random.Range(rect.min.y, rect.max.y);

			if (searchRadius <= 0)
			{
				// Search a single grid block
				grid = GetGridAtPoint(pos, false);
				if (grid != null)
				{
					// Add the single grid block
					if ((result.grid == null) || 
					    ((result.grid.playerWeight > 0) && (grid.playerWeight <= 0)))
					{
						result.grid = grid;
					}
					result.gridWeight += grid.playerWeight;
					
					if (result.gridWeight <= 0)
					{
#if UNITY_EDITOR
						// DEBUG
						if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
						{
							SsDebugMatch.DrawCircleUp(pos, 1.0f, 10, Color.yellow, 1.0f);
							grid = result.grid;
							if (grid != null)
							{
								SsDebugMatch.DrawRectUp(grid.bounds.center, 
								                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
								                        Color.yellow, 1.0f);
							}
						}
#endif //UNITY_EDITOR

						// Found an empty grid area
						return (result);
					}
				}
				else
				{
					result.gridWeight += outsideBoundaryWeight;
				}
			}
			else
			{
				// Search multiple grid blocks within the radius
				count = FindGridsInCircle(pos, searchRadius);
				if (count > 0)
				{
					for (i = 0; i < count; i++)
					{
						grid = gridSearchArray[i];
						if (grid != null)
						{
							if ((result.grid == null) || 
							    ((result.grid.playerWeight > 0) && (grid.playerWeight <= 0)) ||
							    ((grid.playerWeight <= 0) && (Random.Range(0, 2) == 0)))
							{
								result.grid = grid;
							}
							result.gridWeight += grid.playerWeight;
						}
						else
						{
							result.gridWeight += outsideBoundaryWeight;
						}
					}

					if ((result.grid != null) && (result.gridWeight <= 0))
					{
						// Found an empty grid area

#if UNITY_EDITOR
						// DEBUG
						if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showPlayerSearches))
						{
							SsDebugMatch.DrawCircleUp(pos, searchRadius, 10, Color.yellow, 1.0f);
							for (i = 0; i < count; i++)
							{
								grid = gridSearchArray[i];
								if (grid != null)
								{
									SsDebugMatch.DrawRectUp(grid.bounds.center, 
									                        new Vector2(grid.bounds.size.x, grid.bounds.size.z),
									                        Color.yellow, 1.0f);
								}
							}
						}
#endif //UNITY_EDITOR

						return (result);
					}
				}
			}

			search --;
		}

		return (result);
	}


	/// <summary>
	/// Gets the goal area for the team (the area infront of the goal posts).
	/// </summary>
	/// <returns>The goal area.</returns>
	/// <param name="team">Team.</param>
	public Bounds GetGoalArea(SsTeam team)
	{
		if (team == match.LeftTeam)
		{
			return (_leftGoalArea);
		}
		return (_rightGoalArea);
	}


	/// <summary>
	/// Gets the goal posts area for the team. If ball in this area then it is a goal.
	/// </summary>
	/// <returns>The goal posts.</returns>
	/// <param name="team">Team.</param>
	public Bounds GetGoalPosts(SsTeam team)
	{
		if (team == match.LeftTeam)
		{
			return (_leftGoalPosts);
		}
		return (_rightGoalPosts);
	}


	/// <summary>
	/// Gets the penalty area for the team.
	/// </summary>
	/// <returns>The penalty area.</returns>
	/// <param name="team">Team.</param>
	public Bounds GetPenaltyArea(SsTeam team)
	{
		if (team == match.LeftTeam)
		{
			return (_leftPenaltyArea);
		}
		return (_rightPenaltyArea);
	}


	/// <summary>
	/// Get a position to shoot at in the goal posts.
	/// </summary>
	/// <returns>The shoot position.</returns>
	/// <param name="player">Player.</param>
	public Vector3 GetShootPos(SsPlayer player)
	{
		Bounds bounds;
		SsTeam team = player.Team;
		bool randomAim;

		if (team.PlayDirection > 0)
		{
			bounds = _rightGoalPosts;
		}
		else
		{
			bounds = _leftGoalPosts;
		}


		// Randomly aim?
		randomAim = (Random.Range(0.0f, 100.0f) < player.Skills.chanceRandomAimAtGoal);
		if (randomAim == false)
		{
			if (player.OtherTeam.GoalKeeper != null)
			{
				// Aim for a part of the goal area where the goalkeeper is Not standing
				if (player.OtherTeam.GoalKeeper.transform.position.z > bounds.center.z)
				{
					// Goalkeeper is top, so shoot bottom (3rd of bottom area)
					return (new Vector3(Random.Range(bounds.min.x, bounds.max.x), 
					                    Random.Range(bounds.min.y, bounds.max.y),
					                    Random.Range(bounds.min.z, bounds.min.z + (bounds.size.z * 0.33f))));
				}
				else
				{
					// Goalkeeper is bottom, so shoot top (3rd of top area)
					return (new Vector3(Random.Range(bounds.min.x, bounds.max.x), 
					                    Random.Range(bounds.min.y, bounds.max.y),
					                    Random.Range(bounds.min.z + (bounds.size.z * 0.67f), bounds.max.y)));
				}
			}
			else
			{
				// Anywhere in the goals
				return (new Vector3(Random.Range(bounds.min.x, bounds.max.x), 
				                    Random.Range(bounds.min.y, bounds.max.y),
				                    Random.Range(bounds.min.z, bounds.max.y)));
			}
		}
		else
		{
			// Random aim
			// Select a random spot in the goal area to shoot to
			// NOTE: goalBarWidth adds chance of hitting the goal bars
			return (new Vector3(Random.Range(bounds.min.x, bounds.max.x), 
			                    Random.Range(bounds.min.y, bounds.max.y + goalBarWidth),
			                    Random.Range(bounds.min.z - goalBarWidth, bounds.max.y + goalBarWidth)));
		}
	}



	/// <summary>
	/// Test if the player is past the specified zone's right edge (i.e. completely past it).
	/// </summary>
	/// <returns>The past zone.</returns>
	/// <param name="player">Player.</param>
	/// <param name="zone">Zone.</param>
	/// <param name="team">The team whose play direction to use. If null then the player's team will be used.</param>
	/// <param name="immediateForHuman">Indicates if the human controlled player must use immediate zone changes (i.e. pending zone).</param>
	public bool PlayerPastZone(SsPlayer player, int zone, SsTeam team, bool immediateForHuman)
	{
		int playerZone = player.Zone;
		
		if (team == null)
		{
			team = player.Team;
		}
		
		if ((immediateForHuman) && (player.IsUserControlled))
		{
			// Human player needs immediate zone changes
			if (player.ZonePending == -1)
			{
				playerZone = player.Zone;
			}
			else
			{
				playerZone = player.ZonePending;
			}
		}
		
		if ( ((team.PlayDirection > 0) && (playerZone >= zone)) ||
		    ((team.PlayDirection < 0) && (playerZone < maxZones - zone)) )
		{
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Test if the player is past the specified zone's left edge. This method uses a float to allow testing for partially in the  
	/// zone past its left edge (e.g. 1.5).
	/// </summary>
	/// <returns>The past zone.</returns>
	/// <param name="player">Player.</param>
	/// <param name="zone">Zone. Float to allow testing for partially in the zone past its left edge (e.g. 1.5).</param>
	/// <param name="team">The team whose play direction to use. If null then the player's team will be used.</param>
	public bool PlayerPastZone(SsPlayer player, float zone, SsTeam team)
	{
		if (team == null)
		{
			team = player.Team;
		}
		
		float fraction = (float)zone - (int)zone;
		float zoneX;
		if (team.PlayDirection > 0)
		{
			zoneX = GetZoneLeftEdge((int)zone) + (zoneSize.x * fraction);
			if (player.transform.position.x > zoneX)
			{
				return (true);
			}
		}
		else if (team.PlayDirection < 0)
		{
			zoneX = GetZoneLeftEdge(maxZones - (int)zone) - (zoneSize.x * fraction);
			if (player.transform.position.x < zoneX)
			{
				return (true);
			}
		}
		
		return (false);
	}


	/// <summary>
	/// Test if the player is before the specified zone (i.e. before its left edge).
	/// </summary>
	/// <returns>The before zone.</returns>
	/// <param name="player">Player.</param>
	/// <param name="zone">Zone.</param>
	/// <param name="team">The team whose play direction to use. If null then the player's team will be used.</param>
	/// <param name="immediateForHuman">Indicates if the human controlled player must use immediate zone changes (i.e. pending zone).</param>
	public bool PlayerBeforeZone(SsPlayer player, int zone, SsTeam team, bool immediateForHuman)
	{
		int playerZone = player.Zone;
		
		if (team == null)
		{
			team = player.Team;
		}
		
		if ((immediateForHuman) && (player.IsUserControlled))
		{
			// Human player needs immediate zone changes
			if (player.ZonePending == -1)
			{
				playerZone = player.Zone;
			}
			else
			{
				playerZone = player.ZonePending;
			}
		}
		
		if ( ((team.PlayDirection > 0) && (playerZone < zone)) ||
		    ((team.PlayDirection < 0) && (playerZone >= maxZones - zone)) )
		{
			return (true);
		}
		return (false);
	}


	/// <summary>
	/// Test if the player is moving towards the team's goalpost.
	/// </summary>
	/// <returns><c>true</c> if this instance is player moving to goalpost the specified team player intersectPoint wantToDive
	/// useDirection useLookVectorAngle; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team who's goalpost to use.</param>
	/// <param name="player">Player.</param>
	/// <param name="intersectPoint">Get the point where the player's path will intersect the goal posts.</param>
	/// <param name="wantToDive">Indicates if the player wants to dive.</param>
	/// <param name="useDirection">Indicates if the player's direction must be used if the player is Not moving.</param>
	/// <param name="useLookVectorAngle">Indicates if the player's look vector angle must be used, if the direction or movement angle fails.</param>
	/// <param name="padding">Padding percent to add to edges of goal post, to make it seem bigger (i.e. easier to intercept). Percent of goal posts length, e.g. 0.5 = half.</param>
	public bool IsPlayerMovingToGoalpost(SsTeam team, SsPlayer player, out Vector3 intersectPoint, 
	                                     bool wantToDive, bool useDirection, bool useLookVectorAngle,
	                                     float paddingPercent = 0.0f)
	{
		intersectPoint = new Vector3(0.0f, GroundY, 0.0f);

		Vector3 vec;
		Vector3 endPoint;
		int i, iMax;
		Vector2 intersectPoint2;
		
		if (useLookVectorAngle)
		{
			iMax = 2;
		}
		else
		{
			iMax = 1;
		}
		
		if (player.IsMoving())
		{
			vec = player.MoveVec;
		}
		else if (useDirection)
		{
			vec = player.transform.forward;
		}
		else if (useLookVectorAngle)
		{
			vec = player.LookVec;
			iMax = 1;
		}
		else
		{
			return (false);
		}

		for (i = 0; i < iMax; i++)
		{
			if (i == 1)
			{
				vec = player.LookVec;
			}

			endPoint = player.transform.position + (vec * 10000.0f);	// Project a line in the direction the player is moving
			if (team.PlayDirection > 0.0f)
			{
				// Play right, use left goal posts
				// Test if the sprite is going to intersect the front of the goal posts
				if ((vec.x < 0.0f) && 
				    (GeUtils.LineSegmentIntersection(new Vector2(player.transform.position.x, player.transform.position.z), 
				                                 new Vector2(endPoint.x, endPoint.z), 
				                                 new Vector2(_leftGoalPosts.max.x, _leftGoalPosts.min.z - (_leftGoalPosts.size.z * paddingPercent)),
				                                 new Vector2(_leftGoalPosts.max.x, _leftGoalPosts.max.z + (_leftGoalPosts.size.z * paddingPercent)), 
				                                 out intersectPoint2)))
				{
					intersectPoint.x = intersectPoint2.x;
					intersectPoint.z = intersectPoint2.y;

					if (wantToDive)
					{
						// Try to dive towards the middle of the goalkeeper area (looks better than diving to the goal posts)
						intersectPoint.x = _leftGoalArea.center.x;
					}
					return (true);
				}
			}
			else
			{
				// Play left, use right goal posts
				// Test if the sprite is going to intersect the front of the goal posts
				if ((vec.x > 0.0f) && 
				    (GeUtils.LineSegmentIntersection(new Vector2(player.transform.position.x, player.transform.position.z), 
				                                 new Vector2(endPoint.x, endPoint.z), 
				                                 new Vector2(_rightGoalPosts.min.x, _rightGoalPosts.min.z - (_rightGoalPosts.size.z * paddingPercent)),
				                                 new Vector2(_rightGoalPosts.min.x, _rightGoalPosts.max.z + (_rightGoalPosts.size.z * paddingPercent)), 
				                                 out intersectPoint2)))
				{
					intersectPoint.x = intersectPoint2.x;
					intersectPoint.z = intersectPoint2.y;

					if (wantToDive)
					{
						// Try to dive towards the middle of the goalkeeper area (looks better than diving to the goal posts)
						intersectPoint.x = _rightGoalArea.center.x;
					}
					return (true);
				}
			}
		}

		return (false);
	}


	/// <summary>
	/// Test if the ball is moving towards the team's goalpost.
	/// </summary>
	/// <returns><c>true</c> if this instance is ball moving to goalpost the specified team intersectPoint wantToDive; otherwise, <c>false</c>.</returns>
	/// <param name="team">The team who's goalpost to use.param>
	/// <param name="intersectPoint">Get the point where the ball's path will intersect the goal posts.</param>
	/// <param name="wantToDive">Indicates if the player wants to dive. (The player/AI calling this method.)</param>
	/// <param name="padding">Padding to add to edges of goal post, to make it seem bigger (i.e. easier to intercept).param>
	public bool IsBallMovingToGoalpost(SsTeam team, out Vector3 intersectPoint, bool wantToDive,
	                                   float padding = 0.0f)
	{
		intersectPoint = new Vector3(0.0f, GroundY, 0.0f);

		if (ball.IsMoving() == false)
		{
			return (false);
		}

		Vector3 vec = ball.MoveVec;;
		Vector3 endPoint;
		Vector2 intersectPoint2;

		endPoint = ball.transform.position + (vec * 10000.0f);	// Project a line in the direction the ball is moving
		if (team.PlayDirection > 0.0f)
		{
			// Play right, use left goal posts
			// Test if the sprite is going to intersect the front of the goal posts
			if ((vec.x < 0.0f) && 
			    (GeUtils.LineSegmentIntersection(new Vector2(ball.transform.position.x, ball.transform.position.z), 
			                                 new Vector2(endPoint.x, endPoint.z), 
			                                 new Vector2(_leftGoalPosts.max.x, _leftGoalPosts.min.z - padding),
			                                 new Vector2(_leftGoalPosts.max.x, _leftGoalPosts.max.z + padding), 
			                                 out intersectPoint2)))
			{
				intersectPoint.x = intersectPoint2.x;
				intersectPoint.z = intersectPoint2.y;

				if (wantToDive)
				{
					// Try to dive towards the middle of the goalkeeper area (looks better than diving to the goal posts)
					intersectPoint.x = _leftGoalArea.center.x;
				}
				return (true);
			}
		}
		else
		{
			// Play left, use right goal posts
			// Test if the sprite is going to intersect the front of the goal posts
			if ((vec.x > 0.0f) && 
			    (GeUtils.LineSegmentIntersection(new Vector2(ball.transform.position.x, ball.transform.position.z), 
			                                 new Vector2(endPoint.x, endPoint.z), 
			                                 new Vector2(_rightGoalPosts.min.x, _rightGoalPosts.min.z - padding),
			                                 new Vector2(_rightGoalPosts.min.x, _rightGoalPosts.max.z + padding), 
			                                 out intersectPoint2)))
			{
				intersectPoint.x = intersectPoint2.x;
				intersectPoint.z = intersectPoint2.y;

				if (wantToDive)
				{
					// Try to dive towards the middle of the goalkeeper area (looks better than diving to the goal posts)
					intersectPoint.x = _rightGoalArea.center.x;
				}
				return (true);
			}
		}

		return (false);
	}


#if UNITY_EDITOR
	// DEBUG
#if SIMSOC_ENABLE_ONGUI
	void OnGUI()
	{
		Camera cam = (SsMatchCamera.Instance != null) ? SsMatchCamera.Instance.Cam : Camera.current;
		Vector3 pos;
		float addY = 25.0f;
		int i, x, y;

		// Show zone names
		if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showZones) && 
		    (zones != null) && (zones.Length > 0) && (cam != null))
		{
			for (i = 0; i < zones.Length; i++)
			{
				pos = new Vector3(zones[i].min.x, zones[i].max.y, zones[i].max.z);
				pos = cam.WorldToScreenPoint(pos);
				GUI.Label(new Rect(pos.x, Screen.height - pos.y, 100, addY), 
				          "Zone " + i + " / " + (SsFieldProperties.maxZones - i - 1));
			}
		}

		// Show grid weights
		if ((SsDebugMatch.Instance != null) && (SsDebugMatch.Instance.showGridWeights) && 
		    (grids != null) && (grids.GetLength(0) > 0) && (grids.GetLength(1) > 0))
		{
			SsGrid grid;
			
			Gizmos.color = new Color(173.0f / 255.0f, 111.0f / 255.0f, 255.0f / 255.0f);
			for (y = 0; y < grids.GetLength(1); y++)
			{
				for (x = 0; x < grids.GetLength(0); x++)
				{
					grid = grids[x, y];
					if (grid != null)
					{
						// Weight
						pos = cam.WorldToScreenPoint(grid.bounds.center);
						GUI.Label(new Rect(pos.x, Screen.height - pos.y, 100, addY), "" + grid.playerWeight);

						// Array index
						//pos.y -= 20.0f;
						//GUI.Label(new Rect(pos.x, Screen.height - pos.y, 100, addY), "" + grid.x + ", " + grid.y);
					}
				}
			}
		}
	}
#endif

	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos()
	{
		if (SsDebugMatch.Instance == null)
		{
			return;
		}

		int i, x, y;
		Bounds bounds;

		// Draw the zones
		if ((SsDebugMatch.Instance.showZones) && (zones != null) && (zones.Length > 0))
		{
			for (i = 0; i < zones.Length; i++)
			{
				if ((i % 2) == 0)
				{
					Gizmos.color = new Color(0.5f, 0.5f, 0.0f, 0.5f);
				}
				else
				{
					Gizmos.color = new Color(0.0f, 0.5f, 0.5f, 0.5f);
				}

				Gizmos.DrawCube(zones[i].center, zones[i].size);
			}
		}


		// Draw the rows
		if ((SsDebugMatch.Instance.showRows) && (rows != null) && (rows.Length > 0))
		{
			Gizmos.color = Color.gray;
			for (i = 0; i < rows.Length; i++)
			{
				Gizmos.DrawWireCube(rows[i].center, rows[i].size);
			}

			// Highlight row in which ball player is
			if ((match != null) && (match.BallPlayer != null))
			{
				bounds = GetRowBounds(GetRowAtPoint(match.BallPlayer.transform.position));
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
		}


		// Draw the grid
		if ((SsDebugMatch.Instance.showGrid) && (grids != null) && (grids.GetLength(0) > 0) && (grids.GetLength(1) > 0))
		{
			SsGrid grid;

			Gizmos.color = new Color(173.0f / 255.0f, 111.0f / 255.0f, 255.0f / 255.0f);
			for (y = 0; y < grids.GetLength(1); y++)
			{
				for (x = 0; x < grids.GetLength(0); x++)
				{
					grid = grids[x, y];
					if (grid != null)
					{
						Gizmos.DrawWireCube(grid.bounds.center, grid.bounds.size);
					}
				}
			}

			// Highlight grid in which ball player is
			if ((match != null) && (match.BallPlayer != null))
			{
				grid = GetGridAtPoint(match.BallPlayer.transform.position, false);
				if (grid != null)
				{
					Gizmos.color = Color.white;
					Gizmos.DrawWireCube(grid.bounds.center, grid.bounds.size);
				}
			}
		}

		if (SsDebugMatch.Instance.showHalves)
		{
			// Draw left/right half
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(leftHalfArea.center, leftHalfArea.size);
			Gizmos.DrawWireCube(rightHalfArea.center, rightHalfArea.size);
		}
	}
#endif //UNITY_EDITOR

}
