using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationDevices.Behavior.GetDataBehavior;
using CommunicationDevices.DataProviders;
using Library.Logs;
using Domain.Entitys.Authentication;

namespace MainExample.Services.GetDataService
{
    class GetCisUsersDb : GetSheduleAbstract
    {
        public GetCisUsersDb(BaseGetDataBehavior baseGetDataBehavior, SortedDictionary<string, SoundRecord> soundRecords) 
            : base(baseGetDataBehavior, soundRecords)
        {
        }

        public override void GetaDataRxEventHandler(IEnumerable<UniversalInputType> data)
        {
            try
            {
                if (!Enable)
                    return;

                var universalInputTypes = data as IList<UniversalInputType> ?? data.ToList();
                if (universalInputTypes.Any())
                {
                    var usersDb = Program.UsersDbRepository.List().ToList();
                    if (usersDb == null)
                        return;
                    usersDb.Clear();
                    Program.UsersDbRepository.Delete(u => true);
                    foreach (var uit in universalInputTypes)
                    {
                        try
                        {
                            User user = new User();

                            user.Id = uit.Id;
                            user.Login = uit.ViewBag["login"];
                            user.Password = uit.ViewBag["hash_salt_pass"];
                            int role_id = uit.ViewBag["role"];
                            switch (role_id)
                            {
                                case 9:
                                    user.Role = Role.Диктор; break; // users - Основные пользователи
                                case 1:
                                    user.Role = Role.Администратор; break; // imperator - Основная роль root админа
                                case 7:
                                    user.Role = Role.Администратор; break; // administrator - Администратор ЦТС ПАСС
                                case 8:
                                    user.Role = Role.Инженер; break; // sysadmin - Администратор ПТК
                                case 3:
                                    user.Role = Role.Инженер; break; // apiReaders - Для чтения с API
                                case 4:
                                    user.Role = Role.Инженер; break; // system - system
                                case 5:
                                    user.Role = Role.Инженер; break; // daemon - Демоны
                                default:
                                    user.Role = Role.Наблюдатель; break; // любой недокументированный id
                            }
                            user.IsEnabled = uit.ViewBag["status"];
                            user.StartDate = uit.ViewBag["start_date"];
                            user.EndDate = uit.ViewBag["end_date"];

                            usersDb.Add(user);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show("Не получилось обновить репозиторий. Ошибка: " + ex);
                        }
                    }
                    Program.UsersDbRepository.AddRange(usersDb);
                }
            }
            catch (Exception ex)
            {
                Log.log.Error(ex);
            }
        }
    }
}
