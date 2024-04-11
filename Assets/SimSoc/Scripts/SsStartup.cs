using UnityEngine;
using System.Collections;

/// <summary>
/// Add this to a gameobject in the startup scene.
/// </summary>
public class SsStartup : MonoBehaviour {

	// Public
	//-------
	public string nextSceneToLoad = "SplashScreen";


	// Methods
	//--------

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start()
	{
		// NOTE: Here you can initialise plugins

		GeUtils.LoadLevel(nextSceneToLoad);
	}

}
