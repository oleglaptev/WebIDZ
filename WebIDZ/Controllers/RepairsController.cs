using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebIDZ.Models;
using WebIDZ.Models.ViewModels;

namespace WebIDZ.Controllers
{
    [Authorize]
    public class RepairsController : Controller
    {
        private IDZEntities1 db = new IDZEntities1();

        // GET: Repairs
        public ActionResult Index()
        {
            var текущаяДата = DateTime.Now;

            // Получаем все ремонты и связанные данные
            var ремонты = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .ToList();

            // Преобразуем данные в RepairVM с проверкой на null
            var repairViewModels = ремонты.Select(р => new RepairVM
            {
                ID_ремонта = р.ID_ремонта,
                Дата_начала = р.Дата_начала.Date, // Убираем время
                Дата_окончания = р.Дата_окончания?.Date, // Убираем время
                ФИО_мастера = GetMasterFullName(р.Мастера), // Формируем ФИО мастера с проверкой на null
                Местоположение_оборудования = р.Оборудование?.Местоположение ?? "Не указано", // Проверяем на null
                Состояние = GetRepairStatus(р.Дата_начала, р.Дата_окончания, текущаяДата), // Вычисляем состояние
                Название_оборудования = р.Оборудование.Название,
            }).ToList();

            // Сортировка ремонтов по состоянию
            var sortstatus = new Dictionary<string, int>
                {
                    { "В процессе", 1 },
                    { "Запланирован", 2 },
                    { "Завершен", 3 }
                };

            var sortedRepairs = repairViewModels
                .OrderBy(р => sortstatus[р.Состояние])
                .ThenBy(р => р.Дата_начала) // Дополнительная сортировка по дате начала (если нужно)
                .ToList();

            return View(sortedRepairs);
        }

        // Метод для безопасного формирования ФИО мастера
        private string GetMasterFullName(Мастер мастер)
        {
            if (мастер == null)
            {
                return "-";
            }

            return $"{мастер.Фамилия} {мастер.Имя} {мастер.Отчество ?? ""}";
        }

        // Метод для вычисления состояния ремонта
        private string GetRepairStatus(DateTime дата_начала, DateTime? дата_окончания, DateTime текущаяДата)
        {
            if (дата_окончания == null)
            {
                if (дата_начала >= текущаяДата)
                {
                    return "Запланирован";
                }
                else
                {
                    return "В процессе";
                }
            }
            else
            {
                return "Завершен";
            }
        }

        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound();
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

            return View(repairVM);
        }

