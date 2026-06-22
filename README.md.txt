# SecureShield Hub - Cybersecurity Risk Management Dashboard

A WPF-based desktop application designed for security tracking, conversational risk assessment, and user compliance training. Developed as part of the PROG6221 Portfolio of Evidence.

## Features
* **Security Chatbot:** Simulates natural language keyword processing and automated security coaching.
* **Task Manager:** Direct CRUD integration with SQL Server LocalDB to organize defensive workflows.
* **Knowledge Quiz:** A 10-question evaluation engine assessing system security awareness baselines.
* **Activity Auditor:** Automated tracking of user operations logged straight to the database.

## Local Setup Instructions
1. Run the `setup.sql` script inside SQL Server Management Studio (SSMS) to initialize the `SecurityLogDB` instance on `(localdb)\MSSQLLocalDB`.
2. Open the solution file in Visual Studio.
3. Build and launch the application.