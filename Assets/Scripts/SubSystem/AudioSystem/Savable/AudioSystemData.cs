using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace Savable
    {
        [CreateAssetMenu(fileName = "AudioSystemData", menuName = "Savable/AudioSystemData")]
        public class AudioSystemData : SavableObject
        {
            public float musicVolume;
            public float soundVolume;

            public override void ApplyData()
            {
                AudioSystem.SetMusicVolume(musicVolume);
                AudioSystem.SetSoundVolume(soundVolume);
            }

            public override void UpdateData()
            {
                AudioSystem.Setting.mainMixer.GetFloat("MusicVolume", out musicVolume);
                AudioSystem.Setting.mainMixer.GetFloat("SoundVolume", out soundVolume);
            }
        }
    }
}
