
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/LerpColorLinker")]
        public class LerpColorLinker : MonoBehaviour
        {
            public Color colorA;
            public Color colorB;
            public AnimationCurve remapCurve;
            public bool clamp = false;

            //Output
            public ColorEvent output;

            //Input
            public void Invoke(float input)
            {
                float t = remapCurve.Evaluate(clamp ? Mathf.Clamp01(input) : input);
                output?.Invoke(Color.Lerp(colorA, colorB, t));
            }
        }
    }
}
