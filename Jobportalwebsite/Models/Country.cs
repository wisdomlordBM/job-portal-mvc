namespace Jobportalwebsite.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public string Iso3Code { get; set; } = string.Empty;
        public int CurrencyId { get; set; }

        public virtual Currency Currency { get; set; } = null!;
    }
}
