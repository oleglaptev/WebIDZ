using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebIDZ.Models;

namespace WebIDZ.Controllers
{
    [Authorize] // Доступ к контроллеру разрешен только авторизованным пользователям.
    public class WorkController : Controller
    {
        private IDZEntities1 db = new IDZEntities1(); // Контекст базы данных.

        // GET: Work
        /// <summary>
        /// Метод для отображения списка всех работ.
        /// </summary>
        /// <returns>Представление со списком работ, отсортированным по коду работы.</returns>
        public ActionResult Index()
        {
            return View(db.Работа.OrderBy(x => x.Код_работы).ToList()); // Получаем список всех работ и передаем его в представление.
        }

        // GET: Work/Create
        /// <summary>
        /// Метод для отображения формы создания новой работы.
        /// </summary>
        /// <returns>Представление для создания новой работы.</returns>
        public ActionResult Create()
        {
            return View(); // Возвращаем пустую форму для создания новой работы.
        }

        // POST: Work/Create
        /// <summary>
        /// Метод для обработки создания новой работы.
        /// </summary>
        /// <param name="работа">Данные новой работы.</param>
        /// <returns>Перенаправление на страницу со списком работ или форма с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от атак межсайтовой подделки запроса.
        public ActionResult Create([Bind(Include = "Код_работы,Наименование_работы,Стоимость_работы")] Работа работа)
        {
            if (ModelState.IsValid) // Проверяем корректность модели.
            {
                // Проверяем уникальность кода работы.
                if (db.Работа.Any(x => x.Код_работы == работа.Код_работы))
                {
                    ModelState.AddModelError("Код_работы", "Такой код уже существует."); // Добавляем сообщение об ошибке.
                    return View(работа); // Возвращаем форму с ошибкой.
                }

                db.Работа.Add(работа); // Добавляем новую работу в контекст базы данных.
                db.SaveChanges(); // Сохраняем изменения.
                return RedirectToAction("Index"); // Перенаправляем на страницу со списком работ.
            }

            return View(работа); // Если модель некорректна, возвращаем форму с ошибками.
        }

        // GET: Work/Edit/5
        /// <summary>
        /// Метод для отображения формы редактирования существующей работы.
        /// </summary>
        /// <param name="id">Идентификатор работы.</param>
        /// <returns>Представление для редактирования работы или ошибка, если работа не найдена.</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если ID не указан.
            }

            Работа работа = db.Работа.Find(id); // Находим работу по ID.
            if (работа == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если работа не найдена.
            }

            return View(работа); // Возвращаем представление для редактирования работы.
        }

        // POST: Work/Edit/5
        /// <summary>
        /// Метод для обработки редактирования существующей работы.
        /// </summary>
        /// <param name="работа">Данные для обновления работы.</param>
        /// <returns>Перенаправление на страницу со списком работ или форма с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от атак межсайтовой подделки запроса.
        public ActionResult Edit([Bind(Include = "Код_работы,Наименование_работы,Стоимость_работы")] Работа работа)
        {
            if (ModelState.IsValid) // Проверяем корректность модели.
            {
                db.Entry(работа).State = EntityState.Modified; // Отмечаем запись как измененную.
                db.SaveChanges(); // Сохраняем изменения в базе данных.
                return RedirectToAction("Index"); // Перенаправляем на страницу со списком работ.
            }

            return View(работа); // Если модель некорректна, возвращаем форму с ошибками.
        }

        // GET: Work/Delete/5
        /// <summary>
        /// Метод для отображения формы подтверждения удаления работы.
        /// </summary>
        /// <param name="id">Идентификатор работы.</param>
        /// <returns>Представление для подтверждения удаления работы или ошибка, если работа не найдена.</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если ID не указан.
            }

            Работа работа = db.Работа.Find(id); // Находим работу по ID.
            if (работа == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если работа не найдена.
            }

            return View(работа); // Возвращаем представление для подтверждения удаления.
        }

        // POST: Work/Delete/5
        /// <summary>
        /// Метод для обработки удаления работы.
        /// </summary>
        /// <param name="id">Идентификатор работы.</param>
        /// <returns>Перенаправление на страницу со списком работ.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Защита от атак межсайтовой подделки запроса.
        public ActionResult DeleteConfirmed(int id)
        {
            Работа работа = db.Работа.Find(id); // Находим работу по ID.
            db.Работа.Remove(работа); // Удаляем работу из контекста базы данных.
            db.SaveChanges(); // Сохраняем изменения.
            return RedirectToAction("Index"); // Перенаправляем на страницу со списком работ.
        }

        // Освобождение ресурсов при завершении работы контроллера.
        /// <summary>
        /// Освобождает ресурсы Entity Framework.
        /// </summary>
        /// <param name="disposing">true, если нужно освободить управляемые ресурсы.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Освобождаем ресурсы Entity Framework.
            }
            base.Dispose(disposing); // Вызываем метод базового класса для завершения освобождения ресурсов.
        }
    }
}