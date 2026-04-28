using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using OrionEngine;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

internal class Program
{
    // Holds instantiated OrionBehaviour script instances created at startup.
    public static List<OrionBehaviour> ScriptInstances { get; } = new();

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
            Width = 400,
            Height = 150,
            StartPosition = FormStartPosition.CenterScreen
        };

        var runButton = new Button()
        {
            Text = "Run",
            Width = 120,
            Height = 40,
            Location = new Point((form.ClientSize.Width - 120) / 2, (form.ClientSize.Height - 40) / 2),
            Anchor = AnchorStyles.None
        };

        var statusLabel = new Label()
        {
            Text = "Click Run to compile scripts in the Assets folder.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 30
        };

        form.Controls.Add(statusLabel);
        form.Controls.Add(runButton);

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
