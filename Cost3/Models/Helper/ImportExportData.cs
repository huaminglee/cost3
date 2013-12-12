using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using System.Linq;
using System;
using System.Web.Routing;
using System.Text;
using System.IO;

namespace Helper
{
    public class ImportExportData
    {
        public static string sqlConnectionString = @"Data Source=.\sql2012;Database=Cost3;Trusted_Connection=true;Persist Security Info=True";

        ///<summary>
        /// Excel数据导入MSSQL数据库
        ///</summary>
        /// <param name="filePath">上传文件路径</param>
        /// <param name="cnnStrMSSQL">MSSQL连接字符串</param>
        /// <param name="dbTableName">数据库表名</param>

        //public static void ImportExcel(string filePath,string dbTableName)
        public static void ImportExcel(string filePath, string dbTableName, List<string> columnMapping)
        {
            //不支持关键字：metadata
            //string sqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Cost3Entities"].ConnectionString;

            //Create connection string to Excel work book
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1';";

            //Create Connection to Excel work book
            OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
            //Create OleDbCommand to fetch data from Excel
            OleDbCommand cmd = new OleDbCommand("Select * from [Sheet1$]", excelConnection);

            excelConnection.Open();//打开连接
                OleDbDataReader dReader = cmd.ExecuteReader();
                //转为datatable
                DataTable dt = new DataTable();
                dt.Load(dReader);
                // 添加共有的额外的列
                string user = HttpContext.Current.User.Identity.Name;
                dt.Columns.Add(new DataColumn("CreatedBy", typeof(System.String)));
                dt.Columns.Add(new DataColumn("CreatedOn", typeof(System.DateTime)));
                //添加特定的列
                RouteData rd = HttpContext.Current.Request.RequestContext.RouteData;
                string currentController = rd.GetRequiredString("controller");
                string currentAction = rd.GetRequiredString("action");
                if (currentController == "Labour" || currentController == "RawStockQty")
                {
                    dt.Columns.Add(new DataColumn("FactoryCode", typeof(System.String)));
                    //填充列
                    foreach (DataRow dr in dt.Rows)
                    {
                        dr["FactoryCode"] = user;
                    }
                }
                // 填充额外的列            
                foreach (DataRow dr in dt.Rows)
                {
                    dr["CreatedBy"] = user;
                    dr["CreatedOn"] = DateTime.Now;
                }
                try
                {
                    BatchCopy(dt, dbTableName, columnMapping);//复制数据
                }
                catch (Exception)
                {
                    excelConnection.Close();//关闭连接，释放进程。
                    throw;
                }
               
            excelConnection.Close();//关闭连接
        }

        ///<summary>
        ///导出到Excel
        ///</summary>
        ///<param name="ctrl">包含数据的控件</param>
        ///<param name="fileName">文件名</param>
        public static void ExportToExcel(WebControl ctrl, string fileName)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            //HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + "" + fileName + ".xls");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8) + ".xls");//这样的话，可以设置文件名为中文，且文件名不会乱码。其实就是将汉字转换成UTF8

            StringWriter sw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            ctrl.RenderControl(htw);
            HttpContext.Current.Response.Write(sw.ToString());
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// 复制数据
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="destinationTableName">目标数据表名称</param>
        /// <param name="columnMapping">映射列</param>
        public static void BatchCopy(DataTable dt, string destinationTableName, List<string> columnMapping)
        {
            //bool IsOK = true;
            using (SqlConnection cnn = new SqlConnection(sqlConnectionString))
            {
                cnn.Open();//打开连接
                using (SqlTransaction tran = cnn.BeginTransaction())//数据库级别的事务
                {
                    using (SqlBulkCopy sqlCopy = new SqlBulkCopy(cnn, SqlBulkCopyOptions.KeepIdentity, tran))
                    {
                        sqlCopy.DestinationTableName = destinationTableName;
                        if (columnMapping != null)
                        {
                            foreach (var mapping in columnMapping)
                            {
                                var split = mapping.Split(new[] { ',' });
                                sqlCopy.ColumnMappings.Add(split.First(), split.Last());
                            }
                        }
                        try
                        {
                            sqlCopy.WriteToServer(dt);//写入数据库
                            tran.Commit();//提交事务
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();//回滚事务
                            throw ex;       //抛出异常
                            //IsOK = false;
                        }
                    }
                }
                cnn.Close();//关闭连接
            }
            //return IsOK;
        }

        ///复制数据-不带事务的
        public static void BatchBulkCopy(DataTable dataTable, string DestinationTbl, List<string> columnMapping)
        {
            // Get the DataTable 
            DataTable dtInsertRows = dataTable;

            using (SqlBulkCopy sbc = new SqlBulkCopy(sqlConnectionString, SqlBulkCopyOptions.KeepIdentity))
            {
                sbc.DestinationTableName = DestinationTbl;

                // Add your column mappings here
                foreach (var mapping in columnMapping)
                {
                    var split = mapping.Split(new[] { ',' });
                    sbc.ColumnMappings.Add(split.First(), split.Last());
                }

                // Finally write to server
                sbc.WriteToServer(dtInsertRows);
            }
        }

    }
}