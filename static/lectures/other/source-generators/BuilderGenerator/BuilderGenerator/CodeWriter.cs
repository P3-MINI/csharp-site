using System;
using System.Text;

namespace BuilderGenerator
{
    /// <summary>
    /// A helper class to write indented C# code.
    /// Wraps a StringBuilder and manages indentation levels.
    /// </summary>
    internal class CodeWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indent = 0;

        /// <summary>
        /// Appends a line with the current indentation.
        /// </summary>
        public void AppendLine(string line = "")
        {
            if (!string.IsNullOrEmpty(line))
            {
                _sb.Append(' ', _indent * 4);
                _sb.Append(line);
            }
            _sb.AppendLine();
        }

        /// <summary>
        /// Increases indentation and opens a block with '{'.
        /// Use inside a 'using' statement with Block().
        /// </summary>
        public void StartBlock()
        {
            AppendLine("{");
            _indent++;
        }

        /// <summary>
        /// Decreases indentation and closes a block with '}'.
        /// </summary>
        public void EndBlock()
        {
            _indent--;
            AppendLine("}");
        }

        /// <summary>
        /// Returns an IDisposable that opens a block and closes it when disposed.
        /// Usage: using (writer.Block()) { ... }
        /// </summary>
        public IDisposable Block()
        {
            StartBlock();
            return new DisposableAction(EndBlock);
        }

        public override string ToString() => _sb.ToString();

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            public DisposableAction(Action action) => _action = action;
            public void Dispose() => _action();
        }
    }
}
