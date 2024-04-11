using UnityEngine;

namespace SimSoc
{
	/// <summary>
	/// Score item used by the <see cref="TournamentKnockoutScreen"/>.
	/// </summary>
	public class TournamentScoreItem : MonoBehaviour
	{
		[SerializeField]
		protected UnityEngine.UI.Text _score;

		public virtual void SetScore(string text)
		{
			if (_score != null)
			{
				_score.text = text;
			}
		}
	}
}
