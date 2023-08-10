using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiSelectCombo
{
    public class DataSource
    {
        private readonly string[] _strings = new[] {
            "string 1",
            "string 2",
            "string 3",
        };

        public IEnumerable<string> Strings => _strings;

        private readonly IEnumerable<Data> _data = new[] {
            new Data { Name = "First", Age = 1 },
            new Data { Name = "Second", Age = 2 },
            new Data { Name = "Third", Age = 3 },
        };
        public IEnumerable<Data> Data => _data;
    }

    public class Data
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
