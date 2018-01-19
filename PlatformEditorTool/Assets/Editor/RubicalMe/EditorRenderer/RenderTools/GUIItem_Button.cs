using UnityEngine;
using System;

namespace RubicalMe
{
	public enum RM_ButtonEvents
	{
		LMB,
		RMB,
		Enter,
		Exit
	}

	namespace RenderTools
	{
		// TODO: Create  a UnityButton GUIItem that uses the default unity button with a string input
		public class Button : GUIItem
		{
			public delegate void ButtonDelegate (RM_ButtonEvents e);

			private ButtonDelegate onMouseEvent;
			private bool containsMouse;

			[Obsolete]
			public Button (Texture2D texture)
			{
				Initialize (texture, new Rect (Vector2.zero, new Vector2 (texture.width, texture.height)), GUISnapMode.TopLeft);
			}

			[Obsolete]
			public Button (Texture2D texture, Rect rect)
			{
				Initialize (texture, rect, GUISnapMode.TopLeft);
			}

			[Obsolete]
			public Button (Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				Initialize (texture, rect, snapmode);
			}

			public Button (ButtonDelegate del, Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				Initialize (texture, rect, snapmode);
				onMouseEvent += del;
			}

			public override bool ProcessEvents (Event currentEvent, Rect r, float unitSize)
			{
				//	Debug.Log ("ProcessHandler called on button");
				if (!Contains (r, currentEvent.mousePosition, unitSize)) {
					if (containsMouse) {
						containsMouse = false;
						if (onMouseEvent != null) {
							onMouseEvent (RM_ButtonEvents.Exit);
						}
					}
					return false;
				}

				switch (currentEvent.type) {
				case EventType.MouseDown:
					if (currentEvent.button == 0) {
						if (onMouseEvent != null) {
							onMouseEvent (RM_ButtonEvents.LMB);
						}
						return true;
					}
					if (currentEvent.button == 1) {
						if (onMouseEvent != null) {
							onMouseEvent (RM_ButtonEvents.RMB);
						}
						return true;
					}
					break;
				default:
					if (!containsMouse) {
						containsMouse = true;
						if (onMouseEvent != null) {
							onMouseEvent (RM_ButtonEvents.Enter);
						}
					}
					return false;
				}

				return false;
			}
		}
	}
}