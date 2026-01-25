using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TrabajoInterfacesFinal    
{
    public partial class MainWindow : Window
    {
        // =========================================================
        // 1. VARIABLES Y LISTAS GLOBALES
        // ========================================================= 
        #region Variables Globales

        // Listas de datos
        List<DatosJuego> carrito = new List<DatosJuego>();
        List<MetodoPago> misMetodosPago = new List<MetodoPago>();

        // Catálogo maestro para filtros
        List<DatosJuego> catalogoCompleto = new List<DatosJuego>();

        // Variable temporal para detalle
        DatosJuego juegoActualEnDetalle = new DatosJuego();
        private ObservableCollection<DatosJuego> bibliotecaJuegos = new ObservableCollection<DatosJuego>();

        // Usuario actual logueado
        Usuario usuarioActual;

        #endregion

        // =========================================================
        // 2. INICIO DE LA APLICACIÓN
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            
            using (var db = new AppDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                
                if (!db.Usuarios.Any())
                {
                    db.Usuarios.Add(new Usuario 
                    { 
                        Nombre = "Admin",
                        Email = "admin@steam.com",
                        Contraseña = "1234",
                        Saldo = 0m
                    });
                    db.SaveChanges();
                }

                if (!db.Juegos.Any())
                {
                    db.Juegos.AddRange(
                        new Juego
                        {
                            Titulo = "Cyberpunk 2077",
                            Precio = 59.99m,
                            Genero = "RPG",
                            Imagen = "Fotos/cyberpunk.jpg",
                            Descripcion = "Género: RPG / Acción Un juego de rol en mundo abierto ambientado en Night City, una megalópolis obsesionada con el poder, el glamur y la modificación corporal. Juegas como V, un mercenario proscrito que busca un implante único que es la clave de la inmortalidad. Destaca por su estética futurista, neones y una narrativa profunda donde tus decisiones afectan la historia."
                        },
                        new Juego
                        {
                            Titulo = "Elden Ring",
                            Precio = 69.99m,
                            Genero = "RPG",
                            Imagen = "Fotos/eldenring.jpg",
                            Descripcion = "Género: RPG / Fantasía Oscura Desarrollado por los creadores de Dark Souls con la colaboración de George R.R. Martin (Juego de Tronos). Es una aventura desafiante en un mundo abierto inmenso y desolado llamado las Tierras Intermedias. Es famoso por su alta dificultad, combates épicos contra jefes monstruosos y una total libertad de exploración sin guías."
                        },
                        new Juego
                        {
                            Titulo = "FIFA 24",
                            Precio = 69.99m,
                            Genero = "Deportes",
                            Imagen = "Fotos/FIFA24.jpg",
                            Descripcion = "Género: Deportes El simulador de fútbol más popular del mundo (ahora rebautizado como EA Sports FC). Ofrece la experiencia más realista de partidos con licencias oficiales de ligas, equipos y jugadores reales. Sus modos estrella son el \"Modo Carrera\" (gestionar un equipo) y \"Ultimate Team\" (crear tu equipo de ensueño con cartas de jugadores)."
                        },
                        new Juego
                        {
                            Titulo = "Call of Duty",
                            Precio = 49.99m,
                            Genero = "Acción",
                            Imagen = "Fotos/Callofduty.jpg",
                            Descripcion = "Género: Acción / Shooter (FPS) El rey de los disparos en primera persona. Es un juego de guerra frenético y cinematográfico. Aunque tiene campañas de historia (operaciones militares, espionaje), es mundialmente conocido por su multijugador online competitivo y rápido, donde los reflejos lo son todo."
                        },
                        new Juego
                        {
                            Titulo = "Zelda: TOTK",
                            Precio = 59.99m,
                            Genero = "Aventura",
                            Imagen = "Fotos/Zelda.webp",
                            Descripcion = "Género: Aventura La joya de Nintendo. Es la secuela de Breath of the Wild. Controlas a Link en el reino de Hyrule, pero esta vez puedes explorar islas flotantes en el cielo y profundidades subterráneas. Su gran novedad es la capacidad de construir vehículos y armas fusionando objetos del entorno con magia, fomentando la creatividad total del jugador."
                        },
                        new Juego
                        {
                            Titulo = "Age of Empires IV",
                            Precio = 29.99m,
                            Genero = "Estrategia",
                            Imagen = "Fotos/Ageofempire.jpg",
                            Descripcion = "Género: Estrategia (RTS) Un clásico de estrategia en tiempo real actualizado. Aquí eres el comandante: debes elegir una civilización histórica (ingleses, chinos, mongoles, etc.), recolectar recursos (madera, oro, comida), construir edificios, crear ejércitos y dirigir batallas tácticas para conquistar a tus enemigos. Es como jugar al ajedrez, pero con ejércitos vivos."
                        },
                        new Juego
                        {
                            Titulo = "God of War",
                            Precio = 39.99m,
                            Genero = "Acción",
                            Imagen = "Fotos/Godofwar.jpg",
                            Descripcion = "Género: Acción / Aventura Basado en la mitología nórdica. Sigues la historia de Kratos, un antiguo dios de la guerra griego que ahora vive en el retiro en tierras escandinavas con su hijo, Atreus. Es un viaje emotivo de padre e hijo con un combate brutal, visceral y unos gráficos impresionantes. Mezcla peleas intensas con puzles y exploración."
                        }
                    );
                    db.SaveChanges();
                }
            }
            
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
            Grid_Registro.Visibility = Visibility.Collapsed;
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
            if (usuarioActual != null)
            {
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios.Include(u => u.MetodosPago).FirstOrDefault(u => u.Id == usuarioActual.Id);
                    if (usuario != null)
                    {
                        usuarioActual.Saldo = usuario.Saldo;
                        misMetodosPago = usuario.MetodosPago.ToList();
                    }
                }
                lblSaldoPerfil.Text = $"{usuarioActual.Saldo}€";
            }
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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserLogin.Text.Length > 0 && txtPassLogin.Password.Length > 0)
            {
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios
                        .Include(u => u.MetodosPago)
                        .FirstOrDefault(u => 
                            u.Nombre == txtUserLogin.Text && 
                            u.Contraseña == txtPassLogin.Password);

                    if (usuario != null)
                    {
                        usuarioActual = usuario;
                        misMetodosPago = usuario.MetodosPago.ToList();
                        txtUsuarioMenu.Text = usuario.Nombre;
                        txtEditUser.Text = usuario.Nombre;
                        ActualizarSaldoVisual();
                        ActualizarCombosMetodos();

                        OcultarTodas();
                        Grid_Aplicacion.Visibility = Visibility.Visible;
                        Grid_Tienda.Visibility = Visibility.Visible;
                        
                        lblErrorLogin.Text = "";
                        txtUserLogin.Text = "";
                        txtPassLogin.Password = "";
                    }
                    else
                    {
                        lblErrorLogin.Text = "Usuario o contraseña incorrectos";
                    }
                }
            }
            else
            {
                lblErrorLogin.Text = "Introduce usuario y contraseña";
            }
        }

        private void BtnMostrarRegistro_Click(object sender, RoutedEventArgs e)
        {
            Grid_Login.Visibility = Visibility.Collapsed;
            Grid_Registro.Visibility = Visibility.Visible;
            lblErrorRegistro.Text = "";
        }

        private void BtnMostrarLogin_Click(object sender, RoutedEventArgs e)
        {
            Grid_Registro.Visibility = Visibility.Collapsed;
            Grid_Login.Visibility = Visibility.Visible;
            lblErrorLogin.Text = "";
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombreRegistro.Text.Trim();
            string email = txtEmailRegistro.Text.Trim();
            string pass = txtPassRegistro.Password;
            string passConfirmar = txtPassConfirmarRegistro.Password;

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(passConfirmar))
            {
                lblErrorRegistro.Text = "Todos los campos son obligatorios";
                return;
            }

            if (nombre.Length < 3)
            {
                lblErrorRegistro.Text = "El nombre debe tener al menos 3 caracteres";
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                lblErrorRegistro.Text = "Introduce un email válido";
                return;
            }

            if (pass.Length < 4)
            {
                lblErrorRegistro.Text = "La contraseña debe tener al menos 4 caracteres";
                return;
            }

            if (pass != passConfirmar)
            {
                lblErrorRegistro.Text = "Las contraseñas no coinciden";
                return;
            }

            using (var db = new AppDbContext())
            {
                var usuarioExistente = db.Usuarios.FirstOrDefault(u => u.Nombre == nombre);
                if (usuarioExistente != null)
                {
                    lblErrorRegistro.Text = "El nombre de usuario ya existe";
                    return;
                }

                var emailExistente = db.Usuarios.FirstOrDefault(u => u.Email == email);
                if (emailExistente != null)
                {
                    lblErrorRegistro.Text = "El email ya está registrado";
                    return;
                }

                Usuario nuevoUsuario = new Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Contraseña = pass
                };

                db.Usuarios.Add(nuevoUsuario);
                db.SaveChanges();

                MessageBox.Show("¡Cuenta creada exitosamente!", "Registro Completado", MessageBoxButton.OK, MessageBoxImage.Information);

                txtNombreRegistro.Text = "";
                txtEmailRegistro.Text = "";
                txtPassRegistro.Password = "";
                txtPassConfirmarRegistro.Password = "";

                Grid_Registro.Visibility = Visibility.Collapsed;
                Grid_Login.Visibility = Visibility.Visible;
                lblErrorLogin.Text = "";
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Grid_Aplicacion.Visibility = Visibility.Collapsed;
            Grid_Login.Visibility = Visibility.Visible;
            txtPassLogin.Password = "";
            txtUserLogin.Text = "";
            usuarioActual = null;
            misMetodosPago.Clear();
        }

        #endregion

        // =========================================================
        // 5. TIENDA, FILTROS Y BUSCADOR
        // =========================================================
        #region Tienda Logic

        private void CargarJuegosEnTienda()
        {
            catalogoCompleto.Clear();
            
            using (var db = new AppDbContext())
            {
                var juegos = db.Juegos.ToList();
                foreach (var juego in juegos)
                {
                    catalogoCompleto.Add(new DatosJuego 
                    { 
                        Titulo = juego.Titulo, 
                        Precio = juego.Precio, 
                        Genero = juego.Genero 
                    });
                }
            }

            RenderizarJuegos(catalogoCompleto);
        }

        private void RenderizarJuegos(List<DatosJuego> listaParaMostrar)
        {
            WrapPanel panelVisual = this.FindName("PanelJuegos") as WrapPanel;
            if (panelVisual == null) return;

            panelVisual.Children.Clear();

            using (var db = new AppDbContext())
            {
                foreach (var juego in listaParaMostrar)
                {
                    var juegoCompleto = db.Juegos.FirstOrDefault(j => j.Titulo == juego.Titulo);
                    if (juegoCompleto == null) continue;

                    Border carta = new Border
                    {
                        Width = 180,
                        Height = 250,
                        Background = new SolidColorBrush(Color.FromRgb(22, 32, 45)),
                        Margin = new Thickness(0, 0, 15, 15),
                        CornerRadius = new CornerRadius(5)
                    };

                    StackPanel panel = new StackPanel();

                    Border imagenBorder = new Border 
                    { 
                        Height = 120, 
                        Margin = new Thickness(0, 0, 0, 10), 
                        CornerRadius = new CornerRadius(5, 5, 0, 0),
                        Background = Brushes.Black
                    };

                    if (!string.IsNullOrEmpty(juegoCompleto.Imagen))
                    {
                        try
                        {
                            string rutaCompleta = Path.GetFullPath(juegoCompleto.Imagen);
                            
                            if (File.Exists(rutaCompleta))
                            {
                                var imagen = new Image
                                {
                                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute)),
                                    Stretch = Stretch.UniformToFill
                                };
                                imagenBorder.Child = imagen;
                            }
                            else
                            {
                                imagenBorder.Background = Brushes.DarkRed;
                            }
                        }
                        catch (Exception ex)
                        {
                            imagenBorder.Background = Brushes.DarkOrange;
                            System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex.Message}");
                        }
                    }

                    TextBlock genero = new TextBlock { Text = juego.Genero.ToUpper(), Foreground = Brushes.Gray, FontSize = 10, Margin = new Thickness(10, 0, 0, 0) };
                    TextBlock titulo = new TextBlock { Text = juego.Titulo, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Margin = new Thickness(10, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
                    TextBlock precio = new TextBlock { Text = juego.Precio + "€", Foreground = Brushes.LightGreen, Margin = new Thickness(10, 5, 0, 10) };

                    Button btnVer = new Button { Content = "Ver", Height = 25, Margin = new Thickness(10, 0, 10, 0), Background = new SolidColorBrush(Color.FromRgb(60, 64, 75)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };

                    btnVer.Tag = juego;
                    btnVer.Click += BtnVerDetalle_Click;

                    panel.Children.Add(imagenBorder);
                    panel.Children.Add(genero);
                    panel.Children.Add(titulo);
                    panel.Children.Add(precio);
                    panel.Children.Add(btnVer);
                    carta.Child = panel;

                    panelVisual.Children.Add(carta);
                }
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

            using (var db = new AppDbContext())
            {
                var juegoCompleto = db.Juegos.FirstOrDefault(j => j.Titulo == datos.Titulo);
                if (juegoCompleto != null)
                {
                    juegoActualEnDetalle = datos;
                    txtDetalleTitulo.Text = juegoCompleto.Titulo;
                    txtDetallePrecio.Text = juegoCompleto.Precio + "€";
                    txtDetalleDescripcion.Text = juegoCompleto.Descripcion ?? "No hay descripción disponible.";
                    
                    // Cargar imagen
                    if (!string.IsNullOrEmpty(juegoCompleto.Imagen))
                    {
                        try
                        {
                            string rutaCompleta = Path.GetFullPath(juegoCompleto.Imagen);
                            
                            if (File.Exists(rutaCompleta))
                            {
                                imgDetalleJuego.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute));
                            }
                            else
                            {
                                imgDetalleJuego.Source = null;
                                borderDetalleImagen.Background = Brushes.DarkRed;
                            }
                        }
                        catch (Exception ex)
                        {
                            imgDetalleJuego.Source = null;
                            borderDetalleImagen.Background = Brushes.DarkOrange;
                            System.Diagnostics.Debug.WriteLine($"Error cargando imagen en detalle: {ex.Message}");
                        }
                    }
                    else
                    {
                        imgDetalleJuego.Source = null;
                        borderDetalleImagen.Background = Brushes.Black;
                    }
                    
                    OcultarTodas();
                    Grid_Detalle.Visibility = Visibility.Visible;
                }
            }
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

            if (usuarioActual.Saldo >= total)
            {
                usuarioActual.Saldo -= total;
                
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios.Find(usuarioActual.Id);
                    if (usuario != null)
                    {
                        usuario.Saldo = usuarioActual.Saldo;
                        db.SaveChanges();
                    }
                }
                
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

        private void BtnGuardarMetodo_Click(object sender, RoutedEventArgs e)
        {
            if (txtDatoNuevo.Text.Length < 4) 
            { 
                MessageBox.Show("Introduce un ID válido."); 
                return; 
            }

            using (var db = new AppDbContext())
            {
                MetodoPago nuevoMetodo = new MetodoPago
                {
                    UsuarioId = usuarioActual.Id,
                    Tipo = cmbTipoNuevo.Text,
                    Nombre = txtDatoNuevo.Text
                };

                db.MetodosPago.Add(nuevoMetodo);
                db.SaveChanges();

                misMetodosPago.Add(nuevoMetodo);
                usuarioActual.MetodosPago.Add(nuevoMetodo);
            }

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
                usuarioActual.Saldo += cantidad;
                
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios.Find(usuarioActual.Id);
                    if (usuario != null)
                    {
                        usuario.Saldo = usuarioActual.Saldo;
                        db.SaveChanges();
                    }
                }
                
                ActualizarSaldoVisual();
                lblSaldoPerfil.Text = $"{usuarioActual.Saldo}€";
                MessageBox.Show($"¡Recarga de {cantidad}€ realizada!");
            }
            else
            {
                MessageBox.Show("Cantidad inválida.");
            }
        }

        private void ActualizarSaldoVisual()
        {
            if (usuarioActual != null)
            {
                txtSaldoMenu.Text = $"Cartera: {usuarioActual.Saldo}€";
            }
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
                MessageBox.Show("Error al generar factura: " + ex.Message);
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
    }

    // =========================================================
    // 10. CLASES AUXILIARES (MODELOS DE DATOS)
    // =========================================================
    public class DatosJuego
    {
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public string Genero { get; set; }
    }

    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public string Genero { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
    }

    public class MetodoPago
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public override string ToString() { return $"{Tipo}: {Nombre}"; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public decimal Saldo { get; set; } = 0m;
        public List<MetodoPago> MetodosPago { get; set; } = new List<MetodoPago>();
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Juego> Juegos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=usuarios.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Nombre)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<MetodoPago>()
                .HasOne<Usuario>()
                .WithMany(u => u.MetodosPago)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}