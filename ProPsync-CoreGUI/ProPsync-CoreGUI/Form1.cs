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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Semrau Software Consulting").OpenSubKey("ProPsync");
                try
                {
                    vars.dns = key.GetValue("dns").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting DNS");
                }
                try
                {
                    vars.mediarepo = key.GetValue("mediarepo").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Media repo");
                }
                try
                {
                    vars.libraryrepo = key.GetValue("libraryrepo").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Library repo");
                }
                try
                {
                    vars.prefrepo = key.GetValue("prefrepo").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Preferences repo");
                }
                try
                {
                    vars.synclib = key.GetValue("synclib").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Sync library setting");
                }
                try
                {
                    vars.syncmedia = key.GetValue("syncmedia").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Sync media setting");
                }
                try
                {
                    vars.syncpref = key.GetValue("syncpref").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting Sync preferences setting");
                }
                try
                {
                    vars.version = key.GetValue("pro-ver").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting ProPresenter version");
                }
                try
                {
                    vars.username = key.GetValue("username").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting username");
                }
                try
                {
                    vars.fullname = key.GetValue("fullname").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting full name");
                }
                try
                {
                    vars.libpath = key.GetValue("libpath").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting library path");
                }
                try
                {
                    vars.mediapath = key.GetValue("mediapath").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting media path");
                }
                try
                {
                    vars.prefpath = key.GetValue("prefpath").ToString();
                }
                catch
                {
                    MessageBox.Show("Error getting preferences path");
                }
                try
                {
                    vars.mode = key.GetValue("mode").ToString();
                }catch
                {
                    MessageBox.Show("Error getting mode");
                }
                

                key.Close();

                if (vars.version == "6")
                {
                    vars.ProPdir = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("Renewed Vision").OpenSubKey("ProPresenter 6").GetValue("InstalledLocation").ToString() + @"\";
                    vars.ProPexe = "ProPresenter.exe";
                }
                else
                {
                    //Handle other versions here
                }

                if (vars.mode == "auto")
                {
                    label1.Visible = true;
                    button1.Visible = false;
                    System.Threading.Thread trd = new System.Threading.Thread(open);
                    trd.Start();
                }else
                {
                    label1.Visible = false;
                    button1.Visible = true;
                }
                
            }catch (Exception ex)
            { 
                MessageBox.Show(ex.Message.ToString());
            }
            
        }

        private void open()
        {
            sync();
            updatestatus("Current status: Opening ProPresenter");

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = vars.ProPdir + vars.ProPexe;
            proc.Start();
            updatestatus("Current status: Waiting for ProPresenter to close");
            proc.WaitForExit();
            updatestatus("Current status: Confirmed ProPresenter closed, starting sync");
            sync();
            updatestatus("Current status: Sync completed.  You can now close this window.");
        }


        private void sync()
        {

            if (button2.InvokeRequired)
            {
                button2.BeginInvoke((MethodInvoker)delegate () { button2.Visible = false; });
            }
            else
            {
                button2.Visible = false;
            }
            updatestatus("Current status: Commiting any new changes");
            commitchanges();
            updatestatus("Current status: Pulling any new changes from server");
            pullchanges();
            updatestatus("Current status: Pushing current repo to server");
            pushchanges();
            if (!(vars.mode == "auto"))
            {
                if (button1.InvokeRequired)
                {
                    button1.BeginInvoke((MethodInvoker)delegate () { button1.Visible = true; });
                }
                else
                {
                    button1.Visible = true;
                }
            }

            if (button2.InvokeRequired)
            {
                button2.BeginInvoke((MethodInvoker)delegate () { button2.Visible = true; });
            }
            else
            {
                button2.Visible = true;
            }
        }

        private void updatestatus(string message)
        {
            if (label1.InvokeRequired)
            {
                label1.BeginInvoke((MethodInvoker)delegate () { label1.Text = message; });
            }
            else
            {
                label1.Text = message;
            }

        }

        private void commitchanges()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = Environment.SystemDirectory + @"\cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            if (vars.synclib == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git status";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (!(status.Contains("nothing to commit")))
                {
                    proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git add --all & git commit -m """ + vars.fullname + @" @ " + DateTime.Now + @"""";
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            if (vars.syncmedia == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.mediapath + @" & git status";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (!(status.Contains("nothing to commit")))
                {
                    proc.StartInfo.Arguments = @"/C cd /d " + vars.mediapath + @" & git add --all & git commit -m """ + vars.fullname + @" @ " + DateTime.Now + @"""";
                    proc.Start();
                    proc.WaitForExit();
                }
            }

            if (vars.syncpref == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.prefpath + @" & git status";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (!(status.Contains("nothing to commit")))
                {
                    proc.StartInfo.Arguments = @"/C cd /d " + vars.prefpath + @" & git add --all & git commit -m """ + vars.fullname + @" @ " + DateTime.Now + @"""";
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
        private void pullchanges()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = Environment.SystemDirectory + @"\cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            if (vars.synclib == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git pull origin master";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (status.Contains("Automatic merge failed"))
                {
                    DialogResult dr = MessageBox.Show("Error: Library merge conflict detected.  Do you want to replace your files with the updated versions from the server?  If you choose no, the changes on the server will be overwritten with your version.  Either way, we recommend backing up before choosing either option as this could lead to data loss.", "Merge conflict detected", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git checkout --theirs -- . & git add --all & git commit -m ""Resolve merge error by taking server version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }else
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git checkout --ours -- . & git add --all & git commit -m ""Resolve merge error by taking local version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }

            if (vars.syncmedia == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.mediapath + @" & git pull origin master";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (status.Contains("Automatic merge failed"))
                {
                    DialogResult dr = MessageBox.Show("Error: Media merge conflict detected.  Do you want to replace your files with the updated versions from the server?  If you choose no, the changes on the server will be overwritten with your version.  Either way, we recommend backing up before choosing either option as this could lead to data loss.", "Merge conflict detected", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.syncmedia + @" & git checkout --theirs -- . & git add --all & git commit -m ""Resolve merge error by taking server version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }
                    else
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.syncmedia + @" & git checkout --ours -- . & git add --all & git commit -m ""Resolve merge error by taking local version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }

            if (vars.syncpref == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.prefpath + @" & git pull origin master";
                proc.Start();
                proc.WaitForExit();
                string status = proc.StandardOutput.ReadToEnd();
                if (status.Contains("Automatic merge failed"))
                {
                    DialogResult dr = MessageBox.Show("Error: Preference/playlist merge conflict detected.  Do you want to replace your files with the updated versions from the server?  If you choose no, the changes on the server will be overwritten with your version.  Either way, we recommend backing up before choosing either option as this could lead to data loss.", "Merge conflict detected", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.syncpref + @" & git checkout --theirs -- . & git add --all & git commit -m ""Resolve merge error by taking server version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }
                    else
                    {
                        proc.StartInfo.Arguments = @"/C cd /d " + vars.syncpref + @" & git checkout --ours -- . & git add --all & git commit -m ""Resolve merge error by taking local version.  (" + vars.fullname + @" @ " + DateTime.Now.ToString() + @")""";
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }
        }
        private void pushchanges()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = Environment.SystemDirectory + @"\cmd.exe";

            if (vars.synclib == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.libpath + @" & git push origin master";
                proc.Start();
                proc.WaitForExit();
            }

            if (vars.syncmedia == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.mediapath + @" & git push origin master";
                proc.Start();
                proc.WaitForExit();
            }

            if (vars.syncpref == "True")
            {
                proc.StartInfo.Arguments = @"/C cd /d " + vars.prefpath + @" & git push origin master";
                proc.Start();
                proc.WaitForExit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            label1.Visible = true;
            button2.Visible = false;
            sync();
            updatestatus("Current status: Sync completed.  You can now close this window.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            derp drp = new derp();
            drp.Show();
        }
    }
    public class vars
    {
        public static string dns { get; set; }
        public static string mediarepo { get; set; }
        public static string libraryrepo { get; set; }
        public static string prefrepo { get; set; }
        public static string synclib { get; set; }
        public static string syncmedia { get; set; }
        public static string syncpref { get; set; }
        public static string version { get; set; }
        public static string username { get; set; }
        public static string fullname { get; set; }
        public static string libpath { get; set; }
        public static string mediapath { get; set; }
        public static string prefpath { get; set; }
        public static string ProPdir { get; set; }
        public static string ProPexe { get; set; }
        public static string mode { get; set; }
    }
}
