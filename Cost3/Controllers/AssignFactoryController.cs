using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;

namespace Cost.Controllers
{
    [Authorize(Roles = "Technic,Administrator")]
    public class AssignFactoryController : Controller
    {
        private Cost3Entities db = new Cost3Entities();

        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            var query = db.AssignFactory as IQueryable<AssignFactory>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string factorycode = Request.QueryString["factorycode"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(factorycode)) { query = query.Where(w => w.FactoryCode.Contains(factorycode)); }
            }
            return GridSearchHelper.GetJsonResult<AssignFactory>(grid, query);
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
                AssignFactory assignFactory = new AssignFactory();
                assignFactory.MatNumber = form["MatNumber"].Trim();
                assignFactory.FactoryCode = form["FactoryCode"].Trim();
                assignFactory.Version =Convert.ToInt32(form["Version"].Trim());
                assignFactory.CreatedOn = DateTime.Now;

                db.AssignFactory.Add(assignFactory);
                db.SaveChanges();
            }
            //edit
            else if (operation.Equals("edit"))
            {
                try
                {
                    AssignFactory assignFactory = db.AssignFactory.First(f => f.Id == operId);
                    assignFactory.MatNumber = form["MatNumber"].Trim();
                    assignFactory.FactoryCode = form["FactoryCode"].Trim();
                    assignFactory.Version =Convert.ToInt32(form["Version"].Trim());

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
                AssignFactory assignFactory = db.AssignFactory.First(f => f.Id == operId);
                db.AssignFactory.Remove(assignFactory);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult ExportToExcel()
        {
            GridSettings grid = new GridSettings();
            grid.SortColumn = "Id";
            grid.SortOrder = "asc";
            if (Request.QueryString["mySearch"] == "" || Request.QueryString["mySearch"] == "false")
            { grid.IsSearch = false; }
            else
            {
                grid.IsSearch = Convert.ToBoolean(Request.QueryString["mySearch"]);
            }

            string fil = Request.QueryString["myFilters"];
            grid.Where = MvcJqGrid.Filter.Create(fil, "", "", "");

            var query = db.AssignFactory as IQueryable<AssignFactory>;
            List<AssignFactory> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string factorycode = Request.QueryString["factorycode"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(factorycode)) { query = query.Where(w => w.FactoryCode.Contains(factorycode)); }
            }
            GridSearchHelper.ForExcel<AssignFactory>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "分配工厂.xls");

            return View();
        }

        public ActionResult ImportExcel0()
        {
            if (Request.Files["FileUpload1"] != null && Request.Files["FileUpload1"].ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(Request.Files["FileUpload1"].FileName);
                //string extension = System.IO.Path.GetExtension(fileName);
                string serverPath = string.Format("{0}/{1}", Server.MapPath("~/Content/UploadedFolder"), fileName);
                if (System.IO.File.Exists(serverPath))
                    System.IO.File.Delete(serverPath);

                Request.Files["FileUpload1"].SaveAs(serverPath);

                try
                {
                    ImportExportData.ImportExcel(serverPath, "AssignFactory");
                    ViewBag.Msg = "good";
                    System.IO.File.Delete(serverPath);
                    //为避免IE8出现下载文件提示，需将ContentType设置为"text/html"
                    JsonResult jt = Json(new { success = true, message = "导入成功！", fileName = fileName });
                    jt.ContentType = "text/html";
                    return jt;
                    //增加以上3行代码。注释以下1行代码
                    //return Json(new { success = true, message = "导入成功！", fileName = fileName });
                }
                catch (Exception ex)
                {
                    ViewBag.Msg = ex.Message;
                    //System.IO.File.Delete(serverPath);
                    JsonResult jt = Json(new { success = false, message = "导入失败" + ex.Message, fileName = fileName });
                    jt.ContentType = "text/html";
                    return jt;
                    //return Json(new { success = false, message = "导入失败" + ex.Message, fileName = fileName });
                }
            }
            return View("Index");
        }

        //自动完成
        public ActionResult QuickSearchMatNumber(string term)
        {
            var q =
                 db.AssignFactory.Where(p => p.MatNumber.Contains(term))
                //.Distinct()
                .Take(10)
                .Select(r => new { label = r.MatNumber }).Distinct();

            return Json(q, JsonRequestBehavior.AllowGet);
        }
        public ActionResult QuickSearchFactory(string term)
        {
            var q =
                 db.Factory.Where(p => p.FactoryCode.Contains(term))
                .Take(10)
                .Select(r => new {
                    label = r.FactoryCode+" "+r.FactoryName,
                    value = r.FactoryCode
                });

            return Json(q, JsonRequestBehavior.AllowGet);
        }

    }
}
