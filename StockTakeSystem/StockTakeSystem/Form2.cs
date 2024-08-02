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
using ZXing;

namespace StockTakeSystem
{
    public partial class Main : Form
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Morgan\\Documents\\InventoryDB.mdf;Integrated Security=True;Connect Timeout=30";

        public Main()
        {
            InitializeComponent();
            dataGridViewPrint.SelectionChanged += DataGridViewPrint_SelectionChanged;
        }

        //*private void btnInventory_Click(object sender, EventArgs e)
       // {
            // Close Form2
           // this.Close();

            // Open the Inventory form
          //  Inventory inventoryForm = new Inventory();
           // inventoryForm.WindowState = FormWindowState.Maximized;
            //inventoryForm.Show();
       // }

        

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to select all data from the Parts table
                    string query = "SELECT * FROM Parts";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Create a DataTable to hold the data
                        DataTable dataTable = new DataTable();

                        // Load the data from the query into the DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Bind the DataTable to the DataGridView
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Open a form to add a new part
            AddPartForm addPartForm = new AddPartForm();
            DialogResult result = addPartForm.ShowDialog();

            // If the user clicked the "OK" button on the add part form
            if (result == DialogResult.OK)
            {
                try
                {
                    // Connect to the database
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Check if the barcode already exists
                        string barcodeQuery = "SELECT COUNT(*) FROM Parts WHERE Barcode = @Barcode";
                        using (SqlCommand barcodeCommand = new SqlCommand(barcodeQuery, connection))
                        {
                            barcodeCommand.Parameters.AddWithValue("@Barcode", addPartForm.PartBarcode);
                            int existingCount = (int)barcodeCommand.ExecuteScalar();
                            if (existingCount > 0)
                            {
                                MessageBox.Show("Part with the same barcode already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return; // Exit the method to prevent further execution
                            }
                        }

                        // SQL query to insert a new part into the Parts table
                        string query = "INSERT INTO Parts (Name, Description, Price, QuantityOnHand, Barcode) " +
                                       "VALUES (@Name, @Description, @Price, @QuantityOnHand, @Barcode)";

                        // Create a SqlCommand with parameters to avoid SQL injection
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Set parameter values from the add part form
                            command.Parameters.AddWithValue("@Name", addPartForm.PartName);
                            command.Parameters.AddWithValue("@Description", addPartForm.PartDescription);
                            command.Parameters.AddWithValue("@Price", addPartForm.PartPrice);
                            command.Parameters.AddWithValue("@QuantityOnHand", addPartForm.PartQuantity);
                            command.Parameters.AddWithValue("@Barcode", addPartForm.PartBarcode);

                            // Execute the query
                            int rowsAffected = command.ExecuteNonQuery();

                            // Check if the part was successfully added
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Part added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Refresh the DataGridView to show the new part
                                btnView.PerformClick(); // Trigger the btnView_Click event to reload the data
                            }
                            else
                            {
                                MessageBox.Show("Failed to add part!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Unique constraint violation error code
                    {
                        MessageBox.Show("Part with the same barcode already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Get the PartID from the selected row's "PartID" cell
                if (selectedRow.Cells["PartID"].Value != null && int.TryParse(selectedRow.Cells["PartID"].Value.ToString(), out int partID))
                {
                    // Prompt the user for confirmation
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this part?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Connect to the database and execute the DELETE statement
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                string query = "DELETE FROM Parts WHERE PartID = @PartID";

                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@PartID", partID);

                                    int rowsAffected = command.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        // Remove the selected row from the DataGridView
                                        dataGridView1.Rows.Remove(selectedRow);
                                        MessageBox.Show("Part deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to delete part!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                else
                {
                    MessageBox.Show("Unable to retrieve PartID for selected part.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a part to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // Get the details of the selected part
                int partID = Convert.ToInt32(selectedRow.Cells["PartID"].Value);
                string name = Convert.ToString(selectedRow.Cells["Name"].Value);
                string description = Convert.ToString(selectedRow.Cells["Description"].Value);
                decimal price = Convert.ToDecimal(selectedRow.Cells["Price"].Value);
                int quantity = Convert.ToInt32(selectedRow.Cells["QuantityOnHand"].Value);
                string barcode = Convert.ToString(selectedRow.Cells["Barcode"].Value);

                // Open the Edit form and pass the details of the selected part
                EditForm editForm = new EditForm(partID, name, description, price, quantity, barcode);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Update the DataGridView with the edited details
                    selectedRow.Cells["Name"].Value = editForm.PartName;
                    selectedRow.Cells["Description"].Value = editForm.PartDescription;
                    selectedRow.Cells["Price"].Value = editForm.PartPrice;
                    selectedRow.Cells["QuantityOnHand"].Value = editForm.PartQuantity;
                    selectedRow.Cells["Barcode"].Value = editForm.PartBarcode;
                }
            }
            else
            {
                MessageBox.Show("Please select a part to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Page 2
        private void InitializeDataGridViewInvoice()
        {
            // Clear existing columns
            dataGridView3.Columns.Clear();

            // Add columns
            dataGridView3.Columns.Add("Name", "Name");
            dataGridView3.Columns.Add("Price", "Price");
            dataGridView3.Columns.Add("Quantity", "Quantity");
        }
        private void PopulateDataGridViewTabPage()
        {
            try
            {
                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to select only necessary columns from the Parts table
                    string query = "SELECT Name, Price, QuantityOnHand FROM Parts";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Create a DataTable to hold the data
                        DataTable dataTable = new DataTable();

                        // Load the data from the query into the DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Bind the DataTable to the DataGridView
                        dataGridView2.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {
            PopulateDataGridViewTabPage();
            // Call the InitializeDataGridViewInvoice method to set up the columns
            InitializeDataGridViewInvoice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected in the DataGridView
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];

                    // Get the details of the selected part
                    string name = Convert.ToString(selectedRow.Cells["Name"].Value);
                    decimal price = Convert.ToDecimal(selectedRow.Cells["Price"].Value);
                    int quantity = Convert.ToInt32(selectedRow.Cells["QuantityOnHand"].Value);

                    // Add the selected part to the other DataGridView
                    dataGridView3.Rows.Add(name, price, quantity);

                    // Optionally, you can remove the selected row from the tabPage DataGridView
                    // dataGridViewTabPage.Rows.Remove(selectedRow);
                }
                else
                {
                    MessageBox.Show("Please select a part to add to the invoice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected in the dataGridViewInvoice
                if (dataGridView3.SelectedRows.Count > 0)
                {
                    // Get the selected row
                    DataGridViewRow selectedRow = dataGridView3.SelectedRows[0];

                    // Remove the selected row from the dataGridViewInvoice
                    dataGridView3.Rows.Remove(selectedRow);
                }
                else
                {
                    MessageBox.Show("Please select a part to remove from the invoice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Create an instance of the Invoice form
            Invoice invoiceForm = new Invoice();

            // Show the Invoice form
            invoiceForm.Show();
        }


        //tabpadge 3

        
        private void tabPage3_Enter(object sender, EventArgs e)
        {
            // Call a method to load user data into the DataGridView
            LoadUserData();
        }
        private void LoadUserData()
        {
            try
            {
                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to select all data from the Users table
                    string query = "SELECT UserID, Username, FullName, RoleID FROM Users";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Create a DataTable to hold the data
                        DataTable dataTable = new DataTable();

                        // Load the data from the query into the DataTable
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }

                        // Bind the DataTable to the DataGridView
                        dataGridViewUsers.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDataLoad_Click(object sender, EventArgs e)
        {
            LoadUserData();
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the DataGridView
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridViewUsers.SelectedRows[0];

                // Get the UserID from the selected row's "UserID" cell
                if (selectedRow.Cells["UserID"].Value != null && int.TryParse(selectedRow.Cells["UserID"].Value.ToString(), out int userID))
                {
                    // Prompt the user for confirmation
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Connect to the database and execute the DELETE statement
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                string query = "DELETE FROM Users WHERE UserID = @UserID";

                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@UserID", userID);

                                    int rowsAffected = command.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        // Remove the selected row from the DataGridView
                                        dataGridViewUsers.Rows.Remove(selectedRow);
                                        MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to delete user!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                else
                {
                    MessageBox.Show("Unable to retrieve UserID for selected user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            // Open the Add User Form
            AddUser addUserForm = new AddUser();
            DialogResult result = addUserForm.ShowDialog();

            // Check if the user confirmed the addition
            if (result == DialogResult.OK)
            {
                // If the user was successfully added, refresh the user data
                LoadUserData(); // You may need to call your method to reload user data
            }
        }

        private void btnEditUser_Click(object sender, EventArgs e)
        {
            // Check if a row is selected in the DataGridView
            if (dataGridViewUsers.SelectedRows.Count > 0)
            {
                // Get the selected UserID
                int selectedUserID = Convert.ToInt32(dataGridViewUsers.SelectedRows[0].Cells["UserID"].Value);

                // Create an instance of the EditUser form
                EditUser editUserForm = new EditUser(selectedUserID);

                // Show the EditUser form
                editUserForm.ShowDialog();

                // Refresh the DataGridView after editing (if needed)
                // Example: dataGridViewUsers.Refresh();
            }
            else
            {
                MessageBox.Show("Please select a user to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //padge 4
        private void button3_Click(object sender, EventArgs e)
        {
            // Generate unique barcode value
            string barcodeValue = GenerateBarcodeValue();

            // Generate barcode image
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.CODE_128; // You can change the barcode format as needed
            Bitmap barcodeImage = writer.Write(barcodeValue);

            // Display barcode image and value
            pictureBoxBarcode.Image = barcodeImage;
            txtBarcodeValue.Text = barcodeValue;
        }
        private string GenerateBarcodeValue()
        {
            // Generate a unique barcode value using date and time
            string dateTimePart = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Date and time part
            string randomPart = Guid.NewGuid().ToString().Substring(0, 4); // Random part
            string barcodeValue = dateTimePart + randomPart; // Concatenate both parts
            return barcodeValue;
        }

        private void btnSaveBarcode_Click(object sender, EventArgs e)
        {
            try
            {
                // Generate a unique barcode value
                string barcodeValue = GenerateBarcodeValue();

                // Extract other information from the GUI
                string name = txtName.Text;
                string description = txtPartDescription.Text;
                decimal price = numericUpDownPrice.Value;
                int quantity = (int)numericUpDownQuantity.Value;

                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert the information into the Parts table
                    string query = "INSERT INTO Parts (Name, Description, Price, QuantityOnHand, Barcode) " +
                                   "VALUES (@Name, @Description, @Price, @QuantityOnHand, @Barcode)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@QuantityOnHand", quantity);
                        command.Parameters.AddWithValue("@Barcode", barcodeValue);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to save data to the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving data to the database: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear all input fields
            txtName.Clear();
            txtPartDescription.Clear();
            numericUpDownPrice.Value = 0; // Set the value of numeric up-down control for price to 0
            numericUpDownQuantity.Value = 0; // Set the value of numeric up-down control for quantity to 0

            pictureBoxBarcode.Image = null; // Clear the barcode image if you're displaying it in a PictureBox
        }

        private void btnLoadPartsPrint_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT PartID, Name, Barcode FROM Parts";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);

                        dataGridViewPrint.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while populating parts: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewParts_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewPrint.SelectedRows.Count > 0)
            {
                // Retrieve the barcode number from the selected row
                string barcode = dataGridViewPrint.SelectedRows[0].Cells["Barcode"].Value.ToString();

                // Display barcode number in a textbox
                txtBarcodePrint.Text = barcode;

                
            }
        }


        private void DataGridViewPrint_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewPrint.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewPrint.SelectedRows[0];
                // Assuming the PartID is in the first column, adjust it according to your structure
                int partID = Convert.ToInt32(selectedRow.Cells[0].Value);

                // Fetching barcode from database
                string barcode = FetchBarcodeFromDatabase(partID);

                // Setting the barcode value to the TextBox
                txtBarcodePrint.Text = barcode;
            }
        }

        private string FetchBarcodeFromDatabase(int partID)
        {
            
            string barcode = "";

            
            
            
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Barcode FROM Parts WHERE PartID = @PartID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PartID", partID);

                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        barcode = result.ToString();
                    }
                }
            }

            return barcode;
        }
        private void btnMakeBarcode_Click(object sender, EventArgs e)
        {
            // Get the text from the textbox
            string barcodeText = txtBarcodePrint.Text;

            // Check if the textbox is not empty
            if (!string.IsNullOrEmpty(barcodeText))
            {
                // Generate barcode image
                Image barcodeImage = GenerateBarcodeImage(barcodeText);

                // Display barcode image in picture box
                pictureBox1.Image = barcodeImage;
            }
            else
            {
                MessageBox.Show("Please enter text to generate a barcode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to generate barcode image using ZXing library
        private Image GenerateBarcodeImage(string text)
        {
            // Create a new BarcodeWriter instance
            BarcodeWriter writer = new BarcodeWriter();

            // Set the barcode format (CODE_128 in this example)
            writer.Format = BarcodeFormat.CODE_128;

            // Generate the barcode image
            Bitmap barcodeBitmap = writer.Write(text);

            // Return the barcode image
            return barcodeBitmap;
        }

        private void btnPrintBarcode_Click(object sender, EventArgs e)
        {

        }
    }
}
