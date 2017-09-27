using MEIKScreen.Common;
using MEIKScreen.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MEIKScreen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AddFolderPage : Window
    {
        private string lastCode = OperateIniFile.ReadIniData("Data", "Last Code", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        
        public AddFolderPage()
        {
            InitializeComponent();
            this.txtLastName.Focus();
            string dateStr=DateTime.Now.ToString("yyMMdd");
            int num = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                try
                {
                    num = Convert.ToInt32(lastCode.Substring(lastCode.Length - 2, 2));
                }
                catch (Exception)
                {
                    num = 50;
                }
                num = lastCode.StartsWith(dateStr) ? (num+1) : 1;
            }            
            this.txtPatientCode.Text = dateStr + App.reportSettingModel.DeviceNo + num.ToString("00");
            this.listTechnicianName.ItemsSource = App.reportSettingModel.TechNames;
        }                        

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtLastName.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_22").ToString());
                this.txtLastName.Focus();
            }
            else if (App.reportSettingModel.ShowTechSignature && this.listTechnicianName.SelectedIndex==-1)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_28").ToString());
                this.listTechnicianName.Focus();
            }
            else
            {
                //string dayFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("MM_yyyy") + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("dd");
                string monthFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy_MM");
                if (!Directory.Exists(monthFolder))
                {
                    Directory.CreateDirectory(monthFolder);                    
                }
                
                string patientFolder = null;
                try
                {
                    patientFolder = monthFolder + System.IO.Path.DirectorySeparatorChar + this.txtPatientCode.Text + "-" + this.txtLastName.Text;
                    if (!string.IsNullOrEmpty(this.txtFirstName.Text))
                    {
                        patientFolder = patientFolder + "," + this.txtFirstName.Text;
                    }                    
                    if (!string.IsNullOrEmpty(this.txtMiddleInitial.Text))
                    {
                        patientFolder = patientFolder + " " + this.txtMiddleInitial.Text;
                    }
                    if (!Directory.Exists(patientFolder))
                    {                        
                        Directory.CreateDirectory(patientFolder);                                                
                        OperateIniFile.WriteIniData("Data", "Last Code", this.txtPatientCode.Text, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");  
                        //創建用戶信息文件
                        string patientFile = patientFolder + System.IO.Path.DirectorySeparatorChar + this.txtPatientCode.Text + ".ini";
                        if (!File.Exists(patientFile))
                        {
                            var fs=File.Create(patientFile);
                            fs.Close();
                        }
                        
                        OperateIniFile.WriteIniData("Personal data", "surname", this.txtLastName.Text, patientFile);
                        OperateIniFile.WriteIniData("Personal data", "given name", this.txtFirstName.Text, patientFile);
                        OperateIniFile.WriteIniData("Personal data", "other name", this.txtMiddleInitial.Text, patientFile);
                        OperateIniFile.WriteIniData("Personal data", "birth date", this.txtBirthDate.Text, patientFile);
                        OperateIniFile.WriteIniData("Personal data", "birth month", this.txtBirthMonth.Text, patientFile);
                        OperateIniFile.WriteIniData("Personal data", "birth year", this.txtBirthYear.Text, patientFile);

                        OperateIniFile.WriteIniData("Personal data", "registration date", DateTime.Now.Day.ToString(), patientFile);
                        OperateIniFile.WriteIniData("Personal data", "registration month", DateTime.Now.Month.ToString(), patientFile);
                        OperateIniFile.WriteIniData("Personal data", "registration year", DateTime.Now.Year.ToString(), patientFile);                        


                        //創建Crd文件
                        string patientCrdFile = patientFolder + System.IO.Path.DirectorySeparatorChar + this.txtPatientCode.Text + ".crd";
                        File.Copy(patientFile, patientCrdFile, true);


                        OperateIniFile.WriteIniData("Report", "Technician Name Required", App.reportSettingModel.ShowTechSignature.ToString(), patientFile);
                        var tech=this.listTechnicianName.SelectedItem as User;
                        if (tech != null) { 
                            OperateIniFile.WriteIniData("Report", "Technician Name", tech.Name, patientFile);
                            OperateIniFile.WriteIniData("Report", "Technician License", tech.License, patientFile);
                        }
                        OperateIniFile.WriteIniData("Report", "Screen Venue", App.reportSettingModel.ScreenVenue, patientFile);
                        
                    }
                    else
                    {
                        MessageBox.Show(this, string.Format(App.Current.FindResource("Message_24").ToString(), patientFolder));                       
                    }

                    UserList userlistWin = this.Owner as UserList;
                    userlistWin.loadArchiveFolder(patientFolder);
                    try
                    {
                        Clipboard.SetText(this.txtPatientCode.Text);
                    }
                    catch (Exception) { }
                }
                catch (Exception ex)
                {
                    FileHelper.SetFolderPower(patientFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(patientFolder, "Users", "FullControl");
                    MessageBox.Show(this, App.Current.FindResource("Message_23").ToString());
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_25").ToString(), patientFolder, ", Error: " + ex.Message));
                }
                finally
                {
                    this.Close();
                }
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //private string MatchFolder(string folderName,string code)
        //{            
        //    //遍历指定文件夹下所有文件
        //    DirectoryInfo theFolder = new DirectoryInfo(folderName);
        //    try
        //    {
        //        DirectoryInfo[] folderList = theFolder.GetDirectories();
        //        //遍历文件
        //        foreach (DirectoryInfo NextDir in folderList)
        //        {
        //            if (NextDir.Name.StartsWith(code))
        //            {
        //                return NextDir.FullName;
        //            }
        //        }
        //        return null;
        //    }
        //    catch (Exception) {
        //        return null;
        //    }

            
        //} 


        private void Year_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_59").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Month_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_57").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }

        private void Day_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_58").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }

        }
        
    }

     
}
