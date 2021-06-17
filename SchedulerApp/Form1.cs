using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SchedulerApp
{
    public partial class Form1 : Form
    {
        private HubConnection connection;
        private string url = "http://neuotec.com:51926/mainhub";

        public Form1()
        {
            InitializeComponent();
            InitializePanel();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Console.WriteLine("Key is being pressed!");
            if (keyData == Keys.Oemtilde)
            {
                Console.WriteLine("Tilde was pressed!");
                url = Prompt.ShowDialog("", "Current Hub Url: " + url);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InitializePanel()
        {
            for (int i = 0; i < 31; i++)
            {
                // Parent container for each date
                Panel panel = new Panel
                {
                    Location = new Point(0, i * 34),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(512, 34)
                };

                // Date Label
                Label dateLabel = new Label
                {
                    Text = DateTime.Now.AddDays(i).ToString("dddd, dd 'of' MMMM"),
                    TextAlign = ContentAlignment.MiddleRight,
                    Size = new Size(229, 33),
                    Padding = new Padding(0, 3, 50, 3)
                };

                // Start & End Time DateTimePicker
                DateTimePicker startTimePicker = new DateTimePicker
                {
                    Location = new Point(230, 5),
                    Size = new Size(130, 23),
                    Format = DateTimePickerFormat.Time,
                    Value = DateTime.Today,
                    ShowUpDown = true,
                    
                };
                DateTimePicker endTimePicker = new DateTimePicker
                {
                    Location = new Point(378, 5),
                    Size = new Size(130, 23),
                    Format = DateTimePickerFormat.Time,
                    Value = DateTime.Today,
                    ShowUpDown = true
                };

                // Compile controls to parent container and add to panel
                panel.Controls.Add(dateLabel);
                panel.Controls.Add(startTimePicker);
                panel.Controls.Add(endTimePicker);
                panel1.Controls.Add(panel);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // First time connection
            if (connection == null)
            {
                connection = new HubConnectionBuilder().WithUrl(url).Build();

                connection.Closed += (error) =>
                {
                    label1.Text = "Disconnected";
                    label1.ForeColor = Color.Red;
                    ShowError(error);
                    return Task.CompletedTask;
                };

                connection.Reconnected += (connectionID) =>
                {
                    label1.Text = "Connection Established";
                    label1.ForeColor = Color.Green;
                    return Task.CompletedTask;
                };

                connection.On<string>("Recieve", (msg) =>
                {
                    ShowMessage(msg);
                });

                connection.On<string>("Update", async (msg) =>
                {
                    DialogResult doUpdate = MessageBox.Show(msg, "Reply from Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (doUpdate == DialogResult.Yes)
                    {
                        await connection.InvokeAsync("Update", textBox1.Text, PackageToJson());
                        UpdateActivity("Updated availability");
                    }
                });

                try
                {
                    await connection.StartAsync();
                    label1.Text = "Connection Established";
                    label1.ForeColor = Color.Green;
                    UpdateActivity("First time connection with server established");
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    UpdateActivity("Failed to establish first time connection");
                }
            }
            // Reconnection
            else
            {
                if (connection.State == HubConnectionState.Disconnected)
                    await connection.StartAsync();
                UpdateActivity("Attempted reconnection");
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    ShowWarning("You must enter a name.");
                    return;
                }
                await connection.InvokeAsync("Send", textBox1.Text, PackageToJson());
                UpdateActivity("Sent availabilities under " + textBox1.Text);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private string PackageToJson()
        {
            List<DateTime> availability = new List<DateTime>();
            foreach (Control panel in panel1.Controls)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control.GetType() == typeof(DateTimePicker))
                    {
                        DateTimePicker picker = (DateTimePicker)control;
                        availability.Add(picker.Value);
                    }
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(availability);
        }

        private void UpdateActivity(string msg)
        {
            label3.Text = "Last update: " + msg + " at " + DateTime.Now.ToString("t");
        }

        private void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg, "Reply from Server", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
