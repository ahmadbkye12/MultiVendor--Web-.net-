using FluentValidation.Results;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Extensions;

public static class FluentValidationHttpExtensions
{
    public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult result) =>
        new(result.ToProblemErrorDictionary());

    public static IDictionary<string, string[]> ToProblemErrorDictionary(this ValidationResult result) =>
        result.Errors
            .GroupBy(e => string.IsNullOrEmpty(e.PropertyName) ? "_" : e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
