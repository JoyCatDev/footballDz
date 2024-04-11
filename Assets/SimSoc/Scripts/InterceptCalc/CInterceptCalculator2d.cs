using System;

/// <summary>
/// This class will determine at what time and position a given point will intercept another
/// point on a 2d plane.
/// http://www.codeproject.com/Articles/990452/Interception-of-Two-Moving-Objects-in-D-Space
/// </summary>
public class CInterceptCalculator2d
{
    /// <summary>
    /// INPUT: The Position Vector for the chasing object.
    /// </summary>
    private SVector2d m_chaserPosition = SVector2d.NotAVector;

    /// <summary>
    /// INPUT: The speed of the chasing object
    /// </summary>
    private double m_chaserSpeed = double.NaN;

    /// <summary>
    /// INPUT: The position of the object being chased (the Runner)
    /// </summary>
    private SVector2d m_runnerPosition = SVector2d.NotAVector;

    /// <summary>
    /// INPUT: The Velocity of the Runner
    /// </summary>
    private SVector2d m_runnerVelocity = SVector2d.NotAVector;

    /// <summary>
    /// OUTPUT: TRUE if the interception is possible given the inputs
    /// </summary>
    private bool m_interceptionPossible = false;

    /// <summary>
    /// OUTPUT: If "IsAVector" and InterceptionPossible, this is the velocity that the
    /// Chaser should have in order to intercept the Runner.
    /// </summary>
    private SVector2d m_chaserVelocity = SVector2d.NotAVector;

    /// <summary>
    /// OUTPUT: If "IsAVector" and InterceptionPossible, this is the point at which
    /// interception shall occur, given that the Chaser immmediately assumes ChaserVelocity
    /// </summary>
    private SVector2d m_interceptionPosition = SVector2d.NotAVector;

    /// <summary>
    /// OUTPUT: If not Nan and InterceptionPossible, this is the The amount of time that
    /// must pass before interception.
    /// </summary>
    private double m_timeToInterception = double.NaN;

    /// <summary>
    /// Set to TRUE when the routine
    /// </summary>
    private bool m_calculationPerformed = false;


    /// <summary>
    /// INPUT: The Position Vector for the chasing object.
    /// </summary>
    public SVector2d ChaserPosition
    {
        get { return m_chaserPosition; }
        set
        {
            ClearResults();
            m_chaserPosition = value;
        }
    }

    /// <summary>
    /// INPUT: The speed of the chasing object
    /// </summary>
    public double ChaserSpeed
    {
        get { return m_chaserSpeed; }
        set
        {
            ClearResults();
            m_chaserSpeed = value;
        }
    }

    /// <summary>
    /// INPUT: The position of the object being chased (the Runner)
    /// </summary>
    public SVector2d RunnerPosition
    {
        get { return m_runnerPosition; }
        set
        {
            ClearResults();
            m_runnerPosition = value;
        }
    }

    /// <summary>
    /// INPUT: The Velocity of the Runner
    /// </summary>
    public SVector2d RunnerVelocity
    {
        get { return m_runnerVelocity; }
        set
        {
            ClearResults();
            m_runnerVelocity = value;
        }
    }




    /// <summary>
    /// OUTPUT: If "IsAVector" and InterceptionPossible, this is the point at which
    /// interception shall occur, given that the Chaser immmediately assumes ChaserVelocity
    /// </summary>
    public SVector2d InterceptionPoint
    {
        get
        {
            SetResults();
            return m_interceptionPosition;
        }
    }

    /// <summary>
    /// OUTPUT: If "IsAVector" and InterceptionPossible, this is the velocity that the
    /// Chaser should have in order to intercept the Runner.
    /// </summary>
    public SVector2d ChaserVelocity
    {
        get
        {
            SetResults();
            return m_chaserVelocity;
        }
    }

    /// <summary>
    /// OUTPUT: If not Nan and InterceptionPossible, this is the The amount of time that
    /// must pass before interception.
    /// </summary>
    public double TimeToInterception
    {
        get
        {
            SetResults();
            return m_timeToInterception;
        }
    }

