using UnityEngine;
using System.Collections;


/// <summary>
/// Resource manager. Keeps a list of resources (e.g. teams, players, balls).
/// </summary>
public class SsResourceManager : MonoBehaviour {

	// Const/Static
	//-------------
	// Default resource manager paths
	public const string resourceManagerRuntimePath = "Resource Manager";
	public const string resourceManagerEditorPath = "Assets/SimSoc/Resources/Resource Manager.prefab";



	// Classes
	//--------
	// Resource (e.g. team, ball)
	[System.Serializable]
	public class SsResource
	{
		[Tooltip("Path name in the Resources folder. Path names exclude \"Resources/\"")]
		public string path;

		[Tooltip("Unique ID.")]
		public string id;
	}

	// Ball resource
	[System.Serializable]
	public class SsBallResource : SsResource
	{
		public string displayName;
	}

	// Team resource
	[System.Serializable]
	public class SsTeamResource : SsResource
	{
		public string teamName;
		public string preferredName;
		public string shortName;
		public string name3Letters;
		public string homeFieldId;
	}

	// Player resource
	[System.Serializable]
	public class SsPlayerResource : SsResource
	{
		public string firstName;
		public string lastName;
		public string preferredName;
		public string shortName;
		public string name3Letters;
		public SsPlayer.genders gender;
		public int age;
		public int jerseyNumber;
		public float headHeight;
	}


	// Public
	//-------
	[Header("Resources added automatically, or via the \"Scan Resources\" menu.")]
	[Tooltip("Balls in the Resources folder.")]
	public SsBallResource[] balls;

	[Tooltip("Teams in the Resources folder.")]
	public SsTeamResource[] teams;

	[Tooltip("Players in the Resources folder.")]
	public SsPlayerResource[] players;



	// Private
	//--------
	static private SsResourceManager instance;

	private string[] teamIds;



	// Properties
	//-----------
	static public SsResourceManager Instance
	{
		get { return(instance); }
	}



