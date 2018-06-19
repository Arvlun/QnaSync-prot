using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using qnaSyncGUI.Helpers.QnASync.Models;
using qnaSyncGUI.Models;

namespace qnaSyncGUI.Helpers
{
    public class QnASyncService
    {
        public string KnowledgeBaseId { get; set; }
        public string SubscriptionKey { get; set; }
        public string KnowledgeBaseName { get; set; }
        public string QnaMakerEndpoint { get; set; }

        public QnASyncService(string knowledgeBaseId, string subscriptionKey, string knowledgeBaseName, string qnaMakerEndpoint = "https://westus.api.cognitive.microsoft.com/qnamaker/v4.0")
        {
            KnowledgeBaseId = knowledgeBaseId;
            SubscriptionKey = subscriptionKey;
            KnowledgeBaseName = knowledgeBaseName;
            QnaMakerEndpoint = qnaMakerEndpoint;
        }

        public async Task UpdateKnowlegdeBase(List<QnAItem> qnaItems)
        {
            var currentKnowledgeBase = GetCurrentKnowledgeBase();
            var qnaMakerUpdateModel = GenerateUpdateModel(qnaItems, currentKnowledgeBase);
            if (qnaMakerUpdateModel != null)
            {
                await UpdateKnowledgeBase(qnaMakerUpdateModel);
            }
        }

        public async Task UpdateKnowlegdeBaseSingleItem(List<QnAItem> qnaItems, bool resetQuestions)
        {
            var currentKnowledgeBase = GetCurrentKnowledgeBase();
            var qnaMakerUpdateModel = GenerateUpdateModelSingleItem(qnaItems, currentKnowledgeBase, resetQuestions);
            if (qnaMakerUpdateModel != null)
            {
                await UpdateKnowledgeBase(qnaMakerUpdateModel);
            }
        }

        public bool CheckIfUpdatedQuestion(QnAItem item)
        {
            bool updatedQ = false;
            var currentKnowledgeBase = GetCurrentKnowledgeBase();
            int matchingQuestions = 0;

            foreach (var kbItem in currentKnowledgeBase.qnaList.ToList())
            {
                var kbItemIdMetaDataItem = kbItem.metadata.FirstOrDefault(m => m.name == "itemid");

                if (kbItemIdMetaDataItem != null)
                {
                    // id convert
                    if (kbItemIdMetaDataItem.value == item.ItemId.ToString())
                    {
                        foreach(string question in item.Questions)
                        {
                            if (kbItem.questions.Contains(question, StringComparer.OrdinalIgnoreCase))
                            {
                                matchingQuestions++;
                            }
                        }
                        break;
                    } 
                }
            }

            if (matchingQuestions != item.Questions.Count)
            {
                updatedQ = true;
            }

            return updatedQ;
        }

        public async Task PublishKnowledgeBase()
        {
            var uri = $"{QnaMakerEndpoint}/knowledgebases/{KnowledgeBaseId}";
            var method = new HttpMethod("POST");
            var request = new HttpRequestMessage(method, uri);

            Console.WriteLine(request);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            var result = await client.SendAsync(request);
            //Console.WriteLine(result);
        }

        private async Task UpdateKnowledgeBase(QnAMakerUpdateModel qnaMakerUpdateModel)
        {
            var uri = $"{QnaMakerEndpoint}/knowledgebases/{KnowledgeBaseId}";
            var content = new StringContent(JsonConvert.SerializeObject(qnaMakerUpdateModel), Encoding.UTF8, "application/json");
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, uri)
            {
                Content = content
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await client.SendAsync(request);
            int test = 0;
        }

        private QnAMakerKnowledgeBaseModel GetCurrentKnowledgeBase()
        {
            var uri = QnaMakerEndpoint;
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync($"{uri}/knowledgebases/{KnowledgeBaseId}/Test/qna").Result;

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }


            var result = response.Content.ReadAsStringAsync().Result;

            var knowledgeBase = JsonConvert.DeserializeObject<QnAMakerKnowledgeBaseModel>(result);
            return knowledgeBase;
        }

