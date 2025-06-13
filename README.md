# Jira Time Logger for Windows 11

A user-friendly Windows 11 desktop application to log hours to Jira tickets, with features for active time tracking and offline/non-ticketed work.

## Core Features
- **Jira Integration:** Log time directly to Jira tickets, including work descriptions (comments) using Atlassian Document Format (ADF).
- **Timer Functionality:** Start, pause, stop, and display elapsed time for each work session.
- **Automatic Pause:** Detect workstation lock/logoff and pause timer automatically.
- **Tracking Without Jira ID:** Track hours without a Jira ID and associate them later.
- **Simple UI:** Intuitive, modern, and branded window for easy access.
- **Brand Colors:** Uses official Pharos brand colors for a professional look.
- **Status Feedback:** Shows a "Time logged" message after successful logging.
- **Offline Support:** Local data storage for timer state and logs.

## Technical Considerations
- Jira API interaction (authentication, logging work, validating IDs, ADF comment formatting)
- Windows 11 app development (WPF UI, timer logic, system event handling)
- Local data storage for timer state and logs
- .NET 8, C# 12, WPF, Windows Forms interop for tray icon

## Getting Started
1. Clone the repository.
2. Set your Jira credentials as environment variables: `JIRA_EMAIL` and `JIRA_API_TOKEN`.
3. Build and run the solution with Visual Studio 2022 or later.
4. Log time to Jira issues and track your work efficiently!

---

This project targets .NET 8 and is designed for practical application development with AI assistance.
