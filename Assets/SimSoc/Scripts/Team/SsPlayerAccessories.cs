using UnityEngine;
using System.Collections;

/// <summary>
/// Accessories attached to the player (e.g. hair, hat). Can be used to create a variety of players, from the same base player mesh.
/// Attach this component to the player and setup the accessories array.
/// </summary>
public class SsPlayerAccessories : MonoBehaviour {

	// Classes
	//--------
	[System.Serializable]
	public class SsPlayerAccessory
	{
		public GameObject accessoryPrefab;
		
		[Tooltip("Name of object/bone to which to attach the accessory (e.g. player's head or hand).")]
		public string attachTo;
		
		[Tooltip("Attach position offset.")]
		public Vector3 attachOffset;

		[Tooltip("Attach rotation.")]
		public Vector3 attachRotation;
	}
	
	
	// Public
	//-------
	[Tooltip("Accessories")]
	public SsPlayerAccessory[] accessories;
	
	
	// Methods
	//--------
	// Use this for initialization
	void Start()
	{
		SpawnAccessories();
	}
	
	
	/// <summary>
	/// Spawns the accessories.
	/// </summary>
	/// <returns>The accessories.</returns>
	private void SpawnAccessories()
	{
		if ((accessories == null) || (accessories.Length <= 0))
		{
			return;
		}

		SsPlayer player = gameObject.GetComponent<SsPlayer>();

		if (player == null)
		{
			return;
		}

		int i;
		Transform t;
		SsPlayerAccessory accessory;
		GameObject go;

		for (i = 0; i < accessories.Length; i++)
		{
			accessory = accessories[i];
			if ((accessory != null) && (accessory.accessoryPrefab != null) &&
			    (string.IsNullOrEmpty(accessory.attachTo) == false))
			{
				go = (GameObject)Instantiate(accessory.accessoryPrefab);
				if (go != null)
				{
					t = GeUtils.FindChild(player.transform, accessory.attachTo);
					if (t != null)
					{
						go.transform.parent = t;
					}
					else
					{
						go.transform.parent = player.transform;

#if UNITY_EDITOR
						Debug.LogWarning("Player  [" + player.GetAnyName() + "]  had no child named  [" + accessory.attachTo + "]  " + 
						                 "for accessory  [" + accessory.accessoryPrefab + "].");
#endif //UNITY_EDITOR
					}

					go.transform.localPosition = accessory.attachOffset;
					go.transform.localRotation = Quaternion.Euler(accessory.attachRotation);
				}
			}
		}
	}
}
