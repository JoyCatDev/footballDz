using UnityEngine;
using System.Collections;

/// <summary>
/// Base menu. Allows for simple transitions.
/// </summary>
public class SsBaseMenu : MonoBehaviour {

	// Const/Static
	//-------------
	// Default durations for animations
	public const float defaultInDuration = 0.3f;
	public const float defaultOutDuration = defaultInDuration;

	public delegate void SsMenuCallback();			// Menu show/hide callback



	// Enums
	//------
	// Animate From directions
	public enum fromDirections
	{
		invalid = -1,
		
		fromRight = 0,
		fromLeft,
		fromTop,
		fromBottom,
		
		// REMINDER: DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.
	}
	
	// Animate To directions
	public enum toDirections
	{
		invalid = -1,
		
		toLeft = 0,
		toRight,
		toTop,
		toBottom,
		
		// REMINDER: DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.
	}
	
	
	// Menu states
	public enum states
	{
		invalid = 0,			// Invalid
		hidden,					// Hidden
		comingIn,				// Coming in (from one of the animate directions)
		visibleAndIdle,			// Visible and idle
		goingOut,				// Going out (to one of the animate directions)
	}
	
	
	// Start-up states
	public enum startUpStates
	{
		snapOffscreenThenHide = 0,			// Snap offscreen then hide/disable (gives menu time to initialise some settings, e.g. in Awake)
		hideImmediate,						// Hide immediately (no animation)
		showAndAnimateOntoScreen,			// Visible, will animate onto screen
		showAndSnapOntoScreen,				// Visible, will Not animate
		doNothing,							// Do nothing. Visibility is probably handled by another game object
		
		// REMINDER: DO NOT CHANGE THE ORDER. ADD NEW ONES ABOVE THIS LINE.
	}




	// Public
	//-------
	[Tooltip("Start up behaviour.")]
	public startUpStates startUpState = startUpStates.snapOffscreenThenHide;
	
	[Space(10)]
	[Tooltip("The default direction from which to enter.")]
	public fromDirections enterDirection = fromDirections.fromRight;
	[Tooltip("The default direction to which to exit.")]
	public toDirections exitDirection = toDirections.toLeft;

	[Space(10)]
	[Tooltip("Override the default animation durations.")]
	public bool overrideDurations;
	[Tooltip("Override the duration of in animation.")]
	public float overrideInDuration = 0.3f;
	[Tooltip("Override the duration of out animation")]
	public float overrideOutDuration = 0.3f;

	[Space(10)]
	[Tooltip("Prevent menu animation dropping below a minimum frame rate, to avoid large jumps in animation.")]
	public bool clampFrameRate = true;

	[Tooltip("Can animate when timescale is zero?")]
	public bool animateZeroTimeScale = true;

	[Tooltip("Do not check if event system exists on start up. (Use this for menus that spawn before the event system.)")]
	public bool doNotCheckEventSystem;


	[Header("Audio")]
	[Tooltip("Music to play when the menu is shown or enabled (in Awake).\nNote: The music stops when the menu is hidden.")]
	public AudioClip music;
	[Tooltip("Play music when menu is shown.")]
	public bool playMusicOnShow;
	[Tooltip("Play music when menu is enabled (in Awake).")]
	public bool playMusicOnAwake;
	[Tooltip("Use a 2D music audio source.")]
	public bool use2dMusic = true;
	[Tooltip("Loop the music.")]
	public bool loopMusic = true;

	[Header("Audio SFX")]
	[Tooltip("Click for buttons. Overrides the default click.")]
	public AudioClip sfxButtonClick;
	[Tooltip("Do not play sound when a button is clicked.")]
	public bool doNotPlayButtonSfx;

	[Tooltip("Show menu sound. Overrides the default show sound.")]
	public AudioClip sfxShowMenu;
	[Tooltip("Do not play sound when menu is shown.")]
	public bool doNotPlayShowSfx;
	
	[Tooltip("Hide menu sound. Overrides the default hide sound.")]
	public AudioClip sfxHideMenu;
	[Tooltip("Do not play sound when menu is hidden.")]
	public bool doNotPlayHideSfx;



	// Private
	//--------
	private states state = states.invalid;
	private float stateTime = 0.0f;

	private bool didShowOrHideAtLeastOnce = false;
	private int showCount;						// Count how many times the menu has been shown

