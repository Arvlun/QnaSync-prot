using Newtonsoft.Json;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class QnAMakerKnowledgeBaseModel
    {
        [JsonProperty(PropertyName = "qnaDocuments")]
        public KbItem[] qnaList { get; set; }
    }
}