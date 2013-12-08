using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Helper
{
    public class ImportExportData
    {
        ///<summary>
        /// Excel数据导入MSSQL数据库
        ///</summary>
        /// <param name="filePath">上传文件路径</param>
        /// <param name="cnnStrMSSQL">MSSQL连接字符串</param>
        /// <param name="dbTableName">数据库表名</param>

        public static void ImportExcel(string filePath,string dbTableName)
        {   
            //不支持关键字：metadata
            //string sqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Cost3Entities"].ConnectionString;
            
            string sqlConnectionString = @"Data Source=.\sql2012;Database=Cost3;Trusted_Connection=true;Persist Security Info=True";

            //Create connection string to Excel work book
            //string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=Excel 12.0;Persist Security Info=False";
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1';";

            //Create Connection to Excel work book
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            //Create OleDbCommand to fetch data from Excel
            OleDbCommand cmd = new OleDbCommand("Select * from [Sheet1$]", excelConnection);

            excelConnection.Open();
                OleDbDataReader dReader;
                dReader = cmd.ExecuteReader();
                SqlBulkCopy sqlBulk = new SqlBulkCopy(sqlConnectionString);
                //Give your Destination table name
                sqlBulk.DestinationTableName = dbTableName;
                sqlBulk.WriteToServer(dReader);
            excelConnection.Close();
        }

        ///<summary>
        ///导出到Excel
        ///</summary>
        ///<param name="ctrl">包含数据的控件</param>
        ///<param name="fileName">文件名</param>
        public static void ExportToExcel(WebControl ctrl,string fileName)
        {
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename="+System.Web.HttpUtility.UrlEncode(fileName,System.Text.Encoding.UTF8));
            HttpContext.Current.Response.ContentType = "application/excel";
            System.IO.StringWriter sw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            ctrl.RenderControl(htw);//传入控件
            HttpContext.Current.Response.Write(sw.ToString());
            HttpContext.Current.Response.End();
        }


    }
}