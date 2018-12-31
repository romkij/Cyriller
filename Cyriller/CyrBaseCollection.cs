using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cyriller.Model;

namespace Cyriller
{
    public abstract class CyrBaseCollection
    {
        public const string EndOfTheRulesBlock = "// -- End of the Rules block -- //";

        protected GendersEnum[] genders = Enum.GetValues(typeof(GendersEnum)).OfType<GendersEnum>().ToArray();
        protected CasesEnum[] cases = Enum.GetValues(typeof(CasesEnum)).OfType<CasesEnum>().ToArray();
        protected NumbersEnum[] numbers = Enum.GetValues(typeof(NumbersEnum)).OfType<NumbersEnum>().ToArray();
        protected CyrData cyrData = new CyrData();

        protected virtual bool IsSkipLine(string line)
        {
            bool comment = string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#");
            return comment;
        }
    }
}
