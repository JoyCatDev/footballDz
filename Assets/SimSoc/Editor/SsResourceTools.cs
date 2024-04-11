using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Resource tools.
/// </summary>
public class SsResourceTools : SsBaseTools {

	// Private
	//--------
	static private bool busyScanning = false;


	// Methods
	//--------
	[MenuItem(menuPath + "Scan Resources", false, menuIndex + 100)]
	static void ScanResourcesMenu()
	{
		ScanResources();
	}


	[MenuItem(menuPath + "Enable Auto Scan", false, menuIndex + 101)]
	static void EnableAutoScanResourcesMenu()
	{
		if (SsEditorSettings.AutoScanResources == false)
		{
			SsEditorSettings.ResourceChanged = true;
		}

		SsEditorSettings.AutoScanResources = true;

		EditorUtility.DisplayDialog("Auto scan resources", 
		                            "Auto scan has been ENABLED.",
		                            "OK");
	}


	[MenuItem(menuPath + "Disable Auto Scan", false, menuIndex + 102)]
	static void DisableAutoScanResourcesMenu()
	{
		if (SsEditorSettings.AutoScanResources == false)
		{
			SsEditorSettings.ResourceChanged = true;
		}
		
		SsEditorSettings.AutoScanResources = false;
		
		EditorUtility.DisplayDialog("Auto scan resources", 
		                            "Auto scan has been DISABLED.\n" + 
		                            "You have to manually run Scan Resources to scan for new resources.", 
		                            "OK");
	}


	/// <summary>
	/// Creates the temp Resource Manager prefab. First tries to load the existing prefab, then creates new one if Not found.
	/// </summary>
	/// <returns>The temp prefab.</returns>
	private static SsResourceManager CreateTempPrefab()
	{
		GameObject go;
		SsResourceManager resourceManager;
		string tempName = "TEMP - DELETE ME";

		// Create a new one
		go = new GameObject(tempName);
		if (go != null)
		{
			resourceManager = go.AddComponent<SsResourceManager>();
			if (resourceManager == null)
			{
				DestroyImmediate(go);
				return (null);
			}
			return (resourceManager);
		}
		return (null);
	}
	
	
	/// <summary>
	/// Deletes the temp prefab.
	/// </summary>
	/// <returns>The temp prefab.</returns>
	/// <param name="prefab">Prefab.</param>
	private static void DeleteTempPrefab(SsResourceManager prefab)
	{
		if (prefab != null)
		{
			DestroyImmediate(prefab.gameObject);
		}
	}


	/// <summary>
	/// Scans the resources.
	/// </summary>
	/// <returns>The resources internal.</returns>
	static public void ScanResources()
	{
		if (busyScanning)
		{
			return;
		}

		busyScanning = true;

		try
		{
			ScanResourcesInternal();
		}
		catch
		{
		}
		
		busyScanning = false;
	}


	/// <summary>
	/// Scans the resources internal.
	/// </summary>
	/// <returns>The resources internal.</returns>
	static private void ScanResourcesInternal()
	{
		SsResourceManager resourceManager = CreateTempPrefab();
		if (resourceManager != null)
		{
			try
			{
				ScanResourcesNow(resourceManager);
			}
			catch (System.Exception e)
			{
				Debug.LogError("ERROR: Failed to create resource manager: " + SsResourceManager.resourceManagerEditorPath);
				Debug.LogError(e);
			}
			
			DeleteTempPrefab(resourceManager);
		}
		
		Resources.UnloadUnusedAssets();
	}


