using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Common;
using Shivakala.Core.Entities;
using Shivakala.Core.Interfaces;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;

namespace Shivakala.Infrastructure.Services;

public sealed class OnlineExamService(
    ShivakalaDbContext db,
    IExamRepository examRepo,
    IAuditService audit,
    ILogger<OnlineExamService> logger) : IOnlineExamService
{
    // ── Admin / Teacher Wizard & Setup ─────────────────────────────────────

    public async Task<OnlineExamWizardViewModel?> GetWizardViewModelAsync(int? examId, CancellationToken ct = default)
    {
        if (!examId.HasValue || examId.Value == 0)
        {
            return new OnlineExamWizardViewModel
            {
                DurationMinutes = 30,
                PassingMarks = 35,
                ResultReleaseMode = "Immediate",
                ShowCorrectAnswers = true,
                IsRegistrationRequired = true,
                StartDateTime = UtcDateTime.StartOfToday().AddHours(9),
                EndDateTime = UtcDateTime.StartOfToday().AddDays(1).AddHours(18)
            };
        }

        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
            .FirstOrDefaultAsync(e => e.Id == examId.Value, ct);

        if (exam is null) return null;

        return new OnlineExamWizardViewModel
        {
            Id = exam.Id,
            Title = exam.Title,
            Standard = exam.Standard,
            Subject = exam.Subject,
            BatchId = exam.BatchId,
            ExamType = exam.ExamType,
            Description = exam.Description,
            Instructions = exam.Instructions,
            DurationMinutes = exam.DurationMinutes > 0 ? exam.DurationMinutes : 30,
            RegistrationOpenDate = exam.RegistrationOpenDate,
            RegistrationCloseDate = exam.RegistrationCloseDate,
            StartDateTime = exam.StartDateTime ?? exam.ExamDate,
            EndDateTime = exam.EndDateTime,
            PassingMarks = exam.PassingMarks,
            NegativeMarkingEnabled = exam.NegativeMarkingEnabled,
            NegativeMarks = exam.NegativeMarks,
            RandomizeQuestions = exam.RandomizeQuestions,
            RandomizeOptions = exam.RandomizeOptions,
            AllowMultipleAttempts = exam.AllowMultipleAttempts,
            MaximumAttempts = exam.MaximumAttempts,
            ResultReleaseMode = exam.ResultReleaseMode,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            IsRegistrationRequired = exam.IsRegistrationRequired,
            IsPublished = exam.IsPublished,
            Audience = exam.Audience,
            Venue = exam.Venue,
            Capacity = exam.Capacity,
            SelectedQuestionIds = exam.ExamQuestions.OrderBy(eq => eq.DisplayOrder).Select(eq => eq.QuestionId).ToList()
        };
    }

    public async Task<int> SaveOnlineExamWizardAsync(OnlineExamWizardViewModel model, CancellationToken ct = default)
    {
        Exam exam;
        if (model.Id == 0)
        {
            exam = new Exam
            {
                Title = model.Title.Trim(),
                Standard = model.Standard.Trim(),
                Subject = model.Subject.Trim(),
                BatchId = model.BatchId,
                ExamType = string.IsNullOrWhiteSpace(model.ExamType) ? "Weekly" : model.ExamType,
                ExamMode = "Online",
                Description = model.Description,
                Instructions = model.Instructions,
                DurationMinutes = model.DurationMinutes > 0 ? model.DurationMinutes : 30,
                Duration = $"{model.DurationMinutes} mins",
                RegistrationOpenDate = model.RegistrationOpenDate,
                RegistrationCloseDate = model.RegistrationCloseDate,
                StartDateTime = model.StartDateTime ?? UtcDateTime.StartOfToday(),
                EndDateTime = model.EndDateTime,
                ExamDate = model.StartDateTime ?? UtcDateTime.StartOfToday(),
                PassingMarks = model.PassingMarks,
                NegativeMarkingEnabled = model.NegativeMarkingEnabled,
                NegativeMarks = model.NegativeMarks,
                RandomizeQuestions = model.RandomizeQuestions,
                RandomizeOptions = model.RandomizeOptions,
                AllowMultipleAttempts = model.AllowMultipleAttempts,
                MaximumAttempts = model.MaximumAttempts > 0 ? model.MaximumAttempts : 1,
                ResultReleaseMode = string.IsNullOrWhiteSpace(model.ResultReleaseMode) ? "Immediate" : model.ResultReleaseMode,
                ShowCorrectAnswers = model.ShowCorrectAnswers,
                IsRegistrationRequired = model.IsRegistrationRequired,
                IsPublished = model.IsPublished,
                Audience = string.IsNullOrWhiteSpace(model.Audience) ? "School" : model.Audience,
                Venue = model.Venue,
                Capacity = model.Capacity,
                CreatedDate = DateTime.UtcNow
            };
            db.Exams.Add(exam);
        }
        else
        {
            exam = await db.Exams.FindAsync([model.Id], ct)
                ?? throw new InvalidOperationException($"Exam #{model.Id} not found.");

            exam.Title = model.Title.Trim();
            exam.Standard = model.Standard.Trim();
            exam.Subject = model.Subject.Trim();
            exam.BatchId = model.BatchId;
            exam.ExamType = model.ExamType;
            exam.ExamMode = "Online";
            exam.Description = model.Description;
            exam.Instructions = model.Instructions;
            exam.DurationMinutes = model.DurationMinutes;
            exam.Duration = $"{model.DurationMinutes} mins";
            exam.RegistrationOpenDate = model.RegistrationOpenDate;
            exam.RegistrationCloseDate = model.RegistrationCloseDate;
            exam.StartDateTime = model.StartDateTime;
            exam.EndDateTime = model.EndDateTime;
            exam.ExamDate = model.StartDateTime ?? exam.ExamDate;
            exam.PassingMarks = model.PassingMarks;
            exam.NegativeMarkingEnabled = model.NegativeMarkingEnabled;
            exam.NegativeMarks = model.NegativeMarks;
            exam.RandomizeQuestions = model.RandomizeQuestions;
            exam.RandomizeOptions = model.RandomizeOptions;
            exam.AllowMultipleAttempts = model.AllowMultipleAttempts;
            exam.MaximumAttempts = model.MaximumAttempts;
            exam.ResultReleaseMode = model.ResultReleaseMode;
            exam.ShowCorrectAnswers = model.ShowCorrectAnswers;
            exam.IsRegistrationRequired = model.IsRegistrationRequired;
            exam.IsPublished = model.IsPublished;
            exam.Audience = string.IsNullOrWhiteSpace(model.Audience) ? "School" : model.Audience;
            exam.Venue = model.Venue;
            exam.Capacity = model.Capacity;
        }

        await db.SaveChangesAsync(ct);

        // Update ExamQuestions mapping
        var currentEqs = await db.ExamQuestions.Where(eq => eq.ExamId == exam.Id).ToListAsync(ct);
        db.ExamQuestions.RemoveRange(currentEqs);

        int order = 1;
        foreach (var qId in model.SelectedQuestionIds.Distinct())
        {
            db.ExamQuestions.Add(new ExamQuestion
            {
                ExamId = exam.Id,
                QuestionId = qId,
                DisplayOrder = order++
            });
        }

        await db.SaveChangesAsync(ct);

        // Recalculate QuestionCount and TotalMarks
        exam.QuestionCount = await db.ExamQuestions.CountAsync(eq => eq.ExamId == exam.Id, ct);
        var totalMarks = await db.ExamQuestions
            .Where(eq => eq.ExamId == exam.Id)
            .SumAsync(eq => eq.MarksOverride ?? eq.Question!.Marks, ct);
        exam.TotalMarks = totalMarks > 0 ? totalMarks : exam.QuestionCount * 1;

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Created", "OnlineExam", exam.Id, null, exam.Title, username: null, ip: null, ct: ct);

        return exam.Id;
    }

    public async Task TogglePublishAsync(int examId, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is not null)
        {
            exam.IsPublished = !exam.IsPublished;
            await db.SaveChangesAsync(ct);
            await audit.LogAsync("Updated", "OnlineExam", examId, null, $"Publish state: {exam.IsPublished}", username: null, ip: null, ct: ct);
        }
    }

    // ── Question Bank ────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Question>> SearchQuestionBankAsync(
        string? search = null,
        string? subject = null,
        string? standard = null,
        string? topic = null,
        string? difficulty = null,
        CancellationToken ct = default)
    {
        var query = db.Questions.Include(q => q.Options).Where(q => q.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(q => q.QuestionText.ToLower().Contains(s) || (q.Topic != null && q.Topic.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(subject))
            query = query.Where(q => q.Subject == subject);

        if (!string.IsNullOrWhiteSpace(standard))
            query = query.Where(q => q.Standard == standard);

        if (!string.IsNullOrWhiteSpace(topic))
            query = query.Where(q => q.Topic == topic);

        if (!string.IsNullOrWhiteSpace(difficulty))
            query = query.Where(q => q.Difficulty == difficulty);

        return await query.OrderByDescending(q => q.Id).ToListAsync(ct);
    }

    public async Task<Question> AddQuestionToBankAsync(Question question, CancellationToken ct = default)
    {
        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);
        return question;
    }

    public async Task<bool> AddQuestionsToExamAsync(int examId, IEnumerable<int> questionIds, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is null) return false;

        var existingMax = await db.ExamQuestions
            .Where(eq => eq.ExamId == examId)
            .Select(eq => (int?)eq.DisplayOrder)
            .MaxAsync(ct) ?? 0;

        int order = existingMax + 1;
        foreach (var qId in questionIds.Distinct())
        {
            var exists = await db.ExamQuestions.AnyAsync(eq => eq.ExamId == examId && eq.QuestionId == qId, ct);
            if (!exists)
            {
                db.ExamQuestions.Add(new ExamQuestion
                {
                    ExamId = examId,
                    QuestionId = qId,
                    DisplayOrder = order++
                });
            }
        }

        await db.SaveChangesAsync(ct);

        exam.QuestionCount = await db.ExamQuestions.CountAsync(eq => eq.ExamId == examId, ct);
        var totalMarks = await db.ExamQuestions
            .Where(eq => eq.ExamId == examId)
            .SumAsync(eq => eq.MarksOverride ?? eq.Question!.Marks, ct);
        exam.TotalMarks = totalMarks > 0 ? totalMarks : exam.QuestionCount * 1;
        await db.SaveChangesAsync(ct);

        return true;
    }

    // ── Student Portal & Engine ──────────────────────────────────────────────

    public async Task<IReadOnlyList<OnlineExamStudentListViewModel>> GetStudentExamsAsync(int studentId, CancellationToken ct = default)
    {
        var student = await db.Students
            .Include(s => s.StudentBatches)
            .FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null) return Array.Empty<OnlineExamStudentListViewModel>();

        var studentBatchIds = student.StudentBatches.Where(sb => sb.IsActive).Select(sb => sb.BatchId).ToList();

        var exams = await db.Exams
            .Where(e => e.IsPublished && e.ExamMode == "Online")
            .Where(e => e.Standard == student.Standard || (e.BatchId.HasValue && studentBatchIds.Contains(e.BatchId.Value)) || e.BatchId == null)
            .OrderByDescending(e => e.StartDateTime ?? e.ExamDate)
            .ToListAsync(ct);

        var registrations = await db.ExamRegistrations
            .Where(r => r.StudentId == studentId)
            .ToDictionaryAsync(r => r.ExamId, ct);

        var attempts = await db.OnlineExamAttempts
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(ct);

        var resultList = new List<OnlineExamStudentListViewModel>();
        var now = DateTime.UtcNow;

        foreach (var e in exams)
        {
            var reg = registrations.GetValueOrDefault(e.Id);
            var userAttempts = attempts.Where(a => a.ExamId == e.Id).ToList();
            var latestAttempt = userAttempts.FirstOrDefault();

            var isRegistered = reg != null && reg.Status != "Cancelled";
            var start = e.StartDateTime ?? e.ExamDate;
            var end = e.EndDateTime ?? start.AddDays(7);

            var isUpcoming = now < start;
            var isAvailableNow = now >= start && now <= end && (!e.IsRegistrationRequired || isRegistered);
            var isCompleted = userAttempts.Any(a => a.Status is "Submitted" or "AutoSubmitted") || (latestAttempt != null && now > latestAttempt.ExpiresAt);

            resultList.Add(new OnlineExamStudentListViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Standard = e.Standard,
                Subject = e.Subject,
                ExamType = e.ExamType,
                TotalMarks = e.TotalMarks,
                QuestionCount = e.QuestionCount,
                DurationMinutes = e.DurationMinutes > 0 ? e.DurationMinutes : 30,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                IsRegistrationRequired = e.IsRegistrationRequired,
                IsRegistered = isRegistered,
                IsAvailableNow = isAvailableNow && !isCompleted,
                IsUpcoming = isUpcoming,
                IsCompleted = isCompleted,
                UserAttemptCount = userAttempts.Count,
                MaxAttempts = e.MaximumAttempts > 0 ? e.MaximumAttempts : 1,
                LatestScore = latestAttempt?.Score,
                LatestPercentage = latestAttempt?.Percentage,
                LatestGrade = CalculateGrade(latestAttempt?.Percentage ?? 0),
                LatestAttemptId = latestAttempt?.Id
            });
        }

        return resultList;
    }

    public async Task<bool> RegisterStudentForExamAsync(int examId, int studentId, CancellationToken ct = default)
    {
        var exam = await db.Exams.FindAsync([examId], ct);
        if (exam is null || !exam.IsPublished || exam.ExamMode != "Online")
            return false;

        var existing = await db.ExamRegistrations
            .FirstOrDefaultAsync(r => r.ExamId == examId && r.StudentId == studentId, ct);

        if (existing is not null)
        {
            existing.Status = "Registered";
            await db.SaveChangesAsync(ct);
            return true;
        }

        db.ExamRegistrations.Add(new ExamRegistration
        {
            ExamId = examId,
            StudentId = studentId,
            RegisteredAt = DateTime.UtcNow,
            Status = "Registered"
        });

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Created", "ExamRegistration", examId, null, $"Student #{studentId} registered", username: null, ip: null, ct: ct);
        return true;
    }

    public async Task<(bool Success, string Message, OnlineExamAttemptViewModel? Attempt)> StartOrResumeAttemptAsync(
        int examId, int studentId, CancellationToken ct = default)
    {
        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(e => e.Id == examId, ct);

        if (exam is null || !exam.IsPublished || exam.ExamMode != "Online")
            return (false, "Exam is not available.", null);

        var now = DateTime.UtcNow;
        var start = exam.StartDateTime ?? exam.ExamDate;
        var end = exam.EndDateTime ?? start.AddDays(7);

        if (now < start)
            return (false, "Exam has not started yet.", null);

        if (now > end)
            return (false, "Exam period has expired.", null);

        if (exam.IsRegistrationRequired)
        {
            var isReg = await db.ExamRegistrations
                .AnyAsync(r => r.ExamId == examId && r.StudentId == studentId && r.Status != "Cancelled", ct);
            if (!isReg)
                return (false, "Registration is required before attempting this exam.", null);
        }

        // Check for existing in-progress attempt
        var existingAttempt = await db.OnlineExamAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId && a.Status == "InProgress", ct);

        if (existingAttempt is not null)
        {
            if (now >= existingAttempt.ExpiresAt)
            {
                // Auto-submit expired attempt
                await SubmitAttemptAsync(existingAttempt.Id, studentId, isAutoSubmit: true, ct: ct);
                return (false, "Your exam time has expired and the attempt was automatically submitted.", null);
            }

            // Resume attempt
            var attemptVm = BuildAttemptViewModel(exam, existingAttempt, now);
            return (true, "Resumed in-progress attempt.", attemptVm);
        }

        // Verify max attempts limit
        var previousAttemptsCount = await db.OnlineExamAttempts
            .CountAsync(a => a.ExamId == examId && a.StudentId == studentId, ct);

        var maxAttempts = exam.MaximumAttempts > 0 ? exam.MaximumAttempts : 1;
        if (!exam.AllowMultipleAttempts && previousAttemptsCount >= maxAttempts)
        {
            return (false, "You have reached the maximum allowed attempts for this exam.", null);
        }

        // Create new attempt session with server-authoritative expiration
        var durationMins = exam.DurationMinutes > 0 ? exam.DurationMinutes : 30;
        var attempt = new OnlineExamAttempt
        {
            ExamId = examId,
            StudentId = studentId,
            AttemptNumber = previousAttemptsCount + 1,
            StartedAt = now,
            ExpiresAt = now.AddMinutes(durationMins),
            Status = "InProgress",
            TotalMarks = exam.TotalMarks,
            CreatedDate = now
        };

        db.OnlineExamAttempts.Add(attempt);
        await db.SaveChangesAsync(ct);

        // Pre-create attempt answer placeholders for all exam questions
        var examQuestions = exam.ExamQuestions.OrderBy(eq => eq.DisplayOrder).ToList();
        foreach (var eq in examQuestions)
        {
            db.OnlineExamAttemptAnswers.Add(new OnlineExamAttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = eq.QuestionId,
                IsMarkedForReview = false,
                AnsweredAt = now
            });
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Created", "OnlineExamAttempt", attempt.Id, null, $"Attempt started for student #{studentId}", username: null, ip: null, ct: ct);

        var newAttemptVm = BuildAttemptViewModel(exam, attempt, now);
        return (true, "Attempt started successfully.", newAttemptVm);
    }

    public async Task<(bool Success, string Message)> AutoSaveAnswerAsync(
        SaveAnswerRequestModel model, int studentId, CancellationToken ct = default)
    {
        var attempt = await db.OnlineExamAttempts.FindAsync([model.AttemptId], ct);
        if (attempt is null || attempt.StudentId != studentId || attempt.Status != "InProgress")
            return (false, "Invalid attempt session.");

        if (DateTime.UtcNow >= attempt.ExpiresAt)
        {
            await SubmitAttemptAsync(attempt.Id, studentId, isAutoSubmit: true, ct: ct);
            return (false, "Exam time expired.");
        }

        var answer = await db.OnlineExamAttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId && a.QuestionId == model.QuestionId, ct);

        if (answer is null)
        {
            db.OnlineExamAttemptAnswers.Add(new OnlineExamAttemptAnswer
            {
                AttemptId = model.AttemptId,
                QuestionId = model.QuestionId,
                SelectedOptionId = model.SelectedOptionId,
                IsMarkedForReview = model.IsMarkedForReview,
                AnsweredAt = DateTime.UtcNow
            });
        }
        else
        {
            answer.SelectedOptionId = model.SelectedOptionId;
            answer.IsMarkedForReview = model.IsMarkedForReview;
            answer.AnsweredAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return (true, "Saved");
    }

    public async Task<(bool Success, string Message, int? AttemptId)> SubmitAttemptAsync(
        int attemptId, int studentId, bool isAutoSubmit = false, CancellationToken ct = default)
    {
        var attempt = await db.OnlineExamAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null) return (false, "Attempt session not found.", null);
        if (attempt.StudentId != studentId) return (false, "Unauthorized attempt access.", null);

        if (attempt.Status is "Submitted" or "AutoSubmitted")
            return (true, "Attempt already submitted.", attempt.Id);

        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(e => e.Id == attempt.ExamId, ct);

        if (exam is null) return (false, "Exam not found.", null);

        var now = DateTime.UtcNow;
        var answersMap = attempt.Answers.ToDictionary(a => a.QuestionId);

        double totalScore = 0;
        int correctCount = 0;
        int incorrectCount = 0;
        int unansweredCount = 0;
        int totalMarks = 0;

        foreach (var eq in exam.ExamQuestions)
        {
            var q = eq.Question;
            if (q is null) continue;

            var qMarks = eq.MarksOverride ?? q.Marks;
            var negMarks = eq.NegativeMarksOverride ?? (exam.NegativeMarkingEnabled ? exam.NegativeMarks : q.NegativeMarks);
            totalMarks += qMarks;

            if (answersMap.TryGetValue(q.Id, out var ans) && ans.SelectedOptionId.HasValue)
            {
                var selectedOption = q.Options.FirstOrDefault(o => o.Id == ans.SelectedOptionId.Value);
                if (selectedOption?.IsCorrect == true)
                {
                    correctCount++;
                    totalScore += qMarks;
                }
                else
                {
                    incorrectCount++;
                    if (exam.NegativeMarkingEnabled || negMarks > 0)
                    {
                        totalScore -= negMarks;
                    }
                }
            }
            else
            {
                unansweredCount++;
            }
        }

        totalScore = Math.Max(0, totalScore);
        double percentage = totalMarks > 0 ? (totalScore / totalMarks) * 100 : 0;
        percentage = Math.Round(percentage, 2);

        attempt.Status = isAutoSubmit ? "AutoSubmitted" : "Submitted";
        attempt.SubmittedAt = now;
        attempt.Score = Math.Round(totalScore, 2);
        attempt.TotalMarks = totalMarks > 0 ? totalMarks : exam.TotalMarks;
        attempt.CorrectCount = correctCount;
        attempt.IncorrectCount = incorrectCount;
        attempt.UnansweredCount = unansweredCount;
        attempt.Percentage = percentage;
        attempt.TimeTakenSeconds = (int)(now - attempt.StartedAt).TotalSeconds;
        attempt.IsAutoSubmitted = isAutoSubmit;

        await db.SaveChangesAsync(ct);

        // Sync into standard ExamResult for ranking & portal reports
        var existingResult = await db.ExamResults
            .FirstOrDefaultAsync(r => r.ExamId == exam.Id && r.StudentId == studentId, ct);

        int marksObtained = (int)Math.Round(totalScore);
        var grade = CalculateGrade(percentage);

        if (existingResult is null)
        {
            db.ExamResults.Add(new ExamResult
            {
                ExamId = exam.Id,
                StudentId = studentId,
                MarksObtained = marksObtained,
                Grade = grade,
                IsAbsent = false,
                Remarks = $"Online Attempt #{attempt.AttemptNumber} ({attempt.Status})",
                CreatedDate = now
            });
        }
        else
        {
            if (!existingResult.MarksObtained.HasValue || marksObtained > existingResult.MarksObtained.Value)
            {
                existingResult.MarksObtained = marksObtained;
                existingResult.Grade = grade;
                existingResult.IsAbsent = false;
                existingResult.Remarks = $"Online Attempt #{attempt.AttemptNumber} ({attempt.Status})";
            }
        }

        await db.SaveChangesAsync(ct);
        await examRepo.RecalculateRanksAsync(exam.Id, ct);
        await audit.LogAsync("Updated", "OnlineExamAttempt", attempt.Id, null, $"Submitted score: {attempt.Score}/{attempt.TotalMarks}", username: null, ip: null, ct: ct);

        return (true, "Exam submitted successfully.", attempt.Id);
    }

    // ── MPSC Candidate Exam Engine ───────────────────────────────────────────

    public async Task<(bool Success, string Message, OnlineExamAttemptViewModel? Attempt)> StartOrResumeMpscAttemptAsync(
        int examId, int mpscStudentId, CancellationToken ct = default)
    {
        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(e => e.Id == examId, ct);

        if (exam is null || !exam.IsPublished || exam.ExamMode != "Online" || (exam.Audience != "Mpsc" && exam.Audience != "Both"))
            return (false, "Exam is not available for MPSC candidates.", null);

        var now = DateTime.UtcNow;
        var start = exam.StartDateTime ?? exam.ExamDate;
        var end = exam.EndDateTime ?? start.AddDays(7);

        if (now < start)
            return (false, "Exam has not started yet.", null);

        if (now > end)
            return (false, "Exam period has expired.", null);

        if (exam.IsRegistrationRequired)
        {
            var isReg = await db.MpscExamRegistrations
                .AnyAsync(r => r.ExamId == examId && r.MpscStudentId == mpscStudentId && r.Status != "Cancelled", ct);
            if (!isReg)
                return (false, "Registration is required before attempting this exam.", null);
        }

        // Check for existing in-progress attempt
        var existingAttempt = await db.OnlineExamAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.MpscStudentId == mpscStudentId && a.Status == "InProgress", ct);

        if (existingAttempt is not null)
        {
            if (now >= existingAttempt.ExpiresAt)
            {
                await SubmitMpscAttemptAsync(existingAttempt.Id, mpscStudentId, isAutoSubmit: true, ct: ct);
                return (false, "Your exam time has expired and the attempt was automatically submitted.", null);
            }

            var attemptVm = BuildAttemptViewModel(exam, existingAttempt, now);
            return (true, "Resumed in-progress attempt.", attemptVm);
        }

        // Verify max attempts limit
        var previousAttemptsCount = await db.OnlineExamAttempts
            .CountAsync(a => a.ExamId == examId && a.MpscStudentId == mpscStudentId, ct);

        var maxAttempts = exam.MaximumAttempts > 0 ? exam.MaximumAttempts : 1;
        if (!exam.AllowMultipleAttempts && previousAttemptsCount >= maxAttempts)
        {
            return (false, "You have reached the maximum allowed attempts for this exam.", null);
        }

        var durationMins = exam.DurationMinutes > 0 ? exam.DurationMinutes : 30;
        var attempt = new OnlineExamAttempt
        {
            ExamId = examId,
            MpscStudentId = mpscStudentId,
            AttemptNumber = previousAttemptsCount + 1,
            StartedAt = now,
            ExpiresAt = now.AddMinutes(durationMins),
            Status = "InProgress",
            TotalMarks = exam.TotalMarks,
            CreatedDate = now
        };

        db.OnlineExamAttempts.Add(attempt);
        await db.SaveChangesAsync(ct);

        var examQuestions = exam.ExamQuestions.OrderBy(eq => eq.DisplayOrder).ToList();
        foreach (var eq in examQuestions)
        {
            db.OnlineExamAttemptAnswers.Add(new OnlineExamAttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = eq.QuestionId,
                IsMarkedForReview = false,
                AnsweredAt = now
            });
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Created", "OnlineExamAttempt", attempt.Id, null, $"Attempt started for MPSC candidate #{mpscStudentId}", username: null, ip: null, ct: ct);

        var newAttemptVm = BuildAttemptViewModel(exam, attempt, now);
        return (true, "Attempt started successfully.", newAttemptVm);
    }

    public async Task<(bool Success, string Message)> AutoSaveMpscAnswerAsync(
        SaveAnswerRequestModel model, int mpscStudentId, CancellationToken ct = default)
    {
        var attempt = await db.OnlineExamAttempts.FindAsync([model.AttemptId], ct);
        if (attempt is null || attempt.MpscStudentId != mpscStudentId || attempt.Status != "InProgress")
            return (false, "Invalid attempt session.");

        if (DateTime.UtcNow >= attempt.ExpiresAt)
        {
            await SubmitMpscAttemptAsync(attempt.Id, mpscStudentId, isAutoSubmit: true, ct: ct);
            return (false, "Exam time expired.");
        }

        var answer = await db.OnlineExamAttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId && a.QuestionId == model.QuestionId, ct);

        if (answer is null)
        {
            db.OnlineExamAttemptAnswers.Add(new OnlineExamAttemptAnswer
            {
                AttemptId = model.AttemptId,
                QuestionId = model.QuestionId,
                SelectedOptionId = model.SelectedOptionId,
                IsMarkedForReview = model.IsMarkedForReview,
                AnsweredAt = DateTime.UtcNow
            });
        }
        else
        {
            answer.SelectedOptionId = model.SelectedOptionId;
            answer.IsMarkedForReview = model.IsMarkedForReview;
            answer.AnsweredAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return (true, "Saved");
    }

    public async Task<(bool Success, string Message, int? AttemptId)> SubmitMpscAttemptAsync(
        int attemptId, int mpscStudentId, bool isAutoSubmit = false, CancellationToken ct = default)
    {
        var attempt = await db.OnlineExamAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null) return (false, "Attempt session not found.", null);
        if (attempt.MpscStudentId != mpscStudentId) return (false, "Unauthorized attempt access.", null);

        if (attempt.Status is "Submitted" or "AutoSubmitted")
            return (true, "Attempt already submitted.", attempt.Id);

        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(e => e.Id == attempt.ExamId, ct);

        if (exam is null) return (false, "Exam not found.", null);

        var now = DateTime.UtcNow;
        var answersMap = attempt.Answers.ToDictionary(a => a.QuestionId);

        double totalScore = 0;
        int correctCount = 0;
        int incorrectCount = 0;
        int unansweredCount = 0;
        int totalMarks = 0;

        foreach (var eq in exam.ExamQuestions)
        {
            var q = eq.Question;
            if (q is null) continue;

            var qMarks = eq.MarksOverride ?? q.Marks;
            var negMarks = eq.NegativeMarksOverride ?? (exam.NegativeMarkingEnabled ? exam.NegativeMarks : q.NegativeMarks);
            totalMarks += qMarks;

            if (answersMap.TryGetValue(q.Id, out var ans) && ans.SelectedOptionId.HasValue)
            {
                var selectedOption = q.Options.FirstOrDefault(o => o.Id == ans.SelectedOptionId.Value);
                if (selectedOption?.IsCorrect == true)
                {
                    correctCount++;
                    totalScore += qMarks;
                }
                else
                {
                    incorrectCount++;
                    if (exam.NegativeMarkingEnabled || negMarks > 0)
                    {
                        totalScore -= negMarks;
                    }
                }
            }
            else
            {
                unansweredCount++;
            }
        }

        totalScore = Math.Max(0, totalScore);
        double percentage = totalMarks > 0 ? (totalScore / totalMarks) * 100 : 0;
        percentage = Math.Round(percentage, 2);

        attempt.Status = isAutoSubmit ? "AutoSubmitted" : "Submitted";
        attempt.SubmittedAt = now;
        attempt.Score = Math.Round(totalScore, 2);
        attempt.TotalMarks = totalMarks > 0 ? totalMarks : exam.TotalMarks;
        attempt.CorrectCount = correctCount;
        attempt.IncorrectCount = incorrectCount;
        attempt.UnansweredCount = unansweredCount;
        attempt.Percentage = percentage;
        attempt.TimeTakenSeconds = (int)(now - attempt.StartedAt).TotalSeconds;
        attempt.IsAutoSubmitted = isAutoSubmit;

        await db.SaveChangesAsync(ct);

        // Sync into MpscExamResult (strictly decoupled from school ExamResult)
        var existingResult = await db.MpscExamResults
            .FirstOrDefaultAsync(r => r.ExamId == exam.Id && r.MpscStudentId == mpscStudentId, ct);

        var grade = CalculateGrade(percentage);

        if (existingResult is null)
        {
            db.MpscExamResults.Add(new MpscExamResult
            {
                ExamId = exam.Id,
                MpscStudentId = mpscStudentId,
                MarksObtained = totalScore,
                Grade = grade,
                Mode = "Online",
                OnlineAttemptId = attempt.Id,
                CreatedDate = DateTime.UtcNow
            });
        }
        else
        {
            if (totalScore > (existingResult.MarksObtained ?? 0))
            {
                existingResult.MarksObtained = totalScore;
                existingResult.Grade = grade;
                existingResult.OnlineAttemptId = attempt.Id;
            }
        }

        await db.SaveChangesAsync(ct);
        await audit.LogAsync("Updated", "OnlineExamAttempt", attempt.Id, null, $"Submitted MPSC score: {attempt.Score}/{attempt.TotalMarks}", username: null, ip: null, ct: ct);

        return (true, "Exam submitted successfully.", attempt.Id);
    }

    public async Task<OnlineExamResultViewModel?> GetMpscAttemptResultAsync(
        int attemptId, int mpscStudentId, CancellationToken ct = default)
    {
        return await GetAttemptResultAsync(attemptId, mpscStudentId, isStudent: true, ct: ct);
    }

    // ── Results & Analytics ──────────────────────────────────────────────────

    public async Task<OnlineExamResultViewModel?> GetAttemptResultAsync(
        int attemptId, int currentUserId, bool isStudent = true, CancellationToken ct = default)
    {
        var attempt = await db.OnlineExamAttempts
            .Include(a => a.Student)
            .Include(a => a.MpscStudent)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null) return null;

        if (isStudent && attempt.StudentId != currentUserId && attempt.MpscStudentId != currentUserId)
            return null;

        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
                    .ThenInclude(q => q!.Options)
            .FirstOrDefaultAsync(e => e.Id == attempt.ExamId, ct);

        if (exam is null) return null;

        var now = DateTime.UtcNow;
        var isExamEnded = exam.EndDateTime.HasValue && now >= exam.EndDateTime.Value;
        var canViewReview = exam.ResultReleaseMode == "Immediate" ||
                            (exam.ResultReleaseMode == "AfterExamEnds" && isExamEnded) ||
                            (exam.ResultReleaseMode == "Manual" && exam.IsPublished);

        var vm = new OnlineExamResultViewModel
        {
            AttemptId = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            Subject = exam.Subject,
            Standard = exam.Standard,
            StudentName = attempt.MpscStudent?.FullName ?? attempt.Student?.FullName ?? "Candidate",
            Score = attempt.Score,
            TotalMarks = attempt.TotalMarks,
            Percentage = attempt.Percentage,
            Grade = CalculateGrade(attempt.Percentage),
            Passed = attempt.Score >= exam.PassingMarks,
            CorrectCount = attempt.CorrectCount,
            IncorrectCount = attempt.IncorrectCount,
            UnansweredCount = attempt.UnansweredCount,
            TimeTakenSeconds = attempt.TimeTakenSeconds,
            ResultReleaseMode = exam.ResultReleaseMode,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            CanViewReview = canViewReview
        };

        // Build Topic Analytics
        var answersMap = attempt.Answers.ToDictionary(a => a.QuestionId);
        var topicStats = new Dictionary<string, (int Total, int Correct)>();

        int qNum = 1;
        foreach (var eq in exam.ExamQuestions.OrderBy(x => x.DisplayOrder))
        {
            var q = eq.Question;
            if (q is null) continue;

            var topic = string.IsNullOrWhiteSpace(q.Topic) ? exam.Subject : q.Topic.Trim();
            if (!topicStats.ContainsKey(topic)) topicStats[topic] = (0, 0);

            var currentStat = topicStats[topic];
            currentStat.Total++;

            answersMap.TryGetValue(q.Id, out var ans);
            var isCorrect = ans?.SelectedOption?.IsCorrect == true;
            if (isCorrect) currentStat.Correct++;
            topicStats[topic] = currentStat;

            if (canViewReview)
            {
                var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect);
                vm.QuestionReviews.Add(new QuestionReviewItemViewModel
                {
                    QuestionNumber = qNum++,
                    QuestionText = q.QuestionText,
                    QuestionImageUrl = q.QuestionImageUrl,
                    Marks = eq.MarksOverride ?? q.Marks,
                    Explanation = q.Explanation,
                    StudentSelectedOptionKey = ans?.SelectedOption?.OptionKey,
                    StudentSelectedOptionText = ans?.SelectedOption?.OptionText,
                    CorrectOptionKey = exam.ShowCorrectAnswers ? (correctOpt?.OptionKey ?? "") : "-",
                    CorrectOptionText = exam.ShowCorrectAnswers ? (correctOpt?.OptionText ?? "") : "-",
                    IsCorrect = isCorrect,
                    IsUnanswered = ans?.SelectedOptionId == null,
                    Options = q.Options.OrderBy(o => o.DisplayOrder).Select(o => new StudentAttemptOptionViewModel
                    {
                        Id = o.Id,
                        OptionKey = o.OptionKey,
                        OptionText = o.OptionText,
                        OptionImageUrl = o.OptionImageUrl
                    }).ToList()
                });
            }
        }

        vm.TopicAnalytics = topicStats.Select(kvp => new TopicAccuracyViewModel
        {
            Topic = kvp.Key,
            Total = kvp.Value.Total,
            Correct = kvp.Value.Correct,
            AccuracyPercentage = kvp.Value.Total > 0 ? Math.Round((double)kvp.Value.Correct / kvp.Value.Total * 100, 1) : 0
        }).ToList();

        return vm;
    }

    public async Task<OnlineExamAnalyticsViewModel?> GetExamAnalyticsAsync(int examId, CancellationToken ct = default)
    {
        var exam = await db.Exams
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
            .FirstOrDefaultAsync(e => e.Id == examId, ct);

        if (exam is null) return null;

        var attempts = await db.OnlineExamAttempts
            .Include(a => a.Answers)
            .Where(a => a.ExamId == examId && (a.Status == "Submitted" || a.Status == "AutoSubmitted"))
            .ToListAsync(ct);

        var totalRegistered = await db.ExamRegistrations
            .CountAsync(r => r.ExamId == examId && r.Status != "Cancelled", ct);

        var completedCount = attempts.Count;
        var avgScore = completedCount > 0 ? attempts.Average(a => a.Score) : 0;
        var maxScore = completedCount > 0 ? attempts.Max(a => a.Score) : 0;
        var minScore = completedCount > 0 ? attempts.Min(a => a.Score) : 0;
        var passCount = attempts.Count(a => a.Score >= exam.PassingMarks);
        var passPct = completedCount > 0 ? ((double)passCount / completedCount) * 100 : 0;
        var avgTime = completedCount > 0 ? (int)attempts.Average(a => a.TimeTakenSeconds) : 0;

        var vm = new OnlineExamAnalyticsViewModel
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            Subject = exam.Subject,
            Standard = exam.Standard,
            TotalRegistered = totalRegistered,
            TotalAttempts = attempts.Count,
            CompletedAttempts = completedCount,
            AverageScore = Math.Round(avgScore, 2),
            HighestScore = Math.Round(maxScore, 2),
            LowestScore = Math.Round(minScore, 2),
            PassPercentage = Math.Round(passPct, 1),
            AverageTimeTakenSeconds = avgTime
        };

        // Per-Question accuracy analytics
        int qNum = 1;
        foreach (var eq in exam.ExamQuestions.OrderBy(x => x.DisplayOrder))
        {
            var q = eq.Question;
            if (q is null) continue;

            int correct = 0, incorrect = 0, skipped = 0;
            foreach (var a in attempts)
            {
                var ans = a.Answers.FirstOrDefault(ans => ans.QuestionId == q.Id);
                if (ans is null || !ans.SelectedOptionId.HasValue)
                {
                    skipped++;
                }
                else
                {
                    var opt = await db.QuestionOptions.FindAsync([ans.SelectedOptionId.Value], ct);
                    if (opt?.IsCorrect == true) correct++;
                    else incorrect++;
                }
            }

            vm.QuestionAnalytics.Add(new QuestionAnalyticsItemViewModel
            {
                QuestionId = q.Id,
                QuestionNumber = qNum++,
                QuestionText = q.QuestionText,
                Topic = q.Topic ?? exam.Subject,
                Difficulty = q.Difficulty,
                TotalAttempts = completedCount,
                CorrectCount = correct,
                IncorrectCount = incorrect,
                SkippedCount = skipped,
                CorrectPercentage = completedCount > 0 ? Math.Round((double)correct / completedCount * 100, 1) : 0
            });
        }

        return vm;
    }

    public async Task<byte[]> ExportResultsToCsvAsync(int examId, CancellationToken ct = default)
    {
        var attempts = await db.OnlineExamAttempts
            .Include(a => a.Student)
            .Include(a => a.Exam)
            .Where(a => a.ExamId == examId)
            .OrderByDescending(a => a.Score)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Student Name,Roll Number,Admission Number,Exam Title,Attempt Number,Score,Total Marks,Percentage,Grade,Status,Time Taken (mins),Submitted Date");

        foreach (var a in attempts)
        {
            var s = a.Student;
            var timeMins = Math.Round((double)a.TimeTakenSeconds / 60, 1);
            var status = a.Score >= (a.Exam?.PassingMarks ?? 35) ? "Passed" : "Failed";
            sb.AppendLine($"\"{s?.FullName}\",\"{s?.RollNumber}\",\"{s?.AdmissionNumber}\",\"{a.Exam?.Title}\",{a.AttemptNumber},{a.Score},{a.TotalMarks},{a.Percentage}%,{CalculateGrade(a.Percentage)},{status},{timeMins},\"{a.SubmittedAt?.ToLocalTime():yyyy-MM-dd HH:mm}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // ── Helper Mapping Methods ─────────────────────────────────────────────

    private static OnlineExamAttemptViewModel BuildAttemptViewModel(Exam exam, OnlineExamAttempt attempt, DateTime now)
    {
        var remainingSeconds = Math.Max(0, (long)(attempt.ExpiresAt - now).TotalSeconds);
        var answersMap = attempt.Answers.ToDictionary(a => a.QuestionId);

        var vm = new OnlineExamAttemptViewModel
        {
            AttemptId = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            Subject = exam.Subject,
            Standard = exam.Standard,
            DurationMinutes = exam.DurationMinutes,
            StartedAt = attempt.StartedAt,
            ExpiresAt = attempt.ExpiresAt,
            RemainingSeconds = remainingSeconds,
            TotalQuestions = exam.ExamQuestions.Count
        };

        int qNum = 1;
        var eqs = exam.RandomizeQuestions
            ? exam.ExamQuestions.OrderBy(_ => Guid.NewGuid()).ToList()
            : exam.ExamQuestions.OrderBy(eq => eq.DisplayOrder).ToList();

        foreach (var eq in eqs)
        {
            var q = eq.Question;
            if (q is null) continue;

            answersMap.TryGetValue(q.Id, out var ans);

            var opts = exam.RandomizeOptions
                ? q.Options.OrderBy(_ => Guid.NewGuid()).ToList()
                : q.Options.OrderBy(o => o.DisplayOrder).ToList();

            vm.Questions.Add(new StudentAttemptQuestionViewModel
            {
                QuestionId = q.Id,
                Number = qNum++,
                QuestionText = q.QuestionText,
                QuestionImageUrl = q.QuestionImageUrl,
                Marks = eq.MarksOverride ?? q.Marks,
                SelectedOptionId = ans?.SelectedOptionId,
                IsMarkedForReview = ans?.IsMarkedForReview ?? false,
                Options = opts.Select(o => new StudentAttemptOptionViewModel
                {
                    Id = o.Id,
                    OptionKey = o.OptionKey,
                    OptionText = o.OptionText,
                    OptionImageUrl = o.OptionImageUrl
                }).ToList()
            });
        }

        return vm;
    }

    private static string CalculateGrade(double percentage) => percentage switch
    {
        >= 90 => "A+",
        >= 80 => "A",
        >= 70 => "B+",
        >= 60 => "B",
        >= 50 => "C",
        _ => "D"
    };
}
