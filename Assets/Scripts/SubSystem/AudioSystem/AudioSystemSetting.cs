using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


namespace GameSystem
{
    namespace Setting
    {
        [CreateAssetMenu(fileName = "AudioSystemSetting", menuName = "系统配置文件/AudioSystemSetting")]
        public class AudioSystemSetting : ScriptableObject
        {
            [Header("声音通道")]
            public AudioMixer mainMixer;
            public AudioMixerGroup musicGroup;
            public AudioMixerGroup soundGroup;

            [Header("音乐文件")]
            public List<AudioClip> musicClips;
            [Header("音效文件")]
            public SoundClipMap soundClips;
        }
    }
}