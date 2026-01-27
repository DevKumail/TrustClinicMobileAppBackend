
using CoherentMobile.Application.DTOs.PatientEducation;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Application.Services;

public class PatientEducationService : IPatientEducationService
{
    private readonly IPatientEducationCategoryRepository _categoryRepo;
    private readonly IPatientEducationRepository _educationRepo;
    private readonly IPatientEducationAssignmentRepository _assignmentRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PatientEducationService> _logger;

    public PatientEducationService(
        IPatientEducationCategoryRepository categoryRepo,
        IPatientEducationRepository educationRepo,
        IPatientEducationAssignmentRepository assignmentRepo,
        IPatientRepository patientRepo,
        IConfiguration configuration,
        ILogger<PatientEducationService> logger)
    {
        _categoryRepo = categoryRepo;
        _educationRepo = educationRepo;
        _assignmentRepo = assignmentRepo;
        _patientRepo = patientRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<PatientEducationCategoryDto>> GetCategoriesAsync(bool? generalOnly)
    {
        var categories = await _categoryRepo.GetActiveAsync(generalOnly);
        return categories.Select(MapCategory);
    }

    public async Task<IEnumerable<PatientEducationAssignmentDto>> GetAssignedEducationsByMrnoAsync(string mrno, int categoryId, bool includeExpired)
    {
        var trimmedMrno = (mrno ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedMrno))
        {
            return Array.Empty<PatientEducationAssignmentDto>();
        }

        if (categoryId <= 0)
        {
            return Array.Empty<PatientEducationAssignmentDto>();
        }

        var patient = await _patientRepo.GetByMRNOAsync(trimmedMrno);
        if (patient == null)
        {
            return Array.Empty<PatientEducationAssignmentDto>();
        }

        var assignments = await _assignmentRepo.GetMyActiveAsync(patient.Id, includeExpired);

        var list = new List<PatientEducationAssignmentDto>();
        foreach (var a in assignments)
        {
            PatientEducationDto? educationDto = null;
            try
            {
                var education = await _educationRepo.GetByIdAsync(a.EducationId);
                if (education != null && education.CategoryId == categoryId)
                    educationDto = MapEducation(education);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load education {EducationId} for assignment {AssignmentId}", a.EducationId, a.AssignmentId);
            }

            if (educationDto == null)
                continue;

            list.Add(new PatientEducationAssignmentDto
            {
                AssignmentId = a.AssignmentId,
                PatientId = a.PatientId,
                EducationId = a.EducationId,
                AssignedAt = a.AssignedAt,
                Notes = a.Notes,
                ArNotes = a.ArNotes,
                IsViewed = a.IsViewed,
                ViewedAt = a.ViewedAt,
                ExpiresAt = a.ExpiresAt,
                Education = educationDto
            });
        }

        return list;
    }

    private PatientEducationCategoryDto MapCategory(PatientEducationCategory c)
    {
        return new PatientEducationCategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            ArCategoryName = c.ArCategoryName,
            CategoryDescription = c.CategoryDescription,
            ArCategoryDescription = c.ArCategoryDescription,
            IconImageName = c.IconImageName,
            DisplayOrder = c.DisplayOrder,
            IsGeneral = c.IsGeneral
        };
    }

    private PatientEducationDto MapEducation(PatientEducation e)
    {
        var imageBaseUrl = _configuration["ImageSettings:BaseUrl"] ?? "https://localhost:7162/images";

        var pdfUrl = string.IsNullOrWhiteSpace(e.PdfFilePath)
            ? null
            : CombineUrl(imageBaseUrl, e.PdfFilePath);

        var thumbnailUrl = string.IsNullOrWhiteSpace(e.ThumbnailImageName)
            ? null
            : CombineUrl(imageBaseUrl, e.ThumbnailImageName);

        return new PatientEducationDto
        {
            EducationId = e.EducationId,
            CategoryId = e.CategoryId,
            Title = e.Title,
            ArTitle = e.ArTitle,
            PdfFileName = e.PdfFileName,
            PdfFilePath = e.PdfFilePath,
            Summary = e.Summary,
            ArSummary = e.ArSummary,
            ThumbnailImageName = e.ThumbnailImageName,
            DisplayOrder = e.DisplayOrder,
            PdfUrl = pdfUrl,
            ThumbnailUrl = thumbnailUrl,
            Active = e.Active,
            IsDeleted = e.IsDeleted,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            CreatedBy = e.CreatedBy,
            UpdatedBy = e.UpdatedBy,
            ContentDeltaJson = e.ContentDeltaJson,
            ArContentDeltaJson = e.ArContentDeltaJson
        };
    }

    private static string CombineUrl(string baseUrl, string relativeOrAbsolute)
    {
        if (Uri.TryCreate(relativeOrAbsolute, UriKind.Absolute, out var abs))
            return abs.ToString();

        var b = baseUrl.TrimEnd('/');
        var r = relativeOrAbsolute.TrimStart('/');
        return $"{b}/{r}";
    }
}
