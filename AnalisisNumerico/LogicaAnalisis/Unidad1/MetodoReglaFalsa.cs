using System;
using Calculus;

namespace LogicaAnalisis
{
    public class MetodoReglaFalsa
    {
        public ResultadoReglaFalsa Calcular(
            string funcion,
            double xi,
            double xd,
            double tolerancia,
            int iteracionesMaximas)
        {
            if (string.IsNullOrWhiteSpace(funcion))
            {
                throw new ArgumentException("Debe ingresar una funcion.");
            }

            if (tolerancia <= 0)
            {
                throw new ArgumentException("La tolerancia debe ser mayor a cero.");
            }

            if (iteracionesMaximas <= 0)
            {
                throw new ArgumentException("Las iteraciones maximas deben ser mayores a cero.");
            }

            if (xi >= xd)
            {
                throw new ArgumentException("Xi debe ser menor que Xd.");
            }

            funcion = funcion.Trim().Replace(',', '.');

            Calculo analizador = new Calculo();
            if (!analizador.Sintaxis(funcion, 'x'))
            {
                throw new ArgumentException("La funcion ingresada tiene una sintaxis invalida.");
            }

            ResultadoReglaFalsa resultado = new ResultadoReglaFalsa
            {
                Funcion = funcion,
                Tolerancia = tolerancia,
                XiInicial = xi,
                XdInicial = xd
            };

            double fxi = Evaluar(analizador, xi);
            double fxd = Evaluar(analizador, xd);

            if (Math.Abs(fxi) <= tolerancia)
            {
                resultado.Converge = true;
                resultado.Raiz = xi;
                resultado.Error = 0;
                resultado.Mensaje = "La raiz esta en Xi.";
                return resultado;
            }

            if (Math.Abs(fxd) <= tolerancia)
            {
                resultado.Converge = true;
                resultado.Raiz = xd;
                resultado.Error = 0;
                resultado.Mensaje = "La raiz esta en Xd.";
                return resultado;
            }

            if (fxi * fxd > 0)
            {
                throw new ArgumentException("No se puede aplicar regla falsa porque f(Xi) y f(Xd) tienen el mismo signo.");
            }

            double xrAnterior = double.NaN;

            for (int i = 1; i <= iteracionesMaximas; i++)
            {
                double xr = xd - (fxd * (xi - xd)) / (fxi - fxd);
                double fxr = Evaluar(analizador, xr);
                double error = double.IsNaN(xrAnterior)
                    ? Math.Abs(xd - xi)
                    : Math.Abs(xr - xrAnterior);

                resultado.Iteraciones.Add(new IteracionReglaFalsa
                {
                    Iteracion = i,
                    Xi = xi,
                    Xd = xd,
                    Xr = xr,
                    Fxi = fxi,
                    Fxd = fxd,
                    Fxr = fxr,
                    Error = error
                });

                if (Math.Abs(fxr) <= tolerancia || error <= tolerancia)
                {
                    resultado.Converge = true;
                    resultado.Raiz = xr;
                    resultado.Error = error;
                    resultado.IteracionesRealizadas = i;
                    resultado.Mensaje = "Raiz aproximada encontrada.";
                    return resultado;
                }

                if (fxi * fxr < 0)
                {
                    xd = xr;
                    fxd = fxr;
                }
                else
                {
                    xi = xr;
                    fxi = fxr;
                }

                xrAnterior = xr;
            }

            resultado.Converge = false;
            resultado.Raiz = xrAnterior;
            resultado.Error = resultado.Iteraciones.Count > 0
                ? resultado.Iteraciones[resultado.Iteraciones.Count - 1].Error
                : 0;
            resultado.IteracionesRealizadas = resultado.Iteraciones.Count;
            resultado.Mensaje = "Se alcanzo el maximo de iteraciones.";

            return resultado;
        }

        private double Evaluar(Calculo analizador, double x)
        {
            double resultado = analizador.EvaluaFx(x);

            if (double.IsNaN(resultado) || double.IsInfinity(resultado))
            {
                throw new ArgumentException("La funcion no se puede evaluar en x = " + x);
            }

            return resultado;
        }
    }
}
