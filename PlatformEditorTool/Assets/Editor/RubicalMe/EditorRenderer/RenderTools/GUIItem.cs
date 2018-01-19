using UnityEngine;

namespace RubicalMe
{
	namespace RenderTools
	{
		public class GUIItem
		{
			protected static uint GUIItemsAlive;

			public Texture2D Texture {
				get {
					return texture;
				}
			}

			public Rect Rect {
				get {
					return rect;
				}
			}

			public GUISnapMode SnapMode {
				get {
					return snapmode;
				}
			}

			[System.Obsolete]
			public uint ID {
				get {
					return mID;
				}
			}

			protected Texture2D texture;
			protected Rect rect;
			protected GUISnapMode snapmode;
			protected uint mID;

			protected GUIItem ()
			{
				
			}

			public GUIItem (Texture2D texture)
			{
				Initialize (texture, new Rect (Vector2.zero, new Vector2 (texture.width, texture.height)), GUISnapMode.TopLeft);
			}

			public GUIItem (Texture2D texture, Rect rect)
			{
				Initialize (texture, rect, GUISnapMode.TopLeft);
			}

			public GUIItem (Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				Initialize (texture, rect, snapmode);
			}

			public void SetTexture (Texture2D texture)
			{
				this.texture = texture;
			}

			public void SetPosition (Rect rect)
			{
				this.rect = rect;
			}

			public void SetSnapMode (GUISnapMode snapmode)
			{
				this.snapmode = snapmode;
			}

			public void Draw (Rect r)
			{
				Draw (r, 1);
			}

			public void Draw (Rect r, float unitSize)
			{
				GUI.DrawTexture (new Rect (r.position + rect.position * unitSize + GetOffset (r, unitSize), rect.size * unitSize), texture);
			}

			public bool ProcessEvents (Event currentEvent, Rect r)
			{
				return ProcessEvents (currentEvent, r, 1);
			}

			public virtual bool ProcessEvents (Event currentEvent, Rect r, float unitSize)
			{
				return false;
			}

			public bool Contains (Rect r, Vector2 position)
			{
				return Contains (r, position, 1);
			}

			public bool Contains (Rect r, Vector2 position, float unitSize)
			{
				return new Rect (r.position + rect.position * unitSize + GetOffset (r, unitSize), rect.size * unitSize).Contains (position);
			}

			protected Vector2 GetOffset (Rect r)
			{
				return GetOffset (r, 1);
			}

			protected Vector2 GetOffset (Rect r, float unitSize)
			{
//				Debug.Log (snapmode);
				switch (snapmode) {
				case GUISnapMode.TopLeft:
					return Vector2.zero;
				case GUISnapMode.TopCenter:
					return new Vector2 (r.width / 2, 0) - new Vector2 (rect.width / 2, 0) * unitSize;
				case GUISnapMode.TopRight:
					return new Vector2 (r.width, 0) - new Vector2 (rect.width, 0) * unitSize;
				case GUISnapMode.MidLeft:
					return new Vector2 (0, r.height / 2) - new Vector2 (0, rect.height / 2) * unitSize;
				case GUISnapMode.MidCenter:
					return new Vector2 (r.width / 2, r.height / 2) - new Vector2 (rect.width / 2, rect.height / 2) * unitSize;
				case GUISnapMode.MidRight:
					return new Vector2 (r.width, r.height / 2) - new Vector2 (rect.width, rect.height / 2) * unitSize;
				case GUISnapMode.BottomLeft:
					return new Vector2 (0, r.height) - new Vector2 (0, rect.height) * unitSize;
				case GUISnapMode.BottomCenter:
					return new Vector2 (r.width / 2, r.height) - new Vector2 (rect.width / 2, rect.height) * unitSize;
				case GUISnapMode.BottomRight:
					return r.size - rect.size * unitSize;
				default:
					return Vector2.zero;
				}
			}

			protected void Initialize (Texture2D texture, Rect rect, GUISnapMode snapmode)
			{
				this.texture = texture;
				this.rect = rect;
				this.snapmode = snapmode;
				this.mID = GUIItemsAlive++;
			}
		}
	}
}