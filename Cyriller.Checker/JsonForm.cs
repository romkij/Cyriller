using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Cyriller.Checker.Model;

namespace Cyriller.Checker
{
    public partial class JsonForm : UserControl
    {
        public JsonForm()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (fbdDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = fbdDialog.SelectedPath;
            }
        }

        private void WriteToFile(string content, string filePath)
        {
            FileInfo fi = new FileInfo(filePath);

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            else if (fi.Exists)
            {
                fi.Delete();
            }

            TextWriter writer = new StreamWriter(fi.FullName, true, Encoding.UTF8);

            writer.Write(content);
            writer.Dispose();
        }

        private void btnNouns_Click(object sender, EventArgs e)
        {
            CyrNounCollection collection = new CyrNounCollection();
            ConcurrentBag<Dictionary<string, object>> words = new ConcurrentBag<Dictionary<string, object>>();
            string filePath = Path.Combine(txtFolder.Text, "nouns.json");

            collection.SelectWords().AsParallel().ForAll(x =>
            {
                CyrNoun noun = collection.Get(x);
                CyrResult singular = noun.Decline();
                CyrResult plural = noun.DeclinePlural();
                Dictionary<string, object> result = new Dictionary<string, object>();

                result.Add(nameof(CyrNoun.Animate), noun.Animate.ToString());
                result.Add(nameof(CyrNoun.Gender), noun.Gender.ToString());
                result.Add(nameof(CyrNoun.Name), noun.Name);
                result.Add(nameof(CyrNoun.WordType), noun.WordType.ToString());

                result.Add("Singular", singular.ToArray());
                result.Add("Plural", plural.ToArray());

                words.Add(result);
            });

            string json = JsonConvert.SerializeObject(words.OrderBy(x => x[nameof(CyrNoun.Name)]), Formatting.Indented);

            this.WriteToFile(json, filePath);
        }

        private void btnAdjectives_Click(object sender, EventArgs e)
        {
            CyrAdjectiveCollection collection = new CyrAdjectiveCollection();
            ConcurrentBag<Dictionary<string, object>> words = new ConcurrentBag<Dictionary<string, object>>();
            string filePath = Path.Combine(txtFolder.Text, "adjectives.json");

            collection.SelectNeuterWords().AsParallel().ForAll(x =>
            {
                CyrAdjective adjective = collection.Get(x, Cyriller.Model.GendersEnum.Neuter);
                CyrResult animatedSingular = adjective.Decline(Cyriller.Model.AnimatesEnum.Animated);
                CyrResult animatedPlural = adjective.DeclinePlural(Cyriller.Model.AnimatesEnum.Animated);
                CyrResult inanimatedSingular = adjective.Decline(Cyriller.Model.AnimatesEnum.Inanimated);
                CyrResult inanimatedPlural = adjective.DeclinePlural(Cyriller.Model.AnimatesEnum.Inanimated);
                Dictionary<string, object> result = new Dictionary<string, object>();

                result.Add(nameof(CyrAdjective.Gender), adjective.Gender.ToString());
                result.Add(nameof(CyrAdjective.Name), adjective.Name);

                result.Add(nameof(Cyriller.Model.AnimatesEnum.Animated), new
                {
                    Singular = animatedSingular.ToArray(),
                    Plural = animatedPlural.ToArray()
                });

                result.Add(nameof(Cyriller.Model.AnimatesEnum.Inanimated), new
                {
                    Singular = inanimatedSingular.ToArray(),
                    Plural = inanimatedPlural.ToArray()
                });

                words.Add(result);
            });

            string json = JsonConvert.SerializeObject(words.OrderBy(x => x[nameof(CyrAdjective.Name)]), Formatting.Indented);

            this.WriteToFile(json, filePath);
        }
    }
}
