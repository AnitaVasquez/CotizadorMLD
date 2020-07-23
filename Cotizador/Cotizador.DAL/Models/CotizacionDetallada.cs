using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cotizador.DAL.Models
{
    public class CotizacionDetallada
    {
        public Cotizacion CotizacionGeneral { get; set; }
        public List<DetalleCotizacion> DetalleCotizacion { get; set; }


        public CotizacionDetallada()
        {
            CotizacionGeneral = new Cotizacion();
            DetalleCotizacion = new List<DetalleCotizacion>();
        }

        public CotizacionDetallada(Cotizacion _CotizacionGeneral, List<DetalleCotizacion> _DetalleCotizacion)
        {
            CotizacionGeneral = _CotizacionGeneral;
            DetalleCotizacion = _DetalleCotizacion;
        }
    }
}