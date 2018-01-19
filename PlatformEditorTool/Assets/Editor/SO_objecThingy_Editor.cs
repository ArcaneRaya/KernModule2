using UnityEngine;
using UnityEditor;

using RubicalMe;

[CustomEditor (typeof(SO_objecThingy))]
public class SO_objecThingy_Editor : Editor_EasyInteractiveDisplay
{
	SerializedProperty pillar;
	SerializedProperty top;
	SerializedProperty bottom;


	public override void OnEnable ()
	{
		base.OnEnable (); 
		Display.Camera.transform.position = new Vector3 (3, 6, -7);
		Display.Camera.transform.rotation = Quaternion.Euler (30, -20, 0);
		Display.Camera.orthographic = false;
		Display.Camera.fieldOfView = 60;
		{
			Display.GUISystem.AddButton (
				SetDayMode, 
				(Texture2D)EditorGUIUtility.Load ("sun.png"),
				new Rect (-80, 5, 50, 50),
				GUISnapMode.TopCenter
			);
			Display.GUISystem.AddButton (
				SetNightMode, 
				(Texture2D)EditorGUIUtility.Load ("moon.png"),
				new Rect (30, 10, 40, 40),
				GUISnapMode.TopCenter
			);
		}
		pillar = serializedObject.FindProperty ("pillarPrefab");
		top = serializedObject.FindProperty ("topPrefab");
		bottom = serializedObject.FindProperty ("bottomPrefab");
		SetDayMode (RM_ButtonEvents.LMB);
		UpdateDisplay ();
	}

	public override void OnInspectorGUI ()
	{
		EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		EditorGUILayout.PropertyField (pillar);
		EditorGUILayout.PropertyField (top);
		EditorGUILayout.PropertyField (bottom);
		serializedObject.ApplyModifiedProperties ();
		if (EditorGUI.EndChangeCheck ()) {
			UpdateDisplay ();
		}
	}

	private void UpdateDisplay ()
	{
		Display.ClearRenderQueue ();
		if (pillar.objectReferenceValue != null) {
			AddPillars ();
		}
		if (top.objectReferenceValue != null) {
			
			Display.AddGameObject (
				top.objectReferenceValue as GameObject, 
				new Vector3 (0, 4, 0), 
				Quaternion.identity);
			
		}
		if (bottom.objectReferenceValue != null) {
			Display.AddGameObject (bottom.objectReferenceValue as GameObject);
		}
	}

	private void AddPillars ()
	{
		GameObject pillarGO = pillar.objectReferenceValue as GameObject;
		Display.AddGameObject (pillarGO, new Vector3 (2, 2, 2), Quaternion.identity);
		Display.AddGameObject (pillarGO, new Vector3 (-2, 2, 2), Quaternion.identity);
		Display.AddGameObject (pillarGO, new Vector3 (-2, 2, -2), Quaternion.identity);
		Display.AddGameObject (pillarGO, new Vector3 (2, 2, -2), Quaternion.identity);
	}

	private void SetDayMode (RM_ButtonEvents e)
	{
		if (e != RM_ButtonEvents.LMB)
			return;
		Display.Camera.clearFlags = CameraClearFlags.Color;
		Display.Camera.backgroundColor = new Color (.5f, .9f, .6f);
		Display.Lights [0].transform.rotation = Quaternion.Euler (45, -10, 0);
		Display.Lights [0].intensity = 1.2f;
		Display.Lights [0].color = Color.white;
		Display.Lights [1].enabled = true;
		Display.Lights [1].transform.rotation = Quaternion.Euler (50, 20, 0);
		Display.Lights [1].intensity = 0.35f;
		Display.Lights [1].color = Color.white;
	}

	private void SetNightMode (RM_ButtonEvents e)
	{
		if (e != RM_ButtonEvents.LMB)
			return;
		Display.Camera.clearFlags = CameraClearFlags.Color;
		Display.Camera.backgroundColor = new Color (.5f, .35f, .35f);
		Display.Lights [0].transform.rotation = Quaternion.Euler (20, -10, 0);
		Display.Lights [0].intensity = 1f;
		Display.Lights [1].color = new Color (.8f, .25f, .25f);
		Display.Lights [1].enabled = true;
		Display.Lights [1].transform.rotation = Quaternion.Euler (50, 50, 0);
		Display.Lights [1].intensity = 0.35f;
		Display.Lights [1].color = new Color (.8f, .6f, .15f);
	}
}