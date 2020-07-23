using Cotizador.DAL.Models;
using Cotizador.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Cotizador.DAL.Methods
{
	public class CotizacionDAL
	{ 

        public static RespuestaTransaccion CrearCotizacion(Cotizacion cabecera, List<DetalleCotizacion> productos)
        {
            using (var context = new CotizadorEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //consultar secuencial
                        var secuencial = context.ConsultarSecuencial().FirstOrDefault().secuencial;
                        //Convertir a 6 digitos

                        string codigoSolicitud = secuencial.ToString().PadLeft(6, '0');

                        //agregar cabecera
                        cabecera.Estado = true;
                        cabecera.Version = 1;
                        cabecera.codigoCotizacion = codigoSolicitud;
                        context.Cotizacion.Add(cabecera);
                        context.SaveChanges();

						//guardar los detalles
                        foreach (var item in productos)
                        {
                            item.IdCotizador = cabecera.IdCotizacion;
                            context.DetalleCotizacion.Add(item);
                            context.SaveChanges();
                        }

                        transaction.Commit();
                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa, EntidadID = cabecera.IdCotizacion };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                    }
                }
            }
        }

        public static RespuestaTransaccion EditCotizacion(Cotizacion cabecera, List<DetalleCotizacion> productos)
        {
            using (var context = new CotizadorEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //consultar 
                        Cotizacion cotizacion = context.Cotizacion.Find(cabecera.IdCotizacion);
                        //agregar cabecera
                        cotizacion.Estado = true;
                        //obtener la version
                        List<Cotizacion> listadoCotizaciones = ListarCotizaciones();
                        int version = listadoCotizaciones.Where(c => c.codigoCotizacion == cotizacion.codigoCotizacion).Count();
                        cotizacion.Version = version+1;
                        cotizacion.TotalCotizacion = cabecera.TotalCotizacion;
                        cotizacion.PorcentajeComision = cabecera.PorcentajeComision;
                        cotizacion.ValorComision = cabecera.ValorComision;
                        cotizacion.SubtotalCotizacion = cabecera.SubtotalCotizacion;
                        context.Cotizacion.Add(cotizacion);
                        context.SaveChanges();

                        //guardar los detalles
                        foreach (var item in productos)
                        {
                            item.IdCotizador = cotizacion.IdCotizacion;
                            context.DetalleCotizacion.Add(item);
                            context.SaveChanges();
                        }

                        transaction.Commit();
                        return new RespuestaTransaccion { Estado = true, Respuesta = Mensajes.MensajeTransaccionExitosa, EntidadID = cotizacion.IdCotizacion };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new RespuestaTransaccion { Estado = false, Respuesta = Mensajes.MensajeTransaccionFallida + " ;" + ex.Message.ToString() };
                    }
                }
            }
        }

        public static List<Cotizacion> ListarCotizaciones()
        {
            try
            {
                using (var context = new CotizadorEntities())
                {
                    return context.Cotizacion.ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static Cotizacion ConsultarCotizacion(int id)
        {
            try
            {
                using (var context = new CotizadorEntities())
                {
                    Cotizacion cotizacion = context.Cotizacion.Find(id);
                    return cotizacion;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static List<ConsultarDetalleCotizacion> ConsultarDetalleCotizacion(int id)
        {
            try
            {
                using (var context = new CotizadorEntities())
                {
                    List<ConsultarDetalleCotizacion> listadoDetalle = context.ConsultarDetalleCotizacion().Where( d => d.IdCotizador == id).ToList();
                    return listadoDetalle;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}