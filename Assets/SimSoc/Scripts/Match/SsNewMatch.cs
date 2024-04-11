using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Stores settings for a new match, and starts the match.
/// </summary>
public class SsNewMatch : MonoBehaviour {

	// Enums
	//------
	public enum states
	{
		init = 0,
		loadLoadingScreen,		// Busy loading the loading screen scene (if any).
		loadFieldScene,			// Busy loading the field scene (if any).
		creatingMatch,			// Busy creating the match.
		sleeping,				// Sleeping, inactive. Until needed again to restart a match, or destroyed when returning to main menu.
	}


	// Public
	//-------

	[System.NonSerialized]
	public string[] teamResources;

	[System.NonSerialized]
	public SsTeam[] teamPrefabs;


	[System.NonSerialized]
	public SsInput.inputTypes[] teamInput;


	[System.NonSerialized]
	public string ballResource;

	[System.NonSerialized]
	public SsBall ballPrefab;



	[System.NonSerialized]
	public string fieldScene;

	[System.NonSerialized]
	public string optionalLoadingScene;

	[System.NonSerialized]
	public float overrideDuration;

	[System.NonSerialized]
	public bool needsWinner;

	[System.NonSerialized]
	public int overrideDifficulty;



	// Private
	//--------
	static private SsNewMatch instance;

	static private List<SsNewMatch> newMatches = new List<SsNewMatch>();	// List so they can be cleaned up

	private states state = states.init;
	private int stateCount;



	// Properties
	//-----------
	static public SsNewMatch Instance
	{
		get { return(instance); }
	}



	// Methods
	//--------
	/// <summary>
	/// Destroy all instances. Call this before returning to the main menu from the field scene.
	/// </summary>
	/// <returns>The all.</returns>
	static public void DestroyAll()
	{
		if ((newMatches != null) && (newMatches.Count > 0))
		{
			int i;
			SsNewMatch newMatch;
			List<SsNewMatch> toDestroy = new List<SsNewMatch>();
			for (i = 0; i < newMatches.Count; i++)
			{
				newMatch = newMatches[i];
				if (newMatch != null)
				{
					toDestroy.Add(newMatch);
				}
			}

			if (toDestroy.Count > 0)
			{
				for (i = 0; i < toDestroy.Count; i++)
				{
					Destroy(toDestroy[i].gameObject);
				}
				toDestroy.Clear();
			}

			newMatches.Clear();
		}
	}


	/// <summary>
	/// Restart the match with the previous match's settings.
	/// </summary>
	/// <returns>The match.</returns>
	static public void RestartMatch()
	{
		if (instance == null)
		{
			return;
		}

		SsVolumeController.FadeVolumeOut(SsVolumeController.matchMusicFadeOutTime);

		// We must load a field scene when a match restarts
		if (string.IsNullOrEmpty(instance.fieldScene))
		{
			// Reload the current scene. (It should be a field scene!)
			instance.fieldScene = GeUtils.GetLoadedLevelName();
		}


		instance.gameObject.SetActive(true);
		instance.SetState(states.init);
	}


	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		if (instance != null)
		{
			// Must only have one instance at a time
			DestroyAll();
		}

		instance = this;

		if (newMatches != null)
		{
			newMatches.Add(this);
		}

		SetState(states.init);

		DontDestroyOnLoad(gameObject);
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}

		if ((newMatches != null) && (newMatches.Contains(this)))
		{
			newMatches.Remove(this);
		}
	}


	/// <summary>
	/// Sets the state.
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="newState">New state.</param>
	void SetState(states newState)
	{
		state = newState;
		stateCount = 0;

		switch (state)
		{
		case states.init:
		{
			break;
		}
		case states.loadLoadingScreen:
		{
			if (string.IsNullOrEmpty(optionalLoadingScene) == false)
			{
				GeUtils.LoadLevel(optionalLoadingScene);
			}
			break;
		}
		case states.loadFieldScene:
		{
			if (string.IsNullOrEmpty(fieldScene) == false)
			{
				GeUtils.LoadLevel(fieldScene);
			}
			break;
		}
		case states.creatingMatch:
		{
			if ((SsMatch.CreateMatch()) && (SsMatch.Instance != null))
			{
				SsMatch.Instance.StartMatchWithSettings(this);
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogError("ERROR: Failed to create match.");
			}
#endif //UNITY_EDITOR
			break;
		}
		case states.sleeping:
		{
			// Sleeping, inactive. Until needed again to restart a match.
			gameObject.SetActive(false);
			break;
		}
		} //switch
	}

	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		stateCount ++;

		switch (state)
		{
		case states.init:
		{
			if (stateCount > 2)
			{
				SetState(states.loadLoadingScreen);
			}
			break;
		}
		case states.loadLoadingScreen:
		{
			if (stateCount > 2)
			{
				SetState(states.loadFieldScene);
			}
			break;
		}
		case states.loadFieldScene:
		{
			if (stateCount > 2)
			{
				SetState(states.creatingMatch);
			}
			break;
		}
		case states.creatingMatch:
		{
			SetState(states.sleeping);
			break;
		}
		} //switch
	}
}
