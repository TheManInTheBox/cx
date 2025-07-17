# Copilot Instructions for CX Language

## Project Overview
The CX Language is an AI-native programming language with a focus on autonomous workflows. It features:
- First-class support for AI functions (task, synthesize, reason, process, generate, embed, adapt)
- JavaScript/TypeScript-like syntax for familiarity
- Function introspection capabilities with the `self` keyword
- Built for .NET runtime via IL code generation

## Coding Standards

### General Guidelines
- Follow C# best practices for backend code (compiler, parser, AST)
- Use descriptive naming for functions, variables, and classes
- Add XML documentation comments for public APIs and complex logic
- Use consistent formatting with 4-space indentation
- Do not use K&R-style brackets in CX code examples (opening bracket on same line as control statement)

### Grammar and Parser
- All grammar changes must be made in `grammar/Cx.g4`
- Follow ANTLR4 conventions and patterns
- When adding new syntax, ensure it has a corresponding visitor in `AstBuilder.cs`
- All new AST nodes should be defined in `AstNodes.cs`

### Compiler
- All IL code generation is in `CxCompiler.cs`
- Follow the visitor pattern for AST traversal
- Track function scope and variable state appropriately
- Ensure type safety where possible

### AI Integration
- All AI-native functions should be implemented in a consistent manner
- AI functions should support both direct arguments and options objects
- Preserve context and state appropriately for autonomous execution
- Document AI model requirements and expected behaviors

## Feature: Self Keyword

The `self` keyword allows functions to reference their own source code, enabling:
- Function introspection for debugging
- Self-optimization through AI models
- Code manipulation and metaprogramming

Implementation details:
- Grammar: `self` is a primary expression
- Compiler: `VisitSelfReference` returns the function's source code
- Source tracking: Functions need position and source code tracking

## Testing Guidelines
- **IMPORTANT:** All CX language files (.cx) MUST be placed in the `examples/` folder
- Create test files in `examples/` directory for new features
- Follow naming convention: `test_<feature_name>.cx`
- Include basic examples and edge cases
- Verify both parsing and runtime behavior
- Always use Allman-style brackets (opening bracket on a new line) in test examples:
  ```
  // Correct
  function example() 
  {
      // code
  }
  
  // Incorrect - Do not use
  function example() {
      // code
  }
  ```

## Build and Run Instructions
- Build with `dotnet build CxLanguage.sln` 
- Run examples with `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj <path-to-example>`
- Debug with VS Code or Visual Studio
