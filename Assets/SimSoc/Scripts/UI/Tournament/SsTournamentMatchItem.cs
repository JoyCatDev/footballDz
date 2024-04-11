using UnityEngine;
using System.Collections;

/// <summary>
/// Tournament match item for UI. It is dynamically added to the matches' scroll view list.
/// </summary>
public class SsTournamentMatchItem : MonoBehaviour {

	// Public
	//-------
	public UnityEngine.UI.Text no;
	public UnityEngine.UI.Text leftTeam;
	public UnityEngine.UI.Text leftTeamScore;
	public UnityEngine.UI.Text rightTeam;
	public UnityEngine.UI.Text rightTeamScore;
	public GameObject bgEvenRow;
	public GameObject bgOddRow;


	// Methods
	//--------

	/// <summary>
	/// Sets the info.
	/// </summary>
	/// <returns>The info.</returns>
	/// <param name="matchInfo">Match info.</param>
	public void SetInfo(SsTournamentMatchInfo matchInfo, int matchIndex)
	{
		if (matchInfo == null)
		{
			return;
		}

		int i;
		string teamName;
		bool visible;

		if (no != null)
		{
			no.text = (matchIndex + 1).ToString();
		}

		i = 0;
		if (leftTeam != null)
		{
			teamName = matchInfo.teamName[i];
			if (string.IsNullOrEmpty(teamName))
			{
				teamName = matchInfo.teamId[i];
			}
			if (string.IsNullOrEmpty(teamName))
			{
				teamName = "?";
			}

			if (matchInfo.humanIndex[i] != -1)
			{
				leftTeam.text = $"(P{matchInfo.humanIndex[i] + 1}) {teamName}";
			}
			else
			{
				leftTeam.text = teamName;
			}
		}
		if (leftTeamScore != null)
		{
			if (matchInfo.teamScore[i] < 0)
			{
				leftTeamScore.text = "";
			}
			else
			{
				leftTeamScore.text = matchInfo.teamScore[i].ToString();
			}
		}


		i = 1;
		if (rightTeam != null)
		{
			teamName = matchInfo.teamName[i];
			if (string.IsNullOrEmpty(teamName))
			{
				teamName = matchInfo.teamId[i];
			}
			if (string.IsNullOrEmpty(teamName))
			{
				teamName = "?";
			}

			if (matchInfo.humanIndex[i] != -1)
			{
				rightTeam.text = $"(P{matchInfo.humanIndex[i] + 1}) {teamName}";
			}
			else
			{
				rightTeam.text = teamName;
			}
		}
		if (rightTeamScore != null)
		{
			if (matchInfo.teamScore[i] < 0)
			{
				rightTeamScore.text = "";
			}
			else
			{
				rightTeamScore.text = matchInfo.teamScore[i].ToString();
			}
		}


		visible = ((matchIndex % 2) == 0);
		if ((bgEvenRow != null) && (bgEvenRow.activeInHierarchy != visible))
		{
			bgEvenRow.SetActive(visible);
		}
		
		visible = ((matchIndex % 2) == 1);
		if ((bgOddRow != null) && (bgOddRow.activeInHierarchy != visible))
		{
			bgOddRow.SetActive(visible);
		}
	}

}
