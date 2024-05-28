using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Team.
/// </summary>
public class SsTeam : MonoBehaviour
{

    // Const
    //------
    public const int minPlayers = 4;                    // Min active players in a team
    public const int maxPlayers = 11;                   // Max active players in a team

    public const int maxMarkedByPlayers = 1;            // Max number of players that can mark another player

    public const float defaultBallerPredatorDelayTime = 1.0f;   // Default delay between setting the baller predator.



    // Public
    //-------
    [Tooltip("Unique ID. It is used to identify the resource. Do Not change this after the game has been released.")]
    public string id;


    // Team info
    [Header("Team Info")]
    public string teamName;
    [Tooltip("Preferred name/nickname.")]
    public string preferredName;
    [Tooltip("Short name used on certain UIs.")]
    public string shortName;
    [Tooltip("3 letter name used on certain UIs.")]
    public string name3Letters;

    [Space(10)]
    [Tooltip("The scene ID of the team's home field.")]
    public string homeFieldId;

    [Space(10)]
    [Tooltip("Colour of the player icons on the mini-map.")]
    public Color miniMapColour = Color.gray;

    [Tooltip("Icon to display on the HUD.")]
    public Sprite hudIcon;
    [Tooltip("Icon to display on the result screen.")]
    public Sprite resultIcon;
    [Tooltip("Icon to display on the half time screen. Leave empty if the same as the result icon.")]
    public Sprite halfTimeIcon;


    // Players
    [Header("Players")]
    [Tooltip("Player prefabs to clone.")]
    public SsPlayer[] playerPrefabs;                // Player prefabs


    // Shadow
    [Header("Shadow")]
    [Tooltip("Fake shadow to use for all the players. Each player can override this and have their own shadow.")]
    public SsFakeShadow fakeShadowPrefab;



    // Private
    //--------
    protected SsMatch match;                            // Reference to the match
    protected SsFieldProperties field;                  // Reference to the field
    protected SsBall ball;                              // Reference to the ball

    protected int index;                                // Team's index in the match array. Corresponds to players in the menus. (0 or 1).
    protected int userControlIndex = -1;                // Index of user controlling the team (-1 = no user/AI)
    protected SsInput.inputTypes inputType = SsInput.inputTypes.ai; // Team's input type (e.g. AI, keyboard, gamepad, etc.)
    protected SsPlayer controlPlayer;                   // User controlled player. Set via SsMatchInputManager.
    protected float controlPlayerChangeTime;            // The Time.time when the control player last changed

    protected int score;                                // Score (goals)
    protected SsTeamSkills skills;                      // Team skills
    protected int difficulty;                           // Team difficulty

    protected SsTeam otherTeam;                         // The other team

    protected float playDirection;                      // The direction the team is playing (left = -1, right = 1)
    protected float originalPlayDirection;              // Play direction at the start of the match
    protected SsFormation formation;                    // Play formation

    protected int maxPlayersInList = maxPlayers;        // Max players in a list. Updated during spawning.
    protected List<SsPlayer> players;                   // Active players
    protected List<SsPlayer> poolPlayers;               // Pool of disbled players (e.g. used when formation changes)
    protected GameObject poolHolder;                    // Holder for pool players
    protected SsPlayer goalKeeper;                      // Goal keeper

    // Ball predator
    protected List<SsPlayer> ballPredators;             // Ball predators. Players tasked to get a loose ball.
    protected float ballPredatorDelayTime;              // Time when next ball predator can be set (delay too many players trying at once)

    // Baller predator
    protected List<SsPlayer> ballerPredators;           // Baller predators. Players trying to get the player who has the ball.
    protected float ballerPredatorDelayTime;            // Delay between setting the baller predator
    protected int ballerPredatorsMovingCount;           // Count how many of the baller predators are moving

    protected SsPlayer nearestPlayerToBall;             // Nearest player to the ball
    protected SsPlayer nearestHumanPlayerToBall;        // Nearest human player to the ball
    protected SsPlayer nearestAiPlayerToBall;           // Nearest AI player to the ball
    protected SsPlayer nearestUnhurtAiPlayerToBall;     // Nearest un-hurt AI player to the ball (i.e. player Not falling or in pain)
    protected SsPlayer nearestNonGoalkeeperToBall;      // Nearest non-goalkeeper player to ball
    protected SsPlayer foremostPlayer;                  // The player in the front.
    protected SsPlayer secondForemostPlayer;            // The second player in the front.

    protected SsPlayer potentialPassPlayer;             // Potential player to pass to next
    protected bool selectReceiverAfterPass;             // Indicates if the user must select the receiver after a pass
    protected SsPlayer passPlayer;                      // Player being passed to
    protected SsPlayer prevPassPlayer;                  // Previous player who was passed to
    protected SsPlayer lastPassPlayer;                  // The last player passed to. This is used to detect if ball goes by the pass player.
    protected float passPlayerMinBallDistanceSquared;   // Min distance between the pass player and the ball. Used to detect is ball goes by pass player.
    protected float passPlayerBallDistanceIncreaseTime; // Timer to keep track how long distance between the pass player and the ball is increasing.
    protected bool predictPass;                         // Indicates if a predict pass was made to the pass player

    protected float delayAiUpdateTime;                  // Timer to delay all AI updates.

    // Counts how many times the opponent team passed the ball since this team last slided. If other team constantly pass to each other, 
    //	then this team's sliding will be delayed. This counter prevents that.
    protected int opponentPassesSinceLastSlide;

    protected float delaySlidingTime;                   // Timer to delay sliding for all the players.

    protected float updateMousePassPlayerTime;          // Time to delay changing the player to pass to when passing to the mouse position



    // Properties
    //-----------
    public int Index
    {
        get { return (index); }
        set
        {
            index = value;
        }
    }


    public int UserControlIndex
    {
        get { return (userControlIndex); }
        set
        {
            userControlIndex = value;
        }
    }


    public bool IsUserControlled
    {
        get { return (userControlIndex >= 0); }
    }


    public bool IsAiControlled
    {
        get { return (userControlIndex < 0); }
    }


    public SsInput.inputTypes InputType
    {
        get { return (inputType); }
        set
        {
            inputType = value;
            if (SsMatchInputManager.Instance != null)
            {
                SsMatchInputManager.Instance.OnUserInputChanged(userControlIndex, inputType, this);
            }
        }
    }


    /// <summary>
    /// User controlled player.
    /// IMPORTANT: Set it via SsMatchInputManager.SetControlPlayer.
    /// </summary>
    /// <value>The control player.</value>
    public SsPlayer ControlPlayer
    {
        get { return (controlPlayer); }
        set
        {
            controlPlayer = value;
            controlPlayerChangeTime = Time.time;
        }
    }


    public float ControlPlayerChangeTime
    {
        get { return (controlPlayerChangeTime); }
    }


    public int Score
    {
        get { return (score); }
        set
        {
            score = value;
        }
    }


    public SsTeamSkills Skills
    {
        get { return (skills); }
    }