	// Delta time in the last Update method. Use this in overridden Update methods if you want the frame rate clamped (see Update method).
	protected float lastDt = 0.0f;

	private float lastUpdateTime = 0.0f;		// Last time during the update method
	private float unpausedDT = 0.0f;			// Delta time unpaused (i.e. Not affected when game is paused when Time.timeScale is zero).

	protected Canvas parentCanvas;				// Parent canvas
	protected RectTransform rectTransform;		// The menu's RectTransform. It will be animated for in/out transitions.
	private Vector2 defaultRectAnchorMin;		// Default rectTransform anchor min. Animations will be relative to this.
	private Vector2 defaultRectAnchorMax;		// Default rectTransform anchor max. Animations will be relative to this.
	private Vector2 startPos = Vector2.zero;	// Position to animate from. Relative to default anchor positions.
	private Vector2 endPos = Vector2.zero;		// Position to animate to. Relative to default anchor positions.

	protected float inDuration = defaultInDuration;		// In animation duration
	protected float outDuration = defaultOutDuration;	// Out animation duration
	protected float outInvokeDelay = defaultOutDuration;	// Delay before invoking methods to load the next scene (e.g. to start a match)

	private float hideTimer = 0.0f;						// Timer to hide the menu, used for start-up state.

	private fromDirections lastInDirection = fromDirections.invalid;	// Last animated in from direction
	private toDirections lastOutDirection = toDirections.invalid;		// Last animated out to direction
	
	private SsMenuCallback showCallback = null;		// Callback when shown
	private SsMenuCallback hideCallback = null;		// Callback when hidden

	private AudioSource audioSource;				// Audio source to play music
	private bool audioSourceDidExist;				// Did menu have an AudioSource attached?



	// Properties
	//-----------
	public bool IsVisible
	{
		get
		{
			return ((state == states.comingIn) || (state == states.goingOut) || (state == states.visibleAndIdle));
		}
	}
	
	
	public bool IsAnimating
	{
		get
		{
			return((state == states.comingIn) || (state == states.goingOut));
		}
	}
	
	
	public states State
	{
		get { return(state); }
	}
	
	
	public fromDirections LastInDirection
	{
		get { return(lastInDirection); }
	}
	
	
	public toDirections LastOutDirection
	{
		get { return(lastOutDirection); }
	}


	/// <summary>
	/// Get/set the show callback. It will be called when the menu's show animation is done.
	/// NOTE: The callback is cleared after it is called. So it must be set everytime before the menu is shown.
	/// </summary>
	/// <value>The show callback.</value>
	public SsMenuCallback ShowCallback
	{
		get { return(showCallback); }
		set
		{
			showCallback = value;
		}
	}


	/// <summary>
	/// Get/set the hide callback. It will be called when the menu's hide animation is done.
	/// NOTE: The callback is cleared after it is called. So it must be set everytime before the menu is hidden.
	/// </summary>
	/// <value>The hide callback.</value>
	public SsMenuCallback HideCallback
	{
		get { return(hideCallback); }
		set
		{
			hideCallback = value;
		}
	}


	public int ShowCount
	{
		get { return(showCount); }
	}


	public Canvas ParentCanvas
	{
		get { return(parentCanvas); }
	}



	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Awake()
	{
		if (doNotCheckEventSystem == false)
		{
			SsUiManager.CheckEventSystem();
		}

		if ((music != null) && (playMusicOnAwake))
		{
			PlayMusic();
		}

		parentCanvas = gameObject.GetComponentInParent<Canvas>();

		rectTransform = gameObject.GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			defaultRectAnchorMin = rectTransform.anchorMin;
			defaultRectAnchorMax = rectTransform.anchorMax;
		}

		if (overrideDurations)
		{
			inDuration = overrideInDuration;
			outDuration = overrideOutDuration;
		}
		outInvokeDelay = outDuration + 0.1f;


#if UNITY_EDITOR
		// Editor warnings
		//----------------
		if (rectTransform == null)
		{
			Debug.LogError("ERROR: Menu (" + name + ") does not have a rect transform.");
		}
#endif //UNITY_EDITOR

	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void OnDestroy()
	{
	}


	/// <summary>
	/// Raises the enable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void OnEnable()
	{
	}


