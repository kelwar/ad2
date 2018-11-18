﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domain.Entitys.Authentication;

namespace MainExample
{
    public partial class AdminForm : Form
    {
        public List<User> Users { get; set; }



        public AdminForm()
        {
            InitializeComponent();

            btn_Load_Click(null, EventArgs.Empty);
        }





        private void FillTable(IEnumerable<User> users )
        {
            //Очистить грид.
            dgv_пользователи.Rows.Clear();

            //Заполнить ComboBox вариантами выбора.
            var column = dgv_пользователи.Columns[2] as DataGridViewComboBoxColumn;
            if (column != null)
            {
                column.DataSource = Enum.GetValues(typeof(Role)).Cast<Role>().Where(r=> (r != Role.Администратор) && (r != Role.Наблюдатель)).Select(r => r.ToString()).ToArray();
                column.DataPropertyName = "Role";
            }

            //Заполнить грид.
            for (int i= 0; i < Users.Count; i++)
            {
                var user = Users[i];

                dgv_пользователи.Rows.Add();
                dgv_пользователи["clLogin", i].Value = user.Login;
                dgv_пользователи["clPassword", i].Value = user.Password;

                DataGridViewComboBoxCell cell = dgv_пользователи["clRole", i] as DataGridViewComboBoxCell;
                if (cell != null)
                {
                    cell.Value = user.Role.ToString();
                }
            }
        }






        #region EventHander

        /// <summary>
        /// Нумерация строк
        /// </summary>
        private void dgv_RowPrePaint(object sender,DataGridViewRowPrePaintEventArgs e)
        {
            object head = this.dgv_пользователи.Rows[e.RowIndex].HeaderCell.Value;
            if (head == null || !head.Equals((e.RowIndex + 1).ToString()))
                this.dgv_пользователи.Rows[e.RowIndex].HeaderCell.Value =
                    (e.RowIndex + 1).ToString();
        }



        /// <summary>
        /// Загрузить из БД
        /// </summary>
        private void btn_Load_Click(object sender, EventArgs e)
        {
            Users = Program.UsersDbRepository.List(user => user.Role != Role.Администратор).ToList();
            FillTable(Users);
        }


        /// <summary>
        /// Сохранит в БД
        /// </summary>
        private void btn_Save_Click(object sender, EventArgs e)
        {
            Users.Clear();

            for (int i = 0; i < dgv_пользователи.Rows.Count - 1; ++i)
            {
                var login = (string)dgv_пользователи["clLogin", i].Value;
                var password = (string)dgv_пользователи["clPassword", i].Value;
                var role = (string)dgv_пользователи["clRole", i].Value;

                if (string.IsNullOrEmpty(login) || string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show($@"Задайте верно логин в строке: {i+1}");
                    return;
                }
                if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show($@"Задайте верно Пароль в строке: {i + 1}");
                    return;
                }
                if (string.IsNullOrEmpty(role) || string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show($@"Задайте верно Роль в строке: {i + 1}");
                    return;
                }

                Users.Add(Program.AuthenticationService.CreateUser(login, password, (Role)Enum.Parse(typeof(Role), role)));
            }


            //удалим всех пользователей кроме Админа.
            Program.UsersDbRepository.Delete(user => user.Role != Role.Администратор);

            //Добавим из таблицы
            Program.UsersDbRepository.AddRange(Users);

        }


        private void btn_сменитьПароль_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_ТекПароль.Text) || string.IsNullOrWhiteSpace(tb_ТекПароль.Text))
            {
                MessageBox.Show(@"Введите ТЕКУЩИЙ пароль");
            }
            if (string.IsNullOrEmpty(tb_НовыйПароль.Text) || string.IsNullOrWhiteSpace(tb_НовыйПароль.Text))
            {
                MessageBox.Show(@"Введите НОВЫЙ пароль");
            }

            var currentPassword = tb_ТекПароль.Text;
            var newPassword = tb_НовыйПароль.Text;
            var adminUser = Program.UsersDbRepository.List().FirstOrDefault(user => user.Role == Role.Администратор && user.IsEnabled);
            if (adminUser != null)
            {
                if (adminUser.Password == currentPassword)
                {
                    adminUser.Password = newPassword;
                    Program.UsersDbRepository.Edit(adminUser);
                    MessageBox.Show(@"ПАРОЛЬ УСПЕШНО ИЗМЕНЕН");
                }
                else
                {
                    MessageBox.Show(@"текущий пароль не верен !!!");
                }
            }
        }


        /// <summary>
        /// Пустой обработчик ошибок в данных грида. 
        /// </summary>
        private void dgv_пользователи_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void btnSoundPlayer_Click(object sender, EventArgs e)
        {
            if (SoundPlayersForm.MyMainForm == null)
            {
                var soundPlayers = new SoundPlayersForm();
                soundPlayers.Show();
            }
        }
		
        #endregion

    }
}
