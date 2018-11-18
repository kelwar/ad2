using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Domain.Entitys;

namespace MainExample
{
    public partial class EditVagonDialog : Form
    {
        private Vagon _vagon;

        public EditVagonDialog(Vagon vagon)
        {
            InitializeComponent();
            _vagon = vagon;
            cmbPsType.Items.AddRange(Enum.GetValues(typeof(PsType)).Cast<object>().ToArray());
            cmbVagonType.Items.AddRange(Enum.GetValues(typeof(VagonType)).Cast<object>().ToArray());
        }

        private void AddVagonDialog_Load(object sender, EventArgs e)
        {
            numVagonId.Value = _vagon.VagonId;
            txtVagonNumber.Text = _vagon.VagonNumber.ToString();
            cmbPsType.SelectedItem = _vagon.PsType;
            cmbVagonType.SelectedItem = _vagon.VagonType;
            numLength.Value = _vagon.Length;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _vagon.VagonId = (int)numVagonId.Value;
            int num;
            _vagon.VagonNumber = int.TryParse(txtVagonNumber.Text, out num) ? num : -1;
            _vagon.PsType = (PsType)cmbPsType.SelectedItem;
            _vagon.VagonType = (VagonType)cmbVagonType.SelectedItem;
            _vagon.Length = (int)numLength.Value;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
