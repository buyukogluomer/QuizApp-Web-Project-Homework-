using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.ViewModels;

namespace QuizApp.Controllers;

public class LobbyController : Controller
{
    private readonly AppDbContext _context;

    public LobbyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Join(string? pin = null)
    {
        ViewBag.Pin = pin;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Create(int quizId)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
        {
            return NotFound();
        }

        string pin;
        do
        {
            pin = Random.Shared.Next(100000, 999999).ToString();
        } while (await _context.Lobbies.AnyAsync(l => l.PinCode == pin));

        var lobby = new Lobbies
        {
            PinCode = pin,
            QuizId = quizId,
            IsStarted = false,
            CreatedAt = DateTime.Now
        };

        _context.Lobbies.Add(lobby);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(WaitingRoom), new { pin });
    }

    [HttpGet]
    public async Task<IActionResult> WaitingRoom(string pin)
    {
        var lobby = await _context.Lobbies
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PinCode == pin);

        if (lobby == null)
        {
            TempData["ErrorMessage"] = "Oda bulunamadi.";
            return RedirectToAction(nameof(Join));
        }

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == lobby.QuizId);

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

        var playerNames = distinctLobbyPlayers
            .OrderBy(p => p.Id)
            .Select(p => p.PlayerName ?? "Oyuncu")
            .ToList();

        var rankings = distinctLobbyPlayers
            .OrderByDescending(p => p.Score != -1)
            .ThenByDescending(p => p.Score)
            .ThenBy(p => p.PlayerName)
            .Select((player, index) => new QuizRankingEntryViewModel
            {
                Rank = index + 1,
                PlayerName = player.PlayerName ?? "Oyuncu",
                Score = player.Score == -1 ? 0 : player.Score,
                HasFinished = player.Score != -1
            })
            .ToList();

        var model = new LobbyWaitingRoomViewModel
        {
            PinCode = lobby.PinCode,
            QuizId = lobby.QuizId,
            QuizTitle = quiz?.Title ?? "Quiz",
            Players = playerNames,
            Rankings = rankings
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> PlayerWaiting(string pin, string name)
    {
        if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "PIN ve oyuncu adi gerekli.";
            return RedirectToAction(nameof(Join), new { pin });
        }

        var lobby = await _context.Lobbies
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.PinCode == pin);

        if (lobby == null)
        {
            TempData["ErrorMessage"] = "Girdigin PIN ile bir oda bulunamadi.";
            return RedirectToAction(nameof(Join), new { pin });
        }

        if (lobby.IsStarted)
        {
            return RedirectToAction("Play", "Quiz", new { id = lobby.QuizId });
        }

        var quiz = await _context.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == lobby.QuizId);

        var model = new PlayerWaitingViewModel
        {
            PinCode = lobby.PinCode,
            PlayerName = name.Trim(),
            QuizId = lobby.QuizId,
            QuizTitle = quiz?.Title ?? "Quiz",
            Rankings = (await _context.LobbyPlayers
                .AsNoTracking()
                .Where(p => p.LobbyPin == pin)
                .ToListAsync())
                .GroupBy(p => (p.PlayerName ?? "Oyuncu").Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(p => p.Score != -1)
                    .ThenByDescending(p => p.Score)
                    .ThenByDescending(p => p.Id)
                    .First())
                .OrderByDescending(p => p.Score != -1)
                .ThenByDescending(p => p.Score)
                .ThenBy(p => p.PlayerName)
                .Select((player, index) => new QuizRankingEntryViewModel
                {
                    Rank = index + 1,
                    PlayerName = player.PlayerName ?? "Oyuncu",
                    Score = player.Score == -1 ? 0 : player.Score,
                    HasFinished = player.Score != -1,
                    IsCurrentPlayer = string.Equals(player.PlayerName, name.Trim(), StringComparison.OrdinalIgnoreCase)
                })
                .ToList()
        };

        return View(model);
    }
}
