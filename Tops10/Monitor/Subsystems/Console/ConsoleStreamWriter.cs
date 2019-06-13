using System;
using System.IO;
using System.Text;

namespace Monitor.Subsystems.Console
{
    public class ConsoleStreamWriter : Stream
    {
        public override void Flush() {}

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
                sb.Append(buffer[offset + i]);

            //MonitorContextSingleton.Singleton.TTCALL.OUTSTR(sb.ToString());
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}