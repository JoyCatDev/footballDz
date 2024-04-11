using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// Formation manager.
/// </summary>
public class SsFormationManager : MonoBehaviour {

	// Const/Static
	//-------------
	// Field image size in the editor. Used to find the player locations on the 3D field.
	public const float halfFieldImageWidth = 24.5f;
	public const float halfFieldImageHeight = 16.0f;


	// Public
	//-------
	[Tooltip("Formations. (The list is automatically updated in the editor.)")]
	public SsFormation[] formations;


	// Private
	//--------
	static private SsFormationManager instance;


	// Properties
	//-----------
	static public SsFormationManager Instance
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

		// In case normalised positions were Not set
		int i;
		SsFormation formation;
		if ((formations != null) && (formations.Length > 0))
		{
			for (i = 0; i < formations.Length; i++)
			{
				formation = formations[i];
				if (formation != null)
				{
					formation.UpdateNormalisedPositions();
				}
			}
		}
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Call this after field scene is loaded, before match starts, and before players are positioned.
	/// </summary>
	public void OnPreMatchStart()
	{
		int i;
		SsFormation formation;

		if ((formations != null) && (formations.Length > 0))
		{
			for (i = 0; i < formations.Length; i++)
			{
				formation = formations[i];
				if (formation != null)
				{
					formation.OnPreMatchStart();
				}
			}
		}
	}

	
#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		// Update the formations, in the editor
		UpdateInEditor();
	}


	/// <summary>
	/// Update the formations, in the editor.
	/// </summary>
	/// <returns>The in editor.</returns>
	void UpdateInEditor()
	{
		formations = gameObject.GetComponentsInChildren<SsFormation>(true);
		
		// Check for duplicate IDs
		if ((formations != null) && (formations.Length > 1))
		{
			int i, n;
			SsFormation f, f2;
			
			for (i = 0; i < formations.Length; i++)
			{
				f = formations[i];
				if (f == null)
				{
					continue;
				}
				
				if (string.IsNullOrEmpty(f.id))
				{
					Debug.LogError("Formation has an empty Id.     [" + f.name + "]");
					continue;
				}
				
				for (n = 0; n < formations.Length; n++)
				{
					f2 = formations[n];
					if ((f != f2) && (f2 != null) && 
					    (string.IsNullOrEmpty(f2.id) == false) && 
					    (f.id.ToLower() == f2.id.ToLower()))
					{
						Debug.LogError("Two formations have the same Id: " + f.id + 
						               "     [" + f.name + "],  [" + f2.name + "]");
						break;
					}
				}
			}
		}
	}
#endif //UNITY_EDITOR



	/// <summary>
	/// Get the formation with the specified Id.
	/// </summary>
	/// <returns>The formation.</returns>
	/// <param name="id">Identifier.</param>
	public SsFormation GetFormation(string id)
	{
		if ((formations == null) || (formations.Length <= 0) || 
		    (string.IsNullOrEmpty(id)))
		{
			return (null);
		}

		string idLower = id.ToLower();
		int i;
		SsFormation formation;

		for (i = 0; i < formations.Length; i++)
		{
			formation = formations[i];
			if ((formation != null) && (string.IsNullOrEmpty(formation.id) == false) &&
			    (formation.id.ToLower() == idLower))
			{
				return (formation);
			}
		}

		return (null);
	}


	/// <summary>
	/// Get a random formation.
	/// </summary>
	/// <returns>The random formation.</returns>
	/// <param name="excludeId">Id of formation to exclude in the search.</param>
	public SsFormation GetRandomFormation(string excludeId)
	{
		if ((formations == null) || (formations.Length <= 0))
		{
			return (null);
		}
		
		int i, retry;
		SsFormation formation;
		string excludeIdLower = (string.IsNullOrEmpty(excludeId) == false) ? excludeId.ToLower() : null;
		
		retry = 0;
		while (retry < 10)
		{
			formation = formations[Random.Range(0, formations.Length)];
			if ((formation != null) && 
			    (string.IsNullOrEmpty(formation.id) == false) && 
			    (formation.id.ToLower() != excludeIdLower))
			{
				return (formation);
			}
			retry ++;
		}
		
		// Use the first available one
		for (i = 0; i < formations.Length; i++)
		{
			formation = formations[i];
			if ((formation != null) && 
			    (string.IsNullOrEmpty(formation.id) == false) && 
			    (formation.id.ToLower() != excludeIdLower))
			{
				return (formation);
			}
		}
		
		return (null);
	}
}
