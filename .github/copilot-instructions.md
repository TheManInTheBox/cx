# Copilot Instructions for CX Language

## Project Overview
The CX Language is an AI-native agentic programming language designed for autonomous workflows. It features:
- First-class support for AI functions (task, synthesize, reason, process, generate, embed, adapt)
- JavaScript/TypeScript-like syntax for familiarity
- Function introspection capabilities with the `self` keyword
- Built for .NET runtime via IL code generation
- Focus on autonomous agent capabilities and self-modifying code

## Development Phases and Scope

### ✅ Phase 1: Core Language Foundation (COMPLETED)
- Variables, data types, arithmetic operations
- Control flow (if/else, while loops)
- Logical and comparison operators
- String operations and concatenation
- Compound assignment operators

### ✅ Phase 2: Function System (COMPLETED!)
- ✅ Two-pass compilation architecture 
- ✅ Function declarations and definitions
- ✅ Function parameters (multiple parameters supported)
- ✅ Function calls with argument passing
- ✅ Parameter access within function bodies
- ✅ Local variable scoping within functions
- ✅ Function return handling (void functions)

### 📋 Phase 3: Advanced Language Features (CURRENT PRIORITY)
- For-in loops and iterators
- Exception handling (try/catch/throw)
- Array and object literals
- Class system and inheritance
- Module system and imports
- Function return values (non-void)

### 🤖 Phase 4: AI Integration (FUTURE)
- Implementation of native AI functions
- Self keyword for function introspection
- Autonomous workflow capabilities

### 🚀 Phase 5: Autonomous Agentic Features (VISION)
- Multi-agent coordination
- Learning and adaptation mechanisms
- Self-modifying code capabilities

## Current Development Focus

**🎉 PHASE 2 COMPLETE: Function System Fully Functional!**
- ✅ Two-pass compilation system working perfectly
- ✅ Function declarations with multiple parameters
- ✅ Function calls with argument passing  
- ✅ Parameter access and local variable scoping
- ✅ All function system features operational

**Priority 1: Advanced Language Features (Phase 3)**
- Function return values (non-void functions)
- For-in loops and array iteration
- Exception handling (try/catch/throw)
- Array and object literal syntax
- Current scope: Complete core language features before AI integration

**Stay in Scope**: Complete Phase 3 advanced features before moving to Phase 4.

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
- Follow naming convention: `test_<feature_name>.cx`
- Include basic examples and edge cases
- Verify both parsing and runtime behavior
- Focus on testing current phase features (Phase 1 complete, Phase 2 function core working)
- **DEBUG INSTRUMENTATION**: Debug console output is currently enabled in CxCompiler.cs for tracking compilation flow - this helps diagnose issues and will be removed when Phase 2 is complete
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
  ```

## Development Priorities

1. **Implement Return Values** (Phase 3)
   - Non-void function returns
   - Return statement handling
   - Type inference for returns

2. **Complete Core Language** (Phase 3)
   - Advanced control structures
   - Data structures (arrays, objects)
   - Exception handling

3. **Implement AI Functions** (Phase 4)
   - Native AI function integration
   - Self keyword implementation
   - Autonomous capabilities

4. **Agentic Features** (Phase 5)
   - Multi-agent coordination
   - Learning mechanisms
   - Self-modification

## Build and Run Instructions
- Build with `dotnet build CxLanguage.sln` 
- Run examples with `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run <path-to-example>`
- Test working features with `examples/comprehensive_working_demo.cx`
- Debug with VS Code or Visual Studio
