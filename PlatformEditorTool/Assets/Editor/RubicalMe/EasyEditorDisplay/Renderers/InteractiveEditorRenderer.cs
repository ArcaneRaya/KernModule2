using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RubicalMe.RenderTools;


// thanks to https://gist.github.com/prodigga/53ab658e1a818cd4ddfd

namespace RubicalMe
{
	namespace Renderers
	{

		public class InteractiveEditorRenderer : EditorRenderer
		{
			public delegate void InputEvent ();

			public delegate void GUIEvent (EditorRendererEvent e);

			public GUISystem GUISystem {
				get {
					if (guiSystem == null) {
						guiSystem = new GUISystem ();
					}
					return guiSystem;
				}
			}

			/// <summary>
			/// Gets the GUI system scaled to the world, only works well when camera is orthographic. One Unity unit equals one pixel.
			/// </summary>
			public GUISystem GUISystemWorldScaled {
				get {
					if (guiSystemWorldSpace == null) {
						guiSystemWorldSpace = new GUISystem (true);
					}
					return guiSystemWorldSpace;
				}
			}

			public GUIEvent OnGUIEvent;
			public InputEvent OnMouseDown;
			public InputEvent OnMouseUp;
			public Vector2 mousePosition;
			public Vector2 mousePositionWorld;

			protected GUISystem guiSystem;
			protected GUISystem guiSystemWorldSpace;

			public InteractiveEditorRenderer ()
			{
				guiSystem = new GUISystem ();
				guiSystemWorldSpace = new GUISystem (true);
			}

			public InteractiveEditorRenderer (Rect r)
			{
				this.rect = r;
				guiSystem = new GUISystem ();
				guiSystemWorldSpace = new GUISystem (true);
			}

			// THIS IS NOT A UNITY-CALLED METHOD
			public override bool OnGUI ()
			{
				if (ProcessEvents (Event.current)) {
//					return true;
				}
				return base.OnGUI ();
			}

			[System.Obsolete]
			public List<RenderItem> GetRenderItemsAt (Vector3 position)
			{
				List<RenderItem> rItems = new List<RenderItem> ();
				foreach (RenderItem rItem in RenderQueue) {
					if (rItem.Position == position) {
						rItems.Add (rItem);
					}
				}
				return rItems;
			}

			[System.Obsolete]
			public void ReplaceRenderItem (RenderItem oldItem, RenderItem newItem)
			{
				int index = RenderQueue.IndexOf (oldItem);
				RenderQueue.Remove (oldItem);
				RenderQueue.Insert (index, newItem);
				CallDirty ();
			}

			protected override void Display ()
			{
				base.Display ();
//				Debug.Log (Camera.orthographicSize);
				GUISystemWorldScaled.Draw (rect, Camera.orthographicSize * 2);
				guiSystem.Draw (rect);
			}

			protected bool ProcessEvents (Event currentEvent)
			{	
				if (rect.Contains (currentEvent.mousePosition)) {
					//	mousePosition = currentEvent.mousePosition - rect.position;
					bool e = GUISystem.ProcessEvents (currentEvent, rect);
					if (e) {
						return true;
					}
					e = GUISystemWorldScaled.ProcessEvents (currentEvent, rect, Camera.orthographicSize * 2);
					if (e) {
						return true;
					}

					if (renderUtil.camera.orthographic) {
						Vector2 invertedY = new Vector2 (mousePosition.x, rect.height - mousePosition.y);
						mousePositionWorld = renderUtil.camera.ScreenToWorldPoint (invertedY * renderUtil.GetScaleFactor (rect.width, rect.height));
					}

					switch (currentEvent.type) {
					case EventType.MouseDown:
						if (OnMouseDown != null) {
							OnMouseDown ();
						}
						break;

					case EventType.MouseUp:
						if (OnMouseUp != null) {
							OnMouseUp ();
						}
						break;
					}
				} else {
					mousePosition = new Vector2 (-1, -1);
					mousePositionWorld = Vector2.zero;
				}

				return false;
			}
		}
	}
}