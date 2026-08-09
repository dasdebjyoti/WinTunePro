# Contributing to WinTunePro

Thanks for your interest in contributing to WinTunePro. We welcome contributions of all kinds — bug reports, feature requests, documentation improvements, and code changes.

This document explains the preferred workflow, repository conventions, and how to prepare a high‑quality contribution.

---

## Quick start (recommended)

1. Fork the repository on GitHub.
2. Clone your fork:

   git clone https://github.com/<your-username>/WinTunePro.git
   cd WinTunePro

3. Create a topic branch for your change:

   git checkout -b feat/my-feature

4. Implement changes, add tests, and update documentation.
5. Run build and tests locally:

   dotnet build WinTunePro.slnx
   dotnet test

6. Commit with a clear message and push to your fork:

   git add .
   git commit -m "Add: short, descriptive message"
   git push origin feat/my-feature

7. Open a Pull Request against `master` in the upstream repository. Fill the PR description with the problem, your changes, and any manual verification steps.

---

## Branching & workflow

- Base branch: `master` (keep it stable). Create feature branches from `master`.
- Branch names: use readable, prefixed names like `feat/`, `fix/`, `chore/`, `docs/` (e.g. `feat/preset-profiles`).
- Make small, focused PRs. Each PR should do one logical thing and include tests where appropriate.

---

## Code style & conventions

- Language: C# targeting .NET 10.
- IDE: Visual Studio 2026 recommended, but command line `dotnet` tooling works.
- Follow standard .NET naming and formatting conventions. We recommend enabling an `.editorconfig` in your IDE.
- Keep public APIs documented with XML comments for new types/methods.
- Avoid large formatting-only changes in the same commit as functional changes.

---

## Tests

- Add unit tests for new or changed behavior where applicable.
- Use `dotnet test` to run tests locally. CI will run the test suite on PRs.
- If your change is non‑functional (docs, README), mark the PR accordingly.

---

## Pull request checklist

Before creating a PR, ensure:

- [ ] The code builds: `dotnet build`
- [ ] Tests pass locally: `dotnet test`
- [ ] You added or updated tests for new behavior
- [ ] You updated README or relevant documentation when adding user‑facing features
- [ ] The PR description explains the purpose and how to test

---

## Issue reporting

- Use the GitHub Issues page to report bugs or request features.
- Provide a concise title, steps to reproduce, and expected vs actual behavior. Attach logs/screenshots where helpful.

---

## Security issues

Do not open a public issue for security vulnerabilities. Instead, contact the maintainers privately (open an issue labeled `private` or use the GitHub security advisories workflow) and provide enough detail to triage. We will respond and coordinate a fix.

---

## Coding tips for WinTunePro

- For registry and system changes:
  - Always implement a backup/restore path and include tests for the backup logic where possible.
  - Mark UI actions that require admin elevation and provide clear undo behavior.
- Keep tweaks modular. Each tweak should be encapsulated in a single class implementing the tweak interface (see docs or codebase for `ITweak`/`TweakBase`).
- Keep user data and logs in the user profile (not application install folders) and avoid storing secrets.

---

## License and contributor agreement

By contributing, you agree that your contributions are licensed under the project MIT License (see LICENSE). If you cannot accept this, please contact the maintainers before submitting.

---

## Contact

For general questions, open an issue. For urgent or security matters, use the private security contact method described above.

Thank you — contributions make WinTunePro better!