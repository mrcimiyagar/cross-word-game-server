using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CrossWordGameServer.Models
{
    [DataContract]
    public class TourPlayer
    {
        [DataMember]
        public long id { get; set; }
        public string passkey { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int score { get; set; }
        [DataMember]
        public int rank { get; set; }
    }
}