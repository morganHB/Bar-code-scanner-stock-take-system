using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace StockTakeSystem
{
    public partial class EditForm : Form
    {
        // Properties to hold the details of the part
        public string PartName { get; private set; }
        public string PartDescription { get; private set; }
        public decimal PartPrice { get; private set; }
        public int PartQuantity { get; private set; }
        public string PartBarcode { get; private set; }
        public EditForm(int partID, string name, string description, decimal price, int quantity, string barcode)
        {
            InitializeComponent();
            // Populate the textboxes with the details of the selected part
            txtName.Text = name;
            txtDescription.Text = description;
            numericUpDownPrice.Value = price;
            numericUpDownQuantity.Value = quantity;
            txtBarcode.Text = barcode;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a part name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Please enter a part barcode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set the properties with the edited details
            PartName = txtName.Text;
            PartDescription = txtDescription.Text;
            PartPrice = numericUpDownPrice.Value;
            PartQuantity = (int)numericUpDownQuantity.Value;
            PartBarcode = txtBarcode.Text;

            // Close the form
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
