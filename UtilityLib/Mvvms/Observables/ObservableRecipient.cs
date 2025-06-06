﻿//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using UtilityLib.Mvvms.Messaging;

//namespace UtilityLib.Mvvms
//{
//    public abstract class ObservableRecipient : ObservableObject
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="ObservableRecipient"/> class.
//        /// </summary>
//        /// <remarks>
//        /// This constructor will produce an instance that will use the <see cref="WeakReferenceMessenger.Default"/> instance
//        /// to perform requested operations. It will also be available locally through the <see cref="Messenger"/> property.
//        /// </remarks>
//        protected ObservableRecipient()
//            : this(WeakReferenceMessenger.Default)
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="ObservableRecipient"/> class.
//        /// </summary>
//        /// <param name="messenger">The <see cref="IMessenger"/> instance to use to send messages.</param>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="messenger"/> is <see langword="null"/>.</exception>
//        protected ObservableRecipient(IMessenger messenger)
//        {
//            ArgumentNullException.ThrowIfNull(messenger);

//            Messenger = messenger;
//        }

//        /// <summary>
//        /// Gets the <see cref="IMessenger"/> instance in use.
//        /// </summary>
//        protected IMessenger Messenger { get; }

//        private bool isActive;

//        /// <summary>
//        /// Gets or sets a value indicating whether the current view model is currently active.
//        /// </summary>
//        public bool IsActive
//        {
//            get => this.isActive;

//            [RequiresUnreferencedCode(
//                "When this property is set to true, the OnActivated() method will be invoked, which will register all necessary message handlers for this recipient. " +
//                "This method requires the generated CommunityToolkit.Mvvm.Messaging.__Internals.__IMessengerExtensions type not to be removed to use the fast path. " +
//                "If this type is removed by the linker, or if the target recipient was created dynamically and was missed by the source generator, a slower fallback " +
//                "path using a compiled LINQ expression will be used. This will have more overhead in the first invocation of this method for any given recipient type. " +
//                "Alternatively, OnActivated() can be manually overwritten, and registration can be done individually for each required message for this recipient.")]
//            [RequiresDynamicCode(
//                "When this property is set to true, the OnActivated() method will be invoked, which will register all necessary message handlers for this recipient. " +
//                "This method requires the generated CommunityToolkit.Mvvm.Messaging.__Internals.__IMessengerExtensions type not to be removed to use the fast path. " +
//                "If that is present, the method is AOT safe, as the only methods being invoked to register the messages will be the ones produced by the source generator. " +
//                "If it isn't, this method will need to dynamically create the generic methods to register messages, which might not be available at runtime. " +
//                "Alternatively, OnActivated() can be manually overwritten, and registration can be done individually for each required message for this recipient.")]
//            set
//            {
//                if (SetProperty(ref this.isActive, value, true))
//                {
//                    if (value)
//                    {
//                        OnActivated();
//                    }
//                    else
//                    {
//                        OnDeactivated();
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Invoked whenever the <see cref="IsActive"/> property is set to <see langword="true"/>.
//        /// Use this method to register to messages and do other initialization for this instance.
//        /// </summary>
//        /// <remarks>
//        /// The base implementation registers all messages for this recipients that have been declared
//        /// explicitly through the <see cref="IRecipient{TMessage}"/> interface, using the default channel.
//        /// For more details on how this works, see the <see cref="IMessengerExtensions.RegisterAll"/> method.
//        /// If you need more fine tuned control, want to register messages individually or just prefer
//        /// the lambda-style syntax for message registration, override this method and register manually.
//        /// </remarks>
//        [RequiresUnreferencedCode(
//            "This method requires the generated CommunityToolkit.Mvvm.Messaging.__Internals.__IMessengerExtensions type not to be removed to use the fast path. " +
//            "If this type is removed by the linker, or if the target recipient was created dynamically and was missed by the source generator, a slower fallback " +
//            "path using a compiled LINQ expression will be used. This will have more overhead in the first invocation of this method for any given recipient type. " +
//            "Alternatively, OnActivated() can be manually overwritten, and registration can be done individually for each required message for this recipient.")]
//        [RequiresDynamicCode(
//            "This method requires the generated CommunityToolkit.Mvvm.Messaging.__Internals.__IMessengerExtensions type not to be removed to use the fast path. " +
//            "If that is present, the method is AOT safe, as the only methods being invoked to register the messages will be the ones produced by the source generator. " +
//            "If it isn't, this method will need to dynamically create the generic methods to register messages, which might not be available at runtime. " +
//            "Alternatively, OnActivated() can be manually overwritten, and registration can be done individually for each required message for this recipient.")]
//        protected virtual void OnActivated()
//        {
//            Messenger.RegisterAll(this);
//        }

