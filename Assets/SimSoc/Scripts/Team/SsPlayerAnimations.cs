using UnityEngine;
using System.Collections;

/// <summary>
/// Player animations. Properties for the animations and animation methods. Attach this to the player (or children).
/// </summary>
public class SsPlayerAnimations : MonoBehaviour {

	// Const/Static
	//-------------
	public const int animatorLayerIndex = 0;					// Animator component's layer index to use
	

	// Classes
	//--------
	// Animation properties
	[System.Serializable]
	public class SsAnimationProperties
	{
		[Tooltip("Name of animation in the mesh (e.g. fbx). Can leave blank if the animation name is the same as the state name.")]
		public string animationName;		// NOTE: Use finalAnimationName to read the animation name.
		[Tooltip("State for which to use the animation.")]
		public SsPlayer.states state;
		
		[Space(10)]
		[Tooltip("Name of object/bone to which to attach the ball for this animation (e.g. player's hand when throwing in).")]
		public string attachBallTo;
		[Tooltip("Attach ball position offset.")]
		public Vector3 attachBallOffset;
		[Tooltip("The direction in which to adjust the ball's attach position by the ball's radius.")]
		public Vector3 attachBallRadiusDir;
		
		[Space(10)]
		[Tooltip("Release ball at this time (e.g. kick ball, throw in ball). This should be less than the animation " +
		         "duration.")]
		public float releaseBallTime;
		
		[System.NonSerialized]
		public int hashCode;				// Hash code, if using an Animator component
		
		[System.NonSerialized]
		public string finalAnimationName;	// Final animation name to use. Might be converted to lowercase.
		
		[System.NonSerialized]
		public GameObject attachBallToGo;	// Game object to which to attach the ball.
	}



	// Public
	//-------
	[Space(10)]
	[Tooltip("The animator controller to use. (Optional. Only needed if the player is animated via a controller, and the mesh's Animator does not have one specified.)")]
	public RuntimeAnimatorController runtimeAnimatorController;
	
	[Tooltip("Cross fade (blending) time. (0 = no blending)")]
	public float crossFadeTime = 0.2f;

	[Space(10)]
	[Tooltip("Use medium kick animation when distance to kick is further than this.")]
	public float mediumKickDistance = 10.0f;
	[Tooltip("Use far kick animation when distance to kick is further than this.")]
	public float farKickDistance = 30.0f;
	
	[Space(10)]
	[Tooltip("Goalkeeper use medium throw in animation when distance to throw is further than this.")]
	public float gkMediumThrowDistance = 10.0f;
	[Tooltip("Goalkeeper use far throw animation when distance to throw is further than this.")]
	public float gkFarThrowDistance = 30.0f;

	[Space(10)]
	[Tooltip("Animation properties. Links states to mesh animations.")]
	public SsAnimationProperties[] animationProperties;



	// Private
	//--------
	private Animation anim;						// Animation
	private Animator animator;					// Animator
	private float animationSpeed;				// Animation speed

	private SsAnimationProperties[] animationPropertiesSorted;	// Sorted animation properties. Sorted via states.

	private string animaName;					// Current animation name
	private int animaHashCode;					// Current animation hash code (if using Animator component).



	// Properties
	//-----------
	/// <summary>
	/// Get the animation speed. Use SetAnimationSpeed to change it.
	/// </summary>
	/// <value>The animation speed.</value>
	public float AnimationSpeed
	{
		get { return(animationSpeed); }
	}


	/// <summary>
	/// Current animation name.
	/// </summary>
	/// <value>The name of the anima.</value>
	public string AnimaName
	{
		get { return(animaName); }
	}



	// Methods
	//--------
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		anim = gameObject.GetComponentInChildren<Animation>();
		animator = gameObject.GetComponentInChildren<Animator>();
		if (animator != null)
		{
			// Disable root motion, so that the animation does Not change the mesh's position.
			animator.applyRootMotion = false;
			
			if (animator.runtimeAnimatorController == null)
			{
				animator.runtimeAnimatorController = runtimeAnimatorController;
			}
		}

