using UnityEngine;

namespace SimSoc
{
	/// <summary>
	/// Header item used by the <see cref="TournamentGroupsScreen"/>.
	/// </summary>
	public class TournamentGroupHeaderItem : MonoBehaviour
	{
		[SerializeField]
		protected UnityEngine.UI.Text _header;

		public virtual void SetHeader(string text)
		{
			if (_header != null)
			{
				_header.text = text;
			}
		}
	}
}
