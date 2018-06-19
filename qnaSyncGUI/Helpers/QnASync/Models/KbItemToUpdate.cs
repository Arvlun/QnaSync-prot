using Newtonsoft.Json;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class KbItemToUpdate
    {
        [JsonProperty(PropertyName = "id")]
        public int qnaId { get; set; }
        public string answer { get; set; }
        public string source { get; set; }
        public QuestionsUpdateModel questions { get; set; }
        public MetaDataUpdateModel metadata { get; set; }
    }
}