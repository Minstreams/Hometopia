namespace MagicaVoxelToolbox.Saving {

	using UnityEditor;
	using UnityEngine;



	public class EditorSavingBool : Saving<bool> {

		public EditorSavingBool (string key, bool defaultValue) : base(key, defaultValue) { }

		protected override bool GetValueFromPref () {
			return EditorPrefs.GetBool(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetBool(Key, Value);
		}

		public static implicit operator bool (EditorSavingBool value) {
			return value.Value;
		}

	}


	public class EditorSavingInt : Saving<int> {

		public EditorSavingInt (string key, int defaultValue) : base(key, defaultValue) { }

		protected override int GetValueFromPref () {
			return EditorPrefs.GetInt(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetInt(Key, Value);
		}

		public static implicit operator int (EditorSavingInt value) {
			return value.Value;
		}

	}


	public class EditorSavingString : Saving<string> {

		public EditorSavingString (string key, string defaultValue) : base(key, defaultValue) { }

		protected override string GetValueFromPref () {
			return EditorPrefs.GetString(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetString(Key, Value);
		}

		public static implicit operator string (EditorSavingString value) {
			return value.Value;
		}

	}


	public class EditorSavingFloat : Saving<float> {

		public EditorSavingFloat (string key, float defaultValue) : base(key, defaultValue) { }

		protected override float GetValueFromPref () {
			return EditorPrefs.GetFloat(Key, DefaultValue);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key, Value);
		}

		public static implicit operator float (EditorSavingFloat value) {
			return value.Value;
		}

	}


	public class EditorSavingVector2 : Saving<Vector2> {

		public EditorSavingVector2 (string key, Vector2 defaultValue) : base(key, defaultValue) { }

		protected override Vector2 GetValueFromPref () {
			return new Vector2(
				EditorPrefs.GetFloat(Key + ".x", DefaultValue.x),
				EditorPrefs.GetFloat(Key + ".y", DefaultValue.y)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".x", Value.x);
			EditorPrefs.SetFloat(Key + ".y", Value.y);
		}

		public static implicit operator Vector2 (EditorSavingVector2 value) {
			return value.Value;
		}

	}


	public class EditorSavingVector3 : Saving<Vector3> {

		public EditorSavingVector3 (string key, Vector3 defaultValue) : base(key, defaultValue) { }

		protected override Vector3 GetValueFromPref () {
			return new Vector3(
				EditorPrefs.GetFloat(Key + ".x", DefaultValue.x),
				EditorPrefs.GetFloat(Key + ".y", DefaultValue.y),
				EditorPrefs.GetFloat(Key + ".z", DefaultValue.z)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".x", Value.x);
			EditorPrefs.SetFloat(Key + ".y", Value.y);
			EditorPrefs.SetFloat(Key + ".z", Value.z);
		}

		public static implicit operator Vector3 (EditorSavingVector3 value) {
			return value.Value;
		}

	}


	public class EditorSavingColor : Saving<Color> {

		public EditorSavingColor (string key, Color defaultValue) : base(key, defaultValue) { }

		protected override Color GetValueFromPref () {
			return new Color(
				EditorPrefs.GetFloat(Key + ".r", DefaultValue.r),
				EditorPrefs.GetFloat(Key + ".g", DefaultValue.g),
				EditorPrefs.GetFloat(Key + ".b", DefaultValue.b),
				EditorPrefs.GetFloat(Key + ".a", DefaultValue.a)
			);
		}

		protected override void SetValueToPref () {
			EditorPrefs.SetFloat(Key + ".r", Value.r);
			EditorPrefs.SetFloat(Key + ".g", Value.g);
			EditorPrefs.SetFloat(Key + ".b", Value.b);
			EditorPrefs.SetFloat(Key + ".a", Value.a);
		}

		public static implicit operator Color (EditorSavingColor value) {
			return value.Value;
		}

	}






}