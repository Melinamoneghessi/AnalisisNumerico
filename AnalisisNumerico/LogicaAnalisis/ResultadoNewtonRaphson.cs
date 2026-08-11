using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaAnalisis
{
    public class ResultadoNewtonRaphson
    {
        public string Funcion { get; set; }
        public string Metodo { get; set; }
        public int IteracionesRealizadas { get; set; }
        public double Tolerancia { get; set; }

        // Valor inicial usado por Newton
        public double X0Inicial { get; set; }

        public bool Converge { get; set; }
        public double Raiz { get; set; }
        public double Error { get; set; }
        public string Mensaje { get; set; }

        public List<IteracionNewtonRaphson> Iteraciones { get; set; }

        public ResultadoNewtonRaphson()
        {
            Metodo = "Newton-Raphson";
            Mensaje = "";
            Iteraciones = new List<IteracionNewtonRaphson>();
        }
    }
}