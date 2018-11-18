using System;
using System.Windows.Forms;
using CommunicationDevices.Verification;
using System.IO;

namespace MainExample
{
    public partial class AboutForm : Form
    {
        private readonly IVerificationActivation _verificationActivation;
        
        #region ctor

        public AboutForm(IVerificationActivation verificationActivation)
        {
            _verificationActivation = verificationActivation;
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void DisplayVersion()
        {
            try
            {
                using (StreamReader file = new StreamReader("version"))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        label6.Text = $"Версия: {line}";
                    }
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error($"Файл версии программы не найден. {ex}");
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            tb_ActivationInfo.Text = $@"До блокировки программы осталось {_verificationActivation.GetDeltaDayBeforeBlocking()} дней";
            DisplayVersion();
            base.OnLoad(e);
        }
        
        private void btn_activationWindow_Click(object sender, EventArgs e)
        {
            if (BlockingForm.MyMainForm == null)
            {
                var blockingForm = new BlockingForm(_verificationActivation);
                blockingForm.ShowDialog();

                tb_ActivationInfo.Text = $@"До блокировки программы осталось {_verificationActivation.GetDeltaDayBeforeBlocking()} дней";
            }
        }
    }
}