        [HttpGet]
        public JsonResult GetEquipmentName(int инвентарный_номер)
        {
            var оборудование = db.Оборудование.FirstOrDefault(о => о.Инвентарный_номер == инвентарный_номер);
            if (оборудование != null)
            {
                return Json(new { Name = оборудование.Название }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Name = "Не указано" }, JsonRequestBehavior.AllowGet);
        }
        // GET: Repairs/Create
        public ActionResult Create()
        {
            // Подготовка списка инвентарных номеров для выпадающего списка
            ViewBag.InventoryList = db.Оборудование
                .Select(о => new SelectListItem
                {
                    Value = о.Инвентарный_номер.ToString(),
                    Text = о.Инвентарный_номер.ToString()
                })
                .ToList();

            // Подготовка списка мастеров для выпадающего списка
            ViewBag.MasterList = db.Мастер
                .AsEnumerable()
                .Select(м => new SelectListItem
                {
                    Value = GetMasterFullName(м), // Полное ФИО как значение
                    Text = GetMasterShortName(м) // Короткое отображение "Фамилия И.О."
                })
                .ToList();

            // Подготовка списка всех работ для чекбоксов
            ViewBag.WorkList = db.Работа.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы
            }).ToList();

            return View(new RepairVM());
        }

        // POST: Repairs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RepairVM repairVM, string[] selectedWorks)
        {
            if (ModelState.IsValid)
            {
                using (var dbContext = new IDZEntities1())
                {
                    // Разбиваем строку ФИО на отдельные части
                    string[] parts = repairVM.ФИО_мастера?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    string фамилия = parts.Length > 0 ? parts[0] : null;
                    string имя = parts.Length > 1 ? parts[1] : null;
                    string отчество = parts.Length > 2 ? parts[2] : null;

                    // Проверка существования мастера
                    if (!string.IsNullOrEmpty(фамилия) && !string.IsNullOrEmpty(имя))
                    {
                        var выбранныйМастер = db.Мастер.FirstOrDefault(м =>
                            м.Фамилия == фамилия &&
                            м.Имя == имя &&
                            (м.Отчество == отчество || string.IsNullOrEmpty(отчество))
                        );

                        if (выбранныйМастер == null)
                        {
                            ModelState.AddModelError("", "Выбранный мастер не существует.");
                            return View(repairVM);
                        }
                    }

                    // Преобразуем массив выбранных работ в строку с разделителями
                    string workList = selectedWorks?.Length > 0 ? string.Join(",", selectedWorks.Select(int.Parse)) : null;

                    // Вызываем хранимую процедуру
                    try
                    {
                        dbContext.ЗаявкаНаРемонт(
                            repairVM.Инвентарный_номер,
                            фамилия,
                            имя,
                            отчество,
                            workList
                        );

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Ошибка при создании ремонта: " + ex.Message);
                    }
                }
            }

            // Если модель недействительна или возникла ошибка, перезагружаем форму
            ViewBag.MasterList = db.Мастер
                .AsEnumerable()
                .Select(м => new SelectListItem
                {
                    Value = GetMasterFullName(м), // Полное ФИО как значение
                    Text = GetMasterShortName(м)  // Короткое отображение "Фамилия И.О."
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

            return View(repairVM);
        }

        // GET: Repairs/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            // Находим ремонт по ID
            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Мастера)
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound();
            }

            // Создаем ViewModel
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

            // Подготовка списка мастеров для выпадающего списка
            ViewBag.MasterList = db.Мастер
                .AsEnumerable() // Выгружаем данные в память
                .Select(м => new SelectListItem
                {
                    Value = м.ID_мастера.ToString(),
                    Text = GetMasterShortName(м), // Формат "Фамилия И.О."
                    Selected = ремонт.ID_мастера == м.ID_мастера
                }).ToList();

            // Подготовка списка всех работ для чекбоксов
            var все_работы = db.Работа.ToList(); // Выгрузка в память
            var текущие_работы = ремонт.Работы.Select(w => w.Код_работы).ToList(); // Список ID текущих работ

            ViewBag.WorkList = все_работы.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы,
                Selected = текущие_работы.Contains(р.Код_работы)
            }).ToList();

            return View(repairVM);
        }

        // Вспомогательная функция для формирования ФИО мастера в формате "Фамилия И.О."
        private string GetMasterShortName(Мастер мастер)
        {
            if (мастер == null)
            {
                return "Не назначен";
            }

            return $"{мастер.Фамилия} {мастер.Имя.FirstOrDefault()}. {мастер.Отчество?.FirstOrDefault() ?? ' '}.".Trim();
        }
        // POST: Repairs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_ремонта,Дата_начала,Дата_окончания,ID_мастера")] RepairVM repairVM, string[] selectedWorks)
        {
            if (ModelState.IsValid)
            {
                // Находим существующий ремонт
                var ремонт = db.Ремонты_и_техобслуживания.Find(repairVM.ID_ремонта);

                if (ремонт == null)
                {
                    return HttpNotFound();
                }

                // Обновляем данные о ремонте
                ремонт.Дата_начала = repairVM.Дата_начала;
                ремонт.Дата_окончания = repairVM.Дата_окончания;
                ремонт.ID_мастера = repairVM.ID_мастера;

                // Обновляем список работ
                UpdateRepairWorks(ремонт, selectedWorks);

                db.Entry(ремонт).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Если модель недействительна, перезагружаем форму
            ViewBag.MasterList = new SelectList(db.Мастер, "ID_мастера", "Фамилия", repairVM.ID_мастера);
            ViewBag.WorkList = GetWorkList(db.Работа.ToList(), repairVM.Работы);

            return View(repairVM);
        }

        // Вспомогательная функция для обновления работ
        private void UpdateRepairWorks(Ремонты_и_техобслуживания ремонт, string[] selectedWorks)
        {
            if (selectedWorks == null || selectedWorks.Length == 0)
            {
                // Если нет выбранных работ, очищаем список
                ремонт.Работы.Clear();
                return;
            }

            var workIds = selectedWorks.Select(int.Parse).ToList(); // Преобразуем строки в целые числа
            var currentWorks = ремонт.Работы.Select(w => w.Код_работы).ToList(); // Текущие работы

            // Удаляем работы, которые больше не выбраны
            foreach (var работа in ремонт.Работы.ToList())
            {
                if (!workIds.Contains(работа.Код_работы))
                {
                    ремонт.Работы.Remove(работа);
                }
            }

            // Добавляем новые выбранные работы
            foreach (var workId in workIds)
            {
                if (!currentWorks.Contains(workId))
                {
                    var работа = db.Работа.Find(workId); // Находим работу по ID
                    if (работа != null)
                    {
                        ремонт.Работы.Add(работа); // Добавляем работу к ремонту
                    }
                }
            }
        }

        // Вспомогательная функция для создания списка работ
        private List<SelectListItem> GetWorkList(List<Работа> все_работы, List<Работа> текущие_работы)
        {
            return все_работы.Select(р => new SelectListItem
            {
                Value = р.Код_работы.ToString(),
                Text = р.Наименование_работы,
                Selected = текущие_работы.Any(w => w.Код_работы == р.Код_работы) // Помечаем выбранные работы
            }).ToList();
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var ремонт = db.Ремонты_и_техобслуживания
                .Include(r => r.Оборудование)
                .Include(r => r.Работы)
                .FirstOrDefault(r => r.ID_ремонта == id);

            if (ремонт == null)
            {
                return HttpNotFound();
            }

            // Создаем ViewModel
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

            return View(repairVM);
        }

        // POST: Repairs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var ремонт = db.Ремонты_и_техобслуживания.Find(id);

            if (ремонт == null)
            {
                return HttpNotFound();
            }

            try
            {

                // Удаляем сам ремонт
                db.Ремонты_и_техобслуживания.Remove(ремонт);

                // Обновляем состояние оборудования на "в работе" (true)
                var оборудование = ремонт.Оборудование;
                if (оборудование != null && !оборудование.Состояние)
                {
                    оборудование.Состояние = true; // Возвращаем оборудование в рабочее состояние
                    db.Entry(оборудование).State = EntityState.Modified;
                }

                // Сохраняем изменения
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ошибка при удалении ремонта: " + ex.Message);
                return View("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}