﻿using System.Web;
using System.Web.Optimization;
using WebHelpers.Mvc5;

namespace Cotizador.UI
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Bundles/css")
                 .Include("~/Content/css/bootstrap.min.css", new CssRewriteUrlTransformAbsolute())
                 .Include("~/Content/css/bootstrap-select.css")
                 .Include("~/Content/css/bootstrap-datepicker3.min.css")
                 .Include("~/Content/css/font-awesome.min.css", new CssRewriteUrlTransformAbsolute())
                 .Include("~/Content/css/pace/pace.min.css", new CssRewriteUrlTransformAbsolute()) //Animación cargas ajax
                 .Include("~/Content/css/AdminLTE.css", new CssRewriteUrlTransformAbsolute())
                 //.Include("~/Content/css/skins/skin-blue.css")
                 .Include("~/Content/css/skins/skin-asertec.css")
                 );

            bundles.Add(new ScriptBundle("~/Bundles/js")
               .Include("~/Content/js/plugins/jquery/jquery-3.3.1.min.js")
               //.Include("~/Scripts/jquery.easyui.min.js") // easyUI
               .Include("~/Content/js/plugins/bootstrap/bootstrap.js")
               .Include("~/Content/js/plugins/fastclick/fastclick.js")
               .Include("~/Content/js/plugins/slimscroll/jquery.slimscroll.js")
               .Include("~/Content/js/plugins/bootstrap-select/bootstrap-select.js")
               .Include("~/Content/js/plugins/moment/moment.js")
               .Include("~/Content/js/plugins/datepicker/bootstrap-datepicker.js")
               .Include("~/Content/js/plugins/pace/pace.min.js") //Animaciones ajax
               .Include("~/Content/js/plugins/validator/validator.js")
               .Include("~/Content/js/plugins/inputmask/jquery.inputmask.bundle.js")
               .Include("~/Content/js/adminlte.js")
               .Include("~/Content/js/init.js"));


                bundles.Add(new ScriptBundle("~/Bundles/js_login")
               .Include("~/Content/js/plugins/jquery/jquery-3.3.1.js")
               //.Include("~/Scripts/jquery.easyui.min.js") // easyUI
               .Include("~/Content/js/plugins/pace/pace.min.js") //Animaciones ajax
               .Include("~/Content/js/plugins/bootstrap/bootstrap.js")
               .Include("~/Content/js/plugins/fastclick/fastclick.js")
               .Include("~/Content/js/plugins/slimscroll/jquery.slimscroll.js")
               .Include("~/Content/js/plugins/bootstrap-select/bootstrap-select.js")
               .Include("~/Content/js/plugins/moment/moment.js")
               .Include("~/Content/js/plugins/datepicker/bootstrap-datepicker.js")
               //.Include("~/Content/js/plugins/icheck/icheck.js")
               .Include("~/Content/js/plugins/validator/validator.js")
               .Include("~/Content/js/plugins/inputmask/jquery.inputmask.bundle.js")
               //.Include("~/Content/js/adminlte.js")
               //.Include("~/Content/js/init.js")
               //.Include("~/Content/js/init.js")
               );

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
