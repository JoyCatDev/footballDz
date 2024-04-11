using UnityEngine;
using System.Collections;

/// <summary>
/// Scene manager. Similar to the resource manager (SsResourceManager), but for scenes.
/// Mainly used for the field scenes.
/// </summary>
public class SsSceneManager : MonoBehaviour {

	// Classes
	//--------
	// Scene resource
	[System.Serializable]
	public class SsSceneResource
	{
		[Tooltip("Scene name to link to the ID. Excludes the path. It must also be added to the build settings.")]
		public string sceneName;
		
		[Tooltip("Unique ID. Do Not change this after the game has been released.")]
		public string id;
	}

	// Field scene resource
	[System.Serializable]
	public class SsFieldSceneResource : SsSceneResource
	{
		[Tooltip("Field name to display on the UI.")]
		public string displayName;
	}


	// Public
	//-------
	[Header("Scenes")]
	[Tooltip("Default loading screen scene name. It must also be added to the build settings. Clear if there is no default loading screen.")]
	public string defaultLoadingScene = "LoadingScreen";

	[Tooltip("The scene to load after a match ends (or user quits the match).")]
	public string matchEndScene = "MainMenus";

	[Tooltip("Field scenes.")]
	public SsFieldSceneResource[] fieldScenes;


	// Private
	//--------
	static private SsSceneManager instance;
	
	
	// Properties
	//-----------
	static public SsSceneManager Instance
	{
		get { return(instance); }
	}
	
	
	
	// Methods
	//--------
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
	/// Get a random field scene.
	/// </summary>
	/// <returns>The random field.</returns>
	public SsFieldSceneResource GetRandomField(string excludeField1 = null, string excludeField2 = null)
	{
		if ((fieldScenes == null) || (fieldScenes.Length <= 0))
		{
			return (null);
		}
		
		int i, retry;
		SsFieldSceneResource field;
		
		retry = 0;
		while (retry < 10)
		{
			field = fieldScenes[Random.Range(0, fieldScenes.Length)];
			if ((field != null) && 
			    ((string.IsNullOrEmpty(excludeField1)) || (field.id != excludeField1)) && 
			    ((string.IsNullOrEmpty(excludeField2)) || (field.id != excludeField2)))
			{
				return (field);
			}
			retry ++;
		}
		
		// Use the first available one
		for (i = 0; i < fieldScenes.Length; i++)
		{
			field = fieldScenes[i];
			if ((field != null) && 
			    ((string.IsNullOrEmpty(excludeField1)) || (field.id != excludeField1)) && 
			    ((string.IsNullOrEmpty(excludeField2)) || (field.id != excludeField2)))
			{
				return (field);
			}
		}
		
		return (null);
	}
	
	
	/// <summary>
	/// Get the field, based on its ID.
	/// </summary>
	/// <returns>The field.</returns>
	/// <param name="id">Identifier.</param>
	public SsFieldSceneResource GetField(string id, bool returnRandomIfNotFound = true)
	{
		if ((fieldScenes == null) || (fieldScenes.Length <= 0))
		{
			return (null);
		}

		if (string.IsNullOrEmpty(id))
		{
			if (returnRandomIfNotFound)
			{
				return (GetRandomField());
			}
			return (null);
		}
		
		int i;
		SsFieldSceneResource field;
		string idLower = id.ToLower();
		
		for (i = 0; i < fieldScenes.Length; i++)
		{
			field = fieldScenes[i];
			if ((field != null) && 
			    (string.IsNullOrEmpty(field.id) == false) && 
			    (field.id.ToLower() == idLower))
			{
				return (field);
			}
		}

		if (returnRandomIfNotFound)
		{
			return (GetRandomField());
		}

		return (null);
	}


	/// <summary>
	/// Gets the field at the specified index.
	/// </summary>
	/// <returns>The field.</returns>
	/// <param name="index">Index.</param>
	public SsFieldSceneResource GetField(int index)
	{
		if ((fieldScenes == null) || (fieldScenes.Length <= 0) || 
		    (index < 0) || (index >= fieldScenes.Length))
		{
			return (null);
		}
		return (fieldScenes[index]);
	}


	/// <summary>
	/// Get the field that is currently loaded. Based on the loaded scene name.
	/// </summary>
	/// <returns>The loaded field.</returns>
	public SsFieldSceneResource GetLoadedField()
	{
		if ((fieldScenes == null) || (fieldScenes.Length <= 0))
		{
			return (null);
		}

		int i;
		SsFieldSceneResource field;

		for (i = 0; i < fieldScenes.Length; i++)
		{
			field = fieldScenes[i];
			if ((field != null) && 
			    (string.IsNullOrEmpty(field.sceneName) == false) && 
			    (field.sceneName == GeUtils.GetLoadedLevelName()))
			{
				return (field);
			}
		}

		return (null);
	}
}
