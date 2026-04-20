using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Questions
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        // DOKÜMAN GEREĞİ EKLENEN ALANLAR:
        public int TimeLimitSecond { get; set; } = 10; // Her soru için 10 saniye
        public int Points { get; set; } = 10;          // Her soru için 10 puan

        public int QuizId { get; set; }
        public Quizzes? Quiz { get; set; }

        public List<Answers> Answers { get; set; } = new List<Answers>();
       
    }
}
