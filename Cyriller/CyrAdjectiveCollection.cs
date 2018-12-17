using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cyriller.Model;

namespace Cyriller
{
    public class CyrAdjectiveCollection
    {
        protected Dictionary<int, string> rules = new Dictionary<int, string>();
        protected Dictionary<string, string> masculineWords = new Dictionary<string, string>();
        protected Dictionary<string, KeyValuePair<string, string>> feminineWords = new Dictionary<string, KeyValuePair<string, string>>();
        protected Dictionary<string, KeyValuePair<string, string>> neuterWords = new Dictionary<string, KeyValuePair<string, string>>();

        /// <summary>The minimum number of matching letters, from the right end, when searching for similar matches, for adjectives</summary>
        protected int AdjectiveMinSameLetters = 3;

        public CyrAdjectiveCollection()
        {
            CyrData data = new CyrData();
            TextReader treader = data.GetData("adjective-rules.gz");
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
            treader = data.GetData("adjectives.gz");
            line = treader.ReadLine();

            while (line != null)
            {
                parts = line.Split(' ');

                if (!masculineWords.ContainsKey(parts[0]))
                {
                    masculineWords.Add(parts[0], parts[1]);
                }

                line = treader.ReadLine();
            }

            treader.Dispose();

            this.Fill();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word">Прилагательное</param>
        /// <param name="defaultGender">Пол, в котором указано прилагательное, используется при поиске неточных совпадений</param>
        /// <returns></returns>
        public CyrAdjective Get(string word, GendersEnum defaultGender = GendersEnum.Masculine)
        {
            return this.Get(word, GetConditionsEnum.Strict, defaultGender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word">Прилагательное</param>
        /// <param name="condition">Вариант поиска в словаре</param>
        /// <param name="defaultGender">Пол, в котором указано прилагательное, используется при поиске неточных совпадений</param>
        /// <returns></returns>
        public CyrAdjective Get(string word, GetConditionsEnum condition, GendersEnum defaultGender = GendersEnum.Masculine)
        {
            GendersEnum gender = GendersEnum.Masculine;
            string t = word;
            string details = this.GetStrictDetails(ref t, ref gender);

            if (details.IsNullOrEmpty() && condition == GetConditionsEnum.Similar)
            {
                details = this.GetSimilarDetails(word, defaultGender, ref gender, out t);
            }

            if (details.IsNullOrEmpty())
            {
                throw new CyrWordNotFoundException(word);
            }

            int ruleID = int.Parse(details);
            string[] parts = this.rules[ruleID].Split(',');

            CyrRule[] rules = parts.Select(val => new CyrRule(val)).ToArray();

            if (gender == GendersEnum.Feminine)
            {
                word = rules[22].Apply(word);
            }
            else if(gender == GendersEnum.Neuter)
            {
                word = rules[23].Apply(word);
            }

            CyrAdjective adj = new CyrAdjective(word, t, gender, rules);

            return adj;
        }

        public IEnumerable<string> SelectMasculineWords()
        {
            return this.masculineWords.Select(x => x.Key);
        }

        public IEnumerable<string> SelectFeminineWords()
        {
            return this.feminineWords.Select(x => x.Key);
        }

        public IEnumerable<string> SelectNeuterWords()
        {
            return this.neuterWords.Select(x => x.Key);
        }

        protected string GetStrictDetails(ref string word, ref GendersEnum gender)
        {
            string details = this.GetDictionaryItem(word, this.masculineWords);

            if (details.IsNullOrEmpty())
            {
                KeyValuePair<string, string> f = this.GetDictionaryItem(word, this.feminineWords);

                if (f.Key.IsNotNullOrEmpty())
                {
                    word = f.Key;
                    details = f.Value;
                    gender = GendersEnum.Feminine;
                }
            }

            if (details.IsNullOrEmpty())
            {
                KeyValuePair<string, string> f = this.GetDictionaryItem(word, this.neuterWords);

                if (f.Key.IsNotNullOrEmpty())
                {
                    word = f.Key;
                    details = f.Value;
                    gender = GendersEnum.Neuter;
                }
            }

            return details;
        }

        protected string GetSimilarDetails(string word, GendersEnum defaultGender, ref GendersEnum gender, out string foundWord)
        {
            string similar = word;
            KeyValuePair<string, string> v;

            switch (defaultGender)
            {
                case GendersEnum.Feminine:
                    v = this.GetSimilarDetails(word, this.feminineWords, out foundWord);
                    similar = v.Key;
                    gender = GendersEnum.Feminine;
                    break;
                case GendersEnum.Neuter:
                    v = this.GetSimilarDetails(word, this.neuterWords, out foundWord);
                    similar = v.Key;
                    gender = GendersEnum.Neuter;
                    break;
            }

            string details = this.GetSimilarDetails(similar, this.masculineWords, out foundWord);

            if (details.IsNullOrEmpty() && defaultGender == 0)
            {
                v = this.GetSimilarDetails(word, this.feminineWords, out foundWord);
                similar = v.Key;
                gender = GendersEnum.Feminine;
                details = this.GetSimilarDetails(similar, this.masculineWords, out foundWord);
            }

            if (details.IsNullOrEmpty() && defaultGender == 0)
            {
                v = this.GetSimilarDetails(word, this.neuterWords, out foundWord);
                similar = v.Key;
                gender = GendersEnum.Neuter;
                details = this.GetSimilarDetails(similar, this.masculineWords, out foundWord);
            }

            return details;
        }

        protected T GetDictionaryItem<T>(string key, Dictionary<string, T> items)
        {
            string t = key;
            T details = this.GetDetails(t, items);

            if (details == null)
            {
                t = key.ToLower();
                details = this.GetDetails(t, items);
            }

            if (details == null)
            {
                t = key.ToLower().UppercaseFirst();
                details = this.GetDetails(t, items);
            }

            if (details == null)
            {
                List<int> indexes = new List<int>();
                string lower = key.ToLower();

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
                    details = this.GetDetails(t, items);

                    if (details != null)
                    {
                        break;
                    }
                }
            }

            return details;
        }

        protected T GetSimilarDetails<T>(string word, Dictionary<string, T> collection, out string collectionWord)
        {
            CyrData data = new CyrData();

            collectionWord = data.GetSimilar(word, collection.Keys.ToList(), this.AdjectiveMinSameLetters);

            if (collectionWord.IsNullOrEmpty())
            {
                return default(T);
            }

            return this.GetDetails(collectionWord, collection);
        }

        protected T GetDetails<T>(string word, Dictionary<string, T> collection)
        {
            if (collection.ContainsKey(word))
            {
                return collection[word];
            }

            return default(T);
        }

        protected void Fill()
        {
            foreach (KeyValuePair<string, string> item in this.masculineWords)
            {
                string rules = this.rules[int.Parse(item.Value)];
                string[] parts = rules.Split(',');
                CyrRule rule = new CyrRule(parts[5]);
                string w = rule.Apply(item.Key);

                if (!this.feminineWords.ContainsKey(w))
                {
                    this.feminineWords.Add(w, item);
                }

                rule = new CyrRule(parts[11]);
                w = rule.Apply(item.Key);

                if (!this.neuterWords.ContainsKey(w))
                {
                    this.neuterWords.Add(w, item);
                }
            }
        }
    }
}
