using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Tournament tree screen.
/// </summary>
public class SsTournamentTreeScreen : SsBaseMenu {

	// Const/Static
	//-------------
	private const int defaultPoolsAmount = 7;			// Default number of pools (list capacity)
	private const int defaultPoolImagesAmount = 50;		// Default number of images in each pool (list capacity)


	// Classes
	//--------
	// Image pool to re-use for the tree
	private class SsTreePoolImages
	{
		public UnityEngine.UI.Image prefab;				// Prefab to clone
		public List<UnityEngine.UI.Image> images;		// Images to re-use

		// Images instance IDs. Keeps track of images removed/returned to the pool. These will Not be in same order as images.
		public List<int> imageInstanceIDs;
	}


	// Public
	//-------
	[Header("Prefabs")]
	public UnityEngine.UI.Image treeCornerRightDownPrefab;
	public UnityEngine.UI.Image treeCornerRightUpPrefab;
	public UnityEngine.UI.Image treeIntersectionPrefab;
	public UnityEngine.UI.Image treeLineHorizontalPrefab;
	public UnityEngine.UI.Image treeLineVerticalPrefab;
	public UnityEngine.UI.Image treeTeamPrefab;
	public UnityEngine.UI.Image treeTeamBgPrefab;

	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;
	public RectTransform content;
	[Tooltip("Panel to control the content size.")]
	public RectTransform sizePanel;

	[Header("Tree Dimensions")]
	[Tooltip("Min vertical space between teams in a match")]
	public float minVerticalSpaceTeams = 25.0f;
	[Tooltip("Min vertical space between matches")]
	public float minVerticalSpaceMatches = 30.0f;
	[Tooltip("Min length of the horizontal line")]
	public float minHorizontalLineSize = 16.0f;
	[Tooltip("Min length of the vertical line")]
	public float minVerticalLineSize = 16.0f;
	[Tooltip("Inner space between panel edge and tree elements.")]
	public float innerBorder = 50.0f;


	// Private
	//--------
	private bool didShowOnce;
	private int tournamentCount = -1;					// Keep track if new tournaments have started.
	private int tournamentMatch = -1;					// Keep track if tournaments matches have changed.
	private bool didCheckFinalMatch;					// Keep track if checked final match

	private List<SsTreePoolImages> pools = new List<SsTreePoolImages>(defaultPoolsAmount);		// Pools of images to re-use

