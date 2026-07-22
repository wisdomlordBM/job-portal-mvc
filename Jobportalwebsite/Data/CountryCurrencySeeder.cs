using System.Globalization;
using Jobportalwebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobportalwebsite.Data
{
    public static class CountryCurrencySeeder
    {
        private sealed record CountryDefinition(string Name, string IsoCode, string Iso3Code, string CurrencyCode, string CurrencyName, string CurrencySymbol);

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Countries.AnyAsync())
            {
                var symbols = GetCountries()
                    .GroupBy(country => country.CurrencyCode)
                    .ToDictionary(group => group.Key, group => group.First().CurrencySymbol);

                foreach (var currency in await context.Currencies
                             .Where(currency => string.IsNullOrEmpty(currency.Symbol))
                             .ToListAsync())
                {
                    if (symbols.TryGetValue(currency.Code, out var symbol))
                    {
                        currency.Symbol = symbol;
                    }
                }

                await context.SaveChangesAsync();
                return;
            }

            var countries = GetCountries();

            // Create unique currencies (DO NOT assign Id)
            var currencies = countries
                .GroupBy(c => c.CurrencyCode)
                .Select(g => g.First())
                .OrderBy(c => c.CurrencyCode)
                .Select(c => new Currency
                {
                    Code = c.CurrencyCode,
                    Name = c.CurrencyName,
                    Symbol = c.CurrencySymbol
                })
                .ToList();

            context.Currencies.AddRange(currencies);
            await context.SaveChangesAsync();

            // Get the generated IDs
            var currencyIds = await context.Currencies
                .ToDictionaryAsync(c => c.Code, c => c.Id);

            // Create countries using the generated CurrencyId
            var countryEntities = countries
                .OrderBy(c => c.IsoCode)
                .Select(c => new Country
                {
                    Name = c.Name,
                    IsoCode = c.IsoCode,
                    Iso3Code = c.Iso3Code,
                    CurrencyId = currencyIds[c.CurrencyCode]
                })
                .ToList();

            context.Countries.AddRange(countryEntities);
            await context.SaveChangesAsync();
        }

        private static List<CountryDefinition> GetCountries()
        {
            var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture =>
                {
                    try { return new RegionInfo(culture.Name); }
                    catch (ArgumentException) { return null; }
                })
                .Where(region => region is not null && region.TwoLetterISORegionName.Length == 2)
                .GroupBy(region => region!.TwoLetterISORegionName)
                .Select(group => group.First()!)
                .Where(region => region.TwoLetterISORegionName != "XK")
                .Select(region => new CountryDefinition(
                    region.EnglishName,
                    region.TwoLetterISORegionName,
                    region.ThreeLetterISORegionName,
                    region.ISOCurrencySymbol,
                    region.CurrencyEnglishName,
                    region.CurrencySymbol))
                .ToList();

            // ISO 3166-1 entries without a corresponding specific .NET culture.
            countries.AddRange(new[]
            {
                new CountryDefinition("Antarctica", "AQ", "ATA", "XXX", "No currency", "¤"),
                new CountryDefinition("Bouvet Island", "BV", "BVT", "NOK", "Norwegian Krone", "kr"),
                new CountryDefinition("Western Sahara", "EH", "ESH", "MAD", "Moroccan Dirham", "MAD"),
                new CountryDefinition("South Georgia and the South Sandwich Islands", "GS", "SGS", "GBP", "British Pound", "£"),
                new CountryDefinition("Heard Island and McDonald Islands", "HM", "HMD", "AUD", "Australian Dollar", "$") ,
                new CountryDefinition("French Southern Territories", "TF", "ATF", "EUR", "Euro", "€")
            });

            return countries;
        }
    }
}
