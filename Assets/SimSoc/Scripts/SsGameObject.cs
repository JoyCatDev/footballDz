using UnityEngine;
using System.Collections;

/// <summary>
/// Base game object used in a match (e.g. players, ball).
/// </summary>
public class SsGameObject : MonoBehaviour {

	// Enums
	//------
	// Special object types
	public enum objectTypes
	{
		unknown = 0,
		player,
		ball,

		// REMINDER: ADD NEW ENUMS ABOVE THIS LINE. DO NOT CHANGE THE ORDER.
		
		maxTypes
	}


	// Private
	//--------
	protected SsMatch match;					// Reference to the match
	protected SsFieldProperties field;			// Reference to the field

	protected int zone;							// Zone index. There's a delay before it's updated, in case running between 2 zone boundaries.
	protected int zoneTransformed;				// Zone index transformed based on the team's play direction.
	protected int zonePending;					// Zone to change to after timer runs out.
	protected float zonePendingTimer;			// Timer to delay switching zones.

	protected int row;							// Row index.

	protected SsFieldProperties.SsGrid grid;	// Grid the object is on.

	protected Rect visibleRect = new Rect(Vector2.zero, Vector2.one);	// 2D rect for determining the object's visibility. Local.
	protected Rect screenRect;					// The object's screen rect. Use IsVisble to test if object is visible.
	protected bool isVisible;					// Is the object visible? (i.e. it's screen rect if seen by the camera)
	protected bool behindCamera;				// Is the object behind the camera?
	protected bool isPartiallyOffScreen;		// Is object off screen or partially off screen?

	private objectTypes objectType;				// Object type
	private int gridWeight;						// Weight to add to a grid

	// Radar position (e.g. used for a mini-map radar). If within field then range is -1 to 1 (left to right, bottom to top) relative to field centre.
	private Vector2 radarPos;


	// Properties
	//-----------
	public objectTypes ObjectType
	{
		get { return(objectType); }
	}


	public int Zone
	{
		get { return(zone); }
	}
	
	
	public int ZoneTransformed
	{
		get { return(zoneTransformed); }
	}
	
	
	public int ZonePending
	{
		get { return(zonePending); }
	}


	public int Row
	{
		get { return(row); }
	}


	public SsFieldProperties.SsGrid Grid
	{
		get { return(grid); }
	}



	/// <summary>
	/// Visible rectangle, in world coordinates size, but position is local coordinates.
	/// </summary>
	/// <value>The visible rect.</value>
	public Rect VisibleRect
	{
		get { return(visibleRect); }
	}


	/// <summary>
	/// Rectangle on screen. Aspect ratio may change, and sometimes width or height may be negative. 
	/// Use IsVisble to test if object is visible.
	/// </summary>
	/// <value>The screen rect.</value>
	public Rect ScreenRect
	{
		get { return(screenRect); }
	}


	/// <summary>
	/// Is the object visible (i.e. is its screen rect on screen)?
	/// </summary>
	/// <value><c>true</c> if this instance is visible; otherwise, <c>false</c>.</value>
	public bool IsVisible
	{
		get { return(isVisible); }
	}


	/// <summary>
	/// Test if object is behind the camera. Object should be invisible, but its safer to use IsVisibleAndInfrontOfCamera to be sure.
	/// </summary>
	/// <value>The behind camera.</value>
	public bool BehindCamera
	{
		get { return(behindCamera); }
	}


	/// <summary>
	/// Is the object visible and in front of the camera?
	/// </summary>
	/// <value><c>true</c> if this instance is visible and infront of camera; otherwise, <c>false</c>.</value>
	public bool IsVisibleAndInfrontOfCamera
	{
		get { return(IsVisible && !BehindCamera); }
	}


	/// <summary>
	/// Is object off screen or partially off screen?
	/// </summary>
	/// <value><c>true</c> if this instance is partially off screen; otherwise, <c>false</c>.</value>
	public bool IsPartiallyOffScreen
	{
		get { return(isPartiallyOffScreen); }
	}


	/// <summary>
	/// Radar position (e.g. used for a mini-map radar). If within field then range is -1 to 1 (left to right, bottom to top) relative to field centre.
	/// </summary>
	/// <value>The radar position.</value>
	public Vector2 RadarPos
	{
		get { return(radarPos); }
	}



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Awake()
	{
		if ((this as SsPlayer) != null)
		{
			objectType = objectTypes.player;
			gridWeight = 1;
		}
		else if ((this as SsBall) != null)
		{
			objectType = objectTypes.ball;
		}
	}
	

