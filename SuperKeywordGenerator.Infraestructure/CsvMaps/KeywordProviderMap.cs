using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperKeywordGenerator.Infraestructure.CsvMaps
{
    public class KeywordProviderMap : ClassMap<Domain.KeywordProvider>
    {
        public KeywordProviderMap()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Url).Index(1);

        }
    }
}
