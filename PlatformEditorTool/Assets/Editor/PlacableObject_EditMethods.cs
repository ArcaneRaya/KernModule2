using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace PlatformEditor
{
	public static class PlacableObject_EditMethods
	{
		public static bool Validate (this SerializedObject sobj, int version)
		{
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			int objVersion = sobj.FindProperty ("version").intValue;
			if (objVersion == version) {
				return sobj.ValidateValues ();
			} else if (objVersion == 0) {
				Debug.LogWarning ("The selected object " + target.name + " was the wrong version, but a conversion was possible.");
				sobj.Update ();
				sobj.FindProperty ("version").intValue = 1;
				sobj.ApplyModifiedProperties ();
				return sobj.ValidateValues ();
			} else {
				Debug.LogWarning ("Not loading PlacableObject " + target.name + " because it's the wrong version. No conversion available. \n Current version = "
				+ version.ToString () + " | Selected object version = " + sobj.FindProperty ("version").intValue.ToString ());
				return false;
			}
		}

		private static bool ValidateValues (this SerializedObject sobj)
		{
			PlacableObject target = (PlacableObject)sobj.targetObject;
			bool invalidPositions = sobj.ContainsInvalidValues ();
			if (invalidPositions) {
				if (EditorUtility.DisplayDialog ("Remove incorrect position values?", 
				                                 "There are some incorrect position values in " + target.name + ". Do you want the editor to remove them or stop validation?", 
				                                 "Remove Values", 
				                                 "Stop Validation")) {
					while (sobj.ContainsInvalidValues ()) {
						sobj.FixInvalidValues ();
					}
					return true;
				} else {
					return false;
				}
			} else {
				return true;
			}
		}

		public static void FixInvalidValues (this SerializedObject sobj)
		{
			sobj.Update ();
			PlacableObject target = (PlacableObject)sobj.targetObject;
			PlacableObject temp = ScriptableObject.CreateInstance <PlacableObject> ();
			temp.positions = new System.Collections.Generic.List<Vector2Int> ();
			temp.eventPositions_keys = new System.Collections.Generic.List<Vector2Int> ();
			temp.eventPositions_values = new System.Collections.Generic.List<string> ();
			foreach (var pos in target.positions) {
				if (pos.x < PlacableObject_Editor.MIN_X || pos.x > PlacableObject_Editor.MAX_X || pos.y < PlacableObject_Editor.MIN_Y || pos.y > PlacableObject_Editor.MAX_Y) {
					continue;
				}
				if (!temp.positions.Contains (pos) && !temp.eventPositions_keys.Contains (pos)) {
					temp.positions.Add (pos);
				}
			}
			int maxI = Mathf.Min (target.eventPositions_keys.Count, target.eventPositions_values.Count);
			for (int i = 0; i < maxI; i++) {
				Vector2 pos = target.eventPositions_keys [i];
				if (pos.x < PlacableObject_Editor.MIN_X || pos.x > PlacableObject_Editor.MAX_X || pos.y < PlacableObject_Editor.MIN_Y || pos.y > PlacableObject_Editor.MAX_Y) {
					continue;
				}
				if (target.eventPositions_values [i].Length != 5) {
					continue;
				}
				string defChar = target.eventPositions_values [i].Substring (4, 1);
				if (defChar != GridVectorEventCubed.KeyDeadly && defChar != GridVectorEventCubed.KeyMoveTo && defChar != GridVectorEventCubed.KeyMoveToTarget) {
					continue;
				}
				if (defChar == GridVectorEventCubed.KeyMoveTo) {
					string tPosString = target.eventPositions_values [i].Substring (0, 4);
					if (tPosString == "XXXX") {
						continue;
					}
					Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
					Vector2 tPos = target.eventPositions_keys [i] + difference;
					int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
					if (tIndex == -1) {
						continue;
					}
					if (target.eventPositions_values [tIndex].Length != 5) {
						continue;
					}
					string rPosString = target.eventPositions_values [tIndex].Substring (0, 4);
					if (rPosString == "XXXX") {
						continue;
					}
					Vector2 rDifference = new Vector2 (int.Parse (rPosString.Substring (0, 2)), int.Parse (rPosString.Substring (2, 2)));
					if (difference != rDifference * -1) {
						continue;
					}
				} else if (defChar == GridVectorEventCubed.KeyMoveToTarget) {
					string tPosString = target.eventPositions_values [i].Substring (0, 4);
					if (tPosString == "XXXX") {
						continue;
					}
					Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
					Vector2 tPos = target.eventPositions_keys [i] + difference;
					int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
					if (tIndex == -1) {
						continue;
					}
					if (target.eventPositions_values [tIndex].Length != 5) {
						continue;
					}
					string rPosString = target.eventPositions_values [tIndex].Substring (0, 4);
					if (rPosString == "XXXX") {
						continue;
					}
					Vector2 rDifference = new Vector2 (int.Parse (rPosString.Substring (0, 2)), int.Parse (rPosString.Substring (2, 2)));
					if (difference != rDifference * -1) {
						continue;
					}
				}
				if (!temp.eventPositions_keys.Contains (target.eventPositions_keys [i]) && !temp.positions.Contains (target.eventPositions_keys [i])) {
					temp.eventPositions_keys.Add (target.eventPositions_keys [i]);
					temp.eventPositions_values.Add (target.eventPositions_values [i]);
				}
			}
			sobj.CopyFromSerializedProperty (new SerializedObject (temp).FindProperty ("positions"));
			sobj.CopyFromSerializedProperty (new SerializedObject (temp).FindProperty ("eventPositions_keys"));
			sobj.CopyFromSerializedProperty (new SerializedObject (temp).FindProperty ("eventPositions_values"));
			sobj.ApplyModifiedProperties ();
		}

		public static bool ContainsInvalidValues (this SerializedObject sobj)
		{
			PlacableObject target = (PlacableObject)sobj.targetObject;
			foreach (var pos in target.positions) {
				if (pos.x < PlacableObject_Editor.MIN_X || pos.x > PlacableObject_Editor.MAX_X || pos.y < PlacableObject_Editor.MIN_Y || pos.y > PlacableObject_Editor.MAX_Y) {
					return true;
				}
				if (target.positions.ContainsDuplicatesOfValue (pos)) {
					return true;
				}
			}
			if (target.eventPositions_keys.Count != target.eventPositions_values.Count) {
				return true;
			} else {
				for (int i = 0; i < target.eventPositions_keys.Count; i++) {
					Vector2Int pos = target.eventPositions_keys [i];
					if (target.eventPositions_keys.ContainsDuplicatesOfValue (pos)) {
						return true;
					}
					if (pos.x < PlacableObject_Editor.MIN_X || pos.x > PlacableObject_Editor.MAX_X || pos.y < PlacableObject_Editor.MIN_Y || pos.y > PlacableObject_Editor.MAX_Y) {
						return true;
					}
					if (target.eventPositions_values [i].Length != 5) {
						return true;
					}
					string defChar = target.eventPositions_values [i].Substring (4, 1);
					if (defChar != GridVectorEventCubed.KeyDeadly && defChar != GridVectorEventCubed.KeyMoveTo && defChar != GridVectorEventCubed.KeyMoveToTarget) {
						return true;
					}
					if (defChar == GridVectorEventCubed.KeyMoveTo) {
						string tPosString = target.eventPositions_values [i].Substring (0, 4);
						if (tPosString == "XXXX") {
							return true;
						}
						Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
						Vector2 tPos = target.eventPositions_keys [i] + difference;
						int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
						if (tIndex == -1) {
							return true;
						}
						if (target.eventPositions_values [tIndex].Length != 5) {
							return true;
						}
						string rPosString = target.eventPositions_values [tIndex].Substring (0, 4);
						if (rPosString == "XXXX") {
							return true;
						}
						Vector2 rDifference = new Vector2 (int.Parse (rPosString.Substring (0, 2)), int.Parse (rPosString.Substring (2, 2)));
						if (difference != rDifference * -1) {
							return true;
						}
					} else if (defChar == GridVectorEventCubed.KeyMoveToTarget) {
						string tPosString = target.eventPositions_values [i].Substring (0, 4);
						if (tPosString == "XXXX") {
							return true;
						}
						Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
						Vector2 tPos = target.eventPositions_keys [i] + difference;
						int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
						if (tIndex == -1) {
							return true;
						}
						if (target.eventPositions_values [tIndex].Length != 5) {
							return true;
						}
						string rPosString = target.eventPositions_values [tIndex].Substring (0, 4);
						if (rPosString == "XXXX") {
							return true;
						}
						Vector2 rDifference = new Vector2 (int.Parse (rPosString.Substring (0, 2)), int.Parse (rPosString.Substring (2, 2)));
						if (difference != rDifference * -1) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public static void ClearCell (this SerializedObject sobj, Vector2Int intPos)
		{
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			if (target.positions.Contains (intPos)) {
				target.positions.Remove (intPos);
			} else if (target.eventPositions_keys.Contains (intPos)) {
				int index = target.eventPositions_keys.IndexOf (intPos);
				if (target.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveTo) {
					string tPosString = target.eventPositions_values [index].Substring (0, 4);
					if (tPosString != "XXXX") {
						Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
						Vector2 tPos = intPos + difference;
						int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
						target.eventPositions_keys.RemoveAt (tIndex);
						target.eventPositions_values.RemoveAt (tIndex);
					}
					index = target.eventPositions_keys.IndexOf (intPos);
					target.eventPositions_keys.RemoveAt (index);
					target.eventPositions_values.RemoveAt (index);
				} else if (target.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyMoveToTarget) {
					string tPosString = target.eventPositions_values [index].Substring (0, 4);
					if (tPosString != "XXXX") {
						Vector2 difference = new Vector2 (int.Parse (tPosString.Substring (0, 2)), int.Parse (tPosString.Substring (2, 2)));
						Vector2 tPos = intPos + difference;
						int tIndex = target.eventPositions_keys.IndexOf (Vector2Int.RoundToInt (tPos));
						target.eventPositions_keys.RemoveAt (tIndex);
						target.eventPositions_values.RemoveAt (tIndex);
					}
					index = target.eventPositions_keys.IndexOf (intPos);
					target.eventPositions_keys.RemoveAt (index);
					target.eventPositions_values.RemoveAt (index);
				} else if (target.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyDeadly) {
					target.eventPositions_keys.RemoveAt (index);
					target.eventPositions_values.RemoveAt (index);
				} else if (target.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyReverseDirection) {
					target.eventPositions_keys.RemoveAt (index);
					target.eventPositions_values.RemoveAt (index);
				} else if (target.eventPositions_values [index].Substring (4, 1) == GridVectorEventCubed.KeyConnectPoint) {
					target.eventPositions_keys.RemoveAt (index);
					target.eventPositions_values.RemoveAt (index);
				}
			} else {
				return;
			}
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
			sobj.ApplyModifiedProperties ();
		}

		public static void SetOccupied (this SerializedObject sobj, Vector2Int intPos)
		{
			sobj.ClearCell (intPos);
			sobj.Update ();
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			target.positions.Add (Vector2Int.RoundToInt (intPos));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("positions"));
			sobj.ApplyModifiedProperties ();
		}

		public static void SetDeadly (this SerializedObject sobj, Vector2Int intPos)
		{
			sobj.ClearCell (intPos);
			sobj.Update ();
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			target.eventPositions_keys.Add (Vector2Int.RoundToInt (intPos));
			target.eventPositions_values.Add ("0000" + GridVectorEventCubed.KeyDeadly);
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
			sobj.ApplyModifiedProperties ();
		}

		public static int SetMoveTo (this SerializedObject sobj, Vector2Int intPos)
		{
			sobj.ClearCell (intPos);
			sobj.Update ();
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			target.eventPositions_keys.Add (Vector2Int.RoundToInt (intPos));
			target.eventPositions_values.Add ("XXXX" + GridVectorEventCubed.KeyMoveTo);
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
			sobj.ApplyModifiedProperties ();
			return target.eventPositions_keys.Count - 1;
		}

		public static int SetMoveToTarget (this SerializedObject sobj, Vector2Int intPos, int moveToWithoutTarget)
		{
			sobj.Update ();
			PlacableObject target;
			try {
				target = (PlacableObject)sobj.targetObject;
			} catch {
				throw new ArgumentException ("Casting SerializedObject as PlacableObject failed. Note that this method is a custom helper-method for the PlacableObject_Editor.");
			}
			if (target.eventPositions_values [moveToWithoutTarget] != "XXXX" + GridVectorEventCubed.KeyMoveTo) {
				return -1;
			}
			if (target.eventPositions_keys [moveToWithoutTarget] == intPos) {
				Debug.LogWarning ("Target can not be set to startpoint");
				return moveToWithoutTarget;
			}
			sobj.ClearCell (intPos);
			Vector2Int difference = intPos - target.eventPositions_keys [moveToWithoutTarget];
			target.eventPositions_values [moveToWithoutTarget] = 
				difference.x.ToString ().PadLeft (2, '0') +
			difference.y.ToString ().PadLeft (2, '0') +
			GridVectorEventCubed.KeyMoveTo;
			difference *= -1;
			target.eventPositions_keys.Add (intPos);
			target.eventPositions_values.Add (
				difference.x.ToString ().PadLeft (2, '0') +
				difference.y.ToString ().PadLeft (2, '0') +
				GridVectorEventCubed.KeyMoveToTarget
			);
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_keys"));
			sobj.CopyFromSerializedProperty (new SerializedObject (target).FindProperty ("eventPositions_values"));
			sobj.ApplyModifiedProperties ();
			return -1;
		}

		private static bool ContainsDuplicatesOfValue (this List<Vector2Int> list, Vector2Int value)
		{
			int count = 0;
			foreach (var item in list) {
				if (item == value) {
					count++;
					if (count > 1) {
						return true;
					}
				}
			}
			return false;
		}
	}
}