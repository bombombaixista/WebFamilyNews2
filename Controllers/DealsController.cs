using Microsoft.AspNetCore.Mvc;

namespace Kanban.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DealsController : Controller
    {
        // Lista em memória simulando o banco
        private static List<Deal> _deals = new()
        {
            new Deal { Id = 1, StageId = 1, Nome = "Primeiro negócio" },
            new Deal { Id = 2, StageId = 2, Nome = "Segundo negócio" }
        };

        [HttpPost("Move")]
        public IActionResult Move([FromBody] MoveDealDto dto)
        {
            var deal = _deals.FirstOrDefault(d => d.Id == dto.DealId);
            if (deal == null)
                return NotFound();

            deal.StageId = dto.StageId;
            // não há SaveChangesAsync, pois não existe banco
            return Ok(deal);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_deals);
        }
    }

    public class MoveDealDto
    {
        public int DealId { get; set; }
        public int StageId { get; set; }
    }

    public class Deal
    {
        public int Id { get; set; }
        public int StageId { get; set; }
        public string? Nome { get; set; }
    }
}
