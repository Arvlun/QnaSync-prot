using qnaSyncGUI.Models;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class KbItemToAdd
    {
        public string answer { get; set; }
        public string[] questions { get; set; }
        public MetadataItem[] metadata { get; set; }
    }
}