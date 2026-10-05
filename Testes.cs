namespace DesafioErp;
internal static class Testes
{
    public static void Executar()
    {
        int total = 0;
        void Igual<T>(T esperado, T obtido)
        {
            if (!EqualityComparer<T>.Default.Equals(esperado, obtido)) throw new Exception($"Esperado {esperado}; obtido {obtido}");
            total++;
        }
        void Rejeita(Action acao)
        {
            try { acao(); } catch (ArgumentException) { total++; return; }
            throw new Exception("Operação inválida aceita.");
        }
        // Os limites garantem que R$100 e R$500 entrem na faixa correta.
        Igual(0m, Regras.Comissao(99.99m));
        Igual(1m, Regras.Comissao(100m));
        Igual(5m, Regras.Comissao(499.99m));
        Igual(25m, Regras.Comissao(500m));
        Igual(60.03m, Regras.Comissao(1200.50m));
        Igual(26m, Regras.Comissoes([new("Ana", 100m), new("Ana", 500m)])["Ana"]);
        Rejeita(() => Regras.Comissao(-1m));
        var p = new Produto(101, "Caneta Azul", 150);
        var entrada = Regras.Movimentar(p, "entrada", "Compra", 10);
        Igual(160, entrada.EstoqueFinal);
        Igual(0, Regras.Movimentar(p, "saida", "Venda", 150).EstoqueFinal);
        Igual(false, entrada.Id == Regras.Movimentar(p, "entrada", "Compra", 10).Id);
        Rejeita(() => Regras.Movimentar(p, "saida", "Venda", 151));
        Rejeita(() => Regras.Movimentar(p, "entrada", "Compra", 0));
        Rejeita(() => Regras.Movimentar(p, "entrada", " ", 1));
        Rejeita(() => Regras.Movimentar(p, "outro", "Compra", 1));
        // Uma data fixa mantém o resultado dos testes igual em qualquer dia.
        var hoje = new DateOnly(2026, 10, 4);
        Igual(0m, Regras.Juros(100m, hoje, hoje));
        Igual(0m, Regras.Juros(100m, hoje.AddDays(1), hoje));
        Igual(2.5m, Regras.Juros(100m, hoje.AddDays(-1), hoje));
        Igual(75m, Regras.Juros(100m, hoje.AddDays(-30), hoje));
        Igual(5m, Regras.Juros(100m, new(2024, 2, 28), new(2024, 3, 1)));
        Rejeita(() => Regras.Juros(-1m, hoje, hoje));
        Console.WriteLine($"{total} verificações passaram.");
    }
}
