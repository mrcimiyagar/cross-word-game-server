using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossWordGameServer.Models
{
    public class Tournament
    {
        public bool active { get; set; }
        public int totalDays { get; set; }
        public int leftDays { get; set; }
        public int playersCount { get; set; }
    }
}