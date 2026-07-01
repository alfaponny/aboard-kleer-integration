# Aboard–Kleer Integration

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/tests-xUnit-25A162)](https://xunit.net/)

A .NET service that synchronizes employee data from **Aboard** (HR system) to **Kleer** (payroll system). It runs on a schedule, detects changes, updates Kleer accordingly, and sends Slack notifications for unmatched or offboarded employees.

> **Note:** This is a sanitized portfolio version of an integration I built during my internship in April 2026 at a web development company. Credentials, internal endpoints, and deployment-specific details have been removed or replaced with placeholders. The code shown reflects my own implementation work on the project.

---

## Table of Contents

- [How It Works](#how-it-works)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Running](#running)
- [Tests](#tests)

---

## How It Works

1. Fetches all employees from Aboard (with pagination) and Kleer
2. Matches users by work email
3. Compares fields (name, email, address, phone, etc.) and updates Kleer when changes are detected
4. Identifies employees in Kleer but not in Aboard (potential offboarding)
5. Sends Slack notifications for unmatched and offboarded users

The sync runs automatically every Sunday at midnight via Quartz scheduling.

## Tech Stack

| Component          | Choice                  |
| ------------------ | ----------------------- |
| Language / Runtime | .NET 10.0 / C#          |
| Scheduling         | Quartz.NET (cron-based) |
| Deployment         | Docker                  |
| Testing            | xUnit                   |

## Project Structure

```
Project/src/
├── Program.cs                 # DI setup & Quartz scheduling
├── Config/                    # KleerOptions, AboardOptions, SlackOptions
├── Models/                    # Data models (User, Aboard/Kleer API models)
├── Services/
│   ├── Aboard/                # Fetches employee data from Aboard (JSON, Bearer auth)
│   ├── Kleer/                 # Reads/updates Kleer profiles & payroll (XML, token auth)
│   └── Slack/                 # Sends webhook notifications
├── Sync/
│   └── UserSyncService.cs    # Core sync orchestration logic
└── Jobs/
    └── UserSyncJob.cs         # Quartz job wrapper
```

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and fill in your own credentials:

```json
{
  "Kleer": {
    "BaseUrl": "https://api.kleer.se/v1/",
    "ApiKey": "<your-token>",
    "CompanyId": "<your-company-id>"
  },
  "Aboard": {
    "BaseUrl": "https://api.aboardhr.com/v1/",
    "ApiKey": "<your-bearer-token>"
  },
  "Slack": {
    "WebhookUrl": "<your-slack-webhook-url>"
  }
}
```

> No real credentials are included in this repository. You'll need your own Aboard and Kleer API access to run this against live data.

## Running

**Local**

This project runs on a cron schedule. To trigger the integration manually, see the comments in `Program.cs`.

```bash
DOTNET_ENVIRONMENT=Development dotnet run
```

**Docker**

```bash
docker build -t aboard-kleer-integration:latest ./Project
docker run aboard-kleer-integration:latest
```

## Tests

```bash
dotnet test
```

