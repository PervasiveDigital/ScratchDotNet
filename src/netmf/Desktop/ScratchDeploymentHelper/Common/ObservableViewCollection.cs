//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PervasiveDigital.Scratch.DeploymentHelper.Common
{
    public class GroupHeader<K, T> : ObservableCollection<T>, IEnumerable<T>, IGrouping<K, T>, IDisposable
        where T : class, IDisposable
    {
        public GroupHeader(K key)
        {
            Key = key;
        }

        public GroupHeader(K key, IEnumerable<T> values)
        {
            Key = key;
            this.AddRange(values);
        }

        public void Dispose()
        {
            foreach (var item in this)
            {
                item.Dispose();
            }
            this.Clear();
        }

        public K Key { get; set; }

        public new IEnumerator<T> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<T>)base.GetEnumerator();
        }
    }

    public class ObservableGroupedViewCollection<TItemData, TItemView, TGroupKey> : ObservableCollection<GroupHeader<TGroupKey, TItemView>>
        where TItemData : class
        where TItemView : class, IViewProxy<TItemData>, new()
    {
        private Func<TItemData, TGroupKey> _keyGeneratorFn;
        private ObservableCollection<TItemData> _source = null;
        private Predicate<TItemData> _predicate;

        public ObservableGroupedViewCollection(Func<TItemData, TGroupKey> keyGeneratorFn)
        {
            _keyGeneratorFn = keyGeneratorFn;
        }

        public virtual void Attach(ObservableCollection<TItemData> source, Predicate<TItemData> pred = null)
        {
            _predicate = null;
            if (_source != null)
                Detach();
            _predicate = pred;
            _source = source;
            _source.CollectionChanged += _source_CollectionChanged;
            foreach (var item in _source)
            {
                InternalAdd(item);
            }
        }

        public void Detach()
        {
            if (_source != null)
            {
                _source.CollectionChanged -= _source_CollectionChanged;
                _source = null;
                foreach (var item in this)
                {
                    item.Dispose();
                }
                this.Clear();
            }
        }

        void _source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private void InternalAdd(TItemData item)
        {
        }

        public Func<TItemData, TGroupKey> KeyGenerator
        {
            get { return _keyGeneratorFn; }
            set { _keyGeneratorFn = value; }
        }

    }

    /// <summary>
    /// An observable view collection for MVVM.  Each time you add a data type, it will be wrapped in the correct view model type.
    /// </summary>
    /// <typeparam name="TData">The data model type</typeparam>
    /// <typeparam name="TView">The view model type that is used to wrap the view model type in this collection</typeparam>
    public class ObservableViewCollection<TData, TView> : ObservableCollection<TView>, IDisposable
        where TData : class
        where TView : class, IViewProxy<TData>
    {
        private ObservableCollection<TData> _source = null;
        private TView _placeholder;
        private CancellationToken? _ct;
        private Predicate<TData> _predicate;
        private readonly Dispatcher _dispatcher;
        private Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();
        private AsyncLock _lock = new AsyncLock();

        public ObservableViewCollection(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispose()
        {
            this.Detach();
            foreach (var item in this)
            {
                item.Dispose();
            }
            this.Clear();
        }

        public virtual TView Placeholder
        {
            get { return _placeholder; }
            set { _placeholder = value; EnforceCollectionInvariants(); }
        }

        public virtual Dispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        /// <summary>
        /// The view map allows for mapping more data types to view types.  Each data type must 
        /// inherit from TData and each view type must inherit from TView
        /// </summary>
        public Dictionary<Type, Type> ViewMap
        {
            get { return _typeMap; }
        }

        public virtual async Task Attach(ObservableCollection<TData> source, CancellationToken? ct = null, Predicate<TData> pred = null)
        {
            _ct = ct;
            _predicate = null;
            if (_source != null)
                Detach();
            _predicate = pred;
            _source = source;
            ((INotifyCollectionChanged)_source).CollectionChanged += _source_CollectionChanged;

            // capture this to prevent against exceptions due to list-changed-during-enumeration problems
            //   The collchanged event will patch things up
            using (var releaser = await _lock.LockAsync())
            {
                var list = new List<TData>(_source);
                foreach (var item in list)
                {
                    if (!this.Contains(item))
                    {
                        InternalAdd(item);
                    }
                }
                EnforceCollectionInvariants();
            }
        }

        protected virtual TView InternalAdd(TData item)
        {
            if (_predicate == null || _predicate(item))
            {
                TView view = null;

                if (_typeMap.ContainsKey(item.GetType()))
                {
                    if (this.Dispatcher != null)
                    {
                        if (_ct!=null)
                            view = Activator.CreateInstance(_typeMap[item.GetType()], this.Dispatcher, _ct) as TView;
                        else
                            view = Activator.CreateInstance(_typeMap[item.GetType()], this.Dispatcher) as TView;
                    }
                    else
                        view = Activator.CreateInstance(_typeMap[item.GetType()]) as TView;
                    
                    view.ViewSource = item;
                }
                else
                {
                    if (this.Dispatcher != null)
                        view = Activator.CreateInstance(typeof(TView), this.Dispatcher) as TView;
                    else
                        view = Activator.CreateInstance(typeof(TView)) as TView;
                    view.ViewSource = item;
                }

                if (view != null)
                {
                    this.Add(view);
                }
                else
                {
                    throw new InvalidOperationException("No view type was found for this data type");
                }

                EnforceCollectionInvariants();
                return view;
            }
            return null;
        }

        public bool Contains(TData data)
        {
            return (from item in this where item.ViewSource == data select item.ViewSource).FirstOrDefault() != null;
        }

        public bool Remove(TData dataItem)
        {
            bool fResult;
            var itemWrapper = (from item in this where item.ViewSource == dataItem select item).FirstOrDefault();
            if (itemWrapper != null)
                fResult = this.Remove(itemWrapper);
            else
                fResult = false;
            EnforceCollectionInvariants();
            return fResult;
        }

        public void Detach()
        {
            if (_source != null)
            {
                ((INotifyCollectionChanged)_source).CollectionChanged -= _source_CollectionChanged;
                _source = null;
            }
        }

        // used to briefly freeze updates if you want to iterate over Items with a foreach or use a .ToArray()
        private AsyncLock _updateLock = new AsyncLock();
        public AsyncLock UpdateLock
        {
            get { return _updateLock; }
        }

        async void _source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            // block simultaneous accesses that might occur during Attach initialization
            using (var releaser = await _lock.LockAsync())
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var item in args.NewItems)
                            {
                                if (item is TData)
                                {
                                    InternalAdd((TData)item);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var item in args.OldItems)
                            {
                                if (item is TData)
                                {
                                    this.Remove((TData)item);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            this.Clear();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            foreach (var item in args.OldItems)
                            {
                                if (item is TData)
                                {
                                    this.Remove((TData)item);
                                }
                            }
                            foreach (var item in args.NewItems)
                            {
                                if (item is TData)
                                {
                                    InternalAdd((TData)item);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Move:
                            this.Move(args.OldStartingIndex, args.NewStartingIndex);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                });
            }
        }

        private void EnforceCollectionInvariants()
        {
            if (_placeholder != null)
            {
                if (!this.Any())
                {
                    this.Add(_placeholder);
                }
                if (this.Count() > 1)
                {
                    if (this.Contains(_placeholder))
                    {
                        this.Remove(_placeholder);
                    }
                }
            }
        }

        public void Sort<TKey>(Func<TView, TKey> keySelector)
        {
            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = this.Count - 1; i >= 0; i--)
            {
                for (var j = 1; j <= i; j++)
                {
                    var o1 = this[j - 1];
                    var o2 = this[j];
                    if (comparer.Compare(keySelector(o1), keySelector(o2)) > 0)
                    {
                        this.Move(j-1,j);
                    }
                }
            }
        }
    }

    public delegate TView GetErrorItemForExceptionFn<TView>(Exception exc);

    public class ObservableViewCollection<TParent, TData, TView> : ObservableViewCollection<TData, TView>
        where TParent : class
        where TData : class
        where TView : class, IViewProxyWithParent<TParent, TData>
    {
        private TParent _parent = null;

        public ObservableViewCollection(Dispatcher dispatcher, TParent parent = null) : base(dispatcher)
        {
            _parent = parent;
        }

        public TParent Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        protected override TView InternalAdd(TData item)
        {
            var newView = base.InternalAdd(item);
            if (newView != null && _parent != null)
            {
                newView.Parent = _parent;
            }
            return newView;
        }
    }

    public static class ObservableCollectionExtensions
    {
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) return;

            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = source.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    if (comparer.Compare(keySelector(o1), keySelector(o2)) > 0)
                    {
                        source.Move(j-1,j);
                        //source.Remove(o1);
                        //source.Insert(j, o1);
                    }
                }
            }
        }
    }

}
