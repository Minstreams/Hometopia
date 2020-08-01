using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 与枚举挂钩的Map，本质是List，通过Editor扩展实现与枚举挂钩
/// </summary>
/// <typeparam name="ET">Enum Type 关联的枚举类型</typeparam>
/// <typeparam name="DT">Data Type 数据类型</typeparam>
[System.Serializable]
public class EnumMap<ET, DT> where ET : System.Enum
{
    public List<DT> list = new List<DT>(System.Enum.GetNames(typeof(ET)).Length);

    public DT this[ET key]
    {
        get
        {
            return this.list[(int)(object)key];
        }
        set
        {
            this.list[(int)(object)key] = value;
        }
    }
}
