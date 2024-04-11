using UnityEngine;
using System.Collections;

/// <summary>
/// Fake shadow for players and balls. It is positioned on the ground, and is usually just a quad with a texture.
/// The object (player/ball) must call UpdatePosition to update the shadow's position.
/// </summary>
public class SsFakeShadow : MonoBehaviour {

	// Const/Static
	//-------------
	public const float offsetY = 0.01f;					// Shadow offset above the ground


	// Enums
	//------
	// Specify when the shadow should update
	public enum shadowUpdateModes
	{
		inLateUpdate = 0,				// Update in the LateUpdate method.
		inFixedUpdate,					// Update in the FixedUpdate method.

		// REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.
		
		maxModes
	}


	// Public
	//-------
	[Space(10)]
	[Tooltip("Shadow's scale when the object is on the ground.")]
	public float scaleOnGround = 1.0f;
	[Tooltip("Shadow's scale when the object is at max height.")]
	public float scaleAtMaxHeight = 0.5f;
	[Tooltip("Max height used for scaling.")]
	public float maxHeight = 30.0f;

	[Space(10)]
	[Tooltip("Specify when the shadow should be updated (e.g. in LateUpdate method, or in FixedUpdate method). The player's shadow is usually in LateUpdate and the ball's in FixedUpdate.")]
	public shadowUpdateModes updateMode = shadowUpdateModes.inLateUpdate;

	[Tooltip("Update the shadow's rotation so that it points in the same direction as the player. Only applies to the player's shadows. (Set this to false for optimisation.)")]
	public bool updateRotation;


	[Space(10)]
	[Tooltip("Use a light in the level to calculate the shadow's position. Otherwise the shadow will be drawn directly beneath the object. (For optimisation: Set players to false and ball to true. Or all to false if you do not need to use the light.)")]
	public bool useLight;
	[Tooltip("The type of light to use. It will use the first light of this type it finds in the level.")]
	public LightType lightType = LightType.Directional;
	


	// Private
	//--------
	static private GameObject fakeShadowHolder;		// Holder for all the fake shadows. The shadows are attached to the player/ball when spawned, but then attached to the holder after initialisation.

	private SsFieldProperties field;				// Field reference.

	private SsPlayer player;						// The player to which the shadow is attached.
	private SsBall ball;							// The ball to which the shadow is attached.

	private Light sceneLight;						// Reference for a light in the scene
	private Ray lightRay;							// Light ray
	private Vector3 lightPrevPos;					// Light previous position
	private Vector3 lightPrevRotation;				// Light previous rotation

	private Plane groundPlane;						// Ground plane
	private float groundPlaneY;						// Ground plane Y



	// Methods
	//--------

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start()
	{
		field = SsFieldProperties.Instance;

		player = gameObject.GetComponentInParent<SsPlayer>();
		ball = gameObject.GetComponentInParent<SsBall>();

		if (fakeShadowHolder == null)
		{
			fakeShadowHolder = new GameObject("Fake Shadows");
		}

		if (fakeShadowHolder != null)
		{
			// Attach to the holder
			transform.parent = fakeShadowHolder.transform;
			transform.localPosition = Vector3.zero;
			transform.rotation = Quaternion.identity;
		}

		if (useLight)
		{
			Light[] lights = FindObjectsOfType<Light>();
			Light tempLight;
			int i;

			if ((lights != null) && (lights.Length > 0))
			{
				for (i = 0; i < lights.Length; i++)
				{
					tempLight = lights[i];
					if ((tempLight != null) && (tempLight.type == lightType))
					{
						sceneLight = tempLight;
						break;
					}
				}
			}

			if (sceneLight != null)
			{
				lightPrevPos = sceneLight.transform.position;
				lightPrevRotation = sceneLight.transform.rotation.eulerAngles;
				lightRay = new Ray(sceneLight.transform.position, sceneLight.transform.forward);
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogError("ERROR: A fake shadow (" + name + ") is set to use a light, but no light of the specified type (" + lightType + ") was found in the scene (" + GeUtils.GetLoadedLevelName() + ").");
			}
#endif //UNITY_EDITOR
		}

		groundPlaneY = field.GroundY + offsetY;
		groundPlane = new Plane(Vector3.up, new Vector3(0.0f, groundPlaneY, 0.0f));

		UpdatePosition();
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		field = null;
		player = null;
		ball = null;
		sceneLight = null;
	}


	/// <summary>
	/// Lates the update.
	/// </summary>
	void LateUpdate()
	{
		if (updateMode == shadowUpdateModes.inLateUpdate)
		{
			UpdatePosition();
		}
	}


	/// <summary>
	/// Fixeds the update.
	/// </summary>
	void FixedUpdate()
	{
		if (updateMode == shadowUpdateModes.inFixedUpdate)
		{
			UpdatePosition();
		}
	}


	/// <summary>
	/// Updates the position.
	/// </summary>
	public void UpdatePosition()
	{
		Vector3 parentPos, targetPos;
		float scale;

		if (player != null)
		{
			parentPos = player.transform.position;

			if (updateRotation)
			{
				transform.rotation = player.transform.rotation;
			}
		}
		else if (ball != null)
		{
			if ((updateMode == shadowUpdateModes.inFixedUpdate) && (ball.Rb != null))
			{
				parentPos = ball.Rb.position - (Vector3.up * ball.radius);
			}
			else
			{
				parentPos = ball.transform.position - (Vector3.up * ball.radius);
			}
		}
		else
		{
			return;
		}

		scale = GeUtils.Lerp(parentPos.y - field.GroundY, 0.0f, maxHeight, scaleOnGround, scaleAtMaxHeight);
		if (transform.localScale.x != scale)
		{
			transform.localScale = new Vector3(scale, scale, scale);
		}

		// Ground pos beneath the parent
		targetPos = new Vector3(parentPos.x, field.GroundY + offsetY, parentPos.z);

		if ((useLight) && (sceneLight != null))
		{
			float rayDistance;

			if ((lightPrevPos != sceneLight.transform.position) || (lightPrevRotation != sceneLight.transform.rotation.eulerAngles))
			{
				lightPrevPos = sceneLight.transform.position;
				lightPrevRotation = sceneLight.transform.rotation.eulerAngles;
				lightRay.direction = sceneLight.transform.forward;
			}

			if (parentPos.y <= groundPlaneY)
			{
				parentPos.y = groundPlaneY + 0.001f;
			}
			lightRay.origin = parentPos;

			if (groundPlane.Raycast(lightRay, out rayDistance))
			{
				targetPos = lightRay.GetPoint(rayDistance);
			}
			else
			{
				targetPos = transform.position;
			}
		}

		transform.position = targetPos;
	}

}
