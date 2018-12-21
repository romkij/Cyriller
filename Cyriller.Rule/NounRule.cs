using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cyriller.Model.Json;

namespace Cyriller.Rule
{
    public class NounRule
    {
        public const string Hyphen = "-";
        public const string Separator = "|";
        public string Value { get; protected set; }

        public NounRule(NounJson source)
        {
            this.ValidateSource(source);

            string noun = source.Name;
            string[] parts = noun.Split(Hyphen.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> rules = new List<string>();
            string[] variants = new string[]
            {
                source.Singular[1],
                source.Singular[2],
                source.Singular[3],
                source.Singular[4],
                source.Singular[5],
                source.Plural[0],
                source.Plural[1],
                source.Plural[2],
                source.Plural[3],
                source.Plural[4],
                source.Plural[5]
            };
            string[][] variantParts = variants.Select(x => x.Split(Hyphen.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                string[] variant = variantParts.Select(x => x.Length > i ? x[i] : null).ToArray();

                rules.Add(this.GetNounRule(part, variant));
            }

            this.Value = string.Join(Separator, rules.ToArray());
        }

        protected virtual void ValidateSource(NounJson source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Validate();
        }

        protected virtual string GetNounRule(string noun, string[] variants)
        {
            List<string> rules = new List<string>();

            foreach (string variant in variants)
            {
                if (string.IsNullOrEmpty(variant))
                {
                    rules.Add("*");
                    continue;
                }

                int index = 0;
                StringBuilder sb = new StringBuilder();

                for (index = 0; index < noun.Length && index < variant.Length; index++)
                {
                    if (noun[index] != variant[index])
                    {
                        break;
                    }
                }

                string end = variant.Substring(index);
                int cut = noun.Length - index;

                sb.Append(end);

                if (cut > 0)
                {
                    sb.Append(cut);
                }

                rules.Add(sb.ToString());
            }

            return string.Join(",", rules.ToArray());
        }
    }
}