		InitAnimations();
	}


	/// <summary>
	/// Reset.
	/// </summary>
	/// <returns>The me.</returns>
	public void ResetMe()
	{
		animaHashCode = -1;
		SetAnimationSpeed(1.0f);
	}


	/// <summary>
	/// Initialise animations. Creates the sorted animation properties.
	/// </summary>
	/// <returns>The sorted animation properties.</returns>
	void InitAnimations()
	{
		int i, n;
		SsAnimationProperties props;
		SsPlayer.states state;
		string defaultName, lowerName;
		Transform t;
		
		animationPropertiesSorted = new SsAnimationProperties[(int)SsPlayer.states.maxStates];
		
		for (i = 0; i < animationPropertiesSorted.Length; i++)
		{
			state = (SsPlayer.states)i;
			animationPropertiesSorted[i] = null;
			
			// Check if properties already exist
			for (n = 0; n < animationProperties.Length; n++)
			{
				props = animationProperties[n];
				if ((props != null) && ((int)props.state == i))
				{
					animationPropertiesSorted[i] = props;
					props.finalAnimationName = props.animationName;
					break;
				}
			}
			
			if (animationPropertiesSorted[i] == null)
			{
				// Create new properties
				animationPropertiesSorted[i] = new SsAnimationProperties();
				props = animationPropertiesSorted[i];
				props.state = state;
				props.animationName = state.ToString();
				props.finalAnimationName = props.animationName;
			}
			else
			{
				props = animationPropertiesSorted[i];
			}
			
			if (props != null)
			{
				// Check animation name case sensitivity
				if (string.IsNullOrEmpty(props.finalAnimationName) == false)
				{
					defaultName = props.finalAnimationName;
					lowerName = defaultName.ToLower();
					if (anim != null)
					{
						// Animation component
						if ((anim[defaultName] == null) && 
						    (anim[lowerName] != null))
						{
							// Use lower case name
							props.finalAnimationName = lowerName;
						}
					}
					else if (animator != null)
					{
						// Animator component
						if ((animator.HasState(animatorLayerIndex, Animator.StringToHash(defaultName)) == false) && 
						    (animator.HasState(animatorLayerIndex, Animator.StringToHash(lowerName))))
						{
							// Use lower case name
							props.finalAnimationName = lowerName;
						}
					}
				}
				
				props.hashCode = Animator.StringToHash(props.finalAnimationName);
				
				if (string.IsNullOrEmpty(props.attachBallTo) == false)
				{
					t = GeUtils.FindChild(transform, props.attachBallTo);
					if (t != null)
					{
						props.attachBallToGo = t.gameObject;
					}
#if UNITY_EDITOR
					else
					{
						SsPlayer player = gameObject.GetComponentInParent<SsPlayer>();
						Debug.LogWarning("Player  [" + player.GetAnyName() + "]  had no child named  [" + props.attachBallTo + "]  " + 
						                 "for animation  [" + props.animationName + "].");
					}
#endif //UNITY_EDITOR
				}
			}
		}
		
	}


	/// <summary>
	/// Get the animation name for the specified state.
	/// </summary>
	/// <returns>The animation name.</returns>
	/// <param name="state">State.</param>
	public string GetAnimationName(SsPlayer.states state)
	{
		int i = (int)state;
		
		if ((i >= 0) && (animationPropertiesSorted != null) && (i < animationPropertiesSorted.Length) && 
		    (animationPropertiesSorted[i] != null))
		{
			return (animationPropertiesSorted[i].finalAnimationName);
		}
		return (state.ToString());
	}
	
	
	/// <summary>
	/// Get the animation properties for the specified state.
	/// </summary>
	/// <returns>The animation properties.</returns>
	/// <param name="state">State.</param>
	public SsAnimationProperties GetAnimationProperties(SsPlayer.states state)
	{
		int i = (int)state;
		
		if ((i >= 0) && (animationPropertiesSorted != null) && (i < animationPropertiesSorted.Length))
		{
			return (animationPropertiesSorted[i]);
		}
		return (null);
	}
	
	
	/// <summary>
	/// Get the animation hash code for the specified state.
	/// </summary>
	/// <returns>The animation hash code.</returns>
	/// <param name="state">State.</param>
	public int GetAnimationHashCode(SsPlayer.states state)
	{
		int i = (int)state;
		
		if ((i >= 0) && (animationPropertiesSorted != null) && (i < animationPropertiesSorted.Length) && 
		    (animationPropertiesSorted[i] != null))
		{
			return (animationPropertiesSorted[i].hashCode);
		}
		return (-1);
	}
	
	
	/// <summary>
	/// Get the release ball time for the specified state.
	/// </summary>
	/// <returns>The release ball time.</returns>
	/// <param name="state">State.</param>
	public float GetReleaseBallTime(SsPlayer.states state)
	{
		int i = (int)state;
		
		if ((i >= 0) && (animationPropertiesSorted != null) && (i < animationPropertiesSorted.Length) && 
		    (animationPropertiesSorted[i] != null))
		{
			return (animationPropertiesSorted[i].releaseBallTime);
		}
		return (0.0f);
	}


	/// <summary>
	/// Play the specified animation, using the animation name (if using an Animation component) or hashcode (if using an Animator component).
	/// In most cases, rather call SetState instead of PlayAnimation.
	/// </summary>
	/// <returns>The animation.</returns>
	/// <param name="animaName">Animation name. Used by Animation component.</param>
	/// <param name="animaHashCode">Animation hash code. Used by Animator component.</param>
	public bool PlayAnimation(string animaName, int animaHashCode)
	{
		return (PlayAnimation(animaName, animaHashCode, crossFadeTime));
	}
	
	
	/// <summary>
	/// Play the specified animation, using the animation name (if using an Animation component) or hashcode (if using an Animator component).
	/// In most cases, rather call SetState instead of PlayAnimation.
	/// </summary>
	/// <returns>The animation.</returns>
	/// <param name="animaName">Animation name. Used by Animation component.</param>
	/// <param name="animaHashCode">Animation hash code. Used by Animator component.</param>
	/// <param name="fadeTime">Cross fade time. 0 = no fading.</param>
	public bool PlayAnimation(string animaName, int animaHashCode, 
	                          float fadeTime)
	{
		if (string.IsNullOrEmpty(animaName) == false)
		{
			this.animaName = animaName;
		}
		this.animaHashCode = animaHashCode;
		
		if (anim != null)
		{
			// Animation component
			if (string.IsNullOrEmpty(animaName) == false)
			{
				if ((anim != null) && (anim[animaName] != null))
				{
					if (fadeTime > 0.0f)
					{
						anim.CrossFade(animaName, fadeTime);
						return (true);
					}
					
					return (anim.Play(animaName));
				}
			}
		}
		else if (animator != null)
		{
			// Animator component
			if (animator.HasState(animatorLayerIndex, animaHashCode))
			{
				if (fadeTime > 0.0f)
				{
					animator.CrossFadeInFixedTime(animaHashCode, fadeTime, animatorLayerIndex, 0.0f);
				}
				else
				{
					animator.Play(animaHashCode, animatorLayerIndex);
				}
				return (true);
			}
		}
		
		return (false);
	}
	
	
	/// <summary>
	/// Test if the player has the specified animation.
	/// </summary>
	/// <returns><c>true</c> if this instance has animation the specified animaName; otherwise, <c>false</c>.</returns>
	/// <param name="animaName">Animation name. Used by Animation component.</param>
	/// <param name="animaHashCode">Animation hash code. Used by Animator component.</param>
	public bool HasAnimation(string animaName, int animaHashCode)
	{
		if (anim != null)
		{
			// Animation component
			if (string.IsNullOrEmpty(animaName) == false)
			{
				return (anim[animaName] != null);
			}
		}
		else if (animator != null)
		{
			// Animator component
			return (animator.HasState(animatorLayerIndex, animaHashCode));
		}
		
		return (false);
	}
	
	
	/// <summary>
	/// Test if player has animation for the specified state.
	/// </summary>
	/// <returns><c>true</c> if this instance has animation the specified forState; otherwise, <c>false</c>.</returns>
	/// <param name="forState">For state.</param>
	public bool HasAnimation(SsPlayer.states forState)
	{
		return (HasAnimation(GetAnimationName(forState), GetAnimationHashCode(forState)));
	}
	
	
	/// <summary>
	/// Test if the current animation is still playing.
	/// </summary>
	/// <returns><c>true</c> if this instance is animation playing the specified assumeStoppedIfPlayedOnce; otherwise, <c>false</c>.</returns>
	/// <param name="assumeStoppedIfPlayedOnce">Assume it stopped if it played at least once, when it is looping or clamping forever, etc.</param>
	public bool IsAnimationPlaying(bool assumeStoppedIfPlayedOnce = true)
	{
		if (anim != null)
		{
			// Animation component
			if (string.IsNullOrEmpty(animaName) == false)
			{
				if (anim.IsPlaying(animaName))
				{
					if ((assumeStoppedIfPlayedOnce) && 
					    (anim[animaName].normalizedTime >= 1.0f))
					{
						return (false);
					}
					return (true);
				}
			}
		}
		else if (animator != null)
		{
			// Animator component
			AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(animatorLayerIndex);
			if (info.shortNameHash != animaHashCode)
			{
				// Check the next animation (if busy transitioning to a next animation)
				info = animator.GetNextAnimatorStateInfo(animatorLayerIndex);
			}
			if (info.shortNameHash == animaHashCode)
			{
				if ((assumeStoppedIfPlayedOnce) && 
				    (info.normalizedTime >= 1.0f) && (animator.IsInTransition(animatorLayerIndex) == false))
				{
					return (false);
				}
				return (true);
			}
		}
		
		return (false);
	}
	
	
	/// <summary>
	/// Get the time of the current animation.
	/// </summary>
	/// <returns>The animation time.</returns>
	public float GetAnimationTime()
	{
		if (anim != null)
		{
			// Animation component
			if (string.IsNullOrEmpty(animaName) == false)
			{
				if (anim.IsPlaying(animaName))
				{
					return (anim[animaName].time);
				}
			}
		}
		else if (animator != null)
		{
			// Animator component
			AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(animatorLayerIndex);
			if (info.shortNameHash == animaHashCode)
			{
				return (info.length * info.normalizedTime);
			}
		}
		
		return (0.0f);
	}
	
	
	/// <summary>
	/// Set the animation playback speed.
	/// </summary>
	/// <returns>The animation speed.</returns>
	/// <param name="speed">Speed.</param>
	public void SetAnimationSpeed(float speed)
	{
		if (animationSpeed == speed)
		{
			return;
		}
		animationSpeed = speed;
		
		if (anim != null)
		{
			foreach (AnimationState state in anim)
			{
				state.speed = speed;
			}
		}
		else if (animator != null)
		{
			animator.speed = speed;
		}
	}


}
