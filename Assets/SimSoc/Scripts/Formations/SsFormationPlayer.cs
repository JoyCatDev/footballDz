using UnityEngine;
using System.Collections;

/// <summary>
/// Formation player. For defining a player in a formation.
/// </summary>
public class SsFormationPlayer : MonoBehaviour {

	// Public
	//-------
	[Tooltip("Position to play.")]
	public SsPlayer.positions position;

	[HideInInspector]
	public Vector3 normalisedPos;		// Normalised position on the field (e.g. 0 = min, 0.5 = half way, 1 = max)


	// Private
	//--------
	private int linkedToPlayer = -1;	// Was the position linked to a spawned player?
	private Vector3 pos3D;				// Default position on the 3D field (playing towards the right)
	private Vector3 relativePos;		// Position relative to min and max player positions (e.g. 0 = min, 0.5 = half way, 1 = max)


	// Properties
	//-----------
	public int LinkedToPlayer
	{
		get { return(linkedToPlayer); }
		set
		{
			linkedToPlayer = value;
		}
	}


	public Vector3 Pos3D
	{
		get { return(pos3D); }
		set
		{
			pos3D = value;
		}
	}


	public Vector3 RelativePos
	{
		get { return(relativePos); }
		set
		{
			relativePos = value;
		}
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		UpdateNormalisedPos();
	}


	/// <summary>
	/// Updates the normalised position on the field.
	/// </summary>
	/// <returns>The normalised position.</returns>
	public void UpdateNormalisedPos()
	{
		normalisedPos.x = GeUtils.Lerp(transform.localPosition.x, 
		                               -SsFormationManager.halfFieldImageWidth, 
		                               SsFormationManager.halfFieldImageWidth,
		                               0.0f, 1.0f);
		normalisedPos.y = 0.0f;
		normalisedPos.z = GeUtils.Lerp(transform.localPosition.z, 
		                               -SsFormationManager.halfFieldImageHeight, 
		                               SsFormationManager.halfFieldImageHeight,
		                               0.0f, 1.0f);
	}


#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			return;
		}

		string icon = "";

		if (position == SsPlayer.positions.goalkeeper)
		{
			icon = "SimSoc/goalkeeper.png";
		}
		else if (position == SsPlayer.positions.defender)
		{
			icon = "SimSoc/defender.png";
		}
		else if (position == SsPlayer.positions.midfielder)
		{
			icon = "SimSoc/midfielder.png";
		}
		else if (position == SsPlayer.positions.forward)
		{
			icon = "SimSoc/forward.png";
		}

		// Limit position to within the field
		Vector3 pos = transform.localPosition;
		pos.x = Mathf.Clamp(pos.x, -SsFormationManager.halfFieldImageWidth, SsFormationManager.halfFieldImageWidth);
		pos.y = 0.0f;
		pos.z = Mathf.Clamp(pos.z, -SsFormationManager.halfFieldImageHeight, SsFormationManager.halfFieldImageHeight);
		transform.localPosition = pos;

		UpdateNormalisedPos();

		if (string.IsNullOrEmpty(icon) == false)
		{
			Gizmos.DrawIcon(transform.position, icon, false);
		}
	}
#endif //UNITY_EDITOR

}
