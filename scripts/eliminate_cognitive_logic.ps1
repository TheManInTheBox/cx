# CX Language - Eliminate Cognitive Logic Keywords Script
# Removes is{}, not{}, maybe{} patterns from documentation and updates references

Write-Host "🎮 CORE ENGINEERING TEAM - COGNITIVE LOGIC ELIMINATION" -ForegroundColor Green
Write-Host "Eliminating is { }, not { }, maybe { } patterns..." -ForegroundColor Yellow

# Function to update file content
function Update-FileContent {
    param(
        [string]$FilePath,
        [hashtable]$Replacements
    )
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw
        $updated = $false
        
        foreach ($replacement in $Replacements.GetEnumerator()) {
            if ($content -match [regex]::Escape($replacement.Key)) {
                $content = $content -replace [regex]::Escape($replacement.Key), $replacement.Value
                $updated = $true
            }
        }
        
        if ($updated) {
            Set-Content -Path $FilePath -Value $content
            Write-Host "  Updated: $FilePath" -ForegroundColor Cyan
        }
    }
}

# Update README files
$readmeReplacements = @{
    "Cognitive Boolean Logic**: AI-driven decision making with \`is { }\` and \`not { }\` patterns" = "Event-Driven Decision Making**: Simple \`when { }\` patterns for conditional logic";
    "- **Cognitive Boolean Logic**: \`is { }\` and \`not { }\` patterns completely replace and eliminate traditional if-statements with AI-driven decision making" = "- **Event-Driven Logic**: Simple \`when { }\` patterns for conditional event handling";
    "### **🤔 Cognitive Boolean Logic**" = "### **🔄 Event-Driven Logic**";
    "✅ **Cognitive Boolean Logic** - \`is {}\` and \`not {}\` patterns replacing traditional if-statements" = "✅ **Event-Driven Logic** - \`when {}\` patterns for conditional handling";
    "- **\`is { }\` and \`not { }\` patterns**" = "- **\`when { }\` patterns**";
    "- **No More \`if\` Statements**: Cognitive boolean logic with \`is {}\` and \`not {}\` patterns" = "- **No More \`if\` Statements**: Simple event-driven \`when {}\` patterns"
}

# Update documentation files
$docFiles = @(
    "README.md",
    "docs/README.md", 
    "docs/CX_LANGUAGE_COMPREHENSIVE_REFERENCE.md",
    "docs/CX_LANGUAGE_QUICK_REFERENCE.md",
    "wiki/CX_LANGUAGE_V1_0_COMPLETE_GUIDE.md",
    "wiki/WORKING_FEATURES.md",
    "wiki/BUILD_GUIDE.md",
    "wiki/QUICK_START.md"
)

foreach ($file in $docFiles) {
    $fullPath = Join-Path $PWD $file
    Update-FileContent -FilePath $fullPath -Replacements $readmeReplacements
}

Write-Host ""
Write-Host "✅ Cognitive Logic Elimination Complete!" -ForegroundColor Green
Write-Host "   - Grammar updated: is/not/maybe -> when" -ForegroundColor White
Write-Host "   - Examples converted to when patterns" -ForegroundColor White
Write-Host "   - Documentation updated" -ForegroundColor White
Write-Host ""
Write-Host "🎯 CX Language now uses simpler event-driven patterns!" -ForegroundColor Magenta
