using Lotofacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotofacil.Infra.Data.EntityConfiguration
{
    public class ContestConfiguration : ContestBaseEntityConfiguration<Contest>
    {
        /// <summary>
        /// Aproveitarmos a regra DRY (Don't Repeat Yourself).
        /// Lembrando que utilizar a herança nas entidades já torna mais simples e possui o conceito do DRY.
        /// A expanção foi para o FluentAPI que aplicamos também esse conceito criando um método que aproveita 
        /// automáticamente essas configurações comuns entre as entidades.
        /// </summary>
        /// <param name="builder"></param>
        protected override void AppendConfig(EntityTypeBuilder<Contest> builder)
        {
            builder.ToTable("Contest");

            //Propriedade LastProcessedMainJob
            builder.Property(b => b.LastProcessedMainJob)
                .HasColumnName("LastProcessedMainJob")
                .IsRequired(false);

            // Índice de performance (Etapa 3): Name é usado em buscas de igualdade no ContestValidator.
            builder.HasIndex(b => b.Name);
        }
    }
}
