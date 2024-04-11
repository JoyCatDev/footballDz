using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// Player state matrix. Defines which states can change to other states, and contains some state settings.
/// </summary>
public class SsPlayerStateMatrix : MonoBehaviour {

	// Classes
	//--------
	// State that can change to another state
	[System.Serializable]
	public class SsStateToState
	{
		public SsPlayer.states from;
		public SsPlayer.states to;

		public SsStateToState(SsPlayer.states newFrom = SsPlayer.states.idle, 
		                      SsPlayer.states newTo = SsPlayer.states.idle)
		{
			from = newFrom;
			to = newTo;
		}
	}


	// Public
	//-------


	// Private
	//--------
	// Specify which states can change to other states.
	// Note: This is only used in certain parts of the code (e.g. testing
	// if player can slide tackle from the current state), so changes may have no affect.
	private SsStateToState[] states;
	
	// A human controlled player can only move while in one of these states. Leave empty to use default setup.
	private SsPlayer.states[] canHumanMove;

	private bool[,] sortedCanChange;			// Sorted lookup table [from, to]
	private bool[] sortedCanHumanMove;			// Sorted lookup table



	// Methods
	//--------

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake()
	{
		InitDefaultMatrix();
		SortStates();
	}


	/// <summary>
	/// Inits the default matrix.
	/// </summary>
	/// <returns>The default matrix.</returns>
	public void InitDefaultMatrix()
	{
		if ((states != null) && (states.Length > 0) && 
		    (canHumanMove != null) && (canHumanMove.Length > 0))
		{
			return;
		}

		int i;


		// States
		if ((states == null) || (states.Length <= 0))
		{
			List<SsStateToState> list = new List<SsStateToState>();

			// Idle
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.run));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.kickNear));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.kickMedium));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.kickFar));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.slideTackle));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.falling));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.inPain));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.gk_diveForward));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.gk_diveUp));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.gk_diveLeft));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.gk_diveRight));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.gk_standHoldBall));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.throwInHold));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.throwIn));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.header));
			list.Add(new SsStateToState(SsPlayer.states.idle, 
			                            SsPlayer.states.bicycleKick));

			// Run
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.idle));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.kickNear));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.kickMedium));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.kickFar));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.slideTackle));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.falling));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.inPain));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.gk_diveForward));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.gk_diveUp));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.gk_diveLeft));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.gk_diveRight));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.gk_standHoldBall));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.throwInHold));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.throwIn));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.header));
			list.Add(new SsStateToState(SsPlayer.states.run, 
			                            SsPlayer.states.bicycleKick));

			// Kick near
			list.Add(new SsStateToState(SsPlayer.states.kickNear, 
			                            SsPlayer.states.slideTackle));

			// Copy the kick near states to the kick medium and kick far
			CopyStateToState(ref list, SsPlayer.states.kickNear, SsPlayer.states.kickMedium);
			CopyStateToState(ref list, SsPlayer.states.kickNear, SsPlayer.states.kickFar);


			states = new SsStateToState[list.Count];
			for (i = 0; i < list.Count; i++)
			{
				states[i] = list[i];
			}
			list.Clear();
		}


		// Can human controller player move
		if ((canHumanMove == null) || (canHumanMove.Length <= 0))
		{
			List<SsPlayer.states> list = new List<SsPlayer.states>();

			list.Add(SsPlayer.states.idle);
			list.Add(SsPlayer.states.run);
			list.Add(SsPlayer.states.throwInHold);
			list.Add(SsPlayer.states.header);

			canHumanMove = new SsPlayer.states[list.Count];
			for (i = 0; i < list.Count; i++)
			{
				canHumanMove[i] = list[i];
			}
			list.Clear();
		}
	}


	/// <summary>
	/// Copy one state's settings to another state, for the canHumanMove.
	/// </summary>
	/// <returns>The state to state.</returns>
	/// <param name="list">List.</param>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	private void CopyStateToState(ref List<SsStateToState> list, SsPlayer.states from, SsPlayer.states to)
	{
		if ((list == null) || (list.Count > 0))
		{
			return;
		}

		int i;
		SsStateToState state;
		List<SsStateToState> newList = new List<SsStateToState>();

		for (i = 0; i < list.Count; i++)
		{
			state = list[i];
			if (state.from == from)
			{
				newList.Add(new SsStateToState(to, state.to));
			}
		}

		if ((newList != null) && (newList.Count > 0))
		{
			for (i = 0; i < newList.Count; i++)
			{
				list.Add(newList[i]);
			}
			newList.Clear();
		}
	}


	/// <summary>
	/// Sorts the states into a lookup table.
	/// </summary>
	/// <returns>The state.</returns>
	public void SortStates()
	{
		if (sortedCanChange != null)
		{
			sortedCanChange = null;
		}

		int i, from, to, max;
		SsStateToState state;

		max = (int)SsPlayer.states.maxStates;

		if ((states != null) && (states.Length > 0))
		{
			sortedCanChange = new bool[max, max];

			for (from = 0; from < max; from ++)
			{
				for (to = 0; to < max; to++)
				{
					sortedCanChange[from, to] = false;
				}
			}

			for (i = 0; i < states.Length; i++)
			{
				state = states[i];
				if (state != null)
				{
					from = (int)state.from;
					to = (int)state.to;
					if ((from >= 0) && (from < max) && (to >= 0) && (to < max))
					{
						sortedCanChange[from, to] = true;
					}
				}
			}
		}


		if ((canHumanMove != null) && (canHumanMove.Length > 0))
		{
			sortedCanHumanMove = new bool[max];

			for (i = 0; i < max; i++)
			{
				sortedCanHumanMove[i] = false;
			}

			for (i = 0; i < canHumanMove.Length; i++)
			{
				to = (int)canHumanMove[i];
				if ((to >= 0) && (to < sortedCanHumanMove.Length))
				{
					sortedCanHumanMove[to] = true;
				}
			}
		}

	}


	/// <summary>
	/// Test if the player can change from one state to another state.
	/// </summary>
	/// <returns><c>true</c> if this instance can change the specified from to; otherwise, <c>false</c>.</returns>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	public bool CanChange(SsPlayer.states from, SsPlayer.states to)
	{
		if ((sortedCanChange == null) || (sortedCanChange.GetLength(0) <= 0) || (sortedCanChange.GetLength(1) <= 0))
		{
			return (false);
		}

		int f = (int)from;
		int t = (int)to;

		if ((f >= 0) && (f < sortedCanChange.GetLength(0)) && 
		    (t >= 0) && (t < sortedCanChange.GetLength(1)))
		{
			return (sortedCanChange[f, t]);
		}

		return (false);
	}


	/// <summary>
	/// Test if a user controlled player can move in the specified state.
	/// </summary>
	/// <returns><c>true</c> if this instance can user move the specified state; otherwise, <c>false</c>.</returns>
	/// <param name="state">State.</param>
	public bool CanUserMove(SsPlayer.states state)
	{
		int i = (int)state;
		if ((i >= 0) && (sortedCanHumanMove != null) && (i < sortedCanHumanMove.Length))
		{
			return (sortedCanHumanMove[i]);
		}
		return (false);
	}



#if UNITY_EDITOR
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		// Initialise in the editor
		if ((states == null) || (states.Length <= 0))
		{
			InitDefaultMatrix();
		}
	}
#endif //UNITY_EDITOR
}
