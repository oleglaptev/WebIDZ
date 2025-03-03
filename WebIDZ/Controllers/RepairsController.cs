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
    [Authorize] // Доступ к контроллеру разрешен только авторизованным пользователям.
    public class RepairsController : Controller
    {
        private IDZEntities1 db = new IDZEntities1(); // Контекст базы данных.

        // GET: Repairs
        /// <summary>
        /// Метод для отображения списка ремонтов и технического обслуживания.
        /// </summary>
        /// <returns>Представление со списком ремонтов, отсортированных по состоянию.</returns>
        public ActionResult Index()
        {
            var текущаяДата = DateTime.Now; // Текущая дата для расчета состояния ремонта.

            // Получаем все записи о ремонтах и связанные данные (мастер, оборудование).
            var ремонты = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .ToList();

            // Преобразуем данные в RepairVM с проверкой на null.
            var repairViewModels = ремонты.Select(р => new RepairVM
            {
                ID_ремонта = р.ID_ремонта,
                Дата_начала = р.Дата_начала.Date, // Убираем время из даты начала.
                Дата_окончания = р.Дата_окончания?.Date, // Убираем время из даты окончания.
                ФИО_мастера = GetMasterFullName(р.Мастера), // Формируем полное ФИО мастера.
                Местоположение_оборудования = р.Оборудование?.Местоположение ?? "Не указано", // Проверяем на null.
                Состояние = GetRepairStatus(р.Дата_начала, р.Дата_окончания, текущаяДата), // Вычисляем состояние ремонта.
                Название_оборудования = р.Оборудование.Название,
            }).ToList();

            // Сортируем ремонтные работы по состоянию (Запланирован, В процессе, Завершен).
            var sortstatus = new Dictionary<string, int>
            {
                { "В процессе", 1 },
                { "Запланирован", 2 },
                { "Завершен", 3 }
            };

            var sortedRepairs = repairViewModels
                .OrderBy(р => sortstatus[р.Состояние]) // Основная сортировка по состоянию.
                .ThenBy(р => р.Дата_начала) // Дополнительная сортировка по дате начала.
                .ToList();

            return View(sortedRepairs); // Возвращаем представление со списком ремонтов.
        }

        // Метод для формирования полного ФИО мастера.
        /// <summary>
        /// Возвращает полное ФИО мастера или "-" если мастер не найден.
        /// </summary>
        /// <param name="мастер">Объект мастера.</param>
        /// <returns>Строка с полным ФИО мастера.</returns>
        private string GetMasterFullName(Мастер мастер)
        {
            if (мастер == null)
            {
                return "-"; // Если мастер не найден, возвращаем дефолтное значение.
            }

            return $"{мастер.Фамилия} {мастер.Имя} {мастер.Отчество ?? ""}"; // Формируем ФИО.
        }

        // Метод для определения текущего состояния ремонта.
        /// <summary>
        /// Определяет состояние ремонта на основе дат начальной и конечной даты.
        /// </summary>
        /// <param name="дата_начала">Дата начала ремонта.</param>
        /// <param name="дата_окончания">Дата окончания ремонта (может быть null).</param>
        /// <param name="текущаяДата">Текущая дата.</param>
        /// <returns>Строка с состоянием ремонта ("Запланирован", "В процессе", "Завершен").</returns>
        private string GetRepairStatus(DateTime дата_начала, DateTime? дата_окончания, DateTime текущаяДата)
        {
            if (дата_окончания == null)
            {
                if (дата_начала >= текущаяДата)
                {
                    return "Запланирован"; // Если дата начала еще не наступила.
                }
                else
                {
                    return "В процессе"; // Если дата начала уже прошла, но нет даты окончания.
                }
            }
            else
            {
                return "Завершен"; // Если дата окончания указана.
            }
        }

        // GET: Repairs/Details/5
        /// <summary>
        /// Метод для отображения детальной информации о конкретном ремонте.
        /// </summary>
        /// <param name="id">Идентификатор ремонта.</param>
        /// <returns>Представление с детальной информацией о ремонте.</returns>
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если ID не указан.
            }

            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если ремонт не найден.
            }

            var repairVM = new RepairVM
            {
                ID_ремонта = ремонт.ID_ремонта,
                Дата_начала = ремонт.Дата_начала.Date,
                Дата_окончания = ремонт.Дата_окончания?.Date,
                Состояние = GetRepairStatus(ремонт.Дата_начала, ремонт.Дата_окончания, DateTime.Now),
                ФИО_мастера = GetMasterFullName(ремонт.Мастера),
                Название_оборудования = ремонт.Оборудование?.Название ?? "Не указано",
                Инвентарный_номер = ремонт.Оборудование?.Инвентарный_номер ?? 0,
                Работы = ремонт.Работы?.ToList() ?? new List<Работа>(),
                Суммарная_стоимость = ремонт.Работы?.Sum(р => р.Стоимость_работы) ?? 0
            };

            return View(repairVM); // Возвращаем представление с данными о ремонте.
        }

        // Метод для получения названия оборудования по инвентарному номеру.
        /// <summary>
        /// Возвращает название оборудования по его инвентарному номеру через JSON.
        /// </summary>
        /// <param name="инвентарный_номер">Инвентарный номер оборудования.</param>
        /// <returns>JSON-ответ с названием оборудования.</returns>
        [HttpGet]
        public JsonResult GetEquipmentName(int инвентарный_номер)
        {
            var оборудование = db.Оборудование.FirstOrDefault(о => о.Инвентарный_номер == инвентарный_номер);
            if (оборудование != null)
            {
                return Json(new { Name = оборудование.Название }, JsonRequestBehavior.AllowGet); // Возвращаем название оборудования.
            }
            return Json(new { Name = "Не указано" }, JsonRequestBehavior.AllowGet); // Если оборудование не найдено.
        }

        // GET: Repairs/Create
        /// <summary>
        /// Метод для отображения формы создания новой записи о ремонте.
        /// </summary>
        /// <returns>Представление для создания новой записи о ремонте.</returns>
        public ActionResult Create()
        {
            // Подготовка списка инвентарных номеров оборудования для выпадающего списка.
            ViewBag.InventoryList = db.Оборудование
                .Select(о => new SelectListItem
                {
                    Value = о.Инвентарный_номер.ToString(),
                    Text = о.Инвентарный_номер.ToString()
                })
                .ToList();

            // Подготовка списка мастеров для выпадающего списка.
            ViewBag.MasterList = db.Мастер
                .AsEnumerable()
                .Select(м => new SelectListItem
                {
                    Value = GetMasterFullName(м), // Полное ФИО как значение.
                    Text = GetMasterShortName(м) // Короткое отображение "Фамилия И.О."
                })
                .ToList();

            // Подготовка списка всех работ для чекбоксов.
            ViewBag.WorkList = db.Работа.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы
            }).ToList();

            return View(new RepairVM()); // Возвращаем пустую форму для создания новой записи.
        }

        // POST: Repairs/Create
        /// <summary>
        /// Метод для создания новой записи о ремонте.
        /// </summary>
        /// <param name="repairVM">Данные для создания новой записи о ремонте.</param>
        /// <param name="selectedWorks">Выбранные работы (массив строк с ID работ).</param>
        /// <returns>Перенаправление на список ремонтов или форма с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RepairVM repairVM, string[] selectedWorks)
        {
            if (ModelState.IsValid) // Проверяем корректность модели.
            {
                using (var dbContext = new IDZEntities1()) // Создаем новый контекст для выполнения хранимой процедуры.
                {
                    // Разбиваем строку ФИО мастера на отдельные части.
                    string[] parts = repairVM.ФИО_мастера?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    string фамилия = parts.Length > 0 ? parts[0] : null;
                    string имя = parts.Length > 1 ? parts[1] : null;
                    string отчество = parts.Length > 2 ? parts[2] : null;

                    // Проверяем существование мастера.
                    if (!string.IsNullOrEmpty(фамилия) && !string.IsNullOrEmpty(имя))
                    {
                        var выбранныйМастер = db.Мастер.FirstOrDefault(м =>
                            м.Фамилия == фамилия &&
                            м.Имя == имя &&
                            (м.Отчество == отчество || string.IsNullOrEmpty(отчество))
                        );

                        if (выбранныйМастер == null)
                        {
                            ModelState.AddModelError("", "Выбранный мастер не существует."); // Добавляем сообщение об ошибке.
                            return View(repairVM); // Возвращаем форму с ошибкой.
                        }
                    }

                    // Преобразуем массив выбранных работ в строку с разделителями.
                    string workList = selectedWorks?.Length > 0 ? string.Join(",", selectedWorks.Select(int.Parse)) : null;

                    // Вызываем хранимую процедуру для создания записи о ремонте.
                    try
                    {
                        dbContext.ЗаявкаНаРемонт(
                            repairVM.Инвентарный_номер,
                            фамилия,
                            имя,
                            отчество,
                            workList
                        );

                        return RedirectToAction("Index"); // Перенаправляем на страницу со списком ремонтов.
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Ошибка при создании ремонта: " + ex.Message); // Добавляем сообщение об ошибке.
                    }
                }
            }

            // Если модель некорректна или возникла ошибка, перезагружаем форму.
            ViewBag.MasterList = db.Мастер
                .AsEnumerable()
                .Select(м => new SelectListItem
                {
                    Value = GetMasterFullName(м), // Полное ФИО как значение.
                    Text = GetMasterShortName(м) // Короткое отображение "Фамилия И.О."
                })
                .ToList();

            ViewBag.InventoryList = db.Оборудование
                .Select(о => new SelectListItem
                {
                    Value = о.Инвентарный_номер.ToString(),
                    Text = о.Инвентарный_номер.ToString()
                })
                .ToList();

            ViewBag.WorkList = db.Работа.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы
            }).ToList();

            return View(repairVM); // Возвращаем форму с ошибками.
        }

        // GET: Repairs/Edit/5
        /// <summary>
        /// Метод для отображения формы редактирования записи о ремонте.
        /// </summary>
        /// <param name="id">Идентификатор записи о ремонте.</param>
        /// <returns>Представление для редактирования записи о ремонте.</returns>
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если ID не указан.
            }

            // Находим запись о ремонте по ID.
            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если запись не найдена.
            }

            // Создаем ViewModel для представления.
            var repairVM = new RepairVM
            {
                ID_ремонта = ремонт.ID_ремонта,
                Дата_начала = ремонт.Дата_начала,
                Дата_окончания = ремонт.Дата_окончания,
                ID_мастера = ремонт.ID_мастера,
                Название_оборудования = ремонт.Оборудование?.Название ?? "Не указано",
                Инвентарный_номер = ремонт.Оборудование?.Инвентарный_номер ?? 0,
                Работы = ремонт.Работы?.ToList() ?? new List<Работа>()
            };

            // Подготовка списка мастеров для выпадающего списка.
            ViewBag.MasterList = db.Мастер
                .AsEnumerable()
                .Select(м => new SelectListItem
                {
                    Value = м.ID_мастера.ToString(),
                    Text = GetMasterShortName(м), // Формат "Фамилия И.О."
                    Selected = ремонт.ID_мастера == м.ID_мастера
                }).ToList();

            // Подготовка списка работ для чекбоксов.
            var все_работы = db.Работа.ToList();
            var текущие_работы = ремонт.Работы.Select(w => w.Код_работы).ToList();

            ViewBag.WorkList = все_работы.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы,
                Selected = текущие_работы.Contains(р.Код_работы) // Отмечаем выбранные работы.
            }).ToList();

            return View(repairVM); // Возвращаем представление для редактирования.
        }

        // POST: Repairs/Edit/5
        /// <summary>
        /// Метод для сохранения изменений при редактировании записи о ремонте.
        /// </summary>
        /// <param name="repairVM">Данные для обновления записи о ремонте.</param>
        /// <param name="selectedWorks">Массив строк с ID выбранных работ.</param>
        /// <returns>Перенаправление на список ремонтов или форма с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_ремонта,Дата_начала,Дата_окончания,ID_мастера")] RepairVM repairVM, string[] selectedWorks)
        {
            if (ModelState.IsValid) // Проверяем корректность модели.
            {
                var ремонт = db.Ремонты_и_техобслуживания.Find(repairVM.ID_ремонта); // Находим запись по ID.

                if (ремонт == null)
                {
                    return HttpNotFound(); // Возвращаем ошибку, если запись не найдена.
                }

                // Обновляем данные о ремонте.
                ремонт.Дата_начала = repairVM.Дата_начала;
                ремонт.Дата_окончания = repairVM.Дата_окончания;
                ремонт.ID_мастера = repairVM.ID_мастера;

                // Обновляем список работ.
                UpdateRepairWorks(ремонт, selectedWorks);

                db.Entry(ремонт).State = EntityState.Modified; // Отмечаем запись как измененную.
                db.SaveChanges(); // Сохраняем изменения в базе данных.

                return RedirectToAction("Index"); // Перенаправляем на страницу со списком ремонтов.
            }

            // Если модель некорректна, перезагружаем форму.
            ViewBag.MasterList = new SelectList(db.Мастер, "ID_мастера", "Фамилия", repairVM.ID_мастера);
            ViewBag.WorkList = GetWorkList(db.Работа.ToList(), repairVM.Работы);

            return View(repairVM); // Возвращаем форму с ошибками.
        }

        // Метод для обновления списка работ в записи о ремонте.
        /// <summary>
        /// Обновляет список работ для записи о ремонте.
        /// </summary>
        /// <param name="ремонт">Запись о ремонте.</param>
        /// <param name="selectedWorks">Массив строк с ID выбранных работ.</param>
        private void UpdateRepairWorks(Ремонты_и_техобслуживания ремонт, string[] selectedWorks)
        {
            if (selectedWorks == null || selectedWorks.Length == 0)
            {
                ремонт.Работы.Clear(); // Очищаем список работ, если ничего не выбрано.
                return;
            }

            var workIds = selectedWorks.Select(int.Parse).ToList(); // Преобразуем строки в целые числа.
            var currentWorks = ремонт.Работы.Select(w => w.Код_работы).ToList(); // Текущий список работ.

            // Удаляем работы, которые больше не выбраны.
            foreach (var работа in ремонт.Работы.ToList())
            {
                if (!workIds.Contains(работа.Код_работы))
                {
                    ремонт.Работы.Remove(работа);
                }
            }

            // Добавляем новые выбранные работы.
            foreach (var workId in workIds)
            {
                if (!currentWorks.Contains(workId))
                {
                    var работа = db.Работа.Find(workId); // Находим работу по ID.
                    if (работа != null)
                    {
                        ремонт.Работы.Add(работа); // Добавляем работу к ремонту.
                    }
                }
            }
        }

        // Метод для создания списка работ с отмеченными элементами.
        /// <summary>
        /// Создает список работ с отметками о выбранных элементах.
        /// </summary>
        /// <param name="все_работы">Список всех доступных работ.</param>
        /// <param name="текущие_работы">Список работ, связанных с текущим ремонтом.</param>
        /// <returns>Список работ для чекбоксов.</returns>
        private List<SelectListItem> GetWorkList(List<Работа> все_работы, List<Работа> текущие_работы)
        {
            return все_работы.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы,
                Selected = текущие_работы.Any(w => w.Код_работы == р.Код_работы) // Помечаем выбранные работы.
            }).ToList();
        }

        // GET: Repairs/Delete/5
        /// <summary>
        /// Метод для подтверждения удаления записи о ремонте.
        /// </summary>
        /// <param name="id">Идентификатор записи о ремонте.</param>
        /// <returns>Представление для подтверждения удаления.</returns>
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если ID не указан.
            }

            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если запись не найдена.
            }

            // Создаем ViewModel для представления.
            var repairVM = new RepairVM
            {
                ID_ремонта = ремонт.ID_ремонта,
                Дата_начала = ремонт.Дата_начала,
                Дата_окончания = ремонт.Дата_окончания,
                Состояние = GetRepairStatus(ремонт.Дата_начала, ремонт.Дата_окончания, DateTime.Now),
                ФИО_мастера = ремонт.Мастера != null ? GetMasterShortName(ремонт.Мастера) : "Не назначен",
                Название_оборудования = ремонт.Оборудование?.Название ?? "Не указано",
                Инвентарный_номер = ремонт.Оборудование?.Инвентарный_номер ?? 0,
                Работы = ремонт.Работы?.ToList() ?? new List<Работа>(),
                Суммарная_стоимость = ремонт.Работы?.Sum(р => р.Стоимость_работы) ?? 0
            };

            return View(repairVM); // Возвращаем представление для подтверждения удаления.
        }

        // POST: Repairs/Delete/5
        /// <summary>
        /// Метод для удаления записи о ремонте.
        /// </summary>
        /// <param name="id">Идентификатор записи о ремонте.</param>
        /// <returns>Перенаправление на список ремонтов или страница с ошибкой.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var ремонт = db.Ремонты_и_техобслуживания.Find(id); // Находим запись по ID.

            if (ремонт == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если запись не найдена.
            }

            try
            {
                db.Ремонты_и_техобслуживания.Remove(ремонт); // Удаляем запись о ремонте.

                // Обновляем состояние оборудования на "в работе" (true), если оно было в ремонте.
                var оборудование = ремонт.Оборудование;
                if (оборудование != null && !оборудование.Состояние)
                {
                    оборудование.Состояние = true;
                    db.Entry(оборудование).State = EntityState.Modified; // Отмечаем запись как измененную.
                }

                db.SaveChanges(); // Сохраняем изменения в базе данных.

                return RedirectToAction("Index"); // Перенаправляем на страницу со списком ремонтов.
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при удалении ремонта: " + ex.Message); // Добавляем сообщение об ошибке.
                return View("Error"); // Возвращаем страницу с ошибкой.
            }
        }

        // Метод для формирования короткого ФИО мастера.
        /// <summary>
        /// Возвращает короткое ФИО мастера в формате "Фамилия И.О.".
        /// </summary>
        /// <param name="мастер">Объект мастера.</param>
        /// <returns>Строка с коротким ФИО мастера.</returns>
        private string GetMasterShortName(Мастер мастер)
        {
            if (мастер == null)
            {
                return "Не назначен"; // Если мастер не найден.
            }

            return $"{мастер.Фамилия} {мастер.Имя.FirstOrDefault()}. {мастер.Отчество?.FirstOrDefault() ?? ' '}.".Trim(); // Формат "Фамилия И.О."
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
            base.Dispose(disposing);
        }
    }
}