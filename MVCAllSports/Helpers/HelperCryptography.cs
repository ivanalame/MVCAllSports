using System.Security.Cryptography;
using System.Text;

namespace AllSports.Helpers
{
    public class HelperCryptography
    {
        public static string GenerateSalt()
        {
            Random random = new Random();
            string salt = "";

            for (int i = 1; i<=50;i++)
            {
                int aleat = random.Next(1, 255);
                char letra = Convert.ToChar(aleat);
                salt = salt + letra;
            }
            return salt;
        }

        //COMPARAMOS LOS PASSWORD A NIVEL DE BYTE
        public static bool CompareArrays(byte[] a, byte[] b)
        {
            bool iguales = true;
            if (a.Length != b.Length)
            {
                iguales = false;
            }
            else
            {
                for (int i = 0; i<a.Length; i++)
                {
                    //HAY QUE PREGUNTAR SI EL CONTENIDO DE CADA BYTE ES DISTINTO
                    if (a[i].Equals(b[i]) == false)
                    {
                        iguales = false;
                        break;
                    }
                }
            }
            return iguales;
        }

        public static byte[] EncryptPassword(string password,string salt)
        {
            string contenido = password + salt;
            SHA512 sha = SHA512.Create();

            //CONVERTIMOS contenido A BYTES[] 
            byte[] salida = Encoding.UTF8.GetBytes(contenido);
            //CREAMOS LAS ITERACIONES 
           for (int i = 1; i <= 114; i++)
            {
                salida = sha.ComputeHash(salida);
            }
            sha.Clear();
            return salida;
        }
    }
}
