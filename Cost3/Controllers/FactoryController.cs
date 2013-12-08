using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;

namespace Cost.Controllers
{
    [Authorize(Roles = "Technic,Finace,Administrator")]
    public class FactoryController : Controller
    {
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            var query = db.Factory as IQueryable<Factory>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string factoryName = Request.QueryString["factoryName"];
                if (!string.IsNullOrEmpty(factoryName)) { query = query.Where(w => w.FactoryName.Contains(factoryName)); }
            }
            return GridSearchHelper.GetJsonResult<Factory>(grid, query);
        }

        public ActionResult OperateData(FormCollection form)
        {
            string operation = form["oper"];
            int operId = 0;
            if (form["myId"] != null)  //Id $ id         
                operId = Int32.Parse(form["myId"]);

            //add
            if (operation.Equals("add"))
            {
                Factory factory = new Factory();
                factory.FactoryCode=form["FactoryCode"].Trim();
                factory.FactoryName = form["FactoryName"].Trim();
                factory.CreatedOn = DateTime.Now;

                db.Factory.Add(factory);
                db.SaveChanges();
            }
            //edit
            else if (operation.Equals("edit"))
            {
                try
                {
                    Factory factory = db.Factory.First(f => f.Id == operId);
                    factory.FactoryCode = form["FactoryCode"].Trim();
                    factory.FactoryName = form["FactoryName"].Trim();

                    db.SaveChanges();
                    return Json(new { success = true, message = "操作成功！" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "操作失败！" + ex.Message });
                }
                //delete
            }
            else if (operation == "del")
            {
                Factory factory=db.Factory.First(f=>f.Id==operId);
                db.Factory.Remove(factory);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }      

    }
}
