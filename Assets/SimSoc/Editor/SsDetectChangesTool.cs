using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Detect when assets change.
/// </summary>
public class SsDetectChangesTool : AssetPostprocessor  
{
	/// <summary>
	/// Raises the postprocess all assets event.
	/// </summary>
	/// <param name="importedAssets">Imported assets.</param>
	/// <param name="deletedAssets">Deleted assets.</param>
	/// <param name="movedAssets">Moved assets.</param>
	/// <param name="movedFromAssetPaths">Moved from asset paths.</param>
	static void OnPostprocessAllAssets (
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths) 
	{
		if (SsEditorSettings.AutoScanResources == false)
		{
			return;
		}

		if ((EditorApplication.isCompiling) || (EditorApplication.isPaused) || 
		    (EditorApplication.isPlaying) || (EditorApplication.isPlayingOrWillChangePlaymode))
		{
			return;
		}

		bool scanResources = SsEditorSettings.ResourceChanged;
		int i, n;
		string[] assets;


		if (scanResources == false)
		{
			for (n = 0; n < 4; n++)
			{
				if (n == 0)
				{
					assets = importedAssets;
				}
				else if (n == 1)
				{
					assets = deletedAssets;
				}
				else if (n == 2)
				{
					assets = movedAssets;
				}
				else
				{
					assets = movedFromAssetPaths;
				}

				if ((scanResources == false) && (assets != null) && (assets.Length > 0))
				{
					for (i = 0; i < assets.Length; i++)
					{
						if (IsValidResource(assets[i]))
						{
							scanResources = true;
							break;
						}
					}
				}

				if (scanResources)
				{
					break;
				}
			}
		}


		if (scanResources)
		{
			SsResourceTools.ScanResources();
		}
	}


	/// <summary>
	/// Test if the asset is in a Resources folder and it is a player, team or ball.
	/// </summary>
	/// <returns><c>true</c> if is valid resource the specified path; otherwise, <c>false</c>.</returns>
	/// <param name="path">Path.</param>
	static bool IsValidResource(string path)
	{
		string resourcePrefix = "/Resources/";

		if ((string.IsNullOrEmpty(path) == false) && 
		    (path.IndexOf(resourcePrefix) >= 0))
		{
			string shortPath = SsResourceTools.RemoveResourcesPrefixAndExtentionFromPath(path);

			if ((Resources.Load<SsPlayer>(shortPath) != null) || 
			    (Resources.Load<SsTeam>(shortPath) != null) || 
			    (Resources.Load<SsBall>(shortPath) != null))
			{
				return (true);
			}
		}

		return (false);
	}
}