using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Formation select screen.
/// </summary>
public class SsFormationSelectScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Formations")]
	[Tooltip("Formation item prefab to clone.")]
	public SsFormationUiItem formationItemPrefab;

	public UnityEngine.UI.Button randomButton;

	[Space(10)]
	public UnityEngine.UI.ScrollRect scrollView;

	public GameObject content;
	
	[Tooltip("Formation highlight selectors.")]
	public UnityEngine.UI.Image[] selectors;


	// Private
	//--------
	private SsUiManager.SsButtonToResource[] formationButtons;	// Link buttons to formations

	private int activeUser = 0;							// Active user who has to select a formation (0 or 1)
	private int maxActiveUsers = 2;						// Max number of active users


	// Methods
	//--------
	
	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		SpawnFormations();

		// Reset scroll view position, after all content has been added
		if (scrollView != null)
		{
			scrollView.normalizedPosition = Vector2.zero;
		}

		maxActiveUsers = SsSettings.selectedNumPlayers;
		activeUser = 0;
		
		UpdateSelectors();
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
		
		maxActiveUsers = SsSettings.selectedNumPlayers;
		activeUser = 0;
		
		UpdateSelectors();
	}


	/// <summary>
	/// Spawns the formation buttons and setup their links to the the formation IDs.
	/// </summary>
	/// <returns>The formations.</returns>
	private void SpawnFormations()
	{
		List<SsUiManager.SsButtonToResource> list = new List<SsUiManager.SsButtonToResource>();
		SsUiManager.SsButtonToResource newRes;
		int i;

		if ((formationItemPrefab != null) && (content != null) && 
			(SsFormationManager.Instance != null) && (SsFormationManager.Instance.formations != null) && 
		    (SsFormationManager.Instance.formations.Length > 0))
		{
			SsFormationUiItem newItem;
			RectTransform rectTransform;
			UnityEngine.UI.Button btn;
			SsFormation formation;

			for (i = 0; i < SsFormationManager.Instance.formations.Length; i++)
			{
				formation = SsFormationManager.Instance.formations[i];
				if (formation == null)
				{
					continue;
				}

				newItem = (SsFormationUiItem)Instantiate(formationItemPrefab);
				if (newItem == null)
				{
					continue;
				}

				newItem.SetFormation(formation);

				rectTransform = newItem.gameObject.GetComponent<RectTransform>();
				btn = newItem.gameObject.GetComponent<UnityEngine.UI.Button>();

				// Need to use a separate reference for the delegate parameter, or else it will only use the last button in the loop.
				UnityEngine.UI.Button btnParameter = btn;
				btn.onClick.AddListener(delegate{OnFormation(btnParameter);});

				if (rectTransform != null)
				{
					rectTransform.SetParent(content.transform, false);
				}
				else
				{
					newItem.transform.parent = content.transform;
				}

				newRes = SsUiManager.SsButtonToResource.CreateButtonResource(btn, formation.id);
				if (newRes != null)
				{
					list.Add(newRes);
				}
			}
		}


		// Random button
		if (randomButton != null)
		{
			newRes = SsUiManager.SsButtonToResource.CreateButtonResource(randomButton, null);
			if (newRes != null)
			{
				list.Add(newRes);
			}

			// Move random to the end of the list
			randomButton.transform.SetAsLastSibling();

			// Move selectors after the random button
			if ((selectors != null) && (selectors.Length > 0))
			{
				for (i = 0; i < selectors.Length; i++)
				{
					if (selectors[i] != null)
					{
						selectors[i].transform.SetAsLastSibling();
					}
				}
			}
		}

		if (list.Count > 0)
		{
			formationButtons = list.ToArray();
		}

		// Not needed, but just in case
		Canvas.ForceUpdateCanvases();
	}


	/// <summary>
	/// Get the formation ID from the button. null if Not found.
	/// </summary>
	/// <returns>The formation identifier.</returns>
	/// <param name="button">Button.</param>
	private string GetFormationId(UnityEngine.UI.Button button)
	{
		return (SsUiManager.SsButtonToResource.GetIdFromButton(button, formationButtons));
	}


	/// <summary>
	/// Updates the selectors.
	/// </summary>
	/// <returns>The selectors.</returns>
	public void UpdateSelectors()
	{
		int i;
		
		for (i = 0; i < 2; i++)
		{
			if ((i >= 0) && 
			    (selectors != null) && (i < selectors.Length) && 
			    (SsSettings.selectedFormationIds != null) && (i < SsSettings.selectedFormationIds.Length))
			{
				UpdateSelector(selectors[i], SsSettings.selectedFormationIds[i], (i <= maxActiveUsers - 1), (i == activeUser));
			}
		}
	}


	/// <summary>
	/// Update the selector.
	/// </summary>
	/// <returns>The selector.</returns>
	/// <param name="selector">Selector.</param>
	/// <param name="selectedFormation">Selected formation ID.</param>
	/// <param name="enable">Enable.</param>
	/// <param name="animate">Animate.</param>
	public void UpdateSelector(UnityEngine.UI.Image selector, string selectedFormation, bool enable, bool animate)
	{
		if (selector == null)
		{
			return;
		}
		
		if ((formationButtons == null) || (formationButtons.Length <= 0) || (enable == false))
		{
			selector.gameObject.SetActive(false);
			return;
		}

		int i;
		SsUiManager.SsButtonToResource res;
		RectTransform rectTransform;
		float z;
		string selectedFormationLower = (string.IsNullOrEmpty(selectedFormation) == false) ? selectedFormation.ToLower() : null;
		
		for (i = 0; i < formationButtons.Length; i++)
		{
			res = formationButtons[i];
			if ((res == null) || (res.button == null))
			{
				continue;
			}
			if (((string.IsNullOrEmpty(res.id)) && (string.IsNullOrEmpty(selectedFormation))) || 
			    ((string.IsNullOrEmpty(res.id) == false) && (res.id.ToLower() == selectedFormationLower)))
			{
				rectTransform = res.GetButtonRectTransform();
				if (rectTransform != null)
				{
					if (selector.gameObject.activeInHierarchy == false)
					{
						selector.gameObject.SetActive(true);
					}

					z = selector.transform.localPosition.z;
					selector.rectTransform.SetParent(rectTransform, false);
					selector.transform.localPosition = new Vector3(0.0f, 0.0f, z);
					
					if (animate)
					{
						selector.color = new Color(selector.color.r, selector.color.g, selector.color.b, 1.0f);
					}
					else
					{
						selector.color = new Color(selector.color.r, selector.color.g, selector.color.b, 0.5f);
					}
					return;
				}
			}
		}
		
		selector.gameObject.SetActive(false);
	}


	/// <summary>
	/// Raises the formation event.
	/// </summary>
	/// <param name="button">Button.</param>
	public void OnFormation(UnityEngine.UI.Button button)
	{
		if ((activeUser >= 0) && 
		    (selectors != null) && (activeUser < selectors.Length) && 
		    (SsSettings.selectedFormationIds != null) && (activeUser < SsSettings.selectedFormationIds.Length))
		{
			SsSettings.selectedFormationIds[activeUser] = GetFormationId(button);

			if (maxActiveUsers > 1)
			{
				// Switch to the next user
				if (activeUser == 0)
				{
					activeUser = 1;
				}
				else
				{
					activeUser = 0;
				}
			}
			
			UpdateSelectors();
		}
	}
}
