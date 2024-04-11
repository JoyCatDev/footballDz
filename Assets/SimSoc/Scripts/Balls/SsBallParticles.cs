using UnityEngine;
using System.Collections;

/// <summary>
/// Ball particles.
/// </summary>
public class SsBallParticles : MonoBehaviour {

	// Public
	//-------
	[Header("Prefabs")]
	[Tooltip("Shot at goal particle prefab.")]
	public ParticleSystem shotAtGoalPrefab;

	
	// Public: Not visible in inspector:
	//----------------------------------
	[System.NonSerialized]
	public ParticleSystem shotAtGoal;


	// Private
	//--------
	private ParticleSystem[] allParticles;


	// Methods
	//--------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start()
	{
		if (shotAtGoalPrefab != null)
		{
			shotAtGoal = (ParticleSystem)Instantiate(shotAtGoalPrefab);
			if (shotAtGoal != null)
			{
				shotAtGoal.Stop(true);
				shotAtGoal.transform.parent = transform;
				shotAtGoal.transform.localPosition = Vector3.zero;
				shotAtGoal.transform.localRotation = Quaternion.identity;
			}
		}


		// Keep this at the bottom of the method
		allParticles = gameObject.GetComponentsInChildren<ParticleSystem>(true);
	}
	
	
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable()
	{
		StopAllParticles();
	}
	
	
	/// <summary>
	/// Raises the disable event.
	/// </summary>
	void OnDisable()
	{
		StopAllParticles();
	}
	
	
	/// <summary>
	/// Stops all particles.
	/// </summary>
	/// <returns>The all particles.</returns>
	public void StopAllParticles()
	{
		if ((allParticles == null) || (allParticles.Length <= 0))
		{
			return;
		}

		int i;
		ParticleSystem p;
		for (i = 0; i < allParticles.Length; i++)
		{
			p = allParticles[i];
			if (p != null)
			{
				Stop(p);
			}
		}
	}
	
	
	/// <summary>
	/// Play the particle system.
	/// </summary>
	/// <param name="particle">Particle.</param>
	public void Play(ParticleSystem particle)
	{
		if (particle != null)
		{
			particle.Play(true);
		}
	}
	
	
	/// <summary>
	/// Stop the particle system.
	/// </summary>
	/// <param name="particle">Particle.</param>
	public void Stop(ParticleSystem particle)
	{
		if (particle != null)
		{
			particle.Stop(true);
		}
	}

}
