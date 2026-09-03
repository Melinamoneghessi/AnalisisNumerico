using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculus;

namespace LogicaAnalisis
{
    public class EvaluadorFuncion
    {
        public bool TryEvaluar(
                    string funcion,
                    double valorX,
                    out double resultado,
                    out string mensajeError)
        {
            resultado = 0;
            mensajeError = "";

            if (string.IsNullOrWhiteSpace(funcion))
            {
                mensajeError = "Debe ingresar una función.";
                return false;
            }

            funcion = funcion.Replace(',', '.');

            Calculo analizador = new Calculo();

            if (!analizador.Sintaxis(funcion, 'x'))
            {
                mensajeError = "La función tiene una sintaxis inválida.";
                return false;
            }

            resultado = analizador.EvaluaFx(valorX);

            if (double.IsNaN(resultado) || double.IsInfinity(resultado))
            {
                mensajeError = "La función no está definida para ese valor.";
                return false;
            }

            return true;
        }
    }
}
