using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimSoc;
using UnityEngine.Assertions;


/// <summary>
/// Tournament matches screen.
/// </summary>
public class SsTournamentMatchesScreen : SsBaseMenu
{
	// String format for displaying the match day.
	private const string MatchDayFormat = "MATCHDAY {0} OF {1}";
	
	// Public
	//-------
	[Header("Prefabs")]
	public SsTournamentMatchItem matchItemPrefab;
	public TournamentMatchHeaderItem matchHeaderItemPrefab;
	
	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;
	public GameObject content;
	public GameObject highlight;



	// Private
	//--------
	private List<SsTournamentMatchItem> items;			// Items in the scroll list
	private List<TournamentMatchHeaderItem> itemHeaders = new List<TournamentMatchHeaderItem>();
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

		Assert.IsNotNull(scrollView);
		Assert.IsNotNull(matchItemPrefab);
		Assert.IsNotNull(content);
	}

	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	/// <param name="buildList">Build list.</param>
	public void UpdateControls(bool buildList)
	{
		if ((buildList) && (scrollView != null) && (matchItemPrefab != null) && (content != null))
		{
			SsTournamentMatchItem item;
			int i, n;
			SsTournamentMatchInfo matchInfo;
			SsTournamentMatchInfo[] matchInfos = SsTournament.tournamentMatchInfos;
			bool didSetHighlight;

			didSetHighlight = false;

			i = 0;
			string headerText = null;
			var groupMatchDay = -1;
			var headerCount = 0;
			if ((matchInfos != null) && (matchInfos.Length > 0))
			{
				if (items == null)
				{
					items = new List<SsTournamentMatchItem>(matchInfos.Length);
				}
				
				n = matchInfos.Length;
				for (i = 0; i < n; i++)
				{
					matchInfo = matchInfos[i];
					if (matchInfo != null)
					{
						var needHeader = !string.IsNullOrEmpty(matchInfo.displayHeading) &&
						                 headerText != matchInfo.displayHeading;
						if (needHeader)
						{
							headerText = matchInfo.displayHeading;
							AddHeaderItem(headerText, ref headerCount);
						}

						var needMatchDay = matchInfo.matchDay >= 0 && groupMatchDay != matchInfo.matchDay &&
						                   SsTournament.GroupStageMatchDays > 0;
						if (needMatchDay)
						{
							groupMatchDay = matchInfo.matchDay;
							AddHeaderItem(
								string.Format(MatchDayFormat, groupMatchDay + 1, SsTournament.GroupStageMatchDays + 1),
								ref headerCount);
						}
						
						// Not enough items in the list?
						if (i >= items.Count)
						{
							item = Instantiate(matchItemPrefab, content.transform, false);
							items.Add(item);
						}
						else
						{
							item = items[i];
						}

						if (item != null)
						{
							item.SetInfo(matchInfo, i);

							// Highlight the current match
							if ((highlight != null) && (i == SsTournament.tournamentMatch))
							{
								didSetHighlight = true;
								highlight.transform.SetParent(item.transform, false);
								if (highlight.activeInHierarchy == false)
								{
									highlight.SetActive(true);
								}
							}
							
							item.transform.SetAsLastSibling();
							if (item.gameObject.activeInHierarchy == false)
							{
								item.gameObject.SetActive(true);
							}
						}
					}
				}
			}


			if ((highlight != null) && (didSetHighlight == false) && (highlight.activeInHierarchy))
			{
				// Hide the highlight
				highlight.SetActive(false);
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

			if (itemHeaders != null && itemHeaders.Count > 0 && headerCount < itemHeaders.Count)
			{
				n = headerCount;
				for (i = n; i < itemHeaders.Count; i++)
				{
					var headerItem = itemHeaders[i];
					if (headerItem != null && headerItem.gameObject.activeSelf)
					{
						headerItem.gameObject.SetActive(false);
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
	
	private void AddHeaderItem(string text, ref int headerCount)
	{
		if (matchHeaderItemPrefab == null)
		{
			return;
		}
		
		TournamentMatchHeaderItem headerItem;
		if (headerCount >= itemHeaders.Count)
		{
			headerItem = Instantiate(matchHeaderItemPrefab, content.transform, false);
			itemHeaders.Add(headerItem);
		}
		else
		{
			headerItem = itemHeaders[headerCount];
		}
		headerCount++;
		headerItem.SetHeader(text);
		headerItem.transform.SetAsLastSibling();
		if (!headerItem.gameObject.activeSelf)
		{
			headerItem.gameObject.SetActive(true);	
		}
	}
}
