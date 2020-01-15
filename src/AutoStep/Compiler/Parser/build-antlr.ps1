#${env:PATH} = "D:\Program Files\Java\jdk1.8.0_172\bin;";

$ErrorActionPreference = "Stop";

if(!(Test-Path "${PSScriptRoot}\antlr-4.7.2-complete.jar"))
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest "https://www.antlr.org/download/antlr-4.7.2-complete.jar" -OutFile "${PSScriptRoot}\antlr-4.7.2-complete.jar"
}

java -jar $PSScriptRoot\antlr-4.7.2-complete.jar .\AutoStepLexer.g4 -Dlanguage=CSharp -package AutoStep.Compiler.Parser
java -jar $PSScriptRoot\antlr-4.7.2-complete.jar .\AutoStepParser.g4 -Dlanguage=CSharp -package AutoStep.Compiler.Parser -visitor

$files = Get-Item "*.cs";

# Convert public antlr components to internal.
foreach($f in $files)
{
    $content = Get-Content $f -Raw;

    $content = $content.Replace('public partial class', 'internal partial class');
    $content = $content.Replace('public interface', 'internal interface');

    $content | Set-Content $f;
}