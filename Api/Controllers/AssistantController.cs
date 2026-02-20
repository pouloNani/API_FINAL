using Core.DTOs.Agent;
using Infrastructure.AI;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Route("assistant")]
public class AssistantController(AgentLoop agentLoop) : BaseApiController
{
    [HttpPost("chat")]
    public async Task<ActionResult<AgentResponse>> Chat([FromBody] AgentChatRequest request)
    {
        try
        {
            var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest";
            var response = await agentLoop.RunAsync(request, userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AssistantController ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = ex.Message, detail = ex.StackTrace });
        }
    }

    [HttpPost("confirm")]
    public async Task<ActionResult> Confirm([FromBody] ConfirmActionRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            return Ok(new { message = "Action confirmée.", request });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AssistantController Confirm ERROR] {ex.Message}");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}