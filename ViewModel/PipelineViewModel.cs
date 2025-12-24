using Kanban.Models;
using System.Collections.Generic;

namespace Kanban.ViewModels
{
    public class PipelineViewModel
    {
        public List<Stage> Stages { get; set; } = new();
        public List<Deal> Deals { get; set; } = new();
    }
}
