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
using System.Security.Cryptography;
using System.Text;


namespace StockTakeSystem
{
    public partial class EditUser : Form
    {
        private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Morgan\\Documents\\InventoryDB.mdf;Integrated Security=True;Connect Timeout=30";
        private int userID;
        public EditUser(int userID)
        {
            InitializeComponent();
            // Store the provided UserID in the field
            this.userID = userID;

            PopulateUserData();
        }

        private void PopulateUserData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch user data
                    string userQuery = "SELECT Username, FullName, RoleID FROM Users WHERE UserID = @UserID";
                    using (SqlCommand userCommand = new SqlCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@UserID", userID);

                        using (SqlDataReader userReader = userCommand.ExecuteReader())
                        {
                            if (userReader.Read())
                            {
                                // Populate textboxes with user information
                                txtUsername.Text = userReader["Username"].ToString();
                                txtFullName.Text = userReader["FullName"].ToString();

                                // Set selected item in combobox based on RoleID
                                int roleID = Convert.ToInt32(userReader["RoleID"]);
                                cmbRole.SelectedItem = GetRoleName(roleID);
                            }
                        }
                    }

                    // Fetch roles and populate the combobox
                    string rolesQuery = "SELECT RoleID, RoleName FROM Roles";
                    using (SqlCommand rolesCommand = new SqlCommand(rolesQuery, connection))
                    {
                        using (SqlDataReader rolesReader = rolesCommand.ExecuteReader())
                        {
                            while (rolesReader.Read())
                            {
                                // Add role name to combobox
                                string roleName = rolesReader["RoleName"].ToString();
                                cmbRole.Items.Add(roleName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while populating user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetRoleName(int roleID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to select role name based on RoleID
                    string query = "SELECT RoleName FROM Roles WHERE RoleID = @RoleID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleID", roleID);
                        return command.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while getting role name: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private void EditUser_Load(object sender, EventArgs e)
        {

        }

        // Method to hash a password using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private int GetRoleID(string roleName)
        {
            // Assuming roles are stored in a dictionary or some other data structure
            Dictionary<string, int> roleIDs = new Dictionary<string, int>
            {
                 { "Admin", 1 }, // Replace "Admin" with the actual role names used in your application
                 { "Employee", 2 }
            // Add more role mappings as needed
            };

            // Lookup RoleID based on RoleName
            if (roleIDs.ContainsKey(roleName))
            {
                return roleIDs[roleName];
            }
            else
            {
                // Handle case where RoleName is not found (e.g., throw exception or return default value)
                throw new ArgumentException("RoleName not found");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update user data in the database
                    string updateQuery = "UPDATE Users SET Username = @Username, FullName = @FullName, Password = @Password, RoleID = @RoleID WHERE UserID = @UserID";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", txtUsername.Text);
                        command.Parameters.AddWithValue("@FullName", txtFullName.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text); // Use the password directly without hashing

                        // Retrieve RoleID based on selected RoleName from combobox
                        string roleName = cmbRole.SelectedItem.ToString();
                        int roleID = GetRoleID(roleName); // Assuming GetRoleID is a method that retrieves RoleID based on RoleName
                        command.Parameters.AddWithValue("@RoleID", roleID);

                        // Use UserID to identify the user to be updated
                        command.Parameters.AddWithValue("@UserID", userID);

                        // Execute the update query
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User data updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update user data!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
