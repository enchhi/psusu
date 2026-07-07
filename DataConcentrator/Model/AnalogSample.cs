using System;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator
{
    // Uzorak (ocitavanje) AI vrednosti u vremenu. 4. tabela (uvedena feature-om F4,
    // koji izricito trazi pretragu "iz baze podataka"). Koristi je i F2 (grafik).
    public class AnalogSample
    {
        [Key]
        public int Id { get; set; }
        public string TagName { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
