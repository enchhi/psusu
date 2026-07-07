using System;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator
{
    // Zapis o alarmu koji se desio (istorija aktiviranih alarma) - trece perzistirano stanje.
    public class ActivatedAlarm
    {
        [Key]
        public int Id { get; set; }
        public int AlarmId { get; set; }        // koji alarm se desio
        public string TagName { get; set; }      // naziv velicine
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }  // vreme alarma
    }
}
