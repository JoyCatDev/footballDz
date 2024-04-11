using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// Team formation.
/// </summary>
public class SsFormation : MonoBehaviour {

	// Public
	//-------
	[Tooltip("Unique ID. It is used to identify the resource.")]
	public string id;

	[Tooltip("Name to display in the menus.")]
	public string displayName;

	[Space(10)]
	[Tooltip("Automatically sort the formation's players in the editor. If this is false then the players are sorted in the order in which they are attached to the parent.\n" + 
	         "When a match starts then the team's players are spawned in the order of the formation's players. " + 
	         "If the team has less players than the players in the formation, then only the first few players in formation are used.")]
	public bool autoSort = true;

	[Tooltip("The players. (The list is automatically updated in the editor.)")]
	public SsFormationPlayer[] players;



	// Private
	//--------
	private Vector3 minPos;			// Min players pos (3D field)
	private Vector3 maxPos;			// Max players pos (3D field)


	// Properties
	//-----------
	public Vector3 MinPos
	{
		get { return(minPos); }
	}


	public Vector3 MaxPos
	{
		get { return(maxPos); }
	}



	// Methods
	//--------

	/// <summary>
	/// Updates the players' normalised positions.
	/// </summary>
	/// <returns>The normalised positions.</returns>
	public void UpdateNormalisedPositions()
	{
		if ((players == null) || (players.Length <=0 ))
		{
			return;
		}

		int i;
		SsFormationPlayer player;

		for (i = 0; i < players.Length; i++)
		{
			player = players[i];
			if (player == null)
			{
				continue;
			}
			player.UpdateNormalisedPos();
		}
	}


