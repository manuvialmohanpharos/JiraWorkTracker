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

### Option 1: Build from Source (Developers)
1. Clone the repository.
2. Build and run the solution with Visual Studio 2022 or later.
3. On first launch, you will be prompted to enter your Jira email and API token. These will be securely saved to `config.json` for future use.
4. Log time to Jira issues and track your work efficiently!

### Option 2: Install Using the Windows Installer (Recommended for End Users)
1. Download the latest `JiraTimeLoggerSetup.exe` installer from your team share or release location.
2. Run the installer and follow the prompts to install Jira Time Logger.
3. Launch Jira Time Logger from the Start Menu or desktop shortcut.
4. On first launch, you will be prompted to enter your Jira email and API token. These will be securely saved to `config.json` for future use.

---

## How to Generate Your Jira API Token

Each user must generate their own Jira API token for authentication. Follow these steps:

1. Go to [Atlassian API tokens page](https://id.atlassian.com/manage-profile/security/api-tokens).
2. Click **Create API token**.
3. Enter a label (e.g., "Jira Time Logger") and click **Create**.
4. Copy the generated API token (it will only be shown once).
5. When prompted by the app, enter your Jira email and the API token you just generated.

---

## How to Publish and Create the Installer (For Maintainers)

1. Publish the application as a single executable file:dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true   The output will be in `bin/Release/net8.0-windows/win-x64/publish/`.
2. Ensure `config.json.template` is present in the project root (optional for reference).
3. Use the provided `JiraTimeLoggerInstaller.iss` script with [Inno Setup](https://jrsoftware.org/isinfo.php) to create the installer:
   - Open `JiraTimeLoggerInstaller.iss` in Inno Setup Compiler.
   - Build the installer. The script expects the exe at `bin/Release/net8.0-windows/win-x64/publish/JiraWorkTracker.exe` and the config template in the project root.
4. Distribute the generated `JiraTimeLoggerSetup.exe` to your team.

---

This project targets .NET 8 and is designed for practical application development with AI assistance.
