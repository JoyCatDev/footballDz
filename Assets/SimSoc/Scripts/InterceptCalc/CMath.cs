using System;

/// <summary>
/// A class containing a variety of Math helper functions
/// http://www.codeproject.com/Articles/990452/Interception-of-Two-Moving-Objects-in-D-Space
/// </summary>
public static class CMath
{
    /// <summary>
    /// The natural logarithm of 2
    /// </summary>
    public static double NaturalLogOf2 = Math.Log( 2.0 );

    /// <summary>
    /// PI
    /// </summary>
    public const double PI = Math.PI;

    /// <summary>
    /// PI over 2 (half of PI)
    /// </summary>
    public const double PiOver2 = Math.PI / 2;

    /// <summary>
    /// PI over 4
    /// </summary>
    public const double PiOver4 = Math.PI / 4;

    /// <summary>
    /// Two times PI
    /// </summary>
    public const double TwoPi = Math.PI * 2;

    /// <summary>
    /// PI as a float (not a double)
    /// </summary>
    public const float PiFloat = (float) Math.PI;

    /// <summary>
    /// Two times PI as a float (not a double)
    /// </summary>
    public const float TwoPiFloat = (float) Math.PI * 2;



    /// <summary>
    /// The square root of 2
    /// </summary>
    public static double SquareRootOf2 = Math.Sqrt( 2 );


    /// <summary>
    /// Calculate the sum of all consecutive integers between 0 and some value, inclusive.
    /// </summary>
    /// <param name="p_int">The last consecutive integer to include in the sum</param>
    /// <returns>
    /// The sum of all integers between 0 and <paramref name="p_int"/> , inclusive
    /// </returns>
    public static int SumOfConsecutiveInts( int p_int )
    {
        return p_int * (p_int + 1) / 2;
    }

    /// <summary>
    /// Calculate the largest integer where the <see cref="SumOfConsecutiveInts"/> is less
    /// than or equal to some value.
    /// </summary>
    /// <param name="p_sum">An integer</param>
    /// <returns>
    /// The largest integer producing a "SumOfConsecutiveInts" less than or equal to
    /// <paramref name="p_sum"/> .
    /// </returns>
    public static int InverseSumOfConsecutiveInts( int p_sum )
    {
        return (int) ((Math.Sqrt( 8 * p_sum + 1 ) - 1) / 2);
    }

    /// <summary>
    /// A sigmoid function that will re-scale values between 0 and 1 to the curve between
    /// p_base and (1 - p_base).
    /// </summary>
    /// <remarks>
    /// This calculation takes approx .165 Microseconds on a 2.8GHz Intel processor.
    /// </remarks>
    /// <param name="p_x">The value to scale</param>
    /// <param name="p_base">
    /// The return value for an input of 0, or (1-return) for an input of 1
    /// </param>
    /// <returns>
    /// Approaches 0 as p_x approaches negative-infinity, and approches 1 as p_x approaches
    /// infinity
    /// </returns>
    public static double Sigmoid( double p_x, double p_base )
    {
        double K = 2 * Math.Log( (1 / p_base) - 1 );
        double a = 1 + Math.Exp( -K * (p_x - .5) );
        return 1 / a;
    }

    /// <summary>
    /// Given a number between -1 and 1, assumed to be a linear scale, translate the number
    /// onto an "S" with the degree being the "verticalness" of the middle line of the "S"
    /// </summary>
    /// <param name="p_number"></param>
    /// <param name="p_degree"></param>
    /// <returns></returns>
    public static double CheapSigmoid( this double p_number, double p_degree )
    {
        return Math.Sign( p_number ) * Math.Pow( Math.Abs( p_number ), 1.0 / p_degree );
    }

    /// <summary>
    /// Given a number between 0 and 1, assumed to be a linear scale, translate the number
    /// onto an "S" with the degree being the "verticalness" of the middle line of the "S"
    /// </summary>
    /// <param name="p_number"></param>
    /// <param name="p_degree"></param>
    /// <returns></returns>
    public static double CheapSigmoidZeroBased( this double p_number, double p_degree )
    {
        double x = p_number * 2 - 1;
        x = x.CheapSigmoid( p_degree );
        return (x + 1) / 2.0;
    }

    /// <summary>
    /// Return a number that's guaranteed to be greater than min, but less than max, and
    /// equal to "this" number if its in this range.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_min">If the number is less than this value, return this value</param>
    /// <param name="p_max">
    /// If the number is greater than this value, return this value
    /// </param>
    /// <returns>A number "clamped" between two numbers</returns>
    public static double Clamp( this double p_value, double p_min, double p_max )
    {
        if (p_value < p_min)
            return p_min;
        if (p_value > p_max)
            return p_max;
        return p_value;
    }

