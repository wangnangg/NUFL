//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using Mono.Cecil;
using NUFL.Framework.Model;

namespace NUFL.Framework.Symbol
{
    public interface ISymbolManager
    {
        string ModulePath { get; }

        string ModuleName { get; }

        File[] GetFiles();

        Class[] GetInstrumentableTypes();

        Method[] GetMethodsForType(Class type, File[] files);

        SequencePoint[] GetSequencePointsForToken(int token);

        BranchPoint[] GetBranchPointsForToken(int token);

        int GetCyclomaticComplexityForToken(int token);

        AssemblyDefinition SourceAssembly { get; }
    }
}