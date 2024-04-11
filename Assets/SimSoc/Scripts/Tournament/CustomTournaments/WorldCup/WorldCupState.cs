namespace SimSoc
{
	/// <summary>
	/// Different states of a world cup tournament, which includes the stages.
	/// </summary>
	public enum WorldCupState
	{
		/// <summary>
		/// The world cup has not started.
		/// </summary>
		NotStarted,
	
		/// <summary>
		/// The group stage.
		/// </summary>
		GroupStage,
	
		/// <summary>
		/// Round of 32 knockout stage.
		/// </summary>
		RounfOf32,
	
		/// <summary>
		/// Round of 16 knockout stage.
		/// </summary>
		RoundOf16,
	
		/// <summary>
		/// Quarter-finals knockout stage.
		/// </summary>
		QuarterFinals,
	
		/// <summary>
		/// Semi-finals knockout stage.
		/// </summary>
		SemiFinals,
	
		/// <summary>
		/// Third place play-off.
		/// </summary>
		ThirdPlacePlayOff,
	
		/// <summary>
		/// Final match.
		/// </summary>
		Final,
	
		/// <summary>
		/// World cup is done.
		/// </summary>
		Done
	}
}
