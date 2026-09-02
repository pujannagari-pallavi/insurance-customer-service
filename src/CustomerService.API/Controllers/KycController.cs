using System.Security.Claims;
using CustomerService.Application.Exceptions;
using CustomerService.Infrastructure.Kyc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/kyc")]
public sealed class KycController(KycSecurityService kycSecurityService) : ControllerBase
{
    [HttpGet("cases/pending")]
    public async Task<IActionResult> GetPendingCases(CancellationToken cancellationToken)
    {
        RequirePermission("Kyc.Verify");
        return Ok(await kycSecurityService.GetPendingCasesAsync(cancellationToken));
    }

    [HttpGet("cases/{kycCaseId:guid}/document")]
    public async Task<IActionResult> GetDocument(Guid kycCaseId, CancellationToken cancellationToken)
    {
        RequirePermission("Kyc.Verify");
        var document = await kycSecurityService.GetDocumentAsync(kycCaseId, cancellationToken);
        return File(document.Content, document.ContentType, enableRangeProcessing: true);
    }

    [HttpPost("customers/{customerId:guid}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid customerId, [FromForm] KycUploadRequest request, CancellationToken cancellationToken)
    {
        RequirePermission("Kyc.Submit");
        await using var stream = request.File.OpenReadStream();
        var kycCaseId = await kycSecurityService.UploadAsync(customerId, ActorId(), request.DocumentType, request.File.FileName, request.File.ContentType, stream, request.File.Length, cancellationToken);
        return Accepted(new { kycCaseId, status = "PendingReview" });
    }

    [HttpGet("customers/{customerId:guid}/submission")]
    public async Task<IActionResult> GetLatestSubmission(Guid customerId, CancellationToken cancellationToken)
    {
        var submission = await kycSecurityService.GetLatestSubmissionAsync(customerId, ActorId(), cancellationToken);
        return submission is null ? NoContent() : Ok(submission);
    }

    [HttpPost("cases/{kycCaseId:guid}/decision")]
    public async Task<IActionResult> Decide(Guid kycCaseId, [FromBody] KycDecisionRequest request, CancellationToken cancellationToken)
    {
        RequirePermission("Kyc.Verify");
        await kycSecurityService.DecideAsync(kycCaseId, ActorId(), request.Verify, request.RejectionReason, cancellationToken);
        return NoContent();
    }

    private Guid ActorId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("The access token does not contain a valid user identifier.");
    private void RequirePermission(string permission) { if (!User.Claims.Any(claim => claim.Type == "permission" && claim.Value == permission)) throw new UnauthorizedAccessException("You do not have the required KYC permission."); }
}

public sealed class KycUploadRequest
{
    public required string DocumentType { get; init; }
    public required IFormFile File { get; init; }
}

public sealed record KycDecisionRequest(bool Verify, string? RejectionReason);