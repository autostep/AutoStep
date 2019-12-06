#${env:PATH} = "D:\Program Files\Java\jdk1.8.0_172\bin;";

$ErrorActionPreference = "Stop";

if(!(Test-Path "${PSScriptRoot}\antlr-4.7.2-complete.jar"))
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest "https://www.antlr.org/download/antlr-4.7.2-complete.jar" -OutFile "${PSScriptRoot}\antlr-4.7.2-complete.jar"
}

java -jar $PSScriptRoot\antlr-4.7.2-complete.jar .\AutoStepLexer.g4 -Dlanguage=CSharp -package AutoStep.Compiler.Parser
java -jar $PSScriptRoot\antlr-4.7.2-complete.jar .\AutoStepParser.g4 -Dlanguage=CSharp -package AutoStep.Compiler.Parser -visitor
