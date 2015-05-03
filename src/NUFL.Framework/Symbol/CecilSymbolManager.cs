//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using NUFL.Framework.Model;
using NUFL.Framework.Setting;
using log4net;

namespace NUFL.Framework.Symbol
{
    public class CecilSymbolManager : ISymbolManager
    {
        private const int StepOverLineCode = 0xFEEFEE;
        private readonly IOption _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;
        private AssemblyDefinition _sourceAssembly;
        private readonly Dictionary<int, MethodDefinition> _methodMap = new Dictionary<int, MethodDefinition>();

        public CecilSymbolManager(IOption commandLine, IFilter filter, ILog logger)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
        }

        public string ModulePath { get; private set; }

        public string ModuleName { get; private set; }

        public void Initialise(string modulePath, string moduleName)
        {
            ModulePath = modulePath;
            ModuleName = moduleName;
        }

        private SymbolFolder FindSymbolsFolder()
        {
            var origFolder = Path.GetDirectoryName(ModulePath);

            return FindSymbolsFolder(ModulePath, origFolder) ?? FindSymbolsFolder(ModulePath, _commandLine.TargetDir) ?? FindSymbolsFolder(ModulePath, Environment.CurrentDirectory);
        }

        private static SymbolFolder FindSymbolsFolder(string fileName, string targetfolder)
        {
            if (!string.IsNullOrEmpty(targetfolder) && Directory.Exists(targetfolder))
            {
                var name = Path.GetFileName(fileName);
                //Console.WriteLine(targetfolder);
                if (name != null)
                {
                    if (System.IO.File.Exists(Path.Combine(targetfolder,
                        Path.GetFileNameWithoutExtension(fileName) + ".pdb")))
                    {
                        if (System.IO.File.Exists(Path.Combine(targetfolder, name)))
                            return new SymbolFolder(targetfolder, new PdbReaderProvider());
                    }

                    if (System.IO.File.Exists(Path.Combine(targetfolder, fileName + ".mdb")))
                    {
                        if (System.IO.File.Exists(Path.Combine(targetfolder, name)))
                            return new SymbolFolder(targetfolder, new MdbReaderProvider());
                    }
                }
            }
            return null;
        }

