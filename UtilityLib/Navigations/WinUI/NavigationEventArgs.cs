using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Navigations.WinUI
{
    public sealed class NavigationEventArgs : ICustomQueryInterface, IDynamicInterfaceCastable, IEquatable<NavigationEventArgs>
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct InterfaceTag<I>
        {
        }

        private IObjectReference _inner;

        private volatile ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> _queryInterfaceCache;

        private volatile ConcurrentDictionary<RuntimeTypeHandle, object> _additionalTypeData;

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

        private IObjectReference _objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs => _inner;

        private INavigationEventArgs _default => null;

        bool IWinRTObject.HasUnwrappableNativeObject => true;

        IObjectReference IWinRTObject.NativeObject => _inner;

        ConcurrentDictionary<RuntimeTypeHandle, IObjectReference> IWinRTObject.QueryInterfaceCache => _queryInterfaceCache ?? MakeQueryInterfaceCache();

        ConcurrentDictionary<RuntimeTypeHandle, object> IWinRTObject.AdditionalTypeData => _additionalTypeData ?? MakeAdditionalTypeData();

        //
        // 概要:
        //     Gets the root node of the target page's content.
        //
        // 戻り値:
        //     The root node of the target page's content.
        public object Content => INavigationEventArgsMethods.get_Content(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);

        //
        // 概要:
        //     Gets a value that indicates the direction of movement during navigation
        //
        // 戻り値:
        //     A value of the enumeration.
        public NavigationMode NavigationMode => INavigationEventArgsMethods.get_NavigationMode(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);

        //
        // 概要:
        //     Gets a value that indicates the animated transition associated with the navigation.
        //
        //
        // 戻り値:
        //     Info about the animated transition.
        public NavigationTransitionInfo NavigationTransitionInfo => INavigationEventArgsMethods.get_NavigationTransitionInfo(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);

        //
        // 概要:
        //     Gets any "Parameter" object passed to the target page for the navigation.
        //
        // 戻り値:
        //     An object that potentially passes parameters to the navigation target. May be
        //     null.
        public object Parameter => INavigationEventArgsMethods.get_Parameter(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);

        //
        // 概要:
        //     Gets the data type of the source page.
        //
        // 戻り値:
        //     The data type of the source page, represented as *namespace*.*type* or simply
        //     *type*.
        public Type SourcePageType => INavigationEventArgsMethods.get_SourcePageType(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);

        //
        // 概要:
        //     Gets the Uniform Resource Identifier (URI) of the target.
        //
        // 戻り値:
        //     A value that represents the Uniform Resource Identifier (URI).
        public Uri Uri
        {
            get
            {
                return INavigationEventArgsMethods.get_Uri(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs);
            }
            set
            {
                INavigationEventArgsMethods.set_Uri(_objRef_global__Microsoft_UI_Xaml_Navigation_INavigationEventArgs, value);
            }
        }

        public static NavigationEventArgs FromAbi(IntPtr thisPtr)
        {
            if (thisPtr == IntPtr.Zero)
            {
                return null;
            }

            return MarshalInspectable<NavigationEventArgs>.FromAbi(thisPtr);
        }

        internal NavigationEventArgs(IObjectReference objRef)
        {
            _inner = objRef.As(new Guid(global::_003CGuidPatcherImplementationDetails_003E._003CIIDData_003EMicrosoft_002EUI_002EXaml_002ENavigation_002EINavigationEventArgs()));
        }

        public static bool operator ==(NavigationEventArgs x, NavigationEventArgs y)
        {
            return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
        }

        public static bool operator !=(NavigationEventArgs x, NavigationEventArgs y)
        {
            return !(x == y);
        }

        public bool Equals(NavigationEventArgs other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is NavigationEventArgs navigationEventArgs)
            {
                return this == navigationEventArgs;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ThisPtr.GetHashCode();
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

        private bool IsOverridableInterface(Guid iid)
        {
            return false;
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

