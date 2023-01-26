using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

class Obfuscator {
    public static dynamic Obfuscate(string code) {
        var variableRegex = new Regex(@"(var|let|const)\s(\w+)");
        var newCode = code;

        // variable obfuscation
        foreach (Match match in variableRegex.Matches(code)) {
            var variableName = match.Groups[2].Value;
            var newVariableName = "";
            for (var i = 0; i < variableName.Length; i++) {
                newVariableName += (char)(variableName[i] + 2);
            }
            newCode = newCode.Replace(variableName, newVariableName);
        }

        // control flow obfuscation
        var ifRegex = new Regex(@"if\s*\(");
        var ifStatements = ifRegex.Matches(newCode);
        var jumpTable = "var jumpTable = {};\n";
        for (var i = 0; i < ifStatements.Count; i++) {
            jumpTable += "jumpTable[" + i + "] = function() { " + ifStatements[i] + " };\n";
        }
        newCode = ifRegex.Replace(newCode, match => "jumpTable[" + Array.IndexOf(ifStatements, match) + "]();\n");
newCode = jumpTable + newCode;
    // string obfuscation
    var stringRegex = new Regex(@"(\""|')([^""']+)(\""|')");
    newCode = stringRegex.Replace(newCode, match => {
        var newString = "";
        for (var i = 0; i < match.Value.Length; i++) {
            newString += (char)(match.Value[i] + 2);
        }
        return newString;
    });

    // data obfuscation
    var dataRegex = new Regex(@"(\d+)");
    newCode = dataRegex.Replace(newCode, match => (int.Parse(match.Value) + 2).ToString());

    // code generation
    var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
    var parameters = new CompilerParameters() {
        GenerateInMemory = true,
        GenerateExecutable = false
    };
    var results = csc.CompileAssemblyFromSource(parameters, newCode);
    if (results.Errors.Count > 0) {
        throw new Exception("There were errors generating the code.");
    }
    var assembly = results.CompiledAssembly;
    var type = assembly.GetType("ObfuscatedCode");
    var instance = Activator.CreateInstance(type);
    return instance;
}