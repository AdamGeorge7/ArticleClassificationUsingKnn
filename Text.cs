using System;
using System.Collections.Generic;
using System.Text;
using Porter2StemmerStandard;

namespace iad_test
{
    class Text
    {
        public string place;
        public string assignedPlace;
        public string body;
        public string[] splitBody;
        public Dictionary<string, int> wordOccurancy { get; set; }
        public List<int> wordOccurancyInt { get; set; }
        public List<double> wordTFIDFDouble { get; set; }
        public Text(string place, string body)
        {
            this.place = place;
            this.body = body;
            wordOccurancy = new Dictionary<string, int>();
            wordOccurancyInt = new List<int>();
            wordTFIDFDouble = new List<double>();
        }

        public void StemSplitBody()
        {
            char[] separator = { '.', ',', ' ', '\t', '"', '=', '-', '<', '>', ')', '(', ';'};
            //char[] separator = { ' ' };
            EnglishPorter2Stemmer stemmer = new EnglishPorter2Stemmer();
            splitBody = body.Split(separator);
            string pom;
            for (int i = 0; i < splitBody.Length; i++)
            {
                pom = stemmer.Stem(splitBody[i]).Value;
                splitBody[i] = pom;
            }
        }
    }
}
