using Cotizador.DAL.Models;
using Cotizador.DAL.Methods;
using Cotizador.Repository; 
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Threading.Tasks; 
using System.IO; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Net;
using System.Configuration;

namespace Cotizador.UI.Controllers
{
    public class CotizadorController : Controller
    {
        // GET: Cotizador
        public ActionResult Index()
        {
            return View();  
        }

        [HttpGet]
        public async Task<PartialViewResult> IndexGrid(String search)
        {
            ViewBag.NombreListado = Etiquetas.TituloGridCodigoCotizacion; 

            //Búsqueda
            var listado = CotizacionDAL.ListarCotizaciones();

            search = !string.IsNullOrEmpty(search) ? search.Trim() : "";

            if (!string.IsNullOrEmpty(search))//filter
            {
                var type = listado.GetType().GetGenericArguments()[0];
                var properties = type.GetProperties();

                listado = listado.Where(x => properties
                            .Any(p =>
                            {
                                var value = p.GetValue(x);
                                return value != null && value.ToString().ToLower().Contains(search.ToLower());
                            })).ToList();
            }

            // Only grid query values will be available here.
            return PartialView("_IndexGrid", await Task.Run(() => listado));
        }

        // GET: Cotizador/Create
        public ActionResult Create()
        {
            ViewBag.TituloPanel = Etiquetas.TituloPanelFormularioFichaIngreso; 

            try
            {
                Cotizacion model = new Cotizacion();
                model.FechaCotizacion = DateTime.Now;
                model.PorcentajeComision = 10;
                return View(model);
            }
            catch (Exception ex)
            {
                return View(new Cotizacion());
            }
        }

        // POST: Cotizador/Create
        [HttpPost]
        public ActionResult Create(Cotizacion cotizacion, List<DetalleCotizacion> productos)
        {
            try
            { 
                //Guardar la Cotizacion 

                RespuestaTransaccion Resultado = new RespuestaTransaccion();
                Resultado = CotizacionDAL.CrearCotizacion(cotizacion, productos);
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return View();
            }
        }

        // GET: Cotizador/Edit/5
        public ActionResult Edit(int? id)
        {

            Cotizacion cabecera = new Cotizacion();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.VieneSolicitud = false;

            cabecera = CotizacionDAL.ConsultarCotizacion(id.Value);

            ViewBag.detalleCotizacion = CotizacionDAL.ConsultarDetalleCotizacion(id.Value);

            if (cabecera == null)
            {
                return HttpNotFound();
            }
            return View(cabecera);
        }

