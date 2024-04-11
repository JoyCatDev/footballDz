using UnityEngine;
using System.Collections;

/// <summary>
/// Ball select screen.
/// </summary>
public class SsBallSelectScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Link buttons to balls")]
	public SsUiManager.SsButtonToResource[] ballButtons;
	
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
	/// Get the ball ID from the button. null if Not found.
	/// </summary>
	/// <returns>The ball identifier.</returns>
	/// <param name="button">Button.</param>
	private string GetBallId(UnityEngine.UI.Button button)
	{
		return (SsUiManager.SsButtonToResource.GetIdFromButton(button, ballButtons));
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
		
		if ((ballButtons == null) || (ballButtons.Length <= 0))
		{
			selector.gameObject.SetActive(false);
			return;
		}
		
		int i;
		SsUiManager.SsButtonToResource res;
		RectTransform rectTransform;
		float z;
		string ballIdLower = (string.IsNullOrEmpty(SsSettings.selectedBallId) == false) ? SsSettings.selectedBallId.ToLower() : null;
		
		for (i = 0; i < ballButtons.Length; i++)
		{
			res = ballButtons[i];
			if ((res == null) || (res.button == null))
			{
				continue;
			}
			
			if (((string.IsNullOrEmpty(res.id)) && (string.IsNullOrEmpty(SsSettings.selectedBallId))) || 
			    ((string.IsNullOrEmpty(res.id) == false) && (res.id.ToLower() == ballIdLower)))
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
	/// Raises the ball event.
	/// </summary>
	/// <param name="button">Button.</param>
	public void OnBall(UnityEngine.UI.Button button)
	{
		SsSettings.selectedBallId = GetBallId(button);
		UpdateSelector();
	}
}
