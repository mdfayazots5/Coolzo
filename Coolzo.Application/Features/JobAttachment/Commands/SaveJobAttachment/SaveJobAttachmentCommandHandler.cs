using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Features.TechnicianJob;
using Coolzo.Contracts.Responses.FieldExecution;
using Coolzo.Domain.Enums;
using Coolzo.Shared.Constants;
using Coolzo.Shared.Exceptions;
using Coolzo.Shared.Models;
using MediatR;

namespace Coolzo.Application.Features.JobAttachment.Commands.SaveJobAttachment;

public sealed class SaveJobAttachmentCommandHandler : IRequestHandler<SaveJobAttachmentCommand, JobAttachmentResponse>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IJobAttachmentStorageService _jobAttachmentStorageService;
    private readonly IJobCardFactory _jobCardFactory;
    private readonly ITechnicianJobAccessService _technicianJobAccessService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveJobAttachmentCommandHandler(
        ITechnicianJobAccessService technicianJobAccessService,
        IJobCardFactory jobCardFactory,
        IJobAttachmentStorageService jobAttachmentStorageService,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentDateTime currentDateTime,
        ICurrentUserContext currentUserContext)
    {
        _technicianJobAccessService = technicianJobAccessService;
        _jobCardFactory = jobCardFactory;
        _jobAttachmentStorageService = jobAttachmentStorageService;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentDateTime = currentDateTime;
        _currentUserContext = currentUserContext;
    }

    public async Task<JobAttachmentResponse> Handle(SaveJobAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<JobAttachmentType>(request.AttachmentType, true, out var attachmentType))
        {
            throw new AppException(ErrorCodes.InvalidAttachmentType, "The attachment type is invalid.", 400);
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            throw new AppException(ErrorCodes.InvalidAttachmentContent, "Only JPEG, PNG, and WEBP attachments are supported.", 400);
        }

        byte[] fileBytes;

        try
        {
            fileBytes = Convert.FromBase64String(request.Base64Content);
        }
        catch (FormatException)
        {
            throw new AppException(ErrorCodes.InvalidAttachmentContent, "The attachment content is not valid base64.", 400);
        }

        if (fileBytes.LongLength > 5 * 1024 * 1024)
        {
            throw new AppException(ErrorCodes.AttachmentTooLarge, "Attachment size must not exceed 5 MB.", 409);
        }

        var serviceRequest = await _technicianJobAccessService.GetOwnedServiceRequestForUpdateAsync(request.ServiceRequestId, cancellationToken);
        var jobCard = _jobCardFactory.EnsureCreated(serviceRequest);
        var storedResult = await _jobAttachmentStorageService.SaveAsync(
            request.FileName,
            request.ContentType,
            fileBytes,
            cancellationToken);
        var now = _currentDateTime.UtcNow;
        var userName = _currentUserContext.UserName;
        var ipAddress = _currentUserContext.IPAddress;

        var attachment = new Coolzo.Domain.Entities.JobAttachment
        {
            AttachmentType = attachmentType,
            FileName = request.FileName,
            StoredFileName = storedResult.StoredFileName,
            RelativePath = storedResult.RelativePath,
            ContentType = request.ContentType,
            FileSizeInBytes = storedResult.FileSizeInBytes,
            AttachmentRemarks = request.AttachmentRemarks?.Trim() ?? string.Empty,
            UploadedDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        };

        jobCard.Attachments.Add(attachment);
        jobCard.ExecutionTimelines.Add(new Coolzo.Domain.Entities.JobExecutionTimeline
        {
            Status = serviceRequest.CurrentStatus,
            EventType = "AttachmentUploaded",
            EventTitle = attachmentType.ToString(),
            Remarks = attachment.AttachmentRemarks,
            EventDateUtc = now,
            CreatedBy = userName,
            DateCreated = now,
            IPAddress = ipAddress
        });

        await _auditLogRepository.AddAsync(
            new Coolzo.Domain.Entities.AuditLog
            {
                UserId = _currentUserContext.UserId,
                ActionName = "SaveJobAttachment",
                EntityName = "JobCard",
                EntityId = jobCard.JobCardNumber,
                TraceId = _currentUserContext.TraceId,
                StatusName = "Success",
                NewValues = attachmentType.ToString(),
                CreatedBy = userName,
                DateCreated = now,
                IPAddress = ipAddress
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JobAttachmentResponse(
            attachment.JobAttachmentId,
            attachment.AttachmentType.ToString(),
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeInBytes,
            attachment.RelativePath,
            attachment.AttachmentRemarks,
            attachment.UploadedDateUtc);
    }
}
