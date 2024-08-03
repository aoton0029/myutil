using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static UtilityLib.Navigations.NavigationTransitionInfo;

namespace UtilityLib.Navigations
{
    public class NavigationTransitionInfo : DependencyObject, INavigationTransitionInfoOverrides, ICustomQueryInterface, IWinRTObject, IDynamicInterfaceCastable, IEquatable<NavigationTransitionInfo>
    {
        internal sealed class _INavigationTransitionInfoFactory : IWinRTObject, IDynamicInterfaceCastable
        {
            private IObjectReference _obj;

            private static _INavigationTransitionInfoFactory _instance = new _INavigationTransitionInfoFactory();

            private volatile ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> _queryInterfaceCache;

            private volatile ConcurrentDictionary<RuntimeTypeHandle, object> _additionalTypeData;

            private IntPtr ThisPtr => _obj.ThisPtr;

            internal static _INavigationTransitionInfoFactory Instance => _instance;

            IObjectReference IWinRTObject.NativeObject => _obj;

            bool IWinRTObject.HasUnwrappableNativeObject => false;

            ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> IWinRTObject.QueryInterfaceCache => _queryInterfaceCache ?? MakeQueryInterfaceCache();

            ConcurrentDictionary<RuntimeTypeHandle, object> IWinRTObject.AdditionalTypeData => _additionalTypeData ?? MakeAdditionalTypeData();

            public _INavigationTransitionInfoFactory()
            {
                _obj = WinRT.ActivationFactory<NavigationTransitionInfo>.As(GuidGenerator.GetIID(typeof(INavigationTransitionInfoFactory).GetHelperType()));
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

        private volatile IObjectReference ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo;

        private volatile IObjectReference ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides;

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

        private IObjectReference _objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo => ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo ?? Make___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo();

        private IObjectReference _objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides => ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides ?? Make___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides();

        private INavigationTransitionInfo _default => null;

        bool IWinRTObject.HasUnwrappableNativeObject => GetType() == typeof(NavigationTransitionInfo);

        IObjectReference IWinRTObject.NativeObject => _inner;

        private IObjectReference Make___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo()
        {
            Interlocked.CompareExchange(ref ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo, ((IWinRTObject)this).NativeObject.As<IUnknownVftbl>(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EMedia_002EAnimation_002EINavigationTransitionInfo())), null);
            return ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfo;
        }

        private IObjectReference Make___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides()
        {
            Interlocked.CompareExchange(ref ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides, ((IWinRTObject)this).NativeObject.As<IUnknownVftbl>(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EMedia_002EAnimation_002EINavigationTransitionInfoOverrides())), null);
            return ___objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides;
        }

        //
        // 概要:
        //     Initializes a new instance of the NavigationTransitionInfo class.
        protected NavigationTransitionInfo()
            : base(DerivedComposed.Instance)
        {
            bool flag = GetType() != typeof(NavigationTransitionInfo);
            IntPtr innerInterface;
            IntPtr newInstance = _INavigationTransitionInfoFactory.Instance.CreateInstance(flag ? this : null, out innerInterface);
            try
            {
                ComWrappersHelper.Init(flag, this, newInstance, innerInterface, out _inner);
            }
            finally
            {
                Marshal.Release(innerInterface);
            }
        }

        public new static NavigationTransitionInfo FromAbi(IntPtr thisPtr)
        {
            if (thisPtr == IntPtr.Zero)
            {
                return null;
            }

            return MarshalInspectable<NavigationTransitionInfo>.FromAbi(thisPtr);
        }

        protected internal NavigationTransitionInfo(IObjectReference objRef)
            : base(DerivedComposed.Instance)
        {
            _inner = objRef.As(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002EMedia_002EAnimation_002EINavigationTransitionInfo()));
        }

        public static bool operator ==(NavigationTransitionInfo x, NavigationTransitionInfo y)
        {
            return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
        }

        public static bool operator !=(NavigationTransitionInfo x, NavigationTransitionInfo y)
        {
            return !(x == y);
        }

        public bool Equals(NavigationTransitionInfo other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is NavigationTransitionInfo navigationTransitionInfo)
            {
                return this == navigationTransitionInfo;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ThisPtr.GetHashCode();
        }

        protected NavigationTransitionInfo(DerivedComposed _)
            : base(_)
        {
        }

        //
        // 概要:
        //     When implemented in a derived class, gets the navigation state string that is
        //     reported for navigation actions through Frame.Navigate and similar API.
        //
        // 戻り値:
        //     The string to use for navigation state info.
        protected virtual string GetNavigationStateCore()
        {
            return INavigationTransitionInfoOverridesMethods.GetNavigationStateCore(_objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides);
        }

        string INavigationTransitionInfoOverrides.GetNavigationStateCore()
        {
            return GetNavigationStateCore();
        }

        //
        // 概要:
        //     When implemented in a derived class, sets the navigation state string that is
        //     passed for navigation actions through Frame.Navigate and similar API.
        //
        // パラメーター:
        //   navigationState:
        //     The string to use for navigation state info.
        protected virtual void SetNavigationStateCore(string navigationState)
        {
            INavigationTransitionInfoOverridesMethods.SetNavigationStateCore(_objRef_global__Microsoft_UI_Xaml_Media_Animation_INavigationTransitionInfoOverrides, navigationState);
        }

        void INavigationTransitionInfoOverrides.SetNavigationStateCore(string navigationState)
        {
            SetNavigationStateCore(navigationState);
        }

        protected override bool IsOverridableInterface(Guid iid)
        {
            if (!(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EABI_002EMicrosoft_002EUI_002EXaml_002EMedia_002EAnimation_002EINavigationTransitionInfoOverrides()) == iid))
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
