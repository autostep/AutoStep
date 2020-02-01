![](https://github.com/autostep/autostep/workflows/AutoStep%20Build/badge.svg)

# AutoStep

AutoStep is a new compiler and runner for Gherkin-style BDD tests, with some extra language features on top of standard Gherkin that add useful functionality.
Unlike some existing BDD tools, there is no test code generation or separate code compilation process. The tests compile and execute directly inside AutoStep.

This repository contains the core .NET library that provides compilation and execution behaviour.

---

**Status**

AutoStep is currently under development (in alpha). You can grab the CI package
from our feedz.io package feed: https://f.feedz.io/autostep/ci/nuget/index.json.
Get the 'develop' tagged pre-release package for latest develop. 

At the moment the compiler and linker is mostly stable (but need a couple of extra features), and tests can be executed.

We're about to add our own results collector, at the moment you would have to add an event handler to do that.
Shouldn't be too long though.


This library will soon be wrapped by both command-line and visual tooling to make writing and running tests a breeze!

---

# Using It

As a basic introduction, here is a BDD test I want to execute in AutoStep:

```cucumber
Feature: My Feature 

  Scenario: My Scenario
    
    # This will run my defined step below, twice!
    Given I have added an order for 100.0
      And I have added an order for 200.0

    Then I should have 2 orders

# You can define steps inside your test files
Step: Given I have added an order for {arg}

    Given I have entered <arg> into 'Order Value'
    
    When I click 'Submit'

    Then the 'Order Success' page should be displayed
```

In order to run this test, I can use the following C# code:

```csharp

// Create a project
var project = new Project();

// Define some steps based on callbacks.
// You can actually define anything as a source of steps if you
// want to!
var steps = new CallbackDefinitionSource();

// You can put arguments in curly braces:
steps.Given("I have entered {value} into {field}", (string val, string field) =>
{
    // Run the selenium/manipulation code as needed
});

// You can inject a DI scope and resolve services:
steps.When("I click {button}", (IServiceScope scope, string buttonName) =>
{
    // Click the button
});

// Step methods can be async:
steps.Then("the {title} page should be displayed", async (string title) => 
{
    // Check for the title page
});

// You can do type hinting.
// Test compilation will give an error if someone tries to pass a string as count.
steps.Then("I should have {count:int} orders", (IServiceScope scope, int count) => 
{
});

// Add your source to the project compiler.
project.Compiler.AddStaticStepDefinitionSource(steps);

// Read a file that you want to run
var content = new File.ReadAllText("testfile.as");

var file = new ProjectFile("/test", new StringContentSource(TestFile)); 

// Add your file to the project (you can have as many files as you like)
// Steps defined in one project file can be used in any other.
project.TryAddFile(file);

// Compile the project (will return any errors).
// After compilation, file.LastCompilationResult will be set.
await project.Compiler.CompileAsync();

// Link the project.
// After linking, file.LastLinkResult will be set.
project.Compiler.Link();

// Create a test run.
var testRun = project.CreateTestRun();

// The test will run:
await testRun.Execute();

```

# Why AutoStep

Why does AutoStep exist? The team behind AutoStep has spent years testing enterprise-level web applications using the existing Gherkin technologies (mostly SpecFlow) and while we are a big fan of these, we've also identified a couple of things we'd like to add. Here are a few of the useful features in AutoStep that help us with that.

## Defining Steps in the same Language as the Tests

People often end up writing automated web tests that look like this:

```gherkin
Scenario: Shipping an Order Increments the User Shipped Count

    Given I have clicked on the 'Orders' menu item
      And I have entered 100 into the 'Order Value' field
      And I have clicked 'Place Order'
      And I have clicked 'Confirm'

    Given I have clicked on the 'Shipping' menu item
      And I have selected the order for 100
      And I have pressed 'Ship'
      And I have pressed 'Confirm'

    Then the user's shipped count should be 1

```

This isn't strictly great BDD, because you're putting a lot of detail into the test itself about how to actually place an order,
which makes it harder to simply describe what actual behaviour is being tested (in this example, that the shipped count has been incremented).

So we want to resolve this by hiding the 'implementation details' of the test behind another step:

```gherkin
Scenario: Shipping an Order Increments the User Shipped Count

    Given I have placed an order for 100
      And I have shipped an order for 100

    Then the user's shipped count should be 1

```

That's much more readable, but where has that behaviour gone? In most existing gherkin implementations, that behaviour is now in Javascript, or C#,
of whatever your binding language is.

We've found that it's not really ideal to completely hide the **detail** of a behaviour in the code that the steps
bind to. It's pretty common for the person reading the test to both:

- Want to find out the implementation is doing at a fine-grained level
- Not be a developer.

To this end, AutoStep allows you to define steps in the **same language as the tests**, either in the same file or in a separate file shared with all the tests in your project:

```gherkin
Step: Given I have placed an order for {value:float}

   This step will place an order for the specified value.
    
    Given I have clicked on the 'Orders' menu item
      And I have entered <value> into the 'Order Value' field
      And I have clicked 'Place Order'
      And I have clicked 'Confirm'

Step: Given I have shipped an order for {value:float}

    This step ships the order with the specified value.

    Given I have clicked on the 'Shipping' menu item
      And I have selected the order for <value>
      And I have pressed 'Ship'
      And I have pressed 'Confirm'
```

Step definitions can have descriptions, just like scenarios, which act
like documentation 

You can nest the references too, so we could wrap both of those again:

```gherkin
Step: Given I have placed and shipped an order for {value:float}

    Given I have placed an order for <value>
      And I have shipped an order for <value>

# Use the new single step
Scenario: Shipping an Order Increments the User Shipped Count

    Given I have placed and shipped an order for 100

    Then the user's shipped count should be 1
```

## No Visual Studio/MSBuild Dependency

One of the frustrations we've had in the past is that the writing and building
of tests requires a development environment, such as Visual Studio, or Visual Studio Code. In addition, tests must be 'built'.

Not all the people we want to be able to write AutoStep tests are developers,
or even have a license for Visual Studio.

AutoStep does **not** generate code for another test system (NUnit, JUnit, XUnit, etc) and does not depend on any existing testing framework. Tests are compiled and executed completely by AutoStep, with no other execution dependencies. This means we don't need any build system other than our own!

AutoStep 1.0 will ship with its own mini-IDE designed for writing BDD tests, that 
removes a lot of the complexity of using Visual Studio or similar IDEs, making the experience much more straightforward for test writers/readers.

Eventually we are going to add support for writing tests in the popular IDEs, but our priority is getting people who don't already use one writing tests first.

## Tags vs Options

When we have built our tests in Gherkin in the past, and we want to add some automatic functionality to a test (for example, spinning up a database), we have
ended up horribly mis-using tags to achieve this:

```gherkin

@fresh-database-per-scenario
@use-db-backup:customer-setup
@orders
Feature: Customer Orders

  Scenario: Enter an Order
  
```

In this test, we want to tell our before-feature handlers what *options* we want to use for the feature; but this is now mixed in with
the actual tags for the Feature, that is how we group, find and report on tests.

So, in AutoStep, we differentiate *tags* from *options*, by having a different character to indicate options:

```gherkin

$fresh-database-per-scenario
$use-db-backup:customer-setup
@orders
Feature: Customer Orders

  Scenario: Enter an Order
  
```

## Sourcing Steps from Anywhere

While AutoStep will support the standard notation of binding functionality to methods in classes people write, we also believe that a step definition should be able to come from almost anywhere.

For example, we want to create steps automatically from control configuration,
or application options, without having to write the step bindings.

# Architecture

AutoStep is built in .NET, targeting .netstandard21 for all libraries, and netcoreapp31 for all applications.

It uses the Antlr parser generator to parse the AutoStep language.