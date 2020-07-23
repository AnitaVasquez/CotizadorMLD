using Cotizador.DAL.Methods;
using Cotizador.DAL.Models;
using Cotizador.Repository;
using Cotizador.UI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cotizador.UI.Controllers
{
    public class ProductoController : Controller
    {
        // GET: Producto
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _CrearProducto(string codigoCatalogo)
        {
            string titulo = "Agregar Nuevo Material Suelto";
            try
            {
                Producto producto = new Producto();

                ViewBag.TituloModal = string.Format(titulo);
                return PartialView(producto);
            }
            catch (Exception ex)
            {
                ViewBag.TituloModal = string.Format(titulo, ex.Message);
                return PartialView(new Producto());
            }
        }

        [HttpPost]
        public ActionResult CrearProducto(Producto formulario)
        {
            RespuestaTransaccion Resultado = new RespuestaTransaccion();
            Resultado = ProductosDAL.CrearProducto(formulario);
            return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
        }


    }
}