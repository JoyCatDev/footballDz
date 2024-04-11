using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif //UNITY_EDITOR


/// <summary>
/// Editor settings. Saves/loads settings to the EditorPrefs.
/// NOTE: The script is Not in the "Editor" folder, because we need to access it from ball, team and player scripts.
/// </summary>
public class SsEditorSettings
{
	// Properties
	//-----------
#if UNITY_EDITOR

	static public bool AutoScanResources
	{
		get
		{
			if (EditorPrefs.HasKey("SsAutoScan"))
			{
				return (EditorPrefs.GetBool("SsAutoScan"));
			}
			return (true);
		}
		set
		{
			EditorPrefs.SetBool("SsAutoScan", value);
		}
	}


	static public bool ResourceChanged
	{
		get
		{
			if (EditorPrefs.HasKey("SsResChanged"))
			{
				return (EditorPrefs.GetBool("SsResChanged"));
			}
			return (true);
		}
		set
		{
			EditorPrefs.SetBool("SsResChanged", value);
		}
	}
#endif //UNITY_EDITOR

}
