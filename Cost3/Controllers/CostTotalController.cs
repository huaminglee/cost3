using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cost.Models;
using MvcJqGrid;
using Helper;

namespace Cost.Controllers
{
    [Authorize(Roles="Administrator,Technic")]
    public class CostTotalController : Controller
    {
        private Cost3Entities db = new Cost3Entities();

        // GET: /CostTotal/

        public ActionResult Index()
        {
            return View(db.CostSumByProductVersion.ToList());
        }
        public string GetData(GridSettings grid)
        {
            var query = db.CostSumByProductVersion as IQueryable<CostSumByProductVersion>;
            if (grid.IsSearch == false)//如果是自定义查询
            {
                string parentNum = Request.QueryString["pn"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
            }
            return GridSearchHelper.GetJsonResult<CostSumByProductVersion>(grid, query);
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

            var query = db.CostSumByProductVersion as IQueryable<CostSumByProductVersion>;
            List<CostSumByProductVersion> data;

            if (grid.IsSearch == false)//如果是自定义查询
            {
                string parentNum = Request.QueryString["pn1"];
                if (!string.IsNullOrEmpty(parentNum)) { query = query.Where(w => w.PNumber.Contains(parentNum)); }
            }
            GridSearchHelper.ForExcel<CostSumByProductVersion>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "成本总览.xls");

            return View();
        }
        // GET: /CostTotal/Details/5

        public ActionResult LabourDetail(DetailLabour labour)
        {
            var productVersion = labour.ProductVersion.Trim();
            var pNumber = labour.PNumber.Trim();

            var q = from p in db.DetailLabour
                    where p.ProductVersion.Equals(productVersion) && p.PNumber.Equals(pNumber)
                    select p;

            if (q == null)
                return HttpNotFound();
            return View(q);        
        }

        public ActionResult LabourDetail2()
        {
            return View();
        }
        public string GetLabourDetail(GridSettings grid)
        {
            var query = db.DetailLabour as IQueryable<DetailLabour>;
            //从上一页面获得值
            string productVersion = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["ProductVersion"];
            string pNumber = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["PNumber"];
            if (!string.IsNullOrEmpty(productVersion)) { query = query.Where(w => w.ProductVersion.Equals(productVersion)); }
            if (!string.IsNullOrEmpty(pNumber)) { query = query.Where(w => w.PNumber.Equals(pNumber)); }
            if (grid.IsSearch == false)//如果是自定义查询
            {              
               //获得本页的查询条件
                string workCenterCode = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(workCenterCode)) { query = query.Where(w => w.WorkCenterCode.Contains(workCenterCode)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }             
            }

            return GridSearchHelper.GetJsonResult<DetailLabour>(grid, query);//string
        }
        public ActionResult LabourDetailExportToExcel()
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

            var query = db.DetailLabour as IQueryable<DetailLabour>;
            List<DetailLabour> data;
            //从上一页面获得值
            string productVersion = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["ProductVersion"];
            string pNumber = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["PNumber"];
            if (!string.IsNullOrEmpty(productVersion)) { query = query.Where(w => w.ProductVersion.Equals(productVersion)); }
            if (!string.IsNullOrEmpty(pNumber)) { query = query.Where(w => w.PNumber.Equals(pNumber)); }
            if (grid.IsSearch == false)//如果是自定义查询
            {
                //获得本页的查询条件
                string workCenterCode = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(workCenterCode)) { query = query.Where(w => w.WorkCenterCode.Contains(workCenterCode)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            GridSearchHelper.ForExcel<DetailLabour>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "工时明细.xls");

            return View();
        }

        public ActionResult RawStockDetail(DetailRawStock rawstock)
        {
            var productVersion = rawstock.ProductVersion.Trim();
            var pNumber = rawstock.PNumber.Trim();
            var q = from p in db.DetailRawStock
                    where p.ProductVersion.Equals(productVersion) && p.PNumber.Equals(pNumber)
                    select p;
            if (q == null)
                return HttpNotFound();
            return View(q);
        }

        public ActionResult RawStockDetail2()
        {
            return View();
        }
        public string GetRawStockDetail(GridSettings grid)
        {
            var query = db.DetailRawStock as IQueryable<DetailRawStock>;
            //从上一页面获得值
            string productVersion = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["ProductVersion"];
            string pNumber = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["PNumber"];
            if (!string.IsNullOrEmpty(productVersion)) { query = query.Where(w => w.ProductVersion.Equals(productVersion)); }
            if (!string.IsNullOrEmpty(pNumber)) { query = query.Where(w => w.PNumber.Equals(pNumber)); }
            if (grid.IsSearch == false)//如果是自定义查询
            {
                //获得本页的查询条件
                string MatNR = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(MatNR)) { query = query.Where(w => w.MatNR.Contains(MatNR)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }

            return GridSearchHelper.GetJsonResult<DetailRawStock>(grid, query);//string
        }
        public ActionResult RawStockDetailExportToExcel()
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

            var query = db.DetailRawStock as IQueryable<DetailRawStock>;
            List<DetailRawStock> data;
            //从上一页面获得值
            string productVersion = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["ProductVersion"];
            string pNumber = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["PNumber"];
            if (!string.IsNullOrEmpty(productVersion)) { query = query.Where(w => w.ProductVersion.Equals(productVersion)); }
            if (!string.IsNullOrEmpty(pNumber)) { query = query.Where(w => w.PNumber.Equals(pNumber)); }
            if (grid.IsSearch == false)//如果是自定义查询
            {
                //获得本页的查询条件
                string MatNR = Request.QueryString["pn"];
                string childNum = Request.QueryString["cn"];
                if (!string.IsNullOrEmpty(MatNR)) { query = query.Where(w => w.MatNR.Contains(MatNR)); }
                if (!string.IsNullOrEmpty(childNum)) { query = query.Where(w => w.CNumber.Contains(childNum)); }
            }
            GridSearchHelper.ForExcel<DetailRawStock>(grid, query, out data);
            var returnData = data;

            var myGrid = new System.Web.UI.WebControls.GridView();
            myGrid.DataSource = from p in returnData
                                select p;
            myGrid.DataBind();
            ImportExportData.ExportToExcel(myGrid, "材料明细.xls");

            return View();
        }
    }
}