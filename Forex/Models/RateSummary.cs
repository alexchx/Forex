using System;

namespace Forex.Models
{
    public class RateSummary
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CurrencyId { get; set; }

        public double MinRate { get; set; }

        public double MaxRate { get; set; }

        public DateTime Date { get; set; }
    }
}
