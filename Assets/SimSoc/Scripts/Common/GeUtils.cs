using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Utilities
// Useful static methods.
static public class GeUtils
{
	
	/// <summary>
	/// Get the angle (degrees) from the vector. Angle 0 is up and increases clockwise.
	/// </summary>
	/// <returns>The angle from vector.</returns>
	/// <param name="vector">Vector.</param>
	static public float GetAngleFromVector(Vector2 vector)
	{
		float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
		angle += 90.0f;
		if (angle < 0.0f)
		{
			angle += 360.0f;
		}
		return (angle);
	}


	/// <summary>
	/// Get the angle (degrees) from the vector3. Y is ignored. Angle 0 is up and increases clockwise.
	/// </summary>
	/// <returns>The angle from vector.</returns>
	/// <param name="vector">Vector.</param>
	static public float GetAngleFromVector3(Vector3 vector)
	{
		float angle = Mathf.Atan2(vector.z, vector.x) * Mathf.Rad2Deg;
		angle += 90.0f;
		if (angle < 0.0f)
		{
			angle += 360.0f;
		}
		return (angle);
	}
	
	
	/// <summary>
	/// Get the vector from the angle (degrees). Angle 0 is up and increases clockwise. 
	/// At 0 degrees: X=0, Y=-1. At 45 degrees: X=0.7, Y=-0.7. At 90 degrees: X=1, Y=0.
	/// </summary>
	/// <returns>The vector from angle.</returns>
	/// <param name="angle">Angle.</param>
	static public Vector2 GetVectorFromAngle(float angle)
	{
		// Coordinates are rotated by these equations
		// x' = x * cos(angle) - y * sin(angle)
		// y' = x * sin(angle) + y * cos(angle)
		Vector2 vec = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad));
		return (vec);
	}
	

	/// <summary>
	/// Get the vector from the angle (degrees). Angle 0 is up and increases clockwise. Ignores Z.
	/// At 0 degrees: X=0, Y=-1. At 45 degrees: X=0.7, Y=-0.7. At 90 degrees: X=1, Y=0.
	/// </summary>
	/// <returns>The vector3 from angle.</returns>
	/// <param name="angle">Angle.</param>
	static public Vector3 GetVector3FromAngle(float angle)
	{
		// Coordinates are rotated by these equations
		// x' = x * cos(angle) - y * sin(angle)
		// y' = x * sin(angle) + y * cos(angle)
		Vector3 vec = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f);
		return (vec);
	}
	
	
	/// <summary>
	/// Get the angle between 2 vectors, from "from" to "to". Positive is clockwise and negative is counter-clockwise.
	/// </summary>
	/// <returns>The <see cref="[UnknownType System.Single]"/>.</returns>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	static public float GetAngleBetween(Vector2 from, Vector2 to)
	{
		float angle = Vector2.Angle(from, to);
		Vector3 cross = Vector3.Cross(new Vector3(from.x, from.y, 0.0f), new Vector3(to.x, to.y, 0.0f));
		if (cross.z < 0.0f)
		{
			angle = -angle;
		}
		return (angle);
	}


	/// <summary>
	/// Get the angle between 2 vectors, from "from" to "to". Positive is clockwise and negative is counter-clockwise.
	/// </summary>
	/// <returns>The <see cref="[UnknownType System.Single]"/>.</returns>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	static public float GetAngleBetween(Vector3 from, Vector3 to)
	{
		float angle = Vector3.Angle(from, to);
		Vector3 cross = Vector3.Cross(from, to);
		if (cross.z < 0.0f)
		{
			angle = -angle;
		}
		return (angle);
	}

	
	/// <summary>
	/// Rotate a vector clockwise.
	/// </summary>
	/// <returns>The vector.</returns>
	/// <param name="vector">Vector.</param>
	/// <param name="angle">Angle.</param>
	static public Vector2 RotateVector(Vector2 vector, float angle)
	{
		// Coordinates are rotated by these equations
		// x' = x * cos(angle) - y * sin(angle)
		// y' = x * sin(angle) + y * cos(angle)
		Vector2 vec;
		vec.x = (vector.x * Mathf.Cos(angle * Mathf.Deg2Rad)) - (vector.y * Mathf.Sin(angle * Mathf.Deg2Rad));
		vec.y = (vector.x * Mathf.Sin(angle * Mathf.Deg2Rad)) + (vector.y * Mathf.Cos(angle * Mathf.Deg2Rad));
		return (vec);
	}
	

	/// <summary>
	/// Rotate a vector clockwise. Ignores Z.
	/// </summary>
	/// <returns>The vector.</returns>
	/// <param name="vector">Vector.</param>
	/// <param name="angle">Angle.</param>
	static public Vector3 RotateVector(Vector3 vector, float angle)
	{
		// Coordinates are rotated by these equations
		// x' = x * cos(angle) - y * sin(angle)
		// y' = x * sin(angle) + y * cos(angle)
		Vector3 vec;
		vec.x = (vector.x * Mathf.Cos(angle * Mathf.Deg2Rad)) - (vector.y * Mathf.Sin(angle * Mathf.Deg2Rad));
		vec.y = (vector.x * Mathf.Sin(angle * Mathf.Deg2Rad)) + (vector.y * Mathf.Cos(angle * Mathf.Deg2Rad));
		vec.z = 0.0f;
		return (vec);
	}
	
	
	//  public domain function by Darel Rex Finley, 2006
	//
	//  Determines the intersection point of the line segment defined by points A and B
	//  with the line segment defined by points C and D.
	//
	//  Returns YES if the intersection point was found, and stores that point in X,Y.
	//  Returns NO if there is no determinable intersection point, in which case X,Y will
	//  be unmodified.
	static public bool LineSegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 getPoint)
	{
        float Seg1X = B.x - A.x;
        float Seg1Y = B.y - A.y;
        float Seg2X = D.x - C.x;
        float Seg2Y = D.y - C.y;
        
        float DiffSegX = C.x - A.x;
        float DiffSegY = C.y - A.y;
        
        float Seg2CrossSeg1Z = Seg2X * Seg1Y - Seg2Y * Seg1X;
        float lengthA = (Seg2X * DiffSegY - Seg2Y * DiffSegX) / Seg2CrossSeg1Z;
        float lengthB = (Seg1X * DiffSegY - Seg1Y * DiffSegX) / Seg2CrossSeg1Z;
        
        if(lengthA >= 0f && lengthA <= 1f && lengthB >= 0f && lengthB <= 1f)
        {
			getPoint.x = A.x + lengthA*Seg1X;
			getPoint.y = A.y + lengthA*Seg1Y;
            return true;
        }
		getPoint = Vector2.zero;
        return false;
	}


	static public bool LineSegmentIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D, out Vector2 getPoint, out float getDistance)
	{
		float Seg1X = B.x - A.x;
		float Seg1Y = B.y - A.y;
		float Seg2X = D.x - C.x;
		float Seg2Y = D.y - C.y;
		
		float DiffSegX = C.x - A.x;
		float DiffSegY = C.y - A.y;
		
		float Seg2CrossSeg1Z = Seg2X * Seg1Y - Seg2Y * Seg1X;
		float lengthA = (Seg2X * DiffSegY - Seg2Y * DiffSegX) / Seg2CrossSeg1Z;
		float lengthB = (Seg1X * DiffSegY - Seg1Y * DiffSegX) / Seg2CrossSeg1Z;
		
		if(lengthA >= 0f && lengthA <= 1f && lengthB >= 0f && lengthB <= 1f)
		{
			getPoint.x = A.x + lengthA*Seg1X;
			getPoint.y = A.y + lengthA*Seg1Y;
			getDistance = new Vector2(getPoint.x - A.x, getPoint.y - A.y).magnitude;
			return true;
		}
		getPoint = Vector2.zero;
		getDistance = 0.0f;
		return false;
	}
	
	// getDistance - Get the distance from point A to the collision point.
	static public bool LineSegmentIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D, ref Vector3 getPoint, ref float getDistance)
	{
        float Seg1X = B.x - A.x;
        float Seg1Y = B.y - A.y;
        float Seg2X = D.x - C.x;
        float Seg2Y = D.y - C.y;
        
        float DiffSegX = C.x - A.x;
        float DiffSegY = C.y - A.y;
        
        float Seg2CrossSeg1Z = Seg2X * Seg1Y - Seg2Y * Seg1X;
        float lengthA = (Seg2X * DiffSegY - Seg2Y * DiffSegX) / Seg2CrossSeg1Z;
        float lengthB = (Seg1X * DiffSegY - Seg1Y * DiffSegX) / Seg2CrossSeg1Z;
        
        if (lengthA >= 0f && lengthA <= 1f && lengthB >= 0f && lengthB <= 1f)
        {
			getPoint.x = A.x + lengthA*Seg1X;
			getPoint.y = A.y + lengthA*Seg1Y;
			getPoint.z = 0.0f;
			
			getDistance = new Vector3(getPoint.x - A.x, getPoint.y - A.y, 0.0f).magnitude;
            return true;
        }
        return false;
	}


	/// <summary>
	/// Test if the line intersects the rectangle.
	/// </summary>
	/// <returns>The rectangle intersect.</returns>
	/// <param name="A">A.</param>
	/// <param name="B">B.</param>
	/// <param name="rect">Rect.</param>
	/// <param name="getPoint">Get point.</param>
	static public bool LineRectangleIntersect(Vector2 A, Vector2 B, Rect rect, out Vector2 getPoint)
	{
		Vector2 point, nearPoint;
		float distance, nearDistance;
		bool result = false;

		nearPoint = Vector2.zero;
		nearDistance = float.MaxValue;

		// Left
		if (LineSegmentIntersection(A, B, 
		                            new Vector2(rect.min.x, rect.min.y), 
		                            new Vector2(rect.min.x, rect.max.y), 
		                            out point, out distance))
		{
			if (distance < nearDistance)
			{
				result = true;
				nearPoint = point;
				nearDistance = distance;
			}
		}

		// Right
		if (LineSegmentIntersection(A, B, 
		                            new Vector2(rect.max.x, rect.min.y), 
		                            new Vector2(rect.max.x, rect.max.y), 
		                            out point, out distance))
		{
			if (distance < nearDistance)
			{
				result = true;
				nearPoint = point;
				nearDistance = distance;
			}
		}

		// Top
		if (LineSegmentIntersection(A, B, 
		                            new Vector2(rect.min.x, rect.max.y), 
		                            new Vector2(rect.max.x, rect.max.y), 
		                            out point, out distance))
		{
			if (distance < nearDistance)
			{
				result = true;
				nearPoint = point;
				nearDistance = distance;
			}
		}

		// Bottom
		if (LineSegmentIntersection(A, B, 
		                            new Vector2(rect.min.x, rect.min.y), 
		                            new Vector2(rect.max.x, rect.min.y), 
		                            out point, out distance))
		{
			if (distance < nearDistance)
			{
				result = true;
				nearPoint = point;
				nearDistance = distance;
			}
		}

		getPoint = nearPoint;

		return (result);
	}

	
	// Test if a line intersects a bounding box from the outside towards the inside. Ignores Z component.
	static public bool LineIntersectBoundingBox(Vector3 lineStart, Vector3 lineEnd, Bounds bounds, 
												ref float getDistance, ref Vector3 getPoint, ref Vector3 getEdgeNormal, 
												ref Vector3 getReflection, ref Vector3 getProjection)
	{
		float nearestDistance = float.MaxValue;
		float tempDistance = float.MaxValue;
		Vector3 tempPoint = Vector3.zero;
		Vector3 edgeStart, edgeEnd, vec;
		bool collided = false;
		
		vec = lineEnd - lineStart;
		
		if (vec.y > 0.0f)
		{
			// Top edge
			edgeStart = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, 0.0f);
			edgeEnd = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, 0.0f);
			if (LineSegmentIntersection(lineStart, lineEnd, edgeStart, edgeEnd, ref tempPoint, ref tempDistance))
			{
				nearestDistance = tempDistance;
				getDistance = tempDistance;
				getPoint = tempPoint;
				getEdgeNormal = new Vector3(0.0f, -1.0f, 0.0f);
				getReflection = Vector3.Reflect(vec, getEdgeNormal);
				getProjection = Vector3.Project(vec, new Vector3(1.0f, 0.0f, 0.0f));
				collided = true;
			}
		}
		else if (vec.y < 0.0f)
		{
			// Bottom edge
			edgeStart = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, 0.0f);
			edgeEnd = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, 0.0f);
			if (LineSegmentIntersection(lineStart, lineEnd, edgeStart, edgeEnd, ref tempPoint, ref tempDistance))
			{
				nearestDistance = tempDistance;
				getDistance = tempDistance;
				getPoint = tempPoint;
				getEdgeNormal = new Vector3(0.0f, 1.0f, 0.0f);
				getReflection = Vector3.Reflect(vec, getEdgeNormal);
				getProjection = Vector3.Project(vec, new Vector3(-1.0f, 0.0f, 0.0f));
				collided = true;
			}
		}
		
		if (vec.x > 0.0f)
		{
			// Left edge
			edgeStart = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, 0.0f);
			edgeEnd = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, 0.0f);
			if ((LineSegmentIntersection(lineStart, lineEnd, edgeStart, edgeEnd, ref tempPoint, ref tempDistance)) && (nearestDistance > tempDistance))
			{
				nearestDistance = tempDistance;
				getDistance = tempDistance;
				getPoint = tempPoint;
				getEdgeNormal = new Vector3(-1.0f, 0.0f, 0.0f);
				getReflection = Vector3.Reflect(vec, getEdgeNormal);
				getProjection = Vector3.Project(vec, new Vector3(0.0f, -1.0f, 0.0f));
				collided = true;
			}
		}
		else if (vec.x < 0.0f)
		{
			// Right edge
			edgeStart = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, 0.0f);
			edgeEnd = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, 0.0f);
			if ((LineSegmentIntersection(lineStart, lineEnd, edgeStart, edgeEnd, ref tempPoint, ref tempDistance)) && (nearestDistance > tempDistance))
			{
				nearestDistance = tempDistance;
				getDistance = tempDistance;
				getPoint = tempPoint;
				getEdgeNormal = new Vector3(1.0f, 0.0f, 0.0f);
				getReflection = Vector3.Reflect(vec, getEdgeNormal);
				getProjection = Vector3.Project(vec, new Vector3(0.0f, 1.0f, 0.0f));
				collided = true;
			}
		}
		
		return (collided);
	}


	/// <summary>
	/// Gets the closest point on line segment.
	/// </summary>
	/// <returns>The closest point on line segment.</returns>
	/// <param name="A">A.</param>
	/// <param name="B">B.</param>
	/// <param name="P">P.</param>
	public static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
	{
		Vector2 AP = P - A;       //Vector from A to P   
		Vector2 AB = B - A;       //Vector from A to B  
		
		float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
		float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
		float distance = 0;	//ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

		if (magnitudeAB > 0)
		{
			distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  
		}

		if (distance < 0)     //Check if P projection is over vectorAB     
		{
			return A;
			
		}
		else if (distance > 1)
		{
			return B;
		}
		else
		{
			return A + AB * distance;
		}
	}


	/// <summary>
	/// Find the closest point on a line to another point in 3D space.
	/// </summary>
	/// <returns>The point on line.</returns>
	/// <param name="vA">Line start point.</param>
	/// <param name="vB">Line end point,</param>
	/// <param name="vPoint">Point to which to find the closest point on the line.</param>
	static public Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
	{
		Vector3 vVector1 = vPoint - vA;
		Vector3 vVector2 = (vB - vA).normalized;
		
		var d = Vector3.Distance(vA, vB);
		var t = Vector3.Dot(vVector2, vVector1);
		
		if (t <= 0)
			return vA;
		
		if (t >= d)
			return vB;
		
		var vVector3 = vVector2 * t;
		
		var vClosestPoint = vA + vVector3;
		
		return vClosestPoint;
	}


	/// <summary>
	/// Test if a 2D circle and rect collide.
	/// http://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection/402010#402010
	/// </summary>
	/// <returns>The rect collision.</returns>
	/// <param name="circleCentre">Circle centre.</param>
	/// <param name="circleRadius">Circle radius.</param>
	/// <param name="rect">Rect.</param>
	static public bool CircleRectCollision(Vector2 circleCentre, float circleRadius, Rect rect)
	{
		Vector2 circleDistance = new Vector2(Mathf.Abs(circleCentre.x - rect.center.x),
		                                     Mathf.Abs(circleCentre.y - rect.center.y));

		if (circleDistance.x > ((rect.width / 2.0f) + circleRadius))
		{
			return (false);
		}
		if (circleDistance.y > ((rect.height / 2.0f) + circleRadius))
		{
			return (false);
		}
		
		if (circleDistance.x <= (rect.width / 2.0f))
		{
			return (true);
		} 
		if (circleDistance.y <= (rect.height / 2.0f))
		{
			return (true);
		}
		
		float cornerDistance_sq = ((circleDistance.x - (rect.width / 2.0f)) * (circleDistance.x - (rect.width / 2.0f))) +
								  ((circleDistance.y - (rect.height / 2.0f)) * (circleDistance.y - (rect.height / 2.0f)));
		
		return (cornerDistance_sq <= (circleRadius * circleRadius));
	}

	
	static public float Lerp(float input, float inputMin, float inputMax, float outputMin, float outputMax)
	{
		if ((inputMax - inputMin) == 0.0f)
		{
			return (outputMax);
		}
		return (Mathf.Lerp(outputMin, outputMax, (input - inputMin) / (inputMax - inputMin)));
	}
	
	
	// Returns value from 0 to 1
	static public float BellCurveInterpolate(float time)
	{
		return ((float)(0.5f + ((Mathf.Cos((time * Mathf.PI) + Mathf.PI)) * 0.5f)));
	}

	
	static public float SquareNumber(float number)
	{
		return (number * number);
	}
	
	
	// Test if two floats are approximately the same, using float.Epsilon.
	static public bool Approximately(float a, float b)
	{
		return (Mathf.Abs(a - b) < float.Epsilon);
	}
	
	// Test if two floats are approximately the same, using epsilon.
	static public bool Approximately(float a, float b, float epsilon)
	{
		return (Mathf.Abs(a - b) < epsilon);
	}
	
	
	// Load a texture from the Resources folder.
	static public Texture LoadTexture(string fileName)
	{
		if ((fileName != null) && (fileName.Length > 0))
		{
			Texture texture = (Texture)Resources.Load(fileName, typeof(Texture));
			return (texture);
		}
		return (null);
	}
	
	
	// Load a texture from the Resources folder.
	//	fileName1 - The first file to try.
	//	fileName2 - The second file to try, if fileName1 fails.
	static public Texture LoadTexture(string fileName1, string fileName2)
	{
		if ((fileName1 != null) && (fileName1.Length > 0))
		{
			Texture texture = (Texture)Resources.Load(fileName1, typeof(Texture));
			if (texture != null)
			{
				return (texture);
			}
			else if ((fileName2 != null) && (fileName2.Length > 0))
			{
				texture = (Texture)Resources.Load(fileName2, typeof(Texture));
				return (texture);
			}
		}
		return (null);
	}
	
	
	// Load a text file from the Resources folder.
	static public TextAsset LoadTextFile(string fileName)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(fileName, typeof(TextAsset));
		return (textAsset);
	}
	
	
	// Read a Vector2 from a string that has the format "x,y".
	static public Vector2 ReadVector2FromString(string s)
	{
		if ((s == null) || (s.Length <= 0))
		{
			return (Vector2.zero);
		}
		Vector2 result = Vector2.zero;
		string noSpaces = s.Trim();
		string[] fields = noSpaces.Split(',');
		if (fields != null)
		{
			if (fields.Length >= 1)
			{
				try
				{
					result.x = float.Parse(fields[0]);
				}
				catch
				{
				}
			}
			if (fields.Length >= 2)
			{
				try
				{
					result.y = float.Parse(fields[1]);
				}
				catch
				{
				}
			}
		}
		return (result);
	}
	
	
	// Read a Color from a string that has the format "r,g,b" or "r,g,b,a" in byte values.
	static public Color ReadColourFromString(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return (Color.white);
		}
		
		Color result = Color.white;
		
		try
		{
			string noSpaces = s.Trim();
			string[] colours = noSpaces.Split(',');
			if (colours != null)
			{
				if (colours.Length >= 1)
				{
					result.r = float.Parse(colours[0]) / 255.0f;
				}
				if (colours.Length >= 2)
				{
					result.g = float.Parse(colours[1]) / 255.0f;
				}
				if (colours.Length >= 3)
				{
					result.b = float.Parse(colours[2]) / 255.0f;
				}
				if (colours.Length >= 4)
				{
					result.a = float.Parse(colours[3]) / 255.0f;
				}
			}
		}
		catch
		{
		}
		
		return (result);
	}
	
	
	// Return the squared distance between the two points
	static public float SqrDistance(Vector3 point1, Vector3 point2)
	{
		Vector3 vec = point1 - point2;
		return (vec.sqrMagnitude);
	}

	
	// Round off a number to the nearest integer.
	static public float Round(float number)
	{
		float result = Mathf.Round(number);
		
		// Unity's Round method rounds to the nearest even number when the decimal is .5.
		// This rounds to the next bigger number if the decimal is .5.
		if (((number - (int)number) == 0.5f) && (result < number))
		{
			result += 1.0f;
		}
		
		return (result);
	}


	/// <summary>
	/// Load a game object from the Resources folder. This loads the prefab.
	/// Exclude the "Assets/Resources/" prefix.
	/// </summary>
	/// <returns>
	/// The game object.
	/// </returns>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	static public GameObject LoadGameObject(string fileName)
	{
		if (string.IsNullOrEmpty(fileName) == false)
		{
			GameObject go = (GameObject)Resources.Load(fileName, typeof(GameObject));
			return (go);
		}
		return (null);
	}


	/// <summary>
	/// Load a game object (and create a clone) from the Resources folder.
	/// Exclude the "Assets/Resources/" prefix.
	/// </summary>
	/// <returns>The clone.</returns>
	/// <param name="fileName">File name.</param>
	static public GameObject LoadGameObjectAndInstantiate(string fileName)
	{
		if (string.IsNullOrEmpty(fileName) == false)
		{
			GameObject prefab = (GameObject)Resources.Load(fileName, typeof(GameObject));
			if (prefab != null)
			{
				GameObject go = (GameObject)Object.Instantiate(prefab);
				return (go);
			}
		}
		return (null);
	}


	/// <summary>
	/// Makes the first letter uppercase.
	/// </summary>
	/// <returns>The letter to upper.</returns>
	/// <param name="str">String.</param>
	static public string FirstLetterToUpper(string str)
	{
		if (str == null)
			return null;
		
		if (str.Length > 1)
			return char.ToUpper(str[0]) + str.Substring(1);
		
		return str.ToUpper();
	}


	/// <summary>
	/// Sets the layer recursively.
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <param name="newLayer">New layer.</param>
	static public void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (null == obj)
		{
			return;
		}
		
		obj.layer = newLayer;
		
		foreach (Transform child in obj.transform)
		{
			if (null == child)
			{
				continue;
			}
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}


	/// <summary>
	/// Makes the object and its children static or non-static.
	/// </summary>
	/// <returns>The static recursively.</returns>
	/// <param name="obj">Object.</param>
	/// <param name="isStatic">Is static.</param>
	static public void MakeStaticRecursively(GameObject obj, bool isStatic)
	{
		if (null == obj)
		{
			return;
		}
		
		obj.isStatic = isStatic;
		
		foreach (Transform child in obj.transform)
		{
			if (null == child)
			{
				continue;
			}
			MakeStaticRecursively(child.gameObject, isStatic);
		}
	}


	/// <summary>
	/// Finds the child with the specified name. Also searches grandchildren.
	/// </summary>
	/// <returns>The child.</returns>
	/// <param name="parent">Parent transform.</param>
	/// <param name="childName">Child name.</param>
	static public Transform FindChild(Transform parent, string childName)
	{
		if ((parent == null) || (string.IsNullOrEmpty(childName)))
		{
			return (null);
		}
		Transform result = parent.transform.Find(childName);
		if (result != null)
		{
			return (result);
		}

		foreach (Transform child in parent)
		{
			if (child == null)
			{
				continue;
			}
			result = FindChild(child, childName);
			if (result != null)
			{
				return (result);
			}
		}

		return (null);
	}


	/// <summary>
	/// Convert inches to pixels, if device DPI can be detected.
	/// </summary>
	/// <returns>The to pixels.</returns>
	/// <param name="inches">Inches to convert.</param>
	/// <param name="defaultPixels">Default pixels to use if device DPI can Not be detected.</param>
	/// <param name="minPixels">Minimum pixels to return. 25 if the usual amount when using Unity GUI font.</param>
	static public float InchesToPixels(float inches, float defaultPixels, float minPixels = 25.0f)
	{
		if (Screen.dpi > 0.0f)
		{
			return (Mathf.Max(Screen.dpi * inches, minPixels));
		}
		return (Mathf.Max(defaultPixels, minPixels));
	}


	// The following 2 methods have been disabled, because PreLoadTextures generates a warning in the editor.
	// Enable them (remove #if #endif) if you want to use either method.
