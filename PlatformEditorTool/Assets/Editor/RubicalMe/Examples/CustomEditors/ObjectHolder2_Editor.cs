using UnityEngine;
using UnityEditor;
using RubicalMe;

[CustomEditor (typeof(ScriptableObjectHolder2))]
public class ObjectHolder2_Editor : Editor_EasyInteractiveDisplay
{
	SerializedProperty prefab;
	SerializedProperty rotation;

	private uint buttonBackID;
	private uint buttonForwardID;
	private uint buttonSquareID;

	public override void OnEnable ()
	{
		base.OnEnable ();
		prefab = serializedObject.FindProperty ("prefab");
		rotation = serializedObject.FindProperty ("rotation");

		if (prefab.objectReferenceValue != null) {
			SetObjectInDisplay ();
		}

		Texture2D backButtonTex = (Texture2D)EditorGUIUtility.Load ("RubicalMe/EditorRendererExample/back.png");
		Texture2D forwardButtonTex = (Texture2D)EditorGUIUtility.Load ("RubicalMe/EditorRendererExample/forward.png");

		// provide a method as a normal delegate
		Display.GUISystem.AddButton (RotateRight, forwardButtonTex, new Vector2 (0, 0), GUISnapMode.BottomRight);

		// use a lambda to add custom parameters
		Display.GUISystem.AddButton ((RM_ButtonEvents e) => RotateLeft (e, 90), backButtonTex, new Vector2 (0, 0), GUISnapMode.BottomLeft);
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();

		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField (prefab);
		if (EditorGUI.EndChangeCheck ()) {
			rotation.vector3Value = Vector3.zero;
			SetObjectInDisplay ();
		}

		serializedObject.ApplyModifiedProperties ();
	}

	private void SetObjectInDisplay ()
	{
		Display.ClearRenderQueue ();
		Display.AddGameObject (prefab.objectReferenceValue as GameObject, Vector3.zero, rotation.vector3Value);
	}

	private void RotateLeft (RM_ButtonEvents e, int value)
	{
		serializedObject.Update ();
		rotation.vector3Value -= new Vector3 (0, value, 0);
		serializedObject.ApplyModifiedProperties ();
		SetObjectInDisplay ();
	}

	private void RotateRight (RM_ButtonEvents e)
	{
		serializedObject.Update ();
		rotation.vector3Value += new Vector3 (0, 45, 0);
		serializedObject.ApplyModifiedProperties ();
		SetObjectInDisplay ();
	}
}