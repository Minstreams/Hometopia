using System;
using UnityEngine;

/// <summary>
/// 满足特定条件时才显示
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ConditionalShowAttribute : PropertyAttribute
{
    //条件字段，必须是整型
    public string ConditionalIntField = "";
    //预期值
    public int ExpectedValue;

    public ConditionalShowAttribute(string conditionalIntField, int expectedValue)
    {
        this.ConditionalIntField = conditionalIntField;
        this.ExpectedValue = expectedValue;
    }
    public ConditionalShowAttribute(string conditionalIntField, object expectedValue)
    {
        this.ConditionalIntField = conditionalIntField;
        this.ExpectedValue = (int)expectedValue;
    }
}