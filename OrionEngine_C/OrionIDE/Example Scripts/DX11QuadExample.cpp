// Converted from OrionIDE/Example Scripts/DX11QuadExample.cs.
//
// The entire C# file is commented out; it is preserved here as a placeholder.
//
// The disabled code was the Silk.NET refactor of the .NET Foundation's
// Direct3D11 tutorial: it opened a Silk.NET window with GraphicsAPI.None, created
// a D3D11 device and a double-buffered flip-discard swapchain, uploaded a
// four-vertex / six-index quad, compiled an inline HLSL vs_5_0 + ps_5_0 shader
// pair with D3DCompiler, built the matching input layout, and drew the quad each
// frame, closing the window on Escape.
//
// Porting it means dropping Silk.NET and calling Direct3D 11 through the Windows
// SDK headers directly (<d3d11.h>, <dxgi1_2.h>, <d3dcompiler.h>, linking
// d3d11.lib / dxgi.lib / d3dcompiler.lib) plus a window: Win32 CreateWindowEx, or
// a cross-platform layer such as SDL or GLFW. That is a Windows-only path, so it
// is deliberately left out of the portable console engine build; the ConsoleCanvas
// renderer remains the only renderer OrionEngine ships.
//
// Original references:
//   https://github.com/dotnet/Silk.NET - Direct3D11 tutorial sample
