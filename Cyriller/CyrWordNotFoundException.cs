using System;

namespace Cyriller
{
    public class CyrWordNotFoundException : Exception
    {
        public CyrWordNotFoundException(string word)
            : base("The word was not found in the collection. Word: [" + word + "].")
        {
            this.Word = word;
        }

        public string Word
        {
            get;
            protected set;
        }
    }
}