#if DO_NOT_COMPILE
	/// <summary>
	/// Turn off the renderers: receive shadows, cast shadows, reflection probes, light probes.
	/// </summary>
	/// <returns>The off renderers shadows.</returns>
	/// <param name="go">Go.</param>
	/// <param name="renderers">The game object's renderers. If null then the renderers will be retrieved from the game object.</param>
	/// <param name="preLoadTextures">Indicates if the textures must be pre-loaded to the GPU, to prevent stutter when object appears on screen the first time.</param>
	static public void TurnOffRenderersShadows(GameObject go, Renderer[] renderers = null, bool preLoadTextures = true)
	{
		if ((go == null) && (renderers == null))
		{
			return;
		}
		
		Renderer[] tempRenderers = renderers;
		int i;	//, tempWidth;
		Renderer r;
		
		if (go != null)
		{
			if ((tempRenderers == null) || (tempRenderers.Length <= 0))
			{
				tempRenderers = go.GetComponentsInChildren<Renderer>(true);
			}
		}
		
		if ((tempRenderers == null) || (tempRenderers.Length <= 0))
		{
			return;
		}
		
		for (i = 0; i < tempRenderers.Length; i++)
		{
			r = tempRenderers[i];
			if (r != null)
			{
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				r.receiveShadows = false;
				r.useLightProbes = false;
				r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			}
		}
		
		if (preLoadTextures)
		{
			PreLoadTextures(go, tempRenderers);
		}
	}
	
	
	/// <summary>
	/// Pre-load the textures to the GPU, to prevent stutter when object appears on screen the first time.
	/// </summary>
	/// <returns>The off renderers shadows.</returns>
	/// <param name="go">Go.</param>
	/// <param name="renderers">The game object's renderers. If null then the renderers will be retrieved from the game object.</param>
	static public void PreLoadTextures(GameObject go, Renderer[] renderers = null)
	{
		if ((go == null) && (renderers == null))
		{
			return;
		}
		
		Renderer[] tempRenderers = renderers;
		int i, tempWidth;	// NOTE: This may throw a compiler warning about being assigned but never used.
		Renderer r;
		
		if (go != null)
		{
			if ((tempRenderers == null) || (tempRenderers.Length <= 0))
			{
				tempRenderers = go.GetComponentsInChildren<Renderer>(true);
			}
		}
		
		if ((tempRenderers == null) || (tempRenderers.Length <= 0))
		{
			return;
		}
		
		for (i = 0; i < tempRenderers.Length; i++)
		{
			r = tempRenderers[i];
			if (r != null)
			{
				// Accessing the texture forces it to be loaded to the GPU, to prevent stutter when object appears on screen the first time
				if ((r.sharedMaterial != null) && (r.sharedMaterial.mainTexture != null))
				{
					tempWidth = r.sharedMaterial.mainTexture.width;
				}
				else if ((r.material != null) && (r.material.mainTexture != null))
				{
					tempWidth = r.material.mainTexture.width;
				}
			}
		}
	}
