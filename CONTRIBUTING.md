# Contributing to Programming 3 – Advanced Course Website

This document outlines guidelines and procedures for contributing to the project's development and maintenance.

Repository: https://github.com/P3-MINI/csharp-site  
Website: https://csharp.mini.pw.edu.pl

## Contribution Scope

Contributions are welcome in the following areas:

- Content updates (e.g., correcting errors, improving explanations, updating outdated information)
- Adding new course materials or sections
- Fixing issues related to site rendering or building
- Enhancing structure, accessibility, or formatting

## Submitting Changes

1. Fork the repository and create a new branch from the `master` branch:

   ```bash
   git checkout -b feature/short-description
   ```

2. Make your changes and verify site still builds and displays correctly by running Hugo locally.

   ```bash
   hugo server
   ```

3. Commit with a concise and descriptive message:

   ```bash
   git commit -am "Update Week 2 notes: fixed code example"
   ```

4. Push your branch to your fork:

   ```bash
   git push origin feature/short-description
   ```

5. Open a Pull Request (PR) targeting the `master` branch. Provide a clear summary of your changes.

## Guidelines

- Use consistent formatting and follow the structure used in other parts of the site.
- Keep your changes focused on a single topic or fix. Avoid combining unrelated changes.
- Include only relevant changes in each PR (avoid bundling unrelated updates together).
- Verify that the site builds successfully and renders correctly before submitting a PR.
- Use proper grammar, spelling, and technical language.

## Reporting Issues

To report issues (e.g., bugs, missing/incorrect content, or suggestions):

1. Open a GitHub issue in this repository.
2. Provide a clear and specific description, including:
   - The observed behavior and expected result
   - Steps to reproduce (if applicable)
   - Screenshots or URLs (if relevant)
