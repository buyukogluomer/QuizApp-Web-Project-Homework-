namespace QuizApp.ViewModels;

public class LobbyWaitingRoomViewModel
{
    public string PinCode { get; set; } = string.Empty;
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public List<string> Players { get; set; } = [];
    public List<QuizRankingEntryViewModel> Rankings { get; set; } = [];
}