	// Methods
	//--------
	/// <summary>
	/// Load the resource manager and instantiate it.
	/// </summary>
	static public void Load()
	{
		// Check in all Resources folders.
		SsResourceManager[] resourceManagers = Resources.LoadAll<SsResourceManager>("");
		if ((resourceManagers != null) && (resourceManagers.Length > 0))
		{
			if ((resourceManagers.Length == 1) && (resourceManagers[0] != null))
			{
				SsResourceManager resourceManager = Instantiate(resourceManagers[0]);
				if (resourceManager != null)
				{
					DontDestroyOnLoad(resourceManager.gameObject);
					Resources.UnloadUnusedAssets();
					return;
				}
			}
#if UNITY_EDITOR
			else if (resourceManagers.Length > 1)
			{
				Debug.LogWarning("WARNING: Found more than 1 resource manager. Going to load the default one: " + resourceManagerRuntimePath);
			}
#endif //UNITY_EDITOR
		}


		// Try the default path
		GameObject go = GeUtils.LoadGameObjectAndInstantiate(resourceManagerRuntimePath);
		if (go != null)
		{
			DontDestroyOnLoad(go);
		}
#if UNITY_EDITOR
		else
		{
			Debug.LogError("ERROR: Failed to load the resource manager: " + resourceManagerRuntimePath + "       Try running Scan Resources in the menu.");
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Get a random team resource.
	/// </summary>
	/// <returns>The random team. Null if none was found.</returns>
	/// <param name="excludeTeamId">ID of the team to exclude (e.g. a team that has already been selected). (Optional)</param>
	public SsTeamResource GetRandomTeam(string excludeId)
	{
		if ((teams == null) || (teams.Length <= 0))
		{
			return (null);
		}

		int i, retry;
		SsTeamResource team;
		string excludeIdLower = (string.IsNullOrEmpty(excludeId) == false) ? excludeId.ToLower() : null;

		retry = 0;
		while (retry < 10)
		{//rafik
			team = teams[Random.Range(1, 2)];
			if ((team != null) && 
			    (string.IsNullOrEmpty(team.id) == false) && 
			    (team.id.ToLower() != excludeIdLower))
			{
				return (team);
			}
			retry ++;
		}

		// Use the first available one
		for (i = 0; i < teams.Length; i++)
		{
			team = teams[i];
			if ((team != null) && 
			    (string.IsNullOrEmpty(team.id) == false) && 
			    (team.id.ToLower() != excludeIdLower))
			{
				return (team);
			}
		}

		return (null);
	}


	/// <summary>
	/// Get a random team resource from the specified team IDs.
	/// </summary>
	/// <returns>The random team.</returns>
	/// <param name="excludeId">ID of the team to exclude (e.g. a team that has already been selected). (Optional)</param>
	/// <param name="fromTeamIds">Array of team IDs from which to select a random one. (Optional)</param>
	public SsTeamResource GetRandomTeam(string excludeId, string[] fromTeamIds)
	{
		if ((teams == null) || (teams.Length <= 0) || 
		    (fromTeamIds == null) || (fromTeamIds.Length <= 0))
		{
			return (null);
		}
		
		int i, retry;
		string teamId;
		string excludeIdLower = (string.IsNullOrEmpty(excludeId) == false) ? excludeId.ToLower() : null;
		
		retry = 0;
		while (retry < 10)
		{
			teamId = fromTeamIds[Random.Range(0, fromTeamIds.Length)];
			if ((string.IsNullOrEmpty(teamId) == false) && 
			    (teamId.ToLower() != excludeIdLower))
			{
				return (GetTeam(teamId));
			}
			retry ++;
		}
		
		// Use the first available one
		for (i = 0; i < fromTeamIds.Length; i++)
		{
			teamId = fromTeamIds[i];
			if ((string.IsNullOrEmpty(teamId) == false) && 
			    (teamId.ToLower() != excludeIdLower))
			{
				return (GetTeam(teamId));
			}
		}
		
		return (null);
	}


	/// <summary>
	/// Get the team, based on its ID.
	/// </summary>
	/// <returns>The team.</returns>
	/// <param name="id">Identifier.</param>
	public SsTeamResource GetTeam(string id)
	{
		if ((teams == null) || (teams.Length <= 0) ||
		    (string.IsNullOrEmpty(id)))
		{
			return (null);
		}
		
		int i;
		SsTeamResource team;
		string idLower = id.ToLower();

		for (i = 0; i < teams.Length; i++)
		{
			team = teams[i];
			if ((team != null) && 
			    (string.IsNullOrEmpty(team.id) == false) && 
			    (team.id.ToLower() == idLower))
			{
				return (team);
			}
		}

		return (null);
	}


	/// <summary>
	/// Get a random ball resource.
	/// </summary>
	/// <returns>The random ball.</returns>
	public SsBallResource GetRandomBall()
	{
		if ((balls == null) || (balls.Length <= 0))
		{
			return (null);
		}
		
		int i, retry;
		SsBallResource ball;
		
		retry = 0;
		while (retry < 10)
		{
			ball = balls[Random.Range(0, balls.Length)];
			if (ball != null)
			{
				return (ball);
			}
			retry ++;
		}
		
		// Use the first available one
		for (i = 0; i < balls.Length; i++)
		{
			ball = balls[i];
			if (ball != null)
			{
				return (ball);
			}
		}
		
		return (null);
	}


	/// <summary>
	/// Get the ball, based on its ID.
	/// </summary>
	/// <returns>The ball.</returns>
	/// <param name="id">Identifier.</param>
	public SsBallResource GetBall(string id, bool returnRandomIfNotFound = true)
	{
		if ((balls == null) || (balls.Length <= 0))
		{
			return (null);
		}

		if (string.IsNullOrEmpty(id))
		{
			if (returnRandomIfNotFound)
			{
				return (GetRandomBall());
			}
			return (null);
		}
		
		int i;
		SsBallResource ball;
		string idLower = id.ToLower();
		
		for (i = 0; i < balls.Length; i++)
		{
			ball = balls[i];
			if ((ball != null) && 
			    (string.IsNullOrEmpty(ball.id) == false) && 
			    (ball.id.ToLower() == idLower))
			{
				return (ball);
			}
		}

		if (returnRandomIfNotFound)
		{
			return (GetRandomBall());
		}

		return (null);
	}


	/// <summary>
	/// Gets the ball.
	/// </summary>
	/// <returns>The ball.</returns>
	/// <param name="index">Index.</param>
	public SsBallResource GetBall(int index)
	{
		if ((balls == null) || (balls.Length <= 0) || 
		    (index < 0) || (index >= balls.Length))
		{
			return (null);
		}
		return (balls[index]);
	}

}
