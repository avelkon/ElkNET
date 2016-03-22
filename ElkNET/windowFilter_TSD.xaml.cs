using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElkNET
{
    /// <summary>
    /// Логика взаимодействия для windowFilter_TSD.xaml
    /// </summary>
    public partial class windowFilter_TSD : Window
    {
        public windowFilter_TSD()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                FilterResources.file_name = tbFileName.Text;
                FilterResources.start_date = dpStartDate.SelectedDate;
                FilterResources.end_date = dpEndDate.SelectedDate;
                FilterResources.doc_name = tbDocName.Text;
                FilterResources.has_doc = cbHasDoc.IsChecked.Value;
                FilterResources.is_error = cbIsError.IsChecked.Value;
                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                throw;
            }
        }

        private void cbHasDoc_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cbHasDoc.IsChecked.HasValue)
            {
                tbDocName.IsEnabled = false;
                lbDocName.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbFileName.Text = FilterResources.file_name;
            dpStartDate.SelectedDate = FilterResources.start_date;
            dpStartDate.DisplayDate = DateTime.Now;
            dpEndDate.SelectedDate = FilterResources.end_date;
            dpEndDate.DisplayDate = DateTime.Now;
            tbDocName.Text = FilterResources.doc_name;
            cbHasDoc.IsChecked = FilterResources.has_doc;
            cbIsError.IsChecked = FilterResources.is_error;
            if (cbHasDoc.IsChecked.HasValue && !cbHasDoc.IsChecked.Value)
            {
                tbDocName.IsEnabled = false;
                lbDocName.IsEnabled = false;
            }
        }

        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Escape))
            {
                (sender as DatePicker).SelectedDate = null;
            }
        }

        private void cbHasDoc_Checked(object sender, RoutedEventArgs e)
        {
            if (cbHasDoc.IsChecked.HasValue)
            {
                tbDocName.IsEnabled = true;
                lbDocName.IsEnabled = true;
            }
        }
    }
}
