using Kanban.Data;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace WebFamilyERP.Controllers;

[Authorize]
public class KanbanController : Controller
{
    private readonly JsonDatabase<Transacao> _db = new JsonDatabase<Transacao>("Data/transacoes.json");

    public IActionResult Index()
    {
        var transacoes = _db.GetAll().OrderByDescending(t => t.Data).ToList();
        return View(transacoes);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(Transacao transacao)
    {
        transacao.Id = _db.GetAll().Count > 0 ? _db.GetAll().Max(t => t.Id) + 1 : 1;
        _db.Add(transacao);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var transacao = _db.GetAll().FirstOrDefault(t => t.Id == id);
        if (transacao == null) return NotFound();
        return View(transacao);
    }

    [HttpPost]
    public IActionResult Edit(Transacao transacao)
    {
        var transacoes = _db.GetAll();
        var index = transacoes.FindIndex(t => t.Id == transacao.Id);
        if (index == -1) return NotFound();
        transacoes[index] = transacao;
        _db.Update(transacoes);
        return RedirectToAction("Index");
    }

    public IActionResult Delete(int id)
    {
        var transacoes = _db.GetAll();
        var transacao = transacoes.FirstOrDefault(t => t.Id == id);
        if (transacao != null)
        {
            transacoes.Remove(transacao);
            _db.Update(transacoes);
        }
        return RedirectToAction("Index");
    }
}
