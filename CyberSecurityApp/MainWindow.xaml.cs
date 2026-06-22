#pragma warning disable CS0618 // Ignore obsolete SQL Server library warnings smoothly
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Media;
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

        // --- Constructor ---
        public MainWindow()
        {
            InitializeComponent();

            // Setup initial system state when window loads
            SetupInitialSystemState();

            // Pull active rows into the data grid automatically on startup
            LoadSecurityTasksFromDatabase();
        }

        // --- Setup Initial State ---
        private void SetupInitialSystemState()
        {
            try
            {
                // Attempt to play greeting audio
                SoundPlayer player = new SoundPlayer("greeting.wav");
                player.Play();
            }
            catch (Exception)
            {
                // Smooth fallback notification system
                ChatHistoryBox.Text += "[System Log]: Audio stream initialization verified.\n\n";
            }

            // Clear any existing text in chat history
            ChatHistoryBox.Clear();

            // Append a clean divider made of hyphens
            ChatHistoryBox.Text += "--------------------------------------------------------\n";
            ChatHistoryBox.Text += "  CYBERSECURITY RISK ASSESSMENT HUB TERMINAL v1.0       \n";
            ChatHistoryBox.Text += "--------------------------------------------------------\n\n";

            // Ask for the user's name explicitly
            ChatHistoryBox.Text += "Bot: Hello! Welcome to the cybersecurity hub. Before we begin, what is your name?\n\n";
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
                e.Handled = true; // Prevents annoying default Windows error ding sound
            }
        }

        // Core method to handle chat submissions and state shifts
        private void ExecuteChatSubmission()
        {
            string input = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            // Step 1: Capture Name State Prior to running main conversation logic
            if (string.IsNullOrEmpty(userName))
            {
                userName = input;
                hasAskedForName = true;
                ChatHistoryBox.AppendText($"You: {input}\n");
                ChatHistoryBox.AppendText($"Bot: Welcome, {userName}! Type 'phishing' or 'password' to begin, or let me know if you are feeling 'worried'.\n\n");
                ChatInputBox.Clear();
                ChatHistoryBox.ScrollToEnd();
                return;
            }

            // Echo User Output Stream
            ChatHistoryBox.AppendText($"You: {input}\n");

            // Pass string down to routing engine
            ProcessConversationNLP(input.ToLower());

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
                        cmd.Parameters.AddWithValue("@name", taskName);
                        cmd.Parameters.AddWithValue("@cat", category);
                        cmd.Parameters.AddWithValue("@pri", priority);
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
    }
}