	/// <summary>
	/// Raises the disable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void OnDisable()
	{
	}


	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Start()
	{
		if (didShowOrHideAtLeastOnce == false)
		{
			if (startUpState == startUpStates.hideImmediate)
			{
				HideImmediate();
			}
			else if (startUpState == startUpStates.snapOffscreenThenHide)
			{
				if (rectTransform != null)
				{
					// Position off screen, to the right
					Vector2 pos = new Vector2(1.0f, 0.0f);
					rectTransform.anchorMin = defaultRectAnchorMin + pos;
					rectTransform.anchorMax = defaultRectAnchorMax + pos;
				}

				hideTimer = 0.1f;
			}
			else if (startUpState == startUpStates.showAndSnapOntoScreen)
			{
				Show(fromDirections.invalid, true);
			}
			else if (startUpState == startUpStates.doNothing)
			{
				// Do nothing
			}
			else
			{
				Show(fromDirections.invalid, false);
			}
		}

		InitAudio();
	}


	/// <summary>
	/// Inits the audio.
	/// </summary>
	/// <returns>The audio.</returns>
	public virtual void InitAudio()
	{
		if (doNotPlayButtonSfx == false)
		{
			// Set up button click sounds
			AudioClip click = sfxButtonClick;

			if ((click == null) && (SsUiManager.Instance != null))
			{
				click = SsUiManager.Instance.sfxButtonClick;
			}

			if (click != null)
			{
				int i;
				UnityEngine.UI.Button button;
				UnityEngine.UI.Button[] buttons = gameObject.GetComponentsInChildren<UnityEngine.UI.Button>(true);
				if ((buttons != null) && (buttons.Length > 0))
				{
					for (i = 0; i < buttons.Length; i++)
					{
						button = buttons[i];
						if (button == null)
						{
							continue;
						}
						button.onClick.AddListener(delegate{PlayButtonClickSound();});
					}
				}
			}
		}
	}


	/// <summary>
	/// Play the button click sound.
	/// </summary>
	/// <returns>The button click sound.</returns>
	public void PlayButtonClickSound()
	{
		SsUiManager.PlayButtonClickSound(sfxButtonClick);
	}


	/// <summary>
	/// Gets the audio source, used for the music.
	/// </summary>
	/// <returns>The audio source.</returns>
	/// <param name="create">Create the audio source if it does Not exist.</param>
	private AudioSource GetAudioSource(bool create)
	{
		if (audioSource != null)
		{
			return (audioSource);
		}

		audioSource = gameObject.GetComponentInParent<AudioSource>();
		audioSourceDidExist = (audioSource != null);
		if (audioSource == null)
		{
			if (create)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
			}
		}
		if (audioSource != null)
		{
			if (audioSourceDidExist == false)
			{
				if (use2dMusic)
				{
					// 2D audio
					audioSource.spatialBlend = 0.0f;
				}
			}
			audioSource.loop = loopMusic;
		}

