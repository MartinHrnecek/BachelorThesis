//===========================================================================
// File:        Program.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Entry point of the CLI test application. Sequentially
//              exercises the six language-feature categories used to
//              evaluate obfuscation behaviour: string literals,
//              branching and loops, async/await, generics with LINQ,
//              reflection, and exception handling.
//
//              The output is deterministic and is consumed by the
//              pipeline to verify functional correctness of the
//              obfuscated builds against the reference build.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using CliApp;

Console.WriteLine("=== CLI Obfuscation Test App ===");

// 1. String literals
var greeter = new Greeter("BachelorThesis");
greeter.Greet();

// 2. Branching + loops
var analyzer = new DataAnalyzer();
analyzer.RunAnalysis();

// 3. Async/await
await AsyncWorker.DoWorkAsync();

// 4. Generics + LINQ
var repository = new Repository<string>();
repository.Add("alpha");
repository.Add("beta");
repository.Add("gamma");
repository.Add("delta");
repository.Add("epsilon");

var filtered = repository.GetFiltered(x => x.StartsWith("a") || x.Length > 4);
Console.WriteLine($"Filtered: {string.Join(", ", filtered)}");

// 5. Reflection
var inspector = new ReflectionInspector();
inspector.InspectType(typeof(Greeter));

// 6. Exception handling
var safeCalc = new SafeCalculator();
safeCalc.RunDemo();

Console.WriteLine("=== Done ===");