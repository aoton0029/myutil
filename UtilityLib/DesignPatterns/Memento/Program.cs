using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Memento
{
    private class Memento : IMemento
    {
        public IMementoable Originator { get; }

        public bool IsEditing { get; }

        public Fish? CurrentItem { get; }

        public ImmutableList<Fish> Items { get; }

        public Memento(
            MainViewModel snapshotable,
            bool isEditing,
            Fish? currentItem,
            ImmutableList<Fish> items)
        {
            this.Originator = snapshotable;
            this.IsEditing = isEditing;
            this.CurrentItem = currentItem;
            this.Items = items;
        }
    }

    internal class Program
    {
        private readonly IMementoCaretaker? _caretaker;
        private readonly IFishGenerator _fishGenerator;
        private bool _isEditing;
        private Fish? _currentFish;
        private ImmutableList<Fish> _fishes = ImmutableList<Fish>.Empty;


        public Program()
        {
            
        }
    }
}
