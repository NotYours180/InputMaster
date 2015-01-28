using System.Collections.Generic;
using System.Linq;
using BSGTools.IO.Xbox;
using UnityEditor;
using UnityEngine;

namespace BSGTools.Editors {
	[CustomEditor(typeof(XboxControlConfig))]
	public class XboxConfigEditor : Editor {
		[SerializeField]
		XboxControlConfig config;
		[SerializeField]
		Vector2 scroll;
		[SerializeField]
		List<bool> xbFoldouts, xsFoldouts, xtFoldouts;
		[SerializeField]
		string filterStr;
		[SerializeField]
		bool filterFoldout, filterXB, filterXS, filterXT, filterAny;

		void OnEnable() {
			scroll = Vector2.zero;
			config = target as XboxControlConfig;
			xbFoldouts = Enumerable.Repeat(false, config.totalCount).ToList();
			xsFoldouts = Enumerable.Repeat(false, config.totalCount).ToList();
			xtFoldouts = Enumerable.Repeat(false, config.totalCount).ToList();
		}

		public override void OnInspectorGUI() {
			DrawHeaderControls();
			DrawControls();

			if(GUI.changed)
				EditorUtility.SetDirty(target);
		}

		void DrawControls() {
			scroll = EditorGUILayout.BeginScrollView(scroll);
			//XButtons
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("XButtonControls", EditorStyles.boldLabel);
			for(int i = 0;i < config.xbControls.Count;i++) {
				var c = config.xbControls[i];
				if(ShouldFilterOut(c))
					continue;
				EditorGUI.indentLevel = 1;
				xbFoldouts[i] = EditorGUILayout.Foldout(xbFoldouts[i], string.Format("{0}: {1}", i + 1, c.identifier));
				if(xbFoldouts[i]) {
					EditorGUI.indentLevel = 2;
					if(DrawControl(c) && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to delete this control?", "DELETE", "CANCEL")) {
						config.xbControls.RemoveAt(i);
						xbFoldouts.RemoveAt(i);
					}
				}
			}

			//XSticks
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("XStickControls", EditorStyles.boldLabel);
			for(int i = 0;i < config.xsControls.Count;i++) {
				var c = config.xsControls[i];
				if(ShouldFilterOut(c))
					continue;
				EditorGUI.indentLevel = 1;
				xsFoldouts[i] = EditorGUILayout.Foldout(xsFoldouts[i], string.Format("{0}: {1}", i + 1, c.identifier));
				if(xsFoldouts[i]) {
					EditorGUI.indentLevel = 2;
					if(DrawControl(c) && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to delete this control?", "DELETE", "CANCEL")) {
						config.xsControls.RemoveAt(i);
						xsFoldouts.RemoveAt(i);
					}
				}
			}

			//XTrigger
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("XTriggerControls", EditorStyles.boldLabel);
			for(int i = 0;i < config.xtControls.Count;i++) {
				var c = config.xtControls[i];
				if(ShouldFilterOut(c))
					continue;
				EditorGUI.indentLevel = 1;
				xtFoldouts[i] = EditorGUILayout.Foldout(xtFoldouts[i], string.Format("{0}: {1}", i + 1, c.identifier));
				if(xtFoldouts[i]) {
					EditorGUI.indentLevel = 2;
					if(DrawControl(c) && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to delete this control?", "DELETE", "CANCEL")) {
						config.xtControls.RemoveAt(i);
						xtFoldouts.RemoveAt(i);
					}
				}
			}
			EditorGUI.indentLevel = 0;
			EditorGUILayout.EndScrollView();
		}

		//Returns true if the control shouldn't be shown due to filtering.
		private bool ShouldFilterOut(XboxControl c) {
			if(!string.IsNullOrEmpty(filterStr) && !c.identifier.ToLower().Contains(filterStr.ToLower()))
				return true;
			if(filterAny) {
				if(filterXB && !(c is XButtonControl))
					return true;
				else if(filterXS && !(c is XStickControl))
					return true;
				else if(filterXT && !(c is XTriggerControl))
					return true;
			}
			return false;
		}

