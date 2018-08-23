using MEIKReport.Common;
using MEIKReport.Model;
using MEIKReport.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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

namespace MEIKReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OpenFolderPage : Window
    {
        public string SelectedPath { get; set; }
        public OpenFolderPage()
        {
            InitializeComponent();            
        }                        

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.treeView.SelectedItem as TreeViewItem;
            SelectedPath=(string)selectedItem.Tag;
            var userWin=this.Owner as UserList;
            userWin.loadArchiveFolder(SelectedPath);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void treeView_Loaded(object sender, RoutedEventArgs e)
        {
            
            //TreeViewItem MeikData = new TreeViewItem { Header = "MEIK Data", IsExpanded = true };
            DirectoryInfo meikDataFolder = new DirectoryInfo(App.reportSettingModel.DataBaseFolder);            
            //添加目录
            var rootItem=AddDirectoryItems(meikDataFolder);
            rootItem.IsExpanded = true;
            this.treeView.Items.Add(rootItem);
            
        }

        private TreeViewItem AddDirectoryItems(DirectoryInfo folder)
        {
            TreeViewItem folderItem = new TreeViewItem { Header = folder.Name,Tag=folder.FullName, IsExpanded = false };
            var set = folder.GetDirectories().OrderByDescending(x => x.Name);
            foreach (var subfolder in set)
            {
                folderItem.Items.Add(AddDirectoryItems(subfolder));
            }
            folderItem.IsSelected = folder.FullName.Equals(SelectedPath);
            if (folderItem.IsSelected) folderItem.IsExpanded = true;
            return folderItem;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.treeView.SelectedItem as TreeViewItem;
            var SelectedFolder = (string)selectedItem.Tag;
            var parentItme=selectedItem.Parent as TreeViewItem;
            if (parentItme!=null)
            {
                DirectoryInfo selectedDir = new DirectoryInfo(SelectedFolder);
                MessageBoxResult result = MessageBox.Show(this, App.Current.FindResource("Message_50").ToString(), "Delete Selected Folder", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteFolder(selectedDir);
                    selectedItem.Visibility = Visibility.Collapsed;
                    parentItme.IsSelected = true;
                }            
            }                        
        }

        /// <summary>
        /// 删除文件夹下所有内容
        /// </summary>
        /// <param name="folder"></param>
        private void DeleteFolder(DirectoryInfo folder)
        {
            FileInfo[] fileInfo = folder.GetFiles();
            //遍历文件
            foreach (FileInfo NextFile in fileInfo)
            {
                NextFile.Delete();
            }

            DirectoryInfo[] folders=folder.GetDirectories();
            foreach (var item in folders)
            {
                DeleteFolder(item);
            }
            folder.Delete();
        }

        private bool CanDeleteFolder(DirectoryInfo folder)
        {
            FileInfo[] fileInfo = folder.GetFiles();
                //遍历文件
            foreach (FileInfo NextFile in fileInfo)
            {
                if (".tdb".Equals(NextFile.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            foreach (var item in folder.GetDirectories())
            {
                if(!CanDeleteFolder(item)){
                    return false;
                }
            }
            return true;
        }
        
    }
}
