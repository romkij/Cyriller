using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Resources;
using Cyriller.Model;

namespace Cyriller
{
    public class CyrNounCollection
    {
        protected Dictionary<int, string> rules = new Dictionary<int, string>();
        protected Dictionary<string, List<string>> words = new Dictionary<string, List<string>>();

        public CyrNounCollection()
        {
            CyrData data = new CyrData();
            TextReader treader = data.GetData("noun-rules.gz");
            string line;
            string[] parts;

            line = treader.ReadLine();

            while (line != null)
            {
                parts = line.Split(' ');
                rules.Add(int.Parse(parts[0]), parts[1]);
                line = treader.ReadLine();
            }

            treader.Dispose();
            treader = data.GetData("nouns.gz");
            line = treader.ReadLine();

            while (line != null)
            {
                parts = line.Split(' ');

                if (!words.ContainsKey(parts[0]))
                {
                    words.Add(parts[0], new List<string>());
                }

                words[parts[0]].Add(parts[1]);

                line = treader.ReadLine();
            }

            treader.Dispose();
        }

        public CyrNoun Get(string word)
        {
            return this.Get(word, GetConditionsEnum.Strict);
        }

        public CyrNoun Get(string word, GetConditionsEnum condition, GendersEnum? genderID = null, AnimatesEnum? animateID = null, WordTypesEnum? typeID = null)
        {
            string t = word;
            List<string> list = this.GetDetails(t);

            if (list == null || !list.Any())
            {
                t = word.ToLower();
                list = this.GetDetails(t);
            }

            if (list == null || !list.Any())
            {
                t = word.ToLower().UppercaseFirst();
                list = this.GetDetails(t);
            }

            if (list == null || !list.Any())
            {
                List<int> indexes = new List<int>();
                string lower = word.ToLower();

                for (int i = 0; i < lower.Length; i++)
                {
                    if (lower[i] == 'е')
                    {
                        indexes.Add(i);
                    }
                }

                foreach (int index in indexes)
                {
                    t = lower.Substring(0, index) + "ё" + lower.Substring(index + 1);
                    list = this.GetDetails(t);

                    if (list != null && list.Any())
                    {
                        break;
                    }
                }
            }

            if ((list == null || !list.Any()) && condition == GetConditionsEnum.Similar)
            {
                list = this.GetSimilarDetails(word, out t);
            }

            if (list == null || !list.Any())
            {
                throw new CyrWordNotFoundException(word);
            }

            IEnumerable<CyrNounCollectionRow> rows = list.Select(x => CyrNounCollectionRow.Parse(x));
            IEnumerable<CyrNounCollectionRow> filter = rows;

            if (genderID.HasValue)
            {
                filter = filter.Where(x => x.GenderID == (int)genderID);
            }

            if (animateID.HasValue)
            {
                filter = filter.Where(x => x.AnimateID == (int)animateID);
            }

            if (typeID.HasValue)
            {
                filter = filter.Where(x => x.TypeID == (int)typeID);
            }

            CyrNounCollectionRow row = filter.FirstOrDefault();

            if (row == null && condition == GetConditionsEnum.Similar)
            {
                row = rows.FirstOrDefault();
            }

            if (row == null)
            {
                throw new CyrWordNotFoundException(word);
            }

            string[] parts = this.rules[row.RuleID].Split(new char[] { ',', '|' });

            CyrRule[] rules = parts.Select(val => new CyrRule(val)).ToArray();
            CyrNoun noun = new CyrNoun(word, t, (GendersEnum)row.GenderID, (AnimatesEnum)row.AnimateID, (WordTypesEnum)row.TypeID, rules);

            return noun;
        }

        protected List<string> GetSimilarDetails(string word, out string collectionWord)
        {
            CyrData data = new CyrData();
            
            collectionWord = data.GetSimilar(word, words.Keys.ToList());

            if (collectionWord.IsNullOrEmpty())
            {
                return null;
            }

            return this.GetDetails(collectionWord);
        }

        protected List<string> GetDetails(string word)
        {
            if (word.IsNullOrEmpty())
            {
                return null;
            }

            if (words.ContainsKey(word))
            {
                return words[word];
            }

            return null;
        }
    }
}
