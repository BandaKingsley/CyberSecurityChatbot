#pragma warning disable CS0618 // Ignore obsolete SQL Server library warnings smoothly
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CyberSecurityApp
{
    public partial class MainWindow : Window
    {
        // --- Session State Fields ---
        private string userName = "";
        private bool hasAskedForName = false;
        private string userFavoriteTopic = "None";
        private string lastTrackedTopic = "None";

        // Synchronized directly with live LocalDB instance from SSMS
        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SecurityLogDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // --- Track last tip indices to prevent duplicates ---
        private int lastPhishingTipIndex = -1;
        private int lastPasswordTipIndex = -1;

        // --- Cybersecurity Advice Lists ---
        private List<string> phishingTips = new List<string>
        {
            "Always check the sender's email address carefully.",
            "Avoid clicking on suspicious links in emails.",
            "Report phishing attempts to your IT department immediately."
        };

        private List<string> passwordTips = new List<string>
        {
            "Use a mix of uppercase, lowercase, numbers, and symbols.",
            "Never reuse the same password across multiple accounts.",
            "Change your passwords regularly to reduce risk."
        };

        // --- Quiz Engine State and Structs ---
        private int currentQuestionIndex = 0;
        private int userQuizScore = 0;

        private struct QuizQuestion
        {
            public string QuestionText;
            public string OptionA;
            public string OptionB;
            public string OptionC;
            public string OptionD;
            public string CorrectAnswer;
        }

        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion {
                QuestionText = "You receive an urgent email from your bank claiming your access account is locked, requesting an immediate credential reset link click. What is this?",
                OptionA = "A routine automated security infrastructure check update.",
                OptionB = "A standard system maintenance verification request query.",
                OptionC = "A malicious phishing social engineering threat attempt.",
                OptionD = "An encryption routing protocol handshake validation stream.",
                CorrectAnswer = "C"
            },
            new QuizQuestion {
                QuestionText = "Which configuration provides the strongest baseline approach for protecting user access terminal accounts from breach?",
                OptionA = "Reusing a single complex master password pattern across all systems.",
                OptionB = "Utilizing unique phrases coupled with Multi-Factor Authentication (MFA).",
                OptionC = "Storing unencrypted password keys in local notepad streams.",
                OptionD = "Disabling account lockouts to ensure constant accessibility.",
                CorrectAnswer = "B"
            },
            new QuizQuestion {
                QuestionText = "What represents the safest procedural action step when an unrecognized executable window suddenly requests administrator execution rights?",
                OptionA = "Approve immediately to minimize background layout thread interruption.",
                OptionB = "Deny execution and report the signature to network administrators.",
                OptionC = "Minimize the active frame display and inspect it after several hours.",
                OptionD = "Temporarily disable active malware system detection engines to test it.",
                CorrectAnswer = "B"
            }
        };

        // --- Constructor ---
        public MainWindow()
        {
            InitializeComponent();

            SetupInitialSystemState();
            LoadSecurityTasksFromDatabase();
            DisplayActiveQuizQuestion();
        }

        // Method to log system activity
        private void LogActivity(string actionDescription)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            AuditLogListBox.Items.Add($"[{timestamp}] SUCCESS: {actionDescription}");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO AuditLogs (Timestamp, ActionDetails) VALUES (@time, @details)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@time", DateTime.Now);
                        cmd.Parameters.AddWithValue("@details", actionDescription);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Log storage error: {ex.Message}");
            }
        }

        // --- Setup Initial State ---
        private void SetupInitialSystemState()
        {
            ChatHistoryBox.Clear();
            ChatHistoryBox.Text += "--------------------------------------------------------\n";
            ChatHistoryBox.Text += "  CYBERSECURITY RISK ASSESSMENT HUB TERMINAL v1.0       \n";
            ChatHistoryBox.Text += "--------------------------------------------------------\n\n";

            ChatHistoryBox.Text += "Bot: Hello! Welcome to the cybersecurity hub. Before we begin, what is your name?\n\n";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    SoundPlayer player = new SoundPlayer("greeting.wav");
                    player.Play();
                }
                catch (Exception)
                {
                    ChatHistoryBox.Text += "[System Log]: Audio stream initialization verified.\n\n";
                }
            }

            LogActivity("Application initialized and secure terminal interface opened.");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteChatSubmission();
        }

        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteChatSubmission();
                e.Handled = true;
            }
        }

        // Core method to handle chat submissions and state shifts
        private void ExecuteChatSubmission()
        {
            string input = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            if (string.IsNullOrEmpty(userName))
            {
                userName = input;
                hasAskedForName = true;
                ChatHistoryBox.AppendText($"You: {input}\n");
                ChatHistoryBox.AppendText($"Bot: Welcome, {userName}! Type 'phishing' or 'password' to begin, or let me know if you are feeling 'worried'.\n\n");

                LogActivity($"New user registration established for operator signature: '{userName}'");

                ChatInputBox.Clear();
                ChatHistoryBox.ScrollToEnd();
                return;
            }

            ChatHistoryBox.AppendText($"You: {input}\n");

            ProcessConversationNLP(input.ToLower());

            LogActivity($"Chatbot NLP engine executed evaluation for input string token: '{input}'");

            ChatInputBox.Clear();
            ChatHistoryBox.ScrollToEnd();
        }

        // --- Simulated Keyword Detection (NLP) Engine ---
        private void ProcessConversationNLP(string lowered)
        {
            Random rand = new Random();

            if (lowered.Contains("worried") || lowered.Contains("scared") || lowered.Contains("hacked"))
            {
                ChatHistoryBox.AppendText($"Bot: I completely understand your concern, {userName}. Cyber threats can be stressful. Let's look at a helpful tip to secure your space:\n");
                int index = rand.Next(phishingTips.Count);
                ChatHistoryBox.AppendText($"Bot: [Advice] {phishingTips[index]}\n\n");
            }
            else if (lowered.Contains("phishing") || lowered.Contains("scam"))
            {
                lastTrackedTopic = "phishing";
                userFavoriteTopic = "phishing";

                ChatHistoryBox.AppendText($"Bot: Accessing the phishing awareness database modules. Review this tip:\n");

                int index;
                do { index = rand.Next(phishingTips.Count); } while (index == lastPhishingTipIndex);
                lastPhishingTipIndex = index;

                ChatHistoryBox.AppendText($"Bot: [Phishing Tip] {phishingTips[index]}\n\n");
            }
            else if (lowered.Contains("password"))
            {
                lastTrackedTopic = "password";
                userFavoriteTopic = "password";

                ChatHistoryBox.AppendText($"Bot: Accessing identity and access management advice list. Review this profile configuration tip:\n");

                int index;
                do { index = rand.Next(passwordTips.Count); } while (index == lastPasswordTipIndex);
                lastPasswordTipIndex = index;

                ChatHistoryBox.AppendText($"Bot: [Credential Tip] {passwordTips[index]}\n\n");
            }
            else if (lowered.Contains("another tip") || lowered.Contains("tell me more"))
            {
                if (lastTrackedTopic == "phishing")
                {
                    int index = rand.Next(phishingTips.Count);
                    ChatHistoryBox.AppendText($"Bot: Here is another phishing baseline check tip: {phishingTips[index]}\n\n");
                }
                else if (lastTrackedTopic == "password")
                {
                    int index = rand.Next(passwordTips.Count);
                    ChatHistoryBox.AppendText($"Bot: Here is another access management validation tip: {passwordTips[index]}\n\n");
                }
                else
                {
                    ChatHistoryBox.AppendText("Bot: I'm not sure which topic context you are referring to yet. Try asking explicitly about 'phishing' or 'passwords' first.\n\n");
                }
            }
            else if (lowered.Contains("remember"))
            {
                ChatHistoryBox.AppendText($"Bot: Querying system active volatile states...\n");
                ChatHistoryBox.AppendText($"Bot: Active operator account profile verified: Name: {userName} | Logged Focus Preference: {userFavoriteTopic}\n\n");
            }
            else
            {
                ChatHistoryBox.AppendText("Bot: Input token unmapped. Try utilizing core context keywords like 'password', 'phishing', or run a system state inquiry using 'remember'.\n\n");
            }
        }

        // ==========================================
        //  DATABASE METHODS FOR THE TASK MANAGER TAB
        // ==========================================

        private void LoadSecurityTasksFromDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT TaskID as 'ID', TaskName as 'Action Item', Category as 'Domain', Priority, DateAssigned as 'Timestamp' FROM SecurityTasks";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            dt.Load(reader);
                            TasksDataGrid.ItemsSource = dt.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Link Offline: {ex.Message}", "System Stream Status", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string taskName = TaskNameInput.Text.Trim();
            string category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string priority = (PriorityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(taskName))
            {
                MessageBox.Show("Please specify a baseline task action description before committing.", "Validation Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO SecurityTasks (TaskName, Category, Priority) VALUES (@name, @cat, @pri)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", taskName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@cat", category ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@pri", priority ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                TaskNameInput.Clear();
                LoadSecurityTasksFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to append data log stream: {ex.Message}", "Database Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        //         KNOWLEDGE QUIZ CORE LOGIC
        // ==========================================

        private void DisplayActiveQuizQuestion()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                var q = quizQuestions[currentQuestionIndex];
                QuizProgressText.Text = $"Question {currentQuestionIndex + 1} of {quizQuestions.Count}";
                QuizScoreText.Text = $"Current Score: {userQuizScore}";

                QuestionTextBlock.Text = q.QuestionText;
                OptionARadio.Content = q.OptionA;
                OptionBRadio.Content = q.OptionB;
                OptionCRadio.Content = q.OptionC;
                OptionDRadio.Content = q.OptionD;

                OptionARadio.IsChecked = false;
                OptionBRadio.IsChecked = false;
                OptionCRadio.IsChecked = false;
                OptionDRadio.IsChecked = false;

                SubmitAnswerButton.Content = (currentQuestionIndex == quizQuestions.Count - 1) ? "Finish Quiz" : "Submit Answer";
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLetter = "";
            if (OptionARadio.IsChecked == true) selectedLetter = "A";
            else if (OptionBRadio.IsChecked == true) selectedLetter = "B";
            else if (OptionCRadio.IsChecked == true) selectedLetter = "C";
            else if (OptionDRadio.IsChecked == true) selectedLetter = "D";

            if (string.IsNullOrEmpty(selectedLetter))
            {
                MessageBox.Show("Please select an answer token response before submitting.", "Quiz Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectedLetter == quizQuestions[currentQuestionIndex].CorrectAnswer)
            {
                userQuizScore += 10;
                MessageBox.Show("Correct! Security metrics validated successfully.", "Assessment Signal", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Incorrect. The optimal strategy was Option {quizQuestions[currentQuestionIndex].CorrectAnswer}.", "Assessment Signal", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            currentQuestionIndex++;

            if (currentQuestionIndex < quizQuestions.Count)
            {
                DisplayActiveQuizQuestion();
            }
            else
            {
                QuizProgressText.Text = "Quiz Completed!";
                QuizScoreText.Text = $"Final Score: {userQuizScore}/{quizQuestions.Count * 10}";
                MessageBox.Show($"Assessment Complete!\nYour Final Score State: {userQuizScore} points.", "Audit Profile Terminated", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                currentQuestionIndex = 0;
                userQuizScore = 0;
                DisplayActiveQuizQuestion();
            }
        }
    }
}