using UnityEngine;
using System.Collections;
using UnityEditor;

public static class EditorExts {

	public static void GUILayoutIndent(int indentLevel) {
		GUILayout.Space(18f * indentLevel);
	}

	public static void GUILayoutIndent() {
		GUILayoutIndent(EditorGUI.indentLevel);
	}
}
