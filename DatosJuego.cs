using System.ComponentModel;

public class DatosJuego : INotifyPropertyChanged
{
    public string Titulo { get; set; }
    public decimal Precio { get; set; }
    public string Genero { get; set; }

    private int valoracion;
    public int Valoracion
    {
        get => valoracion;
        set
        {
            if (valoracion != value)
            {
                valoracion = value;
                OnPropertyChanged(nameof(Valoracion));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
