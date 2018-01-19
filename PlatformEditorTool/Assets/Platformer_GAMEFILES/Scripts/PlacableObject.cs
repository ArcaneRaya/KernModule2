using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu (fileName = "PlacableObject", menuName = "Placable Object", order = 1)]
public class PlacableObject : ScriptableObject
{
	public bool IsValid {
		get {
			if (prefab == null || eventPositions_keys.Count != eventPositions_values.Count) {
				return false;
			}
			return true;
		}
	}

	public GameObject prefab;
	public Vector3 spawnOffset;
	public Vector3 rotation;
	public List<Vector2Int> positions;
	public List<Vector2Int> eventPositions_keys;
	public List<string> eventPositions_values;
	public int version = 0;
}