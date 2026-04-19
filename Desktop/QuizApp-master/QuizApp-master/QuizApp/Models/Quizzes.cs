using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Quizzes
{
    [Key]
    public int Id { get; set; }
  
    public string? Title { get; set; }
  
    public string? Description { get; set; }
   
    public DateTime CreatedAt { get; set; }
   
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<Questions>? Questions { get; set; } = new(); 
}