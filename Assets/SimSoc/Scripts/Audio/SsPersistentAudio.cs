using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Audio that is persistent through multiple scenes (e.g. music in the menus outside a match).
/// NOTE: All persitent audio is destroyed when StartMatch/StartMatchTest is called, to ensure previous scenes' music is unloaded (e.g. menu music).
/// </summary>
public class SsPersistentAudio : MonoBehaviour {

	// Public
	//-------
	[Tooltip("Clips to play (e.g. menu music).")]
	public AudioClip[] clips;

	[Space(10)]
	[Tooltip("Do not play clips that are already playing in the scene.")]
	public bool dontPlayDuplicates = true;

	[Space(10)]
	[Tooltip("Use 2D audio sources.")]
	public bool use2dAudio = true;
	[Tooltip("Loop the audio.")]
	public bool loopAudio = true;

	[Space(10)]
	[Tooltip("Volume of the clips to play.")]
	public float volume = 1.0f;
	[Tooltip("Fade in the volume.")]
	public bool fadeInVolume = true;
	[Tooltip("Override the default fade in duration. (The menu music's fade in duration is used as the default.) ")]
	public bool overrideFadeInDuration = false;
	[Tooltip("Volume fade in duration (override).")]
	public float volumeFadeInDuration;
	[Tooltip("Volume fade in delay (override).")]
	public float volumeFadeInDelay;



	// Private
	//--------
	static private List<SsPersistentAudio> list;			// List of persitent audio game objects


	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		AddToList(this);
		DontDestroyOnLoad(gameObject);

		int i, n;
		AudioClip clip;
		AudioSource audioSource, compareSource;
		AudioSource[] audioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		bool foundPlaying, foundDuplicate;

		foundPlaying = false;
		if ((audioSources != null) && (audioSources.Length > 0))
		{
			for (n = 0; n < audioSources.Length; n++)
			{
				compareSource = audioSources[n];
				if ((compareSource != null) && (compareSource.isPlaying))
				{
					foundPlaying = true;
					break;
				}
			}
		}

		if ((clips != null) && (clips.Length > 0))
		{
			for (i = 0; i < clips.Length; i++)
			{
				clip = clips[i];
				if (clip != null)
				{
					foundDuplicate = false;
					if ((dontPlayDuplicates) && (foundPlaying))
					{
						if ((audioSources != null) && (audioSources.Length > 0))
						{
							for (n = 0; n < audioSources.Length; n++)
							{
								compareSource = audioSources[n];
								if ((compareSource != null) && (compareSource.clip == clip) && 
								    (compareSource.isPlaying))
								{
									foundDuplicate = true;
									break;
								}
							}
						}
					}

					if (foundDuplicate)
					{
						continue;
					}

					audioSource = gameObject.AddComponent<AudioSource>();
					if (audioSource != null)
					{
						audioSource.clip = clip;
						audioSource.volume = volume;

						if (use2dAudio)
						{
							// 2D audio
							audioSource.spatialBlend = 0.0f;
						}
						audioSource.loop = loopAudio;

						audioSource.Play();
					}
				}
			}
		}


		if ((audioSources != null) && (audioSources.Length > 0))
		{
			for (n = 0; n < audioSources.Length; n++)
			{
				audioSources[n] = null;
			}
		}
		audioSources = null;


		if (fadeInVolume)
		{
			// Is already fading in?
			if ((foundPlaying) && (AudioListener.volume <= SsSettings.volume))
			{
				if (overrideFadeInDuration)
				{
					SsVolumeController.SetVolume(SsSettings.volume, volumeFadeInDuration, volumeFadeInDelay);
				}
				else
				{
					SsVolumeController.SetVolume(SsSettings.volume, SsVolumeController.menuMusicFadeInDuration, 
					                             SsVolumeController.menuMusicFadeInDelay);
				}
			}
			else
			{
				if (overrideFadeInDuration)
				{
					SsVolumeController.FadeVolumeIn(SsSettings.volume, volumeFadeInDuration, volumeFadeInDelay);
				}
				else
				{
					SsVolumeController.FadeVolumeIn(SsSettings.volume, SsVolumeController.menuMusicFadeInDuration, 
					                                SsVolumeController.menuMusicFadeInDelay);
				}
			}
		}
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		RemoveFromList(this);
	}



	/// <summary>
	/// Destroy all persistent audio game objects.
	/// </summary>
	/// <returns>The all.</returns>
	static public void DestroyAll()
	{
		if ((list == null) || (list.Count <= 0))
		{
			return;
		}

		SsPersistentAudio[] all = list.ToArray();
		int i;

		if ((all != null) && (all.Length > 0))
		{
			for (i = 0; i < all.Length; i++)
			{
				if (all[i] != null)
				{
					RemoveFromList(all[i]);
					Destroy(all[i].gameObject);
					all[i] = null;
				}
			}
		}
		all = null;

		list.Clear();
	}


	/// <summary>
	/// Adds persistent audio gameo bject to list.
	/// </summary>
	/// <returns>The to list.</returns>
	/// <param name="audio">Audio.</param>
	static private void AddToList(SsPersistentAudio audio)
	{
		if (audio == null)
		{
			return;
		}
		if (list == null)
		{
			list = new List<SsPersistentAudio>(10);
		}
		if ((list.Count > 0) && (list.Contains(audio)))
		{
			return;
		}
		list.Add(audio);
	}


	/// <summary>
	/// Removes persistent audio gameo bject from list.
	/// </summary>
	/// <returns>The from list.</returns>
	/// <param name="audio">Audio.</param>
	static private void RemoveFromList(SsPersistentAudio audio)
	{
		if ((audio == null) || (list == null) || (list.Count <= 0) || (list.Contains(audio) == false))
		{
			return;
		}
		list.Remove(audio);
	}

}