//        /// <summary>
//        /// Invoked whenever the <see cref="IsActive"/> property is set to <see langword="false"/>.
//        /// Use this method to unregister from messages and do general cleanup for this instance.
//        /// </summary>
//        /// <remarks>
//        /// The base implementation unregisters all messages for this recipient. It does so by
//        /// invoking <see cref="IMessenger.UnregisterAll"/>, which removes all registered
//        /// handlers for a given subscriber, regardless of what token was used to register them.
//        /// That is, all registered handlers across all subscription channels will be removed.
//        /// </remarks>
//        protected virtual void OnDeactivated()
//        {
//            Messenger.UnregisterAll(this);
//        }

//        /// <summary>
//        /// Broadcasts a <see cref="PropertyChangedMessage{T}"/> with the specified
//        /// parameters, without using any particular token (so using the default channel).
//        /// </summary>
//        /// <typeparam name="T">The type of the property that changed.</typeparam>
//        /// <param name="oldValue">The value of the property before it changed.</param>
//        /// <param name="newValue">The value of the property after it changed.</param>
//        /// <param name="propertyName">The name of the property that changed.</param>
//        /// <remarks>
//        /// You should override this method if you wish to customize the channel being
//        /// used to send the message (eg. if you need to use a specific token for the channel).
//        /// </remarks>
//        protected virtual void Broadcast<T>(T oldValue, T newValue, string? propertyName)
//        {
//            PropertyChangedMessage<T> message = new(this, propertyName, oldValue, newValue);

//            _ = Messenger.Send(message);
//        }

//        /// <summary>
//        /// Compares the current and new values for a given property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property with
//        /// the new value, then raises the <see cref="ObservableObject.PropertyChanged"/> event.
//        /// </summary>
//        /// <typeparam name="T">The type of the property that changed.</typeparam>
//        /// <param name="field">The field storing the property's value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <remarks>
//        /// This method is just like <see cref="ObservableObject.SetProperty{T}(ref T,T,string)"/>, just with the addition
//        /// of the <paramref name="broadcast"/> parameter. As such, following the behavior of the base method,
//        /// the <see cref="ObservableObject.PropertyChanging"/> and <see cref="ObservableObject.PropertyChanged"/> events
//        /// are not raised if the current and new value for the target property are the same.
//        /// </remarks>
//        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, bool broadcast, [CallerMemberName] string? propertyName = null)
//        {
//            T oldValue = field;

//            // We duplicate the code as in the base class here to leverage
//            // the intrinsics support for EqualityComparer<T>.Default.Equals.
//            bool propertyChanged = SetProperty(ref field, newValue, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }

//        /// <summary>
//        /// Compares the current and new values for a given property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property with
//        /// the new value, then raises the <see cref="ObservableObject.PropertyChanged"/> event.
//        /// See additional notes about this overload in <see cref="SetProperty{T}(ref T,T,bool,string)"/>.
//        /// </summary>
//        /// <typeparam name="T">The type of the property that changed.</typeparam>
//        /// <param name="field">The field storing the property's value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> instance to use to compare the input values.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="comparer"/> is <see langword="null"/>.</exception>
//        protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, IEqualityComparer<T> comparer, bool broadcast, [CallerMemberName] string? propertyName = null)
//        {
//            ArgumentNullException.ThrowIfNull(comparer);

//            T oldValue = field;

//            bool propertyChanged = SetProperty(ref field, newValue, comparer, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }

//        /// <summary>
//        /// Compares the current and new values for a given property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property with
//        /// the new value, then raises the <see cref="ObservableObject.PropertyChanged"/> event. Similarly to
//        /// the <see cref="ObservableObject.SetProperty{T}(T,T,Action{T},string)"/> method, this overload should only be
//        /// used when <see cref="ObservableObject.SetProperty{T}(ref T,T,string)"/> can't be used directly.
//        /// </summary>
//        /// <typeparam name="T">The type of the property that changed.</typeparam>
//        /// <param name="oldValue">The current property value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="callback">A callback to invoke to update the property value.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <remarks>
//        /// This method is just like <see cref="ObservableObject.SetProperty{T}(T,T,Action{T},string)"/>, just with the addition
//        /// of the <paramref name="broadcast"/> parameter. As such, following the behavior of the base method,
//        /// the <see cref="ObservableObject.PropertyChanging"/> and <see cref="ObservableObject.PropertyChanged"/> events
//        /// are not raised if the current and new value for the target property are the same.
//        /// </remarks>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
//        protected bool SetProperty<T>(T oldValue, T newValue, Action<T> callback, bool broadcast, [CallerMemberName] string? propertyName = null)
//        {
//            ArgumentNullException.ThrowIfNull(callback);

//            bool propertyChanged = SetProperty(oldValue, newValue, callback, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }

