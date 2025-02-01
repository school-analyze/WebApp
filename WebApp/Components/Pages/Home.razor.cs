using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using MudBlazor.Utilities;
using WebApp.Models;

namespace WebApp.Components.Pages;

public partial class Home
{
    private MudTheme _theme = new MudTheme();
    private bool _open = false;
    [BindProperty] public IEnumerable<GradeModel>? Grades { get; set; }
    
    private void ToggleOpen() => _open = !_open;

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
        return grade switch
        {
            >= 6 => _theme.PaletteLight.Success,
            >= 5.5m and < 6 => _theme.PaletteLight.Warning,
            _ => _theme.PaletteLight.Error
        };
    }

    private string FormatGrade(decimal grade)
    {
        if (grade % 1 == 0.85m)
            return $"{Math.Floor(grade + 1)}-";
        if (grade % 1 == 0.15m)
            return $"{Math.Floor(grade)}+";
        if (grade % 1 == 0.5m)
            return $"{Math.Floor(grade)}\u00bd";
        if (grade % 1 == 0)
            return $"{Math.Floor(grade)}";
        return grade.ToString();
    }
    
    private async Task OpenDialogAsync(GradeModel grade)
    {
        var parameters = new DialogParameters<AddGradeDialog> { { x => x.Grade, grade } };

        var dialog = await DialogService.ShowAsync<AddGradeDialog>("Add Grade", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            ReloadPage(NavigationManager);
        }
    }
    public static void ReloadPage(NavigationManager manager)
    {
        manager.NavigateTo(manager.Uri, true);
    }
    
    private async Task RemoveGradeAsync(int id)
    {
        var client = HttpClientFactory.CreateClient("WebApp.ServerAPI");
        var response = await client.DeleteAsync($"/grades/delete/{id}");
        if (response.IsSuccessStatusCode)
        {
            Grades = Grades?.Where(g => g.id != id);
        }
    }
}