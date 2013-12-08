using System.Web;
using System.Web.Optimization;

namespace Cost
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"
                      ));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            //bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            //            "~/Content/themes/base/jquery.ui.core.css",
            //            "~/Content/themes/base/jquery.ui.resizable.css",
            //            "~/Content/themes/base/jquery.ui.selectable.css",
            //            "~/Content/themes/base/jquery.ui.accordion.css",
            //            "~/Content/themes/base/jquery.ui.autocomplete.css",
            //            "~/Content/themes/base/jquery.ui.button.css",
            //            "~/Content/themes/base/jquery.ui.dialog.css",
            //            "~/Content/themes/base/jquery.ui.slider.css",
            //            "~/Content/themes/base/jquery.ui.tabs.css",
            //            "~/Content/themes/base/jquery.ui.datepicker.css",
            //            "~/Content/themes/base/jquery.ui.progressbar.css",
            //            "~/Content/themes/base/jquery.ui.theme.css"));

            //===================================================================
            //jqgrid
            bundles.Add(new ScriptBundle("~/bundles/jqgrid").Include(
                       "~/Scripts/i18n/grid.locale-cn.js",
                       "~/Scripts/jquery.jqGrid.src.js"));
            bundles.Add(new StyleBundle("~/Content/jqgrid").Include(
               "~/Content/ui.jqgrid.css"));

            //jquery主题
            string themeName = System.Configuration.ConfigurationManager.AppSettings["jqueryTheme"];
            string themeFolder = "~/Content/themes/" + themeName;
            bundles.Add(new StyleBundle(themeFolder + "/css").Include(
                    themeFolder + "/jquery.ui.core.css",
                    themeFolder + "/jquery.ui.resizable.css",
                    themeFolder + "/jquery.ui.selectable.css",
                    themeFolder + "/jquery.ui.accordion.css",
                    themeFolder + "/jquery.ui.autocomplete.css",
                    themeFolder + "/jquery.ui.button.css",
                    themeFolder + "/jquery.ui.dialog.css",
                    themeFolder + "/jquery.ui.slider.css",
                    themeFolder + "/jquery.ui.tabs.css",
                    themeFolder + "/jquery.ui.datepicker.css",
                    themeFolder + "/jquery.ui.progressbar.css",
                    themeFolder + "/jquery.ui.theme.css"
                )
             );
            //========================================================================

            //自定义Javascript
            bundles.Add(new ScriptBundle("~/bundles/myjs").Include(
                "~/Scripts/myjs.js"
            ));
        }
    }
}