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
    
    public partial class ConsultarProducto
    {
        public long IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public Nullable<decimal> PVP { get; set; }
        public Nullable<decimal> Comision { get; set; }
        public Nullable<bool> Estado { get; set; }
    }
}
