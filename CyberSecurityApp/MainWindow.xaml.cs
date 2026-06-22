#pragma warning disable CS0618 // Ignore obsolete SQL Server library warnings smoothly
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberSecurityApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml - Controls Cybersecurity Hub dashboard.
    /// </summary>
    public partial class MainWindow : Window
    {
        private string userName = "";
        // Connection string configured for LocalDB (Standard for PROG6221)
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SecurityLogDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // Chatbot response sets
        private List<string> phishingTips = new List<string> { "Always check the sender's email address.", "Avoid clicking suspicious links.", "Report phishing to IT immediately." };
        private List<string> passwordTips = new List<string> { "Use a mix of characters.", "Use a Password Manager.", "Enable 2FA wherever possible." };

        private int currentQuestionIndex = 0, userQuizScore = 0;

        // Structured data model for Quiz questions ensuring clean data handling
        private struct QuizQuestion { public string QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer; }

        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion { QuestionText = "Bank email says account locked. What is this?", OptionA = "Routine maintenance", OptionB = "System check", OptionC = "Malicious phishing", OptionD = "Encryption", CorrectAnswer = "C" },
            new QuizQuestion { QuestionText = "Strongest account protection?", OptionA = "Reusing passwords", OptionB = "Unique phrases + MFA", OptionC = "Unencrypted keys", OptionD = "No lockout", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "Unknown executable requests admin rights?", OptionA = "Approve", OptionB = "Deny and report", OptionC = "Inspect later", OptionD = "Disable AV", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "Goal of Social Engineering?", OptionA = "Exploit bugs", OptionB = "Manipulate individuals", OptionC = "Brute force", OptionD = "Optimize", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "Indicates a secure website?", OptionA = "http://", OptionB = "Pop-ups", OptionC = "https:// and padlock", OptionD = "Asks for passwords", CorrectAnswer = "C" },
            new QuizQuestion { QuestionText = "Danger of public Wi-Fi?", OptionA = "Slow speed", OptionB = "Data interception", OptionC = "Compatibility", OptionD = "Updates", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "Why use a Password Manager?", OptionA = "Sharing", OptionB = "Store unique complex passwords", OptionC = "Avoid login", OptionD = "Speed", CorrectAnswer = "B" },
            new QuizQuestion { QuestionText = "What does MFA stand for?", OptionA = "Multi-Factor Authentication", OptionB = "Main Firewall", OptionC = "Mobile Archive", OptionD = "Master Format", CorrectAnswer = "A" },
            new QuizQuestion { QuestionText = "Symptom of ransomware?", OptionA = "Files encrypted", OptionB = "Screen bright", OptionC = "Mouse fast", OptionD = "System fast", CorrectAnswer = "A" },
            new QuizQuestion { QuestionText = "Handle accidental suspicious link click?", OptionA = "Do nothing", OptionB = "Disconnect and report", OptionC = "Clear history", OptionD = "Ignore", CorrectAnswer = "B" }
        };

        public MainWindow()
        {
            InitializeComponent();
            SetupInitialSystemState();
            LoadSecurityTasksFromDatabase();
            LoadAuditLogsFromDatabase();
            DisplayActiveQuizQuestion();
        }

        private void SetupInitialSystemState()
        {
            ChatHistoryBox.Text = "--- SECURITY HUB TERMINAL v1.0 ---\n\nBot: Hello! Please enter your name to start.\n\n";
            LogActivity("Application Initialized.");
        }

        // Centralized logging method to track system events in the AuditLog database table
        private void LogActivity(string action)
        {
            AuditLogListBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] SUCCESS: {action}");
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Using SQL parameters to sanitize input and prevent SQL Injection attacks
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO AuditLogs (Timestamp, ActionDetails) VALUES (@t, @d)", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", DateTime.Now);
                        cmd.Parameters.AddWithValue("@d", action);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private void LoadAuditLogsFromDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Timestamp, ActionDetails FROM AuditLogs ORDER BY Timestamp ASC", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                        while (reader.Read()) AuditLogListBox.Items.Add($"[{reader["Timestamp"]}] SUCCESS: {reader["ActionDetails"]}");
                }
            }
            catch { }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => ExecuteChat();
        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ExecuteChat(); }

        private void ExecuteChat()
        {
            string input = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            if (string.IsNullOrEmpty(userName))
            {
                userName = input;
                ChatHistoryBox.AppendText($"You: {input}\nBot: Welcome {userName}! Ask me about 'phishing' or 'password'.\n\n");
            }
            else
            {
                ChatHistoryBox.AppendText($"You: {input}\n");
                Random rand = new Random();
                if (input.ToLower().Contains("phishing")) ChatHistoryBox.AppendText($"Bot: {phishingTips[rand.Next(phishingTips.Count)]}\n\n");
                else if (input.ToLower().Contains("password")) ChatHistoryBox.AppendText($"Bot: {passwordTips[rand.Next(passwordTips.Count)]}\n\n");
                else ChatHistoryBox.AppendText("Bot: I did not understand that. Try 'phishing' or 'password'.\n\n");
            }
            ChatInputBox.Clear();
            LogActivity("Chat interaction processed.");
        }

        private void LoadSecurityTasksFromDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlDataReader reader = new SqlCommand("SELECT TaskName, Category, Priority FROM SecurityTasks", conn).ExecuteReader())
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Load(reader);
                        TasksDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Database Connection Error: " + ex.Message); }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string task = TaskNameInput.Text.Trim();
            if (string.IsNullOrEmpty(task)) { MessageBox.Show("Please enter a task name."); return; }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO SecurityTasks (TaskName, Category, Priority) VALUES (@n, @c, @p)", conn))
                    {
                        cmd.Parameters.AddWithValue("@n", task);
                        cmd.Parameters.AddWithValue("@c", (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                        cmd.Parameters.AddWithValue("@p", (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
                LogActivity("Task Added: " + task);
                LoadSecurityTasksFromDatabase();
            }
            catch (Exception ex) { MessageBox.Show("Error committing task: " + ex.Message); }
        }

        private void DisplayActiveQuizQuestion()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                var q = quizQuestions[currentQuestionIndex];
                QuizProgressText.Text = $"Question {currentQuestionIndex + 1} of 10";
                QuestionTextBlock.Text = q.QuestionText;
                OptionARadio.Content = q.OptionA; OptionBRadio.Content = q.OptionB;
                OptionCRadio.Content = q.OptionC; OptionDRadio.Content = q.OptionD;
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = (OptionARadio.IsChecked == true) ? "A" : (OptionBRadio.IsChecked == true) ? "B" : (OptionCRadio.IsChecked == true) ? "C" : (OptionDRadio.IsChecked == true) ? "D" : "";
            if (string.IsNullOrEmpty(selected)) { MessageBox.Show("Please select an option."); return; }
            if (selected == quizQuestions[currentQuestionIndex].CorrectAnswer) userQuizScore += 10;

            currentQuestionIndex++;
            if (currentQuestionIndex < quizQuestions.Count)
            {
                DisplayActiveQuizQuestion();
                QuizScoreText.Text = $"Current Score: {userQuizScore}";
            }
            else
            {
                MessageBox.Show($"Quiz Complete! Final Score: {userQuizScore}");
                currentQuestionIndex = 0; userQuizScore = 0;
                DisplayActiveQuizQuestion();
                QuizScoreText.Text = "Current Score: 0";
            }
            LogActivity("Quiz question completed.");
        }
    }
}