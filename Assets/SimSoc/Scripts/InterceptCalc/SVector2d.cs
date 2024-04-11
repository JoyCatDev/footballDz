using System;


/// <summary>
/// An immutable 2d vector class implemented as a value-type and featuring a fluent API
/// http://www.codeproject.com/Articles/990452/Interception-of-Two-Moving-Objects-in-D-Space
/// </summary>
public struct SVector2d
{
    /// <summary>
    /// Something that can be used to denote a value-type that is invalid. A reference type
    /// may use NULL, but a value type has to express this differently.
    /// </summary>
    public static readonly SVector2d NotAVector = new SVector2d( double.NaN, double.NaN );

    /// <summary>
    /// A zero-valued vector.
    /// </summary>
    public static readonly SVector2d Zero = new SVector2d( 0, 0 );

    /// <summary>
    /// The 'X' coordinate
    /// </summary>
    public readonly double X;

    /// <summary>
    /// The 'Y' coordinate
    /// </summary>
    public readonly double Y;


    /// <summary>
    /// Construct a vector with set X,Y
    /// </summary>
    /// <param name="x">The 'X' coordinate</param>
    /// <param name="y">The 'Y' coordinate</param>
    public SVector2d( double x, double y )
    {
        X = x;
        Y = y;
    }


    /// <summary>
    /// Is another vector the "same" as this vector? "Same" implies "really close", as
    /// opposed to "double==double"
    /// </summary>
    /// <param name="p_other">The vector to compare to this one</param>
    /// <returns>TRUE if the X,Y values are "close"</returns>
    public bool AreSame( SVector2d p_other )
    {
        return X.IsClose( p_other.X ) && Y.IsClose( p_other.Y );
    }

    /// <summary>
    /// This is a vector when both X and Y are not NaN and they are both not Infinity
    /// </summary>
    public bool IsAVector
    {
        get
        {
            return !double.IsInfinity( X ) &&
                   !double.IsInfinity( Y ) &&
                   !double.IsNaN( X ) &&
                   !double.IsNaN( Y );
        }
    }

    /// <summary>
    /// The Square of the Length of this vector- Also the dot-product of this vector and
    /// itself
    /// </summary>
    public double LengthSquared
    {
        get { return X * X + Y * Y; }
    }

    /// <summary>
    /// The Length of this vector (same as "Magnitude").
    /// </summary>
    public double Length
    {
        get { return Math.Sqrt( X * X + Y * Y ); }
    }

    /// <summary>
    /// The square of the distance between this vector and another vector (assumed to be
    /// point vectors)
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>The distance squared between this and another vector</returns>
    public double DistanceSquared( SVector2d p_other )
    {
        double xx = X - p_other.X;
        double yy = Y - p_other.Y;
        return xx * xx + yy * yy;
    }

    /// <summary>
    /// The distance between this vector and another vector
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>The distance between this and another vector</returns>
    public double Distance( SVector2d p_other )
    {
        return Math.Sqrt( DistanceSquared( p_other ) );
    }

    /// <summary>
    /// Calculate the dot-product of this vector and another vector
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>The dot-product, a scalar value</returns>
    public double Dot( SVector2d p_other )
    {
        return X * p_other.X + Y * p_other.Y;
    }


    /// <summary>
    /// Add two vectors together and get a new SVector2d
    /// </summary>
    /// <param name="p_1">The left operand</param>
    /// <param name="p_2">The right operand</param>
    /// <returns>A new SVector2d resulting from the sum of two vectors</returns>
    public static SVector2d operator +( SVector2d p_1, SVector2d p_2 )
    {
        return new SVector2d( p_1.X + p_2.X, p_1.Y + p_2.Y );
    }

    /// <summary>
    /// Return a vector equal to this vector plus another vector
    /// </summary>
    /// <param name="p_other">The vector to add to this one</param>
    /// <returns>A vector representing the sum of two vectors</returns>
    public SVector2d AddTo( SVector2d p_other )
    {
        return this + p_other;
    }

