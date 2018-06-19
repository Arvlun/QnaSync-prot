using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using qnaSyncGUI.Models;

namespace qnaSyncGUI.Helpers
{
    public class RequestHandler
    {
        //Temp
        public static List<QnAItem> ParseDocumentForQnAPairs(string path)
        {
            var qnaItems = new List<QnAItem>();

            string basePath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            //string path = basePath + "/qnapairs.json";

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                qnaItems = JsonConvert.DeserializeObject<List<QnAItem>>(json);
            }

            //qnaItems.Sort(itemId);
            var sortedList = qnaItems.OrderByDescending(qn => qn.ItemId).ToList();

            return sortedList;
        }


        public static void WriteToJson(List<QnAItem> qnaItems, string path)
        {
            var json = JsonConvert.SerializeObject(qnaItems);
            
            using (StreamWriter w = new StreamWriter(path))
            {
                w.WriteLine(json);
            }

        }

        public static void AddQnaItem(QnAItem qnaItem, string path)
        {
            var qnaItems = ParseDocumentForQnAPairs(path);
            var usedIds = qnaItems.Select(item => item.ItemId);
            qnaItems.Add(qnaItem);

            int? firstAvailable = Enumerable.Range(0, int.MaxValue)
                .Except(usedIds)
                .FirstOrDefault();

            if(firstAvailable != null)
            {
                qnaItem.ItemId = (int)firstAvailable;
                WriteToJson(qnaItems, path);
            }
        }

        public static QnAItem FetchQnaItem(int itemid, string path)
        {
            var qnaItems = ParseDocumentForQnAPairs(path);

            var item = qnaItems.FirstOrDefault(q => q.ItemId == itemid);

            return item;
        }

        public static void DeleteQnaItem(int itemid, string path)
        {
            var qnaItems = ParseDocumentForQnAPairs(path);

            var item = qnaItems.FirstOrDefault(q => q.ItemId == itemid);

            qnaItems.Remove(item);

            WriteToJson(qnaItems, path);
        }

        public static void UpdateQnaItem(QnAItem item, string path)
        {
            var qnaItems = ParseDocumentForQnAPairs(path);

            var itemToUpdate = qnaItems.FirstOrDefault(q => q.ItemId == item.ItemId);

            qnaItems.Remove(itemToUpdate);
            qnaItems.Add(item);
            WriteToJson(qnaItems, path);

        }
    }
}