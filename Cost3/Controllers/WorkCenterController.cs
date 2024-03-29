﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;
using System.IO;
using Cost.ViewModels;

namespace Cost.Controllers
{
    [Authorize(Roles = "Finance,Technic,Administrator")]
    public class WorkCenterController : Controller
    {
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            //var query = db.WorkCenter as IQueryable<WorkCenter>;
            var query = (from w in db.WorkCenter
                         select new WorkCenterViewModel
                         {
                             Id = w.Id,
                             WorkCenterCode = w.WorkCenterCode,
                             WorkCenterName = w.WorkCenterName,
                             CostCenter = w.CostCenter,
                             WorkRate = w.WorkRate,
                             FactoryCode = w.FactoryCode,
                             FactoryName = w.Factory.FactoryName
                         }) as IQueryable<WorkCenterViewModel>;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string workCenterCode = Request.QueryString["workCenterCode"];
                string workCenterName = Request.QueryString["workCenterName"];
                string costCenter = Request.QueryString["costCenter"];
                if (!string.IsNullOrEmpty(workCenterCode)) { query = query.Where(w => w.WorkCenterCode.Contains(workCenterCode)); }
                if (!string.IsNullOrEmpty(workCenterName)) { query = query.Where(w => w.WorkCenterName.Contains(workCenterName)); }
                if (!string.IsNullOrEmpty(costCenter)) { query = query.Where(w => w.CostCenter.Contains(costCenter)); }
            }
            return GridSearchHelper.GetJsonResult<WorkCenterViewModel>(grid, query);
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
                WorkCenter workCenter = new WorkCenter();
                workCenter.WorkCenterCode = form["WorkCenterCode"].Trim();
                workCenter.WorkCenterName = form["WorkCenterName"].Trim();
                workCenter.CostCenter = form["CostCenter"].Trim();
                workCenter.WorkRate = Convert.ToDecimal(form["WorkRate"].Trim());
                //workCenter.FactoryId = Convert.ToInt16(form["FactoryId"]);
                workCenter.FactoryCode = form["FactoryCode"];
                workCenter.CreatedOn = DateTime.Now;
                workCenter.CreatedBy = User.Identity.Name;
                try
                {
                    db.WorkCenter.Add(workCenter);
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
                    WorkCenter workCenter = db.WorkCenter.First(f => f.Id == operId);

                    workCenter.WorkCenterCode = form["WorkCenterCode"];//主键不能更改，如有空格保持空格
                    workCenter.WorkCenterName = form["WorkCenterName"].Trim();
                    workCenter.CostCenter = form["CostCenter"].Trim();
                    workCenter.WorkRate = Convert.ToDecimal(form["WorkRate"].Trim());
                    workCenter.FactoryCode = form["FactoryCode"];

                    db.SaveChanges();
                    //return Json(new {success = true, showMessage = true, message = "操作成功！"  }); 
                    return Json(new { success = true, message = "操作成功！" });
                    //return Content("OK");
                }
                catch (Exception ex)
                {
                    //return Json(new { success = true, showMessage = false, message = "操作失败！"+ex.Message });
                    return Json(new { success = false, message = "操作失败！" + ex.Message });
                    //return Content("SORRY");
                }
                //Response.Write(success);
                //return Json(success);

                //delete
            }
            else if (operation == "del")
            {
                WorkCenter workCenter = db.WorkCenter.First(f => f.Id == operId);
                db.WorkCenter.Remove(workCenter);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
            //return View("Index");
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

            var query = db.WorkCenter as IQueryable<WorkCenter>;
            List<WorkCenter> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string workCenterCode = Request.QueryString["workCenterCode1"];
                string workCenterName = Request.QueryString["workCenterName1"];
                string costCenter = Request.QueryString["costCenter"];

                if (!string.IsNullOrEmpty(workCenterCode)) { query = query.Where(w => w.WorkCenterCode.Contains(workCenterCode)); }
                if (!string.IsNullOrEmpty(workCenterName)) { query = query.Where(w => w.WorkCenterName.Contains(workCenterName)); }
                if (!string.IsNullOrEmpty(costCenter)) { query = query.Where(w => w.CostCenter.Contains(costCenter)); }
            }
            GridSearchHelper.ForExcel<WorkCenter>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select new
                                {
                                    生产厂=p.FactoryCode,
                                    生产厂描述=p.Factory.FactoryName,
                                    工作中心=p.WorkCenterCode,
                                    工作中心描述=p.WorkCenterName,
                                    成本中心 =p.CostCenter,
                                    工费率=p.WorkRate
                                };
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "工作中心");

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
                    columnMapping.Add("工作中心,WorkCenterCode");
                    columnMapping.Add("工作中心描述,WorkCenterName");
                    columnMapping.Add("成本中心,CostCenter");
                    columnMapping.Add("工费率,WorkRate");
                    columnMapping.Add("生产厂,FactoryCode");
                    columnMapping.Add("CreatedBy,CreatedBy");
                    columnMapping.Add("CreatedOn,CreatedOn");
                    ImportExportData.ImportExcel(serverPath, "WorkCenter",columnMapping);
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
    }
}
