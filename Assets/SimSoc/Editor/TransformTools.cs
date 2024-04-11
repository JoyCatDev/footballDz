using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

/// <summary>
/// A few transform tools.
/// </summary>
public class TransformTools : SsBaseTools {

	private const string menuPathTransform = menuPath + "Transform/";

	private class CopyData
	{
		public Vector3 position;
		public Vector3 localPosition;
		public Quaternion rotation;
		public Quaternion localRotation;
		public Vector3 lossyScale;
		public Vector3 localScale;
	}
	
	private static CopyData copiedTransform = null;
	
	
	private static void CopyTransformData(Transform t)
	{
		if (copiedTransform == null)
		{
			copiedTransform = new CopyData();
		}
		if (copiedTransform != null)
		{
			copiedTransform.position = t.position;
			copiedTransform.localPosition = t.localPosition;
			copiedTransform.rotation = t.rotation;
			copiedTransform.localRotation = t.localRotation;
			copiedTransform.lossyScale = t.lossyScale;
			copiedTransform.localScale = t.localScale;
		}
	}
	

	[MenuItem(menuPathTransform + "Reset Local Transform", false, menuIndex + 1000)]
	public static void CopyToolsResetLocal()
	{
		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj != null)
			{
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
			}
		}
	}


	[MenuItem(menuPathTransform + "Copy Transform", false, menuIndex + 1001)]
	public static void CopyToolsCopyTransform()
	{
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object you want to copy.");
			return;
		}
		
		// Only copy the first selected object
		GameObject obj = Selection.gameObjects[0];
		if (obj == null)
		{
			Debug.LogError("Please select the object you want to copy.");
			return;
		}
		
		CopyTransformData(obj.transform);
	}
	
	
	
	[MenuItem(menuPathTransform + "Copy Transform (+ Clipboard)", false, menuIndex + 1002)]
	public static void CopyToolsCopyTransformToClipboard()
	{
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object you want to copy.");
			return;
		}
		
		// Only copy the first selected object
		GameObject obj = Selection.gameObjects[0];
		if (obj == null)
		{
			Debug.LogError("Please select the object you want to copy.");
			return;
		}
		
		CopyTransformData(obj.transform);
		
		string info = string.Format("{0}    \n" +
		                            "Pos:{1}, {2}, {3}    LocalPos:{4}, {5}, {6}    \n" +
		                            "Rot:{7}, {8}, {9}    LocalRot:{10}, {11}, {12}    \n" +
		                            "Scale:{13}, {14}, {15}    LocalScale: {16}, {17}, {18}    \n",
		                            obj.name, 
		                            obj.transform.position.x, obj.transform.position.y, obj.transform.position.z,
		                            obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z,
		                            obj.transform.rotation.x, obj.transform.rotation.y, obj.transform.rotation.z,
		                            obj.transform.localRotation.x, obj.transform.localRotation.y, obj.transform.localRotation.z,
		                            obj.transform.lossyScale.x, obj.transform.lossyScale.y, obj.transform.lossyScale.z,
		                            obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z);

#if UNITY_4
		System.Type T = typeof(GUIUtility);
		System.Reflection.PropertyInfo systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", 
		                                                                        System.Reflection.BindingFlags.Static | 
		                                                                        System.Reflection.BindingFlags.NonPublic);
		if (systemCopyBufferProperty != null)
		{
			systemCopyBufferProperty.SetValue(null, info, null);
		}
#else
		GUIUtility.systemCopyBuffer = info;
#endif

	}
	
	
	[MenuItem(menuPathTransform + "Paste Local Position, Rotation, Scale", false, menuIndex + 1003)]
	public static void CopyToolsPasteLocalPosRotScale()
	{
		if (copiedTransform == null)
		{
			Debug.LogError("You first have to copy a transform.");
			return;
		}
		
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object(s) you want paste transform.");
			return;
		}
		
		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj != null)
			{
				obj.transform.localPosition = copiedTransform.localPosition;
				obj.transform.localRotation = copiedTransform.localRotation;
				obj.transform.localScale = copiedTransform.localScale;
			}
		}
	}
	
	
	[MenuItem(menuPathTransform + "Paste Local Position", false, menuIndex + 1004)]
	public static void CopyToolsPasteLocalPos()
	{
		if (copiedTransform == null)
		{
			Debug.LogError("You first have to copy a transform.");
			return;
		}
		
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object(s) you want paste transform.");
			return;
		}
		
		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj != null)
			{
				obj.transform.localPosition = copiedTransform.localPosition;
			}
		}
	}
	
	
	[MenuItem(menuPathTransform + "Paste Local Rotation", false, menuIndex + 1005)]
	public static void CopyToolsPasteLocalRot()
	{
		if (copiedTransform == null)
		{
			Debug.LogError("You first have to copy a transform.");
			return;
		}
		
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object(s) you want paste transform.");
			return;
		}
		
		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj != null)
			{
				obj.transform.localRotation = copiedTransform.localRotation;
			}
		}
	}
	
	
	[MenuItem(menuPathTransform + "Paste Local Scale", false, menuIndex + 1006)]
	public static void CopyToolsPasteLocalScale()
	{
		if (copiedTransform == null)
		{
			Debug.LogError("You first have to copy a transform.");
			return;
		}
		
		if ((Selection.gameObjects == null) || (Selection.gameObjects.Length == 0))
		{
			Debug.LogError("Please select the object(s) you want paste transform.");
			return;
		}
		
		foreach (GameObject obj in Selection.gameObjects)
		{
			if (obj != null)
			{
				obj.transform.localScale = copiedTransform.localScale;
			}
		}
	}
	
}
