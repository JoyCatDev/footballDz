using UnityEngine;
using System.Collections;

/// <summary>
/// Half time menu.
/// </summary>
public class SsHalfTime : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.Image leftIcon;
	public UnityEngine.UI.Image rightIcon;
	public UnityEngine.UI.Text leftScore;
	public UnityEngine.UI.Text rightScore;

	[Space(10)]
	[Tooltip("Auto swap the icons and score based on the camera's play direction.")]
	public bool autoSwapIconsForCam = true;



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

		UpdateControls(true);
	}


	/// <summary>
	/// Hide the menu immediately (no animation).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The immediate.</returns>
	protected override void HideImmediate()
	{
		base.HideImmediate();

		SsMatch match = SsMatch.Instance;
		if (match != null)
		{
			match.OnHalfTimeUiClosed();
		}
	}


	/// <summary>
	/// Updates the controls.
	/// </summary>
	/// <returns>The user interface.</returns>
	/// <param name="updateIcons">Update team icons.</param>
	public void UpdateControls(bool updateIcons)
	{
		SsMatch match = SsMatch.Instance;

		if ((match == null) || (match.LeftTeam == null) || (match.RightTeam == null))
		{
			return;
		}
		
		Sprite sprite;
		SsTeam leftTeam, rightTeam;
		
		if (autoSwapIconsForCam)
		{
			// Get the team's based on the camera's play direction
			SsHud.GetTeamSidesForUI(out leftTeam, out rightTeam);
		}
		else
		{
			leftTeam = match.LeftTeam;
			rightTeam = match.RightTeam;
		}

		
		if (leftScore != null)
		{
			leftScore.text = leftTeam.Score.ToString();
		}
		if (rightScore != null)
		{
			rightScore.text = rightTeam.Score.ToString();
		}
		
		if (updateIcons)
		{
			if (leftIcon != null)
			{
				if (leftTeam.halfTimeIcon != null)
				{
					sprite = leftTeam.halfTimeIcon;
				}
				else
				{
					sprite = leftTeam.resultIcon;
				}
				if (sprite != null)
				{
					leftIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Half Icon/Result Icon not set. Team: " + leftTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}
			if (rightIcon != null)
			{
				if (rightTeam.halfTimeIcon != null)
				{
					sprite = rightTeam.halfTimeIcon;
				}
				else
				{
					sprite = rightTeam.resultIcon;
				}
				if (sprite != null)
				{
					rightIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Half Icon/Result Icon not set. Team: " + rightTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}
		}
	}

}
