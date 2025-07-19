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

### AI Integration (Phase 4 - COMPLETE!)
- All 6 AI services fully operational: TextGeneration, ChatCompletion, DALL-E 3, TextEmbeddings, TextToSpeech, VectorDatabase
- AI functions implemented with both direct arguments and options objects
- Vector Database integration with KernelMemory 0.98.x for RAG workflows
- text-embedding-3-small deployed with 62% better performance than ada-002
- Production-ready with enterprise-grade reliability and cost optimization
- Context and state preserved appropriately for autonomous execution
- All AI model requirements documented and behaviors verified

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
- Focus on testing current phase features (Phase 1-4 complete, Phase 5 ready to begin)
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

### ✅ **PHASE 1-4: COMPLETED**
- **Phase 1**: Core language foundation (variables, operators, control flow)
- **Phase 2**: Function system with two-pass compilation
- **Phase 3**: Advanced features (for-in loops, exception handling, classes, data structures)
- **Phase 4**: Complete AI integration with 6 operational services including vector database

### 🚀 **PHASE 5: AUTONOMOUS AGENTIC FEATURES (READY TO BEGIN)**
1. **Cx.Ai.Adaptations Standard Library** - AI-powered .NET IL generator for dynamic code generation
2. **Self Keyword Implementation** - Function introspection for autonomous workflows
3. **Multi-agent Coordination** - Agent communication and task delegation systems
4. **Learning and Adaptation** - Dynamic behavior modification based on outcomes  
5. **Self-modifying Code** - Runtime code generation and optimization capabilities
6. **Autonomous Workflow Orchestration** - Complex task planning and execution

## Build and Run Instructions
- Build with `dotnet build CxLanguage.sln` 
- Run examples with `dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run <path-to-example>`
- Test working features with `examples/comprehensive_working_demo.cx`
- Debug with VS Code or Visual Studio
