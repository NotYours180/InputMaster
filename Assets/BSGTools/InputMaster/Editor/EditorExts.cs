using UnityEditor;
using UnityEngine;

namespace BSGTools.Editors {
	public static class EditorExts {

		public static void GUILayoutIndent(int indentLevel) {
			GUILayout.Space(18f * indentLevel);
		}

		public static void GUILayoutIndent() {
			GUILayoutIndent(EditorGUI.indentLevel);
		}
	}
}