using AutoMapper;
using ScoreRebanho.Application.Clima;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Web.ViewModels.Acompanhamento;

namespace ScoreRebanho.Web.Mapeamentos;

public sealed class AcompanhamentoProfile : Profile
{
    public AcompanhamentoProfile()
    {
        CreateMap<ConsultaPrevisaoViewModel, ConsultarPrevisaoDto>();
        CreateMap<PaginaRebanhosDto, PaginaAcompanhamentoViewModel>();
        CreateMap<ItemListaRebanhosDto, RebanhoAcompanhamentoViewModel>();
        CreateMap<PrevisaoRebanhoDto, PrevisaoRebanhoViewModel>();
        CreateMap<PrevisaoDto, PrevisaoViewModel>();
        CreateMap<DiaPrevisaoDto, DiaPrevisaoViewModel>();
    }
}
