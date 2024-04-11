﻿using UnityEngine;

namespace SimSoc
{
	/// <summary>
	/// Header item used by the <see cref="SsTournamentMatchesScreen"/>.
	/// </summary>
	public class TournamentMatchHeaderItem : MonoBehaviour
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