	/// <summary>
	/// Start this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Start()
	{
		match = SsMatch.Instance;
		field = SsFieldProperties.Instance;

		ResetMe();
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void OnDestroy()
	{
		CleanUp();
	}


	/// <summary>
	/// Clean up. Includes freeing resources and clearing references.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The up.</returns>
	public virtual void CleanUp()
	{
		match = null;
		field = null;
		ClearGrid();
	}


	/// <summary>
	/// Reset this instance.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void ResetMe()
	{
		zone = -1;
		zonePending = -1;
		zonePendingTimer = 0.0f;
		
		screenRect = new Rect(Vector2.zero, Vector2.one);
		isVisible = false;
		isPartiallyOffScreen = false;

		ClearGrid();
	}


	/// <summary>
	/// Raises the enable event.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void OnEnable()
	{
	}


	/// <summary>
	/// Raises the disable event.
	/// NOTE: Derived method must call the base method.
	/// </summary>
	public virtual void OnDisable()
	{
	}


	/// <summary>
	/// Clears the grid.
	/// </summary>
	/// <returns>The grid.</returns>
	public void ClearGrid()
	{
		UpdateGrid(0.0f, true);
	}


	/// <summary>
	/// Calc the visible rect, based on a box collider attached to the object.
	/// </summary>
	/// <returns>The visible rect.</returns>
	/// <param name="bc">Bc.</param>
	public void CalcVisibleRect(BoxCollider bc)
	{
		if (bc == null)
		{
			return;
		}

		Vector3 scale = new Vector3(Mathf.Abs(bc.transform.lossyScale.x), 
		                            Mathf.Abs(bc.transform.lossyScale.y),
		                            Mathf.Abs(bc.transform.lossyScale.z));

		// Use average X and Z for centre
		visibleRect.size = new Vector2(Mathf.Max(Mathf.Abs(bc.size.x), Mathf.Abs(bc.size.z)) * Mathf.Max(scale.x, scale.z),
		                               Mathf.Abs(bc.size.y) * scale.y);
		visibleRect.center = new Vector2(((bc.center.x + bc.center.z) / 2.0f) * ((scale.x + scale.z) / 2.0f), 
		                                 bc.center.y * scale.y);
	}


	/// <summary>
	/// calc the visible ret, based on a sphere collider attached to the object.
	/// </summary>
	/// <returns>The visible rect.</returns>
	/// <param name="sc">Sc.</param>
	public void CalcVisibleRect(SphereCollider sc)
	{
		if (sc == null)
		{
			return;
		}

		Vector3 scale = new Vector3(Mathf.Abs(sc.transform.lossyScale.x), 
		                            Mathf.Abs(sc.transform.lossyScale.y),
		                            Mathf.Abs(sc.transform.lossyScale.z));
		float radius = sc.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));

