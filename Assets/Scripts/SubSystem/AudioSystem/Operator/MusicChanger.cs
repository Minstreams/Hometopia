using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace Operator
    {
        [AddComponentMenu("Operator/AudioSystem/MusicChanger")]
        public class MusicChanger : Linker.AudioConfigLinker
        {
            public int clipIndex;
            //Input
            public override void Invoke()
            {
                base.Invoke();
                AudioSystem.ChangeMusic(AudioSystem.Setting.musicClips[clipIndex], data);
            }
            public void SetClipIndex(int clipIndex)
            {
                this.clipIndex = clipIndex;
            }
        }
    }
}