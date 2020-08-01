using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/TimerLinker")]
        public class TimerLinker : MonoBehaviour
        {
            [Tooltip("time must be between 0 and 1")]
            public AnimationCurve curve;
            public float time;
            private IEnumerator invoke(float time)
            {
                float timer = 0;
                while (timer < 1)
                {
                    yield return 0;
                    output?.Invoke(curve.Evaluate(timer));
                    timer += Time.deltaTime / time;
                }
                output?.Invoke(curve.Evaluate(1));
            }

            //Output
            public FloatEvent output;


            //Input
            public void Invoke()
            {
                StopAllCoroutines();
                StartCoroutine(invoke(time));
            }
            public void Invoke(float time)
            {
                StartCoroutine(invoke(time));
            }
        }
    }
}