using UnityEngine;
using System.Collections;

/// <summary>
/// References to common resources used by the menus, such as the team logos.
/// NOTE: This is mainly for menus outside the match. Avoid using it in the match, because we do Not want all the team images
/// to be loaded into memory during a match.
/// </summary>
public class SsMenuResources : MonoBehaviour {

	// Public
	//-------
	[Header("Link images to teams")]
	[Tooltip("Large team logos (e.g. used on tournament overview screen)")]
	public SsUiManager.SsSpriteToResource[] teamLogosBig;
	[Tooltip("Small team logos (e.g. used on tournament tree screen)")]
	public SsUiManager.SsSpriteToResource[] teamLogosSmall;


	// Private
	//--------
	static private SsMenuResources instance;


	// Properties
	//-----------
	static public SsMenuResources Instance
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
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
	}


	/// <summary>
	/// Get the big team logo for the specified sprite.
	/// </summary>
	/// <returns>The big team logo.</returns>
	/// <param name="id">Identifier.</param>
	public Sprite GetBigTeamLogo(string id)
	{
		return (SsUiManager.SsSpriteToResource.GetSpriteWithId(id, teamLogosBig));
	}


	/// <summary>
	/// Get the small team logo for the specified sprite.
	/// </summary>
	/// <returns>The small team logo.</returns>
	/// <param name="id">Identifier.</param>
	public Sprite GetSmallTeamLogo(string id)
	{
		return (SsUiManager.SsSpriteToResource.GetSpriteWithId(id, teamLogosSmall));
	}
}
