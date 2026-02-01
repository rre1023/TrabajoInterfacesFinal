using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using Microsoft.EntityFrameworkCore;

// LIBRERÍAS PARA ITEXT 9 (La versión que tienes instalada)
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace TrabajoInterfacesFinal    
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                    string rutaCompleta = System.IO.Path.Combine(directorioBase, imagePath);
                    
                    if (File.Exists(rutaCompleta))
                    {
                        return new BitmapImage(new Uri(rutaCompleta, UriKind.Absolute));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex.Message}");
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        // =========================================================
        // 1. VARIABLES Y LISTAS GLOBALES
        // ========================================================= 
        #region Variables Globales

        // Listas de datos
        List<DatosJuego> carrito = new List<DatosJuego>();
        List<MetodoPago> misMetodosPago = new List<MetodoPago>();
        List<DatosJuego> catalogoCompleto = new List<DatosJuego>();
        DatosJuego juegoActualEnDetalle = new DatosJuego();
        private ObservableCollection<DatosJuego> bibliotecaJuegos = new ObservableCollection<DatosJuego>();
        private ObservableCollection<DatosJuego> listaDeseosJuegos = new ObservableCollection<DatosJuego>();
        private bool filtroFavoritosActivo = false;


        Usuario usuarioActual;
        #endregion

        // =========================================================
        // 2. INICIO
        // =========================================================
        public MainWindow() 
        {
            InitializeComponent();
            
            using (var db = new AppDbContext())
            {
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
                            Descripcion = "Género: Aventura La joya de Nintendo. Es la secuela de Breath of the Wild. Controlas a Link in el reino de Hyrule, pero esta vez puedes explorar islas flotantes en el cielo y profundidades subterráneas. Su gran novedad es la capacidad de construir vehículos y armas fusionando objetos del entorno con magia, fomentando la creatividad total del jugador."
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

            // Vinculamos la lista visual con nuestros datos
            listaBiblioteca.ItemsSource = bibliotecaJuegos;
            // listaDeseos.ItemsSource = listaDeseosJuegos; // Se inicializa al navegar a la lista de deseos
        }

        // =========================================================
        // 3. NAVEGACIÓN
        // =========================================================
        #region Navegacion

        private void OcultarTodas()
        {
            Grid_Tienda.Visibility = Visibility.Collapsed;
            Grid_Detalle.Visibility = Visibility.Collapsed;
            Grid_Biblioteca.Visibility = Visibility.Collapsed;
            if (this.FindName("Grid_ListaDeseos") is Grid gridDeseos)
            {
                gridDeseos.Visibility = Visibility.Collapsed;
            }
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
            
            if (usuarioActual != null)
            {
                using (var db = new AppDbContext())
                {
                    foreach (var juego in bibliotecaJuegos)
                    {
                        var valoracionUsuario = db.Valoraciones
                            .FirstOrDefault(v => v.JuegoId == juego.JuegoId && v.UsuarioId == usuarioActual.Id);
                        
                        if (valoracionUsuario != null)
                        {
                            juego.Valoracion = valoracionUsuario.Puntuacion;
                        }
                        
                        var esFavorito = db.JuegosFavoritos.Any(f => f.UsuarioId == usuarioActual.Id && f.JuegoId == juego.JuegoId);
                        juego.EsFavorito = esFavorito;
                    }
                }
            }
            
            AplicarFiltroBiblioteca();
        }

        private void Nav_Perfil_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Perfil.Visibility = Visibility.Visible;
            if (usuarioActual != null)
            {
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios
                        .Include(u => u.MetodosPago)
                        .Include(u => u.BibliotecaJuegos)
                        .FirstOrDefault(u => u.Id == usuarioActual.Id);
                    
                    if (usuario != null)
                    {
                        usuarioActual.Saldo = usuario.Saldo;
                        misMetodosPago = usuario.MetodosPago.ToList();
                        
                        lblSaldoPerfil.Text = $"{usuarioActual.Saldo:0.00}€";
                        txtCantidadJuegos.Text = usuario.BibliotecaJuegos.Count.ToString();
                        
                        var totalValoraciones = db.Valoraciones.Count(v => v.UsuarioId == usuarioActual.Id);
                        txtCantidadValoraciones.Text = totalValoraciones.ToString();
                    }
                }
            }
            ActualizarCombosMetodos();
        }

        private void Nav_Carrito_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            Grid_Carrito.Visibility = Visibility.Visible;
            ActualizarVistaCarrito();
        }

        private void BtnFiltroFavoritos_Click(object sender, RoutedEventArgs e)
        {
            filtroFavoritosActivo = !filtroFavoritosActivo;
            
            if (sender is Button btn)
            {
                btn.Tag = filtroFavoritosActivo ? "True" : "False";
            }
            
            AplicarFiltroBiblioteca();
        }

        private void Nav_ListaDeseos_Click(object sender, RoutedEventArgs e)
        {
            OcultarTodas();
            // Grid_ListaDeseos.Visibility = Visibility.Visible; // Comentado temporalmente
            if (this.FindName("Grid_ListaDeseos") is Grid gridDeseos)
            {
                gridDeseos.Visibility = Visibility.Visible;
            }
            CargarListaDeseos();
        }

        private void CargarListaDeseos()
        {
            if (usuarioActual == null) return;

            listaDeseosJuegos.Clear();
            
            using (var db = new AppDbContext())
            {
                var deseos = db.ListasDeseos
                    .Where(l => l.UsuarioId == usuarioActual.Id)
                    .Include(l => l.Juego)
                        .ThenInclude(j => j.Valoraciones)
                    .ToList();

                foreach (var deseo in deseos)
                {
                    listaDeseosJuegos.Add(new DatosJuego
                    {
                        Titulo = deseo.Juego.Titulo,
                        Genero = deseo.Juego.Genero,
                        Precio = deseo.Juego.Precio,
                        JuegoId = deseo.Juego.Id,
                        Imagen = deseo.Juego.Imagen,
                        ValoracionPromedio = deseo.Juego.ValoracionPromedio,
                        TotalValoraciones = deseo.Juego.TotalValoraciones
                    });
                }
            }
            
            if (this.FindName("listaDeseos") is ListView lista)
            {
                lista.ItemsSource = listaDeseosJuegos;
            }
        }

        private void AplicarFiltroBiblioteca()
        {
            if (filtroFavoritosActivo)
            {
                var favoritosTemp = bibliotecaJuegos.Where(j => j.EsFavorito).ToList();
                listaBiblioteca.ItemsSource = new ObservableCollection<DatosJuego>(favoritosTemp);
            }
            else
            {
                listaBiblioteca.ItemsSource = bibliotecaJuegos;
            }
        }

        #endregion

        // =========================================================
        // 4. LOGIN
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
                        .Include(u => u.BibliotecaJuegos)
                            .ThenInclude(b => b.Juego)
                                .ThenInclude(j => j.Valoraciones)
                        .FirstOrDefault(u => 
                            u.Nombre == txtUserLogin.Text && 
                            u.Contraseña == txtPassLogin.Password);

                    if (usuario != null)
                    {
                        usuarioActual = usuario;
                        misMetodosPago = usuario.MetodosPago.ToList();
                        
                        bibliotecaJuegos.Clear();
                        foreach (var bibItem in usuario.BibliotecaJuegos)
                        {
                            var valoracionUsuario = bibItem.Juego.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuario.Id);
                            var esFavorito = db.JuegosFavoritos.Any(f => f.UsuarioId == usuario.Id && f.JuegoId == bibItem.Juego.Id);
                            
                            bibliotecaJuegos.Add(new DatosJuego
                            {
                                Titulo = bibItem.Juego.Titulo,
                                Genero = bibItem.Juego.Genero,
                                Precio = bibItem.Juego.Precio,
                                JuegoId = bibItem.Juego.Id,
                                Imagen = bibItem.Juego.Imagen,
                                ValoracionPromedio = bibItem.Juego.ValoracionPromedio,
                                TotalValoraciones = bibItem.Juego.TotalValoraciones,
                                Valoracion = valoracionUsuario?.Puntuacion ?? 0,
                                VecesJugadas = bibItem.Juego.VecesJugadas,
                                EsFavorito = esFavorito
                            });
                        }
                        
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
            var dialog = new Window
            {
                Title = "Cerrar Sesión",
                Width = 450,
                Height = 340,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(27, 40, 56)),
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true
            };

            var mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 47, 62)),
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(102, 192, 244)),
                BorderThickness = new Thickness(2)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(30) };

            var iconoTexto = new TextBlock
            {
                Text = "⚠️",
                FontSize = 48,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 20)
            };

            var txtTitulo = new TextBlock
            {
                Text = "¿Cerrar Sesión?",
                Foreground = Brushes.White,
                FontSize = 24,
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            var txtMensaje = new TextBlock
            {
                Text = "¿Estás seguro de que deseas cerrar sesión?\nTus datos estarán guardados de forma segura.",
                Foreground = new SolidColorBrush(Color.FromRgb(163, 179, 193)),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            var panelBotones = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var cancelarStyle = new System.Windows.Style(typeof(Button));
            cancelarStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(61, 68, 80))));
            cancelarStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            
            var cancelarTemplate = new ControlTemplate(typeof(Button));
            var cancelarFactory = new FrameworkElementFactory(typeof(Border));
            cancelarFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            cancelarFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            cancelarFactory.SetValue(Border.PaddingProperty, new Thickness(10));
            var cancelarContent = new FrameworkElementFactory(typeof(ContentPresenter));
            cancelarContent.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            cancelarContent.SetValue(ContentPresenter.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            cancelarFactory.AppendChild(cancelarContent);
            cancelarTemplate.VisualTree = cancelarFactory;
            cancelarStyle.Setters.Add(new Setter(Button.TemplateProperty, cancelarTemplate));
            
            var cancelarTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            cancelarTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(74, 85, 96))));
            cancelarStyle.Triggers.Add(cancelarTrigger);

            var btnCancelar = new Button
            {
                Content = "CANCELAR",
                Width = 140,
                Height = 45,
                Margin = new Thickness(0, 0, 15, 0),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Style = cancelarStyle
            };

            var confirmarStyle = new System.Windows.Style(typeof(Button));
            confirmarStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(201, 79, 79))));
            confirmarStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            
            var confirmarTemplate = new ControlTemplate(typeof(Button));
            var confirmarFactory = new FrameworkElementFactory(typeof(Border));
            confirmarFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            confirmarFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            confirmarFactory.SetValue(Border.PaddingProperty, new Thickness(10));
            var confirmarContent = new FrameworkElementFactory(typeof(ContentPresenter));
            confirmarContent.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            confirmarContent.SetValue(ContentPresenter.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            confirmarFactory.AppendChild(confirmarContent);
            confirmarTemplate.VisualTree = confirmarFactory;
            confirmarStyle.Setters.Add(new Setter(Button.TemplateProperty, confirmarTemplate));
            
            var confirmarTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            confirmarTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(214, 57, 57))));
            confirmarStyle.Triggers.Add(confirmarTrigger);

            var btnConfirmar = new Button
            {
                Content = "SÍ, CERRAR SESIÓN",
                Width = 180,
                Height = 45,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Style = confirmarStyle
            };

            btnCancelar.Click += (s, ev) => dialog.Close();

            btnConfirmar.Click += (s, ev) =>
            {
                dialog.Close();
                Grid_Aplicacion.Visibility = Visibility.Collapsed;
                Grid_Login.Visibility = Visibility.Visible;
                txtPassLogin.Password = "";
                txtUserLogin.Text = "";
                usuarioActual = null;
                misMetodosPago.Clear();
                bibliotecaJuegos.Clear();
                listaDeseosJuegos.Clear();
                carrito.Clear();
            };

            panelBotones.Children.Add(btnCancelar);
            panelBotones.Children.Add(btnConfirmar);

            stackPanel.Children.Add(iconoTexto);
            stackPanel.Children.Add(txtTitulo);
            stackPanel.Children.Add(txtMensaje);
            stackPanel.Children.Add(panelBotones);

            mainBorder.Child = stackPanel;
            dialog.Content = mainBorder;
            
            dialog.ShowDialog();
        }

        #endregion

        // =========================================================
        // 5. TIENDA LOGIC
        // =========================================================
        #region Tienda Logic

        private void CargarJuegosEnTienda()
        {
            catalogoCompleto.Clear();
            
            using (var db = new AppDbContext())
            {
                var juegos = db.Juegos.Include(j => j.Valoraciones).ToList();
                foreach (var juego in juegos)
                {
                    catalogoCompleto.Add(new DatosJuego 
                    { 
                        Titulo = juego.Titulo, 
                        Precio = juego.Precio, 
                        Genero = juego.Genero,
                        ValoracionPromedio = juego.ValoracionPromedio
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
                    var juegoCompleto = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Titulo == juego.Titulo);
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
                            string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                            string rutaCompleta = System.IO.Path.Combine(directorioBase, juegoCompleto.Imagen);
                            
                            if (File.Exists(rutaCompleta))
                            {
                                var imagen = new System.Windows.Controls.Image
                                {
                                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute)),
                                    Stretch = Stretch.UniformToFill
                                };
                                imagenBorder.Child = imagen;
                            }
                            else
                            {
                                imagenBorder.Background = Brushes.DarkRed;
                                System.Diagnostics.Debug.WriteLine($"Imagen no encontrada: {rutaCompleta}");
                            }
                        }
                        catch (Exception ex)
                        {
                            imagenBorder.Background = Brushes.DarkOrange;
                            System.Diagnostics.Debug.WriteLine($"Error cargando imagen: {ex.Message}");
                        }
                    }

                    TextBlock genero = new TextBlock { Text = juego.Genero.ToUpper(), Foreground = Brushes.Gray, FontSize = 10, Margin = new Thickness(10, 0, 0, 0) };
                    TextBlock titulo = new TextBlock { Text = juego.Titulo, Foreground = Brushes.White, FontWeight = System.Windows.FontWeights.Bold, Margin = new Thickness(10, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
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

            if (filtroFavoritosActivo)
            {
                resultados = resultados.Where(j => j.EsFavorito).ToList();
            }

            RenderizarJuegos(resultados);
        }

        private void TxtBuscador_TextChanged(object sender, TextChangedEventArgs e) { AplicarFiltros(); }
        private void CmbFiltroGenero_SelectionChanged(object sender, SelectionChangedEventArgs e) { AplicarFiltros(); }

        #endregion

        // =========================================================
        // 6. DETALLES
        // =========================================================
        #region Detalles

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DatosJuego datos = (DatosJuego)btn.Tag;

            using (var db = new AppDbContext())
            {
                var juegoCompleto = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Titulo == datos.Titulo);
                if (juegoCompleto != null)
                {
                    juegoActualEnDetalle = new DatosJuego 
                    { 
                        Titulo = datos.Titulo,
                        Precio = datos.Precio,
                        Genero = datos.Genero,
                        JuegoId = juegoCompleto.Id,
                        ValoracionPromedio = juegoCompleto.ValoracionPromedio,
                        TotalValoraciones = juegoCompleto.TotalValoraciones
                    };
                    
                    if (usuarioActual != null)
                    {
                        var valoracionUsuario = juegoCompleto.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuarioActual.Id);
                        if (valoracionUsuario != null)
                        {
                            juegoActualEnDetalle.Valoracion = valoracionUsuario.Puntuacion;
                        }
                    }
                    
                    panelEstrellasDetalle.DataContext = juegoActualEnDetalle;
                    
                    txtDetalleTitulo.Text = juegoCompleto.Titulo;
                    txtDetallePrecio.Text = juegoCompleto.Precio + "€";
                    txtDetalleDescripcion.Text = juegoCompleto.Descripcion ?? "No hay descripción disponible.";

                    // Cargar imagen
                    if (!string.IsNullOrEmpty(juegoCompleto.Imagen))
                    {
                        try
                        {
                            string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                            string rutaCompleta = System.IO.Path.Combine(directorioBase, juegoCompleto.Imagen);
                            
                            if (File.Exists(rutaCompleta))
                            {
                                imgDetalleJuego.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute));
                                borderDetalleImagen.Background = Brushes.Black;
                            }
                            else
                            {
                                imgDetalleJuego.Source = null;
                                borderDetalleImagen.Background = Brushes.DarkRed;
                            }
                        }
                        catch
                        {
                            imgDetalleJuego.Source = null;
                            borderDetalleImagen.Background = Brushes.DarkOrange;
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

        private void BtnAñadirListaDeseos_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("Debes iniciar sesión para añadir a lista de deseos.", "Sesión requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new AppDbContext())
            {
                var yaEnBiblioteca = db.BibliotecaUsuarios
                    .Any(b => b.UsuarioId == usuarioActual.Id && b.JuegoId == juegoActualEnDetalle.JuegoId);

                if (yaEnBiblioteca)
                {
                    MessageBox.Show("Ya tienes este juego en tu biblioteca.", "Ya adquirido", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var yaEnDeseos = db.ListasDeseos
                    .Any(l => l.UsuarioId == usuarioActual.Id && l.JuegoId == juegoActualEnDetalle.JuegoId);

                if (yaEnDeseos)
                {
                    MessageBox.Show("Este juego ya está en tu lista de deseos.", "Ya añadido", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var nuevoDeseo = new ListaDeseos
                {
                    UsuarioId = usuarioActual.Id,
                    JuegoId = juegoActualEnDetalle.JuegoId,
                    FechaAgregado = DateTime.Now
                };

                db.ListasDeseos.Add(nuevoDeseo);
                db.SaveChanges();

                MessageBox.Show($"{juegoActualEnDetalle.Titulo} añadido a tu lista de deseos!", "Añadido", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnVerDesdeDeseos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego datos)
            {
                using (var db = new AppDbContext())
                {
                    var juegoCompleto = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Id == datos.JuegoId);
                    if (juegoCompleto != null)
                    {
                        juegoActualEnDetalle = new DatosJuego 
                        { 
                            Titulo = datos.Titulo,
                            Precio = datos.Precio,
                            Genero = datos.Genero,
                            JuegoId = juegoCompleto.Id,
                            ValoracionPromedio = juegoCompleto.ValoracionPromedio,
                            TotalValoraciones = juegoCompleto.TotalValoraciones
                        };
                        
                        if (usuarioActual != null)
                        {
                            var valoracionUsuario = juegoCompleto.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuarioActual.Id);
                            if (valoracionUsuario != null)
                            {
                                juegoActualEnDetalle.Valoracion = valoracionUsuario.Puntuacion;
                            }
                        }
                        
                        panelEstrellasDetalle.DataContext = juegoActualEnDetalle;
                        
                        txtDetalleTitulo.Text = juegoCompleto.Titulo;
                        txtDetallePrecio.Text = juegoCompleto.Precio + "€";
                        txtDetalleDescripcion.Text = juegoCompleto.Descripcion ?? "No hay descripción disponible.";

                        if (!string.IsNullOrEmpty(juegoCompleto.Imagen))
                        {
                            try
                            {
                                string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                                string rutaCompleta = System.IO.Path.Combine(directorioBase, juegoCompleto.Imagen);
                                
                                if (File.Exists(rutaCompleta))
                                {
                                    imgDetalleJuego.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute));
                                    borderDetalleImagen.Background = Brushes.Black;
                                }
                                else
                                {
                                    imgDetalleJuego.Source = null;
                                    borderDetalleImagen.Background = Brushes.DarkRed;
                                }
                            }
                            catch
                            {
                                imgDetalleJuego.Source = null;
                                borderDetalleImagen.Background = Brushes.DarkOrange;
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
        }

        private void BtnEliminarDeseos_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null) return;

            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                using (var db = new AppDbContext())
                {
                    var deseo = db.ListasDeseos
                        .FirstOrDefault(l => l.UsuarioId == usuarioActual.Id && l.JuegoId == juego.JuegoId);

                    if (deseo != null)
                    {
                        db.ListasDeseos.Remove(deseo);
                        db.SaveChanges();

                        listaDeseosJuegos.Remove(juego);
                        MessageBox.Show($"{juego.Titulo} eliminado de tu lista de deseos", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void BtnVerDesdeBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego datos)
            {
                using (var db = new AppDbContext())
                {
                    var juegoCompleto = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Id == datos.JuegoId);
                    if (juegoCompleto != null)
                    {
                        juegoActualEnDetalle = new DatosJuego 
                        { 
                            Titulo = datos.Titulo,
                            Precio = datos.Precio,
                            Genero = datos.Genero,
                            JuegoId = juegoCompleto.Id,
                            ValoracionPromedio = juegoCompleto.ValoracionPromedio,
                            TotalValoraciones = juegoCompleto.TotalValoraciones
                        };
                        
                        if (usuarioActual != null)
                        {
                            var valoracionUsuario = juegoCompleto.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuarioActual.Id);
                            if (valoracionUsuario != null)
                            {
                                juegoActualEnDetalle.Valoracion = valoracionUsuario.Puntuacion;
                            }
                        }
                        
                        panelEstrellasDetalle.DataContext = juegoActualEnDetalle;
                        
                        txtDetalleTitulo.Text = juegoCompleto.Titulo;
                        txtDetallePrecio.Text = juegoCompleto.Precio + "€";
                        txtDetalleDescripcion.Text = juegoCompleto.Descripcion ?? "No hay descripción disponible.";

                        if (!string.IsNullOrEmpty(juegoCompleto.Imagen))
                        {
                            try
                            {
                                string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                                string rutaCompleta = System.IO.Path.Combine(directorioBase, juegoCompleto.Imagen);
                                
                                if (File.Exists(rutaCompleta))
                                {
                                    imgDetalleJuego.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(rutaCompleta, UriKind.Absolute));
                                    borderDetalleImagen.Background = Brushes.Black;
                                }
                                else
                                {
                                    imgDetalleJuego.Source = null;
                                    borderDetalleImagen.Background = Brushes.DarkRed;
                                }
                            }
                            catch
                            {
                                imgDetalleJuego.Source = null;
                                borderDetalleImagen.Background = Brushes.DarkOrange;
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
        }

        private void BtnEliminarBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null) return;

            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                var resultado = MessageBox.Show(
                    $"¿Estás seguro de que deseas eliminar '{juego.Titulo}' de tu biblioteca?\n\nEsta acción no se puede deshacer y perderás acceso al juego.",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (resultado == MessageBoxResult.Yes)
                {
                    using (var db = new AppDbContext())
                    {
                        var bibliotecaItem = db.BibliotecaUsuarios
                            .FirstOrDefault(b => b.UsuarioId == usuarioActual.Id && b.JuegoId == juego.JuegoId);

                        if (bibliotecaItem != null)
                        {
                            db.BibliotecaUsuarios.Remove(bibliotecaItem);
                            db.SaveChanges();

                            bibliotecaJuegos.Remove(juego);
                            MessageBox.Show($"{juego.Titulo} eliminado de tu biblioteca", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        #endregion

        // =========================================================
        // 7. CARRITO
        // =========================================================
        #region Carrito

        private void BtnComprar_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("Debes iniciar sesión para comprar juegos.", "Sesión requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new AppDbContext())
            {
                var tienJuego = db.BibliotecaUsuarios
                    .Any(b => b.UsuarioId == usuarioActual.Id && 
                         b.Juego.Titulo == juegoActualEnDetalle.Titulo);
                
                if (tienJuego)
                {
                    MessageBox.Show("¡Ya tienes este juego!");
                    return;
                }
            }
            
            foreach (var j in carrito)
            {
                if (j.Titulo == juegoActualEnDetalle.Titulo) { MessageBox.Show("Ya está en el carrito."); Nav_Carrito_Click(null, null); return; }
            }

            using (var db = new AppDbContext())
            {
                var juegoCompleto = db.Juegos.FirstOrDefault(j => j.Titulo == juegoActualEnDetalle.Titulo);
                if (juegoCompleto != null)
                {
                    carrito.Add(new DatosJuego
                    {
                        Titulo = juegoActualEnDetalle.Titulo,
                        Precio = juegoActualEnDetalle.Precio,
                        Genero = juegoActualEnDetalle.Genero,
                        Imagen = juegoCompleto.Imagen
                    });
                }
            }
            
            MessageBox.Show("Añadido al carrito");
            ActualizarVistaCarrito();
        }

        private void ActualizarVistaCarrito()
        {
            listaCarrito.ItemsSource = null;
            listaCarrito.ItemsSource = carrito;

            decimal total = 0;
            foreach (var item in carrito) total += item.Precio;

            txtTotalCarrito.Text = total.ToString("0.00") + "€";
            txtSubtotal.Text = total.ToString("0.00") + "€";
            txtCantidadItems.Text = carrito.Count.ToString();
            
            if (usuarioActual != null)
            {
                txtSaldoCarrito.Text = usuarioActual.Saldo.ToString("0.00") + "€";
            }
            
            // Mostrar mensaje de carrito vacío si no hay items
            if (this.FindName("mensajeCarritoVacio") is StackPanel panelVacio)
            {
                panelVacio.Visibility = carrito.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnEliminarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DatosJuego juegoAEliminar)
            {
                carrito.Remove(juegoAEliminar);
                ActualizarVistaCarrito();
                MessageBox.Show($"{juegoAEliminar.Titulo} eliminado del carrito", "Artículo eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPagarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0) return;

            decimal total = 0;
            foreach (var item in carrito) total += item.Precio;

            if (usuarioActual.Saldo >= total)
            {
                using (var db = new AppDbContext())
                {
                    var usuario = db.Usuarios
                        .Include(u => u.BibliotecaJuegos)
                        .FirstOrDefault(u => u.Id == usuarioActual.Id);
                    
                    if (usuario != null)
                    {
                        usuario.Saldo -= total;
                        usuarioActual.Saldo = usuario.Saldo;
                        
                        foreach (var item in carrito)
                        {
                            var juegoCompleto = db.Juegos
                                .Include(j => j.Valoraciones)
                                .FirstOrDefault(j => j.Titulo == item.Titulo);
                        
                            if (juegoCompleto != null)
                            {
                                if (!usuario.BibliotecaJuegos.Any(b => b.JuegoId == juegoCompleto.Id))
                                {
                                    var nuevaBiblioteca = new BibliotecaUsuario
                                    {
                                        UsuarioId = usuario.Id,
                                        JuegoId = juegoCompleto.Id,
                                        FechaAdquisicion = DateTime.Now
                                    };
                                    
                                    db.BibliotecaUsuarios.Add(nuevaBiblioteca);
                                    
                                    if (!bibliotecaJuegos.Any(j => j.Titulo == item.Titulo))
                                    {
                                        bibliotecaJuegos.Add(new DatosJuego
                                        {
                                            Titulo = item.Titulo,
                                            Genero = item.Genero,
                                            Precio = item.Precio,
                                            JuegoId = juegoCompleto.Id,
                                            Imagen = juegoCompleto.Imagen,
                                            ValoracionPromedio = juegoCompleto.ValoracionPromedio,
                                            TotalValoraciones = juegoCompleto.TotalValoraciones,
                                            VecesJugadas = juegoCompleto.VecesJugadas,
                                            EsFavorito = false
                                        });
                                    }
                                    
                                    var deseo = db.ListasDeseos.FirstOrDefault(l => l.UsuarioId == usuario.Id && l.JuegoId == juegoCompleto.Id);
                                    if (deseo != null)
                                    {
                                        db.ListasDeseos.Remove(deseo);
                                        var juegoEnDeseos = listaDeseosJuegos.FirstOrDefault(j => j.JuegoId == juegoCompleto.Id);
                                        if (juegoEnDeseos != null)
                                        {
                                            listaDeseosJuegos.Remove(juegoEnDeseos);
                                        }
                                    }
                                }
                            }
                        }
                        
                        db.SaveChanges();
                    }
                }
                
                ActualizarSaldoVisual();

                GenerarFacturaPDF(new List<DatosJuego>(carrito), total);

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
        // 8. PERFIL
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
            cmbMetodosGuardados.Items.Clear();
            foreach (var metodo in misMetodosPago) cmbMetodosGuardados.Items.Add(metodo);
            if (cmbMetodosGuardados.Items.Count > 0) cmbMetodosGuardados.SelectedIndex = 0;

            listaMetodosVisual.ItemsSource = null;
            listaMetodosVisual.ItemsSource = misMetodosPago.Select(m => $"{m.Tipo} - {m.Nombre}").ToList();
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
        // 9. FACTURACIÓN PDF (PARA iTEXT 9.5.0)
        // =========================================================
        #region Factura PDF

        private void GenerarFacturaPDF(List<DatosJuego> juegosComprados, decimal totalPagado)
        {
            try
            {
                // Guardar en el Escritorio para evitar problemas de permisos
                string rutaEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string nombreArchivo = $"Factura_Enova_{Guid.NewGuid().ToString().Substring(0, 5)}.pdf";
                string rutaCompleta = System.IO.Path.Combine(rutaEscritorio, nombreArchivo);

                // CREAR FUENTES (iText 9)
                PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont fontItalic = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                using (PdfWriter writer = new PdfWriter(rutaCompleta))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        using (Document document = new Document(pdf))
                        {
                            // CABECERA
                            // Usamos iText.Layout.Element.Paragraph para no chocar con WPF
                            document.Add(new iText.Layout.Element.Paragraph("ENEVO - FACTURA")
                                .SetFont(fontBold)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                                .SetFontSize(20));

                            document.Add(new iText.Layout.Element.Paragraph($"Fecha: {DateTime.Now}")
                                .SetFont(fontNormal)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(10));

                            document.Add(new iText.Layout.Element.Paragraph($"Cliente: {txtUsuarioMenu.Text}")
                                .SetFont(fontNormal)
                                .SetFontSize(12));

                            document.Add(new iText.Layout.Element.Paragraph("\n"));

                            // TABLA
                            iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(new float[] { 3, 1 }));
                            table.SetWidth(UnitValue.CreatePercentValue(100));

                            // Encabezados
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRODUCTO").SetFont(fontBold)));
                            table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("PRECIO").SetFont(fontBold)));

                            // Filas
                            foreach (var juego in juegosComprados)
                            {
                                table.AddCell(new iText.Layout.Element.Paragraph(juego.Titulo).SetFont(fontNormal));
                                table.AddCell(new iText.Layout.Element.Paragraph($"{juego.Precio}€").SetFont(fontNormal));
                            }

                            document.Add(table);

                            // TOTALES
                            document.Add(new iText.Layout.Element.Paragraph("\n"));
                            decimal baseImponible = totalPagado / 1.21m;
                            decimal iva = totalPagado - baseImponible;

                            document.Add(new iText.Layout.Element.Paragraph($"Base Imponible: {baseImponible:F2}€")
                                .SetFont(fontNormal)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                            document.Add(new iText.Layout.Element.Paragraph($"IVA (21%): {iva:F2}€")
                                .SetFont(fontNormal)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                            document.Add(new iText.Layout.Element.Paragraph($"TOTAL PAGADO: {totalPagado:F2}€")
                                .SetFont(fontBold)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                                .SetFontSize(14));

                            document.Add(new iText.Layout.Element.Paragraph("\n\nGracias por su compra digital.")
                                .SetFont(fontItalic)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        }
                    }
                }

                // ABRIR EL PDF AUTOMÁTICAMENTE
                var p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(rutaCompleta) { UseShellExecute = true };
                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar PDF: " + ex.Message);
            }
        }

        #endregion

        // =========================================================
        // 10. VENTANAS DE AYUDA
        // =========================================================
        #region Ayuda

        private void BtnAyudaTienda_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentanaAyuda(
                "Guía de la Tienda",
                new List<(string icono, string titulo, string descripcion)>
                {
                    ("🔍", "Buscar Juegos", "Utiliza la barra de búsqueda para encontrar juegos por nombre. El sistema filtra automáticamente mientras escribes."),
                    ("🎮", "Filtrar por Género", "Usa el selector de géneros para filtrar los juegos por categorías: Acción, RPG, Aventura, Deportes y Estrategia."),
                    ("👁️", "Ver Detalles", "Haz clic en el botón 'Ver' de cualquier juego para acceder a información detallada, incluyendo descripción completa e imágenes."),
                    ("⭐", "Valoraciones", "Cada juego muestra su valoración promedio basada en las opiniones de otros usuarios. Esto te ayuda a tomar decisiones informadas."),
                    ("🛒", "Añadir al Carrito", "En la página de detalles, puedes añadir el juego al carrito para comprarlo más tarde. También puedes añadirlo directamente a tu lista de deseos."),
                    ("💡", "Consejo Útil", "Los juegos que ya están en tu biblioteca o en el carrito no se pueden volver a añadir para evitar compras duplicadas.")
                }
            );
        }

        private void BtnAyudaBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentanaAyuda(
                "Guía de la Biblioteca",
                new List<(string icono, string titulo, string descripcion)>
                {
                    ("📚", "Tu Colección", "Aquí se muestran todos los juegos que has comprado. Cada juego incluye información sobre el precio pagado, género y valoraciones."),
                    ("🎯", "Jugar", "Haz clic en el botón 'JUGAR' para iniciar cualquier juego de tu biblioteca. El sistema registrará cuántas veces has jugado cada título."),
                    ("⭐", "Marcar Favoritos", "Usa el botón de estrella para marcar tus juegos favoritos. Esto te permite organizarlos mejor y acceder rápidamente a ellos."),
                    ("🔍", "Filtrar Favoritos", "Utiliza el botón 'FAVORITOS' en la parte superior para mostrar únicamente los juegos que has marcado como favoritos."),
                    ("📊", "Valorar Juegos", "Haz clic en 'VALORAR' para dar tu opinión sobre un juego (1-5 estrellas). Tu valoración ayuda a otros usuarios y contribuye a la valoración global del juego."),
                    ("📈", "Estadísticas", "El sistema muestra cuántas veces has jugado cada juego y tu valoración personal. Esta información te ayuda a seguir tu actividad gaming.")
                }
            );
        }

        private void BtnAyudaListaDeseos_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentanaAyuda(
                "Guía de Lista de Deseos",
                new List<(string icono, string titulo, string descripcion)>
                {
                    ("💙", "¿Qué es la Lista de Deseos?", "Es una lista personalizada donde guardas juegos que te interesan pero aún no quieres comprar. Ideal para hacer seguimiento de ofertas futuras."),
                    ("➕", "Añadir Juegos", "Desde la página de detalles de cualquier juego en la tienda, haz clic en 'AÑADIR A LISTA DE DESEOS'. El juego se guardará automáticamente aquí."),
                    ("👁️", "Ver Detalles", "Haz clic en 'VER' para acceder a la información completa del juego, incluyendo descripción, valoraciones y precio actual."),
                    ("🗑️", "Eliminar de la Lista", "Si ya no te interesa un juego, usa el botón 'ELIMINAR' para quitarlo de tu lista de deseos. Esta acción es reversible: puedes añadirlo nuevamente cuando quieras."),
                    ("🛒", "Comprar Después", "Cuando decidas comprar un juego de tu lista de deseos, simplemente ve a sus detalles y añádelo al carrito. Se eliminará automáticamente de la lista al completar la compra."),
                    ("📋", "Organización", "La lista de deseos te ayuda a organizar tus compras futuras y no olvidar los juegos que te interesan. ¡Mantenla actualizada!")
                }
            );
        }

        private void MostrarVentanaAyuda(string titulo, List<(string icono, string titulo, string descripcion)> contenidos)
        {
            var dialog = new Window
            {
                Title = titulo,
                Width = 700,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(27, 40, 56)),
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true
            };

            var mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(35, 47, 62)),
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(102, 192, 244)),
                BorderThickness = new Thickness(2)
            };

            var mainStack = new StackPanel { Margin = new Thickness(0) };

            // Encabezado
            var headerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 71, 94)),
                Padding = new Thickness(30, 20, 30, 20),
                CornerRadius = new CornerRadius(10, 10, 0, 0)
            };

            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };

            var iconoTitulo = new TextBlock
            {
                Text = "📖",
                FontSize = 32,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            var txtTitulo = new TextBlock
            {
                Text = titulo,
                Foreground = Brushes.White,
                FontSize = 26,
                FontWeight = System.Windows.FontWeights.Bold,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            headerStack.Children.Add(iconoTitulo);
            headerStack.Children.Add(txtTitulo);
            headerBorder.Child = headerStack;

            // Contenido scrollable
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 420,
                Margin = new Thickness(30, 20, 30, 20)
            };

            var contentStack = new StackPanel();

            foreach (var (icono, tituloItem, descripcion) in contenidos)
            {
                var itemBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(27, 40, 56)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(20),
                    Margin = new Thickness(0, 0, 0, 15),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(61, 68, 80)),
                    BorderThickness = new Thickness(1)
                };

                var itemStack = new StackPanel();

                var tituloStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                var iconoItem = new TextBlock
                {
                    Text = icono,
                    FontSize = 22,
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };

                var txtTituloItem = new TextBlock
                {
                    Text = tituloItem,
                    Foreground = new SolidColorBrush(Color.FromRgb(102, 192, 244)),
                    FontSize = 16,
                    FontWeight = System.Windows.FontWeights.Bold,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };

                tituloStack.Children.Add(iconoItem);
                tituloStack.Children.Add(txtTituloItem);

                var txtDescripcion = new TextBlock
                {
                    Text = descripcion,
                    Foreground = new SolidColorBrush(Color.FromRgb(163, 179, 193)),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                };

                itemStack.Children.Add(tituloStack);
                itemStack.Children.Add(txtDescripcion);
                itemBorder.Child = itemStack;

                contentStack.Children.Add(itemBorder);
            }

            scrollViewer.Content = contentStack;

            // Pie con botón cerrar
            var footerBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(27, 40, 56)),
                Padding = new Thickness(30, 15, 30, 20)
            };

            var btnCerrar = new Button
            {
                Content = "ENTENDIDO",
                Width = 200,
                Height = 45,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = System.Windows.FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var cerrarStyle = new System.Windows.Style(typeof(Button));
            cerrarStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(92, 126, 22))));
            cerrarStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));

            var cerrarTemplate = new ControlTemplate(typeof(Button));
            var cerrarFactory = new FrameworkElementFactory(typeof(Border));
            cerrarFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            cerrarFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            cerrarFactory.SetValue(Border.PaddingProperty, new Thickness(10));
            var cerrarContent = new FrameworkElementFactory(typeof(ContentPresenter));
            cerrarContent.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            cerrarContent.SetValue(ContentPresenter.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            cerrarFactory.AppendChild(cerrarContent);
            cerrarTemplate.VisualTree = cerrarFactory;
            cerrarStyle.Setters.Add(new Setter(Button.TemplateProperty, cerrarTemplate));

            var cerrarTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            cerrarTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(118, 161, 19))));
            cerrarStyle.Triggers.Add(cerrarTrigger);

            btnCerrar.Style = cerrarStyle;
            btnCerrar.Click += (s, ev) => dialog.Close();

            footerBorder.Child = btnCerrar;

            mainStack.Children.Add(headerBorder);
            mainStack.Children.Add(scrollViewer);
            mainStack.Children.Add(footerBorder);

            mainBorder.Child = mainStack;
            dialog.Content = mainBorder;

            dialog.ShowDialog();
        }

        #endregion

        // =========================================================
        // 11. EVENTO JUGAR Y VALORACIONES
        // =========================================================
        #region Eventos Jugar y Valoraciones
        
        private void BtnJugarBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                using (var db = new AppDbContext())
                {
                    var juegoDb = db.Juegos.FirstOrDefault(j => j.Id == juego.JuegoId);
                    if (juegoDb != null)
                    {
                        juegoDb.VecesJugadas++;
                        db.SaveChanges();
                        
                        juego.VecesJugadas = juegoDb.VecesJugadas;
                    }
                }
                
                MessageBox.Show($"¡Lanzando {juego.Titulo}...", "JUGANDO", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnFavoritoBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null) return;

            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                using (var db = new AppDbContext())
                {
                    var favoritoExistente = db.JuegosFavoritos
                        .FirstOrDefault(f => f.UsuarioId == usuarioActual.Id && f.JuegoId == juego.JuegoId);

                    if (favoritoExistente != null)
                    {
                        db.JuegosFavoritos.Remove(favoritoExistente);
                        juego.EsFavorito = false;
                        MessageBox.Show($"{juego.Titulo} eliminado de favoritos", "Favorito eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var nuevoFavorito = new JuegoFavorito
                        {
                            UsuarioId = usuarioActual.Id,
                            JuegoId = juego.JuegoId,
                            FechaAgregado = DateTime.Now
                        };
                        db.JuegosFavoritos.Add(nuevoFavorito);
                        juego.EsFavorito = true;
                        MessageBox.Show($"{juego.Titulo} añadido a favoritos", "Favorito añadido", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    db.SaveChanges();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnValorarBiblioteca_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("Debes iniciar sesión para valorar juegos.", "Sesión requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is Button btn && btn.DataContext is DatosJuego juego)
            {
                var dialog = new Window
                {
                    Title = $"Valorar {juego.Titulo}",
                    Width = 400,
                    Height = 280,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(Color.FromRgb(27, 40, 56)),
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(30) };

                var txtTitulo = new TextBlock
                {
                    Text = "Selecciona tu valoración:",
                    Foreground = Brushes.White,
                    FontSize = 18,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 20),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                var estrellas = new StackPanel { Orientation = Orientation.Horizontal };
                estrellas.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                int valoracionSeleccionada = juego.Valoracion;

                for (int i = 1; i <= 5; i++)
                {
                    var btnEstrella = new Button
                    {
                        Content = "★",
                        FontSize = 36,
                        Background = Brushes.Transparent,
                        Foreground = i <= valoracionSeleccionada ? Brushes.Gold : Brushes.Gray,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = i,
                        Margin = new Thickness(5)
                    };

                    btnEstrella.Click += (s, ev) =>
                    {
                        int valor = (int)((Button)s).Tag;
                        valoracionSeleccionada = valor;

                        foreach (Button b in estrellas.Children)
                        {
                            int estrella = (int)b.Tag;
                            b.Foreground = estrella <= valor ? Brushes.Gold : Brushes.Gray;
                        }
                    };

                    estrellas.Children.Add(btnEstrella);
                }

                var btnGuardar = new Button
                {
                    Content = "GUARDAR VALORACIÓN",
                    Height = 50,
                    Margin = new Thickness(0, 30, 0, 0),
                    Background = new SolidColorBrush(Color.FromRgb(92, 126, 16)),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btnGuardar.FontWeight = System.Windows.FontWeights.Bold;

                btnGuardar.Click += (s, ev) =>
                {
                    using (var db = new AppDbContext())
                    {
                        var juegoDb = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Id == juego.JuegoId);
                        if (juegoDb == null) return;

                        var valoracionExistente = juegoDb.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuarioActual.Id);

                        if (valoracionExistente != null)
                        {
                            valoracionExistente.Puntuacion = valoracionSeleccionada;
                            valoracionExistente.FechaValoracion = DateTime.Now;
                        }
                        else
                        {
                            var nuevaValoracion = new Valoracion
                            {
                                JuegoId = juegoDb.Id,
                                UsuarioId = usuarioActual.Id,
                                Puntuacion = valoracionSeleccionada,
                                FechaValoracion = DateTime.Now
                            };
                            db.Valoraciones.Add(nuevaValoracion);
                        }

                        db.SaveChanges();

                        juego.Valoracion = valoracionSeleccionada;
                        juego.ValoracionPromedio = juegoDb.ValoracionPromedio;
                        juego.TotalValoraciones = juegoDb.TotalValoraciones;

                        var juegoEnBiblioteca = bibliotecaJuegos.FirstOrDefault(j => j.JuegoId == juego.JuegoId);
                        if (juegoEnBiblioteca != null)
                        {
                            juegoEnBiblioteca.ValoracionPromedio = juego.ValoracionPromedio;
                            juegoEnBiblioteca.TotalValoraciones = juego.TotalValoraciones;
                        }

                        var juegoEnCatalogo = catalogoCompleto.FirstOrDefault(j => j.Titulo == juego.Titulo);
                        if (juegoEnCatalogo != null)
                        {
                            juegoEnCatalogo.ValoracionPromedio = juego.ValoracionPromedio;
                            juegoEnCatalogo.TotalValoraciones = juego.TotalValoraciones;
                        }

                        MessageBox.Show($"¡Valoración de {valoracionSeleccionada} estrellas guardada!", "Valoración Registrada", MessageBoxButton.OK, MessageBoxImage.Information);
                        dialog.Close();
                    }
                };

                stackPanel.Children.Add(txtTitulo);
                stackPanel.Children.Add(estrellas);
                stackPanel.Children.Add(btnGuardar);

                dialog.Content = stackPanel;
                dialog.ShowDialog();
            }
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == null)
            {
                MessageBox.Show("Debes iniciar sesión para valorar juegos.", "Sesión requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender is Button btn && btn.DataContext is DatosJuego juego && int.TryParse(btn.Tag.ToString(), out int valor))
            {
                using (var db = new AppDbContext())
                {
                    var juegoDb = db.Juegos.Include(j => j.Valoraciones).FirstOrDefault(j => j.Id == juego.JuegoId);
                    if (juegoDb == null) return;

                    var valoracionExistente = juegoDb.Valoraciones.FirstOrDefault(v => v.UsuarioId == usuarioActual.Id);

                    if (valoracionExistente != null)
                    {
                        valoracionExistente.Puntuacion = valor;
                        valoracionExistente.FechaValoracion = DateTime.Now;
                    }
                    else
                    {
                        var nuevaValoracion = new Valoracion
                        {
                            JuegoId = juegoDb.Id,
                            UsuarioId = usuarioActual.Id,
                            Puntuacion = valor,
                            FechaValoracion = DateTime.Now
                        };
                        db.Valoraciones.Add(nuevaValoracion);
                    }

                    db.SaveChanges();

                    juego.Valoracion = valor;
                    juego.ValoracionPromedio = juegoDb.ValoracionPromedio;
                    juego.TotalValoraciones = juegoDb.TotalValoraciones;

                    var juegoEnBiblioteca = bibliotecaJuegos.FirstOrDefault(j => j.JuegoId == juego.JuegoId);
                    if (juegoEnBiblioteca != null)
                    {
                        juegoEnBiblioteca.ValoracionPromedio = juego.ValoracionPromedio;
                        juegoEnBiblioteca.TotalValoraciones = juego.TotalValoraciones;
                    }

                    var juegoEnCatalogo = catalogoCompleto.FirstOrDefault(j => j.Titulo == juego.Titulo);
                    if (juegoEnCatalogo != null)
                    {
                        juegoEnCatalogo.ValoracionPromedio = juego.ValoracionPromedio;
                        juegoEnCatalogo.TotalValoraciones = juego.TotalValoraciones;
                    }

                    MessageBox.Show($"¡Valoración de {valor} estrellas guardada!", "Valoración Registrada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        
        #endregion
    }

    // CLASES AUXILIARES
    public class DatosJuego : INotifyPropertyChanged
    {
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public string Genero { get; set; }
        public int JuegoId { get; set; }
        public string Imagen { get; set; }

        private int vecesJugadas = 0;
        public int VecesJugadas
        {
            get => vecesJugadas;
            set
            {
                if (vecesJugadas != value)
                {
                    vecesJugadas = value;
                    OnPropertyChanged(nameof(VecesJugadas));
                }
            }
        }

        private bool esFavorito = false;
        public bool EsFavorito
        {
            get => esFavorito;
            set
            {
                if (esFavorito != value)
                {
                    esFavorito = value;
                    OnPropertyChanged(nameof(EsFavorito));
                }
            }
        }

        private int valoracion = 0;
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

        private double valoracionPromedio = 0;
        public double ValoracionPromedio
        {
            get => valoracionPromedio;
            set
            {
                if (Math.Abs(valoracionPromedio - value) > 0.01)
                {
                    valoracionPromedio = value;
                    OnPropertyChanged(nameof(ValoracionPromedio));
                    OnPropertyChanged(nameof(ValoracionPromedioTexto));
                    OnPropertyChanged(nameof(ValoracionPromedioRedondeada));
                }
            }
        }

        private int totalValoraciones = 0;
        public int TotalValoraciones
        {
            get => totalValoraciones;
            set
            {
                if (totalValoraciones != value)
                {
                    totalValoraciones = value;
                    OnPropertyChanged(nameof(TotalValoraciones));
                    OnPropertyChanged(nameof(ValoracionPromedioTexto));
                }
            }
        }

        public int ValoracionPromedioRedondeada => (int)Math.Round(valoracionPromedio);

        public string ValoracionPromedioTexto => TotalValoraciones > 0 
            ? $"⭐ {ValoracionPromedio:F1} ({TotalValoraciones} {(TotalValoraciones == 1 ? "valoración" : "valoraciones")})"
            : "Sin valoraciones";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public decimal Precio { get; set; }
        public string Genero { get; set; }
        public string Descripcion { get; set; }
        public string Imagen { get; set; }
        public int VecesJugadas { get; set; } = 0;
        
        public List<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
        public List<JuegoFavorito> Favoritos { get; set; } = new List<JuegoFavorito>();

        public double ValoracionPromedio
        {
            get
            {
                if (Valoraciones == null || !Valoraciones.Any())
                    return 0;
                
                return Math.Round(Valoraciones.Average(v => v.Puntuacion), 1);
            }
        }

        public int TotalValoraciones
        {
            get
            {
                return Valoraciones?.Count ?? 0;
            }
        }
    }

    public class Valoracion
    {
        public int Id { get; set; }
        public int JuegoId { get; set; }
        public int UsuarioId { get; set; }
        public int Puntuacion { get; set; }
        public DateTime FechaValoracion { get; set; }
        
        public Juego Juego { get; set; }
        public Usuario Usuario { get; set; }
    }

    public class BibliotecaUsuario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int JuegoId { get; set; }
        public DateTime FechaAdquisicion { get; set; }
        
        public Usuario Usuario { get; set; }
        public Juego Juego { get; set; }
    }

    public class JuegoFavorito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int JuegoId { get; set; }
        public DateTime FechaAgregado { get; set; }
        
        public Usuario Usuario { get; set; }
        public Juego Juego { get; set; }
    }

    public class ListaDeseos
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int JuegoId { get; set; }
        public DateTime FechaAgregado { get; set; }
        
        public Usuario Usuario { get; set; }
        public Juego Juego { get; set; }
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
        public List<BibliotecaUsuario> BibliotecaJuegos { get; set; } = new List<BibliotecaUsuario>();
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Juego> Juegos { get; set; }
        public DbSet<Valoracion> Valoraciones { get; set; }
        public DbSet<BibliotecaUsuario> BibliotecaUsuarios { get; set; }
        public DbSet<JuegoFavorito> JuegosFavoritos { get; set; }
        public DbSet<ListaDeseos> ListasDeseos { get; set; }

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

            modelBuilder.Entity<Valoracion>()
                .HasOne(v => v.Juego)
                .WithMany(j => j.Valoraciones)
                .HasForeignKey(v => v.JuegoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Valoracion>()
                .HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Valoracion>()
                .HasIndex(v => new { v.JuegoId, v.UsuarioId })
                .IsUnique();

            modelBuilder.Entity<BibliotecaUsuario>()
                .HasOne(b => b.Usuario)
                .WithMany(u => u.BibliotecaJuegos)
                .HasForeignKey(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BibliotecaUsuario>()
                .HasOne(b => b.Juego)
                .WithMany()
                .HasForeignKey(b => b.JuegoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BibliotecaUsuario>()
                .HasIndex(b => new { b.UsuarioId, b.JuegoId })
                .IsUnique();

            modelBuilder.Entity<JuegoFavorito>()
                .HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JuegoFavorito>()
                .HasOne(f => f.Juego)
                .WithMany(j => j.Favoritos)
                .HasForeignKey(f => f.JuegoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JuegoFavorito>()
                .HasIndex(f => new { f.UsuarioId, f.JuegoId })
                .IsUnique();

            modelBuilder.Entity<ListaDeseos>()
                .HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListaDeseos>()
                .HasOne(l => l.Juego)
                .WithMany()
                .HasForeignKey(l => l.JuegoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListaDeseos>()
                .HasIndex(l => new { l.UsuarioId, l.JuegoId })
                .IsUnique();
        }
    }
}