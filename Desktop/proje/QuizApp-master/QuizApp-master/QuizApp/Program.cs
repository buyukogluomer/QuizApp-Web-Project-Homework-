using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Hubs;
using QuizApp.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(); 

// 1. Veritabanı Bağlantısı (DbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. MVC servislerini ekle
builder.Services.AddControllersWithViews();

// 3. DOKÜMAN GEREĞİ: Session (Oturum) servisini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika işlem yapılmazsa oturum düşer
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. HTTP Context Accessor (Session'a her yerden erişmek için gerekebilir)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.EnsureCreatedAsync();

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'[dbo].[Lobbies]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Lobbies] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [PinCode] NVARCHAR(MAX) NOT NULL,
                [QuizId] INT NOT NULL,
                [IsStarted] BIT NOT NULL,
                [CreatedAt] DATETIME2 NOT NULL
            );
        END
        """);

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        IF OBJECT_ID(N'[dbo].[LobbyPlayers]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[LobbyPlayers] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [LobbyPin] NVARCHAR(MAX) NOT NULL,
                [PlayerName] NVARCHAR(MAX) NULL,
                [Score] INT NOT NULL,
                [ConnectionId] NVARCHAR(MAX) NULL
            );
        END
        """);

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        IF NOT EXISTS (
            SELECT *
            FROM sys.columns
            WHERE object_id = OBJECT_ID(N'[dbo].[Questions]')
              AND name = 'ImageUrl'
        )
        BEGIN
            ALTER TABLE [dbo].[Questions] ADD [ImageUrl] NVARCHAR(MAX) NULL;
        END
        """);

    await dbContext.Database.ExecuteSqlRawAsync(
        """
        UPDATE [dbo].[Questions]
        SET [ImageUrl] = '/images/players/cristiano-ronaldo.jpg'
        WHERE [ImageUrl] LIKE '%Cristiano_Ronaldo%';

        UPDATE [dbo].[Questions]
        SET [ImageUrl] = '/images/players/lionel-messi.jpg'
        WHERE [ImageUrl] LIKE '%Lionel_Messi%';

        UPDATE [dbo].[Questions]
        SET [ImageUrl] = '/images/players/neymar.jpg'
        WHERE [ImageUrl] LIKE '%Neymar%';

        UPDATE [dbo].[Questions]
        SET [ImageUrl] = '/images/players/kevin-de-bruyne.jpg'
        WHERE [ImageUrl] LIKE '%Kevin_De_Bruyne%';
        """);

    await SeedCapitalsQuizAsync(dbContext);
}

// Development ortamı ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

// 5. KRİTİK SIRALAMA: Session, Authentication'dan önce gelmelidir
app.UseSession();

app.UseAuthorization();

// 6. Varsayılan Route (Giriş ekranı Login olsun istersen burayı değiştirebilirsin)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<QuizHub>("/quizHub"); 
app.Run();

static async Task SeedCapitalsQuizAsync(AppDbContext dbContext)
{
    const string quizTitle = "Countries and Capitals";

    var existingQuiz = await dbContext.Quizzes
        .AsNoTracking()
        .FirstOrDefaultAsync(q => q.Title == quizTitle);

    if (existingQuiz != null)
    {
        return;
    }

    var quiz = new Quizzes
    {
        Title = quizTitle,
        Description = "Test your knowledge of world capitals.",
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
    };

    dbContext.Quizzes.Add(quiz);
    await dbContext.SaveChangesAsync();

    var questions = new[]
    {
        new
        {
            Text = "What is the capital of France?",
            CorrectIndex = 1,
            Options = new[] { "Lyon", "Paris", "Marseille", "Nice" }
        },
        new
        {
            Text = "What is the capital of Germany?",
            CorrectIndex = 2,
            Options = new[] { "Munich", "Hamburg", "Berlin", "Frankfurt" }
        },
        new
        {
            Text = "What is the capital of Italy?",
            CorrectIndex = 0,
            Options = new[] { "Rome", "Milan", "Naples", "Turin" }
        },
        new
        {
            Text = "What is the capital of Spain?",
            CorrectIndex = 3,
            Options = new[] { "Barcelona", "Valencia", "Seville", "Madrid" }
        },
        new
        {
            Text = "What is the capital of Japan?",
            CorrectIndex = 1,
            Options = new[] { "Osaka", "Tokyo", "Kyoto", "Hiroshima" }
        },
        new
        {
            Text = "What is the capital of Canada?",
            CorrectIndex = 2,
            Options = new[] { "Toronto", "Vancouver", "Ottawa", "Montreal" }
        },
        new
        {
            Text = "What is the capital of Brazil?",
            CorrectIndex = 1,
            Options = new[] { "Rio de Janeiro", "Brasilia", "Sao Paulo", "Salvador" }
        },
        new
        {
            Text = "What is the capital of Australia?",
            CorrectIndex = 0,
            Options = new[] { "Canberra", "Sydney", "Melbourne", "Perth" }
        },
        new
        {
            Text = "What is the capital of Egypt?",
            CorrectIndex = 3,
            Options = new[] { "Alexandria", "Giza", "Luxor", "Cairo" }
        },
        new
        {
            Text = "What is the capital of Argentina?",
            CorrectIndex = 2,
            Options = new[] { "Cordoba", "Rosario", "Buenos Aires", "Mendoza" }
        }
    };

    foreach (var item in questions)
    {
        var question = new Questions
        {
            QuizId = quiz.Id,
            Text = item.Text,
            Points = 10,
            TimeLimitSecond = 10
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        for (var i = 0; i < item.Options.Length; i++)
        {
            dbContext.Answers.Add(new Answers
            {
                QuestionId = question.QuestionId,
                AnswerText = item.Options[i],
                IsCorrect = i == item.CorrectIndex
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
