using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Helpers;
using QuizApp.Hubs;
using QuizApp.Models;
using QuizApp.ViewModels;

namespace QuizApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<QuizHub> _hubContext;

        public QuizController(AppDbContext context, IHubContext<QuizHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return View(quizzes);
        }

        [HttpGet]
        public async Task<IActionResult> Play(
            int id,
            int? currentQuestionId,
            int score = 0,
            int correctAnswers = 0,
            int totalProcessed = 0,
            string? pin = null,
            string? playerName = null)
        {
            Questions? question;

            if (currentQuestionId.HasValue)
            {
                question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.QuestionId == currentQuestionId.Value);
            }
            else
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null || quiz.Questions == null || !quiz.Questions.Any())
                {
                    return RedirectToAction(nameof(Index));
                }

                var firstQuestionId = quiz.Questions.OrderBy(q => q.QuestionId).First().QuestionId;
                return RedirectToAction(nameof(Play), new
                {
                    id,
                    currentQuestionId = firstQuestionId,
                    score,
                    correctAnswers,
                    totalProcessed,
                    pin,
                    playerName
                });
            }

            if (question == null)
            {
                return NotFound();
            }

            question.ImageUrl = ResolveQuestionImage(question);
            question.Answers = question.Answers
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            ViewBag.Score = score;
            ViewBag.CorrectAnswers = correctAnswers;
            ViewBag.TotalProcessed = totalProcessed;
            ViewBag.Pin = pin;
            ViewBag.PlayerName = playerName;

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(
            int questionId,
            int? selectedAnswerId,
            int score,
            int correctAnswers,
            int totalProcessed,
            int remainingSeconds,
            string? pin,
            string? playerName)
        {
            var currentQuestion = await _context.Questions
                .Include(q => q.Answers)
                .Include(q => q.Quiz)
                .ThenInclude(q => q!.Questions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (currentQuestion == null || currentQuestion.Quiz == null || currentQuestion.Quiz.Questions == null)
            {
                return NotFound();
            }

            var isCorrect = selectedAnswerId.HasValue &&
                            currentQuestion.Answers.Any(a => a.Id == selectedAnswerId.Value && a.IsCorrect);

            if (isCorrect)
            {
                correctAnswers++;
                score += CalculateCorrectAnswerScore(currentQuestion, remainingSeconds);
            }
            else
            {
                score -= CalculateWrongAnswerPenalty(currentQuestion);
            }

            totalProcessed++;
            var allQuestions = currentQuestion.Quiz.Questions.OrderBy(q => q.QuestionId).ToList();

            if (totalProcessed < allQuestions.Count)
            {
                var nextQuestion = allQuestions[totalProcessed];
                return RedirectToAction(nameof(Play), new
                {
                    id = currentQuestion.Quiz.Id,
                    currentQuestionId = nextQuestion.QuestionId,
                    score,
                    correctAnswers,
                    totalProcessed,
                    pin,
                    playerName
                });
            }

            return RedirectToAction(nameof(Results), new
            {
                score,
                totalQuestions = allQuestions.Count,
                correctAnswers,
                pin,
                playerName
            });
        }

        [HttpGet]
        public async Task<IActionResult> Results(int score, int totalQuestions, int correctAnswers, string? pin, string? playerName)
        {
            var model = new QuizResultsViewModel
            {
                Score = score,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                SuccessRate = totalQuestions > 0 ? Math.Round((double)correctAnswers / totalQuestions * 100, 1) : 0,
                PinCode = pin,
                PlayerName = playerName
            };

            if (!string.IsNullOrWhiteSpace(pin) && !string.IsNullOrWhiteSpace(playerName))
            {
                await UpsertLobbyPlayerScoreAsync(pin, playerName, score);

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

                model.TotalPlayers = distinctLobbyPlayers.Count;
                model.FinishedPlayers = distinctLobbyPlayers.Count(p => p.Score != -1);
                model.AreAllPlayersFinished = model.TotalPlayers > 0 && model.FinishedPlayers == model.TotalPlayers;

                var rankingPlayers = distinctLobbyPlayers
                    .OrderByDescending(p => p.Score != -1)
                    .ThenByDescending(p => p.Score)
                    .ThenBy(p => p.PlayerName)
                    .ToList();

                model.Rankings = rankingPlayers
                    .Select((player, index) => new QuizRankingEntryViewModel
                    {
                        Rank = index + 1,
                        PlayerName = player.PlayerName ?? "Oyuncu",
                        Score = player.Score == -1 ? 0 : player.Score,
                        HasFinished = player.Score != -1,
                        IsCurrentPlayer = string.Equals(player.PlayerName, playerName, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            return View(model);
        }

        private async Task BroadcastLobbyRankingAsync(string pin)
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

            await _hubContext.Clients.Group(pin).SendAsync("LobbyStateChanged", new
            {
                pin = lobby.PinCode,
                quizId = lobby.QuizId,
                isStarted = lobby.IsStarted,
                playerCount = players.Count,
                players,
                rankings
            });
        }

        private async Task UpsertLobbyPlayerScoreAsync(string pin, string playerName, int score)
        {
            var normalizedPlayerName = playerName.Trim();

            var matchingPlayers = (await _context.LobbyPlayers
                .Where(p => p.LobbyPin == pin)
                .ToListAsync())
                .Where(p => string.Equals((p.PlayerName ?? string.Empty).Trim(), normalizedPlayerName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.Score != -1)
                .ThenByDescending(p => p.Score)
                .ThenByDescending(p => p.Id)
                .ToList();

            if (matchingPlayers.Count == 0)
            {
                _context.LobbyPlayers.Add(new LobbyPlayers
                {
                    LobbyPin = pin,
                    PlayerName = normalizedPlayerName,
                    Score = score
                });
            }
            else
            {
                matchingPlayers[0].PlayerName = normalizedPlayerName;
                matchingPlayers[0].Score = score;

                for (var i = 1; i < matchingPlayers.Count; i++)
                {
                    _context.LobbyPlayers.Remove(matchingPlayers[i]);
                }
            }

            await _context.SaveChangesAsync();
            await BroadcastLobbyRankingAsync(pin);
        }

        private static string? ResolveQuestionImage(Questions question)
        {
            var mappedImage = PlayerImageMapper.MapQuestionImage(question.ImageUrl);

            if (!string.IsNullOrWhiteSpace(mappedImage) &&
                mappedImage.StartsWith("/images/players/", StringComparison.OrdinalIgnoreCase))
            {
                return mappedImage;
            }

            var correctAnswerImage = question.Answers
                .Where(a => a.IsCorrect)
                .Select(a => PlayerImageMapper.MapPlayerName(a.AnswerText))
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));

            if (!string.IsNullOrWhiteSpace(correctAnswerImage))
            {
                return correctAnswerImage;
            }

            var anyAnswerImage = question.Answers
                .Select(a => PlayerImageMapper.MapPlayerName(a.AnswerText))
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));

            return anyAnswerImage ?? mappedImage;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Quizzes newQuiz)
        {
            if (ModelState.IsValid)
            {
                newQuiz.CreatedAt = DateTime.Now;
                newQuiz.UpdatedAt = DateTime.Now;

                _context.Quizzes.Add(newQuiz);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(AddQuestion), new { quizId = newQuiz.Id });
            }

            return View(newQuiz);
        }

        [HttpGet]
        public IActionResult AddQuestion(int quizId)
        {
            ViewBag.QuizId = quizId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(int quizId, string questionText, string? questionImageUrl, string[] options, int correctAnswerIndex)
        {
            if (quizId <= 0) return BadRequest("Invalid quizId.");

            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return NotFound("Quiz not found.");

            var question = new Questions
            {
                QuizId = quizId,
                Text = questionText,
                ImageUrl = PlayerImageMapper.MapQuestionImage(questionImageUrl),
                Points = 10,
                TimeLimitSecond = 10
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            for (int i = 0; i < options.Length; i++)
            {
                _context.Answers.Add(new Answers
                {
                    QuestionId = question.QuestionId,
                    AnswerText = options[i],
                    IsCorrect = i == correctAnswerIndex
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AddQuestion), new { quizId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            question.Answers = question.Answers
                .OrderBy(a => a.Id)
                .ToList();

            question.ImageUrl = ResolveQuestionImage(question);
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int QuestionId, string Text, string? ImageUrl, string[] options, int[] answerIds, int correctAnswerIndex)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == QuestionId);

            if (question == null) return NotFound();

            question.Text = Text;
            question.ImageUrl = PlayerImageMapper.MapQuestionImage(ImageUrl);

            var answerMap = question.Answers.ToDictionary(a => a.Id);
            for (int i = 0; i < options.Length && i < answerIds.Length; i++)
            {
                if (!answerMap.TryGetValue(answerIds[i], out var answer))
                {
                    continue;
                }

                answer.AnswerText = options[i];
                answer.IsCorrect = i == correctAnswerIndex;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(QuestionsList), new { quizId = question.QuizId });
        }

        public async Task<IActionResult> QuestionsList(int quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Answers)
                .ToListAsync();

            foreach (var question in questions)
            {
                question.ImageUrl = ResolveQuestionImage(question);
                question.Answers = question.Answers.OrderBy(a => a.Id).ToList();
            }

            ViewBag.QuizId = quizId;
            return View(questions);
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            int currentQuizId = question.QuizId;

            _context.Answers.RemoveRange(question.Answers);
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(QuestionsList), new { quizId = currentQuizId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(ques => ques.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            foreach (var question in quiz.Questions ?? [])
            {
                _context.Answers.RemoveRange(question.Answers);
            }

            _context.Questions.RemoveRange(quiz.Questions ?? []);
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static int CalculateCorrectAnswerScore(Questions question, int remainingSeconds)
        {
            var safeRemainingSeconds = Math.Max(0, remainingSeconds);
            return question.Points + safeRemainingSeconds;
        }

        private static int CalculateWrongAnswerPenalty(Questions question)
        {
            return Math.Max(5, question.Points / 2);
        }
    }
}
