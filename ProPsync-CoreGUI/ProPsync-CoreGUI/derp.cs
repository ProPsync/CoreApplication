using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProPsync_CoreGUI
{
    public partial class derp : Form
    {
        public derp()
        {
            InitializeComponent();
        }

        private void derp_Load(object sender, EventArgs e)
        {
            loadrepos();
        }
        private void loadrepos()
        {
            comboBox2.Items.Clear();
            if (vars.synclib == "True")
            {
                comboBox2.Items.Add("Library");
            }
            if (vars.syncmedia == "True")
            {
                comboBox2.Items.Add("Media");
            }
            if (vars.syncpref == "True")
            {
                comboBox2.Items.Add("Preferences/Playlists");
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            loadcommits(comboBox2.SelectedItem.ToString());
        }
        private void loadcommits(string repo)
        {
            string path = "";
            if (repo == "Library")
            {
                path = vars.libpath;
            }
            if (repo == "Media")
            {
                path = vars.mediapath;
            }
            if (repo == "Preferences/Playlists")
            {
                path = vars.prefpath;
            }


            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            proc.StartInfo.FileName = (Environment.SystemDirectory + @"\cmd.exe");
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = @"/C cd /d " + path + @" & git log";
            proc.Start();
            proc.WaitForExit();


            string output = "";
            string commitID = "";
            string commitmsg = "";
            DataTable dt = new DataTable();
            dt.Columns.Add("Commit-ID");
            dt.Columns.Add("Commit-Message");
            Boolean cont = true;
            do
            {
                output = proc.StandardOutput.ReadLine();
                if (!(output == ""))
                {
                    if ((output.Contains("commit ")) && (!(output.Contains("Initial ") && (output.Contains(" for ") && (output.Contains(" on "))))))
                    {
                        commitID = output.Replace("commit ", "");
                    }else
                    {
                        if (!(output.Contains("Author:") || output.Contains("Date:")))
                        {
                            commitmsg = output.TrimStart();
                            dt.Rows.Add(commitID, commitmsg);
                        }
                    }
                }
               if (proc.StandardOutput.EndOfStream == true)
                {
                    cont = false;
                }
            } while (cont == true);

            foreach (DataRow dr in dt.Rows)
            {
                comboBox1.Items.Add(dr[1].ToString());
            }
            comboBox1.Visible = true;
            label1.Visible = true;
        }
    }
}
