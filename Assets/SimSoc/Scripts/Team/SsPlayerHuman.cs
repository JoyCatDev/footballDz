using UnityEngine;
using System.Collections;

/// <summary>
/// Human player. Handles and processes human input.
/// REMINDER: UpdateInput reads the single user input shared by all the players.
/// </summary>
public class SsPlayerHuman : MonoBehaviour
{

    // Const/Static
    //-------------
    // User can press a button just before receiving the ball, to perform an action when the ball is received. The 
    // button must be pressed within this time frame before getting the ball. Do Not make this too long, because it
    // temporarily disables player movement when user switches to the player.
    public const float maxInputPreActionTime = 0.5f;


    // Private
    //--------
    private SsMatch match;                          // Reference to the match
    private SsFieldProperties field;                // Reference to the field
    private SsBall ball;                            // Reference to the ball

    private SsPlayer player;                        // Player to which this is attached
    private SsTeam team;                            // Team to which this player belongs

    private SsMatchInputManager.SsUserInput input;  // Input data. Reference to the player's input;

    private float sprintRemainingTime;
    private float sprintRefillDelay;



    // Methods
    //--------

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        player = gameObject.GetComponentInParent<SsPlayer>();
        if (player != null)
        {
            input = player.Input;
        }
    }


    /// <summary>
    /// Use this for initialization.
    /// </summary>
    void Start()
    {
        match = SsMatch.Instance;
        field = SsFieldProperties.Instance;
        ball = SsBall.Instance;

        if (player != null)
        {
            team = player.Team;

            if (SsMatchInputManager.Instance != null)
            {
                sprintRemainingTime = SsMatchInputManager.Instance.sprintTimeMax;
            }
            else
            {
                sprintRemainingTime = 2.0f;
            }
            sprintRefillDelay = 0.0f;
        }
    }


    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
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

        player = null;
        team = null;
        input = null;
    }


    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        float dt = Time.deltaTime;

        if (player.IsUserControlled)
        {
            UpdateInput(dt);
            UpdateSprint(dt);
            ProcessInput(dt);
        }
        else
        {
            UpdateSprint(dt);

            // Does the player belong to a human controlled team, and he is being passed to?
            if ((team.IsUserControlled) && (player == team.PassPlayer) && (team.ControlPlayer != null))
            {
                // Read input from the current control player
                SsMatchInputManager.SsUserInput tempInput = team.ControlPlayer.Input;
                bool preActionButton2 = ((tempInput.wasPressedPassPlayerButton2 == player) &&
                                         (tempInput.wasPressedTimeButton2 + maxInputPreActionTime >= Time.time));
                if (preActionButton2)
                {
                    if (player.CanBicycleKick())
                    {
                        // Bicycle kick started
                    }
                }
            }
        }
    }


    /// <summary>
    /// Updates the input.
    /// </summary>
    /// <returns>The input.</returns>
    /// <param name="dt">Dt.</param>
    void UpdateInput(float dt)
    {
        if (match == null)
        {
            return;
        }

        input.ClearInput();

        // Read input from the input manager
        // REMINDER: This reads from the single user input shared by all the players. So certain data (e.g. wasPressedPassPlayerButton2)
        //			may have been set by the previously controlled player.
        input.axes = SsMatchInputManager.GetAxes(player.UserControlIndex);
        input.lookAxes = SsMatchInputManager.GetLookAxes(player.UserControlIndex);
        input.mousePos = SsMatchInputManager.GetMousePos(player.UserControlIndex);

        input.button1 = SsMatchInputManager.GetButton1(player.UserControlIndex);
        input.button2 = SsMatchInputManager.GetButton2(player.UserControlIndex);
        input.button3 = SsMatchInputManager.GetButton3(player.UserControlIndex);

        input.wasPressedButton1 = SsMatchInputManager.GetWasPressedButton1(player.UserControlIndex);
        input.wasPressedButton2 = SsMatchInputManager.GetWasPressedButton2(player.UserControlIndex);
        input.wasPressedButton3 = SsMatchInputManager.GetWasPressedButton3(player.UserControlIndex);

        input.wasPressedTimeButton1 = SsMatchInputManager.GetWasPressedTimeButton1(player.UserControlIndex);
        input.wasPressedTimeButton2 = SsMatchInputManager.GetWasPressedTimeButton2(player.UserControlIndex);

        input.wasPressedPlayerButton1 = SsMatchInputManager.GetWasPressedPlayerButton1(player.UserControlIndex);
        input.wasPressedPlayerButton2 = SsMatchInputManager.GetWasPressedPlayerButton2(player.UserControlIndex);

        input.wasPressedPassPlayerButton1 = SsMatchInputManager.GetWasPressedPassPlayerButton1(player.UserControlIndex);
        input.wasPressedPassPlayerButton2 = SsMatchInputManager.GetWasPressedPassPlayerButton2(player.UserControlIndex);

        input.wasPressedAxesButton1 = SsMatchInputManager.GetWasPressedAxesButton1(player.UserControlIndex);
        input.wasPressedAxesButton2 = SsMatchInputManager.GetWasPressedAxesButton2(player.UserControlIndex);

        input.wasPressedPassAxesButton1 = SsMatchInputManager.GetWasPressedPassAxesButton1(player.UserControlIndex);
        input.wasPressedPassAxesButton2 = SsMatchInputManager.GetWasPressedPassAxesButton2(player.UserControlIndex);
    }


    /// <summary>
    /// Updates the sprint state.
    /// </summary>
    /// <returns>The sprint.</returns>
    /// <param name="dt">Dt.</param>
    private void UpdateSprint(float dt)
    {
        if (player.IsUserControlled)
        {
            // Human
            //------

            if (input.button3)
            {
                if (sprintRemainingTime > 0.0f)
                {
                    input.sprintActive = true;
                    // Decrease the sprint "bar"
                    sprintRemainingTime = Mathf.Max(sprintRemainingTime - dt, 0.0f);
                }

                sprintRefillDelay = SsMatchInputManager.Instance.sprintRefillDelayMax;
            }
            else
            {
                input.sprintActive = false;
                if (sprintRefillDelay > 0.0f)
                {
                    // Delay before refilling the sprint "bar"
                    sprintRefillDelay = Mathf.Max(sprintRefillDelay - dt, 0.0f);
                }
                else
                {
                    // Refill the sprint "bar"
                    sprintRemainingTime = Mathf.Min(sprintRemainingTime + (SsMatchInputManager.Instance.sprintRefillSpeed * dt),
                                                    SsMatchInputManager.Instance.sprintTimeMax);
                }
            }
        }
        else
        {
            // AI
            //---
            // This happens when player's control is switched from human to AI

            // NOTE: For now set it to false, but need to set it false somewhere else when code added to allow AI to sprint
            input.sprintActive = false;

            if (sprintRefillDelay > 0.0f)
            {
                // Delay before refilling the sprint "bar"
                sprintRefillDelay = Mathf.Max(sprintRefillDelay - dt, 0.0f);
            }
            else
            {
                // Refill the sprint "bar"
                sprintRemainingTime = Mathf.Min(sprintRemainingTime + (SsMatchInputManager.Instance.sprintRefillSpeed * dt),
                                                SsMatchInputManager.Instance.sprintTimeMax);
            }
        }
    }


    /// <summary>
    /// Processes the input.
    /// </summary>
    /// <returns>The input.</returns>
    /// <param name="dt">Dt.</param>
    void ProcessInput(float dt)
    {
        Vector3 pos, shootPos, vec;
        bool canMove, canTurn, preActionButton1, preActionButton2;
        bool kickStraightAhead = false;
        SsPlayer switchTo;
        Vector3 tempLookVec = new Vector3(input.lookAxes.x, 0.0f, input.lookAxes.y);
        Vector3 kickAheadVec = transform.forward;
        float kickAheadDistanceMin, kickAheadDistanceMax;

        canMove = false;
        canTurn = false;
        pos = Vector3.zero;
        kickAheadDistanceMin = SsPlayer.kickToNoOneAheadDistanceMin;
        kickAheadDistanceMax = SsPlayer.kickToNoOneAheadDistanceMax;

        // Did user press a button just before getting the ball?
        preActionButton1 = ((input.wasPressedPassPlayerButton1 == player) && (input.wasPressedTimeButton1 + maxInputPreActionTime >= Time.time));
        preActionButton2 = ((input.wasPressedPassPlayerButton2 == player) && (input.wasPressedTimeButton2 + maxInputPreActionTime >= Time.time));
        if ((input.type == SsInput.inputTypes.mouse) &&
            ((preActionButton1) || (preActionButton2)))
        {
            // Mouse input
            // Change axes to point from this player to the mouse (it was pointing from previous control player to the mouse)
            if (preActionButton1)
            {
                input.wasPressedAxesButton1 = input.wasPressedPassAxesButton1;
            }
            if (preActionButton2)
            {
                input.wasPressedAxesButton2 = input.wasPressedPassAxesButton2;
            }
        }


        // Test if the user can move
        if ((match.State == SsMatch.states.play) ||
            (match.State == SsMatch.states.preThrowIn) ||
            (match.State == SsMatch.states.preGoalKick) ||
            (match.State == SsMatch.states.preCornerKick) ||
            (match.State == SsMatch.states.goal))
        {
            if ((player.StateMatrix.CanUserMove(player.State)) &&
                (preActionButton1 == false) && (preActionButton2 == false))
            {
                canMove = true;
                canTurn = true;
            }
        }

        if ((canTurn == false) &&
            (preActionButton1 == false) && (preActionButton2 == false) &&
            (player.IsHurt == false))
        {
            if ((player.State == SsPlayer.states.idle) ||
                (player.State == SsPlayer.states.gk_standHoldBall) ||
                (player.State == SsPlayer.states.run) ||
                (player.State == SsPlayer.states.throwInHold))
            {
                canTurn = true;
            }
        }

        if (canMove == false)
        {
            input.axes = Vector2.zero;
        }
        if (canTurn == false)
        {
            input.lookAxes = Vector2.zero;
        }


        if (player.CanShootOrPass())
        {
            // Has ball
            //---------

            // Button 1: Pass
            if ((input.wasPressedButton1) || (preActionButton1))
            {
                if (preActionButton1)
                {
                    // Temporarily change lookVec so player passes in the look direction
                    pos = player.LookVec;
                    tempLookVec = new Vector3(input.wasPressedAxesButton1.x, 0.0f, input.wasPressedAxesButton1.y);
                    player.LookVec = tempLookVec;
                }

                if ((team.PotentialPassPlayer == null) || (preActionButton1))
                {
                    // Try to find a player to pass to
                    team.UpdatePotentialPassPlayer(preActionButton1);
                }

                if (team.PotentialPassPlayer != null)
                {
                    // Pass to another player
                    player.Pass(team.PotentialPassPlayer, Vector3.zero);

                    team.SelectReceiverAfterPass = true;
                }
                else if ((match.State != SsMatch.states.kickOff) &&
                         (player.State != SsPlayer.states.gk_standHoldBall))
                {
                    // Kick straight ahead (i.e. pass to no-one)
                    kickStraightAhead = true;

                    if (preActionButton1)
                    {
                        kickAheadVec = tempLookVec;
                    }
                    else if ((input.type == SsInput.inputTypes.mouse) && (SsSettings.passToMouse))
                    {
                        // Kick towards the mouse position
                        kickAheadVec = input.mousePos - transform.position;
                        kickAheadDistanceMin = 1.0f;
                        kickAheadDistanceMax = 1.0f;
                    }
                }

                if (preActionButton1)
                {
                    // Restore lookVec
                    player.LookVec = pos;
                }
            }

            // Button 2: Shoot
            else if ((input.wasPressedButton2) || (preActionButton2))
            {
                bool canShoot = false;

                if (preActionButton2)
                {
                    // Temporarily change lookVec so player passes in the look direction
                    pos = player.LookVec;
                    tempLookVec = new Vector3(input.wasPressedAxesButton2.x, 0.0f, input.wasPressedAxesButton2.y);
                    player.LookVec = tempLookVec;
                }

                shootPos = Vector3.zero;

                if ((match.State == SsMatch.states.play) &&
                    (player.State != SsPlayer.states.gk_standHoldBall))
                {
                    // First try to shoot to goal
                    canShoot = player.CanShootToGoal(out shootPos, (team.PotentialPassPlayer == null),
                                                     false, true,
                                                     preActionButton2,
                                                     false);
                }

                if (canShoot)
                {
                    // Go for goal
                    player.Shoot(shootPos, true);
                }
                else if ((match.State != SsMatch.states.kickOff) &&
                         (player.State != SsPlayer.states.gk_standHoldBall))
                {
                    // Kick straight ahead (i.e. pass to no-one)
                    kickStraightAhead = true;

                    if (preActionButton2)
                    {
                        kickAheadVec = tempLookVec;
                    }
                    else if ((input.type == SsInput.inputTypes.mouse) && (SsSettings.passToMouse))
                    {
                        // Kick towards the mouse position
                        kickAheadVec = input.mousePos - transform.position;
                        kickAheadDistanceMin = 1.0f;
                        kickAheadDistanceMax = 1.0f;
                    }
                }

                if (preActionButton2)
                {
                    // Restore lookVec
                    player.LookVec = pos;
                }
            }
        }
        else if ((match.State == SsMatch.states.play) &&
                 (player != match.BallPlayer))
        {
            //rafikbenamara  switch player
            // No ball
            //--------

            // Button 1: Switch
            if (input.wasPressedButton1)
            {
               /* // Only allow 1 switch per update
                if (team.ControlPlayerChangeTime != Time.time)
                {
                    // Switch player
                    if ((team.SelectReceiverAfterPass) && (team.PassPlayer != null) && (team.PassPlayer.IsVisible))
                    {
                        // Select the receiver immediately after a pass (but only if he is on screen)
                        switchTo = team.PassPlayer;
                        team.SelectReceiverAfterPass = false;
                    }
                    else
                    {
                        switchTo = team.GetNearestPlayer(null, null, SsBall.Instance,
                                                         player, null, -1.0f, false, -1, -1, null, false, false, -1.0f, -1.0f, false, true);
                    }

                    if (switchTo != null)
                    {
                        SsMatchInputManager.SetControlPlayer(switchTo, player.UserControlIndex);
                    }
                }*/
            }

            // Button 2: Tackle/Dive/Bicycle Kick
            else if (input.wasPressedButton2)
            {
                if (player.CanSlide(out vec))
                {
                    player.SetState(SsPlayer.states.slideTackle, true, SsPlayer.states.idle, vec);
                }
                else if ((player.Position == SsPlayer.positions.goalkeeper) &&
                         (DiveToBallIfBeforeZone(2)))
                {
                    // Dive started
                }
                else if (player.CanBicycleKick())
                {
                    // Bicycle kick started
                }
            }
        }


        if (kickStraightAhead)
        {
            // Kick straight ahead (i.e. pass to no-one)
            pos = transform.position + (kickAheadVec * Random.Range(kickAheadDistanceMin, kickAheadDistanceMax));
            player.Pass(null, pos, true);
        }
    }


    /// <summary>
    /// Goalkeeper dives towards the ball if it is flying towards the goalpost and it is before the specified zone.
    /// </summary>
    /// <returns>The to ball if before zone.</returns>
    /// <param name="zone">Zone.</param>
    private bool DiveToBallIfBeforeZone(int zone)
    {
        if (match.BallPlayer != null)
        {
            return (false);
        }

        Vector3 intersectPoint = ball.transform.position;
        bool validDive = false;

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

}
