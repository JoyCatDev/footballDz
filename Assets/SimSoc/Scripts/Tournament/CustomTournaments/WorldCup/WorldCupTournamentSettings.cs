using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimSoc
{
	/// <inheritdoc cref="ICustomTournamentSettings"/>
	/// <remarks>
	/// <para>World cup tournament settings.</para>
	/// <para>Attach this to the <see cref="SsMatchSettings"/> prefab. You can attach more than one, as long as each  
	/// one has a different <see cref="_id"/>.</para>
	/// </remarks>
	public class WorldCupTournamentSettings : MonoBehaviour, ICustomTournamentSettings
	{
		[Serializable]
		public class StateHeadingsInfo
		{
			[SerializeField, Tooltip("Heading to display for the state.")]
			protected string _heading;
		
			[SerializeField]
			protected WorldCupState _state;
		
			public WorldCupState State => _state;
			public string Heading => _heading;
		}
	
		[SerializeField, Tooltip("Unique ID. It is used to identify the tournament. Do Not change this after the " + 
		                         "game has been released.")]
		protected string _id;

		[SerializeField, Tooltip("Name to display on UI.")]
		protected string _displayName;

		[SerializeField, Tooltip("How a team's field is selected in the tournament matches. It will try to stick " +
		                         "to this, but may not always be possible.")]
		protected SsTournament.fieldSelectSequences _fieldSelectSequence;

		[SerializeField, Tooltip("Max AI difficulty for the tournament. Only valid if teams/players have skills " +
		                         "setup for different difficulties.")]
		protected int _maxAiDifficulty = 2;

		[SerializeField, Tooltip("If two teams have equal rank then give the human team a higher rank.")]
		protected bool _preferHumanRank = true;
		
		[Header("Points")]
		[SerializeField, Tooltip("Points awarded for a win.")]
		protected int _pointsWin = 3;
	
		[SerializeField, Tooltip("Points awarded for a draw.")]
		protected int _pointsDraw = 1;
	
		[SerializeField, Tooltip("Points awarded for a loss.")]
		protected int _pointsLose;
		
		[Header("Group Stage")]
		[SerializeField, Tooltip("Group stage settings.")]
		protected WorldCupGroupStageSettings _groupStage;
	
		[Header("Third place play-off")]
		[SerializeField, Tooltip("Should the teams play for third place?")]
		protected bool _playForThirdPlace = true;

		[Header("Display info")]
		[SerializeField, Tooltip("Headings to display on the UI.")]
		protected StateHeadingsInfo[] _stateHeadings;

		// Base tournament settings.
		protected SsTournamentSettings _settings;

		protected bool _didInitializeKnockoutStages;
		protected readonly WorldCupKnockoutStageSettings _roundOf32 = new WorldCupKnockoutStageSettings(16);
		protected readonly WorldCupKnockoutStageSettings _roundOf16 = new WorldCupKnockoutStageSettings(8);
		protected readonly WorldCupKnockoutStageSettings _quarterFinals = new WorldCupKnockoutStageSettings(4);
		protected readonly WorldCupKnockoutStageSettings _semiFinals = new WorldCupKnockoutStageSettings(2);
		protected readonly WorldCupKnockoutStageSettings _thirdPlace = new WorldCupKnockoutStageSettings(1);
		protected readonly WorldCupKnockoutStageSettings _final = new WorldCupKnockoutStageSettings(1);

		protected int _numKnockoutRounds;
		protected readonly List<int> _numMatchesInKnockoutRound = new List<int>();
		protected readonly List<WorldCupState> _knockoutRoundState = new List<WorldCupState>();
		protected int _thirdPlaceRoundIndex;

		/// <inheritdoc/>
		public virtual SsTournamentSettings TournamentSettings => _settings;

		/// <inheritdoc/>
		public virtual ICustomTournamentController CustomController { get; protected set; }

		/// <inheritdoc/>
		public virtual ICustomTournamentMatchMaker MatchMaker { get; protected set; }

		/// <inheritdoc/>
		public virtual bool UseFieldSelectSequence => true;

		/// <summary>
		/// How a team's field is selected in the tournament matches. It will try to stick to this, but may not always 
		/// be possible.
		/// </summary>
		public virtual SsTournament.fieldSelectSequences FieldSelectSequence => _fieldSelectSequence;
	
		/// <summary>
		/// Max AI difficulty for the tournament. Only valid if teams/players have skills setup for different
		/// difficulties.
		/// </summary>
		public virtual int MaxAiDifficulty => _maxAiDifficulty;

		/// <summary>
		/// If two teams have equal rank then give the human team a higher rank.
		/// </summary>
		public virtual bool PreferHumanRank => _preferHumanRank;
	
		/// <summary>
		/// Group stage settings. 
		/// </summary>
		public virtual WorldCupGroupStageSettings GroupStage => _groupStage;
	
		/// <summary>
		/// Points awarded for a win.
		/// </summary>
		public virtual int PointsWin => _pointsWin;
	
		/// <summary>
		/// Points awarded for a draw.
		/// </summary>
		public virtual int PointsDraw => _pointsDraw;
	
		/// <summary>
		/// Points awarded for a loss.
		/// </summary>
		public virtual int PointsLose => _pointsLose;
	
		/// <summary>
		/// Should the teams play for third place?
		/// </summary>
		public virtual bool PlayForThirdPlace => _playForThirdPlace;

		/// <summary>
		/// Round of 32 knockout stage settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings RoundOf32 => _roundOf32;
	
		/// <summary>
		/// Round of 16 knockout stage settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings RoundOf16 => _roundOf16;
	
		/// <summary>
		/// Quarter-finals settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings QuarterFinals => _quarterFinals;
	
		/// <summary>
		/// Semi-finals settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings SemiFinals => _semiFinals;
	
		/// <summary>
		/// Match for third place settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings ThirdPlace => _thirdPlace;
	
		/// <summary>
		/// Final match settings.
		/// </summary>
		public virtual WorldCupKnockoutStageSettings Final => _final;

		/// <summary>
		/// Number of teams in the tournament.
		/// </summary>
		public virtual int NumTeams => _groupStage.NumTeamsPerGroup * _groupStage.NumGroups;

		/// <summary>
		/// Total number of matches that will be played in the tournament.
		/// </summary>
		public virtual int NumMatches => _groupStage.NumMatches + NumKnockoutMatches;

		/// <summary>
		/// Total number of matches in the knockout stages of the tournament.
		/// </summary>
		public virtual int NumKnockoutMatches => _roundOf32.NumMatches + _roundOf16.NumMatches +
		                                         _quarterFinals.NumMatches + _semiFinals.NumMatches +
		                                         _thirdPlace.NumMatches + _final.NumMatches;

		/// <summary>
		/// Number of rounds in the knockout stages of the tournament.
		/// </summary>
		public virtual int NumKnockoutRounds => _numKnockoutRounds;

		protected virtual void Awake()
		{
			InitializeKnockoutStages();
		
			Assert.IsTrue(_groupStage.NumGroups > 0 && (_groupStage.NumGroups % 2 == 0),
				"The number of groups must be an even number.");
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			_didInitializeKnockoutStages = false;
			InitializeKnockoutStages();
		}
#endif

#region ICustomTournamentSettings
		/// <inheritdoc/>
		public virtual SsTournamentSettings CreateSettings()
		{
			InitializeKnockoutStages();
		
			_settings = new SsTournamentSettings(this)
			{
				id = _id, 
				displayName = _displayName, 
				type = SsTournament.tournamentTypes.custom,
				humansInSameGroupChance = _groupStage.HumansInSameGroupChance,
				humansInSameMatchChance = _groupStage.HumansInSameMatchChance,
				fieldSelectSequence = _fieldSelectSequence,
				maxAiDifficulty = _maxAiDifficulty
			};

			return _settings;
		}

		/// <inheritdoc/>
		public virtual void Initialize(ITournamentManager tournamentManager)
		{
			_settings.maxMatches = NumMatches;
			_settings.maxTeams = NumTeams;

			CustomController = new WorldCupTournament(tournamentManager);
			MatchMaker = new WorldCupMatchMaker(tournamentManager);
		}
#endregion

		/// <summary>
		/// Gets the heading to display for the state. Returns null if it could not be found.
		/// </summary>
		public virtual string GetStateHeading(WorldCupState state)
		{
			if (_stateHeadings == null || _stateHeadings.Length <= 0)
			{
				return null;
			}

			for (int i = 0, len = _stateHeadings.Length; i < len; i++)
			{
				var heading = _stateHeadings[i];
				if (heading != null && heading.State == state)
				{
					return heading.Heading;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the heading to display for the state, based on the round. Returns null if it could not be found.
		/// </summary>
		public virtual string GetStateHeadingFromRound(int roundIndex)
		{
			if (_knockoutRoundState == null || roundIndex < 0 || roundIndex >= _knockoutRoundState.Count)
			{
				return null;
			}
			return GetStateHeading(_knockoutRoundState[roundIndex]);
		}
	
		/// <summary>
		/// Gets the number of matches in the knockout round.
		/// </summary>
		public virtual int GetNumberOfMatchesInKnockoutRound(int roundIndex)
		{
			if (_numMatchesInKnockoutRound == null || roundIndex < 0 || roundIndex >= _numMatchesInKnockoutRound.Count)
			{
				return 0;
			}
			return _numMatchesInKnockoutRound[roundIndex];
		}

		/// <summary>
		/// Gets whether the specified round is the third place play-off round.
		/// </summary>
		public virtual bool IsThirdPlacePlayOffRound(int roundIndex)
		{
			return _thirdPlaceRoundIndex >= 0 && roundIndex == _thirdPlaceRoundIndex;
		}

		/// <summary>
		/// Gets whether the specified round is the final knockout round.
		/// </summary>
		public virtual bool IsFinalRound(int roundIndex)
		{
			return _numKnockoutRounds > 0 && roundIndex == _numKnockoutRounds - 1;
		}

		/// <summary>
		/// Lazy initialization of the knockout stages.
		/// </summary>
		protected virtual void InitializeKnockoutStages()
		{
			if (_didInitializeKnockoutStages || _groupStage == null)
			{
				return;
			}

			_didInitializeKnockoutStages = true;

			_numKnockoutRounds = 0;
			_numMatchesInKnockoutRound.Clear();
			_knockoutRoundState.Clear();
			_thirdPlaceRoundIndex = -1;

			InitializeKnockoutSettings(WorldCupState.RounfOf32, _roundOf32);
			InitializeKnockoutSettings(WorldCupState.RoundOf16, _roundOf16);
			InitializeKnockoutSettings(WorldCupState.QuarterFinals, _quarterFinals);
			InitializeKnockoutSettings(WorldCupState.SemiFinals, _semiFinals);
			InitializeKnockoutSettings(WorldCupState.ThirdPlacePlayOff, _thirdPlace, true, !_playForThirdPlace);
			InitializeKnockoutSettings(WorldCupState.Final, _final, false);
		}

		protected virtual void InitializeKnockoutSettings(WorldCupState state, WorldCupKnockoutStageSettings settings, 
			bool updateNumberOfMatches = true, bool forceNumberOfMatchesToZero = false)
		{
			if (settings == null)
			{
				return;
			}

			if (updateNumberOfMatches)
			{
				settings.UpdateNumberOfMatches(_groupStage.NumAdvanceTeams, forceNumberOfMatchesToZero);	
			}

			var numMatches = settings.NumMatches;
			if (numMatches <= 0)
			{
				return;
			}
		
			_numKnockoutRounds++;
			if (state == WorldCupState.ThirdPlacePlayOff)
			{
				_thirdPlaceRoundIndex = _numMatchesInKnockoutRound.Count;
			}
			_numMatchesInKnockoutRound.Add(numMatches);
			_knockoutRoundState.Add(state);
		}
	}
}
