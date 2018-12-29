using System;
using System.Collections.Generic;
using System.Text;

namespace Cyriller
{
    public abstract class CyrBaseCollection
    {
        public const string EndOfTheRulesBlock = "// -- End of the Rules block -- //";

        public virtual bool IsSkipLine(string line)
        {
            bool comment = string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#");
            return comment;
        }
    }
}