    /// <summary>
    /// Return a number that's guaranteed to be greater than min, but less than max, and
    /// equal to "this" number if its in this range.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_min">If the number is less than this value, return this value</param>
    /// <param name="p_max">
    /// If the number is greater than this value, return this value
    /// </param>
    /// <returns>A number "clamped" between two numbers</returns>
    public static float Clamp( this float p_value, float p_min, float p_max )
    {
        if (p_value < p_min)
            return p_min;
        if (p_value > p_max)
            return p_max;
        return p_value;
    }

    /// <summary>
    /// Return a number that's guaranteed to be greater than or equal to min, but less than
    /// or equal to max, and equal to "this" number if its in this range.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_min">If the number is less than this value, return this value</param>
    /// <param name="p_max">
    /// If the number is greater than this value, return this value
    /// </param>
    /// <returns>A number "clamped" between two numbers</returns>
    public static int Clamp( this int p_value, int p_min, int p_max )
    {
        if (p_value < p_min)
            return p_min;
        if (p_value > p_max)
            return p_max;
        return p_value;
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref double p_1, ref double p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref float p_1, ref float p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref byte p_1, ref byte p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref short p_1, ref short p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref int p_1, ref int p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }

    /// <summary>
    /// Order two numbers- Swap them if they are not in ascending order.
    /// </summary>
    /// <param name="p_1">The first number, you want this to be smaller</param>
    /// <param name="p_2">The second number, you want this to be larger</param>
    public static void Order( ref long p_1, ref long p_2 )
    {
        if (p_1 > p_2)
        {
            var tmp = p_1;
            p_1 = p_2;
            p_2 = tmp;
        }
    }


    /// <summary>
    /// Determine if a value is "between" two other values- If the value EQUALS either
    /// value, it IS considered "between" them. The parameters can be in either order.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_1">The first value to check against</param>
    /// <param name="p_2">The second value to check against</param>
    /// <returns></returns>
    public static bool IsBetween( this double p_value, double p_1, double p_2 )
    {
        return (p_value >= p_1 && p_value <= p_2) || (p_value >= p_2 && p_value <= p_1);
    }

    /// <summary>
    /// Determine if a value is "between" two other values- If the value EQUALS either
    /// value, it IS considered "between" them. The parameters can be in either order.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_1">The first value to check against</param>
    /// <param name="p_2">The second value to check against</param>
    /// <returns></returns>
    public static bool IsBetween( this float p_value, float p_1, float p_2 )
    {
        return (p_value >= p_1 && p_value <= p_2) || (p_value >= p_2 && p_value <= p_1);
    }

    /// <summary>
    /// Determine if a value is "between" two other values- If the value EQUALS either
    /// value, it IS considered "between" them. The parameters can be in either order.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_1">The first value to check against</param>
    /// <param name="p_2">The second value to check against</param>
    /// <returns></returns>
    public static bool IsBetween( this byte p_value, byte p_1, byte p_2 )
    {
        return (p_value >= p_1 && p_value <= p_2) || (p_value >= p_2 && p_value <= p_1);
    }

    /// <summary>
    /// Determine if a value is "between" two other values- If the value EQUALS either
    /// value, it IS considered "between" them. The parameters can be in either order.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_1">The first value to check against</param>
    /// <param name="p_2">The second value to check against</param>
    /// <returns></returns>
    public static bool IsBetween( this int p_value, int p_1, int p_2 )
    {
        return (p_value >= p_1 && p_value <= p_2) || (p_value >= p_2 && p_value <= p_1);
    }

    /// <summary>
    /// Determine if a value is "between" two other values- If the value EQUALS either
    /// value, it IS considered "between" them. The parameters can be in either order.
    /// </summary>
    /// <param name="p_value">The value to test</param>
    /// <param name="p_1">The first value to check against</param>
    /// <param name="p_2">The second value to check against</param>
    /// <returns></returns>
    public static bool IsBetween( this long p_value, long p_1, long p_2 )
    {
        return (p_value >= p_1 && p_value <= p_2) || (p_value >= p_2 && p_value <= p_1);
    }




    /// <summary>
    /// Returns TRUE if two values are "close", as defined by one hundredth of one percent
    /// of the smaller of the two values
    /// </summary>
    /// <param name="p_this">One of the values to test</param>
    /// <param name="p_other">One of the values to test</param>
    /// <returns>TRUE if the two values are close to each other</returns>
    public static bool IsClose( this double p_this, double p_other )
    {
        var tolerance = Math.Min( p_this, p_other ) * 0.0001;

        var delta = p_this - p_other;
        if (delta < 0)
            delta = -delta;
        return delta < tolerance;
    }

    /// <summary>
    /// Returns TRUE if two values are "close", as defined by one hundredth of one percent
    /// of the smaller of the two values
    /// </summary>
    /// <param name="p_this">One of the values to test</param>
    /// <param name="p_other">One of the values to test</param>
    /// <returns>TRUE if the two values are close to each other</returns>
    public static bool IsClose( this float p_this, float p_other )
    {
        var tolerance = Math.Min( p_this, p_other ) * 0.0001f;

        var delta = p_this - p_other;
        if (delta < 0)
            delta = -delta;
        return delta < tolerance;
    }



