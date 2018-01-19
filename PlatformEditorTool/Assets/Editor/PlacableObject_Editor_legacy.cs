/*

#define DEBUGMODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RubicalMe;

// Learned from https://gist.github.com/prodigga/53ab658e1a818cd4ddfd
//
//[CustomEditor (typeof(PlacableObject))]
public class PlacableObject_Editor_legacy : Editor_EasyInteractiveDisplay
{
	private PlacableObject obj;
	private Vector2 selectedpos = new Vector2 (100, 100);
	private List<Vector2> availablePositions;
	private int moveToWithoutTarget = -1;

	private SerializedProperty prefab;
	private SerializedProperty spawnOffset;
	private SerializedProperty rotation;
	private SerializedProperty objectVersion;

	private int version = 1;

	#if DEBUGMODE
	private SerializedProperty positions;
	private SerializedProperty eventPositions_keys;
	private SerializedProperty eventPositions_values;
	#endif

	public override bool HasPreviewGUI ()
	{
		return true;
	}

	void OnEnable ()
	{
		objectVersion = serializedObject.FindProperty ("version");
		if (objectVersion.intValue != version) {
			ConvertVersion ();
		}
		prefab = serializedObject.FindProperty ("prefab");
		spawnOffset = serializedObject.FindProperty ("spawnOffset");
		rotation = serializedObject.FindProperty ("rotation");
		#if DEBUGMODE
		positions = serializedObject.FindProperty ("positions");
		eventPositions_keys = serializedObject.FindProperty ("eventPositions_keys");
		eventPositions_values = serializedObject.FindProperty ("eventPositions_values");
		#endif
	}

	public override void OnInspectorGUI ()
	{
		if (objectVersion.intValue != version) {
			return;
		}
		EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		EditorGUILayout.PropertyField (prefab);
		EditorGUILayout.PropertyField (spawnOffset);
		EditorGUILayout.PropertyField (rotation);
		#if DEBUGMODE
		EditorGUILayout.PropertyField (positions, true);
		EditorGUILayout.PropertyField (eventPositions_keys, true);
		EditorGUILayout.PropertyField (eventPositions_values, true);
		#endif
		serializedObject.ApplyModifiedProperties ();
		if (EditorGUI.EndChangeCheck ()) {
			ResetView ();
		}
	}

	public override void OnPreviewGUI (Rect r, GUIStyle background)
	{
		if (objectVersion.intValue != version) {
			return;
		}
		if (Event.current.type == EventType.Repaint) {
			if (eRenderer == null) {
				obj = target as PlacableObject;
				InitializeRenderer (r);
				PlaceObjects ();
				eRenderer.OnMouseDown += MouseClick;
				eRenderer.OnMouseMove += MouseMove;
			}
			if (eRenderer.Rect != r) {
				eRenderer.Rect = r;
			}
			eRenderer.Display ();
		}
		if (eRenderer != null) {
			eRenderer.ProcessEvents (Event.current);
		}
	}

	private void InitializeRenderer (Rect r)
	{
		eRenderer = new InteractiveEditorRenderer (r, GUI.skin.box);
		eRenderer.Camera.transform.position = new Vector3 (4.5f, 1.5f, -10);
		eRenderer.Camera.clearFlags = CameraClearFlags.Color;
		eRenderer.Camera.backgroundColor = new Color (0.9f, 0.9f, 0.9f);
		eRenderer.Camera.farClipPlane = 1000;
	}

	private void PlaceObjects ()
	{
		if (obj.prefab == null) {
			return;
		}
		eRenderer.Add (obj.prefab, Vector3.zero + spawnOffset.vector3Value, obj.rotation);
		availablePositions = new List<Vector2> ();
		string basePath = "Assets/NO_TOUCHY_PLEASE_ObjectSetup/";
		GameObject VectorBlock = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlock.prefab", typeof(GameObject)) as GameObject;		
		GameObject VectorBlockConnectPoint = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockConnectPoint.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockDeadly = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockDeadly.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockMoveTo = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockMoveTo.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockTeleport = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockMoveToTarget.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockOccupied = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockOccupied.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockSelected = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockSelected.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockStartpoint = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockStartpoint.prefab", typeof(GameObject)) as GameObject;
		GameObject VectorBlockReverseDirection = AssetDatabase.LoadAssetAtPath (basePath + "ObjectSetup_VectorBlockReverseDirection.prefab", typeof(GameObject)) as GameObject;
		Vector3 offset = new Vector3 (0.5f, 0.5f, 0);
		eRenderer.Add (moveToWithoutTarget == -1 ? VectorBlockSelected : VectorBlockMoveTo, new Vector3 (selectedpos.x, selectedpos.y, -3) + offset, Quaternion.identity);
		eRenderer.Add (VectorBlockStartpoint, Vector3.zero + offset + Vector3.up / 2 + Vector3.back * 5, Quaternion.identity);
		for (int x = -1; x < 9; x++) {
			for (int y = -3; y < 6; y++) {
				Vector3 pos = new Vector3 (x, y, -2);
				availablePositions.Add (pos);
				GameObject vb = null;
				if (obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
					int index = obj.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (pos));
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyDeadly) {
						vb = VectorBlockDeadly;
					}
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveTo) {
						vb = VectorBlockMoveTo;
					}
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveToTarget) {
						vb = VectorBlockMoveTo;
					}
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyReverseDirection) {
						vb = VectorBlockReverseDirection;
					}
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyTeleport) {
						vb = VectorBlockTeleport;
					}
					if (obj.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyConnectPoint) {
						vb = VectorBlockConnectPoint;
					}
				}
				if (vb == null) {
					if (obj.positions.Contains (Vector2Int.RoundToInt (pos))) {
						vb = VectorBlockOccupied;
					} else {
						vb = VectorBlock;
					}
				}
				eRenderer.Add (vb, pos + offset, Quaternion.identity);
			}
		}
	}

	private void MouseMove (Event currentEvent)
	{
		Vector2 floored = new Vector2 (Mathf.Floor (eRenderer.mousePositionWorld.x), Mathf.Floor (eRenderer.mousePositionWorld.y));
//		Debug.Log (floored);
		if (floored == selectedpos) {
			return;
		}
		if (!availablePositions.Contains (floored)) {
			return;
		}
		selectedpos = floored;
		ResetView ();
	}

	private void MouseClick (Event currentEvent)
	{
		Vector2 floored = new Vector2 (Mathf.Floor (eRenderer.mousePositionWorld.x), Mathf.Floor (eRenderer.mousePositionWorld.y));
//		Debug.Log (floored);
		if (!availablePositions.Contains (floored)) {
			return;
		}

		if (moveToWithoutTarget != -1) {
			SetMoveToTarget (floored);
			return;
		}

		switch (currentEvent.button) {
		case 0:
			if (obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (floored))) {
				break;
			}
			SetOccupied (floored);
			break;
		case 1:
			ProcessContextMenu (currentEvent.mousePosition, floored);
			break;
		default:
			break;
		}
	}

	private void ProcessContextMenu (Vector2 mousePosition, Vector2 floored)
	{
		if (obj.positions.Contains (Vector2Int.RoundToInt (floored)) || obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (floored))) {
			ClearPoint (floored);
			return;
		}
		GenericMenu genericMenu = new GenericMenu ();
		if (!obj.positions.Contains (Vector2Int.RoundToInt (floored)) && !obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (floored))) {
			genericMenu.AddItem (new GUIContent ("Set Occupied"), false, () => SetOccupied (floored));
		}
		if (!obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (floored))) {
			genericMenu.AddItem (new GUIContent ("Set Deadly"), false, () => SetDeadly (floored));
			genericMenu.AddItem (new GUIContent ("Set MoveTo"), false, () => SetMoveTo (floored));
			genericMenu.AddItem (new GUIContent ("Set Reverse Direction"), false, () => SetReverseDirection (floored));
			genericMenu.AddItem (new GUIContent ("Set Connectpoint"), false, () => SetConnectPoint (floored));
		}
		if (obj.eventPositions_keys.Contains (Vector2Int.RoundToInt (floored)) || obj.positions.Contains (Vector2Int.RoundToInt (floored))) {
			genericMenu.AddItem (new GUIContent ("Clear Point"), false, () => ClearPoint (floored));
		}
		genericMenu.ShowAsContext ();
	}

	private void ClearPoint (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
			temp.positions.Remove (Vector2Int.RoundToInt (pos));
		}
		if (temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			int index = temp.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (pos));
			if (temp.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveTo) {
				string tPosString = temp.eventPositions_values [index].Substring (0, 4);
				if (tPosString != "XXXX") {
					Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
					Vector2 tPos = pos + difference;
					int tIndex = temp.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
					temp.eventPositions_keys.RemoveAt (tIndex);
					temp.eventPositions_values.RemoveAt (tIndex);
				}
				index = temp.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (pos));
				temp.eventPositions_keys.RemoveAt (index);
				temp.eventPositions_values.RemoveAt (index);
			} else if (temp.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveToTarget) {
				string tPosString = temp.eventPositions_values [index].Substring (0, 4);
				if (tPosString != "XXXX") {
					Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
					Vector2 tPos = pos + difference;
					int tIndex = temp.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
					temp.eventPositions_keys.RemoveAt (tIndex);
					temp.eventPositions_values.RemoveAt (tIndex);
				}
				index = temp.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (pos));
				temp.eventPositions_keys.RemoveAt (index);
				temp.eventPositions_values.RemoveAt (index);
			} else if (temp.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyDeadly) {
				temp.eventPositions_keys.RemoveAt (index);
				temp.eventPositions_values.RemoveAt (index);
			} else if (temp.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyReverseDirection) {
				temp.eventPositions_keys.RemoveAt (index);
				temp.eventPositions_values.RemoveAt (index);
			} else if (temp.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyConnectPoint) {
				temp.eventPositions_keys.RemoveAt (index);
				temp.eventPositions_values.RemoveAt (index);
			}
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetReverseDirection (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (!temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
				temp.positions.Remove (Vector2Int.RoundToInt (pos));
			}
			temp.eventPositions_keys.Add (Vector2Int.RoundToInt (pos));
			temp.eventPositions_values.Add ("0000" + GridVectorEventCubed.KeyReverseDirection);
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetConnectPoint (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (!temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
				temp.positions.Remove (Vector2Int.RoundToInt (pos));
			}
			temp.eventPositions_keys.Add (Vector2Int.RoundToInt (pos));
			temp.eventPositions_values.Add ("0000" + GridVectorEventCubed.KeyConnectPoint);
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetMoveTo (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (!temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
				temp.positions.Remove (Vector2Int.RoundToInt (pos));
			}
			temp.eventPositions_keys.Add (Vector2Int.RoundToInt (pos));
			temp.eventPositions_values.Add ("XXXX" + GridVectorEventCubed.KeyMoveTo);
			moveToWithoutTarget = temp.eventPositions_keys.Count - 1;
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetMoveToTarget (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (temp.eventPositions_values [moveToWithoutTarget] != "XXXX" + GridVectorEventCubed.KeyMoveTo) {
			moveToWithoutTarget = -1;
			return;
		}
		if (temp.eventPositions_keys [moveToWithoutTarget] == pos) {
			Debug.LogWarning ("Target can not be set to startpoint");
			return;
		}
		if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
			temp.positions.Remove (Vector2Int.RoundToInt (pos));
		}
		Vector2 difference = pos - temp.eventPositions_keys [moveToWithoutTarget];
		temp.eventPositions_values [moveToWithoutTarget] = difference.x.ToString ().PadLeft (2, '0') + difference.y.ToString ().PadLeft (2, '0') + GridVectorEventCubed.KeyMoveTo;
		difference *= -1;
		temp.eventPositions_keys.Add (Vector2Int.RoundToInt (pos));
		temp.eventPositions_values.Add (difference.x.ToString ().PadLeft (2, '0') + difference.y.ToString ().PadLeft (2, '0') + GridVectorEventCubed.KeyMoveToTarget);
		moveToWithoutTarget = -1;
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetDeadly (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (!temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			if (temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
				temp.positions.Remove (Vector2Int.RoundToInt (pos));
			}
			temp.eventPositions_keys.Add (Vector2Int.RoundToInt (pos));
			temp.eventPositions_values.Add ("0000" + GridVectorEventCubed.KeyDeadly);
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void SetOccupied (Vector2 pos)
	{
		serializedObject.Update ();
		PlacableObject temp = target as PlacableObject;
		if (temp.eventPositions_keys.Contains (Vector2Int.RoundToInt (pos))) {
			ClearPoint (pos);
		}
		if (!temp.positions.Contains (Vector2Int.RoundToInt (pos))) {
			temp.positions.Add (Vector2Int.RoundToInt (pos));
		} else {
			temp.positions.Remove (Vector2Int.RoundToInt (pos));
		}
		serializedObject.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
		serializedObject.ApplyModifiedProperties ();
		ResetView ();
	}

	private void ResetView ()
	{
		eRenderer.Clear ();
		PlaceObjects ();
		Repaint ();
	}

	private void OnDisable ()
	{
		if (objectVersion.intValue != version) {
			return;
		}
		if (obj != null) {
			try {
				int count = obj.eventPositions_keys.Count;
			} catch (System.NullReferenceException ex) {
				Debug.Log ("Everything is probably okay, but I caught this error: \n" + ex);
				return;
			}
		}
		if (obj.eventPositions_keys.Count > 0) {
			for (int i = 0; i < obj.eventPositions_values.Count; i++) {
				if (obj.eventPositions_values [i] == "XXXX" + GridVectorEventCubed.KeyMoveTo) {
					ClearPoint (obj.eventPositions_keys [i]);
				}
			}
		}
	}

	private void OnDestroy ()
	{
		if (eRenderer != null) {
			eRenderer.Cleanup ();
			eRenderer.OnMouseDown -= MouseClick;
			eRenderer.OnMouseMove -= MouseMove;
		}
	}

	private void ConvertVersion ()
	{
		if (objectVersion.intValue == 0) {
			Debug.LogWarning ("The selected object " + target.name + " was the wrong version, but a conversion was possible.");
			serializedObject.Update ();
			objectVersion.intValue = 1;
			serializedObject.ApplyModifiedProperties ();
		} else {
			Debug.LogWarning ("Not loading PlacableObject " + target.name + " because it's the wrong version. No conversion available. \n Current version = "
			+ version.ToString () + " | Selected object version = " + objectVersion.intValue.ToString ());
			return;
		}
	}

}

*/