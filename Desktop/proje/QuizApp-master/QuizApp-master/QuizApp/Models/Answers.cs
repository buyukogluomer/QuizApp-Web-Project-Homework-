using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models;

public class Answers
{
    [Key]
    public int Id { get; set; }

    public string? AnswerText { get; set; }

    public bool IsCorrect { get; set; } = false;

    public int QuestionId { get; set; }

    // İlişkiler
    public Questions? Question { get; set; }
}