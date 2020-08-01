using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/CanvasMaterialFloatSetter")]
        public class CanvasMaterialFloatSetter : MonoBehaviour
        {
            public float target;
            public float time = 0.5f;
            public string paramName = "_EmissionFactor";

            public UnityEngine.UI.Image canvasRenderer;

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
                canvasRenderer.material.SetFloat(paramName, target);
            }

        }
    }
}