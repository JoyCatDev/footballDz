using UnityEngine;
using System.Collections;

/// <summary>
/// Message box.
/// </summary>
public class SsMsgBox : SsBaseMenu {

	// Public
	//-------
	[Header("Elements")]
	public GameObject headingPanel;
	public UnityEngine.UI.Text headingText;

	public UnityEngine.UI.Text msg;

	public UnityEngine.UI.Text okText;
	public string defaultOkText = "OK";

	public UnityEngine.UI.ScrollRect scrollView;



	// Private
	//--------
	static private SsMsgBox instance;

	static private bool didClickCancel;
	static private bool didClickOk;

	static private SsMenuCallback cancelCallback;
	static private SsMenuCallback okCallback;

	private TextAnchor defaultMsgAlignment;
	private bool pauseGame;



	// Properties
	//-----------
	static public SsMsgBox Instance
	{
		get { return(instance); }
	}


	/// <summary>
	/// Did user click Cancel when msg box was last open?
	/// </summary>
	/// <value>The did click cancel.</value>
	static public bool DidClickCancel
	{
		get { return(didClickCancel); }
	}


	/// <summary>
	/// Did user click OK when msg box was last open?
	/// </summary>
	/// <value>The did click ok.</value>
	static public bool DidClickOk
	{
		get { return(didClickOk); }
	}


	public TextAnchor DefaultMsgAlignment
	{
		get { return(defaultMsgAlignment); }
	}


	public bool PauseGame
	{
		get { return (pauseGame); }
		set
		{
			bool oldValue = pauseGame;
			pauseGame = value;
			if (pauseGame)
			{
				Time.timeScale = 0.0f;
			}
			else if (oldValue)
			{
				// Was previously paused, so unpause
				Time.timeScale = 1.0f;
			}
		}
	}


	// Methods
	//--------
	/// <summary>
	/// Awake this instance.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void Awake()
	{
		base.Awake();
		instance = this;

		if (msg != null)
		{
			defaultMsgAlignment = msg.alignment;
		}
	}


	/// <summary>
	/// Raises the destroy event.
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	public override void OnDestroy()
	{
		base.OnDestroy();
		instance = null;
	}


	/// <summary>
	/// Show the message box.
	/// </summary>
	/// <returns>The message box.</returns>
	/// <param name="msg">Message.</param>
	/// <param name="heading">Heading. Null or empty to hide the heading.</param>
	/// <param name="okText">Ok text. Null or empty to show the default Ok text.</param>
	/// <param name="callbackOk">Callback when Ok is clicked. Null if none.</param>
	/// <param name="callbackCancel">Callback when Cancel is clicked. Null if none.</param>
	static public void ShowMsgBox(string msg, string heading, string okText,
	                              SsMenuCallback callbackOk, SsMenuCallback callbackCancel,
	                              bool leftAlignText = false,
	                              bool pauseGame = false)
	{
		if (instance == null)
		{
			return;
		}

		bool hasHeading = (string.IsNullOrEmpty(heading) == false);

		cancelCallback = callbackCancel;
		okCallback = callbackOk;

		if (instance.msg != null)
		{
			instance.msg.text = msg;

			if (leftAlignText)
			{
				instance.msg.alignment = TextAnchor.MiddleLeft;
			}
			else
			{
				instance.msg.alignment = instance.DefaultMsgAlignment;
			}
		}

		if (instance.headingPanel != null)
		{
			instance.headingPanel.SetActive(hasHeading);
			if ((hasHeading) && (instance.headingText != null))
			{
				instance.headingText.text = heading;
			}
		}

		if (instance.okText != null)
		{
			if (string.IsNullOrEmpty(okText))
			{
				instance.okText.text = instance.defaultOkText;
			}
			else
			{
				instance.okText.text = okText;
			}
		}

		instance.PauseGame = pauseGame;

		instance.Show(fromDirections.invalid, false);
	}


	/// <summary>
	/// Show the menu, and play the in animation (if snap = false).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <param name="fromDirection">Direction to enter from. Set to invalid to use the default one.</param>
	/// <param name="snap">Snap to end position.</param>
	public override void Show(fromDirections fromDirection, bool snap)
	{
		base.Show(fromDirection, snap);

		// Reset scroll view position, after all content has been added
		if (scrollView != null)
		{
			// REMINDER: This scroll bar is vertically inverted.
			scrollView.normalizedPosition = new Vector2(0.0f, 1.0f);
		}
	}


	/// <summary>
	/// Hide the menu immediately (no animation).
	/// NOTE: Derived methods must call the base method.
	/// </summary>
	/// <returns>The immediate.</returns>
	protected override void HideImmediate()
	{
		base.HideImmediate();

		PauseGame = false;
	}


	/// <summary>
	/// Raises the cancel event.
	/// </summary>
	public void OnCancel()
	{
		didClickCancel = true;
		didClickOk = false;
		HideCallback = cancelCallback;

		cancelCallback = null;
		okCallback = null;
	}


	/// <summary>
	/// Raises the ok event.
	/// </summary>
	public void OnOk()
	{
		didClickOk = true;
		didClickCancel = false;
		HideCallback = okCallback;

		cancelCallback = null;
		okCallback = null;
	}

}
