using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/AudioSystem/SoundPlayer")]
        public class SoundPlayer : Linker.AudioConfigLinker
        {
            public AudioSystem.Sound sound;
            //Input
            public override void Invoke()
            {
                base.Invoke();
                AudioSystem.PlaySound(AudioSystem.Setting.soundClips[sound], data);
            }
            public void SetClipIndex(AudioSystem.Sound sound)
            {
                this.sound = sound;
            }
        }
    }
}