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


        Class[] GetInstrumentableTypes();

        Method[] GetMethodsForType(Class type);

        InstrumentationPoint[] GetSequencePointsForToken(int token);

        int GetCyclomaticComplexityForToken(int token);

        AssemblyDefinition SourceAssembly { get; }
    }
}