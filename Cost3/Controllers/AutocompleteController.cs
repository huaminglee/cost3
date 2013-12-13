using System.Linq;
using System.Web.Mvc;
using Cost.Models;

namespace Cost.Controllers
{
    public class AutocompleteController : Controller
    {
        private Cost3Entities db = new Cost3Entities();

        public ActionResult Index()
        {
            return View();
        }

        //图号（产品和零件）
        public ActionResult QuickSearch(string term)
        {
            var q =
                (
                    from p in db.BOM
                    where p.PNumber.Contains(term)
                    select p.PNumber
                )
                .Union
                (
                    from c in db.BOM
                    where c.CNumber.Contains(term)
                    select c.CNumber
                )
                .Take(10);
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //产品图号
        public ActionResult QuickSearchPN(string term)
        {
            var q = (from p in db.BOM
                     where p.PNumber.Contains(term)
                     select p.PNumber).Distinct().Take(10);
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //零件图号
        public ActionResult QuickSearchCN(string term)
        {
            var q = (from p in db.BOM
                     where p.CNumber.Contains(term)
                     select p.CNumber).Distinct().Take(10);
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //生产厂
        public ActionResult QuickSearchFactory(string term)
        {
            var q =
                 db.Factory.Where(p => p.FactoryCode.Contains(term))
                .Take(10)
                .Select(r => new
                {
                    label = r.FactoryCode + " " + r.FactoryName,
                    value = r.FactoryCode
                });

            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //物料代码
        public ActionResult QuickSearchMatNR(string term)
        {
            var q =
                 db.RawStock.Where(p => p.MatNR.Contains(term))
                .Take(10)
                .Select(r => new { label = r.MatNR });

            return Json(q, JsonRequestBehavior.AllowGet);
        }
        
        //物料描述
        public ActionResult QuickSearchMatDB(string term)
        {
            var q =
                 db.RawStock.Where(p => p.MatDB.Contains(term))
                .Take(10)
                .Select(r => new { label = r.MatDB });

            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //工作中心(代码+描述)
        public ActionResult QuickSearchWorkCenter(string term)
        {
            var q = db.WorkCenter.Where(w => w.WorkCenterCode.Contains(term))
                .Take(10)
                .Select(w => new
                {
                    label = w.WorkCenterCode + " " + w.WorkCenterName,
                    value = w.WorkCenterCode
                });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //工作中心代码
        public ActionResult QuickSearchWorkCenterCode(string term)
        {
            var q =
                 db.WorkCenter.Where(p => p.WorkCenterCode.Contains(term))
                .Take(10)
                .Select(r => new { label = r.WorkCenterCode });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        
        //工作中心描述
        public ActionResult QuickSearchWorkCenterName(string term)
        {
            var q =
                 db.WorkCenter.Where(p => p.WorkCenterName.Contains(term))
                .Take(10)
                .Select(r => new { label = r.WorkCenterName });
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //成本中心
        public ActionResult QuickSearchCostCenter(string term)
        {
            var q =
                 db.WorkCenter.Where(p => p.CostCenter.Contains(term))
                .Take(10)
                .Select(r => new { label = r.CostCenter });
            return Json(q, JsonRequestBehavior.AllowGet);
        }
    }
}
