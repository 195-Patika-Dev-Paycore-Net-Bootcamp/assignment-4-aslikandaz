using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using PayCore_H4.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCore_H4.StartUpExtension
{
    public static class NHibernateExtensions
    {

        public static IServiceCollection AddNHibernatePosgreSql(this IServiceCollection services, string connectionString)
        {
            var mapper = new ModelMapper();
            mapper.AddMappings(typeof(NHibernateExtensions).Assembly.ExportedTypes);
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            var configuration = new Configuration();
            configuration.DataBaseIntegration(c =>
            {
                c.Dialect<PostgreSQLDialect>();
                c.ConnectionString = connectionString;
                c.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                c.SchemaAction = SchemaAutoAction.Create;
                c.LogFormattedSql = true;
                c.LogSqlInConsole = true;
            });
            configuration.AddMapping(domainMapping);

            var sessionFactory = configuration.BuildSessionFactory();

            services.AddSingleton(sessionFactory);
            services.AddScoped(factory => sessionFactory.OpenSession());


            // inject
            services.AddScoped<IMapperSession, MapperSession>();

            return services;
        }
    }
}
