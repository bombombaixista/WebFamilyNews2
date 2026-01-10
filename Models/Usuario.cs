public class Usuario
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }

    public DateTime DataCadastro { get; set; }
    public DateTime DataExpiracao { get; set; }

    public string? Plano { get; set; } // Free, Basico, Profissional
}
