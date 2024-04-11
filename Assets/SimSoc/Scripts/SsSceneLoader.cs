using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene loader. A helper to load a scene with the option to display a loading screen (scene).
/// </summary>
public class SsSceneLoader : MonoBehaviour {

	// Const/Static
	//-------------
	private const float delayLoadingFinalScene = 0.1f;		// Delay before loading the final scene



	// Enums
	//------
	public enum states
	{
		idle = 0,
		loadingLoadingScreen,
		showingLoadingScreen,
		loadingFinalScene,
	}


	// Private
	//--------
	static private SsSceneLoader instance;

	static private states state = states.idle;
	static private float stateTime;

	static private string finalScene;
	static private string loadingScreen;
	static private float loadDelay = 0.0f;

	static private float lastUpdateTime = 0.0f;		// Last time during the update method
	static private float unpausedDT = 0.0f;			// Delta time unpaused (i.e. Not affected when game is paused when Time.timeScale is zero).


	// Properties
	//-----------
	static public SsSceneLoader Instance
	{
		get { return(instance); }
		set
		{
			instance = value;
		}
	}


	/// <summary>
	/// The final scene that will be loaded.
	/// </summary>
	/// <value>The final scene.</value>
	static public string FinalScene
	{
		get { return(finalScene); }
	}


	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	
	
	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	/// <summary>
	/// Called when a scene loaded.
	/// </summary>
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if ((state == states.loadingLoadingScreen) && 
		    (GeUtils.GetLoadedLevelName() == loadingScreen))
		{
			SetState(states.showingLoadingScreen);
			loadDelay = delayLoadingFinalScene;
			
			Resources.UnloadUnusedAssets();
		}
		else if (state == states.loadingFinalScene)
		{
			SetState(states.idle);
		}
	}

	/// <summary>
	/// Sets the state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="newState">New state.</param>
	static private void SetState(states newState)
	{
		state = newState;
		stateTime = 0.0f;
	}


	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		float dt;
		
		unpausedDT = Time.realtimeSinceStartup - lastUpdateTime;
		lastUpdateTime = Time.realtimeSinceStartup;
		dt = unpausedDT;

		stateTime += dt;

		if (state == states.showingLoadingScreen)
		{
			loadDelay -= dt;
			if (loadDelay <= 0.0f)
			{
				loadDelay = 0.0f; 
				SetState(states.loadingFinalScene);
				GeUtils.LoadLevel(finalScene);
			}
		}
		else if (state == states.loadingLoadingScreen)
		{
			// Taking too long to load the loading screen (possibly Not added to build settings)?
			if (stateTime > 10.0f)
			{
				SetState(states.showingLoadingScreen);
			}
		}
	}


	/// <summary>
	/// Load the scene. First load the loading screen, then load the scene.
	/// </summary>
	/// <returns>The scene.</returns>
	/// <param name="sceneName">Name of scene to load.</param>
	/// <param name="loadingScreenName">Name of the scene that contains the loading screen to show. If Null then will Not show a loading screen.</param>
	static public void LoadScene(string sceneName, string loadingScreenName)
	{
		if (instance == null)
		{
			GameObject go = new GameObject("Scene Loader");
			if (go != null)
			{
				DontDestroyOnLoad(go);
				go.AddComponent<SsSceneLoader>();
			}
		}

		if (instance == null)
		{
#if UNITY_EDITOR
			Debug.LogError("ERROR: There is no scene loader.");
#endif //UNITY_EDITOR
			return;
		}

		lastUpdateTime = Time.realtimeSinceStartup;

		loadingScreen = loadingScreenName;
		stateTime = 0.0f;

		if (string.IsNullOrEmpty(loadingScreenName))
		{
			// Load the final scene immediately
			SetState(states.loadingFinalScene);
			GeUtils.LoadLevel(sceneName);
		}
		else
		{
			// First load the loading screen scene
			SetState(states.loadingLoadingScreen);
			finalScene = sceneName;
			loadDelay = delayLoadingFinalScene;
			GeUtils.LoadLevel(loadingScreenName);
		}
	}

}
