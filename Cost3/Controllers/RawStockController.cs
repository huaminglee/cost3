using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;

namespace Cost.Controllers
{
    [Authorize(Roles="Technic,Finance,Administrator")]
    public class RawStockController : Controller
    {
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            var query = db.RawStock as IQueryable<RawStock>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnr = Request.QueryString["matnr"];
                string matdb = Request.QueryString["matdb"];
                if (!string.IsNullOrEmpty(matnr)) { query = query.Where(w => w.MatNR.Equals(matnr)); }
                if (!string.IsNullOrEmpty(matdb)) { query = query.Where(w => w.MatDB.Contains(matdb)); }
            }
            return GridSearchHelper.GetJsonResult<RawStock>(grid, query);
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
                RawStock rawStock = new RawStock();
                rawStock.MatNR = form["MatNR"].Trim();
                rawStock.MatDB = form["MatDB"].Trim();
                rawStock.BUn = form["BUn"].Trim();
                rawStock.UnitPrice = Convert.ToDecimal(form["UnitPrice"].Trim());
                rawStock.CreatedOn = DateTime.Now;
                rawStock.CreatedBy = User.Identity.Name;
                try
                {
                    db.RawStock.Add(rawStock);
                    db.SaveChanges();
                    return Json(new { success = true, message = "操作成功！" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "操作失败！" + ex.Message });
                }
            }
            //edit
            else if (operation.Equals("edit"))
            {
                try
                {
                    RawStock rawStock = db.RawStock.First(f => f.Id == operId);
                    rawStock.MatNR = form["MatNR"].Trim();
                    rawStock.MatDB = form["MatDB"].Trim();
                    rawStock.BUn = form["BUn"].Trim();
                    rawStock.UnitPrice = Convert.ToDecimal(form["UnitPrice"].Trim());

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
                RawStock rawStock = db.RawStock.First(f => f.Id == operId);
                db.RawStock.Remove(rawStock);
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

            var query = db.RawStock as IQueryable<RawStock>;
            List<RawStock> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnr = Request.QueryString["matnr1"];
                string matdb = Request.QueryString["matdb1"];

                if (!string.IsNullOrEmpty(matnr)) { query = query.Where(w => w.MatNR.Contains(matnr)); }
                if (!string.IsNullOrEmpty(matdb)) { query = query.Where(w => w.MatDB.Contains(matdb)); }
            }
            GridSearchHelper.ForExcel<RawStock>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select new
                                {
                                    物料代码 = p.MatNR,
                                    物料描述 = p.MatDB,
                                    计量单位 = p.BUn,
                                    单价 = p.UnitPrice
                                };
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "原材料.xls");

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
                    var columnMapping = new List<string>();
                    columnMapping.Add("CreatedBy,CreatedBy");
                    columnMapping.Add("CreatedOn,CreatedOn");
                    columnMapping.Add("物料代码,MatNR");
                    columnMapping.Add("物料描述,MatDB");
                    columnMapping.Add("基本单位,BUn");
                    columnMapping.Add("单价,UnitPrice");

                    ImportExportData.ImportExcel(serverPath, "RawStock",columnMapping);
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
                    System.IO.File.Delete(serverPath);
                    JsonResult jt = Json(new { success = false, message = "导入失败" + ex.Message, fileName = fileName });
                    jt.ContentType = "text/html";
                    return jt;
                    //return Json(new { success = false, message = "导入失败" + ex.Message, fileName = fileName });
                }
            }
            return View("Index");
        }

        //自动完成
        [AllowAnonymous]
        public ActionResult QuickSearchMatNR(string term)
        {
            var q =
                 db.RawStock.Where(p => p.MatNR.Contains(term))
                .Take(10)
                .Select(r => new { label = r.MatNR });

            return Json(q, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult QuickSearchMatDB(string term)
        {
            var q =
                 db.RawStock.Where(p => p.MatDB.Contains(term))
                .Take(10)
                .Select(r => new { label = r.MatDB });

            return Json(q, JsonRequestBehavior.AllowGet);
        }
    }
}
