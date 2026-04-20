namespace QuizApp.ViewModels;

public class PlayerWaitingViewModel
{
    public string PinCode { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public List<QuizRankingEntryViewModel> Rankings { get; set; } = [];
}
