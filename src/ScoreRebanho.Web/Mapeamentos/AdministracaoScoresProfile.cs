using AutoMapper;
using ScoreRebanho.Application.Administracao;
using ScoreRebanho.Web.ViewModels.Administracao;
using ScoreRebanho.Web.ViewModels.Rebanhos;

namespace ScoreRebanho.Web.Mapeamentos;

public sealed class AdministracaoScoresProfile : Profile
{
    public AdministracaoScoresProfile()
    {
        CreateMap<PaginacaoViewModel, ListarScoresAdministrativosDto>();
        CreateMap<PaginaScoresAdministrativosDto, ListaScoresAdministrativosViewModel>();
        CreateMap<ItemScoreAdministrativoDto, ItemScoreAdministrativoViewModel>();
        CreateMap<RelatorioScoreAdministrativoDto, RelatorioScoreAdministrativoViewModel>();
    }
}
