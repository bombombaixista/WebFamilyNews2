using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class RelatorioController : Controller
    {
        private readonly string _dataPath;

        public RelatorioController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
        }

        // ===================== UTIL =====================
        private List<JsonElement> LerArquivo(string nome)
        {
            var path = Path.Combine(_dataPath, nome);
            if (!System.IO.File.Exists(path))
                return new();

            return JsonSerializer.Deserialize<List<JsonElement>>(System.IO.File.ReadAllText(path)) ?? new();
        }

        private decimal GetDecimal(JsonElement el, string prop)
        {
            if (!el.TryGetProperty(prop, out var v)) return 0m;

            return v.ValueKind switch
            {
                JsonValueKind.Number => v.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(v.GetString(), out var d) => d,
                _ => 0m
            };
        }

        private string GetString(JsonElement el, string prop)
        {
            return el.TryGetProperty(prop, out var v) ? v.GetString() ?? "" : "";
        }

        // Helper para desenhar texto
        private void DrawText(SKCanvas canvas, string text, float x, float y, SKPaint paint, float size = 10, SKTextAlign align = SKTextAlign.Left)
        {
            using var font = new SKFont(SKTypeface.Default, size);
            canvas.DrawText(text, x, y, align, font, paint);
        }

        // ===================== GRÁFICOS =====================

        private byte[] GraficoPizza(decimal entradas, decimal saidas)
        {
            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            float total = (float)(entradas + saidas);
            var center = new SKPoint(110, 100);
            float radius = 70;

            using var entradaPaint = new SKPaint { Color = SKColors.Green, IsAntialias = true };
            using var saidaPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
            using var emptyPaint = new SKPaint { Color = SKColors.LightGray, IsAntialias = true };

            if (total <= 0f)
            {
                canvas.DrawCircle(center, radius, emptyPaint);
            }
            else
            {
                float angEntrada = 360f * (float)entradas / total;

                using var p1 = new SKPath();
                p1.MoveTo(center);
                p1.ArcTo(new SKRect(40, 30, 180, 170), -90, angEntrada, false);
                p1.Close();
                canvas.DrawPath(p1, entradaPaint);

                using var p2 = new SKPath();
                p2.MoveTo(center);
                p2.ArcTo(new SKRect(40, 30, 180, 170), -90 + angEntrada, 360 - angEntrada, false);
                p2.Close();
                canvas.DrawPath(p2, saidaPaint);
            }

            return SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoCategoria(List<JsonElement> lancamentos)
        {
            var dadosDec = lancamentos
                .GroupBy(l => string.IsNullOrWhiteSpace(GetString(l, "categoria")) ? "Outros" : GetString(l, "categoria"))
                .Select(g => new { Key = g.Key, Total = g.Sum(l => GetDecimal(l, "valor")) })
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (!dadosDec.Any()) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            decimal maxDec = dadosDec.Max(d => Math.Abs(d.Total));
            if (maxDec <= 0m) maxDec = 1m;

            int x = 20;
            using var barPaint = new SKPaint { Color = SKColors.SteelBlue, IsAntialias = true };
            using var textPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            foreach (var d in dadosDec)
            {
                float h = (float)(Math.Abs(d.Total) / maxDec) * 120f;
                canvas.DrawRect(x, 160 - h, 18, h, barPaint);
                DrawText(canvas, d.Key.Length > 6 ? d.Key.Substring(0, 6) : d.Key, x, 178, textPaint, 10, SKTextAlign.Left);
                x += 30;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoSaldoDiario(List<JsonElement> lancamentos)
        {
            var dados = lancamentos
                .Select(l => new
                {
                    Data = DateTime.TryParse(GetString(l, "data"), out var d) ? d : DateTime.MinValue,
                    Tipo = GetString(l, "tipo"),
                    Valor = GetDecimal(l, "valor")
                })
                .Where(x => x.Data != DateTime.MinValue)
                .OrderBy(x => x.Data)
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (dados.Count < 2) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            float saldo = 0f;
            var pontos = new List<float>();

            foreach (var d in dados)
            {
                saldo += d.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase) ? (float)d.Valor : -(float)d.Valor;
                pontos.Add(saldo);
            }

            float max = pontos.Max();
            float min = pontos.Min();
            float range = Math.Max(1f, max - min);

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
                float x = 20f + i * 180f / Math.Max(1, (pontos.Count - 1));
                float y = 180f - ((pontos[i] - min) / range * 140f);
                if (i == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }

            canvas.DrawPath(path, paint);
            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoMensal(List<JsonElement> lancamentos)
        {
            var dadosMes = lancamentos
                .Select(l => new
                {
                    Data = DateTime.TryParse(GetString(l, "data"), out var d) ? d : DateTime.MinValue,
                    Tipo = GetString(l, "tipo"),
                    Valor = GetDecimal(l, "valor")
                })
                .Where(x => x.Data != DateTime.MinValue)
                .GroupBy(x => new { x.Data.Year, x.Data.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Entrada = g.Where(x => x.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Valor),
                    Saida = g.Where(x => x.Tipo.Contains("saida", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Valor)
                })
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (!dadosMes.Any()) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            decimal maxDec = dadosMes.Max(d => Math.Max(d.Entrada, d.Saida));
            if (maxDec <= 0m) maxDec = 1m;

            int x = 20;
            using var pe = new SKPaint { Color = SKColors.Green, IsAntialias = true };
            using var ps = new SKPaint { Color = SKColors.Red, IsAntialias = true };

            foreach (var d in dadosMes)
            {
                float he = (float)(d.Entrada / maxDec) * 120f;
                float hs = (float)(d.Saida / maxDec) * 120f;

                canvas.DrawRect(x, 160 - he, 8, he, pe);
                canvas.DrawRect(x + 10, 160 - hs, 8, hs, ps);
                x += 30;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoSaldoMensal(List<JsonElement> lancamentos)
        {
            var dadosDec = lancamentos
                .Select(l => new
                {
                    Data = DateTime.TryParse(GetString(l, "data"), out var d) ? d : DateTime.MinValue,
                    Tipo = GetString(l, "tipo"),
                    Valor = GetDecimal(l, "valor")
                })
                .Where(x => x.Data != DateTime.MinValue)
                .GroupBy(x => new { x.Data.Year, x.Data.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Sum(x => x.Tipo.Contains("entrada", StringComparison.OrdinalIgnoreCase) ? x.Valor : -x.Valor))
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (dadosDec.Count < 2) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            var dados = dadosDec.Select(d => (float)d).ToList();

            float max = dados.Max();
            float min = dados.Min();
            float range = Math.Max(1f, max - min);

            using var paint = new SKPaint
            {
                Color = SKColors.DarkBlue,
                StrokeWidth = 2,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            var path = new SKPath();
            for (int i = 0; i < dados.Count; i++)
            {
                float x = 20f + i * 180f / Math.Max(1, (dados.Count - 1));
                float y = 180f - ((dados[i] - min) / range * 140f);
                if (i == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }

            canvas.DrawPath(path, paint);
            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private byte[] GraficoTopGastos(List<JsonElement> lancamentos)
        {
            var dados = lancamentos
                .Where(l => GetString(l, "tipo").Contains("saida", StringComparison.OrdinalIgnoreCase))
                .GroupBy(l => string.IsNullOrWhiteSpace(GetString(l, "categoria")) ? "Outros" : GetString(l, "categoria"))
                .Select(g => new { Key = g.Key, Total = g.Sum(l => GetDecimal(l, "valor")) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            var bmp = new SKBitmap(220, 200);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            if (!dados.Any()) return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();

            decimal maxDec = dados.Max(d => d.Total);
            if (maxDec <= 0m) maxDec = 1m;

            int y = 30;
            using var bar = new SKPaint { Color = SKColors.IndianRed, IsAntialias = true };
            using var text = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            foreach (var d in dados)
            {
                float w = (float)(d.Total / maxDec) * 140f;
                canvas.DrawRect(70, y, w, 12, bar);
                DrawText(canvas, d.Key.Length > 10 ? d.Key.Substring(0, 10) : d.Key, 5, y + 10, text, 10, SKTextAlign.Left);
                y += 25;
            }

            return bmp.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        // ===================== PDF =====================
        [HttpGet("Financeiro")]
        public IActionResult Financeiro()
        {
            var lancamentos = LerArquivo("financeiro.json");

            decimal entradas = lancamentos
                .Where(l => GetString(l, "tipo").Contains("entrada", StringComparison.OrdinalIgnoreCase))
                .Sum(l => GetDecimal(l, "valor"));

            decimal saidas = lancamentos
                .Where(l => GetString(l, "tipo").Contains("saida", StringComparison.OrdinalIgnoreCase))
                .Sum(l => GetDecimal(l, "valor"));

            var pdf = Document.Create(doc =>
            {
                doc.Page(p =>
                {
                    p.Size(PageSizes.A4);
                    p.Margin(25);

                    p.Header().Text("📊 Relatório Financeiro Detalhado").FontSize(20).Bold().AlignCenter();

                    p.Content().Column(c =>
                    {
                        c.Item().Text("📌 Resumo Financeiro").FontSize(14).Bold();
                        c.Item().Text($"Entradas totais: R$ {entradas:F2}").FontSize(12);
                        c.Item().Text($"Saídas totais: R$ {saidas:F2}").FontSize(12);
                        c.Item().Text($"Saldo atual: R$ {(entradas - saidas):F2}").FontSize(12).Bold();

                        c.Item().Text("\n📊 Análise Detalhada:").FontSize(13).Bold();
                        c.Item().Text("• O gráfico de pizza mostra a proporção entre entradas e saídas.").FontSize(11);
                        c.Item().Text("• O gráfico de barras exibe total por categoria, útil para identificar maiores despesas.").FontSize(11);
                        c.Item().Text("• O saldo diário mostra como o caixa evoluiu ao longo do tempo.").FontSize(11);
                        c.Item().Text("• Os gráficos mensais indicam tendências de entradas e saídas por mês.").FontSize(11);
                        c.Item().Text("• Top 5 gastos apresenta as categorias com maiores saídas.").FontSize(11);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Image(GraficoPizza(entradas, saidas));
                            r.RelativeItem().Image(GraficoCategoria(lancamentos));
                            r.RelativeItem().Image(GraficoSaldoDiario(lancamentos));
                        });

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Image(GraficoMensal(lancamentos));
                            r.RelativeItem().Image(GraficoSaldoMensal(lancamentos));
                            r.RelativeItem().Image(GraficoTopGastos(lancamentos));
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "RelatorioFinanceiroDetalhado.pdf");
        }
    }
}
