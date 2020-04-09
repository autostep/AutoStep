# AutoStep Contributor Guide

Contributions to AutoStep, whether new features or bug fixes, are deeply appreciated and benefit the whole user community.

The following guidelines help ensure the smooth running of the project, and keep a consistent standard across the codebase. They are guidelines only - should you feel a need to deviate from them it is probably for a good reason - but please adhere to them as closely as possible.

If you'd like to contribute code or documentation to AutoStep, we welcome pull requests.

**Your contributions must be your own work and licensed under the same terms as AutoStep.**

## Code of Conduct

The AutoStep Code of Conduct [is posted on GitHub](CODE_OF_CONDUCT.md). It is expected that all contributors follow the code of conduct.

## Repositories

The AutoStep project is broken up into a few different repositories. Where possible, please raise issues in the appropriate repository.

The repos are generally categorised as follows:

- AutoStep Library (https://github.com/autostep/AutoStep)  
  This is the core library that provides the shared functionality for all AutoStep behaviour; language compilation, linking, text execution, etc.

  Changes to the test or interaction languages, the project system, or test execution behaviour will go here.

- Tooling  
    - The CLI (https://github.com/autostep/autostep-cli), providing command-line
      tools for AutoStep, including building and running tests.
    - The VS Code extension (https://github.com/autostep/AutoStep.VsCode),
      that provides language editor support for AutoStep.
    - The AutoStep Extension Host (https://github.com/autostep/AutoStep.Extensions), 
      which is responsible for loading extensions packages and managing the set of dependencies.

- Extensions  
    AutoStep extensions typically provide test step definitions and interaction methods
    to support a given platform or product. 

    They may also add event handlers to power logging, reporting or some other custom functionality.

    Currently, we only have AutoStep.Web (https://github.com/autostep/AutoStep.Web), which provides
    Selenium bindings for AutoStep, to allow you to write web application tests.

We welcome contributions to any of these components, including proposing new extensions.

## Process

**When working through contributions, please file issues and submit pull requests in the repository containing the code in question.** For example, if the issue is with the AutoStep.Web Extension, file it in that repo rather than the core AutoStep repo.

- **File an issue.** Either suggest a feature or note a defect. If it's a feature, explain the challenge you're facing and how you think the feature should work. If it's a defect, include a description and reproduction (ideally one or more failing unit tests).
- **Design discussion.** For new features, some discussion on the issue will take place to determine if it's something that should be included with AutoStep (or an AutoStep-managed extension) or be a user-supplied extension. For defects, discussion may happen around whether the issue is truly a defect or if the behavior is correct.
- **Pull request.** Create [a pull request](https://help.github.com/articles/using-pull-requests/) on the `develop` branch of the repository to submit changes to the code based on the information in the issue. Pull requests need to pass the CI build and follow coding standards. See below for more on coding standards in use with AutoStep. Note all pull requests should include accompanying unit tests to verify the work.
- **Code review.** Some iteration may take place requiring updates to the pull request (e.g., to fix a typo or add error handling).
- **Pull request acceptance.** The pull request will be accepted into the `develop` branch and pushed to `master` with the next release.

## License

By contributing to AutoStep, you assert that:

1. The contribution is your own original work.
2. You have the right to assign the *copyright* for the work (it is not owned by your employer, or you have been given copyright assignment in writing).
3. You license it under the terms applied to the rest of the AutoStep project.

## Coding

### Workflow

All the AutoStep repositories follow the [Gitflow workflow process](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow/) for handling releases. This means active development is done on the `develop` branch and we push to `master` when it's release time. **If you're creating a pull request or contribution, please do it on the `develop` branch.** We can then build, push to Feedz.io for testing, and release to NuGet when everything's verified.

We use [semantic versioning](https://semver.org/) for our package/binary versions.

### Developer Environment

- Visual Studio 2019 or Visual Studio Code
- .NET Core SDK 3.1
- PowerShell 5+
- node v12+ (for working on the VS Code extension)

### Build / Test

Most projects should build straight from the dotnet build command:

```bash
dotnet build
```

Followed by dotnet test:

```base
dotnet test
```

Projects can also be opened and built in Visual Studio or Visual Studio Code. Unit tests can be run
from those IDEs as well.

Some projects will deviate from this, and any different behaviour should be listed in that repo's README.

Unit tests are written in XUnit, with FluentAssertions and Moq. **Code contributions should include tests that exercise/demonstrate the contribution.**

Some tests may require integration test behaviour, which will require additional infrastructure/supporting code.

**Everything should build and test with zero errors and zero warnings.**

### Coding Standards

Normal .NET coding guidelines apply. See the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042.aspx) for suggestions. We have Roslyn analyzers running on most of the code. These analyzers are actually correct a majority of the time. Please try to fix warnings rather than suppressing the message. If you do need to suppress a false positive, use the `[SuppressMessage]` attribute.

AutoStep source code uses four spaces for indents. We use [EditorConfig](https://editorconfig.org/) to ensure consistent formatting in code docs. Visual Studio has this built in since VS 2017. VS Code requires the EditorConfig extension. Many other editors also support EditorConfig.

### Public API and Breaking Changes

> In the current stage of the AutoStep project, there has been no 1.0 release.
> This means that breaking changes are not going to be an issue just yet.
> When we do release though, we'll need a policy where we prevent breaking changes.

Additional considerations:

- Projects should be able to be built straight out of Git (no additional installation needs to take place on the developer's machine). This means NuGet package references, not installation of required components.
- Any third-party libraries consumed by an AutoStep extension must have licenses compatible with AutoStep's (the GPL and licenses like it are incompatible - please ask on the discussion forum if you're unsure).

### Documentation and Examples

> Documentation and example repositories are on the way. When they are up, we'll want to be updating 
> the docs/examples as wel change things.

You should include XML API comments in the code (the Roslyn analyzers should pick this up automatically). These are used to generate API documentation as well as for IntelliSense.

**The Golden Rule of Documentation: Write the documentation you'd want to read.** Every developer has seen self explanatory docs and wondered why there wasn't more information. (Parameter: "index." Documentation: "The index.") Please write the documentation you'd want to read if you were a developer first trying to understand how to make use of a feature.