using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kanban.Controllers
{
    [Authorize]

    public class RelatorioProdutoEstoqueController : Controller
    {
        public IActionResult Executivo()
        {
            var dados = ObterMovimentacoes();

            // Calculando totais e resumos
            int totalEntradas = dados.Where(x => x.Tipo == "Entrada").Sum(x => x.Quantidade);
            int totalSaidas = dados.Where(x => x.Tipo == "Saida").Sum(x => x.Quantidade);
            int saldoEstoque = totalEntradas - totalSaidas;

            var pdf = Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.Header()
                        .Text("RELATÓRIO EXECUTIVO - PRODUTOS E ESTOQUE")
                        .FontSize(22)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // RESUMO
                        col.Item().Text($"Resumo Geral:").FontSize(16).Bold();
                        col.Item().Text($"Total de Entradas: {totalEntradas} unidades")
                            .FontSize(12);
                        col.Item().Text($"Total de Saídas: {totalSaidas} unidades")
                            .FontSize(12);
                        col.Item().Text($"Saldo Atual no Estoque: {saldoEstoque} unidades")
                            .FontSize(12)
                            .SemiBold();

                        // GRÁFICOS
                        col.Item().Text("Gráficos Detalhados:").FontSize(16).Bold();

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Entradas x Saídas").Bold();
                                c.Item().Image(ComLegenda(GraficoPizza(dados), "Entradas vs Saídas"));
                                c.Item().Text($"Mostra a proporção entre entradas e saídas de produtos no período.")
                                    .FontSize(10).Italic();
                            });

                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Quantidade por Produto").Bold();
                                c.Item().Image(ComLegenda(GraficoPorProduto(dados), "Quantidade por Produto"));
                                c.Item().Text("Demonstra o estoque líquido de cada produto, considerando entradas e saídas.")
                                    .FontSize(10).Italic();
                            });

                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Evolução do Estoque").Bold();
                                c.Item().Image(ComLegenda(GraficoEvolucaoEstoque(dados), "Evolução do Estoque"));
                                c.Item().Text("Mostra como o estoque total varia ao longo do tempo.")
                                    .FontSize(10).Italic();
                            });
                        });

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Top 5 Produtos").Bold();
                                c.Item().Image(ComLegenda(GraficoTopProdutos(dados), "Top 5 Produtos"));
                                c.Item().Text("Produtos com maior movimentação no período.")
                                    .FontSize(10).Italic();
                            });

                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Movimentações por Mês").Bold();
                                c.Item().Image(ComLegenda(GraficoMovimentacoesMes(dados), "Movimentações por Mês"));
                                c.Item().Text("Total de movimentações de produtos mês a mês.")
                                    .FontSize(10).Italic();
                            });
                        });

                        // OBSERVAÇÕES
                        col.Item().Text("Observações:").FontSize(14).Bold();
                        col.Item().Text("✔ O relatório reflete os dados atuais do estoque e movimentações registradas.").FontSize(10);
                        col.Item().Text("✔ Gráficos mostram tendências e produtos com maior impacto no estoque.").FontSize(10);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return File(pdf.GeneratePdf(), "application/pdf", "Relatorio_Produto_Estoque.pdf");
        }

        // ===============================
        // DADOS
        // ===============================
        private List<Movimentacao> ObterMovimentacoes()
        {
            return new List<Movimentacao>
            {
                new Movimentacao { ProdutoId = 1, Quantidade = 50, Tipo = "Entrada", Data = DateTime.Today.AddDays(-30) },
                new Movimentacao { ProdutoId = 1, Quantidade = 10, Tipo = "Saida", Data = DateTime.Today.AddDays(-25) },
                new Movimentacao { ProdutoId = 2, Quantidade = 40, Tipo = "Entrada", Data = DateTime.Today.AddDays(-20) },
                new Movimentacao { ProdutoId = 2, Quantidade = 15, Tipo = "Saida", Data = DateTime.Today.AddDays(-10) },
                new Movimentacao { ProdutoId = 3, Quantidade = 30, Tipo = "Entrada", Data = DateTime.Today.AddDays(-5) }
            };
        }

        // ===============================
        // LEGENDA
        // ===============================
        private byte[] ComLegenda(byte[] grafico, string texto)
        {
            using var img = SKBitmap.Decode(grafico);
            var bmp = new SKBitmap(img.Width, img.Height + 30);
            using var canvas = new SKCanvas(bmp);

            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(img, 0, 0);

            using var font = new SKFont { Size = 12 };
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            canvas.DrawText(texto, bmp.Width / 2f, img.Height + 20,
                SKTextAlign.Center, font, paint);

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        // ===============================
        // GRÁFICOS (mantidos os que já funcionam)
        // ===============================
        private byte[] GraficoPizza(List<Movimentacao> dados)
        {
            float entradas = dados.Where(x => x.Tipo == "Entrada").Sum(x => x.Quantidade);
            float saidas = dados.Where(x => x.Tipo == "Saida").Sum(x => x.Quantidade);
            float total = Math.Max(1, entradas + saidas);

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            float angEntrada = entradas / total * 360;

            using var pE = new SKPaint { Color = SKColors.Green, IsAntialias = true };
            using var pS = new SKPaint { Color = SKColors.Red, IsAntialias = true };

            var rect = new SKRect(20, 20, 180, 180);
            canvas.DrawArc(rect, 0, angEntrada, true, pE);
            canvas.DrawArc(rect, angEntrada, 360 - angEntrada, true, pS);

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoPorProduto(List<Movimentacao> dados)
        {
            var grupos = dados
                .GroupBy(x => x.ProdutoId)
                .Select(g => new { Produto = g.Key, Total = g.Sum(x => x.Tipo == "Entrada" ? x.Quantidade : -x.Quantidade) })
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            float max = Math.Max(1, grupos.Max(x => x.Total));
            int x = 20;

            using var paint = new SKPaint { Color = SKColors.SteelBlue };

            foreach (var g in grupos)
            {
                float h = g.Total / max * 120;
                canvas.DrawRect(x, 160 - h, 20, h, paint);
                x += 30;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoEvolucaoEstoque(List<Movimentacao> dados)
        {
            int saldo = 0;
            var pontos = dados.OrderBy(x => x.Data).Select(x =>
            {
                saldo += x.Tipo == "Entrada" ? x.Quantidade : -x.Quantidade;
                return saldo;
            }).ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (pontos.Count < 2) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            float max = pontos.Max();
            float min = pontos.Min();
            float range = Math.Max(1, max - min);

            using var paint = new SKPaint
            {
                Color = SKColors.DarkGreen,
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var path = new SKPath();
            for (int i = 0; i < pontos.Count; i++)
            {
                float x = 20 + i * (180f / (pontos.Count - 1));
                float y = 180 - ((pontos[i] - min) / range * 140);
                if (i == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }

            canvas.DrawPath(path, paint);
            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoTopProdutos(List<Movimentacao> dados)
        {
            var top = dados
                .GroupBy(x => x.ProdutoId)
                .Select(g => new { Produto = g.Key, Total = g.Sum(x => Math.Abs(x.Quantidade)) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            float max = Math.Max(1, top.Max(x => x.Total));
            int y = 30;

            using var paint = new SKPaint { Color = SKColors.IndianRed };

            foreach (var g in top)
            {
                float w = g.Total / max * 150;
                canvas.DrawRect(40, y, w, 12, paint);
                y += 25;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoMovimentacoesMes(List<Movimentacao> dados)
        {
            var grupos = dados
                .GroupBy(x => new { x.Data.Year, x.Data.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Sum(x => x.Quantidade))
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            float max = Math.Max(1, grupos.Max());
            int x = 20;

            using var paint = new SKPaint { Color = SKColors.DarkBlue };

            foreach (var v in grupos)
            {
                float h = v / max * 120;
                canvas.DrawRect(x, 160 - h, 15, h, paint);
                x += 25;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }
    }
}
