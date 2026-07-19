using AmpClean.Domain.Entities;
using AmpClean.Presentation.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmpClean.Presentation.ViewModels
{
    internal partial class MeasureViewModel : ObservableObject
    {
        public MeasureViewModel()
        {
            for (int i = 0; i < 10; i++)
            {
                MeasurementData.Add(new MeasurementData() 
                {
                    index = i ,
                    MeasurementItems = new List<float> { 0.1f * i, 0.2f * i, 0.3f * i }
                });
            }
        }

        [ObservableProperty]
        private ObservableCollection<MeasurementData> _measurementData = new();
    }
}
