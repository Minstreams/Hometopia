using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/MaterialColorSetter")]
        public class MaterialColorSetter : MonoBehaviour
        {
            public Color target;
            public string paramName = "_EmissionFactor";
            [System.Serializable]
            public struct MaterialFloatPair
            {
                public Renderer renderer;
                public int index;
                [HideInInspector]
                public Color value;
            }
            public MaterialFloatPair[] materialFloatPairs;

            public bool setOnStart = true;
            private void Start()
            {
                if (setOnStart) Set();
            }

            //Input
            [ContextMenu("Set")]
            public void Set()
            {
                Set(target);
            }
            public void Set(Color target)
            {
                foreach (MaterialFloatPair mfp in materialFloatPairs)
                {
                    mfp.renderer.materials[mfp.index].SetColor(paramName, target);
                }
            }
        }
    }
}
