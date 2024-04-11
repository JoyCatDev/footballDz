using UnityEngine;
using System.Collections;

/// <summary>
/// Tournament log item for UI. It is dynamically added to the log's scroll view list.
/// </summary>
public class SsTournamentLogItem : MonoBehaviour {

	// Public
	//-------
	public UnityEngine.UI.Text pos;
	public UnityEngine.UI.Text team;
	public UnityEngine.UI.Text played;
	public UnityEngine.UI.Text won;
	public UnityEngine.UI.Text draw;
	public UnityEngine.UI.Text lost;
	public UnityEngine.UI.Text goalDiff;
	public UnityEngine.UI.Text points;
	public GameObject bgEvenRow;
	public GameObject bgOddRow;


	// Methods
	//--------

	/// <summary>
	/// Sets the info.
	/// </summary>
	/// <returns>The info.</returns>
	/// <param name="stats">Stats.</param>
	public void SetInfo(SsTeamStats stats)
	{
		if (stats == null)
		{
			return;
		}

		bool visible;
		string teamName;

		if (pos != null)
		{
			pos.text = (stats.tournamentLogPosition + 1).ToString();
		}

		if (team != null)
		{
			teamName = stats.teamName;
			if (string.IsNullOrEmpty(teamName))
			{
				teamName = stats.teamId;
			}

			if (stats.tournamentHumanIndex != -1)
			{
				team.text = "(P" + (stats.tournamentHumanIndex + 1) + ") " + teamName;
			}
			else
			{
				team.text = teamName;
			}
		}

		if (played != null)
		{
			played.text = stats.tournamentLogPlayed.ToString();
		}

		if (won != null)
		{
			won.text = stats.tournamentLogWon.ToString();
		}

		if (draw != null)
		{
			draw.text = stats.tournamentLogDraw.ToString();
		}

		if (lost != null)
		{
			lost.text = stats.tournamentLogLost.ToString();
		}

		if (goalDiff != null)
		{
			goalDiff.text = stats.tournamentGoalDifference.ToString();
		}

		if (points != null)
		{
			points.text = stats.tournamentPoints.ToString();
		}


		visible = ((stats.tournamentLogPosition % 2) == 0);
		if ((bgEvenRow != null) && (bgEvenRow.activeInHierarchy != visible))
		{
			bgEvenRow.SetActive(visible);
		}

		visible = ((stats.tournamentLogPosition % 2) == 1);
		if ((bgOddRow != null) && (bgOddRow.activeInHierarchy != visible))
		{
			bgOddRow.SetActive(visible);
		}
	}

}
