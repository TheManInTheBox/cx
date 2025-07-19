## Coding Standards

### General Guidelines
- Follow C# best practices for backend code (compiler, parser, AST)
- Use descriptive naming for functions, variables, and classes
- Add XML documentation comments for public APIs and complex logic
- Use consistent formatting with 4-space indentation
- **ALWAYS use Allman-style brackets in ALL CX code examples and documentation** (opening bracket on new line)
- **NEVER use K&R-style brackets** (opening bracket on same line as control statement)
- Apply Allman formatting to all function declarations, if statements, loops, and code blocks

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

### AI Integration (Future Phase)
- All AI-native functions should be implemented in a consistent manner
- AI functions should support both direct arguments and options objects
- Preserve context and state appropriately for autonomous execution
- Document AI model requirements and expected behaviors

## Feature: Function System (Current Priority)

Functions are the next critical feature to implement:
- Basic function declarations (working in parser)
- Function parameters and arguments
- Return values and types
- Function calls and scoping
- Local variable isolation

## Feature: Self Keyword (Future Phase)

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
- Follow naming convention: `test_<feature_name>.cx` or `<phase>_<feature>.cx`
- Include basic examples and edge cases
- Verify both parsing and runtime behavior
- Focus on testing current phase features (Phase 1-3 complete, Phase 4 in progress)
- **WORKING EXAMPLES:** Use `current_features_complete.cx` to test all working features
- **AI FUNCTIONS:** Require Azure OpenAI configuration for testing
- **ALWAYS use Allman-style brackets (opening bracket on a new line) in ALL test examples and documentation:**
  ```
  // Correct - ALWAYS use this format
  function example() 
  {
      if (condition)
      {
          // code here
      }
      else
      {
          // alternative code
      }
  }
  
  // Incorrect - NEVER use this format
  function example() {
      if (condition) {
          // code here
      } else {
          // alternative code
      }
  }
  ``

## Development Priorities

1. **Implement For-in Loops** (Phase 3)
   - Array and collection iteration
   - Iterator interface
   - Loop control (break, continue)

2. **Implement Exception Handling** (Phase 3)
   - Try/catch/throw statements
   - Error object support
   - Stack trace capture

3. **Implement Data Structures** (Phase 3)
   - Array literals and operations
   - Object literals and property access
   - Collection manipulation

4. **Implement AI Functions** (Phase 4)
   - Native AI function integration
   - Self keyword implementation
   - Autonomous capabilities
   - Multi-agent coordination
   - Learning mechanisms
   - Self-modification

## Build and Run Instructions
- Build with `dotnet build CxLanguage.sln` 
- Run examples with `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run <path-to-example>`
- Test working features with `examples/comprehensive_working_demo.cx`
- Debug with VS Code or Visual Studio
