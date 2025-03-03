using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebIDZ.Models;
using WebIDZ.Models.ViewModels;

namespace WebIDZ.Controllers
{
    public class AccountController : Controller
    {
        // Метод для обработки POST-запроса авторизации пользователя.
        /// <summary>
        /// Обрабатывает запрос на авторизацию пользователя.
        /// </summary>
        /// <param name="webUser">Данные, введенные пользователем (логин и пароль).</param>
        /// <returns>Перенаправление на главную страницу или представление с ошибкой.</returns>
        [AllowAnonymous] // Разрешает доступ неавторизованным пользователям.
        [HttpPost]
        public ActionResult Login(UserVM webUser)
        {
            if (ModelState.IsValid) // Проверяем валидность данных формы.
            {
                using (IDZEntities1 context = new IDZEntities1()) // Создаем контекст базы данных.
                {
                    Пользователи user = null; // Инициализируем переменную для хранения найденного пользователя.
                    user = context.Пользователи.Where(u => u.Логин == webUser.Login).FirstOrDefault(); // Ищем пользователя по логину.

                    if (user != null) // Если пользователь найден.
                    {
                        // Генерируем хеш пароля с использованием соли из базы данных.
                        string passwordHash = ReturnHashCode(webUser.Password + user.Salt.ToString().ToUpper());

                        if (passwordHash == user.PasswordHash) // Сравниваем полученный хеш с хешем из базы данных.
                        {
                            string userRole = ""; // Определяем роль пользователя.

                            // Определяем роль пользователя на основе значения UserRole.
                            switch (user.UserRole)
                            {
                                case 1:
                                    userRole = "Admin"; // Администратор.
                                    break;
                                case 2:
                                    userRole = "Worker"; // Рабочий.
                                    break;
                            }

                            // Создаем билет аутентификации FormsAuthenticationTicket.
                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                1, // Версия билета.
                                user.Логин, // Логин пользователя.
                                DateTime.Now, // Время создания билета.
                                DateTime.Now.AddDays(1), // Время истечения срока действия билета (1 день).
                                false, // Не сохранять куки持久ently.
                                userRole // Роль пользователя.
                            );

                            // Шифруем билет аутентификации.
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                            // Добавляем зашифрованный билет в HTTP-куки.
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));

                            // Перенаправляем пользователя на страницу со списком ремонтов.
                            return RedirectToAction("Index", "Repairs");
                        }
                    }
                }
            }

            // Если данные неверны, возвращаем представление с сообщением об ошибке.
            ViewBag.Error = "Пользователя с таким логином и паролем не существует, попробуйте ещё.";
            return View(webUser);
        }

        // Метод для отображения формы входа.
        /// <summary>
        /// Отображает форму входа для пользователя.
        /// </summary>
        /// <returns>Представление формы входа.</returns>
        [AllowAnonymous] // Разрешает доступ неавторизованным пользователям.
        [HttpGet]
        public ActionResult Login()
        {
            return View(); // Возвращает представление формы входа.
        }

        // Метод для вычисления хеша SHA1.
        /// <summary>
        /// Вычисляет хеш SHA1 для заданной строки.
        /// </summary>
        /// <param name="loginAndSalt">Строка, состоящая из пароля и соли.</param>
        /// <returns>Хеш SHA1 в верхнем регистре.</returns>
        private string ReturnHashCode(string loginAndSalt)
        {
            string hash = ""; // Инициализируем переменную для хранения хеша.

            using (SHA1 sha1hash = SHA1.Create()) // Создаем объект SHA1 для вычисления хеша.
            {
                // Вычисляем хеш для входной строки.
                byte[] data = sha1hash.ComputeHash(Encoding.UTF8.GetBytes(loginAndSalt));
                StringBuilder sBuilder = new StringBuilder();

                // Преобразуем массив байтов в шестнадцатеричную строку.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                hash = sBuilder.ToString().ToUpper(); // Преобразуем хеш в верхний регистр.
            }

            return hash; // Возвращаем полученный хеш.
        }

        // Метод для выхода пользователя из системы.
        /// <summary>
        /// Выполняет выход пользователя из системы.
        /// </summary>
        /// <returns>Перенаправление на страницу входа.</returns>
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut(); // Выполняем выход пользователя из системы.
            return RedirectToAction("Login", "Account"); // Перенаправляем пользователя на страницу входа.
        }
    }
}