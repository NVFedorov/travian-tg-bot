using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTB.DAL.Models
{
    public class CommandModel
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string KeyGroup { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public bool IsRunning { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
