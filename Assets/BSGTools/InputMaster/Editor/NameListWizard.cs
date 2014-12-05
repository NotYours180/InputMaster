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
using System.Linq;

namespace BSGTools.Editors {
	public class NameListWizard : ScriptableWizard {

		private const string TEMPLATE = "/BSGTools/InputMaster/AutoGenTemplates/NameListTemplate.txt";
		private const string CONST_FORMAT = @"public const string {0} = ""{1}"";";

		[SerializeField]
		StandaloneControlConfig standaloneConfig;
		[SerializeField]
		XboxControlConfig xboxConfig;
		[SerializeField]
		CombinedOutputsConfig combinedOutputsConfig;
		[SerializeField]
		string scriptName = "NameList";

		CSharpCodeProvider provider = new CSharpCodeProvider();

		[MenuItem("BSGTools/InputMaster/Create NameList")]
		public static void ShowWizard() {
			ScriptableWizard.DisplayWizard<NameListWizard>("NameList Wizard", "Create NameList");
		}

		void OnWizardUpdate() {
			List<string> names = new List<string>();
			if(standaloneConfig != null)
				names.AddRange(standaloneConfig.controls.Select(c => c.identifier));
			if(xboxConfig != null)
				names.AddRange(xboxConfig.LinqSelect(c => c.identifier));
			if(combinedOutputsConfig != null)
				names.AddRange(combinedOutputsConfig.outputs.Select(c => c.identifier));

			if(names.Distinct().Count() != names.Count) {
				errorString = "Cannot create NameList: Ensure that every control and CombinedOutput has a unique identifier.";
				isValid = false;
			}
			else {
				isValid = true;
				errorString = string.Empty;
			}
		}

		void OnWizardCreate() {


			var templateText = File.ReadAllText(Application.dataPath + TEMPLATE, Encoding.Default);
			var sb = new StringBuilder();

			if(standaloneConfig != null && standaloneConfig.controls.Count != 0) {
				sb.AppendLine("\t// Standalone Controls");
				foreach(var c in standaloneConfig.controls)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier),
						c.identifier));
			}
			if(xboxConfig != null && xboxConfig.totalCount != 0) {
				sb.AppendLine();
				sb.AppendLine("\t// Xbox Controls");

				foreach(var c in xboxConfig.Combine())
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));
			}
			if(combinedOutputsConfig != null && combinedOutputsConfig.outputs.Count != 0) {
				sb.AppendLine();
				sb.AppendLine("\t// CombinedOutputs");
				foreach(var c in combinedOutputsConfig.outputs)
					sb.AppendLine("\t" + string.Format(CONST_FORMAT, provider.CreateValidIdentifier(c.identifier), c.identifier));
			}
			templateText = string.Format(templateText, scriptName, sb.ToString().TrimEnd());
			File.WriteAllText(string.Format("{0}/{1}.cs", Application.dataPath, scriptName), templateText);
			AssetDatabase.Refresh();
		}
	}
}