using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Assertion;
using AutoStep.Definitions.Test;
using AutoStep.Language;
using AutoStep.Projects;

namespace AutoStep.Tests.Execution
{
    public static class ProjectMocks
    {
        /// <summary>
        /// Creates a new built and linked project for the given input test source.
        /// Available steps:
        ///  Given I do something
        ///  Then it fails
        /// </summary>
        /// <param name="testSource">The test file.</param>
        /// <returns></returns>
        public static async ValueTask<Project> CreateBuiltProject(params (string path, string content)[] testFiles)
        {
            var project = new Project();

            for (int idx = 0; idx < testFiles.Length; idx++)
            {
                project.TryAddFile(new ProjectTestFile(testFiles[idx].path, new StringContentSource(testFiles[idx].content)));
            }

            var steps = new CallbackDefinitionSource();

            steps.Given("I do something", () => { });
            steps.Then("it fails", () => { throw new AssertionException("Fail!"); });
            steps.Then("it will {arg}", (string arg) =>
            {
                if (arg == "fail")
                {
                    throw new AssertionException("Fail!");
                }
            });

            project.Compiler.AddStepDefinitionSource(steps);

            var compileResult = await project.Compiler.CompileAsync();

            if (!compileResult.Success)
            {
                throw new AssertionException("Failed to build.");
            }

            var linkResult = project.Compiler.Link();

            if (!linkResult.Success)
            {
                throw new AssertionException("Failed to link.");
            }

            return project;
        }
    }
}
