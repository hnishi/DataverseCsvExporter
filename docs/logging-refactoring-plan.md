# Logging Refactoring Plan

## Current State Analysis

### Existing Implementation

1. Dual logging implementation

   - Microsoft.Extensions.Logging (CsvExporter, DataverseClient)
   - Custom ErrorHandler class (Program.cs)

2. Issues

   - Inconsistent logging methods
   - Error messages in Japanese
   - Limited ErrorHandler functionality (no log levels)

3. Good practices in place
   - Proper ILoggerFactory configuration
   - Structured logging in some classes
   - Configurable log levels

## Improvement Plan

### Phase 1: Revamp ErrorHandler

- Convert to ILogger-based implementation
- Introduce log levels
- Implement structured logging

### Phase 2: Improve Program.cs

- Replace ErrorHandler usage with ILogger
- Enhance startup/completion logging
- Improve global error handling

### Phase 3: Standardize Messages

- Convert all error messages to English
- Unify log message format
- Implement message templates

## Implementation Details

### Phase 1: ErrorHandler Class Changes

1. Add ILogger dependency
2. Implement log levels based on error types
3. Convert to structured logging format

### Phase 2: Program.cs Updates

1. Replace ErrorHandler.LogToConsole calls with ILogger
2. Improve application lifecycle logging
3. Enhance global exception handling

### Phase 3: Message Standardization

1. Translate all error messages to English
2. Standardize log message formats
3. Implement consistent message templates

## Migration Guide

1. ErrorHandler migration:

   - Update service registration
   - Replace direct Console usage
   - Implement proper log levels

2. Program.cs updates:

   - Update logging initialization
   - Replace legacy log calls
   - Enhance error handling

3. Message standardization:
   - Update all error messages to English
   - Implement consistent formatting
   - Add message templates
