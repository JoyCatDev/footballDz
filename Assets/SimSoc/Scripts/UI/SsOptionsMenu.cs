using UnityEngine;
using System.Collections;

/// <summary>
/// Options menu.
/// </summary>
public class SsOptionsMenu : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.ScrollRect scrollView;
	
	public UnityEngine.UI.Slider volumeSlider;
	public UnityEngine.UI.Text volumeValue;

	public UnityEngine.UI.Slider durationSlider;
	public UnityEngine.UI.Text durationValue;



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
	public override void Show(fromDirections fromDirection, System.Boolean snap)
	{
		base.Show(fromDirection, snap);

		UpdateControls();
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The controls.</returns>
	void UpdateControls()
	{
		if (volumeSlider != null)
		{
			volumeSlider.value = SsSettings.volume;
		}
		if (volumeValue != null)
		{
			volumeValue.text = ((int)(SsSettings.volume * 100)).ToString();
		}

		if (SsMatchSettings.Instance != null)
		{
			if (durationSlider != null)
			{
				durationSlider.value = SsMatchSettings.Instance.matchDuration;
			}
			if (durationValue != null)
			{
				durationValue.text = SsMatchSettings.Instance.matchDuration.ToString();
			}
		}
	}
	
	
	/// <summary>
	/// Raises the volume event.
	/// </summary>
	public void OnVolume()
	{
		if (volumeSlider != null)
		{
			SsSettings.SetVolume(volumeSlider.value);
		}
		UpdateControls();
	}


	/// <summary>
	/// Raises the duration event.
	/// </summary>
	public void OnDuration()
	{
		if (SsMatchSettings.Instance != null)
		{
			if (durationSlider != null)
			{
				SsMatchSettings.Instance.matchDuration = durationSlider.value;
			}
			UpdateControls();
		}
	}
}
