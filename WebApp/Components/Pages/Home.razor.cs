using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using MudBlazor.Utilities;
using WebApp.Models;

namespace WebApp.Components.Pages;

public partial class Home
{
    private MudTheme _theme = new MudTheme();
    [BindProperty] public IEnumerable<GradeModel>? Grades { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var client = HttpClientFactory.CreateClient("WebApp.ServerAPI");
            var response = await client.GetAsync("/users/1/grades");
            if (response.IsSuccessStatusCode)
            {
                Grades = await client.GetFromJsonAsync<IEnumerable<GradeModel>>("/users/1/grades");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private decimal SubjectAverageGrade(IGrouping<Subject, GradeModel> subjectGroup)
    {
        var totalWeightedGrades = subjectGroup.Sum(g => g.grade * g.percentageInfluence);
        var totalPercentageInfluence = subjectGroup.Sum(g => g.percentageInfluence);
        return Math.Round(totalWeightedGrades / totalPercentageInfluence, 2);
    }
    
    private MudColor GetGradeColor(decimal grade)
    {
        if (grade >= 6)
        {
            return _theme.PaletteLight.Success;
        }
        else if (grade >= 5.5m && grade < 6)
        {
            return _theme.PaletteLight.Warning;
        }
        else
        {
            return _theme.PaletteLight.Error;
        }
    }
}