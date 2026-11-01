using AutoMapper;
using ScoreRebanho.Application.Rebanhos;
using ScoreRebanho.Application.Relatorios;
using ScoreRebanho.Application.Simulacoes;
using ScoreRebanho.Web.ViewModels.Rebanhos;
using ScoreRebanho.Web.ViewModels.Relatorios;

namespace ScoreRebanho.Web.Mapeamentos;

public sealed class RebanhosProfile : Profile
{
    public RebanhosProfile()
    {
        CreateMap<SimulacaoViewModel, EntradaSimulacaoDto>();
        CreateMap<EntradaSimulacaoDto, SimulacaoViewModel>();
        CreateMap<RebanhoFormularioViewModel, CriarRebanhoDto>();
        CreateMap<RebanhoFormularioViewModel, AtualizarRebanhoDto>();
        CreateMap<ExcluirRebanhoViewModel, ExcluirRebanhoDto>();
        CreateMap<DadosEdicaoRebanhoDto, RebanhoFormularioViewModel>().ForMember(d => d.Conferencia, o => o.Ignore());
        CreateMap<DadosEdicaoRebanhoDto, ConfirmacaoExclusaoViewModel>();
        CreateMap<PaginacaoViewModel, ListarRebanhosDto>();
        CreateMap<PaginaRebanhosDto, ListaRebanhosViewModel>();
        CreateMap<ItemListaRebanhosDto, ItemListaRebanhosViewModel>();
        CreateMap<ConferenciaOcupacaoDto, ConferenciaOcupacaoViewModel>()
            .IncludeMembers(s => s.Lotacao);
        CreateMap<LotacaoAnaliseDto, ConferenciaOcupacaoViewModel>()
            .ForMember(d => d.VersaoPremissas, o => o.Ignore());

        CreateMap<ResumoPecuaristaDto, ResumoPecuaristaViewModel>();
        CreateMap<AlertaAnaliseDto, AlertaViewModel>();
        CreateMap<CapacidadeResumoDto, CapacidadeResumoViewModel>();
        CreateMap<FinanceiroResumoDto, FinanceiroResumoViewModel>();
        CreateMap<AnaliseCompletaDto, AnaliseCompletaViewModel>();
        CreateMap<PremissasUtilizadasDto, List<ParametroViewModel>>().ConvertUsing<PremissasParaLinhas>();
        CreateMap<LotacaoAnaliseDto, LotacaoViewModel>();
        CreateMap<ProducaoAnaliseDto, ProducaoViewModel>();
        CreateMap<CustosAnaliseDto, CustosViewModel>();
        CreateMap<ItemCustoDto, ItemCustoViewModel>();
        CreateMap<ConsumoProdutoDto, ProdutoViewModel>();
        CreateMap<FinanceiroAnaliseDto, FinanceiroViewModel>();
        CreateMap<CenarioFinanceiroDto, CenarioFinanceiroViewModel>();
        CreateMap<ScoreAnaliseDto, ScoreViewModel>();
        CreateMap<ComponenteScoreDto, ComponenteScoreViewModel>();
        CreateMap<FormulaAnaliseDto, FormulaViewModel>();

        CreateMap<RelatorioRebanhoDto, RelatorioPecuaristaViewModel>()
            .ForMember(d => d.Resumo, o => o.MapFrom(s => (ResumoPecuaristaDto)s.Relatorio))
            .ForMember(d => d.Historico, o => o.Ignore());
        CreateMap<RelatorioRebanhoDto, RelatorioCooperativaViewModel>()
            .ForMember(d => d.Analise, o => o.MapFrom(s => ((RelatorioCooperativaDto)s.Relatorio).Analise))
            .ForMember(d => d.Historico, o => o.Ignore());
        CreateMap<RelatorioHistoricoDto, RelatorioPecuaristaViewModel>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UltimoScoreId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.NomeRebanho ?? "Simulação avulsa"))
            .ForMember(d => d.Propriedade, o => o.MapFrom(s => s.Propriedade ?? "Não informada"))
            .ForMember(d => d.Resumo, o => o.MapFrom(s => (ResumoPecuaristaDto)s.Relatorio))
            .ForMember(d => d.Historico, o => o.MapFrom(_ => true));
        CreateMap<RelatorioHistoricoDto, RelatorioCooperativaViewModel>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UltimoScoreId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.NomeRebanho ?? "Simulação avulsa"))
            .ForMember(d => d.Propriedade, o => o.MapFrom(s => s.Propriedade ?? "Não informada"))
            .ForMember(d => d.Analise, o => o.MapFrom(s => ((RelatorioCooperativaDto)s.Relatorio).Analise))
            .ForMember(d => d.Historico, o => o.MapFrom(_ => true));
    }
}
