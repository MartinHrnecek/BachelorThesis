//===========================================================================
// File:        Program.cs
// Project:     Analyzer
// Author:      Martin Hrnecek <xhrnecm00>
// Description: IL code analyzer for .NET assemblies. Computes the structural
//              and complexity metrics defined by the thesis methodology
//              (Section 3.4) directly from IL instructions and metadata
//              using the dnlib library.
//
//              Computed metrics:
//                * Structural counts: types, methods, IL instructions,
//                  unique string literals
//                * Halstead complexity metrics: Volume, Difficulty, Effort
//                  (operators = IL opcodes, operands = instruction arguments)
//                * Cyclomatic complexity: average and maximum across all
//                  methods with bodies (approximated by counting flow-control
//                  instructions)
//                * Shannon entropy of type and method names
//
//              The result is serialized as JSON for downstream processing
//              by the pipeline (compare, statistics, deobfuscation scoring).
//
// Usage:       Analyzer <assembly.dll> <output.json>
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Text.Json;

// --- Argument validation ----------------------------------------------------

if (args.Length < 2)
{
    Console.WriteLine("Usage: Analyzer <assembly.dll> <output.json>");
    return;
}

var assemblyPath = args[0];
var outputPath = args[1];

if (!File.Exists(assemblyPath))
{
    Console.WriteLine($"File not found: {assemblyPath}");
    return;
}

Console.WriteLine($"Analyzing: {assemblyPath}");

// --- Load the target assembly -----------------------------------------------
// dnlib gives us full metadata + IL access without loading the assembly into
// the CLR (no execution, no JIT side effects). The synthetic <Module> type is
// excluded so that metrics reflect user-defined types only.

var module = ModuleDefMD.Load(assemblyPath);

var types = module.Types
    .Where(t => t.FullName != "<Module>")
    .ToList();

var methods = types.SelectMany(t => t.Methods).ToList();
var methodsWithBody = methods.Where(m => m.HasBody).ToList();
var allInstructions = methodsWithBody.SelectMany(m => m.Body.Instructions).ToList();

// --- Structural metrics -----------------------------------------------------

// Total number of IL instructions across all method bodies. Forms the
// basis of the M_IL ratio metric (obfuscated / reference).
var ilInstructions = allInstructions.Count;

// Unique non-empty string literals reachable via ldstr instructions.
// Used to compute M_strings: a value close to 0 indicates effective
// string encryption; values above 1 occur when the obfuscator injects
// its own runtime strings (typically .NET Reactor).
var strings = allInstructions
    .Where(i => i.OpCode == OpCodes.Ldstr)
    .Select(i => i.Operand?.ToString() ?? "")
    .Where(s => !string.IsNullOrEmpty(s))
    .Distinct()
    .ToList();

// --- Halstead complexity metrics --------------------------------------------
// IL instructions are treated as operators; their arguments (method/string
// references, numeric literals) are treated as operands. Methods without
// bodies (abstract, external) are implicitly excluded above.

var operators = allInstructions
    .Select(i => i.OpCode.Name)
    .ToList();

var operands = allInstructions
    .Where(i => i.Operand != null)
    .Select(i => i.Operand!.ToString() ?? "")
    .Where(s => !string.IsNullOrEmpty(s))
    .ToList();

int N1 = operators.Count;             // Total operator occurrences
int N2 = operands.Count;              // Total operand occurrences
int n1 = operators.Distinct().Count();// Distinct operators
int n2 = operands.Distinct().Count();// Distinct operands

double vocabulary = n1 + n2;
double length = N1 + N2;
double volume = vocabulary > 0 ? length * Math.Log2(vocabulary) : 0;
double difficulty = n2 > 0 ? (n1 / 2.0) * ((double)N2 / n2) : 0;
double effort = difficulty * volume;

// --- Cyclomatic complexity --------------------------------------------------
// Approximated by counting flow-control instructions per method body.
// CC(m) = 1 + #{instructions whose FlowControl is Branch, Cond_Branch or
// Return}. This avoids constructing an explicit control-flow graph while
// preserving the relative ordering between methods, which is sufficient
// for the comparative analysis in the thesis.

