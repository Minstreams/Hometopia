using System;
using UnityEngine;

/// <summary>
/// 满足特定条件时才显示，为UnityEvent类型量身定制
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ConditionalShow4UEventAttribute : PropertyAttribute
{
    //条件字段，枚举或整型
    public string ConditionalIntField = "";
    //预期值
    public int[] ExpectedValues;

    public ConditionalShow4UEventAttribute(string conditionalIntField, object expectedValue)
    {
        this.ConditionalIntField = conditionalIntField;
        this.ExpectedValues = new int[] { (int)expectedValue };
    }
    public ConditionalShow4UEventAttribute(string conditionalIntField, params object[] expectedValues)
    {
        this.ConditionalIntField = conditionalIntField;
        this.ExpectedValues = new int[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++) this.ExpectedValues[i] = (int)expectedValues[i];
    }
}