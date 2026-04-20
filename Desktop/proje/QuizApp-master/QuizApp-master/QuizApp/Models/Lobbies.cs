namespace QuizApp.Models
{
    public class Lobbies
    {
        public int Id { get; set; }
        public required string PinCode { get; set; }
        public int QuizId { get; set; }
        public bool IsStarted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
