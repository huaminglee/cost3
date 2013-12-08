using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;
using Cost.ViewModels;
using System.Web.Security;

namespace Cost.Controllers
{
    [Authorize(Roles="Factory,Technic,Administrator")]
    public class LabourController : Controller
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
            //var query = db.Labour as IQueryable<Labour>;
            var query = (from l in db.Labour
                         select new LabourViewModel 
                         { 
                            Id=l.Id,
                            MatNumber=l.MatNumber,
                            WorkCenterCode=l.WorkCenterCode,
                            WorkCenterName=l.WorkCenter.WorkCenterName,//viewmodel传来
                            LabourHour=(decimal)l.LabourHour,
                            Version=(int)l.Version,
                            Remark=l.Remark,
                            FactoryCode=l.FactoryCode//viewmodel
                         })as IQueryable<LabourViewModel>;
            
            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string workcentercode = Request.QueryString["workcentercode"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(workcentercode)) { query = query.Where(w => w.WorkCenterCode.Contains(workcentercode)); }
            }
            return GridSearchHelper.GetJsonResult<LabourViewModel>(grid, query);
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
                Labour labour= new Labour();
                labour.MatNumber = form["MatNumber"].Trim();
                labour.WorkCenterCode = form["WorkCenterCode"].Trim();
                labour.LabourHour =Convert.ToDecimal(form["LabourHour"].Trim());
                labour.Version = Convert.ToInt32(form["Version"].Trim());
                labour.Remark = form["Remark"].Trim();
                labour.CreatedOn = DateTime.Now;
                labour.FactoryCode = User.Identity.Name;

                db.Labour.Add(labour);
                db.SaveChanges();
            }
            //edit
            else if (operation.Equals("edit"))
            {
                try
                {
                    Labour labour = db.Labour.First(f => f.Id == operId);
                    labour.MatNumber = form["MatNumber"].Trim();
                    labour.WorkCenterCode = form["WorkCenterCode"].Trim();
                    labour.LabourHour = Convert.ToDecimal(form["LabourHour"].Trim());
                    labour.Version =Convert.ToInt32(form["Version"].Trim());
                    labour.Remark = form["Remark"].Trim();

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
                Labour labour = db.Labour.First(f => f.Id == operId);
                db.Labour.Remove(labour);
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

            var query = db.Labour as IQueryable<Labour>;

            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }

            List<Labour> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string matnumber = Request.QueryString["matnumber"];
                string workcentercode = Request.QueryString["workcentercode"];
                if (!string.IsNullOrEmpty(matnumber)) { query = query.Where(w => w.MatNumber.Contains(matnumber)); }
                if (!string.IsNullOrEmpty(workcentercode)) { query = query.Where(w => w.WorkCenterCode.Contains(workcentercode)); }
            }
            GridSearchHelper.ForExcel<Labour>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "工时.xls");

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
                    ImportExportData.ImportExcel(serverPath, "Labour");
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
        public ActionResult QuickSearchWorkCenter(string term)
        {
            var q =
                 db.WorkCenter.Where(p => p.WorkCenterCode.Contains(term))
                .Take(10)
                .Select(r => new
                {
                    label = r.WorkCenterCode + " " + r.WorkCenterName,
                    value = r.WorkCenterCode
                });

            return Json(q, JsonRequestBehavior.AllowGet);
        }

        //待处理
        public string GetData1(GridSettings grid)
        {
            var query = db.UnfinishedLabour as IQueryable<UnfinishedLabour>;
            //工艺所可以查看所有工厂的数据
            if (User.IsInRole("Factory"))
            {
                query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            }
            return GridSearchHelper.GetJsonResult<UnfinishedLabour>(grid, query);
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

            var query = db.UnfinishedLabour as IQueryable<UnfinishedLabour>;
            //加入当前用户
            query = query.Where(u => u.FactoryCode.Equals(User.Identity.Name));
            List<UnfinishedLabour> data;

            GridSearchHelper.ForExcel<UnfinishedLabour>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "UnLabour.xls");

            return View();
        }
    }
}
