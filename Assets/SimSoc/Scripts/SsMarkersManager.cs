using UnityEngine;
using System.Collections;

/// <summary>
/// Markers manager. For markers that do Not belong to a player (e.g. mouse marker, ball out marker).
/// </summary>
public class SsMarkersManager : MonoBehaviour {

	// Const/Static
	//-------------
	public const float offsetY = 0.01f;				// Offset above the ground


	// Enums
	//------
	// Marker types
	public enum markerTypes
	{
		none = 0,
		mouse,
		ballOut,
	}


	// Private
	//--------
	static private SsMarkersManager instance;

	private SsMatch match;							// Reference to the match
	private bool addedStateAction;

	private GameObject markerMouse;
	private float markerMouseRotateSpeed;
	private GameObject markerBallOut;



	// Properties
	//-----------
	static public SsMarkersManager Instance
	{
		get { return(instance); }
	}



	// Methods
	//--------
	/// <summary>
	/// Creates the instance.
	/// </summary>
	/// <returns>The instance.</returns>
	static public void CreateInstance()
	{
		if (instance == null)
		{
			GameObject go = new GameObject("Markers Manager");
			if (go != null)
			{
				go.AddComponent<SsMarkersManager>();
			}
		}
	}


	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		instance = this;
	}


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		GetReferences();
	}


	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy()
	{
		instance = null;
		CleanUp();
	}


	/// <summary>
	/// Clean up. Includes freeing resources and clearing references.
	/// </summary>
	/// <returns>The up.</returns>
	void CleanUp()
	{
		match = null;
	}


	/// <summary>
	/// Raises the enable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	void OnEnable()
	{
		AddCallbacks();
	}
	
	
	/// <summary>
	/// Raises the disable event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	void OnDisable()
	{
		RemoveCallbacks();
	}
	
	
	/// <summary>
	/// Adds the callbacks.
	/// </summary>
	/// <returns>The callbacks.</returns>
	private void AddCallbacks()
	{
		if ((SsMatch.Instance != null) && (addedStateAction == false))
		{
			SsMatch.Instance.onStateChanged += OnMatchStateChanged;
			addedStateAction = true;
		}
	}
	
	
	/// <summary>
	/// Removes the callbacks.
	/// </summary>
	/// <returns>The callbacks.</returns>
	private void RemoveCallbacks()
	{
		if ((SsMatch.Instance != null) && (addedStateAction))
		{
			SsMatch.Instance.onStateChanged -= OnMatchStateChanged;
			addedStateAction = false;
		}
	}


	/// <summary>
	/// Gets the references.
	/// </summary>
	/// <returns>The references.</returns>
	void GetReferences()
	{
		if ((SsMatch.Instance == null) || (SsMatch.Instance.IsLoading(true)))
		{
			// Match is busy loading
			Invoke("GetReferences", 0.1f);
			return;
		}

		match = SsMatch.Instance;
		AddCallbacks();
	}


	/// <summary>
	/// Attach and disable the marker.
	/// </summary>
	/// <returns>The and disable marker.</returns>
	/// <param name="marker">Marker.</param>
	public void AttachAndDisableMarker(GameObject marker, markerTypes type, float rotateSpeed)
	{
		if (marker != null)
		{
			marker.transform.parent = transform;
			marker.SetActive(false);

			if (type == markerTypes.mouse)
			{
				markerMouse = marker;
				markerMouseRotateSpeed = rotateSpeed;
			}
			else if (type == markerTypes.ballOut)
			{
				markerBallOut = marker;
			}
		}
	}


	/// <summary>
	/// Raises the match state changed event.
	/// </summary>
	/// <param name="newState">New state.</param>
	private void OnMatchStateChanged(SsMatch.states newState)
	{
		if ((newState == SsMatch.states.throwIn) || 
		    (newState == SsMatch.states.goalKick) || 
		    (newState == SsMatch.states.cornerKick))
		{
			ShowOutMarker(false);
		}
	}


	/// <summary>
	/// Shows the mouse marker.
	/// </summary>
	/// <returns>The mouse marker.</returns>
	/// <param name="visible">Visible.</param>
	public void ShowMouseMarker(bool visible)
	{
		if ((markerMouse != null) && (markerMouse.activeInHierarchy != visible))
		{
			markerMouse.SetActive(visible);
		}
	}


	/// <summary>
	/// Sets the position of the mouse marker.
	/// </summary>
	/// <returns>The mouse position.</returns>
	/// <param name="pos">Position.</param>
	public void SetMousePosition(Vector3 pos)
	{
		if (markerMouse != null)
		{
			markerMouse.transform.position = new Vector3(pos.x, pos.y + offsetY, pos.z);
			if (markerMouse.activeInHierarchy == false)
			{
				markerMouse.SetActive(true);
			}
		}
	}


	/// <summary>
	/// Shows/hide out marker.
	/// </summary>
	/// <returns>The out marker.</returns>
	/// <param name="visible">Visible.</param>
	public void ShowOutMarker(bool visible)
	{
		if ((markerBallOut != null) && (markerBallOut.activeInHierarchy != visible))
		{
			markerBallOut.SetActive(visible);
		}
	}


	/// <summary>
	/// Position the out marker.
	/// </summary>
	/// <returns>The out position.</returns>
	/// <param name="pos">Position.</param>
	public void SetOutPosition(Vector3 pos)
	{
		if (markerBallOut != null)
		{
			markerBallOut.transform.position = new Vector3(pos.x, pos.y, pos.z);
			if (markerBallOut.activeInHierarchy == false)
			{
				markerBallOut.SetActive(true);
			}
		}
	}


	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		UpdateMarkers();
	}


	/// <summary>
	/// Updates the markers.
	/// </summary>
	/// <returns>The markers.</returns>
	void UpdateMarkers()
	{
		if (match == null)
		{
			return;
		}

		float dt = Time.deltaTime;

		// Mouse marker
		if ((markerMouse != null) && (markerMouse.activeInHierarchy) && (markerMouseRotateSpeed > 0.0f))
		{
			markerMouse.transform.RotateAround(markerMouse.transform.position, Vector3.up, markerMouseRotateSpeed * dt);
		}

	}

}
