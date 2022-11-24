using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebSocketsSample.Models;
using WebSocketsSample.Session;

namespace WebSocketsSample.Controllers;

// <snippet>
public class WebSocketController : ControllerBase
{
    private readonly ISessionStore _sessionStore;
    public WebSocketController(ISessionStore sessionStore)
    {
        _sessionStore = sessionStore;
    }

    [HttpGet("/ws")]
    public async Task Get([FromQuery]string token)
    {
        if (token == null || token != "1234")
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = _sessionStore.Get(token);
            if (webSocket == null)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _sessionStore.Add(token, webSocket);
            }
            await Echo(webSocket, token);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    // </snippet>

    private async Task Echo(WebSocket webSocket,string token)
    {
        var buffer = new byte[1024 * 4];
        
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
        var record = _sessionStore.Rollback(token);
        var isRollback = false;
        while (!receiveResult.CloseStatus.HasValue)
        {
            
            if (record !=null && !isRollback)
            {
                var content = JsonSerializer.Serialize(record);
                buffer = Encoding.UTF8.GetBytes(content);
                isRollback = true;
            }
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            var aaa = Encoding.UTF8.GetString(buffer);
            record = JsonSerializer.Deserialize<Record>(aaa);
            if (record != null)
            {
                _sessionStore.Push(token, record);
            }
            
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
