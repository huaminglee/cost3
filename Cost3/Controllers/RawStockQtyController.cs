using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;
using Cost.ViewModels;

namespace Cost.Controllers
{
    [Authorize(Roles="Technic,Factory,Administrator")]
    public class RawStockQtyController : Controller
    {
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            if (User.IsInRole("Factory"))//隐藏FactoryCode
                ViewBag.IsFactory = true;
            return View();
        }

        public string GetData(GridSettings grid)
        {
           //r query = db.RawStockQty as IQueryable<RawStockQty>;
            var query = (from r in db.RawStockQty
                         select new RawStockUsageViewModel
                         {
                             Id = r.Id,
                             MatNumber = r.MatNumber,
                             MatNR = r.MatNR,
                             MatDB = r.RawStock.MatDB,
                             Qty = r.Qty,
                             Unit = r.Unit,
                             Version = (int)r.Version,
                             FactoryCode=r.FactoryCode
                         }) as IQueryable<RawStockUsageViewModel>;

            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string matnr = Request.QueryString["matnr"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(matnr)) { query = query.Where(w => w.MatNR.Contains(matnr)); }
            }
            return GridSearchHelper.GetJsonResult<RawStockUsageViewModel>(grid, query);
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
                RawStockQty rawStockQty = new RawStockQty();
                
                rawStockQty.MatNumber = form["MatNumber"].Trim();
                rawStockQty.MatNR = form["MatNR"];
                rawStockQty.Qty = Convert.ToDecimal(form["Qty"].Trim());
                rawStockQty.Unit = form["Unit"].Trim();
                rawStockQty.Version =Convert.ToInt32(form["Version"].Trim());
                rawStockQty.CreatedOn = DateTime.Now;
                rawStockQty.FactoryCode = User.Identity.Name;
                rawStockQty.CreatedBy = User.Identity.Name;

                try
                {
                    db.RawStockQty.Add(rawStockQty);
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
                    RawStockQty rawStockQty = db.RawStockQty.First(f => f.Id == operId);

                    rawStockQty.MatNumber = form["MatNumber"].Trim();
                    rawStockQty.MatNR = form["MatNR"];
                    rawStockQty.Qty = Convert.ToDecimal(form["Qty"].Trim());
                    rawStockQty.Unit = form["Unit"].Trim();
                    rawStockQty.Version =Convert.ToInt32(form["Version"].Trim());

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
                RawStockQty rawStockQty = db.RawStockQty.First(f => f.Id == operId);

                db.RawStockQty.Remove(rawStockQty);
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

            var query = db.RawStockQty as IQueryable<RawStockQty>;
            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }
            List<RawStockQty> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string matnr = Request.QueryString["matnr"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(matnr)) { query = query.Where(w => w.MatNR.Contains(matnr)); }
            }
            GridSearchHelper.ForExcel<RawStockQty>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select new
                                {
                                    Id = p.Id,
                                    图号 = p.MatNumber,
                                    材料代码 = p.MatNR,
                                    材料描述 = p.RawStock.MatDB,
                                    数量 = p.Qty,
                                    单位 = p.Unit,
                                    版本 = p.Version,
                                    工厂 = p.FactoryCode
                                };
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "材料消耗");

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
                    columnMapping.Add("图号,MatNumber");
                    columnMapping.Add("材料代码,MatNR"); 
                    columnMapping.Add("数量,Qty");
                    columnMapping.Add("单位,Unit");
                    columnMapping.Add("版本,Version");
                    columnMapping.Add("CreatedBy,CreatedBy");
                    columnMapping.Add("CreatedOn,CreatedOn");
                    columnMapping.Add("FactoryCode,FactoryCode");

                    ImportExportData.ImportExcel(serverPath, "RawStockQty",columnMapping);
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
       
        //待处理
        public string GetData1(GridSettings grid)
        {
            var query = db.UnfinishedRawStock as IQueryable<UnfinishedRawStock>;
            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }
            return GridSearchHelper.GetJsonResult<UnfinishedRawStock>(grid, query);
        }
        //待处理-导出excel
        public ActionResult ExportToExcel1()
        {
            GridSettings grid = new GridSettings();
            grid.SortColumn = "Id";
            grid.SortOrder = "asc";
            if (Request.QueryString["mySearch"] == "" || Request.QueryString["mySearch"] == "false")
            {
                grid.IsSearch = false;
            }
            else
            {
                grid.IsSearch = Convert.ToBoolean(Request.QueryString["mySearch"]);
            }

            string fil = Request.QueryString["myFilters"];
            grid.Where = MvcJqGrid.Filter.Create(fil, "", "", "");

            var query = db.UnfinishedRawStock as IQueryable<UnfinishedRawStock>;
            //加入当前用户
            query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            List<UnfinishedRawStock> data;

            GridSearchHelper.ForExcel<UnfinishedRawStock>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select new
                                {
                                    Id = p.Id,
                                    图号 = p.MatNumber,
                                    版本 = p.Version,
                                    工厂 = p.FactoryCode
                                };
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "待处理");

            return View();
        }
    }
}
