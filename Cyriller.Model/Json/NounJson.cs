using System;
using System.Collections.Generic;
using System.Text;

namespace Cyriller.Model.Json
{
    public class NounJson
    {
        public AnimatesEnum Animate { get; set; }
        public GendersEnum Gender { get; set; }
        public string Name { get; set; }
        public WordTypesEnum WordType { get; set; }
        public string[] Singular { get; set; }
        public string[] Plural { get; set; }

        /// <summary>
        /// Converts current noun into a string using the Cyriller dictionary format.
        /// See /Cyriller/App_Data/nouns.txt.
        /// Example: абажур 1,2,0,8.
        /// Explanation: [Name] [Gender],[Animate],[WordType],[DeclencionRuleIndex].
        /// </summary>
        /// <param name="ruleIndex">Declencion rule index used to decline this noun.</param>
        /// <returns></returns>
        public string ToDictionaryString(int ruleIndex)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Name)
                .Append(" ")
                .Append((int)this.Gender)
                .Append(",")
                .Append((int)this.Animate)
                .Append(",")
                .Append((int)this.WordType)
                .Append(",")
                .Append(ruleIndex);

            return sb.ToString();
        }

        /// <summary>
        /// Validates current state of the object.
        /// Throws <see cref="ArgumentNullException"/> if <see cref="Name"/> is null or empty.
        /// Throws <see cref="ArgumentNullException"/> if <see cref="Singular"/> or <see cref="Plural"/> arrays are null.
        /// Throws <see cref="ArgumentException"/> if <see cref="Singular"/> or <see cref="Plural"/> arrays do not have exactly six elements.
        /// </summary>
        public void Validate()
        {
            if (this.Singular == null)
            {
                throw new ArgumentNullException(nameof(NounJson.Singular), $"Noun is missing required {nameof(NounJson.Singular)} value.");
            }

            if (this.Plural == null)
            {
                throw new ArgumentNullException(nameof(NounJson.Plural), $"Noun is missing required {nameof(NounJson.Plural)} value.");
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                throw new ArgumentNullException(nameof(NounJson.Name), $"Noun is missing required {nameof(NounJson.Name)} value.");
            }

            if (this.Singular.Length != 6)
            {
                throw new ArgumentException(nameof(NounJson.Singular), $"Noun {nameof(NounJson.Singular)} has invalid number of values. There should be 6 values.");
            }

            if (this.Plural.Length != 6)
            {
                throw new ArgumentException(nameof(NounJson.Plural), $"Noun {nameof(NounJson.Plural)} has invalid number of values. There should be 6 values.");
            }
        }
    }
}
