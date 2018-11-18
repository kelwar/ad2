using Domain.Abstract;
using Domain.Concrete;
using Domain.Entitys;
using Library.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MainExample
{
    public partial class StationsForm : Form
    {
        #region Fields

        private static StationsForm _stationsForm;
        private RepositoryXmlDirection _directions;
        private Direction _selectedDirection;
        private Station _selectedStation;

        #endregion

        

        public StationsForm(IRepository<Direction> directions)
        {
            if (_stationsForm != null)
                return;
            _stationsForm = this;

            InitializeComponent();

            _directions = (RepositoryXmlDirection)directions;
        }

        private void ApplyChange()
        {
            _directions.Save();
        }

        private void lvDirections_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedDirection = _directions.GetByName(lvDirections.SelectedItems[0].Text);
            dgv_Stations.DataSource = _selectedDirection.Stations;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            ApplyChange();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ApplyChange();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void StationsForm_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (var dir in _directions.Load())
                {
                    var lvi = new ListViewItem();
                    lvi.Text = dir.Name;
                    lvDirections.Items.Add(lvi);
                }
            }
            catch (Exception ex)
            {
                Library.Logs.Log.log.Error(ex);
            }
        }

        private void StationsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_stationsForm == this)
                _stationsForm = null;

            base.OnClosing(e);
        }

        private void btnAddDirection_Click(object sender, EventArgs e)
        {
            _selectedDirection = new Direction(_directions.List().Max(dir => dir.Id) + 1,
                                               lvDirections.SelectedItems[0].Text);

            _directions.Add(_selectedDirection);

            lvDirections.Refresh();
        }

        private void btnRemoveDirection_Click(object sender, EventArgs e)
        {
            _directions.Delete(_selectedDirection);
            _selectedDirection = null;

            lvDirections.Refresh();
        }

        private void dgv_Stations_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _selectedStation = (Station)dgv_Stations.SelectedRows[0].DataBoundItem;
        }

        private void btnAddStation_Click(object sender, EventArgs e)
        {
            //dgv_Stations.Rows.Add(;
            //_selectedStation = new Station
            //{
            //    Id = _selectedDirection.Stations.Max(st => st.Id) + 1,
            //    NameRu = dgv_Stations.SelectedCells[0].Value.ToString()
            //};

            //_selectedDirection.Stations.Add(_selectedStation);

            dgv_Stations.Refresh();
        }

        private void btnRemoveStation_Click(object sender, EventArgs e)
        {
            //dgv_Stations.Rows.Remove(dgv_Stations.SelectedRows[0]);
            _selectedDirection.Stations.Remove(_selectedStation);
            _selectedStation = null;

            dgv_Stations.Refresh();
        }

        private void btnMoveStationUp_Click(object sender, EventArgs e)
        {
            var index = _selectedDirection.Stations.IndexOf(_selectedStation);

            if (index > 0)
            {
                _selectedDirection.Stations.Remove(_selectedStation);
                _selectedDirection.Stations.Insert(index - 1, _selectedStation);
            }

            dgv_Stations.Refresh();
        }

        private void MoveStationDown_Click(object sender, EventArgs e)
        {
            var index = _selectedDirection.Stations.IndexOf(_selectedStation);

            if (index < _selectedDirection.Stations.Count - 1)
            {
                _selectedDirection.Stations.Remove(_selectedStation);
                _selectedDirection.Stations.Insert(index + 1, _selectedStation);
            }

            dgv_Stations.Refresh();
        }

        private void btnMoveDirectionUp_Click(object sender, EventArgs e)
        {
            var index = _directions.List().ToList().IndexOf(_selectedDirection);

            if (index > 0)
            {
                _directions.Delete(_selectedDirection);
                _directions.List().ToList().Insert(index - 1, _selectedDirection);
            }

            lvDirections.Refresh();
        }

        private void btnMoveDirectionDown_Click(object sender, EventArgs e)
        {
            var index = _directions.List().ToList().IndexOf(_selectedDirection);

            if (index < _directions.List().Count() - 1)
            {
                _directions.Delete(_selectedDirection);
                _directions.List().ToList().Insert(index + 1, _selectedDirection);
            }

            lvDirections.Refresh();
        }

        private void dgv_Stations_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            _selectedStation = (Station)dgv_Stations.Rows[0].DataBoundItem;
        }
    }
}
