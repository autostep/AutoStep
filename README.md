![](https://github.com/autostep/autostep/workflows/AutoStep%20Build/badge.svg)

# AutoStep

AutoStep is a compiler and runner for Gherkin-style BDD tests, with a few extra language features
on top of standard gherkin that add useful functionality.

---

**Status**

AutoStep is currently under development (in alpha). You can grab the CI package
from our feedz.io package feed: https://f.feedz.io/autostep/ci/nuget/index.json. 

At the moment the compiler and linker is relatively stable, with work starting on test execution.

Get the 'alpha' pre-release for latest develop.

---

# Why AutoStep

Why does AutoStep exist? The team behind AutoStep has spent years testing big web applications using the existing Gherkin technologies (mostly SpecFlow) and while we are a big fan of these, we've also identified a couple of things we'd like to add. Here are a few of the useful features in AutoStep that help us with that.

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

```

## No Visual Studio/MSBuild Dependency

One of the frustrations we've had in the past is that the writing and building
of tests requires a development environment, such as Visual Studio, or Visual Studio Code. In addition, tests must be 'built'.

Not all the people we want to be able to write AutoStep tests are developers,
or even have a license for Visual Studio.

AutoStep does **not** generate code for another test system (NUnit, JUnit, XUnit, etc) and does not depend on any existing testing framework. Tests are compiled and executed completely by AutoStep, with no other execution dependencies. This means we don't need any build system other than our own!

AutoStep 1.0 will ship with its own mini-IDE designed for writing BDD tests, that 
removes a lot of the complexity using Visual Studio or a similar IDE adds for test writers/readers.

Eventually we are going to add support for writing tests in the popular IDEs, but our priority is getting people who don't already use one writing tests first.

## Sourcing Steps from Anywhere

While AutoStep will support the standard notation of binding functionality to methods in classes people write, we also believe that a step definition should be able to come from almost anywhere.

For example, we want to create steps automatically from control configuration,
or application options, without having to write the step bindings.

# Architecture

AutoStep is built in .NET, targeting .netstandard21 for all libraries, and netcoreapp31 for all applications.

It uses the Antlr parser generator to parse the AutoStep language.