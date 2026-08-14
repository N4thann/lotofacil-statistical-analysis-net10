using Lotofacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infra.Data.EntityConfiguration
{
    public class ContestActivityLogConfiguration : ContestBaseEntityConfiguration<ContestActivityLog>
    {
        protected override void AppendConfig(EntityTypeBuilder<ContestActivityLog> builder)
        {
            builder.ToTable("ContestActivityLog");

            // Índice de performance (Etapa 3): usado com .Contains() em filtros/exclusão por nome de
            // concurso base — ajuda em igualdade/prefixo, mesmo não cobrindo buscas livres no meio da string.
            builder.HasIndex(c => c.BaseContestName);

            builder.Property(c => c.BaseContestName)
                .HasColumnName("BaseContestName")
                .HasMaxLength(20) 
                .IsRequired();

            // Propriedade ContestNumbers
            builder.Property(c => c.BaseContestNumbers)
                .HasColumnName("BaseContestNumbers")
                .HasMaxLength(45)
                .IsRequired();

            builder.Property(c => c.CreateTime)
                .HasColumnName("CreateTime")
                .IsRequired();

            builder.Property(b => b.CountHits)
                   .HasColumnName("CountHits");
        }
    }
}
/*Essa abordagem foi utilizada para evitar que as entidades ficassem poluídas com 
 * o uso dos Data Annotations, assim fiz essa configuração para definir as propriedades
 * dos atributos das tabelas(Não confundir com as validações no frontend com Fluent Validations)*/
