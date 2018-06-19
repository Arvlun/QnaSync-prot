using Newtonsoft.Json;
using qnaSyncGUI.Helpers.QnASync;
using qnaSyncGUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace qnaSyncGUI.Helpers
{
    class QnAConstructionService
    {
        public static List<QnAItem> ParseDocumentForQnAPairs()
        {
            var qnaItems = new List<QnAItem>();

            string basePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            //string path = basePath + "/qnapairs.json";

            using (StreamReader r = new StreamReader("qnapairs.json"))
            {
                string json = r.ReadToEnd();
                qnaItems = JsonConvert.DeserializeObject<List<QnAItem>>(json);
            }

            return qnaItems;
        }

        //public static List<QnAItem> ParseXMLForQnAPairs()
        //{
        //    var qnaItems = new List<QnAItem>();

        //    XmlSerializer serializer = new XmlSerializer(typeof(QnAItem[]));

        //    using (StreamReader r = new StreamReader("qnapairtest.json"))
        //    {
        //        r.ReadToEnd();
        //        var qnaArr = (QnAItem[])serializer.Deserialize(r);
        //        qnaItems = qnaArr.ToList();
        //    }

        //    return qnaItems;
        //}
    }
}

