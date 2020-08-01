using System;
using UnityEngine;

/// <summary>
/// 标题/注释/小节等，并可以指定Style
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class MinsHeaderAttribute : PropertyAttribute
{
    //条件字段，必须是整型
    public string Summary = "";
    public string Style = "label";

    public MinsHeaderAttribute(string summary)
    {
        this.Summary = summary;
    }
    public MinsHeaderAttribute(string summary, string style)
    {
        this.Summary = summary;
        this.Style = style;
    }
    public MinsHeaderAttribute(string summary, SummaryType summaryType)
    {
        this.Summary = summary;
        this.Style = getStyle(summaryType);
    }
    private string getStyle(SummaryType summaryType)
    {
        switch (summaryType)
        {
            case SummaryType.Title: return "WarningOverlay";
            case SummaryType.Header: return "LODRendererRemove";
            case SummaryType.Comment: return "HelpBox";
            case SummaryType.CommentRight: return "flow varPin out";
        }
        return "label";
    }
}
/// <summary>
/// 注释类型
/// </summary>
public enum SummaryType
{
    /// <summary>
    /// 标题
    /// </summary>
    Title,
    /// <summary>
    /// 小节标题
    /// </summary>
    Header,
    /// <summary>
    /// 注释
    /// </summary>
    Comment,
    /// <summary>
    /// 注释,右对齐
    /// </summary>
    CommentRight
}