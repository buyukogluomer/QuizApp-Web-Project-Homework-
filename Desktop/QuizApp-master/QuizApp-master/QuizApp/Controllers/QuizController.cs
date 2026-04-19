using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Quizlerin Listelendiği Sayfa (Index)
        public async Task<IActionResult> Index()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return View(quizzes);
        }

        // 2. GET: Soru Görüntüleme (Geri tuşu hatasını engelleyen yapı)
        [HttpGet]
        public async Task<IActionResult> Play(int id, int? currentQuestionId, int correctAnswers = 0, int totalProcessed = 0)
        {
            Questions? question;

            if (currentQuestionId.HasValue)
            {
                // Bir sonraki soruya yönlendirildiysek o soruyu getir
                question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.QuestionId == currentQuestionId.Value);
            }
            else
            {
                // Quiz ilk kez başlıyorsa ilk soruyu bul ve tekrar yönlendir
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null || !quiz.Questions.Any()) return RedirectToAction("Index");

                var firstQuestionId = quiz.Questions.OrderBy(q => q.QuestionId).First().QuestionId;
                return RedirectToAction("Play", new { id = id, currentQuestionId = firstQuestionId });
            }

            if (question == null) return NotFound();

            ViewBag.CorrectAnswers = correctAnswers;
            ViewBag.TotalProcessed = totalProcessed;

            return View(question);
        }

        // 3. POST: Cevap Kontrolü ve Yönlendirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(int questionId, int selectedAnswerId, int correctAnswers, int totalProcessed)
        {
            var currentQuestion = await _context.Questions
                .Include(q => q.Answers)
                .Include(q => q.Quiz)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (currentQuestion == null) return NotFound();

            // Cevap Doğruluğu Kontrolü
            var isCorrect = currentQuestion.Answers.Any(a => a.Id == selectedAnswerId && a.IsCorrect);
            if (isCorrect)
            {
                correctAnswers++;
            }

            totalProcessed++;
            var allQuestions = currentQuestion.Quiz.Questions.OrderBy(q => q.QuestionId).ToList();

            // Sırada soru varsa: Bir sonrakine GET yöntemiyle yönlendir
            if (totalProcessed < allQuestions.Count)
            {
                var nextQuestion = allQuestions[totalProcessed];
                return RedirectToAction("Play", new
                {
                    id = currentQuestion.Quiz.Id,
                    currentQuestionId = nextQuestion.QuestionId,
                    correctAnswers = correctAnswers,
                    totalProcessed = totalProcessed
                });
            }

            // Sorular bittiyse: Results sayfasına puanı gönder
            int finalScore = correctAnswers * 10;

            return RedirectToAction("Results", new { score = finalScore, totalQuestions = allQuestions.Count, correctAnswers = correctAnswers });
        }

        // 4. Sonuç Ekranı (Results)
        public IActionResult Results(int score, int totalQuestions, int correctAnswers)
        {
            ViewBag.Score = score;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.CorrectAnswers = correctAnswers;

            double percentage = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;
            ViewBag.SuccessRate = Math.Round(percentage, 1);

            return View();
        }
        // 1. Form Sayfasını Görüntüleyen Metot
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 2. Form Gönderildiğinde Veriyi Kaydeden Metot
        [HttpPost]
        public async Task<IActionResult> Create(Quizzes newQuiz)
        {
            if (ModelState.IsValid) // Veriler kurallara uygun mu?
            {
                newQuiz.CreatedAt = DateTime.Now;
                newQuiz.UpdatedAt = DateTime.Now;

                _context.Quizzes.Add(newQuiz); // Listeye ekle
                await _context.SaveChangesAsync(); // Veritabanına yaz

                return RedirectToAction("AddQuestion", new { quizId = newQuiz.Id }); // Ana sayfaya geri dön
            }
            return View(newQuiz); // Hata varsa aynı sayfada kal
        }

        // Soru ekleme sayfasını açar
        [HttpGet]
        public IActionResult AddQuestion(int quizId)
        {
            ViewBag.QuizId = quizId; // Sorunun hangi quize ait olduğunu unutmamak için
            return View();
        }

        // Formdan gelen soruyu ve şıkları kaydeder
        // C#
