using System;

namespace Forex.Models
{
    public class RateItem
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CurrencyId { get; set; }

        public double Rate { get; set; }

        public DateTime Time { get; set; }
    }
}
