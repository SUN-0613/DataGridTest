using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DataGridTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// DataGrid.ItemsSource
        /// </summary>
        private ObservableCollection<BindingDataGrid> _Bind { get; set; }

        /// <summary>
        /// DataGridCell.Editmode
        /// </summary>
        private bool _CanEdit = true;

        private bool _IsEditMode = false;

        private Point _SelectedCell = new Point(-1.0, -1.0);

        /// <summary>
        /// new
        /// </summary>
        public MainWindow()
        {

            InitializeComponent();

            _Bind = new ObservableCollection<BindingDataGrid>
            {
                new BindingDataGrid(1, 0)
            };

            DG.LoadingRow += (sender, e) => 
            {
                //e.Row.Header = e.Row.GetIndex() + 1;
                TextBlock textBlock = new TextBlock();
                Binding bind = new Binding("Index")
                {
                    Source = _Bind[_Bind.Count - 1]
                };
                textBlock.SetBinding(TextBlock.TextProperty, bind);
                e.Row.Header = textBlock;
            };
            DG.ItemsSource = _Bind;

        }

        /// <summary>
        /// Add Column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            for (int iLoop = 0; iLoop < _Bind.Count; iLoop++)
            {
                _Bind[iLoop].Column.Add(_Bind[iLoop].Column.Count.ToString());
            }

            DG.Columns.Add(new DataGridTextColumn { Header = _Bind[0].Column.Count, Binding = new Binding(String.Format("Column[{0}]", _Bind[0].Column.Count - 1)) });

            OnDataGridColumnsSizeChange();

        }

        /// <summary>
        /// Delete Column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (DG.Columns.Count > 0)
            {

                DG.Columns.RemoveAt(DG.Columns.Count - 1);

                for (int iLoop = 0; iLoop < _Bind.Count; iLoop++)
                {
                    _Bind[iLoop].Column.RemoveAt(_Bind[iLoop].Column.Count - 1);
                }

                OnDataGridColumnsSizeChange();

            }

        }


        /// <summary>
        /// DataGrid.Columns size change
        /// </summary>
        private void OnDataGridColumnsSizeChange()
        {

            if (DG.Columns.Count > 0)
            {

                double width = (DG.ActualWidth - DG.RowHeaderActualWidth - 2.0) / DG.Columns.Count;

                foreach (var column in DG.Columns)
                {
                    column.Width = width;
                }

            }

        }

        private void DataGridRowHeader_Click(object sender, RoutedEventArgs e)
        {

            var rowHeader = sender as System.Windows.Controls.Primitives.DataGridRowHeader;
            var textBlock = (rowHeader.Content as TextBlock);

            _SelectedCell = new Point(-1.0, -1.0);
            for (int iLoop = 0; iLoop < DG.Items.Count; iLoop++)
            {

                var bind = (DG.Items[iLoop] as BindingDataGrid);

                if (textBlock.Text.Equals(bind.Index.ToString()))
                {
                    _SelectedCell = new Point(Convert.ToDouble(bind.Index - 1), 0.0);
                    break;
                }
            }
            
        }

        /// <summary>
        /// Edit mode by Single click 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DG_GotFocus(object sender, RoutedEventArgs e)
        {

            Point newPoint = new Point(DG.Items.IndexOf(DG.CurrentItem), DG.CurrentColumn.DisplayIndex);

            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {

                if (!newPoint.Equals(_SelectedCell))
                {

                    if (_CanEdit)
                    {
                        DG.BeginEdit(e);
                    }
                    else
                    {
                        _CanEdit = true;
                    }

                    _SelectedCell = newPoint;

                }

            }

        }

        /// <summary>
        /// Move Cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DG_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            var dg = sender as DataGrid;
            int row = dg.SelectedIndex;

            switch (e.Key)
            {

                case Key.Return:
                    row += (!Keyboard.Modifiers.Equals(ModifierKeys.Shift) ? 1 : -1);
                    break;

                case Key.Down:
                    row += 1;
                    break;

                case Key.Up:
                    row -= 1;
                    break;

                case Key.Delete:
                    if (!_IsEditMode)
                    {
                        // 最後の１行は残す
                        if (_Bind.Count > 1)
                        {

                            int count = 0;
                            for (int iLoop = row + 1; iLoop < _Bind.Count; iLoop++)
                            {

                                if (!iLoop.Equals(row))
                                {
                                    _Bind[iLoop].Index = ++count;
                                }
                            }

                            _Bind.RemoveAt(row);

                        }
                    }
                    return;

                default:
                    return;

            }

            if (row < 0 || dg.Items.Count < row)
            {
                return;
            }

            // add line
            if (row.Equals(dg.Items.Count))
            {
                _Bind.Add(new BindingDataGrid(_Bind.Count + 1, DG.Columns.Count));
            }

            _CanEdit = false;
            dg.CommitEdit();
            //_CanEdit = true;

            dg.SelectedIndex = row;
            dg.CurrentCell = new DataGridCellInfo(dg.Items[row], dg.CurrentColumn);

            e.Handled = true;

            dg.BeginEdit();

        }

        private void DG_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _IsEditMode = true;
        }

        private void DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _IsEditMode = false;
        }

    }

    /// <summary>
    /// DataGrid.ItemsSource.Class
    /// </summary>
    public class BindingDataGrid
    {

        public int Index { get; set; }

        public ObservableCollection<string> Column { get; set; }

        public BindingDataGrid(int rowNo, int columnNo)
        {

            Index = rowNo;

            Column = new ObservableCollection<string>();

            for (int iLoop = 0; iLoop < columnNo; iLoop++)
            {
                Column.Add((Index + iLoop).ToString());
            }
            
        }

    }

}
