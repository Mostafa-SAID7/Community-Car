# Contributing to Community Car

First off, thank you for considering contributing to Community Car! It's people like you that make Community Car such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

* **Use a clear and descriptive title**
* **Describe the exact steps to reproduce the problem**
* **Provide specific examples to demonstrate the steps**
* **Describe the behavior you observed after following the steps**
* **Explain which behavior you expected to see instead and why**
* **Include screenshots and animated GIFs if possible**
* **Include your environment details** (OS, .NET version, browser, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

* **Use a clear and descriptive title**
* **Provide a step-by-step description of the suggested enhancement**
* **Provide specific examples to demonstrate the steps**
* **Describe the current behavior and explain the behavior you expected to see**
* **Explain why this enhancement would be useful**

### Pull Requests

* Fill in the required template
* Do not include issue numbers in the PR title
* Follow the C# coding style guide
* Include thoughtfully-worded, well-structured tests
* Document new code based on the Documentation Styleguide
* End all files with a newline

## Development Process

### Setting Up Your Development Environment

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/Community-Car.git`
3. Add upstream remote: `git remote add upstream https://github.com/Mostafa-SAID7/Community-Car.git`
4. Create a branch: `git checkout -b feature/my-feature`

### Coding Standards

#### C# Style Guide

* Use PascalCase for class names, method names, and public members
* Use camelCase for local variables and private fields
* Use meaningful and descriptive names
* Keep methods small and focused on a single responsibility
* Add XML documentation comments for public APIs
* Follow SOLID principles

Example:
```csharp
/// <summary>
/// Represents a question in the Q&A system.
/// </summary>
public class Question : BaseEntity
{
    private readonly List<Answer> _answers = new();

    /// <summary>
    /// Gets or sets the question title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Adds an answer to the question.
    /// </summary>
    /// <param name="answer">The answer to add.</param>
    public void AddAnswer(Answer answer)
    {
        Guard.Against.Null(answer, nameof(answer));
        _answers.Add(answer);
    }
}
```

#### Database Migrations

* Always create migrations for database changes
* Use descriptive migration names
* Test migrations both up and down
* Never modify existing migrations that have been pushed

```bash
dotnet ef migrations add AddQuestionBookmarks -p CommunityCar.Infrastructure -s CommunityCar.Web
```

#### Commit Messages

* Use the present tense ("Add feature" not "Added feature")
* Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line

Example:
```
Add question bookmarking feature

- Add QuestionBookmark entity
- Implement bookmark service
- Add bookmark UI components
- Add unit tests for bookmark functionality

Closes #123
```

### Testing

* Write unit tests for all new features
* Ensure all tests pass before submitting PR
* Aim for at least 80% code coverage
* Use meaningful test names that describe what is being tested

```csharp
[Fact]
public async Task CreateQuestion_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var question = new Question { Title = "Test Question" };
    
    // Act
    var result = await _questionService.CreateAsync(question);
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

### Documentation

* Update README.md if you change functionality
* Add XML comments to public APIs
* Update relevant documentation in the docs/ folder
* Include code examples where appropriate

## Project Structure Guidelines

### Domain Layer
* Contains business logic and domain entities
* No dependencies on other layers
* Use value objects for complex types
* Implement domain events for cross-aggregate communication

### Infrastructure Layer
* Implements interfaces defined in Domain
* Contains EF Core configurations
* Implements repository pattern
* Contains external service integrations

### Web Layer
* Contains controllers, views, and view models
* Handles HTTP concerns
* Implements validation
* Contains localization resources

## Review Process

1. Create a pull request with a clear title and description
2. Link any related issues
3. Ensure all CI checks pass
4. Wait for code review from maintainers
5. Address any feedback
6. Once approved, a maintainer will merge your PR

## Community

* Join our discussions in GitHub Discussions
* Follow us on social media
* Attend our community calls (schedule TBD)

## Questions?

Feel free to open an issue with your question or reach out to the maintainers directly.

Thank you for contributing! 🚗💨
