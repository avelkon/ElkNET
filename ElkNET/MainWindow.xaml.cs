﻿using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System;
using System.IO;

namespace ElkNET
{

    public static class FilterResources 
    {
        public static string file_name;
        public static DateTime? start_date;
        public static DateTime? end_date;
        public static bool has_doc;
        public static string doc_name;
    }

    public enum MSG_Code:int { MESSAGE, ERROR}
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
                
        string currentLogin = Properties.Settings.Default.LastUser;
        string currentPassword = Properties.Settings.Default.LastPassword;
        DBConnection dbConnection;

        public static void writeLog(string message, MSG_Code type = MSG_Code.MESSAGE)
        {
            string typeMsg = type.Equals(MSG_Code.MESSAGE) ? "[MESSAGE]" : "[ERROR]";
            File.AppendAllText(Properties.Settings.Default.log_filepath, "[" + DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")+ "]" + typeMsg + "\n" + message);
        }

        public MainWindow()
        {
            InitializeComponent();
            FilterResources.file_name = Properties.Settings.Default.filter_tsd_FileName;
            FilterResources.doc_name = Properties.Settings.Default.filter_tsd_DocName;
            if (Properties.Settings.Default.filter_tsd_StartDate.Equals(DateTime.MinValue)) FilterResources.start_date = null;
                else FilterResources.start_date = Properties.Settings.Default.filter_tsd_StartDate;

            if (Properties.Settings.Default.filter_tsd_EndDate.Year >= (DateTime.Now.Year+10) ||
                Properties.Settings.Default.filter_tsd_EndDate.Year <= (DateTime.Now.Year - 500)) FilterResources.end_date = null;
                else FilterResources.end_date = Properties.Settings.Default.filter_tsd_EndDate;

            FilterResources.has_doc = Properties.Settings.Default.filter_tsd_hasDoc;

        }

        private bool showFilterWindow()
        {
            try
            {
                windowFilter_TSD filterWindow = new windowFilter_TSD();
                filterWindow.Owner = this;
                bool? dialRes = filterWindow.ShowDialog();
                if (!dialRes.HasValue || !dialRes.Value) return false;
                this.Cursor = Cursors.Wait;
                dbConnection.clearDataSet();
                clearDataGrid(dg_from_tsd);
                clearDataGrid(dg_boxes_tsd);
                clearDataGrid(dg_marks_tsd);
                dg_tsd_tc.ItemsSource = dbConnection.getTable_tsd_tc(FilterResources.start_date,
                        FilterResources.end_date,
                        FilterResources.has_doc,
                        FilterResources.doc_name,
                        FilterResources.file_name);
                dg_tsd_tc.Items.Refresh();
                this.Cursor = Cursors.Arrow;
                return true;
            }
            catch { throw; }
        }

        private void dg_tsd_tc_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                showFilterWindow();
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dbConnection = new DBConnection(currentLogin, currentPassword);
                dbConnection.Open();
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                dbConnection.Close();
                Properties.Settings.Default.filter_tsd_DocName = FilterResources.doc_name;
                Properties.Settings.Default.filter_tsd_FileName = FilterResources.file_name;
                Properties.Settings.Default.filter_tsd_hasDoc = FilterResources.has_doc;
                Properties.Settings.Default.filter_tsd_StartDate = FilterResources.start_date.HasValue ? FilterResources.start_date.Value : DateTime.MinValue;
                Properties.Settings.Default.filter_tsd_EndDate = FilterResources.end_date.HasValue ? FilterResources.end_date.Value : DateTime.MaxValue;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
        }

        private void clearDataGrid(DataGrid dataGrid)
        {
            dataGrid.ItemsSource = null;
            dataGrid.Items.Refresh();
        }

        private void dg_tsd_tc_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                if ((sender as DataGrid).SelectedIndex == -1) return;
                string fileName = ((sender as DataGrid).SelectedItem as DataRowView)["FILE_NAME"].ToString();
                if (exDg_From_tsd.IsExpanded) dg_from_tsd.ItemsSource = dbConnection.getTable_from_tsd(fileName);
                dg_boxes_tsd.ItemsSource = dbConnection.getTable_boxes_tsd(fileName);
                clearDataGrid(dg_marks_tsd);
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void dg_boxes_tsd_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                if (((DataGrid)sender).SelectedIndex == -1) return;
                dg_marks_tsd.ItemsSource = dbConnection.getTable_marks_tsd(((sender as DataGrid).SelectedItem as DataRowView)["RN"].ToString());
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void exDg_From_tsd_Collapsed(object sender, RoutedEventArgs e)
        {
            tsdTabMainGrid.ColumnDefinitions[1].Width = new GridLength(tsdTabMainGrid.ColumnDefinitions[1].MinWidth);
            tsdTabMainGrid.ColumnDefinitions[1].MaxWidth = tsdTabMainGrid.ColumnDefinitions[1].MinWidth;
        }

        private void exDg_From_tsd_Expanded(object sender, RoutedEventArgs e)
        {
            tsdTabMainGrid.ColumnDefinitions[1].MaxWidth = int.MaxValue;
            tsdTabMainGrid.ColumnDefinitions[1].Width = new GridLength(150, GridUnitType.Star);
        }

        private void btShowFilter_Click(object sender, RoutedEventArgs e)
        {
            showFilterWindow();
            this.Cursor = Cursors.Arrow;
        }

        private void datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key.Equals(Key.Delete) && (sender as DataGrid).HasItems && MessageBox.Show("Подтвердить удаление?", "Удаление записей", MessageBoxButton.OKCancel, MessageBoxImage.Question).Equals(MessageBoxResult.OK))
                {
                    this.Cursor = Cursors.Wait;
                    string current_dg = "";
                    if((sender as DataGrid).Equals(dg_boxes_tsd)) current_dg = DBConnection.boxes_tsd_table;
                        else if ((sender as DataGrid).Equals(dg_marks_tsd)) current_dg = DBConnection.marks_tsd_table;
                    foreach (DataRowView selectedRow in (sender as DataGrid).SelectedItems)
                    {
                        string rn = selectedRow["RN"].ToString();
                        dbConnection.deleteRowFromDB(current_dg, rn);
                    }
                }
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void datagrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit && MessageBox.Show("Подтвердить изменения?", "Изменение записи", MessageBoxButton.OKCancel, MessageBoxImage.Question).Equals(MessageBoxResult.OK))
                {
                    this.Cursor = Cursors.Wait;
                    DataRowView drv = (DataRowView)e.Row.Item;
                    string t_quan = drv["QUANTITY"].ToString(), t_barcode = drv["BARCODE"].ToString();
                    DateTime? t_daterazl = null;
                    if(e.Column.Header.Equals("Кол-во")) {
                        t_quan = (e.EditingElement as TextBox).Text;
                    }
                    if (e.Column.Header.Equals("Barcode"))
                    {
                        t_barcode = (e.EditingElement as TextBox).Text;
                    }
                    //not used
                    if (e.Column.Header.Equals("Дата розлива"))
                    {
                        t_daterazl = (e.EditingElement as DatePicker).SelectedDate;
                    }
                    //-------
                    if((sender as DataGrid).Equals(dg_boxes_tsd))
                        dbConnection.updateRowFromDB(DBConnection.boxes_tsd_table, drv["RN"].ToString(), t_barcode, t_quan);
                    else if((sender as DataGrid).Equals(dg_from_tsd))
                        dbConnection.updateRowFromDB(DBConnection.from_tsd_table, drv["RN"].ToString(), t_barcode, t_quan);
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                writeLog(ex.Message, MSG_Code.ERROR);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }
      
    }
}