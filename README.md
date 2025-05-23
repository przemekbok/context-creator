# LLM Context Preparation Tool

A specialized WPF application for efficiently preparing and managing context files for Large Language Model (LLM) operations.

## Overview

The LLM Context Preparation Tool helps users select, filter, and organize files that will be included in the context window for LLM operations. This tool allows data scientists, machine learning engineers, researchers, and developers to create optimized context sets for their LLM tasks.

![Application Screenshot](docs/screenshot.png)

## Features

### Core Features

- **Tree View File Explorer**: Browse folder structures with checkboxes for easy selection
- **File Selection**: Select individual files or entire folders for inclusion in the context
- **Configuration Management**: Save and load file selection configurations for reuse
- **Content Filtering**: Quickly find files containing specific terms or concepts
- **Filename Filtering**: Locate files based on filename patterns
- **Visual Highlighting**: Clearly see which files are selected and which match filter criteria
- **Token Estimation**: Get an estimate of token count for selected files
- **Context Export**: Export selected files in various formats (Markdown, JSON, Text)

### Additional Features

- **File Preview**: Preview selected files directly in the application
- **Recent Items**: Quick access to recently opened folders and configurations
- **Status Information**: View stats about selected files (count, size, token estimate)

## Technical Details

- Built with WPF and .NET 8
- Follows the MVVM (Model-View-ViewModel) architectural pattern
- Pure WPF implementation without Windows Forms dependencies
- Built for Windows 10 and newer operating systems

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10 or newer

### Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the application

```bash
git clone https://github.com/przemekbok/context-creator.git
cd context-creator
```

## Usage

### Basic Workflow

1. Open a folder containing your source files (File > Open Folder)
2. Select files for inclusion in your LLM context using checkboxes
3. Use filters to quickly find relevant files (Tools > Content Filter or Filename Filter)
4. Save your configuration for future use (Edit > Save Configuration)
5. Export the selected files in the desired format (File > Export Context)

### Filtering Files

1. Choose between content-based or filename-based filtering
2. Enter your filter criteria (supports regular expressions)
3. Choose whether to include or exclude matching files
4. Apply the filter to see highlighted matches in the file tree

### Keyboard Shortcuts

- **Ctrl+O**: Open folder
- **Ctrl+S**: Save configuration
- **Ctrl+L**: Load configuration
- **Ctrl+E**: Export context
- **Ctrl+F**: Content filter
- **Ctrl+Alt+F**: Filename filter
- **Ctrl+T**: Estimate token count
- **Ctrl+A**: Select all
- **Ctrl+D**: Deselect all
- **Ctrl+I**: Invert selection
- **Esc**: Clear filters

## Architecture

This project follows the MVVM (Model-View-ViewModel) architecture:

### Models

- **FileItem**: Represents a file in the file system
- **FolderItem**: Represents a folder in the file system
- **ContextConfiguration**: Represents a saved context configuration
- **FilterOptions**: Represents options for filtering files

### ViewModels

- **MainViewModel**: Handles the main window logic
- **FilterViewModel**: Handles the filter dialog logic

### Services

- **FilterService**: Handles filtering functionality
- **ContextExportService**: Handles context export functionality
- **DialogService**: Handles dialog display and interaction

### Views

- **MainWindow**: The main application window
- **FilterDialog**: Dialog for filtering files

## Development

### Building from Source

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution

### Project Structure

- `/Models`: Data models
- `/ViewModels`: View models for MVVM pattern
- `/Views`: WPF views
- `/Services`: Services for functionality
- `/Commands`: Command classes for MVVM
- `/Converters`: Value converters for data binding
- `/Resources`: Application resources

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built based on requirements from the LLM Context Preparation Tool Software Requirements Specification
- Follows Windows application design guidelines
- Implements the MVVM pattern for clean separation of concerns
