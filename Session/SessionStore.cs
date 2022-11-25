using System.Collections.Concurrent;
using System.Net.WebSockets;
using WebSocketsSample.Models;

namespace WebSocketsSample.Session
{
    public interface ISessionStore
    {
        void Add(string token, WebSocket webSocket);
        WebSocket? Get(string token);
        Record? Rollback(string token);
        void Push(string token, Record record);
    }
    public class SessionStore : ISessionStore
    {
        private readonly ConcurrentDictionary<string, WebSocket> _concurrentDictionary = new ConcurrentDictionary<string, WebSocket>();
        private readonly ConcurrentDictionary<string, Stack<Record>> _history = new ConcurrentDictionary<string, Stack<Record>>();
        public void Add(string token, WebSocket webSocket)
        {
            _concurrentDictionary.TryAdd(token, webSocket);
            _history.TryAdd(token, new Stack<Record>());
        }
        public WebSocket? Get(string token)
        {
            _concurrentDictionary.TryGetValue(token, out var webScoket);
            return webScoket;
        }

        public void Push(string token,Record record)
        {
            _history.TryGetValue(token, out var records);
            records?.Push(record);
        }

        
        public Record? Rollback(string token)
        {
            _history.TryGetValue(token, out var records);
            return records?.Count > 0 ? records?.Pop() : null;
        }

    }
}
