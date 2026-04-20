IF NOT EXISTS (
    SELECT *
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Questions]')
      AND name = 'ImageUrl'
)
BEGIN
    ALTER TABLE [dbo].[Questions] ADD [ImageUrl] NVARCHAR(MAX) NULL;
END

INSERT INTO Quizzes (Title, Description, CreatedAt, UpdatedAt)
VALUES ('Football Legends', 'Guess the football player from the photo.', GETDATE(), GETDATE());

DECLARE @QuizID INT = SCOPE_IDENTITY();

INSERT INTO Questions (Text, ImageUrl, QuizId, Points, TimeLimitSecond)
VALUES
('Who is this football player?', '/images/players/lionel-messi.jpg', @QuizID, 10, 30);
DECLARE @Q1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId)
VALUES ('Lionel Messi', 1, @Q1), ('Cristiano Ronaldo', 0, @Q1), ('Neymar', 0, @Q1), ('Kevin De Bruyne', 0, @Q1);

INSERT INTO Questions (Text, ImageUrl, QuizId, Points, TimeLimitSecond)
VALUES
('Who is this football player?', '/images/players/cristiano-ronaldo.jpg', @QuizID, 10, 30);
DECLARE @Q2 INT = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId)
VALUES ('Kylian Mbappe', 0, @Q2), ('Cristiano Ronaldo', 1, @Q2), ('Erling Haaland', 0, @Q2), ('Neymar', 0, @Q2);

INSERT INTO Questions (Text, ImageUrl, QuizId, Points, TimeLimitSecond)
VALUES
('Who is this football player?', '/images/players/neymar.jpg', @QuizID, 10, 30);
DECLARE @Q3 INT = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId)
VALUES ('Kevin De Bruyne', 0, @Q3), ('Neymar', 1, @Q3), ('Lionel Messi', 0, @Q3), ('Mohamed Salah', 0, @Q3);

INSERT INTO Questions (Text, ImageUrl, QuizId, Points, TimeLimitSecond)
VALUES
('Who is this football player?', '/images/players/kevin-de-bruyne.jpg', @QuizID, 10, 30);
DECLARE @Q4 INT = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId)
VALUES ('Vinicius Junior', 0, @Q4), ('Cristiano Ronaldo', 0, @Q4), ('Kevin De Bruyne', 1, @Q4), ('Robert Lewandowski', 0, @Q4);

SELECT
    q.QuestionId,
    q.Text,
    q.ImageUrl
FROM Questions q
WHERE q.QuizId = @QuizID;
