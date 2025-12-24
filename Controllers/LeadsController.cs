using Kanban.Models;
using Kanban.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Kanban.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {
        private static List<Stage> _stages = new()
        {
            new Stage { Id = 1, OrderIndex = 1, Name = "Lead" },
            new Stage { Id = 2, OrderIndex = 2, Name = "Qualificação" },
            new Stage { Id = 3, OrderIndex = 3, Name = "Proposta" },
            new Stage { Id = 4, OrderIndex = 4, Name = "Negociação" },
            new Stage { Id = 5, OrderIndex = 5, Name = "Fechado" }
        };

        private static List<Kanban.Models.Deal> _deals = new();

        public IActionResult Index()
        {
            var vm = new PipelineViewModel
            {
                Stages = _stages.OrderBy(s => s.OrderIndex).ToList(),
                Deals = _deals.ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult AddDeal([FromBody] Kanban.Models.Deal newDeal)
        {
            newDeal.Id = _deals.Count > 0 ? _deals.Max(d => d.Id) + 1 : 1;
            _deals.Add(newDeal);
            return Json(new { success = true, id = newDeal.Id });
        }

        [HttpPost]
        public IActionResult DeleteDeal([FromBody] DeleteDealRequest request)
        {
            var deal = _deals.FirstOrDefault(d => d.Id == request.DealId);
            if (deal == null) return NotFound();

            _deals.Remove(deal);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult MoveDeal([FromBody] MoveDealRequest request)
        {
            var deal = _deals.FirstOrDefault(d => d.Id == request.DealId);
            if (deal == null) return NotFound();

            deal.StageId = request.NewStageId;
            return Json(new { success = true });
        }
    }

    public class DeleteDealRequest
    {
        public int DealId { get; set; }
    }

    public class MoveDealRequest
    {
        public int DealId { get; set; }
        public int NewStageId { get; set; }
    }
}
