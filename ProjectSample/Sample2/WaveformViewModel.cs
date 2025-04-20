using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class WaveformViewModel : IDisposable
    {
        private readonly UcChart _ucChart;
        private readonly UcGrid _ucGrid;
        private IChartRenderer _renderer;
        private readonly EventHandlerManager _handlerManager;
        public ObservableCollection<WaveformStep> EditWaveformSteps { get; set; }
        public int EditPitch { get; set; }
        public string EditName { get; set; }

        public WaveformViewModel(UcChart ucChart, UcGrid ucGrid, EventBus eventBus)
        {
            _ucChart = ucChart;
            _ucGrid = ucGrid;
            _handlerManager = new EventHandlerManager(eventBus);
        }

        public void SetDataSource(WaveformSequence sequence, IChartRenderer renderer)
        {
            _renderer = renderer;
            EditPitch = sequence.Pitch;
            EditName = sequence.Name;
            EditWaveformSteps = new ObservableCollection<WaveformStep>(sequence.WaveformSteps);
            EditWaveformSteps.CollectionChanged += EditWaveformSteps_CollectionChanged;
            foreach(var item in EditWaveformSteps)
            {
                item.PropertyChanged += Item_PropertyChanged;
                item.PropertyChanging += Item_PropertyChanging;
                item.ErrorsChanged += Item_ErrorsChanged;
            }
        }

        private void Item_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
        {

        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(WaveformStep.Frequency) or nameof(WaveformStep.Amplitude) or nameof(WaveformStep.Phase))
            {
                redraw();
            }
        }

        private void Item_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
         
        }

        private void EditWaveformSteps_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        public void redraw()
        {
            _renderer.Render(_ucChart, EditPitch, EditWaveformSteps.ToList());
        }

        public void ApplyChangesTo(WaveformSequence sequence)
        {
            sequence.Pitch = EditPitch;
            sequence.Name = EditName;
            sequence.WaveformSteps = EditWaveformSteps.ToList();
        }

        public void Dispose()
        {
            EditWaveformSteps.CollectionChanged -= EditWaveformSteps_CollectionChanged;
            foreach (var item in EditWaveformSteps)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanging -= Item_PropertyChanging;
                item.ErrorsChanged -= Item_ErrorsChanged;
            }
        }
    }
}
