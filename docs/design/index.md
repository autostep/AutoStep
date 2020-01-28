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

When you invoke the <xref:AutoStep.Projects.IProjectCompiler.Compile(System.Threading.CancellationToken)?text=Compile> method on the <xref:AutoStep.Projects.IProjectCompiler> available
from the <xref:AutoStep.Projects.Project.Compiler> property, we go through all the files registered and, if they have been modified since the last time they were compiled,
we run the compilation process on that file.

On each file, we invoke the <xref:AutoStep.Compiler.IAutoStepCompiler.CompileAsync(AutoStep.Compiler.IContentSource,System.Threading.CancellationToken)?text=CompileAsync> 
method to actually doing the compilation.

### Antlr and Creating the Parse Tree

When we need to compile a file, we use the Antlr parser engine


