using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataConcentrator
{
    // Bazna klasa za sve tagove. EF mapiranje: Table-Per-Hierarchy (jedna tabela Tags).
    // Naslednici: AnalogInput, AnalogOutput, DigitalInput, DigitalOutput.
    public abstract class Tag : INotifyPropertyChanged
    {
        private string name;
        private string description;
        private string ioAddress;
        private double currentValue;

        #region Properties

        [Key]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        // Adresa u PLC-u (ADDR001..ADDR016).
        public string IOAddress
        {
            get { return ioAddress; }
            set
            {
                ioAddress = value;
                OnPropertyChanged("IOAddress");
            }
        }

        // Trenutna (runtime) vrednost; ne perzistira se u bazu.
        [NotMapped]
        public double CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }

        // Tip taga (AI/AO/DI/DO) - implementira svaki naslednik.
        [NotMapped]
        public abstract TagType Type { get; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
