using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TendalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TendalProject.Service
{
    public class UsuarioService
    {
        private readonly BdTendalDefinitivoContext _context;

        public UsuarioService(BdTendalDefinitivoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<string> InsertarUsuarioLogin(string nombreUsuario, string apellidoPaterno, string apellidoMaterno, string celular, string direccion, string correo, string clave)
        {
            // Crear los parámetros para el procedimiento almacenado
            var parameterNombreUsuario = new SqlParameter("@NombreUsuario", nombreUsuario);
            var parameterApellidoPaterno = new SqlParameter("@ApellidoPaterno", apellidoPaterno);
            var parameterApellidoMaterno = new SqlParameter("@ApellidoMaterno", apellidoMaterno);
            var parameterCelular = new SqlParameter("@Celular", celular);
            var parameterDireccion = new SqlParameter("@Direccion", direccion);
            var parameterCorreo = new SqlParameter("@Correo", correo);
            var parameterClave = new SqlParameter("@Clave", clave); // La clave en texto plano



            try
            {
                // Ejecutar el procedimiento almacenado
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertarUsuarioLogin @NombreUsuario, @ApellidoPaterno, @ApellidoMaterno, @Celular, @Direccion, @Correo, @Clave",
                    parameterNombreUsuario,
                    parameterApellidoPaterno,
                    parameterApellidoMaterno,
                    parameterCelular,
                    parameterDireccion,
                    parameterCorreo,
                    parameterClave


                );

                return "Usuario insertado exitosamente.";
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error: {ex.Message}");
                return "Error al insertar el usuario.";
            }
        }

        public async Task<string> InsertarUsuarioAsync(string nombreUsuario, string apellidoPaterno, string apellidoMaterno, string celular, string direccion, string correo, string clave, int idRol)
        {
            // Crear los parámetros para el procedimiento almacenado
            var parameterNombreUsuario = new SqlParameter("@NombreUsuario", nombreUsuario);
            var parameterApellidoPaterno = new SqlParameter("@ApellidoPaterno", apellidoPaterno);
            var parameterApellidoMaterno = new SqlParameter("@ApellidoMaterno", apellidoMaterno);
            var parameterCelular = new SqlParameter("@Celular", celular);
            var parameterDireccion = new SqlParameter("@Direccion", direccion);
            var parameterCorreo = new SqlParameter("@Correo", correo);
            var parameterClave = new SqlParameter("@Clave", clave); // La clave en texto plano
            var parameterIdRol = new SqlParameter("@IdRol", idRol);


            try
            {
                // Ejecutar el procedimiento almacenado
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC InsertarUsuario @NombreUsuario, @ApellidoPaterno, @ApellidoMaterno, @Celular, @Direccion, @Correo, @Clave, @IdRol",
                    parameterNombreUsuario,
                    parameterApellidoPaterno,
                    parameterApellidoMaterno,
                    parameterCelular,
                    parameterDireccion,
                    parameterCorreo,
                    parameterClave,
                    parameterIdRol

                );

                return "Usuario insertado exitosamente.";
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error: {ex.Message}");
                return "Error al insertar el usuario.";
            }
        }

        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            try
            {
                var parameter = new SqlParameter("@IdUsuario", id);
                await _context.Database.ExecuteSqlRawAsync("EXEC EliminarUsuario @IdUsuario", parameter);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<Usuario> ValidarUsuarioAsync(string correo, string clave)
        {
            var parameterCorreo = new SqlParameter("@Correo", correo);
            var parameterClave = new SqlParameter("@Clave", clave);
            var parameterExiste = new SqlParameter("@Existe", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            // Ejecutar el procedimiento almacenado
            await _context.Database.ExecuteSqlRawAsync("EXEC ValidarUsuario @Correo, @Clave, @Existe OUTPUT", parameterCorreo, parameterClave, parameterExiste);

            if ((bool)parameterExiste.Value)
            {
                // Recuperar el usuario, asegurándote de incluir el Rol
                return await _context.Usuarios
                    .Include(u => u.IdRolNavigation) // Esto carga la navegación del rol
                    .FirstOrDefaultAsync(u => u.Correo == correo);
            }

            return null;
        }









    }
}