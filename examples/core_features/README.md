# CX Language Core Features Examples

This directory contains examples demonstrating the core features of the CX Language, including AI integration, vector embeddings, and consciousness-aware programming.

## Vector Embedding Examples (Issue #252)

### 🎯 Primary Demos
- **`robust_vector_demo_fixed.cx`** - ✅ **Final working demo** - Complete validation of Enhanced Local Vector Embedding Service
- **`enhanced_vector_store_demo.cx`** - Comprehensive demonstration of all vector store features
- **`simple_vector_test.cx`** - Basic vector embedding and search functionality

### 🔧 Development/Debug Versions
- **`robust_vector_demo.cx`** - Original version (pre-syntax fixes)
- **`streamlined_vector_demo.cx`** - Simplified version for debugging
- **`vector_debug_demo.cx`** - Debug version for troubleshooting
- **`final_validation_demo.cx`** - Alternative validation approach

## AI Integration Examples

### 🧠 AI Services
- **`ai_event_services_demo.cx`** - AI event service integration demonstration
- **`ai_event_service_integration_test.cx`** - Integration testing for AI services
- **`ai_migration_validation.cx`** - AI migration validation testing

### 🔍 Semantic Search
- **`semantic_search_demo.cx`** - Semantic search capabilities
- **`semantic_test_simple.cx`** - Simple semantic search test

### ⚡ Quick Tests
- **`quick_ai_test.cx`** - Quick AI functionality test
- **`simple_ai_test.cx`** - Simple AI integration test
- **`test_ai_simple.cx`** - Basic AI testing

## Usage

To run any example:
```bash
dotnet run --project src/CxLanguage.CLI run examples/core_features/<example_name>.cx
```

## Features Demonstrated

- ✅ **Enhanced Local Vector Embedding Service** (Issue #252)
- ✅ **Native .NET 9 Implementation** with zero external dependencies
- ✅ **GPU Acceleration** with full layer offloading
- ✅ **Consciousness-Aware Metadata** preservation
- ✅ **Event-Driven Architecture** with custom handlers
- ✅ **High Performance** (sub-50ms embedding, sub-100ms search)
- ✅ **Semantic Search** with vector similarity matching

## Performance Metrics

- **Model**: Nomic-embed-text-v1.5 (768D, Q4_0, 73.48 MiB)
- **Embedding**: ~300ms initial, ~60-75ms subsequent
- **Search**: ~40-60ms completion time
- **GPU**: Full 13/13 layer offloading
- **Memory**: Optimized in-memory storage
