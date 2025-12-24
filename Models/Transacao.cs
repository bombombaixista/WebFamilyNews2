using System;
using System.ComponentModel.DataAnnotations;

namespace Kanban.Models
{
    public class Transacao
    {
        public int Id { get; set; }

        [Required]
        public string? Tipo { get; set; }

        [Required]
        public string? Descricao { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Now;
    }
}