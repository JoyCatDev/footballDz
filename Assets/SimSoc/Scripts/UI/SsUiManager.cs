using UnityEngine;
using System.Collections;

/// <summary>
/// User interface manager.
/// </summary>
public class SsUiManager : MonoBehaviour {

	// Const/Static
	//-------------
	// Min frames per second when menus update, to avoid objects jumping when there's a lag
	private const float minUpdateFps = 5.0f;
	public const float minUpdateDt = (1.0f / minUpdateFps);


	// Classes
	//--------
	// Link a button to a resource (e.g. team, ball)
	[System.Serializable]
	public class SsButtonToResource
	{
		[Tooltip("Button to link to resource.")]
		public UnityEngine.UI.Button button;

		[Tooltip("Resource ID to link to. (Empty = random)")]
		public string id;

		private RectTransform buttonRectTransform;


		// Methods
		//--------
		/// <summary>
		/// Creates the button resource.
		/// </summary>
		/// <returns>The button resource.</returns>
		/// <param name="newButton">New button.</param>
		/// <param name="newId">New identifier.</param>
		static public SsButtonToResource CreateButtonResource(UnityEngine.UI.Button newButton, string newId)
		{
			SsButtonToResource newRes = new SsButtonToResource();
			if (newRes != null)
			{
				newRes.button = newButton;
				newRes.id = newId;
			}
			return (newRes);
		}


		/// <summary>
		/// Gets the button rect transform.
		/// </summary>
		/// <returns>The button rect transform.</returns>
		public RectTransform GetButtonRectTransform()
		{
			if ((buttonRectTransform == null) && (button != null))
			{
				buttonRectTransform = button.gameObject.GetComponent<RectTransform>();
			}
			return (buttonRectTransform);
		}


		/// <summary>
		/// Get the ID linked to the button. Search the provided resources list.
		/// </summary>
		/// <returns>The identifier from button.</returns>
		/// <param name="button">Button.</param>
		/// <param name="list">List of resources.</param>
		static public string GetIdFromButton(UnityEngine.UI.Button button, SsButtonToResource[] list)
		{
			if ((button == null) || (list == null) || (list.Length <= 0))
			{
				return (null);
			}
			
			int i;
			SsButtonToResource res;
			
			for (i = 0; i < list.Length; i++)
			{
				res = list[i];
				if ((res != null) && (res.button == button))
				{
					return (res.id);
				}
			}
			
			return (null);
		}
	}


	// Link a sprite texture to a resource (e.g. team, ball).
	[System.Serializable]
	public class SsSpriteToResource
	{
		[Tooltip("Resource ID to link to. (Empty = random)")]
		public string id;

		[Tooltip("Sprite texture to link to resource.")]
		public Sprite sprite;


		/// <summary>
		/// Gets the sprite with id from the list.
		/// </summary>
		/// <returns>The sprite with identifier.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="list">List.</param>
		static public Sprite GetSpriteWithId(string id, SsSpriteToResource[] list)
		{
			if ((list == null) || (list.Length <= 0))
			{
				return (null);
			}

			int i;
			SsSpriteToResource res;

			for (i = 0; i < list.Length; i++)
			{
				res = list[i];
				if ((res != null) && (res.id == id))
				{
					return (res.sprite);
				}
			}

			return (null);
		}
	}


	/// <summary>
	/// Check if an event system exists in the scene.
	/// </summary>
	/// <returns>The event system.</returns>
	static public bool CheckEventSystem(bool showWarning = true)
	{
		UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

#if UNITY_EDITOR
		if ((eventSystem == null) && (showWarning))
		{
			Debug.LogWarning("WARNING: No UI EventSystem in the scene: " + GeUtils.GetLoadedLevelName());
		}
#endif //UNITY_EDITOR

		return (eventSystem != null);
	}


	// Public
	//-------
	[Header("Audio")]
	[Tooltip("AudioSource for sounds. If empty then one will be added.")]
	public AudioSource audioSource;
	[Tooltip("Use 2D audio.")]
	public bool use2dAudio = true;
	[Tooltip("Volume for audio.")]
	public float audioVolume = 1.0f;


	[Header("Audio SFX")]
	[Tooltip("Default click for buttons.")]
	public AudioClip sfxButtonClick;

	[Tooltip("Show menu sound.")]
	public AudioClip sfxShowMenu;

	[Tooltip("Hide menu sound.")]
	public AudioClip sfxHideMenu;



	// Private
	//--------
	static private SsUiManager instance;

	static private int frameOfLastShowHideSfx;		// Keep track in which frame the last show/hide sfx was played


	// Properties
	//-----------
	static public SsUiManager Instance
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

		InitAudio();
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
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
	/// Play the button click sound.
	/// </summary>
	/// <returns>The button click.</returns>
	static public void PlayButtonClickSound(AudioClip overrideClip = null)
	{
		if (instance != null)
		{
			if (overrideClip != null)
			{
				instance.PlaySfx(overrideClip);
			}
			else
			{
				instance.PlaySfx(instance.sfxButtonClick);
			}
		}
	}


	/// <summary>
	/// Play the show menu sound.
	/// </summary>
	/// <returns>The show menu.</returns>
	static public void PlayShowMenuSound(AudioClip overrideClip = null)
	{
		// Avoid playing 2 or more menu show/hide sounds too close together
		if (frameOfLastShowHideSfx >= Time.frameCount - 1)
		{
			return;
		}

		if (instance != null)
		{
			frameOfLastShowHideSfx = Time.frameCount;
			if (overrideClip != null)
			{
				instance.PlaySfx(overrideClip);
			}
			else
			{
				instance.PlaySfx(instance.sfxShowMenu);
			}
		}
	}


	/// <summary>
	/// Play the hide menu sound.
	/// </summary>
	/// <returns>The hide menu sound.</returns>
	static public void PlayHideMenuSound(AudioClip overrideClip = null)
	{
		// Avoid playing 2 or more menu show/hide sounds too close together
		if (frameOfLastShowHideSfx >= Time.frameCount - 1)
		{
			return;
		}

		if (instance != null)
		{
			frameOfLastShowHideSfx = Time.frameCount;
			if (overrideClip != null)
			{
				instance.PlaySfx(overrideClip);
			}
			else
			{
				instance.PlaySfx(instance.sfxHideMenu);
			}
		}
	}


	/// <summary>
	/// Play the sound.
	/// </summary>
	/// <returns>The sound.</returns>
	/// <param name="clip">Clip.</param>
	static public void PlaySound(AudioClip clip)
	{
		if (instance != null)
		{
			instance.PlaySfx(clip);
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
