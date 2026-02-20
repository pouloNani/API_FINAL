using System.Text.Json;
using Core.DTOs.Agent;
using StackExchange.Redis;

namespace Infrastructure.AI;

public class ConversationHistory
{
    private readonly IDatabase _db;
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(2);

    public ConversationHistory(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string Key(string userId) => $"conversation:{userId}";

    public async Task<List<MessageDto>> GetAsync(string userId)
    {
        var data = await _db.StringGetAsync(Key(userId));
        if (data.IsNullOrEmpty) return new List<MessageDto>();
       return JsonSerializer.Deserialize<List<MessageDto>>((string)data!) ?? new();
    }

    public async Task AppendAsync(string userId, MessageDto message)
    {
        var history = await GetAsync(userId);
        history.Add(message);

        if (history.Count > 20)
            history = history.TakeLast(20).ToList();

        await _db.StringSetAsync(
            Key(userId),
            JsonSerializer.Serialize(history),
            Ttl
        );
    }

    public async Task AppendManyAsync(string userId, IEnumerable<MessageDto> messages)
    {
        foreach (var msg in messages)
            await AppendAsync(userId, msg);
    }

    public async Task ClearAsync(string userId)
        => await _db.KeyDeleteAsync(Key(userId));
}