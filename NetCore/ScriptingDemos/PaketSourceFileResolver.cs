using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System;
using System.IO;
using Microsoft.CodeAnalysis.Scripting;

namespace ScriptingDemos
{
    internal sealed class PaketSourceFileResolver : SourceReferenceResolver
    {
        private readonly SourceReferenceResolver _inner;

        public PaketSourceFileResolver()
        {
            _inner = ScriptSourceResolver.Default;
        }

        public override bool Equals(object other)
        {
            return _inner.Equals(other);
        }

        public override int GetHashCode()
        {
            return _inner.GetHashCode();
        }

        public override string NormalizePath(string path, string baseFilePath)
        {
            return _inner.NormalizePath(path, baseFilePath);
        }

        public override Stream OpenRead(string resolvedPath)
        {
            return _inner.OpenRead(resolvedPath);
        }

        public override string ResolveReference(string path, string baseFilePath)
        {
            return _inner.ResolveReference(path, baseFilePath);
        }
    }
}
