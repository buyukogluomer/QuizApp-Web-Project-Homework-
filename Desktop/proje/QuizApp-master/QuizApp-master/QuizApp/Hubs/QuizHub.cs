using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;

namespace QuizApp.Hubs;

public class QuizHub : Hub
{
    private readonly AppDbContext _context;

    public QuizHub(AppDbContext context)
    {
        _context = context;
    }

    public async Task JoinLobby(string pin, string playerName, bool isHost = false)
    {
        pin = pin.Trim();
        playerName = playerName.Trim();

        if (string.IsNullOrWhiteSpace(pin))
        {
            throw new HubException("PIN gerekli.");
        }

        if (!isHost && string.IsNullOrWhiteSpace(playerName))
        {
            throw new HubException("Oyuncu adi gerekli.");
        }

        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.PinCode == pin);
        if (lobby == null)
        {
            throw new HubException("Lobby bulunamadi.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, pin);

        if (!string.IsNullOrWhiteSpace(playerName))
        {
            await UpsertLobbyPlayerConnectionAsync(pin, playerName);
        }

        await BroadcastLobbyState(pin);

        if (lobby.IsStarted)
        {
            await Clients.Caller.SendAsync("QuizStarted", lobby.QuizId, pin);
        }
    }

    public async Task StartQuiz(string pin)
    {
        var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.PinCode == pin);
        if (lobby == null)
        {
            throw new HubException("Lobby bulunamadi.");
        }

        lobby.IsStarted = true;
        await _context.SaveChangesAsync();

        await BroadcastLobbyState(pin);
        await Clients.Group(pin).SendAsync("QuizStarted", lobby.QuizId, pin);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var player = await _context.LobbyPlayers.FirstOrDefaultAsync(p => p.ConnectionId == Context.ConnectionId);

        if (player != null)
        {
            var pin = player.LobbyPin;
            var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.PinCode == pin);

            if (lobby != null && !lobby.IsStarted && player.Score == -1)
            {
                _context.LobbyPlayers.Remove(player);
            }
            else
            {
                player.ConnectionId = null;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.ChangeTracker.Clear();
            }

            await BroadcastLobbyState(pin);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastLobbyState(string pin)
    {
        var lobby = await _context.Lobbies
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PinCode == pin);

        if (lobby == null)
        {
            return;
        }

        var lobbyPlayers = await _context.LobbyPlayers
            .AsNoTracking()
            .Where(p => p.LobbyPin == pin)
            .ToListAsync();

        var distinctLobbyPlayers = lobbyPlayers
            .GroupBy(p => (p.PlayerName ?? "Oyuncu").Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(p => p.Score != -1)
                .ThenByDescending(p => p.Score)
                .ThenByDescending(p => p.Id)
                .First())
            .ToList();

        var players = distinctLobbyPlayers
            .OrderBy(p => p.Id)
            .Select(p => p.PlayerName ?? "Oyuncu")
            .ToList();

        var rankings = distinctLobbyPlayers
            .Where(p => p.Score != -1)
            .OrderByDescending(p => p.Score)
            .ThenBy(p => p.PlayerName)
            .Select((player, index) => new
            {
                rank = index + 1,
                playerName = player.PlayerName ?? "Oyuncu",
                score = player.Score
            })
            .ToList();

        await Clients.Group(pin).SendAsync("LobbyStateChanged", new
        {
            pin = lobby.PinCode,
            quizId = lobby.QuizId,
            isStarted = lobby.IsStarted,
            playerCount = players.Count,
            players,
            rankings
        });
    }

    private async Task UpsertLobbyPlayerConnectionAsync(string pin, string playerName)
    {
        try
        {
            var matchingPlayers = await _context.LobbyPlayers
                .Where(p => p.LobbyPin == pin && p.PlayerName == playerName)
                .OrderByDescending(p => p.Score != -1)
                .ThenByDescending(p => p.Score)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            var existingPlayer = matchingPlayers.FirstOrDefault();

            if (existingPlayer == null)
            {
                _context.LobbyPlayers.Add(new QuizApp.Models.LobbyPlayers
                {
                    LobbyPin = pin,
                    PlayerName = playerName,
                    Score = -1,
                    ConnectionId = Context.ConnectionId
                });
            }
            else
            {
                existingPlayer.ConnectionId = Context.ConnectionId;
                if (matchingPlayers.Count > 1)
                {
                    _context.LobbyPlayers.RemoveRange(matchingPlayers.Skip(1));
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.ChangeTracker.Clear();

            var matchingPlayers = await _context.LobbyPlayers
                .Where(p => p.LobbyPin == pin && p.PlayerName == playerName)
                .OrderByDescending(p => p.Score != -1)
                .ThenByDescending(p => p.Score)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            var existingPlayer = matchingPlayers.FirstOrDefault();

            if (existingPlayer == null)
            {
                _context.LobbyPlayers.Add(new QuizApp.Models.LobbyPlayers
                {
                    LobbyPin = pin,
                    PlayerName = playerName,
                    Score = -1,
                    ConnectionId = Context.ConnectionId
                });
            }
            else
            {
                existingPlayer.ConnectionId = Context.ConnectionId;
                if (matchingPlayers.Count > 1)
                {
                    _context.LobbyPlayers.RemoveRange(matchingPlayers.Skip(1));
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