    public int Difficulty
    {
        get { return (difficulty); }
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
    /// The direction the team is playing (left = -1, right = 1). Use SetPlayDirection to set it.
    /// </summary>
    /// <value>The play direction.</value>
    public float PlayDirection
    {
        get { return (playDirection); }
        set
        {
            playDirection = value;
        }
    }


    public float OriginalPlayDirection
    {
        get { return (originalPlayDirection); }
        set
        {
            originalPlayDirection = value;
        }
    }


    public SsFormation Formation
    {
        get { return (formation); }
        set
        {
            formation = value;
        }
    }


    public List<SsPlayer> Players
    {
        get { return (players); }
    }


    public int BallPredatorsCount
    {
        get
        {
            if (ballPredators != null)
            {
                return (ballPredators.Count);
            }
            return (0);
        }
    }


    public float BallPredatorDelayTime
    {
        get { return (ballPredatorDelayTime); }
        set
        {
            ballPredatorDelayTime = value;
        }
    }


    public int BallerPredatorsCount
    {
        get
        {
            if (ballerPredators != null)
            {
                return (ballerPredators.Count);
            }
            return (0);
        }
    }


    public int BallerPredatorsMovingCount
    {
        get
        {
            if (ballerPredators != null)
            {
                return (Mathf.Min(ballerPredatorsMovingCount, ballerPredators.Count));
            }
            return (0);
        }
    }


    public float BallerPredatorDelayTime
    {
        get { return (ballerPredatorDelayTime); }
        set
        {
            ballerPredatorDelayTime = value;
        }
    }


    public SsPlayer GoalKeeper
    {
        get { return (goalKeeper); }
        set
        {
            goalKeeper = value;
        }
    }


    public SsPlayer NearestPlayerToBall
    {
        get { return (nearestPlayerToBall); }
    }


    public SsPlayer NearestHumanPlayerToBall
    {
        get { return (nearestHumanPlayerToBall); }
    }


    public SsPlayer NearestAiPlayerToBall
    {
        get { return (nearestAiPlayerToBall); }
    }


    public SsPlayer NearestUnhurtAiPlayerToBall
    {
        get { return (nearestUnhurtAiPlayerToBall); }
    }


    public SsPlayer NearestNonGoalkeeperToBall
    {
        get { return (nearestNonGoalkeeperToBall); }
    }


    public SsPlayer ForemostPlayer
    {
        get { return (foremostPlayer); }
    }


    public SsPlayer SecondForemostPlayer
    {
        get { return (secondForemostPlayer); }
    }


    public SsPlayer PotentialPassPlayer
    {
        get { return (potentialPassPlayer); }
    }


    public bool SelectReceiverAfterPass
    {
        get { return (selectReceiverAfterPass); }
        set
        {
            selectReceiverAfterPass = value;
        }
    }


    /// <summary>
    /// Player being passed to.
    /// </summary>
    /// <value>The pass player.</value>
    public SsPlayer PassPlayer
    {
        get { return (passPlayer); }
    }


    /// <summary>
    /// Previous player who was passed to.
    /// </summary>
    /// <value>The previous pass player.</value>
    public SsPlayer PrevPassPlayer
    {
        get { return (prevPassPlayer); }
    }


    public SsPlayer LastPassPlayer
    {
        get { return (lastPassPlayer); }
        set
        {
            lastPassPlayer = value;
        }
    }


    public float PassPlayerMinBallDistanceSquared
    {
        get { return (passPlayerMinBallDistanceSquared); }
        set
        {
            passPlayerMinBallDistanceSquared = value;
        }
    }


    public float PassPlayerBallDistanceIncreaseTime
    {
        get { return (passPlayerBallDistanceIncreaseTime); }
        set
        {
            passPlayerBallDistanceIncreaseTime = value;
        }
    }


    public bool PredictPass
    {
        get { return (predictPass); }
        set
        {
            predictPass = value;
        }
    }


    public float DelayAiUpdateTime
    {
        get { return (delayAiUpdateTime); }
        set
        {
            delayAiUpdateTime = value;
        }
    }


    public int OpponentPassesSinceLastSlide
    {
        get { return (opponentPassesSinceLastSlide); }
        set
        {
            opponentPassesSinceLastSlide = value;
        }
    }




    // Methods
    //--------

    /// <summary>
    /// Creates a team from the prefab.
    /// </summary>
    /// <returns>The team.</returns>
    /// <param name="prefab">Prefab.</param>
    static public SsTeam CreateTeam(SsTeam prefab)
    {
        if (prefab == null)
        {
            return (null);
        }

        SsTeam team = (SsTeam)Instantiate(prefab);

        return (team);
    }


    /// <summary>
    /// Awake this instance.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void Awake()
    {
        poolHolder = new GameObject("Pool");
        if (poolHolder != null)
        {
            poolHolder.transform.parent = transform;
            poolHolder.transform.localPosition = Vector3.zero;
            poolHolder.transform.localRotation = Quaternion.identity;
            poolHolder.transform.localScale = Vector3.one;

            poolHolder.SetActive(false);
        }

    }


    /// <summary>
    /// Start this instance.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void Start()
    {
        PostStart();
    }


    /// <summary>
    /// Post-start.
    /// </summary>
    /// <returns>The start.</returns>
    void PostStart()
    {
        // NOTE: For match.IsLoading we do Not check if the players are loaded, because they will be loaded here
        if ((SsMatch.Instance == null) || (SsMatch.Instance.IsLoading(false)) ||
            (SsFieldProperties.Instance == null) || (SsBall.Instance == null))
        {
            // Match is busy loading
            Invoke("PostStart", 0.1f);
            return;
        }

        match = SsMatch.Instance;
        field = SsFieldProperties.Instance;
        ball = SsBall.Instance;

        difficulty = 0;
        if (SsMatchInputManager.Instance != null)
        {
            SsMatchInputManager.Instance.OnUserInputChanged(userControlIndex, inputType, this);

            // Set the team's difficulty
            if (SsSettings.selectedMatchType == SsMatch.matchTypes.tournament)
            {
                // Tournament
                if (IsUserControlled)
                {
                    // Human team
                    difficulty = match.Difficulty;
                }
                else
                {
                    // AI team
                    SsTeamStats stats = SsTeamStats.GetTeamStats(id);
                    if (stats != null)
                    {
                        difficulty = stats.tournamentAi;
                    }
                }
            }
            else
            {
                // If both teams are AI then select a random difficulty (e.g. spectator match)
                if ((match.LeftTeam.IsAiControlled) && (match.RightTeam.IsAiControlled))
                {
                    difficulty = Random.Range(0, match.Difficulty + 1);
                }
                else
                {
                    difficulty = match.Difficulty;
                }
            }
        }


        SelectSkills();

        // Set formation and spawn players
        SetFormationAtKickOff(true);

        ResetMe();


        // Finally done
        OnPostStartDone();


#if UNITY_EDITOR
        // Editor warnings
        //----------------
        if (skills == null)
        {
            Debug.LogError("ERROR: Team (or children) does not have a Team Skills component: " + GetAnyName());
        }
#endif //UNITY_EDITOR
    }


    /// <summary>
    /// Called after PostStart is done, i.e. got reference to match, field, ball and spawned players.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void OnPostStartDone()
    {
    }


    /// <summary>
    /// Reset this instance. Reset at start og match, half time, etc.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void ResetMe()
    {
        controlPlayer = null;

        nearestPlayerToBall = null;
        nearestHumanPlayerToBall = null;
        nearestAiPlayerToBall = null;
        nearestUnhurtAiPlayerToBall = null;
        nearestNonGoalkeeperToBall = null;
        foremostPlayer = null;
        secondForemostPlayer = null;

        potentialPassPlayer = null;
        selectReceiverAfterPass = false;
        passPlayer = null;
        prevPassPlayer = null;
        lastPassPlayer = null;
        passPlayerMinBallDistanceSquared = 0.0f;
        passPlayerBallDistanceIncreaseTime = 0.0f;
        predictPass = false;

        delayAiUpdateTime = 0.0f;

        opponentPassesSinceLastSlide = 0;
        delaySlidingTime = 0.0f;
        updateMousePassPlayerTime = 0.0f;

        ClearBallPredators();
        ballPredatorDelayTime = 0.0f;

        ClearBallerPredators();
        ballerPredatorDelayTime = 0.0f;


        ClearPlayersGrid();
    }


    /// <summary>
    /// Raises the destroy event.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void OnDestroy()
    {
        CleanUp();
    }


    /// <summary>
    /// Clean up. Includes freeing resources and clearing references.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The up.</returns>
    protected virtual void CleanUp()
    {
        match = null;
        field = null;
        ball = null;

        formation = null;

        otherTeam = null;

        controlPlayer = null;
        goalKeeper = null;

        nearestPlayerToBall = null;
        nearestHumanPlayerToBall = null;
        nearestAiPlayerToBall = null;
        nearestUnhurtAiPlayerToBall = null;
        nearestNonGoalkeeperToBall = null;
        foremostPlayer = null;
        secondForemostPlayer = null;

        potentialPassPlayer = null;
        passPlayer = null;
        prevPassPlayer = null;
        lastPassPlayer = null;


        ClearPlayers();
        ClearPoolPlayers();

        ClearBallPredators();
        ballPredators = null;

        ClearBallerPredators();
        ballerPredators = null;
    }



    /// <summary>
    /// Selects the relevant skills from all the ones attached to the team (or the team's children).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The skills.</returns>
    public virtual void SelectSkills()
    {
        SsTeamSkills[] allSkills = gameObject.GetComponentsInChildren<SsTeamSkills>(true);
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
        SsTeamSkills tempSkills;


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
                        if (((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.humanTeamAndAnyMatchDifficulty) && (IsUserControlled)) ||
                            ((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.computerTeamAndAnyMatchDifficulty) && (IsAiControlled)))
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
                             (IsUserControlled) && (Difficulty >= tempSkills.useForDifficulty)) ||
                            ((tempSkills.whenToUseSkill == SsPlayerSkills.whenToUseSkills.computerTeamForSpecificMatchDifficulty) &&
                         (IsAiControlled) && (Difficulty >= tempSkills.useForDifficulty)))
                        {
                            if (Difficulty == tempSkills.useForDifficulty)
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
    /// Clears the list of players and sets the list to null.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The players.</returns>
    protected virtual void ClearPlayers(bool moveThemToPool = false)
    {
        if ((players != null) && (players.Count > 0))
        {
            int i;
            SsPlayer player;
            for (i = 0; i < players.Count; i++)
            {
                if (moveThemToPool)
                {
                    player = players[i];
                    if (player != null)
                    {
                        if (poolPlayers == null)
                        {
                            poolPlayers = new List<SsPlayer>(maxPlayersInList);
                        }
                        if (poolPlayers != null)
                        {
                            poolPlayers.Add(player);

                            player.gameObject.SetActive(false);
                            if (poolHolder != null)
                            {
                                player.transform.parent = poolHolder.transform;
                            }
                        }
                    }
                }

                players[i] = null;
            }

            players.Clear();
        }

        players = null;
    }


    /// <summary>
    /// Clears the pool players.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The pool players.</returns>
    protected virtual void ClearPoolPlayers()
    {
        if ((poolPlayers != null) && (poolPlayers.Count > 0))
        {
            int i;
            for (i = 0; i < poolPlayers.Count; i++)
            {
                poolPlayers[i] = null;
            }
            poolPlayers.Clear();
        }
        poolPlayers = null;
    }


    /// <summary>
    /// Sets the formation and spawns the players needed for the formation.
    /// If existing players are no longer needed then they are disabled and put in a pool.
    /// If formation is null then it will select a random formation. If it can't then it will spawn random players.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The formation.</returns>
    /// <param name="newFormation">New formation.</param>
    public virtual void SetFormation(SsFormation newFormation)
    {
        if ((newFormation == null) && (SsFormationManager.Instance != null))
        {
            newFormation = SsFormationManager.Instance.GetRandomFormation(null);
        }

        int i, n, r, max, count;
        SsPlayer prefab;
        SsPlayer[] prefabs;
        List<SsPlayer> goalkeeperPrefabs = new List<SsPlayer>();
        List<SsPlayer> fowardsPrefabs = new List<SsPlayer>();
        List<SsPlayer> midfieldersPrefabs = new List<SsPlayer>();
        List<SsPlayer> defendersPrefabs = new List<SsPlayer>();
        SsPlayer.SsPositionProperties posProps;
        SsFormationPlayer formationPlayer;
        int defendersNeeded, midfieldersNeeded, forwardsNeeded;
        SsPlayer player;

        formation = newFormation;

        prefabs = playerPrefabs;

        if ((prefabs == null) || (prefabs.Length <= 0))
        {
#if UNITY_EDITOR
            Debug.LogWarning("WARNING: Team has no player prefabs. Team: " + GetAnyName());
#endif //UNITY_EDITOR

            return;
        }

        ClearPlayers(true);

        max = field.NumPlayersPerSide;

        // Build the prefab lists of position types available (e.g. midfielders, forwards, etc.)
        for (i = 0; i < prefabs.Length; i++)
        {
            prefab = prefabs[i];
            if ((prefab == null) || (prefab.positionProperties == null) ||
                (prefab.positionProperties.Length <= 0))
            {
                continue;
            }

            for (n = 0; n < prefab.positionProperties.Length; n++)
            {
                posProps = prefab.positionProperties[n];
                if (posProps == null)
                {
                    continue;
                }

                if (posProps.position == SsPlayer.positions.goalkeeper)
                {
                    goalkeeperPrefabs.Add(prefab);
                }
                if (posProps.position == SsPlayer.positions.forward)
                {
                    fowardsPrefabs.Add(prefab);
                }
                if (posProps.position == SsPlayer.positions.midfielder)
                {
                    midfieldersPrefabs.Add(prefab);
                }
                if (posProps.position == SsPlayer.positions.defender)
                {
                    defendersPrefabs.Add(prefab);
                }
            }
        }

        maxPlayersInList = Mathf.Max(max, Mathf.Max(maxPlayers, prefabs.Length));
        i = 0;
        if (goalkeeperPrefabs != null)
        {
            i += goalkeeperPrefabs.Count;
        }
        if (fowardsPrefabs != null)
        {
            i += fowardsPrefabs.Count;
        }
        if (midfieldersPrefabs != null)
        {
            i += midfieldersPrefabs.Count;
        }
        if (defendersPrefabs != null)
        {
            i += defendersPrefabs.Count;
        }
        if (maxPlayersInList < i)
        {
            maxPlayersInList = i;
        }
        maxPlayersInList += 2;  // Extra, just in case


        count = 0;

        // First spawn the goalkeeper (we always need a goalkeeper!)
        if ((goalkeeperPrefabs != null) && (goalkeeperPrefabs.Count > 0))
        {
            count += SpawnPlayersFromList(goalkeeperPrefabs, 1, SsPlayer.positions.goalkeeper);
        }

#if UNITY_EDITOR
        if (GoalKeeper == null)
        {
            Debug.LogError("ERROR: Team  (" + GetAnyName() + ")  does not have a goalkeeper. " +
                           "Another player will be used as the goalkeeper.");
        }
#endif //UNITY_EDITOR


        if ((formation != null) && (formation.players != null) && (formation.players.Length > 0))
        {
            // Spawn based on the needs of the formation
            defendersNeeded = 0;
            midfieldersNeeded = 0;
            forwardsNeeded = 0;
            for (i = 0; i < formation.players.Length; i++)
            {
                if (i >= max)
                {
                    // Limit to the max allowed for the field/match settings
                    break;
                }

                formationPlayer = formation.players[i];
                if (formationPlayer != null)
                {
                    formationPlayer.LinkedToPlayer = -1;

                    if (formationPlayer.position == SsPlayer.positions.goalkeeper)
                    {
                        // We should already have a goalkeeper
                    }
                    else if (formationPlayer.position == SsPlayer.positions.defender)
                    {
                        defendersNeeded++;
                    }
                    else if (formationPlayer.position == SsPlayer.positions.midfielder)
                    {
                        midfieldersNeeded++;
                    }
                    else if (formationPlayer.position == SsPlayer.positions.forward)
                    {
                        forwardsNeeded++;
                    }
                }
            }

            if ((defendersNeeded > 0) && (defendersPrefabs != null) && (defendersPrefabs.Count > 0))
            {
                count += SpawnPlayersFromList(defendersPrefabs, defendersNeeded, SsPlayer.positions.defender);
            }

            if ((midfieldersNeeded > 0) && (midfieldersPrefabs != null) && (midfieldersPrefabs.Count > 0))
            {
                count += SpawnPlayersFromList(midfieldersPrefabs, midfieldersNeeded, SsPlayer.positions.midfielder);
            }

            if ((forwardsNeeded > 0) && (fowardsPrefabs != null) && (fowardsPrefabs.Count > 0))
            {
                count += SpawnPlayersFromList(fowardsPrefabs, forwardsNeeded, SsPlayer.positions.forward);
            }
        }


        if (count < max)
        {
            // No formation, or formation did Not have enough players.
            // Spawn random players (i.e. forwards, midfielders and defenders)
            for (i = count; i < max; i++)
            {
                if ((fowardsPrefabs != null) && (fowardsPrefabs.Count > 0) &&
                    (midfieldersPrefabs != null) && (midfieldersPrefabs.Count > 0) &&
                    (defendersPrefabs != null) && (defendersPrefabs.Count > 0))
                {
                    r = Random.Range(0, 3);
                    if (r == 0)
                    {
                        count += SpawnPlayersFromList(fowardsPrefabs, 1, SsPlayer.positions.forward);
                    }
                    else if (r == 1)
                    {
                        count += SpawnPlayersFromList(midfieldersPrefabs, 1, SsPlayer.positions.midfielder);
                    }
                    else
                    {
                        count += SpawnPlayersFromList(defendersPrefabs, 1, SsPlayer.positions.defender);
                    }
                }
                else if ((fowardsPrefabs != null) && (fowardsPrefabs.Count > 0) &&
                         (midfieldersPrefabs != null) && (midfieldersPrefabs.Count > 0))
                {
                    r = Random.Range(0, 2);
                    if (r == 0)
                    {
                        count += SpawnPlayersFromList(fowardsPrefabs, 1, SsPlayer.positions.forward);
                    }
                    else
                    {
                        count += SpawnPlayersFromList(midfieldersPrefabs, 1, SsPlayer.positions.midfielder);
                    }
                }
                else if ((fowardsPrefabs != null) && (fowardsPrefabs.Count > 0) &&
                         (defendersPrefabs != null) && (defendersPrefabs.Count > 0))
                {
                    r = Random.Range(0, 2);
                    if (r == 0)
                    {
                        count += SpawnPlayersFromList(fowardsPrefabs, 1, SsPlayer.positions.forward);
                    }
                    else
                    {
                        count += SpawnPlayersFromList(defendersPrefabs, 1, SsPlayer.positions.defender);
                    }
                }
                else if ((midfieldersPrefabs != null) && (midfieldersPrefabs.Count > 0) &&
                         (defendersPrefabs != null) && (defendersPrefabs.Count > 0))
                {
                    r = Random.Range(0, 2);
                    if (r == 0)
                    {
                        count += SpawnPlayersFromList(midfieldersPrefabs, 1, SsPlayer.positions.midfielder);
                    }
                    else
                    {
                        count += SpawnPlayersFromList(defendersPrefabs, 1, SsPlayer.positions.defender);
                    }
                }
                else if ((fowardsPrefabs != null) && (fowardsPrefabs.Count > 0))
                {
                    count += SpawnPlayersFromList(fowardsPrefabs, 1, SsPlayer.positions.forward);
                }
                else if ((midfieldersPrefabs != null) && (midfieldersPrefabs.Count > 0))
                {
                    count += SpawnPlayersFromList(midfieldersPrefabs, 1, SsPlayer.positions.midfielder);
                }
                else if ((defendersPrefabs != null) && (defendersPrefabs.Count > 0))
                {
                    count += SpawnPlayersFromList(defendersPrefabs, 1, SsPlayer.positions.defender);
                }
            }
        }


        if ((GoalKeeper == null) && (players != null) && (players.Count > 0) && (players[0] != null))
        {
            // Did Not find a goalkeeper, so make the first player a goalkeeper
            // This is a fail safe so team at least has a goalkeeper, even though he might Not have all the goalkeeper animations
            players[0].Position = SsPlayer.positions.goalkeeper;
        }


        // Link the formations to the players
        if ((players != null) && (players.Count > 0) &&
            (formation != null) && (formation.players != null) && (formation.players.Length > 0))
        {
            for (i = 0; i < players.Count; i++)
            {
                player = players[i];
                if (player == null)
                {
                    continue;
                }
                for (n = 0; n < formation.players.Length; n++)
                {
                    formationPlayer = formation.players[n];
                    if ((formationPlayer != null) &&
                        (formationPlayer.LinkedToPlayer < 0) &&
                        (formationPlayer.position == player.Position))
                    {
                        formationPlayer.LinkedToPlayer = i;
                        player.FormationIndex = n;
                        break;
                    }
                }
            }
        }
    }


    /// <summary>
    /// Spawns the players from the list of prefabs, or take them from the pool of players.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The players from list.</returns>
    /// <param name="prefabs">Prefabs.</param>
    /// <param name="max">Max.</param>
    /// <param name="position">Position.</param>
    protected virtual int SpawnPlayersFromList(List<SsPlayer> prefabs, int max, SsPlayer.positions position)
    {
        if ((prefabs == null) || (prefabs.Count <= 0) || (max <= 0))
        {
            return (0);
        }

        int i, n, count, retry;
        SsPlayer prefab;
        bool found;

        count = 0;
        for (i = 0; i < max; i++)
        {
            found = false;
            retry = 0;
            while (retry < 10)
            {
                prefab = prefabs[Random.Range(0, prefabs.Count)];
                if (SpawnPlayer(prefab, position) != null)
                {
                    count++;
                    found = true;
                    break;
                }
                retry++;
            } //while

            if (found == false)
            {
                // Use the first available one
                for (n = 0; n < prefabs.Count; n++)
                {
                    prefab = prefabs[n];
                    if (SpawnPlayer(prefab, position) != null)
                    {
                        count++;
                        found = true;
                        break;
                    }
                } //for n
            }
        } //for i

        return (count);
    }


    /// <summary>
    /// Spawns the player from the prefab, or take one from the pool.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The player.</returns>
    /// <param name="prefab">Prefab.</param>
    /// <param name="position">Position.</param>
    protected virtual SsPlayer SpawnPlayer(SsPlayer prefab, SsPlayer.positions position)
    {
        if ((field == null) || (prefab == null))
        {
            return (null);
        }

        SsPlayer player = null;
        int i, n;
        SsPlayer poolPlayer;


        // First check if the player is already in the pool
        if ((poolPlayers != null) && (poolPlayers.Count > 0))
        {
            // Two passes: 1st pass checks ID, 2nd pass checks if player can play the same position
            for (n = 0; n < 2; n++)
            {
                for (i = 0; i < poolPlayers.Count; i++)
                {
                    poolPlayer = poolPlayers[i];
                    if (poolPlayer == null)
                    {
                        continue;
                    }

                    if (((n == 0) && (poolPlayer.id == prefab.id)) ||
                        ((n == 1) && (poolPlayer.CanPlayPosition(position))))
                    {
                        player = poolPlayer;
                        player.gameObject.SetActive(true);
                        player.ResetMe();

                        poolPlayers.RemoveAt(i);

                        break;
                    }
                }

                if (player != null)
                {
                    break;
                }
            }
        }


        if (player == null)
        {
            // Not in pool, so spawn him.
            player = SsPlayer.CreatePlayer(prefab);
        }

        player.Team = this;
        player.OtherTeam = otherTeam;
        player.Position = position;
        player.Index = (players != null) ? players.IndexOf(player) : -1;

        return (player);
    }


    /// <summary>
    /// Set/changes the formation at kick off.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The formation at kick off.</returns>
    public virtual void SetFormationAtKickOff(bool firstTime = false)
    {
        if (SsFormationManager.Instance == null)
        {
            if (firstTime)
            {
                // Spawn some players
                SetFormation(null);
            }
            return;
        }

        if (IsUserControlled)
        {
            // Human team
            if (firstTime)
            {
                // Set formation that was selected in the menu
                if ((index >= 0) && (SsSettings.selectedFormationIds != null) &&
                    (index < SsSettings.selectedFormationIds.Length))
                {
                    SetFormation(SsFormationManager.Instance.GetFormation(SsSettings.selectedFormationIds[index]));
                }
                else
                {
                    // Random formation
                    SetFormation(SsFormationManager.Instance.GetRandomFormation(null));
                }
            }
        }
        else if (IsAiControlled)
        {
            // AI team
            if ((firstTime) || (skills.ai.canChangeFormationAtKickOff))
            {
                SetFormation(SsFormationManager.Instance.GetRandomFormation((formation != null) ? formation.id : null));
            }
        }
    }


    /// <summary>
    /// Positions the players on the field (e.g. kick off, throw in, corner kick, etc.).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The players.</returns>
    /// <param name="teamWithBall">Team with ball.</param>
    /// <param name="forState">The state for which to set the start positions.</param>
    public virtual void PositionPlayers(SsTeam teamWithBall, SsMatch.states forState)
    {
        if (field == null)
        {
            return;
        }

        int i;
        SsPlayer player, kickoffPassToPlayer;
        SsFormationPlayer formationPlayer;
        bool found;
        Vector3 minPos, maxPos, pos, vec, throwInPos;
        float width, height, border, randomRadius;
        Bounds playBounds = field.PlayArea;
        Vector2 randomPos;
        Bounds bounds, halfBounds, otherHalfBounds;
        float arcRadiusMin, arcRadiusMax, arcAngleStart, arcAngleEnd, gridSearchRadius, kickoffVerticalDirection;
        SsFieldProperties.SsGridSearchData data;
        Rect rect;

        StopAllBallPredators();
        StopAllBallerPredators();

        randomRadius = field.PlayArea.size.z / 10.0f;   // Randomness to some players' position
        randomPos = Vector2.zero;

        gridSearchRadius = 1.0f;

        arcRadiusMin = 0.0f;
        arcRadiusMax = 0.0f;
        arcAngleStart = 0.0f;
        arcAngleEnd = 0.0f;

        throwInPos = ball.BallOutPosition;
        throwInPos.y = field.GroundY;

        kickoffPassToPlayer = null;
        kickoffVerticalDirection = 0.0f;
        updateMousePassPlayerTime = 0.0f;

        if (this == match.LeftTeam)
        {
            halfBounds = field.LeftHalfArea;
            otherHalfBounds = field.RightHalfArea;
        }
        else
        {
            halfBounds = field.RightHalfArea;
            otherHalfBounds = field.LeftHalfArea;
        }

        if (forState == SsMatch.states.kickOff)
        {
            // Kick off
            //---------
            // Make sure AI update
            delayAiUpdateTime = 0.0f;

            if (this == teamWithBall)
            {
                // Team with ball
                // Determine which player to pass to
                kickoffPassToPlayer = GetFirstPlayer(SsPlayer.positions.forward, match.BallPlayer);
                if (kickoffPassToPlayer == null)
                {
                    // Give the ball to a midfielder
                    kickoffPassToPlayer = GetFirstPlayer(SsPlayer.positions.midfielder, match.BallPlayer);
                    if (kickoffPassToPlayer == null)
                    {
                        // Give the ball to a defender
                        kickoffPassToPlayer = GetFirstPlayer(SsPlayer.positions.defender, match.BallPlayer);
                    }
                }

                // Determine if ball player should stand below or above ball
                if (Random.Range(0, 100) < 50)
                {
                    // Towards the top of ball
                    kickoffVerticalDirection = 1.0f;
                }
                else
                {
                    // Towards the bottom of ball
                    kickoffVerticalDirection = -1.0f;
                }
            }
        }
        else if (forState == SsMatch.states.throwIn)
        {
            // Throw in
            //---------
            arcRadiusMin = Mathf.Max(field.PlayArea.size.z / 10.0f, 5.0f);
            arcRadiusMax = field.PlayArea.size.z * 0.5f;

            if (ball.BallOutPosition.z < field.CentreMark.z)
            {
                // Bottom
                arcAngleStart = -89.0f;     // -90 + 1
                arcAngleEnd = 89.0f;        // 90 - 1
            }
            else
            {
                // Top
                arcAngleStart = 91.0f;      // 90 + 1
                arcAngleEnd = 269.0f;       // 270 - 1
            }
        }
        else if (forState == SsMatch.states.cornerKick)
        {
            // Corner kick
            //------------
            arcRadiusMin = Mathf.Max(field.PlayArea.size.x / 10.0f, 5.0f);
            arcRadiusMax = field.PlayArea.size.x * 0.5f;

            if ((ball.BallOutPosition.x < field.CentreMark.x) && (ball.BallOutPosition.z >= field.CentreMark.z))
            {
                // Top-left corner
                arcAngleStart = 91.0f;  // 90 + 1
                arcAngleEnd = 181.0f;   // 180 - 1

                pos = GeUtils.GetVector3FromAngle(Random.Range(arcAngleStart, arcAngleEnd)) * Random.Range(0.0f, field.cornerRadius);
                pos = new Vector3(pos.x + playBounds.min.x,
                                  field.GroundY,
                                  -pos.y + playBounds.max.z);
            }
            else if ((ball.BallOutPosition.x >= field.CentreMark.x) && (ball.BallOutPosition.z >= field.CentreMark.z))
            {
                // Top-right corner
                arcAngleStart = 181.0f; // 180 + 1
                arcAngleEnd = 269.0f;   // 270 - 1

                pos = GeUtils.GetVector3FromAngle(Random.Range(arcAngleStart, arcAngleEnd)) * Random.Range(0.0f, field.cornerRadius);
                pos = new Vector3(pos.x + playBounds.max.x,
                                  field.GroundY,
                                  -pos.y + playBounds.max.z);
            }
            else if ((ball.BallOutPosition.x >= field.CentreMark.x) && (ball.BallOutPosition.z < field.CentreMark.z))
            {
                // Bottom-right corner
                arcAngleStart = 271.0f; // 270 + 1
                arcAngleEnd = 359.0f;   // 360 - 1

                pos = GeUtils.GetVector3FromAngle(Random.Range(arcAngleStart, arcAngleEnd)) * Random.Range(0.0f, field.cornerRadius);
                pos = new Vector3(pos.x + playBounds.max.x,
                                  field.GroundY,
                                  -pos.y + playBounds.min.z);
            }
            else
            {
                // Bottom-left corner
                arcAngleStart = 1.0f;   // 0 + 1
                arcAngleEnd = 89.0f;    // 90 - 1

                pos = GeUtils.GetVector3FromAngle(Random.Range(arcAngleStart, arcAngleEnd)) * Random.Range(0.0f, field.cornerRadius);
                pos = new Vector3(pos.x + playBounds.min.x,
                                  field.GroundY,
                                  -pos.y + playBounds.min.z);
            }

            ball.transform.position = new Vector3(pos.x,
                                                  field.GroundY + ball.radius,
                                                  pos.z);
        }


        // Loop through the players
        if ((players != null) && (players.Count > 0))
        {
            for (i = 0; i < players.Count; i++)
            {
                player = players[i];
                if (player == null)
                {
                    continue;
                }

                if (player.gameObject.activeInHierarchy == false)
                {
                    player.gameObject.SetActive(true);
                }

                player.Input.ClearInput();  // Prevent lingering user/AI input moving the player
                player.StopMoving(true, true, true, false);
                player.SetMarkPlayer(null, -1, -1);
                player.Ai.ClearTimers();
                player.Particles.StopAllParticles();
                player.SetState(SsPlayer.states.idle, true, SsPlayer.states.idle, null, true);


                if (forState == SsMatch.states.kickOff)
                {
                    // Kick off
                    //---------
                    // Get the formation
                    found = false;
                    if ((formation != null) && (formation.players != null) &&
                        (player.FormationIndex >= 0) && (player.FormationIndex < formation.players.Length) &&
                        (formation.players[player.FormationIndex] != null))
                    {
                        found = true;
                        formationPlayer = formation.players[player.FormationIndex];
                        minPos = formation.MinPos;
                        maxPos = formation.MaxPos;

                        maxPos.x = field.CentreMark.x - player.PersonalSpaceRadius;

                        width = maxPos.x - minPos.x;
                        height = maxPos.z - minPos.z;

                        if (this == match.LeftTeam)
                        {
                            player.SetPosition(new Vector3(minPos.x + (width * formationPlayer.RelativePos.x),
                                field.GroundY, minPos.z + (height * formationPlayer.RelativePos.z)));
                        }
                        else
                        {
                            player.SetPosition(new Vector3(-minPos.x - (width * formationPlayer.RelativePos.x),
                                field.GroundY, minPos.z + (height * formationPlayer.RelativePos.z)));
                        }

                        if (player.Ai != null)
                        {
                            player.Ai.DefaultVerticalPos = field.GetRowAtPoint(formationPlayer.Pos3D);
                        }
                    }

                    if (found == false)
                    {
                        // No formation found! (Should never happen, but this is a fail safe.)
                        if (this == match.LeftTeam)
                        {
                            // Left team
                            if (player.Position == SsPlayer.positions.goalkeeper)
                            {
                                // Position randomly in the goal area
                                player.SetPosition(new Vector3(
                                    Random.Range(field.LeftGoalArea.min.x, field.LeftGoalArea.max.x),
                                    field.GroundY, Random.Range(field.LeftGoalArea.min.z, field.LeftGoalArea.max.z)));
                            }
                            else
                            {
                                // Position randomly in left half of field
                                player.SetPosition(new Vector3(
                                    Random.Range(field.LeftHalfArea.min.x, field.LeftHalfArea.max.x),
                                    field.GroundY, Random.Range(field.LeftHalfArea.min.z, field.LeftHalfArea.max.z)));
                            }
                        }
                        else
                        {
                            // Right team
                            if (player.Position == SsPlayer.positions.goalkeeper)
                            {
                                // Position randomly in the goal area
                                player.SetPosition(new Vector3(
                                    Random.Range(field.RightGoalArea.min.x, field.RightGoalArea.max.x),
                                    field.GroundY, Random.Range(field.RightGoalArea.min.z, field.RightGoalArea.max.z)));
                            }
                            else
                            {
                                // Position randomly in right half of field
                                player.SetPosition(new Vector3(
                                    Random.Range(field.RightHalfArea.min.x, field.RightHalfArea.max.x),
                                    field.GroundY, Random.Range(field.RightHalfArea.min.z, field.RightHalfArea.max.z)));
                            }
                        }

                        if (player.Ai != null)
                        {
                            player.Ai.DefaultVerticalPos = field.GetRowAtPoint(player.transform.position);
                        }
                    }


                    // Adjust position, and make sure it is inside play area
                    pos = new Vector3(player.transform.position.x, field.GroundY, player.transform.position.z);
                    if (player == match.BallPlayer)
                    {
                        // Ball player
                        // Position next to ball, in the centre of field
                        pos.x = ball.transform.position.x - (playDirection * player.BallDribbleDistanceMin);
                        pos.z = ball.transform.position.z + (kickoffVerticalDirection * player.BallDribbleDistanceMin);
                    }
                    else if (player.Position == SsPlayer.positions.goalkeeper)
                    {
                        // Goalkeeper
                        // Select random spot in goalkeeper area
                        bounds = field.GetGoalArea(this);
                        pos = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                                          pos.y,
                                          Random.Range(bounds.min.z, bounds.max.z));
                    }
                    else if (player == kickoffPassToPlayer)
                    {
                        // Player to pass to
                        pos.x = ball.transform.position.x - (playDirection * player.BallDribbleDistanceMin);
                        pos.z = ball.transform.position.z - (kickoffVerticalDirection * player.PersonalSpaceRadius * 2.0f);
                    }
                    else
                    {
                        // Other players
                        // Some randomness to position
                        randomPos = Random.insideUnitCircle * randomRadius;
                        pos.x += randomPos.x;
                        pos.z += randomPos.y;

                        // Make sure player is Not inside the kickoff circle
                        vec = pos - field.CentreMark;
                        vec.y = 0.0f;   // Ignore height
                        if (vec.magnitude < field.CentreRadius + player.PersonalSpaceRadius)
                        {
                            vec.Normalize();
                            pos = field.CentreMark + (vec * Random.Range(field.CentreRadius + player.PersonalSpaceRadius,
                                                                         field.CentreRadius + player.PersonalSpaceRadius + 1.0f));
                            pos.y = field.GroundY;
                        }
                    }

                    if ((player != match.BallPlayer) &&
                        (player != kickoffPassToPlayer))
                    {
                        // Clamp to field half
                        border = Mathf.Max(player.PersonalSpaceRadius * 4.0f, 1.0f);
                        pos.x = Mathf.Clamp(pos.x, halfBounds.min.x + border, halfBounds.max.x - border);
                        pos.z = Mathf.Clamp(pos.z, halfBounds.min.z + border, halfBounds.max.z - border);
                    }

                    player.SetPosition(pos);

                    if (player.Ai != null)
                    {
                        player.Ai.PreferredVerticalPos = player.Ai.DefaultVerticalPos;
                    }

                    // Turn towards ball
                    player.RotateToObject(ball.gameObject);
                }
                else if (forState == SsMatch.states.throwIn)
                {
                    // Throw in
                    //---------
                    pos = new Vector3(player.transform.position.x, field.GroundY, player.transform.position.z);

                    if (player == match.BallPlayer)
                    {
                        // Player with ball
                        if (ball.BallOutPosition.z < field.CentreMark.z)
                        {
                            // Bottom
                            throwInPos.z = field.PlayArea.min.z - player.PersonalSpaceRadius;
                        }
                        else
                        {
                            // Top
                            throwInPos.z = field.PlayArea.max.z + player.PersonalSpaceRadius;
                        }
                        pos = throwInPos;

                        // Move ball near the player. Its position will be updated in the player's update.
                        ball.transform.position = new Vector3(pos.x, field.GroundY + ball.radius, pos.z);

                        player.SetPosition(pos);
                        player.SetState(SsPlayer.states.throwInHold);
                    }
                    else if (player.Position == SsPlayer.positions.goalkeeper)
                    {
                        // Goalkeeper, select random spot in goalkeeper area
                        bounds = field.GetGoalArea(this);
                        pos = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                                          pos.y,
                                          Random.Range(bounds.min.z, bounds.max.z));
                    }
                    else
                    {
                        // Find an open spot
                        data = field.FindOpenGridInArc(ball.BallOutPosition,
                                                       arcAngleStart, arcAngleEnd, arcRadiusMin, arcRadiusMax,
                                                       gridSearchRadius, 1, 5);
                        if ((data != null) && (data.grid != null) && (data.gridWeight <= 0))
                        {
                            // Select a random spot in the open grid
                            pos = new Vector3(Random.Range(data.grid.rect.min.x, data.grid.rect.max.x),
                                              field.GroundY,
                                              Random.Range(data.grid.rect.min.y, data.grid.rect.max.y));
                        }
                        else
                        {
                            if (player.Position == SsPlayer.positions.forward)
                            {
                                // Select random spot in opponent half
                                pos = new Vector3(Random.Range(otherHalfBounds.min.x + player.PersonalSpaceRadius * 2, otherHalfBounds.max.x - player.PersonalSpaceRadius * 2),
                                                  field.GroundY,
                                                  Random.Range(otherHalfBounds.min.z + player.PersonalSpaceRadius * 2, otherHalfBounds.max.z - player.PersonalSpaceRadius * 2));
                            }
                            else
                            {
                                // Select random spot in own half
                                pos = new Vector3(Random.Range(halfBounds.min.x + player.PersonalSpaceRadius * 2, halfBounds.max.x - player.PersonalSpaceRadius * 2),
                                                  field.GroundY,
                                                  Random.Range(halfBounds.min.z + player.PersonalSpaceRadius * 2, halfBounds.max.z - player.PersonalSpaceRadius * 2));
                            }
                        }
                    }

                    if (player != match.BallPlayer)
                    {
                        // Clamp to play area
                        border = Mathf.Max(player.PersonalSpaceRadius, 1.0f);
                        pos.x = Mathf.Clamp(pos.x, playBounds.min.x + border, playBounds.max.x - border);
                        pos.z = Mathf.Clamp(pos.z, playBounds.min.z + border, playBounds.max.z - border);
                    }

                    player.SetPosition(pos);

                    if (player != match.BallPlayer)
                    {
                        // Turn towards ball
                        player.RotateToObject(ball.gameObject);
                    }
                }
                else if (forState == SsMatch.states.cornerKick)
                {
                    // Corner kick
                    //------------
                    pos = new Vector3(player.transform.position.x, field.GroundY, player.transform.position.z);

                    if (player == match.BallPlayer)
                    {
                        // Player with ball
                        if ((ball.BallOutPosition.x < field.CentreMark.x) && (ball.BallOutPosition.z >= field.CentreMark.z))
                        {
                            // Top-left corner
                            throwInPos = GeUtils.GetVector3FromAngle(-45.0f) * player.BallDribbleDistanceMin;
                            throwInPos = new Vector3(throwInPos.x + playBounds.min.x,
                                                     field.GroundY,
                                                     -throwInPos.y + playBounds.max.z);
                        }
                        else if ((ball.BallOutPosition.x >= field.CentreMark.x) && (ball.BallOutPosition.z >= field.CentreMark.z))
                        {
                            // Top-right corner
                            throwInPos = GeUtils.GetVector3FromAngle(45.0f) * player.BallDribbleDistanceMin;
                            throwInPos = new Vector3(throwInPos.x + playBounds.max.x,
                                                     field.GroundY,
                                                     -throwInPos.y + playBounds.max.z);
                        }
                        else if ((ball.BallOutPosition.x >= field.CentreMark.x) && (ball.BallOutPosition.z < field.CentreMark.z))
                        {
                            // Bottom-right corner
                            throwInPos = GeUtils.GetVector3FromAngle(135.0f) * player.BallDribbleDistanceMin;
                            throwInPos = new Vector3(throwInPos.x + playBounds.max.x,
                                                     field.GroundY,
                                                     -throwInPos.y + playBounds.min.z);
                        }
                        else
                        {
                            // Bottom-left corner
                            throwInPos = GeUtils.GetVector3FromAngle(225.0f) * player.BallDribbleDistanceMin;
                            throwInPos = new Vector3(throwInPos.x + playBounds.min.x,
                                                     field.GroundY,
                                                     -throwInPos.y + playBounds.min.z);
                        }
                        pos = throwInPos;
                    }
                    else if (player.Position == SsPlayer.positions.goalkeeper)
                    {
                        // Goalkeeper, select random spot in goalkeeper area
                        bounds = field.GetGoalArea(this);
                        pos = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                                          pos.y,
                                          Random.Range(bounds.min.z, bounds.max.z));
                    }
                    else
                    {
                        // Find an open spot
                        data = field.FindOpenGridInArc(ball.BallOutPosition,
                                                       arcAngleStart, arcAngleEnd, arcRadiusMin, arcRadiusMax,
                                                       gridSearchRadius, 1, 5);
                        if ((data != null) && (data.grid != null) && (data.gridWeight <= 0))
                        {
                            // Select a random spot in the open grid
                            pos = new Vector3(Random.Range(data.grid.rect.min.x, data.grid.rect.max.x),
                                              field.GroundY,
                                              Random.Range(data.grid.rect.min.y, data.grid.rect.max.y));
                        }
                        else
                        {
                            if ((player.Position == SsPlayer.positions.forward) ||
                                (player.Position == SsPlayer.positions.midfielder))
                            {
                                // Select random spot in opponent half
                                pos = new Vector3(Random.Range(otherHalfBounds.min.x + player.PersonalSpaceRadius * 2, otherHalfBounds.max.x - player.PersonalSpaceRadius * 2),
                                                  field.GroundY,
                                                  Random.Range(otherHalfBounds.min.z + player.PersonalSpaceRadius * 2, otherHalfBounds.max.z - player.PersonalSpaceRadius * 2));
                            }
                            else
                            {
                                // Select random spot in own half
                                pos = new Vector3(Random.Range(halfBounds.min.x + player.PersonalSpaceRadius * 2, halfBounds.max.x - player.PersonalSpaceRadius * 2),
                                                  field.GroundY,
                                                  Random.Range(halfBounds.min.z + player.PersonalSpaceRadius * 2, halfBounds.max.z - player.PersonalSpaceRadius * 2));
                            }
                        }
                    }

                    if (player != match.BallPlayer)
                    {
                        // Clamp to play area
                        border = Mathf.Max(player.PersonalSpaceRadius, 1.0f);
                        pos.x = Mathf.Clamp(pos.x, playBounds.min.x + border, playBounds.max.x - border);
                        pos.z = Mathf.Clamp(pos.z, playBounds.min.z + border, playBounds.max.z - border);
                    }

                    player.SetPosition(pos);

                    // Turn towards ball
                    player.RotateToObject(ball.gameObject);
                }
                else if (forState == SsMatch.states.goalKick)
                {
                    // Goal kick
                    //----------
                    pos = new Vector3(player.transform.position.x, field.GroundY, player.transform.position.z);

                    if (player == match.BallPlayer)
                    {
                        // Player with ball (Should be goalkeeper!)

                        // Goalkeeper, select random spot in front of goalkeeper area
                        bounds = field.GetGoalArea(this);
                        if (playDirection > 0)
                        {
                            pos = new Vector3(Random.Range(bounds.center.x, bounds.max.x),
                                              pos.y,
                                              Random.Range(bounds.min.z, bounds.max.z));
                        }
                        else
                        {
                            pos = new Vector3(Random.Range(bounds.min.x, bounds.center.x),
                                              pos.y,
                                              Random.Range(bounds.min.z, bounds.max.z));
                        }

                        // Position ball in front of player
                        if (playDirection > 0)
                        {
                            ball.transform.position = new Vector3(pos.x + player.PersonalSpaceRadius, field.GroundY + ball.radius, pos.z);
                        }
                        else
                        {
                            ball.transform.position = new Vector3(pos.x - player.PersonalSpaceRadius, field.GroundY + ball.radius, pos.z);
                        }
                    }
                    else if (player.Position == SsPlayer.positions.goalkeeper)
                    {
                        // Goalkeeper, select random spot in goalkeeper area
                        bounds = field.GetGoalArea(this);
                        pos = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                                          pos.y,
                                          Random.Range(bounds.min.z, bounds.max.z));
                    }
                    else
                    {
                        // Use the row's height (Z)
                        bounds = field.GetRowBounds(player.Ai.PreferredVerticalPos);
                        rect = new Rect(0, bounds.min.z, 1, bounds.size.z);

                        // Determine the X
                        if (ball.BallOutPosition.x < field.CentreMark.x)
                        {
                            // Ball went out on left side
                            if (this == teamWithBall)
                            {
                                // Defending team
                                if (player.Position == SsPlayer.positions.defender)
                                {
                                    // Defender
                                    // Position in zone 1.5-2.5
                                    bounds = field.GetZoneBounds(1);
                                    rect.x = bounds.center.x;
                                    bounds = field.GetZoneBounds(2);
                                    rect.width = bounds.center.x - rect.x;
                                }
                                else if (player.Position == SsPlayer.positions.midfielder)
                                {
                                    // Midfielder
                                    // Position is zone 2
                                    bounds = field.GetZoneBounds(2);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                                else
                                {
                                    // Forward
                                    // Position is zone 3
                                    bounds = field.GetZoneBounds(3);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                            }
                            else
                            {
                                // Attacking team
                                if (player.Position == SsPlayer.positions.defender)
                                {
                                    // Defender
                                    // Position in zone 5
                                    bounds = field.GetZoneBounds(5);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                                else if (player.Position == SsPlayer.positions.midfielder)
                                {
                                    // Midfielder
                                    // Position in zone 4-5
                                    bounds = field.GetZoneBounds(4);
                                    rect.x = bounds.min.x;
                                    bounds = field.GetZoneBounds(5);
                                    rect.width = bounds.max.x - rect.x;
                                }
                                else
                                {
                                    // Forward
                                    // Position in zone 3-4
                                    bounds = field.GetZoneBounds(3);
                                    rect.x = bounds.min.x;
                                    bounds = field.GetZoneBounds(4);
                                    rect.width = bounds.max.x - rect.x;
                                }
                            }
                        }
                        else
                        {
                            // Ball went out on right side
                            if (this == teamWithBall)
                            {
                                // Defending team
                                if (player.Position == SsPlayer.positions.defender)
                                {
                                    // Defender
                                    // Position in zone 5.5-6.5
                                    bounds = field.GetZoneBounds(5);
                                    rect.x = bounds.center.x;
                                    bounds = field.GetZoneBounds(6);
                                    rect.width = bounds.center.x - rect.x;
                                }
                                else if (player.Position == SsPlayer.positions.midfielder)
                                {
                                    // Midfielder
                                    // Position is zone 5
                                    bounds = field.GetZoneBounds(5);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                                else
                                {
                                    // Forward
                                    // Position is zone 4
                                    bounds = field.GetZoneBounds(4);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                            }
                            else
                            {
                                // Attacking team
                                if (player.Position == SsPlayer.positions.defender)
                                {
                                    // Defender
                                    // Position in zone 2
                                    bounds = field.GetZoneBounds(2);
                                    rect.x = bounds.min.x;
                                    rect.width = bounds.size.x;
                                }
                                else if (player.Position == SsPlayer.positions.midfielder)
                                {
                                    // Midfielder
                                    // Position in zone 2-3
                                    bounds = field.GetZoneBounds(2);
                                    rect.x = bounds.min.x;
                                    bounds = field.GetZoneBounds(3);
                                    rect.width = bounds.max.x - rect.x;
                                }
                                else
                                {
                                    // Forward
                                    // Position in zone 3-4
                                    bounds = field.GetZoneBounds(3);
                                    rect.x = bounds.min.x;
                                    bounds = field.GetZoneBounds(4);
                                    rect.width = bounds.max.x - rect.x;
                                }
                            }
                        }

                        data = field.FindOpenGridInRect(ref rect, 3.0f, 1, 5);
                        if ((data != null) && (data.grid != null) && (data.gridWeight <= 0))
                        {
                            // Select a random spot in the open grid
                            pos = new Vector3(Random.Range(data.grid.rect.min.x, data.grid.rect.max.x),
                                              field.GroundY,
                                              Random.Range(data.grid.rect.min.y, data.grid.rect.max.y));
                        }
                        else
                        {
                            // Select random spot in the rect
                            pos.x = Random.Range(rect.min.x, rect.max.x);
                            pos.z = Random.Range(rect.min.y, rect.max.y);
                        }
                    }

                    player.SetPosition(pos);

                    // Turn towards ball
                    player.RotateToObject(ball.gameObject);
                }


                // Move player slightly down, so he touches the ground
                player.Controller.SimpleMove(Vector3.up * 0.1f);

                player.OnPositionSet();
            }
        }

        UpdateNearestPlayers(0.0f, true);
    }


    /// <summary>
    /// Clears all the players' grid.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The players grid.</returns>
    public virtual void ClearPlayersGrid()
    {
        if ((players == null) || (players.Count <= 0))
        {
            return;
        }

        int i;
        SsPlayer player;

        for (i = 0; i < players.Count; i++)
        {
            player = players[i];
            if (player != null)
            {
                player.ClearGrid();
            }
        }
    }


    /// <summary>
    /// Get the first non-empty name from team name, short name, etc. If none are found then return the game object's name.
    /// </summary>
    /// <returns>The any name.</returns>
    public virtual string GetAnyName()
    {
        if (string.IsNullOrEmpty(teamName) == false)
        {
            return (teamName);
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
    /// Sets the play direction (i.e. left or right).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The play direction.</returns>
    /// <param name="direction">Direction: -1 = left, 1 = right</param>
    /// <param name="isStartOfMatch">Is start of match.</param>
    public virtual void SetPlayDirection(float direction, bool isStartOfMatch = false)
    {
        playDirection = direction;
        if (isStartOfMatch)
        {
            originalPlayDirection = direction;
        }
    }


    /// <summary>
    /// Adds the player to the active list.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>True if added or the player is already in the team.</returns>
    /// <param name="player">Player.</param>
    public virtual bool AddPlayer(SsPlayer player)
    {
        if (player == null)
        {
            return (false);
        }

        if (players == null)
        {
            players = new List<SsPlayer>(maxPlayersInList);
        }

        if ((players == null) || (players.Contains(player)))
        {
            return (((players != null) && (players.Contains(player))));
        }

        player.transform.parent = transform;

        if (player.Input != null)
        {
            player.Input.type = inputType;
            player.Input.team = this;
        }

        players.Add(player);

        return (true);
    }


    /// <summary>
    /// Get the first player of the specified type.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The first player.</returns>
    /// <param name="position">Position.</param>
    /// <param name="excludePlayer">Exclude this player.</param>
    public virtual SsPlayer GetFirstPlayer(SsPlayer.positions position, SsPlayer excludePlayer = null)
    {
        if ((players == null) || (players.Count <= 0))
        {
            return (null);
        }

        int i;
        SsPlayer player;

        for (i = 0; i < players.Count; i++)
        {
            player = players[i];
            if ((player != null) && (player.Position == position) &&
                (player != excludePlayer))
            {
                return (player);
            }
        }

        return (null);
    }


    /// <summary>
    /// Get the first player in the list.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The first player.</returns>
    public virtual SsPlayer GetFirstPlayer()
    {
        if ((players == null) || (players.Count <= 0))
        {
            return (null);
        }
        return (players[0]);
    }


    /// <summary>
    /// Set the player who is being passed to (the receiver).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The pass player.</returns>
    /// <param name="player">Player.</param>
    /// <param name="clearPrevPassPlayer">Clear previous pass player.</param>
    public virtual void SetPassPlayer(SsPlayer player, bool clearPrevPassPlayer)
    {
        if (clearPrevPassPlayer)
        {
            prevPassPlayer = null;
        }
        else if (passPlayer != null)
        {
            prevPassPlayer = passPlayer;
        }

        passPlayer = player;

        selectReceiverAfterPass = false;

        predictPass = false;

        if (player != null)
        {
            // Stop moving, to wait for the ball
            player.StopMoving();

            // Turn towards the ball
            player.RotateToObject(ball.gameObject);

            // A receiver is allowed to touch the ball immediately
            player.CannotTouchBallTimer = 0.0f;

            lastPassPlayer = player;
            passPlayerMinBallDistanceSquared = new Vector2(player.transform.position.x - ball.transform.position.x,
                                                           player.transform.position.z - ball.transform.position.z).sqrMagnitude;
            passPlayerBallDistanceIncreaseTime = 0.0f;
        }
    }


    /// <summary>
    /// Clears the list of ball predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball predators.</returns>
    public virtual void ClearBallPredators()
    {
        if ((ballPredators != null) && (ballPredators.Count > 0))
        {
            int i;
            for (i = 0; i < ballPredators.Count; i++)
            {
                ballPredators[i] = null;
            }
            ballPredators.Clear();
        }
    }


    /// <summary>
    /// Add the player to the list of ball predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball predator.</returns>
    /// <param name="player">Player.</param>
    public virtual void AddBallPredator(SsPlayer player)
    {
        if (ballPredators == null)
        {
            if ((players != null) && (players.Count > 0))
            {
                ballPredators = new List<SsPlayer>(players.Count);
            }
            else
            {
                ballPredators = new List<SsPlayer>();
            }
        }

        if ((player != null) && (ballPredators != null) && (ballPredators.Contains(player) == false))
        {
            ballPredators.Add(player);
        }
    }


    /// <summary>
    /// Remove the player from the list of ball predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball predator.</returns>
    /// <param name="player">Player.</param>
    public virtual void RemoveBallPredator(SsPlayer player)
    {
        if ((player != null) && (ballPredators != null) && (ballPredators.Contains(player)))
        {
            ballPredators.Remove(player);
        }
    }


    /// <summary>
    /// Stops all ball predators from chasing the ball.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The all ball predators.</returns>
    public virtual void StopAllBallPredators()
    {
        if ((ballPredators == null) || (ballPredators.Count <= 0))
        {
            return;
        }

        int i;
        SsPlayer player;

        for (i = 0; i < ballPredators.Count; i++)
        {
            player = ballPredators[i];
            if (player != null)
            {
                player.SetBallPredator(false, true, false);
            }
        }
        ballPredators.Clear();
    }



    /// <summary>
    /// Clears the list of baller predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The baller predators.</returns>
    public virtual void ClearBallerPredators()
    {
        if ((ballerPredators != null) && (ballerPredators.Count > 0))
        {
            int i;
            for (i = 0; i < ballerPredators.Count; i++)
            {
                ballerPredators[i] = null;
            }
            ballerPredators.Clear();
        }
        ballerPredatorsMovingCount = 0;
    }


    /// <summary>
    /// Add the player to the list of ball predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball predator.</returns>
    /// <param name="player">Player.</param>
    public virtual void AddBallerPredator(SsPlayer player)
    {
        if (ballerPredators == null)
        {
            if ((players != null) && (players.Count > 0))
            {
                ballerPredators = new List<SsPlayer>(players.Count);
            }
            else
            {
                ballerPredators = new List<SsPlayer>();
            }
        }

        if ((player != null) && (ballerPredators != null) && (ballerPredators.Contains(player) == false))
        {
            ballerPredators.Add(player);
        }
    }


    /// <summary>
    /// Remove the player from the list of ball predators.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The ball predator.</returns>
    /// <param name="player">Player.</param>
    public virtual void RemoveBallerPredator(SsPlayer player)
    {
        if ((player != null) && (ballerPredators != null) && (ballerPredators.Contains(player)))
        {
            ballerPredators.Remove(player);
        }
    }


    /// <summary>
    /// Stops all baller predators from chasing the player who has the ball.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The all baller predators.</returns>
    public virtual void StopAllBallerPredators()
    {
        if ((ballerPredators == null) || (ballerPredators.Count <= 0))
        {
            return;
        }

        int i;
        SsPlayer player;

        for (i = 0; i < ballerPredators.Count; i++)
        {
            player = ballerPredators[i];
            if (player != null)
            {
                player.SetBallerPredator(false, true, false);
            }
        }
        ballerPredators.Clear();
    }


    /// <summary>
    /// Stop the players' movement when the goalkeeper gets the ball. It only stops those in certain states.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The players when goalkeeper gets ball.</returns>
    public virtual void StopPlayersWhenGoalkeeperGetsBall()
    {
        if ((players == null) || (players.Count <= 0))
        {
            return;
        }

        int i;
        SsPlayer player;

        for (i = 0; i < players.Count; i++)
        {
            player = players[i];

            if ((player == null) ||
                (player == match.BallPlayer) ||
                (player.State == SsPlayer.states.slideTackle) ||
                (player.IsDiving) ||
                (player.State == SsPlayer.states.falling) ||
                (player.State == SsPlayer.states.bicycleKick))
            {
                continue;
            }

            player.StopMoving(true, true, true);
        }
    }


    /// <summary>
    /// Sets the delay sliding time. 
    /// IMPORTANT: "time" is usually in the format: Time.time + delay value
    /// </summary>
    /// <returns>The delay sliding time.</returns>
    /// <param name="time">Time. Usually in the format: Time.time + delay value</param>
    /// <param name="allowLess">Allow setting the time if it is less than the current delay time.</param>
    public virtual void SetDelaySlidingTime(float time, bool allowLess)
    {
        if ((allowLess) || (time > delaySlidingTime))
        {
            delaySlidingTime = time;
        }
    }


    /// <summary>
    /// Gets the delay sliding time. Use a method to differentiate it from the player's delaySlidingTime.
    /// </summary>
    /// <returns>The delay sliding time.</returns>
    public virtual float GetDelaySlidingTime()
    {
        return (delaySlidingTime);
    }


    /// <summary>
    /// Update this instance.
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    public virtual void Update()
    {
        if ((match == null) || (match.IsLoading(true)))
        {
            // Match is busy loading
            return;
        }

        float dt = Time.deltaTime;

        UpdateNearestPlayers(dt);
        UpdatePotentialPassPlayer();
    }


    /// <summary>
    /// Updates the nearest players to the ball, and the foremost players. Also does some other processing of the players (e.g. counts
    /// how many baller predators are moving).
    /// NOTE: Derived methods must call the base method.
    /// </summary>
    /// <returns>The nearest players.</returns>
    /// <param name="dt">Dt.</param>
    public virtual void UpdateNearestPlayers(float dt, bool forceUpdate = false)
    {
        if ((ball == null) || (players == null) || (players.Count <= 0))
        {
            return;
        }

        int i;
        SsPlayer player;
        float distance, nearestDist, nearestHumanDist, nearestAiDist, nearestUnhurtAiDist, nearestNonGoalkeeperDist;
        Vector3 vec;

        nearestPlayerToBall = null;
        nearestHumanPlayerToBall = null;
        nearestAiPlayerToBall = null;
        nearestUnhurtAiPlayerToBall = null;
        nearestNonGoalkeeperToBall = null;
        foremostPlayer = null;
        secondForemostPlayer = null;

        nearestDist = float.MaxValue;
        nearestHumanDist = float.MaxValue;
        nearestAiDist = float.MaxValue;
        nearestUnhurtAiDist = float.MaxValue;
        nearestNonGoalkeeperDist = float.MaxValue;

        ballerPredatorsMovingCount = 0;


        // First pass
        //------------
        for (i = 0; i < players.Count; i++)
        {
            player = players[i];
            if (player == null)
            {
                continue;
            }

            vec = ball.transform.position - player.transform.position;
            vec.y = 0.0f;

            distance = vec.magnitude;

            if (player.IsBallerPredator)
            {
                ballerPredatorsMovingCount++;
            }

            // Foremost player
            if ((foremostPlayer == null) ||
                ((playDirection > 0.0f) && (player.transform.position.x > foremostPlayer.transform.position.x)) ||
                ((playDirection < 0.0f) && (player.transform.position.x < foremostPlayer.transform.position.x)))
            {
                foremostPlayer = player;
            }

            // Nearest player
            if ((nearestDist > distance) || (nearestPlayerToBall == null))
            {
                nearestDist = distance;
                nearestPlayerToBall = player;
            }

            // Nearest non-goalkeeper player
            if (player.Position != SsPlayer.positions.goalkeeper)
            {
                if ((nearestNonGoalkeeperDist > distance) || (nearestNonGoalkeeperToBall == null))
                {
                    nearestNonGoalkeeperDist = distance;
                    nearestNonGoalkeeperToBall = player;
                }
            }


            if (player.IsUserControlled)
            {
                // Human
                if ((nearestHumanDist > distance) || (nearestHumanPlayerToBall == null))
                {
                    nearestHumanDist = distance;
                    nearestHumanPlayerToBall = player;
                }
            }
            else if (player.IsAiControlled)
            {
                // AI
                if ((nearestAiDist > distance) || (nearestAiPlayerToBall == null))
                {
                    nearestAiDist = distance;
                    nearestAiPlayerToBall = player;
                }

                if ((player.IsHurt == false) &&
                    ((nearestUnhurtAiDist > distance) || (nearestUnhurtAiPlayerToBall == null)))
                {
                    nearestUnhurtAiDist = distance;
                    nearestUnhurtAiPlayerToBall = player;
                }
            }
        }


        // Second pass
        //------------
        for (i = 0; i < players.Count; i++)
        {
            player = players[i];
            if (player == null)
            {
                continue;
            }

            // Second foremost player
            if (player != foremostPlayer)
            {
                if ((secondForemostPlayer == null) ||
                    ((playDirection > 0.0f) && (player.transform.position.x > secondForemostPlayer.transform.position.x)) ||
                    ((playDirection < 0.0f) && (player.transform.position.x < secondForemostPlayer.transform.position.x)))
                {
                    secondForemostPlayer = player;
                }
            }
        }

    }


    /// <summary>
    /// Updates the potential player to pass to.
    /// </summary>
    /// <returns>The potential pass player.</returns>
    public virtual void UpdatePotentialPassPlayer(bool useLookVector = false)
    {
        SsPlayer player = match.BallPlayer;

        if ((player != null) && (player.IsUserControlled) &&
            (player.Team == this) && (player.ValidPassAngle > 0.0f))
        {
            SsPlayer ignorePlayer = null;

            // Do Not pass to the goalkeeper during kickoff or corner kick
            if ((match.State == SsMatch.states.kickOff) || (match.State == SsMatch.states.cornerKick))
            {
                ignorePlayer = player.Team.GoalKeeper;
            }

            if ((match.State != SsMatch.states.kickOff) &&
                (player.State != SsPlayer.states.gk_standHoldBall) &&
                (player.Input.type == SsInput.inputTypes.mouse) && (SsSettings.passToMouse))
            {
                // Pass to player near the mouse cursor
                if (passPlayer != null)
                {
                    potentialPassPlayer = null;
                }
                else if ((potentialPassPlayer == null) || (updateMousePassPlayerTime <= Time.time))
                {
                    updateMousePassPlayerTime = Time.time + SsMatchInputManager.Instance.passToMousePlayerUpdateDelay;
                    potentialPassPlayer = GetNearestPlayerToPoint(player.Input.mousePos, SsMatchInputManager.Instance.passToMousePlayerRadius,
                                                                  player, ignorePlayer, null,
                                                                  false,
                                                                  true, SsPlayer.passToMinLastTimeHadBall,
                                                                  true);
                }
            }
            else
            {
                potentialPassPlayer = GetPotentialPassPlayer(player,
                                                             ignorePlayer,
                                                             null, player.ValidPassAngle, SsPlayer.passToMinLastTimeHadBall, false,
                                                             useLookVector);
            }
        }
        else
        {
            potentialPassPlayer = null;
        }
    }



    /// <summary>
    /// Find a potential player to pass to.
    /// </summary>
    /// <returns>The potential pass player.</returns>
    /// <param name="player">Player.</param>
    /// <param name="ignorePlayer1">Ignore player1.</param>
    /// <param name="ignorePlayer2">Ignore player2.</param>
    /// <param name="validAngle">Valid angle.</param>
    /// <param name="minLastTimeHadBall">Minimum last time had ball.</param>
    /// <param name="passForward">Pass forward.</param>
    public virtual SsPlayer GetPotentialPassPlayer(SsPlayer player, SsPlayer ignorePlayer1, SsPlayer ignorePlayer2,
                                                   float validAngle, float minLastTimeHadBall, bool passForward,
                                                   bool useLookVector = false)
    {
        if (player == null)
        {
            return (null);
        }

        SsPlayer passPlayer = null;

        if (passForward)
        {
            // First try to pass forward
            // Try to find an un-marked player
            passPlayer = GetNearestPlayer(player.gameObject, player, null, ignorePlayer1, ignorePlayer2,
                                          validAngle, true, -1, -1, null, false, true, minLastTimeHadBall, -1.0f, true, true,
                                          useLookVector);
            if ((passPlayer == null) || (passPlayer.GetMarkedByPlayer() != null))
            {
                // Try to find any player, even if he is marked
                passPlayer = GetNearestPlayer(player.gameObject, player, null, ignorePlayer1, ignorePlayer2,
                                              validAngle, false, -1, -1, null, false, true, -1.0f, -1.0f, true, true,
                                              useLookVector);
            }
        }
        if (passPlayer == null)
        {
            // Try to find an un-marked player
            passPlayer = GetNearestPlayer(player.gameObject, player, null, ignorePlayer1, ignorePlayer2,
                                          validAngle, true, -1, -1, null, false, true, minLastTimeHadBall, -1.0f, false, true,
                                          useLookVector);
            if ((passPlayer == null) || (passPlayer.GetMarkedByPlayer() != null))
            {
                // Try to find any player, even if he is marked
                passPlayer = GetNearestPlayer(player.gameObject, player, null, ignorePlayer1, ignorePlayer2,
                                              validAngle, false, -1, -1, null, false, true, -1.0f, -1.0f, false, true,
                                              useLookVector);
            }
        }
        return (passPlayer);
    }


    /// <summary>
    /// Get the nearest player to the specified object (i.e. player, ball).
    /// </summary>
    /// <returns>The nearest player.</returns>
    /// <param name="nearThisObject">The object to which to find the nearest player. Can be null if nearThisPlayer or nearThisBall is supplied.</param>
    /// <param name="nearThisPlayer">Find the player near this player.</param>
    /// <param name="nearThisBall">Find the player near this ball.</param>
    /// <param name="ignorePlayer1">Player to ignore in the search. Set to null if Not used.</param>
    /// <param name="ignorePlayer2">Player to ignore in the search. Set to null if Not used.</param>
    /// <param name="validAngle">Only include players in front of the specified object within the angle range. Set to -1 if Not used.</param>
    /// <param name="unmarkedOnly">Indicates if only unmarked players must be included in the search.</param>
    /// <param name="beforeZone">Only include players before this zone. Set to -1 if Not used.</param>
    /// <param name="pastZone">Only include players past this zone. Set to -1 if Not used.</param>
    /// <param name="teamZone">The team to use for the zone test, for the play direction. Set to null if Not used.</param>
    /// <param name="wantToMark">Indicates if nearThisObject wants to mark the nearest player.</param>
    /// <param name="wantToPass">Indicates if nearThisObject wants to pass to the nearest player.</param>
    /// <param name="minLastTimeHadBall">Only include players with lastTimeHadBall greater than this value. Set to -1 if Not used.</param>
    /// <param name="validDistanceSquared">Only include players within this distance from the specified object. Set to -1 if Not used.</param>
    /// <param name="forwardOnly">Indicates if players must only be included infront of the specified object (i.e. in the direction the team is playing).</param>
    /// <param name="ignoreInPain">Indicates if players in pain must be ignored (e.g. falling or lying on the ground).</param>
    /// <param name="useLookVector">Use look vector instead of forward vector (if possible).</param>
    public virtual SsPlayer GetNearestPlayer(GameObject nearThisObject, SsPlayer nearThisPlayer, SsBall nearThisBall,
                                             SsPlayer ignorePlayer1, SsPlayer ignorePlayer2, float validAngle,
                                             bool unmarkedOnly, int beforeZone, int pastZone, SsTeam teamZone, bool wantToMark,
                                             bool wantToPass, float minLastTimeHadBall, float validDistanceSquared, bool forwardOnly,
                                             bool ignoreInPain = false, bool useLookVector = false)
    {
        if ((players == null) || (players.Count <= 0))
        {
            return (null);
        }

        if (nearThisObject == null)
        {
            if (nearThisPlayer != null)
            {
                nearThisObject = nearThisPlayer.gameObject;
            }
            if ((nearThisObject == null) && (nearThisBall != null))
            {
                nearThisObject = nearThisBall.gameObject;
            }
        }

        SsTeam nearThisObjectTeam = (nearThisPlayer != null) ? nearThisPlayer.Team : null;
        int i;
        SsPlayer nearest, player;
        float vecSqrDistance, nearestDistance, distance, angle, cost;
        bool useAngleAsCost;
        Vector3 nearThisObjectMoveVector = Vector3.zero;
        Vector3 nearThisObjectLookVector = (nearThisPlayer != null) ? nearThisPlayer.LookVec : nearThisObject.transform.forward;
        Vector3 vec, facing, tempVec;
        bool nearThisObjectIsMoving = false;

        if (nearThisPlayer != null)
        {
            nearThisObjectMoveVector = nearThisPlayer.MoveVec;
            nearThisObjectIsMoving = nearThisPlayer.IsMoving();
        }
        else if (nearThisBall != null)
        {
            nearThisObjectMoveVector = nearThisBall.MoveVec;
            nearThisObjectIsMoving = nearThisBall.IsMoving();
        }

        nearest = null;
        nearestDistance = float.MaxValue;
        useAngleAsCost = false;
        cost = 0.0f;

        for (i = 0; i < players.Count; i++)
        {
            player = players[i];

            // Test if player should Not be included
            //--------------------------------------
            if ((player == null) || (player.gameObject == nearThisObject) ||
                (player == ignorePlayer1) || (player == ignorePlayer2))
            {
                continue;
            }

            vec = player.transform.position - nearThisObject.transform.position;
            vec.y = 0.0f;   // Ignore height
            vecSqrDistance = vec.sqrMagnitude;

            if ((validDistanceSquared >= 0.0f) && (vecSqrDistance > validDistanceSquared))
            {
                // Too far
                continue;
            }
            else if (validAngle >= 0.0f)
            {
                // Check if within angle
                if ((nearThisObjectIsMoving == false) || (useLookVector))
                {
                    facing = nearThisObjectLookVector;
                }
                else
                {
                    facing = nearThisObjectMoveVector;
                }
                facing.y = 0.0f;    // Ignore height

                angle = Vector3.Angle(facing, vec);
                if (angle > validAngle)
                {
                    continue;
                }
                else
                {
                    if (useAngleAsCost)
                    {
                        cost = angle;
                    }
                    else
                    {
                        // Calculate the cost: a combination of the distance and angle
                        //	Use 3 vectors:
                        //		Vector1 is nearThisObject to object. (vec)
                        //		Vector2 is the forward vector of nearThisObject with a length of the distance to object. (facing)
                        //		Vector3 is the vector from Vector1 to Vector2. (tempVec)
                        //	The cost is the length of Vector3 plus the distance to object.
                        distance = vec.magnitude;
                        facing.Normalize();
                        facing.x *= distance;
                        facing.z *= distance;
                        tempVec = facing - vec;
                        cost = distance + tempVec.magnitude;
                    }
                }
            }

            if ((forwardOnly) && (nearThisObjectTeam != null))
            {
                if (((nearThisObjectTeam.playDirection > 0.0f) && (player.transform.position.x < nearThisObject.transform.position.x)) ||
                    ((nearThisObjectTeam.playDirection < 0.0f) && (player.transform.position.x > nearThisObject.transform.position.x)))
                {
                    continue;
                }
            }

            if ((ignoreInPain) && (player.IsHurt))
            {
                continue;
            }

            // Test if the player is outside the play area, and the other player wants to pass to him
            if ((wantToPass) && (field.IsInPlayArea(player.gameObject) == false))
            {
                continue;
            }

            if ((pastZone >= 0) && (field.PlayerPastZone(player, pastZone, teamZone, false) == false))
            {
                continue;
            }
            else if ((beforeZone >= 0) && (field.PlayerBeforeZone(player, beforeZone, teamZone, false) == false))
            {
                continue;
            }
            else if ((minLastTimeHadBall >= 0.0f) && (player.LastTimeHadBall < minLastTimeHadBall))
            {
                continue;
            }


            // Finally select the player
            //--------------------------

            if (validAngle >= 0.0f)
            {
                // Use the cost
                if ((nearest == null) || (nearestDistance > cost * cost))
                {
                    nearest = player;
                    nearestDistance = cost * cost;
                }
            }
            else
            {
                // Use the distance
                if ((nearest == null) || (nearestDistance > vecSqrDistance))
                {
                    nearest = player;
                    nearestDistance = vecSqrDistance;
                }
            }

        }

        return (nearest);
    }


    /// <summary>
    /// Gets the nearest player to the point.
    /// </summary>
    /// <returns>The nearest player to point.</returns>
    /// <param name="point">Point.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="ignorePlayer1">Ignore player1.</param>
    /// <param name="ignorePlayer2">Ignore player2.</param>
    /// <param name="ignorePlayer3">Ignore player3.</param>
    /// <param name="wantToMark">If set to <c>true</c> want to mark.</param>
    /// <param name="wantToPass">If set to <c>true</c> want to pass.</param>
    /// <param name="minLastTimeHadBall">Minimum last time had ball.</param>
    /// <param name="ignoreInPain">Indicates if players in pain must be ignored (e.g. falling or lying on the ground).</param>
    public virtual SsPlayer GetNearestPlayerToPoint(Vector3 point, float radius,
                                                    SsPlayer ignorePlayer1, SsPlayer ignorePlayer2, SsPlayer ignorePlayer3,
                                                    bool wantToMark,
                                                    bool wantToPass, float minLastTimeHadBall,
                                                    bool ignoreInPain = false)
    {
        if ((players == null) || (players.Count <= 0))
        {
            return (null);
        }

        int i;
        SsPlayer nearest, player;
        float vecSqrDistance, nearestDistance, validDistanceSquared;
        Vector3 vec;

        nearest = null;
        nearestDistance = float.MaxValue;
        validDistanceSquared = radius * radius;

        for (i = 0; i < players.Count; i++)
        {
            player = players[i];

            // Test if player should Not be included
            //--------------------------------------
            if ((player == null) ||
                (player == ignorePlayer1) || (player == ignorePlayer2) || (player == ignorePlayer3))
            {
                continue;
            }

            vec = player.transform.position - point;
            vec.y = 0.0f;   // Ignore height
            vecSqrDistance = vec.sqrMagnitude;

            if ((validDistanceSquared >= 0.0f) && (vecSqrDistance > validDistanceSquared))
            {
                // Too far
                continue;
            }

            if ((ignoreInPain) && (player.IsHurt))
            {
                continue;
            }

            // Test if the player is outside the play area, and the other player wants to pass to him
            if ((wantToPass) && (field.IsInPlayArea(player.gameObject) == false))
            {
                continue;
            }
            else if ((minLastTimeHadBall >= 0.0f) && (player.LastTimeHadBall < minLastTimeHadBall))
            {
                continue;
            }


            // Finally select the player
            //--------------------------

            // Use the distance
            if ((nearest == null) || (nearestDistance > vecSqrDistance))
            {
                nearest = player;
                nearestDistance = vecSqrDistance;
            }

        }

        return (nearest);
    }


    /// <summary>
    /// Make the nearest player to the ball the control player if the team is human controlled.
    /// </summary>
    /// <returns>The nearest player to ball the control player.</returns>
    public virtual void MakeNearestPlayerToBallTheControlPlayer(bool includeGoalkeeper)
    {
        if (IsAiControlled)
        {
            return;
        }

        if ((nearestPlayerToBall != null) &&
            (nearestPlayerToBall.IsAiControlled) &&
            (nearestPlayerToBall.IsHurt == false) &&
            ((includeGoalkeeper) || (nearestPlayerToBall.Position != SsPlayer.positions.goalkeeper)))
        {
            // Make the nearest player the control player
            SsMatchInputManager.SetControlPlayer(nearestPlayerToBall, UserControlIndex);
        }
        else if ((includeGoalkeeper == false) &&
                 (nearestNonGoalkeeperToBall != null) &&
                 (nearestNonGoalkeeperToBall.IsAiControlled) &&
                 (nearestNonGoalkeeperToBall.IsHurt == false))
        {
            //rafikbenamara switch player
            /*// Make the nearest non-goalkeeper player the control player
			SsMatchInputManager.SetControlPlayer(nearestNonGoalkeeperToBall, UserControlIndex);*/
        }
    }

    /// <summary>
    /// Makes the player, who is holding the ball, the control player.
    /// </summary>
    /// <returns>The the hold player the control player.</returns>
    /// <param name="player">Player.</param>
    public virtual void MakeTheHoldPlayerTheControlPlayer(SsPlayer player)
    {
        if ((IsUserControlled) && (player != null) &&
            (player.Position == SsPlayer.positions.goalkeeper) &&
            (player.IsUserControlled == false))
        {
            SsMatchInputManager.SetControlPlayer(player, UserControlIndex);
        }
    }


    /// <summary>
    /// Test if the player collides with any of the other players in the team.
    /// </summary>
    /// <returns>The collided player.</returns>
    /// <param name="player">Player.</param>
    public virtual SsPlayer CollideWithPlayer(SsPlayer player)
    {
        if ((players == null) || (players.Count <= 0))
        {
            return (null);
        }

        int i;
        SsPlayer testPlayer;

        for (i = 0; i < players.Count; i++)
        {
            testPlayer = players[i];
            if ((testPlayer != null) && (player.CollideWithPlayer(testPlayer)))
            {
                return (testPlayer);
            }
        }

        return (null);
    }
}
