using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossWordGameServerProject.Models
{
    public class TourPlayer
    {
        public int id { get; set; }
        [JsonIgnore]
        public string passkey { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public string levelsDone { get; set; }
        public int levelsDoneCount { get; set; }
    }
}