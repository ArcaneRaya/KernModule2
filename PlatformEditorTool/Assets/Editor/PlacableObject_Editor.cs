#define DEBUGMODE

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RubicalMe;
using PlatformEditor;


[CustomEditor (typeof(PlacableObject))]
public class PlacableObject_Editor : Editor_EasyInteractiveDisplay
{
	public const int MIN_X = -1;
	public const int MAX_X = 8;
	public const int MIN_Y = -3;
	public const int MAX_Y = 5;

	private const int VERSION = 1;
	private const int GUIOFFSET_X = 4;
	private const int GUIOFFSET_Y = 1;
	private const string TEXTURESPATH = "Rubical/PlatformEditor/Textures/";
	private const int BUTTONSIZE = 100;
	private const int PADDING = 20;
	private const float BUTTONOFFSET_X = -.5f;
	private const float BUTTONOFFSET_Y = .5f;

	private readonly Vector2 outsideGridPosition = new Vector2 (-100, -100);

	private TextAsset XMLFile;

	private SerializedProperty prefab;
	private SerializedProperty spawnOffset;
	private SerializedProperty rotation;

	#if DEBUGMODE
	private SerializedProperty positions;
	private SerializedProperty eventPositions_keys;
	private SerializedProperty eventPositions_values;
	#endif

	private bool validated;
	private Vector2 selectedPos;
	private Texture2D buttonTex_neutral;
	private Texture2D buttonTex_selected;
	private Texture2D buttonTex_occupied;
	private Texture2D buttonTex_deadly;
	private Texture2D buttonTex_moveTo;
	private Texture2D startpointTex;

	private int moveToWithoutTarget = -1;

