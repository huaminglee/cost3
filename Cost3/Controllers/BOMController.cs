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
    public class BOMController : Controller
    {
        //
        // GET: /BOM/
        private Cost3Entities db = new Cost3Entities();
        public ActionResult Index()
        {
            return View();
        }

        public string GetData(GridSettings grid)
        {
            var query = db.BOM as IQueryable<BOM>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string a = Request.Form["pn"];
                string parentNum = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            return GridSearchHelper.GetJsonResult<BOM>(grid, query);
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
                BOM bom = new BOM();
                bom.PNumber = form["PNumber"].Trim();
                bom.CNumber = form["CNumber"].Trim();
                bom.CUnit = form["CUnit"].Trim();
                bom.CQty = Convert.ToDecimal(form["CQty"].Trim());
                bom.CreatedOn = DateTime.Now;
                bom.CreatedBy = User.Identity.Name;
                try
                {
                    db.BOM.Add(bom);
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
                    BOM bom = db.BOM.First(f => f.Id == operId);
                    bom.PNumber = form["PNumber"].Trim();
                    bom.CNumber = form["CNumber"].Trim();
                    bom.CUnit = form["CUnit"].Trim();
                    bom.CQty = Convert.ToDecimal(form["CQty"].Trim());

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
                BOM bom = db.BOM.First(f => f.Id == operId);
                db.BOM.Remove(bom);
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

            var query = db.BOM as IQueryable<BOM>;
            List<BOM> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string parentNum = Request.QueryString["pn1"];
                string childNum = Request.QueryString["cn1"];
                //string parentNum = Request.QueryString["pn"];
                //string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            GridSearchHelper.ForExcel<BOM>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select new
                                {
                                    序号 = p.Id,
                                    产品 = p.PNumber,
                                    子项 = p.CNumber,
                                    单位 = p.CUnit,
                                    数量 = p.CQty
                                };
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "BOM数据");

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
                    columnMapping.Add("产品图号,PNumber");
                    columnMapping.Add("子项图号,CNumber");
                    columnMapping.Add("子项单位,CUnit");
                    columnMapping.Add("子项数量,CQty");
                    columnMapping.Add("CreatedBy,CreatedBy");
                    columnMapping.Add("CreatedOn,CreatedOn");
                    ImportExportData.ImportExcel(serverPath, "BOM",columnMapping);
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
                        //ImportExportData.ImportExcel(filePath, "BOMs");
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
    }
}