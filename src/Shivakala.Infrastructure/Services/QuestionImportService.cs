using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shivakala.Core.Entities;
using Shivakala.Core.Services;
using Shivakala.Core.ViewModels;
using Shivakala.Infrastructure.Data;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Shivakala.Infrastructure.Services;

public sealed partial class QuestionImportService(
    ShivakalaDbContext db,
    ILogger<QuestionImportService> logger) : IQuestionImportService
{
    public async Task<QuestionImportPreviewViewModel> ParseAndPreviewAsync(
        Stream fileStream,
        string fileName,
        string defaultSubject,
        string defaultStandard,
        CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var textContent = string.Empty;

        try
        {
            if (ext is ".pptx" or ".ppt")
            {
                textContent = await ExtractTextFromPowerPointAsync(fileStream, ct);
            }
            else if (ext is ".pdf")
            {
                textContent = await ExtractTextFromPdfAsync(fileStream, ct);
            }
            else if (ext is ".jpg" or ".jpeg" or ".png")
            {
                textContent = await ExtractTextFromImageAsync(fileStream, ct);
            }
            else
            {
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                textContent = await reader.ReadToEndAsync(ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Document text extraction failed for file {FileName}", fileName);
            textContent = string.Empty;
        }

        var parsedItems = ParseQuestionItemsFromText(textContent, defaultSubject, defaultStandard);

        // Validate items
        var preview = new QuestionImportPreviewViewModel
        {
            FileName = fileName,
            TotalDetected = parsedItems.Count,
            Questions = parsedItems
        };

        foreach (var q in parsedItems)
        {
            ValidateQuestionItem(q);
        }

        preview.ValidCount = parsedItems.Count(q => q.ValidationStatus == "Valid");
        preview.NeedsReviewCount = parsedItems.Count(q => q.ValidationStatus == "NeedsReview");
        preview.InvalidCount = parsedItems.Count(q => q.ValidationStatus == "Invalid");

        return preview;
    }

    public async Task<int> SaveImportedQuestionsAsync(
        QuestionImportPreviewViewModel preview,
        int? examId = null,
        string? createdBy = null,
        CancellationToken ct = default)
    {
        int savedCount = 0;
        int displayOrder = 1;

        if (examId.HasValue)
        {
            var existingMaxOrder = await db.ExamQuestions
                .Where(eq => eq.ExamId == examId.Value)
                .Select(eq => (int?)eq.DisplayOrder)
                .MaxAsync(ct) ?? 0;
            displayOrder = existingMaxOrder + 1;
        }

        foreach (var qItem in preview.Questions.Where(q => q.ValidationStatus != "Invalid"))
        {
            if (string.IsNullOrWhiteSpace(qItem.QuestionText) || qItem.Options.Count < 2)
                continue;

            // Check duplicate in question bank
            var existing = await db.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionText == qItem.QuestionText.Trim(), ct);

            Question question;
            if (existing is not null)
            {
                question = existing;
            }
            else
            {
                var questionSubject = !string.IsNullOrWhiteSpace(qItem.Topic) && qItem.Topic != "General"
                    ? qItem.Topic
                    : (preview.Questions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Topic))?.Topic ?? "General");

                question = new Question
                {
                    QuestionText = qItem.QuestionText.Trim(),
                    QuestionImageUrl = qItem.QuestionImageUrl,
                    QuestionType = "MCQ",
                    Marks = qItem.Marks > 0 ? qItem.Marks : 1,
                    NegativeMarks = qItem.NegativeMarks,
                    Explanation = qItem.Explanation,
                    Subject = questionSubject,
                    Standard = "General",
                    Topic = qItem.Topic ?? questionSubject,
                    Difficulty = string.IsNullOrWhiteSpace(qItem.Difficulty) ? "Medium" : qItem.Difficulty,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                int optOrder = 1;
                foreach (var opt in qItem.Options)
                {
                    var optKey = string.IsNullOrWhiteSpace(opt.OptionKey)
                        ? ((char)('A' + optOrder - 1)).ToString()
                        : MapOptionKey(opt.OptionKey);

                    bool isCorrect = (!string.IsNullOrWhiteSpace(qItem.CorrectOptionKey) &&
                                     optKey.Equals(MapOptionKey(qItem.CorrectOptionKey), StringComparison.OrdinalIgnoreCase))
                                    || opt.IsCorrect;

                    question.Options.Add(new QuestionOption
                    {
                        OptionKey = optKey,
                        OptionText = opt.OptionText.Trim(),
                        IsCorrect = isCorrect,
                        DisplayOrder = optOrder++
                    });
                }

                // Fallback: If none designated as correct but CorrectOptionKey is provided, match first corresponding option
                if (!question.Options.Any(o => o.IsCorrect) && !string.IsNullOrWhiteSpace(qItem.CorrectOptionKey))
                {
                    var firstMatch = question.Options.FirstOrDefault(o =>
                        o.OptionKey.Equals(MapOptionKey(qItem.CorrectOptionKey), StringComparison.OrdinalIgnoreCase));
                    if (firstMatch is not null) firstMatch.IsCorrect = true;
                }

                db.Questions.Add(question);
                await db.SaveChangesAsync(ct);
            }

            savedCount++;

            // Link to Exam if examId provided
            if (examId.HasValue)
            {
                var existsInExam = await db.ExamQuestions
                    .AnyAsync(eq => eq.ExamId == examId.Value && eq.QuestionId == question.Id, ct);

                if (!existsInExam)
                {
                    db.ExamQuestions.Add(new ExamQuestion
                    {
                        ExamId = examId.Value,
                        QuestionId = question.Id,
                        DisplayOrder = displayOrder++,
                        MarksOverride = qItem.Marks > 0 ? qItem.Marks : null
                    });
                }
            }
        }

        if (examId.HasValue)
        {
            var exam = await db.Exams.FindAsync([examId.Value], ct);
            if (exam is not null)
            {
                exam.QuestionCount = await db.ExamQuestions.CountAsync(eq => eq.ExamId == examId.Value, ct);
                var totalMarks = await db.ExamQuestions
                    .Where(eq => eq.ExamId == examId.Value)
                    .SumAsync(eq => eq.MarksOverride ?? eq.Question!.Marks, ct);
                exam.TotalMarks = totalMarks > 0 ? totalMarks : exam.QuestionCount * 1;
                await db.SaveChangesAsync(ct);
            }
        }

        await db.SaveChangesAsync(ct);
        return savedCount;
    }

    // ── Document Extractor Helpers ──────────────────────────────────────────

    private static async Task<string> ExtractTextFromPowerPointAsync(Stream stream, CancellationToken ct)
    {
        var sb = new StringBuilder();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        var slideEntries = archive.Entries
            .Where(e => e.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase)
                     && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e =>
            {
                var match = Regex.Match(e.FullName, @"slide(\d+)\.xml", RegexOptions.IgnoreCase);
                return match.Success && int.TryParse(match.Groups[1].Value, out int num) ? num : 999999;
            })
            .ToList();

        foreach (var entry in slideEntries)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8);
            var xml = await reader.ReadToEndAsync(ct);

            var matches = Regex.Matches(xml, @"<a:t[^>]*>(.*?)</a:t>", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var text = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(System.Net.WebUtility.HtmlDecode(text));
                }
            }
            sb.AppendLine("\n--- SLIDE BREAK ---\n");
        }

        return sb.ToString();
    }

    private async Task<string> ExtractTextFromPdfAsync(Stream stream, CancellationToken ct)
    {
        var sb = new StringBuilder();
        using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream, ct);
        memStream.Position = 0;

        try
        {
            using var document = PdfDocument.Open(memStream);
            foreach (var page in document.GetPages())
            {
                var words = page.GetWords().ToList();
                if (words.Count > 0)
                {
                    // Group words that share a similar bottom baseline (within 3.5 points)
                    var lineGroups = new List<List<Word>>();
                    foreach (var word in words.OrderByDescending(w => w.BoundingBox.Bottom).ThenBy(w => w.BoundingBox.Left))
                    {
                        var placed = false;
                        foreach (var group in lineGroups)
                        {
                            var avgBottom = group.Average(w => w.BoundingBox.Bottom);
                            if (Math.Abs(word.BoundingBox.Bottom - avgBottom) < 3.5)
                            {
                                group.Add(word);
                                placed = true;
                                break;
                            }
                        }
                        if (!placed)
                        {
                            lineGroups.Add([word]);
                        }
                    }

                    var sortedLines = lineGroups
                        .OrderByDescending(g => g.Average(w => w.BoundingBox.Bottom))
                        .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text.Trim())));

                    foreach (var line in sortedLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            sb.AppendLine(line);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(page.Text))
                {
                    sb.AppendLine(page.Text);
                }

                sb.AppendLine("\n--- PAGE BREAK ---\n");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "PdfPig failed to extract PDF content, attempting fallback extraction.");
            memStream.Position = 0;
            using var reader = new StreamReader(memStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var content = await reader.ReadToEndAsync(ct);
            sb.Append(content);
        }

        return sb.ToString();
    }

    private static async Task<string> ExtractTextFromImageAsync(Stream stream, CancellationToken ct)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);
        var bytes = memoryStream.ToArray();
        // Basic plain text OCR fallback decoding
        return Encoding.UTF8.GetString(bytes);
    }

    // ── Parser Engine ────────────────────────────────────────────────────────

    private static List<ImportedQuestionItem> ParseQuestionItemsFromText(
        string rawText,
        string defaultSubject,
        string defaultStandard)
    {
        var items = new List<ImportedQuestionItem>();
        if (string.IsNullOrWhiteSpace(rawText)) return items;

        // 1. Clean up known OCR / font artifacts like black squares or missing glyph indicators
        rawText = Regex.Replace(rawText, @"[■▪●□]", "");

        // 2. Separate Answer Key section if present at the end of the document
        var answerKeyMap = new Dictionary<int, string>();
        string questionTextBody = rawText;

        var answerKeyHeaderRegex = new Regex(
            @"(?:\r?\n|^)\s*(?:Answer\s*Keys?|Answers?|Answer\s*Sheet|उत्तरतालिका|उत्तरपत्रिका|उत्तर\s*सूची|KEY)\s*[\:\-\=]*\s*(?:\r?\n)",
            RegexOptions.IgnoreCase);

        var akMatch = answerKeyHeaderRegex.Match(rawText);
        if (akMatch.Success)
        {
            questionTextBody = rawText.Substring(0, akMatch.Index);
            var answerKeyText = rawText.Substring(akMatch.Index + akMatch.Length);

            var pairRegex = new Regex(@"(?:(?:Q(?:uestion)?\.?|प्रश्न\.?)\s*)?(\d+|[०-९]+)\s*[\.\:\)\-\]\=\s]+\s*\(?([A-D]|[a-d]|[अ-ड]|1|2|3|4)\)?", RegexOptions.IgnoreCase);
            foreach (Match m in pairRegex.Matches(answerKeyText))
            {
                if (int.TryParse(NormalizeDigits(m.Groups[1].Value), out int qNum))
                {
                    var optKey = MapOptionKey(m.Groups[2].Value);
                    answerKeyMap[qNum] = optKey;
                }
            }
        }

        // 3. Extract topic / subject from preamble lines if present
        string detectedTopic = defaultSubject;
        var lines = questionTextBody.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                                    .Select(l => l.Trim())
                                    .Where(l => l.Length > 0)
                                    .ToList();

        var qHeaderRegex = new Regex(@"^\s*(?:(?:Q(?:uestion)?\.?|प्रश्न\.?)\s*)?(\d+|[०-९]+)[\.\:\)\-\]]\s*(.+)", RegexOptions.IgnoreCase);
        // Option prefix: A-D, Marathi letters, or digits 1-4. Dots must not be followed by digits to prevent decimal numbers (e.g. 21.6) matching.
        var optPrefixRegex = new Regex(@"^\s*(?:\(?([A-D]|[a-d]|[अ-ड]|1|2|3|4)\)?(?:[\:\)\-\]]|\.(?!\d))|\(([A-D]|[a-d]|[अ-ड]|1|2|3|4)\))\s*(.*)", RegexOptions.IgnoreCase);
        var inlineAnsRegex = new Regex(@"^(?:Ans(?:wer)?|Correct|उत्तर)[\:\=]?\s*\(?([A-D]|[a-d]|[अ-ड]|1|2|3|4)\)?", RegexOptions.IgnoreCase);
        var pageHeaderRegex = new Regex(@"^(?:Page\s+\d+|\d+\s*(?:of|\/)\s*\d+|\-\s*\d+\s*\-)$", RegexOptions.IgnoreCase);

        for (int idx = 0; idx < Math.Min(lines.Count, 5); idx++)
        {
            if (qHeaderRegex.IsMatch(lines[idx])) break;
            var topicMatch = Regex.Match(lines[idx], @"(?:Top\s+\d+\s+)?(.+?)\s*(?:MCQs?|Questions?|Test|Quiz|Paper|Exam)", RegexOptions.IgnoreCase);
            if (topicMatch.Success)
            {
                var candidate = topicMatch.Groups[1].Value.Trim();
                if (candidate.Length > 2 && !candidate.Equals("Practice", StringComparison.OrdinalIgnoreCase))
                {
                    detectedTopic = candidate;
                }
            }
        }

        ImportedQuestionItem? currentItem = null;
        int autoQNum = 1;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.StartsWith("--- SLIDE BREAK ---", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("--- PAGE BREAK ---", StringComparison.OrdinalIgnoreCase) ||
                pageHeaderRegex.IsMatch(line))
            {
                continue;
            }

            // Check if Question Header
            var qMatch = qHeaderRegex.Match(line);
            if (qMatch.Success)
            {
                bool isLikelyOption = false;
                if (currentItem is not null && currentItem.Options.Count > 0)
                {
                    var numStr = qMatch.Groups[1].Value;
                    if (int.TryParse(numStr, out int optNum) && optNum >= 1 && optNum <= 4)
                    {
                        if (optNum == currentItem.Options.Count + 1)
                        {
                            isLikelyOption = true;
                        }
                    }
                }

                if (!isLikelyOption)
                {
                    if (currentItem is not null && !string.IsNullOrWhiteSpace(currentItem.QuestionText))
                    {
                        items.Add(currentItem);
                    }

                    int qParsedNum = autoQNum++;
                    if (int.TryParse(NormalizeDigits(qMatch.Groups[1].Value), out int parsed))
                    {
                        qParsedNum = parsed;
                        autoQNum = parsed + 1;
                    }

                    currentItem = new ImportedQuestionItem
                    {
                        QuestionNumber = qParsedNum,
                        QuestionText = qMatch.Groups[2].Value.Trim(),
                        Topic = detectedTopic,
                        Difficulty = "Medium",
                        Marks = 1
                    };
                    continue;
                }
            }

            if (currentItem is null)
            {
                continue;
            }

            // Check for Inline Answer
            var ansMatch = inlineAnsRegex.Match(line);
            if (ansMatch.Success)
            {
                var ansKey = MapOptionKey(ansMatch.Groups[1].Value);
                currentItem.CorrectOptionKey = ansKey;
                foreach (var opt in currentItem.Options)
                {
                    opt.IsCorrect = (opt.OptionKey == ansKey);
                }
                continue;
            }

            // Check for Option(s)
            var optMatch = optPrefixRegex.Match(line);
            if (optMatch.Success)
            {
                // Check if this line contains multiple inline options: e.g. "A. 15  B. 16  C. 18  D. 20"
                var multiMatches = Regex.Matches(line, @"(?<=(?:\A|\s+))(?:\(?([A-D]|[a-d]|[अ-ड])\)?(?:[\:\)\-\]]|\.(?!\d))|\(([A-D]|[a-d]|[अ-ड]|1|2|3|4)\))\s*(.+?)(?=(?:\s+(?:\(?([A-D]|[a-d]|[अ-ड])\)?(?:[\:\)\-\]]|\.(?!\d))|\([A-D]|[a-d]|[अ-ड]|1|2|3|4\)))|\z)", RegexOptions.IgnoreCase);

                if (multiMatches.Count > 1)
                {
                    foreach (Match mm in multiMatches)
                    {
                        var keyGroup = !string.IsNullOrEmpty(mm.Groups[1].Value) ? mm.Groups[1].Value : mm.Groups[2].Value;
                        var optKey = MapOptionKey(keyGroup);
                        var optText = mm.Groups[3].Value.Trim();
                        var isCorrect = !string.IsNullOrWhiteSpace(currentItem.CorrectOptionKey) && currentItem.CorrectOptionKey == optKey;

                        currentItem.Options.Add(new ImportedQuestionOptionItem
                        {
                            OptionKey = optKey,
                            OptionText = optText,
                            IsCorrect = isCorrect
                        });
                    }
                    continue;
                }

                // Single option on this line
                var key = !string.IsNullOrEmpty(optMatch.Groups[1].Value) ? optMatch.Groups[1].Value : optMatch.Groups[2].Value;
                var singleOptKey = MapOptionKey(key);
                var singleOptText = optMatch.Groups[3].Value.Trim();
                var isSingleCorrect = !string.IsNullOrWhiteSpace(currentItem.CorrectOptionKey) && currentItem.CorrectOptionKey == singleOptKey;

                currentItem.Options.Add(new ImportedQuestionOptionItem
                {
                    OptionKey = singleOptKey,
                    OptionText = singleOptText,
                    IsCorrect = isSingleCorrect
                });
                continue;
            }

            // If we are still before options started, append line to question text
            if (currentItem.Options.Count == 0)
            {
                currentItem.QuestionText += " " + line;
            }
            else
            {
                var lastOpt = currentItem.Options.LastOrDefault();
                if (lastOpt is not null)
                {
                    lastOpt.OptionText += " " + line;
                }
            }
        }

        if (currentItem is not null && !string.IsNullOrWhiteSpace(currentItem.QuestionText))
        {
            items.Add(currentItem);
        }

        // Apply Answer Key if available
        for (int idx = 0; idx < items.Count; idx++)
        {
            var item = items[idx];
            if (string.IsNullOrEmpty(item.CorrectOptionKey))
            {
                if (answerKeyMap.TryGetValue(item.QuestionNumber, out var correctKey) ||
                    answerKeyMap.TryGetValue(idx + 1, out correctKey))
                {
                    item.CorrectOptionKey = correctKey;
                }
            }

            if (!string.IsNullOrEmpty(item.CorrectOptionKey))
            {
                foreach (var opt in item.Options)
                {
                    opt.IsCorrect = opt.OptionKey.Equals(item.CorrectOptionKey, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        // Fallback split if no structured question numbers were found
        if (items.Count == 0 && lines.Count >= 2)
        {
            var fallbackItem = new ImportedQuestionItem
            {
                QuestionNumber = 1,
                QuestionText = lines[0],
                Topic = detectedTopic
            };

            for (int i = 1; i < lines.Count && i <= 4; i++)
            {
                fallbackItem.Options.Add(new ImportedQuestionOptionItem
                {
                    OptionKey = ((char)('A' + i - 1)).ToString(),
                    OptionText = lines[i],
                    IsCorrect = i == 1
                });
            }
            fallbackItem.CorrectOptionKey = "A";
            items.Add(fallbackItem);
        }

        return items;
    }

    private static void ValidateQuestionItem(ImportedQuestionItem item)
    {
        var messages = new List<string>();

        if (string.IsNullOrWhiteSpace(item.QuestionText))
        {
            messages.Add("Question text is empty.");
        }

        if (item.Options.Count < 2)
        {
            messages.Add("Question must have at least 2 options.");
        }

        var correctCount = item.Options.Count(o => o.IsCorrect);
        if (correctCount == 0)
        {
            messages.Add("Correct answer requires review (none designated).");
        }
        else if (correctCount > 1)
        {
            messages.Add("Multiple options are marked correct.");
        }

        if (item.Options.Select(o => o.OptionText.Trim().ToLowerInvariant()).Distinct().Count() < item.Options.Count)
        {
            messages.Add("Duplicate option text detected.");
        }

        if (messages.Count == 0)
        {
            item.ValidationStatus = "Valid";
            item.ValidationMessage = "Valid question ready for import.";
        }
        else if (string.IsNullOrWhiteSpace(item.QuestionText) || item.Options.Count < 2)
        {
            item.ValidationStatus = "Invalid";
            item.ValidationMessage = string.Join(" ", messages);
        }
        else
        {
            item.ValidationStatus = "NeedsReview";
            item.ValidationMessage = string.Join(" ", messages);
        }
    }

    private static string MapOptionKey(string key)
    {
        var k = key.Trim(' ', '(', ')', '[', ']', '.', ':', '-').ToUpperInvariant();
        return k switch
        {
            "अ" or "1" => "A",
            "ब" or "2" => "B",
            "क" or "3" => "C",
            "ड" or "4" => "D",
            _ => k
        };
    }

    private static string NormalizeDigits(string text)
    {
        return text.Replace("०", "0").Replace("१", "1").Replace("२", "2").Replace("३", "3").Replace("४", "4")
                   .Replace("५", "5").Replace("६", "6").Replace("७", "7").Replace("८", "8").Replace("९", "9");
    }

    private static string NormalizeText(string text)
    {
        return Regex.Replace(text.Trim().ToLowerInvariant(), @"\s+", " ");
    }
}
