using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace qnaSyncGUI.Models
{
    public class QnAItem
    {
        public List<string> Questions { get; set; }

        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Answer { get; set; }

        public int ItemId { get; set; }

        public bool IsActive { get; set; }

        //public Dictionary<string,string> Metadata { get; set; }

        public List<MetadataItem> Metadata { get; set; }

        public QnAItem()
        {
            Questions = new List<string>();
            Metadata = new List<MetadataItem>();
        }
    }
}