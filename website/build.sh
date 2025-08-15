#!/bin/bash

# AgileCloud.ai Website Production Build Script
# Prepares the website for deployment with optimization

set -e

echo "🚀 Starting AgileCloud.ai website production build..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Ensure we're in the website directory
cd "$(dirname "$0")"

# Clean previous build
print_status "Cleaning previous build..."
rm -rf dist
mkdir -p dist

# Copy public files
print_status "Copying public files..."
cp -r public/* dist/

# Copy assets
print_status "Copying assets..."
cp -r assets dist/

# Verify all solution pages have JavaScript includes
print_status "Verifying solution pages..."
solution_pages=(
    "dist/solutions/enterprise-ai.html"
    "dist/solutions/data-ingestion.html"
    "dist/solutions/rapid-prototyping.html"
    "dist/solutions/global-vector-db.html"
    "dist/solutions/intelligent-automation.html"
    "dist/solutions/scalable-architecture.html"
)

for page in "${solution_pages[@]}"; do
    if [ -f "$page" ]; then
        if grep -q "main.js" "$page"; then
            print_success "✓ JavaScript verified in $(basename "$page")"
        else
            print_error "✗ Missing JavaScript in $(basename "$page")"
            exit 1
        fi
    else
        print_warning "Solution page not found: $page"
    fi
done

# Check for Node.js and npm (optional optimization)
if command -v npm &> /dev/null; then
    print_status "Node.js detected, running optimizations..."
    
    # Install dependencies if needed
    if [ ! -d "node_modules" ]; then
        print_status "Installing dependencies..."
        npm install
    fi
    
    # Minify CSS
    if command -v csso &> /dev/null; then
        print_status "Minifying CSS..."
        npx csso assets/css/styles.css --output dist/assets/css/styles.min.css
        
        # Update HTML files to use minified CSS
        find dist -name "*.html" -exec sed -i.bak 's|assets/css/styles.css|assets/css/styles.min.css|g' {} \;
        find dist -name "*.bak" -delete
        print_success "CSS minified"
    else
        print_warning "csso not available, skipping CSS minification"
    fi
    
    # Minify JavaScript
    if command -v terser &> /dev/null; then
        print_status "Minifying JavaScript..."
        npx terser assets/js/main.js --output dist/assets/js/main.min.js --compress --mangle
        
        # Update HTML files to use minified JS
        find dist -name "*.html" -exec sed -i.bak 's|assets/js/main.js|assets/js/main.min.js|g' {} \;
        find dist -name "*.bak" -delete
        print_success "JavaScript minified"
    else
        print_warning "terser not available, skipping JavaScript minification"
    fi
    
    # Minify HTML
    if command -v html-minifier-terser &> /dev/null; then
        print_status "Minifying HTML..."
        find dist -name "*.html" -exec npx html-minifier-terser {} --output {} --remove-comments --collapse-whitespace --minify-css --minify-js \;
        print_success "HTML minified"
    else
        print_warning "html-minifier-terser not available, skipping HTML minification"
    fi
else
    print_warning "Node.js not detected, skipping optimization steps"
fi

# Validate build
print_status "Validating build..."

# Check required files
required_files=(
    "dist/index.html"
    "dist/404.html"
    "dist/robots.txt"
    "dist/sitemap.xml"
    "dist/web.config"
    "dist/assets/css/styles.css"
    "dist/assets/js/main.js"
)

for file in "${required_files[@]}"; do
    if [ -f "$file" ]; then
        print_success "✓ $(basename "$file")"
    else
        print_error "✗ Missing required file: $file"
        exit 1
    fi
done

# Check solution pages
for page in "${solution_pages[@]}"; do
    if [ -f "$page" ]; then
        print_success "✓ $(basename "$page")"
    else
        print_error "✗ Missing solution page: $page"
        exit 1
    fi
done

# Check for broken internal links
print_status "Checking for broken internal links..."
broken_links=0

find dist -name "*.html" -exec grep -Ho 'href="[^h#]' {} \; | while IFS=: read -r file link_line; do
    link=$(echo "$link_line" | grep -o 'href="[^"]*"' | sed 's/href="//;s/"//')
    
    # Skip external links and anchors
    if [[ $link =~ ^https?:// ]] || [[ $link =~ ^# ]] || [[ $link =~ ^mailto: ]]; then
        continue
    fi
    
    # Convert relative path to absolute path for checking
    if [[ $link == /* ]]; then
        target_file="dist$link"
    else
        dir_path=$(dirname "$file")
        target_file="$dir_path/$link"
    fi
    
    # Normalize path
    target_file=$(cd "$(dirname "$target_file")" && pwd)/$(basename "$target_file")
    
    if [[ ! -f "$target_file" ]] && [[ ! -f "${target_file}.html" ]]; then
        print_error "Broken link: $link in $(basename "$file")"
        broken_links=$((broken_links + 1))
    fi
done

if [ $broken_links -gt 0 ]; then
    print_error "Found $broken_links broken internal links"
    exit 1
fi

# Generate build report
print_status "Generating build report..."
cat > dist/build-report.txt << EOF
AgileCloud.ai Website Build Report
Generated: $(date)

Files in build:
$(find dist -type f | wc -l) total files

HTML Pages:
$(find dist -name "*.html" | sort)

Assets:
$(find dist/assets -type f | sort)

Build size:
$(du -sh dist | cut -f1) total

Individual page sizes:
$(find dist -name "*.html" -exec ls -lh {} \; | awk '{print $5, $9}' | sort -hr)

Asset sizes:
$(find dist/assets -type f -exec ls -lh {} \; | awk '{print $5, $9}' | sort -hr)
EOF

print_success "Build report generated: dist/build-report.txt"

# Security check
print_status "Running security checks..."
security_issues=0

# Check for eval() usage
if grep -r "eval(" dist/ --include="*.js" --include="*.html" > /dev/null 2>&1; then
    print_warning "Found eval() usage - potential security risk"
    security_issues=$((security_issues + 1))
fi

# Check for innerHTML usage
if grep -r "innerHTML" dist/ --include="*.js" --include="*.html" > /dev/null 2>&1; then
    print_warning "Found innerHTML usage - review for XSS vulnerabilities"
    security_issues=$((security_issues + 1))
fi

# Check for document.write usage
if grep -r "document.write" dist/ --include="*.js" --include="*.html" > /dev/null 2>&1; then
    print_warning "Found document.write usage - potential security risk"
    security_issues=$((security_issues + 1))
fi

if [ $security_issues -eq 0 ]; then
    print_success "No obvious security issues detected"
fi

# Final validation
print_status "Running final validation..."

# Check HTML structure
for html_file in $(find dist -name "*.html"); do
    if ! grep -q "<!DOCTYPE html>" "$html_file"; then
        print_error "Missing DOCTYPE in $(basename "$html_file")"
        exit 1
    fi
    
    if ! grep -q "</html>" "$html_file"; then
        print_error "Missing closing HTML tag in $(basename "$html_file")"
        exit 1
    fi
done

print_success "All HTML files have proper structure"

# Check meta tags
for html_file in $(find dist -name "*.html"); do
    if ! grep -q 'name="description"' "$html_file"; then
        print_warning "Missing meta description in $(basename "$html_file")"
    fi
    
    if ! grep -q 'rel="canonical"' "$html_file"; then
        print_warning "Missing canonical URL in $(basename "$html_file")"
    fi
done

print_success "🎉 Build completed successfully!"
print_success "📁 Build output: dist/"
print_success "📊 Build report: dist/build-report.txt"

# Display deployment instructions
cat << EOF

🚀 DEPLOYMENT READY

Your website is ready for deployment. The optimized files are in the 'dist/' directory.

Next steps:
1. Commit your changes to git
2. Push to main/master branch to trigger automatic deployment
3. Monitor the GitHub Actions workflow
4. Visit https://agilecloud.ai once deployment completes

Manual deployment options:
- Azure: Use the GitHub Actions workflow (automatic)
- GitHub Pages: Copy dist/ contents to gh-pages branch
- Other hosts: Upload dist/ contents to your web server

Build completed at: $(date)
EOF
