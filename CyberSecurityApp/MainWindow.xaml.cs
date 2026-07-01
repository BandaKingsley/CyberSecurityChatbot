#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberSecurityApp
{
    public partial class MainWindow : Window
    {
        private string userName = "";
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SecurityLogDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private List<string> phishingTips = new List<string> { "Check sender domains.", "Hover over links before clicking.", "Never enter passwords via email links.", "Look for generic greetings.", "Verify with IT if urgent." };
        private List<string> passwordTips = new List<string> { "Use 14+ character passphrases.", "Use unique passwords everywhere.", "Use a Password Manager.", "Rotate keys every 90 days.", "Enable MFA." };

        private int currentQuestionIndex = 0, userQuizScore = 0;
        private struct QuizQuestion { public string QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer; }
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion> {
            new QuizQuestion { QuestionText = "What is a phishing attempt?", OptionA = "System Update", OptionB = "Malicious Link", OptionC = "Firewall Check", OptionD = "Routine", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "Strongest protection?", OptionA = "Reusing", OptionB = "MFA", OptionC = "Unencrypted", OptionD = "Short", CorrectAnswer = "B" }
        };

        public MainWindow()
        {
            InitializeComponent();
            SetupInitialSystemState();
            LoadSecurityTasksFromDatabase();
            LoadAuditLogsFromDatabase();
            DisplayActiveQuizQuestion();
        }

        private void SetupInitialSystemState() { ChatHistoryBox.Text = "--- SECURESHIELD AI TERMINAL v2.0 ---\n\nBot: Please enter your name.\n\n"; }

        private void LogActivity(string action)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO AuditLogs (Timestamp, ActionDetails) VALUES (@t, @d)", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", DateTime.Now);
                        cmd.Parameters.AddWithValue("@d", action);
                        cmd.ExecuteNonQuery();
                    }
                }
                AuditLogListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] LOG: {action}");
            }
            catch { }
        }

        private void LoadAuditLogsFromDatabase() { try { using (SqlConnection conn = new SqlConnection(connectionString)) { conn.Open(); using (SqlCommand cmd = new SqlCommand("SELECT Timestamp, ActionDetails FROM AuditLogs ORDER BY Timestamp DESC", conn)) using (SqlDataReader reader = cmd.ExecuteReader()) while (reader.Read()) AuditLogListBox.Items.Add($"[{reader["Timestamp"]}] LOG: {reader["ActionDetails"]}"); } } catch { } }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ExecuteChat();
        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ExecuteChat(); }

        private void ExecuteChat()
        {
            string input = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            ChatHistoryBox.AppendText($"You: {input}\n");
            if (string.IsNullOrEmpty(userName)) { userName = input; ChatHistoryBox.AppendText($"Bot: Hello {userName}. Ask me about 'phishing' or 'password'.\n\n"); }
            else
            {
                string lower = input.ToLower();
                Random rand = new Random();
                if (lower.Contains("phishing")) ChatHistoryBox.AppendText($"Bot: {phishingTips[rand.Next(phishingTips.Count)]}\n\n");
                else if (lower.Contains("password")) ChatHistoryBox.AppendText($"Bot: {passwordTips[rand.Next(passwordTips.Count)]}\n\n");
                else ChatHistoryBox.AppendText("Bot: Try 'phishing' or 'password'.\n\n");
            }
            ChatInputBox.Clear();
            LogActivity($"Query: {input}");
        }

        private void LoadSecurityTasksFromDatabase() { try { using (SqlConnection conn = new SqlConnection(connectionString)) { conn.Open(); using (SqlDataReader reader = new SqlCommand("SELECT TaskName, Category, Priority FROM SecurityTasks", conn).ExecuteReader()) { System.Data.DataTable dt = new System.Data.DataTable(); dt.Load(reader); TasksDataGrid.ItemsSource = dt.DefaultView; } } } catch { } }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string task = TaskNameInput.Text;
            try { using (SqlConnection conn = new SqlConnection(connectionString)) { conn.Open(); using (SqlCommand cmd = new SqlCommand("INSERT INTO SecurityTasks (TaskName, Category, Priority) VALUES (@n, @c, @p)", conn)) { cmd.Parameters.AddWithValue("@n", task); cmd.Parameters.AddWithValue("@c", (CategoryComboBox.SelectedItem as ComboBoxItem).Content.ToString()); cmd.Parameters.AddWithValue("@p", (PriorityComboBox.SelectedItem as ComboBoxItem).Content.ToString()); cmd.ExecuteNonQuery(); } } LogActivity("Added: " + task); LoadSecurityTasksFromDatabase(); } catch { }
        }

        private void DisplayActiveQuizQuestion() { if (currentQuestionIndex < quizQuestions.Count) { var q = quizQuestions[currentQuestionIndex]; QuizProgressText.Text = $"Q{currentQuestionIndex + 1}"; QuestionTextBlock.Text = q.QuestionText; OptionARadio.Content = q.OptionA; OptionBRadio.Content = q.OptionB; OptionCRadio.Content = q.OptionC; OptionDRadio.Content = q.OptionD; } }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = (OptionARadio.IsChecked == true) ? "A" : (OptionBRadio.IsChecked == true) ? "B" : (OptionCRadio.IsChecked == true) ? "C" : (OptionDRadio.IsChecked == true) ? "D" : "";
            if (selected == quizQuestions[currentQuestionIndex].CorrectAnswer) userQuizScore += 10;
            currentQuestionIndex++;
            if (currentQuestionIndex < quizQuestions.Count) { DisplayActiveQuizQuestion(); QuizScoreText.Text = $"Score: {userQuizScore}"; } else { MessageBox.Show("Finished! Score: " + userQuizScore); currentQuestionIndex = 0; userQuizScore = 0; DisplayActiveQuizQuestion(); }
            LogActivity("Quiz answered.");
        }
    }
}