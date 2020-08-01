namespace MagicaVoxelToolbox.Saving {


	public abstract class Saving<T> {

		public T Value {
			get {
				return _Value;
			}
			set {
				if (_Value != null && !_Value.Equals(value)) {
					_Value = value;
					Dirty = true;
				}
			}
		}
		public string Key;

		protected T DefaultValue;

		private bool Dirty;
		private T _Value;


		public Saving (string key, T defaultValue) {
			//UnityEngine.Debug.Log("[New Saving] " + key);
			Key = key;
			DefaultValue = defaultValue;
			Value = defaultValue;
		}

		 
		public void Load () {
			//UnityEngine.Debug.Log("[Load] " + Key + "\nValue = " + Value + "\n PrefData = " + GetValueFromPref());
			_Value = GetValueFromPref();
			Dirty = false;
		}


		public void TrySave () {
			//UnityEngine.Debug.Log("[TrySave] " + Key + "\nDirty = " + Dirty + "\n Value = " + Value + "\n PrefData = " + GetValueFromPref());
			if (Dirty) {
				ForceSave();
			}
		}


		public void ForceSave () {
			SetValueToPref();
			Dirty = false;
		}




		protected abstract T GetValueFromPref ();
		protected abstract void SetValueToPref ();


	}




}