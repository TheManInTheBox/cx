#!/bin/bash
# CX Language v1.0.0-beta++ Release Readiness Validation Script

echo "🎭 CX Language v1.0.0-beta++ Release Readiness Check"
echo "=================================================="
echo ""

# Function to check if file exists and report
check_file() {
    if [ -f "$1" ]; then
        echo "✅ $1"
        return 0
    else
        echo "❌ $1 - MISSING!"
        return 1
    fi
}

# Function to check if directory exists
check_dir() {
    if [ -d "$1" ]; then
        echo "✅ $1/"
        return 0
    else
        echo "❌ $1/ - MISSING!"
        return 1
    fi
}

ERRORS=0

echo "🎯 Premier Multi-Agent Demo Files:"
check_file "examples/advanced_debate_demo.cx" || ((ERRORS++))
check_file "wiki/Premier-Multi-Agent-Voice-Debate-Demo.md" || ((ERRORS++))
echo ""

echo "📚 Documentation Files:"
check_file "README.md" || ((ERRORS++))
check_file "CHANGELOG.md" || ((ERRORS++))
check_file "RELEASE_CHECKLIST_BETA_PLUS_PLUS.md" || ((ERRORS++))
echo ""

echo "🏗️ Build Configuration:"
check_file "Directory.Build.props" || ((ERRORS++))
check_file "CxLanguage.sln" || ((ERRORS++))
echo ""

echo "🔄 CI/CD Pipeline:"
check_file ".github/workflows/ci.yml" || ((ERRORS++))
check_file ".github/workflows/cd.yml" || ((ERRORS++))
echo ""

echo "📁 Project Structure:"
check_dir "src" || ((ERRORS++))
check_dir "examples" || ((ERRORS++))
check_dir "wiki" || ((ERRORS++))
check_dir "grammar" || ((ERRORS++))
echo ""

echo "🔍 Version Consistency Check:"
echo "Checking for version 1.0.0-beta++ in key files..."

if grep -q "1.0.0-beta++" Directory.Build.props; then
    echo "✅ Directory.Build.props version updated"
else
    echo "❌ Directory.Build.props version NOT updated"
    ((ERRORS++))
fi

if grep -q "1.0.0-beta++" CHANGELOG.md; then
    echo "✅ CHANGELOG.md version updated"
else
    echo "❌ CHANGELOG.md version NOT updated" 
    ((ERRORS++))
fi

echo ""
echo "🎭 Premier Demo Validation:"
if grep -q "advanced_debate_demo.cx" README.md; then
    echo "✅ Premier demo referenced in README"
else
    echo "❌ Premier demo NOT referenced in README"
    ((ERRORS++))
fi

if grep -q "Multi-Agent Voice" README.md; then
    echo "✅ Multi-Agent Voice features highlighted in README"
else
    echo "❌ Multi-Agent Voice features NOT highlighted in README"
    ((ERRORS++))
fi

echo ""
echo "🚀 CI/CD Pipeline Validation:"
if grep -q "Multi-Agent Voice" .github/workflows/ci.yml; then
    echo "✅ CI pipeline updated for Multi-Agent Voice"
else
    echo "❌ CI pipeline NOT updated for Multi-Agent Voice"
    ((ERRORS++))
fi

if grep -q "advanced_debate_demo.cx" .github/workflows/ci.yml; then
    echo "✅ Premier demo testing in CI pipeline"
else
    echo "❌ Premier demo testing NOT in CI pipeline"
    ((ERRORS++))
fi

echo ""
echo "=================================================="
if [ $ERRORS -eq 0 ]; then
    echo "🎉 RELEASE READY! All validation checks passed."
    echo ""
    echo "🚀 Next Steps:"
    echo "1. Run: dotnet build --configuration Release"
    echo "2. Test: dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/advanced_debate_demo.cx"
    echo "3. Create tag: git tag v1.0.0-beta++"
    echo "4. Push tag: git push origin v1.0.0-beta++"
    echo ""
    echo "🎭 CX Language Multi-Agent Voice Platform is ready for release!"
    exit 0
else
    echo "❌ RELEASE NOT READY! Found $ERRORS errors."
    echo ""
    echo "Please fix the issues above before releasing."
    exit 1
fi