	public override void OnEnable ()
	{
		base.OnEnable ();
		if (!(validated = serializedObject.Validate (VERSION))) {
			return;
		}

		prefab = serializedObject.FindProperty ("prefab");
		spawnOffset = serializedObject.FindProperty ("spawnOffset");
		rotation = serializedObject.FindProperty ("rotation");

		#if DEBUGMODE
		positions = serializedObject.FindProperty ("positions");
		eventPositions_keys = serializedObject.FindProperty ("eventPositions_keys");
		eventPositions_values = serializedObject.FindProperty ("eventPositions_values");
		#endif

		buttonTex_neutral = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "whiteSquareAlpha.png");
		buttonTex_selected = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "graySquareAlpha.png");
		buttonTex_occupied = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "darkgraySquareAlpha.png");
		buttonTex_deadly = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "redSquareAlpha.png");
		buttonTex_moveTo = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "blueSquareAlpha.png");
		startpointTex = (Texture2D)EditorGUIUtility.Load (TEXTURESPATH + "greenSquareAlpha.png");

		selectedPos = outsideGridPosition;
		InitializeRenderer ();
		UpdatePrefabView ();
		UpdateGUIView ();
	}

	public override void OnInspectorGUI ()
	{
		if (!validated) {
			EditorGUILayout.LabelField ("COULD NOT VALIDATE OBJECT FOR INSPECTION");
			return;
		}

		if (GUILayout.Button ("Import XML")) {
			string path = EditorUtility.OpenFilePanel ("Choose XML file", "Assets", "xml");
			if (path.Length != 0) {
				var fileContent = File.ReadAllBytes (path);
				serializedObject.LoadFromXML (fileContent);
				UpdatePrefabView ();
				UpdateGUIView ();
			}
		}

		EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		EditorGUILayout.PropertyField (prefab);
		EditorGUILayout.PropertyField (spawnOffset);
		EditorGUILayout.PropertyField (rotation);
		serializedObject.ApplyModifiedProperties ();
		if (EditorGUI.EndChangeCheck ()) {
			UpdatePrefabView ();
		}

		EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		#if DEBUGMODE
		EditorGUILayout.PropertyField (positions, true);
		EditorGUILayout.PropertyField (eventPositions_keys, true);
		EditorGUILayout.PropertyField (eventPositions_values, true);
		serializedObject.ApplyModifiedProperties ();
		#endif
		if (EditorGUI.EndChangeCheck ()) {
			UpdateGUIView ();
		}
	}

	private void UpdatePrefabView ()
	{
		Display.ClearRenderQueue ();
		if (prefab.objectReferenceValue != null) {
			Display.AddGameObject (prefab.objectReferenceValue as GameObject, spawnOffset.vector3Value, rotation.vector3Value);
		}
	}

	private void UpdateGUIView ()
	{
		Display.GUISystemWorldScaled.Clear ();

		for (int x = MIN_X; x <= MAX_X; x++) {
			for (int y = MIN_Y; y <= MAX_Y; y++) {
				Vector2Int pos = new Vector2Int (x, y);
				Texture2D tex = buttonTex_neutral;
				PlacableObject obj = (PlacableObject)target;
				if (obj.positions.Contains (pos)) {
					tex = buttonTex_occupied;
				} else if (obj.eventPositions_keys.Contains (pos)) {
					int index = obj.eventPositions_keys.IndexOf (pos);
					string defChar = obj.eventPositions_values [index].Substring (4, 1);
					if (defChar == GridVectorEventCubed.KeyDeadly) {
						tex = buttonTex_deadly;
					} else if (defChar == GridVectorEventCubed.KeyMoveTo || defChar == GridVectorEventCubed.KeyMoveToTarget) {
						tex = buttonTex_moveTo;
					}
				}
				Display.GUISystemWorldScaled.AddButton (
					(RM_ButtonEvents e) => MouseEvent (e, pos),
					tex, 
					new Rect ((x - GUIOFFSET_X - BUTTONOFFSET_X) * BUTTONSIZE, (y - GUIOFFSET_Y - BUTTONOFFSET_Y) * -BUTTONSIZE, BUTTONSIZE - PADDING, BUTTONSIZE - PADDING), 
					GUISnapMode.MidCenter);
			}
		}

		Display.GUISystemWorldScaled.AddImage (
			startpointTex, 
			new Rect ((0 - GUIOFFSET_X) * BUTTONSIZE, (0 - GUIOFFSET_Y) * -BUTTONSIZE, PADDING, PADDING), 
			GUISnapMode.MidCenter);

		if (selectedPos != outsideGridPosition) {
			Display.GUISystemWorldScaled.AddImage (
				buttonTex_selected, 
				new Rect ((selectedPos.x - GUIOFFSET_X - BUTTONOFFSET_X) * BUTTONSIZE, (selectedPos.y - GUIOFFSET_Y - BUTTONOFFSET_Y) * -BUTTONSIZE, BUTTONSIZE - PADDING, BUTTONSIZE - PADDING), 
				GUISnapMode.MidCenter);
		}
	}

	private void MouseEvent (RM_ButtonEvents e, Vector2 pos)
	{
		switch (e) {
		case RM_ButtonEvents.Enter:
			if (pos == selectedPos) {
				break;
			}
			selectedPos = pos;
			UpdateGUIView ();
			break;
		case RM_ButtonEvents.Exit:
			if (selectedPos == pos) {
				selectedPos = outsideGridPosition;
				UpdateGUIView ();
			}
			break;
		case RM_ButtonEvents.LMB:
			LeftClick (pos);
			break;
		case RM_ButtonEvents.RMB:
			RightClick (pos);
			break;
		}
	}

	private void LeftClick (Vector2 pos)
	{
		Vector2Int intPos = Vector2Int.RoundToInt (pos);
		PlacableObject obj = (PlacableObject)target;
		if (moveToWithoutTarget != -1) {
			moveToWithoutTarget = serializedObject.SetMoveToTarget (Vector2Int.RoundToInt (pos), moveToWithoutTarget);
			return;
		}

		if (!obj.eventPositions_keys.Contains (intPos)) {
			if (obj.positions.Contains (intPos)) {
				serializedObject.ClearCell (intPos);
			} else {
				serializedObject.SetOccupied (intPos);
			}
		}
	}

	private void RightClick (Vector2 pos)
	{
		if (moveToWithoutTarget != -1) {
			return;
		}
		Vector2Int intPos = Vector2Int.RoundToInt (pos);
		PlacableObject obj = (PlacableObject)target;
		if (obj.positions.Contains (intPos) || obj.eventPositions_keys.Contains (intPos)) {
			serializedObject.ClearCell (intPos);
			return;
		} else {
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Set Occupied"), false, () => PlacableObject_EditMethods.SetOccupied (serializedObject, intPos));
			menu.AddItem (new GUIContent ("Set Deadly"), false, () => PlacableObject_EditMethods.SetDeadly (serializedObject, intPos));
			menu.AddItem (new GUIContent ("Set MoveTo"), false, () => moveToWithoutTarget = PlacableObject_EditMethods.SetMoveTo (serializedObject, intPos));
			menu.ShowAsContext ();
		}
	}

	private void InitializeRenderer ()
	{
		Display.Camera.transform.position = new Vector3 (GUIOFFSET_X, GUIOFFSET_Y, -10);
		Display.Camera.clearFlags = CameraClearFlags.Color;
		Display.Camera.backgroundColor = new Color (0.9f, 0.9f, 0.9f);
		Display.Camera.farClipPlane = 1000;
	}
}
