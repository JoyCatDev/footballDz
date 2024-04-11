using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// This is a helper script to change the image of all the buttons in the scene.
/// Make sure all the menus, which you want to change, in the scene are active.
/// Create a new, temporary, game object in the scene and attach this component.
/// Set the reference to the image to use. It should immediately change the images of all the buttons.
/// Check if the menu buttons have changed. If the menus are prefabs then apply the changes to the prefabs.
/// Delete the temporary game object and save the scene.
/// </summary>
public class SsChangeButtonImages : MonoBehaviour {

	// Public
	//-------
	[Tooltip("Image to use for replacing all the button images.")]
	public Sprite image;

	[Space(10)]
	[Tooltip("Must the colour also be changed?")]
	public bool changeColour = true;
	[Tooltip("New colour.")]
	public Color colour = Color.white;

	[Space(10)]
	[Tooltip("Names of the sprite images to change. It will only change buttons that use these sprites.")]
	public string[] spritesToChange = {"UISprite", "Background"};
	

#if UNITY_EDITOR
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		if (image == null)
		{
			return;
		}

		int i, n;
		UnityEngine.UI.Button button;
		UnityEngine.UI.Button[] buttons = FindObjectsOfType(typeof(UnityEngine.UI.Button)) as UnityEngine.UI.Button[];
		if ((buttons != null) && (buttons.Length > 0))
		{
			for (i = 0; i < buttons.Length; i++)
			{
				button = buttons[i];
				if ((button != null) && (button.image != null) && 
				    ((button.image.sprite != image) || 
				 	 ((changeColour) && (button.image.color != colour))))
				{
					for (n = 0; n < spritesToChange.Length; n++)
					{
						if ((button.image.sprite.name == spritesToChange[n]))
						{
							button.image.sprite = image;
							if (changeColour)
							{
								button.image.color = colour;
							}
							break;
						}
					}
				}
			}
		}
	}
#endif //UNITY_EDITOR
}
