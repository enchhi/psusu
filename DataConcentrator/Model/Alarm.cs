using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataConcentrator
{
    // Alarm vezan za analogni ulaz (preko TagName = AnalogInput.Name).
    public class Alarm : INotifyPropertyChanged
    {
        private AlarmState state;

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public double LimitValue { get; set; }          // vrednost granice
        public AlarmDirection Direction { get; set; }    // Above ili Below
        public string Message { get; set; }

        // Naziv AI velicine nad kojom je alarm (strani kljuc).
        public string TagName { get; set; }

        // Stanje se menja u runtime-u -> INotifyPropertyChanged za zivu signalizaciju bojom.
        public AlarmState State
        {
            get { return state; }
            set { state = value; OnPropertyChanged("State"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
