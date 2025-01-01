using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Mvvms
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ObservableRecipientAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ObservablePropertyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ObservableObjectAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class NotifyPropertyChangedRecipientsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class NotifyPropertyChangedForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
        public NotifyPropertyChangedForAttribute(string propertyName)
        {
            PropertyNames = new[] { propertyName };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
        /// <param name="otherPropertyNames">
        /// The other property names to also notify when the annotated property changes. This parameter can optionally
        /// be used to indicate a series of dependent properties from the same attribute, to keep the code more compact.
        /// </param>
        public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames)
        {
            PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
        }

        /// <summary>
        /// Gets the property names to also notify when the annotated property changes.
        /// </summary>
        public string[] PropertyNames { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class NotifyDataErrorInfoAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class NotifyCanExecuteChangedForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCanExecuteChangedForAttribute"/> class.
        /// </summary>
        /// <param name="commandName">The name of the command to also notify when the annotated property changes.</param>
        public NotifyCanExecuteChangedForAttribute(string commandName)
        {
            CommandNames = new[] { commandName };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyCanExecuteChangedForAttribute"/> class.
        /// </summary>
        /// <param name="commandName">The name of the property to also notify when the annotated property changes.</param>
        /// <param name="otherCommandNames">
        /// The other command names to also notify when the annotated property changes. This parameter can optionally
        /// be used to indicate a series of dependent commands from the same attribute, to keep the code more compact.
        /// </param>
        public NotifyCanExecuteChangedForAttribute(string commandName, params string[] otherCommandNames)
        {
            CommandNames = new[] { commandName }.Concat(otherCommandNames).ToArray();
        }

        /// <summary>
        /// Gets the command names to also notify when the annotated property changes.
        /// </summary>
        public string[] CommandNames { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class INotifyPropertyChangedAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to also generate all the additional helper methods that are found
        /// in <see cref="ObservableObject"/> as well (eg. <see cref="ObservableObject.SetProperty{T}(ref T, T, string?)"/>).
        /// If set to <see langword="false"/>, only the <see cref="INotifyPropertyChanged.PropertyChanged"/> event and
        /// the two <see cref="ObservableObject.OnPropertyChanged(PropertyChangedEventArgs)"/> overloads will be generated.
        /// The default value is <see langword="true"/>.
        /// </summary>
        public bool IncludeAdditionalHelperMethods { get; init; } = true;
    }
}
