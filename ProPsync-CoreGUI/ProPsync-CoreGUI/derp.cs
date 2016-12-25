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
        DataTable dt = new DataTable();
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

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Warning... you cannot undo this action (at least not easily... we will create a new branch and if you are familiar with git, you could use that to go back to the current state - but this program does not currently have this functionality built in).  The changes will be immediately pushed to the server.  Are you really sure that you want to do this?", "Warning!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string selectedid = dt.Rows[comboBox1.SelectedIndex][0].ToString();
                string repo = comboBox2.SelectedItem.ToString();
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
                string prerevertname = "Pre-revert-" + DateTime.Now.ToString("MM-dd-yyyy-hh-mm");
                proc.StartInfo.Arguments = @"/C cd /d " + path + @" & git branch " + prerevertname;
                proc.Start();
                proc.WaitForExit();
                proc.StartInfo.Arguments = @"/C cd /d " + path + @" & git reset --hard " + selectedid + @" & git push --force origin master & git push origin " + prerevertname;
                proc.Start();
                proc.WaitForExit();
                this.Visible = false;
                MessageBox.Show("Successfully reverted.");
                this.Close();
            }else
            {
                MessageBox.Show("Revert canceled.");
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
