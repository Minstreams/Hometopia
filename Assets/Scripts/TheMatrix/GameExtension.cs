using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameSystem
{
    #region 事件类型定义
    [System.Serializable]
    public class SimpleEvent : UnityEvent { }
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [System.Serializable]
    public class Vec2Event : UnityEvent<Vector2> { }
    [System.Serializable]
    public class Vec3Event : UnityEvent<Vector3> { }
    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    [System.Serializable]
    public class ColorEvent : UnityEvent<Color> { }
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    [System.Serializable]
    public class AudioConfigEvent : UnityEvent<AudioSystem.AudioConfig> { }
    #endregion


    #region EnumMap Class Definition
    [System.Serializable]
    public class GameSceneMap : EnumMap<TheMatrix.GameScene, string> { }
    [System.Serializable]
    public class SoundClipMap : EnumMap<AudioSystem.Sound, AudioClip> { }
    [System.Serializable]
    public class InputKeyMap : EnumMap<InputSystem.InputKey, KeyCode> { }
    #endregion

}