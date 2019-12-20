# AutoStep

AutoStep is a new way of writing Gherkin-style tests, that reduces the requirement on programmers to be heavily involved in writing tests.

It introduces a Gherkin+ syntax that adds a number of features to the language, specifically:

 - Specifying 'options' on tests, as well as tags.
 - The ability to define steps inside AutoStep, improving usability.

In addition, AutoStep will come with its own editor that speeds up the test writing process.

## Architecture

  - AutoStep is built in .NET, targeting .netstandard21 for all libraries, and netcoreapp31 for all applications.