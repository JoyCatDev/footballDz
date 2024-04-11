using UnityEngine;
using System.Collections;

/// <summary>
/// Forward AI.
/// </summary>
public class SsPlayerAiForward : SsPlayerAi {

	// Methods
	//--------

	/// <summary>
	/// Update AI: Team goalkeeper has ball.
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateTeamGoalkeeperHasBall(float dt)
	{
		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		if (RunStraightForwardIfInPenaltyArea())
		{
			return (true);
		}


		if (RunForwardIfBeforeZone(2))
		{
			return (true);
		}


		if (RunToOpenSpotBetweenZonesInRowIfGoalkeeperHoldsBall(2, 4, preferredVerticalPos))
		{
			return (true);
		}


		return (false);
	}


	/// <summary>
	/// Update AI: Other team goalkeeper has the ball
	/// </summary>
	/// <returns>True if the AI made a decision/action, or no other decisions/actions must be made during this update.</returns>
	/// <param name="dt">Dt.</param>
	public override bool UpdateOtherTeamGoalkeeperHasBall(float dt)
	{
		if (RunStraightBackwardIfPastZone(runBackBeforeZoneWhenGoalkeerHasBall))
		{
			return (true);
		}


		if (RunToOpenSpotBetweenZonesInRowIfGoalkeeperHoldsBall(3, 4, preferredVerticalPos))
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


		if (PredictPassPreyOnBall())
		{
			return (true);
		}


		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		if (field.IsBallBeforeHalfway(team))
		{
			// Mark nearest unmarked opponent before 2/8 line (i.e. help the defenders if opponent too close to goalpost.).
			if (MarkNearestUnmarkedOpponentInZone(2, -1))
			{
				return (true);
			}

			if (RunUpDownToOpenSpotIfTooCloseToBaller())
			{
				return (true);
			}

			if (FindOpenSpotAheadAndCloseToBall())
			{
				return (true);
			}

			// Find open spot before 5/8 line if opponent close.
			if (RunToOpenSpotBetweenZonesInRowIfOpponentClose(0, 4, preferredVerticalPos))
			{
				return (true);
			}

			return (true);
		}


		if (field.IsBallPastHalfway(team))
		{
			// Find open spot past half way if opponent close.
			if (RunToOpenSpotBetweenZonesInRowIfOpponentClose(4, SsFieldProperties.maxZones - 1, preferredVerticalPos))
			{
				return (true);
			}


			if (RunBackwardIfPastZone(7))
			{
				return (true);
			}


			if (RunUpDownToOpenSpotIfTooCloseToBaller())
			{
				return (true);
			}


			if (RunForward())
			{
				return (true);
			}

			return (true);
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
		// Any time
		if (TackleBaller())
		{
			return (true);
		}

		if (PreyOnBaller())
		{
			return (true);
		}

		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


		if (player.IsBallerPredator)
		{
			if (BallerPredatorTakingTooLong())
			{
				return (true);
			}

			// Still busy running to the baller
			return (true);
		}


		if (field.IsBallBeforeHalfway(team))
		{
			// Mark nearest unmarked opponent before 5/8 line.
			if (MarkNearestUnmarkedOpponentInZone(5, -1))
			{
				return (true);
			}


			// Find open spot before 5/8 line.
			if (RunToOpenSpotBetweenZonesInRow(0, 4, preferredVerticalPos))
			{
				return (true);
			}

			return (true);
		}


		if (field.IsBallPastHalfway(team))
		{
			// Mark nearest unmarked opponent past 3/8 line and before 5/8 line.
			if (MarkNearestUnmarkedOpponentInZone(5, 3))
			{
				return (true);
			}


			// Find open spot past half way.
			if (RunToOpenSpotBetweenZonesInRow(4, 8, preferredVerticalPos))
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
		// Prey on ball, any time
		if (PreyOnBall())
		{
			return (true);
		}


		if (WaitForPassedBallIfPlayerIsReceiver())
		{
			return (true);
		}


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


		if (field.IsBallBeforeHalfway(team))
		{
			// Mark nearest unmarked opponent before 2/8 line (i.e. help the defenders if opponent too close to goalpost.).
			if (MarkNearestUnmarkedOpponentInZone(2, -1))
			{
				return (true);
			}
			
			// Find open spot between 3/8 and 5/8 lines in preferred row.
			if (RunToOpenSpotBetweenZonesInRow(3, 4, preferredVerticalPos))
			{
				return (true);
			}

			return(true);
		}


		if (field.IsBallPastHalfway(team))
		{
			// Find open spot between 4/8 and 7/8 lines in preferred row.
			if (RunToOpenSpotBetweenZonesInRow(4, 6, preferredVerticalPos))
			{
				return (true);
			}
			
			if (RunBackwardIfPastZone(7))
			{
				return (true);
			}
			
			if (RunForward())
			{
				return (true);
			}

			return (true);
		}


		
		return (false);
	}

}
