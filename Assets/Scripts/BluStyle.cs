using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Blurift {

	public class BluStyle {

		private static Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle> ();

		/// <summary>
		/// Adds the style to the global style list.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <param name="style">Style.</param>
		public static void AddStyle(string id, GUIStyle style)
		{
			if(!styles.ContainsKey(id))
			{
				styles[id] = style;
			}
		}

		/// <summary>
		/// Gets the style from the global style list.
		/// </summary>
		/// <returns>The style.</returns>
		/// <param name="id">Identifier.</param>
		public static GUIStyle GetStyle(string id)
		{
			if (styles.ContainsKey (id))
				return styles [id];
			return null;
		}


		/// <summary>
		/// Makes a custom style from another style.
		/// </summary>
		/// <returns>New GUIStyle</returns>
		/// <param name="styleFrom">Style from which the new style will be based</param>
		/// <param name="size">Size in pixels the font should be.</param>
		public static GUIStyle CustomStyle(GUIStyle styleFrom, float size)
		{
			GUIStyle style = new GUIStyle (styleFrom);
			style.fontSize = (int)size;
			return style;
		}


	}

}
