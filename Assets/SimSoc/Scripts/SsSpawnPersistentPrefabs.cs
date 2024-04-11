using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawn persistent prefabs. These prefabs exist for the duration of the game.
/// The GameObject that contains this class should be placed in each scene, especially the first scene that is loaded.
/// </summary>
public class SsSpawnPersistentPrefabs : MonoBehaviour {
	
	// Public
	//-------
	public GameObject[] prefabs;
	
	
	// Private
	//--------
	static private SsSpawnPersistentPrefabs instance;
	static private bool didSpawn = false;
	static private GameObject persistentlPrefabs = null;
	static List<string> spawnedPrefabsList = new List<string>();
	

	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
		SpawnPrefabs();
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Spawns the prefabs. Call this from any game object's Awake method that accesses the prefabs in Awake, to ensure they 
	/// are spawned before accessing them.
	/// </summary>
	/// <returns>The prefabs.</returns>
	static public void SpawnPrefabs()
	{
		SsSpawnPersistentPrefabs go = instance;

		if (go == null)
		{
			go = FindObjectOfType<SsSpawnPersistentPrefabs>();
		}

		if (go != null)
		{
			go.SpawnPrefabsNow();
		}
	}


	/// <summary>
	/// Spawns the prefabs now.
	/// </summary>
	/// <returns>The prefabs now.</returns>
	private void SpawnPrefabsNow()
	{
		if (didSpawn)
		{
			return;
		}
		didSpawn = true;

		if ((prefabs != null) && (prefabs.Length > 0))
		{
			if (persistentlPrefabs == null)
			{
				persistentlPrefabs = new GameObject("PersistentPrefabs");
				if (persistentlPrefabs != null)
				{
					persistentlPrefabs.transform.position = Vector3.zero;
					persistentlPrefabs.transform.rotation = Quaternion.identity;
					persistentlPrefabs.transform.localPosition = Vector3.zero;
					persistentlPrefabs.transform.localScale = Vector3.one;
					GameObject.DontDestroyOnLoad(persistentlPrefabs);
				}
			}
			
			foreach (GameObject prefab in prefabs)
			{
				if (prefab != null)
				{
					// Test if the prefab already exists in the list
					bool spawn = true;
					if ((spawnedPrefabsList != null) || (spawnedPrefabsList.Count > 0))
					{
						foreach (string s in spawnedPrefabsList)
						{
							if (s == prefab.name)
							{
								spawn = false;
								break;
							}
						}
					}
					
					if (spawn)
					{
						GameObject newPrefab = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
						if (newPrefab != null)
						{
							GameObject.DontDestroyOnLoad(newPrefab);
							if (persistentlPrefabs != null)
							{
								Canvas canvas = newPrefab.GetComponentInChildren<Canvas>();
								RectTransform rectTransform = newPrefab.GetComponentInChildren<RectTransform>();
								if ((canvas != null) || (rectTransform != null))
								{
									newPrefab.transform.SetParent(persistentlPrefabs.transform, true);
								}
								else
								{
									newPrefab.transform.parent = persistentlPrefabs.transform;
								}
							}
							if (spawnedPrefabsList != null)
							{
								spawnedPrefabsList.Add(prefab.name);
							}
						}
					}
				}
			}
		}
		
	}

}
