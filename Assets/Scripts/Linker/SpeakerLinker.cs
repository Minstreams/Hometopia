using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/SpeakerLinker")]
        public class SpeakerLinker : MonoBehaviour
        {
            public AudioSource audioSource;

            [Tooltip("0 = left Channel, 1 = right Channel")]
            public int channel = 0;
            public FFTWindow fTWindow;
            public int outputFrequencyIndex = 3;
            private float[] data = new float[64];

            private void Update()
            {
                audioSource.GetSpectrumData(data, channel, fTWindow);
                onSpeak?.Invoke(data[outputFrequencyIndex]);
            }

            //Output
            public FloatEvent onSpeak;
        }
    }
}
