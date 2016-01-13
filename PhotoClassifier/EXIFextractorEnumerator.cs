using System.Collections;

// See Asim Goheer http://www.codeproject.com/KB/graphics/exifextractor.aspx?fid=207371
namespace PhotoClassifier
{
    // ReSharper disable once InconsistentNaming
    class EXIFextractorEnumerator : IEnumerator
    {
        Hashtable _exifTable;
        IDictionaryEnumerator _index;

        internal EXIFextractorEnumerator(Hashtable exif)
        {
            _exifTable = exif;
            Reset();
            _index = exif.GetEnumerator();
        }

        public void Reset()
        {
            _index = null;
        }

        public object Current => (new System.Web.UI.Pair(_index.Key, _index.Value));

        public bool MoveNext()
        {
            return _index != null && _index.MoveNext();
        }
    }
}