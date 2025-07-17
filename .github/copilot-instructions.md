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

### 🔄 Phase 2: Function System (CURRENT PRIORITY)
- Function parameters and return values
- Function call mechanism fixes
- Proper variable scoping
- Function overloading support

### 📋 Phase 3: Advanced Language Features (FUTURE)
- For-in loops and iterators
- Exception handling (try/catch/throw)
- Array and object literals
- Class system and inheritance
- Module system and imports

### 🤖 Phase 4: AI Integration (FUTURE)
- Implementation of native AI functions
- Self keyword for function introspection
- Autonomous workflow capabilities

### 🚀 Phase 5: Autonomous Agentic Features (VISION)
- Multi-agent coordination
- Learning and adaptation mechanisms
- Self-modifying code capabilities

## Current Development Focus

**Priority 1: Fix Function System**
- Functions are parsed but have compilation issues
- Need to implement proper parameter handling
- Function calls to user-defined functions are not working
- Return values need proper typing

**Stay in Scope**: Focus on Phase 2 functions before moving to AI features.

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
- Focus on testing current phase features (Phase 1 complete, Phase 2 in progress)
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

1. **Fix Function System** (Phase 2)
   - Function parameters and calls
   - Return value handling
   - Variable scoping

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
