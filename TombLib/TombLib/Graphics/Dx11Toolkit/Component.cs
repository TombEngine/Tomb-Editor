using System;
using System.Collections.Generic;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Base class providing disposable tracking via <see cref="ToDispose{T}"/>.
    /// Replaces SharpDX.Toolkit.Component.
    /// </summary>
    public class Component : IDisposable
    {
        private List<IDisposable> _disposables;

        /// <summary>Name property for compatibility with SharpDX.Component.</summary>
        public string Name { get; set; }

        protected T ToDispose<T>(T obj) where T : IDisposable
        {
            _disposables ??= new List<IDisposable>();
            _disposables.Add(obj);
            return obj;
        }

        public virtual void Dispose()
        {
            if (_disposables != null)
            {
                for (int i = _disposables.Count - 1; i >= 0; i--)
                    _disposables[i]?.Dispose();
                _disposables.Clear();
            }
        }
    }
}
