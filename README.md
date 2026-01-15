# CSharpYoloSimple

A modern Windows Forms application for object detection using YOLO deep learning models via ONNX Runtime.

## Overview

CSharpYoloSimple is a C# Windows Forms application that uses ONNX Runtime to load and run YOLO deep learning models for automatic object detection, visualization, and result saving. The application features a modern UI design and intelligent status indication system.

## Features

- **Object Detection**: Uses YOLO ONNX models to detect target objects in images
- **Visualization**: Draws bounding boxes and labels around detected objects
- **Result Saving**: Saves processed images and detection results with timestamp in filenames
- **Smart Status Indicator**: Color-coded status display with text
  - Blue + "_": No image loaded or no detection content
  - Green + "OK": Detection completed, no issues found (normal)
  - Red + "NG": Detection completed, issues found (abnormal)
- **Multi-threaded Processing**: Background thread processing to avoid UI blocking
- **Real-time Clock**: Displays current time
- **Modern UI Design**: Clean, modern interface with flat design principles

## Technical Features

- Built on .NET 8.0 Windows Forms framework
- Uses ONNX Runtime for model inference
- Supports NMS (Non-Maximum Suppression) for duplicate removal
- Multi-threaded asynchronous processing for better user experience
- Comprehensive error handling
- **Auto Path Detection**: Automatically detects model and output paths from project directory
- **Bilingual Comments**: All code comments in both Chinese and English

## System Requirements

- .NET 8.0 Runtime
- Windows OS
- ONNX model file (`sd900.onnx`)

## Installation

1. Clone or download this repository
2. Ensure .NET 8.0 SDK is installed
3. Place the ONNX model file `sd900.onnx` in the project root directory or output directory
4. Build the project using Visual Studio or .NET CLI

## Usage

### Running the Application

1. Build and run the application
2. Click the "選擇圖片" (Select Image) button
3. Choose an image file from the file dialog
4. Wait for processing to complete (runs in background)
5. View results:
   - **Image Display Area**: Shows processed image with detection boxes
   - **Status Indicator**:
     - Blue + "_": No image loaded
     - Green + "OK": Detection normal, no issues
     - Red + "NG": Issues detected, requires attention
   - **Time Display**: Real-time clock
6. Check the `output` folder for saved results (filenames include timestamps)

### Supported Image Formats

- JPEG (.jpg, .jpeg)
- PNG (.png)
- BMP (.bmp)
- GIF (.gif)

## Project Structure

```
CSharpYoloSimple/
├── Program.cs              # Application entry point
├── Form1.cs                # Main form logic (core business code)
├── Form1.Designer.cs       # Form designer code (UI layout)
├── Form1.resx              # Form resource file
├── DetectionBox.cs         # Detection box class (currently unused)
├── test.csproj             # Project configuration file
├── App.config              # Application configuration file
├── sd900.onnx              # ONNX model file (place in project root or output directory)
├── output/                 # Output folder (auto-created)
├── Document.md             # Technical documentation (in Chinese)
└── README.md               # This file
```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.ML.OnnxRuntime | 1.23.2 | ONNX model inference engine |
| OpenCvSharp4 | 4.11.0.20250507 | Image processing library (referenced but not used in code) |

## Output Files

The application saves two types of files in the `output` folder:

1. **Processed Images**: `{original_filename}_{timestamp}_processed.jpg`
   - Example: `image_20240115143025_processed.jpg`
   - Format: JPEG
   - Contains detection boxes and labels

2. **Detection Results**: `{original_filename}_{timestamp}_results.txt`
   - Example: `image_20240115143025_results.txt`
   - Format: UTF-8 text file
   - Contains detection coordinates, confidence scores, and normalized coordinates

**Timestamp Format**: `yyyyMMddHHmmss` (14-digit: year, month, day, hour, minute, second)

## Configuration

The application automatically detects paths:
- **Model Path**: `{project_directory}\sd900.onnx`
- **Output Folder**: `{project_directory}\output` (auto-created if not exists)

The application searches for the project root directory by looking for `.sln` or `.csproj` files, ensuring the model file can be found even when running from the `bin` directory.

## Status Indicator

The status indicator shows three states:

| State | Color | Text | Meaning |
|-------|-------|------|---------|
| No Image | Blue | "_" | No image loaded or no detection content |
| Normal | Green | "OK" | Detection completed, no issues found |
| Abnormal | Red | "NG" | Detection completed, issues detected |

## Building from Source

```bash
# Clone the repository
git clone <repository-url>
cd CSharpYoloSimple

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## Known Issues

- Model output format is assumed to be `[1, 5, 8400]` (may not work with all models)
- Confidence index is hardcoded (assumes confidence at index 4)
- Model filename is fixed to `sd900.onnx` (no dynamic model switching)
- Only supports single image processing (no batch processing)

## Limitations

- Single image processing only (no batch processing)
- No real-time camera detection
- No model switching support
- No parameter adjustment UI (confidence threshold, IOU threshold, etc.)

## Future Improvements

- Batch processing support
- Real-time camera detection
- Model management and switching
- Parameter adjustment UI
- Result export in JSON/XML formats
- Processing history and statistics

## Version Information

- **Project Name**: CSharpYoloSimple
- **Framework**: .NET 8.0-windows
- **ONNX Runtime**: 1.23.2
- **OpenCvSharp4**: 4.11.0.20250507
- **Documentation Version**: 1.2
- **Last Updated**: 2024

## License

[Specify your license here]

## Contributing

[Add contribution guidelines if applicable]

## References

- [ONNX Runtime Documentation](https://onnxruntime.ai/docs/)
- [.NET 8.0 Documentation](https://learn.microsoft.com/dotnet/)
- [Windows Forms Documentation](https://learn.microsoft.com/dotnet/desktop/winforms/)
- [YOLO Model Format](https://github.com/ultralytics/ultralytics)

## Support

For detailed technical documentation, please refer to `Document.md` (in Chinese).

---

**Note**: This application is designed for object detection tasks. Ensure you have the appropriate ONNX model file (`sd900.onnx`) in the correct location before running.
