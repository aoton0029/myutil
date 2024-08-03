using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations
{
    public class Page : UserControl, IPageOverrides, ICustomQueryInterface, IWinRTObject, IDynamicInterfaceCastable, IEquatable<Page>
    {
        internal sealed class _IPageFactory : IWinRTObject, IDynamicInterfaceCastable
        {
            private IObjectReference _obj;

            private static _IPageFactory _instance = new _IPageFactory();

            private volatile ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> _queryInterfaceCache;

            private volatile ConcurrentDictionary<RuntimeTypeHandle, object> _additionalTypeData;

            private IntPtr ThisPtr => _obj.ThisPtr;

            internal static _IPageFactory Instance => _instance;

            IObjectReference IWinRTObject.NativeObject => _obj;

            bool IWinRTObject.HasUnwrappableNativeObject => false;

            ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> IWinRTObject.QueryInterfaceCache => _queryInterfaceCache ?? MakeQueryInterfaceCache();

            ConcurrentDictionary<RuntimeTypeHandle, object> IWinRTObject.AdditionalTypeData => _additionalTypeData ?? MakeAdditionalTypeData();

            public _IPageFactory()
            {
                _obj = WinRT.ActivationFactory<Page>.As(GuidGenerator.GetIID(typeof(IPageFactory).GetHelperType()));
            }

            private ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> MakeQueryInterfaceCache()
            {
                Interlocked.CompareExchange(ref _queryInterfaceCache, new ConcurrentDictionary<RuntimeTypeHandle, IObjectReference>(), null);
                return _queryInterfaceCache;
            }

            private ConcurrentDictionary<RuntimeTypeHandle, object> MakeAdditionalTypeData()
            {
                Interlocked.CompareExchange(ref _additionalTypeData, new ConcurrentDictionary<RuntimeTypeHandle, object>(), null);
                return _additionalTypeData;
            }

            public unsafe IntPtr CreateInstance(object baseInterface, out IntPtr innerInterface)
            {
                ObjectReferenceValue value = default(ObjectReferenceValue);
                IntPtr intPtr = default(IntPtr);
                IntPtr result = default(IntPtr);
                try
                {
                    value = MarshalInspectable<object>.CreateMarshaler2(baseInterface);
                    ExceptionHelpers.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr, out IntPtr, out IntPtr, int>)(*(IntPtr*)((nint)(*(IntPtr*)(void*)ThisPtr) + (nint)6 * (nint)sizeof(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, out IntPtr, out IntPtr, int>))))(ThisPtr, MarshalInspectable<object>.GetAbi(value), out intPtr, out result));
                    innerInterface = intPtr;
                    return result;
                }
                finally
                {
                    MarshalInspectable<object>.DisposeMarshaler(value);
                }
            }
        }

        private IObjectReference _inner;

        private volatile IObjectReference ___objRef_global__Microsoft_UI_Xaml_Controls_IPage;

        private volatile IObjectReference ___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides;

        private static volatile IObjectReference ___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics;

        private IntPtr ThisPtr
        {
            get
            {
                if (_inner != null)
                {
                    return _inner.ThisPtr;
                }

                return ((IWinRTObject)this).NativeObject.ThisPtr;
            }
        }

        private IObjectReference _objRef_global__Microsoft_UI_Xaml_Controls_IPage => ___objRef_global__Microsoft_UI_Xaml_Controls_IPage ?? Make___objRef_global__Microsoft_UI_Xaml_Controls_IPage();

        private IObjectReference _objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides => ___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides ?? Make___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides();

        private IPage _default => null;

        private static IObjectReference _objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics => ___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics ?? Make___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics();

        //
        // 概要:
        //     Identifies the BottomAppBar dependency property.
        //
        // 戻り値:
        //     The identifier for the BottomAppBar dependency property.
        public static DependencyProperty BottomAppBarProperty => IPageStaticsMethods.get_BottomAppBarProperty(_objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics);

        //
        // 概要:
        //     Identifies the Frame dependency property.
        //
        // 戻り値:
        //     The identifier for the Frame dependency property.
        public static DependencyProperty FrameProperty => IPageStaticsMethods.get_FrameProperty(_objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics);

        //
        // 概要:
        //     Identifies the TopAppBar dependency property.
        //
        // 戻り値:
        //     The identifier for the TopAppBar dependency property.
        public static DependencyProperty TopAppBarProperty => IPageStaticsMethods.get_TopAppBarProperty(_objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics);

        bool IWinRTObject.HasUnwrappableNativeObject => GetType() == typeof(Page);

        IObjectReference IWinRTObject.NativeObject => _inner;

        //
        // 概要:
        //     Gets a reference to an AppBar displayed at the bottom of the page, if any.
        //
        // 戻り値:
        //     A reference to an AppBar displayed at the bottom of the page, or **null**.
        public AppBar BottomAppBar
        {
            get
            {
                return IPageMethods.get_BottomAppBar(_objRef_global__Microsoft_UI_Xaml_Controls_IPage);
            }
            set
            {
                IPageMethods.set_BottomAppBar(_objRef_global__Microsoft_UI_Xaml_Controls_IPage, value);
            }
        }

        //
        // 概要:
        //     Gets the controlling Frame for the Page content.
        //
        // 戻り値:
        //     The controlling Frame for the Page content.
        public Frame Frame => IPageMethods.get_Frame(_objRef_global__Microsoft_UI_Xaml_Controls_IPage);

        //
        // 概要:
        //     Gets or sets the navigation mode that indicates whether this Page is cached,
        //     and the period of time that the cache entry should persist.
        //
        // 戻り値:
        //     A value of the enumeration. The default is **Disabled**.
        public NavigationCacheMode NavigationCacheMode
        {
            get
            {
                return IPageMethods.get_NavigationCacheMode(_objRef_global__Microsoft_UI_Xaml_Controls_IPage);
            }
            set
            {
                IPageMethods.set_NavigationCacheMode(_objRef_global__Microsoft_UI_Xaml_Controls_IPage, value);
            }
        }

        //
        // 概要:
        //     Gets a reference to an AppBar displayed at the top of the page, if any.
        //
        // 戻り値:
        //     A reference to an AppBar displayed at the top of the page, or **null**.
        public AppBar TopAppBar
        {
            get
            {
                return IPageMethods.get_TopAppBar(_objRef_global__Microsoft_UI_Xaml_Controls_IPage);
            }
            set
            {
                IPageMethods.set_TopAppBar(_objRef_global__Microsoft_UI_Xaml_Controls_IPage, value);
            }
        }

        private IObjectReference Make___objRef_global__Microsoft_UI_Xaml_Controls_IPage()
        {
            Interlocked.CompareExchange(ref ___objRef_global__Microsoft_UI_Xaml_Controls_IPage, ((IWinRTObject)this).NativeObject.As<IUnknownVftbl>(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EControls_002EIPage())), null);
            return ___objRef_global__Microsoft_UI_Xaml_Controls_IPage;
        }

        private IObjectReference Make___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides()
        {
            Interlocked.CompareExchange(ref ___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides, ((IWinRTObject)this).NativeObject.As<IUnknownVftbl>(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EControls_002EIPageOverrides())), null);
            return ___objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides;
        }

        //
        // 概要:
        //     Initializes a new instance of the Page class.
        public Page()
            : base(DerivedComposed.Instance)
        {
            bool flag = GetType() != typeof(Page);
            IntPtr innerInterface;
            IntPtr newInstance = _IPageFactory.Instance.CreateInstance(flag ? this : null, out innerInterface);
            try
            {
                ComWrappersHelper.Init(flag, this, newInstance, innerInterface, out _inner);
            }
            finally
            {
                Marshal.Release(innerInterface);
            }
        }

        public new static I As<I>()
        {
            return WinRT.ActivationFactory<Page>.AsInterface<I>();
        }

        private static IObjectReference Make___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics()
        {
            Interlocked.CompareExchange(ref ___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics, WinRT.ActivationFactory<Page>.As(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EControls_002EIPageStatics())), null);
            return ___objRef_global__Microsoft_UI_Xaml_Controls_IPageStatics;
        }

        public new static Page FromAbi(IntPtr thisPtr)
        {
            if (thisPtr == IntPtr.Zero)
            {
                return null;
            }

            return MarshalInspectable<Page>.FromAbi(thisPtr);
        }

        protected internal Page(IObjectReference objRef)
            : base(DerivedComposed.Instance)
        {
            _inner = objRef.As(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EControls_002EIPage()));
        }

        public static bool operator ==(Page x, Page y)
        {
            return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
        }

        public static bool operator !=(Page x, Page y)
        {
            return !(x == y);
        }

        public bool Equals(Page other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is Page page)
            {
                return this == page;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ThisPtr.GetHashCode();
        }

        protected Page(DerivedComposed _)
            : base(_)
        {
        }

        //
        // 概要:
        //     Invoked immediately after the Page is unloaded and is no longer the current source
        //     of a parent Frame.
        //
        // パラメーター:
        //   e:
        //     Event data that can be examined by overriding code. The event data is representative
        //     of the navigation that has unloaded the current Page.
        protected virtual void OnNavigatedFrom(NavigationEventArgs e)
        {
            IPageOverridesMethods.OnNavigatedFrom(_objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides, e);
        }

        void IPageOverrides.OnNavigatedFrom(NavigationEventArgs e)
        {
            OnNavigatedFrom(e);
        }

        //
        // 概要:
        //     Invoked when the Page is loaded and becomes the current source of a parent Frame.
        //
        //
        // パラメーター:
        //   e:
        //     Event data that can be examined by overriding code. The event data is representative
        //     of the pending navigation that will load the current Page. Usually the most relevant
        //     property to examine is Parameter.
        protected virtual void OnNavigatedTo(NavigationEventArgs e)
        {
            IPageOverridesMethods.OnNavigatedTo(_objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides, e);
        }

        void IPageOverrides.OnNavigatedTo(NavigationEventArgs e)
        {
            OnNavigatedTo(e);
        }

        //
        // 概要:
        //     Invoked immediately before the Page is unloaded and is no longer the current
        //     source of a parent Frame.
        //
        // パラメーター:
        //   e:
        //     Event data that can be examined by overriding code. The event data is representative
        //     of the navigation that will unload the current Page unless canceled. The navigation
        //     can potentially be canceled by setting Cancel.
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            IPageOverridesMethods.OnNavigatingFrom(_objRef_global__Microsoft_UI_Xaml_Controls_IPageOverrides, e);
        }

        void IPageOverrides.OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            OnNavigatingFrom(e);
        }

        protected override bool IsOverridableInterface(Guid iid)
        {
            if (!(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EABI_002EMicrosoft_002EUI_002EXaml_002EControls_002EIPageOverrides()) == iid))
            {
                return base.IsOverridableInterface(iid);
            }

            return true;
        }

        CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out IntPtr ppv)
        {
            ppv = IntPtr.Zero;
            if (IsOverridableInterface(iid) || WinRT.InterfaceIIDs.IInspectable_IID == iid)
            {
                return CustomQueryInterfaceResult.NotHandled;
            }

            if (((IWinRTObject)this).NativeObject.TryAs(iid, out ppv) >= 0)
            {
                return CustomQueryInterfaceResult.Handled;
            }

            return CustomQueryInterfaceResult.NotHandled;
        }
    }
}
