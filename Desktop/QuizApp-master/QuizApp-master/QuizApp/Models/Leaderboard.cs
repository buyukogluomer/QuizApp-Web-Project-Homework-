using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Leaderboard
    {
        [Key]
        public int Id { get; set; }
      

        public int UserId { get; set; }
    
        public Users? Username { get; set; }
    // Şablonda Username olarak isimlendirilmiş

        public int QuizId { get; set; }
      
        public Quizzes? Quiz { get; set; }


        public int Score { get; set; } = 0; 
        public DateTime AchievedAt { get; set; } = DateTime.Now; 

        // Şablona göre bu skor bir oturumla (Session) ilişkilidir
        public int? SessionId { get; set; }
        public Sessions? Session { get; set; }
     
    }
}