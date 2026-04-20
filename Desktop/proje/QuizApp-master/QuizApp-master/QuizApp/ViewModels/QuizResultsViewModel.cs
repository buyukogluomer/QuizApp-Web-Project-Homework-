namespace QuizApp.ViewModels;

public class QuizResultsViewModel
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double SuccessRate { get; set; }
    public string? PinCode { get; set; }
    public string? PlayerName { get; set; }
    public int TotalPlayers { get; set; }
    public int FinishedPlayers { get; set; }
    public bool AreAllPlayersFinished { get; set; }
    public List<QuizRankingEntryViewModel> Rankings { get; set; } = [];
}

public class QuizRankingEntryViewModel
{
    public int Rank { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsCurrentPlayer { get; set; }
    public bool HasFinished { get; set; }
}