		// Use average X and Z for centre
		visibleRect.size = new Vector2(radius, radius);
		visibleRect.center = new Vector2(((sc.center.x + sc.center.z) / 2.0f) * ((scale.x + scale.z) / 2.0f), 
		                                 sc.center.y * scale.y);
	}


	/// <summary>
	/// Update.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public virtual void Update()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

	}


	/// <summary>
	/// Late update.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The update.</returns>
	public virtual void LateUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

	}


	/// <summary>
	/// Fixed update.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The update.</returns>
	public virtual void FixedUpdate()
	{
		if ((match == null) || (match.IsLoading(true)))
		{
			// Match is busy loading
			return;
		}

	}


	/// <summary>
	/// Update the object's zone, grid, rows and visibility.
	/// IMPORTANT: Call this from the relevant object's Update or FixedUpdate method after the object has moved.
	/// </summary>
	/// <returns>The zone and visibility.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="immediate">Immediate.</param>
	/// <param name="team">Team, if this is a player.</param>
	/// <param name="field">Field.</param>
	public void UpdateZoneAndVisibility(float dt, bool immediate, SsTeam team, SsFieldProperties field)
	{
		if (field == null)
		{
			return;
		}
		
		int newZone = field.GetZoneAtPoint(transform.position);
		bool update = true;
		Camera cam = (SsMatchCamera.Instance != null) ? SsMatchCamera.Instance.Cam : null;


		// Zone
		//-----
		if (immediate)
		{
			zone = newZone;
			zonePending = -1;
			zonePendingTimer = 0.0f;
		}
		else if ((zone != newZone) && (zonePending != newZone))
		{
			zonePending = newZone;
			zonePendingTimer = Time.time + 0.2f;
			update = false;
		}
		
		if ((update) && (immediate == false))
		{
			if ((zonePendingTimer > 0.0f) && (zonePendingTimer < Time.time))
			{
				zonePendingTimer = 0.0f;
				if (zone != zonePending)
				{
					zone = newZone;
					zonePending = -1;
				}
			}
		}
		
		if ((team == null) || (team.PlayDirection > 0.0f))
		{
			zoneTransformed = zone;
		}
		else
		{
			zoneTransformed = SsFieldProperties.maxZones - 1 - zone;
		}


		// Row
		//----
		row = field.GetRowAtPoint(transform.position);


		// Grid
		//-----
		UpdateGrid(dt, false);


		// Radar position
		//---------------
		radarPos = field.GetRadarPos(this);


		// Visiblity
		//----------
		isVisible = false;
		behindCamera = false;
		isPartiallyOffScreen = false;
		if (cam != null)
		{
			Vector3 tl, tr, br, bl;

			tl = transform.position + new Vector3(visibleRect.center.x - (visibleRect.width / 2.0f), 
			                                      visibleRect.center.y + (visibleRect.height / 2.0f), 
			                                      0.0f);
			tr = transform.position + new Vector3(visibleRect.center.x + (visibleRect.width / 2.0f), 
			                                      visibleRect.center.y + (visibleRect.height / 2.0f), 
			                                      0.0f);
			br = transform.position + new Vector3(visibleRect.center.x + (visibleRect.width / 2.0f), 
			                                      visibleRect.center.y - (visibleRect.height / 2.0f), 
			                                      0.0f);
			bl = transform.position + new Vector3(visibleRect.center.x - (visibleRect.width / 2.0f), 
			                                      visibleRect.center.y - (visibleRect.height / 2.0f), 
			                                      0.0f);

			tl = cam.WorldToScreenPoint(tl);
			tr = cam.WorldToScreenPoint(tr);
			br = cam.WorldToScreenPoint(br);
			bl = cam.WorldToScreenPoint(bl);

			// If behind camera then invert x & y
			if (tl.z < 0.0f)
			{
				tl.x = -tl.x;
				tl.y = -tl.y;
			}
			if (tr.z < 0.0f)
			{
				tr.x = -tr.x;
				tr.y = -tr.y;
			}
			if (br.z < 0.0f)
			{
				br.x = -br.x;
				br.y = -br.y;
			}
			if (bl.z < 0.0f)
			{
				bl.x = -bl.x;
				bl.y = -bl.y;
			}

			screenRect.min = new Vector2(Mathf.Min(tl.x, bl.x), 	// left X
			                             Mathf.Min(bl.y, br.y));	// bottom Y
			screenRect.max = new Vector2(Mathf.Max(tr.x, br.x), 	// right X
			                             Mathf.Max(tl.y, tr.y));	// top Y
			
			if ((screenRect.max.x < 0) || (screenRect.min.x > Screen.width) || 
			    (screenRect.max.y < 0) || (screenRect.min.y > Screen.height))
			{
				isVisible = false;
			}
			else
			{
				isVisible = true;
			}

			if ((screenRect.min.x < 0) || (screenRect.max.x > Screen.width) || 
			    (screenRect.min.y < 0) || (screenRect.max.y > Screen.height))
			{
				isPartiallyOffScreen = true;
			}


			tl = cam.WorldToScreenPoint(transform.position);
			if (tl.z < 0.0f)
			{
				behindCamera = true;
			}
			else
			{
				behindCamera = false;
			}
		}

	}


	/// <summary>
	/// Updates the object's grid. It determine's which grid the object is on, and updates the grid (and previous grid) weight.
	/// </summary>
	/// <returns>The grid.</returns>
	/// <param name="dt">Dt.</param>
	/// <param name="clear">Clear the object's grid (i.e. set it to null and decrease its weight).</param>
	public void UpdateGrid(float dt, bool clear)
	{
		if (field == null)
		{
			return;
		}

		SsFieldProperties.SsGrid newGrid = clear ? null : field.GetGridAtPoint(transform.position, true);

		// Moved to a new grid?
		if (newGrid != grid)
		{
			// Decrease weight of previous grid
			if ((grid != null) && (grid.playerWeight > 0))
			{
				grid.playerWeight = Mathf.Max(grid.playerWeight - gridWeight, 0);
			}

			// Increase weight of new grid
			if (newGrid != null)
			{
				newGrid.playerWeight += gridWeight;
			}
		}

		grid = newGrid;
	}

}
