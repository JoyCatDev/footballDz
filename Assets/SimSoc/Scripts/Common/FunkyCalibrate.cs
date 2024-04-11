using UnityEngine;
using System.Collections;

/// <summary>
/// Static methods for calibrating the accelerometer, and reading the calibrated accelerometer.
/// Author: Diorgo Jonkers
/// Version: 1.0.0
/// 
/// DISCLAIMER OF WARRANTY
/// THIS SOFTWARE IS PROVIDED "AS IS". THE AUTHOR DISCLAIMS ALL WARRANTIES, EXPRESSED OR IMPLIED, 
/// INCLUDING, WITHOUT LIMITATION, THE WARRANTIES OF MERCHANTABILITY AND OF FITNESS FOR ANY PURPOSE.
/// THE AUTHOR ASSUMES NO LIABILITY FOR DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
/// CONSEQUENTIAL DAMAGES, WHICH MAY RESULT FROM THE USE OF THE SOFTWARE, EVEN IF ADVISED OF THE 
/// POSSIBILITY OF SUCH DAMAGE. AUTHOR DOES NOT WARRANT THAT THE SOFTWARE WILL FUNCTION WITHOUT 
/// INTERRUPTION OR BE ERROR FREE, THAT AUTHOR WILL CORRECT ALL DEFICIENCIES, ERRORS, DEFECTS OR 
/// NONCONFORMITIES OR THAT THE SOFTWARE WILL MEET YOUR SPECIFIC REQUIREMENTS. THIS DISCLAIMER OF 
/// WARRANTY CONSTITUTES AN ESSENTIAL PART OF THIS AGREEMENT. NO USE OF THE SOFTWARE IS AUTHORIZED 
/// HEREUNDER EXCEPT UNDER THIS DISCLAIMER.
/// 
/// </summary>
public class FunkyCalibrate
{
	// Private
	//--------
	// The calibrated rotation. Must be set via Calibrate() or RestoreCalibration().
	static private Quaternion calibratedRotation = Quaternion.Inverse(Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), Vector3.zero));

	// The deadzone. Keep this small, ideally less than 0.1. Set it via SetDeadZone().
	static private Vector3 deadZone = Vector3.zero;



	// Methods
	//--------

	/// <summary>
	/// Get the calibrated accelerometer values.
	/// If you want your game to be more responsive then normalise the returned vector.
	/// </summary>
	/// <returns>The calibrated accelerometer.</returns>
	static public Vector3 GetAccelerometer()
	{
		Vector3 result = calibratedRotation * Input.acceleration;
		
		if (Mathf.Abs(result.x) < deadZone.x)
		{
			result.x = 0.0f;
		}
		if (Mathf.Abs(result.y) < deadZone.y)
		{
			result.y = 0.0f;
		}
		if (Mathf.Abs(result.z) < deadZone.z)
		{
			result.z = 0.0f;
		}
		
		return (result);
	}


	/// <summary>
	/// Get the calibrated accelerometer values for X and Y. Most games will use this method for input (e.g. to move a character).
	/// If you want your game to be more responsive then normalise the returned vector.
	/// </summary>
	/// <returns>The calibrated accelerometer.</returns>
	static public Vector2 GetAccelerometerXY()
	{
		Vector3 v3 = calibratedRotation * Input.acceleration;
		Vector2 v2 = new Vector2(v3.x, v3.y);

		if (Mathf.Abs(v2.x) < deadZone.x)
		{
			v2.x = 0.0f;
		}
		if (Mathf.Abs(v2.y) < deadZone.y)
		{
			v2.y = 0.0f;
		}

		return (v2);
	}


	/// <summary>
	/// Sets the dead zone.
	/// </summary>
	/// <param name="newDeadZone">New dead zone.</param>
	static public void SetDeadZone(Vector3 newDeadZone)
	{
		deadZone = newDeadZone;
	}


	/// <summary>
	/// Calibrate the accelerometer.
	/// </summary>
	/// <returns>The vector which can be used to restore the calibration via the RestoreCalibration() method. This
	/// vector is usually saved to disk and loaded when the game is run again, so that the user does Not have to calibrate
	/// the device every time.</returns>
	static public Vector3 Calibrate()
	{
		Vector3 vector = Input.acceleration;
		Quaternion rotation = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), vector);
		calibratedRotation = Quaternion.Inverse(rotation);
		return (vector);
	}


	/// <summary>
	/// Set the calibration to a vector that was previously obtained via the Calibrate() method.
	/// </summary>
	/// <param name="vector">Vector.</param>
	static public void RestoreCalibration(Vector3 vector)
	{
		Quaternion rotation = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), vector);
		calibratedRotation = Quaternion.Inverse(rotation);
	}
}
