using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;

namespace MvcJqGrid
{
    #region GridSettings 表格属性
    [ModelBinder(typeof(GridModelBinder))]
    [Serializable]
    public class GridSettings
    {
        public int PageIndex { get; set; }//第几页== page
        public int PageSize { get; set; }//每页的行数==rows
        public string SortColumn { get; set; }//排序字段名==sidx
        public string SortOrder { get; set; }//排序方向==sord

        public bool IsSearch { get; set; }
        public Filter Where { get; set; }
    }
    #endregion

    #region GridModelBinder 实现IModelBinder的方法
    public class GridModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var request = controllerContext.HttpContext.Request;
            return new GridSettings
            {
                IsSearch = bool.Parse(request["_search"] ?? "false"),
                PageIndex = int.Parse(request["page"] ?? "1"),
                PageSize = int.Parse(request["rows"] ?? "10"),
                SortColumn = request["sidx"] ?? "Id",
                SortOrder = request["sord"] ?? "asc",
                Where = Filter.Create(
                                request["filters"],
                                request["searchField"],
                                request["searchString"],
                                request["searchOper"]
                                )
            };
        }
    }
    #endregion

    #region Filter 判断是单个还是多个查询条件-更新后避免单条件搜索出错 2013.10.15
    [DataContract]
    public class Filter
    {
        [DataMember]
        public string groupOp { get; set; }
        [DataMember]
        public Rule[] rules { get; set; }

        public static Filter Create(string jsonData, string searchField, string searchString, string searchOper)
        {
            Filter returnValue = null;

            if (!string.IsNullOrEmpty(jsonData))//多个搜索：multipleSearch:true
            {
                var serializer = new DataContractJsonSerializer(typeof(Filter));
                using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData)))
                {
                    returnValue = serializer.ReadObject(ms) as Filter;
                }

            }
            else //单个搜索：multipleSearch:false
            {
                returnValue = new Filter()
                {
                    groupOp = "AND",
                    rules = new[] { 
                                    new Rule() { 
                                        data = searchString, 
                                        field = searchField, 
                                        op = searchOper 
                                    } 
                                }
                };
            }
            return returnValue;
        }
    }
    #endregion

    #region Rule 多条件查询中的属性
    [DataContract]
    public class Rule
    {
        [DataMember]
        public string field { get; set; }
        [DataMember]
        public string op { get; set; }
        [DataMember]
        public string data { get; set; }
    }
    #endregion

    #region WhereOperation 搜索条件操作符
    public enum WhereOperation
    {
        /// <summary>
        /// The supported operations in where-extension
        /// </summary>

        //等於
        [StringValue("eq")]
        Equal,

        //不等於
        [StringValue("ne")]
        NotEqual,

        //小於
        [StringValue("lt")]
        LessThan,

        //小於等於
        [StringValue("le")]
        LessThanOrEqual,

        //大於
        [StringValue("gt")]
        GreaterThan,

        //大於等於
        [StringValue("ge")]
        GreaterThanOrEqual,

        //開始於
        [StringValue("bw")]
        BeginsWith,

        //不開始於
        [StringValue("bn")]
        NotBeginsWith,

        //在其中
        [StringValue("in")]
        In,

        //不在其中
        [StringValue("ni")]
        NotIn,

        //結束於="ew"
        [StringValue("ew")]
        EndWith,

        //不結束於
        [StringValue("en")]
        NotEndWith,

        //包含
        [StringValue("cn")]
        Contains,

        //不包含
        [StringValue("nc")]
        NotContains,

        //Null
        [StringValue("nu")]
        Null,

        //不Null
        [StringValue("nn")]
        NotNull
    }
    #endregion

    #region StringEnum

    /// <summary>
    /// Helper class for working with 'extended' enums using <see cref="StringValueAttribute"/> attributes.
    /// http://www.codeproject.com/KB/cs/stringenum.aspx
    /// </summary>
    public class StringEnum
    {
        #region Instance implementation

        private Type _enumType;
        private static Hashtable _stringValues = new Hashtable();

        /// <summary>
        /// Creates a new <see cref="StringEnum"/> instance.
        /// </summary>
        /// <param name="enumType">Enum type.</param>
        public StringEnum(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", enumType.ToString()));

            _enumType = enumType;
        }

        /// <summary>
        /// Gets the string value associated with the given enum value.
        /// </summary>
        /// <param name="valueName">Name of the enum value.</param>
        /// <returns>String Value</returns>
        public string GetStringValue(string valueName)
        {
            Enum enumType;
            string stringValue = null;
            try
            {
                enumType = (Enum)Enum.Parse(_enumType, valueName);
                stringValue = GetStringValue(enumType);
            }
            catch (Exception) { }//Swallow!

            return stringValue;
        }

        /// <summary>
        /// Gets the string values associated with the enum.
        /// </summary>
        /// <returns>String value array</returns>
        public Array GetStringValues()
        {
            ArrayList values = new ArrayList();
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs.Length > 0)
                    values.Add(attrs[0].Value);

            }

            return values.ToArray();
        }

        /// <summary>
        /// Gets the values as a 'bindable' list datasource.
        /// </summary>
        /// <returns>IList for data binding</returns>
        public IList GetListValues()
        {
            Type underlyingType = Enum.GetUnderlyingType(_enumType);
            ArrayList values = new ArrayList();
            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in _enumType.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs.Length > 0)
                    values.Add(new DictionaryEntry(Convert.ChangeType(Enum.Parse(_enumType, fi.Name), underlyingType), attrs[0].Value));

            }

            return values;

        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <returns>Existence of the string value</returns>
        public bool IsStringDefined(string stringValue)
        {
            return Parse(_enumType, stringValue) != null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public bool IsStringDefined(string stringValue, bool ignoreCase)
        {
            return Parse(_enumType, stringValue, ignoreCase) != null;
        }

        /// <summary>
        /// Gets the underlying enum type for this instance.
        /// </summary>
        /// <value></value>
        public Type EnumType
        {
            get { return _enumType; }
        }

        #endregion

        #region Static implementation

        /// <summary>
        /// Gets a string value for a particular enum value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>String Value associated via a <see cref="StringValueAttribute"/> attribute, or null if not found.</returns>
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            if (_stringValues.ContainsKey(value))
                output = (_stringValues[value] as StringValueAttribute).Value;
            else
            {
                //Look for our 'StringValueAttribute' in the field's custom attributes
                FieldInfo fi = type.GetField(value.ToString());
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs.Length > 0)
                {
                    _stringValues.Add(value, attrs[0]);
                    output = attrs[0].Value;
                }

            }
            return output;

        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value (case sensitive).
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="stringValue">String value.</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static object Parse(Type type, string stringValue)
        {
            return Parse(type, stringValue, false);
        }

        /// <summary>
        /// Parses the supplied enum and string value to find an associated enum value.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="stringValue">String value.</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Enum value associated with the string value, or null if not found.</returns>
        public static object Parse(Type type, string stringValue, bool ignoreCase)
        {
            object output = null;
            string enumStringValue = null;

            if (!type.IsEnum)
                throw new ArgumentException(String.Format("Supplied type must be an Enum.  Type was {0}", type.ToString()));

            //Look for our string value associated with fields in this enum
            foreach (FieldInfo fi in type.GetFields())
            {
                //Check for our custom attribute
                StringValueAttribute[] attrs = fi.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attrs.Length > 0)
                    enumStringValue = attrs[0].Value;

                //Check for equality then select actual enum value.
                if (string.Compare(enumStringValue, stringValue, ignoreCase) == 0)
                {
                    output = Enum.Parse(type, fi.Name);
                    break;
                }
            }

            return output;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue)
        {
            return Parse(enumType, stringValue) != null;
        }

        /// <summary>
        /// Return the existence of the given string value within the enum.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <param name="enumType">Type of enum</param>
        /// <param name="ignoreCase">Denotes whether to conduct a case-insensitive match on the supplied string value</param>
        /// <returns>Existence of the string value</returns>
        public static bool IsStringDefined(Type enumType, string stringValue, bool ignoreCase)
        {
            return Parse(enumType, stringValue, ignoreCase) != null;
        }

        #endregion
    }
    #endregion

    #region StringValueAttribute

    /// <summary>
    /// Simple attribute class for storing String Values
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        private string _value;

        /// <summary>
        /// Creates a new <see cref="StringValueAttribute"/> instance.
        /// </summary>
        /// <param name="value">Value.</param>
        public StringValueAttribute(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value></value>
        public string Value
        {
            get { return _value; }
        }
    }

    #endregion

    #region LinqExtensions 排序和筛选条件
    public static class LinqExtensions
    {
        /// <summary>Orders the sequence by specific column and direction.</summary>
        /// <param name="query">The query.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="ascending">if set to true [ascending].</param>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, string direction)
        {
            string methodName = string.Format("OrderBy{0}",
                direction.ToLower() == "asc" ? "" : "Descending");

            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            foreach (var property in sortColumn.Split('.'))
                memberAccess = MemberExpression.Property
                   (memberAccess ?? (parameter as Expression), property);

            LambdaExpression orderByLambda = Expression.Lambda(memberAccess, parameter);

            MethodCallExpression result = Expression.Call(
                      typeof(Queryable),
                      methodName,
                      new[] { query.ElementType, memberAccess.Type },
                      query.Expression,
                      Expression.Quote(orderByLambda));

            return query.Provider.CreateQuery<T>(result);
        }


        public static IQueryable<T> Where<T>(this IQueryable<T> query, string column, object value, WhereOperation operation)
        {
            if (string.IsNullOrEmpty(column))
                return query;
            //组建一个表达式树来创建一个参数p
            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            foreach (var property in column.Split('.'))
                memberAccess = MemberExpression.Property
                   (memberAccess ?? (parameter as Expression), property);

            //change param value type
            //necessary to getting bool from string
            //ConstantExpression filter = Expression.Constant
            //    (
            //        Convert.ChangeType(value, memberAccess.Type)
            //    );

            ///如果数据库表字段为null（可为空），则变量filter=null，以上代码遇到null值会异常，改为如下：---------------------
            ConstantExpression filter = null;
            Type t = memberAccess.Type;
            Type t2 = Nullable.GetUnderlyingType(memberAccess.Type);

            // here we know our thing is nullable 
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                //check to see if int
                if (t2 == typeof(int))
                {
                    //var result2 = Activator.CreateInstance(t);
                    int val;
                    //return int value or null int
                    var valNull = int.TryParse(value.ToString(), out val) ? val : (int?)null;
                    filter = Expression.Constant(valNull, t);
                }

            }
            else
                //else not nullable create ContantExpresion as normal
                filter = Expression.Constant(Convert.ChangeType(value, t));
            ///如果数据库表字段为null（可为空），则以上代码可以避免异常。-------------------------------------------------




            //switch operation
            Expression condition = null;
            LambdaExpression lambda = null;
            switch (operation)
            {
                //等於 equal ==
                case WhereOperation.Equal:
                    condition = Expression.Equal(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //不等於 not equal !=
                case WhereOperation.NotEqual:
                    condition = Expression.NotEqual(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //小於
                case WhereOperation.LessThan:
                    condition = Expression.LessThan(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //小於等於
                case WhereOperation.LessThanOrEqual:
                    condition = Expression.LessThanOrEqual(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //大於
                case WhereOperation.GreaterThan:
                    condition = Expression.GreaterThan(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //大於等於
                case WhereOperation.GreaterThanOrEqual:
                    condition = Expression.GreaterThanOrEqual(memberAccess, filter);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //開始於
                case WhereOperation.BeginsWith:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                        Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //不開始於
                case WhereOperation.NotBeginsWith:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                        Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //在其中 string.Contains()
                case WhereOperation.In:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break
                        ;
                //不在其中 string.Contains()
                case WhereOperation.NotIn:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //結束於
                case WhereOperation.EndWith:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                        Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //不結束於
                case WhereOperation.NotEndWith:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("EndsWith", new[] { typeof(string) }),
                        Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //包含 string.Contains()
                case WhereOperation.Contains:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //不包含
                case WhereOperation.NotContains:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //Null
                case WhereOperation.Null:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("IsNullOrEmpty"),
                        Expression.Constant(value));
                    lambda = Expression.Lambda(condition, parameter);
                    break;

                //不Null
                case WhereOperation.NotNull:
                    condition = Expression.Call(memberAccess,
                        typeof(string).GetMethod("IsNullOrEmpty"),
                        Expression.Constant(value));
                    condition = Expression.Not(condition);
                    lambda = Expression.Lambda(condition, parameter);
                    break;
            }

            //组建表达式树:Where语句
            MethodCallExpression result = Expression.Call(
                   typeof(Queryable), "Where",
                   new[] { query.ElementType },
                   query.Expression,
                   lambda);
            //使用表达式树来生成动态查询
            return query.Provider.CreateQuery<T>(result);
        }
    }
    #endregion

    #region 抽取搜索公用部分
    /// <summary>
    /// <para name="gird">Grid设置类</param>
    /// <para name="query">IQueryable类型的实体</para>
    /// <para name="count">返回记录总数</para>
    /// <para name="data">返回泛型集合</para>
    /// </summary>
    #endregion
    public static  class GridSearchHelper 
    {
        public static void GetQuery<T>(GridSettings grid, IQueryable<T> query, out int count, out List<T> data)
        //public static JsonResult  GetQuery<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
            {
                //And
                if (grid.Where.groupOp == "AND")
                    foreach (var rule in grid.Where.rules)
                        query = query.Where<T>(rule.field, rule.data,(WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule.op));
                else
                {  //更新--2013.10.26
                    IQueryable<T> temp = null;
                    foreach (var rule in grid.Where.rules)
                    {
                        var rule1 = rule;
                        var t = query.Where<T>(rule1.field, rule1.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule1.op));

                        if (temp == null)
                            temp = t;
                        else
                            //temp = temp.Union(t); // Union!!
                            temp = temp.Concat<T>(t);//使用union会导致查不出数据
                    }
                    query = temp.Distinct<T>();
                }
            }
            //sorting
            query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);

            //count
             count = query.Count();

            //paging
             data = query.Skip((grid.PageIndex - 1) * grid.PageSize).Take(grid.PageSize).ToList();

            //--------------------------
            // var result = new
            //{
            //    total = (int)Math.Ceiling((double)count / grid.PageSize),
            //    page = grid.PageIndex,
            //    records = count,
            //    rows = (from d in data
            //            select d
            //           ).ToArray()
            //};

            // return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// JSON格式的查询结果（string）-使用Metadata可避免json循环引用
        /// </summary>
        /// <typeparam name="T">实体或视图模型</typeparam>
        /// <param name="grid">grid属性</param>
        /// <param name="query">传入查询参数</param>
        /// <returns></returns>
        public static  string  GetJsonResult<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
            {
                //And
                if (grid.Where.groupOp == "AND")
                    foreach (var rule in grid.Where.rules)
                        query = query.Where<T>(rule.field, rule.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule.op));
                else
                {  //更新--2013.10.26
                    IQueryable<T> temp = null;
                    foreach (var rule in grid.Where.rules)
                    {
                        var rule1 = rule;
                        var t = query.Where<T>(rule1.field, rule1.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule1.op));

                        if (temp == null)
                            temp = t;
                        else
                            //temp = temp.Union(t); // Union!!
                            temp = temp.Concat<T>(t);//使用union会导致查不出数据
                    }
                    query = temp.Distinct<T>();
                }
            }
            //sorting
            query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);

            //count
            var count = query.Count();

            //paging
            var data = query.Skip((grid.PageIndex - 1) * grid.PageSize).Take(grid.PageSize).ToList();

            //-2013.11.3 抽出result对象，然后转为json字符串传递给Controller中的GetData方法-------------------------
            var result = new
            {
                total = (int)Math.Ceiling((double)count / grid.PageSize),
                page = grid.PageIndex,
                records = count,
                rows = (from d in data
                        select d    //用viewmodel/model的字段，通过jqgrid的字段增减。
                       ).ToArray()
            };

            //return System.Web.Mvc.Controller.Json(result, JsonRequestBehavior.AllowGet);//错误	3	“System.Web.Mvc.Controller.Json(object)”不可访问，因为它受保护级别限制	

            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 返回JsonResult类型的数据(会产生循环引用的问题。用Metadata无法解决。解决方法：
        /// rowsrows = (from d in data
        ///           select new
        ///           {
        ///               Id = d.Id.ToString(),
        ///               PNumber = d.PNumber,
        ///               CNumber = d.CNumber,
        ///               CUnit=d.CUnit,
        ///               CQty=d.CQty
        ///           }).ToArray()
        /// </summary>
        /// <typeparam name="T">实体或视图模型</typeparam>
        /// <param name="grid">grid属性</param>
        /// <param name="query">传入查询参数</param>
        /// <returns>JsonResult</returns>
        public static JsonResult GetQuery<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
            {
                //And
                if (grid.Where.groupOp == "AND")
                    foreach (var rule in grid.Where.rules)
                        query = query.Where<T>(rule.field, rule.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule.op));
                else
                {  //更新--2013.10.26
                    IQueryable<T> temp = null;
                    foreach (var rule in grid.Where.rules)
                    {
                        var rule1 = rule;
                        var t = query.Where<T>(rule1.field, rule1.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule1.op));

                        if (temp == null)
                            temp = t;
                        else
                            //temp = temp.Union(t); // Union!!
                            temp = temp.Concat<T>(t);//使用union会导致查不出数据
                    }
                    query = temp.Distinct<T>();
                }
            }
            //sorting
            query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);

            //count
            var count = query.Count();

            //paging
            var data = query.Skip((grid.PageIndex - 1) * grid.PageSize).Take(grid.PageSize).ToList();

            //--------------------------
            var result = new
            {
                total = (int)Math.Ceiling((double)count / grid.PageSize),
                page = grid.PageIndex,
                records = count,
                rows = (from d in data
                        select d
                       ).ToArray()
            };
            //实例化JsonResult
            JsonResult jr = new JsonResult();
            jr.Data = result; 
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //return Json(result, JsonRequestBehavior.AllowGet);
            return jr;
        }

        ///给excel提供数据
        public static void ForExcel<T>(GridSettings grid, IQueryable<T> query, out List<T> data)
        //public static JsonResult  GetQuery<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
            {
                //And
                if (grid.Where.groupOp == "AND")
                    foreach (var rule in grid.Where.rules)
                        query = query.Where<T>(rule.field, rule.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule.op));
                else
                {  //更新--2013.10.26
                    IQueryable<T> temp = null;
                    foreach (var rule in grid.Where.rules)
                    {
                        var rule1 = rule;
                        var t = query.Where<T>(rule1.field, rule1.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule1.op));

                        if (temp == null)
                            temp = t;
                        else
                            //temp = temp.Union(t); // Union!!
                            temp = temp.Concat<T>(t);//使用union会导致查不出数据
                    }
                    query = temp.Distinct<T>();
                }
            }
            //sorting
            data = query.OrderBy<T>(grid.SortColumn, grid.SortOrder).ToList();
            //query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);       
        }

    }
}