	RectTransform scrollViewRectTransform;
	UnityEngine.UI.LayoutElement sizePanelLayout;



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Awake()
	{
		base.Awake();

		if (scrollView != null)
		{
			scrollViewRectTransform = scrollView.gameObject.GetComponent<RectTransform>();
		}
		if (sizePanel != null)
		{
			sizePanelLayout = sizePanel.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
		}
	}

	
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
		if ((scrollView == null) || (content == null))
		{
			Debug.LogError("Tree screen one of the following refs is null: Scroll View, Content.");
		}
#endif //UNITY_EDITOR
	}
	
	
	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	/// <param name="buildTree">Build tree.</param>
	public void UpdateControls(bool buildTree)
	{
		if ((buildTree) && (scrollView != null) && (content != null) && (scrollViewRectTransform != null) && 
		    (sizePanel != null) && 
		    (SsMatchSettings.Instance != null) && 
		    (SsTournament.tournamentMatchInfos != null) && (SsTournament.tournamentMatchInfos.Length > 0))
		{
			SsTournamentSettings settings = SsMatchSettings.Instance.GetTournamentSettings(SsTournament.tournamentId);
			if (settings != null)
			{
				BuildTree(settings);
			}
		}


		// Keep this at the bottom of the method
		if (buildTree)
		{
			// Reset scroll view position, after all content has been added
			if (scrollView != null)
			{
				// Vertical scroll view is inverted
				scrollView.normalizedPosition = new Vector2(0.0f, 1.0f);
			}
		}
	}


	/// <summary>
	/// Builds the tournament tree.
	/// </summary>
	/// <returns>The tree.</returns>
	/// <param name="settings">Settings.</param>
	private void BuildTree(SsTournamentSettings settings)
	{
		int i, r, m, max, maxRounds, maxVerticalMatches, maxVerticalTeams, matchIndex;
		float verticalConnectionWidth, minVerticalSpaceTeams, minVerticalSpaceMatches;
		float totalHorizontalLineSize, totalVerticalLinesIntersectionsSize;
		float totalSpacesBetweenTeams, totalSpaceBetweenMatches, verticalSpaceTeams, verticalSpaceMatches;
		float topY, bottomY, leftX, rightX;
		float newVerticalSpaceTeams, newVerticalSpaceMatches, tempSize, minY;
		Rect safeArea;
		Vector2 minSize, scale, startPos, pos, totalTeamsSize, nextRoundPos, nextMatchPos, midway;
		UnityEngine.UI.Image image, imageBg;
		UnityEngine.UI.Image[] images;
		Vector2 intersectionSize, cornerRightDownSize, cornerRightUpSize;
		Vector2 intersectionPos, cornerRightDownPos, cornerRightUpPos;
		Vector2 contentTopLeft, contentBottomRight;
		SsTournamentMatchInfo matchInfo;
		Sprite sprite;
		RectTransform imageHolder = sizePanel;
		
		
		// Return all previous images to the pools
		images = imageHolder.gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
		if ((images != null) && (images.Length > 0))
		{
			for (i = 0; i < images.Length; i++)
			{
				ReturnImageToPool(images[i]);
			}
			images = null;
		}
		
		
		// Create images (if they do Not exist in the pools, or add extra if there is Not enough)
		max = (settings.maxMatches * 2) + 1;	// 1 extra for the winner's position
		CreatePool(treeTeamBgPrefab, max, imageHolder);
		CreatePool(treeTeamPrefab, max, imageHolder);
		
		max = settings.maxMatches * 3;
		CreatePool(treeLineHorizontalPrefab, max, imageHolder);
		max = settings.maxMatches * 2;
		CreatePool(treeLineVerticalPrefab, max, imageHolder);
		
		max = settings.maxMatches;
		CreatePool(treeCornerRightDownPrefab, max, imageHolder);
		CreatePool(treeCornerRightUpPrefab, max, imageHolder);
		CreatePool(treeIntersectionPrefab, max, imageHolder);
		
		
		maxRounds = settings.maxRounds;						// Max rounds in the tournament
		maxVerticalMatches = settings.matchesInFirstRound;	// Max number of vertical matches
		maxVerticalTeams = maxVerticalMatches * 2;			// Max number of vertical teams
		
		// Max width of the vertical connection elements
		verticalConnectionWidth = Mathf.Max(treeLineVerticalPrefab.rectTransform.rect.width, 
		                                    Mathf.Max(treeIntersectionPrefab.rectTransform.rect.width, 
		          Mathf.Max(treeCornerRightDownPrefab.rectTransform.rect.width, 
		          treeCornerRightUpPrefab.rectTransform.rect.width)));
		
		// The safe area in which the tree can be positioned
		safeArea = new Rect(scrollViewRectTransform.rect.min.x + innerBorder,
		                    scrollViewRectTransform.rect.min.y + innerBorder,
		                    (scrollViewRectTransform.rect.max.x - innerBorder) - (scrollViewRectTransform.rect.min.x + innerBorder),
		                    (scrollViewRectTransform.rect.max.y - innerBorder) - (scrollViewRectTransform.rect.min.y + innerBorder));
		
		// Check if minVerticalSpaceTeams is at least the size of tree_line_horizontal.size.y and tree_intersection.size.y
		// If not then increase it to the largest of the two.
		minVerticalSpaceTeams = this.minVerticalSpaceTeams;
		if (minVerticalSpaceTeams < treeLineHorizontalPrefab.rectTransform.rect.size.y)
		{
			minVerticalSpaceTeams = treeLineHorizontalPrefab.rectTransform.rect.size.y;
		}
		if (minVerticalSpaceTeams < treeIntersectionPrefab.rectTransform.rect.size.y)
		{
			minVerticalSpaceTeams = treeIntersectionPrefab.rectTransform.rect.size.y;
		}
		
		// Check if minVerticalSpaceMatches is at least the size of tree_line_horizontal.size.y and tree_intersection.size.y
		// If not then increase it to the largest of the two.
		minVerticalSpaceMatches = this.minVerticalSpaceMatches;
		if (minVerticalSpaceMatches < treeLineHorizontalPrefab.rectTransform.rect.size.y)
		{
			minVerticalSpaceMatches = treeLineHorizontalPrefab.rectTransform.rect.size.y;
		}
		if (minVerticalSpaceMatches < treeIntersectionPrefab.rectTransform.rect.size.y)
		{
			minVerticalSpaceMatches = treeIntersectionPrefab.rectTransform.rect.size.y;
		}
		
		// Calculate min horizontal content size
		totalTeamsSize.x = (float)(maxRounds + 1) * treeTeamBgPrefab.rectTransform.rect.size.x;
		totalHorizontalLineSize = (float)maxRounds * (minHorizontalLineSize * 2.0f);
		totalVerticalLinesIntersectionsSize = (float)maxRounds * verticalConnectionWidth;
		minSize.x = totalTeamsSize.x + totalHorizontalLineSize + totalVerticalLinesIntersectionsSize;
		
		// Calculate min vertical content size
		totalTeamsSize.y = (float)maxVerticalTeams * treeTeamBgPrefab.rectTransform.rect.size.y;
		totalSpacesBetweenTeams = (float)maxVerticalMatches * minVerticalSpaceTeams;
		totalSpaceBetweenMatches = (float)(maxVerticalMatches - 1) * minVerticalSpaceMatches;
		minSize.y = totalTeamsSize.y + totalSpacesBetweenTeams + totalSpaceBetweenMatches;
		
		
		// If tree fits in smaller area than the safe area, then scale it up to fill the safe area.
		// Note: The scale is only applied to spaces and lines.
		scale = Vector2.one;
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
		
		intersectionSize = Vector2.zero;
		cornerRightDownSize = Vector2.zero;
		cornerRightUpSize = Vector2.zero;
		intersectionPos = Vector2.zero;
		cornerRightDownPos = Vector2.zero;
		cornerRightUpPos = Vector2.zero;
		
		// Position the images
		safeArea = new Rect(innerBorder, -safeArea.height - innerBorder, safeArea.width, safeArea.height);
		startPos.x = safeArea.min.x;
		startPos.y = safeArea.max.y;
		contentTopLeft = startPos;
		contentBottomRight = new Vector2(safeArea.max.x, safeArea.min.y);
		pos = startPos;
		verticalSpaceTeams = (minVerticalSpaceTeams * scale.y);
		verticalSpaceMatches = (minVerticalSpaceMatches * scale.y);
		minY = float.MaxValue;
		
		matchIndex = 0;
		
		// Loop through the rounds (includes 1 extra for the winner)
		for (r = 0; r <= maxRounds; r++)
		{
			if (r == maxRounds)
			{
				max = 1;
			}
			else
			{
				max = settings.matchesInRound[r];
			}
			
			// Next round X position
			nextRoundPos.x = pos.x;
			nextRoundPos.x += treeTeamBgPrefab.rectTransform.rect.width;
			nextRoundPos.x += (minHorizontalLineSize * 2.0f * scale.x);
			nextRoundPos.x += verticalConnectionWidth;
			
			// Midway to next round (X)
			midway.x = pos.x + treeTeamBgPrefab.rectTransform.rect.width;
			midway.x = midway.x + ((nextRoundPos.x - midway.x) / 2.0f);
			
			// Loop through the matches
			for (m = 0; m < max; m++)
			{
				if ((matchIndex >= 0) && (matchIndex < SsTournament.tournamentMatchInfos.Length))
				{
					matchInfo = SsTournament.tournamentMatchInfos[matchIndex];
				}
				else
				{
					matchInfo = null;
				}
				
				// Next match Y position
				nextMatchPos.y = pos.y - treeTeamBgPrefab.rectTransform.rect.height;
				nextMatchPos.y -= verticalSpaceTeams;
				nextMatchPos.y -= treeTeamBgPrefab.rectTransform.rect.height;
				nextMatchPos.y -= verticalSpaceMatches;
				
				// Midway to next team (Y)
				midway.y = pos.y - treeTeamBgPrefab.rectTransform.rect.height;
				midway.y -= (verticalSpaceTeams / 2.0f);
				

				// Team 1
				//-------
				// Team 1 BG holder
				imageBg = GetImageFromPool(treeTeamBgPrefab, imageHolder);
				if (imageBg != null)
				{
					imageBg.rectTransform.localPosition = new Vector3(pos.x + (imageBg.rectTransform.rect.width / 2.0f), 
					                                                  pos.y - (imageBg.rectTransform.rect.height / 2.0f), 
					                                                  imageBg.rectTransform.localPosition.z);
				}

				// Team 1 logo (or winning team logo if this is the last "round")
				sprite = null;
				if (SsMenuResources.Instance != null)
				{
					if (matchInfo != null)
					{
						sprite = SsMenuResources.Instance.GetSmallTeamLogo(matchInfo.teamId[0]);
					}
					else if (SsTournament.IsTournamentDone)
					{
						// Winner
						sprite = SsMenuResources.Instance.GetSmallTeamLogo(SsTournament.WinTeamId);
					}
				}
				if (sprite != null)
				{
					image = GetImageFromPool(treeTeamPrefab, imageHolder);
					if ((image != null) && (imageBg != null))
					{
						image.rectTransform.localPosition = new Vector3(pos.x + (imageBg.rectTransform.rect.width / 2.0f), 
						                                                pos.y - (imageBg.rectTransform.rect.height / 2.0f), 
						                                                image.rectTransform.localPosition.z);
						image.sprite = sprite;
					}
				}
				
				if (r == maxRounds)
				{
					// Last "round" is the winning team
					contentBottomRight.x = pos.x + imageBg.rectTransform.rect.width;
					
					break;
				}
				
				
				// Connection elements
				//--------------------
				if (imageBg != null)
				{
					// Intersection cross |-
					image = GetImageFromPool(treeIntersectionPrefab, imageHolder);
					if (image != null)
					{
						intersectionSize = new Vector2(image.rectTransform.rect.width, image.rectTransform.rect.height);
						intersectionPos = new Vector2(midway.x, midway.y);
						image.rectTransform.localPosition = new Vector3(intersectionPos.x, 
						                                                intersectionPos.y, 
						                                                image.rectTransform.localPosition.z);
					}

					// Corner at the top, flowing right-down
					image = GetImageFromPool(treeCornerRightDownPrefab, imageHolder);
					if (image != null)
					{
						cornerRightDownSize = new Vector2(image.rectTransform.rect.width, image.rectTransform.rect.height);
						cornerRightDownPos = new Vector2(midway.x - (image.rectTransform.rect.width / 2.0f),
						                                 pos.y - (treeTeamBgPrefab.rectTransform.rect.height / 2.0f) - 
						                                 (image.rectTransform.rect.height / 4.0f));
						image.rectTransform.localPosition = new Vector3(cornerRightDownPos.x, 
						                                                cornerRightDownPos.y, 
						                                                image.rectTransform.localPosition.z);
					}

					// Corner at the bottom, flowing right-up
					image = GetImageFromPool(treeCornerRightUpPrefab, imageHolder);
					if (image != null)
					{
						cornerRightUpSize = new Vector2(image.rectTransform.rect.width, image.rectTransform.rect.height);
						cornerRightUpPos = new Vector2(midway.x - (image.rectTransform.rect.width / 2.0f),
						                               pos.y - treeTeamBgPrefab.rectTransform.rect.height - 
						                               verticalSpaceTeams - 
						                               (treeTeamBgPrefab.rectTransform.rect.height / 2.0f) + 
						                               (image.rectTransform.rect.height / 4.0f));
						image.rectTransform.localPosition = new Vector3(cornerRightUpPos.x, 
						                                                cornerRightUpPos.y, 
						                                                image.rectTransform.localPosition.z);
					}

					// Top vertical line
					image = GetImageFromPool(treeLineVerticalPrefab, imageHolder);
					if (image != null)
					{
						topY = cornerRightDownPos.y - (cornerRightDownSize.y / 2.0f);
						bottomY = intersectionPos.y + (intersectionSize.y / 2.0f);
						tempSize = topY - bottomY;
						
						image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, tempSize);
						image.rectTransform.localPosition = new Vector3(midway.x - (image.rectTransform.rect.width / 2.0f), 
						                                                topY - (tempSize / 2.0f), 
						                                                image.rectTransform.localPosition.z);
					}

					// Bottom vertical line
					image = GetImageFromPool(treeLineVerticalPrefab, imageHolder);
					if (image != null)
					{
						topY = intersectionPos.y - (intersectionSize.y / 2.0f);
						bottomY = cornerRightUpPos.y + (cornerRightUpSize.y / 2.0f);
						tempSize = topY - bottomY;
						
						image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, tempSize);
						image.rectTransform.localPosition = new Vector3(midway.x - (image.rectTransform.rect.width / 2.0f), 
						                                                topY - (tempSize / 2.0f), 
						                                                image.rectTransform.localPosition.z);
					}
					
					// Top horizontal line
					image = GetImageFromPool(treeLineHorizontalPrefab, imageHolder);
					if (image != null)
					{
						leftX = pos.x + treeTeamBgPrefab.rectTransform.rect.width;
						rightX = cornerRightDownPos.x - (cornerRightDownSize.x / 2.0f);
						tempSize = rightX - leftX;
						
						image.rectTransform.sizeDelta = new Vector2(tempSize, image.rectTransform.sizeDelta.y);
						image.rectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
						                                                cornerRightDownPos.y + 
						                                                (image.rectTransform.rect.height / 2.0f),
						                                                image.rectTransform.localPosition.z);
					}

					// Bottom horizontal line
					image = GetImageFromPool(treeLineHorizontalPrefab, imageHolder);
					if (image != null)
					{
						leftX = pos.x + treeTeamBgPrefab.rectTransform.rect.width;
						rightX = cornerRightUpPos.x - (cornerRightUpSize.x / 2.0f);
						tempSize = rightX - leftX;
						
						image.rectTransform.sizeDelta = new Vector2(tempSize, image.rectTransform.sizeDelta.y);
						image.rectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
						                                                cornerRightUpPos.y - 
						                                                (image.rectTransform.rect.height / 2.0f), 
						                                                image.rectTransform.localPosition.z);
					}

					// Middle horizontal line
					image = GetImageFromPool(treeLineHorizontalPrefab, imageHolder);
					if (image != null)
					{
						leftX = intersectionPos.x + (intersectionSize.x / 2.0f);
						rightX = nextRoundPos.x;
						tempSize = rightX - leftX;
						
						image.rectTransform.sizeDelta = new Vector2(tempSize, image.rectTransform.sizeDelta.y);
						image.rectTransform.localPosition = new Vector3(leftX + (tempSize / 2.0f), 
						                                                intersectionPos.y, 
						                                                image.rectTransform.localPosition.z);
					}
				}
				
				pos.y -= treeTeamBgPrefab.rectTransform.rect.height;
				pos.y -= verticalSpaceTeams;
				

				// Team 2
				//-------
				// Team 2 BG holder
				imageBg = GetImageFromPool(treeTeamBgPrefab, imageHolder);
				if (imageBg != null)
				{
					imageBg.rectTransform.localPosition = new Vector3(pos.x + (imageBg.rectTransform.rect.width / 2.0f), 
					                                                  pos.y - (imageBg.rectTransform.rect.height / 2.0f), 
					                                                  imageBg.rectTransform.localPosition.z);
				}

				// Team 2 logo
				sprite = null;
				if ((matchInfo != null) && (SsMenuResources.Instance != null))
				{
					sprite = SsMenuResources.Instance.GetSmallTeamLogo(matchInfo.teamId[1]);
				}
				if (sprite != null)
				{
					image = GetImageFromPool(treeTeamPrefab, imageHolder);
					if ((image != null) && (imageBg != null))
					{
						image.rectTransform.localPosition = new Vector3(pos.x + (imageBg.rectTransform.rect.width / 2.0f), 
						                                                pos.y - (imageBg.rectTransform.rect.height / 2.0f), 
						                                                image.rectTransform.localPosition.z);
						image.sprite = sprite;
					}
				}
				
				pos.y -= treeTeamBgPrefab.rectTransform.rect.height;
				if (minY > pos.y)
				{
					minY = pos.y;
				}
				pos.y -= verticalSpaceMatches;
				
				
				matchIndex ++;
			}
			
			if (r == 0)
			{
				contentBottomRight.y = pos.y + verticalSpaceMatches;
			}
			
			// REMINDER: scale has already been applied to verticalSpaceTeams and verticalSpaceMatches.
			
			// Move pos.x for next round
			pos.x = nextRoundPos.x;
			
			// Calc vertical spaces for next round
			topY = startPos.y - treeTeamBgPrefab.rectTransform.rect.height;
			topY -= (verticalSpaceTeams / 2.0f);
			topY -= (treeTeamBgPrefab.rectTransform.rect.height / 2.0f);
			
			bottomY = startPos.y - treeTeamBgPrefab.rectTransform.rect.height;
			bottomY -= verticalSpaceTeams;
			bottomY -= treeTeamBgPrefab.rectTransform.rect.height;
			bottomY -= verticalSpaceMatches;
			bottomY -= treeTeamBgPrefab.rectTransform.rect.height;
			bottomY -= (verticalSpaceTeams / 2.0f);
			bottomY += (treeTeamBgPrefab.rectTransform.rect.height / 2.0f);
			
			newVerticalSpaceTeams = topY - bottomY;
			
			topY = bottomY - treeTeamBgPrefab.rectTransform.rect.height;
			
			bottomY = topY + (treeTeamBgPrefab.rectTransform.rect.height / 2);
			bottomY -= (verticalSpaceTeams / 2);
			bottomY -= treeTeamBgPrefab.rectTransform.rect.height;
			bottomY -= verticalSpaceMatches;
			bottomY -= treeTeamBgPrefab.rectTransform.rect.height;
			bottomY -= (verticalSpaceTeams / 2);
			bottomY += (treeTeamBgPrefab.rectTransform.rect.height / 2);
			
			newVerticalSpaceMatches = topY - bottomY;
			
			
			// Set startPos.y for next round
			startPos.y -= treeTeamBgPrefab.rectTransform.rect.height;
			startPos.y -= (verticalSpaceTeams / 2.0f);
			startPos.y += (treeTeamBgPrefab.rectTransform.rect.height / 2.0f);
			pos.y = startPos.y;
			
			
			// Update the vertical spaces
			verticalSpaceMatches = newVerticalSpaceMatches;
			verticalSpaceTeams = newVerticalSpaceTeams;
		}
		
		
		// Update content size
		sizePanel.sizeDelta = new Vector2((contentBottomRight.x - contentTopLeft.x) + (innerBorder * 2),
		                                  (contentTopLeft.y - contentBottomRight.y) + (innerBorder * 2));
		
		if (sizePanelLayout != null)
		{
			sizePanelLayout.minWidth = sizePanel.sizeDelta.x;
			sizePanelLayout.minHeight = sizePanel.sizeDelta.y;
		}
	}


	/// <summary>
	/// Creates the pool of images by cloning the specified prefab. If the pool already exists then it will add more
	/// images to the pool, if needed.
	/// </summary>
	/// <returns>The amount of images added to the pool. May be less than "amount" if pool already has images.</returns>
	/// <param name="prefab">Prefab.</param>
	/// <param name="amount">Amount to clone.</param>
	/// <param name="parent">Parent to attach to.</param>
	private int CreatePool(UnityEngine.UI.Image prefab, int amount, RectTransform parent)
	{
		if ((pools == null) || (prefab == null) || (amount <= 0))
		{
			return (0);
		}

		int i, amountAdded;
		SsTreePoolImages pool;
		UnityEngine.UI.Image newImage;


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
			pool = new SsTreePoolImages();
			pools.Add(pool);
		}

		pool.prefab = prefab;
		if (pool.images == null)
		{
			pool.images = new List<UnityEngine.UI.Image>(Mathf.Max(amount, defaultPoolImagesAmount));
			pool.imageInstanceIDs = new List<int>(Mathf.Max(amount, defaultPoolImagesAmount));
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
	private SsTreePoolImages GetPool(UnityEngine.UI.Image prefab)
	{
		if ((pools == null) || (pools.Count <= 0) || (prefab == null))
		{
			return (null);
		}

		int i;
		SsTreePoolImages pool;

		for (i = 0; i < pools.Count; i++)
		{
			pool = pools[i];
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
	private SsTreePoolImages GetPool(int imageInstanceID)
	{
		if ((pools == null) || (pools.Count <= 0))
		{
			return (null);
		}
		
		int i, n;
		SsTreePoolImages pool;
		
		for (i = 0; i < pools.Count; i++)
		{
			pool = pools[i];
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
	/// <returns>The image from pool.</returns>
	/// <param name="prefab">Prefab to identify which pool to use.</param>
	/// <param name="parent">Parent to attach to.</param>
	private UnityEngine.UI.Image GetImageFromPool(UnityEngine.UI.Image prefab, RectTransform parent, 
	                                              bool setAsLastSibling = true)
	{
		SsTreePoolImages pool = GetPool(prefab);
		int i;
		UnityEngine.UI.Image image;

		if ((pool != null) && (pool.images != null) && (pool.images.Count > 0))
		{
			for (i = 0; i < pool.images.Count; i++)
			{
				image = pool.images[i];
				if (image != null)
				{
					pool.images[i] = null;	// Clear the slot
					image.gameObject.SetActive(true);
					if (image.transform.parent != parent)
					{
						image.transform.SetParent(parent, false);
					}
					if (setAsLastSibling)
					{
						image.transform.SetAsLastSibling();
					}

					return (image);
				}
			}
		}

		return (null);
	}


	/// <summary>
	/// Returns the image to the pool.
	/// </summary>
	/// <returns>The image to pool.</returns>
	/// <param name="imageToReturn">Image to return to the pool.</param>
	private void ReturnImageToPool(UnityEngine.UI.Image imageToReturn)
	{
		if (imageToReturn == null)
		{
			return;
		}

		SsTreePoolImages pool = GetPool(imageToReturn.GetInstanceID());
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
