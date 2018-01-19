using UnityEngine;
using RubicalMe.RenderTools;
using System.Collections.Generic;
using System;

namespace RubicalMe
{
	namespace Renderers
	{
		public class GUISystem
		{

			public int GUIQueueSize {
				get {
					return GUIRenderQueue.Count;
				}
			}

			private bool scaleWithWorld;
			private List<GUIItem> GUIRenderQueue;

			public GUISystem ()
			{
				Clear ();
			}

			public GUISystem (bool state)
			{
				Clear ();
				scaleWithWorld = state;
			}

			[Obsolete ("the GUISystem will not call the EventHandler anymore, use AddButton (ButtonDelegate ...) instead")]
			/// <summary>
			/// Adds a button with assigned texture to the GUISystem.
			/// </summary>
			/// <returns>The unique ID of the button, relevant for listening to interaction.</returns>
			public uint AddButton (Texture2D texture)
			{
				GUIItem newItem = new Button (texture);
				GUIRenderQueue.Add (newItem);
				return newItem.ID;
			}

			[Obsolete ("the GUISystem will not call the EventHandler anymore, use AddButton (ButtonDelegate ...) instead")]
			/// <summary>
			/// Adds a button with assigned texture to the GUISystem.
			/// </summary>
			/// <returns>The unique ID of the button, relevant for listening to interaction.</returns>
			public uint AddButton (Texture2D texture, Vector2 position)
			{
				GUIItem newItem = new Button (texture, new Rect (position, new Vector2 (texture.width, texture.height)));
				GUIRenderQueue.Add (newItem);
				return newItem.ID;
			}

			[Obsolete ("the GUISystem will not call the EventHandler anymore, use AddButton (ButtonDelegate ...) instead")]
			/// <summary>
			/// Adds a button with assigned texture to the GUISystem.
			/// </summary>
			/// <returns>The unique ID of the button, relevant for listening to interaction.</returns>
			public uint AddButton (Texture2D texture, Vector2 position, GUISnapMode snapmode)
			{
				GUIItem newItem = new Button (texture, new Rect (position, new Vector2 (texture.width, texture.height)), snapmode);
				GUIRenderQueue.Add (newItem);
				return newItem.ID;
			}

			[Obsolete ("the GUISystem will not call the EventHandler anymore, use AddButton (ButtonDelegate ...) instead")]
			/// <summary>
			/// Adds a button with assigned texture to the GUISystem.
			/// </summary>
			/// <returns>The unique ID of the button, relevant for listening to interaction.</returns>
			public uint AddButton (Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				GUIItem newItem = new Button (texture, rect, snapmode);
				GUIRenderQueue.Add (newItem);
				return newItem.ID;
			}

			/// <summary>
			/// Adds a button to the GUISystem with the assigned texture
			/// </summary>
			/// <param name="del">A method to be called when the button is clicked, with a void return and zero parameters.</param>
			public void AddButton (Button.ButtonDelegate del, Texture2D texture, Vector2 position)
			{
				AddButton (del, texture, new Rect (position, new Vector2 (texture.width, texture.height)), GUISnapMode.TopLeft);
			}

			/// <summary>
			/// Adds a button to the GUISystem with the assigned texture
			/// </summary>
			/// <param name="del">A method to be called when the button is clicked, with a void return and zero parameters.</param>
			public void AddButton (Button.ButtonDelegate del, Texture2D texture, Rect rect)
			{
				AddButton (del, texture, rect, GUISnapMode.TopLeft);
			}

			/// <summary>
			/// Adds a button to the GUISystem with the assigned texture
			/// </summary>
			/// <param name="del">A method to be called when the button is clicked, with a void return and zero parameters.</param>
			public void AddButton (Button.ButtonDelegate del, Texture2D texture, Vector2 position, GUISnapMode snapmode)
			{
				AddButton (del, texture, new Rect (position, new Vector2 (texture.width, texture.height)), snapmode);
			}

			/// <summary>
			/// Adds a button to the GUISystem with the assigned texture
			/// </summary>
			/// <param name="del">A method to be called when the button is clicked, with a void return and zero parameters.</param>
			public void AddButton (Button.ButtonDelegate del, Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				GUIItem newItem = new Button (del, texture, rect, snapmode);
				GUIRenderQueue.Add (newItem);
			}

			public void AddImage (Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				GUIItem newItem = new GUIItem (texture, rect, snapmode);
				GUIRenderQueue.Add (newItem);
			}

			public void Clear ()
			{
				GUIRenderQueue = new List<GUIItem> ();
			}

			public void Draw (Rect r)
			{
				if (r.width == 32 && r.height == 32) { // check for mini-preview
					return;
				}
				if (GUIQueueSize > 0) {
					DrawGUIItems (r, 1);
				}
			}

			// camera size implement
			public void Draw (Rect r, float cameraSize)
			{
				if (GUIQueueSize > 0) {
					DrawGUIItems (r, cameraSize);
				}
			}

			public bool ProcessEvents (Event currentEvent, Rect r)
			{
				return ProcessEvents (currentEvent, r, 1);
			}

			public bool ProcessEvents (Event currentEvent, Rect r, float cameraSize)
			{
				Vector2 originalMousePosition = currentEvent.mousePosition;
				bool e = false;
				Rect newR = new Rect ();
				if (scaleWithWorld) {
					newR.y = r.y;
					newR.height = r.height;
					newR.x = (r.x + r.width / 2) - r.height / 2;
					newR.width = r.height;
				} else {
					newR = r;
				}
				float unitSize = scaleWithWorld ? newR.height / (cameraSize * 100) : 1;
				for (int i = GUIQueueSize - 1; i >= 0; i--) {
					if ((e = GUIRenderQueue [i].ProcessEvents (currentEvent, newR, unitSize))) {
						break;
					}
				}
				currentEvent.mousePosition = originalMousePosition;
				return e;
			}

			protected void DrawGUIItems (Rect r, float cameraSize)
			{
				if (scaleWithWorld) {
					Rect newR = new Rect ();
					newR.y = r.y;
					newR.height = r.height;
					newR.x = (r.x + r.width / 2) - r.height / 2;
					newR.width = r.height;
					for (int i = 0; i < GUIQueueSize; i++) {
						GUIRenderQueue [i].Draw (newR, newR.height / (cameraSize * 100));
					}
				} else {
					for (int i = 0; i < GUIQueueSize; i++) {
						GUIRenderQueue [i].Draw (r);
					}
				}
			}
		}
	}
}
