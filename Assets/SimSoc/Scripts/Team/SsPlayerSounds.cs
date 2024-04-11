using UnityEngine;
using System.Collections;

/// <summary>
/// Player sounds/audio.
/// </summary>
public class SsPlayerSounds : MonoBehaviour {

	// Public
	//-------
	[Tooltip("AudioSource for sounds. If empty then one will be added.")]
	public AudioSource audioSource;
	[Tooltip("Use 2D audio.")]
	public bool use2dAudio = true;
	[Tooltip("Volume for audio.")]
	public float audioVolume = 1.0f;

	[Header("SFX")]
	[Tooltip("When player slides.")]
	public AudioClip slideTackle;
	[Tooltip("When player falls (e.g. tackled by opponent)")]
	public AudioClip fall;


	
	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		InitAudio();
	}


	/// <summary>
	/// Inits the audio.
	/// </summary>
	/// <returns>The audio.</returns>
	void InitAudio()
	{
		audioSource = gameObject.GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
		if (audioSource != null)
		{
			audioSource.playOnAwake = false;
			audioSource.volume = audioVolume;

			if (use2dAudio)
			{
				// 2D audio
				audioSource.spatialBlend = 0.0f;
			}
		}
	}


	/// <summary>
	/// Play the sfx.
	/// </summary>
	/// <returns>The sfx.</returns>
	/// <param name="clip">Clip.</param>
	public void PlaySfx(AudioClip clip)
	{
		if ((audioSource != null) && (clip != null))
		{
			audioSource.PlayOneShot(clip);
		}
	}
}
