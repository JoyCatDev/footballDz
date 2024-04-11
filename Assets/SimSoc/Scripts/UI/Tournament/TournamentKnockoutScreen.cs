using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace SimSoc
{
	/// <summary>
	/// World cup knockout screen. It displays the rounds (e.g. Round of 16, Quarter-finals, etc.) as a tree.
	/// </summary>
	public class TournamentKnockoutScreen : SsBaseMenu
	{
		// Image pool to re-use for the tree
		protected class TreePoolImages
		{
			// Prefab to clone
			public Image prefab;
		
			// Images to re-use
			public List<Image> images;

			// Images instance IDs. Keeps track of images removed/returned to the pool. These will Not be in same
			// order as images.
			public List<int> imageInstanceIDs;
		}
	
		// Default number of pools (list capacity)
		protected const int DefaultPoolsAmount = 7;
	
		// Default number of images in each pool (list capacity)
		protected const int DefaultPoolImagesAmount = 50;
	
		[Header("Prefabs")]
		[SerializeField]
		protected Image _treeCornerRightDownPrefab;
	
		[SerializeField]
		protected Image _treeCornerRightUpPrefab;
	
		[SerializeField]
		protected Image _treeIntersectionPrefab;
	
		[SerializeField]
		protected Image _treeLineHorizontalPrefab;
	
		[SerializeField]
		protected Image _treeLineVerticalPrefab;
	
		[SerializeField]
		protected Image _treeTeamPrefab;
	
		[SerializeField]
		protected Image _treeTeamBgPrefab;

		[SerializeField]
		protected TournamentRoundHeaderItem _headerItemPrefab;

		[SerializeField]
		protected TournamentScoreItem _scoreItemPrefab;

		[Header("Elements")]
		[SerializeField]
		protected ScrollRect _scrollView;
	
		[SerializeField]
		protected RectTransform _content;
	
		[SerializeField, Tooltip("Panel to control the content size.")]
		protected RectTransform _sizePanel;

		[Header("Tree Dimensions")]
		[SerializeField, Tooltip("Min vertical space between teams in a match")]
		protected float _minVerticalSpaceTeams = 25.0f;
	
		[SerializeField, Tooltip("Min vertical space between matches")]
		protected float _minVerticalSpaceMatches = 30.0f;
	
		[SerializeField, Tooltip("Min length of the horizontal line")]
		protected float _minHorizontalLineSize = 16.0f;

		[SerializeField, Tooltip("Inner space between panel edge and tree elements.")]
		protected float _innerBorder = 50.0f;

		[Header("Misc")]
		[SerializeField, Tooltip("Display the round headers?")]
		protected bool _displayRoundHeaders = true;
	
		protected bool _didShowOnce;
		protected int _tournamentCount = -1;				// Keep track if new tournaments have started.
		protected int _tournamentMatch = -1;				// Keep track if tournaments matches have changed.
		protected bool _didCheckFinalMatch;					// Keep track if checked final match

		// Pools of images to re-use
		protected readonly List<TreePoolImages> _pools = new List<TreePoolImages>(DefaultPoolsAmount);

		protected RectTransform _scrollViewRectTransform;
		protected LayoutElement _sizePanelLayout;
		protected readonly List<TournamentRoundHeaderItem> _headerItems = new List<TournamentRoundHeaderItem>();
		protected readonly List<TournamentScoreItem> _scoreItems = new List<TournamentScoreItem>();
		protected ITournamentManager _tournamentManager;

		/// <summary>
		/// Awake this instance.
		/// NOTE: Derived methods must call the base method.
		/// </summary>
		public override void Awake()
		{
			base.Awake();

			if (_scrollView != null)
			{
				_scrollViewRectTransform = _scrollView.gameObject.GetComponent<RectTransform>();
			}
			if (_sizePanel != null)
			{
				_sizePanelLayout = _sizePanel.gameObject.GetComponent<LayoutElement>();
			}
		}
	
		/// <summary>
		/// Show the menu, and play the in animation (if snap = false).
		/// NOTE: Derived methods must call the base method.
		/// </summary>
		/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
		/// <param name="snap">Snap to end position.</param>
		public override void Show(fromDirections fromDirection, bool snap)
		{
			base.Show(fromDirection, snap);

			_tournamentManager = SsTournament.Instance;
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
			Assert.IsNotNull(_content);
		}
	
		/// <summary>
		/// Updates the controls.
		/// </summary>
		/// <param name="buildTree">Build tree.</param>
		public virtual void UpdateControls(bool buildTree)
		{
			if (buildTree && _scrollView != null && _content != null && _scrollViewRectTransform != null && 
			    _sizePanel != null && SsMatchSettings.Instance != null && SsTournament.tournamentMatchInfos != null && 
			    SsTournament.tournamentMatchInfos.Length > 0)
			{
				var settings = SsMatchSettings.Instance.GetTournamentSettings(SsTournament.tournamentId);
				if (settings != null)
				{
					BuildTree(settings);
				}
			}

			// Reset scroll view position, after all content has been added
			if (buildTree && _scrollView != null)
			{
				// Vertical scroll view is inverted
				_scrollView.normalizedPosition = new Vector2(0.0f, 1.0f);
			}
		}

		/// <summary>
		/// Builds the tournament tree.
		/// </summary>
		/// <param name="settings">Settings.</param>
		protected virtual void BuildTree(SsTournamentSettings settings)
		{
			var imageHolder = _sizePanel;
		
			// Return all previous images to the pools
			var images = imageHolder.gameObject.GetComponentsInChildren<Image>();
			if (images != null && images.Length > 0)
			{
				for (int i = 0, len = images.Length; i < len; i++)
				{
					ReturnImageToPool(images[i]);
				}
				images = null;
			}

			var worldCupSettings = (WorldCupTournamentSettings)settings.CustomSettings;
			var maxMatches = worldCupSettings.NumKnockoutMatches;
			var maxRounds = worldCupSettings.NumKnockoutRounds;
			var matchesInFirstRound = worldCupSettings.GetNumberOfMatchesInKnockoutRound(0);

			// Create images (if they do Not exist in the pools, or add extra if there is Not enough)
			var max = maxMatches * 2 + 2;
			CreatePool(_treeTeamBgPrefab, max, imageHolder);
			CreatePool(_treeTeamPrefab, max, imageHolder);
		
			max = maxMatches * 3;
			CreatePool(_treeLineHorizontalPrefab, max, imageHolder);
			max = maxMatches * 2;
			CreatePool(_treeLineVerticalPrefab, max, imageHolder);
		
			max = maxMatches;
			CreatePool(_treeCornerRightDownPrefab, max, imageHolder);
			CreatePool(_treeCornerRightUpPrefab, max, imageHolder);
			CreatePool(_treeIntersectionPrefab, max, imageHolder);

			var maxVerticalMatches = matchesInFirstRound;
			var maxVerticalTeams = maxVerticalMatches * 2;
		
			// Max width of the vertical connection elements
			var verticalConnectionWidth = Mathf.Max(_treeLineVerticalPrefab.rectTransform.rect.width, 
				Mathf.Max(_treeIntersectionPrefab.rectTransform.rect.width, 
					Mathf.Max(_treeCornerRightDownPrefab.rectTransform.rect.width, 
						_treeCornerRightUpPrefab.rectTransform.rect.width)));

			// The safe area in which the tree can be positioned
			var scrollViewRect = _scrollViewRectTransform.rect;
			var safeArea = new Rect(scrollViewRect.min.x + _innerBorder, scrollViewRect.min.y + _innerBorder,
				(scrollViewRect.max.x - _innerBorder) - (scrollViewRect.min.x + _innerBorder),
				(scrollViewRect.max.y - _innerBorder) - (scrollViewRect.min.y + _innerBorder));
		
			// Check if minVerticalSpaceTeams is at least the size of tree_line_horizontal.size.y and
			// tree_intersection.size.y. If not then increase it to the largest of the two.
			var minVerticalSpaceTeams = _minVerticalSpaceTeams;
			if (minVerticalSpaceTeams < _treeLineHorizontalPrefab.rectTransform.rect.size.y)
			{
				minVerticalSpaceTeams = _treeLineHorizontalPrefab.rectTransform.rect.size.y;
			}
			if (minVerticalSpaceTeams < _treeIntersectionPrefab.rectTransform.rect.size.y)
			{
				minVerticalSpaceTeams = _treeIntersectionPrefab.rectTransform.rect.size.y;
			}
		
			// Check if minVerticalSpaceMatches is at least the size of tree_line_horizontal.size.y and
			// tree_intersection.size.y. If not then increase it to the largest of the two.
			var minVerticalSpaceMatches = _minVerticalSpaceMatches;
			if (minVerticalSpaceMatches < _treeLineHorizontalPrefab.rectTransform.rect.size.y)
			{
				minVerticalSpaceMatches = _treeLineHorizontalPrefab.rectTransform.rect.size.y;
			}
			if (minVerticalSpaceMatches < _treeIntersectionPrefab.rectTransform.rect.size.y)
			{
				minVerticalSpaceMatches = _treeIntersectionPrefab.rectTransform.rect.size.y;
			}

			var treeTeamBgRect = _treeTeamBgPrefab.rectTransform.rect;
		
			// Calculate min horizontal content size
			Vector2 totalTeamsSize;
			Vector2 minSize;
			totalTeamsSize.x = (maxRounds + 1) * treeTeamBgRect.size.x;
			var totalHorizontalLineSize = maxRounds * (_minHorizontalLineSize * 2.0f);
			var totalVerticalLinesIntersectionsSize = maxRounds * verticalConnectionWidth;
			minSize.x = totalTeamsSize.x + totalHorizontalLineSize + totalVerticalLinesIntersectionsSize;
		
			// Calculate min vertical content size
			totalTeamsSize.y = maxVerticalTeams * treeTeamBgRect.size.y;
			var totalSpacesBetweenTeams = maxVerticalMatches * minVerticalSpaceTeams;
			var totalSpaceBetweenMatches = (maxVerticalMatches - 1) * minVerticalSpaceMatches;
			minSize.y = totalTeamsSize.y + totalSpacesBetweenTeams + totalSpaceBetweenMatches;
		
			// If tree fits in smaller area than the safe area, then scale it up to fill the safe area.
			// Note: The scale is only applied to spaces and lines.
			var scale = Vector2.one;
			float tempSize;
			if (minSize.x < safeArea.width)
			{
				// Scale excludes elements that do Not stretch/scale.
				tempSize = totalTeamsSize.x + totalVerticalLinesIntersectionsSize;
				scale.x = (safeArea.width - tempSize) / (minSize.x - tempSize);
			}
			if (minSize.y < safeArea.height)
			{
				// Scale excludes elements that do Not stretch/scale.
				tempSize = totalTeamsSize.y;
				scale.y = (safeArea.height - tempSize) / (minSize.y - tempSize);
			}
		
			var intersectionSize = Vector2.zero;
			var cornerRightDownSize = Vector2.zero;
			var cornerRightUpSize = Vector2.zero;
			var intersectionPos = Vector2.zero;
			var cornerRightDownPos = Vector2.zero;
			var cornerRightUpPos = Vector2.zero;
		
			// Position the images
			safeArea = new Rect(_innerBorder, -safeArea.height - _innerBorder, safeArea.width, safeArea.height);
			var startPos = new Vector2(safeArea.min.x, safeArea.max.y);
			var contentTopLeft = startPos;
			var contentBottomRight = new Vector2(safeArea.max.x, safeArea.min.y);
		
			var pos = startPos;
			var headerPos = startPos;
			var verticalSpaceTeams = (minVerticalSpaceTeams * scale.y);
			var verticalSpaceMatches = (minVerticalSpaceMatches * scale.y);
		
			var thirdPlaceVerticalSpaceTeams = verticalSpaceTeams;
			var thirdPlaceVerticalSpaceMatches = verticalSpaceMatches;
			var thirdPlaceMatchPos = pos;
		
			const float headerSpacing = 20f;
			var visibleHeaders = 0;
			var visibleScores = 0;
			var headerRectTransform = _headerItemPrefab != null ? (RectTransform) _headerItemPrefab.transform : null;
			var headerPrefabSize = headerRectTransform != null
				? new Vector2(headerRectTransform.rect.width, headerRectTransform.rect.height)
				: Vector2.zero;
			var headerSize = new Vector2(headerPrefabSize.x + headerSpacing, headerPrefabSize.y + headerSpacing);
		
			// Loop through the rounds
			for (var r = 0; r < maxRounds; r++)
			{
				max = worldCupSettings.GetNumberOfMatchesInKnockoutRound(r);

				// Next round X position
				var nextRoundPos = Vector2.zero;
				var tempStartX = pos.x;
				nextRoundPos.x = pos.x;
				nextRoundPos.x += treeTeamBgRect.width;
				nextRoundPos.x += (_minHorizontalLineSize * 2.0f * scale.x);
				nextRoundPos.x += verticalConnectionWidth;

				var saveStartPos = startPos;
				var savePos = pos;
				var saveVerticalSpaceTeams = verticalSpaceTeams;
				var saveVerticalSpaceMatches = verticalSpaceMatches;
				var isThirdPlacePlayOffRound = worldCupSettings.IsThirdPlacePlayOffRound(r);
				if (isThirdPlacePlayOffRound)
				{
					// Position third place play-off below the first round
					startPos = thirdPlaceMatchPos;
					pos = thirdPlaceMatchPos;
					verticalSpaceTeams = thirdPlaceVerticalSpaceTeams;
					verticalSpaceMatches = thirdPlaceVerticalSpaceMatches;
				
					tempStartX = pos.x;
					nextRoundPos.x = pos.x;
					nextRoundPos.x += treeTeamBgRect.width;
					nextRoundPos.x += (_minHorizontalLineSize * 2.0f * scale.x);
					nextRoundPos.x += verticalConnectionWidth;
				}

				if (_displayRoundHeaders)
				{
					// Make sure the rounds' headers do not overlap
					var tempWidth = nextRoundPos.x - tempStartX;
					if (tempWidth < headerSize.x)
					{
						nextRoundPos.x += headerSize.x - tempWidth;
					}
				
					var tempPos = new Vector2(pos.x, isThirdPlacePlayOffRound ? pos.y : headerPos.y);
					AddHeaderItem(worldCupSettings.GetStateHeadingFromRound(r), imageHolder, tempPos,
						ref visibleHeaders);
					pos.y -= headerSize.y;
				
					// Increase content size if the header overlaps it.
					if (contentBottomRight.x < tempPos.x + headerSize.x)
					{
						contentBottomRight.x = tempPos.x + headerSize.x;
					}
					if (contentBottomRight.y > tempPos.y - headerSize.y)
					{
						contentBottomRight.y = tempPos.y - headerSize.y;
					}
				}

				// Midway to next round (X)
				Vector2 midway;
				midway.x = pos.x + treeTeamBgRect.width;
				midway.x = midway.x + ((nextRoundPos.x - midway.x) / 2.0f);
			
				// Loop through the matches
				float topY;
				float bottomY;
				for (var m = 0; m < max; m++)
				{
					var matchInfo = GetKnockoutMatch(r, m);
				
					// Don't show the match if it hasn't been setup
					if (matchInfo == null || 
					    (string.IsNullOrEmpty(matchInfo.teamId[0]) && string.IsNullOrEmpty(matchInfo.teamId[1])))
					{
						continue;
					}

					// Next match Y position
					Vector2 nextMatchPos;
					nextMatchPos.y = pos.y - treeTeamBgRect.height;
					nextMatchPos.y -= verticalSpaceTeams;
					nextMatchPos.y -= treeTeamBgRect.height;
					nextMatchPos.y -= verticalSpaceMatches;
				
					// Midway to next team (Y)
					midway.y = pos.y - treeTeamBgRect.height;
					midway.y -= (verticalSpaceTeams / 2.0f);
				
					// Team 1
					var teamId = matchInfo != null ? matchInfo.teamId[0] : null;
					var teamScore = matchInfo != null && matchInfo.MatchDone ? matchInfo.teamScore[0] : (int?)null;
					var imageBg = DrawTeam(teamId, pos, imageHolder, out _, teamScore, ref visibleScores);
					
					// Connection elements
					if (imageBg != null)
					{
						// Intersection cross |-
						var image = GetImageFromPool(_treeIntersectionPrefab, imageHolder, out var imageRectTransform);
						if (image != null)
						{
							intersectionSize = new Vector2(imageRectTransform.rect.width,
								imageRectTransform.rect.height);
							intersectionPos = new Vector2(midway.x, midway.y);
							imageRectTransform.localPosition = new Vector3(intersectionPos.x, 
								intersectionPos.y, 
								imageRectTransform.localPosition.z);
						}

						// Corner at the top, flowing right-down
						image = GetImageFromPool(_treeCornerRightDownPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							cornerRightDownSize = new Vector2(imageRectTransform.rect.width,
								imageRectTransform.rect.height);
							cornerRightDownPos = new Vector2(midway.x - (imageRectTransform.rect.width / 2.0f),
								pos.y - (treeTeamBgRect.height / 2.0f) - 
								(imageRectTransform.rect.height / 4.0f));
							imageRectTransform.localPosition = new Vector3(cornerRightDownPos.x, 
								cornerRightDownPos.y, 
								imageRectTransform.localPosition.z);
						}

						// Corner at the bottom, flowing right-up
						image = GetImageFromPool(_treeCornerRightUpPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							cornerRightUpSize = new Vector2(imageRectTransform.rect.width,
								imageRectTransform.rect.height);
							cornerRightUpPos = new Vector2(midway.x - (imageRectTransform.rect.width / 2.0f),
								pos.y - treeTeamBgRect.height - 
								verticalSpaceTeams - 
								(treeTeamBgRect.height / 2.0f) + 
								(imageRectTransform.rect.height / 4.0f));
							imageRectTransform.localPosition = new Vector3(cornerRightUpPos.x, 
								cornerRightUpPos.y, 
								imageRectTransform.localPosition.z);
						}

						// Top vertical line
						image = GetImageFromPool(_treeLineVerticalPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							topY = cornerRightDownPos.y - (cornerRightDownSize.y / 2.0f);
							bottomY = intersectionPos.y + (intersectionSize.y / 2.0f);
							tempSize = topY - bottomY;
						
							imageRectTransform.sizeDelta = new Vector2(imageRectTransform.sizeDelta.x, tempSize);
							imageRectTransform.localPosition = new Vector3(
								midway.x - (imageRectTransform.rect.width / 2.0f), topY - (tempSize / 2.0f),
								imageRectTransform.localPosition.z);
						}

						// Bottom vertical line
						image = GetImageFromPool(_treeLineVerticalPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							topY = intersectionPos.y - (intersectionSize.y / 2.0f);
							bottomY = cornerRightUpPos.y + (cornerRightUpSize.y / 2.0f);
							tempSize = topY - bottomY;
						
							imageRectTransform.sizeDelta = new Vector2(imageRectTransform.sizeDelta.x, tempSize);
							imageRectTransform.localPosition = new Vector3(
								midway.x - (imageRectTransform.rect.width / 2.0f), topY - (tempSize / 2.0f),
								imageRectTransform.localPosition.z);
						}
					
						// Top horizontal line
						image = GetImageFromPool(_treeLineHorizontalPrefab, imageHolder, out imageRectTransform);
						float leftX;
						float rightX;
						if (image != null)
						{
							leftX = pos.x + treeTeamBgRect.width;
							rightX = cornerRightDownPos.x - (cornerRightDownSize.x / 2.0f);
							tempSize = rightX - leftX;
						
							imageRectTransform.sizeDelta = new Vector2(tempSize, imageRectTransform.sizeDelta.y);
							imageRectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
								cornerRightDownPos.y + 
								(imageRectTransform.rect.height / 2.0f),
								imageRectTransform.localPosition.z);
						}

						// Bottom horizontal line
						image = GetImageFromPool(_treeLineHorizontalPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							leftX = pos.x + treeTeamBgRect.width;
							rightX = cornerRightUpPos.x - (cornerRightUpSize.x / 2.0f);
							tempSize = rightX - leftX;
						
							imageRectTransform.sizeDelta = new Vector2(tempSize, imageRectTransform.sizeDelta.y);
							imageRectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
								cornerRightUpPos.y - 
								(imageRectTransform.rect.height / 2.0f), 
								imageRectTransform.localPosition.z);
						}
					
						// Middle horizontal line (it connect to the next match/winner).
						image = GetImageFromPool(_treeLineHorizontalPrefab, imageHolder, out imageRectTransform);
						if (image != null)
						{
							leftX = intersectionPos.x + (intersectionSize.x / 2.0f);
							rightX = nextRoundPos.x;
							tempSize = rightX - leftX;
						
							imageRectTransform.sizeDelta = new Vector2(tempSize, imageRectTransform.sizeDelta.y);
							imageRectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
								intersectionPos.y, 
								imageRectTransform.localPosition.z);
						}
					}
				
					pos.y -= treeTeamBgRect.height;
					pos.y -= verticalSpaceTeams;
				
					// Team 2
					teamId = matchInfo != null ? matchInfo.teamId[1] : null;
					teamScore = matchInfo != null && matchInfo.MatchDone ? matchInfo.teamScore[1] : (int?)null;
					DrawTeam(teamId, pos, imageHolder, out _, teamScore, ref visibleScores);

					pos.y -= treeTeamBgRect.height;
					pos.y -= verticalSpaceMatches;
				
					// Winning team for third-place and final
					if (matchInfo != null && (isThirdPlacePlayOffRound || worldCupSettings.IsFinalRound(r)))
					{
						var oldPos = pos;

						pos.x = nextRoundPos.x;
					
						pos.y = startPos.y;
						pos.y -= treeTeamBgRect.height;
						pos.y -= (verticalSpaceTeams / 2.0f);
						pos.y += (treeTeamBgRect.height / 2.0f);
						if (_displayRoundHeaders)
						{
							pos.y -= headerSize.y;	
						}

						teamId = matchInfo.GetWinner();
						DrawTeam(teamId, pos, imageHolder, out var tempRightX, null, ref visibleScores);

						// Increase content X size if the team logo overlaps it.
						if (contentBottomRight.x < tempRightX)
						{
							contentBottomRight.x = tempRightX;
						}

						pos = oldPos;
					}

					if (r == 0)
					{
						thirdPlaceMatchPos = pos;	
					}
				}
			
				if (r == 0 && contentBottomRight.y > pos.y + verticalSpaceMatches)
				{
					contentBottomRight.y = pos.y + verticalSpaceMatches;
				}
			
				// REMINDER: scale has already been applied to verticalSpaceTeams and verticalSpaceMatches.
			
				// Restore the next round pos
				if (isThirdPlacePlayOffRound)
				{
					// Increase content Y size for third place play-off
					if (contentBottomRight.y > pos.y + verticalSpaceMatches)
					{
						contentBottomRight.y = pos.y + verticalSpaceMatches;	
					}

					startPos = saveStartPos;
					pos = savePos;
					verticalSpaceTeams = saveVerticalSpaceTeams;
					verticalSpaceMatches = saveVerticalSpaceMatches;
					continue;
				}

				// Move pos.x for next round
				pos.x = nextRoundPos.x;
			
				// Calc vertical spaces for next round
				topY = startPos.y - treeTeamBgRect.height;
				topY -= (verticalSpaceTeams / 2.0f);
				topY -= (treeTeamBgRect.height / 2.0f);
			
				bottomY = startPos.y - treeTeamBgRect.height;
				bottomY -= verticalSpaceTeams;
				bottomY -= treeTeamBgRect.height;
				bottomY -= verticalSpaceMatches;
				bottomY -= treeTeamBgRect.height;
				bottomY -= (verticalSpaceTeams / 2.0f);
				bottomY += (treeTeamBgRect.height / 2.0f);
			
				var newVerticalSpaceTeams = topY - bottomY;
			
				topY = bottomY - treeTeamBgRect.height;
			
				bottomY = topY + (treeTeamBgRect.height / 2);
				bottomY -= (verticalSpaceTeams / 2);
				bottomY -= treeTeamBgRect.height;
				bottomY -= verticalSpaceMatches;
				bottomY -= treeTeamBgRect.height;
				bottomY -= (verticalSpaceTeams / 2);
				bottomY += (treeTeamBgRect.height / 2);
			
				var newVerticalSpaceMatches = topY - bottomY;
			
				// Set startPos.y for next round
				startPos.y -= treeTeamBgRect.height;
				startPos.y -= (verticalSpaceTeams / 2.0f);
				startPos.y += (treeTeamBgRect.height / 2.0f);
				pos.y = startPos.y;
			
				// Update the vertical spaces
				if (r == 0)
				{
					thirdPlaceVerticalSpaceTeams = verticalSpaceTeams;
					thirdPlaceVerticalSpaceMatches = verticalSpaceMatches;	
				}
				verticalSpaceMatches = newVerticalSpaceMatches;
				verticalSpaceTeams = newVerticalSpaceTeams;
			}

			// Update content size
			_sizePanel.sizeDelta = new Vector2((contentBottomRight.x - contentTopLeft.x) + (_innerBorder * 2),
				(contentTopLeft.y - contentBottomRight.y) + (_innerBorder * 2));
		
			if (_sizePanelLayout != null)
			{
				_sizePanelLayout.minWidth = _sizePanel.sizeDelta.x;
				_sizePanelLayout.minHeight = _sizePanel.sizeDelta.y;
			}
		
			// Hide the remaining items
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
		
			if (_scoreItems.Count > 0 && visibleScores < _scoreItems.Count)
			{
				for (int i = visibleScores, len = _scoreItems.Count; i < len; i++)
				{
					var item = _scoreItems[i];
					if (item != null && item.gameObject.activeSelf)
					{
						item.gameObject.SetActive(false);
					}
				}
			}
		}

		/// <summary>
		/// Draws the team's block, which includes the background and team logo (if there is a team in the block).
		/// </summary>
		protected virtual Image DrawTeam(string teamId, Vector2 pos, RectTransform imageHolder, out float getRightX, 
			int? score, ref int scoreCount)
		{
			getRightX = pos.x;
		
			// Team BG holder
			var imageBg = GetImageFromPool(_treeTeamBgPrefab, imageHolder, out var imageBgRectTransform);
			if (imageBg != null)
			{
				imageBgRectTransform.localPosition = new Vector3(pos.x + (imageBgRectTransform.rect.width / 2.0f), 
					pos.y - (imageBgRectTransform.rect.height / 2.0f), 
					imageBgRectTransform.localPosition.z);
				getRightX = pos.x + imageBgRectTransform.rect.width;
			}

			if (imageBg == null)
			{
				return null;
			}

			if (score != null)
			{
				const float spaceBeforeScore = 10f;
				var tempPos = new Vector2(getRightX + spaceBeforeScore, pos.y);
				AddScoreItem(score.Value.ToString(), imageHolder, tempPos, ref scoreCount);
			}

			// Logo
			var sprite = SsMenuResources.Instance != null ? SsMenuResources.Instance.GetSmallTeamLogo(teamId) : null;
			if (sprite != null)
			{
				var image = GetImageFromPool(_treeTeamPrefab, imageHolder, out var imageRectTransform);
				imageRectTransform.localPosition = new Vector3(pos.x + (imageBgRectTransform.rect.width / 2.0f), 
					pos.y - (imageBgRectTransform.rect.height / 2.0f), 
					imageRectTransform.localPosition.z);
				image.sprite = sprite;
			}

			return imageBg;
		}
	
		protected virtual void AddHeaderItem(string text, RectTransform parent, Vector3 position, ref int headerCount)
		{
			if (_headerItemPrefab == null)
			{
				return;
			}
		
			TournamentRoundHeaderItem headerItem;
			if (headerCount >= _headerItems.Count)
			{
				headerItem = Instantiate(_headerItemPrefab, parent, false);
				_headerItems.Add(headerItem);
			}
			else
			{
				headerItem = _headerItems[headerCount];
			}
			headerItem.transform.localPosition = position;
		
			headerCount++;
			headerItem.SetHeader(text);
			headerItem.transform.SetAsLastSibling();
			if (!headerItem.gameObject.activeSelf)
			{
				headerItem.gameObject.SetActive(true);	
			}
		}
	
		protected virtual void AddScoreItem(string text, RectTransform parent, Vector3 position, ref int scoreCount)
		{
			if (_scoreItemPrefab == null)
			{
				return;
			}
		
			TournamentScoreItem scoreItem;
			if (scoreCount >= _scoreItems.Count)
			{
				scoreItem = Instantiate(_scoreItemPrefab, parent, false);
				_scoreItems.Add(scoreItem);
			}
			else
			{
				scoreItem = _scoreItems[scoreCount];
			}
			scoreItem.transform.localPosition = position;
		
			scoreCount++;
			scoreItem.SetScore(text);
			scoreItem.transform.SetAsLastSibling();
			if (!scoreItem.gameObject.activeSelf)
			{
				scoreItem.gameObject.SetActive(true);	
			}
		}

		protected virtual SsTournamentMatchInfo GetKnockoutMatch(int roundIndex, int matchIndex)
		{
			var matchesInfo = SsTournament.tournamentMatchInfos;
			if (matchesInfo == null || matchesInfo.Length <= 0)
			{
				return null;
			}

			for (int i = 0, len = matchesInfo.Length; i < len; i++)
			{
				var info = matchesInfo[i];
				if (info != null && info.knockoutRoundIndex == roundIndex && info.knockoutRoundMatchIndex == matchIndex)
				{
					return info;
				}
			}
		
			return null;
		}

		/// <summary>
		/// Creates the pool of images by cloning the specified prefab. If the pool already exists then it will add more
		/// images to the pool, if needed.
		/// </summary>
		/// <returns>The amount of images added to the pool. May be less than <paramref name="amount"/> if pool already
		/// has images.</returns>
		/// <param name="prefab">Prefab.</param>
		/// <param name="amount">Amount to clone.</param>
		/// <param name="parent">Parent to attach to.</param>
		protected virtual int CreatePool(Image prefab, int amount, RectTransform parent)
		{
			if ((_pools == null) || (prefab == null) || (amount <= 0))
			{
				return (0);
			}

			int i, amountAdded;
			TreePoolImages pool;
			Image newImage;
		
			// Does it already exist in the pool?
			pool = GetPool(prefab);
			if (pool != null)
			{
				// Are there images in the pool?
				if ((pool.images != null) && (pool.images.Count > 0))
				{
					amount -= pool.images.Count;
					if (amount <= 0)
					{
						// There is enough images in the pool
						return (0);
					}
				}
			}

			if (pool == null)
			{
				pool = new TreePoolImages();
				_pools.Add(pool);
			}

			pool.prefab = prefab;
			if (pool.images == null)
			{
				pool.images = new List<Image>(Mathf.Max(amount, DefaultPoolImagesAmount));
				pool.imageInstanceIDs = new List<int>(Mathf.Max(amount, DefaultPoolImagesAmount));
			}

			amountAdded = 0;
			for (i = 0; i < amount; i++)
			{
				newImage = Instantiate(prefab);
				if (newImage != null)
				{
					if (parent != null)
					{
						newImage.transform.SetParent(parent, false);
					}

					newImage.gameObject.SetActive(false);
					pool.images.Add(newImage);
					pool.imageInstanceIDs.Add(newImage.GetInstanceID());
					amountAdded ++;
				}
			}

			return (amountAdded);
		}
	
		/// <summary>
		/// Gets the pool for the specified prefab.
		/// </summary>
		/// <returns>The pool.</returns>
		/// <param name="prefab">Prefab.</param>
		protected virtual TreePoolImages GetPool(Image prefab)
		{
			if ((_pools == null) || (_pools.Count <= 0) || (prefab == null))
			{
				return (null);
			}

			int i;
			TreePoolImages pool;

			for (i = 0; i < _pools.Count; i++)
			{
				pool = _pools[i];
				if ((pool != null) && (pool.prefab == prefab))
				{
					return (pool);
				}
			}

			return (null);
		}
	
		/// <summary>
		/// Gets the pool for the specified instance ID.
		/// </summary>
		/// <returns>The pool.</returns>
		/// <param name="imageInstanceID">Image instance ID.</param>
		protected virtual TreePoolImages GetPool(int imageInstanceID)
		{
			if ((_pools == null) || (_pools.Count <= 0))
			{
				return (null);
			}
		
			int i, n;
			TreePoolImages pool;
		
			for (i = 0; i < _pools.Count; i++)
			{
				pool = _pools[i];
				if ((pool != null) && (pool.imageInstanceIDs != null))
				{
					for (n = 0; n < pool.imageInstanceIDs.Count; n++)
					{
						if (pool.imageInstanceIDs[n] == imageInstanceID)
						{
							return (pool);
						}
					}
				}
			}
		
			return (null);
		}
	
		/// <summary>
		/// Gets an image from the pool, that was cloned from the specified prefab.
		/// </summary>
		/// <param name="prefab">Prefab to identify which pool to use.</param>
		/// <param name="parent">Parent to attach to.</param>
		/// <param name="imageRectTransform">Returns the image's RectTransform.</param>
		/// <param name="setAsLastSibling">Move the image to the bottom?</param>
		protected virtual Image GetImageFromPool(Image prefab, RectTransform parent, 
			out RectTransform imageRectTransform, bool setAsLastSibling = true)
		{
			var pool = GetPool(prefab);

			if (pool != null && pool.images != null && pool.images.Count > 0)
			{
				for (int i = 0, len = pool.images.Count; i < len; i++)
				{
					var image = pool.images[i];
					if (image == null)
					{
						continue;
					}
				
					// Clear the slot
					pool.images[i] = null;
				
					image.gameObject.SetActive(true);
					if (image.transform.parent != parent)
					{
						image.transform.SetParent(parent, false);
					}
					if (setAsLastSibling)
					{
						image.transform.SetAsLastSibling();
					}

					imageRectTransform = image.rectTransform;
					return image;
				}
			}

			imageRectTransform = null;
			return null;
		}
	
		/// <summary>
		/// Returns the image to the pool.
		/// </summary>
		/// <param name="imageToReturn">Image to return to the pool.</param>
		protected virtual void ReturnImageToPool(Image imageToReturn)
		{
			if (imageToReturn == null)
			{
				return;
			}

			TreePoolImages pool = GetPool(imageToReturn.GetInstanceID());
			int i;

			if ((pool != null) && (pool.images != null) && (pool.images.Count > 0))
			{
				for (i = 0; i < pool.images.Count; i++)
				{
					if (pool.images[i] == null)
					{
						pool.images[i] = imageToReturn;	// Add to slot
						imageToReturn.gameObject.SetActive(false);
						return;
					}
				}
			}
		}
	}
}
