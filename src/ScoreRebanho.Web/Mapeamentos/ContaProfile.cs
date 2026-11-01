using AutoMapper;
using ScoreRebanho.Application.Contas;
using ScoreRebanho.Web.ViewModels.Conta;

namespace ScoreRebanho.Web.Mapeamentos;

public sealed class ContaProfile : Profile
{
    public ContaProfile() => CreateMap<CadastrarViewModel, CadastrarContaDto>();
}
