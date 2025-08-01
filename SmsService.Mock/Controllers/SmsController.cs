using Microsoft.AspNetCore.Mvc;
using SmsService.Mock.Models;

namespace SmsService.Mock.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private static readonly List<SmsLog> _smsLogs = new();

    [HttpPost("send")]
    public IActionResult SendSms([FromBody] SendSmsRequest request)
    {
        try
        {
            // Simulate SMS sending
            var messageId = Guid.NewGuid().ToString();
            var success = true; // In a real service, this might fail sometimes
            
            var smsLog = new SmsLog
            {
                Id = messageId,
                PhoneNumber = request.PhoneNumber,
                Message = request.Message,
                SentAt = DateTime.UtcNow,
                Success = success
            };

            _smsLogs.Add(smsLog);

            // Log to console for demo purposes
            Console.WriteLine($"[SMS MOCK] Sent to {request.PhoneNumber}: {request.Message}");

            var response = new SendSmsResponse
            {
                Success = success,
                MessageId = messageId
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new SendSmsResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            return StatusCode(500, response);
        }
    }

    [HttpGet("logs")]
    public IActionResult GetSmsLogs([FromQuery] string? phoneNumber = null)
    {
        var logs = _smsLogs.AsQueryable();

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            logs = logs.Where(l => l.PhoneNumber == phoneNumber);
        }

        return Ok(logs.OrderByDescending(l => l.SentAt).Take(50).ToList());
    }

    [HttpGet("logs/{id}")]
    public IActionResult GetSmsLog(string id)
    {
        var log = _smsLogs.FirstOrDefault(l => l.Id == id);
        
        if (log == null)
        {
            return NotFound();
        }

        return Ok(log);
    }

    [HttpDelete("logs")]
    public IActionResult ClearLogs()
    {
        _smsLogs.Clear();
        return Ok(new { message = "All SMS logs cleared" });
    }
}
