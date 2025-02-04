using System.Globalization;
using MudBlazor;
using MudBlazor.Utilities;
using WebApp.Models;

namespace WebApp.Services;

public static class GradesUtils
{
    public static decimal SubjectAverageGrade(IGrouping<Subject, GradeModel> subjectGroup)
    {
        var totalWeightedGrades = subjectGroup.Sum(g => g.grade * g.percentageInfluence);
        var totalPercentageInfluence = subjectGroup.Sum(g => g.percentageInfluence);
        return Math.Round(totalWeightedGrades / totalPercentageInfluence, 2);
    }
    public static MudColor GetGradeColor(decimal grade, MudTheme theme)
    {
        return grade switch
        {
            >= 6 => theme.PaletteLight.Success,
            >= 5.5m and < 6 => theme.PaletteLight.Warning,
            _ => theme.PaletteLight.Error
        };
    }
    public static string FormatGrade(decimal grade)
    {
        return (grade % 1) switch
        {
            0.85m => $"{Math.Floor(grade + 1)}-",
            0.15m => $"{Math.Floor(grade)}+",
            0.5m => $"{Math.Floor(grade)}\u00bd",
            0 => $"{Math.Floor(grade)}",
            _ => grade.ToString(CultureInfo.InvariantCulture)
        };
    }
}