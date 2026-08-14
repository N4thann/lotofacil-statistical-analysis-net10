using Lotofacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infra.Data.EntityConfiguration
{
    public class BaseContestConfiguration : ContestBaseEntityConfiguration<BaseContest>
    {
        protected override void AppendConfig(EntityTypeBuilder<BaseContest> builder)
        {
            builder.ToTable("BaseContest");

            // Índices de performance (Etapa 3): Name/Data usados em filtros e ordenação de listagens.
            builder.HasIndex(b => b.Name);
            builder.HasIndex(b => b.Data);

            // Configuração dos atributos Hit11 a Hit15
            builder.Property(b => b.Hit11)
                   .HasColumnName("Hit11");

            builder.Property(b => b.Hit12)
                   .HasColumnName("Hit12");

            builder.Property(b => b.Hit13)
                   .HasColumnName("Hit13");

            builder.Property(b => b.Hit14)
                   .HasColumnName("Hit14");

            builder.Property(b => b.Hit15)
                   .HasColumnName("Hit15");

            // Propriedade CreatedAt
            builder.Property(b => b.CreatedAt)
                   .HasColumnName("CreatedAt")
                   .IsRequired();

            builder.Property(b => b.TotalProcessed)
                   .HasColumnName("TotalProcessed")
                   .IsRequired(false);

            // Propriedade Numbers
            builder.Property(b => b.TopTenNumbers)
                   .HasColumnName("TopTenNumbers")
                   .HasMaxLength(29)
                   .IsRequired(false);

            // Configuração do relacionamento muitos-para-muitos
            builder.HasMany(b => b.ContestsAbove11)
                   .WithMany(c => c.BaseContests)
                   .UsingEntity<Dictionary<string, object>>(
                       "BaseContestContest", // Nome da tabela intermediária
                       j => j
                            .HasOne<Contest>()
                            .WithMany()
                            .HasForeignKey("ContestId")
                            .OnDelete(DeleteBehavior.Cascade),
                       j => j
                            .HasOne<BaseContest>()
                            .WithMany()
                            .HasForeignKey("BaseContestId")
                            .OnDelete(DeleteBehavior.Cascade),
                       j =>
                       {
                           j.HasKey("BaseContestId", "ContestId"); // Chave composta
                           j.ToTable("BaseContestContest");
                       });
        }
    }
}
