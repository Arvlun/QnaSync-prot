using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using qnaSyncGUI.Helpers;
using qnaSyncGUI.Models;
using qnaSyncGUI.Helpers.QnASync;
using System.Configuration;
using System.Threading.Tasks;

namespace qnaSyncGUI.Controllers
{

    public class HomeController : Controller
    {
        string jsonName = "/qnaCustom.json";
        //string jsonName = "/qnapairs.json";

        public ActionResult Index()
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            var qnaList = RequestHandler.ParseDocumentForQnAPairs(jsonUrl);

            return View(qnaList);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Questions, Answer, IsActive, Metadata")] QnAItem item)
        {
            if (ModelState.IsValid)
            {
                var emptyQuestions = item.Questions.Where(q => q == "" || q == null);
                var cleanEmptyQuestions = item.Questions.Except(emptyQuestions).ToList();
                item.Questions = cleanEmptyQuestions;

                var emptyMetadata = item.Metadata.Where(md => md.name == "" || md.value == "" || md.name == null || md.value == null);
                var clearEmptyMetadata = item.Metadata.Except(emptyMetadata).ToList();
                item.Metadata = clearEmptyMetadata;

                string baseUrl = HttpRuntime.AppDomainAppPath;
                string jsonUrl = baseUrl + jsonName;
                RequestHandler.AddQnaItem(item, jsonUrl);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int itemid)
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            var qnaItem = RequestHandler.FetchQnaItem(itemid, jsonUrl);

            return View(qnaItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Questions, Answer, ItemId, IsActive, Metadata")] QnAItem item)
        {
            if (ModelState.IsValid)
            {
                var emptyQuestions = item.Questions.Where(q => q == "");
                var cleanEmptyQuestions = item.Questions.Except(emptyQuestions).ToList();
                item.Questions = cleanEmptyQuestions;

                var emptyMetadata = item.Metadata.Where(md => md.name == "" || md.value == "" || md.name == null || md.value == null);
                var clearEmptyMetadata = item.Metadata.Except(emptyMetadata).ToList();
                item.Metadata = clearEmptyMetadata;

                string baseUrl = HttpRuntime.AppDomainAppPath;
                string jsonUrl = baseUrl + jsonName;
                RequestHandler.UpdateQnaItem(item, jsonUrl);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int itemid)
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            RequestHandler.DeleteQnaItem(itemid, jsonUrl);

            return RedirectToAction("Index");
        }

        public ActionResult Sync()
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            var qnaList = RequestHandler.ParseDocumentForQnAPairs(jsonUrl);
            //var qnaService = new QnASyncService(ConfigurationManager.AppSettings["QnaMakerKnowledgebaseId"], ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnASyncTesting");
            var qnaService = new QnASyncService("07fbe223-9502-44d9-b762-d0164cd678e2", ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnAManual");
            qnaService.UpdateKnowlegdeBase(qnaList);
            return RedirectToAction("Index");
        }

        public ActionResult Publish()
        {
            //var qnaService = new QnASyncService(ConfigurationManager.AppSettings["QnaMakerKnowledgebaseId"], ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnASyncTesting");
            var qnaService = new QnASyncService("07fbe223-9502-44d9-b762-d0164cd678e2", ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnAManual");
            qnaService.PublishKnowledgeBase();
            return RedirectToAction("Index");
        }

        public ActionResult SyncItem(int itemid, bool ResetQuestions)
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            var qnaList = new List<QnAItem>();
            var qnaItem = RequestHandler.FetchQnaItem(itemid, jsonUrl);
            qnaList.Add(qnaItem);
            //var qnaService = new QnASyncService(ConfigurationManager.AppSettings["QnaMakerKnowledgebaseId"], ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnASyncTesting");
            var qnaService = new QnASyncService("07fbe223-9502-44d9-b762-d0164cd678e2", ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnAManual");
            qnaService.UpdateKnowlegdeBaseSingleItem(qnaList, ResetQuestions);
            return RedirectToAction("Index");
        }

        public JsonResult CheckIfUpdatedQuestion(int itemid) //change signature to string and then convert to int 
        {
            string baseUrl = HttpRuntime.AppDomainAppPath;
            string jsonUrl = baseUrl + jsonName;
            var qnaList = new List<QnAItem>();
            var qnaItem = RequestHandler.FetchQnaItem(itemid, jsonUrl);
            qnaList.Add(qnaItem);

            //var qnaService = new QnASyncService(ConfigurationManager.AppSettings["QnaMakerKnowledgebaseId"], ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnASyncTesting");
            var qnaService = new QnASyncService("07fbe223-9502-44d9-b762-d0164cd678e2", ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"], "QnAManual");
            bool changedQuestions = qnaService.CheckIfUpdatedQuestion(qnaItem);

            if (!changedQuestions)
            {
                SyncItem(itemid, false);
            }

            return Json(new { result = changedQuestions }, JsonRequestBehavior.AllowGet);
        }
    }
}