	/// <summary>
	/// Scans the resources now.
	/// </summary>
	/// <returns>The resources now.</returns>
	/// <param name="resourceManager">Resource manager.</param>
	static void ScanResourcesNow(SsResourceManager resourceManager)
	{
		if (resourceManager == null)
		{
			return;
		}

		int foundCount, i;
		SsBall[] balls = Resources.LoadAll<SsBall>("");
		SsTeam[] teams = Resources.LoadAll<SsTeam>("");
		SsPlayer[] players = Resources.LoadAll<SsPlayer>("");
		SsBall ball;
		SsTeam team;
		SsPlayer player;

		// Clear arrays
		resourceManager.balls = new SsResourceManager.SsBallResource[0];
		resourceManager.teams = new SsResourceManager.SsTeamResource[0];
		resourceManager.players = new SsResourceManager.SsPlayerResource[0];

		foundCount = 0;

		if ((balls != null) && (balls.Length > 0))
		{
			resourceManager.balls = new SsResourceManager.SsBallResource[balls.Length];
			for (i = 0; i < balls.Length; i++)
			{
				ball = balls[i];
				if (ball != null)
				{
					foundCount ++;
					resourceManager.balls[i] = new SsResourceManager.SsBallResource();

					// Copy data
					resourceManager.balls[i].id = ball.id;
					resourceManager.balls[i].displayName = ball.displayName;

					resourceManager.balls[i].path = RemoveResourcesPrefixAndExtentionFromPath(AssetDatabase.GetAssetPath(ball));
				}
			}
		}

		if ((teams != null) && (teams.Length > 0))
		{
			resourceManager.teams = new SsResourceManager.SsTeamResource[teams.Length];
			for (i = 0; i < teams.Length; i++)
			{
				team = teams[i];
				if (team != null)
				{
					foundCount ++;
					resourceManager.teams[i] = new SsResourceManager.SsTeamResource();

					// Copy data
					resourceManager.teams[i].id = team.id;
					resourceManager.teams[i].teamName = team.teamName;
					resourceManager.teams[i].preferredName = team.preferredName;
					resourceManager.teams[i].shortName = team.shortName;
					resourceManager.teams[i].name3Letters = team.name3Letters;
					resourceManager.teams[i].homeFieldId = team.homeFieldId;

					resourceManager.teams[i].path = RemoveResourcesPrefixAndExtentionFromPath(AssetDatabase.GetAssetPath(team));
				}
			}
		}

		if ((players != null) && (players.Length > 0))
		{
			resourceManager.players = new SsResourceManager.SsPlayerResource[players.Length];
			for (i = 0; i < players.Length; i++)
			{
				player = players[i];
				if (player != null)
				{
					foundCount ++;
					resourceManager.players[i] = new SsResourceManager.SsPlayerResource();

					// Copy data
					resourceManager.players[i].id = player.id;
					resourceManager.players[i].firstName = player.firstName;
					resourceManager.players[i].lastName = player.lastName;
					resourceManager.players[i].preferredName = player.preferredName;
					resourceManager.players[i].shortName = player.shortName;
					resourceManager.players[i].name3Letters = player.name3Letters;
					resourceManager.players[i].gender = player.gender;
					resourceManager.players[i].age = player.age;
					resourceManager.players[i].jerseyNumber = player.jerseyNumber;
					resourceManager.players[i].headHeight = player.headHeight;

					resourceManager.players[i].path = RemoveResourcesPrefixAndExtentionFromPath(AssetDatabase.GetAssetPath(player));
				}
			}
		}


		Debug.Log("Found resources:  " + foundCount + 
		          "     balls x " + ((balls != null) ? balls.Length.ToString() : "0") + 
		          ",    teams x " + ((teams != null) ? teams.Length.ToString() : "0") + 
		          ",    players x " + ((players != null) ? players.Length.ToString() : "0") + 
		          "            " + System.DateTime.Now);


		// Finally, create the prefab
		if ((resourceManager != null) && (foundCount > 0))
		{
			PrefabUtility.SaveAsPrefabAsset(resourceManager.gameObject, SsResourceManager.resourceManagerEditorPath,
				out var success);
			if (!success)
			{
				Debug.LogError("ERROR: Failed to create resource manager: " + SsResourceManager.resourceManagerEditorPath);
			}
		}
	}


	/// <summary>
	/// Removes the resources prefix and extention from path.
	/// </summary>
	/// <returns>The resources prefix and extention from path.</returns>
	/// <param name="path">Path.</param>
	static public string RemoveResourcesPrefixAndExtentionFromPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return (path);
		}

		string file = path;
		int i;

		i = path.IndexOf("/Resources/");
		if (i >= 0)
		{
			file = path.Substring(i + "/Resources/".Length);
		}

		return (System.IO.Path.ChangeExtension(file, null));
	}
}
