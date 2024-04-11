using UnityEngine;
using System.Collections;

/// <summary>
/// Enable all child menus. Attach it to a Canvas if you want all its child menus to be enabled (e.g. when scene is loaded).
/// Usefull if you enable/disable menus while editing, then forget to enable them again when done editing. This will enable them.
/// </summary>
public class SsEnableMenus : MonoBehaviour {

	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		SsBaseMenu[] menus = gameObject.GetComponentsInChildren<SsBaseMenu>(true);
		if ((menus != null) && (menus.Length > 0))
		{
			int i;
			SsBaseMenu menu;
			for (i = 0; i < menus.Length; i++)
			{
				menu = menus[i];
				if ((menu != null) && (menu.gameObject.activeInHierarchy == false))
				{
					menu.gameObject.SetActive(true);
				}
			}
		}
	}
}