        private QnAMakerUpdateModel GenerateUpdateModel(List<QnAItem> qnaItemsToSync, QnAMakerKnowledgeBaseModel currentKnowledgeBase)
        {
            var qnaMakerUpdateModel = new QnAMakerUpdateModel
            {
                add = new ItemsToAdd(),
                update = new ItemsToUpdate(),
                delete = new ItemsToDelete()
            };

            var currentQnaItemIdsInKb = CurrentQnaItemIdsInKb(currentKnowledgeBase);
            //  id convert
            var qnaItemsToAdd = qnaItemsToSync.Where(f => !currentQnaItemIdsInKb.Contains(f.ItemId.ToString())).ToList();
            qnaMakerUpdateModel.add = GenerateItemsToAddModel(qnaItemsToAdd);

            qnaMakerUpdateModel.delete = GenerateItemsToDeleteModel(currentKnowledgeBase.qnaList.ToList(), qnaItemsToSync);

            qnaMakerUpdateModel.update = GenerateItemsToUpdateModel(currentKnowledgeBase.qnaList.ToList(), qnaItemsToSync);

            return qnaMakerUpdateModel;
        }

        private QnAMakerUpdateModel GenerateUpdateModelSingleItem(List<QnAItem> qnaItemsToSync, QnAMakerKnowledgeBaseModel currentKnowledgeBase, bool resetQuestions)
        {
            var qnaMakerUpdateModel = new QnAMakerUpdateModel
            {
                add = new ItemsToAdd(),
                update = new ItemsToUpdate(),
                delete = new ItemsToDelete()
            };

            //var currentQnaItemIdsInKb = CurrentQnaItemIdsInKb(currentKnowledgeBase);
            //  id convert
            //var qnaItemsToAdd = qnaItemsToSync.Where(f => !currentQnaItemIdsInKb.Contains(f.ItemId.ToString())).ToList();
            //qnaMakerUpdateModel.add = GenerateItemsToAddModel(qnaItemsToAdd);

            //qnaMakerUpdateModel.delete = GenerateItemsToDeleteModel(currentKnowledgeBase.qnaList.ToList(), qnaItemsToSync);

            qnaMakerUpdateModel.update = GenerateItemsToUpdateModel(currentKnowledgeBase.qnaList.ToList(), qnaItemsToSync, resetQuestions);

            return qnaMakerUpdateModel;
        }

        private static List<string> CurrentQnaItemIdsInKb(QnAMakerKnowledgeBaseModel currentKnowledgeBase)
        {
            var currentQnaItemIdsInKb = new List<string>();

            foreach (var kbItem in currentKnowledgeBase.qnaList)
            {
                var kbItemIdMetaItem = kbItem.metadata.FirstOrDefault(m => m.name == "itemid");
                if (kbItemIdMetaItem != null)
                {
                    currentQnaItemIdsInKb.Add(kbItemIdMetaItem.value);
                }
            }
            return currentQnaItemIdsInKb;
        }

        private static ItemsToDelete GenerateItemsToDeleteModel(IEnumerable<KbItem> currentKnowledgeBaseItems, List<QnAItem> qnaItems)
        {
            var kbIdsToDelete = new List<int>();

            foreach (var kbItem in currentKnowledgeBaseItems)
            {
                var kbItemIdMetaDataItem = kbItem.metadata.FirstOrDefault(m => m.name == "itemid");

                if (kbItemIdMetaDataItem != null)
                {
                    //id convert
                    var qnaItem = qnaItems.FirstOrDefault(f => f.ItemId.ToString() == kbItemIdMetaDataItem.value);

                    if (qnaItem == null)
                    {
                        kbIdsToDelete.Add(kbItem.qnaId);
                    }
                }
                else
                {
                    kbIdsToDelete.Add(kbItem.qnaId);
                }
            }

            var itemsToDeleteModel = new ItemsToDelete()
            {
                qnaIds = kbIdsToDelete.ToArray(),
                sources = new string[] { },
                users = new object[] { }
            };

            return itemsToDeleteModel;
        }