//        /// <summary>
//        /// Compares the current and new values for a given property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property with
//        /// the new value, then raises the <see cref="ObservableObject.PropertyChanged"/> event.
//        /// See additional notes about this overload in <see cref="SetProperty{T}(T,T,Action{T},bool,string)"/>.
//        /// </summary>
//        /// <typeparam name="T">The type of the property that changed.</typeparam>
//        /// <param name="oldValue">The current property value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> instance to use to compare the input values.</param>
//        /// <param name="callback">A callback to invoke to update the property value.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="comparer"/> or <paramref name="callback"/> are <see langword="null"/>.</exception>
//        protected bool SetProperty<T>(T oldValue, T newValue, IEqualityComparer<T> comparer, Action<T> callback, bool broadcast, [CallerMemberName] string? propertyName = null)
//        {
//            ArgumentNullException.ThrowIfNull(comparer);
//            ArgumentNullException.ThrowIfNull(callback);

//            bool propertyChanged = SetProperty(oldValue, newValue, comparer, callback, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }

//        /// <summary>
//        /// Compares the current and new values for a given nested property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property and then raises the
//        /// <see cref="ObservableObject.PropertyChanged"/> event. The behavior mirrors that of
//        /// <see cref="ObservableObject.SetProperty{TModel,T}(T,T,TModel,Action{TModel,T},string)"/>, with the difference being that this
//        /// method is used to relay properties from a wrapped model in the current instance. For more info, see the docs for
//        /// <see cref="ObservableObject.SetProperty{TModel,T}(T,T,TModel,Action{TModel,T},string)"/>.
//        /// </summary>
//        /// <typeparam name="TModel">The type of model whose property (or field) to set.</typeparam>
//        /// <typeparam name="T">The type of property (or field) to set.</typeparam>
//        /// <param name="oldValue">The current property value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="model">The model </param>
//        /// <param name="callback">The callback to invoke to set the target property value, if a change has occurred.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="model"/> or <paramref name="callback"/> are <see langword="null"/>.</exception>
//        protected bool SetProperty<TModel, T>(T oldValue, T newValue, TModel model, Action<TModel, T> callback, bool broadcast, [CallerMemberName] string? propertyName = null)
//            where TModel : class
//        {
//            ArgumentNullException.ThrowIfNull(model);
//            ArgumentNullException.ThrowIfNull(callback);

//            bool propertyChanged = SetProperty(oldValue, newValue, model, callback, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }

//        /// <summary>
//        /// Compares the current and new values for a given nested property. If the value has changed,
//        /// raises the <see cref="ObservableObject.PropertyChanging"/> event, updates the property and then raises the
//        /// <see cref="ObservableObject.PropertyChanged"/> event. The behavior mirrors that of
//        /// <see cref="ObservableObject.SetProperty{TModel,T}(T,T,IEqualityComparer{T},TModel,Action{TModel,T},string)"/>,
//        /// with the difference being that this method is used to relay properties from a wrapped model in the
//        /// current instance. For more info, see the docs for
//        /// <see cref="ObservableObject.SetProperty{TModel,T}(T,T,IEqualityComparer{T},TModel,Action{TModel,T},string)"/>.
//        /// </summary>
//        /// <typeparam name="TModel">The type of model whose property (or field) to set.</typeparam>
//        /// <typeparam name="T">The type of property (or field) to set.</typeparam>
//        /// <param name="oldValue">The current property value.</param>
//        /// <param name="newValue">The property's value after the change occurred.</param>
//        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> instance to use to compare the input values.</param>
//        /// <param name="model">The model </param>
//        /// <param name="callback">The callback to invoke to set the target property value, if a change has occurred.</param>
//        /// <param name="broadcast">If <see langword="true"/>, <see cref="Broadcast{T}"/> will also be invoked.</param>
//        /// <param name="propertyName">(optional) The name of the property that changed.</param>
//        /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
//        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="comparer"/>, <paramref name="model"/> or <paramref name="callback"/> are <see langword="null"/>.</exception>
//        protected bool SetProperty<TModel, T>(T oldValue, T newValue, IEqualityComparer<T> comparer, TModel model, Action<TModel, T> callback, bool broadcast, [CallerMemberName] string? propertyName = null)
//            where TModel : class
//        {
//            ArgumentNullException.ThrowIfNull(comparer);
//            ArgumentNullException.ThrowIfNull(model);
//            ArgumentNullException.ThrowIfNull(callback);

//            bool propertyChanged = SetProperty(oldValue, newValue, comparer, model, callback, propertyName);

//            if (propertyChanged && broadcast)
//            {
//                Broadcast(oldValue, newValue, propertyName);
//            }

//            return propertyChanged;
//        }
//    }
//}
