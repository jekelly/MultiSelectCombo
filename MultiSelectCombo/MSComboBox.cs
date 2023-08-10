using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

namespace MultiSelectCombo
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MultiSelectCombo"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MultiSelectCombo;assembly=MultiSelectCombo"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:MSComboBox/>
    ///
    /// </summary>
    public class MSComboBox : ComboBox
    {
        private readonly ObservableCollection<object> _selectedItems = new ObservableCollection<object>();

        static MSComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MSComboBox), new FrameworkPropertyMetadata(typeof(MSComboBox)));
            ComboBox.ItemsSourceProperty.AddOwner(typeof(MSComboBox), new FrameworkPropertyMetadata(OnItemsSourceChanged));
        }

        public MSComboBox()
        {
            ((INotifyCollectionChanged)_selectedItems).CollectionChanged += MSComboBox_CollectionChanged;
        }

        private void MSComboBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CoerceValue(SelectedObjectsSummaryProperty);
        }

        public IValueConverter SummaryConverter
        {
            get { return (IValueConverter)GetValue(SummaryConverterProperty); }
            set { SetValue(SummaryConverterProperty, value); }
        }


        public string SelectedObjectsSummary
        {
            get { return (string)GetValue(SelectedObjectsSummaryProperty); }
        }

        // Using a DependencyProperty as the backing store for SelectedObjectsSummary.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedObjectsSummaryProperty =
            DependencyProperty.Register("SelectedObjectsSummary", typeof(string), typeof(MSComboBox), new PropertyMetadata(null, null, CoerceSummary));

        private static object CoerceSummary(DependencyObject d, object baseValue)
        {
            return ((MSComboBox)d).SummaryConverter.Convert(((MSComboBox)d).SelectedObjects, typeof(string), null, CultureInfo.CurrentCulture);
        }

        // Using a DependencyProperty as the backing store for SummaryConverter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SummaryConverterProperty =
            DependencyProperty.Register("SummaryConverter", typeof(IValueConverter), typeof(MSComboBox), new PropertyMetadata(new DefaultConverter()));

        private class DefaultConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                IEnumerable<object> objects = (IEnumerable<object>)value;

                return string.Join(",", objects.Select(o => o.ToString()));
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MSComboBox msc = (MSComboBox)d;

            if (e.NewValue is TestCollectionView tcv)
            {
                foreach (var x in tcv)
                {
                    ((INotifyPropertyChanged)x).PropertyChanged += (o, ev) =>
                    {
                        if (ev.PropertyName == "IsSelected")
                        {
                            if (o is TestCollectionView.SelectableObject so)
                            {
                                if (so.IsSelected)
                                {
                                    msc._selectedItems.Add(so.Object);
                                }
                                else
                                {
                                    msc._selectedItems.Remove(so.Object);
                                }
                            }
                        }
                    };
                }
            }
        }

        public ObservableCollection<object> SelectedObjects => _selectedItems;
    }

    public class TestCollectionView : CollectionView
    {
        private readonly Dictionary<object, SelectableObject> _map = new Dictionary<object, SelectableObject>();

        private SelectableObject GetOrCreate(object o)
        {
            if (!_map.TryGetValue(o, out var so))
            {
                so = new SelectableObject() { IsSelected = false, Object = o };
                _map.Add(o, so);
            }

            return so;
        }

        public TestCollectionView(IEnumerable collection) : base(collection)
        {
        }

        protected override IEnumerator GetEnumerator()
        {
            return new TransformingEnumerator(this, base.GetEnumerator());
        }

        class TransformingEnumerator : IEnumerator
        {
            private readonly TestCollectionView tcv;
            private readonly IEnumerator enumerator;

            public TransformingEnumerator(TestCollectionView tcv, IEnumerator enumerator)
            {
                this.tcv = tcv;
                this.enumerator = enumerator;
            }

            public object Current => tcv.GetOrCreate(enumerator.Current);

            public bool MoveNext() => enumerator.MoveNext();

            public void Reset() => enumerator.Reset();
        }

        public override object CurrentItem => this.GetOrCreate(base.CurrentItem);

        public override object GetItemAt(int index) => this.GetOrCreate(base.GetItemAt(index));

        public class SelectableObject : INotifyPropertyChanged
        {
            private bool _isSelected;
            private object _object;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }
            public object Object
            {
                get => _object;
                set
                {
                    if (_object != value)
                    {
                        _object = value;
                        OnPropertyChanged(nameof(Object));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
