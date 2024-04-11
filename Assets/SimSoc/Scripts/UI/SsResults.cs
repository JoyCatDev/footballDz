using UnityEngine;
using System.Collections;

/// <summary>
/// Results menu.
/// </summary>
public class SsResults : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public UnityEngine.UI.Image leftIcon;
	public UnityEngine.UI.Image rightIcon;
	public UnityEngine.UI.Text leftScore;
	public UnityEngine.UI.Text rightScore;

	[Space(10)]
	public UnityEngine.UI.Button restart;
	public UnityEngine.UI.Button quit;
	public UnityEngine.UI.Button next;

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
		bool visible;
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
				sprite = leftTeam.resultIcon;
				if (sprite != null)
				{
					leftIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Result Icon not set. Team: " + leftTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}
			if (rightIcon != null)
			{
				sprite = rightTeam.resultIcon;
				if (sprite != null)
				{
					rightIcon.sprite = sprite;
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("WARNING: Team's Result Icon not set. Team: " + rightTeam.GetAnyName());
				}
#endif //UNITY_EDITOR
			}


			visible = (SsSettings.selectedMatchType == SsMatch.matchTypes.friendly);
			if ((restart != null) && (restart.gameObject.activeInHierarchy != visible))
			{
				restart.gameObject.SetActive(visible);
			}
			if ((quit != null) && (quit.gameObject.activeInHierarchy != visible))
			{
				quit.gameObject.SetActive(visible);
			}

			visible = (SsSettings.selectedMatchType == SsMatch.matchTypes.tournament);
			if ((next != null) && (next.gameObject.activeInHierarchy != visible))
			{
				next.gameObject.SetActive(visible);
			}
		}
	}


	/// <summary>
	/// Raises the restart event.
	/// </summary>
	public void OnRestart()
	{
		HideCallback = OnHideDoneRestart;
		Hide(false, toDirections.invalid);
	}


	/// <summary>
	/// Raises the hide done restart event.
	/// </summary>
	private void OnHideDoneRestart()
	{
		SsNewMatch.RestartMatch();
	}


	/// <summary>
	/// Raises the quit event.
	/// </summary>
	public void OnQuit()
	{
		HideCallback = OnHideDoneQuit;
		Hide(false, toDirections.invalid);
	}


	/// <summary>
	/// Raises the hide done quit event.
	/// </summary>
	private void OnHideDoneQuit()
	{
		SsMatch match = SsMatch.Instance;
		if (match != null)
		{
			match.EndMatchAndLoadNextScene();
		}
	}

}
