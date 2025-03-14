﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebIDZ.Models;
using WebIDZ.Models.ViewModels;

namespace WebIDZ.Controllers
{
    [Authorize] // Доступ к контроллеру разрешен только авторизованным пользователям.
    public class HardwareController : Controller
    {
        private IDZEntities1 db = new IDZEntities1(); // Контекст базы данных.

        // GET: Hardware
        /// <summary>
        /// Метод для отображения списка оборудования.
        /// </summary>
        /// <returns>Представление со списком оборудования, отсортированным по состоянию.</returns>
        public ActionResult Index()
        {
            var оборудование = db.Оборудование
                .Select(x => new HardwareVM
                {
                    ID_оборудования = x.ID_оборудования,
                    Название = x.Название,
                    Дата_ввода_в_эскплуатацию = x.Дата_ввода_в_эскплуатацию,
                    Местоположение = x.Местоположение,
                    Состояние = x.Состояние,
                    Инвентарный_номер = x.Инвентарный_номер
                })
                .OrderBy(x => x.Состояние).ThenBy(x => x.Инвентарный_номер) // Оборудование сортируется по состоянию (сначала "Ремонтируются", затем "В работе"), после сортируется по инвентарному номеру.
                .ToList();

            return View(оборудование);
        }

        // GET: Hardware/Details/5
        /// <summary>
        /// Метод для отображения детальной информации об оборудовании.
        /// </summary>
        /// <param name="id">Идентификатор оборудования.</param>
        /// <returns>Представление с детальной информацией об оборудовании.</returns>
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если идентификатор не указан.
            }

            Оборудование оборудование = db.Оборудование
                .Include(o => o.Типы_оборудования) // Включаем связанный тип оборудования для получения дополнительной информации.
                .FirstOrDefault(o => o.ID_оборудования == id);

            if (оборудование == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если оборудование не найдено.
            }

            var оборудованиеVM = new HardwareVM
            {
                ID_оборудования = оборудование.ID_оборудования,
                Название = оборудование.Название,
                Дата_ввода_в_эскплуатацию = оборудование.Дата_ввода_в_эскплуатацию,
                Местоположение = оборудование.Местоположение,
                Состояние = оборудование.Состояние,
                Инвентарный_номер = оборудование.Инвентарный_номер,
                ТипОборудования = оборудование.Типы_оборудования?.Наименование,
                ЧастотаОбслуживания = $"{оборудование.Типы_оборудования?.Количество_раз_обслуживания} раз(а) в {оборудование.Типы_оборудования?.Отрезок_времени}"
            };

