using AutoMapper;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Mapeamentos;

public sealed class SimulacoesProfile : Profile
{
    public SimulacoesProfile()
    {
        CreateMap<SimulacaoFormularioViewModel, GerarSimulacaoDto>();
        CreateMap<DadosEdicaoSimulacaoDto, SimulacaoFormularioViewModel>()
            .ForMember(d => d.Conferencia, o => o.Ignore());
    }
}
