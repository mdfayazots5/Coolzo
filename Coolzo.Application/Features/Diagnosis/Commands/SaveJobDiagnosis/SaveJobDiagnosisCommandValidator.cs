using FluentValidation;

namespace Coolzo.Application.Features.Diagnosis.Commands.SaveJobDiagnosis;

public sealed class SaveJobDiagnosisCommandValidator : AbstractValidator<SaveJobDiagnosisCommand>
{
    public SaveJobDiagnosisCommandValidator()
    {
        RuleFor(request => request.ServiceRequestId).GreaterThan(0);
        RuleFor(request => request.DiagnosisRemarks).MaximumLength(512);
    }
}
