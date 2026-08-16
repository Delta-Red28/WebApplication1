using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(
            IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<RestauranteContext>();

            // Obtener los roles existentes
            var roles = await context.Rols
                .ToDictionaryAsync(
                    r => r.Nombre,
                    r => r.IdRol
                );

            // Verificar que existan los cinco roles
            string[] rolesNecesarios =
            {
                "Administrador",
                "Gerente",
                "Cajero",
                "Mesero",
                "Cocina"
            };

            foreach (var rol in rolesNecesarios)
            {
                if (!roles.ContainsKey(rol))
                {
                    throw new Exception(
                        $"No existe el rol '{rol}' en la base de datos."
                    );
                }
            }

            var usuarios = new[]
            {
                new
                {
                    Rol = "Administrador",
                    Nombres = "Administrador",
                    Apellidos = "Sistema",
                    Correo = "admin@restaurante.com",
                    Usuario = "admin",
                    Password = "Admin123!"
                },
                new
                {
                    Rol = "Gerente",
                    Nombres = "Gerente",
                    Apellidos = "Sistema",
                    Correo = "gerente@restaurante.com",
                    Usuario = "gerente",
                    Password = "Gerente123!"
                },
                new
                {
                    Rol = "Cajero",
                    Nombres = "Cajero",
                    Apellidos = "Sistema",
                    Correo = "cajero@restaurante.com",
                    Usuario = "cajero",
                    Password = "Cajero123!"
                },
                new
                {
                    Rol = "Mesero",
                    Nombres = "Mesero",
                    Apellidos = "Sistema",
                    Correo = "mesero@restaurante.com",
                    Usuario = "mesero",
                    Password = "Mesero123!"
                },
                new
                {
                    Rol = "Cocina",
                    Nombres = "Cocina",
                    Apellidos = "Sistema",
                    Correo = "cocina@restaurante.com",
                    Usuario = "cocina",
                    Password = "Cocina123!"
                }
            };

            foreach (var datos in usuarios)
            {
                var usuarioExistente = await context.Usuarios
                    .FirstOrDefaultAsync(
                        u => u.Usuario1 == datos.Usuario
                    );

                if (usuarioExistente == null)
                {
                    var nuevoUsuario = new Usuario
                    {
                        IdRol = roles[datos.Rol],
                        Nombres = datos.Nombres,
                        Apellidos = datos.Apellidos,
                        Correo = datos.Correo,
                        Usuario1 = datos.Usuario,

                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                datos.Password
                            ),

                        Estado = true,
                        FechaRegistro = DateTime.Now,
                        IntentosFallidos = 0,

                        // Las cuentas creadas inicialmente
                        // deben cambiar su contraseña.
                        DebeCambiarPassword = true,

                        FechaCambioPassword = null
                    };

                    context.Usuarios.Add(nuevoUsuario);
                }
                else
                {
                    // No reemplazamos la contraseña existente.
                    //
                    // Si el usuario todavía nunca ha realizado
                    // un cambio de contraseña, lo obligamos a hacerlo.
                    //
                    // Si ya realizó el cambio, FechaCambioPassword
                    // tendrá un valor y no volvemos a marcarlo.

                    if (usuarioExistente.FechaCambioPassword == null)
                    {
                        usuarioExistente.DebeCambiarPassword = true;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}