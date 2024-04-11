using UnityEngine;
using System.Collections;

/// <summary>
/// Formation UI item.
/// </summary>
public class SsFormationUiItem : MonoBehaviour {

	// Public
	//-------
	[Header("Prefabs")]
	[Tooltip("Goalkeeper icon prefab to clone.")]
	public UnityEngine.UI.Image goalkeeperIconPrefab;

	[Tooltip("Defender icon prefab to clone.")]
	public UnityEngine.UI.Image defenderIconPrefab;

	[Tooltip("Midfielder icon prefab to clone.")]
	public UnityEngine.UI.Image midfielderIconPrefab;

	[Tooltip("Forward icon prefab to clone.")]
	public UnityEngine.UI.Image forwardIconPrefab;

	[Header("Elements")]
	[Tooltip("Box collider that defines the field area. (It will be disabled.)")]
	public BoxCollider fieldArea;

	[Tooltip("Header to display the formation name.")]
	public UnityEngine.UI.Text header;


	// Methods
	//--------
	/// <summary>
	/// Sets the formation.
	/// </summary>
	/// <returns>The formation.</returns>
	/// <param name="formation">Formation.</param>
	public void SetFormation(SsFormation formation)
	{
		int i;
		SsFormationPlayer player;
		RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
		Rect area = new Rect(-50, -50, 100, 100);
		Vector2 scale = Vector2.one;
		UnityEngine.UI.Image icon;

		if (rectTransform != null)
		{
			scale.x = rectTransform.localScale.x;
			scale.y = rectTransform.localScale.y;
		}

		if (fieldArea != null)
		{
			fieldArea.enabled = false;
			area = new Rect(new Vector2((fieldArea.center.x * scale.x) - (fieldArea.size.x * scale.x),
			                            (fieldArea.center.y * scale.y) - (fieldArea.size.y * scale.y)),
			                new Vector2(fieldArea.size.x * scale.x, fieldArea.size.y * scale.y));
		}

		if (header != null)
		{
			if (string.IsNullOrEmpty(formation.displayName) == false)
			{
				header.text = formation.displayName;
			}
			else
			{
				header.text = formation.id;
			}
		}

		if ((formation.players != null) && (formation.players.Length > 0))
		{
			for (i = 0; i < formation.players.Length; i++)
			{
				player = formation.players[i];
				if (player == null)
				{
					continue;
				}

				icon = null;

				if ((player.position == SsPlayer.positions.goalkeeper) && 
				    (goalkeeperIconPrefab != null))
				{
					icon = (UnityEngine.UI.Image)Instantiate(goalkeeperIconPrefab);
				}
				else if ((player.position == SsPlayer.positions.defender) && 
				         (defenderIconPrefab != null))
				{
					icon = (UnityEngine.UI.Image)Instantiate(defenderIconPrefab);
				}
				else if ((player.position == SsPlayer.positions.midfielder) && 
				         (midfielderIconPrefab != null))
				{
					icon = (UnityEngine.UI.Image)Instantiate(midfielderIconPrefab);
				}
				else if ((player.position == SsPlayer.positions.forward) && 
				         (forwardIconPrefab != null))
				{
					icon = (UnityEngine.UI.Image)Instantiate(forwardIconPrefab);
				}

				if (icon != null)
				{
					icon.transform.SetParent(transform, false);
					icon.transform.position = new Vector3(area.center.x + (area.size.x * player.normalisedPos.x), 
					                                      area.center.y + (area.size.y * player.normalisedPos.z), 
					                                      0.0f);
				}
			}
		}
	}
}
