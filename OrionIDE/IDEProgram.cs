using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using OrionEngine;
using OrionEngine.EngineModules.Rendering;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.Text;
using System.Windows.Controls;
internal class IDEProgram
{
    // Holds instantiated OrionBehaviour script instances created at startup.
    public static List<OrionBehaviour> ScriptInstances { get; } = new();

    // Keep original console writers so output still appears in the real console if desired.
    private static TextWriter originalConsoleOut = Console.Out;
    private static TextWriter originalConsoleError = Console.Error;

    [STAThread]
    static void Main(string[] args)
    {
        // Preserve existing CLI behaviour for the Pong example
        if (args.Length > 0 && args[0].Contains('p')) // check if first argument contains 'p' to run the pong example "OrionIDE.exe p"
        {
            PongExample p = new PongExample();
            if (args.Length > 0 && args[0].Contains('e')) // check if first argument contains 'e' to run the pong example exclusively (solo) "OrionIDE.exe pe"
            {
                EngineCore.EngineCoreInit();
                return;
            }
        }

        // Show a simple WinForms window with a "Run" button that triggers compilation/instantiation.
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new Form()
        {
            Text = "OrionIDE - Scripts",
            // Do not autosize; we'll set client size to fit the console grid and then disable resizing.
            AutoSize = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        var runButton = new System.Windows.Forms.Button()
        {
            Text = "\u25B6",
            Font = new Font("Segoe UI Symbol", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Width = 40,
            Height = 40,
            Anchor = AnchorStyles.Top
        };

        var statusLabel = new System.Windows.Forms.Label()
        {
            Text = "Click Run to compile scripts in the Assets folder.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 24
        };

        var consoleFont = new Font("Consolas", 10, FontStyle.Regular, GraphicsUnit.Point);


        //https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.richtextbox?view=windowsdesktop-10.0
        var consoleBox = new System.Windows.Controls.RichTextBox();
        //{
        //    Multiline = true,
        //    ReadOnly = true,
        //    BackColor = Color.Black,
        //    ForeColor = Color.White,
        //    Font = consoleFont,
        //    ScrollBars = RichTextBoxScrollBars.None,
        //    DetectUrls = false,
        //    WordWrap = true
        //};

        ConsoleRendererModule.canvas.formRender = consoleBox;

        form.Controls.Add(statusLabel);
        form.Controls.Add(runButton);
        form.Controls.Add(consoleBox);

        
        int desiredCols = 120;
        int desiredRows = 30;
        try
        {
            if (Console.WindowWidth > 0) desiredCols = Console.WindowWidth;
            if (Console.WindowHeight > 0) desiredRows = Console.WindowHeight;
        }
        catch { /* running in GUI-only environment might throw; use defaults */ }

        // Measure character size using the chosen font.
        using (var g = form.CreateGraphics())
        {
            var size = g.MeasureString("W", consoleFont);
            int charWidth = Math.Max(1, (int)Math.Ceiling(size.Width));
            int charHeight = Math.Max(1, (int)Math.Ceiling(size.Height));

            // Calculate desired pixel size for the console area (client area grid).
            int widthPx = charWidth * desiredCols + 8;   // small padding to account for borders
            int heightPx = charHeight * desiredRows + 8;

            // Compute required client size to fit statusLabel, runButton and consoleBox with margins.
            const int horizontalMargin = 20;
            const int verticalMargin = 20;
            int clientWidth = Math.Max(widthPx + horizontalMargin, 400);
            int clientHeight = statusLabel.Height + runButton.Height + 8 + heightPx + verticalMargin;

            // Apply client size to the form so layout can be calculated consistently.
            form.ClientSize = new Size(clientWidth, clientHeight);

            // Position controls relative to client area.
            runButton.Location = new Point((form.ClientSize.Width - runButton.Width) / 2, statusLabel.Height + 8);

            consoleBox.Width = widthPx;
            consoleBox.Height = heightPx;
            //consoleBox.Location = new Point((form.ClientSize.Width - consoleBox.Width) / 2, runButton.Bottom + 8);
        }

        // After sizing to fit the console, disable resizing so the view remains character-consistent.
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
        form.MinimizeBox = true;

        // Bridge the engine ConsoleCanvas rendering into the RichTextBox (pixel-by-pixel rendering).
        //SetupCanvasRendering(consoleBox);

        // Async click handler so UI stays responsive while compiling
        runButton.Click += async (s, e) =>
        {
            runButton.Enabled = false;
            statusLabel.Text = "Compiling...";
            try
            {
                var instances = await Task.Run(() => CompileAndInstantiateBehaviours("Assets"));
                if (instances.Count > 0)
                {
                    ScriptInstances.AddRange(instances);
                    statusLabel.Text = $"Instantiated {instances.Count} script(s). Initializing engine...";
                }
                else
                {
                    statusLabel.Text = "No scripts instantiated (see console for details). Initializing engine...";
                }

                // Call engine init after scripts have been instantiated (as requested)
                bool ok = EngineCore.EngineCoreInit();
                statusLabel.Text = ok ? "Engine initialized." : "Engine failed to initialize.";
                MessageBox.Show(form, statusLabel.Text, "OrionIDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Compilation failed. See console.";
                MessageBox.Show(form, $"Error compiling/instantiating scripts:\n{ex}", "OrionIDE - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                runButton.Enabled = true;
            }
        };

        Application.Run(form);
    }

    //private static void SetupCanvasRendering(RichTextBox consoleBox)
    //{
    //    if (consoleBox == null) return;

    //    // Ensure consoleBox has the correct settings for character-grid rendering.
    //    consoleBox.WordWrap = false;
    //    consoleBox.Font = new Font("Consolas", consoleBox.Font.Size);
    //    consoleBox.ReadOnly = true;

    //    // Subscribe to the Canvas OnRender event.
    //    // ConsoleRendererModule.canvas is the engine-side canvas that gets updated and rendered.
    //    var canvas = ConsoleRendererModule.canvas;
    //    if (canvas == null) return;

    //    // When the engine calls canvas.Render(), the canvas will raise OnRender; respond by copying the pixel buffer
    //    canvas.OnRender += () => RenderCanvasToRichTextBox(canvas, consoleBox);
    //}

    /// <summary>
    /// Render the ConsoleCanvas pixel buffer into the RichTextBox. Each contiguous run of characters with the same
    /// foreground/background is appended with SelectionColor/SelectionBackColor set accordingly.
    /// This method marshals to the UI thread and replaces the text content for each frame.
    /// </summary>
    //private static void RenderCanvasToRichTextBox(ConsoleCanvas canvas, RichTextBox rbt)
    //{
    //    if (canvas == null || rbt == null || rbt.IsDisposed) return;

    //    // Always marshal to UI thread
    //    if (rbt.InvokeRequired)
    //    {
    //        try { rbt.BeginInvoke((Action)(() => RenderCanvasToRichTextBox(canvas, rbt))); }
    //        catch { /* swallow */ }
    //        return;
    //    }

    //    try
    //    {
    //        // Replace entire content for simplicity and predictability.
    //        rbt.SuspendLayout();
    //        rbt.Clear();

    //        int w = canvas.Width;
    //        int h = canvas.Height;

    //        // Render row by row. Group consecutive characters with same fore/back to reduce selection toggles.
    //        for (int y = 0; y < h; y++)
    //        {
    //            int x = 0;
    //            while (x < w)
    //            {
    //                // Get first pixel in run
    //                var startPixel = canvas.Get(x, y, backBuffer: false);
    //                int runStart = x;
    //                int runLen = 1;
    //                x++;

    //                // Extend run while same colors and within bounds
    //                while (x < w)
    //                {
    //                    var p = canvas.Get(x, y, backBuffer: false);
    //                    if (p.Foreground == startPixel.Foreground && p.Background == startPixel.Background)
    //                    {
    //                        runLen++;
    //                        x++;
    //                    }
    //                    else
    //                    {
    //                        break;
    //                    }
    //                }

    //                // Build the string for this run
    //                var sb = new StringBuilder(runLen);
    //                for (int t = 0; t < runLen; t++)
    //                {
    //                    var px = canvas.Get(runStart + t, y, backBuffer: false);
    //                    // Replace non-printable with space to avoid control chars breaking layout
    //                    char c = px.Character < ' ' ? ' ' : px.Character;
    //                    sb.Append(c);
    //                }

    //                // Apply colors and append
    //                rbt.SelectionStart = rbt.TextLength;
    //                rbt.SelectionLength = 0;
    //                rbt.SelectionColor = MapConsoleColor(startPixel.Foreground);
    //                // SelectionBackColor exists on .NET Framework WinForms RichTextBox
    //                try
    //                {
    //                    rbt.SelectionBackColor = MapConsoleColor(startPixel.Background);
    //                }
    //                catch
    //                {
    //                    // If SelectionBackColor isn't supported for some reason, skip background coloring.
    //                }
    //                rbt.AppendText(sb.ToString());

    //                // reset selection colors to defaults to avoid leaking them
    //                rbt.SelectionColor = rbt.ForeColor;
    //                try { rbt.SelectionBackColor = rbt.BackColor; } catch { }
    //            }

    //            // Append newline at end of row (except maybe after last row to preserve exact height; keep consistent)
    //            if (y < h - 1)
    //                rbt.AppendText(Environment.NewLine);
    //        }

    //        // Make caret at the end and scroll to caret
    //        rbt.SelectionStart = rbt.TextLength;
    //        rbt.ScrollToCaret();
    //    }
    //    finally
    //    {
    //        rbt.ResumeLayout();
    //    }
    //}

    //private static Color MapConsoleColor(ConsoleColor c)
    //{
    //    // Map ConsoleColor to System.Drawing.Color
    //    return c switch
    //    {
    //        ConsoleColor.Black => Color.Black,
    //        ConsoleColor.DarkBlue => Color.DarkBlue,
    //        ConsoleColor.DarkGreen => Color.DarkGreen,
    //        ConsoleColor.DarkCyan => Color.DarkCyan,
    //        ConsoleColor.DarkRed => Color.DarkRed,
    //        ConsoleColor.DarkMagenta => Color.DarkMagenta,
    //        ConsoleColor.DarkYellow => Color.Olive,
    //        ConsoleColor.Gray => Color.Gray,
    //        ConsoleColor.DarkGray => Color.DimGray,
    //        ConsoleColor.Blue => Color.Blue,
    //        ConsoleColor.Green => Color.Lime,
    //        ConsoleColor.Cyan => Color.Cyan,
    //        ConsoleColor.Red => Color.Red,
    //        ConsoleColor.Magenta => Color.Magenta,
    //        ConsoleColor.Yellow => Color.Yellow,
    //        ConsoleColor.White => Color.White,
    //        _ => Color.White
    //    };
    //}

    private static List<OrionBehaviour> CompileAndInstantiateBehaviours(string assetsPathRelative)
    {
        var created = new List<OrionBehaviour>();

        // Resolve Assets path relative to running app folder
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var assetsPath = Path.IsPathRooted(assetsPathRelative)
            ? assetsPathRelative
            : Path.Combine(baseDir, assetsPathRelative);

        if (!Directory.Exists(assetsPath))
        {
            Console.WriteLine($"Assets folder not found at '{assetsPath}'. No scripts compiled.");
            return created;
        }

        var csFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);
        if (csFiles.Length == 0)
        {
            Console.WriteLine("No .cs files found in Assets.");
            return created;
        }

        using var provider = new CSharpCodeProvider();
        var cp = new CompilerParameters
        {
            GenerateInMemory = true,
            GenerateExecutable = false,
            TreatWarningsAsErrors = false
        };

        // Basic framework references
        cp.ReferencedAssemblies.Add("mscorlib.dll");
        cp.ReferencedAssemblies.Add("System.dll");
        cp.ReferencedAssemblies.Add("System.Core.dll");
        cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
        cp.ReferencedAssemblies.Add("System.Drawing.dll");

        // Reference the running assemblies so scripts can use engine types
        cp.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);                      // OrionIDE (this exe)
        cp.ReferencedAssemblies.Add(typeof(GameObject).Assembly.Location);                          // OrionEngine assembly
        cp.ReferencedAssemblies.Add(typeof(object).Assembly.Location);                              // runtime mscorlib

        var results = provider.CompileAssemblyFromFile(cp, csFiles);

        if (results.Errors != null && results.Errors.HasErrors)
        {
            Console.WriteLine("Compilation errors in Assets scripts:");
            foreach (CompilerError err in results.Errors)
                Console.WriteLine($"  {err.FileName}({err.Line},{err.Column}): {err.ErrorText}");
            // stop on compilation errors
            return created;
        }

        var compiled = results.CompiledAssembly;
        if (compiled == null) return created;

        // Find OrionBehaviour type to compare against
        var behaviourType = typeof(OrionBehaviour);

        // Instantiate all non-abstract types that derive from OrionBehaviour and have a parameterless ctor
        foreach (var t in compiled.GetTypes())
        {
            if (t == null) continue;
            if (!behaviourType.IsAssignableFrom(t)) continue;
            if (t.IsAbstract) continue;

            var ctor = t.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                Console.WriteLine($"Skipping '{t.FullName}' - no parameterless constructor.");
                continue;
            }

            try
            {
                var inst = Activator.CreateInstance(t) as OrionBehaviour;
                if (inst != null)
                {
                    created.Add(inst);
                    Console.WriteLine($"Created script instance: {t.FullName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to instantiate '{t.FullName}': {ex.Message}");
            }
        }

        return created;
    }
}
