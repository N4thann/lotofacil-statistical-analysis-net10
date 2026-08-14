using FluentValidation;
using Lotofacil.Application.Common.ViewsModel;
using Lotofacil.Domain.Entities;
using Lotofacil.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lotofacil.Application.Common
{
    /// <summary>
    /// Validador para a entidade ContestViewModel utilizando FluentValidation.
    /// Garante que os dados do concurso sejam válidos antes de serem processados.
    /// </summary>
    public class ContestValidator : AbstractValidator<ContestViewModel>
    {
        private readonly IRepository<Contest> _contestRepository;
        private readonly IRepository<BaseContest> _baseContestRepository;
        /// <summary>
        /// Construtor que inicializa os repositórios necessários para validação.
        /// </summary>
        /// <param name="repository">Repositório de concursos.</param>
        /// <param name="baseContestRepository">Repositório de concursos base.</param>
        public ContestValidator(IRepository<Contest> contestRepository, IRepository<BaseContest> baseContestRepository)
        {
            _contestRepository = contestRepository;
            _baseContestRepository = baseContestRepository;

            RuleFor(x => x.Name)
            .NotNull().WithMessage("O nome é obrigatório.")
            .Length(1, 5).WithMessage("O nome do concurso deve ter entre 1 e 5 caracteres.")
            .MustAsync(async (contestVM, name, cancellation) =>
            {
                var formattedName = $"Concurso {name}";

                return contestVM.IsBaseContest
                    ? !await _baseContestRepository.ExistsAsync(c => c.Name == formattedName)
                    : !await _contestRepository.ExistsAsync(c => c.Name == formattedName);
            })
            .WithMessage("Esse concurso já foi cadastrado.");

            RuleFor(x => x.Numbers)
                .NotNull().WithMessage("Os números são obrigatórios.")
                .Length(30).WithMessage("Os números devem ter exatamente 30 caracteres.")
                .Must(HaveUniqueDezenasWithinRange).WithMessage("As dezenas devem ser 15 valores únicos, cada um entre 01 e 25.");

            RuleFor(x => x.Data)
                .NotNull().WithMessage("A data é obrigatória.")
                .Must(data => data != default(DateTime)).WithMessage("A data deve ser válida.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("A data não pode ser no futuro.");

            // Validação de senha comentada. Pode ser reativada e ajustada conforme necessidade.
            // RuleFor(x => x.Password)
            //     .NotNull().WithMessage("A senha é obrigatória.")
            //     .Must(pass =>
            //     Regex.IsMatch(pass, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$"))
            //     .WithMessage("A senha não corresponde a um padrão seguro.");
        }

        /// <summary>
        /// Verifica se a string de 30 caracteres representa 15 dezenas únicas, cada uma entre 01 e 25.
        /// </summary>
        /// <param name="numbers">String de números no formato de 15 pares de 2 dígitos, sem separador.</param>
        /// <returns><c>true</c> se todas as dezenas forem válidas e únicas; caso contrário, <c>false</c>.</returns>
        private static bool HaveUniqueDezenasWithinRange(string numbers)
        {
            if (string.IsNullOrEmpty(numbers) || numbers.Length != 30)
            {
                return true; // Deixa as regras de NotNull/Length reportarem o erro correspondente.
            }

            var dezenas = new List<int>();
            for (int i = 0; i < numbers.Length; i += 2)
            {
                if (!int.TryParse(numbers.Substring(i, 2), out var dezena) || dezena < 1 || dezena > 25)
                {
                    return false;
                }
                dezenas.Add(dezena);
            }

            return dezenas.Distinct().Count() == 15;
        }
    }
}
/// <summary>
/// .LessThanOrEqualTo(DateTime.Now) - Permite qualquer data anterior ou igual ao momento atual (precisão até milissegundos).
/// </summary>