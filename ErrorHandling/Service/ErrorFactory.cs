using Microsoft.AspNetCore.Http;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;
using Microsoft.Extensions.Configuration;

namespace ErrorHandling.Service;

/// <summary>
/// Service for creating RFC 7807 compliant <see cref="ProblemDetails"/> responses from <see cref="InternalError"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// This service transforms domain-specific errors into standardized problem details responses,
/// including proper type URIs and extension data.
/// </para>
/// <para>
/// The service uses configuration to construct the base URL for error types and preserves
/// the original request path in the response.
/// </para>
/// </remarks>
public class ErrorFactory
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorFactory"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration for accessing base URLs.</param>
    /// <param name="httpContextAccessor">Accessor for current HTTP context.</param>
    public ErrorFactory(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Creates an RFC 7807 compliant <see cref="ProblemDetails"/> response from an <see cref="InternalError"/>.
    /// </summary>
    /// <param name="error">The domain error to convert.</param>
    /// <returns>A fully configured <see cref="ProblemDetails"/> response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public ProblemDetails CreateProblemDetails(InternalError error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        var problemDetails = new ProblemDetails
        {
            Type = "about:blank",
            Title = error.Title,
            Detail = error.Detail,
            Status = (int)error.Code,
            Instance = _httpContextAccessor.HttpContext?.Request.Path,
        };

        if (error.Extensions != null)
        {
            foreach (var (key, value) in error.Extensions)
            {
                problemDetails.Extensions.Add(key, value);
            }
        }

        return problemDetails;
    }
}