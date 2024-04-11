using UnityEngine;
using System.Collections;

/// <summary>
/// Field select screen.
/// </summary>
public class SsFieldSelectScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Link buttons to fields")]
	public SsUiManager.SsButtonToResource[] fieldButtons;

	[Space(10)]
	public UnityEngine.UI.ScrollRect scrollView;

	[Tooltip("Highlight selector.")]
	public UnityEngine.UI.Image selector;


	// Methods
	//--------

	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Start()
	{
		base.Start();

		// Reset scroll view position, after all content has been added
		if (scrollView != null)
		{
			scrollView.normalizedPosition = Vector2.zero;
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
		
		UpdateSelector();
	}


	/// <summary>
	/// Get the field ID from the button. null if Not found.
	/// </summary>
	/// <returns>The field identifier.</returns>
	/// <param name="button">Button.</param>
	private string GetFieldId(UnityEngine.UI.Button button)
	{
		return (SsUiManager.SsButtonToResource.GetIdFromButton(button, fieldButtons));
	}


	/// <summary>
	/// Updates the selector.
	/// </summary>
	/// <returns>The selector.</returns>
	public void UpdateSelector()
	{
		if (selector == null)
		{
			return;
		}
		
		if ((fieldButtons == null) || (fieldButtons.Length <= 0))
		{
			selector.gameObject.SetActive(false);
			return;
		}
		
		int i;
		SsUiManager.SsButtonToResource res;
		RectTransform rectTransform;
		float z;
		string fieldIdLower = (string.IsNullOrEmpty(SsSettings.selectedFieldId) == false) ? SsSettings.selectedFieldId.ToLower() : null;
		
		for (i = 0; i < fieldButtons.Length; i++)
		{
			res = fieldButtons[i];
			if ((res == null) || (res.button == null))
			{
				continue;
			}

			if (((string.IsNullOrEmpty(res.id)) && (string.IsNullOrEmpty(SsSettings.selectedFieldId))) || 
				((string.IsNullOrEmpty(res.id) == false) && (res.id.ToLower() == fieldIdLower)))
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
					
					return;
				}
			}
		}
		
		selector.gameObject.SetActive(false);
	}


	/// <summary>
	/// Raises the field event.
	/// </summary>
	/// <param name="button">Button.</param>
	public void OnField(UnityEngine.UI.Button button)
	{
		SsSettings.selectedFieldId = GetFieldId(button);
		UpdateSelector();
	}


}
