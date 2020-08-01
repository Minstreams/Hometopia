using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/StepLinker")]
        public class StepLinker : MonoBehaviour
        {
            public float stepThreadhold;

            private float stepper = 0;

            //Output
            public SimpleEvent output;

            //Input
            public void StepForward(float input)
            {
                stepper += input;
                while (stepper > stepThreadhold)
                {
                    stepper -= stepThreadhold;
                    output?.Invoke();
                }
            }
            public void ResetStepper()
            {
                stepper = 0;
            }

        }
    }
}