$ErrorActionPreference = "Stop";
$antlrVersion = "4.8";

$classPath = "$PSScriptRoot\antlr-$antlrVersion-complete.jar";

if(!(Test-Path $classPath))
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest "https://www.antlr.org/download/antlr-$antlrVersion-complete.jar" -OutFile "${PSScriptRoot}\antlr-$antlrVersion-complete.jar"
}

java -jar $classPath .\AutoStepInteractionsLexer.g4
java -jar $classPath .\AutoStepInteractionsParser.g4
javac -cp $classPath *.java