        private ItemsToUpdate GenerateItemsToUpdateModel(IEnumerable<KbItem> currentKnowledgeBaseItems, List<QnAItem> qnaItems, bool resetQuestion = false)
        {

            var itemsToUpdateModel = new ItemsToUpdate
            {
                name = KnowledgeBaseName,
                urls = new string[] { }
            };

            var kbItemsToUpdate = new List<KbItemToUpdate>();

            foreach (var kbItem in currentKnowledgeBaseItems)
            {
                var kbItemIdMetaDataItem = kbItem.metadata.FirstOrDefault(m => m.name == "itemid");

                if (kbItemIdMetaDataItem != null)
                {
                    // id convert
                    var qnaItem = qnaItems.FirstOrDefault(f => f.ItemId.ToString() == kbItemIdMetaDataItem.value);

                    if (qnaItem != null)
                    {
                        var updatedKbItem = new KbItemToUpdate
                        {
                            qnaId = kbItem.qnaId,
                            answer = qnaItem.Answer,
                            questions = new QuestionsUpdateModel()
                        };


                        if (resetQuestion)
                        {
                            var questionsToAdd = qnaItem.Questions.ToList().Where(q => !kbItem.questions.ToList().Contains(q));
                            var questionsToDelete = kbItem.questions.ToList().Where(q => !questionsToAdd.Contains(q)); // Will delete questions that aren't in the documentation, resets qna-pairing..
                            updatedKbItem.questions.delete = questionsToDelete.ToArray();
                            updatedKbItem.questions.add = questionsToAdd.ToArray();
                        }
                        else
                        {
                            var questionsToAdd = qnaItem.Questions.ToList();
                            updatedKbItem.questions.add = questionsToAdd.ToArray();
                        }


                        List<MetadataItem> metaDataItemsToDelete = new List<MetadataItem>();
                        metaDataItemsToDelete = kbItem.metadata
                                .Where(m => !qnaItem.Metadata.Select(f => f.name).Contains(m.name)).ToList();


                        var metaDataItemsToAddOrUpdate = new List<MetadataItem>
                   {
                       new MetadataItem()
                       {
                           name = "itemid",
                           value = qnaItem.ItemId.ToString()
                       },
                       new MetadataItem()
                       {
                           name = "isactive",
                           value = qnaItem.IsActive.ToString()
                       }
                   };

                        foreach (var metadataItem in qnaItem.Metadata)
                        {
                            metaDataItemsToAddOrUpdate.Add(new MetadataItem()
                            {
                                name = metadataItem.name,
                                value = metadataItem.value
                            });
                        }

                        updatedKbItem.metadata = new MetaDataUpdateModel
                        {
                            add = metaDataItemsToAddOrUpdate.ToArray(),
                            delete = metaDataItemsToDelete.ToArray()
                        };

                        kbItemsToUpdate.Add(updatedKbItem);
                    }
                }
            }

            itemsToUpdateModel.qnaList = kbItemsToUpdate.ToArray();
            return itemsToUpdateModel;
        }

        private static ItemsToAdd GenerateItemsToAddModel(List<QnAItem> qnaItemsToAdd)
        {
            var itemsToAddModel = new ItemsToAdd
            {
                qnaList = new KbItemToAdd[] { },
                urls = new string[] { },
                users = new object[] { }
            };

            var kbItemsToAdd = new List<KbItemToAdd>();

            foreach (var qnaItem in qnaItemsToAdd)
            {
                var kbItem = new KbItemToAdd
                {
                    answer = qnaItem.Answer,
                    metadata = new MetadataItem[] { },
                    questions = new string[] { }
                };

                var questions = qnaItem.Questions;
                kbItem.questions = questions.ToArray();

                var metadata = new List<MetadataItem>
                {
                    new MetadataItem()
                    {
                        name = "itemid",
                        value = qnaItem.ItemId.ToString()
                    },
                    new MetadataItem()
                    {
                         name = "isactive",
                         value = qnaItem.IsActive.ToString()
                    }
                };

                foreach (var metaDataItem in qnaItem.Metadata)
                {
                    var metaDataItemToAdd = new MetadataItem
                    {
                        name = metaDataItem.name,
                        value = metaDataItem.value
                    };
                    metadata.Add(metaDataItemToAdd);
                }

                kbItem.metadata = metadata.ToArray();

                kbItemsToAdd.Add(kbItem);
            }

            itemsToAddModel.qnaList = kbItemsToAdd.ToArray();

            return itemsToAddModel;
        }
    }
}