    /// <summary>
    /// Subtract two vectors together and get a new SVector2d
    /// </summary>
    /// <param name="p_1">The left operand</param>
    /// <param name="p_2">The right operand</param>
    /// <returns>
    /// A new SVector2d resulting from subtracting a second vector from a first vector
    /// </returns>
    public static SVector2d operator -( SVector2d p_1, SVector2d p_2 )
    {
        return new SVector2d( p_1.X - p_2.X, p_1.Y - p_2.Y );
    }

    /// <summary>
    /// Return a vector equal to another vector minus this vector
    /// </summary>
    /// <param name="p_other">The vector to subtract this vector from</param>
    /// <returns>
    /// A vector pointing from "this" (a position vector) to another position vector.
    /// </returns>
    public SVector2d PointTo( SVector2d p_other )
    {
        return p_other - this;
    }

    /// <summary>
    /// Multiply a vector by a scalar
    /// </summary>
    /// <param name="p_vector">The vector operand</param>
    /// <param name="p_scale">The scalar operand</param>
    /// <returns>
    /// A new vector containing the operand multiplied by the scalar value
    /// </returns>
    public static SVector2d operator *( SVector2d p_vector, double p_scale )
    {
        return new SVector2d( p_vector.X * p_scale, p_vector.Y * p_scale );
    }

    /// <summary>
    /// Scale this vector's magnitude by some amount.
    /// </summary>
    /// <param name="p_scaleAmount">
    /// The amount to multiply this vector's X and Y values by, returning a new vector
    /// containing the result.
    /// </param>
    /// <returns>
    /// A new vector containing this vector's X and Y values multiplied by the scale amount
    /// </returns>
    public SVector2d ScaleBy( double p_scaleAmount )
    {
        return this * p_scaleAmount;
    }

    /// <summary>
    /// Divide a vector by a scalar
    /// </summary>
    /// <param name="p_vector">The vector operand</param>
    /// <param name="p_scale">The scalar operand</param>
    /// <returns>A new vector containing the operand divided by the scalar value</returns>
    public static SVector2d operator /( SVector2d p_vector, double p_scale )
    {
        return new SVector2d( p_vector.X / p_scale, p_vector.Y / p_scale );
    }

    /// <summary>
    /// Negate a vector
    /// </summary>
    /// <param name="p_vector">The vector to negate</param>
    /// <returns>A new vector consisting of the negated vector</returns>
    public static SVector2d operator -( SVector2d p_vector )
    {
        return new SVector2d( -p_vector.X, -p_vector.Y );
    }

    /// <summary>
    /// Return a new vector which is this vector with both X and Y negated (multiplied by
    /// -1)
    /// </summary>
    /// <returns>
    /// A new vector which is this vector with both X and Y negated (multiplied by -1)
    /// </returns>
    public SVector2d Negate()
    {
        return -this;
    }









    /// <summary>
    /// Return a new SVector2d that has a new X value and this vector's Y value.
    /// </summary>
    /// <param name="p_newX">The new X value</param>
    /// <returns>A new SVector2d that has a new X value and this vector's Y value.</returns>
    public SVector2d WithNewX( double p_newX )
    {
        return new SVector2d( p_newX, Y );
    }

    /// <summary>
    /// Return a new SVector2d that has a new Y value and this vector's X value.
    /// </summary>
    /// <param name="p_newY">The new Y value</param>
    /// <returns>A new SVector2d that has a new Y value and this vector's X value.</returns>
    public SVector2d WithNewY( double p_newY )
    {
        return new SVector2d( X, p_newY );
    }


    /// <summary>
    /// Change the Length (Magnitude) of this vector while keeping its direction the same
    /// </summary>
    /// <param name="p_newLength">The new length</param>
    /// <returns>
    /// A new vector whose length is equal to the requested length, but whose direction
    /// hasn't changed.
    /// </returns>
    public SVector2d WithNewLength( double p_newLength )
    {
        double curLength = Math.Sqrt( X * X + Y * Y );
        double ratio = p_newLength / curLength;

        return new SVector2d( X * ratio, Y * ratio );
    }

    /// <summary>
    /// Change the direction of the vector without changing its length/magnitude.
    /// </summary>
    /// <param name="p_radians">The new direction for the vector</param>
    /// <returns>
    /// A new vector with Length equal to this vector, but direction equal to the radians.
    /// </returns>
    public SVector2d WithNewDirection( double p_radians )
    {
        return VectorFromRadians( p_radians ) * Length;
    }

