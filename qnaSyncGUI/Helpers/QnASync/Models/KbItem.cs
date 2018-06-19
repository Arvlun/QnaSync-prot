﻿using Newtonsoft.Json;
using qnaSyncGUI.Models;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class KbItem
    {
        [JsonProperty(PropertyName = "id")]
        public int qnaId { get; set; }
        public string answer { get; set; }
        public string source { get; set; }
        public string[] questions { get; set; }
        public MetadataItem[] metadata { get; set; }
    }
}