using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaAnalisis
{
    public class IteracionNewtonRaphson
    {
        public int Iteracion { get; set; }

        // Valor actual de x
        public double Xn { get; set; }

        // f(xn)
        public double Fxn { get; set; }

        // f'(xn)
        public double Derivada { get; set; }

        // Nuevo valor calculado por Newton
        public double Xsiguiente { get; set; }

        // Diferencia entre el nuevo x y el anterior
        public double Error { get; set; }
    }
}