        public AssemblyDefinition SourceAssembly
        {
            get
            {
                if (_sourceAssembly == null)
                {
                    var currentPath = Environment.CurrentDirectory;
                    try
                    {
                        var symbolFolder = FindSymbolsFolder();
                        var folder = symbolFolder.Maybe(_ => _.TargetFolder) ?? Environment.CurrentDirectory;


                        var parameters = new ReaderParameters
                        {
                            SymbolReaderProvider = symbolFolder.SymbolReaderProvider ?? new PdbReaderProvider(),
                            ReadingMode = ReadingMode.Deferred,
                            ReadSymbols = true
                        };
                        var fileName = Path.GetFileName(ModulePath) ?? string.Empty;
                        _sourceAssembly = AssemblyDefinition.ReadAssembly(Path.Combine(folder, fileName), parameters);

                        if (_sourceAssembly != null)
                            _sourceAssembly.MainModule.ReadSymbols(parameters.SymbolReaderProvider.GetSymbolReader(_sourceAssembly.MainModule, _sourceAssembly.MainModule.FullyQualifiedName));
                    }
                    catch (Exception)
                    {
                        // failure to here is quite normal for DLL's with no PDBs => no instrumentation
                        _sourceAssembly = null;
                    }
                    finally
                    {
                        Environment.CurrentDirectory = currentPath;
                    }
                    if (_sourceAssembly == null)
                    {
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugFormat("Cannot instrument {0} as no PDB/MDB could be loaded", ModulePath);
                        }
                    }
                }
                return _sourceAssembly;
            }
        }


        public Class[] GetInstrumentableTypes()
        {
            if (SourceAssembly == null) return new Class[0];
            var classes = new List<Class>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetInstrumentableTypes(typeDefinitions, classes, _filter, ModuleName);
            return classes.ToArray();
        }

        private static void GetInstrumentableTypes(IEnumerable<TypeDefinition> typeDefinitions, List<Class> classes, IFilter filter, string moduleName)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.IsEnum) continue;
                if (typeDefinition.IsInterface && typeDefinition.IsAbstract) continue;
                var @class = new Class { FullName = typeDefinition.FullName };
                if (!filter.InstrumentClass(moduleName, @class.FullName))
                {
                    @class.Skipped = true;
                }
                else if (filter.ExcludeByAttribute(typeDefinition))
                {
                    @class.Skipped = true;
                }

                string filepath = null;
                int start_line = 0;
                int start_col = 0;
                if (!@class.Skipped)
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.Body != null && methodDefinition.Body.Instructions != null)
                        {
                            foreach (var instruction in methodDefinition.Body.Instructions)
                            {
                                if (instruction.SequencePoint != null)
                                {
                                    filepath = instruction.SequencePoint.Document.Url;
                                    start_line = instruction.SequencePoint.StartLine;
                                    start_col = instruction.SequencePoint.StartColumn;
                                    goto loop_out;
                                }
                            }
                        }
                    }
                }
                loop_out:

                

                // only instrument types that are not structs and have instrumentable points
                if (!typeDefinition.IsValueType || filepath != null)
                {
                    @class.SourcePosition = new Position();
                    @class.SourcePosition.SourceFile = filepath;
                    @class.SourcePosition.StartLine = start_line;
                    @class.SourcePosition.StartColumn = start_col;
                    classes.Add(@class);
                }
                if (typeDefinition.HasNestedTypes)
                    GetInstrumentableTypes(typeDefinition.NestedTypes, classes, filter, moduleName);
            }
        }


        public Method[] GetMethodsForType(Class type)
        {
            var methods = new List<Method>();
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            GetMethodsForType(typeDefinitions, type.FullName, methods,  _filter, _commandLine);
            return methods.ToArray();
        }

        private static string GetFirstFile(MethodDefinition definition)
        {
            if (definition.HasBody && definition.Body.Instructions != null)
            {
                var filePath = definition.Body.Instructions
                    .FirstOrDefault(x => x.SequencePoint != null && x.SequencePoint.Document != null && x.SequencePoint.StartLine != StepOverLineCode)
                    .Maybe(x => x.SequencePoint.Document.Url);
                return filePath;
            }
            return null;
        }

        private static void GetMethodsForType(IEnumerable<TypeDefinition> typeDefinitions, string fullName, List<Method> methods, IFilter filter, IOption commandLine)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                if (typeDefinition.FullName == fullName)
                {
                    BuildPropertyMethods(methods, filter, typeDefinition, commandLine);
                    BuildMethods(methods, filter, typeDefinition, commandLine);
                }
                if (typeDefinition.HasNestedTypes)
                    GetMethodsForType(typeDefinition.NestedTypes, fullName, methods, filter, commandLine);
            }
        }

        private static void BuildMethods(ICollection<Method> methods, IFilter filter, TypeDefinition typeDefinition, IOption commandLine)
        {
            foreach (var methodDefinition in typeDefinition.Methods)
            {
                if (methodDefinition.IsAbstract) continue;
                if (methodDefinition.IsGetter) continue;
                if (methodDefinition.IsSetter) continue;

                var method = BuildMethod(filter, methodDefinition, false, commandLine);
                methods.Add(method);
            }
        }

        private static void BuildPropertyMethods(ICollection<Method> methods, IFilter filter, TypeDefinition typeDefinition, IOption commandLine)
        {
            foreach (var propertyDefinition in typeDefinition.Properties)
            {
                var skipped = filter.ExcludeByAttribute(propertyDefinition);

                if (propertyDefinition.GetMethod != null && !propertyDefinition.GetMethod.IsAbstract)
                {
                    var method = BuildMethod(filter, propertyDefinition.GetMethod, skipped, commandLine);
                    methods.Add(method);
                }

                if (propertyDefinition.SetMethod != null && !propertyDefinition.SetMethod.IsAbstract)
                {
                    var method = BuildMethod(filter, propertyDefinition.SetMethod, skipped, commandLine);
                    methods.Add(method);
                }
            }
        }

        private static Method BuildMethod(IFilter filter, MethodDefinition methodDefinition, bool alreadySkippedDueToAttr, IOption commandLine)
        {
            var method = new Method
            {
                Name = methodDefinition.FullName,
                IsConstructor = methodDefinition.IsConstructor,
                IsStatic = methodDefinition.IsStatic,
                IsGetter = methodDefinition.IsGetter,
                IsSetter = methodDefinition.IsSetter,
                MetadataToken = methodDefinition.MetadataToken.ToInt32()
            };


            if (alreadySkippedDueToAttr || filter.ExcludeByAttribute(methodDefinition))
                method.Skipped = true;
            else if (filter.ExcludeByFile(GetFirstFile(methodDefinition)))
                method.Skipped = true;
            else if (commandLine.SkipAutoImplementedProperties && filter.IsAutoImplementedProperty(methodDefinition))
                method.Skipped = true;

            method.SourcePosition = new Position();
            if (methodDefinition.Body != null && methodDefinition.Body.Instructions != null)
            {
                foreach (var instruction in methodDefinition.Body.Instructions)
                {
                    if (instruction.SequencePoint != null)
                    {
                        method.SourcePosition.SourceFile = instruction.SequencePoint.Document.Url;
                        method.SourcePosition.StartLine = instruction.SequencePoint.StartLine;
                        method.SourcePosition.StartColumn = instruction.SequencePoint.StartColumn;
                        return method;
                    }
                }
            }

            method.Skipped = true;

            return method;
        }

        public InstrumentationPoint[] GetSequencePointsForToken(int token)
        {
            BuildMethodMap();
            var list = new List<InstrumentationPoint>();
            GetSequencePointsForToken(token, list);
            return list.ToArray();
        }


        public int GetCyclomaticComplexityForToken(int token)
        {
            BuildMethodMap();
            var complexity = 0;
            GetCyclomaticComplexityForToken(token, ref complexity);
            return complexity;
        }

        private void BuildMethodMap()
        {
            if (_methodMap.Count > 0) return;
            IEnumerable<TypeDefinition> typeDefinitions = SourceAssembly.MainModule.Types;
            BuildMethodMap(typeDefinitions);
        }

        private void BuildMethodMap(IEnumerable<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDefinition in typeDefinitions)
            {
                foreach (var methodDefinition in typeDefinition.Methods
                    .Where(methodDefinition => methodDefinition.Body != null && methodDefinition.Body.Instructions != null))
                {
                    _methodMap.Add(methodDefinition.MetadataToken.ToInt32(), methodDefinition);
                }
                if (typeDefinition.HasNestedTypes)
                {
                    BuildMethodMap(typeDefinition.NestedTypes);
                }
            }
        }

        private void GetSequencePointsForToken(int token, List<InstrumentationPoint> list)
        {
            var methodDefinition = GetMethodDefinition(token);
            if (methodDefinition == null) return;
            UInt32 ordinal = 0;
            list.AddRange(from instruction in methodDefinition.Body.Instructions
                          where instruction.SequencePoint != null && instruction.SequencePoint.StartLine != StepOverLineCode
                          let sp = instruction.SequencePoint
                          select new InstrumentationPoint 
                          {
                              Offset = instruction.Offset,
                              Ordinal = ordinal++,
                              SourcePosition = new Position(sp.Document.Url, sp.StartLine, sp.StartColumn)
                          });
        }

        private static bool BranchIsInGeneratedFinallyBlock(Instruction branchInstruction, MethodDefinition methodDefinition)
        {
            if (!methodDefinition.Body.HasExceptionHandlers)
                return false;

            // a generated finally block will have no sequence points in its range
            return methodDefinition.Body.ExceptionHandlers
                .Where(e => e.HandlerType == ExceptionHandlerType.Finally)
                .Where(e => branchInstruction.Offset >= e.HandlerStart.Offset && branchInstruction.Offset < e.HandlerEnd.Offset)
                .OrderByDescending(h => h.HandlerStart.Offset) // we need to work inside out
                .Any(eh => !methodDefinition.Body.Instructions
                    .Where(i => i.SequencePoint != null && i.SequencePoint.StartLine != StepOverLineCode)
                    .Any(i => i.Offset >= eh.HandlerStart.Offset && i.Offset < eh.HandlerEnd.Offset));
        }

        private List<int> GetBranchPath(Instruction instruction)
        {
            var offsetList = new List<int>();

            if (instruction != null)
            {
                var point = instruction;
                offsetList.Add(point.Offset);
                while (point.OpCode == OpCodes.Br || point.OpCode == OpCodes.Br_S)
                {
                    var nextPoint = point.Operand as Instruction;
                    if (nextPoint != null)
                    {
                        point = nextPoint;
                        offsetList.Add(point.Offset);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return offsetList;
        }

        private Instruction FindClosestSequencePoints(MethodBody methodBody, Instruction instruction)
        {
            var sequencePointsInMethod = methodBody.Instructions.Where(HasValidSequencePoint).ToList();
            if (!sequencePointsInMethod.Any()) return null;
            var idx = sequencePointsInMethod.BinarySearch(instruction, new InstructionByOffsetCompararer());
            Instruction prev;
            if (idx < 0)
            {
                // no exact match, idx corresponds to the next, larger element
                var lower = Math.Max(~idx - 1, 0);
                prev = sequencePointsInMethod[lower];
            }
            else
            {
                // exact match, idx corresponds to the match
                prev = sequencePointsInMethod[idx];
            }

            return prev;
        }

        private bool HasValidSequencePoint(Instruction instruction)
        {
            return instruction.SequencePoint != null && instruction.SequencePoint.StartLine != StepOverLineCode;
        }

        private class InstructionByOffsetCompararer : IComparer<Instruction>
        {
            public int Compare(Instruction x, Instruction y)
            {
                return x.Offset.CompareTo(y.Offset);
            }
        }

        private MethodDefinition GetMethodDefinition(int token)
        {
            return !_methodMap.ContainsKey(token) ? null : _methodMap[token];
        }

        private void GetCyclomaticComplexityForToken(int token, ref int complexity)
        {
            complexity = 0;
          //  throw new NotImplementedException();
          //  var methodDefinition = GetMethodDefinition(token);
          //  if (methodDefinition == null) return;
          //  complexity = Gendarme.Rules.Maintainability.AvoidComplexMethodsRule.GetCyclomaticComplexity(methodDefinition);
        }

    }
}
