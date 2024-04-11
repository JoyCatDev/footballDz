using UnityEngine;
using System.Collections;

/// <summary>
/// Disables a game object on specified platforms/platform groups (e.g. you might have a game object you want disabled on mobile devices).
/// </summary>
public class SsPlatformDisabler : MonoBehaviour {

	// Public
	//-------
	[Header("Demo")]
	[Tooltip("Disable this UI element for the demo build.")]
	public bool disableForDemoBuild;
	
	[Tooltip("Disable this UI element for the non-demo build.")]
	public bool disableForNonDemoBuild;
	
	
	[Header("Platform groups")]
	[Tooltip("Disable this UI element for these platform groups. The groups are specified on the Global Settings prefab.")]
	public SsPlatform.platformGroups[] disableForPlatformGroups;
	
	
	[Header("Specific platforms")]
	[Tooltip("Disable this UI element for these specific platforms.")]
	public RuntimePlatform[] disableForPlatforms;
	
	
	
	// Methods
	//--------
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable()
	{
		SsSpawnPersistentPrefabs.SpawnPrefabs();
		
		if (SsSettings.Instance == null)
		{
			return;
		}
		
		if ((disableForDemoBuild) && (SsSettings.Instance.demoBuild))
		{
			gameObject.SetActive(false);
			return;
		}
		
		if ((disableForNonDemoBuild) && (SsSettings.Instance.demoBuild == false))
		{
			gameObject.SetActive(false);
			return;
		}
		
		SsPlatform.platformGroups[] groups = SsPlatform.GetCurrentPlatformGroups();
		RuntimePlatform platform = SsPlatform.GetCurrentPlatform();
		int i, n;
		
		if ((disableForPlatformGroups != null) && (disableForPlatformGroups.Length > 0) && 
		    (groups != null) && (groups.Length > 0))
		{
			for (i = 0; i < disableForPlatformGroups.Length; i++)
			{
				for (n = 0; n < groups.Length; n++)
				{
					if (disableForPlatformGroups[i] == groups[n])
					{
						gameObject.SetActive(false);
						return;
					}
				}
			}
		}
		
		if ((disableForPlatforms != null) && (disableForPlatforms.Length > 0))
		{
			for (i = 0; i < disableForPlatforms.Length; i++)
			{
				if (disableForPlatforms[i] == platform)
				{
					gameObject.SetActive(false);
					return;
				}
			}
		}
	}
}