[HttpPost]
public async Task<IActionResult> AddQuestion(int quizId, string questionText, string[] options, int correctAnswerIndex)
{
    if (quizId <= 0) return BadRequest("Invalid quizId.");

    var quiz = await _context.Quizzes.FindAsync(quizId);
    if (quiz == null) return NotFound("Quiz not found.");

    var question = new Questions
    {
        QuizId = quizId,
        Text = questionText,
        Points = 10,
        TimeLimitSecond = 30
    };

    _context.Questions.Add(question);
    await _context.SaveChangesAsync();

    for (int i = 0; i < options.Length; i++)
    {
        _context.Answers.Add(new Answers
        {
            QuestionId = question.QuestionId,
            AnswerText = options[i],
            IsCorrect = (i == correctAnswerIndex)
        });
    }
    await _context.SaveChangesAsync();

    return RedirectToAction("AddQuestion", new { quizId });
}

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Soruyu ve şıklarını beraber çekiyoruz
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            // ÖNEMLİ: Eğer dosya adın Edit.cshtml ise alttaki satır yeterli.
            // Eğer dosya adın EditQuestion.cshtml ise return View("EditQuestion", question); yazmalısın.
            return View(question);
        }
        [HttpPost] // HATAYI ÇÖZEN KRİTİK ETİKET!
        public async Task<IActionResult> Edit(int QuestionId, string Text, string[] options, int[] answerIds, int correctAnswerIndex)
        {
            // 1. Veritabanından mevcut soruyu şıklarıyla beraber bul
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == QuestionId);

            if (question == null) return NotFound();

            // 2. Soru metnini güncelle
            question.Text = Text;
            

            // 3. Şıkları döngüyle güncelle
            var answersList = question.Answers.ToList();
            for (int i = 0; i < options.Length; i++)
            {
                // Gelen dizideki şık metinlerini ve doğru cevabı yerleştiriyoruz
                answersList[i].AnswerText = options[i];
                answersList[i].IsCorrect = (i == correctAnswerIndex);
            }

            await _context.SaveChangesAsync();

            // 4. İşlem bitince tekrar soru listesine dönüyoruz (quizId'yi kullanarak)
            return RedirectToAction("QuestionsList", new { quizId = question.QuizId });
        }
        public async Task<IActionResult> QuestionsList(int quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId) // Sadece bu quize ait soruları getir
                .Include(q => q.Answers)       // Şıkları da beraberinde getir
                .ToListAsync();

            ViewBag.QuizId = quizId; // Yeni soru eklemek istersek lazım olacak
            return View(questions);
        }
        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            // 1. Soruyu şıklarıyla beraber buluyoruz
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();

            int currentQuizId = question.QuizId; // Silme işleminden sonra geri dönmek için ID'yi saklıyoruz

            // 2. Önce bu soruya bağlı tüm şıkları (Answers) siliyoruz
            _context.Answers.RemoveRange(question.Answers);

            // 3. Sonra sorunun kendisini siliyoruz
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();

            // 4. Kullanıcıyı tekrar soru listesine yönlendiriyoruz
            return RedirectToAction("QuestionsList", new { quizId = currentQuizId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(ques => ques.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // Zincirleme silme: EF Core ilişkileri doğru kurulduysa bunu otomatik yapar 
            // ama garanti olması için manuel siliyoruz:
            foreach (var question in quiz.Questions)
            {
                _context.Answers.RemoveRange(question.Answers);
            }
            _context.Questions.RemoveRange(quiz.Questions);
            _context.Quizzes.Remove(quiz);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

    
