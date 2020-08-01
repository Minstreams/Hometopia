using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/AudioSystem/AudioSystemOperator")]
        public class AudioSystemOperator : MonoBehaviour
        {
            //Input
            public void StopLooping()
            {
                AudioSystem.StopLooping();
            }
            public void StopAllSounds()
            {
                AudioSystem.StopAllSounds();
            }
        }
    }
}