	/// <summary>
	/// Call this after field scene is loaded, before match starts, and before players are positioned.
	/// </summary>
	public void OnPreMatchStart()
	{
		if ((players == null) || (players.Length <=0 ))
		{
			return;
		}

		int i;
		Vector3 pos;
		SsFormationPlayer player;
		SsFieldProperties field = SsFieldProperties.Instance;
		// Scale from 2D to 3D field
		Vector3 scale = new Vector3(field.PlayArea.extents.x / SsFormationManager.halfFieldImageWidth, 
		                            1.0f, 
		                            field.PlayArea.extents.z / SsFormationManager.halfFieldImageHeight);

		minPos = new Vector3(float.MaxValue, 0.0f, float.MaxValue);
		maxPos = new Vector3(float.MinValue, 0.0f, float.MinValue);

		// First pass: get extents
		for (i = 0; i < players.Length; i++)
		{
			player = players[i];
			if (player == null)
			{
				continue;
			}

			pos = new Vector3(player.transform.position.x * scale.x, 
			                  player.transform.position.y * scale.y,
			                  player.transform.position.z * scale.z);
			if (minPos.x > pos.x)
			{
				minPos.x = pos.x;
			}
			if (minPos.z > pos.z)
			{
				minPos.z = pos.z;
			}

			if (maxPos.x < pos.x)
			{
				maxPos.x = pos.x;
			}
			if (maxPos.z < pos.z)
			{
				maxPos.z = pos.z;
			}

			player.Pos3D = pos;
		}


		// Second pass: player relative pos
		for (i = 0; i < players.Length; i++)
		{
			player = players[i];
			if (player == null)
			{
				continue;
			}

			pos = player.Pos3D;
			pos.x = GeUtils.Lerp(pos.x, minPos.x, maxPos.x, 0.0f, 1.0f);
			pos.z = GeUtils.Lerp(pos.z, minPos.z, maxPos.z, 0.0f, 1.0f);
			player.RelativePos = pos;
		}
	}


#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		// Update the players, in the editor.
		UpdateInEditor();
	}


	/// <summary>
	/// Update the players, in the editor.
	/// </summary>
	/// <returns>The in editor.</returns>
	void UpdateInEditor()
	{
		players = gameObject.GetComponentsInChildren<SsFormationPlayer>(true);
		if ((players == null) || (players.Length <= 1) || (autoSort == false))
		{
			return;
		}

		// Sort the players
		//-----------------
		// Sort the players so that when a match has less players than the number in the formation, they are still positioned
		// in a balanced formation.
		List<SsFormationPlayer> sorted = new List<SsFormationPlayer>(players.Length);
		List<SsFormationPlayer> forwards = new List<SsFormationPlayer>(players.Length);
		List<SsFormationPlayer> midfielders = new List<SsFormationPlayer>(players.Length);
		List<SsFormationPlayer> defenders = new List<SsFormationPlayer>(players.Length);
		List<SsFormationPlayer> destList = new List<SsFormationPlayer>(players.Length);
		List<SsFormationPlayer> srcList;
		int i, n, step, foundIndex;
		float distance, foundDistance;
		SsFormationPlayer player, foundPlayer;

		// Sort pattern:
		//	- First goalkeeper
		//	- Loop through remaining players:
		//		- Add a forward
		//		- Add a defender
		//		- Add a midfielder
		//		- Repeat loop until all are added
		//		The remaining players are selected in the following order, for each type of player:
		//		- Nearest to middle Z
		//		- Nearest to top Z
		//		- Nearest to bottom Z
		//		Examples: One forward will be near middle Z, next forward will be near top Z, next forward will be near bottom Z, repeat.
		//				  One defender will be near middle Z, next defender will be near top Z, next defender will be near bottom Z, repeat.
		//				  One midfielder will be near middle Z, next midfielder will be near top Z, next midfielder will be near bottom Z, repeat.

		// Add the goalkeeper, and build the lists of other positions
		for (i = 0; i < players.Length; i++)
		{
			player = players[i];
			if (player != null)
			{
				if ((player.position == SsPlayer.positions.goalkeeper) && (sorted.Count <= 0))
				{
					// Goalkeeper first in list
					sorted.Add(player);
					players[i] = null;	// Clear slot
				}
				else if (player.position == SsPlayer.positions.forward)
				{
					forwards.Add(player);
					players[i] = null;	// Clear slot
				}
				else if (player.position == SsPlayer.positions.midfielder)
				{
					midfielders.Add(player);
					players[i] = null;	// Clear slot
				}
				else if (player.position == SsPlayer.positions.defender)
				{
					defenders.Add(player);
					players[i] = null;	// Clear slot
				}
			}
		}

		// Sort each list in the order: near middle, near top and near bottom
		for (n = 0; n < 3; n++)
		{
			if (n == 0)
			{
				srcList = forwards;
			}
			else if (n == 1)
			{
				srcList = defenders;
			}
			else
			{
				srcList = midfielders;
			}

			if ((srcList == null) || (srcList.Count < 2))
			{
				continue;
			}

			destList.Clear();
			step = (n % 3);
			while (true)
			{
				foundIndex = 0;
				foundDistance = 0.0f;
				foundPlayer = null;
				for (i = 0; i < srcList.Count; i++)
				{
					player = srcList[i];
					if (player == null)
					{
						continue;
					}
					distance = player.transform.localPosition.z;

					if (step == 0)
					{
						// Near middle
						if ((foundPlayer == null) || (Mathf.Abs(distance) < Mathf.Abs(foundDistance)))
						{
							foundPlayer = player;
						}
					}
					else if (step == 1)
					{
						// Near top
						if ((foundPlayer == null) || (distance > foundDistance))
						{
							foundPlayer = player;
						}
					}
					else
					{
						// Near bottom
						if ((foundPlayer == null) || (distance < foundDistance))
						{
							foundPlayer = player;
						}
					}

					if (foundPlayer == player)
					{
						foundDistance = distance;
						foundIndex = i;
					}
				}

				if (foundPlayer != null)
				{
					destList.Add(foundPlayer);
					srcList[foundIndex] = null;	// Clear slot
				}
				else
				{
					break;
				}

				step ++;
				if (step > 2)
				{
					step = 0;
				}
			}

			// Copy sorted list
			srcList.Clear();
			for (i = 0; i < destList.Count; i++)
			{
				srcList.Add(destList[i]);
			}
		}


		// Copy the positions to the main sorted list
		n = Mathf.Max(forwards.Count, Mathf.Max(midfielders.Count, defenders.Count));
		for (i = 0; i < n; i++)
		{
			if (i < forwards.Count)
			{
				sorted.Add(forwards[i]);
			}
			if (i < midfielders.Count)
			{
				sorted.Add(midfielders[i]);
			}
			if (i < defenders.Count)
			{
				sorted.Add(defenders[i]);
			}
		}

		// Add remaining players (e.g. in case there are 2 goalkeepers, which should Not be the case)
		for (i = 0; i < players.Length; i++)
		{
			player = players[i];
			if (player != null)
			{
				sorted.Add(player);
				players[i] = null;	// Clear slot
			}
		}

		// Finally, copy the sorted list to the array
		players = new SsFormationPlayer[sorted.Count];
		for (i = 0; i < sorted.Count; i++)
		{
			players[i] = sorted[i];
		}
	}
#endif //UNITY_EDITOR

}
