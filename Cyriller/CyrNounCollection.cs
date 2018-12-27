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
        private const string NounsResourceName = "nouns.gz";

        protected List<string> rules = new List<string>();
        protected Dictionary<string, List<string>> words = new Dictionary<string, List<string>>();

        /// <summary>
        /// Минимальное кол-во совпадающих символов с конца слова, при поиске наиболее подходящего варианта.
        /// </summary>
        public int NounMinSameLetters { get; set; } = 2;

        /// <summary>
        /// Максимальное кол-во совпадающих символов с конца слова, при поиске наиболее подходящего варианта.
        /// </summary>
        public int NounMaxSameLetters { get; set; } = int.MaxValue;

        public CyrNounCollection()
        {
            CyrData data = new CyrData();
            TextReader treader;
            string line;
            string[] parts;

            treader = data.GetData(NounsResourceName);

            for (; ; )
            {
                line = treader.ReadLine();

                if (line == null)
                {
                    break;
                }

                // Skipping the comments and the empty lines.
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
                {
                    continue;
                }

                if (Char.IsDigit(line[0]))
                {
                    parts = line.Split(' ');
                    rules.Add(parts[1]);
                    continue;
                }

                this.AddWordToTheCollection(line, false);
            }

            treader.Dispose();
        }

        /// <summary>
        /// Adds extra words to the existing collection.
        /// Throws <see cref="ArgumentNullException"/> if provided words enumerable is null.
        /// Throws <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/> if at least one property of the provided nouns is invalid.
        /// See <see cref="AddWord(Model.Json.NounJson)"/>, <see cref="Model.Json.NounJson.Validate"/>.
        /// </summary>
        /// <param name="words">Words to add into the collection. Cannot be null.</param>
        /// <param name="overwrite">
        /// True: replace all existing declencion rules with the provided.
        /// False: append another declencion rule to the existing ones.
        /// </param>
        public void AddWords(IEnumerable<Model.Json.NounJson> words, bool overwrite)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words));
            }

            foreach (Model.Json.NounJson word in words)
            {
                this.AddWord(word, overwrite);
            }
        }

        /// <summary>
        /// Adds an extra word to the existing collection.
        /// Throws <see cref="ArgumentNullException"/> if provided word is null.
        /// Throws <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/> if at least one property of the provided noun is invalid.
        /// See <see cref="Model.Json.NounJson.Validate"/>.
        /// </summary>
        /// <param name="word">Word to add into the collection. Cannot be null.</param>
        /// <param name="overwrite">
        /// True: replace all existing declencion rules with the provided.
        /// False: append another declencion rule to the existing ones.
        /// </param>
        public void AddWord(Model.Json.NounJson word, bool overwrite)
        {
            if (word == null)
            {
                throw new ArgumentNullException(nameof(word));
            }

            word.Validate();

            Rule.NounRule rule = new Rule.NounRule(word);
            int ruleIndex = this.rules.IndexOf(rule.Value);

            if (ruleIndex < 0)
            {
                ruleIndex = this.rules.Count;
                this.rules.Add(rule.Value);
            }

            string line = word.ToDictionaryString(ruleIndex);
            this.AddWordToTheCollection(line, overwrite);
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

        public IEnumerable<string> SelectWords()
        {
            return this.words.Select(x => x.Key);
        }

        /// <summary>
        /// Adds a word to the <see cref="words"/> dictionary.
        /// </summary>
        /// <param name="line">
        /// Word in the Cyriller dictionary format.
        /// See /Cyriller/App_Data/nouns.txt.
        /// </param>
        /// <param name="overwrite">
        /// True: replace all existing declencion rules with the provided.
        /// False: append another declencion rule to the existing ones.
        /// </param>
        protected virtual void AddWordToTheCollection(string line, bool overwrite)
        {
            string[] parts = line.Split(' ');

            if (!words.ContainsKey(parts[0]))
            {
                words.Add(parts[0], new List<string>());
            }
            else if (overwrite)
            {
                words[parts[0]].Clear();
            }

            words[parts[0]].Add(parts[1]);
        }

        protected virtual List<string> GetSimilarDetails(string word, out string collectionWord)
        {
            CyrData data = new CyrData();

            collectionWord = data.GetSimilar(word, words.Keys, this.NounMinSameLetters, this.NounMaxSameLetters);

            if (collectionWord.IsNullOrEmpty())
            {
                return null;
            }

            return this.GetDetails(collectionWord);
        }

        protected virtual List<string> GetDetails(string word)
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
