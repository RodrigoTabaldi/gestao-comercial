using System.Globalization;
using System.Text;
using System.Text.Json;
using DesafioErp;

Console.OutputEncoding = Encoding.UTF8;
if (args.Contains("--test"))
{
    Testes.Executar();
    return;
}
var cultura = CultureInfo.GetCultureInfo("pt-BR");
var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
var dados = Path.Combine(AppContext.BaseDirectory, "dados");
var arquivo = Path.Combine(Environment.CurrentDirectory, "estado-estoque.json");
while (true)
{
    Console.WriteLine("\n1 - Comissões | 2 - Estoque | 3 - Juros | 0 - Sair");
    var opcao = Console.ReadLine();
    if (opcao is null or "0") break;
    try
    {
        switch (opcao)
        {
            case "1":
                foreach (var item in Regras.Comissoes(Ler<Vendas>(Path.Combine(dados, "vendas.json")).Itens).OrderBy(x => x.Key))
                    Console.WriteLine($"{item.Key}: {item.Value.ToString("C2", cultura)}");
                break;
            case "2":
                // Depois da primeira movimentação, retomamos o saldo salvo.
                var estado = File.Exists(arquivo) ? Ler<Estado>(arquivo)
                    : new Estado(Ler<EstoqueInicial>(Path.Combine(dados, "estoque.json")).Estoque, []);
                foreach (var p in estado.Produtos) Console.WriteLine($"{p.CodigoProduto} - {p.DescricaoProduto}: {p.Estoque}");
                var codigo = Inteiro("Código: ");
                var produto = estado.Produtos.SingleOrDefault(p => p.CodigoProduto == codigo)
                    ?? throw new ArgumentException("Produto não encontrado.");
                var tipo = Texto("Tipo (entrada/saida): ").Trim().ToLowerInvariant();
                var descricao = Texto("Descrição: ");
                var movimento = Regras.Movimentar(produto, tipo, descricao, Inteiro("Quantidade: "));
                var produtos = estado.Produtos
                    .Select(p => p.CodigoProduto == codigo ? p with { Estoque = movimento.EstoqueFinal } : p)
                    .ToList();
                var novo = new Estado(produtos, [.. estado.Movimentos, movimento]);
                // Saldo e histórico são gravados juntos antes de substituir o arquivo anterior.
                File.WriteAllText(arquivo + ".tmp", JsonSerializer.Serialize(novo, json));
                File.Move(arquivo + ".tmp", arquivo, true);
                Console.WriteLine($"Movimentação: {movimento.Id}. Estoque final: {movimento.EstoqueFinal}");
                break;
            case "3":
                if (!decimal.TryParse(Texto("Valor (ex.: 100,50): "), NumberStyles.Number, cultura, out var valor)) throw new ArgumentException("Valor inválido.");
                if (!DateOnly.TryParseExact(Texto("Vencimento (dd/MM/yyyy): "), "dd/MM/yyyy", cultura, DateTimeStyles.None, out var vencimento)) throw new ArgumentException("Data inválida.");
                // A data vem do computador; a regra recebe a data para permitir testes fixos.
                var hoje = DateOnly.FromDateTime(DateTime.Today);
                var juros = Regras.Juros(valor, vencimento, hoje);
                Console.WriteLine($"Dias em atraso: {Math.Max(0, hoje.DayNumber - vencimento.DayNumber)}");
                Console.WriteLine($"Juros: {juros.ToString("C2", cultura)} | Total: {(valor + juros).ToString("C2", cultura)}");
                break;
            default: Console.WriteLine("Opção inválida."); break;
        }
    }
    catch (Exception e) when (e is ArgumentException or IOException or JsonException or OverflowException or InvalidOperationException)
    {
        Console.WriteLine($"Não foi possível concluir: {e.Message}");
    }
}
T Ler<T>(string caminho) => JsonSerializer.Deserialize<T>(File.ReadAllText(caminho), json) ?? throw new JsonException("Documento vazio.");
string Texto(string mensagem)
{
    Console.Write(mensagem);
    return Console.ReadLine() ?? throw new InvalidOperationException("Entrada encerrada.");
}
int Inteiro(string mensagem) => int.TryParse(Texto(mensagem), out var valor) ? valor : throw new ArgumentException("Informe um inteiro válido.");
record Vendas([property: System.Text.Json.Serialization.JsonPropertyName("vendas")] List<Venda> Itens);
record EstoqueInicial(List<Produto> Estoque);
record Estado(List<Produto> Produtos, List<Movimento> Movimentos);
