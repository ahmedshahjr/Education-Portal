using FluentValidation;
using ValidationException = EducationPortal.Application.Common.Exceptions.ValidationException;

namespace EducationPortal.Application.Common.Validation;

/// <summary>
/// Runs every validator of a request and throws one <see cref="ValidationException"/> with all failures. The validation
/// filter in the Web project calls this for each controller action (ADR-025); the validators are resolved from DI.
/// </summary>
public static class RequestValidation
{
    public static async Task ValidateAsync(IEnumerable<IValidator> validators, object request, CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();
        if (validatorList.Count == 0)
        {
            return;
        }

        var validationResults = await Task.WhenAll(
            validatorList.Select(v => v.ValidateAsync(new ValidationContext<object>(request), cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }
    }
}
