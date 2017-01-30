using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace autocropper
{
    public partial class Index : Form
    {
        string indirectory;
        string outdirectory;

        public Index()
        {
            InitializeComponent();
        }

        private void Index_Load(object sender, EventArgs e)
        {

        }

        //Double click handler for tray icon.
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show(); //show window
            this.WindowState = FormWindowState.Normal; //set state to normal
            this.ShowInTaskbar = true;
        }

        private void Index_Resize(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipTitle = "Notifcations";
            notifyIcon1.BalloonTipText = "Bewander is running in the system tray!";
            
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;

                //Uncomment this to display a notifcation window (Windows 10 style) indicating the app is in the tray.
                //notifyIcon1.ShowBalloonTip(500);

                this.ShowInTaskbar = false;
                this.Hide();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void watchpathButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        indirectory = fbd.SelectedPath;
                    }
                }
            }
            finally
            {
                //save to config file
            }
        }

        private void exportpathButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        outdirectory = fbd.SelectedPath;
                    }
                }
            }
            finally
            {
                //save to config file
            }
        }

        private void stopbutton_Click(object sender, EventArgs e)
        {
            //stop file watcher
        }

        private void startbutton_Click(object sender, EventArgs e)
        {
            //start file watcher

            //check if settings and permissions look good
            if (settings_valid())
            {
                watch_folder();
            }
            //do nothing
        }

        private bool settings_valid()
        {
            return true;
        }

        private void watch_folder()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            //Set the watchers path to the directory chosen
            watcher.Path = indirectory;

            //Need to add more logic to do this maybe?
            watcher.IncludeSubdirectories = false;

            //What do we want the watcher to notice? New filenames...
            watcher.NotifyFilter = NotifyFilters.FileName; //Only need to watch file names. Don't want to over-crop!

            //This will watch all files, but that's taken care of later.
            watcher.Filter = "*.*";

            //register handler for new file event
            watcher.Created += new FileSystemEventHandler(handle_Newfile);

            //Start the watcher
            watcher.EnableRaisingEvents = true;
        }

        private void handle_Newfile(object source, FileSystemEventArgs e)
        {
            
            //Get file extension
            string fileExtension = Path.GetExtension(e.FullPath);

            //Check file types
            if (Regex.IsMatch(fileExtension, @"\.jpeg|\.gif|.png|\.jpg", RegexOptions.IgnoreCase))
            {
                string destinationFile = outdirectory +"\\" + e.Name;
                //Copy file to new directory
                File.Copy(e.FullPath, destinationFile);

                crop_photo(e);
            }
        }

        private void crop_photo(FileSystemEventArgs FileimagePath)
        {
            double width, height;

            Image image = Image.FromFile(FileimagePath.FullPath);
            width = image.Width * .97;
            height = image.Height * .97;
            Rectangle crop = new Rectangle(0,0,(int)width,(int)height);
        
            Bitmap newimage2 = cropImage(image, crop);
            image.Dispose(); //Release image from memory so we can delete it since it's saved.
            
            File.Delete(FileimagePath.FullPath);
            newimage2.Save(FileimagePath.FullPath);
        }

        private static Bitmap cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Indicate the link was visited. Not sure this is necessary since I don't change the text color.
            this.linkLabel1.LinkVisited = true;


            //Open the developer's portfolio website, :)
            System.Diagnostics.Process.Start("http://chrismacintosh.info");
        }
    }
}
