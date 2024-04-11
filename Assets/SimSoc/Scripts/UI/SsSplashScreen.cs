using UnityEngine;
using System.Collections;

/// <summary>
/// Splash screen.
/// </summary>
public class SsSplashScreen : SsBaseMenu {

	// Public
	//-------
	[Header("Next Scene")]
	[Tooltip("Next scene to load when player clicks/taps on the screen.")]
	public string nextScene = "MainMenus";

	[Tooltip("Show loading screen when loading the next scene.")]
	public bool showLoadingSceen = true;



	// Methods
	//--------
	/// <summary>
	/// Raises the continue event.
	/// </summary>
	public void OnContinue()
	{
		if ((showLoadingSceen) && (SsSceneManager.Instance != null))
		{
			SsSceneLoader.LoadScene(nextScene, SsSceneManager.Instance.defaultLoadingScene);
		}
		else
		{
			SsSceneLoader.LoadScene(nextScene, null);
		}
	}

}
