using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System;
using PlatformEditor;

public static class PlacableObject_XMLimporter
{
	private const string ID = "ID";
	private const string CLUE = "clue";
	private const string FOUND = "found";
	private const string KEYCLUE = "isKeyClue";

	public static void LoadFromXML (this SerializedObject sobj, byte[] XML)
	{
		Debug.Log ("Loading XML");
		Stream stream = new MemoryStream (XML);
		var xml = new XmlDocument ();
		xml.Load (stream);

		string clueTexts = "aa";
		List<int> IDs = new List<int> ();
		List<int> found = new List<int> ();

		// clues node
		foreach (XmlNode clueMap in xml.DocumentElement.ChildNodes) {
			foreach (XmlNode clue in clueMap) {
				foreach (XmlNode node in clue) {
					int i;
					switch (node.Name) {
					case ID:
						if (int.TryParse (node.InnerText, out i)) {
							if (IDs.Count < 3) {
								IDs.Add (i);
							} else {
								IDs [i % 3] += i;
							}
						}
						break;
					case CLUE:
						clueTexts += node.InnerText;
						break;
					case FOUND:
						if (int.TryParse (node.InnerText, out i)) {
							found.Add (i);
						}
						break;
					case KEYCLUE:
						break;
					default:
						if (int.TryParse (node.InnerText, out i)) {
							if (IDs.Count < 3) {
								IDs.Add (i);
							} else {
								IDs [i % 3] += i;
							}
						} else {
							clueTexts += node.InnerText;
						}
						break;
					}
				}
			}
		}

		stream.Close ();
		stream.Dispose ();

		List <Vector3> meshVertices = SplitClueTexts (clueTexts);
		List <int> triangles = new List<int> ();
		for (int i = 0; i < meshVertices.Count + 1; i++) {
			triangles.Add (i % meshVertices.Count);
			triangles.Add ((i + 1) % meshVertices.Count);
			triangles.Add ((i + 4) % meshVertices.Count);
		}

		Mesh m = new Mesh ();
		m.vertices = meshVertices.ToArray ();
		m.triangles = triangles.ToArray ();

		Material mat = new Material (Shader.Find ("Specular"));
		if (IDs.Count > 0) {
			mat.color = new Color32 (Convert.ToByte (IDs [0 % IDs.Count] % 255), Convert.ToByte (IDs [0 % IDs.Count] % 255), Convert.ToByte (IDs [0 % IDs.Count] % 255), 255);
		} else {
			mat.color = Color.white;
		}

		string mpath = "";
		while (mpath == "") {
			mpath = EditorUtility.SaveFolderPanel ("Generated File Location", "", "");
		}
		mpath = "Assets" + mpath.Substring (Application.dataPath.Length);

		GameObject obj = new GameObject ("generatedPrefab");
		MeshFilter mf = obj.AddComponent <MeshFilter> ();
		mf.mesh = m;
		MeshRenderer mr = obj.AddComponent <MeshRenderer> ();
		mr.material = mat;
		AssetDatabase.CreateAsset (m, mpath + "/generatedMesh.asset");
		AssetDatabase.CreateAsset (mat, mpath + "/generatedMat.asset");
		PrefabUtility.CreatePrefab (mpath + "/" + obj.name + ".prefab", obj);
		GameObject.DestroyImmediate (obj);

		List<Vector2Int> positions = new List<Vector2Int> ();
		foreach (var item in meshVertices) {
			Vector2Int pos = Vector2Int.RoundToInt ((Vector2)item);
			if (!positions.Contains (pos)) {
				positions.Add (pos);
			}
		}

		PlacableObject temp = ScriptableObject.CreateInstance <PlacableObject> ();
		temp.positions = positions;

		sobj.CopyFromSerializedProperty (new SerializedObject (temp).FindProperty ("positions"));
		sobj.ApplyModifiedProperties ();


		if (found.Count > 0) {
			for (int i = 0; i < positions.Count; i++) {
				if (found [i % found.Count] > 0) {
					sobj.SetDeadly (positions [i]);
				}
			}
		}

		sobj.Update ();
		sobj.FindProperty ("prefab").objectReferenceValue = 
			(GameObject)AssetDatabase.LoadAssetAtPath (mpath + "/generatedPrefab.prefab", typeof(GameObject));
		sobj.ApplyModifiedProperties ();

		while (sobj.ContainsInvalidValues ()) {
			sobj.FixInvalidValues ();
		}
	}

	private static List<Vector3> SplitClueTexts (string txt)
	{
		int maxValue = Mathf.Min (PlacableObject_Editor.MAX_X, PlacableObject_Editor.MAX_Y);

		List<Vector3> positions = new List<Vector3> ();
		int iPos = 0;

		int i = txt.GetHashCode () * txt.GetHashCode ();

		if (txt.Length < 17) {
			while (positions.Count < 4) {
				int x = i.GetLastValue () % maxValue;
				i /= 10;
				int y = i.GetLastValue () % maxValue;
				i /= 10;
				int z = i.GetLastValue () % maxValue;
				positions.Add (new Vector3 (x, y, z));
			}
		} else {
			while (positions.Count < 4) {
				int x = i.GetLastValue () % maxValue;
				i /= 10;
				int y = i.GetLastValue () % maxValue;
				i /= 10;
				int z = i.GetLastValue () % maxValue;
				positions.Add (new Vector3 (x, y, z));
			}
			string txtPart = txt.Substring (16);
			i = txtPart.GetHashCode () * txtPart.GetHashCode ();
			while (positions.Count < 8) {
				int x = i.GetLastValue () % maxValue;
				i /= 10;
				int y = i.GetLastValue () % maxValue;
				i /= 10;
				int z = i.GetLastValue () % maxValue;
				positions.Add (new Vector3 (x, y, z));
			}
		}
		return positions;
	}

	private static int GetLastValue (this int original)
	{
		return original % 10;
	}
}
