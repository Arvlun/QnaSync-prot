using qnaSyncGUI.Models;

namespace qnaSyncGUI.Helpers.QnASync.Models
{
    internal class MetaDataUpdateModel
    {
        public MetadataItem[] add { get; set; }
        public MetadataItem[] delete { get; set; }
    }
}