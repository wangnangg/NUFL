using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
namespace NUFL.Service
{
    class ServiceProviderDict: Dictionary<string, int>, IDisposable
    {
        StreamReader _reader;
        StreamWriter _writer;
        Stream _stream;
        EventWaitHandle _lock;
        public ServiceProviderDict(Stream stream, EventWaitHandle stream_lock)
            : base()
        {
            _lock = stream_lock;
            _stream = stream;
            _lock.WaitOne();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);   
            Recovery();
        }

        void Recovery()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var start = (char)_reader.Read();
            if(start == '#')
            {
                _reader.ReadLine();
                var line = _reader.ReadLine();
                while(line != "#")
                {
                    var split = line.Split(',');
                    var name = split[0];
                    var port = int.Parse(split[1]);
                    this.Add(name, port);
                    line = _reader.ReadLine();
                }
            }
        }

        void Backup()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            _writer.WriteLine("#");
            foreach (var key in this.Keys)
            {
                _writer.WriteLine(key + ',' + this[key].ToString());
            }
            _writer.WriteLine("#");
            _writer.Flush();
        }
        public void Dispose()
        {
            Backup();
            _lock.Set();
        }
    }
}
