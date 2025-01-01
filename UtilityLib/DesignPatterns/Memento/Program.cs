using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Memento
{
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
