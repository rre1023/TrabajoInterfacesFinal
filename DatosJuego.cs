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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Valoracion)));
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
}