    /// <summary>
    /// OUTPUT: TRUE if the interception is possible given the inputs
    /// </summary>
    public bool InterceptionPossible
    {
        get
        {
            SetResults();
            return m_interceptionPossible;
        }
    }



    /// <summary>
    /// Call to force a re-calculation. Re-calculation will also happen any time one of the
    /// INPUT properties changes.
    /// </summary>
    public void ClearResults()
    {
        m_calculationPerformed = false;
        m_interceptionPossible = false;
        m_chaserVelocity = SVector2d.NotAVector;
        m_interceptionPosition = SVector2d.NotAVector;
        m_timeToInterception = double.NaN;
    }

    /// <summary>
    /// Determine if all of the input values are valid. By default, they are all invalid,
    /// forcing the user of this class to set them all before a calculation will be
    /// performed.
    /// </summary>
    public bool HasValidInputs
    {
        get
        {
            return m_chaserPosition.IsAVector &&
                   m_runnerPosition.IsAVector &&
                   m_runnerVelocity.IsAVector &&
                   !double.IsNaN( m_chaserSpeed ) &&
                   !double.IsInfinity( m_chaserSpeed );
        }
    }

    /// <summary>
    /// Called internally to calculate the interception data.
    /// </summary>
    private void SetResults()
    {
        // Don't re-calculate if none of the input parameters have changed.
        if (m_calculationPerformed)
            return;

        // Make sure all results look like "no interception possible".
        ClearResults();

        // Set this to TRUE regardless of the success or failure of interception. This
        // prevents this routine from doing anything until one of the input values has been
        // changed or the application calls ClearResults()
        m_calculationPerformed = true;

        // If the inputs are invalid, then everything is already set for a "no interception"
        // scenario.
        if (!HasValidInputs)
            return;


        // First check- Are we already on top of the target? If so, its valid and we're done
        if (ChaserPosition.AreSame( RunnerPosition ))
        {
            m_interceptionPossible = true;
            m_interceptionPosition = ChaserPosition;
            m_timeToInterception = 0;
            m_chaserVelocity = SVector2d.Zero;
            return;
        }

        // Check- Am I moving? Be gracious about exception throwing even though negative
        // speed is undefined.
        if (ChaserSpeed <= 0)
            return; // No interception


        SVector2d vectorFromRunner = ChaserPosition - RunnerPosition;
        double distanceToRunner = vectorFromRunner.Length;
        double runnerSpeed = RunnerVelocity.Length;

        // Check- Is the Runner not moving? If it isn't, the calcs don't work because we
        // can't use the Law of Cosines
        if (runnerSpeed.IsClose( 0 ))
        {
            m_timeToInterception = distanceToRunner / ChaserSpeed;
            m_interceptionPosition = RunnerPosition;
        }
        else // Everything looks OK for the Law of Cosines approach
        {
            // Now set up the quadratic formula coefficients
            double a = ChaserSpeed * ChaserSpeed - runnerSpeed * runnerSpeed;
            double b = 2 * vectorFromRunner.Dot( RunnerVelocity );
            double c = -distanceToRunner * distanceToRunner;

            double t1, t2;
            if (!CMath.QuadraticSolver( a, b, c, out t1, out t2 ))
            {
                // No real-valued solution, so no interception possible
                return;
            }

            if (t1 < 0 && t2 < 0)
            {
                // Both values for t are negative, so the interception would have to have
                // occured in the past
                return;
            }

            if (t1 > 0 && t2 > 0) // Both are positive, take the smaller one
                m_timeToInterception = Math.Min( t1, t2 );
            else // One has to be negative, so take the larger one
                m_timeToInterception = Math.Max( t1, t2 );

            m_interceptionPosition = RunnerPosition + RunnerVelocity * m_timeToInterception;
        }

        // Calculate the resulting velocity based on the time and intercept position
        m_chaserVelocity = (m_interceptionPosition - ChaserPosition) / m_timeToInterception;

        // Finally, signal that the interception was possible.
        m_interceptionPossible = true;
    }
}
