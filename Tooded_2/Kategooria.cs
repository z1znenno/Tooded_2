using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tooded_2
{
    public class Kategooria
    {
        public int Id { get; set; }
        public string? nimetus { get; set; }
        public string? kirjeldus { get; set; }
        public ICollection<Toode> Tooded { get; set; }
    }
}
