namespace QuizApp.Models
{
    public class LobbyPlayers
    {
        public int Id { get; set; }
        public required string LobbyPin { get; set; }
        public string? PlayerName { get; set; }
        public int Score { get; set; }
        public string? ConnectionId { get; set; }

    }
}
