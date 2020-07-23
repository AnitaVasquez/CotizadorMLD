using Cotizador.DAL.Models;
using Cotizador.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cotizador.DAL.Methods
{
    public class ProductosDAL
    {
        //conexion
        private static readonly CotizadorEntities db = new CotizadorEntities();

        public static RespuestaTransaccion CrearProducto(Producto producto)
        {
            try
            {
                //Validar que no exista otro prodcuto con el mismo nombre y que este activo
                var ValidarCatalogosDuplicados = db.ConsultarProducto(null).Where(p => p.NombreProducto.ToUpper() == producto.NombreProducto.ToUpper()).ToList();

                if (ValidarCatalogosDuplicados.Count() > 0)
                {
                    return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeResgistroExistente };
                }
                else
                {
                    //validar datos obligatorios
                    if (producto.NombreProducto is null || producto.PVP is null || producto.Comision is null)
                    {
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeDatosObligatorios };
                    }
                    else
                    {
                        //Colocar en mayusculas el nombre del producto y en activo el producto cuando lo creas
                        producto.NombreProducto = producto.NombreProducto;
                        producto.Estado = true; 

                        db.Producto.Add(producto);
                        db.SaveChanges();

                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa };
                    }
                }
            }
            catch (Exception ex)
            {
                return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
            }
        }

        public static IEnumerable<SelectListItem> ObtenerListadoProductos(string seleccionado = null)
        {
            List<SelectListItem> listadoProductos = new List<SelectListItem>();
            try
            {

                listadoProductos = db.ConsultarProducto(null).OrderBy(c => c.NombreProducto).Select(c => new SelectListItem
                {
                    Text = c.NombreProducto,
                    Value = c.IdProducto.ToString(),
                }).ToList();

                if (!string.IsNullOrEmpty(seleccionado))
                {
                    if (listadoProductos.FirstOrDefault(s => s.Value == seleccionado.ToString()) != null)
                        listadoProductos.FirstOrDefault(s => s.Value == seleccionado.ToString()).Selected = true;
                }

                return listadoProductos;
            }
            catch (Exception ex)
            {
                return listadoProductos;
            }
        }

        public static Producto ConsultarProducto(int id)
        {
            try
            {
                Producto producto = db.Producto.Find(id);
                return producto;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}