/**
Copyright (c) 2014, Michael Notarnicola
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using UnityEngine;

namespace BSGTools.Structure {
	/// <summary>
	/// Based from: http://wiki.unity3d.com/index.php/Singleton
	/// Be aware this will not prevent a non singleton constructor
	/// such as <c>T myT = new T();</c>
	/// To prevent that, add <c>protected T () {}</c> to your singleton class.
	/// As a note, this is made as MonoBehaviour for Coroutines.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
		private static T _instance;
		private static object _lock = new object();

		/// <value>
		/// Does singleton checking, then returns the instance of this class.
		/// </value>
		public static T Instance {
			get {
				return GetInstance();
			}
		}

		private static T GetInstance() {
			if(applicationIsQuitting)
				return null;

			lock(_lock) {
				if(_instance == null) {
					var typeName = typeof(T).Name;
					var all = FindObjectsOfType<T>();

					if(all.Length > 1) {
						Debug.LogError(string.Format("[Singleton] found {0} instances of {1}", all.Length, typeName));
						_instance = all[0];
					}
					else if(all.Length == 0) {
						var parent = new GameObject("[Singleton] " + typeName);
						DontDestroyOnLoad(parent);
						_instance = parent.AddComponent<T>();
					}
				}
				return _instance;
			}
		}

		private static bool applicationIsQuitting = false;
		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		/// it will create a buggy ghost object that will stay on the Editor scene 
		/// even after stopping playing the Application.
		/// So, this is to be sure that a ghost object is not created.
		/// </summary>
		public void OnDestroy() {
			applicationIsQuitting = true;
		}
	}
}