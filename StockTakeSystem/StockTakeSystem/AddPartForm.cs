using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockTakeSystem
{
    public partial class AddPartForm : Form
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Morgan\\Documents\\InventoryDB.mdf;Integrated Security=True;Connect Timeout=30";
        public AddPartForm()
        {
            InitializeComponent();
        }
        // Properties to expose input values
        public string PartName { get { return txtName.Text; } }
        public string PartDescription { get { return txtDescription.Text; } }
        public decimal PartPrice { get { return numericUpDownPrice.Value; } }
        public int PartQuantity { get { return (int)numericUpDownQuantity.Value; } }
        public string PartBarcode { get { return txtBarcode.Text; } }

        private void AddPartForm_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Close the form with DialogResult.Cancel
            this.DialogResult = DialogResult.Cancel;
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

            // Connect to the database and execute the INSERT statement
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Parts (Name, Description, Price, QuantityOnHand, Barcode) " +
                                   "VALUES (@Name, @Description, @Price, @QuantityOnHand, @Barcode)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", txtName.Text);
                        command.Parameters.AddWithValue("@Description", txtDescription.Text);
                        command.Parameters.AddWithValue("@Price", numericUpDownPrice.Value);
                        command.Parameters.AddWithValue("@QuantityOnHand", (int)numericUpDownQuantity.Value);
                        command.Parameters.AddWithValue("@Barcode", txtBarcode.Text);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Part added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show("Failed to add part!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
