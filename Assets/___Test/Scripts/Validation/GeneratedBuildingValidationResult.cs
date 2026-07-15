using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum GeneratedBuildingValidationSeverity
{
    Error,
    Warning
}

public readonly struct GeneratedBuildingValidationIssue
{
    public readonly GeneratedBuildingValidationSeverity Severity;
    public readonly string Message;

    public GeneratedBuildingValidationIssue(GeneratedBuildingValidationSeverity severity, string message)
    {
        Severity = severity;
        Message = message;
    }
}

public class GeneratedBuildingValidationResult
{
    private readonly List<GeneratedBuildingValidationIssue> issues = new List<GeneratedBuildingValidationIssue>();

    public IReadOnlyList<GeneratedBuildingValidationIssue> Issues => issues;
    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }
    public bool IsValid => ErrorCount == 0;

    public void AddError(string message)
    {
        issues.Add(new GeneratedBuildingValidationIssue(GeneratedBuildingValidationSeverity.Error, message));
        ErrorCount++;
    }

    public void AddWarning(string message)
    {
        issues.Add(new GeneratedBuildingValidationIssue(GeneratedBuildingValidationSeverity.Warning, message));
        WarningCount++;
    }

    public string BuildSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(IsValid
            ? "Generated building validation passed."
            : "Generated building validation failed.");
        builder.AppendLine($"- Errors: {ErrorCount}");
        builder.AppendLine($"- Warnings: {WarningCount}");

        for (int i = 0; i < issues.Count; i++)
        {
            GeneratedBuildingValidationIssue issue = issues[i];
            builder.AppendLine($"  [{issue.Severity}] {issue.Message}");
        }

        return builder.ToString();
    }

    public void Log(Object context)
    {
        string summary = BuildSummary();

        if (ErrorCount > 0)
        {
            Debug.LogError(summary, context);
            return;
        }

        if (WarningCount > 0)
        {
            Debug.LogWarning(summary, context);
            return;
        }

        Debug.Log(summary, context);
    }
}
