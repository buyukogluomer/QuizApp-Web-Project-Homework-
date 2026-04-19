using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Sessions
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }

    public int PinCode { get; set; }
 
    public bool isActive { get; set; }

    public DateTime dateTime { get; set; } = DateTime.Now;
    public int QuizId { get; set; }


    public Users? User { get; set; }
 
    public Quizzes? Quiz { get; set; }
 
    public List<Leaderboard>? LeaderboardEntries { get; set; } = new(); 
}