		//Returns true if delete button is pressed.
		bool DrawControl<T>(T c) where T : XboxControl {
			GUILayout.BeginHorizontal();
			GUILayoutIndent();
			bool shouldDelete = GUILayout.Button("Delete");
			GUILayout.EndHorizontal();
			if(shouldDelete)
				return true;
			EditorGUILayout.Space();

			c.identifier = EditorGUILayout.TextField(new GUIContent("Identifier"), c.identifier);

			if(c is XButtonControl) {
				var xbc = c as XButtonControl;
				xbc.positive = (XButton)EditorGUILayout.EnumPopup("Positive", xbc.positive);
				xbc.negative = (XButton)EditorGUILayout.EnumPopup("Negative", xbc.negative);
			}
			else if(c is XStickControl) {
				var xsc = c as XStickControl;
				xsc.stick = (XStick)EditorGUILayout.EnumPopup("Positive", xsc.stick);
			}
			else if(c is XTriggerControl) {
				var xtc = c as XTriggerControl;
				xtc.trigger = (XTrigger)EditorGUILayout.EnumPopup("Positive", xtc.trigger);
			}

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

		private static void GUILayoutIndent() {
			GUILayoutIndent(EditorGUI.indentLevel);
		}

		private static void GUILayoutIndent(int indentLevel) {
			GUILayout.Space(indentLevel * 18);
		}

		void DrawHeaderControls() {
			EditorGUI.indentLevel = 0;
			var centeredBold = GUI.skin.label;
			centeredBold.alignment = TextAnchor.UpperCenter;
			centeredBold.fontStyle = FontStyle.Bold;

			EditorGUILayout.LabelField("Xbox Control Configuration", centeredBold);
			EditorGUILayout.Space();

			filterFoldout = EditorGUILayout.Foldout(filterFoldout, "Filters");
			if(filterFoldout)
				DrawFilters();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.BeginVertical();
			if(GUILayout.Button("New XBC")) {
				config.Add(new XButtonControl(XButton.A));
				xbFoldouts.Add(false);
			}
			if(GUILayout.Button("New XSC")) {
				config.Add(new XStickControl(XStick.StickLeftX));
				xsFoldouts.Add(false);
			}
			if(GUILayout.Button("New XTC")) {
				config.Add(new XTriggerControl(XTrigger.TriggerLeft));
				xtFoldouts.Add(false);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			if(GUILayout.Button("Remove All") && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to DELETE ALL XBCONTROLS?", "DELETE ALL", "CANCEL")) {
				config.xbControls.Clear();
				xbFoldouts.Clear();
			}
			if(GUILayout.Button("Remove All") && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to DELETE ALL XSCONTROLS?", "DELETE ALL", "CANCEL")) {
				config.xsControls.Clear();
				xsFoldouts.Clear();
			}
			if(GUILayout.Button("Remove All") && EditorUtility.DisplayDialog("Confirm", "Are you sure you'd like to DELETE ALL XTCONTROLS?", "DELETE ALL", "CANCEL")) {
				config.xtControls.Clear();
				xtFoldouts.Clear();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			if(GUILayout.Button("Expand All"))
				for(int i = 0;i < xbFoldouts.Count;i++)
					xbFoldouts[i] = true;
			if(GUILayout.Button("Expand All"))
				for(int i = 0;i < xsFoldouts.Count;i++)
					xsFoldouts[i] = true;
			if(GUILayout.Button("Expand All"))
				for(int i = 0;i < xtFoldouts.Count;i++)
					xtFoldouts[i] = true;
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			if(GUILayout.Button("Collapse All"))
				for(int i = 0;i < xbFoldouts.Count;i++)
					xbFoldouts[i] = false;
			if(GUILayout.Button("Collapse All"))
				for(int i = 0;i < xsFoldouts.Count;i++)
					xsFoldouts[i] = false;
			if(GUILayout.Button("Collapse All"))
				for(int i = 0;i < xtFoldouts.Count;i++)
					xtFoldouts[i] = false;
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
		}

		private void DrawFilters() {
			EditorGUI.indentLevel = 1;
			filterStr = EditorGUILayout.TextField("Identifier Filter:", filterStr);
			filterXB = EditorGUILayout.Toggle("XButton Filter", filterXB);
			filterXS = EditorGUILayout.Toggle("XStick Filter", filterXS);
			filterXT = EditorGUILayout.Toggle("XTrigger Filter", filterXT);
			filterAny = filterXB || filterXS || filterXT;
			EditorGUI.indentLevel = 0;
		}
	}
}
