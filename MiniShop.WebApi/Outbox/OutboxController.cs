using Microsoft.AspNetCore.Mvc;
using MiniShop.ApplicationCore.Interfaces;

namespace MiniShop.WebApi.Outbox;

[ApiController]
[Route("api/outbox")]
public class OutboxController : ControllerBase
{
    private readonly IOutboxService _outboxService;

    public OutboxController(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    [HttpPost("publish-pending")]
    public async Task<IActionResult> PublishPending()
    {
        await _outboxService.PublishedPendingAsync();

        return Ok("Pending outbox messages published.");
    }
}