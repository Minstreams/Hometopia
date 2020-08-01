using System;
using UnityEngine;

/// <summary>
/// 代替原变量标签
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LabelAttribute : PropertyAttribute
{
    /// <summary>
    /// 标签内容
    /// </summary>
    public string Label = "";
    /// <summary>
    /// 常量在游戏中不允许改变
    /// </summary>
    public bool Const = false;

    public LabelAttribute(string label)
    {
        this.Label = label;
    }
    public LabelAttribute(string label, bool isConst)
    {
        this.Label = label;
        this.Const = isConst;
    }
}