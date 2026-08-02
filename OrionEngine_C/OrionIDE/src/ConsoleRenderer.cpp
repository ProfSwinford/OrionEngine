// Converted from OrionIDE/ConsoleRenderer.cs.
//
// The entire C# file is commented out; it is preserved here as a placeholder.
//
// It held two abandoned rendering ideas that predate the ConsoleCanvas the
// engine actually uses:
//
//   Idea 1 - ConsoleRenderer: a System.Drawing (GDI+) renderer that drew into an
//     off-screen Bitmap through a Graphics buffer, then blitted it straight onto
//     the console window via Graphics.FromHwnd(MainWindowHandle). Character cell
//     size was approximated as 8x16 pixels to convert cell coordinates to pixels.
//     A C++ port would talk to GDI/GDI+ directly (or a cross-platform 2D library),
//     and is Windows-only in this form.
//
//   Idea 2 - ASCIIRenderer: an empty stub pointing at a CodeProject article about
//     generating ASCII art from images.
//     https://www.codeproject.com/articles/Generate-ASCII-Art-A-Simple-How-To-in-Csharp
//
// The live renderer is OrionEngine/EngineModules/Rendering/ConsoleRendererModule.
