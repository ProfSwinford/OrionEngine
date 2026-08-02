// Converted from OrionIDE/IDEProgram.cs.
//
// The entire C# file is commented out, so nothing here is compiled into the
// binary either - this file exists to preserve the placeholder and record what
// the disabled code did and what porting it would involve.
//
// The C# original sketched an IDE front end that:
//   1. Ran the Pong example directly when argv[0] contained 'p' (and exclusively
//      when it also contained 'e').
//   2. Otherwise opened a System.Windows.Forms window with a single "Run" button.
//   3. On click, compiled every *.cs file under Assets/ at runtime with
//      Microsoft.CSharp.CSharpCodeProvider, reflected over the resulting assembly
//      for non-abstract OrionBehaviour subclasses with a parameterless
//      constructor, instantiated each one, and then called EngineCore.EngineCoreInit().
//
// Neither half has a drop-in C++ equivalent:
//   * WinForms - a C++ port needs a GUI toolkit (Win32, Qt, Dear ImGui, ...).
//   * Runtime compilation + reflection - C++ has no CodeDom and no reflection.
//     The closest equivalents are shipping a plugin ABI and loading behaviours
//     from shared libraries (LoadLibrary / dlopen), each exporting a factory
//     function that returns an OrionBehaviour*, or rebuilding the scripts into
//     the executable, which is what OrionIDE/src/Program.cpp does today.
//
// Until one of those is chosen, scripts are registered by constructing them in
// Program.cpp, exactly like the shipping C# Program.cs does.
