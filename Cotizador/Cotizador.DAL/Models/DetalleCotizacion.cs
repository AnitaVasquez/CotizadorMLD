//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cotizador.DAL.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DetalleCotizacion
    {
        public long IdDetalleCotizador { get; set; }
        public Nullable<long> IdCotizador { get; set; }
        public Nullable<long> IdProducto { get; set; }
        public Nullable<int> Cantidad { get; set; }
        public Nullable<decimal> ValorUnitario { get; set; }
        public Nullable<decimal> CostoTotal { get; set; }
    }
}
