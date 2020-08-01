using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/MaterialFloatSetter")]
        public class MaterialFloatSetter : MonoBehaviour
        {
            public float target;
            public float time = 0.5f;
            public string paramName = "_EmissionFactor";
            [System.Serializable]
            public struct MaterialFloatPair
            {
                public Renderer renderer;
                public int index;
                [HideInInspector]
                public float value;
            }
            public MaterialFloatPair[] materialFloatPairs;

            public bool setOnStart = true;
            private void Start()
            {
                if (setOnStart) Set();
            }

            public bool setOnEnable;

            private void OnEnable()
            {
                if (setOnEnable) Set();
            }

            //Input
            [ContextMenu("Set")]
            public void Set()
            {
                Set(target);
            }
            public void Set(float target)
            {
                foreach (MaterialFloatPair mfp in materialFloatPairs)
                {
                    mfp.renderer.materials[mfp.index].SetFloat(paramName, target);
                }
            }

            [ContextMenu("SetInTime")]
            public void SetInTime()
            {
                SetInTime(target);
            }
            public void SetInTime(float target)
            {
                StopAllCoroutines();
                StartCoroutine(setInTime(target));
            }
            public IEnumerator setInTime(float target)
            {
                float timer = 0;
                Vector3 oPos = transform.position;
                for (int i = 0; i < materialFloatPairs.Length; i++)
                {
                    materialFloatPairs[i].value = materialFloatPairs[i].renderer.materials[materialFloatPairs[i].index].GetFloat(paramName);
                }
                while (timer < 1)
                {
                    yield return 0;
                    foreach (MaterialFloatPair mfp in materialFloatPairs)
                    {
                        mfp.renderer.materials[mfp.index].SetFloat(paramName, Mathf.Lerp(mfp.value, target, timer));
                    }
                    timer += Time.deltaTime / time;
                }
                foreach (MaterialFloatPair mfp in materialFloatPairs)
                {
                    mfp.renderer.materials[mfp.index].SetFloat(paramName, target);
                }
            }

        }
    }
}
