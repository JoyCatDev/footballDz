using UnityEngine;
using System.Collections;

/// <summary>
/// Ball sounds.
/// </summary>
public class SsBallSounds : MonoBehaviour {

	// Public
	//-------
	[Tooltip("AudioSource for sounds. If empty then one will be added.")]
	public AudioSource audioSource;
	[Tooltip("Use 2D audio.")]
	public bool use2dAudio = true;
	[Tooltip("Volume for audio.")]
	public float audioVolume = 1.0f;

	[Header("SFX")]
	[Tooltip("Kick ball near.")]
	public AudioClip kickNear;
	[Tooltip("Kick ball medium. (Default)")]
	public AudioClip kickMedium;
	[Tooltip("Kick ball far.")]
	public AudioClip kickFar;

	[Tooltip("Goalkeeper punch ball.")]
	public AudioClip punch;

	[Tooltip("Header.")]
	public AudioClip header;

	[Tooltip("Throw ball in.")]
	public AudioClip throwIn;
	
	[Tooltip("Bounce on ground.")]
	public AudioClip bounceGround;
	[Tooltip("Ball must go at least this high before playing the bounce sound.")]
	public float minHeightForBounceSfx = 0.3f;
	
	[Tooltip("Goalkeeper catch ball.")]
	public AudioClip goalkeeperCatch;
	[Tooltip("Goalkeeper throw ball near.")]
	public AudioClip goalkeeperThrowNear;
	[Tooltip("Goalkeeper throw ball medium. (Default)")]
	public AudioClip goalkeeperThrowMedium;
	[Tooltip("Goalkeeper throw ball far.")]
	public AudioClip goalkeeperThrowFar;


	
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
	/// Init the audio.
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
			audioSource.volume = audioVolume;
			audioSource.playOnAwake = false;
			
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
