﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DIs
{

    /// <summary>
    /// Specifies the contract for a collection of service descriptors.
    /// </summary>
    public interface IServiceCollection : IList<ServiceDescriptor>
    {
    }

    /// <summary>
    /// Default implementation of <see cref="IServiceCollection"/>.
    /// </summary>
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

        /// <inheritdoc />
        public int Count => _descriptors.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public ServiceDescriptor this[int index]
        {
            get
            {
                return _descriptors[index];
            }
            set
            {
                _descriptors[index] = value;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _descriptors.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item)
        {
            return _descriptors.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _descriptors.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item)
        {
            return _descriptors.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            _descriptors.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int IndexOf(ServiceDescriptor item)
        {
            return _descriptors.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, ServiceDescriptor item)
        {
            _descriptors.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _descriptors.RemoveAt(index);
        }
    }
}
