using UnityEngine;
using UnityEngine.UI;

namespace SimSoc
{
	/// <summary>
	/// Tournament group item for UI. Used by the <see cref="TournamentGroupsScreen"/>.
	/// </summary>
	public class TournamentGroupItem : MonoBehaviour
	{
		[SerializeField]
		protected Text _rank;
	
		[SerializeField]
		protected Text _team;
	
		[SerializeField]
		protected Text _played;
	
		[SerializeField]
		protected Text _won;
	
		[SerializeField]
		protected Text _draw;
	
		[SerializeField]
		protected Text _lost;
	
		[SerializeField]
		protected Text _goalDifference;
	
		[SerializeField]
		protected Text _points;
	
		[SerializeField]
		protected GameObject _bgEvenRow;
	
		[SerializeField]
		protected GameObject _bgOddRow;

		/// <summary>
		/// Sets the info.
		/// </summary>
		public virtual void SetInfo(TournamentTeamStats stats, int index)
		{
			if (stats == null)
			{
				return;
			}

			var rank = index + 1;
			if (_rank != null)
			{
				_rank.text = rank.ToString();
			}

			if (_team != null)
			{
				var teamName = stats.TeamName;
				if (string.IsNullOrEmpty(teamName))
				{
					teamName = stats.TeamId;
				}

				if (stats.HumanIndex != -1)
				{
					_team.text = $"(P{stats.HumanIndex + 1}) {teamName}";
				}
				else
				{
					_team.text = teamName;
				}
			}

			if (_played != null)
			{
				_played.text = stats.MatchesPlayed.ToString();
			}

			if (_won != null)
			{
				_won.text = stats.MatchesWon.ToString();
			}

			if (_draw != null)
			{
				_draw.text = stats.MatchesDrawn.ToString();
			}

			if (_lost != null)
			{
				_lost.text = stats.MatchesLost.ToString();
			}

			if (_goalDifference != null)
			{
				_goalDifference.text = stats.GoalDifference.ToString();
			}

			if (_points != null)
			{
				_points.text = stats.Points.ToString();
			}

			var row = rank - 1;
			var visible = (row % 2) == 0;
			if (_bgEvenRow != null && _bgEvenRow.activeSelf != visible)
			{
				_bgEvenRow.SetActive(visible);
			}

			visible = !visible;
			if (_bgOddRow != null && _bgOddRow.activeSelf != visible)
			{
				_bgOddRow.SetActive(visible);
			}
		}
	}
}
