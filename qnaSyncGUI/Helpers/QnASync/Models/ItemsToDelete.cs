using Newtonsoft.Json;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class ItemsToDelete
    {
        [JsonProperty(PropertyName = "ids")]
        public int[] qnaIds { get; set; }
        public string[] sources { get; set; }
        public object[] users { get; set; }
    }
}