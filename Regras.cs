namespace DesafioErp;

// decimal evita as imprecisões de ponto flutuante nos cálculos com dinheiro.
public record Venda(string Vendedor, decimal Valor);
public record Produto(int CodigoProduto, string DescricaoProduto, int Estoque);
public record Movimento(Guid Id, int CodigoProduto, string Tipo, string Descricao, int Quantidade, int EstoqueFinal, DateTimeOffset Data);

public static class Regras
{
    public static decimal Comissao(decimal valor)
    {
        if (valor < 0) throw new ArgumentException("Venda não pode ser negativa.");
        var taxa = valor < 100m ? 0m : valor < 500m ? 0.01m : 0.05m;
        // Cada venda gera uma comissão em centavos; meio centavo arredonda para cima.
        return decimal.Round(valor * taxa, 2, MidpointRounding.AwayFromZero);
    }

    public static Dictionary<string, decimal> Comissoes(IEnumerable<Venda> vendas)
    {
        var totais = new Dictionary<string, decimal>();
        foreach (var venda in vendas)
        {
            if (string.IsNullOrWhiteSpace(venda.Vendedor)) throw new ArgumentException("Vendedor obrigatório.");
            totais.TryGetValue(venda.Vendedor, out var total);
            // A faixa depende de cada venda, por isso calculamos antes de somar.
            totais[venda.Vendedor] = total + Comissao(venda.Valor);
        }
        return totais;
    }

    public static decimal Juros(decimal valor, DateOnly vencimento, DateOnly hoje)
    {
        if (valor < 0) throw new ArgumentException("Valor não pode ser negativo.");
        // No vencimento ou antes dele, ainda não existe atraso.
        var dias = Math.Max(0, hoje.DayNumber - vencimento.DayNumber);
        // Juros simples: a taxa diária sempre incide sobre o valor original.
        return decimal.Round(valor * 0.025m * dias, 2, MidpointRounding.AwayFromZero);
    }

    public static Movimento Movimentar(Produto produto, string tipo, string descricao, int quantidade)
    {
        if (quantidade <= 0) throw new ArgumentException("Quantidade deve ser positiva.");
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Descrição obrigatória.");
        if (tipo is not ("entrada" or "saida")) throw new ArgumentException("Tipo inválido.");
        if (tipo == "saida" && quantidade > produto.Estoque) throw new ArgumentException("Estoque insuficiente.");
        // checked impede que um saldo acima do limite de int vire um número inválido.
        int saldo = checked(produto.Estoque + (tipo == "entrada" ? quantidade : -quantidade));
        return new(Guid.NewGuid(), produto.CodigoProduto, tipo, descricao.Trim(), quantidade, saldo, DateTimeOffset.Now);
    }
}
