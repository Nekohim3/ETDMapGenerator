using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;

namespace MapGeneratorTest.Utils;

public class ObservableCollectionWithSelectedItem<T> : ObservableCollection<T> where T : class
{
    #region EventDelegate

    public delegate void                 SelectedChangedHandler(ObservableCollectionWithSelectedItem<T> sender, T newSelection, T oldSelection);
    public event SelectedChangedHandler? SelectionChanged;

    #endregion

    #region Private properties

    private IList<T> _sourceCollection      = new List<T>();
    private bool     _raiseCollectionChange = true;
    private Timer    _filterTimer;

    #endregion

    #region Public properties

    private Func<T, bool>? _filter;
    public Func<T, bool>? Filter
    {
        get => _filter;
        set
        {
            _filter = value;
            OnPropertyChanged();
            if (UseFilter)
            {
                if (UseDeffer)
                    _filterTimer.Change(100, Timeout.Infinite);
                else
                    SetRange(_filter != null ? _sourceCollection.Where(_filter) : _sourceCollection, false);
            }
        }
    }

    private T? _selectedItem;
    public T? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value)
                return;
            var oldSelection = _selectedItem;
            _selectedItem = value;

            SelectionChanged?.Invoke(this, _selectedItem, oldSelection);
            OnPropertyChanged();
            OnPropertyChanged("Position");
        }
    }

    public bool IsSelectedLast  => Count > 0 && SelectedItem != null && IndexOf(SelectedItem) == Count - 1;
    public bool IsSelectedFirst => Count > 0 && SelectedItem != null && IndexOf(SelectedItem) == 0;

    public int Position
    {
        get
        {
            if (Count == 0 || SelectedItem == null)
                return -1;
            return IndexOf(SelectedItem);
        }
    }

    public bool UseDeffer = false;
    public bool UseFilter = false;

    #endregion

    #region Ctor

    public ObservableCollectionWithSelectedItem() : base()
    {
        _filterTimer      =  new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        CollectionChanged += OnCollectionChanged;
        SetSelectedToFirst();
    }

    public ObservableCollectionWithSelectedItem(IEnumerable<T> list) : base(list)
    {
        _filterTimer      =  new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _sourceCollection =  Items.ToList();
        CollectionChanged += OnCollectionChanged;
        SetSelectedToFirst();
    }

    #endregion

    #region Public funcs

    public void SetSelectedToFirst()
    {
        SelectedItem = this.FirstOrDefault();
    }

    public void SetSelectedToLast()
    {
        SelectedItem = this.LastOrDefault();
    }

    public bool SetSelectedTo(T item)
    {
        var obj = this.FirstOrDefault(x => x.Equals(item));
        if (obj == null)
            return false;
        SelectedItem = obj;
        return true;
    }

    public bool SetSelectedToId(int id)
    {
        var prop = typeof(T).GetProperty("Id");
        if (prop == null)
            return false;
        foreach (var x in this)
        {
            if (!int.TryParse(prop.GetValue(x)?.ToString(), out var res) || res != id)
                continue;

            SelectedItem = x;
            return true;
        }

        return false;
    }

    public bool SetSelectedToPosition(int pos)
    {
        if (pos < 0 || pos > Count - 1)
            return false;

        SelectedItem = this[pos];
        return true;
    }

    public bool SetSelectedToNext()
    {
        var next = GetNext();
        return next != null && SetSelectedTo(next);
    }

    public bool SetSelectedToPrev()
    {
        var prev = GetPrev();
        return prev != null && SetSelectedTo(prev);
    }

    public T? GetPrev()
    {
        if (SelectedItem == null)
            return default;
        var ind = IndexOf(SelectedItem);
        return ind == 0 ? default : this[ind - 1];
    }

    public T? GetNext()
    {
        if (SelectedItem == null)
            return default;
        var ind = IndexOf(SelectedItem);
        return ind == Count - 1 ? default : this[ind + 1];
    }

    public new void Clear()
    {
        SelectedItem = null;
        base.Clear();
    }

    public void SetRange(IEnumerable<T> list, bool raise = true)
    {
        _raiseCollectionChange = raise;
        Clear();
        this.AddRange(list);
        SetSelectedToFirst();
        _raiseCollectionChange = true;
    }

    #endregion

    #region Callbacks

    private void TimerCallback(object state)
    {
        Dispatcher.UIThread.Post(() =>
                                 {
                                     SetRange(_filter != null ? _sourceCollection.Where(_filter) : _sourceCollection, false);
                                 }, DispatcherPriority.Background);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_raiseCollectionChange)
            return;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _sourceCollection.Insert(e.NewStartingIndex, e.NewItems[0] as T);
                break;
            case NotifyCollectionChangedAction.Move:
                _sourceCollection.RemoveAt(e.OldStartingIndex);
                _sourceCollection.Insert(e.NewStartingIndex, e.NewItems[0] as T);
                break;
            case NotifyCollectionChangedAction.Remove:
                _sourceCollection.Remove(e.OldItems[0] as T);
                break;
            case NotifyCollectionChangedAction.Replace:
                _sourceCollection.RemoveAt(e.OldStartingIndex);
                _sourceCollection.Insert(e.NewStartingIndex, e.NewItems[0] as T);
                break;
            case NotifyCollectionChangedAction.Reset:
                _sourceCollection.Clear();
                break;
        }

        if (UseFilter)
        {
            if (UseDeffer)
                _filterTimer.Change(100, Timeout.Infinite);
            else
                SetRange(_filter != null ? _sourceCollection.Where(_filter) : _sourceCollection, false);
        }
    }

    #endregion

    #region OnPropertyChange

    protected override event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    #endregion
}
