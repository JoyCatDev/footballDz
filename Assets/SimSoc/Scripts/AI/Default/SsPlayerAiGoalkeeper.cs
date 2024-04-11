using UnityEngine;
using System.Collections;

/// <summary>
/// Goalkeeper AI.
/// </summary>
public class SsPlayerAiGoalkeeper : SsPlayerAi {

	// Methods
	//--------

	/// <summary>
	/// Update AI: Team goalkeeper has ball.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateTeamGoalkeeperHasBall(float dt)
	{
		if (HoldBall())
		{
			return (true);
		}


		if ((match.BallPlayer.State == SsPlayer.states.gk_standHoldBall) && 
		    (Pass(false, false, false)))
		{
			return (true);
		}


		// The goalkeeper has to pass the ball
		return (true);
	}


	/// <summary>
	/// Update AI: Other team goalkeeper has the ball
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateOtherTeamGoalkeeperHasBall(float dt)
	{
		if (RunStraightBackwardIfPastZone(runBackBeforeZoneWhenGoalkeerHasBall - 1))
		{
			return (true);
		}
		
		return (false);
	}


	/// <summary>
	/// Update AI: Team has the ball (Not goalkeeper).
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateTeamHasBallNotGoalkeeper(float dt)
	{
		// Player has the ball
		if (player == match.BallPlayer)
		{
			// Past half way
			if (field.IsBallPastHalfway(team))
			{
				if (ShootIfCloseToGoal())
				{
					return (true);
				}
				
				// Run backwards
				if (RunBackwardIfPastZone(7.8f))
				{
					return (true);
				}
			}
			
			
			// Anywhere on field
			
			if (PassIfOpponentClose())
			{
				return (true);
			}
			
			
			if (PassIfRanLongDistance())
			{
				return (true);
			}
			
			
			if (SideStep())
			{
				return (true);
			}
			
			
			if (PassIfOpponentSlides())
			{
				return (true);
			}
			
			
			if (RunForward())
			{
				return (true);
			}
			
			return (true);
		}


		// Any time

		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		// Dive towards ball if ball is flying towards the goalpost and it is before the 2/8 line.
		if (DiveToBallIfBeforeZone(2))
		{
			return (true);
		}


		if (field.IsBallBeforeHalfway(team))
		{
			if (FollowBallVerticallyAndStayInGoalkeeperArea())
			{
				return (true);
			}

			return (true);
		}


		if (field.IsBallPastHalfway(team))
		{
			if (StandStillInGoalkeeperArea(team))
			{
				return (true);
			}
		}

		return (false);
	}



	/// <summary>
	/// Update AI: Other team has the ball (Not goalkeeper).
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateOtherTeamHasBallNotGoalkeeper(float dt)
	{
		if (player.IsBallerPredator)
		{
			if (BallerPredatorTakingTooLong())
			{
				return (true);
			}
			
			// Still busy running to the baller
			return (true);
		}


		// Dive towards ball if ball is flying towards the goalpost and it is before the 2/8 line.
		if (DiveToBallIfBeforeZone(2))
		{
			return (true);
		}


		if (DiveAtOpponentInPenaltyArea())
		{
			return (true);
		}


		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		// Get the ball if it is in the penalty area
		if ((field.IsInPenaltyArea(ball.gameObject, team)) && 
		    (PreyOnBall()))
		{
			return (true);
		}


		if (field.IsBallBeforeHalfway(team))
		{
			if (FollowBallVerticallyAndStayInGoalkeeperArea())
			{
				return (true);
			}

			return (true);
		}


		if (field.IsBallPastHalfway(team))
		{
			if (StandStillInGoalkeeperArea(team))
			{
				return (true);
			}

			return (true);
		}


		return (false);
	}


	/// <summary>
	/// Update AI: No team has the ball.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateNoTeamHasBall(float dt)
	{
		// Running to the ball
		if (player.IsBallPredator)
		{
			// Taking too long to get the ball?
			if (BallPredatorTakingTooLong())
			{
				return (true);
			}
			
			// Still busy running to the ball
			return (true);
		}


		// Dive towards ball if ball is flying towards the goalpost and it is before the 2/8 line.
		if (DiveToBallIfBeforeZone(2))
		{
			return (true);
		}


		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		// Get the ball if it is in the penalty area
		if ((field.IsInPenaltyArea(ball.gameObject, team)) && 
		    (PreyOnBall()))
		{
			return (true);
		}


		// Ball before half way
		if (field.IsBallBeforeHalfway(team))
		{
			if (FollowBallVerticallyAndStayInGoalkeeperArea())
			{
				return (true);
			}

			return (true);
		}


		// Ball past half way
		if (field.IsBallPastHalfway(team))
		{
			if (StandStillInGoalkeeperArea(team))
			{
				return (true);
			}

			return (true);
		}

		
		return (false);
	}
}
