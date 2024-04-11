using System;
using UnityEngine;

namespace SimSoc
{
	/// <summary>
	/// World cup group stage settings.
	/// </summary>
	[Serializable]
	public class WorldCupGroupStageSettings
	{
		[SerializeField, Tooltip("Number of teams per group.")]
		protected int _numTeamsPerGroup = 4;

		[SerializeField, Tooltip("Number of groups. Must be an even number.")]
		protected int _numGroups = 8;

		[SerializeField, Tooltip("Percentage chance that human teams can start in the same group. (0 to 100)")]
		[Range(0, 100)]
		protected float _humansInSameGroupChance = 30.0f;
	
		[SerializeField, Tooltip("Percentage chance that human teams can start in the same match, if they start in the " +
		                         "same group. This only applies to the first match, because they eventually have to " +
		                         "face each other in the group")]
		[Range(0, 100)]
		protected float _humansInSameMatchChance = 20.0f;

		/// <summary>
		/// Number of teams per group.
		/// </summary>
		public virtual int NumTeamsPerGroup => _numTeamsPerGroup;

		/// <summary>
		/// Number of groups.
		/// </summary>
		public virtual int NumGroups => _numGroups;
	
		/// <summary>
		/// Number of matches per group.
		/// </summary>
		public virtual int NumMatchesPerGroup => NumTeamsPerGroup * (NumTeamsPerGroup - 1) / 2;

		/// <summary>
		/// Total number of matches in the group stage.
		/// </summary>
		public virtual int NumMatches => NumGroups * NumMatchesPerGroup;
		
		/// <summary>
		/// The number of teams in each group to advance to the knockout stage.
		/// </summary>
		public virtual int NumAdvanceTeamsPerGroup => 2;

		/// <summary>
		/// The total number of teams to advance to the knockout stage.
		/// </summary>
		public virtual int NumAdvanceTeams => NumGroups * NumAdvanceTeamsPerGroup;

		/// <summary>
		/// Percentage chance that human teams can start in the same group. (0 to 100)
		/// </summary>
		public virtual float HumansInSameGroupChance => _humansInSameGroupChance;
	
		/// <summary>
		/// Percentage chance that human teams can start in the same match, if they start in the same group. This only
		/// applies to the first match, because they eventually have to face each other in the group.
		/// </summary>
		public virtual float HumansInSameMatchChance => _humansInSameMatchChance;
	}
}