        // POST: Cotizador/Edit/5
        [HttpPost]
        public ActionResult Edit(Cotizacion cotizacion, List<DetalleCotizacion> productos)
        {
            try
            {
                //Guardar nueva version del Cotizacion 
                RespuestaTransaccion Resultado = new RespuestaTransaccion();
                Resultado = CotizacionDAL.EditCotizacion(cotizacion, productos);
                return Json(new { Resultado }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult GetProductos(string query, string codigo)
        {
            query = !string.IsNullOrEmpty(query) ? query.ToLower().Trim() : string.Empty;

            var data = ProductosDAL.ObtenerListadoProductos()
            //if "query" is null, get all records
            .Where(m => string.IsNullOrEmpty(query) || m.Text.ToLower().Contains(query))
            .OrderBy(m => m.Text);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetValoresProducto(int id)
        {
            var data = ProductosDAL.ConsultarProducto(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #region Generar PDF Cotización
        public void CotizadorPDF(int? id)
        {
            //colores por filas pares
            int par = 0;

            using (var context = new CotizadorEntities())
            {

                //Cabecera 
                Cotizacion cabeceraCotizacion = context.Cotizacion.Find(id);

                //Detalle
                List<ConsultarDetalleCotizacion> detalleCotizacion = context.ConsultarDetalleCotizacion().Where(d => d.IdCotizador == id).ToList();

                //Fonts para el PDF
                //Regular
                string fontpath = Server.MapPath("~/Content/fonts/ubuntu/Raleway-Regular.ttf");
                BaseFont customfont = BaseFont.CreateFont(fontpath, BaseFont.CP1252, BaseFont.EMBEDDED);

                //Bold
                string fontpathbold = Server.MapPath("~/Content/fonts/ubuntu/Raleway-Bold.ttf");
                BaseFont customfontbold = BaseFont.CreateFont(fontpathbold, BaseFont.CP1252, BaseFont.EMBEDDED);

                //ruta imagen del formulario
                String FilePath = Server.MapPath("~/Content/img/LOGOMLD.png");
                var physicalPath = Server.MapPath("~/Content/img/LOGOMLD.png");

                string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
                string rutaArchivos = basePath + "\\COTIZADOR";

                //ruta del archivo PDF que se va a crear
                string rutaDocumentos = rutaArchivos + "\\Cotización-" + cabeceraCotizacion.codigoCotizacion + "-Versión" + cabeceraCotizacion.Version.ToString().Replace(",", ".") + ".pdf";

                //Crear el formulario PDF
                FileStream fs = new FileStream(rutaDocumentos, FileMode.Create);
                Document document = new Document(iTextSharp.text.PageSize.A4, -20, -20, 0, 0);
                //Document document = new Document(iTextSharp.text.PageSize.A4);
                PdfWriter pw = PdfWriter.GetInstance(document, fs);

                //Abrir archivo para su edicion
                document.Open();

                //Salto de linea 
                document.Add(new Paragraph(" "));

                //Salto de linea 
                document.Add(new Paragraph(" "));

                //Cabecera del Cotizador         
                PdfPTable cabecera = new PdfPTable(7);
                cabecera.DefaultCell.Border = Rectangle.NO_BORDER;

                Image instance = Image.GetInstance(physicalPath);
                instance.BorderWidth = 0.0f;
                instance.Alignment = 1;
                instance.ScalePercent(10f); 

                //Titulo
                PdfPCell TituloCotizador = new PdfPCell(new Phrase("DEPARTAMENTO DE PRODUCCIÓN AUDIOVISUAL  -  COTIZACIÓN N° " + cabeceraCotizacion.codigoCotizacion + "      ", new Font(customfontbold, 9, 0, new BaseColor(255, 255, 255))));
                TituloCotizador.Rowspan = 2;
                TituloCotizador.Colspan = 5;
                TituloCotizador.Border = Rectangle.NO_BORDER;
                TituloCotizador.HorizontalAlignment = Element.ALIGN_RIGHT;
                TituloCotizador.VerticalAlignment = Element.ALIGN_MIDDLE;
                TituloCotizador.BackgroundColor = new BaseColor(60, 66, 82);
                TituloCotizador.FixedHeight = 50f; 

                //Version
                PdfPCell VersionCotizador = new PdfPCell(new Phrase("Versión  " + cabeceraCotizacion.Version.ToString().Replace(",", "."), new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                VersionCotizador.Rowspan = 2;
                VersionCotizador.Colspan = 1;
                VersionCotizador.Border = Rectangle.NO_BORDER;
                VersionCotizador.HorizontalAlignment = Element.ALIGN_CENTER;
                VersionCotizador.VerticalAlignment = Element.ALIGN_MIDDLE;
                VersionCotizador.BackgroundColor = new BaseColor(225, 225, 225);
                VersionCotizador.FixedHeight = 50f;

                cabecera.AddCell(instance);
                cabecera.AddCell(TituloCotizador);
                cabecera.AddCell(VersionCotizador);

                document.Add(cabecera);

                //Salto Linea      
                PdfPTable saltoLinea1 = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea.Colspan = 4;
                EtiquetaSaltoLinea.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea.FixedHeight = 8f;

                saltoLinea1.AddCell(EtiquetaSaltoLinea);

                document.Add(saltoLinea1);

                //Informacion del Cotizador         
                PdfPTable informacion = new PdfPTable(6);

                PdfPCell EtiquetaFechaCotizacion = new PdfPCell(new Phrase("Fecha de cotización: ", new Font(customfontbold, 7)));
                EtiquetaFechaCotizacion.Colspan = 1;
                EtiquetaFechaCotizacion.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetaFechaCotizacion.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaFechaCotizacion.FixedHeight = 23f;
                EtiquetaFechaCotizacion.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetaFechaCotizacion.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorFechaCotizacion = new PdfPCell(new Phrase("   " + String.Format("{0:D}", cabeceraCotizacion.FechaCotizacion), new Font(customfont, 7)));
                ValorFechaCotizacion.Colspan = 5;
                ValorFechaCotizacion.HorizontalAlignment = Element.ALIGN_LEFT;
                ValorFechaCotizacion.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorFechaCotizacion.FixedHeight = 23f;
                ValorFechaCotizacion.BorderColor = new BaseColor(60, 66, 82);                 
                
                informacion.AddCell(EtiquetaFechaCotizacion);
                informacion.AddCell(ValorFechaCotizacion); 

                document.Add(informacion);

                //Salto Linea       
                PdfPTable saltoLinea2 = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea2 = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea2.Colspan = 4;
                EtiquetaSaltoLinea2.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea2.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea2.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea2.FixedHeight = 8f;

                saltoLinea2.AddCell(EtiquetaSaltoLinea2);

                document.Add(saltoLinea2);

                //Informacion del Cotizador         
                PdfPTable cliente = new PdfPTable(6);

                PdfPCell EtiquetaCliente = new PdfPCell(new Phrase("Cliente: ", new Font(customfontbold, 7)));
                EtiquetaCliente.Colspan = 1;
                EtiquetaCliente.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetaCliente.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaCliente.FixedHeight = 23f;
                EtiquetaCliente.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetaCliente.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorCliente = new PdfPCell(new Phrase("   " + cabeceraCotizacion.Cliente, new Font(customfont, 7)));
                ValorCliente.Colspan = 5;
                ValorCliente.HorizontalAlignment = Element.ALIGN_LEFT;
                ValorCliente.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorCliente.FixedHeight = 23f;
                ValorCliente.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetaContacto = new PdfPCell(new Phrase("Producto: ", new Font(customfontbold, 7)));
                EtiquetaContacto.Colspan = 1;
                EtiquetaContacto.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetaContacto.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaContacto.FixedHeight = 23f;
                EtiquetaContacto.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetaContacto.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorContacto = new PdfPCell(new Phrase("   " + cabeceraCotizacion.Producto, new Font(customfont, 7)));
                ValorContacto.Colspan = 5;
                ValorContacto.HorizontalAlignment = Element.ALIGN_LEFT;
                ValorContacto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorContacto.FixedHeight = 23f;
                ValorContacto.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetaNombreProyecto = new PdfPCell(new Phrase("Concepto: ", new Font(customfontbold, 7)));
                EtiquetaNombreProyecto.Colspan = 1;
                EtiquetaNombreProyecto.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetaNombreProyecto.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaNombreProyecto.FixedHeight = 23f;
                EtiquetaNombreProyecto.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetaNombreProyecto.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorNombreProyecto = new PdfPCell(new Phrase("   " + cabeceraCotizacion.Concepto, new Font(customfont, 7)));
                ValorNombreProyecto.Colspan = 5;
                ValorNombreProyecto.HorizontalAlignment = Element.ALIGN_LEFT;
                ValorNombreProyecto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorNombreProyecto.FixedHeight = 23f;
                ValorNombreProyecto.BorderColor = new BaseColor(60, 66, 82); 

                cliente.AddCell(EtiquetaCliente);
                cliente.AddCell(ValorCliente);

                cliente.AddCell(EtiquetaContacto);
                cliente.AddCell(ValorContacto);

                cliente.AddCell(EtiquetaNombreProyecto);
                cliente.AddCell(ValorNombreProyecto); 

                document.Add(cliente);

                //Salto Linea       
                PdfPTable saltoLinea3 = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea3 = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea3.Colspan = 4;
                EtiquetaSaltoLinea3.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea3.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea3.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea3.FixedHeight = 12f;

                saltoLinea3.AddCell(EtiquetaSaltoLinea3);

                document.Add(saltoLinea3);

                //Entregables del Cotizador         
                PdfPTable entregables = new PdfPTable(7);

                PdfPCell EtiquetaEntregables = new PdfPCell(new Phrase("DESCRIPCIÓN", new Font(customfont, 9, 0, new BaseColor(255, 255, 255))));
                EtiquetaEntregables.Colspan = 4;
                EtiquetaEntregables.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaEntregables.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaEntregables.FixedHeight = 23f;
                EtiquetaEntregables.BackgroundColor = new BaseColor(60, 66, 82);
                EtiquetaEntregables.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetaCantidad = new PdfPCell(new Phrase("CANTIDAD", new Font(customfont, 9, 0, new BaseColor(255, 255, 255))));
                EtiquetaCantidad.Colspan = 1;
                EtiquetaCantidad.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaCantidad.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaCantidad.FixedHeight = 23f;
                EtiquetaCantidad.BackgroundColor = new BaseColor(60, 66, 82);
                EtiquetaCantidad.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetaPrecioUnitario = new PdfPCell(new Phrase("PRECIO UNIT.", new Font(customfont, 9, 0, new BaseColor(255, 255, 255))));
                EtiquetaPrecioUnitario.Colspan = 1;
                EtiquetaPrecioUnitario.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaPrecioUnitario.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaPrecioUnitario.FixedHeight = 23f;
                EtiquetaPrecioUnitario.BackgroundColor = new BaseColor(60, 66, 82);
                EtiquetaPrecioUnitario.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetaCostoTotal = new PdfPCell(new Phrase("COSTO TOTAL", new Font(customfont, 9, 0, new BaseColor(255, 255, 255))));
                EtiquetaCostoTotal.Colspan = 1;
                EtiquetaCostoTotal.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaCostoTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaCostoTotal.FixedHeight = 23f;
                EtiquetaCostoTotal.BackgroundColor = new BaseColor(60, 66, 82);
                EtiquetaCostoTotal.BorderColor = new BaseColor(60, 66, 82);

                entregables.AddCell(EtiquetaEntregables);
                entregables.AddCell(EtiquetaCantidad);
                entregables.AddCell(EtiquetaPrecioUnitario);
                entregables.AddCell(EtiquetaCostoTotal);


                foreach (ConsultarDetalleCotizacion dtc in detalleCotizacion)
                {
                    PdfPCell EtiquetaDetalle = new PdfPCell(new Phrase("   " + dtc.NombreProducto, new Font(customfont, 7)));
                    EtiquetaDetalle.Colspan = 4;
                    EtiquetaDetalle.HorizontalAlignment = Element.ALIGN_LEFT;
                    EtiquetaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                    EtiquetaDetalle.FixedHeight = 23f;
                    EtiquetaDetalle.BorderColor = new BaseColor(60, 66, 82);


                    PdfPCell Cantidad = new PdfPCell(new Phrase("" + dtc.Cantidad, new Font(customfont, 7)));
                    Cantidad.Colspan = 1;
                    Cantidad.HorizontalAlignment = Element.ALIGN_CENTER;
                    Cantidad.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cantidad.FixedHeight = 23f;
                    Cantidad.BorderColor = new BaseColor(60, 66, 82);
                     
                    PdfPCell ValorUnitario = new PdfPCell(new Phrase((((String.Format("{0:n}", dtc.ValorUnitario).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfont, 7)));
                    ValorUnitario.Colspan = 1;
                    ValorUnitario.HorizontalAlignment = Element.ALIGN_RIGHT;
                    ValorUnitario.VerticalAlignment = Element.ALIGN_MIDDLE;
                    ValorUnitario.FixedHeight = 23f;
                    ValorUnitario.BorderColor = new BaseColor(60, 66, 82);

                    PdfPCell ValorTotal = new PdfPCell(new Phrase((((String.Format("{0:n}", dtc.CostoTotal).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfont, 7)));
                    ValorTotal.Colspan = 1;
                    ValorTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    ValorTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    ValorTotal.FixedHeight = 23f;
                    ValorTotal.BorderColor = new BaseColor(60, 66, 82);

                    if (par == 1 || par == 3 || par == 5 || par == 7 || par == 9 || par == 11 || par == 13 || par == 15 || par == 17)
                    {
                        EtiquetaDetalle.BackgroundColor = new BaseColor(225, 225, 225);
                        Cantidad.BackgroundColor = new BaseColor(225, 225, 225);
                        ValorUnitario.BackgroundColor = new BaseColor(225, 225, 225);
                        ValorTotal.BackgroundColor = new BaseColor(225, 225, 225);
                    }

                    entregables.AddCell(EtiquetaDetalle);
                    entregables.AddCell(Cantidad);
                    entregables.AddCell(ValorUnitario);
                    entregables.AddCell(ValorTotal);

                    par = par + 1;
                }

                int registros = 17;

                for (int i = par; i <= registros; i++)
                {
                    PdfPCell EtiquetaDetalle = new PdfPCell(new Phrase("   ", new Font(customfont, 7)));
                    EtiquetaDetalle.Colspan = 4;
                    EtiquetaDetalle.HorizontalAlignment = Element.ALIGN_LEFT;
                    EtiquetaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                    EtiquetaDetalle.FixedHeight = 23f;
                    EtiquetaDetalle.BorderColor = new BaseColor(60, 66, 82);

                    PdfPCell Cantidad = new PdfPCell(new Phrase("        ", new Font(customfont, 7)));
                    Cantidad.Colspan = 1;
                    Cantidad.HorizontalAlignment = Element.ALIGN_CENTER;
                    Cantidad.VerticalAlignment = Element.ALIGN_MIDDLE;
                    Cantidad.FixedHeight = 23f;
                    Cantidad.BorderColor = new BaseColor(60, 66, 82);
                     
                    PdfPCell ValorUnitario = new PdfPCell(new Phrase("        ", new Font(customfont, 7)));
                    ValorUnitario.Colspan = 1;
                    ValorUnitario.HorizontalAlignment = Element.ALIGN_RIGHT;
                    ValorUnitario.VerticalAlignment = Element.ALIGN_MIDDLE;
                    ValorUnitario.FixedHeight = 23f;
                    ValorUnitario.BorderColor = new BaseColor(60, 66, 82);

                    PdfPCell ValorTotal = new PdfPCell(new Phrase("        ", new Font(customfont, 7)));
                    ValorTotal.Colspan = 1;
                    ValorTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    ValorTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    ValorTotal.FixedHeight = 23f;
                    ValorTotal.BorderColor = new BaseColor(60, 66, 82);

                    if (par == 1 || par == 3 || par == 5 || par == 7 || par == 9 || par == 11 || par == 13 || par == 15 || par == 17)
                    {
                        EtiquetaDetalle.BackgroundColor = new BaseColor(225, 225, 225);
                        Cantidad.BackgroundColor = new BaseColor(225, 225, 225);
                        ValorUnitario.BackgroundColor = new BaseColor(225, 225, 225);
                        ValorTotal.BackgroundColor = new BaseColor(225, 225, 225);
                    }

                    entregables.AddCell(EtiquetaDetalle);
                    entregables.AddCell(Cantidad);
                    entregables.AddCell(ValorUnitario);
                    entregables.AddCell(ValorTotal);

                    par = par + 1;
                }

                document.Add(entregables);

                //Salto Linea       
                PdfPTable saltoLinea4 = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea4 = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea4.Colspan = 4;
                EtiquetaSaltoLinea4.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea4.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea4.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea4.FixedHeight = 8f;

                saltoLinea4.AddCell(EtiquetaSaltoLinea3);

                document.Add(saltoLinea4);

                //Datos Finales        
                PdfPTable finales = new PdfPTable(7);

                PdfPCell EtiquetalObservaciones = new PdfPCell(new Phrase("Observaciones: ", new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                EtiquetalObservaciones.Colspan = 1;
                EtiquetalObservaciones.Rowspan = 4;
                EtiquetalObservaciones.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetalObservaciones.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetalObservaciones.FixedHeight = 23f;
                EtiquetalObservaciones.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetalObservaciones.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorObservaciones = new PdfPCell(new Phrase("N/A", new Font(customfont, 7)));
                ValorObservaciones.Colspan = 4;
                ValorObservaciones.Rowspan = 4;
                ValorObservaciones.HorizontalAlignment = Element.ALIGN_CENTER;
                ValorObservaciones.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorObservaciones.FixedHeight = 23f;
                ValorObservaciones.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetalSubtotal = new PdfPCell(new Phrase("Subtotal", new Font(customfont, 7)));
                EtiquetalSubtotal.Colspan = 1;
                EtiquetalSubtotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetalSubtotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetalSubtotal.FixedHeight = 23f;
                EtiquetalSubtotal.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetalSubtotal.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorSubtotal = new PdfPCell(new Phrase("US $ " + (((String.Format("{0:n}", cabeceraCotizacion.SubtotalCotizacion).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfont, 7)));
                ValorSubtotal.Colspan = 1;
                ValorSubtotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                ValorSubtotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorSubtotal.FixedHeight = 23f;
                ValorSubtotal.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetalDescuento = new PdfPCell(new Phrase(((cabeceraCotizacion.PorcentajeComision.ToString().Replace(".", ","))) + " % Comisión", new Font(customfont, 7)));
                EtiquetalDescuento.Colspan = 1;
                EtiquetalDescuento.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetalDescuento.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetalDescuento.FixedHeight = 23f;
                EtiquetalDescuento.BackgroundColor = new BaseColor(225, 225, 225);
                EtiquetalDescuento.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorDescuento = new PdfPCell(new Phrase("US $ " + (((String.Format("{0:n}", cabeceraCotizacion.ValorComision).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfont, 7)));
                ValorDescuento.Colspan = 1;
                ValorDescuento.HorizontalAlignment = Element.ALIGN_RIGHT;
                ValorDescuento.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorDescuento.FixedHeight = 23f;
                ValorDescuento.BorderColor = new BaseColor(60, 66, 82);

                //PdfPCell EtiquetalIva = new PdfPCell(new Phrase(((cabeceraCotizacion.PorcentajeIva.ToString().Replace(".", ","))) + " % IVA", new Font(customfont, 7)));
                //EtiquetalIva.Colspan = 1;
                //EtiquetalIva.HorizontalAlignment = Element.ALIGN_RIGHT;
                //EtiquetalIva.VerticalAlignment = Element.ALIGN_MIDDLE;
                //EtiquetalIva.FixedHeight = 16f;
                //EtiquetalIva.BackgroundColor = new BaseColor(225, 225, 225);
                //EtiquetalIva.BorderColor = new BaseColor(60, 66, 82);

                //PdfPCell ValorIVA = new PdfPCell(new Phrase("US $ " + (((String.Format("{0:n}", cabeceraCotizacion.ValorIva).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfont, 7)));
                //ValorIVA.Colspan = 1;
                //ValorIVA.HorizontalAlignment = Element.ALIGN_RIGHT;
                //ValorIVA.VerticalAlignment = Element.ALIGN_MIDDLE;
                //ValorIVA.FixedHeight = 16f;
                //ValorIVA.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell EtiquetalTotal = new PdfPCell(new Phrase("TOTAL", new Font(customfontbold, 7, 0, new BaseColor(255, 255, 255))));
                EtiquetalTotal.Colspan = 1;
                EtiquetalTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                EtiquetalTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetalTotal.FixedHeight = 23f;
                EtiquetalTotal.BackgroundColor = new BaseColor(60, 66, 82);
                EtiquetalTotal.BorderColor = new BaseColor(60, 66, 82);

                PdfPCell ValorTotalCotizacion = new PdfPCell(new Phrase("US $ " + (((String.Format("{0:n}", cabeceraCotizacion.TotalCotizacion).Replace(",", "-")).Replace(".", ",")).Replace("-", ".")), new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                ValorTotalCotizacion.Colspan = 1;
                ValorTotalCotizacion.HorizontalAlignment = Element.ALIGN_RIGHT;
                ValorTotalCotizacion.VerticalAlignment = Element.ALIGN_MIDDLE;
                ValorTotalCotizacion.FixedHeight = 23f;
                ValorTotalCotizacion.BackgroundColor = new BaseColor(225, 225, 225);
                ValorTotalCotizacion.BorderColor = new BaseColor(60, 66, 82);

                finales.AddCell(EtiquetalObservaciones);
                finales.AddCell(ValorObservaciones);
                finales.AddCell(EtiquetalSubtotal);
                finales.AddCell(ValorSubtotal);

                finales.AddCell(EtiquetalDescuento);
                finales.AddCell(ValorDescuento);

                //finales.AddCell(EtiquetalIva);
                //finales.AddCell(ValorIVA);

                finales.AddCell(EtiquetalTotal);
                finales.AddCell(ValorTotalCotizacion);

                document.Add(finales);

                //Salto Linea       
                PdfPTable saltoLinea5 = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea5 = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea5.Colspan = 4;
                EtiquetaSaltoLinea5.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea5.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea5.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea5.FixedHeight = 12f;

                saltoLinea5.AddCell(EtiquetaSaltoLinea5);

                document.Add(saltoLinea5);

                //Datos Finales        
                PdfPTable terminosCondiciones = new PdfPTable(1);

                PdfPCell EtiquetaTerminos = new PdfPCell(new Phrase("FORMA DE PAGO:     100% DE ACUERDO AL CONTRATO", new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                EtiquetaTerminos.Colspan = 1;
                EtiquetaTerminos.HorizontalAlignment = Element.ALIGN_LEFT;
                EtiquetaTerminos.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaTerminos.FixedHeight = 19f; 
                EtiquetaTerminos.BorderColor = new BaseColor(255, 255, 255);

                PdfPCell EtiquetaTerminos2 = new PdfPCell(new Phrase("TIEMPO DE ENTREGA:     A COORDINAR CON EL PROVEEDOR", new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                EtiquetaTerminos2.Colspan = 1;
                EtiquetaTerminos2.HorizontalAlignment = Element.ALIGN_LEFT;
                EtiquetaTerminos2.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaTerminos2.FixedHeight = 19f; 
                EtiquetaTerminos2.BorderColor = new BaseColor(255, 255, 255);

                PdfPCell EtiquetaTerminos3 = new PdfPCell(new Phrase("*Costos sujetos a cambios", new Font(customfontbold, 7, 0, new BaseColor(60, 66, 82))));
                EtiquetaTerminos3.Colspan = 1;
                EtiquetaTerminos3.HorizontalAlignment = Element.ALIGN_LEFT;
                EtiquetaTerminos3.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaTerminos3.FixedHeight = 19f; 
                EtiquetaTerminos3.BorderColor = new BaseColor(255, 255, 255);

                terminosCondiciones.AddCell(EtiquetaTerminos);
                terminosCondiciones.AddCell(EtiquetaTerminos2);
                terminosCondiciones.AddCell(EtiquetaTerminos3);

                document.Add(terminosCondiciones); 
                //Salto Linea      
                PdfPTable saltoLinea = new PdfPTable(4);

                PdfPCell EtiquetaSaltoLinea0 = new PdfPCell(new Phrase(" ", new Font(customfontbold, 9)));
                EtiquetaSaltoLinea0.Colspan = 4;
                EtiquetaSaltoLinea0.Border = Rectangle.NO_BORDER;
                EtiquetaSaltoLinea0.HorizontalAlignment = Element.ALIGN_CENTER;
                EtiquetaSaltoLinea0.VerticalAlignment = Element.ALIGN_MIDDLE;
                EtiquetaSaltoLinea0.FixedHeight = 8f;

                saltoLinea.AddCell(EtiquetaSaltoLinea0);

                document.Add(saltoLinea);

                //Head      
                PdfPTable head = new PdfPTable(7);
                head.DefaultCell.Border = Rectangle.NO_BORDER;

                //Titulo
                PdfPCell pagina = new PdfPCell(new Phrase("1 de 1", new Font(customfont, 5, 0, new BaseColor(60, 66, 82))));
                pagina.Colspan = 1;
                pagina.Border = Rectangle.NO_BORDER;
                pagina.HorizontalAlignment = Element.ALIGN_LEFT;
                pagina.VerticalAlignment = Element.ALIGN_MIDDLE;

                //Titulo
                PdfPCell head1 = new PdfPCell(new Phrase("GCM-GCO-PRO-001-FOR-004", new Font(customfont, 5, 0, new BaseColor(60, 66, 82))));
                head1.Colspan = 5;
                head1.Border = Rectangle.NO_BORDER;
                head1.HorizontalAlignment = Element.ALIGN_CENTER;
                head1.VerticalAlignment = Element.ALIGN_MIDDLE;

                //Titulo
                PdfPCell head2 = new PdfPCell(new Phrase("Versión 1.0", new Font(customfont, 5, 0, new BaseColor(60, 66, 82))));
                head2.Border = Rectangle.NO_BORDER;
                head2.HorizontalAlignment = Element.ALIGN_RIGHT;
                head2.VerticalAlignment = Element.ALIGN_MIDDLE;

                head.AddCell(pagina);
                head.AddCell(head1);
                head.AddCell(head2);

                //document.Add(head);

                //Cerrar Documento
                document.Close();

            }
        }
        #endregion

        public ActionResult GeneraCotizadorPDF(int id)
        {
            CotizadorPDF(id);

            //Cabecera 
            var cabecera = CotizacionDAL.ConsultarCotizacion(id);

            string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
            string rutaArchivos = basePath + "\\COTIZADOR";

            //ruta del archivo PDF que se va a crear
            string rutaDocumentos = rutaArchivos + "\\Cotización-" + cabecera.codigoCotizacion + "-Versión" + cabecera.Version.ToString().Replace(",", ".") + ".pdf";

            var relativePath = "";
            var filename = "";

            filename = "Cotización-" + cabecera.codigoCotizacion + "-Versión" + cabecera.Version.ToString().Replace(",", ".") + ".pdf";
            relativePath = CrearCaminos(basePath, new List<string>() { "COTIZADOR" });
            if (!relativePath.EndsWith("\\"))
                relativePath += "\\";
            relativePath = relativePath + "\\" + filename;

            return File(relativePath, GetContentType(Path.GetExtension(filename)), filename);

        }
        public ActionResult DescargarPDF(int id)
        {
            var cabecera = CotizacionDAL.ConsultarCotizacion(id);

            string basePath = ConfigurationManager.AppSettings["RepositorioDocumentos"];
            string rutaArchivos = basePath + "\\COTIZADOR";

            //ruta del archivo PDF que se va a crear
            string rutaDocumentos = rutaArchivos + "\\Cotización-" + cabecera.codigoCotizacion + "-Versión" + cabecera.Version.ToString().Replace(",", ".") + ".pdf";

            var relativePath = "";
            var filename = "";

            filename = "Cotización-" + cabecera.codigoCotizacion + "-Versión" + cabecera.Version.ToString().Replace(",", ".") + ".pdf";
            relativePath = CrearCaminos(basePath, new List<string>() { "COTIZADOR" });
            if (!relativePath.EndsWith("\\"))
                relativePath += "\\";
            relativePath = relativePath + "\\" + filename;

            return File(relativePath, GetContentType(Path.GetExtension(filename)), filename);

        }
        public static string CrearCaminos(string carpetaInicial, List<string> carpetas)
        {
            string camino = carpetaInicial;

            foreach (string ele in carpetas)
            {
                if (!Directory.Exists(Path.Combine(camino, ele)))
                {
                    Directory.CreateDirectory(Path.Combine(camino, ele));
                }
                camino = Path.Combine(camino, ele);
            }

            return camino;

        }

        public static string GetContentType(string extension)
        {
            var contentType = "";

            switch (extension.ToLower())
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/doc";
                    break;
                case ".docx":
                    contentType = "application/doc";
                    break;
                case ".txt":
                    contentType = "application/txt";
                    break;
                case ".xls":
                    contentType = "application/xls";
                    //Response.ContentType = "application/ms-excel";
                    break;
                case ".xlsx":
                    contentType = "application/xls";
                    //Response.ContentType = "application/ms-excel";
                    break;
                case ".log":
                    contentType = "application/txt";
                    break;
                case ".rar":
                    contentType = "application/rar";
                    break;
                case ".7z":
                    contentType = "application/7z";
                    break;
                case ".jpg":
                    contentType = "application/jpg";
                    break;
                case ".bmp":
                    contentType = "application/bmp";
                    break;
                case ".png":
                    contentType = "application/png";
                    break;
            }

            return contentType;
        }




    }
}
