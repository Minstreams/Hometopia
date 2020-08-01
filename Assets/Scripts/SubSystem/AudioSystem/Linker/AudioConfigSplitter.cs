using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/AudioSystem/AudioConfigSplitter")]
        public class AudioConfigSplitter : MonoBehaviour
        {
            //Output
            public BoolEvent loopOutput;
            public FloatEvent pitchOutput;
            public FloatEvent volumeOutput;

            //Input
            public void Invoke(AudioSystem.AudioConfig data)
            {
                loopOutput?.Invoke(data.loop);
                pitchOutput?.Invoke(data.pitch);
                volumeOutput?.Invoke(data.volume);
            }
        }
    }
}