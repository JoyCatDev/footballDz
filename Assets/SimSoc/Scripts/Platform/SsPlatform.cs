using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Platform specific stuff.
/// </summary>
public class SsPlatform : MonoBehaviour {

	// Enums
	//------
	// Platform groups. Allows grouping various runtime platforms into groups.
	public enum platformGroups
	{
		unknown = 0,

		editor,
		desktop,
		mobile,
		web,
		console,

		windows,
		osx,
		linux,
		wsa,

		android,
		ios,
		wp8,

		ps,
		psp,
		samsungTv,
		tizen,
		wiiU,
		xbox,
		
		// REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.
		
		maxGroups
	}


	// Classes
	//--------
	// Platform group properties
	[System.Serializable]
	public class SsPlatformGroupsProperties
	{
		[Tooltip("The group.")]
		public platformGroups group;
		[Tooltip("The runtime platforms that belong to this group.")]
		public RuntimePlatform[] platforms;
	}


	// Input allowed per platform/platform group
	[System.Serializable]
	public class SsPlatformInput
	{
		[Tooltip("Platform groups for which the player inputs are allowed.")]
		public platformGroups[] groups;

		[Tooltip("Runtime platforms for which the player inputs are allowed.\n" + 
		         "(Optional. Allows adding specific platforms that are not in any of the groups.)")]
		public RuntimePlatform[] platforms;
		
		[Tooltip("Player inputs allowed on the platforms/platform groups. (The first one is the default one.)")]
		public SsInput.inputTypes[] inputTypes;
	}


	// Methods
	//--------
	/// <summary>
	/// Get the current platform. 
	/// NOTE: If SsSettings.simulateRuntimePlatform has been set then it will return the simulated platform.
	/// </summary>
	/// <returns>The current platform.</returns>
	static public RuntimePlatform GetCurrentPlatform()
	{
		RuntimePlatform platform = Application.platform;
		
#if UNITY_EDITOR
		if ((SsSettings.Instance != null) && (SsSettings.Instance.simulateRuntimePlatform))
		{
			platform = SsSettings.Instance.runtimePlatformToSimulate;
		}
#endif //UNITY_EDITOR
		
		return (platform);
	}


	/// <summary>
	/// Get all the groups that contain the current platform.
	/// NOTE: If SsSettings.simulateRuntimePlatform has been set then it will use the simulated platform.
	/// </summary>
	/// <returns>The current platform group.</returns>
	static public platformGroups[] GetCurrentPlatformGroups()
	{
		if ((SsSettings.Instance != null) && (SsSettings.Instance.platformGroups != null) && 
		    (SsSettings.Instance.platformGroups.Length > 0))
		{
			RuntimePlatform platform = GetCurrentPlatform();
			SsPlatformGroupsProperties props;
			int i, n;
			List<platformGroups> list = new List<platformGroups>(SsSettings.Instance.platformGroups.Length);

			for (i = 0; i < SsSettings.Instance.platformGroups.Length; i++)
			{
				props = SsSettings.Instance.platformGroups[i];
				if ((props != null) && (props.platforms != null) && (props.platforms.Length > 0))
				{
					for (n = 0; n < props.platforms.Length; n++)
					{
						if (props.platforms[n] == platform)
						{
							list.Add(props.group);
							break;
						}
					}
				}
			}

			if ((list != null) && (list.Count > 0))
			{
				return (list.ToArray());
			}
		}

		return (null);
	}

}
