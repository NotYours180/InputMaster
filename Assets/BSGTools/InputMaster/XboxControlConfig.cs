using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BSGTools.IO.Xbox {
	public class XboxControlConfig : ScriptableObject {
		public List<XButtonControl> xbControls = new List<XButtonControl>();
		public List<XStickControl> xsControls = new List<XStickControl>();
		public List<XTriggerControl> xtControls = new List<XTriggerControl>();

		public int totalCount { get { return xbControls.Count + xsControls.Count + xtControls.Count; } }

		public void ForEach(Action<XboxControl> a) {
			foreach(var c in xbControls)
				a(c);
			foreach(var c in xsControls)
				a(c);
			foreach(var c in xtControls)
				a(c);
		}

		public void For(Action<XboxControl> a) {
			for(int i = 0;i < xbControls.Count;i++)
				a(xbControls[i]);
			for(int i = 0;i < xsControls.Count;i++)
				a(xsControls[i]);
			for(int i = 0;i < xtControls.Count;i++)
				a(xtControls[i]);
		}

		public void Add(XboxControl c) {
			if(c is XButtonControl)
				xbControls.Add(c as XButtonControl);
			else if(c is XStickControl)
				xsControls.Add(c as XStickControl);
			else if(c is XTriggerControl)
				xtControls.Add(c as XTriggerControl);
		}

		public void ClearAll() {
			xbControls.Clear();
			xsControls.Clear();
			xtControls.Clear();
		}

		public bool Any(Func<XboxControl, bool> p) {
			return xbControls.Cast<XboxControl>().Any(p) || xsControls.Cast<XboxControl>().Any(p) || xtControls.Cast<XboxControl>().Any(p);
		}

		public IEnumerable<TResult> LinqSelect<TResult>(Func<XboxControl, TResult> f) {
			return xbControls.Cast<XboxControl>().Select(f).Concat(xsControls.Cast<XboxControl>().Select(f).Concat(xtControls.Cast<XboxControl>().Select(f)));
		}
	}
}