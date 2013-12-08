using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcJqGrid;
using Cost.Models;
using Helper;
using System.IO;

namespace Cost.Controllers
{
    [Authorize(Roles="Technic,Administrator")]
    public class VersionManagementController : Controller
    {
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            var query = db.VersionManagement as IQueryable<VersionManagement>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string a = Request.Form["pn"];
                string parentNum = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            return GridSearchHelper.GetJsonResult<VersionManagement>(grid, query);
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
                VersionManagement versionManagement = new VersionManagement();

                versionManagement.PNumber = form["PNumber"].Trim();
                versionManagement.CNumber = form["CNumber"].Trim();
                versionManagement.LabourVersion =Convert.ToInt32( form["LabourVersion"].Trim());
                versionManagement.RawStockVersion =Convert.ToInt32( form["RawStockVersion"].Trim());
                versionManagement.ProductVersion = form["ProductVersion"].Trim();
                versionManagement.CreatedOn = DateTime.Now;

                db.VersionManagement.Add(versionManagement);
                db.SaveChanges();
            }
            //edit
            else if (operation.Equals("edit"))
            {
                try
                {
                    VersionManagement versionManagement = db.VersionManagement.First(f => f.Id == operId);

                    versionManagement.PNumber = form["PNumber"].Trim();
                    versionManagement.CNumber = form["CNumber"].Trim();
                    versionManagement.LabourVersion =Convert.ToInt32(form["LabourVersion"].Trim());
                    versionManagement.RawStockVersion =Convert.ToInt32( form["RawStockVersion"].Trim());
                    versionManagement.ProductVersion = form["ProductVersion"].Trim();

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
                VersionManagement versionManagement = db.VersionManagement.First(f => f.Id == operId);

                db.VersionManagement.Remove(versionManagement);
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

            var query = db.VersionManagement as IQueryable<VersionManagement>;
            List<VersionManagement> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string parentNum = Request.QueryString["pn1"];
                string childNum = Request.QueryString["cn1"];
                //string parentNum = Request.QueryString["pn"];
                //string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            GridSearchHelper.ForExcel<VersionManagement>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "VersionManagement.xls");

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
                    ImportExportData.ImportExcel(serverPath, "VersionManagement");
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

        //上传EXCEL文件
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Content/UploadedFolder"), fileName);
                    //如果存在相同名称文件则删除
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                    file.SaveAs(filePath);

                    try
                    {
                        ImportExportData.ImportExcel(filePath, "VersionManagement");
                        System.IO.File.Delete(filePath);
                        return Json(new { success = true, message = "导入成功！" });
                    }
                    catch (Exception ex)
                    {
                        ViewBag.msg = ex.Message;
                        System.IO.File.Delete(filePath);
                        return Json(new { success = false, message = "导入失败" + ex.Message });
                    }

                }
            }
            return RedirectToAction("Index");
        }

        //自动完成
        public ActionResult QuickSearch(string term)
        {
            //var q =
            //     db.BOM.Where(p => p.PNumber.Contains(term))
            //    .Concat(db.BOM.Where(c => c.CNumber.Contains(term)))
            //    .Distinct()
            //    .Take(10)
            //    .Select(r => new { label = r.PNumber });

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

    }
}
