using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif //UNITY_EDITOR

/// <summary>
/// Team skills.
/// </summary>
public class SsTeamSkills : MonoBehaviour {

	// Classes
	//--------
	// AI skills: These only affect AI controlled players
	[System.Serializable]
	public class SsAiTeamSkills
	{
		// AI: Formations
		[Header("AI: Formations")]
		[Tooltip("Can the team change formations at kick off and after a goal?")]
		public bool canChangeFormationAtKickOff = true;


		// AI: Tackling
		//.............
		[Header("AI: Tackling")]
		[Tooltip("Delay sliding when an opponent gets the ball. Gives the opponent some time to get away. \n" + 
		         "Note: It affects all the players in the team.")]
		public float delaySlidingWhenOpponentGetsBall = 1.2f;


	}


	// Public
	//-------

	[Space(10)]
	[Tooltip("Helpful description to help identify the various skills. (This is automatically set to the name of the game object, in the editor.)")]
	public string description;

	// When To Use
	//............
	[Header("When To Use")]
	[Tooltip("Specify when the team must use this skills component. This is ignored if only one skills component is attached to the team. \n" + 
	         "For example: The team can use different skills for the human and computer controlled teams, and for different match difficulties. \n" + 
	         "So you can attach multiple skills components to the team, and the game will use the relevant one.")]
	public SsPlayerSkills.whenToUseSkills whenToUseSkill = SsPlayerSkills.whenToUseSkills.asDefault;
	[Tooltip("This skills component will only used for this specific match difficulty. Only valid if \"When To Use Skill\" makes use of a difficulty, \n" +
	         "otherwise this is ignored.")]
	public int useForDifficulty = 0;



	// Human and AI: These affect user controlled and AI controlled teams
	//-------------------------------------------------------------------


	// AI: These only affect AI controlled teams
	//------------------------------------------
	[Header("AI")]
	[Tooltip("These only affect AI controlled teams.")]
	public SsAiTeamSkills ai;



	// Private
	//--------
	private int searchScore;				// Score when searching for the best skills to use.
	
	
	// Properties
	//-----------
	public int SearchScore
	{
		get { return(searchScore); }
		set
		{
			searchScore = value;
		}
	}


	// Methods
	//--------

#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}
		
		description = name;
	}
#endif //UNITY_EDITOR

}
