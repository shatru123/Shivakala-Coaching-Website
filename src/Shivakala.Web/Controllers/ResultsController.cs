using Microsoft.AspNetCore.Mvc;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Repositories;

namespace Shivakala.Web.Controllers;

public sealed class ResultsController(ITestResultRepository resultRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? testTitle, string? standard, CancellationToken ct)
    {
        var titles = await resultRepo.GetTestTitlesAsync(ct);
        var selectedTitle = testTitle ?? titles.FirstOrDefault();
        var results = selectedTitle != null
            ? await resultRepo.GetByTestAsync(selectedTitle, standard, ct)
            : [];

        var vm = new ResultsPageViewModel
        {
            Results = results.Select(r => new TestResultViewModel {
                Id=r.Id, StudentName=r.StudentName, Standard=r.Standard, Subject=r.Subject,
                Score=r.Score, TotalMarks=r.TotalMarks, Rank=r.Rank, Grade=r.Grade,
                Remarks=r.Remarks, TestDate=r.TestDate, TestTitle=r.TestTitle
            }).ToList(),
            AvailableTests = titles,
            SelectedTest = selectedTitle,
            SelectedStandard = standard,
            Seo = new() { Title = "Weekly Test Results | Shivakala Coaching Classes", Description = "View weekly test results and merit rank list for all standards." }
        };
        return View(vm);
    }
}
