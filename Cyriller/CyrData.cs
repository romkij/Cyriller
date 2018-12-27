using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;

namespace Cyriller
{
    public class CyrData
    {
        /// <summary>
        /// Вес совпадающих символов с конца слова. 
        /// Большее значение указывает на более полное совпадение.
        /// </summary>
        public int[] SameLetterWeights { get; protected set; } = { 1000, 100, 10 };

        /// <summary>
        /// Лист букв ['е', 'ё', 'Е', 'Ё'] для поиска совпадений в коллекции.
        /// <see cref="GetSimilar(string, IEnumerable{string}, int)"/>.
        /// </summary>
        public char[] SameCharsE { get; protected set; } = new char[] { 'е', 'ё', 'Е', 'Ё' };

        public CyrData()
        {
        }

        /// <summary>
        /// Возвращает открытый <see cref="TextReader"/> для чтения текстовых и сжатых ресурсов из папки "App_Data".
        /// Ресурс должен быть сжат при помощи <see cref="GZipStream"/> и встроен в сборку, как "Embedded resource".
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns></returns>
        public TextReader GetData(string fileName)
        {
            Stream stream = typeof(CyrData).Assembly.GetManifestResourceStream("Cyriller.App_Data." + fileName);
            GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress);
            TextReader treader = new StreamReader(gzip);

            return treader;
        }

        /// <summary>
        /// Ищет наиболее подходящее совпадение в коллекции для указанного слова.
        /// Сравнение символов начинается с конца слова, чем ближе символ к началу слова, тем меньше веса совпадение имеет.
        /// Пример: пара [краснЫй и краснОй] имеет меньший вес совпадения, чем пара [КРАСный и УСТный].
        /// </summary>
        /// <param name="word">Слово для поиска в коллекции.</param>
        /// <param name="collection">Коллекция, в которой искать совпадения.</param>
        /// <param name="minSameLetters">Минимальное кол-во совпадающих символов с конца слова.</param>
        /// <param name="maxSameLetters">
        /// Максимальное кол-во совпадающих символов с конца слова. 
        /// Поиск будет остановлен, если слово с данным кол-вом совпадающих символов будет найдено.
        /// Можно использовать <see cref="int.MaxValue"/> для поиска по всей коллекции.
        /// </param>
        public string GetSimilar(string word, IEnumerable<string> collection, int minSameLetters, int maxSameLetters)
        {
            if (word == null || word.Length < minSameLetters)
            {
                return word;
            }

            string foundWord = null;
            List<SimilarCandidate> candidates = new List<SimilarCandidate>();

            foreach (string str in collection)
            {
                if (str == word)
                {
                    foundWord = str;
                    break;
                }

                int maxPosition = Math.Min(maxSameLetters, Math.Min(word.Length, str.Length));
                int weight = 0;
                int sameLetters = 0;

                for (int i = 1; i <= maxPosition; i++)
                {
                    if (this.Compare(str[str.Length - i], word[word.Length - i]))
                    {
                        int wi = i - 1;
                        weight += wi < SameLetterWeights.Length ? SameLetterWeights[wi] : 1;
                        sameLetters++;
                    }
                    else if (i <= minSameLetters)
                    {
                        continue;
                    }

                    if (sameLetters >= maxSameLetters)
                    {
                        foundWord = str;
                        break;
                    }
                }

                SimilarCandidate c = new SimilarCandidate()
                {
                    Name = str,
                    Weight = weight
                };

                candidates.Add(c);
            }

            if (!string.IsNullOrEmpty(foundWord))
            {
                return foundWord;
            }

            SimilarCandidate candidate = null;

            foreach (SimilarCandidate c in candidates)
            {
                if (candidate == null
                    || c.Weight > candidate.Weight
                    || (c.Weight == candidate.Weight && c.Name.Length < candidate.Name.Length)
                    || (c.Weight == candidate.Weight && c.Name.Length == candidate.Name.Length && c.Name.CompareTo(candidate.Name) < 0))
                {
                    candidate = c;
                }
            }

            return candidate?.Name;
        }

        /// <summary>
        /// Сравнивает два символа между собой. 
        /// Буквы "е" и "ё" воспринимаются, как равнозначные символы.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual bool Compare(char a, char b)
        {
            if (a == b)
            {
                return true;
            }

            return SameCharsE.Contains(a) && SameCharsE.Contains(b);
        }

        internal class SimilarCandidate
        {
            /// <summary>
            /// Найденное слово в коллекции.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Вес совпадения между найденным и искомым словом. 
            /// Большее значение указывает на более полное совпадение.
            /// <see cref="SameLetterWeights"/>.
            /// </summary>
            public int Weight { get; set; }
        }
    }
}
