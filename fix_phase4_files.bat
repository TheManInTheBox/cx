@echo off
echo Replacing problematic files with fixed versions...

rem Create backup of original files
copy src\CxLanguage.Compiler\Modules\AiFunctions.cs src\CxLanguage.Compiler\Modules\AiFunctions.cs.bak
copy src\CxLanguage.Compiler\Modules\VectorDatabase.cs src\CxLanguage.Compiler\Modules\VectorDatabase.cs.bak

rem Replace with fixed versions
copy src\CxLanguage.Compiler\Modules\AiFunctions.cs.fixed src\CxLanguage.Compiler\Modules\AiFunctions.cs
copy src\CxLanguage.Compiler\Modules\VectorDatabase.cs.fixed src\CxLanguage.Compiler\Modules\VectorDatabase.cs

echo Fixed! Now try running the test with:
echo dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/phase4_basic_test.cx
