//===========================================================================
// File:        Program.cs
// Project:     DeobfuscationScorer
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Computes the deobfuscation score by comparing the
//              decompiled C# output of an obfuscated assembly against
//              the decompiled output of its reference (non-obfuscated)
//              counterpart.
//
//              The score combines two components: the proportion of
//              type names that ILSpy reconstructed identically to the
//              reference (S_type), and the proportion of textual
//              identifiers shared between both outputs (S_id). The
//              final S_deobf is the arithmetic mean of the two.
//
//              The metric definitions correspond to the formulas given
//              in the methodology chapter of the thesis. Values close
//              to 1.0 indicate that the obfuscator failed to obscure
//              the program structure from automated decompilation;
//              values close to 0.0 indicate strong resistance.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

if (args.Length < 3)
{
    Console.WriteLine("Usage: DeobfuscationScorer <reference_dir> <obfuscated_dir> <output.json>");
    return;
}

var refDir = args[0];
var obfDir = args[1];
var outputPath = args[2];

if (!Directory.Exists(refDir) || !Directory.Exists(obfDir))
{
    Console.WriteLine("One or both directories not found.");
    return;
}

// --- Type name matching (S_type) -----------------------------------------
// ILSpy emits one .cs file per decompiled type. Type-level matching is
// therefore performed on file name level. The synthetic Program file and
// any names starting with '-' (compiler-generated artefacts) are excluded
// to keep the comparison aligned with the metric definition.
var refFiles = Directory.GetFiles(refDir, "*.cs", SearchOption.AllDirectories)
    .Select(f => Path.GetFileNameWithoutExtension(f))
    .Where(n => n != "Program" && !n.StartsWith("-"))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var obfFiles = Directory.GetFiles(obfDir, "*.cs", SearchOption.AllDirectories)
    .Select(f => Path.GetFileNameWithoutExtension(f))
    .Where(n => n != "Program" && !n.StartsWith("-"))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var matchedTypeNames = refFiles.Intersect(obfFiles, StringComparer.OrdinalIgnoreCase).ToList();
var typeNameScore = refFiles.Count > 0
    ? Math.Round((double)matchedTypeNames.Count / refFiles.Count, 3)
    : 0;

// --- Identifier matching (S_id) ------------------------------------------
// Identifiers are extracted from the textual content of every .cs file
// in both decompilation outputs and compared as case-insensitive sets.
var refIdentifiers = ExtractIdentifiers(refDir);
var obfIdentifiers = ExtractIdentifiers(obfDir);

var matchedIdentifiers = refIdentifiers.Intersect(obfIdentifiers, StringComparer.OrdinalIgnoreCase).ToList();
var identifierScore = refIdentifiers.Count > 0
    ? Math.Round((double)matchedIdentifiers.Count / refIdentifiers.Count, 3)
    : 0;

// --- Heuristic detection of obfuscated-looking names ---------------------
// Type names of length <= 2, or matching the pattern of a few alphabetic
// characters optionally followed by digits (e.g. "a", "b1", "x2"), are
// flagged as obfuscation indicators. Reported separately from S_deobf for
// diagnostic purposes; not part of the deobfuscation score itself.
var obfuscatedLookingNames = obfFiles
    .Count(n => n.Length <= 2 || System.Text.RegularExpressions.Regex.IsMatch(n, @"^[a-zA-Z]{1,2}\d*$"));
var obfuscationRatio = obfFiles.Count > 0
    ? Math.Round((double)obfuscatedLookingNames / obfFiles.Count, 3)
    : 0;

// --- Aggregate result ----------------------------------------------------
var result = new
{
    ReferenceTypeCount = refFiles.Count,
    ObfuscatedTypeCount = obfFiles.Count,
    MatchedTypeNames = matchedTypeNames,
    TypeNameScore = typeNameScore,
    ReferenceIdentifierCount = refIdentifiers.Count,
    ObfuscatedIdentifierCount = obfIdentifiers.Count,
    MatchedIdentifierCount = matchedIdentifiers.Count,
    IdentifierScore = identifierScore,
    ObfuscatedLookingNames = obfuscatedLookingNames,
    ObfuscationDetectionRatio = obfuscationRatio,
    DeobfuscationScore = Math.Round((typeNameScore + identifierScore) / 2, 3)
};

var json = System.Text.Json.JsonSerializer.Serialize(result,
    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, json);

Console.WriteLine($"Reference types:     {result.ReferenceTypeCount}");
Console.WriteLine($"Obfuscated types:    {result.ObfuscatedTypeCount}");
Console.WriteLine($"Type name score:     {result.TypeNameScore} ({matchedTypeNames.Count} matched)");
Console.WriteLine($"Identifier score:    {result.IdentifierScore} ({matchedIdentifiers.Count}/{refIdentifiers.Count})");
Console.WriteLine($"Obfuscation ratio:   {result.ObfuscationDetectionRatio}");
Console.WriteLine($"Deobfuscation score: {result.DeobfuscationScore}");
Console.WriteLine($"Output: {outputPath}");

/// <summary>
/// Extracts the set of identifier-like words (length &gt;= 3, matching
/// <c>[a-zA-Z_][a-zA-Z0-9_]+</c>) from every .cs file in the given
/// directory tree, excluding C# language keywords and the most common
/// framework type names.
/// </summary>
/// <param name="dir">The root directory of the decompiled output.</param>
/// <returns>A case-insensitive set of distinct identifiers.</returns>
static HashSet<string> ExtractIdentifiers(string dir)
{
    var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var keywords = new HashSet<string> { "public", "private", "protected", "internal",
        "class", "void", "int", "string", "bool", "double", "float", "return",
        "new", "this", "base", "static", "readonly", "const", "var", "if", "else",
        "for", "foreach", "while", "using", "namespace", "get", "set", "null",
        "true", "false", "override", "virtual", "abstract", "sealed", "async",
        "await", "throw", "try", "catch", "finally", "object", "List", "IEnumerable",
        "Task", "Console", "Math", "string", "String", "int", "long", "bool" };

    foreach (var file in Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
    {
        var content = File.ReadAllText(file);
        var words = System.Text.RegularExpressions.Regex.Matches(content, @"\b[a-zA-Z_][a-zA-Z0-9_]{2,}\b")
            .Select(m => m.Value)
            .Where(w => !keywords.Contains(w));
        foreach (var word in words)
            identifiers.Add(word);
    }
    return identifiers;
}