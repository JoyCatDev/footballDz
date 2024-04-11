using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Wrapper for Debug.Log, Debug.LogWarning, Debug.LogError. Mainly used when you want to display some of the logs on the 
/// screen (e.g. on a mobile device). It can display the log on the screen via DisplayLog, in which case it will create a game 
/// object and display via OnGUI. The object will be destroyed when a new scene is loaded, but the log items are 
/// persistent (static variables). If you want to display it again in a new scene, call DisplayLog again.
/// </summary>
public class GeLog : MonoBehaviour {

	// Const/Static
	//-------------
	private const int maxItems = 1000;		// Estimated max items in the log. It will create List with this initial capacity.


	// Classes
	//--------
	// Log item (i.e. a line of text).
	public class GeLogItem
	{
		public string msg;
		public System.DateTime time;
		public bool warning;
		public bool error;
	}


	// Private
	//--------
	static private GeLog instance;

	static private bool loggingEnabled = true;
	static private List<GeLogItem> items;
	static private Vector2 scrollPosition = Vector2.zero;
	static private Rect logScreenRect;
	static private int maxVisibleItems = -1;
	static Vector2 prevScrollSize;


	// Properties
	//-----------
	static public GeLog Instance
	{
		get { return(instance); }
	}


	/// <summary>
	/// Enable/disable the logging.
	/// </summary>
	/// <value>The logging enabled.</value>
	static public bool LoggingEnabled
	{
		get { return(loggingEnabled); }
		set
		{
			loggingEnabled = value;
		}
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
	/// Reset and clear the log.
	/// </summary>
	static public void Reset()
	{
		ResetScrollView();

		maxVisibleItems = -1;
		prevScrollSize = Vector2.zero;

		if ((items != null) && (items.Count > 0))
		{
			items.Clear();
		}
	}


	/// <summary>
	/// Resets the scroll view.
	/// </summary>
	/// <returns>The scroll view.</returns>
	static public void ResetScrollView()
	{
		scrollPosition = Vector2.zero;
	}


	/// <summary>
	/// Log the msg.
	/// </summary>
	/// <param name="msg">Message.</param>
	/// <param name="displayInColsole">Display in colsole.</param>
	static public void Log(string msg, bool displayInColsole = true)
	{
		if (loggingEnabled == false)
		{
			return;
		}

		if (displayInColsole)
		{
			Debug.Log(msg);
		}
		AddLogItem(msg, false, false);
	}


	/// <summary>
	/// Log the warning.
	/// </summary>
	/// <returns>The warning.</returns>
	/// <param name="msg">Message.</param>
	/// <param name="displayInColsole">Display in colsole.</param>
	static public void LogWarning(string msg, bool displayInColsole = true)
	{
		if (loggingEnabled == false)
		{
			return;
		}

		if (displayInColsole)
		{
			Debug.LogWarning(msg);
		}
		AddLogItem(msg, true, false);
	}


	/// <summary>
	/// Log the error.
	/// </summary>
	/// <returns>The error.</returns>
	/// <param name="msg">Message.</param>
	/// <param name="displayInColsole">Display in colsole.</param>
	static public void LogError(string msg, bool displayInColsole = true)
	{
		if (loggingEnabled == false)
		{
			return;
		}

		if (displayInColsole)
		{
			Debug.LogError(msg);
		}
		AddLogItem(msg, false, true);
	}


	/// <summary>
	/// Adds the log item.
	/// </summary>
	/// <returns>The log item.</returns>
	/// <param name="msg">Message.</param>
	/// <param name="warning">Warning.</param>
	/// <param name="error">Error.</param>
	static private void AddLogItem(string msg, bool warning, bool error)
	{
		if (loggingEnabled == false)
		{
			return;
		}

		if (items == null)
		{
			items = new List<GeLogItem>(maxItems);
		}

		GeLogItem item = new GeLogItem();

		item.msg = msg;
		item.time = System.DateTime.Now;
		item.warning = warning;
		item.error = error;

		items.Add(item);
	}


	/// <summary>
	/// Show the log on screen.
	/// </summary>
	/// <returns>The log.</returns>
	/// <param name="screenRect">Screen rect.</param>
	/// <param name="resetScrollView">Reset scroll view.</param>
	/// <param name="maxItemsToShow">Max items to show in the log.</param>
	static public void ShowLog(Rect screenRect, bool resetScrollView = false,
	                           int maxItemsToShow = -1)
	{
		if (loggingEnabled == false)
		{
			return;
		}

		if (instance == null)
		{
			GameObject go = new GameObject("Debug Log");
			if (go != null)
			{
				go.AddComponent<GeLog>();
			}
			else
			{
				Destroy(go);
			}
		}

		if (resetScrollView)
		{
			ResetScrollView();
		}

		if (instance != null)
		{
			instance.gameObject.SetActive(true);
		}

		logScreenRect = screenRect;
		maxVisibleItems = maxItemsToShow;
	}


	/// <summary>
	/// Hide the log.
	/// </summary>
	/// <returns>The log.</returns>
	static public void HideLog()
	{
		if (instance != null)
		{
			instance.gameObject.SetActive(false);
		}
	}

#if SIMSOC_ENABLE_ONGUI
	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI()
	{
		Rect rect = logScreenRect;
		int max = (items != null) ? items.Count : 0;
		int i, start;
		string msg;
		GeLogItem item;
		Vector2 pos = new Vector2(5, 5);
		float addY = 25.0f;
		float width = rect.width - (pos.x * 2);
		string msgType;
		GUIStyle labelStyle = (GUI.skin != null) ? GUI.skin.GetStyle("Label") : null;
		Vector2 labelSize, scrollSize;

		if (maxVisibleItems >= 0)
		{
			if (items != null)
			{
				start = items.Count - maxVisibleItems;
				if (start < 0)
				{
					start = 0;
				}
				max = items.Count;
			}
			else
			{
				start = 0;
				max = 0;
			}
		}
		else
		{
			start = 0;
			max = (items != null) ? items.Count : 0;
		}

		labelSize = new Vector2(width, addY);

		scrollSize.x = Mathf.Max(prevScrollSize.x, rect.width);
		scrollSize.y = Mathf.Max(prevScrollSize.y, addY * max);
		prevScrollSize = Vector2.zero;

		// Background
		GUI.Box(rect, "");

		// List in a scroll view
		scrollPosition = GUI.BeginScrollView(new Rect(rect.x, rect.y + 5, rect.width, rect.height - addY), 
		                                     scrollPosition, 
		                                     new Rect(0, 0, scrollSize.x, scrollSize.y));
		{
			if ((items != null) && (items.Count > 0))
			{
				for (i = start; i < max; i++)
				{
					item = items[i];
					if (item != null)
					{
						if (item.warning)
						{
							msgType = " [Warning]";
						}
						else if (item.error)
						{
							msgType = " [Error]";
						}
						else
						{
							msgType = "";
						}

						msg = string.Format("{0}{1}: {2}",
						                    item.time.ToString(),
						                    msgType,
						                    item.msg);

						if (labelStyle != null)
						{
							labelSize = labelStyle.CalcSize(new GUIContent(msg));
							labelSize.x += 10;
						}

						GUI.Label(new Rect(pos.x, pos.y, labelSize.x, labelSize.y), msg);
						pos.y += labelSize.y;
						prevScrollSize.y += labelSize.y;
						if (prevScrollSize.x < labelSize.x)
						{
							prevScrollSize.x = labelSize.x;
						}
					}
				}
			}
		}
		GUI.EndScrollView();
	}
#endif

}
