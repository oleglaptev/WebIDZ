using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebIDZ.Models;
using WebIDZ.Models.ViewModels;

namespace WebIDZ.Controllers
{
    [Authorize(Users ="admin")]
    public class RepairmanController : Controller
    {
        private IDZEntities1 db = new IDZEntities1();

        // Метод для получения списка мастеров (Index)
        // Возвращает представление со списком мастеров, отсортированным по фамилии
        public ActionResult Index()
        {
            return View(db.Мастер.OrderBy(x => x.Фамилия).ToList()); // Получаем список мастеров и сортируем их по фамилии
        }

        // Метод для просмотра деталей конкретного мастера (Details)
        // Принимает ID мастера и возвращает представление с его детальной информацией
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если ID не указан
            }
            Мастер мастер = db.Мастер.Find(id); // Находим мастера по ID
            if (мастер == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если мастер не найден
            }
            return View(мастер); // Возвращаем представление с данными о мастере
        }

        // Метод для создания нового мастера (Create GET)
        // Возвращает пустую форму для создания нового мастера
        public ActionResult Create()
        {
            return View(); // Возвращаем представление для создания нового мастера
        }

        // Метод для сохранения нового мастера (Create POST)
        // Принимает данные формы и создает нового мастера в базе данных
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RepairmanVM viewModel)
        {
            if (ModelState.IsValid) // Проверяем корректность модели
            {
                var мастер = new Мастер
                {
                    ID_мастера = Guid.NewGuid(), // Генерируем уникальный ID для нового мастера
                    Квалификация = viewModel.Квалификация,
                    Должность = viewModel.Должность,
                    Фамилия = viewModel.Фамилия,
                    Имя = viewModel.Имя,
                    Отчество = viewModel.Отчество
                };
                db.Мастер.Add(мастер); // Добавляем нового мастера в контекст
                db.SaveChanges(); // Сохраняем изменения в базе данных
                return RedirectToAction("Index"); // Перенаправляем на страницу со списком мастеров
            }
            return View(viewModel); // Если модель некорректна, возвращаем форму с ошибками
        }

        // Метод для редактирования мастера (Edit GET)
        // Принимает ID мастера и возвращает форму для его редактирования
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest); // Возвращаем ошибку, если ID не указан
            }
            Мастер мастер = db.Мастер.Find(id); // Находим мастера по ID
            if (мастер == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если мастер не найден
            }
            var viewModel = new RepairmanVM
            {
                ID_мастера = мастер.ID_мастера,
                Квалификация = мастер.Квалификация,
                Должность = мастер.Должность,
                Фамилия = мастер.Фамилия,
                Имя = мастер.Имя,
                Отчество = мастер.Отчество
            };
            return View(viewModel); // Возвращаем представление для редактирования мастера
        }

        // Метод для сохранения изменений мастера (Edit POST)
        // Принимает обновленные данные и обновляет мастера в базе данных
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RepairmanVM viewModel)
        {
            if (ModelState.IsValid) // Проверяем корректность модели
            {
                var мастер = db.Мастер.Find(viewModel.ID_мастера); // Находим мастера по ID
                if (мастер == null)
                {
                    return HttpNotFound(); // Возвращаем ошибку, если мастер не найден
                }
                мастер.Квалификация = viewModel.Квалификация;
                мастер.Должность = viewModel.Должность;
                мастер.Фамилия = viewModel.Фамилия;
                мастер.Имя = viewModel.Имя;
                мастер.Отчество = viewModel.Отчество;
                db.Entry(мастер).State = EntityState.Modified; // Отмечаем запись как измененную
                db.SaveChanges(); // Сохраняем изменения в базе данных
                return RedirectToAction("Index"); // Перенаправляем на страницу со списком мастеров
            }
            return View(viewModel); // Если модель некорректна, возвращаем форму с ошибками
        }

        // Метод для удаления мастера (Delete GET)
        // Принимает ID мастера и возвращает форму подтверждения удаления
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если ID не указан
            }
            Мастер мастер = db.Мастер.Find(id); // Находим мастера по ID
            if (мастер == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если мастер не найден
            }
            return View(мастер); // Возвращаем представление для подтверждения удаления
        }

        // Метод для подтверждения удаления мастера (Delete POST)
        // Принимает ID мастера и удаляет его из базы данных
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Мастер мастер = db.Мастер.Find(id); // Находим мастера по ID
            db.Мастер.Remove(мастер); // Удаляем мастера из контекста
            db.SaveChanges(); // Сохраняем изменения в базе данных
            return RedirectToAction("Index"); // Перенаправляем на страницу со списком мастеров
        }

        // Освобождение ресурсов при завершении работы контроллера
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Освобождаем ресурсы Entity Framework
            }
            base.Dispose(disposing);
        }
    }
}