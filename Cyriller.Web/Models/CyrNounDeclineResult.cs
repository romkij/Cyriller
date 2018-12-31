using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cyriller.Model;

namespace Cyriller.Web.Models
{
    public class CyrNounDeclineResult
    {
        public string Name { get; set; }
        public string OriginalWord { get; set; }
        public string FoundWord { get; set; }
        public CasesEnum FoundCase { get; set; }
        public NumbersEnum FoundNumber { get; set; }
        public CyrResult Singular { get; set; }
        public CyrResult Plural { get; set; }
        public GendersEnum Gender { get; set; }
        public WordTypesEnum WordType { get; set; }
        public bool IsAnimated { get; set; }

        public bool ExactMatch => this.OriginalWord == this.FoundWord;

        public string GenderStringRu
        {
            get
            {
                switch (this.Gender)
                {
                    case GendersEnum.Feminine: return "женский род";
                    case GendersEnum.Neuter: return "средний род";
                    case GendersEnum.Masculine: return "мужской род";
                    default: return string.Empty;
                }
            }
        }

        public string AnimatedStringRu
        {
            get
            {
                return this.IsAnimated ? "одушевленное" : "неодушевленное";
            }
        }

        public string WordTypeStringRu
        {
            get
            {
                switch (this.WordType)
                {
                    case WordTypesEnum.Abbreviation: return "аббревиатура";
                    case WordTypesEnum.Name: return "имя";
                    case WordTypesEnum.Surname: return "фамилия";
                    case WordTypesEnum.Patronymic: return "отчество";
                    case WordTypesEnum.Toponym: return "топоним";
                    case WordTypesEnum.Organization: return "организация";
                    default: return string.Empty;
                }
            }
        }

        public string FoundCaseStringRu
        {
            get
            {
                switch (this.FoundCase)
                {
                    case CasesEnum.Nominative: return "именительный падеж";
                    case CasesEnum.Genitive: return "родительный падеж";
                    case CasesEnum.Dative: return "дательный падеж";
                    case CasesEnum.Accusative: return "винительный падеж";
                    case CasesEnum.Instrumental: return "творительный падеж";
                    case CasesEnum.Prepositional: return "предложный падеж";
                    default: return string.Empty;
                }
            }
        }

        public string NumberStringRu
        {
            get
            {
                switch (this.FoundNumber)
                {
                    case NumbersEnum.Singular: return "единственное число";
                    case NumbersEnum.Plural: return "множественное число";
                    default: return string.Empty;
                }
            }
        }
    }
}