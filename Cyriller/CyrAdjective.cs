using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cyriller.Model;

namespace Cyriller
{
    public class CyrAdjective
    {
        /// <summary>
        /// Прилагательное мужского рода в именительном падеже.
        /// </summary>
        public string Name { get; protected set; }

        protected CyrRule[] rules;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Прилагательное мужского рода в именительном падеже.</param>
        /// <param name="rules">Правила склонения.</param>
        public CyrAdjective(string name, CyrRule[] rules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (rules.Length != 24)
            {
                throw new ArgumentException(nameof(rules), "Adjective rules collection must have exactly 22 elements.");
            }

            this.Name = name;
            this.rules = rules;
        }

        public CyrAdjective(CyrAdjective source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Name = source.Name;
            this.rules = source.rules;
        }

        public CyrResult Decline(GendersEnum gender, AnimatesEnum animate)
        {
            CyrResult result;

            if (gender == GendersEnum.Feminine)
            {
                result = new CyrResult(this.rules[5].Apply(this.Name),
                    this.rules[6].Apply(this.Name),
                    this.rules[7].Apply(this.Name),
                    this.rules[8].Apply(this.Name),
                    this.rules[9].Apply(this.Name),
                    this.rules[10].Apply(this.Name));
            }
            else if (gender == GendersEnum.Neuter)
            {
                result = new CyrResult(this.rules[11].Apply(this.Name),
                    this.rules[12].Apply(this.Name),
                    this.rules[13].Apply(this.Name),
                    this.rules[14].Apply(this.Name),
                    this.rules[15].Apply(this.Name),
                    this.rules[16].Apply(this.Name));
            }
            else
            {
                result = new CyrResult(this.Name,
                    this.rules[0].Apply(this.Name),
                    this.rules[1].Apply(this.Name),
                    animate == AnimatesEnum.Animated ? this.rules[2].Apply(this.Name) : this.Name,
                    this.rules[3].Apply(this.Name),
                    this.rules[4].Apply(this.Name));
            }

            return result;
        }

        public CyrResult DeclinePlural(AnimatesEnum animate)
        {
            CyrResult result = new CyrResult(this.rules[17].Apply(this.Name),
                this.rules[18].Apply(this.Name),
                this.rules[19].Apply(this.Name),
                animate == AnimatesEnum.Animated ? this.rules[21].Apply(this.Name) : this.rules[17].Apply(this.Name),
                this.rules[20].Apply(this.Name),
                this.rules[21].Apply(this.Name));

            return result;
        }

        public void SetName(string name, GendersEnum gender, CasesEnum @case, NumbersEnum number, AnimatesEnum animate)
        {
            CyrRule[] rules = this.GetRules(gender, number, animate);
            CyrRule rule = rules[(int)@case - 1];

            this.Name = rule.Revert(this.Name, name);
        }

        protected virtual CyrRule[] GetRules(GendersEnum gender, NumbersEnum number, AnimatesEnum animate)
        {
            CyrRule[] rules;

            if (gender == GendersEnum.Feminine)
            {
                rules = new CyrRule[]
                {
                    this.rules[5],
                    this.rules[6],
                    this.rules[7],
                    this.rules[8],
                    this.rules[9],
                    this.rules[10]
                };
            }
            else if (gender == GendersEnum.Neuter)
            {
                rules = new CyrRule[]
                {
                    this.rules[11],
                    this.rules[12],
                    this.rules[13],
                    this.rules[14],
                    this.rules[15],
                    this.rules[16]
                };
            }
            else if (gender == GendersEnum.Masculine)
            {
                rules = new CyrRule[]
                {
                    new CyrRule(string.Empty),
                    this.rules[0],
                    this.rules[1],
                    animate == AnimatesEnum.Animated ? this.rules[2] : new CyrRule(string.Empty),
                    this.rules[3],
                    this.rules[4]
                };
            }
            else if (number == NumbersEnum.Plural)
            {
                rules = new CyrRule[]
                {
                    this.rules[17],
                    this.rules[18],
                    this.rules[19],
                    animate == AnimatesEnum.Animated ? this.rules[21] : this.rules[17],
                    this.rules[20],
                    this.rules[21]
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return rules;
        }
    }
}
