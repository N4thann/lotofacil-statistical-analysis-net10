using Lotofacil.Infra.Data.Initialization.Seed;
using Shouldly;

namespace Lotofacil.Tests.Initialization
{
    public class ContestCsvReaderTests
    {
        [Fact(DisplayName = "SUCESSO - Deve parsear concurso, data e números formatados corretamente")]
        public void Read_WhenCsvHasValidRows_ShouldParseNameDataAndNumbers()
        {
            // Arrange
            var csvPath = Path.GetTempFileName();
            File.WriteAllLines(csvPath, new[]
            {
                "Concurso,Data,Números",
                "1,29/09/2003,02,03,05,06,09,10,11,13,14,16,18,20,23,24,25"
            });

            try
            {
                // Act
                var result = ContestCsvReader.Read(csvPath);

                // Assert
                result.ShouldHaveSingleItem();
                result[0].Name.ShouldBe("Concurso 1");
                result[0].Data.ShouldBe(new DateTime(2003, 9, 29, 20, 0, 0));
                result[0].Numbers.ShouldBe("02-03-05-06-09-10-11-13-14-16-18-20-23-24-25");
            }
            finally
            {
                File.Delete(csvPath);
            }
        }

        [Fact(DisplayName = "SUCESSO - Deve ignorar linhas em branco")]
        public void Read_WhenCsvHasBlankLines_ShouldSkipThem()
        {
            // Arrange
            var csvPath = Path.GetTempFileName();
            File.WriteAllLines(csvPath, new[]
            {
                "Concurso,Data,Números",
                "1,29/09/2003,02,03,05,06,09,10,11,13,14,16,18,20,23,24,25",
                "",
                "2,06/10/2003,01,04,05,06,07,09,11,12,13,15,16,19,20,23,24"
            });

            try
            {
                // Act
                var result = ContestCsvReader.Read(csvPath);

                // Assert
                result.Count.ShouldBe(2);
                result[0].Name.ShouldBe("Concurso 1");
                result[1].Name.ShouldBe("Concurso 2");
            }
            finally
            {
                File.Delete(csvPath);
            }
        }

        [Fact(DisplayName = "SUCESSO - Deve retornar lista vazia quando o CSV só tem o header")]
        public void Read_WhenCsvOnlyHasHeader_ShouldReturnEmptyList()
        {
            // Arrange
            var csvPath = Path.GetTempFileName();
            File.WriteAllLines(csvPath, new[] { "Concurso,Data,Números" });

            try
            {
                // Act
                var result = ContestCsvReader.Read(csvPath);

                // Assert
                result.ShouldBeEmpty();
            }
            finally
            {
                File.Delete(csvPath);
            }
        }
    }
}
