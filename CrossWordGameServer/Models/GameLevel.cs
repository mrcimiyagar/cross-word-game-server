using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossWordGameServer.Models
{
    public class GameLevel
    {
        public int id { get; set; }
        public int number { get; set; }
        public int prize { get; set; }
        public string tableData { get; set; }
        public string questionData { get; set; }
        public string answerData { get; set; }
    }
}