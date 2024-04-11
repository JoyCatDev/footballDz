using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Tournament log screen.
/// </summary>
public class SsTournamentLog : SsBaseMenu {

	// Public
	//-------
	[Header("Prefabs")]
	public SsTournamentLogItem logItemPrefab;

	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;
	public GameObject content;



	// Private
	//--------
	private List<SsTournamentLogItem> items;			// Items in the scrol list
	private bool didShowOnce;
	private int tournamentCount = -1;					// Keep track if new tournaments have started.
	private int tournamentMatch = -1;					// Keep track if tournaments matches have changed.
	private bool didCheckFinalMatch;					// Keep track if checked final match



	// Methods
	//--------

	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		base.Show(fromDirection, snap);

		UpdateControls((didShowOnce == false) || 
		               (tournamentCount != SsTournament.TournamentCount) || 
		               (tournamentMatch != SsTournament.tournamentMatch) || 
		               ((didCheckFinalMatch == false) && (SsTournament.IsFinalMatch)));

		didShowOnce = true;
		tournamentCount = SsTournament.TournamentCount;
		tournamentMatch = SsTournament.tournamentMatch;
		if ((didCheckFinalMatch == false) && (SsTournament.IsFinalMatch))
		{
			didCheckFinalMatch = true;
		}


#if UNITY_EDITOR
		if ((scrollView == null) || (logItemPrefab == null) || (content == null))
		{
			Debug.LogError("Log screen one of the following refs is null: Scroll View, Log Item Prefab, Content.");
		}
#endif //UNITY_EDITOR
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	/// <param name="buildList">Build list.</param>
	public void UpdateControls(bool buildList)
	{
		if ((buildList) && (scrollView != null) && (logItemPrefab != null) && (content != null))
		{
			SsTournamentLogItem item;
			int i, n;
			SsTeamStats stats;
			List<SsTeamStats> log = SsTournament.Log;

			i = 0;
			if ((log != null) && (log.Count > 0))
			{
				if (items == null)
				{
					items = new List<SsTournamentLogItem>(log.Count);
				}

				n = log.Count;
				for (i = 0; i < n; i++)
				{
					stats = log[i];
					if (stats != null)
					{
						// Not enough items in the list?
						if (i >= items.Count)
						{
							item = Instantiate(logItemPrefab);
							item.transform.SetParent(content.transform, false);
							items.Add(item);
						}
						else
						{
							item = items[i];
						}

						if (item != null)
						{
							item.SetInfo(stats);

							if (item.gameObject.activeInHierarchy == false)
							{
								item.gameObject.SetActive(true);
							}
						}
					}
				}
			}


			// Hide the remaining items
			if ((items != null) && (items.Count > 0) && (i < items.Count))
			{
				n = i;
				for (i = n; i < items.Count; i++)
				{
					item = items[i];
					if ((item != null) && (item.gameObject.activeInHierarchy))
					{
						item.gameObject.SetActive(false);
					}
				}
			}
		}


		// Keep this at the bottom of the method
		if (buildList)
		{
			// Reset scroll view position, after all content has been added
			if (scrollView != null)
			{
				// Vertical scroll view is inverted
				scrollView.normalizedPosition = new Vector2(0.0f, 1.0f);
			}
		}
	}
}