#endif //DO_NOT_COMPILE


	/// <summary>
	/// Set the quality settings level, by name.
	/// </summary>
	/// <returns>The quality settings level.</returns>
	/// <param name="levelName">Level name.</param>
	static public void SetQualitySettingsLevel(string levelName, bool applyExpensiveChanges = true)
	{
		if (string.IsNullOrEmpty(levelName))
		{
			return;
		}

		string[] names = QualitySettings.names;
		int i;

		for (i = 0; i < names.Length; i++)
		{
			if (names[i] == levelName)
			{
#if UNITY_EDITOR
				Debug.Log("Setting quality level: " + QualitySettings.names[i]);
#endif //UNITY_EDITOR

				QualitySettings.SetQualityLevel(i, applyExpensiveChanges);
				return;
			}
		}
	}


	/// <summary>
	/// Calculates the vertical velocity needed to reach the max height. It uses "Physics.gravity.y" for gravity.
	/// NOTE: If using this to predict how a Unity physics object will move then it may be off, because other factors affect the object (e.g. drag).
	/// </summary>
	/// <returns>The vertical velocity.</returns>
	/// <param name="maxHeight">Max height.</param>
	static public float CalcVerticalVelocity(float maxHeight)
	{
		// Calculating the max height:
		//	vf = velocity at max height (zero)
		//	vi = initial velocity
		//	a = gravity (acceleration)
		//	s = max height
		//	
		//	sqr(vf) - sqr(vi) = 2 * a * s 
		//		
		//	s =  (sqr(vf) - sqr(vi)) / 2a
		//
		// Calculating the velocity:
		//	vi = sqrt( -((2 * a * s) - sqr(vf)) )
		
		// Mathf.Sqrt( -((2.0f * Physics.gravity * maxHeight) - (0.0f * 0.0f)) )
		return (Mathf.Sqrt(-(2.0f * Physics.gravity.y * maxHeight)));
	}
	
	
	/// <summary>
	/// Calc the height that will be reach based on the upward velocity. It uses "Physics.gravity.y" for gravity.
	/// NOTE: If using this to predict how a Unity physics object will move then it may be off, because other factors affect the object (e.g. drag).
	/// </summary>
	/// <returns>The height.</returns>
	/// <param name="velocity">Velocity.</param>
	static public float CalcHeight(float velocity)
	{
		// Calculating the max height:
		//	vf = velocity at max height (zero)
		//	vi = initial velocity
		//	a = gravity (acceleration)
		//	s = max height
		//	
		//	sqrt(vf) - sqr(vi) = 2 * a * s 
		//		
		//	s =  (sqr(vf) - sqr(vi)) / 2a
		//
		
		// s = (sqr(0.0f) - sqr(vi)) / 2a
		return ((-(velocity * velocity)) / (2.0f * Physics.gravity.y));
	}


	/// <summary>
	/// Calc the height that will be reach after the time, based on the upward velocity. It uses "Physics.gravity.y" for gravity.
	/// NOTE: If using this to predict how a Unity physics object will move then it may be off, because other factors affect the object (e.g. drag).
	/// </summary>
	/// <returns>The height at time.</returns>
	/// <param name="velocity">Velocity.</param>
	/// <param name="time">Time.</param>
	static public float CalcHeightAtTime(float velocity, float time)
	{
		// Movement/arc calculations:
		// http://www.physicsclassroom.com/class/vectors/Lesson-2/Horizontally-Launched-Projectiles-Problem-Solving
		// Vertical arc (horizontal is same, just replace y with x):
		//	y = vert. displacement
		//	ay = vert. acceleration (in this case, gravity)
		//	t = time
		//	vfy = final vert. velocity
		//	viy = initial vert. velocity
		//		
		//	y = (viy * t) + (0.5 * ay * square(t))
		//		
		//	vfy = viy + (ay * t)
		//		
		//	square(vfy) = square(viy) + (2 * ay * y)

		//	y = (viy * t) + (0.5 * ay * square(t))
		return((velocity * time) + (0.5f * Physics.gravity.y * (time * time)));
	}



	/// <summary>
	/// Shuffle/randomize the specified list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void Shuffle<T>(this List<T> list)
	{
		int k;
		T value;
		int n = list.Count;  
		while (n > 1)
		{
			n--;  
			k = Random.Range(0, n + 1);
			value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}


	/// <summary>
	/// Loads the level by name. (Wrapper for Application.LoadLevel and SceneManager.LoadScene)
	/// </summary>
	/// <returns>The level.</returns>
	/// <param name="name">Name.</param>
	public static void LoadLevel(string name)
	{
#if UNITY_5_2
		Application.LoadLevel(name);
#else
		UnityEngine.SceneManagement.SceneManager.LoadScene(name);
#endif
	}


	/// <summary>
	/// Gets the name of the loaded level. (Wrapper for Application.loadedLevelName and SceneManager.GetActiveScene().name)
	/// </summary>
	/// <returns>The loaded level name.</returns>
	public static string GetLoadedLevelName()
	{
#if UNITY_5_2
		return (Application.loadedLevelName);
#else
		return (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
#endif
	}

}
