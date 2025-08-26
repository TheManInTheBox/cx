# Document Ingestion Demo

This example demonstrates the consciousness-aware document ingestion service that processes text documents into searchable vector chunks for the local vector database.

## Features

- **Multi-format Support**: TXT, MD, JSON document processing
- **Intelligent Chunking**: Splits large documents into vector-sized chunks with overlap
- **Event-Driven Processing**: Uses CX Language event patterns for document processing
- **Batch Processing**: Efficient processing of multiple documents
- **Vector Integration**: Stores processed chunks in the local vector database

## Usage

Run this example to see document ingestion in action:

```
dotnet run --project src/CxLanguage.CLI/CxLanguage.CLI.csproj run examples/production/document_ingestion_demo/document_ingestion_demo.cx
```

The demo will:
1. Create sample documents
2. Process them through the ingestion service
3. Store vector chunks in the local database
4. Demonstrate search capabilities