    /// <summary>
    /// Create a new SVector2d that contains the unit vector for this vector
    /// </summary>
    /// <returns>A new unit vector</returns>
    public SVector2d AsUnitVector()
    {
        double len = Length;
        return new SVector2d( X / len, Y / len );
    }


    /// <summary>
    /// Calculate the dot-product of this vector and another vector. This is also equal to
    /// the cosine of the angle between the two vectors.
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>The normalized dot-product, a scalar value</returns>
    public double NormalizedDot( SVector2d p_other )
    {
        return Dot( p_other ) / (Length * p_other.Length);
    }

    /// <summary>
    /// Calculate the dot-product of this vector and another vector. This is also equal to
    /// the cosine of the angle between the two vectors.
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>The normalized dot-product, a scalar value</returns>
    public double CosineOfAngleBetween( SVector2d p_other )
    {
        return Dot( p_other ) / (Length * p_other.Length);
    }





    /// <summary>
    /// Determine if this and another vector are parallel
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are parallel</returns>
    public bool AreParallel( SVector2d p_other )
    {
        double dot = NormalizedDot( p_other );
        return dot.IsClose( 1 ) || dot.IsClose( -1 );
    }

    /// <summary>
    /// Determine if this and another vector are parallel but pointing in opposite
    /// directions
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are parallel but in opposite directions</returns>
    public bool AreParallelOppositeDir( SVector2d p_other )
    {
        double dot = NormalizedDot( p_other );
        return dot.IsClose( -1 );
    }

    /// <summary>
    /// Determine if this and another vector are parallel and pointing in the same direction
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are parallel and in the same direction</returns>
    public bool AreParallelSameDir( SVector2d p_other )
    {
        double dot = NormalizedDot( p_other );
        return dot.IsClose( 1 );
    }

    /// <summary>
    /// Determine if this and another vector are orthogonal- perpendicular
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are orthogonal</returns>
    public bool AreOrthogonal( SVector2d p_other )
    {
        return NormalizedDot( p_other ).IsClose( 0 );
    }

    /// <summary>
    /// Determine if this and another vector form obtuse angles with each other
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are obtuse</returns>
    public bool AreObtuse( SVector2d p_other )
    {
        return NormalizedDot( p_other ) < 0;
    }

    /// <summary>
    /// Determine if this and another vector form acute angles with each other
    /// </summary>
    /// <param name="p_other">The other vector</param>
    /// <returns>TRUE if the vectors are acute</returns>
    public bool AreAcute( SVector2d p_other )
    {
        return NormalizedDot( p_other ) > 0;
    }




    /****************************************************************************************
     * The coordinate system for these vectors uses these quadrants:
     * 
     * 
     *                     -PI/2
     *                     
     *                       -
     * 
     *            Q3         |          Q4
     *                       |     
     *             (-1,-1)   |   (1,-1)
     *                       |     
     *                       |     
     *  PI (-PI)  -----------+------------ +   0rad
     *                       |     
     *                       |     
     *              (-1,1)   |   (1,1)   
     *                       |     
     *            Q2         |          Q1        
     * 
     *                       +
     * 
     *                      PI/2
     * 
     * 
     ***************************************************************************************/

    /// <summary>
    /// Create a unit vector from an angle specified in radians
    /// </summary>
    /// <param name="p_radians">The angle to create the unit vector from</param>
    /// <returns>A new SVector2d (unit) created from the angle specified</returns>
    public static SVector2d VectorFromRadians( double p_radians )
    {
        return new SVector2d( Math.Cos( p_radians ), Math.Sin( p_radians ) );
    }

    /// <summary>
    /// Turn this vector (assumed to be based at (0,0)) into an angle measure.
    /// </summary>
    /// <returns>The radians (-PI to PI) for this vector</returns>
    public double ToRadians()
    {
        return Math.Atan2( Y, X );
    }


    /// <summary>
    /// Turn this vector into a string
    /// </summary>
    /// <returns>Turn this vector into a string</returns>
    public override string ToString()
    {
        return string.Format( "({0:N3},{1:N3})", X, Y );
    }
}
