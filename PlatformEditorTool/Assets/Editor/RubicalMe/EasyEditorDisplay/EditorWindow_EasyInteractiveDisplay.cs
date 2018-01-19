using UnityEngine;
using UnityEditor;
using RubicalMe.Renderers;

namespace RubicalMe
{
	// NOTE: Without summary tags because mono-develop acts weird when summary tags are used.....
	/// <para>
	/// This extension of the EditorWindow class adds the functionality of a controlled / customizable 
	/// preview area at the bottom. Moreover,it provides a simple GUI functionality with
	/// image planes and buttons.
	/// </para>
	/// <remarks>
	/// Override the InteractiveRendererDisplay property to assign a custom drawing position of the 
	/// preview area.
	/// <para>When overriding the OnEnable or OnGUI methods, make sure to call the base method or
	/// functionality could get lost.</para>
	/// </remarks>
	/// <seealso cref="EditorWindow_EasySimpleDisplay"/>
	public class EditorWindow_EasyInteractiveDisplay : EditorWindow_EasySimpleDisplay
	{
		new protected InteractiveEditorRenderer Display {
			get {
				return privateDisplay as InteractiveEditorRenderer;
			}
			set {
				privateDisplay = value;
			}
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		public override void OnEnable ()
		{
			if (Display == null) {
				Display = new InteractiveEditorRenderer (InteractiveRendererDisplay);
			}
			Display.OnGUIEvent += EventHandler;
			Display.OnIsDirty += Repaint;
		}

		public override void OnDisable ()
		{
			Display.OnGUIEvent -= EventHandler;
			Display.OnIsDirty -= Repaint;
		}

		protected virtual void EventHandler (EditorRendererEvent e)
		{
			throw new System.NotImplementedException ("EventHandler has not been overridden but GUIEvens are called from " + this.GetType ());
		}
	}
}