-- 1. Önce Quizzes Tablosunu ekliyoruz (Tarihleri GETDATE() ile zorunlu dolduruyoruz)
INSERT INTO Quizzes (Title, Description, CreatedAt, UpdatedAt) 
VALUES ('General Knowledge', 'A set of 10 English questions to test your knowledge.', GETDATE(), GETDATE());

-- Eklenen Quiz'in ID'sini yakalıyoruz
DECLARE @QuizID int = SCOPE_IDENTITY();

-- 2. Soruları ve Cevapları Ekliyoruz
-- Soru 1
INSERT INTO Questions (Text, QuizId) VALUES ('What is the capital city of France?', @QuizID);
DECLARE @Q1 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('London',0,@Q1), ('Paris',1,@Q1), ('Berlin',0,@Q1), ('Madrid',0,@Q1);

-- Soru 2
INSERT INTO Questions (Text, QuizId) VALUES ('Which planet is known as the Red Planet?', @QuizID);
DECLARE @Q2 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Venus',0,@Q2), ('Jupiter',0,@Q2), ('Mars',1,@Q2), ('Saturn',0,@Q2);

-- Soru 3
INSERT INTO Questions (Text, QuizId) VALUES ('What is the chemical symbol for water?', @QuizID);
DECLARE @Q3 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('CO2',0,@Q3), ('H2O',1,@Q3), ('O2',0,@Q3), ('NaCl',0,@Q3);

-- Soru 4
INSERT INTO Questions (Text, QuizId) VALUES ('Who painted the Mona Lisa?', @QuizID);
DECLARE @Q4 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Picasso',0,@Q4), ('Van Gogh',0,@Q4), ('Da Vinci',1,@Q4), ('Dalí',0,@Q4);

-- Soru 5
INSERT INTO Questions (Text, QuizId) VALUES ('Which country is home to the Kangaroo?', @QuizID);
DECLARE @Q5 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Australia',1,@Q5), ('Brazil',0,@Q5), ('India',0,@Q5), ('Egypt',0,@Q5);

-- Soru 6
INSERT INTO Questions (Text, QuizId) VALUES ('Which is the largest ocean on Earth?', @QuizID);
DECLARE @Q6 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Atlantic',0,@Q6), ('Indian',0,@Q6), ('Pacific',1,@Q6), ('Arctic',0,@Q6);

-- Soru 7
INSERT INTO Questions (Text, QuizId) VALUES ('In which year did the Titanic sink?', @QuizID);
DECLARE @Q7 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('1905',0,@Q7), ('1912',1,@Q7), ('1920',0,@Q7), ('1935',0,@Q7);

-- Soru 8
INSERT INTO Questions (Text, QuizId) VALUES ('What is the hardest natural substance on Earth?', @QuizID);
DECLARE @Q8 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Gold',0,@Q8), ('Iron',0,@Q8), ('Diamond',1,@Q8), ('Silver',0,@Q8);

-- Soru 9
INSERT INTO Questions (Text, QuizId) VALUES ('How many players are in a standard football team?', @QuizID);
DECLARE @Q9 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('7',0,@Q9), ('9',0,@Q9), ('11',1,@Q9), ('12',0,@Q9);

-- Soru 10
INSERT INTO Questions (Text, QuizId) VALUES ('Which element has the atomic symbol "O"?', @QuizID);
DECLARE @Q10 int = SCOPE_IDENTITY();
INSERT INTO Answers (AnswerText, IsCorrect, QuestionId) VALUES ('Gold',0,@Q10), ('Oxygen',1,@Q10), ('Osmium',0,@Q10), ('Zinc',0,@Q10);

SELECT 
    (SELECT COUNT(*) FROM Quizzes) AS TotalQuizzes,
    (SELECT COUNT(*) FROM Questions) AS TotalQuestions,
    (SELECT COUNT(*) FROM Answers) AS TotalAnswers;