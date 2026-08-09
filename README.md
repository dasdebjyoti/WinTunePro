# WinTunePro

![Build](https://github.com/dasdebjyoti/WinTunePro/actions/workflows/dotnet.yml/badge.svg)
![Releases](https://img.shields.io/github/v/release/dasdebjyoti/WinTunePro)
![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)

WinTunePro is a modern, modular Windows optimization and tweaking utility targeted at Windows 11 and future Windows releases. It provides user‑friendly controls to safely adjust performance, privacy, UI, networking, and gaming settings.

This repository contains the WinTunePro application built with .NET 10 and a Windows Forms UI. The project aims to be extensible, data‑driven, and safe by default.

---

## Quick links

- Project: https://github.com/dasdebjyoti/WinTunePro
- Issues: https://github.com/dasdebjyoti/WinTunePro/issues
- Releases: https://github.com/dasdebjyoti/WinTunePro/releases
- License: LICENSE

---

## Key features

- Modular tweak engine (each tweak is a self‑contained unit)
- Registry backup & restore with change history
- PowerShell automation engine for scriptable tweaks
- Service and scheduled task controls
- Admin elevation handling and safe defaults
- Logging and diagnostics
- Preset profiles (planned)

---

## Prerequisites

- Windows 11 (x64 or ARM64 supported where applicable)
- .NET 10 SDK / Runtime installed for development and build
- Visual Studio 2026 (recommended) or the dotnet CLI
- Administrative privileges required for many tweaks (UAC prompt)

---

## Quick start (developer)

Clone the repository and open the solution in Visual Studio or build with dotnet CLI:

PowerShell

cd "D:\\DEV\\WinTunePro"
git clone https://github.com/dasdebjyoti/WinTunePro.git
cd WinTunePro
dotnet build WinTunePro.slnx

Open WinTunePro.slnx in Visual Studio 2026 and run (Debug/Release) as needed.

To run Visual Studio elevated (required for testing admin flows):

1. Close Visual Studio.
2. Right‑click on Visual Studio and choose "Run as administrator".

Or run the compiled executable elevated from PowerShell:

Start-Process -FilePath "path\to\WinTunePro.exe" -Verb RunAs

---

## Usage notes & safety

- Many tweaks modify registry keys or system services. The app will create a registry backup and (optionally) prompt to create a System Restore point before applying risky changes.
- Always review the list of changes before applying a preset.
- The app provides rollback where possible; some changes may require sign‑out, Explorer restart, or full system restart.

---

## Recommended workflow for contributors

1. Fork the repository
2. Create a feature branch: git checkout -b feat/my-tweak
3. Implement changes and add tests where applicable
4. Commit and push to your fork
5. Open a Pull Request against the upstream master branch

See CONTRIBUTING.md for more details (if present).

---

## Roadmap (summary)

Phases include: foundation and core engine (v1.0), essential tweaks pack (v1.1+), advanced tweaks & power tools (v2.0), presets & automation (v2.5), UX polish and safety (v2.8), and future Windows support (v3.0).

Refer to the in‑repo roadmap or project board for the current schedule and issues.

---

## Technology stack

- Language: C# (.NET 10)
- UI: Windows Forms (.NET)
- Automation: PowerShell, Win32 APIs
- CI: GitHub Actions (dotnet build/test)

---

## Security & Privacy

WinTunePro does not collect telemetry by default. Any optional diagnostics or telemetry will be disclosed and opt‑in only. The app operates locally and stores registry backups and logs in the user's profile by default; do not store secrets in those files.

---

## License

This project is released under the MIT License. See the LICENSE file for details.

---

## Contact & support

Open issues on GitHub for bugs, feature requests, or security concerns: https://github.com/dasdebjyoti/WinTunePro/issues

Thank you for using and contributing to WinTunePro!