    /// <summary>
    /// Make sure an angle, in radians, fits between ( -PI .. PI ]
    /// </summary>
    /// <param name="p_radians">
    /// The angle to "fix" if its outside the range ( -PI .. PI ]
    /// </param>
    /// <returns>The angle, adjusted if necessary to fit within ( -PI .. PI ]</returns>
    public static double FixAngle( this double p_radians )
    {
        double rad = p_radians % TwoPi; // yields a number between -2PI and 2PI

        if (rad > Math.PI)
            rad -= TwoPi;
        else if (rad < -Math.PI)
            rad += TwoPi;

        return rad;
    }

    /// <summary>
    /// Make sure an angle, in radians, fits between ( -PI .. PI ]
    /// </summary>
    /// <param name="p_radians">
    /// The angle to "fix" if its outside the range ( -PI .. PI ]
    /// </param>
    /// <returns>The angle, adjusted if necessary to fit within ( -PI .. PI ]</returns>
    public static float FixAngle( this float p_radians )
    {
        float rad = p_radians % TwoPiFloat; // yields a number between -2PI and 2PI

        if (rad > PiFloat)
            rad -= TwoPiFloat;
        else if (rad < -PiFloat)
            rad += TwoPiFloat;

        return rad;
    }


    /// <summary>
    /// Linear Interpolation between two values- Lerp amount 0 returns p_1, Lerp amount 1 =
    /// p_2
    /// </summary>
    /// <param name="p_1">The first point, corresponding to Lerp Amount == 0</param>
    /// <param name="p_2">The second point, corresponding to Lerp Amount == 1</param>
    /// <param name="p_lerpAmount">The position, relative to p_1 and p_2</param>
    /// <returns>
    /// A value linearly interpolated between p_1 and p_2 based on p_lerpAmount
    /// </returns>
    public static float Lerp( float p_1, float p_2, float p_lerpAmount )
    {
        var dist = p_2 - p_1;
        return p_lerpAmount * dist + p_1;
    }

    /// <summary>
    /// Linear Interpolation between two values- Lerp amount 0 returns p_1, Lerp amount 1 =
    /// p_2
    /// </summary>
    /// <param name="p_1">The first point, corresponding to Lerp Amount == 0</param>
    /// <param name="p_2">The second point, corresponding to Lerp Amount == 1</param>
    /// <param name="p_lerpAmount">The position, relative to p_1 and p_2</param>
    /// <returns>
    /// A value linearly interpolated between p_1 and p_2 based on p_lerpAmount
    /// </returns>
    public static double Lerp( double p_1, double p_2, double p_lerpAmount )
    {
        var dist = p_2 - p_1;
        return p_lerpAmount * dist + p_1;
    }


    /// <summary>
    /// Solve a quadratic equation in the form ax^2 + bx + c = 0
    /// </summary>
    /// <param name="a">Coefficient for x^2</param>
    /// <param name="b">Coefficient for x</param>
    /// <param name="c">Constant</param>
    /// <param name="solution1">The first solution</param>
    /// <param name="solution2">The second solution</param>
    /// <returns>TRUE if a solution exists, FALSE if one does not</returns>
    public static bool QuadraticSolver( double a, double b, double c, out double solution1, out double solution2 )
    {
        if (a == 0)
        {
            if (b == 0)
            {
                solution1 = solution2 = double.NaN;
                return false;
            }
            else
            {
                solution1 = solution2 = -c / b;
                return true;
            }
        }

        double tmp = b * b - 4 * a * c;
        if (tmp < 0)
        {
            solution1 = solution2 = double.NaN;
            return false;
        }

        tmp = Math.Sqrt( tmp );
        double _2a = 2 * a;
        solution1 = (-b + tmp) / _2a;
        solution2 = (-b - tmp) / _2a;
        return true;
    }

    /// <summary>
    /// This is a speed-optimized calculator of the integral value of the "log2" (logarithm
    /// base 2) of an Int32
    /// </summary>
    /// <param name="p_number">The number to calculate the Log2 of</param>
    /// <returns>The integer part of the Log2 of p_number</returns>
    public static int Log2Int( this int p_number )
    {
        int bits = 0;
        int n = p_number;

        if (n > 0xffff)
        {
            n >>= 16;
            bits = 0x10;
        }

        if (n > 0xff)
        {
            n >>= 8;
            bits |= 0x8;
        }

        if (n > 0xf)
        {
            n >>= 4;
            bits |= 0x4;
        }

        if (n > 0x3)
        {
            n >>= 2;
            bits |= 0x2;
        }

        if (n > 0x1)
        {
            bits |= 0x1;
        }

        // Note- conscious choice to return 0 if p_number==0, even though the log is
        // undefined for 0.
        return bits;
    }

}
