using UnityEngine;
using System.Collections;
using UnityEditor;
using BSGTools.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BSGTools.Editors {
	[CustomEditor(typeof(StandaloneControlConfig))]
	public class StandaloneConfigEditor : Editor {
		[SerializeField]
		StandaloneControlConfig config;
		[SerializeField]
		Vector2 scroll;
		[SerializeField]
		List<bool> foldouts;

		[SerializeField]
		string filterStr;
		[SerializeField]
		bool filterFoldout;

		void OnEnable() {
			scroll = Vector2.zero;
			config = target as StandaloneControlConfig;
			foldouts = Enumerable.Repeat(false, config.standaloneControls.Count).ToList();
		}

		public override void OnInspectorGUI() {
			DrawHeaderControls();
			DrawControls();

			if(GUI.changed)
				EditorUtility.SetDirty(target);
		}

		void DrawControls() {
			scroll = EditorGUILayout.BeginScrollView(scroll);
			for(int i = 0;i < config.standaloneControls.Count;i++) {
				var c = config.standaloneControls[i];
				if(!string.IsNullOrEmpty(filterStr) && !c.identifier.ToLower().Contains(filterStr.ToLower()))
					continue;

				EditorGUI.indentLevel = 1;
				foldouts[i] = EditorGUILayout.Foldout(foldouts[i], (i + 1) + ": " + c.identifier);
				if(foldouts[i]) {
					EditorGUI.indentLevel = 2;
					if(DrawControl(c)) {
						config.standaloneControls.RemoveAt(i);
						foldouts.RemoveAt(i);
					}
					FixInvalidValues(c);
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUI.indentLevel = 0;
		}

		private void FixInvalidValues(StandaloneControl c) {
			c.positive = (c.positive == KeyCode.None) ? KeyCode.A : c.positive;
			c.sensitivity = (c.sensitivity < 0f) ? Mathf.Abs(c.sensitivity) : c.sensitivity;
			c.gravity = (c.gravity < 0f) ? Mathf.Abs(c.gravity) : c.gravity;
			c.dead = (c.dead < 0f) ? Mathf.Abs(c.dead) : c.dead;
		}

		//Returns true if delete button is pressed.
		bool DrawControl(StandaloneControl c) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(EditorGUI.indentLevel * 18);
			bool shouldDelete = GUILayout.Button("Delete");
			GUILayout.EndHorizontal();
			if(shouldDelete)
				return true;
			EditorGUILayout.Space();

			c.identifier = EditorGUILayout.TextField(new GUIContent("Identifier"), c.identifier);

			c.positive = (KeyCode)EditorGUILayout.EnumPopup("Positive", c.positive);
			c.negative = (KeyCode)EditorGUILayout.EnumPopup("Negative", c.negative);
			c.modifier = ModifierKey.FromMEnum((ModifierKey.ModEnums)EditorGUILayout.EnumPopup("Modifier", ModifierKey.ToMEnum(c.modifier)));

			c.sensitivity = EditorGUILayout.FloatField("Sensitivity", c.sensitivity);
			c.gravity = EditorGUILayout.FloatField("Gravity", c.gravity);
			c.dead = EditorGUILayout.FloatField("Dead", c.dead);
			c.snap = EditorGUILayout.Toggle("Snap", c.snap);
			c.invert = EditorGUILayout.Toggle("Invert", c.invert);
			c.blocked = EditorGUILayout.Toggle("Blocked", c.blocked);
			c.debugOnly = EditorGUILayout.Toggle("Debug Only", c.debugOnly);

			EditorGUILayout.Space();
			return false;
		}

		void DrawHeaderControls() {
			EditorGUI.indentLevel = 0;
			var centeredBold = GUI.skin.label;
			centeredBold.alignment = TextAnchor.UpperCenter;
			centeredBold.fontStyle = FontStyle.Bold;

			EditorGUILayout.LabelField("Standalone Control Configuration", centeredBold);
			EditorGUILayout.Space();
			filterFoldout = EditorGUILayout.Foldout(filterFoldout, "Filters");
			if(filterFoldout)
				DrawFilters();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Add New")) {
				config.standaloneControls.Add(new StandaloneControl(KeyCode.A));
				foldouts.Add(false);
			}
			if(GUILayout.Button("Remove All")) {
				config.standaloneControls.Clear();
				foldouts.Clear();
			}
			if(GUILayout.Button("Expand All"))
				for(int i = 0;i < foldouts.Count;i++)
					foldouts[i] = true;
			if(GUILayout.Button("Collapse All"))
				for(int i = 0;i < foldouts.Count;i++)
					foldouts[i] = false;
			EditorGUILayout.EndHorizontal();
		}

		private void DrawFilters() {
			EditorGUI.indentLevel = 1;
			filterStr = EditorGUILayout.TextField("Identifier Filter:", filterStr);
			EditorGUI.indentLevel = 0;
		}
	}
}