using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrabajoInterfacesFinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // VARIABLES GLOBALES DE DATOS
        decimal saldo = 100.00m;
        List<string> misJuegos = new List<string>();

        // Variable temporal para saber qué juego estamos viendo en detalle
        string juegoSeleccionadoTitulo = "";
        decimal juegoSeleccionadoPrecio = 0;

        public MainWindow()
        {
            InitializeComponent();
            CargarJuegosEnTienda(); // Generar visualmente los juegos
        }

        // ==========================================
        // LÓGICA DE NAVEGACIÓN (VISIBILIDAD)
        // ==========================================

        // Método ayudante para resetear vistas
        private void OcultarTodas()
        {
            Grid_Tienda.Visibility = Visibility.Collapsed;
            Grid_Detalle.Visibility = Visibility.Collapsed;
            Grid_Biblioteca.Visibility = Visibility.Collapsed;
            Grid_Perfil.Visibility = Visibility.Collapsed;
        }

        private void Nav_Tienda_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Tienda.Visibility = Visibility.Visible;
        }

        private void Nav_Biblioteca_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Biblioteca.Visibility = Visibility.Visible;

            // Refrescar lista visual
            listaBiblioteca.Items.Clear();
            foreach (string juego in misJuegos)
            {
                listaBiblioteca.Items.Add(juego);
            }
        }

        private void Nav_Perfil_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Perfil.Visibility = Visibility.Visible;
        }

        // ==========================================
        // LÓGICA DE LOGIN
        // ==========================================
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserLogin.Text.Length > 0 && txtPassLogin.Password.Length > 0)
            {
                // Login Correcto
                txtUsuarioMenu.Text = txtUserLogin.Text;
                txtEditUser.Text = txtUserLogin.Text; // Poner nombre en perfil
                ActualizarSaldoVisual();

                Grid_Login.Visibility = Visibility.Collapsed;
                Grid_Aplicacion.Visibility = Visibility.Visible;
            }
            else
            {
                lblErrorLogin.Text = "Introduce usuario y contraseña";
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Grid_Aplicacion.Visibility = Visibility.Collapsed;
            Grid_Login.Visibility = Visibility.Visible;
            txtPassLogin.Password = "";
        }

        // ==========================================
        // LÓGICA DE TIENDA Y DETALLES
        // ==========================================

        private void CargarJuegosEnTienda()
        {
            // Creamos 6 juegos falsos para rellenar la tienda
            for (int i = 1; i <= 6; i++)
            {
                // Crear el panel de la carta del juego
                Border carta = new Border
                {
                    Width = 180,
                    Height = 250,
                    Background = new SolidColorBrush(Color.FromRgb(22, 32, 45)), // #16202d
                    Margin = new Thickness(0, 0, 15, 15),
                    CornerRadius = new CornerRadius(3)
                };

                // Contenido de la carta
                StackPanel panel = new StackPanel();

                // 1. Imagen simulada (bloque negro)
                Border imagen = new Border { Height = 120, Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 10) };

                // 2. Título
                TextBlock titulo = new TextBlock { Text = "Juego " + i, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(10, 0, 0, 0) };

                // 3. Precio
                TextBlock precio = new TextBlock { Text = (10 * i) + ",00€", Foreground = Brushes.Gray, Margin = new Thickness(10, 5, 0, 10) };

                // 4. Botón VER
                Button btnVer = new Button { Content = "Ver Detalles", Height = 25, Margin = new Thickness(10, 0, 10, 0), Background = new SolidColorBrush(Color.FromRgb(76, 107, 34)) };

                // GUARDAMOS DATOS EN EL BOTÓN PARA USARLOS AL HACER CLIC
                btnVer.Tag = new { Nombre = "Juego " + i, Precio = (decimal)(10 * i) };
                btnVer.Click += BtnVerDetalle_Click; // Asignamos evento

                panel.Children.Add(imagen);
                panel.Children.Add(titulo);
                panel.Children.Add(precio);
                panel.Children.Add(btnVer);
                carta.Child = panel;

                // Añadir al WrapPanel del XAML
                PanelJuegos.Children.Add(carta);
            }
        }

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Recuperamos los datos del botón que se pulsó
            Button btn = sender as Button;
            dynamic datos = btn.Tag;

            // Rellenamos la pantalla de detalle
            juegoSeleccionadoTitulo = datos.Nombre;
            juegoSeleccionadoPrecio = datos.Precio;

            txtDetalleTitulo.Text = juegoSeleccionadoTitulo;
            txtDetallePrecio.Text = juegoSeleccionadoPrecio + "€";

            // Cambiamos de pantalla
            OcultarTodas();
            Grid_Detalle.Visibility = Visibility.Visible;
        }

        // ==========================================
        // LÓGICA DE COMPRA
        // ==========================================
        private void BtnComprar_Click(object sender, RoutedEventArgs e)
        {
            if (misJuegos.Contains(juegoSeleccionadoTitulo))
            {
                MessageBox.Show("¡Ya tienes este juego!");
                return;
            }

            if (saldo >= juegoSeleccionadoPrecio)
            {
                saldo -= juegoSeleccionadoPrecio;
                ActualizarSaldoVisual();

                misJuegos.Add(juegoSeleccionadoTitulo);

                // Generar Factura TXT (Requisito 12)
                System.IO.File.WriteAllText("Factura_Ultima_Compra.txt",
                    $"FACTURA STEAM CLONE\nJuego: {juegoSeleccionadoTitulo}\nPrecio: {juegoSeleccionadoPrecio}€\nFecha: {DateTime.Now}");

                MessageBox.Show($"¡Compra realizada!\nTe quedan {saldo}€");

                // Ir a biblioteca automáticamente
                Nav_Biblioteca_Click(null, null);
            }
            else
            {
                MessageBox.Show("Saldo insuficiente :(");
            }
        }

        private void BtnGuardarPerfil_Click(object sender, RoutedEventArgs e)
        {
            txtUsuarioMenu.Text = txtEditUser.Text;
            MessageBox.Show("Perfil Actualizado");
        }

        private void ActualizarSaldoVisual()
        {
            txtSaldoMenu.Text = $"Cartera: {saldo}€";
        }
    }
}