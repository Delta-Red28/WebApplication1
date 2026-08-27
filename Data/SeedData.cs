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

            Console.WriteLine("");
            Console.WriteLine("========================================");
            Console.WriteLine("SEED: INICIANDO");
            Console.WriteLine("========================================");

            // ============================================================
            // 1. PROBAR CONEXIÓN
            // ============================================================

            Console.WriteLine(
                "SEED: Probando conexión a SQL Server..."
            );

            try
            {
                var conectado =
                    await context.Database.CanConnectAsync();

                Console.WriteLine(
                    $"SEED: Resultado de conexión: {conectado}"
                );

                if (!conectado)
                {
                    throw new Exception(
                        "No se pudo conectar a la base de datos."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SEED: ERROR AL CONECTAR CON SQL SERVER"
                );

                Console.WriteLine(ex.ToString());

                throw;
            }

            // ============================================================
            // 2. OBTENER ROLES
            // ============================================================

            Console.WriteLine(
                "SEED: Consultando tabla Rol..."
            );

            Dictionary<string, int> roles;

            try
            {
                roles = await context.Rols
                    .ToDictionaryAsync(
                        r => r.Nombre,
                        r => r.IdRol
                    );

                Console.WriteLine(
                    $"SEED: Roles encontrados: {roles.Count}"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SEED: ERROR AL CONSULTAR ROLES"
                );

                Console.WriteLine(ex.ToString());

                throw;
            }

            // ============================================================
            // 3. VERIFICAR ROLES NECESARIOS
            // ============================================================

            string[] rolesNecesarios =
            {
                "Administrador",
                "Gerente",
                "Cajero",
                "Mesero",
                "Cocina"
            };

            Console.WriteLine(
                "SEED: Verificando roles necesarios..."
            );

            foreach (var rol in rolesNecesarios)
            {
                Console.WriteLine(
                    $"SEED: Verificando '{rol}'..."
                );

                if (!roles.ContainsKey(rol))
                {
                    throw new Exception(
                        $"No existe el rol '{rol}' en la base de datos."
                    );
                }
            }

            Console.WriteLine(
                "SEED: Los cinco roles existen correctamente."
            );

            // ============================================================
            // 4. USUARIOS INICIALES
            // ============================================================

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

            // ============================================================
            // 5. PROCESAR USUARIOS
            // ============================================================

            foreach (var datos in usuarios)
            {
                Console.WriteLine("");
                Console.WriteLine(
                    $"SEED: Procesando usuario '{datos.Usuario}'..."
                );

                Usuario? usuarioExistente;

                try
                {
                    Console.WriteLine(
                        $"SEED: Consultando usuario '{datos.Usuario}'..."
                    );

                    usuarioExistente =
                        await context.Usuarios
                            .FirstOrDefaultAsync(
                                u => u.Usuario1 == datos.Usuario
                            );

                    Console.WriteLine(
                        $"SEED: Consulta terminada para '{datos.Usuario}'."
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"SEED: ERROR AL CONSULTAR USUARIO '{datos.Usuario}'"
                    );

                    Console.WriteLine(ex.ToString());

                    throw;
                }

                // ========================================================
                // USUARIO NO EXISTE
                // ========================================================

                if (usuarioExistente == null)
                {
                    Console.WriteLine(
                        $"SEED: '{datos.Usuario}' no existe."
                    );

                    Console.WriteLine(
                        $"SEED: Generando contraseña para '{datos.Usuario}'..."
                    );

                    string passwordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            datos.Password
                        );

                    Console.WriteLine(
                        $"SEED: Contraseña generada para '{datos.Usuario}'."
                    );

                    var nuevoUsuario = new Usuario
                    {
                        IdRol = roles[datos.Rol],

                        Nombres = datos.Nombres,

                        Apellidos = datos.Apellidos,

                        Correo = datos.Correo,

                        Usuario1 = datos.Usuario,

                        PasswordHash = passwordHash,

                        Estado = true,

                        FechaRegistro = DateTime.Now,

                        IntentosFallidos = 0,

                        DebeCambiarPassword = true,

                        FechaCambioPassword = null
                    };

                    context.Usuarios.Add(nuevoUsuario);

                    Console.WriteLine(
                        $"SEED: Usuario '{datos.Usuario}' agregado."
                    );
                }
                else
                {
                    // ====================================================
                    // USUARIO YA EXISTE
                    // ====================================================

                    Console.WriteLine(
                        $"SEED: Usuario '{datos.Usuario}' ya existe."
                    );

                    if (usuarioExistente.FechaCambioPassword == null)
                    {
                        Console.WriteLine(
                            $"SEED: '{datos.Usuario}' debe cambiar contraseña."
                        );

                        usuarioExistente.DebeCambiarPassword = true;
                    }
                    else
                    {
                        Console.WriteLine(
                            $"SEED: '{datos.Usuario}' ya cambió su contraseña."
                        );
                    }
                }
            }

            // ============================================================
            // 6. GUARDAR CAMBIOS
            // ============================================================

            Console.WriteLine("");
            Console.WriteLine(
                "SEED: Guardando cambios en la base de datos..."
            );

            try
            {
                await context.SaveChangesAsync();

                Console.WriteLine(
                    "SEED: Cambios guardados correctamente."
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SEED: ERROR EN SaveChangesAsync"
                );

                Console.WriteLine(ex.ToString());

                throw;
            }

            // ============================================================
            // 7. FINAL
            // ============================================================

            Console.WriteLine("");
            Console.WriteLine("========================================");
            Console.WriteLine("SEED: TERMINADO CORRECTAMENTE");
            Console.WriteLine("========================================");
            Console.WriteLine("");
        }
    }
}