using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUFL.Framework.Model;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using System.IO;
namespace NUFL.Framework.Symbol
{
    public class SymbolReader
    {
        string _path;
        ProgramEntityFilter _filter;
        AssemblyDefinition _sourceAssembly;
       IEnumerable<string> _pdb_directories;
        Module _module;
        public SymbolReader(string module_path, ProgramEntityFilter filter, IEnumerable<string> pdb_directories, Module module)
        {
            _path = module_path;
            _filter = filter;
            _pdb_directories = pdb_directories;
            _module = module;
        }

        public List<Class> GetClasses()
        {
            var assembly = SourceAssembly;
            List<Class> classes = new List<Class>();
            foreach (var typeDefinition in SourceAssembly.MainModule.Types)
            {
                if (typeDefinition.IsEnum) continue;
                if (typeDefinition.IsInterface && typeDefinition.IsAbstract) continue;
                classes.Add(ConstructClass(typeDefinition));
                if(typeDefinition.HasNestedTypes)
                {
                    foreach(var nestedTypeDefinition in typeDefinition.NestedTypes)
                    {
                        classes.Add(ConstructClass(nestedTypeDefinition));
                    }
                }

            }

            return classes;
        }

        private Class ConstructClass(TypeDefinition typeDefinition)
        {
            var my_class = new Class(_module)
                {
                    FullName = typeDefinition.FullName,
                    Name = typeDefinition.Name
                };
            if(!_filter.UseClass(my_class))
            {
                my_class.Skipped = true;
                return my_class;
            }
            my_class.Methods = new List<Method>();
            foreach(var methodDefinition in typeDefinition.Methods)
            {
                if(methodDefinition.IsAbstract)
                {
                    continue;
                }
                my_class.Methods.Add(ConstrutMethod(my_class, methodDefinition));
            }
            //foreach(var prop in typeDefinition.Properties)
            //{
            //    if (prop.SetMethod != null && !prop.SetMethod.IsAbstract)
            //    {
            //        my_class.Methods.Add(ConstrutMethod(my_class, prop.SetMethod));
            //    }
            //    if (prop.GetMethod != null && !prop.GetMethod.IsAbstract)
            //    {
            //        my_class.Methods.Add(ConstrutMethod(my_class, prop.GetMethod));
            //    }
            //}

            return my_class;
        }

        private Method ConstrutMethod(Class @class, MethodDefinition methodDefinition)
        {
            var method = new Method(methodDefinition.Body, @class)
            {
                FullName = methodDefinition.FullName,
                IsConstructor = methodDefinition.IsConstructor,
                IsStatic = methodDefinition.IsStatic,
                IsGetter = methodDefinition.IsGetter,
                IsSetter = methodDefinition.IsSetter,
                MetadataToken = methodDefinition.MetadataToken.ToInt32(),
                Name = methodDefinition.Name,
            };
            
            if(!_filter.UseMethod(method))
            {
                method.Skipped = true;
            }
            return method;
        }

        private SymbolFolder FindSymbolsFolder()
        {
            var origFolder = Path.GetDirectoryName(_path);
            foreach (var dir in _pdb_directories)
            {
                var sym_folder = FindSymbolsFolder(_path, dir);
                if (sym_folder != null)
                {
                    return sym_folder;
                }
            }
            return FindSymbolsFolder(_path, origFolder) ?? FindSymbolsFolder(_path, Environment.CurrentDirectory);
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
                        var fileName = Path.GetFileName(_path) ?? string.Empty;
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
                }
                return _sourceAssembly;
            }
        }

    }
}