		return (audioSource);
	}


	/// <summary>
	/// Play the menu's music, if it is Not playing.
	/// </summary>
	/// <returns>The music.</returns>
	public void PlayMusic()
	{
		if (music == null)
		{
			return;
		}

		bool didPlay = false;

		GetAudioSource(true);

		if ((audioSource != null) && (audioSource.isPlaying == false))
		{
			if (audioSource.clip == null)
			{
				audioSource.clip = music;
			}
			audioSource.Play();

			didPlay = true;
		}

		if ((didPlay) || (AudioListener.volume <= 0.0))
		{
			FadeInVolume();
		}
	}


	/// <summary>
	/// Stops the music.
	/// </summary>
	/// <returns>The music.</returns>
	public void StopMusic()
	{
		if (music == null)
		{
			return;
		}

		if (audioSource != null)
		{
			audioSource.Stop();
		}
	}


	/// <summary>
	/// Fade out the volume to zero.
	/// </summary>
	/// <returns>The out volume.</returns>
	public void FadeOutVolume()
	{
		SsVolumeController.FadeVolumeOut(SsVolumeController.menuMusicFadeOutDuration);
	}


	/// <summary>
	/// Fade in the volume from zero.
	/// </summary>
	/// <returns>The out volume.</returns>
	public void FadeInVolume()
	{
		SsVolumeController.FadeVolumeIn(SsSettings.volume, SsVolumeController.menuMusicFadeInDuration);
	}


	/// <summary>
	/// Show from left, animate in.
	/// </summary>
	/// <returns>The from left.</returns>
	public void ShowFromLeft()
	{
		Show(fromDirections.fromLeft, false);
	}


	/// <summary>
	/// Show from right, animate in.
	/// </summary>
	/// <returns>The from right.</returns>
	public void ShowFromRight()
	{
		Show(fromDirections.fromRight, false);
	}


	/// <summary>
	/// Show from bottom, animate in.
	/// </summary>
	/// <returns>The from bottom.</returns>
	public void ShowFromBottom()
	{
		Show(fromDirections.fromBottom, false);
	}


	/// <summary>
	/// Show from top, animate in.
	/// </summary>
	/// <returns>The from top.</returns>
	public void ShowFromTop()
	{
		Show(fromDirections.fromTop, false);
	}


	/// <summary>
	/// Hide to left, animate out.
	/// </summary>
	/// <returns>The to left.</returns>
	public void HideToLeft()
	{
		Hide(false, toDirections.toLeft);
	}


	/// <summary>
	/// Hide to right, animate out.
	/// </summary>
	/// <returns>The to right.</returns>
	public void HideToRight()
	{
		Hide(false, toDirections.toRight);
	}


	/// <summary>
	/// Hide to bottom, animate out.
	/// </summary>
	/// <returns>The to bottom.</returns>
	public void HideToBottom()
	{
		Hide(false, toDirections.toBottom);
	}


	/// <summary>
	/// Hide to top, animate out.
	/// </summary>
	/// <returns>The to top.</returns>
	public void HideToTop()
	{
		Hide(false, toDirections.toTop);
	}


	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public virtual void Show(fromDirections fromDirection, bool snap)
	{
		if ((state == states.comingIn) || (state == states.visibleAndIdle))
		{
			return;
		}

		showCount ++;

		didShowOrHideAtLeastOnce = true;
		gameObject.SetActive(true);
		lastUpdateTime = Time.realtimeSinceStartup;		// Prevents jump in animation when enabled

		if ((music != null) && (playMusicOnShow))
		{
			PlayMusic();
		}

		if ((snap == false) && (doNotPlayShowSfx == false))
		{
			SsUiManager.PlayShowMenuSound(sfxShowMenu);
		}

		if (snap)
		{
			SetState(states.visibleAndIdle);
			ExecuteShowCallback();
		}
		else
		{
			SetState(states.comingIn);
		}
		hideTimer = 0.0f;

		// End at default position on screen
		endPos = Vector2.zero;

		if (fromDirection == fromDirections.invalid)
		{
			fromDirection = enterDirection;
		}
		lastInDirection = fromDirection;
		

		startPos = Vector2.zero;

		switch (fromDirection)
		{
		case fromDirections.fromLeft:
		{
			// From Left
			startPos = new Vector2(-1.0f, 0.0f);
			break;
		}
		case fromDirections.fromRight:
		{
			// From Right
			startPos = new Vector2(1.0f, 0.0f);
			break;
		}
		case fromDirections.fromTop:
		{
			// From Top
			startPos = new Vector2(0.0f, 1.0f);
			break;
		}
		case fromDirections.fromBottom:
		{
			// From Bottom
			startPos = new Vector2(0.0f, -1.0f);
			break;
		}
		} //switch


		if (rectTransform != null)
		{
			if (snap)
			{
				rectTransform.anchorMin = defaultRectAnchorMin + endPos;
				rectTransform.anchorMax = defaultRectAnchorMax + endPos;
			}
			else
			{
				rectTransform.anchorMin = defaultRectAnchorMin + startPos;
				rectTransform.anchorMax = defaultRectAnchorMax + startPos;
			}
		}

	}


	/// <summary>
	/// Hide the menu, and plays the out animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="snap">Snap to end position. Hide immediately.</param>
	/// <param name="outDirection">Out direction.</param>
	public virtual void Hide(bool snap, toDirections outDirection)
	{
		if ((state == states.goingOut) || (state == states.hidden))
		{
			return;
		}

		didShowOrHideAtLeastOnce = true;
		
		if (snap)
		{
			HideImmediate();
			return;
		}

		if (doNotPlayHideSfx == false)
		{
			SsUiManager.PlayHideMenuSound(sfxHideMenu);
		}

		SetState(states.goingOut);
		hideTimer = 0.0f;


		if (rectTransform != null)
		{
			// Start at current position
			startPos = rectTransform.anchorMin - defaultRectAnchorMin;
		}
		else
		{
			// Start at default position
			startPos = Vector2.zero;
		}


		
		if (outDirection == toDirections.invalid)
		{
			outDirection = exitDirection;
		}
		lastOutDirection = outDirection;
		
		switch (outDirection)
		{
		case toDirections.toLeft:
		{
			// To Left
			endPos = new Vector2(-1.0f, 0.0f);
			break;
		}
		case toDirections.toRight:
		{
			// To Right
			endPos = new Vector2(1.0f, 0.0f);
			break;
		}
		case toDirections.toTop:
		{
			// To Top
			endPos = new Vector2(0.0f, 1.0f);
			break;
		}
		case toDirections.toBottom:
		{
			// To Bottom
			endPos = new Vector2(0.0f, -1.0f);
			break;
		}
		
		} //switch

	}


	/// <summary>
	/// Hide the menu immediately (no animation).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	protected virtual void HideImmediate()
	{
		didShowOrHideAtLeastOnce = true;
		StopMusic();
		gameObject.SetActive(false);
		
		SetState(states.hidden);
		hideTimer = 0.0f;

		ExecuteHideCallback();
	}


	/// <summary>
	/// Sets the state.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="newState">New state.</param>
	public virtual void SetState(states newState)
	{
		state = newState;
		stateTime = 0.0f;
	}


	/// <summary>
	/// Executes the show callback.
	/// </summary>
	/// <returns>The show callback.</returns>
	private void ExecuteShowCallback()
	{
		if (showCallback == null)
		{
			return;
		}
		SsMenuCallback tempCallback = showCallback;
		showCallback = null;
		tempCallback();
	}


	/// <summary>
	/// Executes the hide callback.
	/// </summary>
	/// <returns>The hide callback.</returns>
	private void ExecuteHideCallback()
	{
		if (hideCallback == null)
		{
			return;
		}
		SsMenuCallback tempCallback = hideCallback;
		hideCallback = null;
		tempCallback();
	}


	/// <summary>
	/// Update this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Update()
	{
		float dt;

		unpausedDT = Time.realtimeSinceStartup - lastUpdateTime;
		lastUpdateTime = Time.realtimeSinceStartup;

		if (animateZeroTimeScale)
		{
			dt = unpausedDT;
		}
		else
		{
			dt = Time.deltaTime;
		}

		if (clampFrameRate)
		{
			if (dt > SsUiManager.minUpdateDt)
			{
				dt = SsUiManager.minUpdateDt;
			}
		}

		lastDt = dt;

		UpdateState(dt);
	}


	/// <summary>
	/// Updates the state (e.g. animations).
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="dt">Dt.</param>
	private void UpdateState(float dt)
	{
		Vector2 vec = endPos - startPos;
		float percent = 0.0f;
		
		stateTime += dt;

		switch (state)
		{
		case states.comingIn:
		{
			percent = Mathf.Clamp(stateTime / inDuration, 0.0f, 1.0f);

			// Tween animation
			percent = GeTween.QuadEaseInOut(percent, 0.0f, 1.0f, 1.0f);

			if (rectTransform != null)
			{
				rectTransform.anchorMin = defaultRectAnchorMin + startPos + (vec * percent);
				rectTransform.anchorMax = defaultRectAnchorMax + startPos + (vec * percent);
			}
			
			if (percent >= 1.0f)
			{
				SetState(states.visibleAndIdle);
				ExecuteShowCallback();
			}
			break;
		}
		case states.goingOut:
		{
			percent = Mathf.Clamp(stateTime / outDuration, 0.0f, 1.0f);

			// Tween animation
			percent = GeTween.QuadEaseInOut(percent, 0.0f, 1.0f, 1.0f);

			if (rectTransform != null)
			{
				rectTransform.anchorMin = defaultRectAnchorMin + startPos + (vec * percent);
				rectTransform.anchorMax = defaultRectAnchorMax + startPos + (vec * percent);
			}

			if (percent >= 1.0f)
			{
				HideImmediate();
			}
			break;
		}
		case states.hidden:
		{
			break;
		}
		case states.visibleAndIdle:
		{
			break;
		}
		} //switch
		
		
		if (hideTimer > 0.0f)
		{
			hideTimer -= dt;
			if (hideTimer <= 0.0f)
			{
				HideImmediate();
			}
		}
	}


}


