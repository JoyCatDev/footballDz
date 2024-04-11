using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Misc tools.
/// </summary>
public class SsMiscTools : SsBaseTools {

	[MenuItem(menuPath + "Add Field Properties", false, menuIndex + 200)]
	static void AddFieldPropertiesMenu()
	{
		AddFieldProperties();
	}


	/// <summary>
	/// Add field properties to the scene.
	/// </summary>
	/// <returns>The field properties.</returns>
	static void AddFieldProperties()
	{
		// Test if scene already has field properties.
		SsFieldProperties props = (SsFieldProperties)FindObjectOfType(typeof(SsFieldProperties));
		if (props != null)
		{
			EditorUtility.DisplayDialog("Already exists", 
			                            "The scene already contains a Field Properties object.", 
			                            "OK");
			return;
		}

		GameObject go, left, right;
		BoxCollider bc;
		SphereCollider sc;

		go = new GameObject("Field Properties");
		props = go.AddComponent<SsFieldProperties>();


		// Play area
		go = new GameObject("play area");
		go.transform.parent = props.transform;
		bc = go.AddComponent<BoxCollider>();
		bc.size = new Vector3(102.96f, 1.0f, 66.93f);
		props.playArea = bc;


		// Centre
		go = new GameObject("centre");
		go.transform.parent = props.transform;
		sc = go.AddComponent<SphereCollider>();
		sc.radius = 9.14f;
		props.centre = sc;


		// Left
		left = new GameObject("left");
		left.transform.parent = props.transform;
		{
			// Left goal posts
			go = new GameObject("left goal posts");
			go.transform.parent = left.transform;
			go.transform.localPosition = new Vector3(-52.98f, 1.22f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(3.0f, 2.44f, 7.32f);
			props.leftGoalPosts = bc;

			// Left goal area
			go = new GameObject("left goal area");
			go.transform.parent = left.transform;
			go.transform.localPosition = new Vector3(-48.78f, 0.0f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(5.49f, 1.2f, 18.29f);
			props.leftGoalArea = bc;

			// Left penalty area
			go = new GameObject("left penalty area");
			go.transform.parent = left.transform;
			go.transform.localPosition = new Vector3(-43.37f, 0.0f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(16.46f, 1.0f, 40.23f);
			props.leftPenaltyArea = bc;

			// Left penalty mark
			go = new GameObject("left penalty mark");
			go.transform.parent = left.transform;
			go.transform.localPosition = new Vector3(-40.53f, 0.0f, 0.0f);
			props.leftPenaltyMark = go;
		}


		// Right
		right = new GameObject("right");
		right.transform.parent = props.transform;
		{
			// Right goal posts
			go = new GameObject("right goal posts");
			go.transform.parent = right.transform;
			go.transform.localPosition = new Vector3(52.98f, 1.22f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(3.0f, 2.44f, 7.32f);
			props.rightGoalPosts = bc;
			
			// Right goal area
			go = new GameObject("right goal area");
			go.transform.parent = right.transform;
			go.transform.localPosition = new Vector3(48.78f, 0.0f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(5.49f, 1.2f, 18.29f);
			props.rightGoalArea = bc;
			
			// Right penalty area
			go = new GameObject("right penalty area");
			go.transform.parent = right.transform;
			go.transform.localPosition = new Vector3(43.37f, 0.0f, 0.0f);
			bc = go.AddComponent<BoxCollider>();
			bc.size = new Vector3(16.46f, 1.0f, 40.23f);
			props.rightPenaltyArea = bc;
			
			// Right penalty mark
			go = new GameObject("right penalty mark");
			go.transform.parent = right.transform;
			go.transform.localPosition = new Vector3(40.53f, 0.0f, 0.0f);
			props.rightPenaltyMark = go;
		}

		// Select the object
		Selection.activeGameObject = props.gameObject;

		// Register Undo to delete the object
		Undo.RegisterCreatedObjectUndo(props.gameObject, "Create Object");
	}


	[MenuItem(menuPath + "Setup Kit", false, menuIndex + 300)]
	static void SetupKitMenu()
	{
		SetupBuildSettings();
		SetupLayers();
	}


	/// <summary>
	/// Adds the scenes to the build settings.
	/// </summary>
	/// <returns>The build settings.</returns>
	static void SetupBuildSettings()
	{
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		EditorBuildSettingsScene scene;
		int i, n, count;
		List<string> listToAdd = new List<string>();
		List<EditorBuildSettingsScene> finalList = new List<EditorBuildSettingsScene>();
		EditorBuildSettingsScene newScene;
		string[] scenesToAdd = {
			"Assets/SimSoc/Scenes/Startup.unity",
			"Assets/SimSoc/Scenes/UI/LoadingScreen.unity",					
			"Assets/SimSoc/Scenes/UI/MainMenus.unity",
			"Assets/SimSoc/Scenes/UI/SplashScreen.unity",
			"Assets/SimSoc/Scenes/Fields/BlueField.unity",
			"Assets/SimSoc/Scenes/Fields/RedField.unity"};

		count = 0;


		// Build the initial list to add
		for (i = 0; i < scenesToAdd.Length; i++)
		{
			listToAdd.Add(scenesToAdd[i]);
		}

		if ((scenes != null) && (scenes.Length > 0))
		{
			for (i = 0; i < scenes.Length; i++)
			{
				scene = scenes[i];
				if (scene != null)
				{
					finalList.Add(scene);

					// Check if scene has already been added
					for (n = 0; n < listToAdd.Count; n++)
					{
						if (scene.path == listToAdd[n])
						{
							listToAdd.RemoveAt(n);
							break;
						}
					}
				}
			}
		}


		// Add the scenes
		if (listToAdd.Count > 0)
		{
			for (i = 0; i < listToAdd.Count; i++)
			{
				newScene = new EditorBuildSettingsScene(listToAdd[i], true);
				finalList.Add(newScene);
				count ++;
			}
		}

		if ((count > 0) && (finalList.Count > 0))
		{
			EditorBuildSettings.scenes = finalList.ToArray();
		}

		Debug.Log("Number of scenes added to Build Settings: " + count);
	}


	/// <summary>
	/// Setup the layers.
	/// </summary>
	/// <returns>The layers.</returns>
	static void SetupLayers()
	{
		SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		SerializedProperty layersProp = (manager != null) ? manager.FindProperty("layers") : null;
		int i, n, count;
		bool found;
		SerializedProperty property, emptySlot;
		string[] layersToAdd = {
			"World",
			"Players",
			"Ball"};

		if ((manager == null) || (layersProp == null))
		{
			Debug.LogError("ERROR: Failed to add the layers.");
			return;
		}

		count = 0;


		// Add the layers
		for (i = 0; i < layersToAdd.Length; i++)
		{
			found = false;
			for (n = 0; n <= 31; n++)
			{
				property = layersProp.GetArrayElementAtIndex(n);
				if ((property != null) && (property.stringValue == layersToAdd[i]))
				{
					found = true;
					break;
				}
			}

			if (found == false)
			{
				if (!found)
				{
					emptySlot = null;
					for (n = 8; n <= 31; n++)
					{
						property = layersProp.GetArrayElementAtIndex(n);
						if ((property != null) && (string.IsNullOrEmpty(property.stringValue)))
						{
							emptySlot = property;
							break;
						}
					}
					
					if (emptySlot != null)
					{
						emptySlot.stringValue = layersToAdd[i];
						count ++;
					}
					else
					{
						Debug.LogError("ERROR: Could not add the layer: " + layersToAdd[i]);
					}
				}
			}
		}

		if (count > 0)
		{
			manager.ApplyModifiedProperties();
		}

		Debug.Log("Number of layers added: " + count);
	}
}
