using UnityEngine;
using System.Collections;

/// <summary>
/// Match prefabs and resources common to all matches (e.g. camera, music, sfx).
/// These are spawned/created at the start of a match.
/// </summary>
public class SsMatchPrefabs : MonoBehaviour {

	// Public
	//-------
	[Header("Audio")]
	[Tooltip("Default music to play during the match. The Field Properties can override the default music with music specific to the field.")]
	public AudioClip music;
	[Tooltip("Default music volume during the match. The Field Properties can override the default volume for a field.")]
	public float musicVolume = 1.0f;
	[Tooltip("Use 2D audio sources.")]
	public bool use2dAudio = true;
	[Tooltip("Loop the music.")]
	public bool loopMusic = true;

	[Header("Audio SFX")]
	[Tooltip("Default sfx volume during the match. The Field Properties can override the default volume for a field.")]
	public float sfxVolume = 1.0f;
	[Tooltip("SFX when ball goes out.")]
	public AudioClip sfxBallOut;
	[Tooltip("SFX when ball is shot at goal.")]
	public AudioClip sfxShootBall;
	[Tooltip("SFX score goal.")]
	public AudioClip sfxScoreGoal;
	[Tooltip("SFX whistle for kick off.")]
	public AudioClip sfxWhistleKickoff;
	[Tooltip("SFX whistle for half time.")]
	public AudioClip sfxWhistleHalfTime;
	[Tooltip("SFX whistle for full time.")]
	public AudioClip sfxWhistleFullTime;


	[Header("Match prefabs to clone")]
	public SsMatchCamera matchCamera;
	public SsMatchInputManager matchInputManager;


	[Space(10)]
	[Tooltip("Player marker for human controlled player.")]
	public GameObject markerControl;

	[Tooltip("Player marker for human controlled player with the ball.")]
	public GameObject markerControlBall;

	[Tooltip("Player marker for human controlled defender player with the ball. (Optional. Use if you want to be able to identify the player's position.)")]
	public GameObject markerControlBallDefender;

	[Tooltip("Player marker for human controlled midfielder player with the ball. (Optional. Use if you want to be able to identify the player's position.)")]
	public GameObject markerControlBallMidfielder;

	[Tooltip("Player marker for human controlled forward player with the ball. (Optional. Use if you want to be able to identify the player's position.)")]
	public GameObject markerControlBallForward;

	[Tooltip("Player marker for human team player to pass to.")]
	public GameObject markerPass;

	[Tooltip("Player marker for AI team player with the ball.")]
	public GameObject markerAiBall;

	[Tooltip("Marker for when ball goes out.")]
	public GameObject markerOut;

	[Tooltip("Mouse position marker.")]
	public GameObject markerMouse;
	[Tooltip("Rotate speed. 0 = none. (This is an easy way to rotate the marker. You can also add your own animations to marker game objects.)")]
	public float markerMouseRotateSpeed = 100.0f;


	[Space(10)]
	public UnityEngine.UI.Image arrowControl;
	public UnityEngine.UI.Image arrowPass;


	[Space(10)]
	[Tooltip("EventSystem prefab. It will only be spawned if the scene does not have one.")]
	public UnityEngine.EventSystems.EventSystem eventSystemPrefab;

	[Tooltip("In game menu prefabs.")]
	public GameObject[] inGameMenus;





	// Methods
	//--------
	/// <summary>
	/// Spawns the objects.
	/// </summary>
	/// <returns>The objects.</returns>
	static public void SpawnObjects(SsMatchPrefabs matchPrefabs)
	{
		if (matchPrefabs == null)
		{
			return;
		}

		int i;
		GameObject go;

		// Input
		if (matchPrefabs.matchInputManager != null)
		{
			Instantiate(matchPrefabs.matchInputManager);
		}
		
		// Camera
		if ((SsMatchCamera.Instance == null) && 
		    (matchPrefabs.matchCamera != null))
		{
			Instantiate(matchPrefabs.matchCamera);
		}


		// UI
		if (matchPrefabs.eventSystemPrefab != null)
		{
			if (SsUiManager.CheckEventSystem(false) == false)
			{
				Instantiate(matchPrefabs.eventSystemPrefab);
			}
		}

		if ((matchPrefabs.inGameMenus != null) && (matchPrefabs.inGameMenus.Length > 0))
		{
			for (i = 0; i < matchPrefabs.inGameMenus.Length; i++)
			{
				if (matchPrefabs.inGameMenus[i] != null)
				{
					Instantiate(matchPrefabs.inGameMenus[i]);
				}
			}
		}




		// Markers
		// REMINDER: Player markers are spawned on the player
		SsMarkersManager.CreateInstance();
		if (SsMarkersManager.Instance != null)
		{
			if (matchPrefabs.markerMouse != null)
			{
				go = Instantiate(matchPrefabs.markerMouse);
				if (go != null)
				{
					SsMarkersManager.Instance.AttachAndDisableMarker(go, SsMarkersManager.markerTypes.mouse, 
					                                                 matchPrefabs.markerMouseRotateSpeed);
				}
			}

			if (matchPrefabs.markerOut != null)
			{
				go = Instantiate(matchPrefabs.markerOut);
				if (go != null)
				{
					SsMarkersManager.Instance.AttachAndDisableMarker(go, SsMarkersManager.markerTypes.ballOut, 
					                                                 0.0f);
				}
			}
		}


#if UNITY_EDITOR
		// Editor warnings
		//----------------
		if (matchPrefabs.matchCamera == null)
		{/*
			Debug.LogError("ERROR: Match Camera Prefab has not been set on the Match Prefabs.");*/
		}
		if (matchPrefabs.matchInputManager == null)
		{
			Debug.LogError("ERROR: Match Input Manager Prefab has not been set on the Match Prefabs.");
		}


#endif //UNITY_EDITOR

	}
}
