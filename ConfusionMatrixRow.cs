using System;
using System.Collections.Generic;
using System.Text;

namespace iad_test
{
    class ConfusionMatrixRow
    {
        public string label;
        public Dictionary<string, int> columns { get; set; }
        public int truePostive;
        public int falseNegative;
        public double classPrecision;

        public ConfusionMatrixRow(string label)
        {
            this.classPrecision = 0;
            this.label = label;
            columns = new Dictionary<string, int>();
            columns.Add("usa", 0);
            columns.Add("west-germany", 0);
            columns.Add("france", 0);
            columns.Add("uk", 0);
            columns.Add("canada", 0);
            columns.Add("japan", 0);
        }
    }
}
