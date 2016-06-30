using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace htdocsSwitcher
{
    public partial class htdocsSwitcherGUI : Form
    {
        private readonly string configFile = "config.txt";
        private string xamppLocation;
        private IDictionary<string, string> projects;

        public htdocsSwitcherGUI()
        {
            InitializeComponent();
            RefreshAll();
        }

        private void Log(string txt)
        {
            Debug.WriteLine(txt);
            toolStripStatusLabel1.Text = txt;
            statusTimerReset();
        }

        private void statusTimerReset()
        {
            timer1.Stop();
            timer1.Start();
        }

        private void Switch()
        {
            if (comboBox1.SelectedItem == null) return;
            if (xamppLocation == null) return;

            var n = comboBox1.SelectedItem.ToString();
            if (!projects.ContainsKey(n))
            {
                Log("ProjektID konnte nicht gefunden werden.");
                return;
            }
            var loc = projects[n];

            var htdocsLoc = Path.Combine(xamppLocation, "htdocs");
            var isJunction = JunctionPoint.Exists(htdocsLoc);
            var isDirOrFile = Directory.Exists(htdocsLoc) || File.Exists(htdocsLoc);

            if (!isJunction && isDirOrFile)
            {
                Log("htdocs existiert bereits.");
                return;
            }

            JunctionPoint.Create(htdocsLoc, loc, true);
            Log($"{n} erfolgreich verknüpft.");
        }

        private void RefreshAll()
        {
            RefreshEntries();
            UpdateDropDown();
        }

        private void ClearEntries()
        {
            xamppLocation = null;
            projects = null;
        }

        private void UpdateDropDown()
        {
            comboBox1.BeginUpdate();
            try
            {
                comboBox1.Items.Clear();

                if (xamppLocation == null) return;
                if (projects == null) return;

                foreach (var p in projects)
                {
                    comboBox1.Items.Add(p.Key);
                }
            }
            finally
            {
                comboBox1.EndUpdate();
            }
        }

        private void RefreshEntries()
        {
            string[] fileLines = null;

            if (!File.Exists(configFile))
            {
                Log("config.txt nicht gefunden.");
                ClearEntries();
                return;
            }

            try { fileLines = File.ReadAllLines(configFile); }
            catch (Exception ex)
            {
                Log("Fehler config.txt: " + ex.Message);
                ClearEntries();
                return;
            }

            //fileLines = new string[]
            //{
            //    @"D:\Xampp",
            //    @"D:\Code\bitOxide\SimplePoke\src|SimplePoke",
            //    @"D:\Xampp\htdocs.default",
            //};

            if (fileLines.Length < 2)
            {
                Log("Keine XAMPP Path bzw. Projekt gefunden.");
                ClearEntries();
                return;
            }

            xamppLocation = fileLines[0].Trim();
            if (!Directory.Exists(xamppLocation))
            {
                Log("XAMPP Path ungültig.");
                ClearEntries();
                return;
            }

            var dict = new Dictionary<string, string>();

            foreach (var l in fileLines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(l))
                {
                    Debug.WriteLine("Leere Zeile übersprungen");
                    continue;
                }

                var parts = l.Split('|');
                if (parts.Length > 2)
                {
                    Debug.WriteLine("Ungültiges Format (nur eine |-Trennlinie erlaubt).");
                    continue;
                }

                string location = null;
                string name = null;

                if (parts.Length == 1)
                {
                    location = parts[0];
                    name = Path.GetFileName(location);
                }
                else
                {
                    location = parts[0];
                    name = parts[1];
                }

                if (!Directory.Exists(location))
                {
                    Debug.WriteLine("Projektpfad nicht gefunden.");
                    continue;
                }

                dict.Add(name, location);
                Debug.WriteLine($"Projekt \"{name}\" hinzugefügt.");
            }

            projects = dict;
        }

        private void Form1_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show(Properties.Resources.helpText, "Aufbau \"config.txt\"", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Switch();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Bereit.";
        }
    }
}
