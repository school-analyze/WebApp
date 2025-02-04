using Microsoft.AspNetCore.Mvc;
using MudBlazor;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Components.Pages;

public partial class Home
{
    [BindProperty] private IEnumerable<GradeModel>? Grades { get; set; }
    private readonly MudTheme _theme = new();
    
    private GradeModel _gradeBackup = null!;
    private GradeModel? _selectedItem;
    private DateTime? _newItemDate;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Grades = await RequestService.GetGrades(1, HttpClientFactory);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task RemoveGradeAsync(int id)
    {
        if (await RequestService.RemoveGradeAsync(id, HttpClientFactory))
        {
            ReloadPage();
            Snackbar.Add("Grade Removed", Severity.Success);
        }
        else
        {
            Snackbar.Add("Grade not removed", Severity.Error);
        }
    }

    private async Task OpenDialogAsync(GradeModel grade)
    {
        var parameters = new DialogParameters<AddGradeDialog> { { x => x.Grade, grade } };

        var dialog = await DialogService.ShowAsync<AddGradeDialog>("Add Grade", parameters);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            ReloadPage();
        }
    }
    private void ReloadPage() => NavigationManager.NavigateTo(NavigationManager.Uri, true);
    
    private void BackupItem(object grade)
    {
        if (grade is not GradeModel current) return;
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
    private static DateTime? ConvertDate(DateOnly date) => date.ToDateTime(TimeOnly.MinValue);

    private void CommitEdit(object grade)
    {
        if (grade is not GradeModel current) return;
        GradeModel gradeToCommit = new()
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
                Snackbar.Add("Grade Updated", Severity.Success);
                ReloadPage();
            }
            else
                Snackbar.Add("Grade not updated", Severity.Error);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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