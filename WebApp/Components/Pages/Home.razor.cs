using System.Text;
using System.Text.Json;
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

    private static void ReloadPage(NavigationManager manager)
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

    private GradeModel _gradeBackup = new GradeModel();
    private GradeModel selectedItem;

    private void BackupItem(object grade)
    {
        if (grade is GradeModel current)
        {
            _newItemDate = ConvertDate(current.dateAdded);
            _gradeBackup = new GradeModel
            {
                id = current.id,
                userId = current.userId,
                subject = current.subject,
                grade = current.grade,
                percentageInfluence = current.percentageInfluence,
                dateAdded = current.dateAdded
            };
        }
    }

    private DateTime? _newItemDate;
    private int _newItemPercentageInfluence;
    private decimal _newItemGrade;
    private DateTime? ConvertDate(DateOnly date) => date.ToDateTime(TimeOnly.Parse("00:00:00"));

    private void CommitEdit(object grade)
    {
        if (grade is GradeModel current)
        {
            Console.WriteLine(current.grade);
            Console.WriteLine(current.percentageInfluence);
            Console.WriteLine(current.dateAdded);
            Console.WriteLine(current.id);
            GradeModel gradeToCommit = new GradeModel()
            {
                id = current.id,
                userId = current.userId,
                subject = current.subject,
                dateAdded = DateOnly.FromDateTime(_newItemDate ?? DateTime.Now),
                percentageInfluence = current.percentageInfluence,
                grade = current.grade,
            };
            try
            {
                var client = HttpClientFactory.CreateClient("WebApp.ServerAPI");
                var response = client.PutAsJsonAsync($"/grades/update/{gradeToCommit.id}", gradeToCommit);
                if (response.Result.IsSuccessStatusCode)
                {
                    ReloadPage(NavigationManager);
                    Snackbar.Add("Grade Added", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Grade not added", Severity.Error);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void CancelEdit(object grade)
    {
        if (grade is GradeModel current)
        {
            current.grade = _gradeBackup.grade;
            current.percentageInfluence = _gradeBackup.percentageInfluence;
            current.dateAdded = _gradeBackup.dateAdded;
        }
    }
}