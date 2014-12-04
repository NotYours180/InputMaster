using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using BSGTools.IO.Xbox;
using BSGTools.IO;

namespace BSGTools.Editors {
	public class NameListWizard : ScriptableWizard {

		private const string TEMPLATE = "/BSGTools/InputMaster/AutoGenTemplates/NameListTemplate.txt";
		private const string CONST_FORMAT = @"public const string {0} = ""{1}"";";

		[SerializeField]
		StandaloneControlConfig standaloneConfig;
		[SerializeField]
		XboxControlConfig xboxConfig;
		[SerializeField]
		string scriptName = "NameList";

		CSharpCodeProvider provider = new CSharpCodeProvider();

		[MenuItem("BSGTools/InputMaster/Create NameList")]
		public static void ShowWizard() {
			ScriptableWizard.DisplayWizard<NameListWizard>("NameList Wizard", "Create NameList");
		}

		void OnWizardCreate() {
			var templateText = File.ReadAllText(Application.dataPath + TEMPLATE, Encoding.Default);
			var sb = new StringBuilder();

			if(standaloneConfig != null) {
				sb.AppendLine("\t// Standalone Controls");
				foreach(var c in standaloneConfig.controls)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));
			}
			if(xboxConfig != null) {
				sb.AppendLine();
				sb.AppendLine("\t// XButton Controls");
				foreach(var c in xboxConfig.xbControls)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));

				sb.AppendLine();
				sb.AppendLine("\t// XStick Controls");
				foreach(var c in xboxConfig.xsControls)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));

				sb.AppendLine();
				sb.AppendLine("\t// XTrigger Controls");
				foreach(var c in xboxConfig.xtControls)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));
			}
			templateText = string.Format(templateText, scriptName, sb.ToString().TrimEnd());
			File.WriteAllText(string.Format("{0}/{1}.cs", Application.dataPath, scriptName), templateText);
			AssetDatabase.Refresh();
		}
	}
}