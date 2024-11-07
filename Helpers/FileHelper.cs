using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace sunucukontrol.Helpers
{
    public static class FileHelper
    {
        private static string filePath = Path.Combine(Application.StartupPath, "TextBoxData.txt");

        public static void SaveToFile(Dictionary<string, TextBox> ipTextBoxes, Dictionary<string, TextBox> descriptionTextBoxes)
        {
            using (StreamWriter file = new StreamWriter(filePath))
            {
                foreach (var item in ipTextBoxes)
                {
                    string ip = item.Value.Text;
                    string description = descriptionTextBoxes[item.Key].Text;
                    file.WriteLine($"{item.Key}|{ip}|{description}");
                }
            }
        }

        public static void LoadFromFile(Dictionary<string, TextBox> ipTextBoxes, Dictionary<string, TextBox> descriptionTextBoxes)
        {
            if (!File.Exists(filePath)) return;

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');
                if (parts.Length == 3 && ipTextBoxes.ContainsKey(parts[0]))
                {
                    ipTextBoxes[parts[0]].Text = parts[1];
                    descriptionTextBoxes[parts[0]].Text = parts[2];
                }
            }
        }
    }
}
