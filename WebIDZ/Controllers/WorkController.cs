using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebIDZ.Models;

namespace WebIDZ.Controllers
{
    [Authorize]
    public class WorkController : Controller
    {
        private IDZEntities1 db = new IDZEntities1(); // Контекст базы данных

        // Вывод списка всех работ
        public ActionResult Index()
        {
            return View(db.Работа.OrderBy(x => x.Код_работы).ToList());
        }

        // Показ формы создания новой работы
        public ActionResult Create()
        {
            return View();
        }

        // Обработка создания новой работы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Код_работы,Наименование_работы,Стоимость_работы")] Работа работа)
        {
            if (ModelState.IsValid)
            {
                if (db.Работа.Any(x => x.Код_работы == работа.Код_работы))
                {
                    ModelState.AddModelError("Код_работы", "Такой код уже существует.");
                    return View(работа);
                }
                db.Работа.Add(работа);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(работа);
        }

        // Показ формы редактирования работы
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Работа работа = db.Работа.Find(id);
            if (работа == null) return HttpNotFound();
            return View(работа);
        }

        // Обработка редактирования работы
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Код_работы,Наименование_работы,Стоимость_работы")] Работа работа)
        {
            if (ModelState.IsValid)
            {
                db.Entry(работа).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(работа);
        }

        // Показ формы подтверждения удаления работы
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Работа работа = db.Работа.Find(id);
            if (работа == null) return HttpNotFound();
            return View(работа);
        }

        // Обработка удаления работы
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Работа работа = db.Работа.Find(id);
            db.Работа.Remove(работа);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Освобождение ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}