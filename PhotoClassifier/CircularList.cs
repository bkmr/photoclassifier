using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PhotoClassifier
{
    internal class CircularList<T> : List<T>
    {
        private int _ix = -1;
        private readonly int _numberOfItems;

        public CircularList([NotNull] IEnumerable<T> items)
        {
            AddRange(items);
            _numberOfItems = Count;
        }

        private int GetNextIndex()
        {
            _ix++;
            if (_ix >= _numberOfItems) _ix = 0;
            return _ix;
        }

        private int GetPreviousIndex()
        {
            _ix--;
            if (_ix <= 0) _ix = _numberOfItems - 1;
            return _ix;
        }

        public T Current
        {
            get
            {
                if (_ix < 0 || _ix >= _numberOfItems) _ix = 0;
                return this[_ix];
            }
        }

        public T MoveNext() => this[GetNextIndex()];

        public T MovePrevious() => this[GetPreviousIndex()];
    }
}