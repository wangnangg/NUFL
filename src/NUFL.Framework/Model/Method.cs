using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class Method : ProgramEntityBase
    {
        private const int StepOverLineCode = 0xFEEFEE;
        public int MetadataToken { get; set; }

        public string FullName { get; set; }

        public string Name { get; set; }


        SourceFile _file = null;
        public SourceFile File 
        { 
                
            get
            {
                if (_file != null)
                {
                    return _file;
                }
                if (_built)
                {
                    return _file;
                } else
                {
                    TryFindPosition();
                    return _file;
                }
            }
        }

        List<InstrumentationPoint> _points = null;
        public List<InstrumentationPoint> Points 
        { 
            get
            {
                return _points;
            }
        }


        bool _built = false;
        public void BuildMethod()
        {
            if(_built)
            {
                return;
            }
            Program program = (Program)(Parent.Parent.Parent);
            _points = new List<InstrumentationPoint>();
            if (_body != null && _body.Instructions != null)
            {
                foreach (var ins in _body.Instructions)
                {
                    if (ins.SequencePoint != null && ins.SequencePoint.StartLine != StepOverLineCode)
                    {
                        if (File == null)
                        {
                            _file = program.Files.GetFile(ins.SequencePoint.Document.Url);
                            File.Methods.Add(this);
                        }
                        if(StartLine == null)
                        {
                            _start_line = ins.SequencePoint.StartLine;
                        }
                        var point = ConstructPoint(ins.SequencePoint);
                        point.Offset = ins.Offset;
                        _points.Add(point);
                    }
                }
            }
            _built = true;
        }

      
        private InstrumentationPoint ConstructPoint(Mono.Cecil.Cil.SequencePoint sequencePoint)
        {
            var point = new InstrumentationPoint(this)
            {
                StartLine = sequencePoint.StartLine,
                StartColumn = sequencePoint.StartColumn,
                EndLine = sequencePoint.EndLine,
                EndColumn = sequencePoint.EndColumn,
            };
            return point;
        }

        private Mono.Cecil.Cil.MethodBody _body;

        

        public Method(Mono.Cecil.Cil.MethodBody body, Class @class)
            : base(@class)
        {
            _body = body;
            
        }



        public bool IsConstructor { get; set; }

        public bool IsStatic { get; set; }

        public bool IsGetter { get; set; }
        
        public bool IsSetter { get; set; }

        public bool Skipped { get; set; }


        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach (var entry in Points)
                {
                    yield return entry;
                }
                yield break;
            }
        }

        public override string DisplayName
        {
            get
            {
                return Name;
            }
        }


        void TryFindPosition()
        {
            Program program = (Program)(Parent.Parent.Parent);
            if (_body != null && _body.Instructions != null)
            {
                foreach (var ins in _body.Instructions)
                {
                    if (ins.SequencePoint != null && ins.SequencePoint.StartLine != StepOverLineCode)
                    {
                        if (_file == null)
                        {
                            _file = program.Files.GetFile(ins.SequencePoint.Document.Url);
                            File.Methods.Add(this);
                        }
                        if (_start_line == null)
                        {
                            _start_line = ins.SequencePoint.StartLine;
                        }
                        if(_file != null && _start_line != null)
                        {
                            break;
                        }
                    }
                }
            }
        }

        int? _start_line = null;
        public int? StartLine 
        { 
            get
            {
                if (_start_line != null)
                {
                    return _start_line;
                }
                if (_built)
                {
                    return _start_line;
                } else
                {
                    TryFindPosition();
                    return _start_line;
                }
            }
        }

    }
}
