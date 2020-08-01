using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Linker
    {
        [AddComponentMenu("Linker/AudioSystem/AudioConfigLinker")]
        public class AudioConfigLinker : MonoBehaviour
        {
            public AudioSystem.AudioConfig data;

            //Output
            public AudioConfigEvent output;

            //Input
            public virtual void Invoke()
            {
                output?.Invoke(data);
            }
            public void SetLoop(bool loop)
            {
                data.loop = loop;
            }
            public void SetPitch(float pitch)
            {
                data.pitch = pitch;
            }
            public void SetVolume(float volume)
            {
                data.volume = volume;
            }
        }
    }
}
