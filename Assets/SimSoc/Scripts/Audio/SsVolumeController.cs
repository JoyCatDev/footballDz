using UnityEngine;
using System.Collections;

/// <summary>
/// Volume controller, which sets the volume of AudioListener.
/// Can also be used to fade volume in/out. Max volume is limited to SsSettings.volume.
/// It automatically creates a game object in the scene (if needed) when any of the static methods are called.
/// </summary>
public class SsVolumeController : MonoBehaviour {

	// Const/Static
	//-------------
	// Menu music fade times
	public const float menuMusicFadeInDuration = 5.0f;
	public const float menuMusicFadeOutDuration = 1.0f;
	public const float menuMusicFadeInDelay = 0.0f;

	// Match music fade times
	public const float matchMusicFadeInTime = 2.0f;
	public const float matchMusicFadeOutTime = 1.0f;
	public const float matchMusicFadeInDelay = 0.0f;


	// Min frames per second when updating volumes, to avoid volume jumping when there's a lag
	private const float minUpdateFps = 5.0f;
	public const float minUpdateDt = (1.0f / minUpdateFps);



	// Private
	//--------
	static private SsVolumeController instance;

	private float changeDelay;					// Delay before changing the volume
	private float targetVolume;					// Target volume to reach
	private float volumeSpeed;					// Speed at which to change volume

	private float lastUpdateTime = 0.0f;		// Last time during the update method
	private float unpausedDT = 0.0f;			// Delta time unpaused (i.e. Not affected when game is paused when Time.timeScale is zero).


	// Properties
	//-----------
	static public SsVolumeController Instance
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
	/// Creates the game object.
	/// </summary>
	/// <returns>The game object.</returns>
	static public void CreateGameObject()
	{
		if (instance != null)
		{
			return;
		}

		GameObject go = new GameObject("Volume Controller");
		if (go != null)
		{
			go.AddComponent<SsVolumeController>();
		}
	}


	/// <summary>
	/// Fade the volume in from zero. The max volume will be limited by SsSettings.volume.
	/// </summary>
	/// <returns>The volume in.</returns>
	/// <param name="volume">Volume to fade to.</param>
	/// <param name="changeDuration">How long it takes to change the volume. 0 = instant.</param>
	/// <param name="changeDelay">Delay before changing the volume. 0 = no delay.</param>
	static public void FadeVolumeIn(float volume, float changeDuration = 0.0f, float changeDelay = 0.0f)
	{
		AudioListener.volume = 0.0f;
		SetVolume(volume, changeDuration, changeDelay);
	}


	/// <summary>
	/// Fade the volume out to zero.
	/// </summary>
	/// <returns>The volume out.</returns>
	/// <param name="changeDuration">Change duration.</param>
	/// <param name="changeDelay">Change delay.</param>
	static public void FadeVolumeOut(float changeDuration = 0.0f, float changeDelay = 0.0f)
	{
		SetVolume(0.0f, changeDuration, changeDelay);
	}



	/// <summary>
	/// Sets the volume of AudioListener. The max volume will be limited by SsSettings.volume.
	/// </summary>
	/// <returns>The volume.</returns>
	/// <param name="volume">New volume to set.</param>
	/// <param name="changeDuration">How long it takes to change the volume. 0 = instant.</param>
	/// <param name="changeDelay">Delay before changing the volume. 0 = no delay.</param>
	static public void SetVolume(float volume, float changeDuration = 0.0f, float changeDelay = 0.0f)
	{
		bool active = false;	// Activate/deactivate the instance game object

		volume = Mathf.Clamp(volume, 0.0f, SsSettings.volume);

		if ((changeDuration <= 0.0f) && (changeDelay <= 0.0f))
		{
			active = false;

			AudioListener.volume = volume;

			if (instance != null)
			{
				instance.targetVolume = volume;
			}
		}
		else
		{
			active = true;

			CreateGameObject();

			instance.targetVolume = volume;

			if (changeDuration <= 0.0f)
			{
				instance.volumeSpeed = 0.0f;
			}
			else
			{
				instance.volumeSpeed = Mathf.Abs((volume - AudioListener.volume) / changeDuration);
			}

			instance.changeDelay = changeDelay;
		}

		if ((instance != null) && (instance.gameObject.activeInHierarchy != active))
		{
			instance.gameObject.SetActive(active);
		}
	}


	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		float dt;
		
		unpausedDT = Time.realtimeSinceStartup - lastUpdateTime;
		lastUpdateTime = Time.realtimeSinceStartup;

		// Change volume even if Time.timeScale is zero
		dt = unpausedDT;

		// Clamp frame rate to prevent jumps in volume changes
		if (dt > minUpdateDt)
		{
			dt = minUpdateDt;
		}

		if (AudioListener.volume != targetVolume)
		{
			if (changeDelay > 0.0f)
			{
				changeDelay = Mathf.Max(changeDelay - dt, 0.0f);
			}
			else
			{
				AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, targetVolume, volumeSpeed * dt);
			}
		}
	}

}
