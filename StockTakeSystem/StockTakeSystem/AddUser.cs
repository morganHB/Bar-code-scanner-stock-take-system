using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace StockTakeSystem
{
    public partial class AddUser : Form
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Morgan\\Documents\\InventoryDB.mdf;Integrated Security=True;Connect Timeout=30";
        public AddUser()
        {
            InitializeComponent();
            Load += AddUser_Load;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate the input fields
                if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    string.IsNullOrWhiteSpace(txtFullName.Text) ||
                    cmbRole.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in all the fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get the values from the input fields
                string username = txtUsername.Text;
                string password = txtPassword.Text;
                string fullName = txtFullName.Text;
                string roleName = cmbRole.SelectedItem.ToString();

                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Determine the RoleID based on the selected role name
                    int roleId = GetRoleId(roleName, connection);

                    // Insert the new user into the database
                    string query = "INSERT INTO Users (Username, Password, FullName, RoleID) VALUES (@Username, @PasswordHash, @FullName, @RoleID)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@PasswordHash", password); // Store the password as-is (should be hashed for security)
                        command.Parameters.AddWithValue("@FullName", fullName);
                        command.Parameters.AddWithValue("@RoleID", roleId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while adding the user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int GetRoleId(string roleName, SqlConnection connection)
        {
            // Retrieve the RoleID from the Roles table based on the role name
            string query = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RoleName", roleName);
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    throw new Exception("Role not found.");
                }
            }
        }

        

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {
            try
            {
                // Connect to the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to select all roles from the Roles table
                    string query = "SELECT RoleName FROM Roles";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Clear the ComboBox items
                            cmbRole.Items.Clear();

                            // Add roles to the ComboBox
                            while (reader.Read())
                            {
                                cmbRole.Items.Add(reader["RoleName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading roles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
