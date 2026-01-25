using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrabajoInterfacesFinal
{
    public partial class MainWindow : Window
    {
        // =========================================================
        // 1. VARIABLES Y LISTAS GLOBALES
        // =========================================================
        #region Variables Globales

        decimal saldo = 100.00m;

        // Listas de datos
        List<DatosJuego> carrito = new List<DatosJuego>();
        List<MetodoPago> misMetodosPago = new List<MetodoPago>();

        // Catálogo maestro para filtros
        List<DatosJuego> catalogoCompleto = new List<DatosJuego>();

        // Variable temporal para detalle
        DatosJuego juegoActualEnDetalle = new DatosJuego();

        // Cambiado a ObservableCollection<DatosJuego>
        private ObservableCollection<DatosJuego> bibliotecaJuegos = new ObservableCollection<DatosJuego>();

        #endregion

        // =========================================================
        // 2. INICIO DE LA APLICACIÓN
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            CargarJuegosEnTienda();
            ActualizarSaldoVisual();
            listaBiblioteca.ItemsSource = bibliotecaJuegos;
        }

        // =========================================================
        // 3. NAVEGACIÓN (VISIBILIDAD DE PANTALLAS)
        // =========================================================
        #region Navegacion

        private void OcultarTodas()
        {
            Grid_Tienda.Visibility = Visibility.Collapsed;
            Grid_Detalle.Visibility = Visibility.Collapsed;
            Grid_Biblioteca.Visibility = Visibility.Collapsed;
            Grid_Perfil.Visibility = Visibility.Collapsed;
            Grid_Carrito.Visibility = Visibility.Collapsed;
            Grid_Login.Visibility = Visibility.Collapsed;
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
            // El binding se encarga de refrescar la vista automáticamente
        }

        private void Nav_Perfil_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Perfil.Visibility = Visibility.Visible;
            lblSaldoPerfil.Text = $"{saldo}€"; // Actualizar visualmente al entrar
            ActualizarCombosMetodos();
        }

        private void Nav_Carrito_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Carrito.Visibility = Visibility.Visible;
            ActualizarVistaCarrito();
        }

        #endregion

        // =========================================================
        // 4. SISTEMA DE LOGIN
        // =========================================================
        #region Login

        private void BtnLogin_Click(object sender, RoutedEventArgs e) //FRANCISCO TRABAJA
        {
            if (txtUserLogin.Text.Length > 0 && txtPassLogin.Password.Length > 0)
            {
                txtUsuarioMenu.Text = txtUserLogin.Text;
                txtEditUser.Text = txtUserLogin.Text;
                ActualizarSaldoVisual();

                OcultarTodas();
                Grid_Aplicacion.Visibility = Visibility.Visible;
                Grid_Tienda.Visibility = Visibility.Visible; // Ir a tienda por defecto
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

        #endregion

        // =========================================================
        // 5. TIENDA, FILTROS Y BUSCADOR
        // =========================================================
        #region Tienda Logic

        private void CargarJuegosEnTienda()
        {
            // Datos iniciales
            catalogoCompleto.Clear();
            catalogoCompleto.Add(new DatosJuego { Titulo = "Cyberpunk 2077", Precio = 59.99m, Genero = "RPG" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "Elden Ring", Precio = 69.99m, Genero = "RPG" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "FIFA 24", Precio = 69.99m, Genero = "Deportes" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "Call of Duty", Precio = 49.99m, Genero = "Acción" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "Zelda: TOTK", Precio = 59.99m, Genero = "Aventura" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "Age of Empires IV", Precio = 29.99m, Genero = "Estrategia" });
            catalogoCompleto.Add(new DatosJuego { Titulo = "God of War", Precio = 39.99m, Genero = "Acción" });

            RenderizarJuegos(catalogoCompleto);
        }

        private void RenderizarJuegos(List<DatosJuego> listaParaMostrar)
        {
            // --- CORRECCIÓN DEL ERROR DE PANELJUEGOS ---
            // Buscamos el panel manualmente para evitar errores de enlace XAML
            WrapPanel panelVisual = this.FindName("PanelJuegos") as WrapPanel;
            if (panelVisual == null) return;

            panelVisual.Children.Clear();

            foreach (var juego in listaParaMostrar)
            {
                // Crear Carta
                Border carta = new Border
                {
                    Width = 180,
                    Height = 250,
                    Background = new SolidColorBrush(Color.FromRgb(22, 32, 45)),
                    Margin = new Thickness(0, 0, 15, 15),
                    CornerRadius = new CornerRadius(5)
                };

                StackPanel panel = new StackPanel();

                // Elementos visuales
                Border imagen = new Border { Height = 120, Background = Brushes.Black, Margin = new Thickness(0, 0, 0, 10), CornerRadius = new CornerRadius(5, 5, 0, 0) };
                TextBlock genero = new TextBlock { Text = juego.Genero.ToUpper(), Foreground = Brushes.Gray, FontSize = 10, Margin = new Thickness(10, 0, 0, 0) };
                TextBlock titulo = new TextBlock { Text = juego.Titulo, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(10, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
                TextBlock precio = new TextBlock { Text = juego.Precio + "€", Foreground = Brushes.LightGreen, Margin = new Thickness(10, 5, 0, 10) };

                Button btnVer = new Button { Content = "Ver", Height = 25, Margin = new Thickness(10, 0, 10, 0), Background = new SolidColorBrush(Color.FromRgb(60, 64, 75)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };

                // ASIGNAR EVENTO Y DATOS
                btnVer.Tag = juego;
                btnVer.Click += BtnVerDetalle_Click;

                panel.Children.Add(imagen);
                panel.Children.Add(genero);
                panel.Children.Add(titulo);
                panel.Children.Add(precio);
                panel.Children.Add(btnVer);
                carta.Child = panel;

                // Añadimos al panel encontrado
                panelVisual.Children.Add(carta);
            }
        }

        private void AplicarFiltros()
        {
            string textoBusqueda = txtBuscador.Text.ToLower();
            string generoSeleccionado = "Todos los géneros";

            if (cmbFiltroGenero.SelectedItem != null)
            {
                ComboBoxItem item = (ComboBoxItem)cmbFiltroGenero.SelectedItem;
                generoSeleccionado = item.Content.ToString();
            }

            List<DatosJuego> resultados = new List<DatosJuego>();

            foreach (var juego in catalogoCompleto)
            {
                bool coincideNombre = string.IsNullOrEmpty(textoBusqueda) || juego.Titulo.ToLower().Contains(textoBusqueda);
                bool coincideGenero = generoSeleccionado == "Todos los géneros" || juego.Genero == generoSeleccionado;

                if (coincideNombre && coincideGenero) resultados.Add(juego);
            }

            RenderizarJuegos(resultados);
        }

        private void TxtBuscador_TextChanged(object sender, TextChangedEventArgs e) { AplicarFiltros(); }
        private void CmbFiltroGenero_SelectionChanged(object sender, SelectionChangedEventArgs e) { AplicarFiltros(); }

        #endregion

        // =========================================================
        // 6. DETALLES DEL PRODUCTO
        // =========================================================
        #region Detalles

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DatosJuego datos = (DatosJuego)btn.Tag;

            juegoActualEnDetalle = datos;

            txtDetalleTitulo.Text = juegoActualEnDetalle.Titulo;
            txtDetallePrecio.Text = juegoActualEnDetalle.Precio + "€";

            // Añade esta línea para que el binding de las estrellas funcione
            panelEstrellasDetalle.DataContext = juegoActualEnDetalle;

            OcultarTodas();
            Grid_Detalle.Visibility = Visibility.Visible;
        }

        #endregion

        // =========================================================
        // 7. CARRITO DE COMPRA
        // =========================================================
        #region Carrito

        private void BtnComprar_Click(object sender, RoutedEventArgs e)
        {
            // Validar Biblioteca
            foreach (var j in bibliotecaJuegos)
            {
                if (j.Titulo == juegoActualEnDetalle.Titulo)
                {
                    MessageBox.Show("¡Ya tienes este juego!");
                    return;
                }
            }
            // Validar Carrito
            foreach (var j in carrito)
            {
                if (j.Titulo == juegoActualEnDetalle.Titulo)
                {
                    MessageBox.Show("Ya está en el carrito.");
                    Nav_Carrito_Click(null, null);
                    return;
                }
            }

            carrito.Add(new DatosJuego
            {
                Titulo = juegoActualEnDetalle.Titulo,
                Precio = juegoActualEnDetalle.Precio,
                Genero = juegoActualEnDetalle.Genero
            });
            MessageBox.Show("Añadido al carrito");
            ActualizarVistaCarrito();
        }

        private void ActualizarVistaCarrito()
        {
            listaCarrito.ItemsSource = null;
            listaCarrito.ItemsSource = carrito;

            decimal total = 0;
            foreach (var item in carrito) total += item.Precio;

            txtTotalCarrito.Text = total + "€";
        }

        private void BtnEliminarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (listaCarrito.SelectedItem != null)
            {
                DatosJuego juegoAEliminar = (DatosJuego)listaCarrito.SelectedItem;
                carrito.Remove(juegoAEliminar);
                ActualizarVistaCarrito();
            }
        }

        private void BtnPagarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0) return;

            decimal total = 0;
            foreach (var item in carrito) total += item.Precio;

            if (saldo >= total)
            {
                saldo -= total;
                ActualizarSaldoVisual();

                // Añadir a la biblioteca solo si no existe ya
                foreach (var item in carrito)
                {
                    if (!bibliotecaJuegos.Any(j => j.Titulo == item.Titulo))
                    {
                        bibliotecaJuegos.Add(new DatosJuego
                        {
                            Titulo = item.Titulo,
                            Genero = item.Genero,
                            Precio = item.Precio
                        });
                    }
                }

                // Generar Factura
                GenerarFacturaTXT(new List<DatosJuego>(carrito), total);

                carrito.Clear();
                ActualizarVistaCarrito();

                MessageBox.Show("¡Compra realizada!");
                Nav_Biblioteca_Click(null, null);
            }
            else
            {
                MessageBox.Show("Saldo insuficiente.");
            }
        }

        #endregion

        // =========================================================
        // 8. PERFIL Y MÉTODOS DE PAGO (LÓGICA ANTIGUA ADAPTADA)
        // =========================================================
        #region Perfil Logic

        private void BtnGuardarPerfil_Click(object sender, RoutedEventArgs e)
        {
            txtUsuarioMenu.Text = txtEditUser.Text;
            MessageBox.Show("Perfil Actualizado");
        }

        private void BtnGuardarMetodo_Click(object sender, RoutedEventArgs e) //FRANCISCO TRABAJA
        {
            if (txtDatoNuevo.Text.Length < 4) { MessageBox.Show("Introduce un ID válido."); return; }

            MetodoPago nuevoMetodo = new MetodoPago
            {
                Tipo = cmbTipoNuevo.Text,
                Nombre = txtDatoNuevo.Text
            };

            misMetodosPago.Add(nuevoMetodo);
            MessageBox.Show("Método añadido.");
            txtDatoNuevo.Text = "";
            ActualizarCombosMetodos();
        }

        private void ActualizarCombosMetodos() 
        {
            // Actualiza el desplegable de arriba
            cmbMetodosGuardados.Items.Clear();
            foreach (var metodo in misMetodosPago) cmbMetodosGuardados.Items.Add(metodo);
            if (cmbMetodosGuardados.Items.Count > 0) cmbMetodosGuardados.SelectedIndex = 0;

            // Actualiza la lista visual de abajo
            listaMetodosVisual.Items.Clear();
            foreach (var metodo in misMetodosPago) listaMetodosVisual.Items.Add($"{metodo.Tipo} - {metodo.Nombre}");
        }

        private void BtnIngresarDinero_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMetodosGuardados.SelectedItem == null) { MessageBox.Show("Añade un método de pago primero."); return; }

            if (decimal.TryParse(txtCantidadRecarga.Text, out decimal cantidad) && cantidad > 0)
            {
                saldo += cantidad;
                ActualizarSaldoVisual();
                lblSaldoPerfil.Text = $"{saldo}€";
                MessageBox.Show($"¡Recarga de {cantidad}€ realizada!");
            }
            else
            {
                MessageBox.Show("Cantidad inválida.");
            }
        }

        private void ActualizarSaldoVisual()
        {
            txtSaldoMenu.Text = $"Cartera: {saldo}€";
        }

        #endregion

        // =========================================================
        // 9. FACTURACIÓN (GENERAR TXT)
        // =========================================================
        #region Factura

        private void GenerarFacturaTXT(List<DatosJuego> juegosComprados, decimal totalPagado)
        {
            try
            {
                string nombreArchivo = $"Factura_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                System.Text.StringBuilder factura = new System.Text.StringBuilder();

                factura.AppendLine("========================================");
                factura.AppendLine("          STEAM CLONE - FACTURA         ");
                factura.AppendLine("========================================");
                factura.AppendLine($"FECHA: {DateTime.Now}");
                factura.AppendLine($"CLIENTE: {txtUsuarioMenu.Text}");
                factura.AppendLine("----------------------------------------");

                foreach (var juego in juegosComprados)
                {
                    factura.AppendLine(String.Format("{0,-30} | {1,7}€",
                        juego.Titulo.Length > 25 ? juego.Titulo.Substring(0, 25) : juego.Titulo,
                        juego.Precio));
                }

                factura.AppendLine("----------------------------------------");
                factura.AppendLine($"TOTAL PAGADO:       {totalPagado:F2}€");
                factura.AppendLine("========================================");

                File.WriteAllText(nombreArchivo, factura.ToString());
                System.Diagnostics.Process.Start("notepad.exe", nombreArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al factura: " + ex.Message);
            }
        }

        #endregion

        // Método para añadir un juego a la biblioteca
        public void AgregarJuegoABiblioteca(DatosJuego juego)
        {
            if (!bibliotecaJuegos.Any(j => j.Titulo == juego.Titulo))
            {
                bibliotecaJuegos.Add(new DatosJuego
                {
                    Titulo = juego.Titulo,
                    Genero = juego.Genero,
                    Precio = juego.Precio
                });
            }
        }

        // Ejemplo: Llama a este método después de una compra exitosa
        private void SimularCompra()
        {
            var juego = new DatosJuego { Titulo = "Elden Ring", Genero = "RPG", Precio = 59.99m };
            AgregarJuegoABiblioteca(juego);
        }

        private void BtnEliminarBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                bibliotecaJuegos.Remove(juego);
            }
        }

        private void BtnJugarBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                MessageBox.Show($"¡Lanzando {juego.Titulo}!\n(Implementa aquí la lógica para abrir el juego)", "JUGAR", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego juego && int.TryParse(btn.Tag.ToString(), out int valor))
            {
                juego.Valoracion = valor;
            }
        }
    }

    // =========================================================
    // 10. CLASES AUXILIARES (MODELOS DE DATOS)
    // =========================================================
    public class DatosJuego : System.ComponentModel.INotifyPropertyChanged
    {
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public string Genero { get; set; }

        private int valoracion = 0;
        public int Valoracion
        {
            get => valoracion;
            set
            {
                if (valoracion != value)
                {
                    valoracion = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Valoracion)));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public class MetodoPago
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public override string ToString() { return $"{Tipo}: {Nombre}"; }
    }
}