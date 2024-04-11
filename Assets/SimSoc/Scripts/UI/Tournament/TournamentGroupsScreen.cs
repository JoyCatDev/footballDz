using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace SimSoc
{
	/// <summary>
	/// World cup groups screen.
	/// </summary>
	public class TournamentGroupsScreen : SsBaseMenu
	{
		[Header("Prefabs")]
		[SerializeField]
		protected TournamentGroupItem _groupItemPrefab;

		[SerializeField]
		protected TournamentGroupHeaderItem _headerItemPrefab;

		[Header("Elements")]
		[SerializeField]
		protected ScrollRect _scrollView;
	
		[SerializeField]
		protected GameObject _content;
	
		protected readonly List<TournamentGroupItem> _items = new List<TournamentGroupItem>();	// Items in the scroll list
		protected bool _didShowOnce;
		protected int _tournamentCount = -1;					// Keep track if new tournaments have started.
		protected int _tournamentMatch = -1;					// Keep track if tournaments matches have changed.
		protected bool _didCheckFinalMatch;					// Keep track if checked final match
	
		protected IWorldCupTeamStatsController _statsController;
		protected readonly List<TournamentGroupHeaderItem> _headerItems = new List<TournamentGroupHeaderItem>();
	
		// List to re-use.
		protected readonly List<TournamentTeamStats> _groupStats = new List<TournamentTeamStats>();

		/// <summary>
		/// Show the menu, and play the in animation (if snap = false).
		/// NOTE: Derived methods must call the base method.
		/// </summary>
		/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
		/// <param name="snap">Snap to end position.</param>
		public override void Show(fromDirections fromDirection, bool snap)
		{
			base.Show(fromDirection, snap);
		
			var settings = SsMatchSettings.Instance != null
				? SsMatchSettings.Instance.GetTournamentSettings(SsTournament.tournamentId)
				: null;
			var customSettings = settings != null ? settings.CustomSettings : null;
			var worldCup = customSettings != null ? customSettings.CustomController as IWorldCupTournament : null;
			_statsController = worldCup != null ? worldCup.StatsController : null;

			UpdateControls(!_didShowOnce || _tournamentCount != SsTournament.TournamentCount ||
			               _tournamentMatch != SsTournament.tournamentMatch ||
			               (!_didCheckFinalMatch && SsTournament.IsFinalMatch));

			_didShowOnce = true;
			_tournamentCount = SsTournament.TournamentCount;
			_tournamentMatch = SsTournament.tournamentMatch;
			if (!_didCheckFinalMatch && SsTournament.IsFinalMatch)
			{
				_didCheckFinalMatch = true;
			}
		
			Assert.IsNotNull(_scrollView);
			Assert.IsNotNull(_groupItemPrefab);
			Assert.IsNotNull(_content);
		}
	
		/// <summary>
		/// Updates the controls.
		/// </summary>
		/// <param name="buildList">Build list.</param>
		public virtual void UpdateControls(bool buildList)
		{
			if (_statsController == null || _scrollView == null || _groupItemPrefab == null || _content == null || 
			    !buildList)
			{
				return;
			}

			BuildList();
		
			// Reset scroll view position, after all content has been added
			if (_scrollView != null)
			{
				// Vertical scroll view is inverted
				_scrollView.normalizedPosition = new Vector2(0.0f, 1.0f);
			}
		}

		protected virtual void BuildList()
		{
			var tournamentManager = (ITournamentManager)SsTournament.Instance;
			if (tournamentManager == null || tournamentManager.GroupsInfo == null || 
			    tournamentManager.GroupsInfo.Count <= 0)
			{
				return;
			}

			var visibleItems = 0;
			var visibleHeaders = 0;
			var lastGroupIndex = -1;
			var maxGroups = tournamentManager.GroupsInfo.Count;
			for (var g = 0; g < maxGroups; g++)
			{
				_groupStats.Clear();
				_statsController.GetGroupStats(g, _groupStats);
				if (_groupStats.Count <= 0)
				{
					continue;
				}
			
				var teamStats = _groupStats;
				for (int i = 0, len = teamStats.Count; i < len; i++)
				{
					var stats = teamStats[i];
					if (stats == null)
					{
						continue;
					}
				
					// Display group heading
					var needHeader = lastGroupIndex != stats.GroupIndex;
					if (needHeader)
					{
						lastGroupIndex = stats.GroupIndex;
						AddHeaderItem(GetGroupName(stats.GroupIndex), ref visibleHeaders);
					}

					TournamentGroupItem item;
				
					// Not enough items in the list?
					if (visibleItems >= _items.Count)
					{
						item = Instantiate(_groupItemPrefab, _content.transform, false);
						_items.Add(item);
					}
					else
					{
						item = _items[visibleItems];
					}

					if (item == null)
					{
						continue;
					}

					item.SetInfo(stats, i);

					visibleItems++;
					item.transform.SetAsLastSibling();
					if (!item.gameObject.activeSelf)
					{
						item.gameObject.SetActive(true);
					}
				}
			}
		
			// Hide the remaining items
			if (_items.Count > 0 && visibleItems < _items.Count)
			{
				for (int i = visibleItems, len = _items.Count; i < len; i++)
				{
					var item = _items[i];
					if (item != null && item.gameObject.activeSelf)
					{
						item.gameObject.SetActive(false);
					}
				}
			}
		
			if (_headerItems.Count > 0 && visibleHeaders < _headerItems.Count)
			{
				for (int i = visibleHeaders, len = _headerItems.Count; i < len; i++)
				{
					var item = _headerItems[i];
					if (item != null && item.gameObject.activeSelf)
					{
						item.gameObject.SetActive(false);
					}
				}
			}
		}
	
		protected virtual void AddHeaderItem(string text, ref int headerCount)
		{
			if (_headerItemPrefab == null)
			{
				return;
			}
		
			TournamentGroupHeaderItem headerItem;
			if (headerCount >= _headerItems.Count)
			{
				headerItem = Instantiate(_headerItemPrefab, _content.transform, false);
				_headerItems.Add(headerItem);
			}
			else
			{
				headerItem = _headerItems[headerCount];
			}
			headerCount++;
			headerItem.SetHeader(text);
			headerItem.transform.SetAsLastSibling();
			if (!headerItem.gameObject.activeSelf)
			{
				headerItem.gameObject.SetActive(true);	
			}
		}
	
		protected virtual string GetGroupName(int groupIndex)
		{
			var tournamentManager = (ITournamentManager)SsTournament.Instance;
			var info = tournamentManager != null && tournamentManager.GroupsInfo != null && groupIndex >= 0 &&
			           groupIndex < tournamentManager.GroupsInfo.Count
				? tournamentManager.GroupsInfo[groupIndex]
				: null;
			return info != null ? info.DisplayName : null;
		}
	}
}
