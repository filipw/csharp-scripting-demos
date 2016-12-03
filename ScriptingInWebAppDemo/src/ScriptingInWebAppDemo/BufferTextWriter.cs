using System.IO;
using System.Text;

namespace ScriptingInWebAppDemo.Controllers
{
    public class BufferTextWriter : TextWriter
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string Buffer => _builder.ToString();

        public override void Write(char value)
        {
            _builder.Append(value);
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}