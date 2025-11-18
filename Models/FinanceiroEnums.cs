namespace Models
{
    public enum FinanceiroTipoLancamento
    {
        Receita = 1,
        Despesa = 2
    }

    public enum FinanceiroSituacaoParcela
    {
        Pendente = 1,
        Pago = 2,
        Cancelado = 3
    }

    public enum FinanceiroTipoConta
    {
        Caixa = 1,
        Banco = 2,
        Carteira = 3
    }
}
