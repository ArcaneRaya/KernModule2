using UnityEngine;
using UnityEditor;
using RubicalMe;

public class Example_EasyInteractiveDisplay : EditorWindow_EasyInteractiveDisplay
{
	private GameObject gameObject;
	private Vector3 rotationEuler;

	[MenuItem ("Tools/RubicalMe/Examples/EasyInteractiveDisplay")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		Example_EasyInteractiveDisplay window = (Example_EasyInteractiveDisplay)EditorWindow.GetWindow (typeof(Example_EasyInteractiveDisplay));
		window.Show ();
	}

	public override void OnGUI ()
	{
		base.OnGUI ();
		EditorGUI.BeginChangeCheck ();
		gameObject = (GameObject)EditorGUILayout.ObjectField (gameObject, typeof(GameObject), true);
		if (EditorGUI.EndChangeCheck ()) {
			rotationEuler = Vector3.zero;
			SetObjectInDisplay ();
		}
	}

	public override void OnEnable ()
	{
		base.OnEnable ();
		Texture2D backButtonTex = (Texture2D)EditorGUIUtility.Load ("RubicalMe/EditorRendererExample/back.png");
		Texture2D forwardButtonTex = (Texture2D)EditorGUIUtility.Load ("RubicalMe/EditorRendererExample/forward.png");

		// provide a method as a normal delegate
		Display.GUISystem.AddButton (RotateRight, forwardButtonTex, Vector2.zero, GUISnapMode.BottomRight);

		// use a lambda to add custom parameters
		Display.GUISystem.AddButton ((RM_ButtonEvents e) => RotateLeft (e, 90), backButtonTex, Vector2.zero, GUISnapMode.BottomLeft);
	}

	private void SetObjectInDisplay ()
	{
		Display.ClearRenderQueue ();
		Display.AddGameObject (gameObject, Vector3.zero, rotationEuler);
	}

	private void RotateLeft (RM_ButtonEvents e, int value)
	{
		rotationEuler -= new Vector3 (0, value, 0);
		SetObjectInDisplay ();
		Repaint ();
	}

	private void RotateRight (RM_ButtonEvents e)
	{
		rotationEuler += new Vector3 (0, 45, 0);
		SetObjectInDisplay ();
		Repaint ();
	}
}