var cyclomaticValues = methodsWithBody.Select(m =>
{
    int edges = 0;
    int nodes = m.Body.Instructions.Count;
    foreach (var instr in m.Body.Instructions)
    {
        var flow = instr.OpCode.FlowControl;
        if (flow == FlowControl.Branch ||
            flow == FlowControl.Cond_Branch ||
            flow == FlowControl.Return)
            edges++;
    }
    return 1 + edges;
}).ToList();

double avgCyclomatic = cyclomaticValues.Count > 0
    ? Math.Round(cyclomaticValues.Average(), 3)
    : 0;
double maxCyclomatic = cyclomaticValues.Count > 0
    ? cyclomaticValues.Max()
    : 0;

// --- Shannon entropy of symbol names ----------------------------------------
// High entropy implies that most names are unique (typical for Obfuscar and
// .NET Reactor); low entropy implies repeated identifiers (typical for
// Eazfuscator unicode-escape sequences). Constructors are excluded so that
// the fixed names .ctor and .cctor do not bias the result.

var typeNames = types.Select(t => t.Name.ToString()).ToList();
var methodNames = methods.Select(m => m.Name.ToString())
    .Where(n => n != ".ctor" && n != ".cctor")
    .ToList();

double typeEntropy = ComputeEntropy(typeNames);
double methodEntropy = ComputeEntropy(methodNames);

// --- Assemble the result and serialize to JSON ------------------------------

var result = new
{
    AssemblyName = module.Name.ToString(),
    TypeCount = types.Count,
    MethodCount = methods.Count,
    ILInstructionCount = ilInstructions,
    StringLiterals = strings,
    StringLiteralCount = strings.Count,
    Halstead = new
    {
        N1_TotalOperators = N1,
        N2_TotalOperands = N2,
        n1_UniqueOperators = n1,
        n2_UniqueOperands = n2,
        Vocabulary = Math.Round(vocabulary, 3),
        Length = Math.Round(length, 3),
        Volume = Math.Round(volume, 3),
        Difficulty = Math.Round(difficulty, 3),
        Effort = Math.Round(effort, 3)
    },
    CyclomaticComplexity = new
    {
        Average = avgCyclomatic,
        Maximum = maxCyclomatic
    },
    SymbolEntropy = new
    {
        TypeNameEntropy = Math.Round(typeEntropy, 3),
        MethodNameEntropy = Math.Round(methodEntropy, 3)
    }
};

var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, json);

// --- Console summary --------------------------------------------------------

Console.WriteLine($"Types:              {result.TypeCount}");
Console.WriteLine($"Methods:            {result.MethodCount}");
Console.WriteLine($"IL Instructions:    {result.ILInstructionCount}");
Console.WriteLine($"String literals:    {result.StringLiteralCount}");
Console.WriteLine($"Halstead Volume:    {result.Halstead.Volume:F1}");
Console.WriteLine($"Halstead Difficulty:{result.Halstead.Difficulty:F1}");
Console.WriteLine($"Halstead Effort:    {result.Halstead.Effort:F0}");
Console.WriteLine($"Cyclomatic avg:     {result.CyclomaticComplexity.Average}");
Console.WriteLine($"Cyclomatic max:     {result.CyclomaticComplexity.Maximum}");
Console.WriteLine($"Type entropy:       {result.SymbolEntropy.TypeNameEntropy}");
Console.WriteLine($"Method entropy:     {result.SymbolEntropy.MethodNameEntropy}");
Console.WriteLine($"Output: {outputPath}");

// --- Helper functions -------------------------------------------------------

/// <summary>
/// Computes Shannon entropy (in bits) over the multiset of names.
/// </summary>
/// <param name="names">The collection of symbol names.</param>
/// <returns>
/// Entropy in bits: 0 when the input is empty or all names are identical,
/// log2(N) when all names are distinct.
/// </returns>
static double ComputeEntropy(List<string> names)
{
    if (names.Count == 0) return 0;
    var freq = names.GroupBy(n => n)
        .Select(g => (double)g.Count() / names.Count);
    return -freq.Sum(p => p * Math.Log2(p));
}