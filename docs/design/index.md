# AutoStep Core Design

This content goes through the internal architecture of the AutoStep Compilation and Execution process.

As an overview, to execute a test, we:

- Define a Project
- Add files (or other content sources) to the project.
- Add any step definition sources to the Project.
- Compile the Project. This generates in-memory structures representing the syntax tree of each file, and generates any necessary error messages.
- Link the Project. This takes the compiled file structures and attempts to 'bind' each step reference in those files to a known step definition.
- Create and configure a Test Run from the Project.
- Execute the Project's Tests.

## Compilation

When you invoke the <xref:AutoStep.Projects.IProjectCompiler.CompileAsync(System.Threading.CancellationToken)?text=Compile> method on the <xref:AutoStep.Projects.IProjectCompiler> available
from the <xref:AutoStep.Projects.Project.Compiler> property, we go through all the files registered and, if they have been modified since the last time they were compiled,
we run the compilation process on that file.

On each file, we invoke the <xref:AutoStep.Compiler.IAutoStepCompiler.CompileAsync(AutoStep.Compiler.IContentSource,System.Threading.CancellationToken)?text=CompileAsync> 
method to actually doing the compilation.

### Antlr and Creating the Parse Tree

When we need to compile a file, we use the Antlr parser engine. Antlr takes in a **grammar**, which is a file that describes the tokens and structure of a language,
and generates a lexer and parser for that language.

For information on how Antlr works, check out their github (https://github.com/antlr/antlr4).

All Antlr-related files are located in the [AutoStep/Compiler/Parser](https://github.com/autostep/AutoStep/tree/develop/src/AutoStep/Compiler/Parser) folder.
The AutoStep grammar is split into a separate lexer and parser grammar, to allow us to use [lexical modes](https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md#lexical-modes).

The lexer grammar is contained in [``AutoStepLexer.g4``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Parser/AutoStepLexer.g4); the
parser grammar is in [``AutoStepParser.g4``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Parser/AutoStepParser.g4).

Running the ``build-antlr`` powershell script in the same folder will download the required Antlr tool version if it's not already there, then
generate the C# output of the lexer and grammar.

> The powershell file does a search and replace in the generated files to change the public classes and interfaces to internal 
> (there isn't an Antlr setting to control this); we don't really want to expose the Antlr components directly to library consumers.

### AutoStepCompiler

The [``AutoStepCompiler``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/AutoStepCompiler.cs) and it's corresponding interface,
``IAutoStepCompiler``, class is the entry point for compilation of an individual file. 

The source of file content is defined by an implementation of <xref:AutoStep.Compiler.IContentSource>. This allows content to be pulled from memory,
a physical file, the network, etc.

Because the loading of file content can be asynchronous, we declare the compilation methods async as well.

Invoking the <xref:AutoStep.Compiler.AutoStepCompiler.CompileAsync(AutoStep.Compiler.IContentSource,System.Threading.CancellationToken)?text=AutoStepCompiler.CompileAsync> method will:

- Read the file content from the source (invoking <xref:AutoStep.Compiler.IContentSource.GetContentAsync(System.Threading.CancellationToken)>).
- Use Antlr to generate a parse tree for the file.
- Create a [``FileVisitor``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Visitors/FileVisitor.cs),
  which then walks the parse tree, building a <xref:AutoStep.Elements.FileElement> structure. 
  
  The ``FileVisitor`` invokes other sub-visitors to walk specific parts of the tree. These are:
  
  - [``StepDefinitionVisitor``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Visitors/StepDefinitionVisitor.cs)
    Builds a <xref:AutoStep.Elements.StepDefinitionElement> from a step definition block.

  - [``StepReferenceVisitor``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Visitors/StepReferenceVisitor.cs)
    Builds a <xref:AutoStep.Elements.StepReferenceElement > from a step reference line.

  - [``TableVisitor``](https://github.com/autostep/AutoStep/blob/develop/src/AutoStep/Compiler/Visitors/TableVisitor.cs)
    Builds a <xref:AutoStep.Elements.TableElement> from a table block.

- Output the result of the compilation, including the built file and any compilation messages, as a <xref:AutoStep.Compiler.FileCompilerResult>.

## Linking

Linking is the act of mapping compiled step references to known step definitions.