            return View(оборудованиеVM);
        }

        // GET: Hardware/Create
        /// <summary>
        /// Метод для отображения формы создания нового оборудования.
        /// </summary>
        /// <returns>Представление для создания нового оборудования.</returns>
        [HttpGet]
        public ActionResult Create()
        {
            // Подготовка выпадающего списка типов оборудования
            ViewBag.Код_типа = new SelectList(db.Тип_оборудования, "Код_типа", "Наименование");

            // Подготовка выпадающего списка местоположений
            var местоположения = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList();
            ViewBag.Местоположение = new SelectList(местоположения);

            return View();
        }

        // POST: Hardware/Create
        /// <summary>
        /// Метод для создания нового оборудования.
        /// </summary>
        /// <param name="оборудование">Данные нового оборудования.</param>
        /// <returns>Перенаправление на список оборудования или представление с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_оборудования,Название,Дата_ввода_в_эскплуатацию,Местоположение,Состояние,Код_типа,Инвентарный_номер")] Оборудование оборудование)
        {
            if (ModelState.IsValid)
            {
                // Проверяем уникальность инвентарного номера
                if (db.Оборудование.Any(e => e.Инвентарный_номер == оборудование.Инвентарный_номер))
                {
                    ModelState.AddModelError("", "Оборудование с таким инвентарным номером уже существует.");
                    ViewBag.Код_типа = new SelectList(db.Тип_оборудования, "Код_типа", "Наименование");
                    var locations = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList();
                    ViewBag.Местоположение = new SelectList(locations);
                    return View(оборудование);
                }

                оборудование.ID_оборудования = Guid.NewGuid(); // Генерируем уникальный идентификатор.
                db.Оборудование.Add(оборудование); // Добавляем новое оборудование в базу данных.
                db.SaveChanges(); // Сохраняем изменения.

                return RedirectToAction("Index"); // Перенаправляем на страницу со списком оборудования.
            }

            // Если модель недействительна, возвращаем представление с ошибками.
            ViewBag.Код_типа = new SelectList(db.Тип_оборудования, "Код_типа", "Наименование");
            var местоположения = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList();
            ViewBag.Местоположение = new SelectList(местоположения);

            return View(оборудование);
        }

        // GET: Hardware/Edit/5
        /// <summary>
        /// Метод для отображения формы редактирования оборудования.
        /// </summary>
        /// <param name="id">Идентификатор оборудования.</param>
        /// <returns>Представление для редактирования оборудования.</returns>
        [HttpGet]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            // Находим оборудование по ID
            var equipment = db.Оборудование
                .Include(e => e.Типы_оборудования) // Включаем связанный тип оборудования
                .FirstOrDefault(e => e.ID_оборудования == id);

            if (equipment == null)
            {
                return HttpNotFound();
            }

            // Создаем ViewModel на основе объекта оборудования
            var hardwareVM = new HardwareVM
            {
                ID_оборудования = equipment.ID_оборудования,
                Название = equipment.Название,
                Дата_ввода_в_эскплуатацию = equipment.Дата_ввода_в_эскплуатацию,
                Местоположение = equipment.Местоположение,
                Состояние = equipment.Состояние,
                Инвентарный_номер = equipment.Инвентарный_номер,
                ТипОборудования = equipment.Типы_оборудования?.Наименование, // Заполняем название типа оборудования
                ЧастотаОбслуживания = equipment.Типы_оборудования?.Отрезок_времени // Заполняем частоту обслуживания
            };


            // Подготовка списка типов оборудования для выпадающего списка
            var hardwareTypes = db.Тип_оборудования.Select(o => o.Наименование).Distinct().ToList();
            ViewBag.TypeList = hardwareTypes
                .AsEnumerable() // Выгружаем данные в память
                .Select(t => new SelectListItem
                {
                    Value = t ?? "", // Обработка null значений
                    Text = t ?? "Не указано", // Если тип не указан, отображаем "Не указано"
                    Selected = equipment.Типы_оборудования.Наименование == t // Устанавливаем выбранное значение
                }).ToList();

            // Подготовка списка местоположений для выпадающего списка
            var locations = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList(); // Получаем уникальные местоположения
            ViewBag.LocationList = locations
                .AsEnumerable() // Выгружаем данные в память
                .Select(l => new SelectListItem
                {
                    Value = l ?? "", // Обработка null значений
                    Text = l ?? "Не указано", // Если местоположение не указано, отображаем "Не указано"
                    Selected = equipment.Местоположение == l // Устанавливаем выбранное значение
                }).ToList();

            return View(hardwareVM);
        }

        // POST: Hardware/Edit/5
        /// <summary>
        /// Метод для сохранения изменений при редактировании оборудования.
        /// </summary>
        /// <param name="editedHardware">Данные оборудования после редактирования.</param>
        /// <returns>Перенаправление на список оборудования или представление с ошибками.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HardwareVM editedHardware)
        {
            if (ModelState.IsValid)
            {
                // Находим существующее оборудование по ID
                var equipment = db.Оборудование.Find(editedHardware.ID_оборудования);

                if (equipment == null)
                {
                    return HttpNotFound();
                }

                // Проверяем уникальность инвентарного номера
                var existingEquipmentWithSameNumber = db.Оборудование
                    .Any(e => e.Инвентарный_номер == editedHardware.Инвентарный_номер && e.ID_оборудования != editedHardware.ID_оборудования);

                if (existingEquipmentWithSameNumber)
                {
                    ModelState.AddModelError("Инвентарный_номер", "Этот инвентарный номер уже используется другим оборудованием.");

                    // Перезаполняем выпадающие списки
                    var typesList = db.Тип_оборудования.Select(o => o.Наименование).Distinct().ToList();
                    ViewBag.TypeList = typesList
                        .AsEnumerable()
                        .Select(t => new SelectListItem
                        {
                            Value = t ?? "",
                            Text = t ?? "Не указано",
                            Selected = t == editedHardware.ТипОборудования
                        }).ToList();

                    var locationsList = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList();
                    ViewBag.LocationList = locationsList
                        .AsEnumerable()
                        .Select(l => new SelectListItem
                        {
                            Value = l ?? "",
                            Text = l ?? "Не указано",
                            Selected = l == editedHardware.Местоположение
                        }).ToList();

                    return View(editedHardware);
                }

                // Обновляем свойства оборудования
                equipment.Название = editedHardware.Название;
                equipment.Дата_ввода_в_эскплуатацию = editedHardware.Дата_ввода_в_эскплуатацию;
                equipment.Местоположение = editedHardware.Местоположение;
                equipment.Инвентарный_номер = editedHardware.Инвентарный_номер;

                // Получаем выбранное значение типа оборудования из формы
                string selectedTypeName = Request.Form["ТипОборудования"]; // Выносим значение в локальную переменную

                // Находим тип оборудования по имени
                var selectedType = db.Тип_оборудования.FirstOrDefault(t => t.Наименование == selectedTypeName); // Используем локальную переменную

                if (selectedType != null)
                {
                    equipment.Код_типа = selectedType.Код_типа;
                }
                else
                {
                    // Если тип оборудования не найден, можно добавить сообщение об ошибке
                    ModelState.AddModelError("ТипОборудования", "Выбранный тип оборудования не существует.");
                    return View(editedHardware);
                }

                // Сохраняем изменения в базе данных
                db.Entry(equipment).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Если модель недействительна, перезаполняем выпадающие списки
            var hardwareTypes = db.Тип_оборудования.Select(o => o.Наименование).Distinct().ToList();
            ViewBag.TypeList = hardwareTypes
                .AsEnumerable()
                .Select(t => new SelectListItem
                {
                    Value = t ?? "",
                    Text = t ?? "Не указано",
                    Selected = t == editedHardware.ТипОборудования
                }).ToList();

            var locations = db.Оборудование.Select(o => o.Местоположение).Distinct().ToList();
            ViewBag.LocationList = locations
                .AsEnumerable()
                .Select(l => new SelectListItem
                {
                    Value = l ?? "",
                    Text = l ?? "Не указано",
                    Selected = l == editedHardware.Местоположение
                }).ToList();

            return View(editedHardware);
        }

        // GET: Hardware/Delete/5
        /// <summary>
        /// Метод для подтверждения удаления оборудования.
        /// </summary>
        /// <param name="id">Идентификатор оборудования.</param>
        /// <returns>Представление для подтверждения удаления.</returns>
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Возвращаем ошибку, если идентификатор не указан.
            }

            Оборудование оборудование = db.Оборудование.Find(id);

            if (оборудование == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если оборудование не найдено.
            }

            return View(оборудование);
        }

        // POST: Hardware/Delete/5
        /// <summary>
        /// Метод для удаления оборудования.
        /// </summary>
        /// <param name="id">Идентификатор оборудования.</param>
        /// <returns>Перенаправление на список оборудования.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Оборудование оборудование = db.Оборудование.Find(id);

            if (оборудование == null)
            {
                return HttpNotFound(); // Возвращаем ошибку, если оборудование не найдено.
            }

            db.Оборудование.Remove(оборудование); // Удаляем оборудование из базы данных.
            db.SaveChanges(); // Сохраняем изменения.

            return RedirectToAction("Index"); // Перенаправляем на страницу со списком оборудования.
        }

        /// <summary>
        /// Освобождает используемые ресурсы.
        /// </summary>
        /// <param name="disposing">true, если нужно освободить управляемые ресурсы.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Освобождаем контекст базы данных.
            }
            base.Dispose(disposing);
        }
    }
}