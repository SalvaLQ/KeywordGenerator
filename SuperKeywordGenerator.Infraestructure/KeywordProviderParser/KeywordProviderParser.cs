using CsvHelper;
using CsvHelper.Configuration;
using SuperKeywordGenerator.Domain;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperKeywordGenerator.Infraestructure.KeywordProviderParser
{
    public class KeywordProviderParser
    {
        public Stream SourceFile { get; set; }

        public KeywordProviderParser(Stream sourceFile)
        {
            SourceFile = sourceFile;
        }
        public List<KeywordProvider> GetProviders()
        {
            List<KeywordProvider> Providers;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = false };
            using (var reader = new StreamReader(SourceFile))

            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<CsvMaps.KeywordProviderMap>();
                Providers = csv.GetRecords<Domain.KeywordProvider>().ToList();

            }
            return (Providers);
        }
    }
}
